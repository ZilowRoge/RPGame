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

        [SetUp]
        public void SetUp()
        {
            firstManaRegenerationEffect = CreateStatEffect(EffectStat.ManaRegeneration, EffectModifierType.Percent, 0.05f);
            secondManaRegenerationEffect = CreateStatEffect(EffectStat.ManaRegeneration, EffectModifierType.Percent, 0.10f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(firstManaRegenerationEffect);
            Object.DestroyImmediate(secondManaRegenerationEffect);
        }

        [Test]
        public void Add_WhenContainerWasAlreadyAdded_DoesNotAddDuplicate()
        {
            EffectContainer container = new EffectContainer();
            EffectAggregator aggregator = new EffectAggregator();

            aggregator.Add(container);
            aggregator.Add(container);

            Assert.AreEqual(1, aggregator.Containers.Count);
        }

        [Test]
        public void GetEffectValue_WhenMultipleContainersHaveMatchingEffects_ReturnsSum()
        {
            EffectContainer firstContainer = new EffectContainer();
            EffectContainer secondContainer = new EffectContainer();
            firstContainer.Add(firstManaRegenerationEffect);
            secondContainer.Add(secondManaRegenerationEffect);
            EffectAggregator aggregator = new EffectAggregator();
            aggregator.Add(firstContainer);
            aggregator.Add(secondContainer);

            float value = aggregator.GetEffectValue(EffectStat.ManaRegeneration, EffectModifierType.Percent);

            Assert.AreEqual(0.15f, value, 0.0001f);
        }

        [Test]
        public void GetEffectValue_WhenContainerIsRemoved_DoesNotIncludeRemovedContainer()
        {
            EffectContainer firstContainer = new EffectContainer();
            EffectContainer secondContainer = new EffectContainer();
            firstContainer.Add(firstManaRegenerationEffect);
            secondContainer.Add(secondManaRegenerationEffect);
            EffectAggregator aggregator = new EffectAggregator();
            aggregator.Add(firstContainer);
            aggregator.Add(secondContainer);

            bool removed = aggregator.Remove(secondContainer);
            float value = aggregator.GetEffectValue(EffectStat.ManaRegeneration, EffectModifierType.Percent);

            Assert.IsTrue(removed);
            Assert.AreEqual(0.05f, value, 0.0001f);
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
