using System.Collections.Generic;
using NUnit.Framework;
using RPGame.Core.Damage;

namespace RPGame.Core.Tests
{
    public sealed class DamageRangeRollerTests
    {
        [Test]
        public void Roll_WhenDamageRangesAreNull_ReturnsEmptyParts()
        {
            IReadOnlyList<PartialDamage> damageParts = DamageRangeRoller.Roll(null);

            Assert.AreEqual(0, damageParts.Count);
        }

        [Test]
        public void Roll_BuildsPartialDamageFromRanges()
        {
            PartialDamageRange[] damageRanges =
            {
                new(5f, 5f, DamageType.Physical, DamageElement.None),
                new(3f, 3f, DamageType.Magical, DamageElement.Fire)
            };

            IReadOnlyList<PartialDamage> damageParts = DamageRangeRoller.Roll(damageRanges);

            Assert.AreEqual(2, damageParts.Count);
            Assert.AreEqual(5f, damageParts[0].Amount);
            Assert.AreEqual(DamageType.Physical, damageParts[0].DamageType);
            Assert.AreEqual(DamageElement.None, damageParts[0].DamageElement);
            Assert.AreEqual(3f, damageParts[1].Amount);
            Assert.AreEqual(DamageType.Magical, damageParts[1].DamageType);
            Assert.AreEqual(DamageElement.Fire, damageParts[1].DamageElement);
        }
    }
}
