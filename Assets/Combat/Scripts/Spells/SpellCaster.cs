using RPGame.Core.Spells;

namespace RPGame.Combat.Spells
{
    public sealed class SpellCaster
    {
        public bool TryCast(Spell spell, CasterData casterData)
        {
            if (spell == null)
            {
                return false;
            }

            if (spell.ManaCost > 0f && casterData.Statistics == null)
            {
                return false;
            }

            if (casterData.Statistics != null && !casterData.Statistics.TrySpendMana(spell.ManaCost))
            {
                return false;
            }

            spell.OnCast(casterData);
            return true;
        }
    }
}
