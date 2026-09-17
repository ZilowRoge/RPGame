using System.Collections.Generic;
using NUnit.Framework;
using RPGame.Core.Spells;
using UnityEditor;
using UnityEngine;

namespace RPGame.Progression.Tests
{
    public sealed class RuntimeSpellBehaviorProviderTests
    {
        private readonly List<Object> createdObjects = new();

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
        public void CreateRuntimeBehaviors_WhenPerkIsLocked_ReturnsEmpty()
        {
            CharacterProgression progression = CreateProgressionWithPerk(
                out _, out _, out RuntimeSpellBehaviorDefinition definition);

            IReadOnlyList<IRuntimeSpellBehavior> behaviors =
                progression.CreateRuntimeBehaviors(CreateSpell(), null);

            Assert.IsEmpty(behaviors);
        }

        [Test]
        public void CreateRuntimeBehaviors_WhenPerkIsUnlockedForSpell_CreatesBehaviorWithCaster()
        {
            CharacterProgression progression = CreateProgressionWithPerk(
                out PerkDefinition perk, out JobInstance job, out RuntimeSpellBehaviorDefinition definition);
            GameObject caster = CreateGameObject("Caster");
            Spell spell = CreateSpell();
            UnlockPerk(progression, job, perk);

            IReadOnlyList<IRuntimeSpellBehavior> behaviors =
                progression.CreateRuntimeBehaviors(spell, caster);

            Assert.AreEqual(1, behaviors.Count);
            Assert.AreSame(caster, ((TestRuntimeBehavior)behaviors[0]).Source);
        }

        [Test]
        public void CreateRuntimeBehaviors_AggregatesDefinitionsAndCreatesPerCastInstances()
        {
            CharacterProgression progression = CreateProgressionWithPerk(
                out PerkDefinition perk, out JobInstance job, out RuntimeSpellBehaviorDefinition firstDefinition);
            RuntimeSpellBehaviorDefinition secondDefinition = ScriptableObject.CreateInstance<TestRuntimeBehaviorDefinition>();
            createdObjects.Add(secondDefinition);
            SetRuntimeBehaviorDefinitions(perk, firstDefinition, secondDefinition);
            UnlockPerk(progression, job, perk);
            Spell spell = CreateSpell();

            IReadOnlyList<IRuntimeSpellBehavior> firstCast =
                progression.CreateRuntimeBehaviors(spell, null);
            IReadOnlyList<IRuntimeSpellBehavior> secondCast =
                progression.CreateRuntimeBehaviors(spell, null);

            Assert.AreEqual(2, firstCast.Count);
            Assert.AreEqual(2, secondCast.Count);
            Assert.AreNotSame(firstCast[0], secondCast[0]);
            Assert.AreNotSame(firstCast[1], secondCast[1]);
        }

        [Test]
        public void CreateRuntimeBehaviors_WhenDefinitionRejectsSpell_ReturnsEmpty()
        {
            CharacterProgression progression = CreateProgressionWithPerk(
                out PerkDefinition perk, out JobInstance job, out RuntimeSpellBehaviorDefinition definition);
            UnlockPerk(progression, job, perk);
            Spell spell = CreateSpell();
            ((TestRuntimeBehaviorDefinition)definition).AcceptsSpell = false;

            IReadOnlyList<IRuntimeSpellBehavior> behaviors =
                progression.CreateRuntimeBehaviors(spell, null);

            Assert.IsEmpty(behaviors);
        }

        private CharacterProgression CreateProgressionWithPerk(
            out PerkDefinition perk,
            out JobInstance job,
            out RuntimeSpellBehaviorDefinition definition)
        {
            GameObject progressionObject = CreateGameObject("Progression");
            CharacterProgression progression = progressionObject.AddComponent<CharacterProgression>();
            definition = ScriptableObject.CreateInstance<TestRuntimeBehaviorDefinition>();
            createdObjects.Add(definition);
            perk = CreatePerk(definition);
            JobDefinition jobDefinition = CreateJobDefinition(perk);
            progression.Jobs.UnlockJob(jobDefinition);
            job = progression.Jobs.GetJob(jobDefinition.JobId);
            return progression;
        }

        private PerkDefinition CreatePerk(RuntimeSpellBehaviorDefinition definition)
        {
            PerkDefinition perk = ScriptableObject.CreateInstance<PerkDefinition>();
            createdObjects.Add(perk);
            SetPrivateList(perk, "runtimeBehaviorDefinitions", definition);
            return perk;
        }

        private JobDefinition CreateJobDefinition(PerkDefinition perk)
        {
            JobDefinition definition = ScriptableObject.CreateInstance<JobDefinition>();
            createdObjects.Add(definition);
            SerializedObject serializedDefinition = new(definition);
            serializedDefinition.FindProperty("jobId").stringValue = "Wizard";
            serializedDefinition.FindProperty("maxLevel").intValue = 3;
            serializedDefinition.FindProperty("baseXP").intValue = 50;
            serializedDefinition.FindProperty("xpGrowthRate").floatValue = 2f;
            SerializedProperty jobPerks = serializedDefinition.FindProperty("jobPerks");
            jobPerks.arraySize = 1;
            jobPerks.GetArrayElementAtIndex(0).objectReferenceValue = perk;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }

        private static void SetRuntimeBehaviorDefinitions(
            PerkDefinition perk,
            params RuntimeSpellBehaviorDefinition[] definitions)
        {
            SetPrivateList(perk, "runtimeBehaviorDefinitions", definitions);
        }

        private static void SetPrivateList<T>(Object target, string propertyName, params T[] values)
            where T : Object
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void UnlockPerk(CharacterProgression progression, JobInstance job, PerkDefinition perk)
        {
            job.AddExperience(job.GetXPToNextLevel());
            Assert.IsTrue(progression.TryUnlockPerk(job, perk));
        }

        private GameObject CreateGameObject(string objectName)
        {
            GameObject gameObject = new(objectName);
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private Spell CreateSpell()
        {
            Spell spell = ScriptableObject.CreateInstance<TestSpell>();
            createdObjects.Add(spell);
            return spell;
        }

        private sealed class TestSpell : Spell
        {
            public override void OnCast(CasterData casterData)
            {
            }
        }

        private sealed class TestRuntimeBehaviorDefinition : RuntimeSpellBehaviorDefinition
        {
            public bool AcceptsSpell { get; set; } = true;

            public override bool TryCreate(
                Spell spell,
                GameObject casterObject,
                out IRuntimeSpellBehavior behavior)
            {
                behavior = AcceptsSpell ? new TestRuntimeBehavior(casterObject) : null;
                return behavior != null;
            }
        }

        private sealed class TestRuntimeBehavior : IRuntimeSpellBehavior
        {
            public TestRuntimeBehavior(GameObject source)
            {
                Source = source;
            }

            public GameObject Source { get; }
        }
    }
}
