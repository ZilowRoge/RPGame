using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    [CreateAssetMenu(fileName = "EarthZoneSpell", menuName = "RPGame/Spells/Earth Zone")]
    public sealed class EarthZoneSpell : ZoneSpell
    {
        public override SpellTags Tags => SpellTags.AoE | SpellTags.Duration | SpellTags.Control;
    }
}
