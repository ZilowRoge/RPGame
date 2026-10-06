using System.Collections;
using System;
using RPGame.Core.Movement;
using RPGame.Core.Pooling;
using UnityEngine;
using UnityEngine.AI;

namespace RPGame.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class Movement : MonoBehaviour, IEnemyMovement, IKnockbackable, IMovement, IPooledEnemyResettable
    {
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float destinationChangeThreshold = 0.05f;
        [SerializeField] private float traversalLeapSpeed = 6f;
        [SerializeField] private float traversalLeapArcHeight = 1f;

        private NavMeshAgent agent;
        private Vector3 lastDestination;
        private bool hasDestination;
        private Coroutine knockbackCoroutine;
        private Coroutine leapCoroutine;
        private Coroutine chargeCoroutine;
        private int movementBlockCount;
        private bool isKnockedBack;
        private bool isLeaping;
        private bool isCharging;
        private bool leapAgentWasStopped;
        private bool leapAgentUpdatedPosition = true;
        private bool leapTraversesOffMeshLink;
        private Vector3 leapStartPosition;
        private bool chargeAgentWasStopped;
        private bool chargeAgentUpdatedPosition = true;
        private Vector3 chargeDirection;
        private float chargeSpeed;
        private float chargeMaxDistance;
        private float chargeDistance;
        private float chargeKnockbackResistance;
        private Action<Collider, Vector3> chargeCollisionHandler;
        private readonly RaycastHit[] knockbackHitBuffer = new RaycastHit[16];
        private readonly MovementSpeedModifiers movementSpeedModifiers = new();

        public Vector3 Position => transform.position;
        public bool IsLeaping => isLeaping;
        public bool IsCharging => isCharging;
        public bool IsMovementBlocked => movementBlockCount > 0;

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

        private void Update()
        {
            TryTraverseOffMeshLink();
        }

        public void MoveTo(Vector3 position)
        {
            if (isLeaping || isCharging || isKnockedBack || IsMovementBlocked || !CanUseAgent())
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

        public void FaceTowards(Vector3 position)
        {
            if (isCharging)
            {
                return;
            }

            Vector3 direction = position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(direction);
        }

        public void Stop()
        {
            if (isLeaping || isCharging || isKnockedBack || !CanUseAgent())
            {
                return;
            }

            agent.isStopped = true;
        }

        public bool TryResolvePosition(Vector3 desiredPosition, out Vector3 validPosition)
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

        public bool TryLeapTo(Vector3 destination, float speed, float arcHeight)
        {
            if (!CanLeap() || !HasValidLeapParameters(destination, speed, arcHeight))
            {
                return false;
            }

            if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, agent.height, agent.areaMask))
            {
                return false;
            }

            StartLeap(hit.position, speed, arcHeight);
            return true;
        }

        public bool TryStartCharge(
            Vector3 destination,
            float speed,
            float maxDistance,
            float knockbackResistance,
            Action<Collider, Vector3> onCollision)
        {
            if (!CanStartCharge(destination, speed, maxDistance, knockbackResistance))
            {
                return false;
            }

            Vector3 direction = destination - transform.position;
            direction.y = 0f;
            chargeDirection = direction.normalized;
            chargeSpeed = speed;
            chargeMaxDistance = maxDistance;
            chargeDistance = 0f;
            chargeKnockbackResistance = knockbackResistance;
            chargeCollisionHandler = onCollision;
            chargeAgentWasStopped = agent.isStopped;
            chargeAgentUpdatedPosition = agent.updatePosition;
            isCharging = true;
            agent.isStopped = true;
            agent.updatePosition = false;
            chargeCoroutine = StartCoroutine(ChargeRoutine());
            return true;
        }

        internal void CancelCharge()
        {
            if (!isCharging)
            {
                return;
            }

            if (chargeCoroutine != null)
            {
                StopCoroutine(chargeCoroutine);
                chargeCoroutine = null;
            }

            FinishCharge();
        }

        private IEnumerator ChargeRoutine()
        {
            while (chargeDistance < chargeMaxDistance)
            {
                float stepDistance = Mathf.Min(chargeSpeed * Time.deltaTime, chargeMaxDistance - chargeDistance);
                if (stepDistance <= Mathf.Epsilon)
                {
                    yield return null;
                    continue;
                }

                if (!TryApplyChargeStep(chargeDirection * stepDistance))
                {
                    break;
                }

                chargeDistance += stepDistance;
                yield return null;
            }

            FinishCharge();
        }

        private bool CanStartCharge(
            Vector3 destination,
            float speed,
            float maxDistance,
            float knockbackResistance)
        {
            Vector3 direction = destination - transform.position;
            direction.y = 0f;
            return !isLeaping
                && !isCharging
                && !isKnockedBack
                && !IsMovementBlocked
                && CanUseAgent()
                && direction.sqrMagnitude > Mathf.Epsilon
                && !float.IsNaN(destination.x)
                && !float.IsNaN(destination.y)
                && !float.IsNaN(destination.z)
                && !float.IsInfinity(destination.x)
                && !float.IsInfinity(destination.y)
                && !float.IsInfinity(destination.z)
                && speed > 0f
                && maxDistance > 0f
                && !float.IsNaN(speed)
                && !float.IsInfinity(speed)
                && !float.IsNaN(maxDistance)
                && !float.IsInfinity(maxDistance)
                && knockbackResistance >= 0f
                && knockbackResistance <= 1f;
        }

        private bool TryApplyChargeStep(Vector3 displacement)
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
            RaycastHit closestHit = default;
            bool foundHit = false;
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
                closestHit = hit;
                foundHit = true;
            }

            if (!foundHit)
            {
                transform.position += displacement;
                return true;
            }

            transform.position += displacement.normalized * Mathf.Max(0f, closestDistance - 0.01f);
            chargeCollisionHandler?.Invoke(closestHit.collider, closestHit.point);
            return false;
        }

        private void FinishCharge()
        {
            isCharging = false;
            chargeCoroutine = null;
            chargeCollisionHandler = null;
            chargeDirection = default;
            chargeSpeed = 0f;
            chargeMaxDistance = 0f;
            chargeDistance = 0f;
            chargeKnockbackResistance = 0f;

            if (agent == null || !agent.enabled)
            {
                chargeAgentWasStopped = false;
                chargeAgentUpdatedPosition = true;
                return;
            }

            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, agent.height, agent.areaMask))
            {
                transform.position = hit.position;
                agent.Warp(hit.position);
            }

            agent.updatePosition = chargeAgentUpdatedPosition;
            if (agent.isOnNavMesh)
            {
                agent.isStopped = IsMovementBlocked || chargeAgentWasStopped;
            }

            chargeAgentWasStopped = false;
            chargeAgentUpdatedPosition = true;
        }

        private static bool HasValidLeapParameters(Vector3 destination, float speed, float arcHeight)
        {
            return speed > 0f
                && arcHeight >= 0f
                && !float.IsNaN(speed)
                && !float.IsInfinity(speed)
                && !float.IsNaN(arcHeight)
                && !float.IsInfinity(arcHeight)
                && !float.IsNaN(destination.x)
                && !float.IsNaN(destination.y)
                && !float.IsNaN(destination.z)
                && !float.IsInfinity(destination.x)
                && !float.IsInfinity(destination.y)
                && !float.IsInfinity(destination.z);
        }

        internal void CancelLeap()
        {
            if (!isLeaping)
            {
                return;
            }

            if (leapCoroutine != null)
            {
                StopCoroutine(leapCoroutine);
                leapCoroutine = null;
            }

            FinishLeap(leapStartPosition, false);
        }

        private void StartLeap(Vector3 landingPoint, float speed, float arcHeight)
        {
            leapStartPosition = transform.position;
            leapAgentWasStopped = agent.isStopped;
            leapAgentUpdatedPosition = agent.updatePosition;
            leapTraversesOffMeshLink = agent.isOnOffMeshLink;
            isLeaping = true;
            agent.isStopped = true;
            agent.updatePosition = false;
            leapCoroutine = StartCoroutine(LeapRoutine(leapStartPosition, landingPoint, speed, arcHeight));
        }

        private IEnumerator LeapRoutine(Vector3 start, Vector3 end, float speed, float arcHeight)
        {
            float duration = Vector3.Distance(start, end) / speed;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = duration > Mathf.Epsilon ? Mathf.Clamp01(elapsed / duration) : 1f;
                transform.position = Vector3.Lerp(start, end, progress)
                    + Vector3.up * Mathf.Sin(Mathf.PI * progress) * arcHeight;
                yield return null;
            }

            FinishLeap(end, true);
        }

        private void FinishLeap(Vector3 landingPoint, bool completeTraversal)
        {
            bool shouldCompleteTraversal = completeTraversal && leapTraversesOffMeshLink;
            isLeaping = false;
            leapCoroutine = null;
            leapTraversesOffMeshLink = false;

            if (agent == null || !agent.enabled)
            {
                leapAgentWasStopped = false;
                leapAgentUpdatedPosition = true;
                return;
            }

            if (NavMesh.SamplePosition(landingPoint, out NavMeshHit hit, agent.height, agent.areaMask))
            {
                transform.position = hit.position;
                agent.Warp(hit.position);
            }

            if (shouldCompleteTraversal && agent.isOnOffMeshLink)
            {
                agent.CompleteOffMeshLink();
            }

            agent.updatePosition = leapAgentUpdatedPosition;
            if (agent.isOnNavMesh)
            {
                agent.isStopped = IsMovementBlocked || leapAgentWasStopped;
            }

            leapAgentWasStopped = false;
            leapAgentUpdatedPosition = true;
        }

        private void TryTraverseOffMeshLink()
        {
            if (!CanLeap() || !agent.isOnOffMeshLink)
            {
                return;
            }

            TryLeapTo(
                agent.currentOffMeshLinkData.endPos,
                traversalLeapSpeed,
                traversalLeapArcHeight);
        }

        private bool CanLeap()
        {
            return !isLeaping
                && !isKnockedBack
                && !IsMovementBlocked
                && CanUseAgent();
        }

        private void ConfigureAgent()
        {
            if (agent != null)
            {
                agent.speed = GetModifiedSpeed(moveSpeed);
                agent.autoTraverseOffMeshLink = false;
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

        public void ApplyKnockback(
            Vector3 direction,
            float distance,
            float duration,
            Action<Collider, Vector3> onCollision = null)
        {
            if (isLeaping)
            {
                return;
            }

            if (isCharging)
            {
                distance *= chargeKnockbackResistance;
                CancelCharge();
                if (distance <= Mathf.Epsilon)
                {
                    return;
                }
            }

            if (knockbackCoroutine != null)
            {
                StopCoroutine(knockbackCoroutine);
                knockbackCoroutine = null;
                EndKnockback();
            }

            knockbackCoroutine = StartCoroutine(ApplyKnockbackRoutine(
                direction,
                distance,
                duration,
                onCollision));
        }

        private IEnumerator ApplyKnockbackRoutine(
            Vector3 direction,
            float distance,
            float duration,
            Action<Collider, Vector3> onCollision)
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
                if (!TryApplyKnockbackStep(displacement, onCollision))
                {
                    break;
                }

                yield return null;
            }

            EndKnockback();
            knockbackCoroutine = null;
        }

        private bool TryApplyKnockbackStep(
            Vector3 displacement,
            Action<Collider, Vector3> onCollision)
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
            RaycastHit closestHit = default;
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
                closestHit = hit;
                foundObstacle = true;
            }

            if (foundObstacle)
            {
                transform.position += displacement.normalized * Mathf.Max(0f, closestDistance - 0.01f);
                onCollision?.Invoke(closestHit.collider, closestHit.point);
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

        public void ResetForSpawn()
        {
            ResetRuntimeState(stopAgent: false);
        }

        public void ResetForDespawn()
        {
            ResetRuntimeState(stopAgent: true);
        }

        private void ResetRuntimeState(bool stopAgent)
        {
            CacheRequiredComponents();

            CancelCharge();
            CancelLeap();

            if (knockbackCoroutine != null)
            {
                StopCoroutine(knockbackCoroutine);
                knockbackCoroutine = null;
            }

            isKnockedBack = false;
            hasDestination = false;
            lastDestination = default;
            movementBlockCount = 0;
            movementSpeedModifiers.Clear();
            ConfigureAgent();
            ResetAgent(stopAgent);
        }

        private void ResetAgent(bool stopAgent)
        {
            if (agent == null)
            {
                return;
            }

            if (!agent.enabled)
            {
                agent.enabled = true;
            }

            if (!CanUseAgent())
            {
                return;
            }

            agent.ResetPath();
            agent.Warp(transform.position);
            agent.isStopped = stopAgent;
        }

        private float GetModifiedSpeed(float baseSpeed)
        {
            return baseSpeed * movementSpeedModifiers.Multiplier;
        }

        public void BlockMovement()
        {
            movementBlockCount++;
            CancelCharge();
            Stop();
        }

        public void UnblockMovement()
        {
            movementBlockCount = Mathf.Max(0, movementBlockCount - 1);
        }

        public int AddMovementSpeedModifier(float multiplier)
        {
            int modifierId = movementSpeedModifiers.Add(multiplier);
            ConfigureAgent();
            return modifierId;
        }

        public void RemoveMovementSpeedModifier(int modifierId)
        {
            if (movementSpeedModifiers.Remove(modifierId))
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
            traversalLeapSpeed = Mathf.Max(0.01f, traversalLeapSpeed);
            traversalLeapArcHeight = Mathf.Max(0f, traversalLeapArcHeight);
            ConfigureAgent();
        }

    }
}
