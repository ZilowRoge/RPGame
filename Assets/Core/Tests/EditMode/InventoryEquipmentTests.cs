using NUnit.Framework;
using RPGame.Core.Inventory.Data;
using RPGame.Core.Inventory.Logic;
using UnityEditor;
using UnityEngine;
using InventoryModel = RPGame.Core.Inventory.Logic.Inventory;

namespace RPGame.Core.Tests
{
    public sealed class InventoryEquipmentTests
    {
        private ItemDefinition weaponDefinition;
        private ItemDefinition stackableDefinition;
        private ItemDefinition equipmentWithoutSlotDefinition;
        private ItemWeaponData weaponData;

        [SetUp]
        public void SetUp()
        {
            weaponData = ScriptableObject.CreateInstance<ItemWeaponData>();
            weaponDefinition = CreateItemDefinition("weapon_sword", "Sword", weaponData);
            stackableDefinition = CreateItemDefinition("consumable_potion", "Potion", null, 5);
            equipmentWithoutSlotDefinition = CreateItemDefinition("equipment_ring", "Ring", null);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(weaponDefinition);
            Object.DestroyImmediate(stackableDefinition);
            Object.DestroyImmediate(equipmentWithoutSlotDefinition);
            Object.DestroyImmediate(weaponData);
        }

        [Test]
        public void EquipFromInventory_WhenItemFits_MovesItemToEquipment()
        {
            InventoryModel inventory = new InventoryModel(2);
            Equipment equipment = new Equipment();
            InventoryEquipmentService service = new InventoryEquipmentService(inventory, equipment);
            ItemInstance item = new ItemInstance(weaponDefinition);

            inventory.AddItem(item);

            bool equipped = service.EquipFromInventory(0);

            Assert.IsTrue(equipped);
            Assert.IsFalse(inventory.GetSlot(0).HasItem);
            Assert.AreSame(item, equipment.GetSlot(EquipmentSlotType.MainHand).Item);
        }

        [Test]
        public void EquipFromInventory_WhenEquipmentSlotIsOccupied_SwapsItems()
        {
            InventoryModel inventory = new InventoryModel(1);
            Equipment equipment = new Equipment();
            InventoryEquipmentService service = new InventoryEquipmentService(inventory, equipment);
            ItemInstance inventoryItem = new ItemInstance(weaponDefinition);
            ItemInstance equippedItem = new ItemInstance(weaponDefinition);

            inventory.AddItem(inventoryItem);
            equipment.SetItem(EquipmentSlotType.MainHand, equippedItem);

            bool equipped = service.EquipFromInventory(0);

            Assert.IsTrue(equipped);
            Assert.AreSame(equippedItem, inventory.GetSlot(0).Item);
            Assert.AreSame(inventoryItem, equipment.GetSlot(EquipmentSlotType.MainHand).Item);
        }

        [Test]
        public void EquipFromInventory_WhenItemDoesNotFit_DoesNotRemoveFromInventory()
        {
            InventoryModel inventory = new InventoryModel(2);
            Equipment equipment = new Equipment();
            InventoryEquipmentService service = new InventoryEquipmentService(inventory, equipment);
            ItemInstance item = new ItemInstance(equipmentWithoutSlotDefinition);

            inventory.AddItem(item);

            bool equipped = service.EquipFromInventory(0);

            Assert.IsFalse(equipped);
            Assert.AreSame(item, inventory.GetSlot(0).Item);
            Assert.IsFalse(equipment.GetSlot(EquipmentSlotType.MainHand).HasItem);
        }

        [Test]
        public void UnequipToInventory_WhenInventoryHasSpace_MovesItemBackToInventory()
        {
            InventoryModel inventory = new InventoryModel(1);
            Equipment equipment = new Equipment();
            InventoryEquipmentService service = new InventoryEquipmentService(inventory, equipment);
            ItemInstance item = new ItemInstance(weaponDefinition);

            equipment.Equip(item);

            bool unequipped = service.UnequipToInventory(EquipmentSlotType.MainHand);

            Assert.IsTrue(unequipped);
            Assert.AreSame(item, inventory.GetSlot(0).Item);
            Assert.IsFalse(equipment.GetSlot(EquipmentSlotType.MainHand).HasItem);
        }

        [Test]
        public void UnequipToInventory_WhenInventoryIsFull_KeepsItemEquipped()
        {
            InventoryModel inventory = new InventoryModel(1);
            Equipment equipment = new Equipment();
            InventoryEquipmentService service = new InventoryEquipmentService(inventory, equipment);
            ItemInstance equippedItem = new ItemInstance(weaponDefinition);
            ItemInstance inventoryItem = new ItemInstance(weaponDefinition);

            inventory.AddItem(inventoryItem);
            equipment.Equip(equippedItem);

            bool unequipped = service.UnequipToInventory(EquipmentSlotType.MainHand);

            Assert.IsFalse(unequipped);
            Assert.AreSame(inventoryItem, inventory.GetSlot(0).Item);
            Assert.AreSame(equippedItem, equipment.GetSlot(EquipmentSlotType.MainHand).Item);
        }

        [Test]
        public void AddItem_WhenInventoryIsFull_ReturnsFalse()
        {
            InventoryModel inventory = new InventoryModel(1);
            ItemInstance firstItem = new ItemInstance(weaponDefinition);
            ItemInstance secondItem = new ItemInstance(weaponDefinition);

            bool firstAdded = inventory.AddItem(firstItem);
            bool secondAdded = inventory.AddItem(secondItem);

            Assert.IsTrue(firstAdded);
            Assert.IsFalse(secondAdded);
            Assert.AreSame(firstItem, inventory.GetSlot(0).Item);
        }

        [Test]
        public void AddItem_WhenItemIsStackable_FillsExistingStack()
        {
            InventoryModel inventory = new InventoryModel(2);
            ItemInstance firstStack = new ItemInstance(stackableDefinition, 2);
            ItemInstance secondStack = new ItemInstance(stackableDefinition, 3);

            inventory.AddItem(firstStack, 2);
            bool added = inventory.AddItem(secondStack, 3);

            Assert.IsTrue(added);
            Assert.AreEqual(5, inventory.GetSlot(0).Item.StackSize);
            Assert.IsFalse(inventory.GetSlot(1).HasItem);
        }

        [Test]
        public void MoveItem_WhenTargetSlotIsEmpty_MovesItem()
        {
            InventoryModel inventory = new InventoryModel(2);
            ItemInstance item = new ItemInstance(weaponDefinition);

            inventory.AddItem(item);
            bool moved = inventory.MoveItem(0, 1);

            Assert.IsTrue(moved);
            Assert.IsFalse(inventory.GetSlot(0).HasItem);
            Assert.AreSame(item, inventory.GetSlot(1).Item);
        }

        [Test]
        public void MoveItem_WhenTargetStackHasSpace_StacksItem()
        {
            InventoryModel inventory = new InventoryModel(2);
            ItemInstance firstStack = new ItemInstance(stackableDefinition, 2);
            ItemInstance secondStack = new ItemInstance(stackableDefinition, 2);

            inventory.AddItem(firstStack, 2);
            inventory.GetSlot(1).SetItem(secondStack);

            bool moved = inventory.MoveItem(1, 0, 2);

            Assert.IsTrue(moved);
            Assert.AreEqual(4, inventory.GetSlot(0).Item.StackSize);
            Assert.IsFalse(inventory.GetSlot(1).HasItem);
        }

        [Test]
        public void MoveItem_WhenTargetStackIsFull_ReturnsFalse()
        {
            InventoryModel inventory = new InventoryModel(2);
            ItemInstance fullStack = new ItemInstance(stackableDefinition, 5);
            ItemInstance sourceStack = new ItemInstance(stackableDefinition, 1);

            inventory.AddItem(fullStack, 5);
            inventory.GetSlot(1).SetItem(sourceStack);

            bool moved = inventory.MoveItem(1, 0);

            Assert.IsFalse(moved);
            Assert.AreSame(sourceStack, inventory.GetSlot(1).Item);
            Assert.AreEqual(5, inventory.GetSlot(0).Item.StackSize);
        }

        [Test]
        public void MoveItem_WhenInventoryTargetIsOccupied_SwapsItems()
        {
            InventoryModel inventory = new InventoryModel(2);
            Equipment equipment = new Equipment();
            InventoryEquipmentService service = new InventoryEquipmentService(inventory, equipment);
            ItemInstance firstItem = new ItemInstance(weaponDefinition);
            ItemInstance secondItem = new ItemInstance(weaponDefinition);

            inventory.GetSlot(0).SetItem(firstItem);
            inventory.GetSlot(1).SetItem(secondItem);

            bool moved = service.MoveItem(ItemSlotReference.Inventory(0), ItemSlotReference.Inventory(1));

            Assert.IsTrue(moved);
            Assert.AreSame(secondItem, inventory.GetSlot(0).Item);
            Assert.AreSame(firstItem, inventory.GetSlot(1).Item);
        }

        [Test]
        public void MoveItem_WhenInventoryItemIsDroppedOnOccupiedEquipment_SwapsItems()
        {
            InventoryModel inventory = new InventoryModel(1);
            Equipment equipment = new Equipment();
            InventoryEquipmentService service = new InventoryEquipmentService(inventory, equipment);
            ItemInstance inventoryItem = new ItemInstance(weaponDefinition);
            ItemInstance equippedItem = new ItemInstance(weaponDefinition);

            inventory.GetSlot(0).SetItem(inventoryItem);
            equipment.SetItem(EquipmentSlotType.MainHand, equippedItem);

            bool moved = service.MoveItem(
                ItemSlotReference.Inventory(0),
                ItemSlotReference.Equipment(EquipmentSlotType.MainHand));

            Assert.IsTrue(moved);
            Assert.AreSame(equippedItem, inventory.GetSlot(0).Item);
            Assert.AreSame(inventoryItem, equipment.GetSlot(EquipmentSlotType.MainHand).Item);
        }

        [Test]
        public void MoveItem_WhenEquipmentItemIsDroppedOnOccupiedInventory_SwapsItems()
        {
            InventoryModel inventory = new InventoryModel(1);
            Equipment equipment = new Equipment();
            InventoryEquipmentService service = new InventoryEquipmentService(inventory, equipment);
            ItemInstance inventoryItem = new ItemInstance(weaponDefinition);
            ItemInstance equippedItem = new ItemInstance(weaponDefinition);

            inventory.GetSlot(0).SetItem(inventoryItem);
            equipment.SetItem(EquipmentSlotType.MainHand, equippedItem);

            bool moved = service.MoveItem(
                ItemSlotReference.Equipment(EquipmentSlotType.MainHand),
                ItemSlotReference.Inventory(0));

            Assert.IsTrue(moved);
            Assert.AreSame(equippedItem, inventory.GetSlot(0).Item);
            Assert.AreSame(inventoryItem, equipment.GetSlot(EquipmentSlotType.MainHand).Item);
        }

        [Test]
        public void MoveItem_WhenOccupiedInventoryItemCannotEquipToSourceSlot_ReturnsFalse()
        {
            InventoryModel inventory = new InventoryModel(1);
            Equipment equipment = new Equipment();
            InventoryEquipmentService service = new InventoryEquipmentService(inventory, equipment);
            ItemInstance inventoryItem = new ItemInstance(equipmentWithoutSlotDefinition);
            ItemInstance equippedItem = new ItemInstance(weaponDefinition);

            inventory.GetSlot(0).SetItem(inventoryItem);
            equipment.SetItem(EquipmentSlotType.MainHand, equippedItem);

            bool moved = service.MoveItem(
                ItemSlotReference.Equipment(EquipmentSlotType.MainHand),
                ItemSlotReference.Inventory(0));

            Assert.IsFalse(moved);
            Assert.AreSame(inventoryItem, inventory.GetSlot(0).Item);
            Assert.AreSame(equippedItem, equipment.GetSlot(EquipmentSlotType.MainHand).Item);
        }

        private static ItemDefinition CreateItemDefinition(
            string id,
            string itemName,
            ItemTypeData itemTypeData,
            int maxStack = 1)
        {
            ItemDefinition itemDefinition = ScriptableObject.CreateInstance<ItemDefinition>();
            SerializedObject serializedDefinition = new SerializedObject(itemDefinition);
            serializedDefinition.FindProperty("id").stringValue = id;
            serializedDefinition.FindProperty("itemName").stringValue = itemName;
            serializedDefinition.FindProperty("itemType").enumValueIndex = (int)ItemType.Equipment;
            serializedDefinition.FindProperty("maxStack").intValue = maxStack;

            SerializedProperty statBlocks = serializedDefinition.FindProperty("itemTypeData");
            statBlocks.arraySize = itemTypeData != null ? 1 : 0;
            if (itemTypeData != null)
            {
                statBlocks.GetArrayElementAtIndex(0).objectReferenceValue = itemTypeData;
            }

            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            return itemDefinition;
        }
    }
}
