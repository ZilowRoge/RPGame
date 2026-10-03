using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
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
    }
}
