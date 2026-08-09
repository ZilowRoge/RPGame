using System;
using System.Collections.Generic;
using RPGame.Core.Damage;

namespace RPGame.Core.Spells
{
    public interface ILastUsedSpellDamageRangeProvider
    {
        event Action LastUsedSpellDamageRangeChanged;

        bool TryGetLastUsedSpellDamageRanges(out IReadOnlyList<PartialDamageRange> damageRanges);
    }
}
