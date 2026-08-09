using System;
using System.Collections.Generic;
using RPGame.Core.Damage;
using RPGame.Core.Spells;
using RPGame.Core.Statistics;
using RPGame.Core.Statistics.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGame.Combat.Spells
{
    public sealed class SpellCaster : MonoBehaviour, ILastUsedSpellDamageRangeProvider
    {
        [SerializeField] private Spell currentSpell;
        [SerializeField] private Transform castOrigin;
        [SerializeField] private Transform target;
        [SerializeField] private GameObject casterObject;
        [SerializeField] private StatisticsController statisticsController;
        [SerializeField] private CharacterAttributes characterAttributes;
        [SerializeField] private bool castOnLeftMouseButton = true;
        [SerializeField] private bool logLastUsedSpellChanges = true;

        private readonly LastUsedSpellTracker lastUsedSpellTracker = new();

        public event Action LastUsedSpellDamageRangeChanged;

        public Spell CurrentSpell => currentSpell;
        public Transform CastOrigin => castOrigin;
        public Transform Target => target;
        public GameObject CasterObject => casterObject != null ? casterObject : gameObject;
        public IStatisticsController Statistics => ResolveStatisticsController();
        public LastUsedSpellTracker LastUsedSpellTracker => lastUsedSpellTracker;

        private void Awake()
        {
            if (casterObject == null)
            {
                casterObject = gameObject;
            }
        }

        private void Update()
        {
            if (!castOnLeftMouseButton)
            {
                return;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryCast();
            }
        }

        public void SetSpell(Spell spell)
        {
            if (currentSpell == spell)
            {
                return;
            }

            if (currentSpell != null)
            {
                currentSpell.OnDeactivation(CreateCasterData());
            }

            currentSpell = spell;

            if (currentSpell != null)
            {
                currentSpell.OnActivation(CreateCasterData());
            }
        }

        public void SetCastOrigin(Transform origin)
        {
            castOrigin = origin;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void Activate()
        {
            if (currentSpell == null)
            {
                return;
            }

            currentSpell.OnActivation(CreateCasterData());
        }

        public void Deactivate()
        {
            if (currentSpell == null)
            {
                return;
            }

            currentSpell.OnDeactivation(CreateCasterData());
        }

        public bool TryCast()
        {
            if (currentSpell == null)
            {
                Debug.LogWarning("SpellCaster.TryCast failed because currentSpell is not assigned.", this);
                return false;
            }

            IStatisticsController statistics = ResolveStatisticsController();
            if (statistics != null && !statistics.TrySpendMana(currentSpell.ManaCost))
            {
                return false;
            }

            currentSpell.OnCast(CreateCasterData());
            if (lastUsedSpellTracker.SetLastUsedSpell(currentSpell))
            {
                LogLastUsedSpellChanged(currentSpell);
                LastUsedSpellDamageRangeChanged?.Invoke();
            }

            return true;
        }

        public bool TryGetLastUsedSpellDamageRanges(out IReadOnlyList<PartialDamageRange> damageRanges)
        {
            damageRanges = null;
            if (lastUsedSpellTracker.LastUsedSpell is not ICasterDamageRangeProvider damageRangeProvider)
            {
                return false;
            }

            damageRanges = damageRangeProvider.GetDamageRanges(CreateCasterData());
            return damageRanges != null && damageRanges.Count > 0;
        }

        public CasterData CreateCasterData()
        {
            return new CasterDataBuilder(CasterObject, castOrigin, target)
                .WithAttributes(ResolveCharacterAttributes())
                .Build();
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
