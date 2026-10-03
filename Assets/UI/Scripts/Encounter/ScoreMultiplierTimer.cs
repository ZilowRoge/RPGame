using System.Globalization;
using TMPro;
using RPGame.Encounter;
using UnityEngine;
using UnityEngine.UI;

namespace RPGame.UI.Encounter
{
    public sealed class ScoreMultiplierTimer : MonoBehaviour
    {
        [SerializeField] private EncounterRuntime runtime;
        [SerializeField] private TMP_Text multiplierText;
        [SerializeField] private Image timerImage;
        [SerializeField] private CanvasGroup canvasGroup;

        private void Update()
        {
            ScoreSystem scoreSystem = runtime != null ? runtime.ScoreSystem : null;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = scoreSystem != null && scoreSystem.ComboTimeNormalized > 0f ? 1f : 0f;
            }

            if (scoreSystem == null)
            {
                return;
            }

            if (multiplierText != null)
            {
                multiplierText.text = $"x{scoreSystem.CurrentMultiplier.ToString("0.0", CultureInfo.InvariantCulture)}";
            }

            if (timerImage != null)
            {
                timerImage.fillAmount = scoreSystem.ComboTimeNormalized;
            }
        }
    }
}
