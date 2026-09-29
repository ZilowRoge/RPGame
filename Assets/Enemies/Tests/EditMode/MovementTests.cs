using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RPGame.Core.Movement;
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
        public void Knockback_WhenBlockedByObstacle_ProducesCollisionContext()
        {
            Movement movement = CreateMovement(out _);
            GameObject obstacle = CreateObject("Obstacle");
            BoxCollider obstacleCollider = obstacle.AddComponent<BoxCollider>();
            obstacle.transform.position = new Vector3(1.5f, 1f, 0f);
            Physics.SyncTransforms();

            bool completedMove = InvokeTryApplyKnockbackStep(
                movement,
                new Vector3(2f, 0f, 0f),
                out KnockbackEndContext context);

            Assert.IsFalse(completedMove);
            Assert.AreEqual(KnockbackEndReason.Collision, context.Reason);
            Assert.AreSame(obstacleCollider, context.Obstacle);
            Assert.That(context.CollisionPoint.x, Is.EqualTo(1f).Within(0.01f));
            Assert.That(context.CollisionPoint.y, Is.InRange(
                obstacleCollider.bounds.min.y - 0.01f,
                obstacleCollider.bounds.max.y + 0.01f));
            Assert.That(context.CollisionPoint.z, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void Knockback_WhenMoveCompletes_ProducesNoCollisionContext()
        {
            Movement movement = CreateMovement(out _);

            bool completedMove = InvokeTryApplyKnockbackStep(
                movement,
                new Vector3(0.25f, 0f, 0f),
                out KnockbackEndContext context);

            Assert.IsTrue(completedMove);
            Assert.IsNull(context.Obstacle);
            Assert.AreEqual(Vector3.zero, context.CollisionPoint);
        }

        [Test]
        public void Knockback_WhenDurationIsZeroWithoutObstacle_ReportsCompletedOnce()
        {
            Movement movement = CreateMovement(out _);
            TestKnockbackEndCallback callback = new();

            movement.ApplyKnockback(Vector3.right, 0.25f, 0f, callback.Handle);

            Assert.AreEqual(1, callback.CallCount);
            Assert.AreEqual(KnockbackEndReason.Completed, callback.LastContext.Reason);
            Assert.IsNull(callback.LastContext.Obstacle);
        }

        [Test]
        public void Knockback_WhenDurationIsZeroWithObstacle_ReportsCollisionOnce()
        {
            Movement movement = CreateMovement(out _);
            GameObject obstacle = CreateObject("Obstacle");
            BoxCollider obstacleCollider = obstacle.AddComponent<BoxCollider>();
            obstacle.transform.position = new Vector3(1.5f, 1f, 0f);
            Physics.SyncTransforms();
            TestKnockbackEndCallback endCallback = new();

            movement.ApplyKnockback(Vector3.right, 2f, 0f, endCallback.Handle);

            Assert.AreEqual(1, endCallback.CallCount);
            Assert.AreEqual(KnockbackEndReason.Collision, endCallback.LastContext.Reason);
            Assert.AreSame(obstacleCollider, endCallback.LastContext.Obstacle);
            Assert.That(endCallback.LastContext.CollisionPoint.x, Is.EqualTo(1f).Within(0.01f));
        }

        [Test]
        public void Knockback_AfterZeroDuration_CanStartNextKnockbackImmediately()
        {
            Movement movement = CreateMovement(out _);
            TestKnockbackEndCallback firstCallback = new();
            TestKnockbackEndCallback secondCallback = new();

            movement.ApplyKnockback(Vector3.right, 0.25f, 0f, firstCallback.Handle);
            movement.ApplyKnockback(Vector3.right, 0.25f, 0f, secondCallback.Handle);

            Assert.AreEqual(1, firstCallback.CallCount);
            Assert.AreEqual(KnockbackEndReason.Completed, firstCallback.LastContext.Reason);
            Assert.AreEqual(1, secondCallback.CallCount);
            Assert.AreEqual(KnockbackEndReason.Completed, secondCallback.LastContext.Reason);
        }

        [Test]
        public void Knockback_WhenAlreadyKnockedBack_IgnoresNewKnockback()
        {
            Movement movement = CreateMovement(out _);
            TestKnockbackEndCallback firstCallback = new();
            TestKnockbackEndCallback secondCallback = new();

            movement.ApplyKnockback(Vector3.right, 1f, 1f, firstCallback.Handle);
            movement.ApplyKnockback(Vector3.left, 1f, 0f, secondCallback.Handle);

            Assert.AreEqual(0, firstCallback.CallCount);
            Assert.AreEqual(0, secondCallback.CallCount);
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
            out KnockbackEndContext context)
        {
            MethodInfo method = typeof(Movement).GetMethod(
                "TryApplyKnockbackStep",
                BindingFlags.Instance | BindingFlags.NonPublic);
            object[] args = { displacement, default(KnockbackEndContext) };
            bool result = (bool)method.Invoke(movement, args);
            context = (KnockbackEndContext)args[1];
            return result;
        }

        private sealed class TestKnockbackEndCallback
        {
            public int CallCount { get; private set; }
            public KnockbackEndContext LastContext { get; private set; }

            public void Handle(KnockbackEndContext context)
            {
                CallCount++;
                LastContext = context;
            }
        }
    }
}
