using System.Collections.Generic;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Inventory.Data
{
    [CreateAssetMenu(fileName = "ItemWeaponData", menuName = "RPGame/Inventory/Item Weapon Data")]
    public sealed class ItemWeaponData : ItemTypeData
    {
        [SerializeField] private List<PartialDamageRange> damage = new()
        {
            new PartialDamageRange(1f, 1f, DamageType.Physical, DamageElement.None)
        };

        public EquipmentSlotType EquipmentSlotType => EquipmentSlotType.MainHand;
        public IReadOnlyList<PartialDamageRange> Damage => damage;

        public override string GetTooltip()
        {
            if (damage == null || damage.Count == 0)
            {
                return "Weapon";
            }

            return $"Weapon\n{BuildDamageTooltip()}";
        }

        private string BuildDamageTooltip()
        {
            string tooltip = string.Empty;

            for (int i = 0; i < damage.Count; i++)
            {
                PartialDamageRange damageRange = damage[i];
                if (i > 0)
                {
                    tooltip += "\n";
                }

                tooltip += $"Damage: {damageRange.MinDamage:0.#}-{damageRange.MaxDamage:0.#} {damageRange.DamageType}";
                if (damageRange.DamageElement != DamageElement.None)
                {
                    tooltip += $" {damageRange.DamageElement}";
                }
            }

            return tooltip;
        }
    }
}
