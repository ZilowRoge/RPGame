using System;
using System.Collections.Generic;
using RPGame.Core.Effects;
using RPGame.Inventory.Data;
using RPGame.Inventory.Logic;
using UnityEngine;
using InventoryModel = RPGame.Inventory.Logic.Inventory;

namespace RPGame.Inventory
{
    public sealed class ItemManagementController : MonoBehaviour
    {
        [SerializeField] private int inventorySize = 20;
        [SerializeField] private List<ItemDefinition> startingItems = new();
        [SerializeField] private InventoryModel inventory = new();
        [SerializeField] private Equipment equipment = new();
        [SerializeField] private ConsumableSlots consumableSlots = new();
        [SerializeField] private EffectAggregator effectAggregator;

        private InventoryEquipmentService itemTransferService;
        private InventoryConsumableService consumableService;
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

        public event Action OnConsumableSlotsChanged
        {
            add
            {
                EnsureInitialized();
                consumableSlots.OnConsumableSlotsChanged += value;
            }
            remove
            {
                EnsureInitialized();
                consumableSlots.OnConsumableSlotsChanged -= value;
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

        public ConsumableSlots ConsumableSlots
        {
            get
            {
                EnsureInitialized();
                return consumableSlots;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public bool EquipFromInventory(int inventoryIndex)
        {
            EnsureInitialized();
            return itemTransferService.EquipFromInventory(inventoryIndex);
        }

        public bool UnequipToInventory(EquipmentSlotType slotType)
        {
            EnsureInitialized();
            return itemTransferService.UnequipToInventory(slotType);
        }

        public bool AddItem(ItemDefinition definition, int amount)
        {
            EnsureInitialized();
            return inventory.AddItem(definition, amount);
        }

        public bool AddItem(ItemInstance item)
        {
            EnsureInitialized();
            return inventory.AddItem(item);
        }

        public bool MoveItem(ItemSlotReference from, ItemSlotReference to)
        {
            EnsureInitialized();
            return itemTransferService.MoveItem(from, to);
        }

        public bool UseConsumableFromInventory(int inventoryIndex)
        {
            EnsureInitialized();
            return consumableService.UseFromInventory(inventoryIndex);
        }

        public bool UseConsumableSlot(ConsumableSlotType slotType)
        {
            EnsureInitialized();
            return consumableService.UseFromConsumableSlot(slotType);
        }

        public ItemInstance GetEquippedItem(EquipmentSlotType slotType)
        {
            EnsureInitialized();
            return equipment.GetSlot(slotType)?.Item;
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
            consumableSlots ??= new ConsumableSlots();
            consumableSlots.Initialize();
            effectAggregator ??= GetComponent<EffectAggregator>();
            itemTransferService = new InventoryEquipmentService(inventory, equipment, consumableSlots);
            consumableService = new InventoryConsumableService(inventory, consumableSlots, effectAggregator);
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
                    inventory.AddItem(definition, 1);
                }
            }
        }
    }
}
