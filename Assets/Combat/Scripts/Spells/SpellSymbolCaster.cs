using System;
using RPGame.Core.Spells;
using RPGame.Core.Spells.Symbols;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    public sealed class SpellSymbolCaster : SymbolReceiverBase
    {
        public event Action<Spell> SpellSelected;

        [Serializable]
        private sealed class SpellSymbolEntry
        {
            [SerializeField] private int symbolId;
            [SerializeField] private Spell spell;

            public int SymbolId => symbolId;
            public Spell Spell => spell;
        }

        [SerializeField] private SpellSymbolEntry[] spellsBySymbol;

        public override void ReceiveSymbol(SymbolRecognitionResult result)
        {
            if (!result.IsRecognized)
            {
                Debug.LogWarning("Symbol spell selection skipped because symbol was not recognized.", this);
                return;
            }

            if (!TryGetSpell(result.SymbolId, out Spell spell))
            {
                Debug.LogWarning($"No spell configured for symbol id {result.SymbolId}.", this);
                return;
            }

            SpellSelected?.Invoke(spell);
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
