using System;
using System.Collections.Generic;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Core.Spells
{
    public sealed class CasterDataBuilder
    {
        private static readonly IReadOnlyList<PartialDamage> EmptyDamage = Array.Empty<PartialDamage>();

        private readonly GameObject casterObject;
        private readonly Transform castOrigin;
        private readonly Transform target;
        private IReadOnlyList<PartialDamage> damage = EmptyDamage;

        public CasterDataBuilder(GameObject casterObject, Transform castOrigin, Transform target)
        {
            this.casterObject = casterObject;
            this.castOrigin = castOrigin;
            this.target = target;
        }

        public CasterDataBuilder WithDamage(IReadOnlyList<PartialDamage> damage)
        {
            this.damage = damage ?? EmptyDamage;
            return this;
        }

        public CasterData Build()
        {
            return new CasterData(casterObject, castOrigin, target, damage);
        }
    }
}
