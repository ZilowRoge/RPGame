using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    [CreateAssetMenu(fileName = "LightningZoneSpell", menuName = "RPGame/Spells/Lightning Zone")]
    public sealed class LightningZoneSpell : ZoneSpell
    {
        public override SpellTags Tags => SpellTags.AoE | SpellTags.Control;
    }
}
