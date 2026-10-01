using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RPGame.Enemies;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPGame.Encounter.Tests
{
    public sealed class EnemyPoolTests
    {
        private readonly List<GameObject> createdObjects = new();
        private readonly List<GameObject> prefabObjects = new();
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
            prefabObjects.Clear();
            createdAssets.Clear();
        }

        [Test]
        public void Prewarm_CreatesConfiguredInactiveInstances()
        {
            EnemyPool pool = CreatePool();
            EnemyDefinition definition = Enemy("PrewarmEnemy", 3);

            pool.Prewarm(new[] { definition });

            PooledEnemy[] instances = FindPooledEnemies();
            Assert.AreEqual(3, instances.Length);
            for (int i = 0; i < instances.Length; i++)
            {
                Assert.IsFalse(instances[i].gameObject.activeSelf);
            }
        }

        [Test]
        public void Prewarm_DoesNotCallOnSpawned()
        {
            EnemyPool pool = CreatePool();
            EnemyDefinition definition = Enemy("InactivePrewarmEnemy", 1);

            pool.Prewarm(new[] { definition });

            Assert.IsFalse(FindPooledEnemies()[0].IsSpawned);
        }

        [Test]
        public void Acquire_ReusesPrewarmedInstance()
        {
            EnemyPool pool = CreatePool();
            EnemyDefinition definition = Enemy("ReusePrewarmEnemy", 1);
            pool.Prewarm(new[] { definition });
            PooledEnemy prewarmed = FindPooledEnemies()[0];

            PooledEnemy acquired = pool.Acquire(definition, Vector3.one, Quaternion.identity);

            Assert.AreSame(prewarmed, acquired);
        }

        [Test]
        public void Acquire_WhenNoAvailableInstance_GrowsPool()
        {
            EnemyPool pool = CreatePool();
            EnemyDefinition definition = Enemy("GrowEnemy", 1);
            pool.Prewarm(new[] { definition });

            pool.Acquire(definition, Vector3.zero, Quaternion.identity);
            pool.Acquire(definition, Vector3.right, Quaternion.identity);

            Assert.AreEqual(2, FindPooledEnemies().Length);
        }

        [Test]
        public void Acquire_PositionsAndRotatesInstance()
        {
            EnemyPool pool = CreatePool();
            EnemyDefinition definition = Enemy("TransformEnemy", 0);
            Vector3 position = new(1f, 2f, 3f);
            Quaternion rotation = Quaternion.Euler(0f, 45f, 0f);

            PooledEnemy acquired = pool.Acquire(definition, position, rotation);

            Assert.AreEqual(position, acquired.transform.position);
            Assert.AreEqual(rotation, acquired.transform.rotation);
        }

        [Test]
        public void Acquire_ActivatesAndMarksSpawned()
        {
            EnemyPool pool = CreatePool();
            EnemyDefinition definition = Enemy("ActiveEnemy", 1);
            pool.Prewarm(new[] { definition });

            PooledEnemy acquired = pool.Acquire(definition, Vector3.zero, Quaternion.identity);

            Assert.IsTrue(acquired.gameObject.activeSelf);
            Assert.IsTrue(acquired.IsSpawned);
        }

        [Test]
        public void Release_DespawnsAndDeactivatesInstance()
        {
            EnemyPool pool = CreatePool();
            EnemyDefinition definition = Enemy("ReleaseEnemy", 0);
            PooledEnemy acquired = pool.Acquire(definition, Vector3.zero, Quaternion.identity);

            pool.Release(acquired);

            Assert.IsFalse(acquired.IsSpawned);
            Assert.IsFalse(acquired.gameObject.activeSelf);
        }

        [Test]
        public void Release_ReturnsInstanceForNextAcquire()
        {
            EnemyPool pool = CreatePool();
            EnemyDefinition definition = Enemy("ReleaseReuseEnemy", 0);
            PooledEnemy first = pool.Acquire(definition, Vector3.zero, Quaternion.identity);
            pool.Release(first);

            PooledEnemy second = pool.Acquire(definition, Vector3.right, Quaternion.identity);

            Assert.AreSame(first, second);
        }

        [Test]
        public void DifferentDefinitions_DoNotShareInstances()
        {
            EnemyPool pool = CreatePool();
            EnemyDefinition firstDefinition = Enemy("FirstDefinitionEnemy", 1);
            EnemyDefinition secondDefinition = Enemy("SecondDefinitionEnemy", 1);
            pool.Prewarm(new[] { firstDefinition, secondDefinition });

            PooledEnemy first = pool.Acquire(firstDefinition, Vector3.zero, Quaternion.identity);
            PooledEnemy second = pool.Acquire(secondDefinition, Vector3.zero, Quaternion.identity);

            Assert.AreNotSame(first, second);
        }

        [Test]
        public void Release_WhenCalledTwice_FailsClearly()
        {
            EnemyPool pool = CreatePool();
            EnemyDefinition definition = Enemy("DoubleReleaseEnemy", 0);
            PooledEnemy acquired = pool.Acquire(definition, Vector3.zero, Quaternion.identity);
            pool.Release(acquired);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => pool.Release(acquired));

            StringAssert.Contains("not currently acquired", exception.Message);
        }

        [Test]
        public void Release_WhenInstanceIsForeign_FailsClearly()
        {
            EnemyPool pool = CreatePool();
            PooledEnemy foreign = CreateObject("ForeignEnemy").AddComponent<PooledEnemy>();

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => pool.Release(foreign));

            StringAssert.Contains("does not belong", exception.Message);
        }

        [Test]
        public void Acquire_WhenPrefabHasNoPooledEnemy_FailsClearly()
        {
            EnemyPool pool = CreatePool();
            EnemyDefinition definition = EnemyWithoutPooledEnemy("InvalidEnemy");

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => pool.Acquire(definition, Vector3.zero, Quaternion.identity));

            StringAssert.Contains("PooledEnemy", exception.Message);
        }

        [Test]
        public void Acquire_WhenPooledEnemyIsOnlyOnChild_FailsClearly()
        {
            EnemyPool pool = CreatePool();
            EnemyDefinition definition = EnemyWithChildPooledEnemy("ChildOnlyEnemy");

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => pool.Acquire(definition, Vector3.zero, Quaternion.identity));

            StringAssert.Contains("prefab root", exception.Message);
        }

        private EnemyPool CreatePool()
        {
            GameObject poolObject = CreateObject("EnemyPool");
            return poolObject.AddComponent<EnemyPool>();
        }

        private EnemyDefinition Enemy(string name, int prewarmCount)
        {
            GameObject prefab = CreateObject(name);
            prefabObjects.Add(prefab);
            prefab.AddComponent<PooledEnemy>();
            return CreateDefinition(prefab, prewarmCount);
        }

        private EnemyDefinition EnemyWithoutPooledEnemy(string name)
        {
            GameObject prefab = CreateObject(name);
            prefabObjects.Add(prefab);
            return CreateDefinition(prefab, 0);
        }

        private EnemyDefinition EnemyWithChildPooledEnemy(string name)
        {
            GameObject prefab = CreateObject(name);
            prefabObjects.Add(prefab);
            GameObject child = CreateObject($"{name}Child");
            child.transform.SetParent(prefab.transform);
            child.AddComponent<PooledEnemy>();
            return CreateDefinition(prefab, 0);
        }

        private EnemyDefinition CreateDefinition(GameObject prefab, int prewarmCount)
        {
            EnemyDefinition definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            createdAssets.Add(definition);
            SetPrivateField(definition, "prefab", prefab);
            SetPrivateField(definition, "cost", 1);
            SetPrivateField(definition, "unlockWave", 1);
            SetPrivateField(definition, "prewarmCount", prewarmCount);
            return definition;
        }

        private PooledEnemy[] FindPooledEnemies()
        {
            PooledEnemy[] enemies = Object.FindObjectsByType<PooledEnemy>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.InstanceID);

            List<PooledEnemy> instances = new();
            for (int i = 0; i < enemies.Length; i++)
            {
                if (!prefabObjects.Contains(enemies[i].gameObject))
                {
                    TrackIfMissing(enemies[i].gameObject);
                    instances.Add(enemies[i]);
                }
            }

            return instances.ToArray();
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
