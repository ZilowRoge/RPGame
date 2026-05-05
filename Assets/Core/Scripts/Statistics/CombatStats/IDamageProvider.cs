using System.Collections.Generic;
using RPGame.Core.Damage;

namespace RPGame.Core.Statistics.CombatStats
{
    public interface IDamageProvider
    {
        IReadOnlyList<PartialDamage> RollDamage();
    }
}
