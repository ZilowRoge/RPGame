using System;
using RPGame.Core.Damage;
using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Combat.Damage
{
    public sealed class DamageReceiver : MonoBehaviour, IDamageable
    {
        [SerializeField] private StatisticsController statisticsController;

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

            DamageReceived?.Invoke(result);
            return result;
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
