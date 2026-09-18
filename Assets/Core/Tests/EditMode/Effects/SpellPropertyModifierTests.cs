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
        private TestSpell spell;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("Spell Property Modifier Tests");
            aggregator = gameObject.AddComponent<EffectAggregator>();
            spell = ScriptableObject.CreateInstance<TestSpell>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
            Object.DestroyImmediate(spell);
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

            SpellPropertyModifiers modifiers = aggregator.CreateSpellPropertyModifiers(spell);

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

            SpellPropertyModifiers modifiers = aggregator.CreateSpellPropertyModifiers(spell);

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
            SpellPropertyModifiers snapshot = aggregator.CreateSpellPropertyModifiers(spell);

            aggregator.Add(second);

            Assert.AreEqual(1f, snapshot.GetValue(SpellProperty.Radius));
            Assert.AreEqual(3f, aggregator.CreateSpellPropertyModifiers(spell).GetValue(SpellProperty.Radius));
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }

        [Test]
        public void Resolver_UsesCapabilityValuesAndModifiers()
        {
            SpellPropertyModifierEffectDefinition radius = CreateModifier(SpellProperty.Radius, 2.5f);
            SpellPropertyModifierEffectDefinition duration = CreateModifier(SpellProperty.Duration, -2f);
            SpellPropertyModifierEffectDefinition controlPower = CreateModifier(SpellProperty.ControlPower, 3f);
            SpellPropertyModifierEffectDefinition orbCount = CreateModifier(SpellProperty.OrbCount, 1.6f);
            aggregator.AddRange(new[]
            {
                radius,
                duration,
                controlPower,
                orbCount
            });

            SpellPropertyModifiers modifiers = aggregator.CreateSpellPropertyModifiers(spell);

            Assert.AreEqual(3.5f, SpellPropertyModifierResolver.ResolveRadius(spell, modifiers));
            Assert.AreEqual(0f, SpellPropertyModifierResolver.ResolveDuration(spell, modifiers));
            Assert.AreEqual(4f, SpellPropertyModifierResolver.ResolveControlPower(spell, modifiers));
            Assert.AreEqual(3, SpellPropertyModifierResolver.ResolveOrbCount(spell, modifiers));
            Object.DestroyImmediate(radius);
            Object.DestroyImmediate(duration);
            Object.DestroyImmediate(controlPower);
            Object.DestroyImmediate(orbCount);
        }

        [Test]
        public void CreateSnapshot_IgnoresPropertiesUnsupportedBySpell()
        {
            SpellPropertyModifierEffectDefinition radius = CreateModifier(SpellProperty.Radius, 2f);
            SpellPropertyModifierEffectDefinition duration = CreateModifier(SpellProperty.Duration, 3f);
            aggregator.AddRange(new[] { radius, duration });
            NoCapabilitySpell noCapabilitySpell = ScriptableObject.CreateInstance<NoCapabilitySpell>();

            SpellPropertyModifiers modifiers = aggregator.CreateSpellPropertyModifiers(noCapabilitySpell);

            Assert.AreSame(SpellPropertyModifiers.Empty, modifiers);
            Object.DestroyImmediate(radius);
            Object.DestroyImmediate(duration);
            Object.DestroyImmediate(noCapabilitySpell);
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

        private sealed class TestSpell : Spell, IAoECapability, IDurationCapability, IControlCapability, IOrbCapability
        {
            public float Radius => 1f;
            public float Duration => 1f;
            public float ControlPower => 1f;
            public int OrbCount => 1;

            public override void OnCast(CasterData casterData)
            {
            }
        }

        private sealed class NoCapabilitySpell : Spell
        {
            public override void OnCast(CasterData casterData)
            {
            }
        }
    }
}
