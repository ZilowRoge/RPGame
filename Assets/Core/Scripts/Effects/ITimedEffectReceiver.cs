namespace RPGame.Core.Effects
{
    public interface ITimedEffectReceiver
    {
        void ApplyTimedEffect(ActiveEffectDefinition effect, float duration);
    }
}
