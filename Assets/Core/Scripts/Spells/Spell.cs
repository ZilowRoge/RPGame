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

        public virtual void OnActivation(CasterData casterData)
        {
        }

        public abstract void OnCast(CasterData casterData);

        private void OnValidate()
        {
            manaCost = Mathf.Max(0f, manaCost);
        }
    }
}
