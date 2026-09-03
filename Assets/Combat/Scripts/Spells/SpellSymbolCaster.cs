using System;
using RPGame.Core.Spells;
using RPGame.Core.Spells.Symbols;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    public sealed class SpellSymbolCaster : SymbolReceiverBase
    {
        public event Action<Spell> SpellSelected;

        [SerializeField] private SpellSymbolEntry[] spellsBySymbol;

        private SpellSequenceResolver spellSequenceResolver;

        private void Awake()
        {
            spellSequenceResolver = new SpellSequenceResolver(spellsBySymbol);
        }

        public override void ReceiveSymbol(SymbolRecognitionResult result)
        {
            if (!result.IsRecognized)
            {
                Debug.LogWarning("Symbol spell selection skipped because symbol was not recognized.", this);
                return;
            }

            if (!spellSequenceResolver.TryResolve(new[] { result.SymbolId }, out Spell spell))
            {
                Debug.LogWarning($"No spell configured for symbol id {result.SymbolId}.", this);
                return;
            }

            SpellSelected?.Invoke(spell);
        }
    }
}
