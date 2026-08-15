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
        public void Roll_WhenAmountIsFixed_ReturnsFixedAmount()
        {
            ItemDefinition item = CreateItemDefinition();
            LootTable table = CreateLootTable((item, 1, 1));
            LootRoller roller = new LootRoller((min, max) => min);

            List<LootResult> results = roller.Roll(table);

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(item, results[0].Item);
            Assert.AreEqual(1, results[0].Amount);
        }

        [Test]
        public void Roll_WhenAmountIsInRange_ReturnsAmountWithinRange()
        {
            ItemDefinition item = CreateItemDefinition();
            LootTable table = CreateLootTable((item, 2, 5));
            LootRoller roller = new LootRoller((min, max) => 4);

            List<LootResult> results = roller.Roll(table);

            Assert.AreEqual(1, results.Count);
            Assert.GreaterOrEqual(results[0].Amount, 2);
            Assert.LessOrEqual(results[0].Amount, 5);
        }

        [Test]
        public void Roll_WhenRandomizerReturnsLowerBound_ReturnsMinAmount()
        {
            ItemDefinition item = CreateItemDefinition();
            LootTable table = CreateLootTable((item, 2, 5));
            LootRoller roller = new LootRoller((min, max) => min);

            List<LootResult> results = roller.Roll(table);

            Assert.AreEqual(2, results[0].Amount);
        }

        [Test]
        public void Roll_WhenRandomizerReturnsUpperBound_ReturnsMaxAmount()
        {
            ItemDefinition item = CreateItemDefinition();
            LootTable table = CreateLootTable((item, 2, 5));
            LootRoller roller = new LootRoller((min, max) => max);

            List<LootResult> results = roller.Roll(table);

            Assert.AreEqual(5, results[0].Amount);
        }

        [Test]
        public void Roll_WhenTableHasMultipleEntries_ReturnsOneResultPerEntry()
        {
            ItemDefinition firstItem = CreateItemDefinition();
            ItemDefinition secondItem = CreateItemDefinition();
            ItemDefinition thirdItem = CreateItemDefinition();
            LootTable table = CreateLootTable(
                (firstItem, 1, 1),
                (secondItem, 2, 2),
                (thirdItem, 3, 3));
            LootRoller roller = new LootRoller((min, max) => min);

            List<LootResult> results = roller.Roll(table);

            Assert.AreEqual(3, results.Count);
            Assert.AreSame(firstItem, results[0].Item);
            Assert.AreSame(secondItem, results[1].Item);
            Assert.AreSame(thirdItem, results[2].Item);
        }

        [Test]
        public void Roll_WhenTableIsEmpty_ReturnsEmptyResults()
        {
            LootTable table = CreateLootTable();
            LootRoller roller = new LootRoller((min, max) => min);

            List<LootResult> results = roller.Roll(table);

            Assert.IsEmpty(results);
        }

        [Test]
        public void Roll_DoesNotModifyLootTableEntries()
        {
            ItemDefinition item = CreateItemDefinition();
            LootTable table = CreateLootTableWithChance((item, 2, 5, 0.75f));
            LootRoller roller = new LootRoller((min, max) => max);

            roller.Roll(table);

            Assert.AreEqual(2, table.Entries[0].MinAmount);
            Assert.AreEqual(5, table.Entries[0].MaxAmount);
            Assert.AreEqual(0.75f, table.Entries[0].Chance);
        }

        [Test]
        public void Roll_WhenChanceIsZero_DoesNotReturnEntry()
        {
            ItemDefinition item = CreateItemDefinition();
            LootTable table = CreateLootTableWithChance((item, 1, 1, 0f));
            LootRoller roller = new LootRoller((min, max) => min, () => 0f);

            List<LootResult> results = roller.Roll(table);

            Assert.IsEmpty(results);
        }

        [Test]
        public void Roll_WhenChanceIsOne_ReturnsEntry()
        {
            ItemDefinition item = CreateItemDefinition();
            LootTable table = CreateLootTableWithChance((item, 1, 1, 1f));
            LootRoller roller = new LootRoller((min, max) => min, () => 0.99f);

            List<LootResult> results = roller.Roll(table);

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(item, results[0].Item);
        }

        [Test]
        public void Roll_WhenChanceRollIsBelowThreshold_ReturnsEntry()
        {
            ItemDefinition item = CreateItemDefinition();
            LootTable table = CreateLootTableWithChance((item, 1, 1, 0.5f));
            LootRoller roller = new LootRoller((min, max) => min, () => 0.25f);

            List<LootResult> results = roller.Roll(table);

            Assert.AreEqual(1, results.Count);
            Assert.AreSame(item, results[0].Item);
        }

        [Test]
        public void Roll_WhenChanceRollIsAboveThreshold_DoesNotReturnEntry()
        {
            ItemDefinition item = CreateItemDefinition();
            LootTable table = CreateLootTableWithChance((item, 1, 1, 0.5f));
            LootRoller roller = new LootRoller((min, max) => min, () => 0.75f);

            List<LootResult> results = roller.Roll(table);

            Assert.IsEmpty(results);
        }

        [Test]
        public void Roll_WhenMultipleEntriesRollIndependently_ReturnsOnlySuccessfulEntries()
        {
            ItemDefinition firstItem = CreateItemDefinition();
            ItemDefinition secondItem = CreateItemDefinition();
            ItemDefinition thirdItem = CreateItemDefinition();
            LootTable table = CreateLootTableWithChance(
                (firstItem, 1, 1, 0.5f),
                (secondItem, 1, 1, 0.5f),
                (thirdItem, 1, 1, 0.5f));
            Queue<float> chanceRolls = new Queue<float>(new[] { 0.25f, 0.75f, 0.25f });
            LootRoller roller = new LootRoller((min, max) => min, chanceRolls.Dequeue);

            List<LootResult> results = roller.Roll(table);

            Assert.AreEqual(2, results.Count);
            Assert.AreSame(firstItem, results[0].Item);
            Assert.AreSame(thirdItem, results[1].Item);
        }

        [Test]
        public void Roll_WhenAllChanceRollsFail_ReturnsEmptyResults()
        {
            ItemDefinition firstItem = CreateItemDefinition();
            ItemDefinition secondItem = CreateItemDefinition();
            LootTable table = CreateLootTableWithChance(
                (firstItem, 1, 1, 0.5f),
                (secondItem, 1, 1, 0.5f));
            LootRoller roller = new LootRoller((min, max) => min, () => 0.75f);

            List<LootResult> results = roller.Roll(table);

            Assert.IsEmpty(results);
        }

        [Test]
        public void Roll_WhenChanceSucceeds_RollsAmountWithinRange()
        {
            ItemDefinition item = CreateItemDefinition();
            LootTable table = CreateLootTableWithChance((item, 2, 5, 1f));
            LootRoller roller = new LootRoller((min, max) => 4, () => 0f);

            List<LootResult> results = roller.Roll(table);

            Assert.AreEqual(1, results.Count);
            Assert.GreaterOrEqual(results[0].Amount, 2);
            Assert.LessOrEqual(results[0].Amount, 5);
        }

        [Test]
        public void Roll_WhenChanceFails_DoesNotRollAmount()
        {
            ItemDefinition item = CreateItemDefinition();
            LootTable table = CreateLootTableWithChance((item, 1, 1, 0.5f));
            int amountRolls = 0;
            LootRoller roller = new LootRoller(
                (min, max) =>
                {
                    amountRolls++;
                    return min;
                },
                () => 0.75f);

            List<LootResult> results = roller.Roll(table);

            Assert.IsEmpty(results);
            Assert.AreEqual(0, amountRolls);
        }

        private ItemDefinition CreateItemDefinition()
        {
            ItemDefinition itemDefinition = ScriptableObject.CreateInstance<ItemDefinition>();
            itemDefinitions.Add(itemDefinition);
            return itemDefinition;
        }

        private LootTable CreateLootTable(params (ItemDefinition item, int minAmount, int maxAmount)[] entries)
        {
            (ItemDefinition item, int minAmount, int maxAmount, float chance)[] entriesWithChance =
                new (ItemDefinition item, int minAmount, int maxAmount, float chance)[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                entriesWithChance[i] = (entries[i].item, entries[i].minAmount, entries[i].maxAmount, 1f);
            }

            return CreateLootTableWithChance(entriesWithChance);
        }

        private LootTable CreateLootTableWithChance(
            params (ItemDefinition item, int minAmount, int maxAmount, float chance)[] entries)
        {
            LootTable table = ScriptableObject.CreateInstance<LootTable>();
            lootTables.Add(table);

            SerializedObject serializedTable = new SerializedObject(table);
            SerializedProperty serializedEntries = serializedTable.FindProperty("entries");
            serializedEntries.arraySize = entries.Length;

            for (int i = 0; i < entries.Length; i++)
            {
                SerializedProperty entry = serializedEntries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("item").objectReferenceValue = entries[i].item;
                entry.FindPropertyRelative("minAmount").intValue = entries[i].minAmount;
                entry.FindPropertyRelative("maxAmount").intValue = entries[i].maxAmount;
                entry.FindPropertyRelative("chance").floatValue = entries[i].chance;
            }

            serializedTable.ApplyModifiedPropertiesWithoutUndo();
            return table;
        }
    }
}
