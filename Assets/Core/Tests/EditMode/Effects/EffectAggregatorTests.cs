using NUnit.Framework;
using RPGame.Core.Effects;
using UnityEditor;
using UnityEngine;

namespace RPGame.Core.Tests.Effects
{
    public sealed class EffectAggregatorTests
    {
        private StatEffectDefinition firstManaRegenerationEffect;
        private StatEffectDefinition secondManaRegenerationEffect;
        private GameObject gameObject;
        private EffectAggregator aggregator;

        [SetUp]
        public void SetUp()
        {
            firstManaRegenerationEffect = CreateStatEffect(EffectStat.ManaRegeneration, EffectModifierType.Percent, 0.05f);
            secondManaRegenerationEffect = CreateStatEffect(EffectStat.ManaRegeneration, EffectModifierType.Percent, 0.10f);
            gameObject = new GameObject("Effect Aggregator");
            aggregator = gameObject.AddComponent<EffectAggregator>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(firstManaRegenerationEffect);
            Object.DestroyImmediate(secondManaRegenerationEffect);
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void Add_WhenDefinitionIsAdded_StoresEffect()
        {
            aggregator.Add(firstManaRegenerationEffect);

            Assert.AreEqual(1, aggregator.Effects.Count);
            Assert.AreSame(firstManaRegenerationEffect, aggregator.Effects[0].Definition);
        }

        [Test]
        public void GetEffectValue_WhenMultipleEffectsMatch_ReturnsSum()
        {
            aggregator.Add(firstManaRegenerationEffect);
            aggregator.Add(secondManaRegenerationEffect);

            float value = aggregator.GetEffectValue(EffectStat.ManaRegeneration, EffectModifierType.Percent);

            Assert.AreEqual(0.15f, value, 0.0001f);
        }

        [Test]
        public void AddRange_WhenDefinitionsAreAdded_StoresAllEffects()
        {
            aggregator.AddRange(new[] { firstManaRegenerationEffect, secondManaRegenerationEffect });
            float value = aggregator.GetEffectValue(EffectStat.ManaRegeneration, EffectModifierType.Percent);

            Assert.AreEqual(2, aggregator.Effects.Count);
            Assert.AreEqual(0.15f, value, 0.0001f);
        }

        private static StatEffectDefinition CreateStatEffect(
            EffectStat stat,
            EffectModifierType modifierType,
            float value)
        {
            StatEffectDefinition definition = ScriptableObject.CreateInstance<StatEffectDefinition>();
            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("stat").enumValueIndex = (int)stat;
            serializedDefinition.FindProperty("modifierType").enumValueIndex = (int)modifierType;
            serializedDefinition.FindProperty("value").floatValue = value;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }
    }
}
