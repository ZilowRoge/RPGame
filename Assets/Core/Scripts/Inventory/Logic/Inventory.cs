using System;
using System.Collections.Generic;
using RPGame.Core.Inventory.Data;
using UnityEngine;

namespace RPGame.Core.Inventory.Logic
{
    [Serializable]
    public sealed class Inventory
    {
        [SerializeField] private List<InventorySlot> slots = new();

        public event Action OnInventoryChanged;

        public IReadOnlyList<InventorySlot> Slots => slots;
        public int Size => slots.Count;

        public Inventory()
        {
        }

        public Inventory(int size)
        {
            Initialize(size);
        }

        public void Initialize(int size)
        {
            slots = new List<InventorySlot>(Math.Max(0, size));

            for (int i = 0; i < size; i++)
            {
                slots.Add(new InventorySlot(i));
            }
        }

        public bool AddItem(ItemInstance item, int amount = 1)
        {
            if (!CanAddItem(item, amount))
            {
                return false;
            }

            if (item.Definition.MaxStack <= 1)
            {
                InventorySlot emptySlot = FindEmptySlot();
                emptySlot.SetItem(item);
                OnInventoryChanged?.Invoke();
                return true;
            }

            int remainingAmount = amount;
            FillExistingStacks(item.Definition, ref remainingAmount);
            FillEmptySlots(item, ref remainingAmount);

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool RemoveItem(int index, int amount = 1)
        {
            if (!IsValidIndex(index) || amount <= 0)
            {
                return false;
            }

            InventorySlot slot = slots[index];
            if (!slot.HasItem || slot.Item.StackSize < amount)
            {
                return false;
            }

            if (slot.Item.StackSize == amount)
            {
                slot.Clear();
            }
            else
            {
                slot.Item.RemoveFromStack(amount);
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool MoveItem(int from, int to, int amount = 1)
        {
            if (!IsValidIndex(from) || !IsValidIndex(to) || amount <= 0 || from == to)
            {
                return false;
            }

            InventorySlot fromSlot = slots[from];
            InventorySlot toSlot = slots[to];

            if (!fromSlot.HasItem || fromSlot.Item.StackSize < amount)
            {
                return false;
            }

            ItemInstance item = fromSlot.Item;
            if (!toSlot.HasItem)
            {
                MoveToEmptySlot(fromSlot, toSlot, amount);
                OnInventoryChanged?.Invoke();
                return true;
            }

            if (!toSlot.Item.CanStackWith(item) || !toSlot.Item.CanAddToStack(amount))
            {
                return false;
            }

            toSlot.Item.AddToStack(amount);
            RemoveFromSourceSlot(fromSlot, amount);
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool CanAddItem(ItemInstance item, int amount = 1)
        {
            if (item == null || item.Definition == null || amount <= 0 || amount > item.StackSize)
            {
                return false;
            }

            ItemDefinition definition = item.Definition;
            if (definition.MaxStack <= 1)
            {
                return FindEmptySlot() != null;
            }

            return GetAvailableStackSpace(definition) >= amount;
        }

        public InventorySlot GetSlot(int index)
        {
            return IsValidIndex(index) ? slots[index] : null;
        }

        private void FillExistingStacks(ItemDefinition definition, ref int remainingAmount)
        {
            for (int i = 0; i < slots.Count && remainingAmount > 0; i++)
            {
                ItemInstance slotItem = slots[i].Item;
                if (slotItem == null || slotItem.Definition != definition)
                {
                    continue;
                }

                int amountToAdd = Math.Min(definition.MaxStack - slotItem.StackSize, remainingAmount);
                if (amountToAdd <= 0)
                {
                    continue;
                }

                slotItem.AddToStack(amountToAdd);
                remainingAmount -= amountToAdd;
            }
        }

        private void FillEmptySlots(ItemInstance item, ref int remainingAmount)
        {
            while (remainingAmount > 0)
            {
                InventorySlot emptySlot = FindEmptySlot();
                int amountToAdd = Math.Min(item.Definition.MaxStack, remainingAmount);
                ItemInstance slotItem = amountToAdd == item.StackSize ? item : new ItemInstance(item.Definition, amountToAdd);
                emptySlot.SetItem(slotItem);
                remainingAmount -= amountToAdd;
            }
        }

        private int GetAvailableStackSpace(ItemDefinition definition)
        {
            int availableSpace = 0;

            for (int i = 0; i < slots.Count; i++)
            {
                ItemInstance slotItem = slots[i].Item;
                if (slotItem == null)
                {
                    availableSpace += definition.MaxStack;
                    continue;
                }

                if (slotItem.Definition == definition)
                {
                    availableSpace += definition.MaxStack - slotItem.StackSize;
                }
            }

            return availableSpace;
        }

        private InventorySlot FindEmptySlot()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].HasItem)
                {
                    return slots[i];
                }
            }

            return null;
        }

        private void MoveToEmptySlot(InventorySlot fromSlot, InventorySlot toSlot, int amount)
        {
            if (fromSlot.Item.StackSize == amount)
            {
                toSlot.SetItem(fromSlot.Clear());
                return;
            }

            toSlot.SetItem(new ItemInstance(fromSlot.Item.Definition, amount));
            fromSlot.Item.RemoveFromStack(amount);
        }

        private static void RemoveFromSourceSlot(InventorySlot fromSlot, int amount)
        {
            if (fromSlot.Item.StackSize == amount)
            {
                fromSlot.Clear();
                return;
            }

            fromSlot.Item.RemoveFromStack(amount);
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < slots.Count;
        }
    }
}
