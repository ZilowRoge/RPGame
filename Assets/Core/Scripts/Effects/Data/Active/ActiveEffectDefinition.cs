using RPGame.Core.Statistics;

namespace RPGame.Core.Effects
{
    public abstract class ActiveEffectDefinition : EffectDefinition
    {
        public abstract float Amount { get; }

        public abstract void Apply(IStatisticsController statisticsController, float amount);
        public abstract bool IsFinished(IStatisticsController statisticsController);
    }
}
