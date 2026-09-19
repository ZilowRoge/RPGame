using RPGame.Core.Damage;
using RPGame.Core.Movement;
using RPGame.Core.Statistics;

namespace RPGame.Core.Statuses
{
    public readonly struct StatusTarget
    {
        public StatusTarget(
            IStatisticsController statisticsController,
            IMovement movement,
            IDamageable damageable)
        {
            StatisticsController = statisticsController;
            Movement = movement;
            Damageable = damageable;
        }

        public IStatisticsController StatisticsController { get; }
        public IMovement Movement { get; }
        public IDamageable Damageable { get; }
    }
}
