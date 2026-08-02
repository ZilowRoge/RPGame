using NUnit.Framework;
using RPGame.Core.Effects;
using UnityEditor;
using UnityEngine;

namespace RPGame.Core.Tests.Effects
{
    public sealed class EffectContainerTests
    {
        private StatEffectDefinition maxHealthEffect;
        private StatEffectDefinition manaRegenerationEffect;

        [SetUp]
        public void SetUp()
        {
            maxHealthEffect = CreateStatEffect(EffectStat.MaxHealth, EffectModifierType.Flat, 10f);
            manaRegenerationEffect = CreateStatEffect(EffectStat.ManaRegeneration, EffectModifierType.Percent, 0.05f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(maxHealthEffect);
            Object.DestroyImmediate(manaRegenerationEffect);
        }

        [Test]
        public void Add_WhenDefinitionIsFlatStatEffect_StoresRuntimeEffect()
        {
            EffectContainer container = new EffectContainer();

            container.Add(maxHealthEffect);

            Assert.AreEqual(1, container.Effects.Count);
            EffectInstance effect = container.Effects[0];
            Assert.AreSame(maxHealthEffect, effect.Definition);
            Assert.AreEqual(EffectStat.MaxHealth, effect.Stat);
            Assert.AreEqual(EffectModifierType.Flat, effect.ModifierType);
            Assert.AreEqual(10f, effect.Value, 0.0001f);
        }

        [Test]
        public void GetEffectValue_WhenMaxHealthHasFlatBonus_ReturnsFlatValue()
        {
            EffectContainer container = new EffectContainer();
            container.Add(maxHealthEffect);

            float value = container.GetEffectValue(EffectStat.MaxHealth, EffectModifierType.Flat);

            Assert.AreEqual(10f, value, 0.0001f);
        }

        [Test]
        public void GetEffectValue_WhenManaRegenerationHasPercentageBonus_ReturnsFractionalValue()
        {
            EffectContainer container = new EffectContainer();
            container.Add(manaRegenerationEffect);

            float value = container.GetEffectValue(EffectStat.ManaRegeneration, EffectModifierType.Percent);

            Assert.AreEqual(0.05f, value, 0.0001f);
        }

        [Test]
        public void EffectStat_AttributesUseFirstEnumValues()
        {
            Assert.AreEqual(0, (int)EffectStat.Strength);
            Assert.AreEqual(1, (int)EffectStat.Dexterity);
            Assert.AreEqual(2, (int)EffectStat.Endurance);
            Assert.AreEqual(3, (int)EffectStat.Vitality);
            Assert.AreEqual(4, (int)EffectStat.Intelligence);
            Assert.AreEqual(5, (int)EffectStat.Power);
            Assert.AreEqual(6, (int)EffectStat.MaxHealth);
            Assert.AreEqual(7, (int)EffectStat.ManaRegeneration);
        }

        [TestCase(EffectStat.Strength)]
        [TestCase(EffectStat.Dexterity)]
        [TestCase(EffectStat.Endurance)]
        [TestCase(EffectStat.Vitality)]
        [TestCase(EffectStat.Intelligence)]
        [TestCase(EffectStat.Power)]
        public void GetEffectValue_WhenAttributeHasFlatBonus_ReturnsFlatValue(EffectStat attributeStat)
        {
            StatEffectDefinition attributeEffect = CreateStatEffect(attributeStat, EffectModifierType.Flat, 3f);
            EffectContainer container = new EffectContainer();
            container.Add(attributeEffect);

            float value = container.GetEffectValue(attributeStat, EffectModifierType.Flat);

            Assert.AreEqual(3f, value, 0.0001f);
            Object.DestroyImmediate(attributeEffect);
        }

        [Test]
        public void GetEffectValue_WhenMultiplePercentageEffects_ReturnsSumOfFractions()
        {
            StatEffectDefinition secondEffect = CreateStatEffect(EffectStat.ManaRegeneration, EffectModifierType.Percent, 0.10f);
            EffectContainer container = new EffectContainer();
            container.Add(manaRegenerationEffect);
            container.Add(secondEffect);

            float bonus = container.GetEffectValue(EffectStat.ManaRegeneration, EffectModifierType.Percent);

            Assert.AreEqual(0.15f, bonus, 0.0001f);
            Object.DestroyImmediate(secondEffect);
        }

        [Test]
        public void ToString_WhenEffectIsFlat_FormatsTooltipText()
        {
            Assert.AreEqual("+10 Max Health", maxHealthEffect.ToString());
        }

        [Test]
        public void ToString_WhenEffectIsPercent_FormatsFractionAsPercentTooltipText()
        {
            Assert.AreEqual("+5% Mana Regeneration", manaRegenerationEffect.ToString());
        }

        [Test]
        public void ToString_WhenEffectIsAttribute_FormatsAttributeTooltipText()
        {
            StatEffectDefinition strengthEffect = CreateStatEffect(EffectStat.Strength, EffectModifierType.Flat, 2f);

            Assert.AreEqual("+2 Strength", strengthEffect.ToString());
            Object.DestroyImmediate(strengthEffect);
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
