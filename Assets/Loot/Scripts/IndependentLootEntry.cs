using System;
using RPGame.Inventory.Data;
using UnityEngine;

namespace RPGame.Loot
{
    [Serializable]
    public sealed class IndependentLootEntry : LootEntry
    {
        [SerializeField, Range(0f, 1f)] private float chance = 1f;

        public float Chance => chance;

        public IndependentLootEntry()
        {
            ValidateChance();
        }

        public IndependentLootEntry(ItemDefinition item, int minAmount, int maxAmount, float chance = 1f)
            : base(item, minAmount, maxAmount)
        {
            this.chance = chance;
            ValidateChance();
        }

        public override void OnBeforeSerialize()
        {
            base.OnBeforeSerialize();
            ValidateChance();
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            ValidateChance();
        }

        internal void Validate()
        {
            ValidateAmounts();
            ValidateChance();
        }

        private void ValidateChance()
        {
            chance = Mathf.Clamp01(chance);
        }
    }
}
