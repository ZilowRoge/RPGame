using System;
using RPGame.Encounter;
using Unity.Services.Leaderboards;
using UnityEngine;

namespace RPGame.Leaderboard
{
    [AddComponentMenu("Debug/Leaderboard Debug Tester")]
    public sealed class LeaderboardDebugTester : MonoBehaviour
    {
        private const string LeaderboardId = "endless_wave_leaderboard";

        [SerializeField] private string testPlayerName = "TestPlayer";
        [SerializeField] private float testScore = 12345f;
        [SerializeField] private int testWavesCompleted = 7;
        [SerializeField] private int testSeed = 123;

        private async void Start()
        {
            await LeaderboardService.InitializeAsync();
            Debug.Log($"Leaderboard state: {LeaderboardService.State}", this);
            Debug.Log($"Leaderboard PlayerId: {LeaderboardService.PlayerId}", this);
            Debug.Log($"Leaderboard PlayerName: {LeaderboardService.PlayerName}", this);

            if (LeaderboardService.State != LeaderboardServiceState.Ready)
            {
                Debug.LogError("Leaderboard service is not ready. Smoke test stopped.", this);
                return;
            }

            await LeaderboardService.UpdatePlayerNameAsync(testPlayerName);
            Debug.Log($"Leaderboard PlayerName after update: {LeaderboardService.PlayerName}", this);

            EncounterResult result = new(testScore, testWavesCompleted, testSeed);
            await LeaderboardService.SubmitScoreAsync(result);
            Debug.Log("Cloud Code leaderboard submission request completed.", this);

            var entries = await LeaderboardService.GetTopScoresAsync();
            foreach (LeaderboardEntry entry in entries)
            {
                Debug.Log(
                    $"{entry.Rank} | {entry.PlayerName} | {entry.WavesCompleted} | {entry.Score} | CurrentPlayer: {entry.IsCurrentPlayer}",
                    this);
            }

            try
            {
                await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, 99999999f);
                Debug.LogError("Direct leaderboard write was NOT blocked", this);
            }
            catch (Exception exception)
            {
                Debug.Log(
                    $"Direct leaderboard write correctly blocked: {exception.GetType().Name}: {exception.Message}",
                    this);
            }
        }
    }
}
