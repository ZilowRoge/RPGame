using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    [CreateAssetMenu(fileName = "FireZoneSpell", menuName = "RPGame/Spells/Fire Zone")]
    public sealed class FireZoneSpell : ZoneSpell, IDurationCapability
    {
        public override SpellTags Tags => SpellTags.AoE | SpellTags.Duration | SpellTags.DamageOverTime;
        public float Duration => ActiveDuration;
    }
}
