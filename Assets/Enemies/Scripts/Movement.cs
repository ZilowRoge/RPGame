using UnityEngine;
using UnityEngine.AI;

namespace RPGame.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class Movement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float destinationChangeThreshold = 0.05f;

        private NavMeshAgent agent;
        private Vector3 lastDestination;
        private bool hasDestination;

        public float MoveSpeed => moveSpeed;

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

        public void MoveTo(Vector3 position)
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

        public void Stop()
        {
            if (!CanUseAgent())
            {
                return;
            }

            agent.isStopped = true;
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
    }
}
