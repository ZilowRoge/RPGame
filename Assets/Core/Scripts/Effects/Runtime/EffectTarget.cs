using RPGame.Core.Movement;
using RPGame.Core.Statistics;

namespace RPGame.Core.Effects
{
    public readonly struct EffectTarget
    {
        public EffectTarget(
            IStatisticsController statisticsController,
            IStatusController statusController,
            IMovement movement)
        {
            StatisticsController = statisticsController;
            StatusController = statusController;
            Movement = movement;
        }

        public IStatisticsController StatisticsController { get; }
        public IStatusController StatusController { get; }
        public IMovement Movement { get; }
    }
}
