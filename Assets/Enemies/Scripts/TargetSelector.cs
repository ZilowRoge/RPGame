using System.Collections.Generic;
using RPGame.Core.Targeting;
using UnityEngine;

namespace RPGame.Enemies
{
    public static class TargetSelector
    {
        public static ITargetable SelectNearest(
            IReadOnlyList<PlayerTargetable> targets,
            Transform origin,
            float detectionRange)
        {
            if (targets == null || origin == null)
            {
                return null;
            }

            ITargetable nearestTarget = null;
            float nearestDistanceSqr = detectionRange * detectionRange;

            for (int i = 0; i < targets.Count; i++)
            {
                PlayerTargetable target = targets[i];
                if (!IsValidTarget(target, origin))
                {
                    continue;
                }

                float distanceSqr = (target.TargetPoint.position - origin.position).sqrMagnitude;
                if (distanceSqr > nearestDistanceSqr)
                {
                    continue;
                }

                nearestTarget = target;
                nearestDistanceSqr = distanceSqr;
            }

            return nearestTarget;
        }

        private static bool IsValidTarget(PlayerTargetable target, Transform origin)
        {
            if (target == null || target.TargetPoint == null)
            {
                return false;
            }

            if (!target.TargetPoint.gameObject.activeInHierarchy)
            {
                return false;
            }

            if (target.transform.IsChildOf(origin))
            {
                return false;
            }

            return true;
        }
    }
}
