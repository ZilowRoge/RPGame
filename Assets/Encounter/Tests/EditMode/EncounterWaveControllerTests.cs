using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RPGame.Enemies;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RPGame.Encounter.Tests
{
    public sealed class EncounterWaveControllerTests
    {
        private readonly List<GameObject> createdObjects = new();
        private readonly List<ScriptableObject> createdAssets = new();

        [TearDown]
        public void TearDown()
        {
            LogAssert.ignoreFailingMessages = false;

            for (int i = 0; i < createdObjects.Count; i++)
            {
                if (createdObjects[i] != null)
                {
                    Object.DestroyImmediate(createdObjects[i]);
                }
            }

            for (int i = 0; i < createdAssets.Count; i++)
            {
                if (createdAssets[i] != null)
                {
                    Object.DestroyImmediate(createdAssets[i]);
                }
            }

            createdObjects.Clear();
            createdAssets.Clear();
        }

        [UnityTest]
        public IEnumerator StartWave_StartsSpawningAndTracksScheduledCount()
        {
            LogAssert.ignoreFailingMessages = true;
            EncounterWaveController controller = CreateController(10f, CreateSpawnPoint("A", Vector3.zero));
            EnemyDefinition enemy = EnemyWithDeath("ScheduledEnemy");

            controller.StartWave(Wave(new[] { enemy, enemy }, 10), _ => { });

            Assert.IsTrue(controller.IsWaveActive);
            Assert.AreEqual(2, controller.RemainingEnemies);
            Assert.AreEqual(1, FindSpawned(enemy).Count);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyDeath_DecrementsRemainingEnemies()
        {
            LogAssert.ignoreFailingMessages = true;
            EncounterWaveController controller = CreateController(10f, CreateSpawnPoint("A", Vector3.zero));
            EnemyDefinition enemy = EnemyWithDeath("DeathCountEnemy");
            controller.StartWave(Wave(new[] { enemy, enemy }, 10), _ => { });

            RaiseDeath(FindSpawned(enemy)[0]);

            Assert.AreEqual(1, controller.RemainingEnemies);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Wave_DoesNotCompleteBeforeSpawningFinishes()
        {
            LogAssert.ignoreFailingMessages = true;
            EncounterWaveController controller = CreateController(10f, CreateSpawnPoint("A", Vector3.zero));
            EnemyDefinition enemy = EnemyWithDeath("PendingSpawnEnemy");
            int completedCount = 0;
            controller.StartWave(Wave(new[] { enemy, enemy }, 10), _ => completedCount++);

            RaiseDeath(FindSpawned(enemy)[0]);

            Assert.AreEqual(0, completedCount);
            Assert.IsTrue(controller.IsWaveActive);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Wave_DoesNotCompleteWhileEnemiesRemain()
        {
            LogAssert.ignoreFailingMessages = true;
            EncounterWaveController controller = CreateController(0f, CreateSpawnPoint("A", Vector3.zero));
            EnemyDefinition enemy = EnemyWithDeath("RemainingEnemy");
            int completedCount = 0;
            controller.StartWave(Wave(new[] { enemy, enemy }, 10), _ => completedCount++);
            List<PooledEnemy> spawned = FindSpawned(enemy);

            RaiseDeath(spawned[0]);

            Assert.AreEqual(0, completedCount);
            Assert.IsTrue(controller.IsWaveActive);
            Assert.AreEqual(1, controller.RemainingEnemies);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Wave_CompletesExactlyOnceWhenSpawningFinishedAndNoEnemiesRemain()
        {
            LogAssert.ignoreFailingMessages = true;
            EncounterWaveController controller = CreateController(0f, CreateSpawnPoint("A", Vector3.zero));
            EnemyDefinition enemy = EnemyWithDeath("CompletingEnemy");
            WaveData wave = Wave(new[] { enemy, enemy }, 10);
            int completedCount = 0;
            WaveData completedWave = null;
            controller.StartWave(
                wave,
                completed =>
                {
                    completedCount++;
                    completedWave = completed;
                });
            List<PooledEnemy> spawned = FindSpawned(enemy);

            RaiseDeath(spawned[0]);
            RaiseDeath(spawned[1]);
            RaiseDeath(spawned[1]);

            Assert.AreEqual(1, completedCount);
            Assert.AreSame(wave, completedWave);
            Assert.IsFalse(controller.IsWaveActive);
            Assert.AreEqual(0, controller.RemainingEnemies);
            yield return null;
        }

        [Test]
        public void EmptyWave_CompletesCorrectly()
        {
            EncounterWaveController controller = CreateController(0f);
            WaveData wave = Wave(Array.Empty<EnemyDefinition>(), 10);
            int completedCount = 0;
            WaveData completedWave = null;

            controller.StartWave(
                wave,
                completed =>
                {
                    completedCount++;
                    completedWave = completed;
                });

            Assert.AreEqual(1, completedCount);
            Assert.AreSame(wave, completedWave);
            Assert.IsFalse(controller.IsWaveActive);
            Assert.AreEqual(0, controller.RemainingEnemies);
        }

        [UnityTest]
        public IEnumerator CancelWave_CancelsSpawningAndDoesNotComplete()
        {
            LogAssert.ignoreFailingMessages = true;
            EncounterWaveController controller = CreateController(10f, CreateSpawnPoint("A", Vector3.zero));
            EnemyDefinition enemy = EnemyWithDeath("CancelledWaveEnemy");
            int completedCount = 0;
            controller.StartWave(Wave(new[] { enemy, enemy }, 10), _ => completedCount++);

            controller.CancelWave();
            yield return new WaitForSeconds(0.1f);

            Assert.AreEqual(0, completedCount);
            Assert.IsFalse(controller.IsWaveActive);
            Assert.AreEqual(0, controller.RemainingEnemies);
            Assert.AreEqual(1, FindSpawned(enemy).Count);
        }

        [UnityTest]
        public IEnumerator StaleCallbacksAfterCancel_DoNothing()
        {
            LogAssert.ignoreFailingMessages = true;
            EncounterWaveController controller = CreateController(10f, CreateSpawnPoint("A", Vector3.zero));
            EnemyDefinition enemy = EnemyWithDeath("StaleCallbackEnemy");
            int completedCount = 0;
            controller.StartWave(Wave(new[] { enemy, enemy }, 10), _ => completedCount++);
            PooledEnemy spawned = FindSpawned(enemy)[0];

            controller.CancelWave();
            RaiseDeath(spawned);

            Assert.AreEqual(0, completedCount);
            Assert.IsFalse(controller.IsWaveActive);
            Assert.AreEqual(0, controller.RemainingEnemies);
            yield return null;
        }

        [Test]
        public void StartWave_WhenWaveIsAlreadyActive_RejectsOverlap()
        {
            LogAssert.ignoreFailingMessages = true;
            EncounterWaveController controller = CreateController(10f, CreateSpawnPoint("A", Vector3.zero));
            EnemyDefinition enemy = EnemyWithDeath("OverlapWaveEnemy");
            controller.StartWave(Wave(new[] { enemy, enemy }, 10), _ => { });

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => controller.StartWave(Wave(enemy, 11), _ => { }));

            StringAssert.Contains("another wave", exception.Message);
        }

        private EncounterWaveController CreateController(float delay, params SpawnPoint[] points)
        {
            GameObject controllerObject = CreateObject("EncounterWaveController");
            EncounterWaveController controller = controllerObject.AddComponent<EncounterWaveController>();
            SetPrivateField(controller, "spawnManager", CreateSpawnManager(delay, points));
            return controller;
        }

        private SpawnManager CreateSpawnManager(float delay, params SpawnPoint[] points)
        {
            GameObject managerObject = CreateObject("SpawnManager");
            SpawnManager manager = managerObject.AddComponent<SpawnManager>();
            SetPrivateField(manager, "spawnDelay", delay);
            SetPrivateField(manager, "spawnPoints", new List<SpawnPoint>(points));
            SetPrivateField(manager, "enemyPool", CreateEnemyPool());
            return manager;
        }

        private EnemyPool CreateEnemyPool()
        {
            GameObject poolObject = CreateObject("EnemyPool");
            return poolObject.AddComponent<EnemyPool>();
        }

        private SpawnPoint CreateSpawnPoint(string name, Vector3 position)
        {
            GameObject pointObject = CreateObject(name);
            pointObject.transform.position = position;
            return pointObject.AddComponent<SpawnPoint>();
        }

        private EnemyDefinition EnemyWithDeath(string name)
        {
            GameObject prefab = CreateObject(name);
            prefab.AddComponent<PooledEnemy>();
            prefab.AddComponent<Death>();
            EnemyDefinition definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            createdAssets.Add(definition);
            SetPrivateField(definition, "prefab", prefab);
            SetPrivateField(definition, "cost", 1);
            SetPrivateField(definition, "unlockWave", 1);
            return definition;
        }

        private WaveData Wave(EnemyDefinition enemy, int seed)
        {
            return Wave(new[] { enemy }, seed);
        }

        private static WaveData Wave(IEnumerable<EnemyDefinition> enemies, int seed)
        {
            return new WaveData(1, 1f, 1, enemies, seed);
        }

        private List<PooledEnemy> FindSpawned(EnemyDefinition enemy)
        {
            List<PooledEnemy> spawned = new();
            PooledEnemy[] enemies = Object.FindObjectsByType<PooledEnemy>(
                FindObjectsInactive.Exclude);
            for (int i = 0; i < enemies.Length; i++)
            {
                GameObject gameObject = enemies[i].gameObject;
                if (!gameObject.name.StartsWith($"{enemy.Prefab.name}(Clone)", StringComparison.Ordinal))
                {
                    continue;
                }

                TrackIfMissing(gameObject);
                spawned.Add(enemies[i]);
            }

            return spawned;
        }

        private GameObject CreateObject(string name)
        {
            GameObject gameObject = new(name);
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private void TrackIfMissing(GameObject gameObject)
        {
            if (!createdObjects.Contains(gameObject))
            {
                createdObjects.Add(gameObject);
            }
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(target, value);
        }

        private static void RaiseDeath(PooledEnemy pooledEnemy)
        {
            Death death = pooledEnemy.GetComponent<Death>();
            FieldInfo field = typeof(Death).GetField(
                "OnDeathCleanupEnd",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Action callback = (Action)field.GetValue(death);
            callback?.Invoke();
        }
    }
}
