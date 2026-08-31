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
        public IEnumerator EnemyTargetable_RegistersWhenCreatedActive()
        {
            EnemyTargetable targetable = CreateEnemyTargetable("Target");

            yield return null;

            Assert.AreEqual(1, TargetRegistry.EnemyTargetCount);
            Assert.AreSame(targetable, TargetRegistry.EnemyTargets[0]);
        }

        [UnityTest]
        public IEnumerator EnemyTargetable_UnregistersWhenDisabled()
        {
            GameObject targetObject = CreateEnemyTargetableObject("Target");

            yield return null;

            targetObject.SetActive(false);

            yield return null;

            Assert.AreEqual(0, TargetRegistry.EnemyTargetCount);
        }

        [UnityTest]
        public IEnumerator EnemyTargetable_UnregistersWhenDestroyed()
        {
            GameObject targetObject = CreateEnemyTargetableObject("Target");

            yield return null;

            Object.DestroyImmediate(targetObject);
            createdObjects.Remove(targetObject);

            Assert.AreEqual(0, TargetRegistry.EnemyTargetCount);
        }

        [UnityTest]
        public IEnumerator EnemyTargetable_RegistersAgainWhenReenabled()
        {
            GameObject targetObject = CreateEnemyTargetableObject("Target");
            EnemyTargetable targetable = targetObject.GetComponent<EnemyTargetable>();

            yield return null;

            targetObject.SetActive(false);

            yield return null;

            targetObject.SetActive(true);

            yield return null;

            Assert.AreEqual(1, TargetRegistry.EnemyTargetCount);
            Assert.AreSame(targetable, TargetRegistry.EnemyTargets[0]);
        }

        [UnityTest]
        public IEnumerator TargetRegistry_DoesNotRegisterDuplicates()
        {
            EnemyTargetable targetable = CreateEnemyTargetable("Target");

            yield return null;

            TargetRegistry.RegisterTarget(targetable);

            Assert.AreEqual(1, TargetRegistry.EnemyTargetCount);
            Assert.AreSame(targetable, TargetRegistry.EnemyTargets[0]);
        }

        [Test]
        public void TargetPoint_ReturnsAssignedTransform()
        {
            EnemyTargetable targetable = CreateEnemyTargetable("Target");
            Transform targetPoint = CreateObject("TargetPoint").transform;
            SetTargetPoint(targetable, targetPoint);

            Assert.AreSame(targetPoint, targetable.TargetPoint);
        }

        [Test]
        public void TargetPoint_FallsBackToOwnTransform()
        {
            EnemyTargetable targetable = CreateEnemyTargetable("Target");

            Assert.AreSame(targetable.transform, targetable.TargetPoint);
        }

        [UnityTest]
        public IEnumerator TargetRegistry_DoesNotKeepInactiveTargets()
        {
            GameObject targetObject = CreateEnemyTargetableObject("Target");
            EnemyTargetable targetable = targetObject.GetComponent<EnemyTargetable>();

            yield return null;

            targetObject.SetActive(false);

            yield return null;

            Assert.AreEqual(0, TargetRegistry.EnemyTargetCount);
            CollectionAssert.DoesNotContain(TargetRegistry.EnemyTargets, targetable);
        }

        [Test]
        public void TargetRegistry_ReturnsSameTargetsCollection()
        {
            IReadOnlyList<EnemyTargetable> firstRead = TargetRegistry.EnemyTargets;
            IReadOnlyList<EnemyTargetable> secondRead = TargetRegistry.EnemyTargets;

            Assert.AreSame(firstRead, secondRead);
        }

        [Test]
        public void TargetRegistry_KeepsEnemyAndPlayerTargetsSeparate()
        {
            EnemyTargetable enemyTarget = CreateObject("EnemyTarget").AddComponent<EnemyTargetable>();
            PlayerTargetable playerTarget = CreateObject("PlayerTarget").AddComponent<PlayerTargetable>();

            TargetRegistry.RegisterTarget(enemyTarget);
            TargetRegistry.RegisterTarget(playerTarget);

            CollectionAssert.Contains(TargetRegistry.EnemyTargets, enemyTarget);
            CollectionAssert.DoesNotContain(TargetRegistry.EnemyTargets, playerTarget);
            CollectionAssert.Contains(TargetRegistry.PlayerTargets, playerTarget);
            CollectionAssert.DoesNotContain(TargetRegistry.PlayerTargets, enemyTarget);
        }

        private EnemyTargetable CreateEnemyTargetable(string objectName)
        {
            return CreateEnemyTargetableObject(objectName).GetComponent<EnemyTargetable>();
        }

        private GameObject CreateEnemyTargetableObject(string objectName)
        {
            GameObject gameObject = CreateObject(objectName);
            gameObject.AddComponent<EnemyTargetable>();
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
