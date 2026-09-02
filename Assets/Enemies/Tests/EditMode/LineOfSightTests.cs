using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace RPGame.Enemies.Tests
{
    public sealed class LineOfSightTests
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
        public void HasLineOfSight_WhenNoObstacle_ReturnsTrue()
        {
            LineOfSight lineOfSight = CreateLineOfSight(Vector3.zero, LayerMask.GetMask("Default"));

            bool hasLineOfSight = ((IEnemyLineOfSight)lineOfSight).HasLineOfSight(new Vector3(10f, 0f, 0f));

            Assert.IsTrue(hasLineOfSight);
        }

        [Test]
        public void HasLineOfSight_WhenObstacleBlocksTarget_ReturnsFalse()
        {
            LineOfSight lineOfSight = CreateLineOfSight(Vector3.zero, LayerMask.GetMask("Default"));
            CreateBlockingCube("Obstacle", new Vector3(5f, 0f, 0f), 0);

            bool hasLineOfSight = ((IEnemyLineOfSight)lineOfSight).HasLineOfSight(new Vector3(10f, 0f, 0f));

            Assert.IsFalse(hasLineOfSight);
        }

        [Test]
        public void HasLineOfSight_UsesConfiguredLayerMask()
        {
            LineOfSight lineOfSight = CreateLineOfSight(Vector3.zero, LayerMask.GetMask("Ignore Raycast"));
            CreateBlockingCube("DefaultObstacle", new Vector3(5f, 0f, 0f), 0);

            bool hasLineOfSight = ((IEnemyLineOfSight)lineOfSight).HasLineOfSight(new Vector3(10f, 0f, 0f));

            Assert.IsTrue(hasLineOfSight);
        }

        [Test]
        public void HasLineOfSight_UsesConfiguredOrigin()
        {
            LineOfSight lineOfSight = CreateLineOfSight(new Vector3(0f, 10f, 0f), LayerMask.GetMask("Default"));
            Transform origin = CreateObject("Origin").transform;
            origin.position = Vector3.zero;
            SetOrigin(lineOfSight, origin);
            CreateBlockingCube("Obstacle", new Vector3(5f, 0f, 0f), 0);

            bool hasLineOfSight = ((IEnemyLineOfSight)lineOfSight).HasLineOfSight(new Vector3(10f, 0f, 0f));

            Assert.IsFalse(hasLineOfSight);
        }

        [Test]
        public void HasLineOfSightFrom_UsesProvidedOrigin()
        {
            LineOfSight lineOfSight = CreateLineOfSight(new Vector3(0f, 10f, 0f), LayerMask.GetMask("Default"));
            CreateBlockingCube("Obstacle", new Vector3(5f, 0f, 0f), 0);

            bool hasLineOfSight = ((IEnemyLineOfSight)lineOfSight).HasLineOfSightFrom(
                Vector3.zero,
                new Vector3(10f, 0f, 0f));

            Assert.IsFalse(hasLineOfSight);
        }


        [Test]
        public void LineOfSight_DoesNotReferencePlayer()
        {
            bool referencesPlayer = typeof(LineOfSight).Assembly
                .GetReferencedAssemblies()
                .Any(assemblyName => assemblyName.Name == "RPGame.Player");

            Assert.IsFalse(referencesPlayer);
        }

        private LineOfSight CreateLineOfSight(Vector3 position, LayerMask obstacleMask)
        {
            GameObject gameObject = CreateObject("LineOfSight");
            gameObject.transform.position = position;

            LineOfSight lineOfSight = gameObject.AddComponent<LineOfSight>();
            SerializedObject serializedObject = new(lineOfSight);
            serializedObject.FindProperty("obstacleMask").intValue = obstacleMask.value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            return lineOfSight;
        }

        private static void SetOrigin(LineOfSight lineOfSight, Transform origin)
        {
            SerializedObject serializedObject = new(lineOfSight);
            serializedObject.FindProperty("origin").objectReferenceValue = origin;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private GameObject CreateBlockingCube(string objectName, Vector3 position, int layer)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.name = objectName;
            gameObject.layer = layer;
            gameObject.transform.position = position;
            createdObjects.Add(gameObject);
            Physics.SyncTransforms();
            return gameObject;
        }

        private GameObject CreateObject(string objectName)
        {
            GameObject gameObject = new(objectName);
            createdObjects.Add(gameObject);
            return gameObject;
        }
    }
}
