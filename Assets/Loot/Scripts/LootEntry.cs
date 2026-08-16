using System;
using RPGame.Inventory.Data;
using UnityEngine;

namespace RPGame.Loot
{
    [Serializable]
    public abstract class LootEntry : ISerializationCallbackReceiver
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int minAmount = 1;
        [SerializeField] private int maxAmount = 1;

        public ItemDefinition Item => item;
        public int MinAmount => minAmount;
        public int MaxAmount => maxAmount;

        public LootEntry()
        {
            ValidateAmounts();
        }

        public LootEntry(ItemDefinition item, int minAmount, int maxAmount)
        {
            this.item = item;
            this.minAmount = minAmount;
            this.maxAmount = maxAmount;
            ValidateAmounts();
        }

        public virtual void OnBeforeSerialize()
        {
            ValidateAmounts();
        }

        public virtual void OnAfterDeserialize()
        {
            ValidateAmounts();
        }

        internal void ValidateAmounts()
        {
            minAmount = Mathf.Max(1, minAmount);
            maxAmount = Mathf.Max(minAmount, maxAmount);
        }
    }
}
