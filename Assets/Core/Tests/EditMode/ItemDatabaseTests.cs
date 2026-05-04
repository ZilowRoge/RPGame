using NUnit.Framework;
using RPGame.Core.Inventory.Data;
using UnityEditor;
using UnityEngine;

namespace RPGame.Core.Tests
{
    public sealed class ItemDatabaseTests
    {
        private ItemDatabase database;
        private ItemDefinition sword;
        private ItemDefinition potion;

        [SetUp]
        public void SetUp()
        {
            database = ScriptableObject.CreateInstance<ItemDatabase>();
            sword = CreateItemDefinition("item_sword", "Sword");
            potion = CreateItemDefinition("item_potion", "Potion");
            SetDatabaseItems(database, sword, potion);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(database);
            Object.DestroyImmediate(sword);
            Object.DestroyImmediate(potion);
        }

        [Test]
        public void TryGetItem_WhenIdExists_ReturnsItem()
        {
            bool found = database.TryGetItem("item_sword", out ItemDefinition item);

            Assert.IsTrue(found);
            Assert.AreSame(sword, item);
        }

        [Test]
        public void TryGetItem_WhenIdDoesNotExist_ReturnsFalse()
        {
            bool found = database.TryGetItem("missing", out ItemDefinition item);

            Assert.IsFalse(found);
            Assert.IsNull(item);
        }

        [Test]
        public void Contains_WhenIdExists_ReturnsTrue()
        {
            Assert.IsTrue(database.Contains("item_potion"));
        }

        [Test]
        public void GetItem_WhenIdExists_ReturnsItem()
        {
            Assert.AreSame(potion, database.GetItem("item_potion"));
        }

        private static ItemDefinition CreateItemDefinition(string id, string itemName)
        {
            ItemDefinition itemDefinition = ScriptableObject.CreateInstance<ItemDefinition>();
            SerializedObject serializedDefinition = new SerializedObject(itemDefinition);
            serializedDefinition.FindProperty("id").stringValue = id;
            serializedDefinition.FindProperty("itemName").stringValue = itemName;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            return itemDefinition;
        }

        private static void SetDatabaseItems(ItemDatabase itemDatabase, params ItemDefinition[] items)
        {
            SerializedObject serializedDatabase = new SerializedObject(itemDatabase);
            SerializedProperty itemList = serializedDatabase.FindProperty("items");
            itemList.arraySize = items.Length;

            for (int i = 0; i < items.Length; i++)
            {
                itemList.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }

            serializedDatabase.ApplyModifiedPropertiesWithoutUndo();
            itemDatabase.Rebuild();
        }
    }
}
