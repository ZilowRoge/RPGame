using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Core.Effects
{
    public abstract class RestoreResourceEffectDefinition : ActiveEffectDefinition
    {
        [SerializeField] private float amount = 25f;

        public override float Amount => Mathf.Max(0f, amount);

        public override void Apply(IStatisticsController statisticsController, float amount)
        {
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
