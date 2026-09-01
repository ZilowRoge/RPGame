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

namespace RPGame.Enemies.Tests
{
    public sealed class AttackTests
    {
        private readonly List<GameObject> createdObjects = new();
        private readonly List<ScriptableObject> createdAssets = new();

        private GameObject attackerObject;
        private Attack attack;

        [SetUp]
        public void SetUp()
        {
            attackerObject = CreateObject("Enemy");
            attack = attackerObject.AddComponent<Attack>();
            ConfigureAttack(attack, 2f, 0.05f, 10f);
            InvokeStart(attack);
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
        }

        [Test]
        public void IsInRange_WhenTargetIsOutsideAttackRange_ReturnsFalse()
        {
            PlayerTargetable target = CreateDamageableTarget("Target", new Vector3(3f, 0f, 0f)).Targetable;

            Assert.IsFalse(AttackInterface.IsInRange(CreateSelectedTarget(target)));
            Assert.IsFalse(AttackInterface.TryAttack(CreateSelectedTarget(target)));
        }

        [Test]
        public void IsInRange_WhenTargetIsInsideAttackRange_ReturnsTrue()
        {
            PlayerTargetable target = CreateDamageableTarget("Target", new Vector3(1f, 0f, 0f)).Targetable;

            Assert.IsTrue(AttackInterface.IsInRange(CreateSelectedTarget(target)));
        }

        [Test]
        public void TryAttack_PassesDamageThroughExistingPipeline()
        {
            TargetFixture target = CreateDamageableTarget("Target", new Vector3(1f, 0f, 0f));
            DamageResult receivedResult = default;
            target.DamageReceiver.DamageReceived += result => receivedResult = result;

            bool attacked = AttackInterface.TryAttack(CreateSelectedTarget(target.Targetable));

            Assert.IsTrue(attacked);
            Assert.IsTrue(receivedResult.WasApplied);
            Assert.AreEqual(10f, receivedResult.AppliedAmount);
            Assert.AreSame(attackerObject, receivedResult.Data.Source);
            Assert.AreEqual(DamageType.Physical, receivedResult.Data.Parts[0].DamageType);
            Assert.AreEqual(DamageElement.None, receivedResult.Data.Parts[0].DamageElement);
        }

        [Test]
        public void TryAttack_ReducesTargetHealthThroughExistingSystems()
        {
            TargetFixture target = CreateDamageableTarget("Target", new Vector3(1f, 0f, 0f));

            bool attacked = AttackInterface.TryAttack(CreateSelectedTarget(target.Targetable));

            Assert.IsTrue(attacked);
            Assert.AreEqual(90f, target.Statistics.CurrentHealth);
        }

        [Test]
        public void TryAttack_BeforeAttackIntervalExpires_IsBlocked()
        {
            TargetFixture target = CreateDamageableTarget("Target", new Vector3(1f, 0f, 0f));

            Assert.IsTrue(AttackInterface.TryAttack(CreateSelectedTarget(target.Targetable)));
            Assert.IsFalse(AttackInterface.TryAttack(CreateSelectedTarget(target.Targetable)));
            Assert.AreEqual(90f, target.Statistics.CurrentHealth);
        }

        [Test]
        public void TryAttack_WhenAttackIntervalExpired_IsAllowedAgain()
        {
            TargetFixture target = CreateDamageableTarget("Target", new Vector3(1f, 0f, 0f));

            Assert.IsTrue(AttackInterface.TryAttack(CreateSelectedTarget(target.Targetable)));

            AttackInterface.Tick(0.05f);

            Assert.IsTrue(AttackInterface.TryAttack(CreateSelectedTarget(target.Targetable)));
            Assert.AreEqual(80f, target.Statistics.CurrentHealth);
        }

        [Test]
        public void AttackAdapter_DoesNotOwnCooldownRangeOrDamageData()
        {
            FieldInfo[] fields = typeof(Attack).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Assert.IsFalse(fields.Any(field => field.Name.Contains("nextAttackTime")));
            Assert.IsFalse(fields.Any(field => field.Name.Contains("attackRange")));
            Assert.IsFalse(fields.Any(field => field.Name.Contains("attackInterval")));
            Assert.IsFalse(fields.Any(field => field.FieldType == typeof(List<PartialDamageRange>)));
        }

        [Test]
        public void Attack_DoesNotReferencePlayerDetectionOrMovement()
        {
            bool referencesPlayer = typeof(Attack).Assembly
                .GetReferencedAssemblies()
                .Any(assemblyName => assemblyName.Name == "RPGame.Player");

            bool hasForbiddenField = typeof(Attack)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(field => field.FieldType == typeof(Detection) || field.FieldType == typeof(Movement));

            Assert.IsFalse(referencesPlayer);
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
            return new TargetFixture(targetable, statistics, damageReceiver);
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

        private void ConfigureAttack(Attack attack, float attackRange, float attackInterval, float damageAmount)
        {
            MeleeAttackConfig meleeAttackConfig = ScriptableObject.CreateInstance<MeleeAttackConfig>();
            createdAssets.Add(meleeAttackConfig);
            SerializedObject serializedMeleeConfig = new(meleeAttackConfig);
            serializedMeleeConfig.FindProperty("attackInterval").floatValue = attackInterval;
            serializedMeleeConfig.FindProperty("attackRange").floatValue = attackRange;

            SerializedProperty damageProperty = serializedMeleeConfig.FindProperty("damage");
            damageProperty.arraySize = 1;
            SerializedProperty damageEntry = damageProperty.GetArrayElementAtIndex(0);
            damageEntry.FindPropertyRelative("minDamage").floatValue = damageAmount;
            damageEntry.FindPropertyRelative("maxDamage").floatValue = damageAmount;
            damageEntry.FindPropertyRelative("damageType").enumValueIndex = (int)DamageType.Physical;
            damageEntry.FindPropertyRelative("damageElement").enumValueIndex = (int)DamageElement.None;
            serializedMeleeConfig.ApplyModifiedPropertiesWithoutUndo();

            Config config = ScriptableObject.CreateInstance<Config>();
            createdAssets.Add(config);
            SerializedObject serializedConfig = new(config);
            SerializedProperty attacksProperty = serializedConfig.FindProperty("attacks");
            attacksProperty.arraySize = 1;
            SerializedProperty attackEntry = attacksProperty.GetArrayElementAtIndex(0);
            attackEntry.FindPropertyRelative("type").enumValueIndex = (int)AttackType.Melee;
            attackEntry.FindPropertyRelative("config").objectReferenceValue = meleeAttackConfig;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedAttack = new(attack);
            serializedAttack.FindProperty("config").objectReferenceValue = config;
            serializedAttack.FindProperty("attackType").enumValueIndex = (int)AttackType.Melee;
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

        private static void InvokeStart(Attack attack)
        {
            MethodInfo method = typeof(Attack).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(attack, null);
        }

        private static SelectedTarget CreateSelectedTarget(PlayerTargetable targetable)
        {
            return new SelectedTarget(targetable, targetable.TargetPoint.position);
        }

        private IEnemyAttack AttackInterface => attack;

        private readonly struct TargetFixture
        {
            public TargetFixture(
                PlayerTargetable targetable,
                StatisticsController statistics,
                DamageReceiver damageReceiver)
            {
                Targetable = targetable;
                Statistics = statistics;
                DamageReceiver = damageReceiver;
            }

            public PlayerTargetable Targetable { get; }
            public StatisticsController Statistics { get; }
            public DamageReceiver DamageReceiver { get; }
        }
    }
}
