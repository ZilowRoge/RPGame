namespace RPGame.Core.Spells
{
    public sealed class SpellPropertyModifiers
    {
        public static SpellPropertyModifiers Empty { get; } = new(0f, 0f, 0f, 0f);

        private readonly float radius;
        private readonly float duration;
        private readonly float controlPower;
        private readonly float orbCount;

        internal SpellPropertyModifiers(
            float radius,
            float duration,
            float controlPower,
            float orbCount)
        {
            this.radius = radius;
            this.duration = duration;
            this.controlPower = controlPower;
            this.orbCount = orbCount;
        }

        public float GetValue(SpellProperty property)
        {
            return property switch
            {
                SpellProperty.Radius => radius,
                SpellProperty.Duration => duration,
                SpellProperty.ControlPower => controlPower,
                SpellProperty.OrbCount => orbCount,
                _ => 0f
            };
        }
    }
}
