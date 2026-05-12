using NUnit.Framework;
using RPGame.Combat.Damage;
using RPGame.Core.Damage;
using RPGame.Core.Statistics;
using UnityEditor;
using UnityEngine;

namespace RPGame.Combat.Tests
{
    public sealed class DamageReceiverTests
    {
        private GameObject gameObject;
        private StatisticsConfig config;
        private StatisticsController statisticsController;
        private DamageReceiver damageReceiver;

        [SetUp]
        public void SetUp()
        {
            config = CreateConfig(100f);
            gameObject = new GameObject("DamageReceiverTests");
            statisticsController = gameObject.AddComponent<StatisticsController>();
            SetControllerConfig(statisticsController, config);
            statisticsController.ResetToConfig();
            damageReceiver = gameObject.AddComponent<DamageReceiver>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
            Object.DestroyImmediate(config);
        }

        [Test]
        public void ApplyDamage_ReducesStatisticsHealth()
        {
            DamageData data = CreateData(25f);

            DamageResult result = damageReceiver.ApplyDamage(data);

            Assert.IsTrue(result.WasApplied);
            Assert.AreEqual(25f, result.AppliedAmount);
            Assert.AreEqual(75f, statisticsController.CurrentHealth);
        }

        [Test]
        public void ApplyDamage_RaisesDamageReceived()
        {
            int receivedCount = 0;
            DamageResult receivedResult = default;
            damageReceiver.DamageReceived += result =>
            {
                receivedCount++;
                receivedResult = result;
            };

            damageReceiver.ApplyDamage(CreateData(10f));

            Assert.AreEqual(1, receivedCount);
            Assert.AreEqual(10f, receivedResult.AppliedAmount);
        }

        [Test]
        public void ApplyDamage_WhenHealthReachesZero_ReturnsFatalResult()
        {
            DamageResult result = damageReceiver.ApplyDamage(CreateData(150f));

            Assert.IsTrue(result.WasFatal);
            Assert.AreEqual(100f, result.AppliedAmount);
            Assert.AreEqual(0f, statisticsController.CurrentHealth);
        }

        [Test]
        public void ApplyDamage_WhenDataHasNoDamage_IsIgnored()
        {
            DamageResult result = damageReceiver.ApplyDamage(CreateData(0f));

            Assert.IsFalse(result.WasApplied);
            Assert.AreEqual(100f, statisticsController.CurrentHealth);
        }

        [Test]
        public void ApplyDamage_WhenDataHasSource_PreservesSourceInResult()
        {
            GameObject source = new GameObject("DamageSource");

            try
            {
                DamageResult result = damageReceiver.ApplyDamage(CreateData(10f, source));

                Assert.AreSame(source, result.Data.Source);
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void ApplyDamage_WhenTargetIsDead_IsIgnored()
        {
            damageReceiver.ApplyDamage(CreateData(100f));

            DamageResult result = damageReceiver.ApplyDamage(CreateData(10f));

            Assert.IsFalse(result.WasApplied);
            Assert.AreEqual(0f, statisticsController.CurrentHealth);
        }

        [Test]
        public void ApplyDamage_WhenDataHasNoDamage_DoesNotRaiseDamageReceived()
        {
            int receivedCount = 0;
            damageReceiver.DamageReceived += _ => receivedCount++;

            damageReceiver.ApplyDamage(CreateData(0f));

            Assert.AreEqual(0, receivedCount);
        }

        private static DamageData CreateData(float amount, GameObject source = null)
        {
            PartialDamage[] damage =
            {
                new PartialDamage(amount, DamageType.Physical, DamageElement.None)
            };

            return new DamageData(damage, source);
        }

        private static StatisticsConfig CreateConfig(float maxHealth)
        {
            StatisticsConfig statisticsConfig = ScriptableObject.CreateInstance<StatisticsConfig>();
            SerializedObject serializedConfig = new SerializedObject(statisticsConfig);
            serializedConfig.FindProperty("maxHealth").floatValue = maxHealth;
            serializedConfig.FindProperty("maxStamina").floatValue = 50f;
            serializedConfig.FindProperty("maxMana").floatValue = 50f;
            serializedConfig.FindProperty("healthRegenerationPerSecond").floatValue = 0f;
            serializedConfig.FindProperty("staminaRegenerationPerSecond").floatValue = 0f;
            serializedConfig.FindProperty("staminaRegenerationDelay").floatValue = 0f;
            serializedConfig.FindProperty("manaRegenerationPerSecond").floatValue = 0f;
            serializedConfig.FindProperty("manaRegenerationDelay").floatValue = 0f;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return statisticsConfig;
        }

        private static void SetControllerConfig(StatisticsController controller, StatisticsConfig statisticsConfig)
        {
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("config").objectReferenceValue = statisticsConfig;
            serializedController.FindProperty("initializeOnAwake").boolValue = false;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
