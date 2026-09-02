using UnityEngine;
using UnityEngine.AI;

namespace RPGame.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class Movement : MonoBehaviour, IEnemyMovement
    {
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float destinationChangeThreshold = 0.05f;

        private NavMeshAgent agent;
        private Vector3 lastDestination;
        private bool hasDestination;

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
            if (!CanUseAgent())
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
            if (!CanUseAgent())
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
                agent.speed = moveSpeed;
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
