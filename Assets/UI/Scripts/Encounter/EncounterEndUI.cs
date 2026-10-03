using System;
using TMPro;
using RPGame.Encounter;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPGame.UI.Encounter
{
    public sealed class EncounterEndUI : MonoBehaviour
    {
        [SerializeField] private EncounterRuntime runtime;
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text reachedWaveText;
        [SerializeField] private Button restartButton;

        private Action<int> loadScene = sceneBuildIndex => SceneManager.LoadScene(sceneBuildIndex);
        private bool hasDisplayedEnd;

        private void OnEnable()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartCurrentScene);
                restartButton.onClick.AddListener(RestartCurrentScene);
            }
        }

        private void OnDisable()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartCurrentScene);
            }
        }

        private void Update()
        {
            EncounterController encounterController = runtime != null ? runtime.EncounterController : null;
            if (encounterController == null || encounterController.State != EncounterState.Ended)
            {
                hasDisplayedEnd = false;
                SetPanelVisible(false);
                return;
            }

            if (hasDisplayedEnd)
            {
                return;
            }

            SetPanelVisible(true);

            ScoreSystem scoreSystem = runtime.ScoreSystem;
            if (finalScoreText != null && scoreSystem != null)
            {
                finalScoreText.text = $"Final Score: {scoreSystem.CurrentScore}";
            }

            if (reachedWaveText != null)
            {
                reachedWaveText.text = $"Waves Completed: {encounterController.CurrentWaveNumber - 1}";
            }

            hasDisplayedEnd = true;
        }

        public void RestartCurrentScene()
        {
            loadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void SetPanelVisible(bool isVisible)
        {
            if (panel != null && panel.activeSelf != isVisible)
            {
                panel.SetActive(isVisible);
            }
        }
    }
}
