using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Core.Effects
{
    public sealed class EffectAggregator : MonoBehaviour
    {
        private readonly PernamentEffectContainer pernamentContainer = new();
        private readonly TimedEffectContainer timedContainer = new();

        public IReadOnlyList<EffectInstance> Effects => pernamentContainer.Effects;
        public IReadOnlyList<TimedEffectInstance> TimedEffects => timedContainer.Effects;

        private void Update()
        {
            timedContainer.Tick(Time.deltaTime);
        }

        public void Add(PassiveEffectDefinition definition)
        {
            pernamentContainer.Add(definition);
        }

        public void AddRange(IEnumerable<PassiveEffectDefinition> definitions)
        {
            pernamentContainer.AddRange(definitions);
        }

        public void AddTimedEffect(ActiveEffectDefinition definition, float duration)
        {
            timedContainer.Add(definition, duration);
        }

        public void ClearTimedEffects()
        {
            timedContainer.Clear();
        }

        public float GetEffectValue(EffectStat stat, EffectModifierType modifierType)
        {
            return pernamentContainer.GetEffectValue(stat, modifierType);
        }
    }
}
