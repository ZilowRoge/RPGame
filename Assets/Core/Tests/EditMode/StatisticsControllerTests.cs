using NUnit.Framework;
using RPGame.Core.Statistics;
using UnityEditor;
using UnityEngine;

namespace RPGame.Core.Tests
{
    public sealed class StatisticsControllerTests
    {
        private GameObject gameObject;
        private StatisticsConfig config;
        private StatisticsController controller;

        [SetUp]
        public void SetUp()
        {
            config = CreateConfig(
                maxHealth: 100f,
                maxStamina: 50f,
                healthRegenerationPerSecond: 5f,
                staminaRegenerationPerSecond: 10f,
                staminaRegenerationDelay: 0.5f);

            gameObject = new GameObject("StatisticsControllerTests");
            controller = gameObject.AddComponent<StatisticsController>();
            SetControllerConfig(controller, config);
            controller.ResetToConfig();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void ResetToConfig_SetsCurrentValuesToMax()
        {
            Assert.AreEqual(100f, controller.CurrentHealth);
            Assert.AreEqual(50f, controller.CurrentStamina);
            Assert.AreEqual(1f, controller.HealthNormalized);
            Assert.AreEqual(1f, controller.StaminaNormalized);
        }

        [Test]
        public void TakeDamage_ReducesHealthAndClampsAtZero()
        {
            controller.TakeDamage(25f);
            Assert.AreEqual(75f, controller.CurrentHealth);

            controller.TakeDamage(500f);
            Assert.AreEqual(0f, controller.CurrentHealth);
            Assert.IsFalse(controller.IsAlive);
        }

        [Test]
        public void Heal_IncreasesHealthAndClampsAtMax()
        {
            controller.TakeDamage(40f);
            controller.Heal(15f);

            Assert.AreEqual(75f, controller.CurrentHealth);

            controller.Heal(500f);
            Assert.AreEqual(100f, controller.CurrentHealth);
        }

        [Test]
        public void TrySpendStamina_WhenEnoughStamina_ReducesStaminaAndReturnsTrue()
        {
            bool spent = controller.TrySpendStamina(20f);

            Assert.IsTrue(spent);
            Assert.AreEqual(30f, controller.CurrentStamina);
        }

        [Test]
        public void TrySpendStamina_WhenNotEnoughStamina_DoesNotChangeStaminaAndReturnsFalse()
        {
            bool spent = controller.TrySpendStamina(60f);

            Assert.IsFalse(spent);
            Assert.AreEqual(50f, controller.CurrentStamina);
        }

        [Test]
        public void TrySpendStamina_WhenAmountIsNotPositive_DoesNotChangeStaminaAndReturnsTrue()
        {
            bool spent = controller.TrySpendStamina(-10f);

            Assert.IsTrue(spent);
            Assert.AreEqual(50f, controller.CurrentStamina);
        }

        [Test]
        public void RestoreStamina_IncreasesStaminaAndClampsAtMax()
        {
            controller.TrySpendStamina(30f);
            controller.RestoreStamina(15f);

            Assert.AreEqual(35f, controller.CurrentStamina);

            controller.RestoreStamina(500f);
            Assert.AreEqual(50f, controller.CurrentStamina);
        }

        [Test]
        public void Died_IsRaisedOnceWhenHealthReachesZero()
        {
            int diedCount = 0;
            controller.Died += () => diedCount++;

            controller.TakeDamage(100f);
            controller.TakeDamage(100f);

            Assert.AreEqual(1, diedCount);
        }

        [Test]
        public void TrySpendStamina_DelaysStaminaRegeneration()
        {
            controller.TrySpendStamina(20f);

            controller.Tick(0.25f);
            Assert.AreEqual(30f, controller.CurrentStamina);

            controller.Tick(0.25f);
            Assert.AreEqual(30f, controller.CurrentStamina);

            controller.Tick(1f);
            Assert.AreEqual(40f, controller.CurrentStamina);
        }

        [Test]
        public void HealthRegeneration_RestoresHealthOverTime()
        {
            SetRegeneration(controller, regenerateHealth: true, regenerateStamina: false);

            controller.TakeDamage(20f);
            controller.Tick(2f);

            Assert.AreEqual(90f, controller.CurrentHealth);
        }

        private static StatisticsConfig CreateConfig(
            float maxHealth,
            float maxStamina,
            float healthRegenerationPerSecond,
            float staminaRegenerationPerSecond,
            float staminaRegenerationDelay)
        {
            StatisticsConfig statisticsConfig = ScriptableObject.CreateInstance<StatisticsConfig>();
            SerializedObject serializedConfig = new SerializedObject(statisticsConfig);
            serializedConfig.FindProperty("maxHealth").floatValue = maxHealth;
            serializedConfig.FindProperty("maxStamina").floatValue = maxStamina;
            serializedConfig.FindProperty("healthRegenerationPerSecond").floatValue = healthRegenerationPerSecond;
            serializedConfig.FindProperty("staminaRegenerationPerSecond").floatValue = staminaRegenerationPerSecond;
            serializedConfig.FindProperty("staminaRegenerationDelay").floatValue = staminaRegenerationDelay;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return statisticsConfig;
        }

        private static void SetControllerConfig(StatisticsController statisticsController, StatisticsConfig statisticsConfig)
        {
            SerializedObject serializedController = new SerializedObject(statisticsController);
            serializedController.FindProperty("config").objectReferenceValue = statisticsConfig;
            serializedController.FindProperty("initializeOnAwake").boolValue = false;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetRegeneration(StatisticsController statisticsController, bool regenerateHealth, bool regenerateStamina)
        {
            SerializedObject serializedController = new SerializedObject(statisticsController);
            serializedController.FindProperty("regenerateHealth").boolValue = regenerateHealth;
            serializedController.FindProperty("regenerateStamina").boolValue = regenerateStamina;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
