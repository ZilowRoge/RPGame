using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RPGame.Core.Targeting;
using UnityEditor;
using UnityEngine;

namespace RPGame.Enemies.Tests
{
    public sealed class DetectionTests
    {
        private readonly List<GameObject> createdObjects = new();
        private readonly List<PlayerTargetable> playerTargets = new();

        private GameObject enemyObject;
        private Detection detection;

        [SetUp]
        public void SetUp()
        {
            ClearTargetRegistry();

            enemyObject = CreateObject("Enemy");
            detection = enemyObject.AddComponent<Detection>();
            SetDetectionRange(detection, 5f);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(createdObjects[i]);
            }

            createdObjects.Clear();
            playerTargets.Clear();
            ClearTargetRegistry();
        }

        [Test]
        public void RefreshDetection_WhenNoTargetIsInRange_ClearsTarget()
        {
            CreateTarget("DistantTarget", new Vector3(6f, 0f, 0f));

            detection.RefreshDetection();

            Assert.IsFalse(detection.HasTarget);
            Assert.IsNull(detection.CurrentTarget);
            Assert.IsNull(detection.CurrentTargetPoint);
        }

        [Test]
        public void RefreshDetection_WhenTargetIsInDetectionRange_SetsCurrentTarget()
        {
            PlayerTargetable target = CreateTarget("Target", new Vector3(4f, 0f, 0f));

            detection.RefreshDetection();

            Assert.AreSame(target, detection.CurrentTarget);
            Assert.AreSame(target.TargetPoint, detection.CurrentTargetPoint);
            Assert.IsTrue(detection.HasTarget);
        }

        [Test]
        public void SelectNearest_WhenMultipleTargetsAreInRange_SelectsNearestTarget()
        {
            CreateTarget("FarTarget", new Vector3(4f, 0f, 0f));
            PlayerTargetable nearTarget = CreateTarget("NearTarget", new Vector3(2f, 0f, 0f));

            ITargetable selectedTarget = SelectNearest();

            Assert.AreSame(nearTarget, selectedTarget);
        }

        [Test]
        public void SelectNearest_WhenCurrentTargetLeavesDetectionRange_ReturnsNull()
        {
            PlayerTargetable target = CreateTarget("Target", new Vector3(3f, 0f, 0f));

            target.TargetPoint.position = new Vector3(7f, 0f, 0f);
            ITargetable selectedTarget = SelectNearest();

            Assert.IsNull(selectedTarget);
        }

        [Test]
        public void SelectNearest_WhenCurrentTargetBecomesInactive_ReturnsNull()
        {
            GameObject targetObject = CreateObject("Target");
            targetObject.transform.position = new Vector3(3f, 0f, 0f);
            PlayerTargetable target = targetObject.AddComponent<PlayerTargetable>();
            playerTargets.Add(target);

            targetObject.SetActive(false);
            ITargetable selectedTarget = SelectNearest();

            Assert.IsNull(selectedTarget);
        }

        [Test]
        public void DetectionAssembly_DoesNotReferencePlayerAssembly()
        {
            bool referencesPlayer = typeof(Detection).Assembly
                .GetReferencedAssemblies()
                .Any(assemblyName => assemblyName.Name == "RPGame.Player");

            Assert.IsFalse(referencesPlayer);
        }

        [Test]
        public void RefreshDetection_IgnoresEnemyTargets()
        {
            GameObject targetObject = CreateObject("EnemyTarget", new Vector3(1f, 0f, 0f));
            targetObject.AddComponent<EnemyTargetable>();

            detection.RefreshDetection();

            Assert.IsNull(detection.CurrentTarget);
        }

        private PlayerTargetable CreateTarget(string objectName, Vector3 position)
        {
            GameObject targetObject = CreateObject(objectName);
            targetObject.transform.position = position;
            PlayerTargetable target = targetObject.AddComponent<PlayerTargetable>();
            playerTargets.Add(target);
            return target;
        }

        private GameObject CreateObject(string objectName)
        {
            return CreateObject(objectName, Vector3.zero);
        }

        private GameObject CreateObject(string objectName, Vector3 position)
        {
            GameObject gameObject = new(objectName);
            gameObject.transform.position = position;
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private static void SetDetectionRange(Detection detection, float detectionRange)
        {
            SerializedObject serializedObject = new(detection);
            serializedObject.FindProperty("detectionRange").floatValue = detectionRange;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private ITargetable SelectNearest()
        {
            return TargetSelector.SelectNearest(playerTargets, enemyObject.transform, 5f);
        }

        private static void ClearTargetRegistry()
        {
            MethodInfo method = typeof(TargetRegistry).GetMethod("Clear", BindingFlags.Static | BindingFlags.NonPublic);
            method.Invoke(null, null);
        }
    }
}
