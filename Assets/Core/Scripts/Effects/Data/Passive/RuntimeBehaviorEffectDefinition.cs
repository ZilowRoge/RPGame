using System;
using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Core.Effects
{
    [CreateAssetMenu(
        fileName = "RuntimeBehaviorEffect",
        menuName = "RPGame/Progression/Effects/Runtime Behavior Effect")]
    public sealed class RuntimeBehaviorEffectDefinition : PassiveEffectDefinition, IRuntimeSpellBehaviorFactory
    {
        [SerializeReference] private RuntimeSpellBehaviorDefinition behavior;

        public bool TryCreateRuntimeBehavior(
            Spell spell,
            GameObject casterObject,
            out IRuntimeSpellBehavior runtimeBehavior)
        {
            runtimeBehavior = null;
            return behavior != null
                && behavior.TryCreate(spell, casterObject, out runtimeBehavior)
                && runtimeBehavior != null;
        }

        public override string ToString()
        {
            return behavior != null ? behavior.GetType().Name : name;
        }
    }
}
