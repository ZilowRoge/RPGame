namespace RPGame.Core.Pooling
{
    public interface IPooledEnemyResettable
    {
        void ResetForSpawn();
        void ResetForDespawn();
    }
}
