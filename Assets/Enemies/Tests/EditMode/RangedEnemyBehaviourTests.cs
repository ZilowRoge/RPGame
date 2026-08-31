using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RPGame.Core.Targeting;
using UnityEngine;
using UnityEngine.AI;

namespace RPGame.Enemies.Tests
{
    public sealed class RangedEnemyBehaviourTests
    {
        [Test]
        public void Tick_WhenNoTarget_StopsAndStaysIdle()
        {
            FakeDetection detection = new();
            FakeMovement movement = new();
            RangedEnemyBehaviour behaviour = CreateBehaviour(detection, movement);

            behaviour.Tick(0.1f);

            Assert.AreEqual(RangedBehaviourState.Idle, behaviour.State);
            Assert.AreEqual(1, movement.StopCount);
            Assert.AreEqual(0, movement.MoveToCount);
        }

        [Test]
        public void Tick_WhenTargetIsTooFar_Approaches()
        {
            FakeDetection detection = CreateDetectionWithTarget(new Vector3(8f, 0f, 0f));
            FakeMovement movement = new();
            RangedEnemyBehaviour behaviour = CreateBehaviour(detection, movement);

            behaviour.Tick(0.1f);

            Assert.AreEqual(RangedBehaviourState.Approach, behaviour.State);
            Assert.AreEqual(1, movement.MoveToCount);
            Assert.AreEqual(0, movement.StopCount);
        }

        [Test]
        public void Tick_WhenTargetIsInRange_Holds()
        {
            FakeDetection detection = CreateDetectionWithTarget(new Vector3(4f, 0f, 0f));
            FakeMovement movement = new();
            RangedEnemyBehaviour behaviour = CreateBehaviour(detection, movement);

            behaviour.Tick(0.1f);

            Assert.AreEqual(RangedBehaviourState.Hold, behaviour.State);
            Assert.AreEqual(1, movement.StopCount);
            Assert.AreEqual(0, movement.MoveToCount);
        }

        [Test]
        public void Tick_WhenTargetIsTooClose_Retreats()
        {
            FakeDetection detection = CreateDetectionWithTarget(new Vector3(1f, 0f, 0f));
            FakeMovement movement = new();
            movement.Position = Vector3.zero;
            RangedEnemyBehaviour behaviour = CreateBehaviour(detection, movement);

            behaviour.Tick(0.1f);

            Assert.AreEqual(RangedBehaviourState.Retreat, behaviour.State);
            Assert.AreEqual(1, movement.MoveToCount);
            Assert.AreEqual(0, movement.StopCount);
        }

        [Test]
        public void Tick_WhenApproaching_DoesNotOscillateAtMaxRangeBoundary()
        {
            FakeDetection detection = CreateDetectionWithTarget(new Vector3(6.5f, 0f, 0f));
            FakeMovement movement = new();
            RangedEnemyBehaviour behaviour = CreateBehaviour(detection, movement);

            behaviour.Tick(0.1f);
            detection.SetTarget(new SelectedTarget(detection.Target.Targetable, new Vector3(4.75f, 0f, 0f)));
            behaviour.Tick(0.1f);

            Assert.AreEqual(RangedBehaviourState.Approach, behaviour.State);
            Assert.AreEqual(2, movement.MoveToCount);
            Assert.AreEqual(new Vector3(4.75f, 0f, 0f), movement.LastDestination);
        }

        [Test]
        public void Tick_WhenRetreating_DoesNotOscillateAtMinRangeBoundary()
        {
            FakeDetection detection = CreateDetectionWithTarget(new Vector3(1f, 0f, 0f));
            FakeMovement movement = new();
            RangedEnemyBehaviour behaviour = CreateBehaviour(detection, movement);

            behaviour.Tick(0.1f);
            detection.SetTarget(new SelectedTarget(detection.Target.Targetable, new Vector3(2.25f, 0f, 0f)));
            behaviour.Tick(0.1f);

            Assert.AreEqual(RangedBehaviourState.Retreat, behaviour.State);
            Assert.AreEqual(2, movement.MoveToCount);
        }

        [Test]
        public void Tick_WhenCrossingHysteresisThreshold_EntersHold()
        {
            FakeDetection detection = CreateDetectionWithTarget(new Vector3(6.5f, 0f, 0f));
            FakeMovement movement = new();
            RangedEnemyBehaviour behaviour = CreateBehaviour(detection, movement);

            behaviour.Tick(0.1f);
            detection.SetTarget(new SelectedTarget(detection.Target.Targetable, new Vector3(4f, 0f, 0f)));
            behaviour.Tick(0.1f);

            Assert.AreEqual(RangedBehaviourState.Hold, behaviour.State);
            Assert.AreEqual(1, movement.StopCount);
        }

        [Test]
        public void Tick_WhenRetreatCrossesHysteresisThreshold_EntersHold()
        {
            FakeDetection detection = CreateDetectionWithTarget(new Vector3(1f, 0f, 0f));
            FakeMovement movement = new();
            RangedEnemyBehaviour behaviour = CreateBehaviour(detection, movement);

            behaviour.Tick(0.1f);
            detection.SetTarget(new SelectedTarget(detection.Target.Targetable, new Vector3(3f, 0f, 0f)));
            behaviour.Tick(0.1f);

            Assert.AreEqual(RangedBehaviourState.Hold, behaviour.State);
            Assert.AreEqual(1, movement.StopCount);
        }

        [Test]
        public void Tick_WhenApproaching_UsesTargetPositionAsDestination()
        {
            Vector3 targetPosition = new(7f, 0f, 2f);
            FakeDetection detection = CreateDetectionWithTarget(targetPosition);
            FakeMovement movement = new();
            RangedEnemyBehaviour behaviour = CreateBehaviour(detection, movement);

            behaviour.Tick(0.1f);

            Assert.AreEqual(targetPosition, movement.LastDestination);
        }

        [Test]
        public void Tick_WhenRetreating_ChoosesDirectionAwayFromTarget()
        {
            FakeDetection detection = CreateDetectionWithTarget(new Vector3(1f, 0f, 0f));
            FakeMovement movement = new();
            movement.Position = Vector3.zero;
            RangedEnemyBehaviour behaviour = CreateBehaviour(detection, movement);

            behaviour.Tick(0.1f);

            Assert.Less(movement.LastDesiredPosition.x, movement.Position.x);
            Assert.AreEqual(Vector3.left, movement.LastDesiredPosition.normalized);
        }

        [Test]
        public void Tick_WhenRetreatPointCannotBeResolved_Stops()
        {
            FakeDetection detection = CreateDetectionWithTarget(new Vector3(1f, 0f, 0f));
            FakeMovement movement = new();
            movement.CanResolvePosition = false;
            RangedEnemyBehaviour behaviour = CreateBehaviour(detection, movement);

            behaviour.Tick(0.1f);

            Assert.AreEqual(RangedBehaviourState.Retreat, behaviour.State);
            Assert.AreEqual(1, movement.StopCount);
            Assert.AreEqual(0, movement.MoveToCount);
        }

        [Test]
        public void RangedEnemyBehaviour_DoesNotDependOnMonoBehaviourNavMeshAgentOrPhysics()
        {
            Assert.IsFalse(typeof(RangedEnemyBehaviour).IsSubclassOf(typeof(MonoBehaviour)));

            Type[] forbiddenTypes =
            {
                typeof(MonoBehaviour),
                typeof(NavMeshAgent),
                typeof(Physics)
            };

            bool hasForbiddenField = typeof(RangedEnemyBehaviour)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(field => forbiddenTypes.Any(type => type.IsAssignableFrom(field.FieldType)));

            string source = File.ReadAllText(Path.Combine(
                Application.dataPath,
                "Enemies",
                "Scripts",
                "Runtime",
                "RangedEnemy",
                "RangedEnemyBehaviour.cs"));

            Assert.IsFalse(hasForbiddenField);
            Assert.IsFalse(source.Contains("NavMesh", StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("Physics", StringComparison.Ordinal));
        }

        [Test]
        public void RangedBehaviourState_IsLocalToRangedBehaviour()
        {
            Assert.AreEqual("RPGame.Enemies.RangedBehaviourState", typeof(RangedBehaviourState).FullName);

            bool hasGlobalEnemyBehaviourState = typeof(RangedEnemyBehaviour).Assembly
                .GetTypes()
                .Any(type => type.Name == "EnemyBehaviourState");

            Assert.IsFalse(hasGlobalEnemyBehaviourState);
            Assert.IsFalse(typeof(RangedBehaviourState).IsPublic);
        }

        [Test]
        public void Config_WhenRangesAreInvalid_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TestRangedEnemyBehaviourConfig.Create(-1f, 5f, 0.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => TestRangedEnemyBehaviourConfig.Create(5f, 5f, 0.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => TestRangedEnemyBehaviourConfig.Create(2f, 5f, -0.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => TestRangedEnemyBehaviourConfig.Create(3f, 8f, 2.51f));
        }

        [Test]
        public void Config_AllowsHysteresisUpToHalfOfValidRange()
        {
            Assert.DoesNotThrow(() => TestRangedEnemyBehaviourConfig.Create(3f, 8f, 2.5f));
        }

        [Test]
        public void Config_IsScriptableObject()
        {
            Assert.IsTrue(typeof(ScriptableObject).IsAssignableFrom(typeof(RangedEnemyBehaviourConfig)));
            Assert.IsTrue(typeof(EnemyBehaviourConfigBase).IsAssignableFrom(typeof(RangedEnemyBehaviourConfig)));
            Assert.IsTrue(typeof(IRangedEnemyBehaviourConfig).IsAssignableFrom(typeof(RangedEnemyBehaviourConfig)));
        }

        private RangedEnemyBehaviour CreateBehaviour(FakeDetection detection, FakeMovement movement)
        {
            return new RangedEnemyBehaviour(
                detection,
                movement,
                CreateConfig(2f, 5f, 0.5f));
        }

        private static TestRangedEnemyBehaviourConfig CreateConfig(float minRange, float maxRange, float rangeHysteresis)
        {
            return TestRangedEnemyBehaviourConfig.Create(
                minRange,
                maxRange,
                rangeHysteresis);
        }

        private static FakeDetection CreateDetectionWithTarget(Vector3 position)
        {
            FakeDetection detection = new();
            detection.SetTarget(new SelectedTarget(new FakeTargetable(), position));
            return detection;
        }

        private sealed class FakeDetection : IEnemyDetection
        {
            private bool hasTarget;

            public SelectedTarget Target { get; private set; }

            public bool TryGetTarget(out SelectedTarget target)
            {
                target = Target;
                return hasTarget;
            }

            public void SetTarget(SelectedTarget target)
            {
                Target = target;
                hasTarget = true;
            }
        }

        private sealed class FakeMovement : IEnemyMovement
        {
            public bool CanResolvePosition { get; set; } = true;
            public int MoveToCount { get; private set; }
            public int StopCount { get; private set; }
            public Vector3 Position { get; set; }
            public Vector3 LastDestination { get; private set; }
            public Vector3 LastDesiredPosition { get; private set; }

            public void MoveTo(Vector3 position)
            {
                MoveToCount++;
                LastDestination = position;
            }

            public void Stop()
            {
                StopCount++;
            }

            public bool TryResolvePosition(Vector3 desiredPosition, out Vector3 validPosition)
            {
                LastDesiredPosition = desiredPosition;
                validPosition = desiredPosition;
                return CanResolvePosition;
            }
        }

        private sealed class FakeTargetable : ITargetable
        {
            public Transform TargetPoint => null;
        }

        private sealed class TestRangedEnemyBehaviourConfig : IRangedEnemyBehaviourConfig
        {
            private float minRange;
            private float maxRange;
            private float rangeHysteresis;

            public float MinRange => minRange;
            public float MaxRange => maxRange;
            public float RangeHysteresis => rangeHysteresis;

            public static TestRangedEnemyBehaviourConfig Create(
                float minRange,
                float maxRange,
                float rangeHysteresis)
            {
                RangedEnemyBehaviourConfig.ValidateRanges(minRange, maxRange, rangeHysteresis);

                return new TestRangedEnemyBehaviourConfig
                {
                    minRange = minRange,
                    maxRange = maxRange,
                    rangeHysteresis = rangeHysteresis
                };
            }
        }
    }
}
