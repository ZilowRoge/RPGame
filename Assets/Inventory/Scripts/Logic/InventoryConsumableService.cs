using RPGame.Core.Effects;
using RPGame.Inventory.Data;

namespace RPGame.Inventory.Logic
{
    public sealed class InventoryConsumableService
    {
        private readonly Inventory inventory;
        private readonly ConsumableSlots consumableSlots;
        private readonly EffectAggregator effectAggregator;

        public InventoryConsumableService(
            Inventory inventory,
            ConsumableSlots consumableSlots,
            EffectAggregator effectAggregator)
        {
            this.inventory = inventory;
            this.consumableSlots = consumableSlots;
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
            if (!UseItem(item))
            {
                return false;
            }

            return inventory.RemoveItem(inventoryIndex);
        }

        public bool UseFromConsumableSlot(ConsumableSlotType slotType)
        {
            if (consumableSlots == null)
            {
                return false;
            }

            ConsumableSlot slot = consumableSlots.GetSlot(slotType);
            ItemInstance item = slot?.Item;

            return UseItem(item) && consumableSlots.RemoveItem(slotType) != null;
        }

        private bool UseItem(ItemInstance item)
        {
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

            return effectAggregator.TryAddTimedEffect(consumableData.Effect, consumableData.Duration);
        }
    }
}
