using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Core.Effects
{
    public abstract class RestoreResourceEffectDefinition : ActiveEffectDefinition, IAmountTimedEffect
    {
        [SerializeField] private float amount = 25f;

        public float Amount => Mathf.Max(0f, amount);

        public void Tick(EffectTarget target, float deltaTime, float amount)
        {
            IStatisticsController statisticsController = target.StatisticsController;
            if (statisticsController == null || amount <= 0f)
            {
                return;
            }

            Restore(statisticsController, amount);
        }

        public override string ToString()
        {
            return $"Restore {Amount:0.##} {ResourceName}";
        }

        protected abstract string ResourceName { get; }
        protected abstract void Restore(IStatisticsController statisticsController, float amount);
    }
}
