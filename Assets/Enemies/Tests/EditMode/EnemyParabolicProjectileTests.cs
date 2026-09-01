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
        public void Tick_DoesNotCreateTelegraphBeforeApex()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            GameObject telegraphPrefab = CreateObject("TelegraphPrefab");
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f, telegraphPrefab, 2f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);

            projectile.Tick(0.5f);

            Assert.IsNull(projectile.ActiveTelegraph);
        }

        [Test]
        public void Tick_WhenApexIsReached_CreatesTelegraphOnceAtPlannedImpactPoint()
        {
            Vector3 impact = new(10f, 0f, 0f);
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            GameObject telegraphPrefab = CreateObject("TelegraphPrefab");
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f, telegraphPrefab, 2f);
            projectile.Initialize(impact, null, CreateDamageParts(), null);

            projectile.Tick(1f);
            GameObject firstTelegraph = projectile.ActiveTelegraph;
            projectile.Tick(0.25f);

            Assert.IsNotNull(firstTelegraph);
            Assert.AreSame(firstTelegraph, projectile.ActiveTelegraph);
            AssertVector(impact + Vector3.up * 0.02f, firstTelegraph.transform.position);
            AssertVector(new Vector3(4f, 1f, 4f), firstTelegraph.transform.localScale);
            Assert.AreEqual(1, projectile.ApexReachedCount);
        }

        [Test]
        public void Tick_TelegraphKeepsPlannedImpactPoint()
        {
            Vector3 plannedImpact = new(10f, 0f, 0f);
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            GameObject telegraphPrefab = CreateObject("TelegraphPrefab");
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f, telegraphPrefab, 2f);
            projectile.Initialize(plannedImpact, null, CreateDamageParts(), null);

            projectile.Tick(1f);
            projectile.Tick(0.25f);

            AssertVector(plannedImpact + Vector3.up * 0.02f, projectile.ActiveTelegraph.transform.position);
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
        public void Tick_NormalImpactRemovesTelegraph()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            GameObject telegraphPrefab = CreateObject("TelegraphPrefab");
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f, telegraphPrefab, 2f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);

            projectile.Tick(1f);
            Assert.IsNotNull(projectile.ActiveTelegraph);

            projectile.Tick(1f);

            Assert.IsTrue(projectile.IsFinished);
            Assert.IsNull(projectile.ActiveTelegraph);
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
        public void Tick_EarlyCollisionAfterApexRemovesTelegraph()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            GameObject telegraphPrefab = CreateObject("TelegraphPrefab");
            GameObject wall = CreateEnvironment("Wall", new Vector3(5.75f, 3.9f, 0f), new Vector3(0.5f, 2f, 1f));
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f, telegraphPrefab, 2f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);
            Physics.SyncTransforms();

            projectile.Tick(1f);
            Assert.IsNotNull(projectile.ActiveTelegraph);

            projectile.Tick(0.5f);

            Assert.IsTrue(projectile.IsFinished);
            Assert.AreSame(wall.GetComponent<Collider>(), projectile.LastImpact.Collider);
            Assert.IsNull(projectile.ActiveTelegraph);
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
        public void Tick_LifetimeAfterApexRemovesTelegraph()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            GameObject telegraphPrefab = CreateObject("TelegraphPrefab");
            ConfigureProjectile(projectile, 1.25f, 4f, 1f, 1f, telegraphPrefab, 2f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);

            projectile.Tick(1f);
            Assert.IsNotNull(projectile.ActiveTelegraph);

            projectile.Tick(0.25f);

            Assert.IsTrue(projectile.IsFinished);
            Assert.IsNull(projectile.ActiveTelegraph);
        }

        [Test]
        public void Tick_LifetimeBeforeApexDoesNotCreateTelegraph()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            GameObject telegraphPrefab = CreateObject("TelegraphPrefab");
            ConfigureProjectile(projectile, 0.5f, 4f, 1f, 1f, telegraphPrefab, 2f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);

            projectile.Tick(0.5f);

            Assert.IsTrue(projectile.IsFinished);
            Assert.IsNull(projectile.ActiveTelegraph);
            Assert.AreEqual(0, projectile.ApexReachedCount);
        }

        [Test]
        public void SourceDestroy_DoesNotRemoveProjectileOrTelegraph()
        {
            GameObject source = CreateObject("Source");
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            GameObject telegraphPrefab = CreateObject("TelegraphPrefab");
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f, telegraphPrefab, 2f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), source);
            projectile.Tick(1f);

            UnityEngine.Object.DestroyImmediate(source);

            Assert.IsFalse(projectile.IsFinished);
            Assert.IsNotNull(projectile.ActiveTelegraph);
        }

        [Test]
        public void Tick_NormalImpactAppliesAoEAtPlannedImpactPoint()
        {
            Vector3 impact = new(10f, 0f, 0f);
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            TestDamageable target = CreateDamageable("Target", impact + new Vector3(0.5f, 0f, 0f));
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f, null, 2f);
            projectile.Initialize(impact, null, CreateDamageParts(7f), null);
            Physics.SyncTransforms();

            projectile.Tick(1f);
            projectile.Tick(1f);

            Assert.IsTrue(projectile.IsFinished);
            Assert.AreEqual(1, target.ApplyDamageCount);
            Assert.AreEqual(7f, target.LastDamageData.Amount);
            AssertVector(impact, projectile.LastImpact.Point);
        }

        [Test]
        public void HandleHit_EarlyCollisionAppliesAoEAtRealHitPoint()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            GameObject wall = CreateEnvironment("Wall", new Vector3(2f, 0f, 0f), Vector3.one);
            TestDamageable nearRealHit = CreateDamageable("NearRealHit", new Vector3(2.4f, 0f, 0f));
            TestDamageable nearPlannedImpact = CreateDamageable("NearPlannedImpact", new Vector3(10f, 0f, 0f));
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f, null, 1f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);
            projectile.transform.position = wall.transform.position;
            Physics.SyncTransforms();

            projectile.HandleHit(wall.GetComponent<Collider>());

            Assert.IsTrue(projectile.IsFinished);
            Assert.AreEqual(1, nearRealHit.ApplyDamageCount);
            Assert.AreEqual(0, nearPlannedImpact.ApplyDamageCount);
            AssertVector(wall.transform.position, projectile.LastImpact.Point);
        }

        [Test]
        public void Tick_TargetOutsideAoERadiusDoesNotReceiveDamage()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            TestDamageable target = CreateDamageable("Target", new Vector3(12.5f, 0f, 0f));
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f, null, 1f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);
            Physics.SyncTransforms();

            projectile.Tick(1f);
            projectile.Tick(1f);

            Assert.AreEqual(0, target.ApplyDamageCount);
        }

        [Test]
        public void Tick_AoEDamagesMultipleDamageablesWithoutFactionFiltering()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            TestDamageable player = CreateDamageable("Player", new Vector3(9.5f, 0f, 0f));
            TestDamageable enemy = CreateDamageable("Enemy", new Vector3(10.5f, 0f, 0f));
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f, null, 2f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);
            Physics.SyncTransforms();

            projectile.Tick(1f);
            projectile.Tick(1f);

            Assert.AreEqual(1, player.ApplyDamageCount);
            Assert.AreEqual(1, enemy.ApplyDamageCount);
        }

        [Test]
        public void Tick_AoEDoesNotDamageSource()
        {
            GameObject source = CreateDamageable("Source", new Vector3(10f, 0f, 0f)).gameObject;
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            TestDamageable other = CreateDamageable("Other", new Vector3(10.5f, 0f, 0f));
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f, null, 2f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), source);
            Physics.SyncTransforms();

            projectile.Tick(1f);
            projectile.Tick(1f);

            Assert.AreEqual(0, source.GetComponent<TestDamageable>().ApplyDamageCount);
            Assert.AreEqual(1, other.ApplyDamageCount);
        }

        [Test]
        public void Tick_TargetWithMultipleCollidersReceivesAoEDamageOnce()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            TestDamageable target = CreateDamageableWithChildCollider("Target", new Vector3(10f, 0f, 0f));
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f, null, 2f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);
            Physics.SyncTransforms();

            projectile.Tick(1f);
            projectile.Tick(1f);

            Assert.AreEqual(1, target.ApplyDamageCount);
        }

        [Test]
        public void Tick_ObstacleBetweenImpactAndTargetBlocksAoEDamage()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            TestDamageable blocked = CreateDamageable("Blocked", new Vector3(13f, 0f, 0f));
            TestDamageable visible = CreateDamageable("Visible", new Vector3(10f, 0f, 1f));
            CreateEnvironment("Wall", new Vector3(11.5f, 0f, 0f), new Vector3(0.2f, 3f, 3f));
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f, null, 4f, LayerMask.GetMask("Default"));
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);
            Physics.SyncTransforms();

            projectile.Tick(1f);
            projectile.Tick(1f);

            Assert.AreEqual(0, blocked.ApplyDamageCount);
            Assert.AreEqual(1, visible.ApplyDamageCount);
        }

        [Test]
        public void Tick_AoEDamageUsesPreparedDamageParts()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            TestDamageable target = CreateDamageable("Target", new Vector3(10f, 0f, 0f));
            IReadOnlyList<PartialDamage> damageParts = new[]
            {
                new PartialDamage(3f, DamageType.Physical, DamageElement.None),
                new PartialDamage(4f, DamageType.Magical, DamageElement.Fire)
            };
            ConfigureProjectile(projectile, 10f, 4f, 1f, 1f, null, 2f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, damageParts, null);
            Physics.SyncTransforms();

            projectile.Tick(1f);
            projectile.Tick(1f);

            Assert.AreEqual(1, target.ApplyDamageCount);
            Assert.AreEqual(7f, target.LastDamageData.Amount);
            Assert.AreEqual(2, target.LastDamageData.Parts.Count);
            Assert.AreEqual(DamageElement.Fire, target.LastDamageData.Parts[1].DamageElement);
        }

        [Test]
        public void Tick_LifetimeExpiryDoesNotApplyAoEDamage()
        {
            EnemyParabolicProjectile projectile = CreateProjectile(Vector3.zero);
            TestDamageable target = CreateDamageable("Target", new Vector3(0.5f, 0f, 0f));
            ConfigureProjectile(projectile, 0.5f, 4f, 1f, 1f, null, 2f);
            projectile.Initialize(new Vector3(10f, 0f, 0f), null, CreateDamageParts(), null);
            Physics.SyncTransforms();

            projectile.Tick(0.5f);

            Assert.IsTrue(projectile.IsFinished);
            Assert.AreEqual(0, target.ApplyDamageCount);
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
            ConfigureProjectile(projectile, projectileLifetime, arcHeight, ascentDuration, descentDuration, null, 2f);
        }

        private static void ConfigureProjectile(
            EnemyParabolicProjectile projectile,
            float projectileLifetime,
            float arcHeight,
            float ascentDuration,
            float descentDuration,
            GameObject telegraphPrefab,
            float aoeRadius)
        {
            ConfigureProjectile(
                projectile,
                projectileLifetime,
                arcHeight,
                ascentDuration,
                descentDuration,
                telegraphPrefab,
                aoeRadius,
                0);
        }

        private static void ConfigureProjectile(
            EnemyParabolicProjectile projectile,
            float projectileLifetime,
            float arcHeight,
            float ascentDuration,
            float descentDuration,
            GameObject telegraphPrefab,
            float aoeRadius,
            LayerMask aoeObstacleMask)
        {
            SerializedObject serializedProjectile = new(projectile);
            serializedProjectile.FindProperty("hitLayers").intValue = LayerMask.GetMask("Default");
            serializedProjectile.FindProperty("projectileLifetime").floatValue = projectileLifetime;
            serializedProjectile.FindProperty("arcHeight").floatValue = arcHeight;
            serializedProjectile.FindProperty("ascentDuration").floatValue = ascentDuration;
            serializedProjectile.FindProperty("descentDuration").floatValue = descentDuration;
            serializedProjectile.FindProperty("telegraphPrefab").objectReferenceValue = telegraphPrefab;
            serializedProjectile.FindProperty("aoeRadius").floatValue = aoeRadius;
            serializedProjectile.FindProperty("aoeObstacleMask").intValue = aoeObstacleMask.value;
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

        private TestDamageable CreateDamageable(string objectName, Vector3 position)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.name = objectName;
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            gameObject.transform.SetPositionAndRotation(position, Quaternion.identity);
            createdObjects.Add(gameObject);
            return gameObject.AddComponent<TestDamageable>();
        }

        private TestDamageable CreateDamageableWithChildCollider(string objectName, Vector3 position)
        {
            GameObject parent = CreateObject(objectName);
            parent.layer = LayerMask.NameToLayer("Ignore Raycast");
            parent.transform.position = position;
            TestDamageable damageable = parent.AddComponent<TestDamageable>();

            GameObject child = GameObject.CreatePrimitive(PrimitiveType.Cube);
            child.name = objectName + "ChildCollider";
            child.layer = LayerMask.NameToLayer("Ignore Raycast");
            child.transform.SetParent(parent.transform);
            child.transform.localPosition = new Vector3(0.25f, 0f, 0f);

            GameObject secondChild = GameObject.CreatePrimitive(PrimitiveType.Cube);
            secondChild.name = objectName + "SecondChildCollider";
            secondChild.layer = LayerMask.NameToLayer("Ignore Raycast");
            secondChild.transform.SetParent(parent.transform);
            secondChild.transform.localPosition = new Vector3(-0.25f, 0f, 0f);
            return damageable;
        }

        private GameObject CreateObject(string objectName)
        {
            GameObject gameObject = new(objectName);
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private static IReadOnlyList<PartialDamage> CreateDamageParts()
        {
            return CreateDamageParts(1f);
        }

        private static IReadOnlyList<PartialDamage> CreateDamageParts(float amount)
        {
            return new[]
            {
                new PartialDamage(amount, DamageType.Physical, DamageElement.None)
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

        public sealed class TestDamageable : MonoBehaviour, IDamageable
        {
            public int ApplyDamageCount { get; private set; }
            public DamageData LastDamageData { get; private set; }
            public bool CanReceiveDamage => true;

            public DamageResult ApplyDamage(DamageData data)
            {
                ApplyDamageCount++;
                LastDamageData = data;
                return DamageResult.Applied(data, data.Amount, 100f, 100f - data.Amount, false);
            }
        }
    }
}
