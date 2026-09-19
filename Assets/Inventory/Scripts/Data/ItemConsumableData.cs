using RPGame.Core.Statuses;
using UnityEngine;

namespace RPGame.Inventory.Data
{
    [CreateAssetMenu(fileName = "ItemConsumableData", menuName = "RPGame/Inventory/Item Consumable Data")]
    public sealed class ItemConsumableData : ItemTypeData
    {
        [SerializeField] private StatusDefinition effect;
        [SerializeField] private float duration;

        public StatusDefinition Effect => effect;
        public float Duration => duration;

        public override string GetTooltip()
        {
            return effect != null ? effect.ToString() : "Consumable";
        }
    }
}
