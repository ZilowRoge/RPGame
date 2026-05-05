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
        public void CasterData_WhenDamageIsProvided_PreservesDamage()
        {
            GameObject caster = new GameObject("CasterDataTests");
            List<PartialDamage> damage = new()
            {
                new PartialDamage(3f, DamageType.Magical, DamageElement.Fire)
            };

            try
            {
                CasterData casterData = new CasterData(caster, caster.transform, null, damage);

                Assert.AreSame(caster, casterData.CasterObject);
                Assert.AreEqual(1, casterData.Damage.Count);
                Assert.AreEqual(3f, casterData.Damage[0].Amount);
                Assert.AreEqual(DamageElement.Fire, casterData.Damage[0].DamageElement);
            }
            finally
            {
                Object.DestroyImmediate(caster);
            }
        }

        [Test]
        public void CasterData_WhenDamageIsNotProvided_UsesEmptyDamage()
        {
            CasterData casterData = new CasterData(null, null, null);

            Assert.IsNotNull(casterData.Damage);
            Assert.AreEqual(0, casterData.Damage.Count);
        }

        [Test]
        public void CasterDataBuilder_WithDamage_BuildsCasterDataWithDamage()
        {
            GameObject caster = new GameObject("CasterDataBuilderTests");
            List<PartialDamage> damage = new()
            {
                new PartialDamage(4f, DamageType.Magical, DamageElement.Light)
            };

            try
            {
                CasterData casterData = new CasterDataBuilder(caster, caster.transform, null)
                    .WithDamage(damage)
                    .Build();

                Assert.AreSame(caster, casterData.CasterObject);
                Assert.AreSame(caster.transform, casterData.CastOrigin);
                Assert.AreEqual(1, casterData.Damage.Count);
                Assert.AreEqual(4f, casterData.Damage[0].Amount);
                Assert.AreEqual(DamageElement.Light, casterData.Damage[0].DamageElement);
            }
            finally
            {
                Object.DestroyImmediate(caster);
            }
        }
    }
}
