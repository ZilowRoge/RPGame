using System.Collections.Generic;
using RPGame.Core.Targeting;
using UnityEngine;

namespace RPGame.Player.Targeting
{
    public static class TargetSelector
    {
        public readonly struct Candidate
        {
            public Candidate(ITargetable target, float score)
            {
                Target = target;
                Score = score;
            }

            public ITargetable Target { get; }
            public float Score { get; }
            public bool HasTarget => Target != null;
        }

        public static ITargetable SelectBest(
            IReadOnlyList<ITargetable> targets,
            Camera playerCamera,
            Vector3 origin,
            float maxTargetDistance,
            float targetingRadius)
        {
            return SelectBestCandidate(targets, playerCamera, origin, maxTargetDistance, targetingRadius).Target;
        }

        public static Candidate SelectBestCandidate(
            IReadOnlyList<ITargetable> targets,
            Camera playerCamera,
            Vector3 origin,
            float maxTargetDistance,
            float targetingRadius)
        {
            if (targets == null || playerCamera == null)
            {
                return default;
            }

            ITargetable bestTarget = null;
            float bestScore = float.MaxValue;

            for (int i = 0; i < targets.Count; i++)
            {
                ITargetable target = targets[i];

                if (!TryEvaluate(target, playerCamera, origin, maxTargetDistance, out float score))
                {
                    continue;
                }

                if (score > targetingRadius || score >= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestTarget = target;
            }

            return bestTarget != null ? new Candidate(bestTarget, bestScore) : default;
        }

        public static bool TryEvaluate(
            ITargetable target,
            Camera playerCamera,
            Vector3 origin,
            float maxTargetDistance,
            out float score)
        {
            score = 0f;

            if (IsMissing(target) || target.TargetPoint == null || playerCamera == null)
            {
                return false;
            }

            Vector3 targetPosition = target.TargetPoint.position;
            float maxTargetDistanceSqr = maxTargetDistance * maxTargetDistance;

            if ((targetPosition - origin).sqrMagnitude > maxTargetDistanceSqr)
            {
                return false;
            }

            Vector3 viewportPosition = playerCamera.WorldToViewportPoint(targetPosition);

            if (viewportPosition.z <= 0f
                || viewportPosition.x < 0f
                || viewportPosition.x > 1f
                || viewportPosition.y < 0f
                || viewportPosition.y > 1f)
            {
                return false;
            }

            float viewportOffsetX = (viewportPosition.x - 0.5f) * playerCamera.aspect;
            float viewportOffsetY = viewportPosition.y - 0.5f;
            score = Mathf.Sqrt(viewportOffsetX * viewportOffsetX + viewportOffsetY * viewportOffsetY);
            return true;
        }

        private static bool IsMissing(ITargetable target)
        {
            return target == null || target is Object unityObject && unityObject == null;
        }
    }
}

