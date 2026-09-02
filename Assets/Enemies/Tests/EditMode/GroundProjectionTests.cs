using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace RPGame.Enemies.Tests
{
    public sealed class GroundProjectionTests
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
        public void TryProjectToGround_WhenGroundExists_ReturnsGroundPoint()
        {
            GroundProjection projection = CreateProjection(LayerMask.GetMask("Default"));
            CreateGround("Ground", 0);
            Physics.SyncTransforms();

            bool projected = projection.TryProjectToGround(new Vector3(2f, 1f, 3f), out Vector3 groundPoint);

            Assert.IsTrue(projected);
            AssertVector(new Vector3(2f, 0f, 3f), groundPoint);
        }

        [Test]
        public void TryProjectToGround_WhenGroundIsMissing_ReturnsFalse()
        {
            GroundProjection projection = CreateProjection(LayerMask.GetMask("Default"));

            bool projected = projection.TryProjectToGround(Vector3.one, out _);

            Assert.IsFalse(projected);
        }

        [Test]
        public void TryProjectToGround_UsesConfiguredLayerMask()
        {
            GroundProjection projection = CreateProjection(LayerMask.GetMask("Ignore Raycast"));
            CreateGround("DefaultGround", 0);
            Physics.SyncTransforms();

            bool projected = projection.TryProjectToGround(new Vector3(2f, 1f, 3f), out _);

            Assert.IsFalse(projected);
        }

        private GroundProjection CreateProjection(LayerMask groundMask)
        {
            GameObject gameObject = CreateObject("GroundProjection");
            GroundProjection projection = gameObject.AddComponent<GroundProjection>();

            SerializedObject serializedProjection = new(projection);
            serializedProjection.FindProperty("groundMask").intValue = groundMask.value;
            serializedProjection.FindProperty("raycastStartHeight").floatValue = 5f;
            serializedProjection.FindProperty("raycastDistance").floatValue = 20f;
            serializedProjection.ApplyModifiedPropertiesWithoutUndo();
            return projection;
        }

        private void CreateGround(string objectName, int layer)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = objectName;
            ground.layer = layer;
            createdObjects.Add(ground);
        }

        private GameObject CreateObject(string objectName)
        {
            GameObject gameObject = new(objectName);
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, 0.0001f);
            Assert.AreEqual(expected.y, actual.y, 0.0001f);
            Assert.AreEqual(expected.z, actual.z, 0.0001f);
        }
    }
}
