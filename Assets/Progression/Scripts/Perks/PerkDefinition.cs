using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Progression
{
    [CreateAssetMenu(fileName = "PerkDefinition", menuName = "RPGame/Progression/Perk Definition")]
    public sealed class PerkDefinition : ScriptableObject
    {
        [SerializeField] private string perkId = "Perk";
        [SerializeField] private string displayName = "Perk";
        [TextArea]
        [SerializeField] private string description;
        [SerializeField] private List<string> connectedPerkIds = new();
        [SerializeField] private int cost = 1;
        [SerializeField] private bool isStartingPerk;

        public string PerkId => perkId;
        public string DisplayName => displayName;
        public string Description => description;
        public IReadOnlyList<string> ConnectedPerkIds => connectedPerkIds;
        public int Cost => cost;
        public bool IsStartingPerk => isStartingPerk;

        private void OnValidate()
        {
            perkId = string.IsNullOrWhiteSpace(perkId) ? name : perkId.Trim();
            displayName = string.IsNullOrWhiteSpace(displayName) ? perkId : displayName.Trim();
            cost = Mathf.Max(1, cost);

            for (int i = 0; i < connectedPerkIds.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(connectedPerkIds[i]))
                {
                    connectedPerkIds[i] = connectedPerkIds[i].Trim();
                }
            }
        }
    }
}
