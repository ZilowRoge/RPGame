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
        [SerializeField] private float remainingAmount;

        public TimedEffectInstance(ActiveEffectDefinition definition, float duration)
        {
            this.definition = definition;
            this.duration = Mathf.Max(0f, duration);
            remainingDuration = this.duration;
            remainingAmount = definition != null ? definition.Amount : 0f;
        }

        public ActiveEffectDefinition Definition => definition;
        public float Duration => duration;
        public float RemainingDuration => remainingDuration;
        public float RemainingAmount => remainingAmount;
        public bool IsFinished => remainingDuration <= 0f || remainingAmount <= 0f;
        public bool IsInstant => duration <= 0f;

        public bool CanMerge(ActiveEffectDefinition definition)
        {
            return this.definition != null
                && definition != null
                && this.definition.GetType() == definition.GetType();
        }

        public void Merge(ActiveEffectDefinition definition, float duration)
        {
            if (!CanMerge(definition))
            {
                return;
            }

            float additionalDuration = Mathf.Max(0f, duration);
            this.duration += additionalDuration;
            remainingDuration += additionalDuration;
            remainingAmount += definition.Amount;
        }

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
            float amount = remainingAmount * (elapsedDelta / previousRemainingDuration);

            definition.Apply(statisticsController, amount);
            remainingAmount = Mathf.Max(0f, remainingAmount - amount);

            if (remainingDuration <= 0f || remainingAmount <= 0f || definition.IsFinished(statisticsController))
            {
                remainingDuration = 0f;
                remainingAmount = 0f;
            }
        }
    }
}
