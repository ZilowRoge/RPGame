using NUnit.Framework;
using RPGame.Progression;
using UnityEditor;
using UnityEngine;

namespace RPGame.Progression.Tests
{
    public sealed class PerkProgressionTests
    {
        private JobDefinition jobDefinition;
        private PerkDefinition startingPerk;
        private PerkDefinition connectedPerk;
        private PerkDefinition outgoingConnectedPerk;
        private PerkDefinition isolatedPerk;
        private PerkDefinition foreignPerk;
        private PerkProgression service;

        [SetUp]
        public void SetUp()
        {
            startingPerk = CreatePerkDefinition("start", isStartingPerk: true, "outgoingConnected");
            connectedPerk = CreatePerkDefinition("connected", isStartingPerk: false, "start");
            outgoingConnectedPerk = CreatePerkDefinition("outgoingConnected", isStartingPerk: false);
            isolatedPerk = CreatePerkDefinition("isolated", isStartingPerk: false);
            foreignPerk = CreatePerkDefinition("foreign", isStartingPerk: true);
            jobDefinition = CreateJobDefinition("Wizard", startingPerk, connectedPerk, outgoingConnectedPerk, isolatedPerk);
            service = new PerkProgression();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(jobDefinition);
            Object.DestroyImmediate(startingPerk);
            Object.DestroyImmediate(connectedPerk);
            Object.DestroyImmediate(outgoingConnectedPerk);
            Object.DestroyImmediate(isolatedPerk);
            Object.DestroyImmediate(foreignPerk);
        }

        [Test]
        public void GetUnlockState_WhenStartingPerkHasEnoughJobPoints_ReturnsAvailable()
        {
            JobInstance job = CreateJob(jobPoints: 1);

            PerkUnlockState state = service.GetUnlockState(job, startingPerk);

            Assert.AreEqual(PerkUnlockState.Available, state);
        }

        [Test]
        public void GetUnlockState_WhenUnlockedPerkListsThisPerkAsNeighbor_ReturnsAvailable()
        {
            JobInstance job = CreateJob(jobPoints: 2);
            service.TryUnlockPerk(job, startingPerk);

            PerkUnlockState state = service.GetUnlockState(job, outgoingConnectedPerk);

            Assert.AreEqual(PerkUnlockState.Available, state);
        }

        [Test]
        public void TryUnlockPerk_WhenAvailable_SpendsJobPointStoresPerkAndRaisesEvent()
        {
            JobInstance job = CreateJob(jobPoints: 1);
            JobInstance eventJob = null;
            PerkDefinition eventPerk = null;
            service.PerkUnlocked += (unlockedJob, unlockedPerk) =>
            {
                eventJob = unlockedJob;
                eventPerk = unlockedPerk;
            };

            bool unlocked = service.TryUnlockPerk(job, startingPerk);

            Assert.IsTrue(unlocked);
            Assert.AreEqual(0, job.JobPoints);
            CollectionAssert.Contains(job.UnlockedPerkIds, startingPerk.PerkId);
            Assert.AreSame(job, eventJob);
            Assert.AreSame(startingPerk, eventPerk);
        }

        [Test]
        public void GetUnlockState_WhenConnectedPerkNeighborIsUnlocked_ReturnsAvailable()
        {
            JobInstance job = CreateJob(jobPoints: 2);
            service.TryUnlockPerk(job, startingPerk);

            PerkUnlockState state = service.GetUnlockState(job, connectedPerk);

            Assert.AreEqual(PerkUnlockState.Available, state);
        }

        [Test]
        public void GetUnlockState_WhenPerkHasNoUnlockedNeighbor_ReturnsLocked()
        {
            JobInstance job = CreateJob(jobPoints: 1);

            PerkUnlockState state = service.GetUnlockState(job, isolatedPerk);

            Assert.AreEqual(PerkUnlockState.Locked, state);
        }

        [Test]
        public void TryUnlockPerk_WhenAlreadyUnlocked_ReturnsFalse()
        {
            JobInstance job = CreateJob(jobPoints: 2);

            bool firstUnlock = service.TryUnlockPerk(job, startingPerk);
            bool secondUnlock = service.TryUnlockPerk(job, startingPerk);

            Assert.IsTrue(firstUnlock);
            Assert.IsFalse(secondUnlock);
            Assert.AreEqual(1, job.JobPoints);
        }

        [Test]
        public void GetUnlockState_WhenJobDoesNotHavePerk_ReturnsLocked()
        {
            JobInstance job = CreateJob(jobPoints: 1);

            PerkUnlockState state = service.GetUnlockState(job, foreignPerk);

            Assert.AreEqual(PerkUnlockState.Locked, state);
        }

        [Test]
        public void GetUnlockState_WhenJobPointsAreInsufficient_ReturnsLocked()
        {
            JobInstance job = CreateJob(jobPoints: 0);

            PerkUnlockState state = service.GetUnlockState(job, startingPerk);

            Assert.AreEqual(PerkUnlockState.Locked, state);
        }

        private JobInstance CreateJob(int jobPoints)
        {
            return new JobInstance(jobDefinition, currentLevel: 1, currentXP: 0, totalInvestedXP: 0, jobPoints: jobPoints);
        }

        private static JobDefinition CreateJobDefinition(string jobId, params PerkDefinition[] perks)
        {
            JobDefinition definition = ScriptableObject.CreateInstance<JobDefinition>();
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

        private static PerkDefinition CreatePerkDefinition(
            string perkId,
            bool isStartingPerk,
            params string[] connectedPerkIds)
        {
            PerkDefinition definition = ScriptableObject.CreateInstance<PerkDefinition>();
            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("perkId").stringValue = perkId;
            serializedDefinition.FindProperty("displayName").stringValue = perkId;
            serializedDefinition.FindProperty("description").stringValue = "Test perk";
            serializedDefinition.FindProperty("cost").intValue = 1;
            serializedDefinition.FindProperty("isStartingPerk").boolValue = isStartingPerk;

            SerializedProperty connectedIds = serializedDefinition.FindProperty("connectedPerkIds");
            connectedIds.arraySize = connectedPerkIds.Length;
            for (int i = 0; i < connectedPerkIds.Length; i++)
            {
                connectedIds.GetArrayElementAtIndex(i).stringValue = connectedPerkIds[i];
            }

            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }
    }
}
