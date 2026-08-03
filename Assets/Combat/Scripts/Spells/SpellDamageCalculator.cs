using UnityEngine;

namespace RPGame.Combat.Spells
{
    public static class SpellDamageCalculator
    {
        public static float CalculatePowerDamageBonus(int power, float powerScaling)
        {
            return Mathf.Max(0f, power * powerScaling);
        }
    }
}
