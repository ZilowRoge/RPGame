using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    [CreateAssetMenu(fileName = "EarthZoneSpell", menuName = "RPGame/Spells/Earth Zone")]
    public sealed class EarthZoneSpell : ZoneSpell, IDurationCapability
    {
        public float Duration => ActiveDuration;
    }
}
