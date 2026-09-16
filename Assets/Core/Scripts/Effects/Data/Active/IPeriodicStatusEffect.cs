namespace RPGame.Core.Effects
{
    public interface IPeriodicStatusEffect
    {
        float TickInterval { get; }

        void Tick(StatusEffectTarget target);
    }
}
