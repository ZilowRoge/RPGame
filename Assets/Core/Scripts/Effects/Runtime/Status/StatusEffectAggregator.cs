using System.Collections.Generic;
using RPGame.Core.Movement;
using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Core.Effects
{
    public sealed class StatusEffectAggregator : MonoBehaviour, IStatusEffectReceiver
    {
        private StatusEffectContainer statusContainer;
        private IStatisticsController statisticsController;
        private IMovement movement;
        private StatusEffectTarget statusEffectTarget;
        private IStatisticsController subscribedStatisticsController;

        public IReadOnlyList<StatusEffectInstance> StatusEffects => statusContainer.Effects;

        private void Awake()
        {
            statisticsController = GetComponent<IStatisticsController>();
            CacheStatusEffectTarget();
            statusContainer = new StatusEffectContainer(statusEffectTarget);
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
            statusContainer.Tick(Time.deltaTime);
        }

        public void ApplyStatusEffect(StatusEffectDefinition effect, float duration)
        {
            statusContainer.Add(effect, duration);
        }

        public void ClearStatusEffects()
        {
            statusContainer.Clear();
        }

        private IStatisticsController GetStatisticsController()
        {
            if (statisticsController == null)
            {
                statisticsController = GetComponent<IStatisticsController>();
                CacheStatusEffectTarget();
                SubscribeToDied(statisticsController);
            }

            return statisticsController;
        }

        private void CacheStatusEffectTarget()
        {
            if (statisticsController == null)
            {
                statisticsController = GetComponent<IStatisticsController>();
            }

            movement ??= GetComponent<IMovement>();
            statusEffectTarget = new StatusEffectTarget(statisticsController, movement);
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
            ClearStatusEffects();
        }
    }
}
