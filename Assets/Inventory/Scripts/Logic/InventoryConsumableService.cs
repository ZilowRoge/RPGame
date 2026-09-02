using RPGame.Core.Effects;
using RPGame.Inventory.Data;

namespace RPGame.Inventory.Logic
{
    public sealed class InventoryConsumableService
    {
        private readonly Inventory inventory;
        private readonly EffectAggregator effectAggregator;

        public InventoryConsumableService(Inventory inventory, EffectAggregator effectAggregator)
        {
            this.inventory = inventory;
            this.effectAggregator = effectAggregator;
        }

        public bool UseFromInventory(int inventoryIndex)
        {
            if (inventory == null)
            {
                return false;
            }

            InventorySlot inventorySlot = inventory.GetSlot(inventoryIndex);
            ItemInstance item = inventorySlot?.Item;
            ItemDefinition definition = item?.Definition;
            ItemConsumableData consumableData = definition?.GetStatBlock<ItemConsumableData>();

            if (item == null
                || definition == null
                || definition.ItemType != ItemType.Consumable
                || consumableData == null
                || consumableData.Effect == null
                || effectAggregator == null)
            {
                return false;
            }

            if (!effectAggregator.TryAddTimedEffect(consumableData.Effect, consumableData.Duration))
            {
                return false;
            }

            return inventory.RemoveItem(inventoryIndex);
        }
    }
}
