using System;
using System.Collections.Generic;
using RPGame.Core.Damage;
using RPGame.Core.Statistics;
using RPGame.Core.Statistics.Attributes;
using UnityEngine;

namespace RPGame.Core.Spells
{
    public sealed class CasterDataBuilder
    {
        private static readonly IReadOnlyList<PartialDamageRange> EmptyDamageRanges = Array.Empty<PartialDamageRange>();

        private readonly GameObject casterObject;
        private readonly Transform castOrigin;
        private readonly Transform target;
        private IReadOnlyList<PartialDamageRange> damageRanges = EmptyDamageRanges;
        private ICharacterAttributes attributes;
        private IStatisticsController statistics;

        public CasterDataBuilder(GameObject casterObject, Transform castOrigin, Transform target)
        {
            this.casterObject = casterObject;
            this.castOrigin = castOrigin;
            this.target = target;
        }

        public CasterDataBuilder WithAttributes(ICharacterAttributes attributes)
        {
            this.attributes = attributes;
            return this;
        }

        public CasterDataBuilder WithStatistics(IStatisticsController statistics)
        {
            this.statistics = statistics;
            return this;
        }

        public CasterDataBuilder WithDamageRanges(IReadOnlyList<PartialDamageRange> damageRanges)
        {
            this.damageRanges = damageRanges ?? EmptyDamageRanges;
            return this;
        }

        public CasterData Build()
        {
            return new CasterData(casterObject, castOrigin, target, attributes, statistics, damageRanges);
        }
    }
}
