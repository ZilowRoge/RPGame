using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Core.Inventory.Data
{
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "RPGame/Inventory/Item Definition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string itemName;
        [SerializeField] private Sprite icon;
        [SerializeField] private ItemType itemType = ItemType.Equipment;
        [SerializeField] private int maxStack = 1;
        [SerializeField] private List<ItemTypeData> itemTypeData = new();

        public string Id => id;
        public string Name => itemName;
        public Sprite Icon => icon;
        public ItemType ItemType => itemType;
        public int MaxStack => Mathf.Max(1, maxStack);
        public IReadOnlyList<ItemTypeData> ItemTypeData => itemTypeData;

        public T GetStatBlock<T>() where T : ItemTypeData
        {
            for (int i = 0; i < itemTypeData.Count; i++)
            {
                if (itemTypeData[i] is T statBlock)
                {
                    return statBlock;
                }
            }

            return null;
        }
    }
}
