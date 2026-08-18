using System.Collections.Generic;
using RPGame.Core.Damage;
using RPGame.Core.Targeting;
using UnityEngine;

namespace RPGame.Enemies
{
    public sealed class Attack : MonoBehaviour
    {
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackInterval = 1f;
        [SerializeField] private List<PartialDamageRange> damage = new()
        {
            new PartialDamageRange(5f, 5f, DamageType.Physical, DamageElement.None)
        };

        private float nextAttackTime;

        public float AttackRange => attackRange;
        public float AttackInterval => attackInterval;
        public bool CanAttack => Time.time >= nextAttackTime;

        public bool IsInRange(ITargetable target)
        {
            if (target == null || target.TargetPoint == null)
            {
                return false;
            }

            float attackRangeSqr = attackRange * attackRange;
            return (target.TargetPoint.position - transform.position).sqrMagnitude <= attackRangeSqr;
        }

        public bool TryAttack(ITargetable target)
        {
            if (!CanAttack || !IsInRange(target) || !TryGetDamageable(target, out IDamageable damageable))
            {
                return false;
            }

            DamageResult result = damageable.ApplyDamage(BuildDamageData());
            if (!result.WasApplied)
            {
                return false;
            }

            nextAttackTime = Time.time + attackInterval;
            return true;
        }

        private DamageData BuildDamageData()
        {
            List<PartialDamage> damageParts = new();
            if (damage != null)
            {
                for (int i = 0; i < damage.Count; i++)
                {
                    PartialDamageRange damageRange = damage[i];
                    int minDamage = Mathf.CeilToInt(damageRange.MinDamage);
                    int maxDamage = Mathf.Max(minDamage, Mathf.FloorToInt(damageRange.MaxDamage));
                    float amount = Random.Range(minDamage, maxDamage + 1);

                    damageParts.Add(new PartialDamage(
                        amount,
                        damageRange.DamageType,
                        damageRange.DamageElement));
                }
            }

            return new DamageData(damageParts, gameObject);
        }

        private static bool TryGetDamageable(ITargetable target, out IDamageable damageable)
        {
            damageable = null;
            if (target?.TargetPoint == null)
            {
                return false;
            }

            damageable = target.TargetPoint.GetComponentInParent<IDamageable>();
            return damageable != null && damageable.CanReceiveDamage;
        }

        private void OnValidate()
        {
            attackRange = Mathf.Max(0f, attackRange);
            attackInterval = Mathf.Max(0f, attackInterval);
        }
    }
}
