using RPGame.Core.Effects;
using UnityEngine;

namespace RPGame.Inventory.Data
{
    [CreateAssetMenu(fileName = "ItemConsumableData", menuName = "RPGame/Inventory/Item Consumable Data")]
    public sealed class ItemConsumableData : ItemTypeData
    {
        [SerializeField] private ActiveEffectDefinition effect;
        [SerializeField] private float duration;

        public ActiveEffectDefinition Effect => effect;
        public float Duration => duration;

        public override string GetTooltip()
        {
            return effect != null ? effect.ToString() : "Consumable";
        }
    }
}
