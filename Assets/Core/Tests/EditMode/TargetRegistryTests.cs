using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RPGame.Core.Targeting;
using UnityEngine;
using UnityEngine.TestTools;

namespace RPGame.Core.Tests
{
    public sealed class TargetRegistryTests
    {
        private readonly List<GameObject> createdObjects = new();

        [SetUp]
        public void SetUp()
        {
            TargetRegistry.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < createdObjects.Count; i++)
            {
                Object.DestroyImmediate(createdObjects[i]);
            }

            createdObjects.Clear();
            TargetRegistry.Clear();
        }

        [UnityTest]
        public IEnumerator Targetable_RegistersWhenCreatedActive()
        {
            Targetable targetable = CreateTargetable("Target");

            yield return null;

            Assert.AreEqual(1, TargetRegistry.Count);
            Assert.AreSame(targetable, TargetRegistry.Targets[0]);
        }

        [UnityTest]
        public IEnumerator Targetable_UnregistersWhenDisabled()
        {
            GameObject targetObject = CreateTargetableObject("Target");

            yield return null;

            targetObject.SetActive(false);

            yield return null;

            Assert.AreEqual(0, TargetRegistry.Count);
        }

        [UnityTest]
        public IEnumerator Targetable_UnregistersWhenDestroyed()
        {
            GameObject targetObject = CreateTargetableObject("Target");

            yield return null;

            Object.DestroyImmediate(targetObject);
            createdObjects.Remove(targetObject);

            Assert.AreEqual(0, TargetRegistry.Count);
        }

        [UnityTest]
        public IEnumerator Targetable_RegistersAgainWhenReenabled()
        {
            GameObject targetObject = CreateTargetableObject("Target");
            Targetable targetable = targetObject.GetComponent<Targetable>();

            yield return null;

            targetObject.SetActive(false);

            yield return null;

            targetObject.SetActive(true);

            yield return null;

            Assert.AreEqual(1, TargetRegistry.Count);
            Assert.AreSame(targetable, TargetRegistry.Targets[0]);
        }

        [UnityTest]
        public IEnumerator TargetRegistry_DoesNotRegisterDuplicates()
        {
            Targetable targetable = CreateTargetable("Target");

            yield return null;

            TargetRegistry.Register(targetable);

            Assert.AreEqual(1, TargetRegistry.Count);
            Assert.AreSame(targetable, TargetRegistry.Targets[0]);
        }

        [Test]
        public void TargetPoint_ReturnsAssignedTransform()
        {
            Targetable targetable = CreateTargetable("Target");
            Transform targetPoint = CreateObject("TargetPoint").transform;
            SetTargetPoint(targetable, targetPoint);

            Assert.AreSame(targetPoint, targetable.TargetPoint);
        }

        [Test]
        public void TargetPoint_FallsBackToOwnTransform()
        {
            Targetable targetable = CreateTargetable("Target");

            Assert.AreSame(targetable.transform, targetable.TargetPoint);
        }

        [UnityTest]
        public IEnumerator TargetRegistry_DoesNotKeepInactiveTargets()
        {
            GameObject targetObject = CreateTargetableObject("Target");
            Targetable targetable = targetObject.GetComponent<Targetable>();

            yield return null;

            targetObject.SetActive(false);

            yield return null;

            Assert.AreEqual(0, TargetRegistry.Count);
            CollectionAssert.DoesNotContain(TargetRegistry.Targets, targetable);
        }

        [Test]
        public void TargetRegistry_ReturnsSameTargetsCollection()
        {
            IReadOnlyList<ITargetable> firstRead = TargetRegistry.Targets;
            IReadOnlyList<ITargetable> secondRead = TargetRegistry.Targets;

            Assert.AreSame(firstRead, secondRead);
        }

        private Targetable CreateTargetable(string objectName)
        {
            return CreateTargetableObject(objectName).GetComponent<Targetable>();
        }

        private GameObject CreateTargetableObject(string objectName)
        {
            GameObject gameObject = CreateObject(objectName);
            gameObject.AddComponent<Targetable>();
            return gameObject;
        }

        private GameObject CreateObject(string objectName)
        {
            GameObject gameObject = new GameObject(objectName);
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private static void SetTargetPoint(Targetable targetable, Transform targetPoint)
        {
            FieldInfo field = typeof(Targetable).GetField("targetPoint", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(targetable, targetPoint);
        }
    }
}
