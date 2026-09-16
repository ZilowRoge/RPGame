using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    [CreateAssetMenu(fileName = "FireZoneSpell", menuName = "RPGame/Spells/Fire Zone")]
    public sealed class FireZoneSpell : ZoneSpell
    {
        public override SpellTags Tags => SpellTags.AoE | SpellTags.Duration | SpellTags.DamageOverTime;
    }
}
