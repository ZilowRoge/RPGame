using RPGame.Core.Targeting;
using UnityEngine;

namespace RPGame.Enemies
{
    internal sealed class EnemySearchTarget
    {
        private const float SearchRadius = 3f;
        private const float ArrivalThreshold = 0.2f;

        private bool hasSearchPoint;
        private bool hasResolvedDestination;
        private Vector3 destination;

        public void MoveTowardsPlayerArea(IEnemyMovement movement)
        {
            if (!TryGetPlayerPosition(out Vector3 playerPosition))
            {
                Reset();
                movement.Stop();
                return;
            }

            if (!hasSearchPoint || (hasResolvedDestination && HasReachedDestination(movement)))
            {
                hasSearchPoint = true;
                hasResolvedDestination = movement.TryResolvePosition(
                    playerPosition + GetSearchOffset(),
                    out destination);
            }

            if (!hasResolvedDestination)
            {
                movement.Stop();
                return;
            }

            movement.MoveTo(destination);
        }

        public void Reset()
        {
            hasSearchPoint = false;
            hasResolvedDestination = false;
            destination = default;
        }

        private static bool TryGetPlayerPosition(out Vector3 playerPosition)
        {
            foreach (PlayerTargetable playerTarget in TargetRegistry.PlayerTargets)
            {
                if (playerTarget == null || playerTarget.TargetPoint == null)
                {
                    continue;
                }

                playerPosition = playerTarget.TargetPoint.position;
                return true;
            }

            playerPosition = default;
            return false;
        }

        private static Vector3 GetSearchOffset()
        {
            Vector2 offset = Random.insideUnitCircle * SearchRadius;
            return new Vector3(offset.x, 0f, offset.y);
        }

        private bool HasReachedDestination(IEnemyMovement movement)
        {
            return (movement.Position - destination).sqrMagnitude <= ArrivalThreshold * ArrivalThreshold;
        }
    }
}
