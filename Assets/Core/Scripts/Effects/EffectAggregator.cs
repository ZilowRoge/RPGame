using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Core.Effects
{
    public sealed class EffectAggregator : MonoBehaviour
    {
        private readonly PermanentEffectContainer permanentContainer = new();
        private readonly TimedEffectContainer timedContainer = new();

        public IReadOnlyList<EffectInstance> Effects => permanentContainer.Effects;
        public IReadOnlyList<TimedEffectInstance> TimedEffects => timedContainer.Effects;

        private void Update()
        {
            timedContainer.Tick(Time.deltaTime);
        }

        public void Add(PassiveEffectDefinition definition)
        {
            permanentContainer.Add(definition);
        }

        public void AddRange(IEnumerable<PassiveEffectDefinition> definitions)
        {
            permanentContainer.AddRange(definitions);
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
            return permanentContainer.GetEffectValue(stat, modifierType);
        }
    }
}
