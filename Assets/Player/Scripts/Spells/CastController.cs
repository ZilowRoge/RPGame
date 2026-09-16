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
using UnityEngine.InputSystem;

namespace RPGame.Player.Spells
{
    [RequireComponent(typeof(SpellActivationController))]
    public sealed class CastController : MonoBehaviour, ILastUsedSpellDamageRangeProvider
    {
        [SerializeField] private SpellSymbolCaster spellSymbolCaster;
        [SerializeField] private SpellActivationController spellActivationController;
        [SerializeField] private InputActionReference confirmPlacementAction;
        [SerializeField] private InputActionReference cancelPlacementAction;
        [SerializeField] private TargetingController targeting;
        [SerializeField] private StatisticsController statisticsController;
        [SerializeField] private GameObject casterObject;
        [SerializeField] private Transform castOrigin;
        [SerializeField] private CharacterAttributes characterAttributes;

        private readonly SpellCaster spellCaster = new();
        private readonly LastUsedSpellTracker lastUsedSpellTracker = new();
        private IReadOnlyList<PartialDamageRange> lastUsedSpellDamageRanges = Array.Empty<PartialDamageRange>();
        private Spell pendingSpell;
        private ISpellActivationHandle pendingActivationHandle;

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

            if (confirmPlacementAction != null && confirmPlacementAction.action != null)
            {
                confirmPlacementAction.action.performed += OnConfirmPlacementPerformed;
            }

            if (cancelPlacementAction != null && cancelPlacementAction.action != null)
            {
                cancelPlacementAction.action.performed += OnCancelPlacementPerformed;
            }
        }

        private void OnDisable()
        {
            if (spellSymbolCaster != null)
            {
                spellSymbolCaster.SpellSelected -= OnSpellSelected;
            }

            if (confirmPlacementAction != null && confirmPlacementAction.action != null)
            {
                confirmPlacementAction.action.performed -= OnConfirmPlacementPerformed;
            }

            if (cancelPlacementAction != null && cancelPlacementAction.action != null)
            {
                cancelPlacementAction.action.performed -= OnCancelPlacementPerformed;
            }

            CancelActivation();
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
            return CreateCasterData(null);
        }

        private CasterData CreateCasterData(Vector3? targetPosition)
        {
            ITargetable currentTarget = targeting != null ? targeting.CurrentTarget : null;
            Transform target = currentTarget != null ? currentTarget.TargetPoint : null;

            CasterDataBuilder builder = new CasterDataBuilder(ResolveCasterObject(), castOrigin, target)
                .WithAttributes(ResolveCharacterAttributes())
                .WithStatistics(ResolveStatisticsController());

            if (targetPosition.HasValue)
            {
                builder.WithTargetPosition(targetPosition.Value);
            }

            return builder.Build();
        }

        public bool TryGetLastUsedSpellDamageRanges(out IReadOnlyList<PartialDamageRange> damageRanges)
        {
            damageRanges = lastUsedSpellDamageRanges;
            return damageRanges != null && damageRanges.Count > 0;
        }

        private void OnSpellSelected(Spell spell)
        {
            if (pendingSpell != null || pendingActivationHandle != null)
            {
                CancelActivation();
            }

            CasterData casterData = CreateCasterData();
            ISpellActivationHandle activationHandle = spell.OnActivation(casterData);
            if (activationHandle == null)
            {
                bool wasCast = spellCaster.TryCast(spell, casterData);
                if (wasCast)
                {
                    UpdateLastUsedSpell(spell, casterData);
                }

                return;
            }

            if (spellActivationController == null)
            {
                Debug.LogWarning($"{name} cannot activate {spell.name} because {nameof(SpellActivationController)} is missing.", this);
                spell.OnDeactivation(casterData);
                return;
            }

            pendingSpell = spell;
            pendingActivationHandle = activationHandle;
            spellActivationController.Activate(pendingActivationHandle, casterData);
        }

        private void OnConfirmPlacementPerformed(InputAction.CallbackContext context)
        {
            ConfirmPlacement();
        }

        private void OnCancelPlacementPerformed(InputAction.CallbackContext context)
        {
            CancelActivation();
        }

        private void ConfirmPlacement()
        {
            if (pendingSpell == null || pendingActivationHandle == null)
            {
                return;
            }

            CasterData casterData = CreateCasterData();
            if (spellActivationController == null ||
                !spellActivationController.TryCreateCasterData(casterData, out CasterData activatedCasterData))
            {
                return;
            }

            bool wasCast = spellCaster.TryCast(pendingSpell, activatedCasterData);

            if (!wasCast)
            {
                return;
            }

            UpdateLastUsedSpell(pendingSpell, activatedCasterData);
            FinishActivationCast();
        }

        private void CancelActivation()
        {
            if (pendingSpell != null)
            {
                pendingSpell.OnDeactivation(CreateCasterData());
            }

            spellActivationController?.Deactivate();
            pendingSpell = null;
            pendingActivationHandle = null;
        }

        private void FinishActivationCast()
        {
            spellActivationController?.Deactivate();
            pendingSpell = null;
            pendingActivationHandle = null;
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

            if (spellActivationController == null)
            {
                spellActivationController = GetComponent<SpellActivationController>();
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
