using UnityEngine;

namespace RPGame.Core.Spells
{
    public abstract class Spell : ScriptableObject, ICastable
    {
        [SerializeField] private GameObject spellPrefab;
        [SerializeField] private float manaCost;

        public GameObject SpellPrefab => spellPrefab;
        public float ManaCost => manaCost;
        public abstract SpellTags Tags { get; }

        public virtual void OnDeactivation(CasterData casterData)
        {
        }

        public virtual ISpellActivationHandle OnActivation(CasterData casterData)
        {
            return null;
        }

        public abstract void OnCast(CasterData casterData);

        private void OnValidate()
        {
            manaCost = Mathf.Max(0f, manaCost);
        }
    }
}
