using UnityEngine;

namespace RPGame.Core.Inventory.Data
{
    [CreateAssetMenu(fileName = "ItemWeaponData", menuName = "RPGame/Inventory/Item Weapon Data")]
    public sealed class ItemWeaponData : ItemTypeData
    {
        public EquipmentSlotType EquipmentSlotType => EquipmentSlotType.MainHand;

        public override string GetTooltip()
        {
            return "Weapon";
        }
    }
}
