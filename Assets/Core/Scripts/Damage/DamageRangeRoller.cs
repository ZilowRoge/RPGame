using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Core.Damage
{
    public static class DamageRangeRoller
    {
        public static IReadOnlyList<PartialDamage> Roll(IReadOnlyList<PartialDamageRange> damageRanges)
        {
            if (damageRanges == null || damageRanges.Count == 0)
            {
                return Array.Empty<PartialDamage>();
            }

            List<PartialDamage> damageParts = new(damageRanges.Count);
            for (int i = 0; i < damageRanges.Count; i++)
            {
                PartialDamageRange damageRange = damageRanges[i];
                int minDamage = Mathf.CeilToInt(damageRange.MinDamage);
                int maxDamage = Mathf.Max(minDamage, Mathf.FloorToInt(damageRange.MaxDamage));
                float amount = UnityEngine.Random.Range(minDamage, maxDamage + 1);

                damageParts.Add(new PartialDamage(
                    amount,
                    damageRange.DamageType,
                    damageRange.DamageElement));
            }

            return damageParts;
        }
    }
}
