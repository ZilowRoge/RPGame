using System.Collections.Generic;
using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Core.Effects
{
    public sealed class EffectAggregator : MonoBehaviour
    {
        private readonly PermanentEffectContainer permanentContainer = new();
        private readonly TimedEffectContainer timedContainer = new();
        private IStatisticsController statisticsController;
        private IStatisticsController subscribedStatisticsController;

        public IReadOnlyList<EffectInstance> Effects => permanentContainer.Effects;
        public IReadOnlyList<TimedEffectInstance> TimedEffects => timedContainer.Effects;
        public IStatisticsController StatisticsController => GetStatisticsController();

        private void Awake()
        {
            statisticsController = GetComponent<IStatisticsController>();
            timedContainer.SetStatisticsController(statisticsController);
        }

        private void OnEnable()
        {
            SubscribeToDied(GetStatisticsController());
        }

        private void OnDisable()
        {
            UnsubscribeFromDied();
        }

        private void OnDestroy()
        {
            UnsubscribeFromDied();
        }

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

        private IStatisticsController GetStatisticsController()
        {
            if (statisticsController == null)
            {
                statisticsController = GetComponent<IStatisticsController>();
                timedContainer.SetStatisticsController(statisticsController);
                SubscribeToDied(statisticsController);
            }

            return statisticsController;
        }

        private void SubscribeToDied(IStatisticsController targetStatisticsController)
        {
            if (targetStatisticsController == null || subscribedStatisticsController == targetStatisticsController)
            {
                return;
            }

            UnsubscribeFromDied();
            targetStatisticsController.Died += HandleDied;
            subscribedStatisticsController = targetStatisticsController;
        }

        private void UnsubscribeFromDied()
        {
            if (subscribedStatisticsController == null)
            {
                return;
            }

            subscribedStatisticsController.Died -= HandleDied;
            subscribedStatisticsController = null;
        }

        private void HandleDied()
        {
            ClearTimedEffects();
        }
    }
}
