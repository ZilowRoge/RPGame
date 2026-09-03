using System;

namespace RPGame.Core.Spells
{
    [Flags]
    public enum SpellTags
    {
        None = 0,
        Projectile = 1 << 0,
        AoE = 1 << 1,
        Duration = 1 << 2,
        Control = 1 << 3,
        DamageOverTime = 1 << 4
    }
}
