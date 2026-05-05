using System;
using System.Collections.Generic;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Core.Spells
{
    public readonly struct CasterData
    {
        private static readonly IReadOnlyList<PartialDamage> EmptyDamage = Array.Empty<PartialDamage>();

        public CasterData(
            GameObject casterObject,
            Transform castOrigin,
            Transform target,
            IReadOnlyList<PartialDamage> damage = null)
        {
            CasterObject = casterObject;
            CastOrigin = castOrigin;
            Target = target;
            Damage = CopyDamage(damage);
        }

        public GameObject CasterObject { get; }
        public Transform CastOrigin { get; }
        public Transform Target { get; }
        public IReadOnlyList<PartialDamage> Damage { get; }

        private static IReadOnlyList<PartialDamage> CopyDamage(IReadOnlyList<PartialDamage> damage)
        {
            if (damage == null || damage.Count == 0)
            {
                return EmptyDamage;
            }

            List<PartialDamage> copiedDamage = new(damage.Count);
            for (int i = 0; i < damage.Count; i++)
            {
                if (damage[i].HasDamage)
                {
                    copiedDamage.Add(damage[i]);
                }
            }

            return copiedDamage.Count > 0 ? copiedDamage : EmptyDamage;
        }
    }
}
