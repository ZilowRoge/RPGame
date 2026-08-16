using System;
using RPGame.Inventory.Data;
using UnityEngine;

namespace RPGame.Loot
{
    [Serializable]
    public sealed class WeightedLootEntry : LootEntry
    {
        [SerializeField, Min(0f)] private float weight = 1f;

        public float Weight => weight;

        public WeightedLootEntry()
        {
            ValidateWeight();
        }

        public WeightedLootEntry(ItemDefinition item, int minAmount, int maxAmount, float weight = 1f)
            : base(item, minAmount, maxAmount)
        {
            this.weight = weight;
            ValidateWeight();
        }

        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            ValidateWeight();
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            ValidateWeight();
        }

        internal void Validate()
        {
            ValidateAmounts();
            ValidateWeight();
        }

        private void ValidateWeight()
        {
            weight = Mathf.Max(0f, weight);
        }
    }
}
