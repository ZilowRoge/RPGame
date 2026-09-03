using System;
using System.Collections.Generic;
using RPGame.Core.Spells;

namespace RPGame.Combat.Spells
{
    public sealed class SpellSequenceResolver
    {
        private readonly Dictionary<SpellSequenceKey, Spell> spellsBySequence = new();

        public SpellSequenceResolver(SpellSymbolEntry[] entries)
        {
            if (entries == null)
            {
                return;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                SpellSymbolEntry entry = entries[i];
                if (entry == null || entry.Spell == null || entry.SymbolIds == null || entry.SymbolIds.Count == 0)
                {
                    continue;
                }

                SpellSequenceKey key = new(entry.SymbolIds);
                if (!spellsBySequence.ContainsKey(key))
                {
                    spellsBySequence.Add(key, entry.Spell);
                }
            }
        }

        public bool TryResolve(IReadOnlyList<int> symbolIds, out Spell spell)
        {
            if (symbolIds != null && spellsBySequence.TryGetValue(new SpellSequenceKey(symbolIds), out spell))
            {
                return true;
            }

            spell = null;
            return false;
        }

        private readonly struct SpellSequenceKey : IEquatable<SpellSequenceKey>
        {
            private readonly int[] symbolIds;
            private readonly int hashCode;

            public SpellSequenceKey(IReadOnlyList<int> symbolIds)
            {
                this.symbolIds = new int[symbolIds.Count];
                hashCode = 17;
                for (int i = 0; i < symbolIds.Count; i++)
                {
                    this.symbolIds[i] = symbolIds[i];
                    hashCode = hashCode * 31 + symbolIds[i];
                }
            }

            public bool Equals(SpellSequenceKey other)
            {
                if (symbolIds == null || other.symbolIds == null || symbolIds.Length != other.symbolIds.Length)
                {
                    return symbolIds == null && other.symbolIds == null;
                }

                for (int i = 0; i < symbolIds.Length; i++)
                {
                    if (symbolIds[i] != other.symbolIds[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public override bool Equals(object obj) => obj is SpellSequenceKey other && Equals(other);
            public override int GetHashCode() => hashCode;
        }
    }
}
