using System;
using System.Collections.Generic;
using RPGame.Inventory.Data;
using UnityEngine;

namespace RPGame.Inventory.Logic
{
    [Serializable]
    public sealed class ConsumableSlots
    {
        [SerializeField] private List<ConsumableSlot> slots = new();

        public event Action OnConsumableSlotsChanged;

        public IReadOnlyList<ConsumableSlot> Slots => slots;

        public ConsumableSlots()
        {
            Initialize();
        }

        public void Initialize()
        {
            slots ??= new List<ConsumableSlot>();

            foreach (ConsumableSlotType slotType in Enum.GetValues(typeof(ConsumableSlotType)))
            {
                if (GetSlot(slotType) == null)
                {
                    slots.Add(new ConsumableSlot(slotType));
                }
            }
        }

        public bool SetItem(ConsumableSlotType slotType, ItemInstance item)
        {
            ConsumableSlot slot = GetSlot(slotType);
            if (slot == null || (item != null && !CanSetItem(item)))
            {
                return false;
            }

            slot.SetItem(item);
            OnConsumableSlotsChanged?.Invoke();
            return true;
        }

        public ItemInstance RemoveItem(ConsumableSlotType slotType, int amount = 1)
        {
            ConsumableSlot slot = GetSlot(slotType);
            if (slot == null || !slot.HasItem || amount <= 0 || slot.Item.StackSize < amount)
            {
                return null;
            }

            if (slot.Item.StackSize == amount)
            {
                ItemInstance item = slot.Clear();
                OnConsumableSlotsChanged?.Invoke();
                return item;
            }

            ItemInstance removedItem = new ItemInstance(slot.Item.Definition, amount);
            slot.Item.RemoveFromStack(amount);
            OnConsumableSlotsChanged?.Invoke();
            return removedItem;
        }

        public bool CanSetItem(ItemInstance item)
        {
            return item != null
                && item.Definition != null
                && item.Definition.ItemType == ItemType.Consumable;
        }

        public ConsumableSlot GetSlot(ConsumableSlotType slotType)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].SlotType == slotType)
                {
                    return slots[i];
                }
            }

            return null;
        }
    }
}
