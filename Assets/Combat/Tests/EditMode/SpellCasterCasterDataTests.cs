using NUnit.Framework;
using RPGame.Combat.Spells;
using RPGame.Core.Spells;
using RPGame.Core.Statistics.Attributes;
using UnityEditor;
using UnityEngine;

namespace RPGame.Combat.Tests
{
    public sealed class SpellCasterCasterDataTests
    {
        private GameObject gameObject;
        private CharacterAttributesConfig config;
        private CharacterAttributes attributes;
        private SpellCaster spellCaster;
        private CaptureCasterDataSpell spell;

        [SetUp]
        public void SetUp()
        {
            config = CreateConfig(power: 6);
            gameObject = new GameObject("SpellCasterCasterDataTests");
            attributes = gameObject.AddComponent<CharacterAttributes>();
            SetAttributesConfig(attributes, config);
            spellCaster = gameObject.AddComponent<SpellCaster>();
            spell = ScriptableObject.CreateInstance<CaptureCasterDataSpell>();
            spellCaster.SetSpell(spell);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
            Object.DestroyImmediate(config);
            Object.DestroyImmediate(spell);
        }

        [Test]
        public void TryCast_PassesCurrentCharacterAttributes()
        {
            bool wasCast = spellCaster.TryCast();

            Assert.IsTrue(wasCast);
            Assert.IsNotNull(spell.LastCasterData.Attributes);
            Assert.AreEqual(6, spell.LastCasterData.Attributes.Power);
        }

        private static CharacterAttributesConfig CreateConfig(int power)
        {
            CharacterAttributesConfig attributesConfig = ScriptableObject.CreateInstance<CharacterAttributesConfig>();
            SerializedObject serializedConfig = new(attributesConfig);
            serializedConfig.FindProperty("strength").intValue = 0;
            serializedConfig.FindProperty("dexterity").intValue = 0;
            serializedConfig.FindProperty("endurance").intValue = 0;
            serializedConfig.FindProperty("vitality").intValue = 0;
            serializedConfig.FindProperty("intelligence").intValue = 0;
            serializedConfig.FindProperty("power").intValue = power;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return attributesConfig;
        }

        private static void SetAttributesConfig(
            CharacterAttributes characterAttributes,
            CharacterAttributesConfig attributesConfig)
        {
            SerializedObject serializedAttributes = new(characterAttributes);
            serializedAttributes.FindProperty("config").objectReferenceValue = attributesConfig;
            serializedAttributes.ApplyModifiedPropertiesWithoutUndo();
        }

        private sealed class CaptureCasterDataSpell : Spell
        {
            public CasterData LastCasterData { get; private set; }

            public override void OnCast(CasterData casterData)
            {
                LastCasterData = casterData;
            }
        }
    }
}
