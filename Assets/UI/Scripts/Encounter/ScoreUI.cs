using TMPro;
using RPGame.Encounter;
using UnityEngine;

namespace RPGame.UI.Encounter
{
    public sealed class ScoreUI : MonoBehaviour
    {
        [SerializeField] private EncounterRuntime runtime;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text remainingEnemiesText;

        private void Update()
        {
            ScoreSystem scoreSystem = runtime != null ? runtime.ScoreSystem : null;
            if (scoreSystem != null && scoreText != null)
            {
                scoreText.text = $"Score: {Mathf.FloorToInt(scoreSystem.CurrentScore)}";
            }

            if (runtime != null && remainingEnemiesText != null)
            {
                remainingEnemiesText.text = $"Enemies: {runtime.RemainingEnemyCount}";
            }
        }
    }
}
