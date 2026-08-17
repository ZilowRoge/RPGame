using System;
using System.Collections.Generic;
using RPGame.Core.Damage;
using RPGame.Core.Spells;
using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    public sealed class SpellCaster : MonoBehaviour, ILastUsedSpellDamageRangeProvider
    {
        [SerializeField] private StatisticsController statisticsController;
        [SerializeField] private bool logLastUsedSpellChanges = true;

        private readonly LastUsedSpellTracker lastUsedSpellTracker = new();
        private IReadOnlyList<PartialDamageRange> lastUsedSpellDamageRanges = Array.Empty<PartialDamageRange>();

        public event Action LastUsedSpellDamageRangeChanged;

        public IStatisticsController Statistics => ResolveStatisticsController();
        public LastUsedSpellTracker LastUsedSpellTracker => lastUsedSpellTracker;

        public bool TryCast(Spell spell, CasterData casterData)
        {
            if (spell == null)
            {
                Debug.LogWarning("SpellCaster.TryCast failed because spell is not assigned.", this);
                return false;
            }

            IStatisticsController statistics = ResolveStatisticsController();
            if (statistics != null && !statistics.TrySpendMana(spell.ManaCost))
            {
                return false;
            }

            spell.OnCast(casterData);
            CacheLastUsedSpellDamageRanges(spell, casterData);

            if (lastUsedSpellTracker.SetLastUsedSpell(spell))
            {
                LogLastUsedSpellChanged(spell);
                LastUsedSpellDamageRangeChanged?.Invoke();
            }

            return true;
        }

        public bool TryGetLastUsedSpellDamageRanges(out IReadOnlyList<PartialDamageRange> damageRanges)
        {
            damageRanges = lastUsedSpellDamageRanges;
            return damageRanges != null && damageRanges.Count > 0;
        }

        private void CacheLastUsedSpellDamageRanges(Spell spell, CasterData casterData)
        {
            if (spell is not ICasterDamageRangeProvider damageRangeProvider)
            {
                lastUsedSpellDamageRanges = Array.Empty<PartialDamageRange>();
                return;
            }

            lastUsedSpellDamageRanges = damageRangeProvider.GetDamageRanges(casterData) ?? Array.Empty<PartialDamageRange>();
        }

        private StatisticsController ResolveStatisticsController()
        {
            if (statisticsController == null)
            {
                statisticsController = GetComponentInParent<StatisticsController>();
            }

            return statisticsController;
        }

        private void LogLastUsedSpellChanged(Spell spell)
        {
            if (!logLastUsedSpellChanges)
            {
                return;
            }

            string spellName = spell != null ? spell.name : "None";
            Debug.Log($"Last used spell changed to {spellName}.", this);
        }
    }
}
