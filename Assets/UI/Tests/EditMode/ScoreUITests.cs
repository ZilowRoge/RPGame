using System.Reflection;
using NUnit.Framework;
using RPGame.Encounter;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPGame.UI.Encounter.Tests
{
    public sealed class ScoreUITests
    {
        private GameObject runtimeObject;
        private GameObject uiObject;
        private GameObject textObject;

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(textObject);
            Object.DestroyImmediate(uiObject);
            Object.DestroyImmediate(runtimeObject);
        }

        [Test]
        public void Update_ReflectsCurrentScore()
        {
            EncounterRuntime runtime = CreateRuntime(new ScoreSystem());
            runtime.ScoreSystem.RegisterKill(10);
            ScoreUI scoreUI = CreateScoreUI(runtime, out TMP_Text scoreText);

            InvokeUpdate(scoreUI);

            Assert.That(scoreText.text, Is.EqualTo("Score: 10"));
        }

        [Test]
        public void Update_FormatsScoreAsWholePoints()
        {
            ScoreSystem scoreSystem = new();
            scoreSystem.RegisterKill(1);
            scoreSystem.RegisterKill(1);
            EncounterRuntime runtime = CreateRuntime(scoreSystem);
            ScoreUI scoreUI = CreateScoreUI(runtime, out TMP_Text scoreText);

            InvokeUpdate(scoreUI);

            Assert.That(scoreText.text, Is.EqualTo("Score: 2"));
        }

        [Test]
        public void Update_WhenRuntimeIsUninitialized_DoesNotThrow()
        {
            runtimeObject = new GameObject("EncounterRuntime");
            EncounterRuntime runtime = runtimeObject.AddComponent<EncounterRuntime>();
            ScoreUI scoreUI = CreateScoreUI(runtime, out _);

            Assert.DoesNotThrow(() => InvokeUpdate(scoreUI));
        }

        private EncounterRuntime CreateRuntime(ScoreSystem scoreSystem)
        {
            runtimeObject = new GameObject("EncounterRuntime");
            EncounterRuntime runtime = runtimeObject.AddComponent<EncounterRuntime>();
            typeof(EncounterRuntime).GetField("scoreSystem", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(runtime, scoreSystem);
            return runtime;
        }

        private ScoreUI CreateScoreUI(EncounterRuntime runtime, out TMP_Text scoreText)
        {
            uiObject = new GameObject("ScoreUI");
            ScoreUI scoreUI = uiObject.AddComponent<ScoreUI>();
            textObject = new GameObject("ScoreText");
            scoreText = textObject.AddComponent<TextMeshPro>();
            SetPrivateField(scoreUI, "runtime", runtime);
            SetPrivateField(scoreUI, "scoreText", scoreText);
            return scoreUI;
        }

        private static void InvokeUpdate(ScoreUI scoreUI)
        {
            typeof(ScoreUI).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(scoreUI, null);
        }

        private static void SetPrivateField<T>(ScoreUI scoreUI, string fieldName, T value)
        {
            typeof(ScoreUI).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(scoreUI, value);
        }
    }
}
