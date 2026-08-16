using NUnit.Framework;
using RPGame.Inventory.Data;
using UnityEngine;

namespace RPGame.Loot.Tests
{
    public sealed class LootEntryTests
    {
        private ItemDefinition itemDefinition;

        [SetUp]
        public void SetUp()
        {
            itemDefinition = ScriptableObject.CreateInstance<ItemDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(itemDefinition);
        }

        [Test]
        public void Constructor_WhenRangeIsOneToOne_KeepsRange()
        {
            LootEntry entry = new IndependentLootEntry(itemDefinition, 1, 1);

            Assert.AreSame(itemDefinition, entry.Item);
            Assert.AreEqual(1, entry.MinAmount);
            Assert.AreEqual(1, entry.MaxAmount);
        }

        [Test]
        public void Constructor_WhenRangeIsOneToFive_KeepsRange()
        {
            LootEntry entry = new IndependentLootEntry(itemDefinition, 1, 5);

            Assert.AreSame(itemDefinition, entry.Item);
            Assert.AreEqual(1, entry.MinAmount);
            Assert.AreEqual(5, entry.MaxAmount);
        }

        [Test]
        public void IndependentConstructor_WhenChanceIsNotProvided_DefaultsChanceToOne()
        {
            IndependentLootEntry entry = new IndependentLootEntry(itemDefinition, 1, 1);

            Assert.AreEqual(1f, entry.Chance);
        }

        [Test]
        public void Constructor_WhenMinAmountIsLessThanOne_ClampsMinAmountToOne()
        {
            LootEntry entry = new IndependentLootEntry(itemDefinition, 0, 3);

            Assert.AreEqual(1, entry.MinAmount);
            Assert.AreEqual(3, entry.MaxAmount);
        }

        [Test]
        public void Constructor_WhenMaxAmountIsLessThanMinAmount_ClampsMaxAmountToMinAmount()
        {
            LootEntry entry = new IndependentLootEntry(itemDefinition, 4, 2);

            Assert.AreEqual(4, entry.MinAmount);
            Assert.AreEqual(4, entry.MaxAmount);
        }

        [Test]
        public void IndependentConstructor_WhenChanceIsLessThanZero_ClampsChanceToZero()
        {
            IndependentLootEntry entry = new IndependentLootEntry(itemDefinition, 1, 1, -0.25f);

            Assert.AreEqual(0f, entry.Chance);
        }

        [Test]
        public void IndependentConstructor_WhenChanceIsGreaterThanOne_ClampsChanceToOne()
        {
            IndependentLootEntry entry = new IndependentLootEntry(itemDefinition, 1, 1, 1.25f);

            Assert.AreEqual(1f, entry.Chance);
        }

        [Test]
        public void IndependentConstructor_WhenChanceIsInRange_KeepsChance()
        {
            IndependentLootEntry entry = new IndependentLootEntry(itemDefinition, 1, 1, 0.35f);

            Assert.AreEqual(0.35f, entry.Chance);
        }

        [Test]
        public void WeightedConstructor_WhenWeightIsNotProvided_DefaultsWeightToOne()
        {
            WeightedLootEntry entry = new WeightedLootEntry(itemDefinition, 1, 1);

            Assert.AreEqual(1f, entry.Weight);
        }

        [Test]
        public void WeightedConstructor_WhenWeightIsLessThanZero_ClampsWeightToZero()
        {
            WeightedLootEntry entry = new WeightedLootEntry(itemDefinition, 1, 1, -0.25f);

            Assert.AreEqual(0f, entry.Weight);
        }

        [Test]
        public void WeightedConstructor_WhenWeightIsPositive_KeepsWeight()
        {
            WeightedLootEntry entry = new WeightedLootEntry(itemDefinition, 1, 1, 2.5f);

            Assert.AreEqual(2.5f, entry.Weight);
        }
    }
}
