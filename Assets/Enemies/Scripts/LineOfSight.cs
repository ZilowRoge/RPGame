using UnityEngine;

namespace RPGame.Enemies
{
    public sealed class LineOfSight : MonoBehaviour, IEnemyLineOfSight
    {
        private const float MinRaycastDistance = 0.001f;

        [SerializeField] private Transform origin;
        [SerializeField] private LayerMask obstacleMask;

        bool IEnemyLineOfSight.HasLineOfSight(Vector3 targetPosition)
        {
            Vector3 originPosition = origin != null ? origin.position : transform.position;
            Vector3 ray = targetPosition - originPosition;
            float distance = ray.magnitude;
            if (distance <= MinRaycastDistance)
            {
                return true;
            }

            return !Physics.Raycast(
                originPosition,
                ray / distance,
                distance,
                obstacleMask,
                QueryTriggerInteraction.Ignore);
        }
    }
}
