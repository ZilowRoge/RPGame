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
            Assert.That(callback.Point.y, Is.EqualTo(0.5f).Within(0.01f));
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
