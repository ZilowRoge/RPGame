using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Loot
{
    [CreateAssetMenu(fileName = "LootTable", menuName = "RPGame/Loot/Loot Table")]
    public sealed class LootTable : ScriptableObject
    {
        [SerializeField] private List<LootEntry> entries = new();

        public IReadOnlyList<LootEntry> Entries => entries;

        private void OnValidate()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i]?.ValidateAmounts();
            }
        }
    }
}
