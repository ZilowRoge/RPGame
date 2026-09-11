namespace RPGame.Core.Effects
{
    public interface IStatusApplicator
    {
        void ApplyStatus(ActiveEffectDefinition effect, float duration);
    }
}
