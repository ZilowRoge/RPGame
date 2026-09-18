using UnityEngine;

namespace RPGame.Core.Spells
{
    public static class SpellPropertyModifierResolver
    {
        public static float ResolveRadius(IAoECapability capability, SpellPropertyModifiers modifiers)
        {
            return Mathf.Max(0f, capability.Radius + GetModifier(modifiers, SpellProperty.Radius));
        }

        public static float ResolveDuration(IDurationCapability capability, SpellPropertyModifiers modifiers)
        {
            return Mathf.Max(0f, capability.Duration + GetModifier(modifiers, SpellProperty.Duration));
        }

        public static float ResolveControlPower(IControlCapability capability, SpellPropertyModifiers modifiers)
        {
            return Mathf.Max(0f, capability.ControlPower + GetModifier(modifiers, SpellProperty.ControlPower));
        }

        public static int ResolveOrbCount(IOrbCapability capability, SpellPropertyModifiers modifiers)
        {
            int modifier = Mathf.RoundToInt(GetModifier(modifiers, SpellProperty.OrbCount));
            return Mathf.Max(1, capability.OrbCount + modifier);
        }

        public static bool Supports(Spell spell, SpellProperty property)
        {
            return property switch
            {
                SpellProperty.Radius => spell is IAoECapability,
                SpellProperty.Duration => spell is IDurationCapability,
                SpellProperty.ControlPower => spell is IControlCapability,
                SpellProperty.OrbCount => spell is IOrbCapability,
                _ => false
            };
        }

        private static float GetModifier(SpellPropertyModifiers modifiers, SpellProperty property)
        {
            return modifiers != null ? modifiers.GetValue(property) : 0f;
        }
    }
}
