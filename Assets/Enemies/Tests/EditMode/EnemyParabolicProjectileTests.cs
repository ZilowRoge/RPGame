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
    public sealed class EnemyParabolicProjectileTests
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
        public void EnemyParabolicProjectile_UsesSharedProjectileLifecycle()
        {
            Assert.IsTrue(typeof(EnemyProjectile).IsAssignableFrom(typeof(EnemyParabolicProjectile)));

            MethodInfo[] methods = typeof(EnemyParabolicProjectile)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            Assert.IsFalse(methods.Any(method => method.Name == "Tick"));
            Assert.IsFalse(methods.Any(method => method.Name == "HandleHit"));
        }

        [Test]
        public void EnemyParabolicProjectile_RequiresParabolicProjectileMover()
        {
            bool requiresMover = typeof(EnemyParabolicProjectile)
                .GetCustomAttributes<RequireComponent>()
                .Any(attribute =>
                    attribute.m_Type0 == typeof(ParabolicProjectileMover)
                    || attribute.m_Type1 == typeof(ParabolicProjectileMover)
                    || attribute.m_Type2 == typeof(ParabolicProjectileMover));

            Assert.IsTrue(requiresMover);
        }

        [Test]
        public void EnemyParabolicProjectile_ImplementsSharedProjectileInterface()
        {
            Assert.IsInstanceOf<IEnemyProjectile>(CreateProjectile(Vector3.zero));
        }

        [Test]
        public void Initialize_StartsAtStartPointAndCalculatesApex()
        {
            Vector3 start = new(2f, 1f, -1f);
            Vector3 impact = new(10f, 1f, -1f);
            EnemyParabolicProjectile projectile = CreateProjectile(start);
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f);

            projectile.Initialize(impact, null, CreateDamageParts(), null);

            AssertVector(start, projectile.transform.position);
            AssertVector(new Vector3(6f, 5f, -1f), projectile.ApexPoint);
            AssertVector(impact, projectile.ImpactPoint);
            Assert.AreEqual(ParabolicProjectilePhase.Ascending, projectile.Phase);
        }

        [Test]
        public void Tick_ReachesApexOnceWithZeroSpeedAndStartsDescent()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            int eventCount = 0;
            projectile.ApexReached += _ => eventCount++;
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);

            projectile.Tick(1f);
            projectile.Tick(0f);

            AssertVector(new Vector3(5f, 4f, 0f), projectile.ApexPoint);
            Assert.IsTrue(projectile.HasReachedApex);
            Assert.AreEqual(1, projectile.ApexReachedCount);
            Assert.AreEqual(1, eventCount);
            Assert.AreEqual(0f, projectile.CurrentSpeed, 0.0001f);
            Assert.AreEqual(ParabolicProjectilePhase.Descending, projectile.Phase);
        }

        [Test]
        public void Tick_AscentSpeedIncreasesThenFallsToZeroAtApex()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);

            projectile.Tick(0.25f);
            float earlySpeed = projectile.CurrentSpeed;
            projectile.Tick(0.25f);
            float middleSpeed = projectile.CurrentSpeed;
            projectile.Tick(0.25f);
            float lateSpeed = projectile.CurrentSpeed;
            projectile.Tick(0.25f);

            Assert.Greater(middleSpeed, earlySpeed);
            Assert.Less(lateSpeed, middleSpeed);
            Assert.AreEqual(0f, projectile.CurrentSpeed, 0.0001f);
        }

        [Test]
        public void Tick_DescentStartsAfterApexAndAccelerates()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);

            projectile.Tick(1f);
            projectile.Tick(0.25f);
            float earlyDescentSpeed = projectile.CurrentSpeed;
            projectile.Tick(0.25f);
            float laterDescentSpeed = projectile.CurrentSpeed;

            Assert.AreEqual(ParabolicProjectilePhase.Descending, projectile.Phase);
            Assert.Greater(laterDescentSpeed, earlyDescentSpeed);
        }

        [Test]
        public void Tick_CompletesTrajectoryAtImpactPoint()
        {
            Vector3 impact = new(10f, 0f, 0f);
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f);
            projectile.Initialize(impact, null, CreateDamageParts(), null);

            projectile.Tick(1f);
            projectile.Tick(1f);

            Assert.IsTrue(projectile.IsFinished);
            Assert.IsTrue(projectile.HasImpact);
            AssertVector(impact, projectile.transform.position);
            AssertVector(impact, projectile.LastImpact.Point);
            AssertVector(Vector3.up, projectile.LastImpact.Normal);
            Assert.IsNull(projectile.LastImpact.Collider);
        }

        [Test]
        public void Tick_IsFrameRateIndependent()
        {
            Vector3 impact = new(10f, 0f, 0f);
            EnemyParabolicProjectile singleStep = CreateProjectile(Vector3.zero);
            EnemyParabolicProjectile multipleSteps = CreateProjectile(Vector3.zero);
            ConfigureProjectile(singleStep, 10f, 4f, 1f, 1f);
            ConfigureProjectile(multipleSteps, 10f, 4f, 1f, 1f);
            singleStep.Initialize(impact, null, CreateDamageParts(), null);
            multipleSteps.Initialize(impact, null, CreateDamageParts(), null);

            singleStep.Tick(0.75f);
            multipleSteps.Tick(0.25f);
            multipleSteps.Tick(0.25f);
            multipleSteps.Tick(0.25f);

            AssertVector(singleStep.transform.position, multipleSteps.transform.position);
            Assert.AreEqual(singleStep.CurrentSpeed, multipleSteps.CurrentSpeed, 0.0001f);
        }

        [Test]
        public void Tick_WhenDeltaTimeCrossesApex_ConsumesRemainingTimeInDescent()
        {
            Vector3 impact = new(10f, 0f, 0f);
            EnemyParabolicProjectile singleStep = CreateProjectile(Vector3.zero);
            EnemyParabolicProjectile splitStep = CreateProjectile(Vector3.zero);
            int singleStepApexCount = 0;
            int splitStepApexCount = 0;
            Vector3 singleStepPositionAtApex = Vector3.zero;
            float singleStepSpeedAtApex = -1f;
            singleStep.ApexReached += projectile =>
            {
                singleStepApexCount++;
                singleStepPositionAtApex = projectile.transform.position;
                singleStepSpeedAtApex = projectile.CurrentSpeed;
            };

            splitStep.ApexReached += _ => splitStepApexCount++;
            ConfigureProjectile(singleStep, 10f, 4f, 1f, 1f);
            ConfigureProjectile(splitStep, 10f, 4f, 1f, 1f);
            singleStep.Initialize(impact, null, CreateDamageParts(), null);
            splitStep.Initialize(impact, null, CreateDamageParts(), null);

            singleStep.Tick(1.25f);
            splitStep.Tick(0.5f);
            splitStep.Tick(0.5f);
            splitStep.Tick(0.25f);

            Assert.AreEqual(1, singleStepApexCount);
            Assert.AreEqual(1, splitStepApexCount);
            Assert.AreEqual(1, singleStep.ApexReachedCount);
            Assert.AreEqual(1, splitStep.ApexReachedCount);
            Assert.AreEqual(ParabolicProjectilePhase.Descending, singleStep.Phase);
            AssertVector(singleStep.ApexPoint, singleStepPositionAtApex);
            Assert.AreEqual(0f, singleStepSpeedAtApex, 0.0001f);
            AssertVector(splitStep.transform.position, singleStep.transform.position);
            Assert.AreEqual(splitStep.CurrentSpeed, singleStep.CurrentSpeed, 0.0001f);
            Assert.AreNotEqual(singleStep.ApexPoint, singleStep.transform.position);
        }

        [Test]
        public void Tick_EarlyCollisionFinishesBeforePlannedImpactAndKeepsHitData()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            GameObject wall = CreateEnvironment("Wall", new Vector3(5f, 3.9f, 0f), new Vector3(1f, 1f, 1f));
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);
            Physics.SyncTransforms();

            projectile.Tick(1f);

            Assert.IsTrue(projectile.IsFinished);
            Assert.AreSame(wall.GetComponent<Collider>(), projectile.LastImpact.Collider);
            Assert.AreNotEqual(new Vector3(10f, 0f, 0f), projectile.transform.position);
            Assert.AreNotEqual(Vector3.zero, projectile.LastImpact.Point);
            Assert.AreNotEqual(Vector3.zero, projectile.LastImpact.Normal);
        }

        [Test]
        public void Tick_LifetimeStillFinishesProjectile()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            ConfigureProjectile(projectile, 0.5f, 4f, 1f, 1f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);

            projectile.Tick(0.5f);

            Assert.IsTrue(projectile.IsFinished);
            Assert.IsFalse(projectile.HasImpact);
        }

        [Test]
        public void StraightProjectileMover_StillUsesConfiguredSpeed()
        {
            GameObject projectileObject = CreateObject("StraightProjectile");
            StraightProjectileMover mover = projectileObject.AddComponent<StraightProjectileMover>();
            TestMovementSource movementSource = new(6f);
            mover.Initialize(movementSource);

            mover.Tick(0.5f);

            AssertVector(new Vector3(0f, 0f, 3f), projectileObject.transform.position);
        }

        private EnemyParabolicProjectile CreateProjectile(Vector3 position)
        {
            GameObject projectileObject = CreateObject("ParabolicProjectile");
            projectileObject.transform.position = position;
            projectileObject.AddComponent<ParabolicProjectileMover>();
            return projectileObject.AddComponent<EnemyParabolicProjectile>();
        }

        private static void ConfigureProjectile(
            EnemyParabolicProjectile projectile,
            float projectileLifetime,
            float arcHeight,
            float ascentDuration,
            float descentDuration)
        {
            SerializedObject serializedProjectile = new(projectile);
            serializedProjectile.FindProperty("projectileLifetime").floatValue = projectileLifetime;
            serializedProjectile.FindProperty("arcHeight").floatValue = arcHeight;
            serializedProjectile.FindProperty("ascentDuration").floatValue = ascentDuration;
            serializedProjectile.FindProperty("descentDuration").floatValue = descentDuration;
            serializedProjectile.ApplyModifiedPropertiesWithoutUndo();
        }

        private GameObject CreateEnvironment(string objectName, Vector3 position, Vector3 scale)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.name = objectName;
            gameObject.transform.SetPositionAndRotation(position, Quaternion.identity);
            gameObject.transform.localScale = scale;
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private GameObject CreateObject(string objectName)
        {
            GameObject gameObject = new(objectName);
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private static IReadOnlyList<PartialDamage> CreateDamageParts()
        {
            return new[]
            {
                new PartialDamage(1f, DamageType.Physical, DamageElement.None)
            };
        }

        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, 0.0001f);
            Assert.AreEqual(expected.y, actual.y, 0.0001f);
            Assert.AreEqual(expected.z, actual.z, 0.0001f);
        }

        private sealed class TestMovementSource : IProjectileMovementSource
        {
            public TestMovementSource(float currentSpeed)
            {
                CurrentSpeed = currentSpeed;
            }

            public float CurrentSpeed { get; }
        }
    }
}
