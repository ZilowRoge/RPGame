using System;
using RPGame.Inventory.Data;

namespace RPGame.Inventory.Logic
{
    [Serializable]
    public readonly struct ItemSlotReference
    {
        public ItemSlotContainerType ContainerType { get; }
        public int InventoryIndex { get; }
        public EquipmentSlotType EquipmentSlotType { get; }
        public ConsumableSlotType ConsumableSlotType { get; }

        private ItemSlotReference(
            ItemSlotContainerType containerType,
            int inventoryIndex,
            EquipmentSlotType equipmentSlotType,
            ConsumableSlotType consumableSlotType)
        {
            ContainerType = containerType;
            InventoryIndex = inventoryIndex;
            EquipmentSlotType = equipmentSlotType;
            ConsumableSlotType = consumableSlotType;
        }

        public static ItemSlotReference Inventory(int inventoryIndex)
        {
            return new ItemSlotReference(ItemSlotContainerType.Inventory, inventoryIndex, default, default);
        }

        public static ItemSlotReference Equipment(EquipmentSlotType equipmentSlotType)
        {
            return new ItemSlotReference(ItemSlotContainerType.Equipment, -1, equipmentSlotType, default);
        }

        public static ItemSlotReference Consumable(ConsumableSlotType consumableSlotType)
        {
            return new ItemSlotReference(ItemSlotContainerType.Consumable, -1, default, consumableSlotType);
        }
    }
}
