using System;
using RPGame.Inventory.Data;
using UnityEngine;

namespace RPGame.Inventory.Logic
{
    [Serializable]
    public sealed class ItemInstance
    {
        [SerializeField] private ItemDefinition definition;
        [SerializeField] private string instanceId;
        [SerializeField] private int stackSize;

        public ItemDefinition Definition => definition;
        public string InstanceId => instanceId;
        public int StackSize => stackSize;

        public ItemInstance(ItemDefinition definition, int stackSize = 1)
            : this(definition, Guid.NewGuid().ToString("N"), stackSize)
        {
        }

        public ItemInstance(ItemDefinition definition, string instanceId, int stackSize = 1)
        {
            this.definition = definition;
            this.instanceId = string.IsNullOrWhiteSpace(instanceId) ? Guid.NewGuid().ToString("N") : instanceId;
            this.stackSize = Mathf.Clamp(stackSize, 1, definition != null ? definition.MaxStack : 1);
        }

        public bool CanStackWith(ItemInstance other)
        {
            return other != null && definition != null && ReferenceEquals(definition, other.definition);
        }

        public bool CanAddToStack(int amount)
        {
            return definition != null && amount > 0 && stackSize + amount <= definition.MaxStack;
        }

        public bool AddToStack(int amount)
        {
            if (!CanAddToStack(amount))
            {
                return false;
            }

            stackSize += amount;
            return true;
        }

        public bool RemoveFromStack(int amount)
        {
            if (amount <= 0 || amount > stackSize)
            {
                return false;
            }

            stackSize -= amount;
            return true;
        }
    }
}
