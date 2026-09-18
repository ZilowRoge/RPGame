using System.Globalization;
using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Core.Effects
{
    [CreateAssetMenu(
        fileName = "SpellPropertyModifierEffect",
        menuName = "RPGame/Progression/Effects/Spell Property Modifier")]
    public sealed class SpellPropertyModifierEffectDefinition : PassiveEffectDefinition
    {
        [SerializeField] private SpellProperty property;
        [SerializeField] private float value;
        [SerializeField] private Spell targetSpell;

        public SpellProperty Property => property;
        public float Value => value;
        public Spell TargetSpell => targetSpell;

        public override string ToString()
        {
            string displayValue = value.ToString("+0.##;-0.##;0", CultureInfo.InvariantCulture);
            return targetSpell == null
                ? $"{property} {displayValue}"
                : $"{targetSpell.name}: {property} {displayValue}";
        }
    }
}
