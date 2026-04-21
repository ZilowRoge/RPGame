using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Core.Interaction
{
    public static class SelectionUtility
    {
        public static IInteractable SelectBest(
            IReadOnlyList<IInteractable> candidates,
            InteractionContext context,
            Vector3 originPosition,
            Vector3 originForward,
            float minimumForwardDot,
            float forwardWeight,
            float distanceWeight)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            Vector3 normalizedForward = originForward.sqrMagnitude > 0.0001f ? originForward.normalized : Vector3.forward;
            IInteractable bestInteractable = null;
            float bestScore = float.NegativeInfinity;

            for (int i = 0; i < candidates.Count; i++)
            {
                IInteractable candidate = candidates[i];

                if (candidate == null || candidate.InteractionTransform == null || !candidate.CanInteract(context))
                {
                    continue;
                }

                Vector3 toInteractable = candidate.InteractionTransform.position - originPosition;
                float distance = Mathf.Max(toInteractable.magnitude, 0.001f);
                float forwardScore = Vector3.Dot(normalizedForward, toInteractable / distance);

                if (forwardScore < minimumForwardDot)
                {
                    continue;
                }

                float score = forwardScore * forwardWeight - distance * distanceWeight;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestInteractable = candidate;
                }
            }

            return bestInteractable;
        }
    }
}
