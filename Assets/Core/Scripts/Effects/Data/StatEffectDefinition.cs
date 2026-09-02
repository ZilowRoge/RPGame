using System.Globalization;
using System.Text;
using UnityEngine;

namespace RPGame.Core.Effects
{
    [CreateAssetMenu(fileName = "StatEffect", menuName = "RPGame/Progression/Effects/Stat Effect")]
    public sealed class StatEffectDefinition : PassiveEffectDefinition
    {
        [SerializeField] private EffectStat stat = EffectStat.MaxHealth;
        [SerializeField] private EffectModifierType modifierType = EffectModifierType.Flat;
        [SerializeField] private float value = 10f;

        public EffectStat Stat => stat;
        public EffectModifierType ModifierType => modifierType;
        public float Value => value;

        public override string ToString()
        {
            return $"{FormatValue()} {FormatStatName(stat)}";
        }

        private string FormatValue()
        {
            float displayValue = modifierType == EffectModifierType.Percent ? value * 100f : value;
            string sign = displayValue >= 0f ? "+" : string.Empty;
            string suffix = modifierType == EffectModifierType.Percent ? "%" : string.Empty;
            return $"{sign}{displayValue.ToString("0.##", CultureInfo.InvariantCulture)}{suffix}";
        }

        private static string FormatStatName(EffectStat effectStat)
        {
            string statName = effectStat.ToString();
            StringBuilder builder = new StringBuilder(statName.Length + 4);

            for (int i = 0; i < statName.Length; i++)
            {
                char character = statName[i];
                if (i > 0 && char.IsUpper(character) && !char.IsUpper(statName[i - 1]))
                {
                    builder.Append(' ');
                }

                builder.Append(character);
            }

            return builder.ToString();
        }
    }
}
