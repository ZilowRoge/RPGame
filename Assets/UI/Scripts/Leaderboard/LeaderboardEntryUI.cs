using RPGame.Leaderboard;
using TMPro;
using UnityEngine;

namespace RPGame.UI.Leaderboard
{
    public sealed class LeaderboardEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text wavesCompletedText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private GameObject currentPlayerHighlight;

        public void Populate(LeaderboardEntry entry)
        {
            rankText.text = entry.Rank.ToString();
            playerNameText.text = entry.PlayerName;
            wavesCompletedText.text = entry.WavesCompleted.ToString();
            scoreText.text = entry.Score.ToString("N0");

            if (currentPlayerHighlight != null)
            {
                currentPlayerHighlight.SetActive(entry.IsCurrentPlayer);
            }

            gameObject.SetActive(true);
        }

        public void ResetRow()
        {
            rankText.text = string.Empty;
            playerNameText.text = string.Empty;
            wavesCompletedText.text = string.Empty;
            scoreText.text = string.Empty;

            if (currentPlayerHighlight != null)
            {
                currentPlayerHighlight.SetActive(false);
            }

            gameObject.SetActive(false);
        }
    }
}
