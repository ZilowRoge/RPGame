using UnityEngine;

namespace RPGame.Core.Effects
{
    [CreateAssetMenu(fileName = "StunEffect", menuName = "RPGame/Progression/Effects/Stun Effect")]
    public sealed class StunEffectDefinition : ActiveEffectDefinition
    {
        public override ReapplyPolicy ReapplyPolicy => ReapplyPolicy.KeepLongest;

        public override void OnApply(EffectTarget target)
        {
            target.StatusController?.BeginStun();
        }

        public override void OnRemove(EffectTarget target)
        {
            target.StatusController?.EndStun();
        }

        public override string ToString()
        {
            return "Stun";
        }
    }
}
