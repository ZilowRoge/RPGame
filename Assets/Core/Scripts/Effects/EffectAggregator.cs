using System.Collections.Generic;

namespace RPGame.Core.Effects
{
    public sealed class EffectAggregator
    {
        private readonly List<EffectContainer> containers = new();

        public IReadOnlyList<EffectContainer> Containers => containers;

        public void Add(EffectContainer container)
        {
            if (container == null || containers.Contains(container))
            {
                return;
            }

            containers.Add(container);
        }

        public bool Remove(EffectContainer container)
        {
            return container != null && containers.Remove(container);
        }

        public float GetEffectValue(EffectStat stat, EffectModifierType modifierType)
        {
            float value = 0f;

            foreach (EffectContainer container in containers)
            {
                value += container.GetEffectValue(stat, modifierType);
            }

            return value;
        }
    }
}
