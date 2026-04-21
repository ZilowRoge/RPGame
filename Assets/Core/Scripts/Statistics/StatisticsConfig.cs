using UnityEngine;

namespace RPGame.Core.Statistics
{
    [CreateAssetMenu(fileName = "StatisticsConfig", menuName = "RPGame/Statistics/Statistics Config")]
    public sealed class StatisticsConfig : ScriptableObject
    {
        [Header("Vitals")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxStamina = 100f;

        [Header("Regeneration")]
        [SerializeField] private float healthRegenerationPerSecond;
        [SerializeField] private float staminaRegenerationPerSecond = 15f;
        [SerializeField] private float staminaRegenerationDelay = 0.75f;

        public float MaxHealth => maxHealth;
        public float MaxStamina => maxStamina;
        public float HealthRegenerationPerSecond => healthRegenerationPerSecond;
        public float StaminaRegenerationPerSecond => staminaRegenerationPerSecond;
        public float StaminaRegenerationDelay => staminaRegenerationDelay;

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
            maxStamina = Mathf.Max(0f, maxStamina);
            healthRegenerationPerSecond = Mathf.Max(0f, healthRegenerationPerSecond);
            staminaRegenerationPerSecond = Mathf.Max(0f, staminaRegenerationPerSecond);
            staminaRegenerationDelay = Mathf.Max(0f, staminaRegenerationDelay);
        }
    }
}
