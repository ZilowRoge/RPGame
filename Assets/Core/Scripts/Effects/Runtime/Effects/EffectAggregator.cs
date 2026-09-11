using System.Collections.Generic;
using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Core.Effects
{
    public sealed class EffectAggregator : MonoBehaviour
    {
        private readonly PermanentEffectContainer permanentContainer = new();
        private IStatisticsController statisticsController;

        public IReadOnlyList<EffectInstance> Effects => permanentContainer.Effects;
        public IStatisticsController StatisticsController => GetStatisticsController();

        private void Awake()
        {
            statisticsController = GetComponent<IStatisticsController>();
        }

        public void Add(PassiveEffectDefinition definition)
        {
            permanentContainer.Add(definition);
        }

        public void AddRange(IEnumerable<PassiveEffectDefinition> definitions)
        {
            permanentContainer.AddRange(definitions);
        }

        public float GetEffectValue(EffectStat stat, EffectModifierType modifierType)
        {
            return permanentContainer.GetEffectValue(stat, modifierType);
        }

        private IStatisticsController GetStatisticsController()
        {
            if (statisticsController == null)
            {
                statisticsController = GetComponent<IStatisticsController>();
            }

            return statisticsController;
        }
    }
}
