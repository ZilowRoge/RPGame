using System;
using System.Text;
using RPGame.Core.Damage;
using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Combat.Damage
{
    public sealed class DamageReceiver : MonoBehaviour, IDamageable
    {
        [SerializeField] private StatisticsController statisticsController;
        [SerializeField] private bool loggingEnabled;

        public event Action<DamageResult> DamageReceived;

        public bool CanReceiveDamage => ResolveStatisticsController() != null && statisticsController.IsAlive;
        public IStatisticsController Statistics => ResolveStatisticsController();

        private void Awake()
        {
            ResolveStatisticsController();
        }

        public DamageResult ApplyDamage(DamageData data)
        {
            if (!CanReceiveDamage || !data.HasDamage)
            {
                return DamageResult.Ignored(data, GetCurrentHealth());
            }

            float previousHealth = statisticsController.CurrentHealth;
            statisticsController.TakeDamage(data.Amount);
            float currentHealth = statisticsController.CurrentHealth;
            float appliedAmount = Mathf.Max(0f, previousHealth - currentHealth);

            if (appliedAmount <= 0f)
            {
                return DamageResult.Ignored(data, currentHealth);
            }

            DamageResult result = DamageResult.Applied(
                data,
                appliedAmount,
                previousHealth,
                currentHealth,
                !statisticsController.IsAlive);

            LogDamageReceived(result);
            DamageReceived?.Invoke(result);
            return result;
        }

        private void LogDamageReceived(DamageResult result)
        {
            if (loggingEnabled) 
            {
                Debug.Log(
                    $"{name} received {result.AppliedAmount:0.#} damage ({FormatDamageParts(result.Data)}).",
                    this);
            }
        }

        private static string FormatDamageParts(DamageData data)
        {
            if (data.Parts == null || data.Parts.Count == 0)
            {
                return "no damage parts";
            }

            StringBuilder builder = new();
            for (int i = 0; i < data.Parts.Count; i++)
            {
                PartialDamage part = data.Parts[i];
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(part.Amount.ToString("0.#"));
                builder.Append(' ');
                builder.Append(part.DamageType);

                if (part.DamageElement != DamageElement.None)
                {
                    builder.Append(' ');
                    builder.Append(part.DamageElement);
                }
            }

            return builder.ToString();
        }

        private float GetCurrentHealth()
        {
            StatisticsController resolvedStatisticsController = ResolveStatisticsController();
            return resolvedStatisticsController != null ? resolvedStatisticsController.CurrentHealth : 0f;
        }

        private StatisticsController ResolveStatisticsController()
        {
            if (statisticsController == null)
            {
                statisticsController = GetComponentInParent<StatisticsController>();
            }

            return statisticsController;
        }
    }
}
