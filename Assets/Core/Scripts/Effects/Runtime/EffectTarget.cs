using RPGame.Core.Movement;
using RPGame.Core.Statistics;

namespace RPGame.Core.Effects
{
    public readonly struct EffectTarget
    {
        public EffectTarget(
            IStatisticsController statisticsController,
            IMovement movement)
        {
            StatisticsController = statisticsController;
            Movement = movement;
        }

        public IStatisticsController StatisticsController { get; }
        public IMovement Movement { get; }
    }
}
