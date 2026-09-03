using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Core.Effects
{
    [CreateAssetMenu(fileName = "RestoreManaEffect", menuName = "RPGame/Progression/Effects/Restore Mana Effect")]
    public sealed class RestoreManaEffectDefinition : RestoreResourceEffectDefinition
    {
        protected override string ResourceName => "Mana";

        public override bool IsFinished(IStatisticsController statisticsController)
        {
            return statisticsController == null
                || statisticsController.CurrentMana >= statisticsController.MaxMana;
        }

        protected override void Restore(IStatisticsController statisticsController, float amount)
        {
            statisticsController.RestoreMana(amount);
        }
    }
}
