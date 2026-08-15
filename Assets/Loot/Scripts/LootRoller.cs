using System;
using System.Collections.Generic;

namespace RPGame.Loot
{
    public delegate int LootAmountRandomizer(int minInclusive, int maxInclusive);
    public delegate float LootChanceRandomizer();

    public sealed class LootRoller
    {
        private readonly LootAmountRandomizer randomizeAmount;
        private readonly LootChanceRandomizer randomizeChance;

        public LootRoller()
            : this(CreateDefaultAmountRandomizer(), CreateDefaultChanceRandomizer())
        {
        }

        public LootRoller(LootAmountRandomizer randomizeAmount)
            : this(randomizeAmount, CreateDefaultChanceRandomizer())
        {
        }

        public LootRoller(LootAmountRandomizer randomizeAmount, LootChanceRandomizer randomizeChance)
        {
            this.randomizeAmount = randomizeAmount ?? throw new ArgumentNullException(nameof(randomizeAmount));
            this.randomizeChance = randomizeChance ?? throw new ArgumentNullException(nameof(randomizeChance));
        }

        public List<LootResult> Roll(LootTable table)
        {
            List<LootResult> results = new();
            if (table == null)
            {
                return results;
            }

            IReadOnlyList<LootEntry> entries = table.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                LootEntry entry = entries[i];
                if (entry == null)
                {
                    continue;
                }

                if (!RollChance(entry.Chance))
                {
                    continue;
                }

                int amount = randomizeAmount(entry.MinAmount, entry.MaxAmount);
                results.Add(new LootResult(entry.Item, amount));
            }

            return results;
        }

        private bool RollChance(float chance)
        {
            return chance >= 1f || chance > 0f && randomizeChance() < chance;
        }

        private static LootAmountRandomizer CreateDefaultAmountRandomizer()
        {
            Random random = new Random();
            return (minInclusive, maxInclusive) => random.Next(minInclusive, maxInclusive + 1);
        }

        private static LootChanceRandomizer CreateDefaultChanceRandomizer()
        {
            Random random = new Random();
            return () => (float)random.NextDouble();
        }
    }
}
