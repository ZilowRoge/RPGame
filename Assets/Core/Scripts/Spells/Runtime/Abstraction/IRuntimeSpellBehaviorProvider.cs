using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Core.Spells
{
    public interface IRuntimeSpellBehaviorProvider
    {
        IReadOnlyList<IRuntimeSpellBehavior> CreateRuntimeBehaviors(
            Spell spell,
            GameObject casterObject);
    }
}
