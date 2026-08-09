using System;
using RPGame.Core.Spells;

namespace RPGame.Combat.Spells
{
    public sealed class LastUsedSpellTracker
    {
        public event Action<Spell> LastUsedSpellChanged;

        public Spell LastUsedSpell { get; private set; }

        public bool SetLastUsedSpell(Spell spell)
        {
            if (LastUsedSpell == spell)
            {
                return false;
            }

            LastUsedSpell = spell;
            LastUsedSpellChanged?.Invoke(LastUsedSpell);
            return true;
        }

        public void Clear()
        {
            SetLastUsedSpell(null);
        }
    }
}
