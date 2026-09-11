namespace RPGame.Core.Movement
{
    public interface IMovement
    {
        void BlockMovement();
        void UnblockMovement();
        void AddMovementSpeedModifier(IModifierSource source, float multiplier);
        void RemoveMovementSpeedModifier(IModifierSource source);
    }
}
