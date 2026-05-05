using System.Collections.Generic;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Core.Statistics.CombatStats
{
    public abstract class CombatStatsProvider : MonoBehaviour, IDamageProvider
    {
        public abstract IReadOnlyList<PartialDamage> RollDamage();
    }
}
