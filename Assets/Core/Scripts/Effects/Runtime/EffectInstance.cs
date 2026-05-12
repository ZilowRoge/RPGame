using System;
using UnityEngine;

namespace RPGame.Core.Effects
{
    [Serializable]
    public sealed class EffectInstance
    {
        [SerializeField] private EffectDefinition definition;
        [SerializeField] private EffectStat stat;
        [SerializeField] private EffectModifierType modifierType;
        [SerializeField] private float value;

        public EffectInstance(
            EffectDefinition definition,
            EffectStat stat,
            EffectModifierType modifierType,
            float value)
        {
            this.definition = definition;
            this.stat = stat;
            this.modifierType = modifierType;
            this.value = value;
        }

        public EffectDefinition Definition => definition;
        public EffectStat Stat => stat;
        public EffectModifierType ModifierType => modifierType;
        public float Value => value;
    }
}
