using System;
using UnityEngine;

namespace RPGame.Enemies
{
    [CreateAssetMenu(
        fileName = "RangedEnemyBehaviourConfig",
        menuName = "RPGame/Enemies/Ranged Enemy Behaviour Config")]
    public sealed class RangedEnemyBehaviourConfig : EnemyBehaviourConfigBase, IRangedEnemyBehaviourConfig
    {
        [SerializeField] private float minRange = 3f;
        [SerializeField] private float maxRange = 8f;
        [SerializeField] private float rangeHysteresis = 0.5f;

        public float MinRange => minRange;
        public float MaxRange => maxRange;
        public float RangeHysteresis => rangeHysteresis;

        private void OnValidate()
        {
            minRange = Mathf.Max(0f, minRange);
            maxRange = Mathf.Max(minRange + 0.01f, maxRange);
            rangeHysteresis = Mathf.Max(0f, rangeHysteresis);
            rangeHysteresis = Mathf.Min(rangeHysteresis, GetMaxRangeHysteresis(minRange, maxRange));

            ValidateRanges(minRange, maxRange, rangeHysteresis);
        }

        internal static void ValidateRanges(float minRange, float maxRange, float rangeHysteresis)
        {
            if (minRange < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(minRange), "Min range must be greater than or equal to zero.");
            }

            if (maxRange <= minRange)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRange), "Max range must be greater than min range.");
            }

            if (rangeHysteresis < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(rangeHysteresis), "Range hysteresis must be greater than or equal to zero.");
            }

            if (rangeHysteresis > GetMaxRangeHysteresis(minRange, maxRange))
            {
                throw new ArgumentOutOfRangeException(nameof(rangeHysteresis), "Range hysteresis must not exceed half of the valid range.");
            }
        }

        private static float GetMaxRangeHysteresis(float minRange, float maxRange)
        {
            return (maxRange - minRange) / 2f;
        }
    }
}
