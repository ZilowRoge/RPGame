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

        public override IReadOnlyList<PartialDamage> RollDamage()
        {
            ItemManagementController controller = ResolveItemManagementController();
            ItemInstance item = controller != null
                ? controller.GetEquippedItem(EquipmentSlotType.MainHand)
                : null;
            ItemWeaponData weaponData = item?.Definition?.GetStatBlock<ItemWeaponData>();

            if (weaponData == null || weaponData.Damage == null || weaponData.Damage.Count == 0)
            {
                return Array.Empty<PartialDamage>();
            }

            List<PartialDamage> rolledDamage = new(weaponData.Damage.Count);
            for (int i = 0; i < weaponData.Damage.Count; i++)
            {
                PartialDamageRange damageRange = weaponData.Damage[i];
                int minDamage = Mathf.CeilToInt(damageRange.MinDamage);
                int maxDamage = Mathf.Max(minDamage, Mathf.FloorToInt(damageRange.MaxDamage));
                float amount = UnityEngine.Random.Range(minDamage, maxDamage + 1);

                rolledDamage.Add(new PartialDamage(
                    amount,
                    damageRange.DamageType,
                    damageRange.DamageElement));
            }

            return rolledDamage;
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
