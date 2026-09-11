using RPGame.Core.Effects;

namespace RPGame.Core.Movement
{
    public interface IMovement
    {
        void BlockMovement();
        void UnblockMovement();
        void AddMovementSpeedModifier(TimedEffectInstance source, float multiplier);
        void RemoveMovementSpeedModifier(TimedEffectInstance source);
    }
}
