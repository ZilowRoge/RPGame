using System.Collections.Generic;

namespace RPGame.Core.Effects
{
    public sealed class StatusEffectContainer
    {
        private readonly List<StatusEffectInstance> effects = new();
        private readonly StatusEffectTarget target;
        private bool isTicking;
        private bool isClearing;
        private bool clearRequested;

        public IReadOnlyList<StatusEffectInstance> Effects => effects;

        public StatusEffectContainer(StatusEffectTarget target)
        {
            this.target = target;
        }

        public bool Add(StatusEffectDefinition definition, float duration)
        {
            if (definition == null || !definition.CanApply(target))
            {
                return false;
            }

            StatusEffectInstance instance = new StatusEffectInstance(definition, duration);
            if (instance.IsInstant)
            {
                instance.ApplyInstant(target);
                return true;
            }

            StatusEffectInstance existingInstance = FindInstance(definition);
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
            isTicking = true;

            for (int i = effects.Count - 1; i >= 0; i--)
            {
                StatusEffectInstance effect = effects[i];
                effect.Tick(deltaTime, target);

                if (clearRequested)
                {
                    break;
                }

                if (effect.IsFinished)
                {
                    effect.Remove(target);
                    effects.RemoveAt(i);
                }
            }

            isTicking = false;

            if (clearRequested)
            {
                clearRequested = false;
                ClearImmediately();
            }
        }

        public void Clear()
        {
            if (isTicking || isClearing)
            {
                clearRequested = true;
                return;
            }

            ClearImmediately();
        }

        private void ClearImmediately()
        {
            if (isClearing)
            {
                clearRequested = true;
                return;
            }

            isClearing = true;
            clearRequested = false;

            for (int i = effects.Count - 1; i >= 0; i--)
            {
                effects[i].Remove(target);
            }

            effects.Clear();
            clearRequested = false;
            isClearing = false;
        }

        private StatusEffectInstance FindInstance(StatusEffectDefinition definition)
        {
            foreach (StatusEffectInstance effect in effects)
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
