using System.Collections.Generic;
using RPGame.Inventory;
using UnityEngine;

namespace RPGame.Loot
{
    public sealed class LootDropper : MonoBehaviour
    {
        [SerializeField] private LootTable lootTable;
        [SerializeField] private ItemPickup pickupPrefab;
        [SerializeField] private Transform dropOrigin;
        [SerializeField, Min(0f)] private float dropRadius = 0.5f;
        [SerializeField] private bool logDrops = false;

        private readonly LootRoller lootRoller = new();
        private bool hasDropped;

        public void DropLoot()
        {
            if (hasDropped)
            {
                return;
            }

            if (lootTable == null || pickupPrefab == null)
            {
                Debug.LogWarning($"{nameof(LootDropper)} requires a loot table and pickup prefab.", this);
                return;
            }

            List<LootResult> results = lootRoller.Roll(lootTable);
            hasDropped = true;

            if (results.Count == 0)
            {
                LogDrop("No loot dropped.");
                return;
            }

            for (int i = 0; i < results.Count; i++)
            {
                LootResult result = results[i];
                if (result.Item == null || result.Amount <= 0)
                {
                    continue;
                }

                ItemPickup pickup = Instantiate(
                    pickupPrefab,
                    GetDropPosition(i, results.Count),
                    Quaternion.identity);
                pickup.Initialize(result.Item, result.Amount);
                LogDrop($"Dropped {result.Item.Name} x{result.Amount}.");
            }
        }

        private void LogDrop(string message)
        {
            if (logDrops)
            {
                Debug.Log($"{nameof(LootDropper)}: {message}", this);
            }
        }

        private Vector3 GetDropPosition(int index, int totalCount)
        {
            Vector3 origin = dropOrigin != null ? dropOrigin.position : transform.position;
            if (totalCount <= 1 || dropRadius <= 0f)
            {
                return origin;
            }

            float angle = Mathf.PI * 2f * index / totalCount;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * dropRadius;
            return origin + offset;
        }

        private void OnValidate()
        {
            dropRadius = Mathf.Max(0f, dropRadius);
        }
    }
}
