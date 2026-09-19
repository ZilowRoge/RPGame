namespace RPGame.Core.Statuses
{
    public interface IStatusReceiver
    {
        void ApplyStatus(StatusDefinition status, float duration, StatusContext context);
        bool HasStatus(StatusDefinition status);
        bool TryConsumeStatus(StatusDefinition status);
    }
}
