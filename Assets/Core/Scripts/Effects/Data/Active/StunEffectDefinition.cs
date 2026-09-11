using UnityEngine;

namespace RPGame.Core.Effects
{
    [CreateAssetMenu(fileName = "StunEffect", menuName = "RPGame/Progression/Effects/Stun Effect")]
    public sealed class StunEffectDefinition : ActiveEffectDefinition
    {
        public override ReapplyPolicy ReapplyPolicy => ReapplyPolicy.KeepLongest;

        public override bool CanApply(EffectTarget target)
        {
            return target.Movement != null;
        }

        public override void OnApply(EffectTarget target, TimedEffectInstance instance)
        {
            target.Movement.BlockMovement();
        }

        public override void OnRemove(EffectTarget target, TimedEffectInstance instance)
        {
            target.Movement.UnblockMovement();
        }

        public override string ToString()
        {
            return "Stun";
        }
    }
}
