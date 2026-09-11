using System.Collections.Generic;
using RPGame.Core.Damage;
using RPGame.Core.Movement;
using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Core.Effects
{
    [RequireComponent(typeof(StatusController))]
    public sealed class EffectAggregator : MonoBehaviour, IStatusApplicator
    {
        private readonly PermanentEffectContainer permanentContainer = new();
        private TimedEffectContainer timedContainer;
        private IStatisticsController statisticsController;
        private IStatusController statusController;
        private IDamageable damageable;
        private IMovement movement;
        private EffectTarget effectTarget;
        private IStatisticsController subscribedStatisticsController;

        public IReadOnlyList<EffectInstance> Effects => permanentContainer.Effects;
        public IReadOnlyList<TimedEffectInstance> TimedEffects => timedContainer.Effects;
        public IStatisticsController StatisticsController => GetStatisticsController();

        private void Awake()
        {
            statisticsController = GetComponent<IStatisticsController>();
            CacheEffectTarget();
            timedContainer = new TimedEffectContainer(effectTarget);
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

        public void ApplyStatus(ActiveEffectDefinition effect, float duration)
        {
            AddTimedEffect(effect, duration);
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
                CacheEffectTarget();
                SubscribeToDied(statisticsController);
            }

            return statisticsController;
        }

        private void CacheEffectTarget()
        {
            if (statisticsController == null)
            {
                statisticsController = GetComponent<IStatisticsController>();
            }

            statusController ??= GetComponent<IStatusController>();
            damageable ??= GetComponent<IDamageable>();
            movement ??= GetComponent<IMovement>();
            effectTarget = new EffectTarget(statisticsController, statusController, damageable, movement);
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
