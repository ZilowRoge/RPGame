using System;
using UnityEngine;

namespace RPGame.Core.Damage
{
    [Serializable]
    public struct PartialDamageRange
    {
        [SerializeField] private float minDamage;
        [SerializeField] private float maxDamage;
        [SerializeField] private DamageType damageType;
        [SerializeField] private DamageElement damageElement;

        public PartialDamageRange(
            float minDamage,
            float maxDamage,
            DamageType damageType,
            DamageElement damageElement)
        {
            this.minDamage = minDamage;
            this.maxDamage = maxDamage;
            this.damageType = damageType;
            this.damageElement = damageElement;
        }

        public float MinDamage => Mathf.Max(0f, minDamage);
        public float MaxDamage => Mathf.Max(MinDamage, maxDamage);
        public DamageType DamageType => damageType;
        public DamageElement DamageElement => damageElement;
    }
}
