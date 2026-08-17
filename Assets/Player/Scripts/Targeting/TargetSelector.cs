using System.Collections.Generic;
using RPGame.Core.Targeting;
using UnityEngine;

namespace RPGame.Player.Targeting
{
    public static class TargetSelector
    {
        public static ITargetable SelectBest(
            IReadOnlyList<ITargetable> targets,
            Camera playerCamera,
            Vector3 origin,
            float maxTargetDistance,
            float targetingRadius)
        {
            if (targets == null || playerCamera == null)
            {
                return null;
            }

            ITargetable bestTarget = null;
            float bestViewportDistanceSqr = float.MaxValue;
            float maxTargetDistanceSqr = maxTargetDistance * maxTargetDistance;
            float targetingRadiusSqr = targetingRadius * targetingRadius;

            for (int i = 0; i < targets.Count; i++)
            {
                ITargetable target = targets[i];

                if (target == null || target.TargetPoint == null)
                {
                    continue;
                }

                Vector3 targetPosition = target.TargetPoint.position;

                if ((targetPosition - origin).sqrMagnitude > maxTargetDistanceSqr)
                {
                    continue;
                }

                Vector3 viewportPosition = playerCamera.WorldToViewportPoint(targetPosition);

                if (viewportPosition.z <= 0f
                    || viewportPosition.x < 0f
                    || viewportPosition.x > 1f
                    || viewportPosition.y < 0f
                    || viewportPosition.y > 1f)
                {
                    continue;
                }

                float viewportOffsetX = (viewportPosition.x - 0.5f) * playerCamera.aspect;
                float viewportOffsetY = viewportPosition.y - 0.5f;
                float viewportDistanceSqr = viewportOffsetX * viewportOffsetX + viewportOffsetY * viewportOffsetY;

                if (viewportDistanceSqr > targetingRadiusSqr || viewportDistanceSqr >= bestViewportDistanceSqr)
                {
                    continue;
                }

                bestViewportDistanceSqr = viewportDistanceSqr;
                bestTarget = target;
            }

            return bestTarget;
        }
    }
}

