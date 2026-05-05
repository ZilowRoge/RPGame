using UnityEngine;

namespace RPGame.Core.Inventory.Data
{
    [CreateAssetMenu(fileName = "ItemWeaponData", menuName = "RPGame/Inventory/Item Weapon Data")]
    public sealed class ItemWeaponData : ItemTypeData
    {
        [SerializeField] private float minDamage = 1f;
        [SerializeField] private float maxDamage = 1f;

        public EquipmentSlotType EquipmentSlotType => EquipmentSlotType.MainHand;
        public float MinDamage => Mathf.Max(0f, minDamage);
        public float MaxDamage => Mathf.Max(MinDamage, maxDamage);

        public override string GetTooltip()
        {
            return $"Weapon\nDamage: {MinDamage:0.#}-{MaxDamage:0.#}";
        }

        private void OnValidate()
        {
            minDamage = Mathf.Max(0f, minDamage);
            maxDamage = Mathf.Max(minDamage, maxDamage);
        }
    }
}
