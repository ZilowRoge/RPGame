namespace RPGame.Enemies
{
    public interface IEnemyAttack
    {
        float Range { get; }

        void Tick(float deltaTime);
        bool IsInRange(SelectedTarget target);
        bool TryAttack(SelectedTarget target);
    }
}
