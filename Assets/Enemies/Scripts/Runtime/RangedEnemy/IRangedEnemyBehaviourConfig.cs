namespace RPGame.Enemies
{
    public interface IRangedEnemyBehaviourConfig
    {
        float MinRange { get; }
        float MaxRange { get; }
        float RangeHysteresis { get; }
        float RepositionSearchInterval { get; }
        float AttackDelay { get; }
    }
}
