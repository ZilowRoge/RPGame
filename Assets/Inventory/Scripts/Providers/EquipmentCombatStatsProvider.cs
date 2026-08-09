using System;
using System.Collections.Generic;
using RPGame.Core.Damage;
using RPGame.Core.Statistics.CombatStats;
using RPGame.Inventory.Data;
using RPGame.Inventory.Logic;
using UnityEngine;

namespace RPGame.Inventory.Providers
{
    public sealed class EquipmentCombatStatsProvider : CombatStatsProvider
    {
        [SerializeField] private ItemManagementController itemManagementController;

        private void OnEnable()
        {
            SubscribeEquipmentChanges();
        }

        private void OnDisable()
        {
            UnsubscribeEquipmentChanges();
        }

        public override IReadOnlyList<PartialDamageRange> GetDamageRanges()
        {
            ItemWeaponData weaponData = GetEquippedWeaponData();
            return weaponData != null && weaponData.Damage != null
                ? weaponData.Damage
                : Array.Empty<PartialDamageRange>();
        }

        private ItemWeaponData GetEquippedWeaponData()
        {
            ItemManagementController controller = ResolveItemManagementController();
            ItemInstance item = controller != null
                ? controller.GetEquippedItem(EquipmentSlotType.MainHand)
                : null;

            return item?.Definition?.GetStatBlock<ItemWeaponData>();
        }

        private void SubscribeEquipmentChanges()
        {
            ItemManagementController controller = ResolveItemManagementController();
            if (controller != null)
            {
                controller.OnEquipmentChanged -= OnEquipmentChanged;
                controller.OnEquipmentChanged += OnEquipmentChanged;
            }
        }

        private void UnsubscribeEquipmentChanges()
        {
            if (itemManagementController != null)
            {
                itemManagementController.OnEquipmentChanged -= OnEquipmentChanged;
            }
        }

        private void OnEquipmentChanged()
        {
            NotifyChanged();
        }

        private ItemManagementController ResolveItemManagementController()
        {
            if (itemManagementController == null)
            {
                itemManagementController = GetComponentInParent<ItemManagementController>();
            }

            return itemManagementController;
        }
    }
}
