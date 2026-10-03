using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RPGame.Encounter;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;
using UgsLeaderboardEntry = Unity.Services.Leaderboards.Models.LeaderboardEntry;

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
        private const int TopScoreLimit = 50;
        private const int PlayerRangeLimit = 5;

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

        public static async Task<IReadOnlyList<LeaderboardEntry>> GetTopScoresAsync()
        {
            await InitializeAsync();
            if (State != LeaderboardServiceState.Ready)
            {
                return Array.Empty<LeaderboardEntry>();
            }

            try
            {
                var response = await LeaderboardsService.Instance.GetScoresAsync(
                    LeaderboardId,
                    new GetScoresOptions
                    {
                        Limit = TopScoreLimit,
                        IncludeMetadata = true
                    });
                return MapEntries(response.Results);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Leaderboard top scores request failed: {exception.Message}");
                return Array.Empty<LeaderboardEntry>();
            }
        }

        public static async Task<LeaderboardEntry?> GetCurrentPlayerScoreAsync()
        {
            await InitializeAsync();
            if (State != LeaderboardServiceState.Ready)
            {
                return null;
            }

            try
            {
                UgsLeaderboardEntry response = await LeaderboardsService.Instance.GetPlayerScoreAsync(
                    LeaderboardId,
                    new GetPlayerScoreOptions
                    {
                        IncludeMetadata = true
                    });
                return MapEntry(response);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Leaderboard player score request failed: {exception.Message}");
                return null;
            }
        }

        public static async Task<IReadOnlyList<LeaderboardEntry>> GetPlayerCenteredScoresAsync()
        {
            await InitializeAsync();
            if (State != LeaderboardServiceState.Ready)
            {
                return Array.Empty<LeaderboardEntry>();
            }

            try
            {
                var response = await LeaderboardsService.Instance.GetPlayerRangeAsync(
                    LeaderboardId,
                    new GetPlayerRangeOptions
                    {
                        RangeLimit = PlayerRangeLimit,
                        IncludeMetadata = true
                    });
                return MapEntries(response.Results);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Leaderboard player range request failed: {exception.Message}");
                return Array.Empty<LeaderboardEntry>();
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

        private static IReadOnlyList<LeaderboardEntry> MapEntries(IEnumerable<UgsLeaderboardEntry> entries)
        {
            List<LeaderboardEntry> mappedEntries = new();
            if (entries == null)
            {
                return mappedEntries;
            }

            foreach (UgsLeaderboardEntry entry in entries)
            {
                mappedEntries.Add(MapEntry(entry));
            }

            return mappedEntries;
        }

        private static LeaderboardEntry MapEntry(UgsLeaderboardEntry entry)
        {
            string entryPlayerId = entry.PlayerId ?? string.Empty;
            return new LeaderboardEntry(
                Math.Max(1, entry.Rank + 1),
                entryPlayerId,
                entry.PlayerName ?? string.Empty,
                (float)entry.Score,
                GetWavesCompleted(entry.Metadata),
                !string.IsNullOrEmpty(PlayerId) && string.Equals(entryPlayerId, PlayerId, StringComparison.Ordinal));
        }

        private static int GetWavesCompleted(string metadata)
        {
            if (string.IsNullOrWhiteSpace(metadata))
            {
                return 0;
            }

            try
            {
                ScoreMetadata scoreMetadata = JsonUtility.FromJson<ScoreMetadata>(metadata);
                return scoreMetadata != null && scoreMetadata.wavesCompleted >= 0
                    ? scoreMetadata.wavesCompleted
                    : 0;
            }
            catch (Exception)
            {
                return 0;
            }
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
