using System.Collections.Generic;
using NUnit.Framework;
using RPGame.Combat.Spells;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Combat.Tests
{
    public sealed class KnockbackDamageOnImpactCollisionBehaviorTests
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
        public void OnKnockbackCollision_WithDamageableTarget_AppliesConfiguredDamageOnce()
        {
            GameObject target = CreateObject("Target");
            TestDamageable damageable = target.AddComponent<TestDamageable>();
            GameObject source = CreateObject("Source");
            KnockbackDamageOnImpactCollisionBehavior behavior = new(17f, source);

            behavior.OnKnockbackCollision(target, null, Vector3.zero);

            Assert.AreEqual(1, damageable.CallCount);
            Assert.AreEqual(17f, damageable.LastData.Amount);
            Assert.AreSame(source, damageable.LastData.Source);
        }

        [Test]
        public void OnKnockbackCollision_WithoutDamageableTarget_DoesNothing()
        {
            GameObject target = CreateObject("Target");
            KnockbackDamageOnImpactCollisionBehavior behavior = new(17f);

            Assert.DoesNotThrow(() => behavior.OnKnockbackCollision(target, null, Vector3.zero));
        }

        private GameObject CreateObject(string objectName)
        {
            GameObject gameObject = new(objectName);
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private sealed class TestDamageable : MonoBehaviour, IDamageable
        {
            public int CallCount { get; private set; }
            public DamageData LastData { get; private set; }
            public bool CanReceiveDamage => true;

            public DamageResult ApplyDamage(DamageData data)
            {
                CallCount++;
                LastData = data;
                return DamageResult.Applied(data, data.Amount, 100f, 100f - data.Amount, false);
            }
        }
    }
}
