using NUnit.Framework;
using RPGame.Combat.Spells;

namespace RPGame.Combat.Tests
{
    public sealed class ProjectileSpellDamageTests
    {
        [Test]
        public void CalculatePowerDamageBonus_WhenPowerIsPositive_ReturnsScaledBonus()
        {
            float damageBonus = SpellDamageCalculator.CalculatePowerDamageBonus(6, 1.5f);

            Assert.AreEqual(9f, damageBonus);
        }
    }
}
