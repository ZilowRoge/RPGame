using System.Collections;
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
using UnityEngine.TestTools;

namespace RPGame.Enemies.Tests
{
    public sealed class EnemyControllerTests
    {
        private readonly List<GameObject> createdObjects = new();
        private readonly List<ScriptableObject> createdAssets = new();

        private NavMeshDataInstance navMeshDataInstance;
        private GameObject enemyObject;
        private NavMeshAgent agent;
        private Detection detection;
        private Attack attack;
        private EnemyController controller;

        [SetUp]
        public void SetUp()
        {
            ClearTargetRegistry();
            EnsureNavMesh();

            enemyObject = CreateObject("Enemy");
            enemyObject.transform.position = Vector3.zero;
            agent = enemyObject.AddComponent<NavMeshAgent>();
            agent.Warp(Vector3.zero);
            detection = enemyObject.AddComponent<Detection>();
            enemyObject.AddComponent<Movement>();
            attack = enemyObject.AddComponent<Attack>();
            ConfigureAttack(attack, 1.5f, 0.1f, 10f);
            controller = enemyObject.AddComponent<EnemyController>();
            InvokeStart(controller);
        }

        [TearDown]
        public void TearDown()
        {
            navMeshDataInstance.Remove();

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

        [UnityTest]
        public IEnumerator Tick_WhenNoTarget_StopsMovementAndDoesNotAttack()
        {
            agent.isStopped = false;

            controller.Tick();

            yield return null;

            Assert.IsTrue(agent.isStopped);
        }

        [UnityTest]
        public IEnumerator Tick_WhenTargetIsOutsideAttackRange_ChasesTarget()
        {
            TargetFixture target = CreateDamageableTarget("Target", new Vector3(3f, 0f, 0f));
            detection.RefreshDetection();

            controller.Tick();

            yield return null;

            Assert.IsFalse(agent.isStopped);
            AssertVectorApproximately(target.Targetable.TargetPoint.position, agent.destination);
            Assert.AreEqual(100f, target.Statistics.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator Tick_WhenTargetIsInAttackRange_StopsMovement()
        {
            CreateDamageableTarget("Target", new Vector3(1f, 0f, 0f));
            detection.RefreshDetection();
            agent.isStopped = false;

            controller.Tick();

            yield return null;

            Assert.IsTrue(agent.isStopped);
        }

        [Test]
        public void Tick_WhenTargetIsInAttackRange_TriesAttack()
        {
            TargetFixture target = CreateDamageableTarget("Target", new Vector3(1f, 0f, 0f));
            detection.RefreshDetection();

            controller.Tick();

            Assert.AreEqual(90f, target.Statistics.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator Tick_WhenTargetLeavesAttackRange_ChasesAgain()
        {
            TargetFixture target = CreateDamageableTarget("Target", new Vector3(1f, 0f, 0f));
            detection.RefreshDetection();
            controller.Tick();

            target.Targetable.transform.position = new Vector3(3f, 0f, 0f);
            controller.Tick();

            yield return null;

            Assert.IsFalse(agent.isStopped);
            AssertVectorApproximately(target.Targetable.TargetPoint.position, agent.destination);
        }

        [UnityTest]
        public IEnumerator Tick_WhenTargetIsLost_StopsMovement()
        {
            TargetFixture target = CreateDamageableTarget("Target", new Vector3(3f, 0f, 0f));
            detection.RefreshDetection();
            controller.Tick();
            yield return null;

            target.Targetable.gameObject.SetActive(false);
            detection.RefreshDetection();
            controller.Tick();

            Assert.IsTrue(agent.isStopped);
        }

        [Test]
        public void EnemyController_DoesNotStoreAttackCooldown()
        {
            bool hasCooldownField = typeof(EnemyController)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(field => field.FieldType == typeof(float) || field.Name.ToLowerInvariant().Contains("cooldown"));

            Assert.IsFalse(hasCooldownField);
        }

        [Test]
        public void EnemyController_RequiresCoreEnemyComponents()
        {
            Assert.IsTrue(RequiresComponent<Detection>());
            Assert.IsTrue(RequiresComponent<Movement>());
            Assert.IsTrue(RequiresComponent<Attack>());
            Assert.IsTrue(RequiresComponent<EnemyTargetable>());
            Assert.IsTrue(RequiresComponent<DamageReceiver>());
            Assert.IsTrue(RequiresComponent<EnemyDeath>());
        }

        [Test]
        public void EnemyController_DoesNotReferencePlayerDamageOrNavMeshLogic()
        {
            bool referencesForbiddenAssembly = typeof(EnemyController).Assembly
                .GetReferencedAssemblies()
                .Any(assemblyName => assemblyName.Name == "RPGame.Player");

            bool hasForbiddenField = typeof(EnemyController)
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

        private void EnsureNavMesh()
        {
            if (navMeshDataInstance.valid)
            {
                return;
            }

            NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);
            List<NavMeshBuildSource> sources = new()
            {
                new NavMeshBuildSource
                {
                    shape = NavMeshBuildSourceShape.Box,
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one),
                    size = new Vector3(10f, 0.1f, 10f),
                    area = 0
                }
            };

            Bounds bounds = new(Vector3.zero, new Vector3(10f, 2f, 10f));
            NavMeshData navMeshData = NavMeshBuilder.BuildNavMeshData(
                buildSettings,
                sources,
                bounds,
                Vector3.zero,
                Quaternion.identity);

            navMeshDataInstance = NavMesh.AddNavMeshData(navMeshData);
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

        private static void AssertVectorApproximately(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, 0.05f);
            Assert.AreEqual(expected.y, actual.y, 0.05f);
            Assert.AreEqual(expected.z, actual.z, 0.05f);
        }

        private static void ClearTargetRegistry()
        {
            MethodInfo method = typeof(TargetRegistry).GetMethod("Clear", BindingFlags.Static | BindingFlags.NonPublic);
            method.Invoke(null, null);
        }

        private static bool RequiresComponent<T>()
        {
            return typeof(EnemyController)
                .GetCustomAttributes<RequireComponent>()
                .Any(attribute =>
                    attribute.m_Type0 == typeof(T)
                    || attribute.m_Type1 == typeof(T)
                    || attribute.m_Type2 == typeof(T));
        }

        private static void InvokeStart(EnemyController enemyController)
        {
            MethodInfo method = typeof(EnemyController).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
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
