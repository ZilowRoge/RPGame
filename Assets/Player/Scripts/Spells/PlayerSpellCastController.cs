using RPGame.Combat.Spells;
using RPGame.Core.Spells;
using RPGame.Core.Statistics.Attributes;
using RPGame.Core.Targeting;
using RPGame.Player.Targeting;
using UnityEngine;

namespace RPGame.Player.Spells
{
    public sealed class PlayerSpellCastController : MonoBehaviour
    {
        [SerializeField] private SpellSymbolCaster spellSymbolCaster;
        [SerializeField] private PlayerTargeting playerTargeting;
        [SerializeField] private SpellCaster spellCaster;
        [SerializeField] private GameObject casterObject;
        [SerializeField] private Transform castOrigin;
        [SerializeField] private CharacterAttributes characterAttributes;

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

        internal bool CastSpell(Spell spell)
        {
            if (spellCaster == null)
            {
                Debug.LogWarning("PlayerSpellCastController cast skipped because SpellCaster is not assigned.", this);
                return false;
            }

            CasterData casterData = CreateCasterData();
            return spellCaster.TryCast(spell, casterData);
        }

        internal CasterData CreateCasterData()
        {
            ITargetable currentTarget = playerTargeting != null ? playerTargeting.CurrentTarget : null;
            Transform target = currentTarget != null ? currentTarget.TargetPoint : null;

            return new CasterDataBuilder(ResolveCasterObject(), castOrigin, target)
                .WithAttributes(ResolveCharacterAttributes())
                .Build();
        }

        private void OnSpellSelected(Spell spell)
        {
            CastSpell(spell);
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

        private void ResolveReferences()
        {
            if (spellSymbolCaster == null)
            {
                spellSymbolCaster = GetComponent<SpellSymbolCaster>();
            }

            if (playerTargeting == null)
            {
                playerTargeting = GetComponent<PlayerTargeting>();
            }

            if (spellCaster == null)
            {
                spellCaster = GetComponent<SpellCaster>();
            }

            if (casterObject == null)
            {
                casterObject = gameObject;
            }
        }
    }
}
