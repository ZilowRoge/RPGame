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
            LootEntry entry = new LootEntry(itemDefinition, 1, 1);

            Assert.AreSame(itemDefinition, entry.Item);
            Assert.AreEqual(1, entry.MinAmount);
            Assert.AreEqual(1, entry.MaxAmount);
        }

        [Test]
        public void Constructor_WhenRangeIsOneToFive_KeepsRange()
        {
            LootEntry entry = new LootEntry(itemDefinition, 1, 5);

            Assert.AreSame(itemDefinition, entry.Item);
            Assert.AreEqual(1, entry.MinAmount);
            Assert.AreEqual(5, entry.MaxAmount);
        }

        [Test]
        public void Constructor_WhenChanceIsNotProvided_DefaultsChanceToOne()
        {
            LootEntry entry = new LootEntry(itemDefinition, 1, 1);

            Assert.AreEqual(1f, entry.Chance);
        }

        [Test]
        public void Constructor_WhenMinAmountIsLessThanOne_ClampsMinAmountToOne()
        {
            LootEntry entry = new LootEntry(itemDefinition, 0, 3);

            Assert.AreEqual(1, entry.MinAmount);
            Assert.AreEqual(3, entry.MaxAmount);
        }

        [Test]
        public void Constructor_WhenMaxAmountIsLessThanMinAmount_ClampsMaxAmountToMinAmount()
        {
            LootEntry entry = new LootEntry(itemDefinition, 4, 2);

            Assert.AreEqual(4, entry.MinAmount);
            Assert.AreEqual(4, entry.MaxAmount);
        }

        [Test]
        public void Constructor_WhenChanceIsLessThanZero_ClampsChanceToZero()
        {
            LootEntry entry = new LootEntry(itemDefinition, 1, 1, -0.25f);

            Assert.AreEqual(0f, entry.Chance);
        }

        [Test]
        public void Constructor_WhenChanceIsGreaterThanOne_ClampsChanceToOne()
        {
            LootEntry entry = new LootEntry(itemDefinition, 1, 1, 1.25f);

            Assert.AreEqual(1f, entry.Chance);
        }

        [Test]
        public void Constructor_WhenChanceIsInRange_KeepsChance()
        {
            LootEntry entry = new LootEntry(itemDefinition, 1, 1, 0.35f);

            Assert.AreEqual(0.35f, entry.Chance);
        }
    }
}
