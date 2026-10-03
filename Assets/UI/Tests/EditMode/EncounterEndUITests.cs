using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RPGame.Encounter;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RPGame.UI.Encounter.Tests
{
    public sealed class EncounterEndUITests
    {
        private readonly List<Object> createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < createdObjects.Count; i++)
            {
                if (createdObjects[i] != null)
                {
                    Object.DestroyImmediate(createdObjects[i]);
                }
            }
        }

        [Test]
        public void Update_BeforeEncounterEnds_HidesPanel()
        {
            EncounterEndUI endUI = CreateEndUI(out GameObject panel, out _, out _);

            InvokeUpdate(endUI);

            Assert.That(panel.activeSelf, Is.False);
        }

        [Test]
        public void Update_AfterEncounterEnds_ShowsPanelAndDisplaysResults()
        {
            EncounterController encounterController = CreateEndedController(out ScoreSystem scoreSystem);
            scoreSystem.RegisterKill(10);
            EncounterEndUI endUI = CreateEndUI(out GameObject panel, out TMP_Text scoreText, out TMP_Text waveText);
            SetRuntimeState(endUI, encounterController, scoreSystem);

            InvokeUpdate(endUI);

            Assert.That(panel.activeSelf, Is.True);
            Assert.That(scoreText.text, Is.EqualTo("Final Score: 10"));
            Assert.That(waveText.text, Is.EqualTo("Waves Completed: 0"));
        }

        [Test]
        public void Update_WhenRepeatedAfterEncounterEnds_KeepsDisplayedResults()
        {
            EncounterController encounterController = CreateEndedController(out ScoreSystem scoreSystem);
            EncounterEndUI endUI = CreateEndUI(out _, out TMP_Text scoreText, out _);
            SetRuntimeState(endUI, encounterController, scoreSystem);

            InvokeUpdate(endUI);
            scoreSystem.RegisterKill(10);
            InvokeUpdate(endUI);

            Assert.That(scoreText.text, Is.EqualTo("Final Score: 0"));
        }

        [Test]
        public void Update_WithMissingReferences_DoesNotThrow()
        {
            EncounterEndUI endUI = CreateObject("EncounterEndUI").AddComponent<EncounterEndUI>();

            Assert.DoesNotThrow(() => InvokeUpdate(endUI));
        }

        [Test]
        public void RestartCurrentScene_LoadsTheActiveScene()
        {
            EncounterEndUI endUI = CreateObject("EncounterEndUI").AddComponent<EncounterEndUI>();
            int loadedSceneBuildIndex = -1;
            SetPrivateField(endUI, "loadScene", new System.Action<int>(sceneBuildIndex => loadedSceneBuildIndex = sceneBuildIndex));

            endUI.RestartCurrentScene();

            Assert.That(loadedSceneBuildIndex, Is.EqualTo(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex));
        }

        private EncounterController CreateEndedController(out ScoreSystem scoreSystem)
        {
            GameObject waveControllerObject = CreateObject("EncounterWaveController");
            EncounterWaveController waveController = waveControllerObject.AddComponent<EncounterWaveController>();
            WaveScalingConfig scalingConfig = CreateAsset<WaveScalingConfig>();
            EnemyDefinition definition = CreateAsset<EnemyDefinition>();
            ConfigureWaveData(scalingConfig, definition);
            scoreSystem = new ScoreSystem();
            EncounterController encounterController = new(
                waveController,
                new WaveGenerator(scalingConfig, new[] { definition }),
                scoreSystem);
            encounterController.StartEncounter(1234);
            encounterController.EndEncounter();
            return encounterController;
        }

        private EncounterEndUI CreateEndUI(
            out GameObject panel,
            out TMP_Text scoreText,
            out TMP_Text waveText)
        {
            EncounterEndUI endUI = CreateObject("EncounterEndUI").AddComponent<EncounterEndUI>();
            panel = CreateObject("EndPanel");
            scoreText = CreateObject("FinalScoreText").AddComponent<TextMeshPro>();
            waveText = CreateObject("ReachedWaveText").AddComponent<TextMeshPro>();
            Button restartButton = CreateObject("RestartButton").AddComponent<Button>();
            SetPrivateField(endUI, "panel", panel);
            SetPrivateField(endUI, "finalScoreText", scoreText);
            SetPrivateField(endUI, "reachedWaveText", waveText);
            SetPrivateField(endUI, "restartButton", restartButton);
            return endUI;
        }

        private void SetRuntimeState(
            EncounterEndUI endUI,
            EncounterController encounterController,
            ScoreSystem scoreSystem)
        {
            EncounterRuntime runtime = CreateObject("EncounterRuntime").AddComponent<EncounterRuntime>();
            SetPrivateField(runtime, "encounterController", encounterController);
            SetPrivateField(runtime, "scoreSystem", scoreSystem);
            SetPrivateField(endUI, "runtime", runtime);
        }

        private void ConfigureWaveData(WaveScalingConfig scalingConfig, EnemyDefinition definition)
        {
            SerializedObject definitionObject = new(definition);
            definitionObject.FindProperty("cost").intValue = 1;
            definitionObject.FindProperty("unlockWave").intValue = 1;
            definitionObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject scalingConfigObject = new(scalingConfig);
            scalingConfigObject.FindProperty("budgetGrowthCurve").animationCurveValue =
                AnimationCurve.Constant(1f, 10f, 0f);
            scalingConfigObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private GameObject CreateObject(string name)
        {
            GameObject gameObject = new(name);
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private T CreateAsset<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            createdObjects.Add(asset);
            return asset;
        }

        private static void InvokeUpdate(EncounterEndUI endUI)
        {
            typeof(EncounterEndUI).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(endUI, null);
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);
        }
    }
}
