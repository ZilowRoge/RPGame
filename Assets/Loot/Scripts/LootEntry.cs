using System;
using RPGame.Inventory.Data;
using UnityEngine;

namespace RPGame.Loot
{
    [Serializable]
    public sealed class LootEntry : ISerializationCallbackReceiver
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int minAmount = 1;
        [SerializeField] private int maxAmount = 1;
        [SerializeField, Range(0f, 1f)] private float chance = 1f;

        public ItemDefinition Item => item;
        public int MinAmount => minAmount;
        public int MaxAmount => maxAmount;
        public float Chance => chance;

        public LootEntry()
        {
            ValidateAmounts();
        }

        public LootEntry(ItemDefinition item, int minAmount, int maxAmount, float chance = 1f)
        {
            this.item = item;
            this.minAmount = minAmount;
            this.maxAmount = maxAmount;
            this.chance = chance;
            ValidateAmounts();
        }

        public void OnBeforeSerialize()
        {
            ValidateAmounts();
        }

        public void OnAfterDeserialize()
        {
            ValidateAmounts();
        }

        internal void ValidateAmounts()
        {
            minAmount = Mathf.Max(1, minAmount);
            maxAmount = Mathf.Max(minAmount, maxAmount);
            chance = Mathf.Clamp01(chance);
        }
    }
}
