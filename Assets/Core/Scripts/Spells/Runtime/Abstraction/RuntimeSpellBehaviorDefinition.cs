using System;
using UnityEngine;

namespace RPGame.Core.Spells
{
    [Serializable]
    public abstract class RuntimeSpellBehaviorDefinition
    {
        public abstract bool TryCreate(
            Spell spell,
            GameObject casterObject,
            out IRuntimeSpellBehavior behavior);
    }
}
