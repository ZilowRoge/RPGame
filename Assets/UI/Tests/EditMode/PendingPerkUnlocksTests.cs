using System.Collections.Generic;
using NUnit.Framework;
using RPGame.Progression;
using RPGame.UI.Jobs;
using UnityEditor;
using UnityEngine;

namespace RPGame.UI.Tests
{
    public sealed class PendingPerkUnlocksTests
    {
        private readonly List<Object> objectsToDestroy = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = objectsToDestroy.Count - 1; i >= 0; i--)
            {
                if (objectsToDestroy[i] != null)
                {
                    Object.DestroyImmediate(objectsToDestroy[i]);
                }
            }

            objectsToDestroy.Clear();
        }

        [Test]
        public void Toggle_WhenStartingPerkIsPending_MakesConnectedPerkAvailable()
        {
            PerkDefinition start = CreatePerk("start", isStartingPerk: true, "next");
            PerkDefinition next = CreatePerk("next", isStartingPerk: false);
            JobDefinition jobDefinition = CreateJobDefinition("Wizard", start, next);
            JobInstance job = new JobInstance(jobDefinition, currentLevel: 1, currentXP: 0, totalInvestedXP: 0, jobPoints: 2);
            CharacterProgression progression = CreateProgression();
            PendingPerkUnlocks pending = new();

            pending.Toggle(job, start, progression);

            Assert.IsTrue(pending.IsPending(start));
            Assert.AreEqual(PerkUnlockState.Available, pending.GetPreviewUnlockState(job, next, progression));
        }

        [Test]
        public void Toggle_WhenPendingCostUsesAllJobPoints_BlocksAdditionalPerks()
        {
            PerkDefinition start = CreatePerk("start", isStartingPerk: true, "next");
            PerkDefinition next = CreatePerk("next", isStartingPerk: false);
            JobDefinition jobDefinition = CreateJobDefinition("Wizard", start, next);
            JobInstance job = new JobInstance(jobDefinition, currentLevel: 1, currentXP: 0, totalInvestedXP: 0, jobPoints: 1);
            CharacterProgression progression = CreateProgression();
            PendingPerkUnlocks pending = new();

            pending.Toggle(job, start, progression);

            Assert.AreEqual(PerkUnlockState.Locked, pending.GetPreviewUnlockState(job, next, progression));
        }

        private CharacterProgression CreateProgression()
        {
            GameObject gameObject = new GameObject("CharacterProgression", typeof(CharacterProgression));
            objectsToDestroy.Add(gameObject);
            return gameObject.GetComponent<CharacterProgression>();
        }

        private PerkDefinition CreatePerk(
            string perkId,
            bool isStartingPerk,
            params string[] connectedPerkIds)
        {
            PerkDefinition perk = ScriptableObject.CreateInstance<PerkDefinition>();
            objectsToDestroy.Add(perk);

            SerializedObject serializedPerk = new SerializedObject(perk);
            serializedPerk.FindProperty("perkId").stringValue = perkId;
            serializedPerk.FindProperty("displayName").stringValue = perkId;
            serializedPerk.FindProperty("description").stringValue = "Test perk";
            serializedPerk.FindProperty("cost").intValue = 1;
            serializedPerk.FindProperty("isStartingPerk").boolValue = isStartingPerk;

            SerializedProperty connectedIds = serializedPerk.FindProperty("connectedPerkIds");
            connectedIds.arraySize = connectedPerkIds.Length;
            for (int i = 0; i < connectedPerkIds.Length; i++)
            {
                connectedIds.GetArrayElementAtIndex(i).stringValue = connectedPerkIds[i];
            }

            serializedPerk.ApplyModifiedPropertiesWithoutUndo();
            return perk;
        }

        private JobDefinition CreateJobDefinition(string jobId, params PerkDefinition[] perks)
        {
            JobDefinition definition = ScriptableObject.CreateInstance<JobDefinition>();
            objectsToDestroy.Add(definition);

            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("jobId").stringValue = jobId;
            serializedDefinition.FindProperty("displayName").stringValue = jobId;
            serializedDefinition.FindProperty("description").stringValue = "Test job";
            serializedDefinition.FindProperty("tier").enumValueIndex = (int)JobTier.Basic;
            serializedDefinition.FindProperty("maxLevel").intValue = 3;
            serializedDefinition.FindProperty("baseXP").intValue = 50;
            serializedDefinition.FindProperty("xpGrowthRate").floatValue = 2f;

            SerializedProperty jobPerks = serializedDefinition.FindProperty("jobPerks");
            jobPerks.arraySize = perks.Length;
            for (int i = 0; i < perks.Length; i++)
            {
                jobPerks.GetArrayElementAtIndex(i).objectReferenceValue = perks[i];
            }

            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }
    }
}
