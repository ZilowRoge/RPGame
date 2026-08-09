using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RPGame.Core.Effects;
using RPGame.Core.Statistics;
using RPGame.Core.Statistics.Attributes;
using UnityEditor;
using UnityEngine;

namespace RPGame.Core.Tests.Attributes
{
    public sealed class CharacterAttributesTests
    {
        private GameObject gameObject;
        private CharacterAttributesConfig config;
        private CharacterAttributes attributes;

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

            gameObject = new GameObject("CharacterAttributesTests");
            attributes = gameObject.AddComponent<CharacterAttributes>();
            SetConfig(attributes, config);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
            UnityEngine.Object.DestroyImmediate(config);
        }

        [Test]
        public void Properties_ReturnConfiguredAttributeValues()
        {
            Assert.AreEqual(10, attributes.Strength);
            Assert.AreEqual(8, attributes.Dexterity);
            Assert.AreEqual(12, attributes.Endurance);
            Assert.AreEqual(15, attributes.Vitality);
            Assert.AreEqual(11, attributes.Intelligence);
            Assert.AreEqual(6, attributes.Power);
        }

        [TestCase(CharacterAttributeType.Strength, 10)]
        [TestCase(CharacterAttributeType.Dexterity, 8)]
        [TestCase(CharacterAttributeType.Endurance, 12)]
        [TestCase(CharacterAttributeType.Vitality, 15)]
        [TestCase(CharacterAttributeType.Intelligence, 11)]
        [TestCase(CharacterAttributeType.Power, 6)]
        public void GetValue_ReturnsConfiguredValueForAttributeType(CharacterAttributeType attributeType, int expectedValue)
        {
            Assert.AreEqual(expectedValue, attributes.GetValue(attributeType));
        }

        [Test]
        public void GetValue_WhenEffectAggregatorHasFlatAttributeBonus_ReturnsBaseValueWithBonus()
        {
            EffectAggregator aggregator = gameObject.AddComponent<EffectAggregator>();
            StatEffectDefinition powerBonus = CreateStatEffect(EffectStat.Power, EffectModifierType.Flat, 5f);
            aggregator.Add(powerBonus);

            Assert.AreEqual(11, attributes.Power);
            Assert.AreEqual(11, attributes.GetValue(CharacterAttributeType.Power));
            UnityEngine.Object.DestroyImmediate(powerBonus);
        }

        [Test]
        public void GetValue_WhenEffectAggregatorHasMultipleFlatAttributeBonuses_ReturnsBaseValueWithSummedBonuses()
        {
            EffectAggregator aggregator = gameObject.AddComponent<EffectAggregator>();
            StatEffectDefinition firstBonus = CreateStatEffect(EffectStat.Intelligence, EffectModifierType.Flat, 1f);
            StatEffectDefinition secondBonus = CreateStatEffect(EffectStat.Intelligence, EffectModifierType.Flat, 3f);
            aggregator.Add(firstBonus);
            aggregator.Add(secondBonus);

            Assert.AreEqual(15, attributes.Intelligence);
            UnityEngine.Object.DestroyImmediate(firstBonus);
            UnityEngine.Object.DestroyImmediate(secondBonus);
        }

        [Test]
        public void GetValue_WhenEffectAggregatorHasPercentAttributeBonus_IgnoresPercentBonus()
        {
            EffectAggregator aggregator = gameObject.AddComponent<EffectAggregator>();
            StatEffectDefinition powerBonus = CreateStatEffect(EffectStat.Power, EffectModifierType.Percent, 0.5f);
            aggregator.Add(powerBonus);

            Assert.AreEqual(6, attributes.Power);
            UnityEngine.Object.DestroyImmediate(powerBonus);
        }

        [Test]
        public void GetValue_WhenAttributeHasPurchasedPoints_ReturnsBaseValueWithPurchasedPoints()
        {
            attributes.AddPurchasedPoints(CharacterAttributeType.Strength, 2);

            Assert.AreEqual(12, attributes.Strength);
            Assert.AreEqual(2, attributes.GetPurchasedPoints(CharacterAttributeType.Strength));
        }

        [Test]
        public void GetValue_WhenAttributeHasPurchasedPointsAndEffects_ReturnsBasePurchasedAndEffectValue()
        {
            EffectAggregator aggregator = gameObject.AddComponent<EffectAggregator>();
            StatEffectDefinition powerBonus = CreateStatEffect(EffectStat.Power, EffectModifierType.Flat, 5f);
            attributes.AddPurchasedPoints(CharacterAttributeType.Power, 2);
            aggregator.Add(powerBonus);

            Assert.AreEqual(13, attributes.Power);
            UnityEngine.Object.DestroyImmediate(powerBonus);
        }

        [Test]
        public void AddPurchasedPoints_DoesNotModifyConfig()
        {
            attributes.AddPurchasedPoints(CharacterAttributeType.Endurance, 3);

            Assert.AreEqual(12, config.Endurance);
            Assert.AreEqual(12, attributes.GetBaseValue(CharacterAttributeType.Endurance));
            Assert.AreEqual(15, attributes.Endurance);
        }

        [Test]
        public void AddPurchasedPoints_WhenPointsAreAdded_RaisesValuesChanged()
        {
            int changedCount = 0;
            attributes.ValuesChanged += () => changedCount++;

            attributes.AddPurchasedPoints(CharacterAttributeType.Power, 1);

            Assert.AreEqual(1, changedCount);
        }

        [Test]
        public void AddPurchasedPoints_WhenPointsAreNotPositive_DoesNotRaiseValuesChanged()
        {
            int changedCount = 0;
            attributes.ValuesChanged += () => changedCount++;

            attributes.AddPurchasedPoints(CharacterAttributeType.Power, 0);

            Assert.AreEqual(0, changedCount);
        }

        [Test]
        public void StatisticsDataProvider_WhenAttributesChange_RaisesChanged()
        {
            CharacterStatisticsDataProvider provider = gameObject.AddComponent<CharacterStatisticsDataProvider>();
            provider.SetCharacterAttributes(attributes);
            int changedCount = 0;
            provider.Changed += () => changedCount++;

            attributes.AddPurchasedPoints(CharacterAttributeType.Power, 1);

            Assert.AreEqual(1, changedCount);
        }

        [Test]
        public void PublicApi_ExposesAttributeValuesAsReadOnly()
        {
            PropertyInfo[] properties = typeof(ICharacterAttributes).GetProperties();

            Assert.IsTrue(properties.All(property => property.CanRead));
            Assert.IsTrue(properties.All(property => property.SetMethod == null));
            Assert.IsEmpty(typeof(ICharacterAttributes).GetFields(BindingFlags.Public | BindingFlags.Instance));
        }

        [Test]
        public void Values_CanBeDifferentAndAreNotLinked()
        {
            int[] values =
            {
                attributes.Strength,
                attributes.Dexterity,
                attributes.Endurance,
                attributes.Vitality,
                attributes.Intelligence,
                attributes.Power
            };

            Assert.AreEqual(6, values.Distinct().Count());
        }

        [Test]
        public void GetValue_WhenConfigIsMissing_ThrowsClearException()
        {
            GameObject objectWithoutConfig = new GameObject("AttributesWithoutConfig");
            CharacterAttributes attributesWithoutConfig = objectWithoutConfig.AddComponent<CharacterAttributes>();

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => attributesWithoutConfig.GetValue(CharacterAttributeType.Strength));

            StringAssert.Contains(nameof(CharacterAttributesConfig), exception.Message);
            UnityEngine.Object.DestroyImmediate(objectWithoutConfig);
        }

        [Test]
        public void System_DoesNotDependOnProgressionAssembly()
        {
            AssemblyName[] references = typeof(CharacterAttributes).Assembly.GetReferencedAssemblies();

            Assert.IsFalse(references.Any(reference => reference.Name == "RPGame.Progression"));
        }

        [Test]
        public void PublicApi_DoesNotExposePerksOrEffects()
        {
            Type[] apiTypes =
            {
                typeof(CharacterAttributeType),
                typeof(CharacterAttributesConfig),
                typeof(ICharacterAttributes),
                typeof(CharacterAttributes)
            };

            foreach (Type apiType in apiTypes)
            {
                Assert.IsFalse(HasPublicDependency(apiType, "RPGame.Progression"), apiType.FullName);
                Assert.IsFalse(HasPublicDependency(apiType, "RPGame.Core.Effects"), apiType.FullName);
            }
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

        private static void SetConfig(CharacterAttributes characterAttributes, CharacterAttributesConfig attributesConfig)
        {
            SerializedObject serializedAttributes = new SerializedObject(characterAttributes);
            serializedAttributes.FindProperty("config").objectReferenceValue = attributesConfig;
            serializedAttributes.ApplyModifiedPropertiesWithoutUndo();
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

        private static bool HasPublicDependency(Type apiType, string namespacePrefix)
        {
            return apiType.GetProperties().Any(property => IsFromNamespace(property.PropertyType, namespacePrefix))
                || apiType.GetMethods().Any(method => IsFromNamespace(method.ReturnType, namespacePrefix))
                || apiType.GetMethods()
                    .SelectMany(method => method.GetParameters())
                    .Any(parameter => IsFromNamespace(parameter.ParameterType, namespacePrefix));
        }

        private static bool IsFromNamespace(Type type, string namespacePrefix)
        {
            return type.Namespace != null && type.Namespace.StartsWith(namespacePrefix, StringComparison.Ordinal);
        }
    }
}
