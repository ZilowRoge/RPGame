using System;
using UnityEngine;

namespace RPGame.Inventory.Logic
{
    [Serializable]
    public sealed class InventorySlot
    {
        [SerializeField] private int slotId;
        [SerializeField] private ItemInstance item;

        public int SlotId => slotId;
        public ItemInstance Item => item;
        public bool HasItem => item != null && item.Definition != null && item.StackSize > 0;

        public InventorySlot(int slotId)
        {
            this.slotId = slotId;
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
