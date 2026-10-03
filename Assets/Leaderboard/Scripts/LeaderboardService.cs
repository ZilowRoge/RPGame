using System;
using System.Threading.Tasks;
using RPGame.Encounter;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;

namespace RPGame.Leaderboard
{
    public enum LeaderboardServiceState
    {
        NotInitialized,
        Initializing,
        Ready,
        Failed
    }

    public static class LeaderboardService
    {
        private const string LeaderboardId = "endless_wave_leaderboard";

        private static Task initializationTask;

        public static LeaderboardServiceState State { get; private set; } = LeaderboardServiceState.NotInitialized;
        public static string PlayerId { get; private set; }
        public static string PlayerName { get; private set; }

        public static Task InitializeAsync()
        {
            if (State == LeaderboardServiceState.Ready || State == LeaderboardServiceState.Failed)
            {
                return Task.CompletedTask;
            }

            if (initializationTask != null)
            {
                return initializationTask;
            }

            initializationTask = InitializeInternalAsync();
            return initializationTask;
        }

        public static async Task RefreshPlayerNameAsync()
        {
            await InitializeAsync();
            if (State != LeaderboardServiceState.Ready)
            {
                return;
            }

            try
            {
                PlayerName = await AuthenticationService.Instance.GetPlayerNameAsync();
                PlayerId = AuthenticationService.Instance.PlayerId;
            }
            catch (Exception exception)
            {
                SetFailed("refreshing player name", exception);
            }
        }

        public static async Task UpdatePlayerNameAsync(string playerName)
        {
            await InitializeAsync();
            if (State != LeaderboardServiceState.Ready)
            {
                return;
            }

            try
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
                PlayerName = AuthenticationService.Instance.PlayerName;
                PlayerId = AuthenticationService.Instance.PlayerId;
            }
            catch (Exception exception)
            {
                SetFailed("updating player name", exception);
            }
        }

        public static async Task SubmitScoreAsync(EncounterResult result)
        {
            if (!IsValid(result))
            {
                Debug.LogWarning("Leaderboard score submission skipped: invalid run result.");
                return;
            }

            await InitializeAsync();
            if (State != LeaderboardServiceState.Ready)
            {
                return;
            }

            try
            {
                await LeaderboardsService.Instance.AddPlayerScoreAsync(
                    LeaderboardId,
                    result.Score,
                    new AddPlayerScoreOptions
                    {
                        Metadata = new ScoreMetadata
                        {
                            wavesCompleted = result.WavesCompleted,
                            encounterSeed = result.EncounterSeed,
                            gameVersion = Application.version
                        }
                    });
            }
            catch (Exception exception)
            {
                Debug.LogError($"Leaderboard score submission failed: {exception.Message}");
            }
        }

        private static async Task InitializeInternalAsync()
        {
            State = LeaderboardServiceState.Initializing;

            try
            {
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                PlayerId = AuthenticationService.Instance.PlayerId;
                PlayerName = await AuthenticationService.Instance.GetPlayerNameAsync();
                State = LeaderboardServiceState.Ready;
            }
            catch (Exception exception)
            {
                SetFailed("initializing", exception);
            }
        }

        private static void SetFailed(string operation, Exception exception)
        {
            State = LeaderboardServiceState.Failed;
            Debug.LogError($"Leaderboard service failed while {operation}: {exception.Message}");
        }

        private static bool IsValid(EncounterResult result)
        {
            return result.Score >= 0f
                && !float.IsNaN(result.Score)
                && !float.IsInfinity(result.Score)
                && result.WavesCompleted >= 0;
        }

        [Serializable]
        private sealed class ScoreMetadata
        {
            public int wavesCompleted;
            public int encounterSeed;
            public string gameVersion;
        }
    }
}
