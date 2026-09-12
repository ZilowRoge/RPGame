using RPGame.Core.Effects;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    public sealed class EarthZoneBehaviour : TriggerStatusZoneBehaviour
    {
        [SerializeField] private SlowEffectDefinition slowEffect;
        [SerializeField] private float slowDuration = 2.5f;
        [SerializeField] private float reapplyInterval = 0.5f;

        protected override float ReapplyInterval => reapplyInterval;

        protected override void ApplyTo(IStatusEffectReceiver target)
        {
            if (slowEffect != null)
            {
                target.ApplyStatusEffect(slowEffect, slowDuration);
            }
        }

        private void OnValidate()
        {
            slowDuration = Mathf.Max(0f, slowDuration);
            reapplyInterval = Mathf.Max(0f, reapplyInterval);
        }
    }
}
