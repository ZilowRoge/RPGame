using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Core.Effects
{
    public sealed class EffectAggregator : MonoBehaviour
    {
        private readonly EffectContainer container = new();

        public IReadOnlyList<EffectInstance> Effects => container.Effects;

        public void Add(EffectDefinition definition)
        {
            container.Add(definition);
        }

        public void AddRange(IEnumerable<EffectDefinition> definitions)
        {
            container.AddRange(definitions);
        }

        public float GetEffectValue(EffectStat stat, EffectModifierType modifierType)
        {
            return container.GetEffectValue(stat, modifierType);
        }
    }
}
