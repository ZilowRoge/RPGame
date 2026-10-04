namespace RPGame.Encounter
{
    public readonly struct EncounterResult
    {
        public EncounterResult(float score, int wavesCompleted, int encounterSeed)
        {
            Score = score;
            WavesCompleted = wavesCompleted;
            EncounterSeed = encounterSeed;
        }

        public float Score { get; }
        public int WavesCompleted { get; }
        public int EncounterSeed { get; }
    }
}
