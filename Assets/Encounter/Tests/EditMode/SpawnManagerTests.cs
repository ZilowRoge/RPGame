using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RPGame.Encounter.Tests
{
    public sealed class SpawnManagerTests
    {
        private readonly List<GameObject> createdObjects = new();
        private readonly List<ScriptableObject> createdAssets = new();

        [TearDown]
        public void TearDown()
        {
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

        [Test]
        public void AssignEnemiesToSpawnPoints_WhenSeedIsSame_ProducesSameAssignments()
        {
            SpawnPoint firstPoint = CreateSpawnPoint("A", Vector3.zero);
            SpawnPoint secondPoint = CreateSpawnPoint("B", Vector3.right);
            EnemyDefinition firstEnemy = Enemy("EnemyA");
            EnemyDefinition secondEnemy = Enemy("EnemyB");
            EnemyDefinition thirdEnemy = Enemy("EnemyC");
            EnemyDefinition[] enemies = { firstEnemy, secondEnemy, thirdEnemy };
            SpawnPoint[] points = { firstPoint, secondPoint };

            Dictionary<SpawnPoint, List<EnemyDefinition>> firstAssignments =
                SpawnManager.AssignEnemiesToSpawnPoints(enemies, points, 123);
            Dictionary<SpawnPoint, List<EnemyDefinition>> secondAssignments =
                SpawnManager.AssignEnemiesToSpawnPoints(enemies, points, 123);

            CollectionAssert.AreEqual(firstAssignments[firstPoint], secondAssignments[firstPoint]);
            CollectionAssert.AreEqual(firstAssignments[secondPoint], secondAssignments[secondPoint]);
        }

        [Test]
        public void AssignEnemiesToSpawnPoints_WhenSeedDiffers_CanProduceDifferentAssignments()
        {
            SpawnPoint firstPoint = CreateSpawnPoint("A", Vector3.zero);
            SpawnPoint secondPoint = CreateSpawnPoint("B", Vector3.right);
            EnemyDefinition[] enemies =
            {
                Enemy("EnemyA"),
                Enemy("EnemyB"),
                Enemy("EnemyC"),
                Enemy("EnemyD")
            };
            SpawnPoint[] points = { firstPoint, secondPoint };
            Dictionary<SpawnPoint, List<EnemyDefinition>> baseline =
                SpawnManager.AssignEnemiesToSpawnPoints(enemies, points, 1);

            bool foundDifferentAssignment = false;
            for (int seed = 2; seed < 100 && !foundDifferentAssignment; seed++)
            {
                Dictionary<SpawnPoint, List<EnemyDefinition>> assignments =
                    SpawnManager.AssignEnemiesToSpawnPoints(enemies, points, seed);
                foundDifferentAssignment =
                    !baseline[firstPoint].SequenceEqual(assignments[firstPoint]) ||
                    !baseline[secondPoint].SequenceEqual(assignments[secondPoint]);
            }

            Assert.IsTrue(foundDifferentAssignment);
        }

        [UnityTest]
        public IEnumerator StartSpawning_WhenQueueStarts_SpawnsFirstEnemyImmediately()
        {
            SpawnManager manager = CreateSpawnManager(10f, CreateSpawnPoint("A", Vector3.zero));
            EnemyDefinition enemy = Enemy("ImmediateEnemy");

            manager.StartSpawning(Wave(enemy, 10));

            Assert.AreEqual(1, CountSpawned(enemy));
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartSpawning_WhenQueueHasMultipleEnemies_WaitsDelayBeforeSubsequentEnemies()
        {
            SpawnManager manager = CreateSpawnManager(0.1f, CreateSpawnPoint("A", Vector3.zero));
            EnemyDefinition enemy = Enemy("DelayedEnemy");

            manager.StartSpawning(Wave(new[] { enemy, enemy }, 10));

            Assert.AreEqual(1, CountSpawned(enemy));
            yield return null;
            Assert.AreEqual(1, CountSpawned(enemy));
            yield return new WaitForSeconds(0.15f);
            Assert.AreEqual(2, CountSpawned(enemy));
        }

        [UnityTest]
        public IEnumerator StartSpawning_WhenMultipleSpawnPointsAreUsed_StartsQueuesInParallel()
        {
            SpawnPoint firstPoint = CreateSpawnPoint("A", Vector3.zero);
            SpawnPoint secondPoint = CreateSpawnPoint("B", Vector3.right);
            SpawnManager manager = CreateSpawnManager(10f, firstPoint, secondPoint);
            EnemyDefinition enemy = Enemy("ParallelEnemy");
            int seed = FindSeedUsingBothPoints(new[] { enemy, enemy }, new[] { firstPoint, secondPoint });

            manager.StartSpawning(Wave(new[] { enemy, enemy }, seed));

            Assert.AreEqual(2, CountSpawned(enemy));
            Assert.AreEqual(1, CountSpawnedAt(enemy, firstPoint.transform.position));
            Assert.AreEqual(1, CountSpawnedAt(enemy, secondPoint.transform.position));
            yield return null;
        }

        [UnityTest]
        public IEnumerator AllEnemiesSpawned_WhenQueuesAreActive_FiresAfterEveryActiveQueueFinishes()
        {
            SpawnManager manager = CreateSpawnManager(0.1f, CreateSpawnPoint("A", Vector3.zero));
            EnemyDefinition enemy = Enemy("CompletionEnemy");

            manager.StartSpawning(Wave(new[] { enemy, enemy }, 10));

            Assert.IsFalse(manager.AllEnemiesSpawned);
            yield return new WaitForSeconds(0.15f);
            Assert.IsTrue(manager.AllEnemiesSpawned);
        }

        [Test]
        public void StartSpawning_WhenWaveHasNoValidEnemies_CompletesImmediately()
        {
            SpawnManager manager = CreateSpawnManager(0.1f, CreateSpawnPoint("A", Vector3.zero));

            manager.StartSpawning(new WaveData(1, 1f, 1, Array.Empty<EnemyDefinition>(), 10));

            Assert.IsTrue(manager.AllEnemiesSpawned);
            Assert.IsFalse(manager.IsSpawning);
        }

        [Test]
        public void StartSpawning_WhenNonEmptyWaveHasNoValidSpawnPoints_ThrowsClearly()
        {
            SpawnManager manager = CreateSpawnManager(0.1f, null);
            EnemyDefinition enemy = Enemy("NoPointEnemy");

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => manager.StartSpawning(Wave(enemy, 10)));

            StringAssert.Contains("valid spawn points", exception.Message);
        }

        [UnityTest]
        public IEnumerator CancelSpawning_WhenQueueHasRemainingEnemies_PreventsRemainingSpawns()
        {
            SpawnManager manager = CreateSpawnManager(0.1f, CreateSpawnPoint("A", Vector3.zero));
            EnemyDefinition enemy = Enemy("CancelledEnemy");
            manager.StartSpawning(Wave(new[] { enemy, enemy }, 10));

            manager.CancelSpawning();
            yield return new WaitForSeconds(0.15f);

            Assert.AreEqual(1, CountSpawned(enemy));
        }

        [UnityTest]
        public IEnumerator CancelSpawning_WhenCalled_DoesNotReportNormalCompletion()
        {
            SpawnManager manager = CreateSpawnManager(0.1f, CreateSpawnPoint("A", Vector3.zero));
            EnemyDefinition enemy = Enemy("CancelledCompletionEnemy");
            manager.StartSpawning(Wave(new[] { enemy, enemy }, 10));

            manager.CancelSpawning();
            yield return new WaitForSeconds(0.15f);

            Assert.IsFalse(manager.AllEnemiesSpawned);
            Assert.IsFalse(manager.IsSpawning);
        }

        [Test]
        public void StartSpawning_WhenOperationIsActive_RejectsOverlap()
        {
            SpawnManager manager = CreateSpawnManager(10f, CreateSpawnPoint("A", Vector3.zero));
            EnemyDefinition enemy = Enemy("OverlapEnemy");
            manager.StartSpawning(Wave(new[] { enemy, enemy }, 10));

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => manager.StartSpawning(Wave(enemy, 11)));

            StringAssert.Contains("another spawn operation", exception.Message);
        }

        private SpawnManager CreateSpawnManager(float delay, params SpawnPoint[] points)
        {
            GameObject managerObject = CreateObject("SpawnManager");
            SpawnManager manager = managerObject.AddComponent<SpawnManager>();
            SetPrivateField(manager, "spawnDelay", delay);
            SetPrivateField(manager, "spawnPoints", new List<SpawnPoint>(points));
            return manager;
        }

        private SpawnPoint CreateSpawnPoint(string name, Vector3 position)
        {
            GameObject pointObject = CreateObject(name);
            pointObject.transform.position = position;
            return pointObject.AddComponent<SpawnPoint>();
        }

        private EnemyDefinition Enemy(string name)
        {
            GameObject prefab = CreateObject(name);
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

        private int CountSpawned(EnemyDefinition enemy)
        {
            return CountSpawnedAt(enemy, null);
        }

        private int CountSpawnedAt(EnemyDefinition enemy, Vector3? position)
        {
            int count = 0;
            Transform[] transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude);
            for (int i = 0; i < transforms.Length; i++)
            {
                GameObject gameObject = transforms[i].gameObject;
                if (!gameObject.name.StartsWith($"{enemy.Prefab.name}(Clone)", StringComparison.Ordinal))
                {
                    continue;
                }

                if (!position.HasValue || gameObject.transform.position == position.Value)
                {
                    count++;
                    TrackIfMissing(gameObject);
                }
            }

            return count;
        }

        private int FindSeedUsingBothPoints(IReadOnlyList<EnemyDefinition> enemies, IReadOnlyList<SpawnPoint> points)
        {
            for (int seed = 1; seed < 200; seed++)
            {
                Dictionary<SpawnPoint, List<EnemyDefinition>> assignments =
                    SpawnManager.AssignEnemiesToSpawnPoints(enemies, points, seed);
                if (assignments.Values.Count(queue => queue.Count > 0) == points.Count)
                {
                    return seed;
                }
            }

            Assert.Fail("Could not find deterministic seed that uses all spawn points.");
            return 0;
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
    }
}
