using UnityEngine;

namespace RPGame.Encounter
{
    [CreateAssetMenu(fileName = "WaveScalingConfig", menuName = "RPGame/Encounter/Wave Scaling Config")]
    public sealed class WaveScalingConfig : ScriptableObject
    {
        [SerializeField] private float baseBudget = 6f;
        [SerializeField] private float budgetPerWave = 1.2f;
        [SerializeField] private AnimationCurve budgetGrowthCurve = AnimationCurve.Constant(0f, 1f, 0f);
        [SerializeField] private AnimationCurve performanceWaveBonusCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);
        [SerializeField] private float maxPerformanceWaveBonus;

        public float BaseBudget => baseBudget;
        public float BudgetPerWave => budgetPerWave;
        public AnimationCurve BudgetGrowthCurve => budgetGrowthCurve;
        public AnimationCurve PerformanceWaveBonusCurve => performanceWaveBonusCurve;
        public float MaxPerformanceWaveBonus => maxPerformanceWaveBonus;

        private void OnValidate()
        {
            baseBudget = Mathf.Max(1f, baseBudget);
            budgetPerWave = Mathf.Max(0f, budgetPerWave);
            budgetGrowthCurve ??= AnimationCurve.Constant(0f, 1f, 0f);
            performanceWaveBonusCurve ??= AnimationCurve.Linear(0f, 0f, 1f, 0f);
            maxPerformanceWaveBonus = Mathf.Max(0f, maxPerformanceWaveBonus);
        }
    }
}
