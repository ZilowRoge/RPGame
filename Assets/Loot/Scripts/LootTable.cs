using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Loot
{
    [CreateAssetMenu(fileName = "LootTable", menuName = "RPGame/Loot/Loot Table")]
    public sealed class LootTable : ScriptableObject
    {
        [SerializeField] private List<IndependentLootGroup> independentGroups = new();
        [SerializeField] private List<WeightedLootGroup> weightedGroups = new();

        public IReadOnlyList<IndependentLootGroup> IndependentGroups => independentGroups;
        public IReadOnlyList<WeightedLootGroup> WeightedGroups => weightedGroups;

        private void OnValidate()
        {
            for (int i = 0; i < independentGroups.Count; i++)
            {
                independentGroups[i]?.Validate();
            }

            for (int i = 0; i < weightedGroups.Count; i++)
            {
                weightedGroups[i]?.Validate();
            }
        }
    }
}
