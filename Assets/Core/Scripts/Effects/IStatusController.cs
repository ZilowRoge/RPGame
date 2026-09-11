namespace RPGame.Core.Effects
{
    public interface IStatusController
    {
        bool IsStunned { get; }

        void BeginStun();
        void EndStun();
    }
}
