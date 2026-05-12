using UnityEngine;

namespace RPGame.Core.Effects
{
    [CreateAssetMenu(fileName = "StatEffect", menuName = "RPGame/Progression/Effects/Stat Effect")]
    public sealed class StatEffectDefinition : EffectDefinition
    {
        [SerializeField] private EffectStat stat = EffectStat.MaxHealth;
        [SerializeField] private EffectModifierType modifierType = EffectModifierType.Flat;
        [SerializeField] private float value = 10f;

        public EffectStat Stat => stat;
        public EffectModifierType ModifierType => modifierType;
        public float Value => value;
    }
}
