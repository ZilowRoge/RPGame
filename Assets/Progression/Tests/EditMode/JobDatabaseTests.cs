using NUnit.Framework;
using RPGame.Progression;
using UnityEditor;
using UnityEngine;

namespace RPGame.Progression.Tests
{
    public sealed class JobDatabaseTests
    {
        private JobDatabase database;
        private JobDefinition wizard;
        private JobDefinition warrior;

        [SetUp]
        public void SetUp()
        {
            database = ScriptableObject.CreateInstance<JobDatabase>();
            wizard = CreateJobDefinition("Wizard", "Wizard");
            warrior = CreateJobDefinition("Warrior", "Warrior");
            SetDatabaseJobs(database, wizard, warrior);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(database);
            Object.DestroyImmediate(wizard);
            Object.DestroyImmediate(warrior);
        }

        [Test]
        public void TryGetJob_WhenIdExists_ReturnsJob()
        {
            bool found = database.TryGetJob("Wizard", out JobDefinition job);

            Assert.IsTrue(found);
            Assert.AreSame(wizard, job);
        }

        [Test]
        public void TryGetJob_WhenIdDoesNotExist_ReturnsFalse()
        {
            bool found = database.TryGetJob("missing", out JobDefinition job);

            Assert.IsFalse(found);
            Assert.IsNull(job);
        }

        [Test]
        public void Contains_WhenIdExists_ReturnsTrue()
        {
            Assert.IsTrue(database.Contains("Warrior"));
        }

        [Test]
        public void GetJob_WhenIdExists_ReturnsJob()
        {
            Assert.AreSame(warrior, database.GetJob("Warrior"));
        }

        private static JobDefinition CreateJobDefinition(string jobId, string displayName)
        {
            JobDefinition definition = ScriptableObject.CreateInstance<JobDefinition>();
            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("jobId").stringValue = jobId;
            serializedDefinition.FindProperty("displayName").stringValue = displayName;
            serializedDefinition.FindProperty("description").stringValue = "Test job";
            serializedDefinition.FindProperty("tier").enumValueIndex = (int)JobTier.Basic;
            serializedDefinition.FindProperty("maxLevel").intValue = 3;
            serializedDefinition.FindProperty("baseXP").intValue = 50;
            serializedDefinition.FindProperty("xpGrowthRate").floatValue = 2f;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }

        private static void SetDatabaseJobs(JobDatabase jobDatabase, params JobDefinition[] jobs)
        {
            SerializedObject serializedDatabase = new SerializedObject(jobDatabase);
            SerializedProperty jobList = serializedDatabase.FindProperty("jobs");
            jobList.arraySize = jobs.Length;

            for (int i = 0; i < jobs.Length; i++)
            {
                jobList.GetArrayElementAtIndex(i).objectReferenceValue = jobs[i];
            }

            serializedDatabase.ApplyModifiedPropertiesWithoutUndo();
            jobDatabase.Rebuild();
        }
    }
}
