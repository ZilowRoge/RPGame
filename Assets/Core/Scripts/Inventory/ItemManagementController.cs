using System;
using System.Collections.Generic;
using RPGame.Core.Inventory.Data;
using RPGame.Core.Inventory.Logic;
using UnityEngine;
using InventoryModel = RPGame.Core.Inventory.Logic.Inventory;

namespace RPGame.Core.Inventory
{
    public sealed class ItemManagementController : MonoBehaviour
    {
        [SerializeField] private int inventorySize = 20;
        [SerializeField] private List<ItemDefinition> startingItems = new();
        [SerializeField] private InventoryModel inventory = new();
        [SerializeField] private Equipment equipment = new();

        private InventoryEquipmentService service;
        private bool initialized;

        public event Action OnInventoryChanged
        {
            add
            {
                EnsureInitialized();
                inventory.OnInventoryChanged += value;
            }
            remove
            {
                EnsureInitialized();
                inventory.OnInventoryChanged -= value;
            }
        }

        public event Action OnEquipmentChanged
        {
            add
            {
                EnsureInitialized();
                equipment.OnEquipmentChanged += value;
            }
            remove
            {
                EnsureInitialized();
                equipment.OnEquipmentChanged -= value;
            }
        }

        public InventoryModel Inventory
        {
            get
            {
                EnsureInitialized();
                return inventory;
            }
        }

        public Equipment Equipment
        {
            get
            {
                EnsureInitialized();
                return equipment;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public bool EquipFromInventory(int inventoryIndex)
        {
            EnsureInitialized();
            return service.EquipFromInventory(inventoryIndex);
        }

        public bool UnequipToInventory(EquipmentSlotType slotType)
        {
            EnsureInitialized();
            return service.UnequipToInventory(slotType);
        }

        public bool AddItem(ItemInstance item, int amount = 1)
        {
            EnsureInitialized();
            return inventory.AddItem(item, amount);
        }

        private void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            inventory ??= new InventoryModel();
            inventory.Initialize(inventorySize);
            equipment ??= new Equipment();
            equipment.Initialize();
            service = new InventoryEquipmentService(inventory, equipment);
            AddStartingItems();
            initialized = true;
        }

        private void AddStartingItems()
        {
            for (int i = 0; i < startingItems.Count; i++)
            {
                ItemDefinition definition = startingItems[i];
                if (definition != null)
                {
                    inventory.AddItem(new ItemInstance(definition), 1);
                }
            }
        }
    }
}
