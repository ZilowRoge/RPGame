using NUnit.Framework;
using RPGame.Core.Progression;
using RPGame.Progression;
using UnityEditor;
using UnityEngine;

namespace RPGame.Progression.Tests
{
    public sealed class CharacterProgressionTests
    {
        private const string WizardJobId = "Wizard";

        private GameObject gameObject;
        private JobDefinition wizardDefinition;
        private CharacterProgression progression;

        [SetUp]
        public void SetUp()
        {
            wizardDefinition = CreateJobDefinition(WizardJobId, maxLevel: 3, baseXP: 50, xpGrowthRate: 2f);
            gameObject = new GameObject("CharacterProgressionTests");
            progression = gameObject.AddComponent<CharacterProgression>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
            Object.DestroyImmediate(wizardDefinition);
        }

        [Test]
        public void AddExperience_WhenAmountIsPositive_IncreasesAvailableXP()
        {
            progression.AddExperience(500);

            Assert.AreEqual(500, progression.GetAvailableXP());
        }

        [Test]
        public void AddExperience_ThroughCoreContract_IncreasesAvailableXP()
        {
            IExperienceReceiver receiver = progression;

            receiver.AddExperience(500);

            Assert.AreEqual(500, progression.GetAvailableXP());
        }

        [Test]
        public void AddExperience_WhenAmountIsNotPositive_IgnoresAmount()
        {
            progression.AddExperience(100);
            progression.AddExperience(0);
            progression.AddExperience(-50);

            Assert.AreEqual(100, progression.GetAvailableXP());
        }

        [Test]
        public void AddXPToJob_WhenNotEnoughGlobalXP_DoesNotChangeAvailableXP()
        {
            progression.AddExperience(50);
            progression.Jobs.UnlockJob(wizardDefinition);

            bool added = progression.Jobs.AddXPToJob(WizardJobId, 100);

            Assert.IsFalse(added);
            Assert.AreEqual(50, progression.GetAvailableXP());
            Assert.AreEqual(0, progression.Jobs.GetJob(WizardJobId).CurrentXP);
        }

        [Test]
        public void UnlockJob_CreatesUnlockedJobAtLevelOne()
        {
            progression.Jobs.UnlockJob(wizardDefinition);

            Assert.IsTrue(progression.Jobs.HasJob(WizardJobId));
            Assert.AreEqual(1, progression.Jobs.GetJobLevel(WizardJobId));
        }

        [Test]
        public void Jobs_UsesCharacterJobContainerState()
        {
            progression.Jobs.UnlockJob(wizardDefinition);

            Assert.IsTrue(progression.JobContainer.HasJob(WizardJobId));
            Assert.AreSame(progression.JobContainer.GetJob(WizardJobId), progression.Jobs.GetJob(WizardJobId));
        }

        [Test]
        public void AddXPToJob_SpendsGlobalXPAndInvestsPartialProgress()
        {
            progression.AddExperience(500);
            progression.Jobs.UnlockJob(wizardDefinition);

            bool added = progression.Jobs.AddXPToJob(WizardJobId, 50);

            Assert.IsTrue(added);
            Assert.AreEqual(450, progression.GetAvailableXP());
            Assert.AreEqual(1, progression.Jobs.GetJobLevel(WizardJobId));
            Assert.AreEqual(50, progression.Jobs.GetJob(WizardJobId).CurrentXP);
            Assert.AreEqual(50, progression.Jobs.GetJob(WizardJobId).TotalInvestedXP);
        }

        [Test]
        public void AddXPToJob_WhenThresholdIsReached_LevelsAutomatically()
        {
            progression.AddExperience(500);
            progression.Jobs.UnlockJob(wizardDefinition);

            progression.Jobs.AddXPToJob(WizardJobId, 100);

            Assert.AreEqual(2, progression.Jobs.GetJobLevel(WizardJobId));
            Assert.AreEqual(400, progression.GetAvailableXP());
            Assert.AreEqual(0, progression.Jobs.GetJob(WizardJobId).CurrentXP);
        }

        [Test]
        public void AddXPToJob_WhenThresholdIsReached_AddsJobPoint()
        {
            progression.AddExperience(500);
            progression.Jobs.UnlockJob(wizardDefinition);

            progression.Jobs.AddXPToJob(WizardJobId, 100);

            Assert.AreEqual(1, progression.Jobs.GetJob(WizardJobId).JobPoints);
        }

        [Test]
        public void AddXPToJob_WhenMultipleThresholdsAreReached_AddsJobPointPerLevel()
        {
            progression.AddExperience(500);
            progression.Jobs.UnlockJob(wizardDefinition);

            progression.Jobs.AddXPToJob(WizardJobId, 300);

            Assert.AreEqual(2, progression.Jobs.GetJob(WizardJobId).JobPoints);
        }

        [Test]
        public void AddXPToJob_WhenAmountWouldExceedMaxLevel_OnlySpendsAcceptedXP()
        {
            progression.AddExperience(500);
            progression.Jobs.UnlockJob(wizardDefinition);

            bool added = progression.Jobs.AddXPToJob(WizardJobId, 301);

            Assert.IsTrue(added);
            Assert.AreEqual(200, progression.GetAvailableXP());
            Assert.AreEqual(3, progression.Jobs.GetJobLevel(WizardJobId));
        }

        [Test]
        public void AddXPToJob_WhenMaxLevelReached_BlocksFurtherInvestment()
        {
            progression.AddExperience(500);
            progression.Jobs.UnlockJob(wizardDefinition);

            bool reachedMax = progression.Jobs.AddXPToJob(WizardJobId, 300);
            bool addedAfterMax = progression.Jobs.AddXPToJob(WizardJobId, 1);

            Assert.IsTrue(reachedMax);
            Assert.IsFalse(addedAfterMax);
            Assert.AreEqual(3, progression.Jobs.GetJobLevel(WizardJobId));
            Assert.AreEqual(200, progression.GetAvailableXP());
        }

        [Test]
        public void TryUnlockPerk_WhenPerkIsAvailable_StoresUnlockedPerkOnJob()
        {
            PerkDefinition perk = CreatePerkDefinition("ArcaneStart", isStartingPerk: true);
            AddPerksToJobDefinition(wizardDefinition, perk);
            progression.Jobs.UnlockJob(wizardDefinition);
            JobInstance job = progression.Jobs.GetJob(WizardJobId);
            job.AddExperience(job.GetXPToNextLevel());

            bool unlocked = progression.TryUnlockPerk(job, perk);

            Assert.IsTrue(unlocked);
            CollectionAssert.Contains(job.UnlockedPerkIds, perk.PerkId);
            Assert.AreEqual(0, job.JobPoints);
            Object.DestroyImmediate(perk);
        }

        private static JobDefinition CreateJobDefinition(string jobId, int maxLevel, int baseXP, float xpGrowthRate)
        {
            JobDefinition definition = ScriptableObject.CreateInstance<JobDefinition>();
            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("jobId").stringValue = jobId;
            serializedDefinition.FindProperty("displayName").stringValue = jobId;
            serializedDefinition.FindProperty("description").stringValue = "Test job";
            serializedDefinition.FindProperty("tier").enumValueIndex = (int)JobTier.Basic;
            serializedDefinition.FindProperty("maxLevel").intValue = maxLevel;
            serializedDefinition.FindProperty("baseXP").intValue = baseXP;
            serializedDefinition.FindProperty("xpGrowthRate").floatValue = xpGrowthRate;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }

        private static PerkDefinition CreatePerkDefinition(string perkId, bool isStartingPerk)
        {
            PerkDefinition definition = ScriptableObject.CreateInstance<PerkDefinition>();
            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("perkId").stringValue = perkId;
            serializedDefinition.FindProperty("displayName").stringValue = perkId;
            serializedDefinition.FindProperty("description").stringValue = "Test perk";
            serializedDefinition.FindProperty("cost").intValue = 1;
            serializedDefinition.FindProperty("isStartingPerk").boolValue = isStartingPerk;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }

        private static void AddPerksToJobDefinition(JobDefinition definition, params PerkDefinition[] perks)
        {
            SerializedObject serializedDefinition = new SerializedObject(definition);
            SerializedProperty jobPerks = serializedDefinition.FindProperty("jobPerks");
            jobPerks.arraySize = perks.Length;

            for (int i = 0; i < perks.Length; i++)
            {
                jobPerks.GetArrayElementAtIndex(i).objectReferenceValue = perks[i];
            }

            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
