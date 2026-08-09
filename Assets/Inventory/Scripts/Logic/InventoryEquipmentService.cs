using RPGame.Inventory.Data;

namespace RPGame.Inventory.Logic
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

            if (item == null || !equipment.CanEquip(item))
            {
                return false;
            }

            return MoveInventoryToEquipment(inventoryIndex, GetSlotType(item));
        }

        public bool UnequipToInventory(EquipmentSlotType slotType)
        {
            EquipmentSlot equipmentSlot = equipment.GetSlot(slotType);
            ItemInstance item = equipmentSlot?.Item;

            if (item == null || !inventory.CanAddItem(item))
            {
                return false;
            }

            ItemInstance unequippedItem = equipment.Unequip(slotType);
            if (unequippedItem == null)
            {
                return false;
            }

            if (inventory.AddItem(unequippedItem))
            {
                return true;
            }

            equipment.Equip(unequippedItem);
            return false;
        }

        public bool MoveItem(ItemSlotReference from, ItemSlotReference to)
        {
            if (from.ContainerType == ItemSlotContainerType.Inventory
                && to.ContainerType == ItemSlotContainerType.Inventory)
            {
                return MoveInventoryToInventory(from.InventoryIndex, to.InventoryIndex);
            }

            if (from.ContainerType == ItemSlotContainerType.Inventory
                && to.ContainerType == ItemSlotContainerType.Equipment)
            {
                return MoveInventoryToEquipment(from.InventoryIndex, to.EquipmentSlotType);
            }

            if (from.ContainerType == ItemSlotContainerType.Equipment
                && to.ContainerType == ItemSlotContainerType.Inventory)
            {
                return MoveEquipmentToInventory(from.EquipmentSlotType, to.InventoryIndex);
            }

            if (from.ContainerType == ItemSlotContainerType.Equipment
                && to.ContainerType == ItemSlotContainerType.Equipment)
            {
                return MoveEquipmentToEquipment(from.EquipmentSlotType, to.EquipmentSlotType);
            }

            return false;
        }

        private bool MoveInventoryToInventory(int fromIndex, int toIndex)
        {
            InventorySlot fromSlot = inventory.GetSlot(fromIndex);
            InventorySlot toSlot = inventory.GetSlot(toIndex);

            if (fromSlot == null || toSlot == null || !fromSlot.HasItem || fromIndex == toIndex)
            {
                return false;
            }

            if (toSlot.HasItem)
            {
                return inventory.SwapItems(fromIndex, toIndex);
            }

            return inventory.MoveItem(fromIndex, toIndex, fromSlot.Item.StackSize);
        }

        private bool MoveInventoryToEquipment(int inventoryIndex, EquipmentSlotType equipmentSlotType)
        {
            InventorySlot inventorySlot = inventory.GetSlot(inventoryIndex);
            EquipmentSlot equipmentSlot = equipment.GetSlot(equipmentSlotType);
            ItemInstance inventoryItem = inventorySlot?.Item;

            if (inventoryItem == null || equipmentSlot == null || !equipment.CanEquipToSlot(inventoryItem, equipmentSlotType))
            {
                return false;
            }

            ItemInstance equipmentItem = equipmentSlot.HasItem ? equipmentSlot.Item : null;
            if (!equipment.SetItem(equipmentSlotType, inventoryItem))
            {
                return false;
            }

            return equipmentItem != null
                ? inventory.SetItem(inventoryIndex, equipmentItem)
                : inventory.ClearSlot(inventoryIndex);
        }

        private bool MoveEquipmentToInventory(EquipmentSlotType equipmentSlotType, int inventoryIndex)
        {
            EquipmentSlot equipmentSlot = equipment.GetSlot(equipmentSlotType);
            InventorySlot inventorySlot = inventory.GetSlot(inventoryIndex);
            ItemInstance equipmentItem = equipmentSlot?.Item;

            if (equipmentItem == null || inventorySlot == null)
            {
                return false;
            }

            ItemInstance inventoryItem = inventorySlot.HasItem ? inventorySlot.Item : null;
            if (inventoryItem != null && !equipment.CanEquipToSlot(inventoryItem, equipmentSlotType))
            {
                return false;
            }

            if (!inventory.SetItem(inventoryIndex, equipmentItem))
            {
                return false;
            }

            return equipmentItem != null
                ? equipment.SetItem(equipmentSlotType, inventoryItem)
                : equipment.SetItem(equipmentSlotType, null);
        }

        private bool MoveEquipmentToEquipment(EquipmentSlotType fromSlotType, EquipmentSlotType toSlotType)
        {
            if (fromSlotType == toSlotType)
            {
                return false;
            }

            EquipmentSlot fromSlot = equipment.GetSlot(fromSlotType);
            EquipmentSlot toSlot = equipment.GetSlot(toSlotType);
            ItemInstance fromItem = fromSlot?.Item;

            if (fromItem == null || toSlot == null || !equipment.CanEquipToSlot(fromItem, toSlotType))
            {
                return false;
            }

            ItemInstance toItem = toSlot.HasItem ? toSlot.Item : null;
            if (toItem != null && !equipment.CanEquipToSlot(toItem, fromSlotType))
            {
                return false;
            }

            if (!equipment.SetItem(toSlotType, fromItem))
            {
                return false;
            }

            return equipment.SetItem(fromSlotType, toItem);
        }

        private static EquipmentSlotType GetSlotType(ItemInstance item)
        {
            return item.Definition.GetStatBlock<ItemWeaponData>().EquipmentSlotType;
        }
    }
}
