using System;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPGame.Enemies.Tests
{
    public sealed class PooledEnemyTests
    {
        private GameObject gameObject;
        private PooledEnemy pooledEnemy;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("PooledEnemy");
            pooledEnemy = gameObject.AddComponent<PooledEnemy>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void IsSpawned_WhenCreated_IsFalse()
        {
            Assert.IsFalse(pooledEnemy.IsSpawned);
        }

        [Test]
        public void OnSpawned_WhenNotSpawned_SetsIsSpawnedTrue()
        {
            pooledEnemy.OnSpawned();

            Assert.IsTrue(pooledEnemy.IsSpawned);
        }

        [Test]
        public void OnDespawned_WhenSpawned_SetsIsSpawnedFalse()
        {
            pooledEnemy.OnSpawned();

            pooledEnemy.OnDespawned();

            Assert.IsFalse(pooledEnemy.IsSpawned);
        }

        [Test]
        public void OnSpawned_WhenAlreadySpawned_ThrowsClearly()
        {
            pooledEnemy.OnSpawned();

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => pooledEnemy.OnSpawned());

            StringAssert.Contains("already spawned", exception.Message);
        }

        [Test]
        public void OnDespawned_WhenNotSpawned_ThrowsClearly()
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => pooledEnemy.OnDespawned());

            StringAssert.Contains("not spawned", exception.Message);
        }

        [Test]
        public void PooledEnemy_WhenSpawnedDespawnedTwice_Works()
        {
            pooledEnemy.OnSpawned();
            pooledEnemy.OnDespawned();
            pooledEnemy.OnSpawned();
            pooledEnemy.OnDespawned();

            Assert.IsFalse(pooledEnemy.IsSpawned);
        }
    }
}
