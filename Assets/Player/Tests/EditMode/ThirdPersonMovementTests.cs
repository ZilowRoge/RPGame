using System;
using System.Reflection;
using NUnit.Framework;
using RPGame.Core.Statistics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPGame.Player.Tests
{
    public sealed class ThirdPersonMovementTests
    {
        private GameObject playerObject;
        private StatisticsController statisticsController;
        private ThirdPersonMovement movement;

        [SetUp]
        public void SetUp()
        {
            playerObject = new GameObject("Player");
            playerObject.AddComponent<CharacterController>();
            statisticsController = playerObject.AddComponent<StatisticsController>();
            movement = playerObject.AddComponent<ThirdPersonMovement>();
            InvokeLifecycleMethod("Awake");
            InvokeLifecycleMethod("OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void Died_BlocksMovementAndClearsCurrentMovementState()
        {
            SetPrivateField(movement, "groundedMoveVelocity", Vector3.right);
            SetPrivateField(movement, "lockedAirMoveVelocity", Vector3.forward);
            SetPrivateField(movement, "isAirMoveLocked", true);
            SetPrivateField(movement, "isSprinting", true);
            SetPrivateField(movement, "jumpBufferTimer", 1f);

            RaiseDied();

            Assert.That(GetPrivateField<int>(movement, "movementBlockCount"), Is.EqualTo(1));
            Assert.That(movement.HorizontalVelocity, Is.EqualTo(Vector3.zero));
            Assert.That(movement.IsSprinting, Is.False);
            Assert.That(GetPrivateField<float>(movement, "jumpBufferTimer"), Is.Zero);
        }

        [Test]
        public void Died_WhenRaisedRepeatedly_DoesNotStackMovementBlocks()
        {
            RaiseDied();
            RaiseDied();

            Assert.That(GetPrivateField<int>(movement, "movementBlockCount"), Is.EqualTo(1));
        }

        [Test]
        public void OnDisable_UnsubscribesFromDeath()
        {
            movement.enabled = false;
            InvokeLifecycleMethod("OnDisable");

            RaiseDied();

            Assert.That(GetPrivateField<int>(movement, "movementBlockCount"), Is.Zero);
        }

        [Test]
        public void OnDestroy_UnsubscribesFromDeath()
        {
            typeof(ThirdPersonMovement).GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(movement, null);

            RaiseDied();

            Assert.That(GetPrivateField<int>(movement, "movementBlockCount"), Is.Zero);
        }

        private void RaiseDied()
        {
            FieldInfo diedField = typeof(StatisticsController).GetField("Died", BindingFlags.Instance | BindingFlags.NonPublic);
            ((Action)diedField.GetValue(statisticsController))?.Invoke();
        }

        private void InvokeLifecycleMethod(string methodName)
        {
            typeof(ThirdPersonMovement).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(movement, null);
        }

        private static void SetPrivateField<T>(ThirdPersonMovement target, string fieldName, T value)
        {
            typeof(ThirdPersonMovement).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);
        }

        private static T GetPrivateField<T>(ThirdPersonMovement target, string fieldName)
        {
            return (T)typeof(ThirdPersonMovement).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(target);
        }
    }
}
