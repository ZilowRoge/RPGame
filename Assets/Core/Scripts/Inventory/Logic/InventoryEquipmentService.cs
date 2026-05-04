using RPGame.Core.Inventory.Data;

namespace RPGame.Core.Inventory.Logic
{
    public sealed class InventoryEquipmentService
    {
        private readonly Inventory inventory;
        private readonly Equipment equipment;

        public InventoryEquipmentService(Inventory inventory, Equipment equipment)
        {
            this.inventory = inventory;
            this.equipment = equipment;
        }

        public bool EquipFromInventory(int inventoryIndex)
        {
            InventorySlot inventorySlot = inventory.GetSlot(inventoryIndex);
            ItemInstance item = inventorySlot?.Item;

            if (item == null || !equipment.Equip(item))
            {
                return false;
            }

            if (inventory.RemoveItem(inventoryIndex, item.StackSize))
            {
                return true;
            }

            equipment.Unequip(GetSlotType(item));
            return false;
        }

        public bool UnequipToInventory(EquipmentSlotType slotType)
        {
            EquipmentSlot equipmentSlot = equipment.GetSlot(slotType);
            ItemInstance item = equipmentSlot?.Item;

            if (item == null || !inventory.CanAddItem(item, item.StackSize))
            {
                return false;
            }

            ItemInstance unequippedItem = equipment.Unequip(slotType);
            if (unequippedItem == null)
            {
                return false;
            }

            if (inventory.AddItem(unequippedItem, unequippedItem.StackSize))
            {
                return true;
            }

            equipment.Equip(unequippedItem);
            return false;
        }

        private static EquipmentSlotType GetSlotType(ItemInstance item)
        {
            return item.Definition.GetStatBlock<ItemWeaponData>().EquipmentSlotType;
        }
    }
}
