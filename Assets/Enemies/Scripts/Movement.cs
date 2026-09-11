using System.Collections;
using System.Collections.Generic;
using RPGame.Core.Movement;
using UnityEngine;
using UnityEngine.AI;

namespace RPGame.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class Movement : MonoBehaviour, IEnemyMovement, IKnockbackable, IMovement
    {
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float destinationChangeThreshold = 0.05f;

        private NavMeshAgent agent;
        private Vector3 lastDestination;
        private bool hasDestination;
        private Coroutine knockbackCoroutine;
        private int movementBlockCount;
        private bool isKnockedBack;
        private readonly RaycastHit[] knockbackHitBuffer = new RaycastHit[16];
        private readonly Dictionary<IModifierSource, float> movementSpeedModifiers = new();

        private Vector3 Position => transform.position;

        private void Start()
        {
            CacheRequiredComponents();
            if (!HasRequiredComponents())
            {
                enabled = false;
                return;
            }

            ConfigureAgent();
        }

        internal void MoveTo(Vector3 position)
        {
            if (isKnockedBack || IsMovementBlocked || !CanUseAgent())
            {
                return;
            }

            ConfigureAgent();

            if (hasDestination && IsSameDestination(position))
            {
                agent.isStopped = false;
                return;
            }

            agent.isStopped = false;
            if (agent.SetDestination(position))
            {
                lastDestination = position;
                hasDestination = true;
            }
        }

        internal void Stop()
        {
            if (isKnockedBack || !CanUseAgent())
            {
                return;
            }

            agent.isStopped = true;
        }

        private bool TryResolvePosition(Vector3 desiredPosition, out Vector3 validPosition)
        {
            validPosition = default;
            if (agent == null)
            {
                return false;
            }

            if (!NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, agent.height, agent.areaMask))
            {
                return false;
            }

            validPosition = hit.position;
            return true;
        }

        private void ConfigureAgent()
        {
            if (agent != null)
            {
                agent.speed = GetModifiedSpeed(moveSpeed);
            }
        }

        private void CacheRequiredComponents()
        {
            if (agent == null)
            {
                agent = GetComponent<NavMeshAgent>();
            }
        }

        private bool HasRequiredComponents()
        {
            if (agent != null)
            {
                return true;
            }

            Debug.LogError("Missing field agent.", this);
            return false;
        }

        private bool CanUseAgent()
        {
            return agent != null
                && agent.enabled
                && agent.gameObject.activeInHierarchy
                && agent.isOnNavMesh;
        }

        public void ApplyKnockback(Vector3 direction, float distance, float duration)
        {
            if (knockbackCoroutine != null)
            {
                StopCoroutine(knockbackCoroutine);
                knockbackCoroutine = null;
                EndKnockback();
            }

            knockbackCoroutine = StartCoroutine(ApplyKnockbackRoutine(direction, distance, duration));
        }

        private IEnumerator ApplyKnockbackRoutine(Vector3 direction, float distance, float duration)
        {
            direction.y = 0f;
            direction = direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : Vector3.zero;
            isKnockedBack = true;
            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            Vector3 startPosition = transform.position;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = duration > Mathf.Epsilon ? Mathf.Clamp01(elapsed / duration) : 1f;
                float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
                Vector3 desiredPosition = startPosition + direction * (distance * easedProgress);
                Vector3 displacement = desiredPosition - transform.position;
                if (!TryMoveWithSweep(displacement))
                {
                    EndKnockback();
                    break;
                }

                yield return null;
            }

            EndKnockback();
            knockbackCoroutine = null;
        }

        private bool TryMoveWithSweep(Vector3 displacement)
        {
            float distance = displacement.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return true;
            }

            Vector3 center = transform.position + Vector3.up * (agent != null ? agent.height * 0.5f : 0.5f);
            float radius = agent != null ? agent.radius : 0.25f;
            float halfHeight = Mathf.Max(radius, (agent != null ? agent.height : 1f) * 0.5f);
            Vector3 bottom = center + Vector3.down * (halfHeight - radius);
            Vector3 top = center + Vector3.up * (halfHeight - radius);
            int hitCount = Physics.CapsuleCastNonAlloc(
                bottom,
                top,
                radius,
                displacement / distance,
                knockbackHitBuffer,
                distance,
                ~0,
                QueryTriggerInteraction.Ignore);
            float closestDistance = float.MaxValue;
            bool foundObstacle = false;
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = knockbackHitBuffer[i];
                if (hit.collider == null
                    || hit.collider.transform == transform
                    || hit.collider.transform.IsChildOf(transform)
                    || IsEnemyCollider(hit.collider)
                    || hit.distance >= closestDistance)
                {
                    continue;
                }

                closestDistance = hit.distance;
                foundObstacle = true;
            }

            if (foundObstacle)
            {
                transform.position += displacement.normalized * Mathf.Max(0f, closestDistance - 0.01f);
                return false;
            }

            transform.position += displacement;
            return true;
        }

        private static bool IsEnemyCollider(Collider collider)
        {
            return collider.GetComponentInParent<Movement>() != null;
        }

        private void EndKnockback()
        {
            isKnockedBack = false;
            hasDestination = false;

            if (agent == null)
            {
                return;
            }

            bool hasValidPosition = NavMesh.SamplePosition(
                transform.position,
                out NavMeshHit hit,
                agent.height,
                agent.areaMask);
            if (hasValidPosition)
            {
                transform.position = hit.position;
            }

            if (!agent.enabled && hasValidPosition)
            {
                agent.enabled = true;
                agent.Warp(hit.position);
            }

            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = !IsMovementBlocked;
            }
        }

        private bool IsMovementBlocked => movementBlockCount > 0;

        private float MovementSpeedMultiplier
        {
            get
            {
                float multiplier = 1f;
                foreach (float speedModifier in movementSpeedModifiers.Values)
                {
                    multiplier *= speedModifier;
                }

                return multiplier;
            }
        }

        private float GetModifiedSpeed(float baseSpeed)
        {
            return baseSpeed * MovementSpeedMultiplier;
        }

        public void BlockMovement()
        {
            movementBlockCount++;
            Stop();
        }

        public void UnblockMovement()
        {
            movementBlockCount = Mathf.Max(0, movementBlockCount - 1);
        }

        public void AddMovementSpeedModifier(IModifierSource source, float multiplier)
        {
            if (source == null)
            {
                return;
            }

            movementSpeedModifiers[source] = Mathf.Max(0f, multiplier);
            ConfigureAgent();
        }

        public void RemoveMovementSpeedModifier(IModifierSource source)
        {
            if (source != null && movementSpeedModifiers.Remove(source))
            {
                ConfigureAgent();
            }
        }

        private bool IsSameDestination(Vector3 position)
        {
            float thresholdSqr = destinationChangeThreshold * destinationChangeThreshold;
            return (position - lastDestination).sqrMagnitude <= thresholdSqr;
        }

        private void OnValidate()
        {
            moveSpeed = Mathf.Max(0f, moveSpeed);
            destinationChangeThreshold = Mathf.Max(0f, destinationChangeThreshold);
            ConfigureAgent();
        }

        void IEnemyMovement.MoveTo(Vector3 position)
        {
            MoveTo(position);
        }

        void IEnemyMovement.Stop()
        {
            Stop();
        }

        Vector3 IEnemyMovement.Position => Position;

        bool IEnemyMovement.TryResolvePosition(Vector3 desiredPosition, out Vector3 validPosition)
        {
            return TryResolvePosition(desiredPosition, out validPosition);
        }
    }
}
