using RPGame.Core.Statuses;
using UnityEngine;
using UnityEngine.Serialization;

namespace RPGame.Combat.Spells
{
    public sealed class FireZoneBehaviour : TriggerStatusZoneBehaviour
    {
        [FormerlySerializedAs("burningEffect")]
        [SerializeField] private BurningStatusDefinition burningStatus;
        [SerializeField] private float burningDuration = 3f;
        [SerializeField] private float reapplyInterval = 0.5f;

        protected override float ReapplyInterval => reapplyInterval;

        protected override void ApplyTo(IStatusReceiver target)
        {
            if (burningStatus != null)
            {
                target.ApplyStatus(
                    burningStatus,
                    burningDuration,
                    new StatusContext(
                        new StatusSourceId(nameof(FireZoneBehaviour)),
                        StatusSource));
            }
        }

        private void OnValidate()
        {
            burningDuration = Mathf.Max(0f, burningDuration);
            reapplyInterval = Mathf.Max(0f, reapplyInterval);
        }
    }
}
