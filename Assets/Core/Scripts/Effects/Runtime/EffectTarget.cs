using RPGame.Core.Damage;
using RPGame.Core.Statistics;

namespace RPGame.Core.Effects
{
    public readonly struct EffectTarget
    {
        public EffectTarget(
            IStatisticsController statisticsController,
            IStatusController statusController,
            IDamageable damageable)
        {
            StatisticsController = statisticsController;
            StatusController = statusController;
            Damageable = damageable;
        }

        public IStatisticsController StatisticsController { get; }
        public IStatusController StatusController { get; }
        public IDamageable Damageable { get; }
    }
}
