using UnityEngine;

namespace RPGame.Inventory.Data
{
    public abstract class ItemTypeData : ScriptableObject
    {
        public abstract string GetTooltip();
    }
}
