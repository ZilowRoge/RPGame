using System;
using UnityEngine;

namespace RPGame.Inventory.Logic
{
    [Serializable]
    public sealed class ConsumableSlot
    {
        [SerializeField] private ConsumableSlotType slotType;
        [SerializeField] private ItemInstance item;

        public ConsumableSlotType SlotType => slotType;
        public ItemInstance Item => item;
        public bool HasItem => item != null && item.Definition != null && item.StackSize > 0;

        public ConsumableSlot(ConsumableSlotType slotType)
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
