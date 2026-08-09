using System;
using System.Collections.Generic;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Core.Statistics.CombatStats
{
    public abstract class CombatStatsProvider : MonoBehaviour
    {
        public event Action Changed;

        public virtual IReadOnlyList<PartialDamageRange> GetDamageRanges()
        {
            return Array.Empty<PartialDamageRange>();
        }

        protected void NotifyChanged()
        {
            Changed?.Invoke();
        }
    }
}
