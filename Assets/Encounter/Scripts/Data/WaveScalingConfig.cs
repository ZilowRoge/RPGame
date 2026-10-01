using UnityEngine;

namespace RPGame.Encounter
{
    [CreateAssetMenu(fileName = "WaveScalingConfig", menuName = "RPGame/Encounter/Wave Scaling Config")]
    public sealed class WaveScalingConfig : ScriptableObject
    {
        [SerializeField] private AnimationCurve budgetCurve = AnimationCurve.Linear(1f, 1f, 10f, 10f);
        [SerializeField] private AnimationCurve performanceWaveBonusCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);
        [SerializeField] private float maxPerformanceWaveBonus;

        public AnimationCurve BudgetCurve => budgetCurve;
        public AnimationCurve PerformanceWaveBonusCurve => performanceWaveBonusCurve;
        public float MaxPerformanceWaveBonus => maxPerformanceWaveBonus;

        private void OnValidate()
        {
            budgetCurve ??= AnimationCurve.Linear(1f, 1f, 10f, 10f);
            performanceWaveBonusCurve ??= AnimationCurve.Linear(0f, 0f, 1f, 0f);
            maxPerformanceWaveBonus = Mathf.Max(0f, maxPerformanceWaveBonus);
        }
    }
}
