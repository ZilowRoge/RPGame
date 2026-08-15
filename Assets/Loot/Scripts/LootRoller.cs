using System;
using System.Collections.Generic;

namespace RPGame.Loot
{
    public delegate int LootAmountRandomizer(int minInclusive, int maxInclusive);

    public sealed class LootRoller
    {
        private readonly LootAmountRandomizer randomizeAmount;

        public LootRoller()
            : this(CreateDefaultRandomizer())
        {
        }

        public LootRoller(LootAmountRandomizer randomizeAmount)
        {
            this.randomizeAmount = randomizeAmount ?? throw new ArgumentNullException(nameof(randomizeAmount));
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

                int amount = randomizeAmount(entry.MinAmount, entry.MaxAmount);
                results.Add(new LootResult(entry.Item, amount));
            }

            return results;
        }

        private static LootAmountRandomizer CreateDefaultRandomizer()
        {
            Random random = new Random();
            return (minInclusive, maxInclusive) => random.Next(minInclusive, maxInclusive + 1);
        }
    }
}
