using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Core.Inventory.Data
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "RPGame/Inventory/Item Database")]
    public sealed class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemDefinition> items = new();

        private Dictionary<string, ItemDefinition> itemsById;

        public IReadOnlyList<ItemDefinition> Items => items;

        public bool TryGetItem(string id, out ItemDefinition item)
        {
            EnsureLookup();
            if (string.IsNullOrWhiteSpace(id))
            {
                item = null;
                return false;
            }

            return itemsById.TryGetValue(id, out item);
        }

        public ItemDefinition GetItem(string id)
        {
            return TryGetItem(id, out ItemDefinition item) ? item : null;
        }

        public bool Contains(string id)
        {
            return TryGetItem(id, out _);
        }

        public void Rebuild()
        {
            itemsById = new Dictionary<string, ItemDefinition>();

            for (int i = 0; i < items.Count; i++)
            {
                ItemDefinition item = items[i];
                if (item == null || string.IsNullOrWhiteSpace(item.Id))
                {
                    continue;
                }

                if (!itemsById.ContainsKey(item.Id))
                {
                    itemsById.Add(item.Id, item);
                }
            }
        }

        private void EnsureLookup()
        {
            if (itemsById == null)
            {
                Rebuild();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            itemsById = null;
            ValidateItems();
        }

        private void ValidateItems()
        {
            HashSet<string> usedIds = new();

            for (int i = 0; i < items.Count; i++)
            {
                ItemDefinition item = items[i];
                if (item == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    Debug.LogWarning($"Item database contains item without id: {item.name}.", this);
                    continue;
                }

                if (!usedIds.Add(item.Id))
                {
                    Debug.LogWarning($"Item database contains duplicate item id: {item.Id}.", this);
                }
            }
        }
#endif
    }
}
