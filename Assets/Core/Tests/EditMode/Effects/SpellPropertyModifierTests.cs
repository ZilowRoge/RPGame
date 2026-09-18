using NUnit.Framework;
using RPGame.Core.Effects;
using RPGame.Core.Spells;
using UnityEditor;
using UnityEngine;

namespace RPGame.Core.Tests.Effects
{
    public sealed class SpellPropertyModifierTests
    {
        private GameObject gameObject;
        private EffectAggregator aggregator;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("Spell Property Modifier Tests");
            aggregator = gameObject.AddComponent<EffectAggregator>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void ModifierEffect_StoresPropertyAndValue()
        {
            SpellPropertyModifierEffectDefinition effect =
                CreateModifier(SpellProperty.Radius, 1.5f);

            Assert.AreEqual(SpellProperty.Radius, effect.Property);
            Assert.AreEqual(1.5f, effect.Value);
            Object.DestroyImmediate(effect);
        }

        [Test]
        public void CreateSnapshot_SumsModifiersForSameProperty()
        {
            SpellPropertyModifierEffectDefinition first = CreateModifier(SpellProperty.Radius, 1f);
            SpellPropertyModifierEffectDefinition second = CreateModifier(SpellProperty.Radius, 2f);
            aggregator.AddRange(new[] { first, second });

            SpellPropertyModifiers modifiers = aggregator.CreateSpellPropertyModifiers(null);

            Assert.AreEqual(3f, modifiers.GetValue(SpellProperty.Radius));
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }

        [Test]
        public void CreateSnapshot_KeepsPropertiesIndependent()
        {
            SpellPropertyModifierEffectDefinition radius = CreateModifier(SpellProperty.Radius, 1f);
            SpellPropertyModifierEffectDefinition duration = CreateModifier(SpellProperty.Duration, 2f);
            aggregator.AddRange(new[] { radius, duration });

            SpellPropertyModifiers modifiers = aggregator.CreateSpellPropertyModifiers(null);

            Assert.AreEqual(1f, modifiers.GetValue(SpellProperty.Radius));
            Assert.AreEqual(2f, modifiers.GetValue(SpellProperty.Duration));
            Assert.AreEqual(0f, modifiers.GetValue(SpellProperty.ControlPower));
            Object.DestroyImmediate(radius);
            Object.DestroyImmediate(duration);
        }

        [Test]
        public void CreateSnapshot_WhenNoModifierExists_ReturnsZero()
        {
            SpellPropertyModifiers modifiers = aggregator.CreateSpellPropertyModifiers(null);

            Assert.AreSame(SpellPropertyModifiers.Empty, modifiers);
            Assert.AreEqual(0f, modifiers.GetValue(SpellProperty.OrbCount));
        }

        [Test]
        public void CreateSnapshot_IsNotChangedByLaterEffectAdditions()
        {
            SpellPropertyModifierEffectDefinition first = CreateModifier(SpellProperty.Radius, 1f);
            SpellPropertyModifierEffectDefinition second = CreateModifier(SpellProperty.Radius, 2f);
            aggregator.Add(first);
            SpellPropertyModifiers snapshot = aggregator.CreateSpellPropertyModifiers(null);

            aggregator.Add(second);

            Assert.AreEqual(1f, snapshot.GetValue(SpellProperty.Radius));
            Assert.AreEqual(3f, aggregator.CreateSpellPropertyModifiers(null).GetValue(SpellProperty.Radius));
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }

        private static SpellPropertyModifierEffectDefinition CreateModifier(
            SpellProperty property,
            float value)
        {
            SpellPropertyModifierEffectDefinition effect =
                ScriptableObject.CreateInstance<SpellPropertyModifierEffectDefinition>();
            SerializedObject serializedEffect = new(effect);
            serializedEffect.FindProperty("property").enumValueIndex = (int)property;
            serializedEffect.FindProperty("value").floatValue = value;
            serializedEffect.ApplyModifiedPropertiesWithoutUndo();
            return effect;
        }
    }
}
