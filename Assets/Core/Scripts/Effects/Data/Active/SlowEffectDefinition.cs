using UnityEngine;

namespace RPGame.Core.Effects
{
    [CreateAssetMenu(fileName = "SlowEffect", menuName = "RPGame/Progression/Effects/Slow Effect")]
    public sealed class SlowEffectDefinition : StatusEffectDefinition
    {
        [SerializeField] private float movementSpeedMultiplier = 0.7f;

        public override ReapplyPolicy ReapplyPolicy => ReapplyPolicy.Refresh;

        public override bool CanApply(StatusEffectTarget target)
        {
            return target.Movement != null;
        }

        public override void OnApply(StatusEffectTarget target, StatusEffectInstance instance)
        {
            int modifierId = target.Movement.AddMovementSpeedModifier(MovementSpeedMultiplier);
            instance.RegisterCleanup(() => target.Movement.RemoveMovementSpeedModifier(modifierId));
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
