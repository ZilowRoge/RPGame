namespace RPGame.Encounter
{
    public sealed class ScoreSystem
    {
        private readonly float comboWindowSeconds;
        private readonly float multiplierPerKill;
        private readonly float initialMultiplier;

        private float comboTimeRemaining;
        private bool isComboActive;

        public ScoreSystem()
            : this(5f, 0.1f, 1f)
        {
        }

        public ScoreSystem(ScoreConfig scoreConfig)
            : this(
                scoreConfig != null ? scoreConfig.ComboWindowSeconds : 5f,
                scoreConfig != null ? scoreConfig.MultiplierPerKill : 0.1f,
                scoreConfig != null ? scoreConfig.InitialMultiplier : 1f)
        {
        }

        private ScoreSystem(float comboWindowSeconds, float multiplierPerKill, float initialMultiplier)
        {
            this.comboWindowSeconds = comboWindowSeconds > 0f ? comboWindowSeconds : 5f;
            this.multiplierPerKill = multiplierPerKill >= 0f ? multiplierPerKill : 0.1f;
            this.initialMultiplier = initialMultiplier > 0f ? initialMultiplier : 1f;
            CurrentMultiplier = this.initialMultiplier;
        }

        public float CurrentScore { get; private set; }
        public float CurrentMultiplier { get; private set; }
        public float CurrentWaveBaseScore { get; private set; }
        public float CurrentWaveActualScore { get; private set; }
        public float FinalizedWaveBaseScore { get; private set; }
        public float FinalizedWaveActualScore { get; private set; }

        public void BeginWave()
        {
            CurrentWaveBaseScore = 0f;
            CurrentWaveActualScore = 0f;
        }

        public void EndWave()
        {
            FinalizedWaveBaseScore = CurrentWaveBaseScore;
            FinalizedWaveActualScore = CurrentWaveActualScore;
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

        private void ResetCombo()
        {
            isComboActive = false;
            comboTimeRemaining = 0f;
            CurrentMultiplier = initialMultiplier;
        }
    }
}
