namespace RPGame.Core.Spells
{
    public interface IAoECapability
    {
        float Radius { get; }
    }

    public interface IDurationCapability
    {
        float Duration { get; }
    }

    public interface IControlCapability
    {
        float ControlPower { get; }
    }

    public interface IOrbCapability
    {
        int OrbCount { get; }
    }

    public interface IProjectileCapability
    {
    }
}
