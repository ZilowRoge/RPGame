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

        private void Awake()
        {
            ConfigureAgent();
        }

        public void MoveTo(Vector3 position)
        {
            NavMeshAgent resolvedAgent = ResolveAgent();
            if (!CanUseAgent(resolvedAgent))
            {
                return;
            }

            ConfigureAgent();

            if (hasDestination && IsSameDestination(position))
            {
                resolvedAgent.isStopped = false;
                return;
            }

            resolvedAgent.isStopped = false;
            if (resolvedAgent.SetDestination(position))
            {
                lastDestination = position;
                hasDestination = true;
            }
        }

        public void Stop()
        {
            NavMeshAgent resolvedAgent = ResolveAgent();
            if (!CanUseAgent(resolvedAgent))
            {
                return;
            }

            resolvedAgent.isStopped = true;
        }

        private void ConfigureAgent()
        {
            NavMeshAgent resolvedAgent = ResolveAgent();
            if (resolvedAgent != null)
            {
                resolvedAgent.speed = moveSpeed;
            }
        }

        private NavMeshAgent ResolveAgent()
        {
            if (agent == null)
            {
                agent = GetComponent<NavMeshAgent>();
            }

            return agent;
        }

        private bool CanUseAgent(NavMeshAgent resolvedAgent)
        {
            return resolvedAgent != null
                && resolvedAgent.enabled
                && resolvedAgent.gameObject.activeInHierarchy
                && resolvedAgent.isOnNavMesh;
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
