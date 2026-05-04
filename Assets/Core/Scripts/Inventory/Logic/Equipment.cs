using System;
using System.Collections.Generic;
using RPGame.Core.Inventory.Data;
using UnityEngine;

namespace RPGame.Core.Inventory.Logic
{
    [Serializable]
    public sealed class Equipment
    {
        [SerializeField] private List<EquipmentSlot> slots = new();

        public event Action OnEquipmentChanged;

        public IReadOnlyList<EquipmentSlot> Slots => slots;

        public Equipment()
        {
            Initialize();
        }

        public void Initialize()
        {
            slots ??= new List<EquipmentSlot>();

            foreach (EquipmentSlotType slotType in Enum.GetValues(typeof(EquipmentSlotType)))
            {
                if (GetSlot(slotType) == null)
                {
                    slots.Add(new EquipmentSlot(slotType));
                }
            }
        }

        public bool Equip(ItemInstance item)
        {
            if (!CanEquip(item) || !TryGetEquipmentSlotType(item, out EquipmentSlotType slotType))
            {
                return false;
            }

            EquipmentSlot slot = GetSlot(slotType);
            if (slot.HasItem)
            {
                return false;
            }

            slot.SetItem(item);
            OnEquipmentChanged?.Invoke();
            return true;
        }

        public ItemInstance Unequip(EquipmentSlotType slotType)
        {
            EquipmentSlot slot = GetSlot(slotType);
            if (slot == null || !slot.HasItem)
            {
                return null;
            }

            ItemInstance item = slot.Clear();
            OnEquipmentChanged?.Invoke();
            return item;
        }

        public bool CanEquip(ItemInstance item)
        {
            return item != null
                && item.Definition != null
                && item.Definition.ItemType == ItemType.Equipment
                && TryGetEquipmentSlotType(item, out EquipmentSlotType slotType)
                && GetSlot(slotType) != null;
        }

        public EquipmentSlot GetSlot(EquipmentSlotType slotType)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].SlotType == slotType)
                {
                    return slots[i];
                }
            }

            return null;
        }

        private static bool TryGetEquipmentSlotType(ItemInstance item, out EquipmentSlotType slotType)
        {
            slotType = default;
            ItemWeaponData weaponData = item?.Definition?.GetStatBlock<ItemWeaponData>();
            if (weaponData == null)
            {
                return false;
            }

            slotType = weaponData.EquipmentSlotType;
            return true;
        }
    }
}
