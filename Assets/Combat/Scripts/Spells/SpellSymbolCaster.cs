using System;
using RPGame.Core.Spells;
using RPGame.Core.Spells.Symbols;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    public sealed class SpellSymbolCaster : SymbolReceiverBase
    {
        [Serializable]
        private sealed class SpellSymbolEntry
        {
            [SerializeField] private int symbolId;
            [SerializeField] private Spell spell;

            public int SymbolId => symbolId;
            public Spell Spell => spell;
        }

        [SerializeField] private SpellCaster spellCaster;
        [SerializeField] private SpellSymbolEntry[] spellsBySymbol;

        private void Awake()
        {
            if (spellCaster == null)
            {
                spellCaster = GetComponent<SpellCaster>();
            }
        }

        public override void ReceiveSymbol(SymbolRecognitionResult result)
        {
            if (!result.IsRecognized || spellCaster == null)
            {
                Debug.LogWarning($"Symbol spell cast skipped. Recognized: {result.IsRecognized}, SpellCaster assigned: {spellCaster != null}.", this);
                return;
            }

            if (!TryGetSpell(result.SymbolId, out Spell spell))
            {
                Debug.LogWarning($"No spell configured for symbol id {result.SymbolId}.", this);
                return;
            }

            spellCaster.SetSpell(spell);
            bool wasCast = spellCaster.TryCast();
            if (!wasCast)
            {
                Debug.LogWarning($"SpellCaster.TryCast failed for '{spell.name}'.", this);
            }
        }

        private bool TryGetSpell(int symbolId, out Spell spell)
        {
            spell = null;

            if (spellsBySymbol == null || spellsBySymbol.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < spellsBySymbol.Length; i++)
            {
                SpellSymbolEntry entry = spellsBySymbol[i];
                if (entry == null || entry.SymbolId != symbolId || entry.Spell == null)
                {
                    continue;
                }

                spell = entry.Spell;
                return true;
            }

            return false;
        }
    }
}
