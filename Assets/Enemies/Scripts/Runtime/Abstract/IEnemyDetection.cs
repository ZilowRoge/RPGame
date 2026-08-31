namespace RPGame.Enemies
{
    public interface IEnemyDetection
    {
        bool TryGetTarget(out SelectedTarget target);
    }
}
