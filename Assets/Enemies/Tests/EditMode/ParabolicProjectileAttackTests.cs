using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RPGame.Core.Damage;
using RPGame.Core.Targeting;
using UnityEditor;
using UnityEngine;

namespace RPGame.Enemies.Tests
{
    public sealed class ParabolicProjectileAttackTests
    {
        private readonly List<GameObject> createdObjects = new();
        private readonly List<ScriptableObject> createdAssets = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(createdObjects[i]);
            }

            for (int i = createdAssets.Count - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(createdAssets[i]);
            }

            createdObjects.Clear();
            createdAssets.Clear();
        }

        [Test]
        public void TryAttack_FirstAttackIsAvailableImmediately()
        {
            ParabolicProjectileAttack attack = CreateAttack(1f, true, true, new Vector2(0.25f, 0.5f), out FakeProjectileLauncher launcher, out _);

            bool attacked = attack.TryAttack(CreateSelectedTarget(new Vector3(10f, 2f, 20f)));

            Assert.IsTrue(attacked);
            Assert.AreEqual(1, launcher.LaunchCount);
        }

        [Test]
        public void TryAttack_BeforeAttackIntervalExpires_IsBlocked()
        {
            ParabolicProjectileAttack attack = CreateAttack(1f, true, true, Vector2.zero, out FakeProjectileLauncher launcher, out _);
            SelectedTarget target = CreateSelectedTarget(Vector3.one);

            Assert.IsTrue(attack.TryAttack(target));
            Assert.IsFalse(attack.TryAttack(target));
            Assert.AreEqual(1, launcher.LaunchCount);
        }

        [Test]
        public void TryAttack_AfterAttackIntervalExpires_IsAllowedAgain()
        {
            ParabolicProjectileAttack attack = CreateAttack(1f, true, true, Vector2.zero, out FakeProjectileLauncher launcher, out _);
            SelectedTarget target = CreateSelectedTarget(Vector3.one);

            Assert.IsTrue(attack.TryAttack(target));
            attack.Tick(1f);

            Assert.IsTrue(attack.TryAttack(target));
            Assert.AreEqual(2, launcher.LaunchCount);
        }

        [Test]
        public void TryAttack_HasCooldownIndependentFromStraightProjectileAttack()
        {
            ParabolicProjectileAttack parabolicAttack = CreateAttack(1f, true, true, Vector2.zero, out _, out _);
            StraightProjectileAttack straightAttack = CreateStraightAttack();
            SelectedTarget target = CreateSelectedTarget(Vector3.one);

            Assert.IsTrue(parabolicAttack.TryAttack(target));
            Assert.IsFalse(parabolicAttack.TryAttack(target));

            Assert.IsTrue(straightAttack.TryAttack(target));
        }

        [Test]
        public void TryAttack_WhenLineOfSightIsBlocked_DoesNotLaunchOrConsumeCooldown()
        {
            ParabolicProjectileAttack attack = CreateAttack(1f, false, true, Vector2.zero, out FakeProjectileLauncher launcher, out FakeGroundProjection groundProjection);
            SelectedTarget target = CreateSelectedTarget(Vector3.one);

            Assert.IsFalse(attack.TryAttack(target));
            Assert.IsTrue(attack.TryAttack(CreateSelectedTarget(new Vector3(2f, 0f, 2f))));

            Assert.AreEqual(1, launcher.LaunchCount);
            Assert.AreEqual(1, groundProjection.ProjectCallCount);
        }

        [Test]
        public void TryAttack_ProjectsRandomPointAroundCurrentTargetPosition()
        {
            Vector3 targetPosition = new(10f, 2f, 20f);
            ParabolicProjectileAttack attack = CreateAttack(1f, true, true, new Vector2(0.25f, -0.5f), out FakeProjectileLauncher launcher, out FakeGroundProjection groundProjection);

            attack.TryAttack(CreateSelectedTarget(targetPosition));

            AssertVector(new Vector3(10.25f, 2f, 19.5f), groundProjection.LastCandidatePosition);
            AssertVector(groundProjection.ProjectedPosition, launcher.LastLaunchData.TargetPosition);
            Assert.LessOrEqual(Vector2.Distance(
                new Vector2(targetPosition.x, targetPosition.z),
                new Vector2(groundProjection.LastCandidatePosition.x, groundProjection.LastCandidatePosition.z)),
                1f);
        }

        [Test]
        public void TryAttack_WhenGroundProjectionFails_DoesNotLaunchOrConsumeCooldown()
        {
            ParabolicProjectileAttack attack = CreateAttack(1f, true, false, Vector2.zero, out FakeProjectileLauncher launcher, out FakeGroundProjection groundProjection);
            SelectedTarget target = CreateSelectedTarget(Vector3.one);

            Assert.IsFalse(attack.TryAttack(target));
            groundProjection.CanProject = true;
            Assert.IsTrue(attack.TryAttack(target));

            Assert.AreEqual(1, launcher.LaunchCount);
        }

        [Test]
        public void TryAttack_TargetMovementAfterLaunchDoesNotChangeImpactPoint()
        {
            SelectedTarget target = CreateSelectedTarget(new Vector3(10f, 0f, 20f));
            ParabolicProjectileAttack attack = CreateAttack(1f, true, true, Vector2.zero, out FakeProjectileLauncher launcher, out FakeGroundProjection groundProjection);

            attack.TryAttack(target);
            target = CreateSelectedTarget(new Vector3(100f, 0f, 200f));

            AssertVector(groundProjection.ProjectedPosition, launcher.LastLaunchData.TargetPosition);
        }

        [Test]
        public void TryAttack_BuildsDamageAndTrajectoryLaunchDataFromConfig()
        {
            ParabolicProjectileAttack attack = CreateAttack(1f, true, true, Vector2.zero, out FakeProjectileLauncher launcher, out EnemyParabolicProjectile prefab, out _);

            attack.TryAttack(CreateSelectedTarget(Vector3.one));

            Assert.AreSame(prefab.gameObject, launcher.LastLaunchData.ProjectilePrefab);
            Assert.AreEqual(1, launcher.LastLaunchData.DamageParts.Count);
            Assert.AreEqual(10f, launcher.LastLaunchData.DamageParts[0].Amount);
            Assert.AreEqual(DamageType.Physical, launcher.LastLaunchData.DamageParts[0].DamageType);
            Assert.AreEqual(DamageElement.None, launcher.LastLaunchData.DamageParts[0].DamageElement);
        }

        [Test]
        public void ParabolicProjectileAttack_UsesGroundProjectionAbstraction()
        {
            FieldInfo[] fields = typeof(ParabolicProjectileAttack)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Assert.IsTrue(fields.Any(field => field.FieldType == typeof(IEnemyGroundProjection)));
            Assert.IsFalse(fields.Any(field => field.FieldType == typeof(GroundProjection)));
            Assert.IsFalse(fields.Any(field => field.FieldType == typeof(Physics)));
        }

        private ParabolicProjectileAttack CreateAttack(
            float attackInterval,
            bool hasLineOfSight,
            bool canProjectGround,
            Vector2 randomOffset,
            out FakeProjectileLauncher launcher,
            out FakeGroundProjection groundProjection)
        {
            return CreateAttack(attackInterval, hasLineOfSight, canProjectGround, randomOffset, out launcher, out _, out groundProjection);
        }

        private ParabolicProjectileAttack CreateAttack(
            float attackInterval,
            bool hasLineOfSight,
            bool canProjectGround,
            Vector2 randomOffset,
            out FakeProjectileLauncher launcher,
            out EnemyParabolicProjectile prefab,
            out FakeGroundProjection groundProjection)
        {
            launcher = new FakeProjectileLauncher();
            groundProjection = new FakeGroundProjection(canProjectGround);
            prefab = CreateParabolicProjectilePrefab();

            return new ParabolicProjectileAttack(
                CreateParabolicConfig(attackInterval, prefab),
                new FakeLineOfSight(hasLineOfSight),
                groundProjection,
                launcher,
                () => null,
                new FakeRandomPointInCircle(randomOffset));
        }

        private StraightProjectileAttack CreateStraightAttack()
        {
            FakeProjectileLauncher launcher = new();
            EnemyStraightProjectile prefab = CreateStraightProjectilePrefab();
            return new StraightProjectileAttack(
                CreateStraightConfig(prefab),
                new FakeLineOfSight(true),
                launcher,
                _ => new FakeDamageable(),
                () => null);
        }

        private ParabolicProjectileAttackConfig CreateParabolicConfig(
            float attackInterval,
            EnemyParabolicProjectile projectilePrefab)
        {
            ParabolicProjectileAttackConfig config = ScriptableObject.CreateInstance<ParabolicProjectileAttackConfig>();
            createdAssets.Add(config);

            SerializedObject serializedConfig = new(config);
            serializedConfig.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab.gameObject;
            serializedConfig.FindProperty("attackInterval").floatValue = attackInterval;
            serializedConfig.FindProperty("targetRandomRadius").floatValue = 1f;
            SetDamage(serializedConfig, 10f);
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        private StraightProjectileAttackConfig CreateStraightConfig(EnemyStraightProjectile projectilePrefab)
        {
            StraightProjectileAttackConfig config = ScriptableObject.CreateInstance<StraightProjectileAttackConfig>();
            createdAssets.Add(config);

            SerializedObject serializedConfig = new(config);
            serializedConfig.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab.gameObject;
            serializedConfig.FindProperty("attackInterval").floatValue = 1f;
            SetDamage(serializedConfig, 10f);
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        private EnemyParabolicProjectile CreateParabolicProjectilePrefab()
        {
            GameObject projectileObject = CreateObject("ParabolicProjectilePrefab");
            projectileObject.AddComponent<RPGame.Combat.Projectiles.ParabolicProjectileMover>();
            return projectileObject.AddComponent<EnemyParabolicProjectile>();
        }

        private EnemyStraightProjectile CreateStraightProjectilePrefab()
        {
            GameObject projectileObject = CreateObject("StraightProjectilePrefab");
            projectileObject.AddComponent<RPGame.Combat.Projectiles.StraightProjectileMover>();
            return projectileObject.AddComponent<EnemyStraightProjectile>();
        }

        private static void SetDamage(SerializedObject serializedConfig, float damageAmount)
        {
            SerializedProperty damageProperty = serializedConfig.FindProperty("damage");
            damageProperty.arraySize = 1;
            SerializedProperty damageEntry = damageProperty.GetArrayElementAtIndex(0);
            damageEntry.FindPropertyRelative("minDamage").floatValue = damageAmount;
            damageEntry.FindPropertyRelative("maxDamage").floatValue = damageAmount;
            damageEntry.FindPropertyRelative("damageType").enumValueIndex = (int)DamageType.Physical;
            damageEntry.FindPropertyRelative("damageElement").enumValueIndex = (int)DamageElement.None;
        }

        private GameObject CreateObject(string objectName)
        {
            GameObject gameObject = new(objectName);
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private static SelectedTarget CreateSelectedTarget(Vector3 position)
        {
            return new SelectedTarget(new FakeTargetable(), position);
        }

        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, 0.0001f);
            Assert.AreEqual(expected.y, actual.y, 0.0001f);
            Assert.AreEqual(expected.z, actual.z, 0.0001f);
        }

        private sealed class FakeLineOfSight : IEnemyLineOfSight
        {
            private bool hasLineOfSight;

            public FakeLineOfSight(bool hasLineOfSight)
            {
                this.hasLineOfSight = hasLineOfSight;
            }

            public bool HasLineOfSight(Vector3 targetPosition)
            {
                bool result = hasLineOfSight;
                hasLineOfSight = true;
                return result;
            }

            public bool HasLineOfSightFrom(Vector3 origin, Vector3 targetPosition)
            {
                return hasLineOfSight;
            }
        }

        private sealed class FakeGroundProjection : IEnemyGroundProjection
        {
            public FakeGroundProjection(bool canProject)
            {
                CanProject = canProject;
                ProjectedPosition = new Vector3(4f, 0f, 6f);
            }

            public bool CanProject { get; set; }
            public int ProjectCallCount { get; private set; }
            public Vector3 LastCandidatePosition { get; private set; }
            public Vector3 ProjectedPosition { get; }

            public bool TryProjectToGround(Vector3 candidatePosition, out Vector3 groundPosition)
            {
                ProjectCallCount++;
                LastCandidatePosition = candidatePosition;
                groundPosition = ProjectedPosition;
                return CanProject;
            }
        }

        private sealed class FakeProjectileLauncher : IProjectileLauncher
        {
            public int LaunchCount { get; private set; }
            public ProjectileLaunchData LastLaunchData { get; private set; }

            public bool Launch(ProjectileLaunchData data)
            {
                LaunchCount++;
                LastLaunchData = data;
                return true;
            }
        }

        private sealed class FakeRandomPointInCircle : IRandomPointInCircle
        {
            private readonly Vector2 offset;

            public FakeRandomPointInCircle(Vector2 offset)
            {
                this.offset = offset;
            }

            public Vector2 NextPoint(float radius)
            {
                return offset * radius;
            }
        }

        private sealed class FakeDamageable : IDamageable
        {
            public bool CanReceiveDamage => true;

            public DamageResult ApplyDamage(DamageData data)
            {
                return DamageResult.Applied(data, data.Amount, 100f, 100f - data.Amount, false);
            }
        }

        private sealed class FakeTargetable : ITargetable
        {
            public Transform TargetPoint => null;
        }
    }
}
