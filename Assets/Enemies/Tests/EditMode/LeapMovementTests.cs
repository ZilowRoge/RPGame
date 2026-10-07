using System;
using NUnit.Framework;
using UnityEngine;

namespace RPGame.Enemies.Tests
{
    public sealed class LeapMovementTests
    {
        private GameObject gameObject;

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void Tick_InterpolatesArcAndCompletesTraversal()
        {
            gameObject = new GameObject("Leap");
            bool completedTraversal = false;
            LeapMovement leap = new(
                gameObject.transform,
                () => new MovementAgentState(false, true),
                (agentState, position, completeTraversal) => completedTraversal = completeTraversal);

            leap.Start(new Vector3(2f, 0f, 0f), 2f, 1f, true);
            leap.Tick(0.5f);

            Assert.IsTrue(leap.IsActive);
            Assert.That(gameObject.transform.position.y, Is.GreaterThan(0f));

            leap.Tick(0.5f);

            Assert.IsTrue(leap.IsActive);

            leap.Tick(0f);

            Assert.IsFalse(leap.IsActive);
            Assert.IsTrue(completedTraversal);
        }

        [Test]
        public void Cancel_DoesNotCompleteTraversal()
        {
            gameObject = new GameObject("Leap");
            bool completedTraversal = true;
            LeapMovement leap = new(
                gameObject.transform,
                () => new MovementAgentState(false, true),
                (agentState, position, completeTraversal) => completedTraversal = completeTraversal);

            leap.Start(Vector3.forward, 1f, 0f, true);
            leap.Cancel();

            Assert.IsFalse(leap.IsActive);
            Assert.IsFalse(completedTraversal);
        }

        [Test]
        public void LeapMovement_IsNotMonoBehaviour()
        {
            Assert.IsFalse(typeof(LeapMovement).IsSubclassOf(typeof(MonoBehaviour)));
        }
    }
}
