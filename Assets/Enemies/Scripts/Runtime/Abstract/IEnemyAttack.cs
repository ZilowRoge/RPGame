namespace RPGame.Enemies
{
    public interface IEnemyAttack
    {
        bool IsInRange(SelectedTarget target);
        bool TryAttack(SelectedTarget target);
    }
}
