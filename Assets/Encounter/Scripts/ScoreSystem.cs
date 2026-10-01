using UnityEngine;

namespace RPGame.Encounter
{
    public sealed class ScoreSystem
    {
        private readonly float comboWindowSeconds;
        private readonly float multiplierPerKill;
        private readonly float initialMultiplier;
        private readonly WaveScalingConfig waveScalingConfig;

        private float comboTimeRemaining;
        private bool isComboActive;

        public ScoreSystem()
            : this(null, null)
        {
        }

        public ScoreSystem(ScoreConfig scoreConfig)
            : this(scoreConfig, null)
        {
        }

        public ScoreSystem(ScoreConfig scoreConfig, WaveScalingConfig waveScalingConfig)
            : this(
                scoreConfig != null ? scoreConfig.ComboWindowSeconds : 5f,
                scoreConfig != null ? scoreConfig.MultiplierPerKill : 0.1f,
                scoreConfig != null ? scoreConfig.InitialMultiplier : 1f,
                waveScalingConfig)
        {
        }

        private ScoreSystem(
            float comboWindowSeconds,
            float multiplierPerKill,
            float initialMultiplier,
            WaveScalingConfig waveScalingConfig)
        {
            this.comboWindowSeconds = comboWindowSeconds > 0f ? comboWindowSeconds : 5f;
            this.multiplierPerKill = multiplierPerKill >= 0f ? multiplierPerKill : 0.1f;
            this.initialMultiplier = initialMultiplier > 0f ? initialMultiplier : 1f;
            this.waveScalingConfig = waveScalingConfig;
            CurrentMultiplier = this.initialMultiplier;
        }

        public float CurrentScore { get; private set; }
        public float CurrentMultiplier { get; private set; }
        public float CurrentWaveBaseScore { get; private set; }
        public float CurrentWaveActualScore { get; private set; }
        public float FinalizedWaveBaseScore { get; private set; }
        public float FinalizedWaveActualScore { get; private set; }
        public float LastWavePerformanceRatio => FinalizedWaveBaseScore > 0f
            ? FinalizedWaveActualScore / FinalizedWaveBaseScore
            : 1f;
        public float LastWavePerformanceBonus { get; private set; }

        public void BeginWave()
        {
            CurrentWaveBaseScore = 0f;
            CurrentWaveActualScore = 0f;
        }

        public void EndWave()
        {
            FinalizedWaveBaseScore = CurrentWaveBaseScore;
            FinalizedWaveActualScore = CurrentWaveActualScore;
            LastWavePerformanceBonus = CalculatePerformanceBonus();
        }

        public void ResetRun()
        {
            CurrentScore = 0f;
            CurrentWaveBaseScore = 0f;
            CurrentWaveActualScore = 0f;
            FinalizedWaveBaseScore = 0f;
            FinalizedWaveActualScore = 0f;
            LastWavePerformanceBonus = 0f;
            ResetCombo();
        }

        public void RegisterKill(int enemyCost)
        {
            if (enemyCost <= 0)
            {
                return;
            }

            float awardedScore = CalculateScore(enemyCost);
            CurrentScore += awardedScore;
            CurrentWaveBaseScore += enemyCost;
            CurrentWaveActualScore += awardedScore;
            isComboActive = true;
            comboTimeRemaining = comboWindowSeconds;
            CurrentMultiplier += multiplierPerKill;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || !isComboActive)
            {
                return;
            }

            comboTimeRemaining -= deltaTime;
            if (comboTimeRemaining <= 0f)
            {
                ResetCombo();
            }
        }

        private float CalculateScore(int enemyCost)
        {
            return enemyCost * CurrentMultiplier;
        }

        private float CalculatePerformanceBonus()
        {
            if (waveScalingConfig == null || waveScalingConfig.PerformanceWaveBonusCurve == null)
            {
                return 0f;
            }

            float maxBonus = waveScalingConfig.MaxPerformanceWaveBonus > 0f
                ? waveScalingConfig.MaxPerformanceWaveBonus
                : 0f;
            float bonus = waveScalingConfig.PerformanceWaveBonusCurve.Evaluate(LastWavePerformanceRatio);
            return Mathf.Clamp(bonus, 0f, maxBonus);
        }

        private void ResetCombo()
        {
            isComboActive = false;
            comboTimeRemaining = 0f;
            CurrentMultiplier = initialMultiplier;
        }
    }
}
