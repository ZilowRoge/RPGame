using System;

namespace RPGame.Core.Spells
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class SpellDisplayTagAttribute : Attribute
    {
        public SpellDisplayTagAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; }
    }
}
