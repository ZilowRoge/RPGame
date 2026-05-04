using UnityEngine;

namespace RPGame.Core.Inventory.Data
{
    public abstract class ItemTypeData : ScriptableObject
    {
        public abstract string GetTooltip();
    }
}
