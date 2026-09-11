using UnityEngine;

namespace RPGame.Core.Effects
{
    [CreateAssetMenu(fileName = "SlowEffect", menuName = "RPGame/Progression/Effects/Slow Effect")]
    public sealed class SlowEffectDefinition : ActiveEffectDefinition
    {
        [SerializeField] private float movementSpeedMultiplier = 0.7f;

        public override ReapplyPolicy ReapplyPolicy => ReapplyPolicy.Refresh;

        public override bool CanApply(EffectTarget target)
        {
            return target.Movement != null;
        }

        public override void OnApply(EffectTarget target, TimedEffectInstance instance)
        {
            target.Movement.AddMovementSpeedModifier(instance, MovementSpeedMultiplier);
        }

        public override void OnRemove(EffectTarget target, TimedEffectInstance instance)
        {
            target.Movement.RemoveMovementSpeedModifier(instance);
        }

        public override string ToString()
        {
            return $"Slow {MovementSpeedMultiplier:0.##}x";
        }

        private float MovementSpeedMultiplier => Mathf.Max(0f, movementSpeedMultiplier);

        private void OnValidate()
        {
            movementSpeedMultiplier = Mathf.Max(0f, movementSpeedMultiplier);
        }
    }
}
