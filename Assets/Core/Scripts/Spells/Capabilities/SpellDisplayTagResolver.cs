using System;
using System.Collections.Generic;
using System.Reflection;

namespace RPGame.Core.Spells
{
    public static class SpellDisplayTagResolver
    {
        private static readonly IReadOnlyList<string> EmptyDisplayTags = Array.Empty<string>();
        private static readonly Dictionary<Type, IReadOnlyList<string>> DisplayTagsBySpellType = new();

        public static IReadOnlyList<string> GetDisplayTags(Spell spell)
        {
            if (spell == null)
            {
                return EmptyDisplayTags;
            }

            Type spellType = spell.GetType();
            if (!DisplayTagsBySpellType.TryGetValue(spellType, out IReadOnlyList<string> displayTags))
            {
                displayTags = ResolveDisplayTags(spellType);
                DisplayTagsBySpellType.Add(spellType, displayTags);
            }

            return displayTags;
        }

        private static IReadOnlyList<string> ResolveDisplayTags(Type spellType)
        {
            Type[] interfaces = spellType.GetInterfaces();
            Array.Sort(interfaces, CompareByFullName);

            List<string> displayTags = new();
            for (int i = 0; i < interfaces.Length; i++)
            {
                SpellDisplayTagAttribute attribute =
                    interfaces[i].GetCustomAttribute<SpellDisplayTagAttribute>(inherit: false);
                if (attribute != null && !string.IsNullOrWhiteSpace(attribute.DisplayName))
                {
                    displayTags.Add(attribute.DisplayName);
                }
            }

            return displayTags.Count > 0 ? displayTags.ToArray() : EmptyDisplayTags;
        }

        private static int CompareByFullName(Type left, Type right)
        {
            return string.Compare(left.FullName, right.FullName, StringComparison.Ordinal);
        }
    }
}
