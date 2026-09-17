using UnityEngine;

namespace RPGame.Core.Spells
{
    public abstract class RuntimeSpellBehaviorDefinition : ScriptableObject
    {
        public abstract bool TryCreate(
            Spell spell,
            GameObject casterObject,
            out IRuntimeSpellBehavior behavior);
    }
}
