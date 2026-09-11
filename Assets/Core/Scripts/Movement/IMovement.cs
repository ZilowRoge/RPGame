namespace RPGame.Core.Movement
{
    public interface IMovement
    {
        void BlockMovement();
        void UnblockMovement();
        int AddMovementSpeedModifier(float multiplier);
        void RemoveMovementSpeedModifier(int modifierId);
    }
}
