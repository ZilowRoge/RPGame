using System.Collections.Generic;
using RPGame.Core.Statistics;

namespace RPGame.Core.Effects
{
    public sealed class TimedEffectContainer
    {
        private readonly List<TimedEffectInstance> effects = new();
        private IStatisticsController statisticsController;

        public IReadOnlyList<TimedEffectInstance> Effects => effects;

        public void SetStatisticsController(IStatisticsController statisticsController)
        {
            this.statisticsController = statisticsController;
        }

        public bool Add(ActiveEffectDefinition definition, float duration)
        {
            if (definition == null || statisticsController == null)
            {
                return false;
            }

            TimedEffectInstance instance = new TimedEffectInstance(definition, duration);
            if (instance.IsInstant)
            {
                instance.Definition.Apply(statisticsController, definition.Amount);
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
            }

            return true;
        }

        public void Tick(float deltaTime)
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                effects[i].Tick(deltaTime, statisticsController);

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
