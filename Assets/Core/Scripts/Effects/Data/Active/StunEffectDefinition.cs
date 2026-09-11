using UnityEngine;

namespace RPGame.Core.Effects
{
    [CreateAssetMenu(fileName = "StunEffect", menuName = "RPGame/Progression/Effects/Stun Effect")]
    public sealed class StunEffectDefinition : ActiveEffectDefinition
    {
        public override ReapplyPolicy ReapplyPolicy => ReapplyPolicy.KeepLongest;

        public override bool CanApply(EffectTarget target)
        {
            return target.StatusController != null && target.Movement != null;
        }

        public override void OnApply(EffectTarget target, TimedEffectInstance instance)
        {
            target.StatusController.BeginStun();
            target.Movement.BlockMovement();
        }

        public override void OnRemove(EffectTarget target, TimedEffectInstance instance)
        {
            target.StatusController.EndStun();
            target.Movement.UnblockMovement();
        }

        public override string ToString()
        {
            return "Stun";
        }
    }
}
