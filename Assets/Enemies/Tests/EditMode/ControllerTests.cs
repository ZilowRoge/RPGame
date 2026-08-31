using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RPGame.Combat.Damage;
using RPGame.Core.Damage;
using RPGame.Core.Statistics;
using RPGame.Core.Targeting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace RPGame.Enemies.Tests
{
    public sealed class ControllerTests
    {
        private readonly List<GameObject> createdObjects = new();
        private readonly List<ScriptableObject> createdAssets = new();

        private GameObject enemyObject;
        private Detection detection;
        private Attack attack;
        private RPGame.Enemies.Controller controller;

        [SetUp]
        public void SetUp()
        {
            ClearTargetRegistry();

            enemyObject = CreateObject("Enemy");
            enemyObject.transform.position = Vector3.zero;
            enemyObject.AddComponent<NavMeshAgent>();
            detection = enemyObject.AddComponent<Detection>();
            controller = enemyObject.AddComponent<RPGame.Enemies.Controller>();
            attack = enemyObject.GetComponent<Attack>();
            ConfigureAttack(attack, 1.5f, 0.1f, 10f);
            InvokeStart(controller);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(createdObjects[i]);
            }

            for (int i = createdAssets.Count - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(createdAssets[i]);
            }

            createdObjects.Clear();
            createdAssets.Clear();
            ClearTargetRegistry();
        }

        [Test]
        public void Tick_WhenTargetIsInAttackRange_TriesAttack()
        {
            TargetFixture target = CreateDamageableTarget("Target", new Vector3(1f, 0f, 0f));
            detection.RefreshDetection();

            controller.Tick();

            Assert.AreEqual(90f, target.Statistics.CurrentHealth);
        }

        [Test]
        public void Tick_WhenTargetIsOutsideAttackRange_DoesNotAttack()
        {
            TargetFixture target = CreateDamageableTarget("Target", new Vector3(3f, 0f, 0f));
            detection.RefreshDetection();

            controller.Tick();

            Assert.AreEqual(100f, target.Statistics.CurrentHealth);
        }

        [Test]
        public void Tick_WhenTargetIsLost_DoesNotAttack()
        {
            TargetFixture target = CreateDamageableTarget("Target", new Vector3(1f, 0f, 0f));
            detection.RefreshDetection();

            target.Targetable.gameObject.SetActive(false);
            detection.RefreshDetection();
            controller.Tick();

            Assert.AreEqual(100f, target.Statistics.CurrentHealth);
        }

        [Test]
        public void Controller_DoesNotStoreAttackCooldown()
        {
            bool hasCooldownField = typeof(RPGame.Enemies.Controller)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(field => field.FieldType == typeof(float) || field.Name.ToLowerInvariant().Contains("cooldown"));

            Assert.IsFalse(hasCooldownField);
        }

        [Test]
        public void Controller_RequiresCoreEnemyComponents()
        {
            Assert.IsTrue(RequiresComponent<StatisticsController>());
            Assert.IsTrue(RequiresComponent<Detection>());
            Assert.IsTrue(RequiresComponent<Movement>());
            Assert.IsTrue(RequiresComponent<Attack>());
            Assert.IsTrue(RequiresComponent<EnemyTargetable>());
            Assert.IsTrue(RequiresComponent<DamageReceiver>());
            Assert.IsTrue(RequiresComponent<Death>());
        }

        [Test]
        public void Controller_DoesNotReferencePlayerDamageOrNavMeshLogic()
        {
            bool referencesForbiddenAssembly = typeof(RPGame.Enemies.Controller).Assembly
                .GetReferencedAssemblies()
                .Any(assemblyName => assemblyName.Name == "RPGame.Player" || assemblyName.Name == "RPGame.Loot");

            bool hasForbiddenField = typeof(RPGame.Enemies.Controller)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(field =>
                    field.FieldType == typeof(DamageData)
                    || field.FieldType == typeof(NavMeshAgent)
                    || field.FieldType.Namespace == "UnityEngine.AI");

            Assert.IsFalse(referencesForbiddenAssembly);
            Assert.IsFalse(hasForbiddenField);
        }

        private TargetFixture CreateDamageableTarget(string objectName, Vector3 position)
        {
            GameObject targetObject = CreateObject(objectName);
            targetObject.transform.position = position;

            StatisticsConfig config = CreateConfig(100f);
            StatisticsController statistics = targetObject.AddComponent<StatisticsController>();
            SetControllerConfig(statistics, config);
            statistics.ResetToConfig();

            DamageReceiver damageReceiver = targetObject.AddComponent<DamageReceiver>();
            SetDamageReceiverLogging(damageReceiver, false);

            PlayerTargetable targetable = targetObject.AddComponent<PlayerTargetable>();
            return new TargetFixture(targetable, statistics);
        }

        private GameObject CreateObject(string objectName)
        {
            GameObject gameObject = new(objectName);
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private StatisticsConfig CreateConfig(float maxHealth)
        {
            StatisticsConfig statisticsConfig = ScriptableObject.CreateInstance<StatisticsConfig>();
            createdAssets.Add(statisticsConfig);

            SerializedObject serializedConfig = new(statisticsConfig);
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

        private static void ConfigureAttack(Attack attack, float attackRange, float attackInterval, float damageAmount)
        {
            SerializedObject serializedAttack = new(attack);
            serializedAttack.FindProperty("attackRange").floatValue = attackRange;
            serializedAttack.FindProperty("attackInterval").floatValue = attackInterval;

            SerializedProperty damageProperty = serializedAttack.FindProperty("damage");
            damageProperty.arraySize = 1;
            SerializedProperty damageEntry = damageProperty.GetArrayElementAtIndex(0);
            damageEntry.FindPropertyRelative("minDamage").floatValue = damageAmount;
            damageEntry.FindPropertyRelative("maxDamage").floatValue = damageAmount;
            damageEntry.FindPropertyRelative("damageType").enumValueIndex = (int)DamageType.Physical;
            damageEntry.FindPropertyRelative("damageElement").enumValueIndex = (int)DamageElement.None;

            serializedAttack.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetControllerConfig(StatisticsController controller, StatisticsConfig statisticsConfig)
        {
            SerializedObject serializedController = new(controller);
            serializedController.FindProperty("config").objectReferenceValue = statisticsConfig;
            serializedController.FindProperty("initializeOnAwake").boolValue = false;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetDamageReceiverLogging(DamageReceiver damageReceiver, bool loggingEnabled)
        {
            SerializedObject serializedReceiver = new(damageReceiver);
            serializedReceiver.FindProperty("loggingEnabled").boolValue = loggingEnabled;
            serializedReceiver.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ClearTargetRegistry()
        {
            MethodInfo method = typeof(TargetRegistry).GetMethod("Clear", BindingFlags.Static | BindingFlags.NonPublic);
            method.Invoke(null, null);
        }

        private static bool RequiresComponent<T>()
        {
            return typeof(RPGame.Enemies.Controller)
                .GetCustomAttributes<RequireComponent>()
                .Any(attribute =>
                    attribute.m_Type0 == typeof(T)
                    || attribute.m_Type1 == typeof(T)
                    || attribute.m_Type2 == typeof(T));
        }

        private static void InvokeStart(RPGame.Enemies.Controller enemyController)
        {
            MethodInfo method = typeof(RPGame.Enemies.Controller).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(enemyController, null);
        }

        private readonly struct TargetFixture
        {
            public TargetFixture(PlayerTargetable targetable, StatisticsController statistics)
            {
                Targetable = targetable;
                Statistics = statistics;
            }

            public PlayerTargetable Targetable { get; }
            public StatisticsController Statistics { get; }
        }
    }
}
