using System;
using System.Collections.Generic;
using RPGame.Core.Damage;
using RPGame.Core.Statistics.Attributes;
using UnityEngine;

namespace RPGame.Core.Spells
{
    public readonly struct CasterData
    {
        private static readonly IReadOnlyList<PartialDamageRange> EmptyDamageRanges = Array.Empty<PartialDamageRange>();

        public CasterData(
            GameObject casterObject,
            Transform castOrigin,
            Transform target,
            ICharacterAttributes attributes = null,
            IReadOnlyList<PartialDamageRange> damageRanges = null)
        {
            CasterObject = casterObject;
            CastOrigin = castOrigin;
            Target = target;
            Attributes = attributes;
            DamageRanges = CopyDamageRanges(damageRanges);
        }

        public GameObject CasterObject { get; }
        public Transform CastOrigin { get; }
        public Transform Target { get; }
        public ICharacterAttributes Attributes { get; }
        public IReadOnlyList<PartialDamageRange> DamageRanges { get; }

        private static IReadOnlyList<PartialDamageRange> CopyDamageRanges(IReadOnlyList<PartialDamageRange> damageRanges)
        {
            if (damageRanges == null || damageRanges.Count == 0)
            {
                return EmptyDamageRanges;
            }

            List<PartialDamageRange> copiedDamageRanges = new(damageRanges.Count);
            for (int i = 0; i < damageRanges.Count; i++)
            {
                if (damageRanges[i].MaxDamage > 0f)
                {
                    copiedDamageRanges.Add(damageRanges[i]);
                }
            }

            return copiedDamageRanges.Count > 0 ? copiedDamageRanges : EmptyDamageRanges;
        }
    }
}
