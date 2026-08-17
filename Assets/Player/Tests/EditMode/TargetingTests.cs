using System.Collections.Generic;
using NUnit.Framework;
using RPGame.Core.Targeting;
using RPGame.Player.Targeting;
using UnityEngine;

namespace RPGame.Player.Tests
{
    public sealed class TargetingTests
    {
        private readonly List<GameObject> createdObjects = new();
        private readonly List<ITargetable> targets = new();
        private Camera playerCamera;

        [SetUp]
        public void SetUp()
        {
            GameObject cameraObject = CreateObject("PlayerCamera", Vector3.zero);
            playerCamera = cameraObject.AddComponent<Camera>();
            playerCamera.transform.rotation = Quaternion.identity;
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < createdObjects.Count; i++)
            {
                Object.DestroyImmediate(createdObjects[i]);
            }

            createdObjects.Clear();
            targets.Clear();
        }

        [Test]
        public void SelectBest_SelectsTargetClosestToViewportCenter()
        {
            ITargetable centeredTarget = CreateTarget("Centered", new Vector3(0.05f, 0f, 8f));
            CreateTarget("Offset", new Vector3(1f, 0f, 8f));

            ITargetable selectedTarget = SelectBest();

            Assert.AreSame(centeredTarget, selectedTarget);
        }

        [Test]
        public void SelectBest_IgnoresTargetBeyondMaxTargetDistance()
        {
            CreateTarget("Far", new Vector3(0f, 0f, 8f));

            ITargetable selectedTarget = SelectBest(maxTargetDistance: 5f);

            Assert.IsNull(selectedTarget);
        }

        [Test]
        public void SelectBest_IgnoresTargetBehindCamera()
        {
            CreateTarget("Behind", new Vector3(0f, 0f, -5f));

            ITargetable selectedTarget = SelectBest();

            Assert.IsNull(selectedTarget);
        }

        [Test]
        public void SelectBest_IgnoresTargetOutsideTargetingRadius()
        {
            CreateTarget("OutsideRadius", new Vector3(2f, 0f, 8f));

            ITargetable selectedTarget = SelectBest(targetingRadius: 0.05f);

            Assert.IsNull(selectedTarget);
        }

        [Test]
        public void SelectBest_ReturnsNullWhenNoValidTargetsExist()
        {
            ITargetable selectedTarget = SelectBest();

            Assert.IsNull(selectedTarget);
        }

        [Test]
        public void SelectBest_ChangesSelectedTargetWhenAnotherTargetBecomesCloserToCenter()
        {
            TestTargetable firstTarget = CreateTarget("First", new Vector3(0.05f, 0f, 8f));
            TestTargetable secondTarget = CreateTarget("Second", new Vector3(1f, 0f, 8f));

            ITargetable selectedTarget = SelectBest();
            Assert.AreSame(firstTarget, selectedTarget);

            firstTarget.TargetPoint.position = new Vector3(1.5f, 0f, 8f);
            secondTarget.TargetPoint.position = Vector3.forward * 8f;

            selectedTarget = SelectBest();

            Assert.AreSame(secondTarget, selectedTarget);
        }

        [Test]
        public void SelectBest_UsesAspectRatioWhenComparingViewportDistance()
        {
            playerCamera.aspect = 16f / 9f;
            CreateTargetAtViewport("HorizontalOffset", new Vector3(0.62f, 0.5f, 8f));
            ITargetable verticalOffset = CreateTargetAtViewport("VerticalOffset", new Vector3(0.5f, 0.68f, 8f));

            ITargetable selectedTarget = SelectBest(targetingRadius: 0.3f);

            Assert.AreSame(verticalOffset, selectedTarget);
        }

        [TestCase(-0.1f)]
        [TestCase(1.1f)]
        public void SelectBest_IgnoresTargetOutsideHorizontalViewportBounds(float viewportX)
        {
            CreateTargetAtViewport("OutsideHorizontalBounds", new Vector3(viewportX, 0.5f, 8f));

            ITargetable selectedTarget = SelectBest(targetingRadius: 1f);

            Assert.IsNull(selectedTarget);
        }

        [TestCase(-0.1f)]
        [TestCase(1.1f)]
        public void SelectBest_IgnoresTargetOutsideVerticalViewportBounds(float viewportY)
        {
            CreateTargetAtViewport("OutsideVerticalBounds", new Vector3(0.5f, viewportY, 8f));

            ITargetable selectedTarget = SelectBest(targetingRadius: 1f);

            Assert.IsNull(selectedTarget);
        }

        private ITargetable SelectBest(float maxTargetDistance = 20f, float targetingRadius = 0.25f)
        {
            return TargetSelector.SelectBest(
                targets,
                playerCamera,
                Vector3.zero,
                maxTargetDistance,
                targetingRadius);
        }

        private TestTargetable CreateTargetAtViewport(string objectName, Vector3 viewportPosition)
        {
            return CreateTarget(objectName, playerCamera.ViewportToWorldPoint(viewportPosition));
        }

        private TestTargetable CreateTarget(string objectName, Vector3 position)
        {
            GameObject gameObject = CreateObject(objectName, position);
            TestTargetable targetable = new TestTargetable(gameObject.transform);
            targets.Add(targetable);
            return targetable;
        }

        private GameObject CreateObject(string objectName)
        {
            return CreateObject(objectName, Vector3.zero);
        }

        private GameObject CreateObject(string objectName, Vector3 position)
        {
            GameObject gameObject = new GameObject(objectName);
            gameObject.transform.position = position;
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private sealed class TestTargetable : ITargetable
        {
            public TestTargetable(Transform targetPoint)
            {
                TargetPoint = targetPoint;
            }

            public Transform TargetPoint { get; }
        }
    }
}

