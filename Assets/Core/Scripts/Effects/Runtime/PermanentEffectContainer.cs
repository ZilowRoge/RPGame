using System.Collections.Generic;

namespace RPGame.Core.Effects
{
    public sealed class PermanentEffectContainer
    {
        private readonly List<EffectInstance> effects = new();

        public IReadOnlyList<EffectInstance> Effects => effects;

        public void Add(PassiveEffectDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            EffectInstance instance = CreateInstance(definition);
            if (instance != null)
            {
                effects.Add(instance);
            }
        }

        public void AddRange(IEnumerable<PassiveEffectDefinition> definitions)
        {
            if (definitions == null)
            {
                return;
            }

            foreach (PassiveEffectDefinition definition in definitions)
            {
                Add(definition);
            }
        }

        public float GetEffectValue(EffectStat stat, EffectModifierType modifierType)
        {
            float value = 0f;

            foreach (EffectInstance effect in GetEffects(stat, modifierType))
            {
                value += effect.Value;
            }

            return value;
        }

        private IEnumerable<EffectInstance> GetEffects(
            EffectStat stat,
            EffectModifierType modifierType)
        {
            foreach (EffectInstance effect in effects)
            {
                if (effect.Stat == stat && effect.ModifierType == modifierType)
                {
                    yield return effect;
                }
            }
        }

        private static EffectInstance CreateInstance(PassiveEffectDefinition definition)
        {
            if (definition is StatEffectDefinition statEffect)
            {
                return new EffectInstance(
                    statEffect,
                    statEffect.Stat,
                    statEffect.ModifierType,
                    statEffect.Value);
            }

            return null;
        }
    }
}
