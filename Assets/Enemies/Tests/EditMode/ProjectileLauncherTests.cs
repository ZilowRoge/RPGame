using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RPGame.Combat.Projectiles;
using RPGame.Core.Damage;
using UnityEditor;
using UnityEngine;

namespace RPGame.Enemies.Tests
{
    public sealed class ProjectileLauncherTests
    {
        private readonly List<GameObject> createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(createdObjects[i]);
            }

            createdObjects.Clear();
        }

        [Test]
        public void ProjectileLauncher_DoesNotOwnCooldownOrAttackDecision()
        {
            FieldInfo[] fields = typeof(ProjectileLauncher)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Assert.IsFalse(fields.Any(field => field.Name.ToLowerInvariant().Contains("cooldown")));
            Assert.IsFalse(fields.Any(field => field.FieldType == typeof(Config)));
            Assert.IsFalse(fields.Any(field => field.FieldType == typeof(AttackConfig)));
        }

        [Test]
        public void Launch_UsesProjectilePrefabFromLaunchData()
        {
            ProjectileLauncher launcher = CreateLauncher(out Transform spawnPoint);
            EnemyStraightProjectile projectilePrefab = CreateProjectilePrefab("ConfigProjectilePrefab");
            TestDamageable target = CreateTarget();

            bool launched = launcher.Launch(new ProjectileLaunchData(
                projectilePrefab,
                new Vector3(0f, 0f, 10f),
                target,
                CreateDamageParts(10f),
                null,
                6f,
                5f));

            EnemyStraightProjectile spawnedProjectile = FindSpawnedProjectile(projectilePrefab);
            createdObjects.Add(spawnedProjectile.gameObject);

            Assert.IsTrue(launched);
            Assert.AreEqual(spawnPoint.position, spawnedProjectile.transform.position);
            Assert.AreEqual(6f, spawnedProjectile.CurrentSpeed);
            Assert.IsTrue(spawnedProjectile.IsInitialized);
        }

        private ProjectileLauncher CreateLauncher(out Transform spawnPoint)
        {
            GameObject launcherObject = CreateObject("Launcher");
            ProjectileLauncher launcher = launcherObject.AddComponent<ProjectileLauncher>();
            spawnPoint = CreateObject("SpawnPoint").transform;
            spawnPoint.position = new Vector3(1f, 2f, 3f);

            SerializedObject serializedLauncher = new(launcher);
            serializedLauncher.FindProperty("projectileSpawnPoint").objectReferenceValue = spawnPoint;
            serializedLauncher.ApplyModifiedPropertiesWithoutUndo();
            return launcher;
        }

        private EnemyStraightProjectile CreateProjectilePrefab(string objectName)
        {
            GameObject projectileObject = CreateObject(objectName);
            projectileObject.AddComponent<StraightProjectileMover>();
            return projectileObject.AddComponent<EnemyStraightProjectile>();
        }

        private TestDamageable CreateTarget()
        {
            GameObject targetObject = CreateObject("Target");
            return targetObject.AddComponent<TestDamageable>();
        }

        private GameObject CreateObject(string objectName)
        {
            GameObject gameObject = new(objectName);
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private static EnemyStraightProjectile FindSpawnedProjectile(EnemyStraightProjectile projectilePrefab)
        {
            return UnityEngine.Object
                .FindObjectsByType<EnemyStraightProjectile>()
                .First(projectile => projectile != projectilePrefab);
        }

        private static IReadOnlyList<PartialDamage> CreateDamageParts(float amount)
        {
            return new[]
            {
                new PartialDamage(amount, DamageType.Physical, DamageElement.None)
            };
        }

        public sealed class TestDamageable : MonoBehaviour, IDamageable
        {
            public bool CanReceiveDamage => true;

            public DamageResult ApplyDamage(DamageData data)
            {
                return DamageResult.Applied(data, data.Amount, 100f, 100f - data.Amount, false);
            }
        }
    }
}
