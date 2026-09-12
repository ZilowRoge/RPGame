using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Core.Effects
{
    [Serializable]
    public sealed class StatusEffectInstance
    {
        [SerializeField] private StatusEffectDefinition definition;
        [SerializeField] private float duration;
        [SerializeField] private float remainingDuration;
        [SerializeField] private float remainingAmount;
        private readonly List<Action> cleanupActions = new();
        private bool wasApplied;
        private bool wasRemoved;

        public StatusEffectInstance(StatusEffectDefinition definition, float duration)
        {
            this.definition = definition;
            this.duration = Mathf.Max(0f, duration);
            remainingDuration = this.duration;
            remainingAmount = GetAmount(definition);
        }

        public StatusEffectDefinition Definition => definition;
        public float Duration => duration;
        public float RemainingDuration => remainingDuration;
        public float RemainingAmount => remainingAmount;
        public bool IsFinished => remainingDuration <= 0f;
        public bool IsInstant => duration <= 0f;

        public void RegisterCleanup(Action cleanup)
        {
            if (cleanup != null)
            {
                cleanupActions.Add(cleanup);
            }
        }

        public bool CanMerge(StatusEffectDefinition definition)
        {
            return this.definition != null
                && definition != null
                && ReferenceEquals(this.definition, definition);
        }

        public void Merge(StatusEffectDefinition definition, float duration)
        {
            if (!CanMerge(definition))
            {
                return;
            }

            float additionalDuration = Mathf.Max(0f, duration);
            float incomingAmount = GetAmount(definition);
            switch (definition.ReapplyPolicy)
            {
                case ReapplyPolicy.Stack:
                    this.duration += additionalDuration;
                    remainingDuration += additionalDuration;
                    remainingAmount += incomingAmount;
                    break;
                case ReapplyPolicy.Refresh:
                    this.duration = additionalDuration;
                    remainingDuration = additionalDuration;
                    remainingAmount = incomingAmount;
                    break;
                case ReapplyPolicy.KeepLongest:
                    if (additionalDuration > remainingDuration)
                    {
                        this.duration = additionalDuration;
                        remainingDuration = additionalDuration;
                        remainingAmount = incomingAmount;
                    }

                    break;
            }
        }

        public void Apply(StatusEffectTarget target)
        {
            if (wasApplied || definition == null)
            {
                return;
            }

            wasApplied = true;
            definition.OnApply(target, this);
        }

        public void ApplyInstant(StatusEffectTarget target)
        {
            Apply(target);
            definition?.Tick(target, 0f);
            TickAmountEffect(target, 0f, remainingAmount);
            remainingAmount = 0f;
            remainingDuration = 0f;
            Remove(target);
        }

        public void Tick(float deltaTime, StatusEffectTarget target)
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

        public void Remove(StatusEffectTarget target)
        {
            if (wasRemoved || definition == null)
            {
                return;
            }

            wasRemoved = true;
            definition.OnRemove(target, this);
            RunCleanupActions();
        }

        private void TickAmountEffect(StatusEffectTarget target, float deltaTime, float amount)
        {
            if (definition is IAmountStatusEffect amountStatusEffect)
            {
                amountStatusEffect.Tick(target, deltaTime, amount);
            }
        }

        private static float GetAmount(StatusEffectDefinition definition)
        {
            if (definition is not IAmountStatusEffect amountStatusEffect)
            {
                return 0f;
            }

            return Mathf.Max(0f, amountStatusEffect.Amount);
        }

        private void RunCleanupActions()
        {
            for (int i = 0; i < cleanupActions.Count; i++)
            {
                cleanupActions[i]?.Invoke();
            }

            cleanupActions.Clear();
        }
    }
}
