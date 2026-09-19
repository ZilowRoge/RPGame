namespace RPGame.Core.Statuses
{
    public interface IAmountStatus
    {
        float Amount { get; }

        void Tick(StatusTarget target, float deltaTime, float amount);
    }
}
