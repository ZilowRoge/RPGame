using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Core.Effects
{
    [CreateAssetMenu(fileName = "RestoreStaminaEffect", menuName = "RPGame/Progression/Effects/Restore Stamina Effect")]
    public sealed class RestoreStaminaEffectDefinition : RestoreResourceEffectDefinition
    {
        protected override string ResourceName => "Stamina";

        public override bool IsFinished(IStatisticsController statisticsController)
        {
            return statisticsController == null
                || statisticsController.CurrentStamina >= statisticsController.MaxStamina;
        }

        protected override void Restore(IStatisticsController statisticsController, float amount)
        {
            statisticsController.RestoreStamina(amount);
        }
    }
}
