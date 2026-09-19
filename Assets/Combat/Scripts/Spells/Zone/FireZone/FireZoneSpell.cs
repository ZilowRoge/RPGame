using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    [CreateAssetMenu(fileName = "FireZoneSpell", menuName = "RPGame/Spells/Fire Zone")]
    public sealed class FireZoneSpell : ZoneSpell, IDurationCapability
    {
        public float Duration => ActiveDuration;
    }
}
