using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Core.Damage
{
    public readonly struct DamageData
    {
        private static readonly IReadOnlyList<PartialDamage> EmptyParts = Array.Empty<PartialDamage>();

        public DamageData(IReadOnlyList<PartialDamage> parts, GameObject source = null)
        {
            Parts = CopyDamageParts(parts);
            Source = source;
            Amount = CalculateAmount(Parts);
        }

        public float Amount { get; }
        public GameObject Source { get; }
        public IReadOnlyList<PartialDamage> Parts { get; }

        public bool HasDamage => Amount > 0f;

        private static IReadOnlyList<PartialDamage> CopyDamageParts(IReadOnlyList<PartialDamage> parts)
        {
            if (parts == null || parts.Count == 0)
            {
                return EmptyParts;
            }

            List<PartialDamage> copiedParts = new(parts.Count);
            for (int i = 0; i < parts.Count; i++)
            {
                PartialDamage part = parts[i];
                if (part.HasDamage)
                {
                    copiedParts.Add(part);
                }
            }

            return copiedParts.Count > 0 ? copiedParts : EmptyParts;
        }

        private static float CalculateAmount(IReadOnlyList<PartialDamage> parts)
        {
            float amount = 0f;
            for (int i = 0; i < parts.Count; i++)
            {
                amount += parts[i].Amount;
            }

            return Mathf.Max(0f, amount);
        }
    }
}
