using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Core.Effects
{
    public interface IRuntimeSpellBehaviorFactory
    {
        bool TryCreateRuntimeBehavior(
            Spell spell,
            GameObject casterObject,
            out IRuntimeSpellBehavior behavior);
    }
}
