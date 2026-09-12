using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Core.Effects
{
    [CreateAssetMenu(fileName = "BurningEffect", menuName = "RPGame/Progression/Effects/Burning Effect")]
    public sealed class BurningEffectDefinition : StatusEffectDefinition, IAmountStatusEffect
    {
        [SerializeField] private float amount = 6f;

        public override ReapplyPolicy ReapplyPolicy => ReapplyPolicy.Refresh;
        public float Amount => Mathf.Max(0f, amount);

        public override bool CanApply(StatusEffectTarget target)
        {
            return target.Damageable != null;
        }

        public void Tick(StatusEffectTarget target, float deltaTime, float amount)
        {
            IDamageable damageable = target.Damageable;
            if (damageable == null || !damageable.CanReceiveDamage || amount <= 0f)
            {
                return;
            }

            damageable.ApplyDamage(new DamageData(new[]
            {
                new PartialDamage(amount, DamageType.Magical, DamageElement.Fire)
            }));
        }

        public override string ToString()
        {
            return $"Burning {Amount:0.##}";
        }

        private void OnValidate()
        {
            amount = Mathf.Max(0f, amount);
        }
    }
}
