namespace RPGame.Core.Effects
{
    public interface IAmountTimedEffect
    {
        float Amount { get; }

        void Tick(EffectTarget target, float deltaTime, float amount);
    }
}
