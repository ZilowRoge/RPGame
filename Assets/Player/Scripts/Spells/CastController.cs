using System;
using System.Collections.Generic;
using RPGame.Combat.Spells;
using RPGame.Core.Damage;
using RPGame.Core.Spells;
using RPGame.Core.Statistics;
using RPGame.Core.Statistics.Attributes;
using RPGame.Core.Targeting;
using TargetingController = RPGame.Player.Targeting.TargetingController;
using UnityEngine;

namespace RPGame.Player.Spells
{
    public sealed class CastController : MonoBehaviour, ILastUsedSpellDamageRangeProvider
    {
        [SerializeField] private SpellSymbolCaster spellSymbolCaster;
        [SerializeField] private TargetingController targeting;
        [SerializeField] private StatisticsController statisticsController;
        [SerializeField] private GameObject casterObject;
        [SerializeField] private Transform castOrigin;
        [SerializeField] private CharacterAttributes characterAttributes;

        private readonly SpellCaster spellCaster = new();
        private readonly LastUsedSpellTracker lastUsedSpellTracker = new();
        private IReadOnlyList<PartialDamageRange> lastUsedSpellDamageRanges = Array.Empty<PartialDamageRange>();

        public event Action LastUsedSpellDamageRangeChanged;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (spellSymbolCaster != null)
            {
                spellSymbolCaster.SpellSelected += OnSpellSelected;
            }
        }

        private void OnDisable()
        {
            if (spellSymbolCaster != null)
            {
                spellSymbolCaster.SpellSelected -= OnSpellSelected;
            }
        }

        internal void CastSpell(Spell spell)
        {
            CasterData casterData = CreateCasterData();
            bool wasCast = spellCaster.TryCast(spell, casterData);

            if (wasCast)
            {
                UpdateLastUsedSpell(spell, casterData);
            }
        }

        internal CasterData CreateCasterData()
        {
            ITargetable currentTarget = targeting != null ? targeting.CurrentTarget : null;
            Transform target = currentTarget != null ? currentTarget.TargetPoint : null;

            return new CasterDataBuilder(ResolveCasterObject(), castOrigin, target)
                .WithAttributes(ResolveCharacterAttributes())
                .WithStatistics(ResolveStatisticsController())
                .Build();
        }

        public bool TryGetLastUsedSpellDamageRanges(out IReadOnlyList<PartialDamageRange> damageRanges)
        {
            damageRanges = lastUsedSpellDamageRanges;
            return damageRanges != null && damageRanges.Count > 0;
        }

        private void OnSpellSelected(Spell spell)
        {
            CastSpell(spell);
        }

        private void UpdateLastUsedSpell(Spell spell, CasterData casterData)
        {
            CacheLastUsedSpellDamageRanges(spell, casterData);

            if (lastUsedSpellTracker.SetLastUsedSpell(spell))
            {
                LastUsedSpellDamageRangeChanged?.Invoke();
            }
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

        private GameObject ResolveCasterObject()
        {
            return casterObject != null ? casterObject : gameObject;
        }

        private CharacterAttributes ResolveCharacterAttributes()
        {
            if (characterAttributes == null)
            {
                TryGetComponent(out characterAttributes);
            }

            return characterAttributes;
        }

        private StatisticsController ResolveStatisticsController()
        {
            if (statisticsController == null)
            {
                TryGetComponent(out statisticsController);
            }

            return statisticsController;
        }

        private void ResolveReferences()
        {
            if (spellSymbolCaster == null)
            {
                spellSymbolCaster = GetComponent<SpellSymbolCaster>();
            }

            if (targeting == null)
            {
                targeting = GetComponent<TargetingController>();
            }

            if (casterObject == null)
            {
                casterObject = gameObject;
            }
        }
    }
}
