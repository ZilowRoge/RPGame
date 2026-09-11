using System;
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
        private bool wasApplied;
        private bool wasRemoved;

        public TimedEffectInstance(ActiveEffectDefinition definition, float duration)
        {
            this.definition = definition;
            this.duration = Mathf.Max(0f, duration);
            remainingDuration = this.duration;
            remainingAmount = GetAmount(definition);
        }

        public ActiveEffectDefinition Definition => definition;
        public float Duration => duration;
        public float RemainingDuration => remainingDuration;
        public float RemainingAmount => remainingAmount;
        public bool IsFinished => remainingDuration <= 0f;
        public bool IsInstant => duration <= 0f;

        public bool CanMerge(ActiveEffectDefinition definition)
        {
            return this.definition != null
                && definition != null
                && ReferenceEquals(this.definition, definition);
        }

        public void Merge(ActiveEffectDefinition definition, float duration)
        {
            if (!CanMerge(definition))
            {
                return;
            }

            float additionalDuration = Mathf.Max(0f, duration);
            switch (definition.ReapplyPolicy)
            {
                case ReapplyPolicy.Stack:
                    this.duration += additionalDuration;
                    remainingDuration += additionalDuration;
                    remainingAmount += GetAmount(definition);
                    break;
                case ReapplyPolicy.Refresh:
                    this.duration = additionalDuration;
                    remainingDuration = additionalDuration;
                    remainingAmount = GetAmount(definition);
                    break;
                case ReapplyPolicy.KeepLongest:
                    if (additionalDuration > remainingDuration)
                    {
                        this.duration = additionalDuration;
                        remainingDuration = additionalDuration;
                        remainingAmount = GetAmount(definition);
                    }

                    break;
            }
        }

        public void Apply(EffectTarget target)
        {
            if (wasApplied || definition == null)
            {
                return;
            }

            wasApplied = true;
            definition.OnApply(target);
        }

        public void ApplyInstant(EffectTarget target)
        {
            Apply(target);
            definition?.Tick(target, 0f);
            TickAmountEffect(target, 0f, remainingAmount);
            remainingAmount = 0f;
            remainingDuration = 0f;
            Remove(target);
        }

        public void Tick(float deltaTime, EffectTarget target)
        {
            if (IsFinished)
            {
                return;
            }

            if (definition == null || definition.IsFinished(target))
            {
                remainingDuration = 0f;
                return;
            }

            float previousRemainingDuration = remainingDuration;
            remainingDuration = Mathf.Max(0f, remainingDuration - Mathf.Max(0f, deltaTime));

            float elapsedDelta = previousRemainingDuration - remainingDuration;
            float amount = previousRemainingDuration > 0f
                ? remainingAmount * (elapsedDelta / previousRemainingDuration)
                : remainingAmount;

            definition.Tick(target, elapsedDelta);
            TickAmountEffect(target, elapsedDelta, amount);
            remainingAmount = Mathf.Max(0f, remainingAmount - amount);

            if (remainingDuration <= 0f || definition.IsFinished(target))
            {
                remainingDuration = 0f;
            }
        }

        public void Remove(EffectTarget target)
        {
            if (wasRemoved || definition == null)
            {
                return;
            }

            wasRemoved = true;
            definition.OnRemove(target);
        }

        private void TickAmountEffect(EffectTarget target, float deltaTime, float amount)
        {
            if (definition is IAmountTimedEffect amountTimedEffect)
            {
                amountTimedEffect.Tick(target, deltaTime, amount);
            }
        }

        private static float GetAmount(ActiveEffectDefinition definition)
        {
            return definition is IAmountTimedEffect amountTimedEffect ? amountTimedEffect.Amount : 0f;
        }
    }
}
