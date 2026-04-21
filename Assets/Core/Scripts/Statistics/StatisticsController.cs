using System;
using UnityEngine;

namespace RPGame.Core.Statistics
{
    public sealed class StatisticsController : MonoBehaviour, IStatisticsController
    {
        [SerializeField] private StatisticsConfig config;
        [SerializeField] private bool initializeOnAwake = true;

        [Header("Regeneration")]
        [SerializeField] private bool regenerateHealth;
        [SerializeField] private bool regenerateStamina = true;

        public event Action<float, float> HealthChanged;
        public event Action<float, float> StaminaChanged;
        public event Action Died;

        public float CurrentHealth { get; private set; }
        public float CurrentStamina { get; private set; }
        public float MaxHealth => config != null ? config.MaxHealth : 0f;
        public float MaxStamina => config != null ? config.MaxStamina : 0f;
        public float HealthRegenerationPerSecond => config != null ? config.HealthRegenerationPerSecond : 0f;
        public float StaminaRegenerationPerSecond => config != null ? config.StaminaRegenerationPerSecond : 0f;
        public float StaminaRegenerationDelay => config != null ? config.StaminaRegenerationDelay : 0f;
        public float HealthNormalized => MaxHealth > 0f ? CurrentHealth / MaxHealth : 0f;
        public float StaminaNormalized => MaxStamina > 0f ? CurrentStamina / MaxStamina : 0f;
        public bool IsAlive => CurrentHealth > 0f;

        private float staminaRegenerationDelayTimer;

        private void Awake()
        {
            if (initializeOnAwake)
            {
                ResetToConfig();
            }
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            Regenerate(deltaTime);
        }

        public void ResetToConfig()
        {
            SetHealth(MaxHealth);
            SetStamina(MaxStamina);
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || !IsAlive)
            {
                return;
            }

            SetHealth(CurrentHealth - amount);
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || !IsAlive)
            {
                return;
            }

            SetHealth(CurrentHealth + amount);
        }

        public bool CanSpendStamina(float amount)
        {
            return amount <= 0f || CurrentStamina >= amount;
        }

        public bool TrySpendStamina(float amount)
        {
            if (amount <= 0f)
            {
                return true;
            }

            if (!CanSpendStamina(amount))
            {
                return false;
            }

            SetStamina(CurrentStamina - amount);
            staminaRegenerationDelayTimer = StaminaRegenerationDelay;
            return true;
        }

        public void RestoreStamina(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            SetStamina(CurrentStamina + amount);
        }

        private void Regenerate(float deltaTime)
        {
            if (config == null || !IsAlive)
            {
                return;
            }

            if (regenerateHealth)
            {
                Heal(config.HealthRegenerationPerSecond * deltaTime);
            }

            if (staminaRegenerationDelayTimer > 0f)
            {
                staminaRegenerationDelayTimer -= deltaTime;
                return;
            }

            if (regenerateStamina)
            {
                RestoreStamina(config.StaminaRegenerationPerSecond * deltaTime);
            }
        }

        private void SetHealth(float value)
        {
            float previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Clamp(value, 0f, MaxHealth);

            if (!Mathf.Approximately(previousHealth, CurrentHealth))
            {
                HealthChanged?.Invoke(CurrentHealth, MaxHealth);
            }

            if (previousHealth > 0f && CurrentHealth <= 0f)
            {
                Died?.Invoke();
            }
        }

        private void SetStamina(float value)
        {
            float previousStamina = CurrentStamina;
            CurrentStamina = Mathf.Clamp(value, 0f, MaxStamina);

            if (!Mathf.Approximately(previousStamina, CurrentStamina))
            {
                StaminaChanged?.Invoke(CurrentStamina, MaxStamina);
            }
        }
    }
}
