using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RPGame.Enemies;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace RPGame.Enemies.Tests
{
    public sealed class MovementTests
    {
        private readonly List<GameObject> createdObjects = new();
        private NavMeshDataInstance navMeshDataInstance;

        [TearDown]
        public void TearDown()
        {
            navMeshDataInstance.Remove();

            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(createdObjects[i]);
            }

            createdObjects.Clear();
        }

        [UnityTest]
        public IEnumerator MoveSpeed_IsPassedToNavMeshAgent()
        {
            Movement movement = CreateMovementOnNavMesh(out NavMeshAgent agent);
            SetMoveSpeed(movement, 4.75f);

            movement.MoveTo(new Vector3(1f, 0f, 1f));

            yield return null;

            Assert.AreEqual(4.75f, agent.speed);
        }

        [UnityTest]
        public IEnumerator MoveTo_SetsDestination()
        {
            Movement movement = CreateMovementOnNavMesh(out NavMeshAgent agent);
            Vector3 destination = new(2f, 0f, 2f);

            movement.MoveTo(destination);

            yield return null;

            AssertVectorApproximately(destination, agent.destination);
        }

        [UnityTest]
        public IEnumerator MoveTo_ResumesStoppedAgent()
        {
            Movement movement = CreateMovementOnNavMesh(out NavMeshAgent agent);
            agent.isStopped = true;

            movement.MoveTo(new Vector3(2f, 0f, 2f));

            yield return null;

            Assert.IsFalse(agent.isStopped);
        }

        [UnityTest]
        public IEnumerator Stop_StopsMovement()
        {
            Movement movement = CreateMovementOnNavMesh(out NavMeshAgent agent);

            movement.MoveTo(new Vector3(2f, 0f, 2f));
            yield return null;
            movement.Stop();

            Assert.IsTrue(agent.isStopped);
        }

        [UnityTest]
        public IEnumerator MoveTo_AfterStop_ResumesAndUpdatesDestination()
        {
            Movement movement = CreateMovementOnNavMesh(out NavMeshAgent agent);
            Vector3 destination = new(3f, 0f, 1f);

            movement.Stop();
            movement.MoveTo(destination);

            yield return null;

            Assert.IsFalse(agent.isStopped);
            AssertVectorApproximately(destination, agent.destination);
        }

        [Test]
        public void MoveTo_WhenAgentIsNotOnNavMesh_DoesNotThrowOrResumeAgent()
        {
            Movement movement = CreateMovementWithoutNavMesh(out NavMeshAgent agent);

            Assert.DoesNotThrow(() => movement.MoveTo(new Vector3(2f, 0f, 2f)));
            Assert.DoesNotThrow(movement.Stop);
            Assert.IsFalse(agent.isOnNavMesh);
        }

        [Test]
        public void Movement_DoesNotReferenceDetectionPlayerOrCombat()
        {
            bool referencesForbiddenAssembly = typeof(Movement).Assembly
                .GetReferencedAssemblies()
                .Any(assemblyName => assemblyName.Name == "RPGame.Player" || assemblyName.Name == "RPGame.Combat");

            bool referencesDetectionField = typeof(Movement)
                .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                .Any(field => field.FieldType == typeof(Detection));

            Assert.IsFalse(referencesForbiddenAssembly);
            Assert.IsFalse(referencesDetectionField);
        }

        private Movement CreateMovementOnNavMesh(out NavMeshAgent agent)
        {
            EnsureNavMesh();

            GameObject gameObject = CreateObject("Movement");
            gameObject.transform.position = Vector3.zero;
            agent = gameObject.AddComponent<NavMeshAgent>();
            agent.Warp(Vector3.zero);

            Movement movement = gameObject.AddComponent<Movement>();
            InvokeStart(movement);
            return movement;
        }

        private Movement CreateMovementWithoutNavMesh(out NavMeshAgent agent)
        {
            GameObject gameObject = CreateObject("MovementWithoutNavMesh");
            agent = gameObject.AddComponent<NavMeshAgent>();
            Movement movement = gameObject.AddComponent<Movement>();
            InvokeStart(movement);
            return movement;
        }

        private void EnsureNavMesh()
        {
            if (navMeshDataInstance.valid)
            {
                return;
            }

            NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);
            List<NavMeshBuildSource> sources = new()
            {
                new NavMeshBuildSource
                {
                    shape = NavMeshBuildSourceShape.Box,
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one),
                    size = new Vector3(10f, 0.1f, 10f),
                    area = 0
                }
            };

            Bounds bounds = new(Vector3.zero, new Vector3(10f, 2f, 10f));
            NavMeshData navMeshData = NavMeshBuilder.BuildNavMeshData(
                buildSettings,
                sources,
                bounds,
                Vector3.zero,
                Quaternion.identity);

            navMeshDataInstance = NavMesh.AddNavMeshData(navMeshData);
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

        private static void AssertVectorApproximately(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, 0.05f);
            Assert.AreEqual(expected.y, actual.y, 0.05f);
            Assert.AreEqual(expected.z, actual.z, 0.05f);
        }
    }
}
