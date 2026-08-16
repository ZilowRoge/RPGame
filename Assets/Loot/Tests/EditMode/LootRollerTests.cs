using System.Collections.Generic;
using NUnit.Framework;
using RPGame.Inventory.Data;
using UnityEditor;
using UnityEngine;

namespace RPGame.Loot.Tests
{
    public sealed class LootRollerTests
    {
        private readonly List<ItemDefinition> itemDefinitions = new();
        private readonly List<LootTable> lootTables = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < lootTables.Count; i++)
            {
                Object.DestroyImmediate(lootTables[i]);
            }

            for (int i = 0; i < itemDefinitions.Count; i++)
            {
                Object.DestroyImmediate(itemDefinitions[i]);
            }

            lootTables.Clear();
            itemDefinitions.Clear();
        }

        [Test]
        public void IndependentGroup_WhenEmpty_ReturnsEmptyResults()
        {
            IndependentLootGroup group = new();

            List<LootResult> results = group.Roll((min, max) => min, () => 0f);

            Assert.IsEmpty(results);
        }

        [Test]
        public void IndependentGroup_WhenChanceIsZero_ReturnsEmptyResults()
        {
            IndependentLootGroup group = new(new IndependentLootEntry(CreateItemDefinition(), 1, 1, 0f));

            List<LootResult> results = group.Roll((min, max) => min, () => 0f);

            Assert.IsEmpty(results);
        }

        [Test]
        public void IndependentGroup_WhenChanceIsOne_ReturnsResult()
        {
            ItemDefinition item = CreateItemDefinition();
            IndependentLootGroup group = new(new IndependentLootEntry(item, 1, 1, 1f));

            List<LootResult> results = group.Roll((min, max) => min, () => 0.99f);

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(item, results[0].Item);
        }

        [Test]
        public void IndependentGroup_WhenChanceRollIsBelowThreshold_ReturnsResult()
        {
            ItemDefinition item = CreateItemDefinition();
            IndependentLootGroup group = new(new IndependentLootEntry(item, 1, 1, 0.5f));

            List<LootResult> results = group.Roll((min, max) => min, () => 0.25f);

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(item, results[0].Item);
        }

        [Test]
        public void IndependentGroup_WhenChanceRollIsEqualToThreshold_ReturnsEmptyResults()
        {
            IndependentLootGroup group = new(new IndependentLootEntry(CreateItemDefinition(), 1, 1, 0.5f));

            List<LootResult> results = group.Roll((min, max) => min, () => 0.5f);

            Assert.IsEmpty(results);
        }

        [Test]
        public void IndependentGroup_WhenChanceRollIsAboveThreshold_ReturnsEmptyResults()
        {
            IndependentLootGroup group = new(new IndependentLootEntry(CreateItemDefinition(), 1, 1, 0.5f));

            List<LootResult> results = group.Roll((min, max) => min, () => 0.75f);

            Assert.IsEmpty(results);
        }

        [Test]
        public void IndependentGroup_WhenMultipleEntriesSucceed_ReturnsMultipleResults()
        {
            ItemDefinition firstItem = CreateItemDefinition();
            ItemDefinition secondItem = CreateItemDefinition();
            IndependentLootGroup group = new(
                new IndependentLootEntry(firstItem, 1, 1, 0.5f),
                new IndependentLootEntry(secondItem, 1, 1, 0.5f));

            List<LootResult> results = group.Roll((min, max) => min, () => 0.25f);

            Assert.AreEqual(2, results.Count);
            Assert.AreSame(firstItem, results[0].Item);
            Assert.AreSame(secondItem, results[1].Item);
        }

        [Test]
        public void IndependentGroup_WhenAllEntriesFail_ReturnsEmptyResults()
        {
            IndependentLootGroup group = new(
                new IndependentLootEntry(CreateItemDefinition(), 1, 1, 0.5f),
                new IndependentLootEntry(CreateItemDefinition(), 1, 1, 0.5f));

            List<LootResult> results = group.Roll((min, max) => min, () => 0.75f);

            Assert.IsEmpty(results);
        }

        [Test]
        public void IndependentGroup_WhenAmountIsRolled_ReturnsAmountWithinRange()
        {
            IndependentLootGroup group = new(new IndependentLootEntry(CreateItemDefinition(), 2, 5, 1f));

            List<LootResult> results = group.Roll((min, max) => 4, () => 0f);

            Assert.AreEqual(1, results.Count);
            Assert.GreaterOrEqual(results[0].Amount, 2);
            Assert.LessOrEqual(results[0].Amount, 5);
        }

        [Test]
        public void IndependentGroup_WhenAmountRandomizerReturnsLowerBound_ReturnsMinAmount()
        {
            IndependentLootGroup group = new(new IndependentLootEntry(CreateItemDefinition(), 2, 5, 1f));

            List<LootResult> results = group.Roll((min, max) => min, () => 0f);

            Assert.AreEqual(2, results[0].Amount);
        }

        [Test]
        public void IndependentGroup_WhenAmountRandomizerReturnsUpperBound_ReturnsMaxAmount()
        {
            IndependentLootGroup group = new(new IndependentLootEntry(CreateItemDefinition(), 2, 5, 1f));

            List<LootResult> results = group.Roll((min, max) => max, () => 0f);

            Assert.AreEqual(5, results[0].Amount);
        }

        [Test]
        public void WeightedGroup_WithOneEntry_SelectsThatEntry()
        {
            ItemDefinition item = CreateItemDefinition();
            WeightedLootGroup group = new(new WeightedLootEntry(item, 1, 1, 1f));

            List<LootResult> results = group.Roll((min, max) => min, max => 0f);

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(item, results[0].Item);
        }

        [Test]
        public void WeightedGroup_WhenEntryHasZeroWeight_DoesNotSelectIt()
        {
            ItemDefinition zeroWeightItem = CreateItemDefinition();
            ItemDefinition positiveWeightItem = CreateItemDefinition();
            WeightedLootGroup group = new(
                new WeightedLootEntry(zeroWeightItem, 1, 1, 0f),
                new WeightedLootEntry(positiveWeightItem, 1, 1, 1f));

            List<LootResult> results = group.Roll((min, max) => min, max => 0f);

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(positiveWeightItem, results[0].Item);
        }

        [Test]
        public void WeightedGroup_WhenAllWeightsAreZero_ReturnsEmptyResults()
        {
            WeightedLootGroup group = new(
                new WeightedLootEntry(CreateItemDefinition(), 1, 1, 0f),
                new WeightedLootEntry(CreateItemDefinition(), 1, 1, 0f));

            List<LootResult> results = group.Roll((min, max) => min, max => 0f);

            Assert.IsEmpty(results);
        }

        [TestCase(0f, 0)]
        [TestCase(49.99f, 0)]
        [TestCase(50f, 1)]
        [TestCase(79.99f, 1)]
        [TestCase(80f, 2)]
        [TestCase(99.99f, 2)]
        public void WeightedGroup_WithMultipleEntries_SelectsEntryByWeightRange(float roll, int expectedIndex)
        {
            ItemDefinition[] items =
            {
                CreateItemDefinition(),
                CreateItemDefinition(),
                CreateItemDefinition()
            };
            WeightedLootGroup group = new(
                new WeightedLootEntry(items[0], 1, 1, 50f),
                new WeightedLootEntry(items[1], 1, 1, 30f),
                new WeightedLootEntry(items[2], 1, 1, 20f));

            List<LootResult> results = group.Roll((min, max) => min, max => roll);

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(items[expectedIndex], results[0].Item);
        }

        [Test]
        public void WeightedGroup_ReturnsAtMostOneResult()
        {
            WeightedLootGroup group = new(
                new WeightedLootEntry(CreateItemDefinition(), 1, 1, 1f),
                new WeightedLootEntry(CreateItemDefinition(), 1, 1, 1f));

            List<LootResult> results = group.Roll((min, max) => min, max => 0f);

            Assert.AreEqual(1, results.Count);
        }

        [Test]
        public void WeightedGroup_WhenAmountIsRolled_ReturnsAmountWithinRange()
        {
            WeightedLootGroup group = new(new WeightedLootEntry(CreateItemDefinition(), 2, 5, 1f));

            List<LootResult> results = group.Roll((min, max) => 4, max => 0f);

            Assert.AreEqual(1, results.Count);
            Assert.GreaterOrEqual(results[0].Amount, 2);
            Assert.LessOrEqual(results[0].Amount, 5);
        }

        [Test]
        public void Roll_WhenTableIsEmpty_ReturnsEmptyResults()
        {
            LootTable table = CreateLootTable();
            LootRoller roller = new((min, max) => min);

            List<LootResult> results = roller.Roll(table);

            Assert.IsEmpty(results);
        }

        [Test]
        public void Roll_DoesNotModifyLootTableEntries()
        {
            ItemDefinition item = CreateItemDefinition();
            LootTable table = CreateLootTable(
                new[] { new[] { (item, 2, 5, 0.75f) } },
                null);
            LootRoller roller = new((min, max) => max);

            roller.Roll(table);

            IndependentLootEntry entry = table.IndependentGroups[0].Entries[0];
            Assert.AreEqual(2, entry.MinAmount);
            Assert.AreEqual(5, entry.MaxAmount);
            Assert.AreEqual(0.75f, entry.Chance);
        }

        [Test]
        public void Roll_WhenTableHasMultipleWeightedGroups_ReturnsOneResultPerGroup()
        {
            ItemDefinition sword = CreateItemDefinition();
            ItemDefinition iron = CreateItemDefinition();
            LootTable table = CreateLootTable(
                null,
                new[]
                {
                    new[] { (sword, 1, 1, 1f) },
                    new[] { (iron, 1, 1, 1f) }
                });
            LootRoller roller = new((min, max) => min, () => 0f, max => 0f);

            List<LootResult> results = roller.Roll(table);

            Assert.AreEqual(2, results.Count);
            Assert.AreSame(sword, results[0].Item);
            Assert.AreSame(iron, results[1].Item);
        }

        [Test]
        public void Roll_WhenTableHasIndependentAndWeightedGroups_ReturnsResultsFromBoth()
        {
            ItemDefinition potion = CreateItemDefinition();
            ItemDefinition sword = CreateItemDefinition();
            LootTable table = CreateLootTable(
                new[] { new[] { (potion, 1, 1, 1f) } },
                new[] { new[] { (sword, 1, 1, 1f) } });
            LootRoller roller = new((min, max) => min, () => 0f, max => 0f);

            List<LootResult> results = roller.Roll(table);

            Assert.AreEqual(2, results.Count);
            Assert.AreSame(potion, results[0].Item);
            Assert.AreSame(sword, results[1].Item);
        }

        private ItemDefinition CreateItemDefinition()
        {
            ItemDefinition itemDefinition = ScriptableObject.CreateInstance<ItemDefinition>();
            itemDefinitions.Add(itemDefinition);
            return itemDefinition;
        }

        private LootTable CreateLootTable(
            IReadOnlyList<(ItemDefinition item, int minAmount, int maxAmount, float chance)[]> independentGroups = null,
            IReadOnlyList<(ItemDefinition item, int minAmount, int maxAmount, float weight)[]> weightedGroups = null)
        {
            LootTable table = ScriptableObject.CreateInstance<LootTable>();
            lootTables.Add(table);

            SerializedObject serializedTable = new(table);
            WriteIndependentGroups(serializedTable.FindProperty("independentGroups"), independentGroups);
            WriteWeightedGroups(serializedTable.FindProperty("weightedGroups"), weightedGroups);
            serializedTable.ApplyModifiedPropertiesWithoutUndo();
            return table;
        }

        private static void WriteIndependentGroups(
            SerializedProperty serializedGroups,
            IReadOnlyList<(ItemDefinition item, int minAmount, int maxAmount, float chance)[]> groups)
        {
            serializedGroups.arraySize = groups?.Count ?? 0;
            for (int groupIndex = 0; groupIndex < serializedGroups.arraySize; groupIndex++)
            {
                SerializedProperty serializedEntries = serializedGroups
                    .GetArrayElementAtIndex(groupIndex)
                    .FindPropertyRelative("entries");
                serializedEntries.arraySize = groups[groupIndex].Length;

                for (int entryIndex = 0; entryIndex < groups[groupIndex].Length; entryIndex++)
                {
                    SerializedProperty entry = serializedEntries.GetArrayElementAtIndex(entryIndex);
                    entry.FindPropertyRelative("item").objectReferenceValue = groups[groupIndex][entryIndex].item;
                    entry.FindPropertyRelative("minAmount").intValue = groups[groupIndex][entryIndex].minAmount;
                    entry.FindPropertyRelative("maxAmount").intValue = groups[groupIndex][entryIndex].maxAmount;
                    entry.FindPropertyRelative("chance").floatValue = groups[groupIndex][entryIndex].chance;
                }
            }
        }

        private static void WriteWeightedGroups(
            SerializedProperty serializedGroups,
            IReadOnlyList<(ItemDefinition item, int minAmount, int maxAmount, float weight)[]> groups)
        {
            serializedGroups.arraySize = groups?.Count ?? 0;
            for (int groupIndex = 0; groupIndex < serializedGroups.arraySize; groupIndex++)
            {
                SerializedProperty serializedEntries = serializedGroups
                    .GetArrayElementAtIndex(groupIndex)
                    .FindPropertyRelative("entries");
                serializedEntries.arraySize = groups[groupIndex].Length;

                for (int entryIndex = 0; entryIndex < groups[groupIndex].Length; entryIndex++)
                {
                    SerializedProperty entry = serializedEntries.GetArrayElementAtIndex(entryIndex);
                    entry.FindPropertyRelative("item").objectReferenceValue = groups[groupIndex][entryIndex].item;
                    entry.FindPropertyRelative("minAmount").intValue = groups[groupIndex][entryIndex].minAmount;
                    entry.FindPropertyRelative("maxAmount").intValue = groups[groupIndex][entryIndex].maxAmount;
                    entry.FindPropertyRelative("weight").floatValue = groups[groupIndex][entryIndex].weight;
                }
            }
        }
    }
}
