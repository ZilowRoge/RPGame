using RPGame.Core.Damage;
using RPGame.Core.Movement;
using RPGame.Core.Statistics;

namespace RPGame.Core.Effects
{
    public readonly struct EffectTarget
    {
        public EffectTarget(
            IStatisticsController statisticsController,
            IStatusController statusController,
            IDamageable damageable,
            IMovement movement)
        {
            StatisticsController = statisticsController;
            StatusController = statusController;
            Damageable = damageable;
            Movement = movement;
        }

        public IStatisticsController StatisticsController { get; }
        public IStatusController StatusController { get; }
        public IDamageable Damageable { get; }
        public IMovement Movement { get; }
    }
}
