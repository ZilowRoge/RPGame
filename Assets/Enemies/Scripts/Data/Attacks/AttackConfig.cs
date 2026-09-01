using System.Collections.Generic;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Enemies
{
    public abstract class AttackConfig : ScriptableObject
    {
        [SerializeField] private float attackInterval = 1f;
        [SerializeField] private List<PartialDamageRange> damage = new()
        {
            new PartialDamageRange(5f, 5f, DamageType.Physical, DamageElement.None)
        };

        public float AttackInterval => attackInterval;
        public IReadOnlyList<PartialDamageRange> Damage => damage;

        protected virtual void OnValidate()
        {
            attackInterval = Mathf.Max(0f, attackInterval);
        }
    }
}
