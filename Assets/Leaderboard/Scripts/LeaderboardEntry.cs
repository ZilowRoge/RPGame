namespace RPGame.Leaderboard
{
    public readonly struct LeaderboardEntry
    {
        public LeaderboardEntry(
            int rank,
            string playerId,
            string playerName,
            float score,
            int wavesCompleted,
            bool isCurrentPlayer)
        {
            Rank = rank;
            PlayerId = playerId;
            PlayerName = playerName;
            Score = score;
            WavesCompleted = wavesCompleted;
            IsCurrentPlayer = isCurrentPlayer;
        }

        public int Rank { get; }
        public string PlayerId { get; }
        public string PlayerName { get; }
        public float Score { get; }
        public int WavesCompleted { get; }
        public bool IsCurrentPlayer { get; }
    }
}
