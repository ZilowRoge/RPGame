using System;
using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Core.Effects
{
    [Serializable]
    public sealed class TimedEffectInstance
    {
        [SerializeField] private ActiveEffectDefinition definition;
        [SerializeField] private float duration;
        [SerializeField] private float remainingDuration;
        [SerializeField] private float appliedAmount;

        public TimedEffectInstance(ActiveEffectDefinition definition, float duration)
        {
            this.definition = definition;
            this.duration = Mathf.Max(0f, duration);
            remainingDuration = this.duration;
        }

        public ActiveEffectDefinition Definition => definition;
        public float Duration => duration;
        public float RemainingDuration => remainingDuration;
        public bool IsFinished => remainingDuration <= 0f;
        public bool IsInstant => duration <= 0f;

        public void Tick(float deltaTime, IStatisticsController statisticsController)
        {
            if (IsFinished)
            {
                return;
            }

            if (definition == null || statisticsController == null || definition.IsFinished(statisticsController))
            {
                remainingDuration = 0f;
                return;
            }

            float previousRemainingDuration = remainingDuration;
            remainingDuration = Mathf.Max(0f, remainingDuration - Mathf.Max(0f, deltaTime));

            float elapsedDelta = previousRemainingDuration - remainingDuration;
            float amount = definition.Amount * (elapsedDelta / duration);

            definition.Apply(statisticsController, amount);
            appliedAmount += amount;

            if (remainingDuration <= 0f || appliedAmount >= definition.Amount || definition.IsFinished(statisticsController))
            {
                remainingDuration = 0f;
            }
        }
    }
}
