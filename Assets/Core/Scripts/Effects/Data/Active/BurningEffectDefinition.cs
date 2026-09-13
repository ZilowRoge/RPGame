using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Core.Effects
{
    [CreateAssetMenu(fileName = "BurningEffect", menuName = "RPGame/Progression/Effects/Burning Effect")]
    public sealed class BurningEffectDefinition : StatusEffectDefinition, IPeriodicStatusEffect
    {
        private const float MinimumTickInterval = 0.0001f;

        [SerializeField] private float amountPerInterval = 1.5f;
        [SerializeField] private float tickInterval = 1f;

        public override ReapplyPolicy ReapplyPolicy => ReapplyPolicy.Refresh;
        public float Amount => Mathf.Max(0f, amountPerInterval);
        public float TickInterval => Mathf.Max(MinimumTickInterval, tickInterval);

        public override bool CanApply(StatusEffectTarget target)
        {
            return target.Damageable != null;
        }

        public void Tick(StatusEffectTarget target)
        {
            IDamageable damageable = target.Damageable;
            if (damageable == null || !damageable.CanReceiveDamage || Amount <= 0f)
            {
                return;
            }

            damageable.ApplyDamage(new DamageData(new[]
            {
                new PartialDamage(Amount, DamageType.Magical, DamageElement.Fire)
            }));
        }

        public override string ToString()
        {
            return $"Burning {Amount:0.##}";
        }

        private void OnValidate()
        {
            amountPerInterval = Mathf.Max(0f, amountPerInterval);
            tickInterval = Mathf.Max(MinimumTickInterval, tickInterval);
        }
    }
}
