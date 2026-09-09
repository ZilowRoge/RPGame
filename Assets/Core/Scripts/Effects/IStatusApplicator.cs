namespace RPGame.Core.Effects
{
    public interface IStatusApplicator
    {
        void ApplyStatus(StatusEffectDefinition effect, float duration);
    }
}
