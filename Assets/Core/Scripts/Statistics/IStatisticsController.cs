using System;

namespace RPGame.Core.Statistics
{
    public interface IStatisticsController
    {
        event Action<float, float> HealthChanged;
        event Action<float, float> StaminaChanged;
        event Action Died;

        float CurrentHealth { get; }
        float CurrentStamina { get; }
        float MaxHealth { get; }
        float MaxStamina { get; }
        float HealthRegenerationPerSecond { get; }
        float StaminaRegenerationPerSecond { get; }
        float StaminaRegenerationDelay { get; }
        float HealthNormalized { get; }
        float StaminaNormalized { get; }
        bool IsAlive { get; }

        void TakeDamage(float amount);
        void Heal(float amount);
        bool CanSpendStamina(float amount);
        bool TrySpendStamina(float amount);
        void RestoreStamina(float amount);
    }
}
