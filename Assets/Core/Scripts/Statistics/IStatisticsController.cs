using System;

namespace RPGame.Core.Statistics
{
    public interface IStatisticsController
    {
        event Action<float, float> HealthChanged;
        event Action<float, float> StaminaChanged;
        event Action<float, float> OnManaChanged;
        event Action Died;

        float CurrentHealth { get; }
        float CurrentStamina { get; }
        float CurrentMana { get; }
        float MaxHealth { get; }
        float MaxStamina { get; }
        float MaxMana { get; }
        float HealthRegenerationPerSecond { get; }
        float StaminaRegenerationPerSecond { get; }
        float StaminaRegenerationDelay { get; }
        float ManaRegenerationPerSecond { get; }
        float ManaRegenerationDelay { get; }
        float HealthNormalized { get; }
        float StaminaNormalized { get; }
        float ManaNormalized { get; }
        bool IsAlive { get; }

        void TakeDamage(float amount);
        void Heal(float amount);
        bool CanSpendStamina(float amount);
        bool TrySpendStamina(float amount);
        void RestoreStamina(float amount);
        bool CanSpendMana(float amount);
        bool TrySpendMana(float amount);
        void RestoreMana(float amount);
    }
}
