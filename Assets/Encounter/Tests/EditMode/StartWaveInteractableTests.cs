using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace RPGame.Encounter.Tests.EditMode
{
    public sealed class StartWaveInteractableTests
    {
        private const float IntermissionDuration = 1f;

        private GameObject interactableObject;
        private GameObject waveControllerObject;
        private StartWaveInteractable interactable;
        private StartWaveTestWaveController waveController;
        private WaveScalingConfig waveScalingConfig;
        private EnemyDefinition enemyDefinition;
        private EncounterController encounterController;

        [SetUp]
        public void SetUp()
        {
            interactableObject = new GameObject("StartWaveInteractable");
            interactable = interactableObject.AddComponent<StartWaveInteractable>();
            waveControllerObject = new GameObject("StartWaveTestWaveController");
            waveController = waveControllerObject.AddComponent<StartWaveTestWaveController>();
            waveScalingConfig = ScriptableObject.CreateInstance<WaveScalingConfig>();
            enemyDefinition = ScriptableObject.CreateInstance<EnemyDefinition>();
            ConfigureWaveDefinitions();

            encounterController = new EncounterController(
                waveController,
                new WaveGenerator(waveScalingConfig, new[] { enemyDefinition }),
                new ScoreSystem(null, waveScalingConfig),
                IntermissionDuration);
            interactable.Initialize(encounterController);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(interactableObject);
            UnityEngine.Object.DestroyImmediate(waveControllerObject);
            UnityEngine.Object.DestroyImmediate(waveScalingConfig);
            UnityEngine.Object.DestroyImmediate(enemyDefinition);
        }

        [Test]
        public void Interact_DuringIntermission_StartsPendingWave()
        {
            encounterController.StartEncounter(1234);

            interactable.Interact(default);

            Assert.That(encounterController.State, Is.EqualTo(EncounterState.WaveActive));
            Assert.That(waveController.StartCount, Is.EqualTo(1));
        }

        [Test]
        public void Interact_OutsideIntermission_DoesNothing()
        {
            interactable.Interact(default);

            Assert.That(encounterController.State, Is.EqualTo(EncounterState.Idle));
            Assert.That(waveController.StartCount, Is.Zero);
        }

        [Test]
        public void Interact_AfterWaveStarts_DoesNothing()
        {
            encounterController.StartEncounter(1234);
            interactable.Interact(default);

            interactable.Interact(default);

            Assert.That(encounterController.State, Is.EqualTo(EncounterState.WaveActive));
            Assert.That(waveController.StartCount, Is.EqualTo(1));
        }

        [Test]
        public void Interact_WithoutEncounterController_DoesNothing()
        {
            interactable.Initialize(null);

            Assert.DoesNotThrow(() => interactable.Interact(default));
        }

        private void ConfigureWaveDefinitions()
        {
            SerializedObject enemyDefinitionObject = new(enemyDefinition);
            enemyDefinitionObject.FindProperty("cost").intValue = 1;
            enemyDefinitionObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject scalingConfigObject = new(waveScalingConfig);
            scalingConfigObject.FindProperty("budgetCurve").animationCurveValue =
                AnimationCurve.Constant(1f, 10f, enemyDefinition.Cost);
            scalingConfigObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    public sealed class StartWaveTestWaveController : EncounterWaveController
    {
        public int StartCount { get; private set; }

        public override void StartWave(
            WaveData waveData,
            Action<WaveData> onCompleted,
            Action<EnemyDefinition> onEnemyDied = null)
        {
            StartCount++;
        }

        public override void CancelWave()
        {
        }
    }
}
