using System.Collections.Generic;

namespace RPGame.Core.Effects
{
    public sealed class TimedEffectContainer
    {
        private readonly List<TimedEffectInstance> effects = new();

        public IReadOnlyList<TimedEffectInstance> Effects => effects;

        public void Add(ActiveEffectDefinition definition, float duration)
        {
            if (definition == null)
            {
                return;
            }

            TimedEffectInstance instance = new TimedEffectInstance(definition, duration);
            if (!instance.IsFinished)
            {
                effects.Add(instance);
            }
        }

        public void Tick(float deltaTime)
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                effects[i].Tick(deltaTime);

                if (effects[i].IsFinished)
                {
                    effects.RemoveAt(i);
                }
            }
        }

        public void Clear()
        {
            effects.Clear();
        }
    }
}
