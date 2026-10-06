using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RPGame.Enemies;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace RPGame.Enemies.Tests
{
    public sealed class MovementTests
    {
        private readonly List<GameObject> createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(createdObjects[i]);
            }

            createdObjects.Clear();
        }

        [Test]
        public void Start_PassesMoveSpeedToNavMeshAgent()
        {
            Movement movement = CreateMovement(out NavMeshAgent agent);
            SetMoveSpeed(movement, 4.75f);

            InvokeStart(movement);

            Assert.AreEqual(4.75f, agent.speed);
        }

        [Test]
        public void MoveTo_WhenAgentIsNotOnNavMesh_DoesNotThrow()
        {
            Movement movement = CreateMovement(out NavMeshAgent agent);
            InvokeStart(movement);

            Assert.DoesNotThrow(() => movement.MoveTo(new Vector3(2f, 0f, 2f)));
            Assert.DoesNotThrow(movement.Stop);
            Assert.IsFalse(agent.isOnNavMesh);
        }

        [Test]
        public void Start_DisablesAutomaticOffMeshLinkTraversal()
        {
            Movement movement = CreateMovement(out NavMeshAgent agent);

            InvokeStart(movement);

            Assert.IsFalse(agent.autoTraverseOffMeshLink);
        }

        [Test]
        public void ApplyKnockback_WhenLeaping_DoesNotStartKnockback()
        {
            Movement movement = CreateMovement(out _);
            SetIsLeaping(movement, true);

            movement.ApplyKnockback(Vector3.forward, 1f, 0.5f);

            Assert.IsTrue(movement.IsLeaping);
            Assert.IsNull(GetKnockbackCoroutine(movement));
        }

        [Test]
        public void ResetForSpawn_WhenLeaping_ClearsLeapState()
        {
            Movement movement = CreateMovement(out _);
            SetIsLeaping(movement, true);

            movement.ResetForSpawn();

            Assert.IsFalse(movement.IsLeaping);
        }

        [Test]
        public void CancelLeap_WhenTraversingOffMeshLink_ClearsTraversalWithoutNormalCompletion()
        {
            Movement movement = CreateMovement(out _);
            SetIsLeaping(movement, true);
            SetLeapTraversalState(movement, true);

            InvokeCancelLeap(movement);

            Assert.IsFalse(movement.IsLeaping);
            Assert.IsFalse(GetLeapTraversalState(movement));
            Assert.IsNull(GetLeapCoroutine(movement));
        }

        [Test]
        public void BlockMovement_WhenLeaping_DoesNotCancelLeap()
        {
            Movement movement = CreateMovement(out _);
            SetIsLeaping(movement, true);

            movement.BlockMovement();

            Assert.IsTrue(movement.IsLeaping);
        }

        [Test]
        public void BlockMovement_WhenCharging_CancelsCharge()
        {
            Movement movement = CreateMovement(out _);
            SetIsCharging(movement, true);

            movement.BlockMovement();

            Assert.IsFalse(movement.IsCharging);
        }

        [Test]
        public void ApplyKnockback_WhenCharging_UsesConfiguredResistanceAndCancelsCharge()
        {
            Movement movement = CreateMovement(out _);
            SetIsCharging(movement, true);
            SetChargeKnockbackResistance(movement, 0f);

            movement.ApplyKnockback(Vector3.forward, 5f, 0.5f);

            Assert.IsFalse(movement.IsCharging);
            Assert.IsNull(GetKnockbackCoroutine(movement));
        }

        [Test]
        public void MovementSpeedModifier_DoesNotChangeActiveChargeSpeed()
        {
            Movement movement = CreateMovement(out _);
            SetChargeSpeed(movement, 8f);

            movement.AddMovementSpeedModifier(0.5f);

            Assert.AreEqual(8f, GetChargeSpeed(movement));
        }

        [Test]
        public void ResetForDespawn_WhenCharging_ClearsChargeState()
        {
            Movement movement = CreateMovement(out _);
            SetIsCharging(movement, true);

            movement.ResetForDespawn();

            Assert.IsFalse(movement.IsCharging);
        }

        [Test]
        public void Movement_DoesNotReferenceDetectionPlayerOrCombat()
        {
            bool hasForbiddenField = typeof(Movement)
                .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                .Any(field =>
                    field.FieldType == typeof(Detection)
                    || field.FieldType.Namespace == "RPGame.Player"
                    || field.FieldType.Namespace == "RPGame.Combat");

            Assert.IsFalse(hasForbiddenField);
        }

        [Test]
        public void Knockback_WhenBlockedByObstacle_InvokesCollisionCallback()
        {
            Movement movement = CreateMovement(out _);
            GameObject obstacle = CreateObject("Obstacle");
            BoxCollider obstacleCollider = obstacle.AddComponent<BoxCollider>();
            obstacle.transform.position = new Vector3(1.5f, 1f, 0f);
            Physics.SyncTransforms();

            TestKnockbackCollisionCallback callback = new();

            bool completedMove = InvokeTryApplyKnockbackStep(
                movement,
                new Vector3(2f, 0f, 0f),
                callback.Handle);

            Assert.IsFalse(completedMove);
            Assert.AreSame(obstacleCollider, callback.Obstacle);
            Assert.AreEqual(1, callback.CallCount);
            Assert.That(callback.Point.x, Is.EqualTo(1f).Within(0.01f));
            Assert.That(callback.Point.y, Is.InRange(
                obstacleCollider.bounds.min.y - 0.01f,
                obstacleCollider.bounds.max.y + 0.01f));
            Assert.That(callback.Point.z, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void Knockback_WhenMoveCompletes_DoesNotNotifyCollisionHandler()
        {
            Movement movement = CreateMovement(out _);
            TestKnockbackCollisionCallback callback = new();

            bool completedMove = InvokeTryApplyKnockbackStep(
                movement,
                new Vector3(0.25f, 0f, 0f),
                callback.Handle);

            Assert.IsTrue(completedMove);
            Assert.AreEqual(0, callback.CallCount);
        }

        private Movement CreateMovement(out NavMeshAgent agent)
        {
            GameObject gameObject = CreateObject("Movement");
            agent = gameObject.AddComponent<NavMeshAgent>();
            return gameObject.AddComponent<Movement>();
        }

        private GameObject CreateObject(string objectName)
        {
            GameObject gameObject = new(objectName);
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private static void SetMoveSpeed(Movement movement, float moveSpeed)
        {
            SerializedObject serializedObject = new(movement);
            serializedObject.FindProperty("moveSpeed").floatValue = moveSpeed;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void InvokeStart(Movement movement)
        {
            MethodInfo method = typeof(Movement).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(movement, null);
        }

        private static bool InvokeTryApplyKnockbackStep(
            Movement movement,
            Vector3 displacement,
            System.Action<Collider, Vector3> onCollision)
        {
            MethodInfo method = typeof(Movement).GetMethod(
                "TryApplyKnockbackStep",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return (bool)method.Invoke(movement, new object[] { displacement, onCollision });
        }

        private static void SetIsLeaping(Movement movement, bool isLeaping)
        {
            FieldInfo field = typeof(Movement).GetField("isLeaping", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(movement, isLeaping);
        }

        private static void SetIsCharging(Movement movement, bool isCharging)
        {
            FieldInfo field = typeof(Movement).GetField("isCharging", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(movement, isCharging);
        }

        private static void SetChargeKnockbackResistance(Movement movement, float resistance)
        {
            FieldInfo field = typeof(Movement).GetField(
                "chargeKnockbackResistance",
                BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(movement, resistance);
        }

        private static void SetChargeSpeed(Movement movement, float chargeSpeed)
        {
            FieldInfo field = typeof(Movement).GetField("chargeSpeed", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(movement, chargeSpeed);
        }

        private static float GetChargeSpeed(Movement movement)
        {
            FieldInfo field = typeof(Movement).GetField("chargeSpeed", BindingFlags.Instance | BindingFlags.NonPublic);
            return (float)field.GetValue(movement);
        }

        private static Coroutine GetKnockbackCoroutine(Movement movement)
        {
            FieldInfo field = typeof(Movement).GetField("knockbackCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
            return (Coroutine)field.GetValue(movement);
        }

        private static void InvokeCancelLeap(Movement movement)
        {
            MethodInfo method = typeof(Movement).GetMethod("CancelLeap", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(movement, null);
        }

        private static void SetLeapTraversalState(Movement movement, bool traversesOffMeshLink)
        {
            FieldInfo field = typeof(Movement).GetField("leapTraversesOffMeshLink", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(movement, traversesOffMeshLink);
        }

        private static bool GetLeapTraversalState(Movement movement)
        {
            FieldInfo field = typeof(Movement).GetField("leapTraversesOffMeshLink", BindingFlags.Instance | BindingFlags.NonPublic);
            return (bool)field.GetValue(movement);
        }

        private static Coroutine GetLeapCoroutine(Movement movement)
        {
            FieldInfo field = typeof(Movement).GetField("leapCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
            return (Coroutine)field.GetValue(movement);
        }

        private sealed class TestKnockbackCollisionCallback
        {
            public int CallCount { get; private set; }
            public Collider Obstacle { get; private set; }
            public Vector3 Point { get; private set; }

            public void Handle(Collider obstacle, Vector3 point)
            {
                CallCount++;
                Obstacle = obstacle;
                Point = point;
            }
        }
    }
}
