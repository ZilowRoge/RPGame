namespace RPGame.Core.Effects
{
    public interface IAmountStatusEffect
    {
        float Amount { get; }

        void Tick(StatusEffectTarget target, float deltaTime, float amount);
    }
}
