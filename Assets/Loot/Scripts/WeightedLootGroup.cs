using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Loot
{
    [Serializable]
    public sealed class WeightedLootGroup
    {
        [SerializeField] private List<WeightedLootEntry> entries = new();

        public IReadOnlyList<WeightedLootEntry> Entries => entries;

        public WeightedLootGroup()
        {
        }

        public WeightedLootGroup(params WeightedLootEntry[] entries)
        {
            if (entries != null)
            {
                this.entries.AddRange(entries);
            }
        }

        public List<LootResult> Roll(
            LootAmountRandomizer randomizeAmount,
            LootWeightRandomizer randomizeWeight)
        {
            List<LootResult> results = new();
            if (randomizeAmount == null || randomizeWeight == null)
            {
                return results;
            }

            float totalWeight = GetTotalWeight();
            if (totalWeight <= 0f)
            {
                return results;
            }

            WeightedLootEntry selectedEntry = SelectEntry(randomizeWeight(totalWeight));
            if (selectedEntry == null)
            {
                return results;
            }

            int amount = randomizeAmount(selectedEntry.MinAmount, selectedEntry.MaxAmount);
            results.Add(new LootResult(selectedEntry.Item, amount));
            return results;
        }

        internal void Validate()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i]?.Validate();
            }
        }

        private float GetTotalWeight()
        {
            float totalWeight = 0f;
            for (int i = 0; i < entries.Count; i++)
            {
                WeightedLootEntry entry = entries[i];
                if (entry != null && entry.Weight > 0f)
                {
                    totalWeight += entry.Weight;
                }
            }

            return totalWeight;
        }

        private WeightedLootEntry SelectEntry(float roll)
        {
            float cumulativeWeight = 0f;
            WeightedLootEntry fallbackEntry = null;
            for (int i = 0; i < entries.Count; i++)
            {
                WeightedLootEntry entry = entries[i];
                if (entry == null || entry.Weight <= 0f)
                {
                    continue;
                }

                fallbackEntry = entry;
                cumulativeWeight += entry.Weight;
                if (roll < cumulativeWeight)
                {
                    return entry;
                }
            }

            return fallbackEntry;
        }
    }
}
