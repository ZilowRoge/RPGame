using System;
using System.Collections.Generic;

namespace RPGame.Loot
{
    public delegate int LootAmountRandomizer(int minInclusive, int maxInclusive);
    public delegate float LootChanceRandomizer();
    public delegate float LootWeightRandomizer(float maxExclusive);

    public sealed class LootRoller
    {
        private readonly LootAmountRandomizer randomizeAmount;
        private readonly LootChanceRandomizer randomizeChance;
        private readonly LootWeightRandomizer randomizeWeight;

        public LootRoller()
            : this(CreateDefaultAmountRandomizer(), CreateDefaultChanceRandomizer(), CreateDefaultWeightRandomizer())
        {
        }

        public LootRoller(LootAmountRandomizer randomizeAmount)
            : this(randomizeAmount, CreateDefaultChanceRandomizer(), CreateDefaultWeightRandomizer())
        {
        }

        public LootRoller(LootAmountRandomizer randomizeAmount, LootChanceRandomizer randomizeChance)
            : this(randomizeAmount, randomizeChance, CreateDefaultWeightRandomizer())
        {
        }

        public LootRoller(
            LootAmountRandomizer randomizeAmount,
            LootChanceRandomizer randomizeChance,
            LootWeightRandomizer randomizeWeight)
        {
            this.randomizeAmount = randomizeAmount ?? throw new ArgumentNullException(nameof(randomizeAmount));
            this.randomizeChance = randomizeChance ?? throw new ArgumentNullException(nameof(randomizeChance));
            this.randomizeWeight = randomizeWeight ?? throw new ArgumentNullException(nameof(randomizeWeight));
        }

        public List<LootResult> Roll(LootTable table)
        {
            List<LootResult> results = new();
            if (table == null)
            {
                return results;
            }

            RollIndependentGroups(table, results);
            RollWeightedGroups(table, results);
            return results;
        }

        private void RollIndependentGroups(LootTable table, List<LootResult> results)
        {
            IReadOnlyList<IndependentLootGroup> independentGroups = table.IndependentGroups;
            for (int i = 0; i < independentGroups.Count; i++)
            {
                IndependentLootGroup group = independentGroups[i];
                if (group != null)
                {
                    results.AddRange(group.Roll(randomizeAmount, randomizeChance));
                }
            }
        }

        private void RollWeightedGroups(LootTable table, List<LootResult> results)
        {
            IReadOnlyList<WeightedLootGroup> weightedGroups = table.WeightedGroups;
            for (int i = 0; i < weightedGroups.Count; i++)
            {
                WeightedLootGroup group = weightedGroups[i];
                if (group != null)
                {
                    results.AddRange(group.Roll(randomizeAmount, randomizeWeight));
                }
            }
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

        private static LootWeightRandomizer CreateDefaultWeightRandomizer()
        {
            Random random = new Random();
            return maxExclusive => (float)(random.NextDouble() * maxExclusive);
        }
    }
}
