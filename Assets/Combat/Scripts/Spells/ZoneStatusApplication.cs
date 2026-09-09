using RPGame.Core.Effects;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    public readonly struct ZoneStatusApplication
    {
        public ZoneStatusApplication(
            StatusEffectDefinition effect,
            float duration,
            bool reapplyWhileInside,
            float reapplyInterval)
        {
            Effect = effect;
            Duration = Mathf.Max(0f, duration);
            ReapplyWhileInside = reapplyWhileInside;
            ReapplyInterval = Mathf.Max(0f, reapplyInterval);
        }

        public StatusEffectDefinition Effect { get; }
        public float Duration { get; }
        public bool ReapplyWhileInside { get; }
        public float ReapplyInterval { get; }
    }
}
