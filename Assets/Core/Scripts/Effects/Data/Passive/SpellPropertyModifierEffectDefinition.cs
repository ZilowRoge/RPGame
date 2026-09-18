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

        public SpellProperty Property => property;
        public float Value => value;

        public override string ToString()
        {
            return $"{property} {value.ToString("+0.##;-0.##;0", CultureInfo.InvariantCulture)}";
        }
    }
}
