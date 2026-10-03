using System.Reflection;
using NUnit.Framework;
using RPGame.Encounter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RPGame.UI.Encounter.Tests
{
    public sealed class ScoreMultiplierTimerTests
    {
        private GameObject runtimeObject;
        private GameObject uiObject;
        private GameObject textObject;
        private GameObject imageObject;

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(imageObject);
            Object.DestroyImmediate(textObject);
            Object.DestroyImmediate(uiObject);
            Object.DestroyImmediate(runtimeObject);
        }

        [Test]
        public void Update_ReflectsMultiplierAndNormalizedComboTime()
        {
            ScoreSystem scoreSystem = new();
            scoreSystem.RegisterKill(10);
            scoreSystem.Tick(2.5f);
            EncounterRuntime runtime = CreateRuntime(scoreSystem);
            ScoreMultiplierTimer timer = CreateTimer(
                runtime,
                out TMP_Text multiplierText,
                out Image timerImage,
                out CanvasGroup canvasGroup);

            InvokeUpdate(timer);

            Assert.That(multiplierText.text, Is.EqualTo("x1.1"));
            Assert.That(timerImage.fillAmount, Is.EqualTo(0.5f));
            Assert.That(canvasGroup.alpha, Is.EqualTo(1f));
        }

        [Test]
        public void ComboTimeNormalized_AfterComboExpires_IsZero()
        {
            ScoreSystem scoreSystem = new();
            scoreSystem.RegisterKill(10);
            scoreSystem.Tick(5f);

            Assert.That(scoreSystem.ComboTimeNormalized, Is.Zero);
        }

        [Test]
        public void Update_AfterComboExpires_HidesTimerUI()
        {
            ScoreSystem scoreSystem = new();
            scoreSystem.RegisterKill(10);
            scoreSystem.Tick(5f);
            EncounterRuntime runtime = CreateRuntime(scoreSystem);
            ScoreMultiplierTimer timer = CreateTimer(runtime, out _, out _, out CanvasGroup canvasGroup);

            InvokeUpdate(timer);

            Assert.That(canvasGroup.alpha, Is.Zero);
        }

        [Test]
        public void Update_WhenRuntimeIsUninitialized_DoesNotThrow()
        {
            runtimeObject = new GameObject("EncounterRuntime");
            EncounterRuntime runtime = runtimeObject.AddComponent<EncounterRuntime>();
            ScoreMultiplierTimer timer = CreateTimer(runtime, out _, out _, out _);

            Assert.DoesNotThrow(() => InvokeUpdate(timer));
        }

        private EncounterRuntime CreateRuntime(ScoreSystem scoreSystem)
        {
            runtimeObject = new GameObject("EncounterRuntime");
            EncounterRuntime runtime = runtimeObject.AddComponent<EncounterRuntime>();
            typeof(EncounterRuntime).GetField("scoreSystem", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(runtime, scoreSystem);
            return runtime;
        }

        private ScoreMultiplierTimer CreateTimer(
            EncounterRuntime runtime,
            out TMP_Text multiplierText,
            out Image timerImage,
            out CanvasGroup canvasGroup)
        {
            uiObject = new GameObject("ScoreMultiplierTimer");
            ScoreMultiplierTimer timer = uiObject.AddComponent<ScoreMultiplierTimer>();
            canvasGroup = uiObject.AddComponent<CanvasGroup>();
            textObject = new GameObject("MultiplierText");
            multiplierText = textObject.AddComponent<TextMeshPro>();
            imageObject = new GameObject("TimerImage");
            timerImage = imageObject.AddComponent<Image>();
            SetPrivateField(timer, "runtime", runtime);
            SetPrivateField(timer, "multiplierText", multiplierText);
            SetPrivateField(timer, "timerImage", timerImage);
            SetPrivateField(timer, "canvasGroup", canvasGroup);
            return timer;
        }

        private static void InvokeUpdate(ScoreMultiplierTimer timer)
        {
            typeof(ScoreMultiplierTimer).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(timer, null);
        }

        private static void SetPrivateField<T>(ScoreMultiplierTimer timer, string fieldName, T value)
        {
            typeof(ScoreMultiplierTimer).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(timer, value);
        }
    }
}
