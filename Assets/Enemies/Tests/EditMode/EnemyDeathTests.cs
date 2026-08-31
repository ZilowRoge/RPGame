using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RPGame.Combat.Damage;
using RPGame.Core.Damage;
using RPGame.Core.Interaction;
using RPGame.Core.Statistics;
using RPGame.Core.Targeting;
using RPGame.Inventory;
using RPGame.Inventory.Data;
using RPGame.Loot;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace RPGame.Enemies.Tests
{
    public sealed class EnemyDeathTests
    {
        private readonly List<GameObject> createdObjects = new();
        private readonly List<ScriptableObject> createdAssets = new();
        private readonly List<GameObject> spawnedPickups = new();

        private NavMeshDataInstance navMeshDataInstance;
        private GameObject enemyObject;
        private StatisticsController enemyStatistics;
        private NavMeshAgent agent;
        private Movement movement;
        private Attack attack;
        private EnemyController controller;
        private EnemyTargetable targetable;
        private EnemyDeath death;

        [SetUp]
        public void SetUp()
        {
            ClearTargetRegistry();
            EnsureNavMesh();

            enemyObject = CreateObject("Enemy");
            enemyObject.transform.position = Vector3.zero;
            enemyStatistics = enemyObject.AddComponent<StatisticsController>();
            SetControllerConfig(enemyStatistics, CreateConfig(100f));
            enemyStatistics.ResetToConfig();

            agent = enemyObject.AddComponent<NavMeshAgent>();
            agent.Warp(Vector3.zero);
            enemyObject.AddComponent<Detection>();
            movement = enemyObject.AddComponent<Movement>();
            attack = enemyObject.AddComponent<Attack>();
            ConfigureAttack(attack, 2f, 0.1f, 10f);
            controller = enemyObject.AddComponent<EnemyController>();
            targetable = enemyObject.AddComponent<EnemyTargetable>();
            death = enemyObject.AddComponent<EnemyDeath>();
            InvokeStart(death);
        }

        [TearDown]
        public void TearDown()
        {
            navMeshDataInstance.Remove();
            DestroySpawnedPickups();

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
        public IEnumerator DeathEvent_StopsMovement()
        {
            movement.MoveTo(new Vector3(2f, 0f, 0f));
            yield return null;

            KillEnemy();

            Assert.IsTrue(agent.isStopped);
        }

        [Test]
        public void DeathEvent_DisablesController()
        {
            KillEnemy();

            Assert.IsFalse(controller.enabled);
        }

        [Test]
        public void DeathEvent_PreventsControllerFromRunningFurtherLogic()
        {
            TargetFixture target = CreateDamageablePlayerTarget("PlayerTarget", new Vector3(1f, 0f, 0f));

            KillEnemy();
            controller.Tick();

            Assert.AreEqual(100f, target.Statistics.CurrentHealth);
        }

        [Test]
        public void DeathEvent_PreventsFurtherAttacks()
        {
            TargetFixture target = CreateDamageablePlayerTarget("PlayerTarget", new Vector3(1f, 0f, 0f));

            KillEnemy();
            bool attacked = attack.TryAttack(target.Targetable);

            Assert.IsFalse(attack.enabled);
            Assert.IsFalse(attacked);
            Assert.AreEqual(100f, target.Statistics.CurrentHealth);
        }

        [Test]
        public void DeathEvent_DisablesTargetable()
        {
            KillEnemy();

            Assert.IsFalse(targetable.enabled);
        }

        [Test]
        public void DeathEvent_RemovesEnemyFromTargetRegistry()
        {
            Assert.Contains(targetable, (System.Collections.ICollection)TargetRegistry.EnemyTargets);

            KillEnemy();

            CollectionAssert.DoesNotContain(TargetRegistry.EnemyTargets, targetable);
        }

        [Test]
        public void DeathEvent_IsHandledOnlyOnce()
        {
            KillEnemy();

            Assert.DoesNotThrow(InvokeHandleDeathDirectly);

            Assert.IsTrue(death.IsDead);
            Assert.IsFalse(controller.enabled);
            Assert.IsFalse(attack.enabled);
            Assert.IsFalse(targetable.enabled);
        }

        [Test]
        public void DeathEvent_TriggersExistingLootDropper()
        {
            ItemDefinition item = CreateItemDefinition();
            AddLootDropper(CreateLootTable((item, 1, 1)), CreatePickupPrefab(), null);
            HashSet<ItemPickup> existingPickups = GetExistingPickups();

            KillEnemy();

            Assert.AreEqual(1, GetNewPickups(existingPickups).Count);
        }

        [Test]
        public void DeathEvent_GeneratesLootFromExistingConfiguration()
        {
            ItemDefinition item = CreateItemDefinition();
            AddLootDropper(CreateLootTable((item, 3, 3)), CreatePickupPrefab(), null);
            HashSet<ItemPickup> existingPickups = GetExistingPickups();

            KillEnemy();

            List<ItemPickup> pickups = GetNewPickups(existingPickups);
            Assert.AreEqual(1, pickups.Count);
            Assert.AreSame(item, pickups[0].Item);
            Assert.AreEqual(3, pickups[0].Amount);
        }

        [Test]
        public void DeathEvent_DropsLootAtConfiguredOrigin()
        {
            ItemDefinition item = CreateItemDefinition();
            Transform dropOrigin = CreateDropOrigin(new Vector3(2f, 0f, 3f));
            AddLootDropper(CreateLootTable((item, 1, 1)), CreatePickupPrefab(), dropOrigin);
            HashSet<ItemPickup> existingPickups = GetExistingPickups();

            KillEnemy();

            List<ItemPickup> pickups = GetNewPickups(existingPickups);
            Assert.AreEqual(1, pickups.Count);
            Assert.AreEqual(dropOrigin.position, pickups[0].transform.position);
        }

        [Test]
        public void DeathEvent_DropsLootOnlyOnce()
        {
            ItemDefinition item = CreateItemDefinition();
            AddLootDropper(CreateLootTable((item, 1, 1)), CreatePickupPrefab(), null);
            HashSet<ItemPickup> existingPickups = GetExistingPickups();

            KillEnemy();
            InvokeHandleDeathDirectly();

            Assert.AreEqual(1, GetNewPickups(existingPickups).Count);
        }

        [Test]
        public void DroppedPickup_CanBePickedUpIntoExistingInventory()
        {
            ItemDefinition item = CreateItemDefinition();
            AddLootDropper(CreateLootTable((item, 1, 1)), CreatePickupPrefab(), null);
            HashSet<ItemPickup> existingPickups = GetExistingPickups();
            GameObject playerObject = CreateObject("Player");
            ItemManagementController inventoryController = playerObject.AddComponent<ItemManagementController>();

            KillEnemy();

            ItemPickup pickup = GetNewPickups(existingPickups)[0];
            InteractionContext context = new(playerObject, playerObject.transform);

            Assert.IsTrue(pickup.CanInteract(context));

            pickup.Interact(context);

            Assert.IsTrue(inventoryController.Inventory.GetSlot(0).HasItem);
            Assert.AreSame(item, inventoryController.Inventory.GetSlot(0).Item.Definition);
        }

        [Test]
        public void Start_WhenStatisticsHasNoHealthConfig_DoesNotTriggerDeathCleanup()
        {
            GameObject unconfiguredEnemy = CreateObject("UnconfiguredEnemy");
            unconfiguredEnemy.AddComponent<StatisticsController>();
            unconfiguredEnemy.AddComponent<NavMeshAgent>();
            unconfiguredEnemy.AddComponent<Detection>();

            Movement unconfiguredMovement = unconfiguredEnemy.AddComponent<Movement>();
            Attack unconfiguredAttack = unconfiguredEnemy.AddComponent<Attack>();
            EnemyController unconfiguredController = unconfiguredEnemy.AddComponent<EnemyController>();
            EnemyTargetable unconfiguredTargetable = unconfiguredEnemy.AddComponent<EnemyTargetable>();
            EnemyDeath unconfiguredDeath = unconfiguredEnemy.AddComponent<EnemyDeath>();

            InvokeStart(unconfiguredDeath);

            Assert.IsFalse(unconfiguredDeath.IsDead);
            Assert.IsTrue(unconfiguredController.enabled);
            Assert.IsTrue(unconfiguredAttack.enabled);
            Assert.IsTrue(unconfiguredTargetable.enabled);
            Assert.IsTrue(unconfiguredMovement.enabled);
        }

        [Test]
        public void EnemyDeath_DoesNotContainDamageMovementOrAttackLogic()
        {
            bool referencesForbiddenAssembly = typeof(EnemyDeath).Assembly
                .GetReferencedAssemblies()
                .Any(assemblyName => assemblyName.Name == "RPGame.Player");

            bool hasForbiddenField = typeof(EnemyDeath)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(field =>
                    field.FieldType == typeof(DamageData)
                    || field.FieldType == typeof(NavMeshAgent)
                    || field.FieldType.Namespace == "UnityEngine.AI");

            Assert.IsFalse(referencesForbiddenAssembly);
            Assert.IsFalse(hasForbiddenField);
        }

        [Test]
        public void EnemyDeath_DoesNotContainLootRollingLogic()
        {
            bool hasForbiddenLootField = typeof(EnemyDeath)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(field =>
                    field.FieldType == typeof(LootRoller)
                    || field.FieldType == typeof(LootTable)
                    || field.FieldType == typeof(IndependentLootGroup)
                    || field.FieldType == typeof(IndependentLootEntry)
                    || field.FieldType == typeof(WeightedLootGroup)
                    || field.FieldType == typeof(WeightedLootEntry));

            Assert.IsFalse(hasForbiddenLootField);
        }

        [Test]
        public void CoreAndCombat_DoNotReferenceEnemiesOrLoot()
        {
            bool coreReferencesEnemies = typeof(StatisticsController).Assembly
                .GetReferencedAssemblies()
                .Any(assemblyName => assemblyName.Name == "RPGame.Enemies" || assemblyName.Name == "RPGame.Loot");

            bool combatReferencesEnemies = typeof(DamageReceiver).Assembly
                .GetReferencedAssemblies()
                .Any(assemblyName => assemblyName.Name == "RPGame.Enemies" || assemblyName.Name == "RPGame.Loot");

            Assert.IsFalse(coreReferencesEnemies);
            Assert.IsFalse(combatReferencesEnemies);
        }

        private void KillEnemy()
        {
            enemyStatistics.TakeDamage(100f);
        }

        private void InvokeHandleDeathDirectly()
        {
            MethodInfo method = typeof(EnemyDeath).GetMethod("HandleDeath", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(death, null);
        }

        private static void InvokeStart(EnemyDeath enemyDeath)
        {
            MethodInfo method = typeof(EnemyDeath).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(enemyDeath, null);
        }

        private TargetFixture CreateDamageablePlayerTarget(string objectName, Vector3 position)
        {
            GameObject targetObject = CreateObject(objectName);
            targetObject.transform.position = position;

            StatisticsController statistics = targetObject.AddComponent<StatisticsController>();
            SetControllerConfig(statistics, CreateConfig(100f));
            statistics.ResetToConfig();
            targetObject.AddComponent<DamageReceiver>();

            PlayerTargetable playerTargetable = targetObject.AddComponent<PlayerTargetable>();
            return new TargetFixture(playerTargetable, statistics);
        }

        private LootDropper AddLootDropper(
            LootTable lootTable,
            ItemPickup pickupPrefab,
            Transform dropOrigin)
        {
            LootDropper dropper = enemyObject.AddComponent<LootDropper>();
            SerializedObject serializedDropper = new(dropper);
            serializedDropper.FindProperty("lootTable").objectReferenceValue = lootTable;
            serializedDropper.FindProperty("pickupPrefab").objectReferenceValue = pickupPrefab;
            serializedDropper.FindProperty("dropOrigin").objectReferenceValue = dropOrigin;
            serializedDropper.FindProperty("deathSource").objectReferenceValue = enemyStatistics;
            serializedDropper.FindProperty("dropRadius").floatValue = 0f;
            serializedDropper.ApplyModifiedPropertiesWithoutUndo();

            SetEnemyDeathLootDropper(dropper);
            dropper.enabled = false;
            return dropper;
        }

        private void SetEnemyDeathLootDropper(LootDropper dropper)
        {
            SerializedObject serializedDeath = new(death);
            serializedDeath.FindProperty("lootDropper").objectReferenceValue = dropper;
            serializedDeath.ApplyModifiedPropertiesWithoutUndo();
        }

        private ItemPickup CreatePickupPrefab()
        {
            GameObject pickupObject = CreateObject("PickupPrefab");
            return pickupObject.AddComponent<ItemPickup>();
        }

        private Transform CreateDropOrigin(Vector3 position)
        {
            GameObject dropOriginObject = CreateObject("DropOrigin");
            dropOriginObject.transform.position = position;
            return dropOriginObject.transform;
        }

        private ItemDefinition CreateItemDefinition()
        {
            ItemDefinition itemDefinition = ScriptableObject.CreateInstance<ItemDefinition>();
            createdAssets.Add(itemDefinition);
            return itemDefinition;
        }

        private LootTable CreateLootTable(params (ItemDefinition item, int minAmount, int maxAmount)[] entries)
        {
            LootTable table = ScriptableObject.CreateInstance<LootTable>();
            createdAssets.Add(table);

            SerializedObject serializedTable = new(table);
            SerializedProperty serializedGroups = serializedTable.FindProperty("independentGroups");
            serializedGroups.arraySize = 1;

            SerializedProperty serializedEntries = serializedGroups
                .GetArrayElementAtIndex(0)
                .FindPropertyRelative("entries");
            serializedEntries.arraySize = entries.Length;

            for (int i = 0; i < entries.Length; i++)
            {
                SerializedProperty entry = serializedEntries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("item").objectReferenceValue = entries[i].item;
                entry.FindPropertyRelative("minAmount").intValue = entries[i].minAmount;
                entry.FindPropertyRelative("maxAmount").intValue = entries[i].maxAmount;
                entry.FindPropertyRelative("chance").floatValue = 1f;
            }

            serializedTable.FindProperty("weightedGroups").arraySize = 0;
            serializedTable.ApplyModifiedPropertiesWithoutUndo();
            return table;
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

        private static void SetControllerConfig(StatisticsController controller, StatisticsConfig statisticsConfig)
        {
            SerializedObject serializedController = new(controller);
            serializedController.FindProperty("config").objectReferenceValue = statisticsConfig;
            serializedController.FindProperty("initializeOnAwake").boolValue = false;
            serializedController.ApplyModifiedPropertiesWithoutUndo();
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

        private static void ClearTargetRegistry()
        {
            MethodInfo method = typeof(TargetRegistry).GetMethod("Clear", BindingFlags.Static | BindingFlags.NonPublic);
            method.Invoke(null, null);
        }

        private static HashSet<ItemPickup> GetExistingPickups()
        {
            return new HashSet<ItemPickup>(Object.FindObjectsByType<ItemPickup>());
        }

        private List<ItemPickup> GetNewPickups(HashSet<ItemPickup> existingPickups)
        {
            ItemPickup[] pickups = Object.FindObjectsByType<ItemPickup>();
            List<ItemPickup> newPickups = new();
            for (int i = 0; i < pickups.Length; i++)
            {
                if (existingPickups.Contains(pickups[i]))
                {
                    continue;
                }

                newPickups.Add(pickups[i]);
                if (!spawnedPickups.Contains(pickups[i].gameObject))
                {
                    spawnedPickups.Add(pickups[i].gameObject);
                }
            }

            return newPickups;
        }

        private void DestroySpawnedPickups()
        {
            for (int i = spawnedPickups.Count - 1; i >= 0; i--)
            {
                if (spawnedPickups[i] != null)
                {
                    Object.DestroyImmediate(spawnedPickups[i]);
                }
            }

            spawnedPickups.Clear();
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
