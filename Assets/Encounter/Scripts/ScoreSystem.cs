namespace RPGame.Encounter
{
    public sealed class ScoreSystem
    {
        public int CurrentScore { get; private set; }

        public void RegisterKill(int enemyCost)
        {
            if (enemyCost <= 0)
            {
                return;
            }

            CurrentScore += enemyCost;
        }
    }
}
