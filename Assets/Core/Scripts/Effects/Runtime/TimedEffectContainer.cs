using System.Collections.Generic;

namespace RPGame.Core.Effects
{
    public sealed class TimedEffectContainer
    {
        private readonly List<TimedEffectInstance> effects = new();
        private readonly EffectTarget target;

        public IReadOnlyList<TimedEffectInstance> Effects => effects;

        public TimedEffectContainer(EffectTarget target)
        {
            this.target = target;
        }

        public bool Add(ActiveEffectDefinition definition, float duration)
        {
            if (definition == null || !definition.CanApply(target))
            {
                return false;
            }

            TimedEffectInstance instance = new TimedEffectInstance(definition, duration);
            if (instance.IsInstant)
            {
                instance.ApplyInstant(target);
                return true;
            }

            TimedEffectInstance existingInstance = FindInstance(definition);
            if (existingInstance != null)
            {
                existingInstance.Merge(definition, duration);
                return true;
            }

            if (!instance.IsFinished)
            {
                effects.Add(instance);
                instance.Apply(target);
            }

            return true;
        }

        public void Tick(float deltaTime)
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                effects[i].Tick(deltaTime, target);

                if (effects[i].IsFinished)
                {
                    effects[i].Remove(target);
                    effects.RemoveAt(i);
                }
            }
        }

        public void Clear()
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                effects[i].Remove(target);
            }

            effects.Clear();
        }

        private TimedEffectInstance FindInstance(ActiveEffectDefinition definition)
        {
            foreach (TimedEffectInstance effect in effects)
            {
                if (!effect.IsFinished && effect.CanMerge(definition))
                {
                    return effect;
                }
            }

            return null;
        }
    }
}
