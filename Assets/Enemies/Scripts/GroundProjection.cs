using UnityEngine;

namespace RPGame.Enemies
{
    public sealed class GroundProjection : MonoBehaviour, IEnemyGroundProjection
    {
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float raycastStartHeight = 5f;
        [SerializeField] private float raycastDistance = 20f;

        public bool TryProjectToGround(Vector3 candidatePosition, out Vector3 groundPosition)
        {
            Vector3 origin = candidatePosition + Vector3.up * raycastStartHeight;
            if (Physics.Raycast(
                origin,
                Vector3.down,
                out RaycastHit hit,
                raycastDistance,
                groundMask,
                QueryTriggerInteraction.Ignore))
            {
                groundPosition = hit.point;
                return true;
            }

            groundPosition = default;
            return false;
        }

        private void OnValidate()
        {
            raycastStartHeight = Mathf.Max(0f, raycastStartHeight);
            raycastDistance = Mathf.Max(0.001f, raycastDistance);
        }
    }
}
