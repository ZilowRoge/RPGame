using System;
using RPGame.Inventory.Data;
using UnityEngine;

namespace RPGame.Inventory.Logic
{
    [Serializable]
    public sealed class EquipmentSlot
    {
        [SerializeField] private EquipmentSlotType slotType;
        [SerializeField] private ItemInstance item;

        public EquipmentSlotType SlotType => slotType;
        public ItemInstance Item => item;
        public bool HasItem => item != null && item.Definition != null && item.StackSize > 0;

        public EquipmentSlot(EquipmentSlotType slotType)
        {
            this.slotType = slotType;
        }

        public void SetItem(ItemInstance item)
        {
            this.item = item;
        }

        public ItemInstance Clear()
        {
            ItemInstance clearedItem = item;
            item = null;
            return clearedItem;
        }
    }
}
