using RPGame.Core.Effects;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    public sealed class FireZoneBehaviour : TriggerStatusZoneBehaviour
    {
        [SerializeField] private BurningEffectDefinition burningEffect;
        [SerializeField] private float burningDuration = 3f;
        [SerializeField] private float reapplyInterval = 0.5f;

        protected override float ReapplyInterval => reapplyInterval;

        protected override void ApplyTo(IStatusEffectReceiver target)
        {
            if (burningEffect != null)
            {
                target.ApplyStatusEffect(burningEffect, burningDuration);
            }
        }

        private void OnValidate()
        {
            burningDuration = Mathf.Max(0f, burningDuration);
            reapplyInterval = Mathf.Max(0f, reapplyInterval);
        }
    }
}
