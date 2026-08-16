using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Loot
{
    [Serializable]
    public sealed class IndependentLootGroup
    {
        [SerializeField] private List<IndependentLootEntry> entries = new();

        public IReadOnlyList<IndependentLootEntry> Entries => entries;

        public IndependentLootGroup()
        {
        }

        public IndependentLootGroup(params IndependentLootEntry[] entries)
        {
            if (entries != null)
            {
                this.entries.AddRange(entries);
            }
        }

        public List<LootResult> Roll(
            LootAmountRandomizer randomizeAmount,
            LootChanceRandomizer randomizeChance)
        {
            List<LootResult> results = new();
            if (randomizeAmount == null || randomizeChance == null)
            {
                return results;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                IndependentLootEntry entry = entries[i];
                if (entry == null || !RollChance(entry.Chance, randomizeChance))
                {
                    continue;
                }

                int amount = randomizeAmount(entry.MinAmount, entry.MaxAmount);
                results.Add(new LootResult(entry.Item, amount));
            }

            return results;
        }

        internal void Validate()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i]?.Validate();
            }
        }

        private static bool RollChance(float chance, LootChanceRandomizer randomizeChance)
        {
            return chance >= 1f || chance > 0f && randomizeChance() < chance;
        }
    }
}
