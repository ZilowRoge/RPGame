using RPGame.Core.Statuses;
using UnityEngine;
using UnityEngine.Serialization;

namespace RPGame.Combat.Spells
{
    public sealed class EarthZoneBehaviour : TriggerStatusZoneBehaviour
    {
        [FormerlySerializedAs("slowEffect")]
        [SerializeField] private SlowStatusDefinition slowStatus;
        [SerializeField] private float slowDuration = 2.5f;
        [SerializeField] private float reapplyInterval = 0.5f;

        protected override float ReapplyInterval => reapplyInterval;

        protected override void ApplyTo(IStatusReceiver target)
        {
            if (slowStatus != null)
            {
                target.ApplyStatus(
                    slowStatus,
                    slowDuration,
                    new StatusContext(
                        new StatusSourceId(nameof(EarthZoneBehaviour)),
                        StatusSource));
            }
        }

        private void OnValidate()
        {
            slowDuration = Mathf.Max(0f, slowDuration);
            reapplyInterval = Mathf.Max(0f, reapplyInterval);
        }
    }
}
