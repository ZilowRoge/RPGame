namespace RPGame.Core.Effects
{
    public interface IStatusEffectReceiver
    {
        void ApplyStatusEffect(StatusEffectDefinition effect, float duration);
    }
}
