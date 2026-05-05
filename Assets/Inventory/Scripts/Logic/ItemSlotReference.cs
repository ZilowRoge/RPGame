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

        private ItemSlotReference(
            ItemSlotContainerType containerType,
            int inventoryIndex,
            EquipmentSlotType equipmentSlotType)
        {
            ContainerType = containerType;
            InventoryIndex = inventoryIndex;
            EquipmentSlotType = equipmentSlotType;
        }

        public static ItemSlotReference Inventory(int inventoryIndex)
        {
            return new ItemSlotReference(ItemSlotContainerType.Inventory, inventoryIndex, default);
        }

        public static ItemSlotReference Equipment(EquipmentSlotType equipmentSlotType)
        {
            return new ItemSlotReference(ItemSlotContainerType.Equipment, -1, equipmentSlotType);
        }
    }
}
