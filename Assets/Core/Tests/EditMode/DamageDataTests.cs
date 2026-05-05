using System.Collections.Generic;
using NUnit.Framework;
using RPGame.Core.Damage;

namespace RPGame.Core.Tests
{
    public sealed class DamageDataTests
    {
        [Test]
        public void DamageData_WithPartialDamage_SumsAmount()
        {
            List<PartialDamage> parts = new()
            {
                new PartialDamage(3f, DamageType.Magical, DamageElement.None),
                new PartialDamage(5f, DamageType.Magical, DamageElement.Fire)
            };

            DamageData damageData = new DamageData(parts);

            Assert.AreEqual(8f, damageData.Amount);
            Assert.AreEqual(2, damageData.Parts.Count);
            Assert.IsTrue(damageData.HasDamage);
        }

        [Test]
        public void DamageData_WithZeroDamageParts_HasNoDamage()
        {
            List<PartialDamage> parts = new()
            {
                new PartialDamage(0f, DamageType.Physical, DamageElement.None)
            };

            DamageData damageData = new DamageData(parts);

            Assert.AreEqual(0f, damageData.Amount);
            Assert.AreEqual(0, damageData.Parts.Count);
            Assert.IsFalse(damageData.HasDamage);
        }
    }
}
