using RPGame.Core.Spells;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGame.Combat.Spells
{
    public sealed class SpellCaster : MonoBehaviour
    {
        [SerializeField] private Spell currentSpell;
        [SerializeField] private Transform castOrigin;
        [SerializeField] private Transform target;
        [SerializeField] private GameObject casterObject;
        [SerializeField] private bool castOnLeftMouseButton = true;

        public Spell CurrentSpell => currentSpell;
        public Transform CastOrigin => castOrigin;
        public Transform Target => target;
        public GameObject CasterObject => casterObject != null ? casterObject : gameObject;

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

            currentSpell.OnCast(CreateCasterData());
            return true;
        }

        public CasterData CreateCasterData()
        {
            return new CasterData(CasterObject, castOrigin, target);
        }
    }
}
