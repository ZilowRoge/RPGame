using RPGame.Core.Statistics;
using UnityEngine;
using UnityEngine.UI;

namespace RPGame.UI.Statistics
{
    public sealed class StatisticsBarsUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StatisticsController statisticsController;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image staminaFill;
        [SerializeField] private Text healthValueText;
        [SerializeField] private Text staminaValueText;

        private IStatisticsController statistics;

        private void Awake()
        {
            ResolveStatisticsController();
        }

        private void OnEnable()
        {
            ResolveStatisticsController();
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void SetStatisticsController(StatisticsController controller)
        {
            if (statisticsController == controller)
            {
                return;
            }

            Unsubscribe();
            statisticsController = controller;
            statistics = statisticsController;
            Subscribe();
            Refresh();
        }

        private void ResolveStatisticsController()
        {
            if (statisticsController == null)
            {
                statisticsController = FindAnyObjectByType<StatisticsController>();
            }

            statistics = statisticsController;
        }

        private void Subscribe()
        {
            if (statistics == null)
            {
                return;
            }

            statistics.HealthChanged += OnHealthChanged;
            statistics.StaminaChanged += OnStaminaChanged;
        }

        private void Unsubscribe()
        {
            if (statistics == null)
            {
                return;
            }

            statistics.HealthChanged -= OnHealthChanged;
            statistics.StaminaChanged -= OnStaminaChanged;
        }

        private void Refresh()
        {
            if (statistics == null)
            {
                SetFillAmount(healthFill, 0f);
                SetFillAmount(staminaFill, 0f);
                SetValueText(healthValueText, 0f, 0f);
                SetValueText(staminaValueText, 0f, 0f);
                return;
            }

            SetFillAmount(healthFill, statistics.HealthNormalized);
            SetFillAmount(staminaFill, statistics.StaminaNormalized);
            SetValueText(healthValueText, statistics.CurrentHealth, statistics.MaxHealth);
            SetValueText(staminaValueText, statistics.CurrentStamina, statistics.MaxStamina);
        }

        private void OnHealthChanged(float currentHealth, float maxHealth)
        {
            SetFillAmount(healthFill, GetNormalizedValue(currentHealth, maxHealth));
            SetValueText(healthValueText, currentHealth, maxHealth);
        }

        private void OnStaminaChanged(float currentStamina, float maxStamina)
        {
            SetFillAmount(staminaFill, GetNormalizedValue(currentStamina, maxStamina));
            SetValueText(staminaValueText, currentStamina, maxStamina);
        }

        private static float GetNormalizedValue(float currentValue, float maxValue)
        {
            return maxValue > 0f ? Mathf.Clamp01(currentValue / maxValue) : 0f;
        }

        private static void SetFillAmount(Image image, float value)
        {
            if (image != null)
            {
                image.fillAmount = Mathf.Clamp01(value);
            }
        }

        private static void SetValueText(Text text, float currentValue, float maxValue)
        {
            if (text != null)
            {
                text.text = $"{currentValue:0} / {maxValue:0}";
            }
        }

        private void OnValidate()
        {
            if (healthFill != null)
            {
                healthFill.type = Image.Type.Filled;
                healthFill.fillMethod = Image.FillMethod.Horizontal;
            }

            if (staminaFill != null)
            {
                staminaFill.type = Image.Type.Filled;
                staminaFill.fillMethod = Image.FillMethod.Horizontal;
            }
        }
    }
}
