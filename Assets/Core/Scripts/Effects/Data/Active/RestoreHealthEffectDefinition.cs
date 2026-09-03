using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Core.Effects
{
    [CreateAssetMenu(fileName = "RestoreHealthEffect", menuName = "RPGame/Progression/Effects/Restore Health Effect")]
    public sealed class RestoreHealthEffectDefinition : RestoreResourceEffectDefinition
    {
        protected override string ResourceName => "Health";

        public override bool IsFinished(IStatisticsController statisticsController)
        {
            return statisticsController == null
                || statisticsController.CurrentHealth >= statisticsController.MaxHealth;
        }

        protected override void Restore(IStatisticsController statisticsController, float amount)
        {
            statisticsController.Heal(amount);
        }
    }
}
