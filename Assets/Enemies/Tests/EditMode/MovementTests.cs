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
    }
}
