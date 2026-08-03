using System.Collections.Generic;
using NUnit.Framework;
using RPGame.Core.Damage;
using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Core.Tests
{
    public sealed class CasterDataTests
    {
        [Test]
        public void CasterData_WhenDamageRangesAreNotProvided_UsesEmptyDamageRanges()
        {
            CasterData casterData = new CasterData(null, null, null);

            Assert.IsNotNull(casterData.DamageRanges);
            Assert.AreEqual(0, casterData.DamageRanges.Count);
        }

        [Test]
        public void CasterDataBuilder_WithDamageRanges_BuildsCasterDataWithDamageRanges()
        {
            GameObject caster = new GameObject("CasterDataBuilderDamageRangesTests");
            List<PartialDamageRange> damageRanges = new()
            {
                new PartialDamageRange(5f, 9f, DamageType.Magical, DamageElement.Fire)
            };

            try
            {
                CasterData casterData = new CasterDataBuilder(caster, caster.transform, null)
                    .WithDamageRanges(damageRanges)
                    .Build();

                Assert.AreSame(caster, casterData.CasterObject);
                Assert.AreEqual(1, casterData.DamageRanges.Count);
                Assert.AreEqual(5f, casterData.DamageRanges[0].MinDamage);
                Assert.AreEqual(9f, casterData.DamageRanges[0].MaxDamage);
                Assert.AreEqual(DamageElement.Fire, casterData.DamageRanges[0].DamageElement);
            }
            finally
            {
                Object.DestroyImmediate(caster);
            }
        }
    }
}
