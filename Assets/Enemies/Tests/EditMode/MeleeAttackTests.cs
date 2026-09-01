using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RPGame.Core.Damage;
using UnityEditor;
using UnityEngine;

namespace RPGame.Enemies.Tests
{
    public sealed class MeleeAttackTests
    {
        private readonly List<ScriptableObject> createdAssets = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = createdAssets.Count - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(createdAssets[i]);
            }

            createdAssets.Clear();
        }

        [Test]
        public void MeleeAttack_DoesNotDependOnMonoBehaviour()
        {
            Assert.IsFalse(typeof(MeleeAttack).IsSubclassOf(typeof(MonoBehaviour)));

            bool hasMonoBehaviourField = typeof(MeleeAttack)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(field => typeof(MonoBehaviour).IsAssignableFrom(field.FieldType));

            Assert.IsFalse(hasMonoBehaviourField);
        }

        [Test]
        public void TryAttack_BeforeAttackIntervalExpires_IsBlocked()
        {
            MeleeAttack attack = CreateAttack(2f, 1f, 10f, out FakeAttackTarget target);
            SelectedTarget selectedTarget = CreateSelectedTarget(Vector3.one);

            Assert.IsTrue(attack.TryAttack(selectedTarget, target, null));
            Assert.IsFalse(attack.TryAttack(selectedTarget, target, null));
            Assert.AreEqual(1, target.ApplyDamageCount);
        }

        [Test]
        public void TryAttack_AfterAttackIntervalExpires_IsAllowedAgain()
        {
            MeleeAttack attack = CreateAttack(2f, 1f, 10f, out FakeAttackTarget target);
            SelectedTarget selectedTarget = CreateSelectedTarget(Vector3.one);

            Assert.IsTrue(attack.TryAttack(selectedTarget, target, null));
            attack.Tick(1f);

            Assert.IsTrue(attack.TryAttack(selectedTarget, target, null));
            Assert.AreEqual(2, target.ApplyDamageCount);
        }

        [Test]
        public void TryAttack_WhenTargetIsOutsideAttackRange_ReturnsFalse()
        {
            MeleeAttack attack = CreateAttack(2f, 1f, 10f, out FakeAttackTarget target);

            bool attacked = attack.TryAttack(CreateSelectedTarget(new Vector3(3f, 0f, 0f)), target, null);

            Assert.IsFalse(attacked);
            Assert.AreEqual(0, target.ApplyDamageCount);
        }

        [Test]
        public void TryAttack_WhenTargetIsInsideAttackRange_AppliesDamage()
        {
            MeleeAttack attack = CreateAttack(2f, 1f, 10f, out FakeAttackTarget target);

            bool attacked = attack.TryAttack(CreateSelectedTarget(new Vector3(1f, 0f, 0f)), target, null);

            Assert.IsTrue(attacked);
            Assert.AreEqual(1, target.ApplyDamageCount);
        }

        [Test]
        public void TryAttack_BuildsDamageFromPartialDamageRanges()
        {
            MeleeAttack attack = CreateAttack(2f, 1f, 10f, out FakeAttackTarget target);

            attack.TryAttack(CreateSelectedTarget(Vector3.one), target, null);

            Assert.AreEqual(1, target.LastDamageParts.Count);
            Assert.AreEqual(10f, target.LastDamageParts[0].Amount);
            Assert.AreEqual(DamageType.Physical, target.LastDamageParts[0].DamageType);
            Assert.AreEqual(DamageElement.None, target.LastDamageParts[0].DamageElement);
        }

        [Test]
        public void TryAttack_UsesExistingDamageDataShape()
        {
            MeleeAttack attack = CreateAttack(2f, 1f, 10f, out FakeAttackTarget target);
            GameObject source = new("Source");

            try
            {
                attack.TryAttack(CreateSelectedTarget(Vector3.one), target, source);

                Assert.AreEqual(10f, target.LastDamageData.Amount);
                Assert.AreEqual(1, target.LastDamageData.Parts.Count);
                Assert.AreSame(source, target.LastDamageData.Source);
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        private MeleeAttack CreateAttack(
            float attackRange,
            float attackInterval,
            float damageAmount,
            out FakeAttackTarget target)
        {
            target = new FakeAttackTarget();
            MeleeAttackConfig config = CreateMeleeAttackConfig(attackRange, attackInterval, damageAmount);
            return new MeleeAttack(config, () => Vector3.zero);
        }

        private MeleeAttackConfig CreateMeleeAttackConfig(float attackRange, float attackInterval, float damageAmount)
        {
            MeleeAttackConfig config = ScriptableObject.CreateInstance<MeleeAttackConfig>();
            createdAssets.Add(config);

            SerializedObject serializedConfig = new(config);
            serializedConfig.FindProperty("attackRange").floatValue = attackRange;
            serializedConfig.FindProperty("attackInterval").floatValue = attackInterval;

            SerializedProperty damageProperty = serializedConfig.FindProperty("damage");
            damageProperty.arraySize = 1;
            SerializedProperty damageEntry = damageProperty.GetArrayElementAtIndex(0);
            damageEntry.FindPropertyRelative("minDamage").floatValue = damageAmount;
            damageEntry.FindPropertyRelative("maxDamage").floatValue = damageAmount;
            damageEntry.FindPropertyRelative("damageType").enumValueIndex = (int)DamageType.Physical;
            damageEntry.FindPropertyRelative("damageElement").enumValueIndex = (int)DamageElement.None;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();

            return config;
        }

        private static SelectedTarget CreateSelectedTarget(Vector3 position)
        {
            return new SelectedTarget(new FakeTargetable(), position);
        }

        private sealed class FakeAttackTarget : IDamageable
        {
            public bool CanReceiveDamage { get; set; } = true;
            public int ApplyDamageCount { get; private set; }
            public IReadOnlyList<PartialDamage> LastDamageParts { get; private set; }
            public DamageData LastDamageData { get; private set; }

            public DamageResult ApplyDamage(DamageData data)
            {
                ApplyDamageCount++;
                LastDamageParts = data.Parts;
                LastDamageData = data;
                return DamageResult.Applied(LastDamageData, LastDamageData.Amount, 100f, 100f - LastDamageData.Amount, false);
            }
        }

        private sealed class FakeTargetable : RPGame.Core.Targeting.ITargetable
        {
            public Transform TargetPoint => null;
        }
    }
}
