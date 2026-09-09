using System.Collections.Generic;
using RPGame.Core.Damage;

namespace RPGame.Core.Spells
{
    public interface ICasterDamageRangeProvider
    {
        IReadOnlyList<PartialDamageRange> GetDamageRanges(CasterData casterData);
    }
}
