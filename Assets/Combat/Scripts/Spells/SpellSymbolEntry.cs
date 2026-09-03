using System;
using System.Collections.Generic;
using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    [Serializable]
    public sealed class SpellSymbolEntry
    {
        [SerializeField] private int[] symbolIds;
        [SerializeField] private Spell spell;

        public IReadOnlyList<int> SymbolIds => symbolIds;
        public Spell Spell => spell;
    }
}
