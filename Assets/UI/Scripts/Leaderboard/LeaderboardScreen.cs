using System.Collections.Generic;
using System.Threading.Tasks;
using RPGame.Leaderboard;
using UnityEngine;

namespace RPGame.UI.Leaderboard
{
    public sealed class LeaderboardScreen : MonoBehaviour
    {
        [SerializeField] private LeaderboardEntryUI[] topScoreRows;
        [SerializeField] private LeaderboardEntryUI[] playerCenteredRows;
        [SerializeField] private GameObject playerCenteredSection;

        private bool isLoading;

        private void OnEnable()
        {
            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            if (isLoading)
            {
                return;
            }

            isLoading = true;
            ResetRows();

            try
            {
                IReadOnlyList<LeaderboardEntry> topScores = await LeaderboardService.GetTopScoresAsync();
                if (!CanUpdateUI())
                {
                    return;
                }

                PopulateRows(topScoreRows, topScores);

                if (ContainsCurrentPlayer(topScores))
                {
                    return;
                }

                if (playerCenteredSection != null)
                {
                    playerCenteredSection.SetActive(true);
                }

                IReadOnlyList<LeaderboardEntry> playerCenteredScores =
                    await LeaderboardService.GetPlayerCenteredScoresAsync();
                if (!CanUpdateUI())
                {
                    return;
                }

                PopulateRows(playerCenteredRows, playerCenteredScores);
            }
            finally
            {
                if (this != null)
                {
                    isLoading = false;
                }
            }
        }

        private void ResetRows()
        {
            ResetRows(topScoreRows);
            ResetRows(playerCenteredRows);

            if (playerCenteredSection != null)
            {
                playerCenteredSection.SetActive(false);
            }
        }

        private static void ResetRows(LeaderboardEntryUI[] rows)
        {
            if (rows == null)
            {
                return;
            }

            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] != null)
                {
                    rows[i].ResetRow();
                }
            }
        }

        private static void PopulateRows(
            LeaderboardEntryUI[] rows,
            IReadOnlyList<LeaderboardEntry> entries)
        {
            if (rows == null || entries == null)
            {
                return;
            }

            int entryIndex = 0;
            for (int rowIndex = 0; rowIndex < rows.Length && entryIndex < entries.Count; rowIndex++)
            {
                if (rows[rowIndex] == null)
                {
                    continue;
                }

                rows[rowIndex].Populate(entries[entryIndex]);
                entryIndex++;
            }
        }

        private static bool ContainsCurrentPlayer(IReadOnlyList<LeaderboardEntry> entries)
        {
            if (entries == null)
            {
                return false;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].IsCurrentPlayer)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanUpdateUI()
        {
            return this != null && isActiveAndEnabled;
        }
    }
}
