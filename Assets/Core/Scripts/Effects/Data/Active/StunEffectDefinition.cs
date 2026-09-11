using UnityEngine;

namespace RPGame.Core.Effects
{
    [CreateAssetMenu(fileName = "StunEffect", menuName = "RPGame/Progression/Effects/Stun Effect")]
    public sealed class StunEffectDefinition : StatusEffectDefinition
    {
        public override ReapplyPolicy ReapplyPolicy => ReapplyPolicy.KeepLongest;

        public override bool CanApply(StatusEffectTarget target)
        {
            return target.Movement != null;
        }

        public override void OnApply(StatusEffectTarget target, StatusEffectInstance instance)
        {
            target.Movement.BlockMovement();
            instance.RegisterCleanup(target.Movement.UnblockMovement);
        }

        public override string ToString()
        {
            return "Stun";
        }
    }
}
