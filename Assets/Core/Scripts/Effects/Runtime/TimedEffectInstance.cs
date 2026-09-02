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

        public void Tick(float deltaTime)
        {
            if (IsFinished)
            {
                return;
            }

            remainingDuration = Mathf.Max(0f, remainingDuration - Mathf.Max(0f, deltaTime));
        }
    }
}
