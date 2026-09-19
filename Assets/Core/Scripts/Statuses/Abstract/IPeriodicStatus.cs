namespace RPGame.Core.Statuses
{
    public interface IPeriodicStatus
    {
        float TickInterval { get; }

        void Tick(StatusTarget target);
    }
}
