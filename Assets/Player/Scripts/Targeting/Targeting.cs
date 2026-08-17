using RPGame.Core.Targeting;
using UnityEngine;

namespace RPGame.Player.Targeting
{
    public sealed class Targeting : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float maxTargetDistance = 25f;
        [SerializeField, Range(0f, 1f)] private float targetingRadius = 0.2f;

        public ITargetable CurrentTarget { get; private set; }

        private void LateUpdate()
        {
            ITargetable selectedTarget = TargetSelector.SelectBest(
                TargetRegistry.Targets,
                playerCamera,
                transform.position,
                maxTargetDistance,
                targetingRadius);

            if (selectedTarget == CurrentTarget)
            {
                return;
            }

            CurrentTarget = selectedTarget;
            Debug.Log($"[Targeting] Current target: {FormatTarget(CurrentTarget)}", this);
        }

        private void OnValidate()
        {
            maxTargetDistance = Mathf.Max(0f, maxTargetDistance);
            targetingRadius = Mathf.Clamp01(targetingRadius);
        }

        private static string FormatTarget(ITargetable target)
        {
            if (target == null)
            {
                return "None";
            }

            if (target is Object unityObject)
            {
                return unityObject.name;
            }

            return target.TargetPoint != null ? target.TargetPoint.name : target.ToString();
        }
    }
}

