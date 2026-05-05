using UnityEngine;

namespace RPGame.Core.Damage
{
    public readonly struct PartialDamage
    {
        public PartialDamage(float amount, DamageType damageType, DamageElement damageElement)
        {
            Amount = Mathf.Max(0f, amount);
            DamageType = damageType;
            DamageElement = damageElement;
        }

        public float Amount { get; }
        public DamageType DamageType { get; }
        public DamageElement DamageElement { get; }
        public bool HasDamage => Amount > 0f;
    }
}
