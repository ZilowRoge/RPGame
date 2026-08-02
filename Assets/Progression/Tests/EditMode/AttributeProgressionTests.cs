using NUnit.Framework;
using RPGame.Core.Effects;
using RPGame.Core.Statistics.Attributes;
using RPGame.Progression;
using UnityEditor;
using UnityEngine;

namespace RPGame.Progression.Tests
{
    public sealed class AttributeProgressionTests
    {
        private GameObject gameObject;
        private CharacterAttributesConfig config;
        private CharacterAttributes attributes;
        private CharacterProgression progression;

        [SetUp]
        public void SetUp()
        {
            config = CreateConfig(
                strength: 10,
                dexterity: 8,
                endurance: 12,
                vitality: 15,
                intelligence: 11,
                power: 6);

            gameObject = new GameObject("AttributeProgressionTests");
            attributes = gameObject.AddComponent<CharacterAttributes>();
            progression = gameObject.AddComponent<CharacterProgression>();
            SetAttributesConfig(attributes, config);
            SetProgressionAttributes(progression, attributes);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
            UnityEngine.Object.DestroyImmediate(config);
        }

        [Test]
        public void GetNextAttributePointCost_WhenNoPointsPurchased_ReturnsBaseCost()
        {
            Assert.AreEqual(100, progression.GetNextAttributePointCost(CharacterAttributeType.Power));
        }

        [Test]
        public void GetNextAttributePointCost_WhenPointWasPurchased_ReturnsIncreasedCost()
        {
            progression.AddExperience(500);
            progression.TryBuyAttributePoint(CharacterAttributeType.Power);

            Assert.AreEqual(150, progression.GetNextAttributePointCost(CharacterAttributeType.Power));
        }

        [Test]
        public void TryBuyAttributePoint_WhenEnoughXP_SpendsXP()
        {
            progression.AddExperience(250);

            AttributePurchaseResult result = progression.TryBuyAttributePoint(CharacterAttributeType.Strength);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(100, result.SpentXP);
            Assert.AreEqual(150, progression.GetAvailableXP());
        }

        [Test]
        public void TryBuyAttributePoint_WhenSuccessful_IncreasesOnlySelectedAttribute()
        {
            progression.AddExperience(250);

            progression.TryBuyAttributePoint(CharacterAttributeType.Power);

            Assert.AreEqual(10, attributes.Strength);
            Assert.AreEqual(8, attributes.Dexterity);
            Assert.AreEqual(12, attributes.Endurance);
            Assert.AreEqual(15, attributes.Vitality);
            Assert.AreEqual(11, attributes.Intelligence);
            Assert.AreEqual(7, attributes.Power);
        }

        [Test]
        public void TryBuyAttributePoint_WhenNotEnoughXP_DoesNotChangeState()
        {
            progression.AddExperience(50);

            AttributePurchaseResult result = progression.TryBuyAttributePoint(CharacterAttributeType.Power);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(0, result.SpentXP);
            Assert.AreEqual(50, progression.GetAvailableXP());
            Assert.AreEqual(0, attributes.GetPurchasedPoints(CharacterAttributeType.Power));
            Assert.AreEqual(6, attributes.Power);
        }

        [Test]
        public void GetNextAttributePointCost_WhenPerkBonusExists_DoesNotUseEffectBonus()
        {
            EffectAggregator aggregator = gameObject.AddComponent<EffectAggregator>();
            StatEffectDefinition powerEffect = CreateStatEffect(EffectStat.Power, EffectModifierType.Flat, 5f);
            aggregator.Add(powerEffect);

            Assert.AreEqual(11, attributes.Power);
            Assert.AreEqual(100, progression.GetNextAttributePointCost(CharacterAttributeType.Power));
            UnityEngine.Object.DestroyImmediate(powerEffect);
        }

        [Test]
        public void TryBuyAttributePoint_DoesNotModifyConfig()
        {
            progression.AddExperience(250);

            progression.TryBuyAttributePoint(CharacterAttributeType.Endurance);

            Assert.AreEqual(12, config.Endurance);
            Assert.AreEqual(13, attributes.Endurance);
        }

        [Test]
        public void TryBuyAttributePoint_WhenAttributeHasBasePurchasedAndEffectValues_ReturnsFinalValue()
        {
            EffectAggregator aggregator = gameObject.AddComponent<EffectAggregator>();
            StatEffectDefinition powerEffect = CreateStatEffect(EffectStat.Power, EffectModifierType.Flat, 5f);
            progression.AddExperience(250);
            progression.TryBuyAttributePoint(CharacterAttributeType.Power);
            aggregator.Add(powerEffect);

            Assert.AreEqual(12, attributes.Power);
            UnityEngine.Object.DestroyImmediate(powerEffect);
        }

        [TestCase(CharacterAttributeType.Strength)]
        [TestCase(CharacterAttributeType.Dexterity)]
        [TestCase(CharacterAttributeType.Endurance)]
        [TestCase(CharacterAttributeType.Vitality)]
        [TestCase(CharacterAttributeType.Intelligence)]
        [TestCase(CharacterAttributeType.Power)]
        public void TryBuyAttributePoint_AllAttributesCanBeIncreased(CharacterAttributeType attributeType)
        {
            int initialValue = attributes.GetValue(attributeType);
            progression.AddExperience(250);

            AttributePurchaseResult result = progression.TryBuyAttributePoint(attributeType);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(initialValue + 1, attributes.GetValue(attributeType));
        }

        private static CharacterAttributesConfig CreateConfig(
            int strength,
            int dexterity,
            int endurance,
            int vitality,
            int intelligence,
            int power)
        {
            CharacterAttributesConfig attributesConfig = ScriptableObject.CreateInstance<CharacterAttributesConfig>();
            SerializedObject serializedConfig = new SerializedObject(attributesConfig);
            serializedConfig.FindProperty("strength").intValue = strength;
            serializedConfig.FindProperty("dexterity").intValue = dexterity;
            serializedConfig.FindProperty("endurance").intValue = endurance;
            serializedConfig.FindProperty("vitality").intValue = vitality;
            serializedConfig.FindProperty("intelligence").intValue = intelligence;
            serializedConfig.FindProperty("power").intValue = power;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return attributesConfig;
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

        private static void SetAttributesConfig(
            CharacterAttributes characterAttributes,
            CharacterAttributesConfig attributesConfig)
        {
            SerializedObject serializedAttributes = new SerializedObject(characterAttributes);
            serializedAttributes.FindProperty("config").objectReferenceValue = attributesConfig;
            serializedAttributes.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetProgressionAttributes(
            CharacterProgression characterProgression,
            CharacterAttributes characterAttributes)
        {
            SerializedObject serializedProgression = new SerializedObject(characterProgression);
            serializedProgression.FindProperty("attributes").objectReferenceValue = characterAttributes;
            serializedProgression.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
