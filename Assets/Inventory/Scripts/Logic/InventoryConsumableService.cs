using RPGame.Core.Statuses;
using RPGame.Inventory.Data;

namespace RPGame.Inventory.Logic
{
    public sealed class InventoryConsumableService
    {
        private readonly Inventory inventory;
        private readonly ConsumableSlots consumableSlots;
        private readonly StatusAggregator statusAggregator;

        public InventoryConsumableService(
            Inventory inventory,
            ConsumableSlots consumableSlots,
            StatusAggregator statusAggregator)
        {
            this.inventory = inventory;
            this.consumableSlots = consumableSlots;
            this.statusAggregator = statusAggregator;
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
                || statusAggregator == null)
            {
                return false;
            }

            StatusContext context = new(
                new StatusSourceId(nameof(InventoryConsumableService)),
                statusAggregator.gameObject);
            statusAggregator.ApplyStatus(consumableData.Effect, consumableData.Duration, context);
            return true;
        }
    }
}
