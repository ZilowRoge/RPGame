using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace RPGame.Encounter.Tests.EditMode
{
    public sealed class EncounterControllerTests
    {
        private const float IntermissionDuration = 1f;
        private const float PerformanceBonus = 0.5f;

        private GameObject waveControllerObject;
        private EncounterWaveControllerSpy waveController;
        private WaveScalingConfig waveScalingConfig;
        private EnemyDefinition enemyDefinition;
        private WaveGenerator waveGenerator;
        private ScoreSystem scoreSystem;
        private EncounterController encounterController;

        [SetUp]
        public void SetUp()
        {
            waveControllerObject = new GameObject("EncounterWaveControllerSpy");
            waveController = waveControllerObject.AddComponent<EncounterWaveControllerSpy>();
            waveScalingConfig = ScriptableObject.CreateInstance<WaveScalingConfig>();
            enemyDefinition = ScriptableObject.CreateInstance<EnemyDefinition>();
            ConfigureDefinitions();

            waveGenerator = new WaveGenerator(waveScalingConfig, new[] { enemyDefinition });
            scoreSystem = new ScoreSystem(null, waveScalingConfig);
            waveController.ObservedScoreSystem = scoreSystem;
            encounterController = new EncounterController(
                waveController,
                waveGenerator,
                scoreSystem,
                IntermissionDuration);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(waveControllerObject);
            UnityEngine.Object.DestroyImmediate(waveScalingConfig);
            UnityEngine.Object.DestroyImmediate(enemyDefinition);
        }

        [Test]
        public void StartEncounter_ResetsRunAndEntersIntermission()
        {
            scoreSystem.RegisterKill(enemyDefinition.Cost);

            encounterController.StartEncounter(1234);

            Assert.That(encounterController.State, Is.EqualTo(EncounterState.Intermission));
            Assert.That(encounterController.CurrentWaveNumber, Is.EqualTo(1));
            Assert.That(encounterController.EncounterSeed, Is.EqualTo(1234));
            Assert.That(encounterController.IntermissionTimeRemaining, Is.EqualTo(IntermissionDuration));
            Assert.That(scoreSystem.CurrentScore, Is.Zero);
        }

        [Test]
        public void StartEncounter_FirstWaveUsesZeroPerformanceBonus()
        {
            encounterController.StartEncounter(1234);
            encounterController.StartPendingWaveNow();

            Assert.That(waveController.LastStartedWave.EffectiveWave, Is.EqualTo(1f));
        }

        [Test]
        public void Tick_WhenIntermissionExpires_StartsPendingWave()
        {
            encounterController.StartEncounter(1234);

            encounterController.Tick(IntermissionDuration);

            Assert.That(encounterController.State, Is.EqualTo(EncounterState.WaveActive));
            Assert.That(waveController.LastStartedWave, Is.Not.Null);
        }

        [Test]
        public void StartPendingWaveNow_StartsWaveEarly()
        {
            encounterController.StartEncounter(1234);

            encounterController.StartPendingWaveNow();

            Assert.That(encounterController.State, Is.EqualTo(EncounterState.WaveActive));
            Assert.That(encounterController.IntermissionTimeRemaining, Is.Zero);
        }

        [Test]
        public void StartPendingWaveNow_BeginsScoringBeforeStartingWave()
        {
            encounterController.StartEncounter(1234);
            scoreSystem.RegisterKill(enemyDefinition.Cost);

            encounterController.StartPendingWaveNow();

            Assert.That(waveController.WaveBaseScoreWhenStarted, Is.Zero);
        }

        [Test]
        public void EnemyDeath_RegistersTheEnemyDefinitionCost()
        {
            encounterController.StartEncounter(1234);
            encounterController.StartPendingWaveNow();

            waveController.ReportEnemyDeath(enemyDefinition);

            Assert.That(scoreSystem.CurrentScore, Is.EqualTo(enemyDefinition.Cost));
        }

        [Test]
        public void CompletedWave_FinalizesScoreAndPerformance()
        {
            encounterController.StartEncounter(1234);
            encounterController.StartPendingWaveNow();
            waveController.ReportEnemyDeath(enemyDefinition);

            waveController.CompleteActiveWave();

            Assert.That(scoreSystem.FinalizedWaveBaseScore, Is.EqualTo(enemyDefinition.Cost));
            Assert.That(scoreSystem.FinalizedWaveActualScore, Is.EqualTo(enemyDefinition.Cost));
            Assert.That(scoreSystem.LastWavePerformanceBonus, Is.EqualTo(PerformanceBonus));
        }

        [Test]
        public void CompletedWave_IncrementsWaveNumberAndUsesPerformanceBonusForNextWave()
        {
            encounterController.StartEncounter(1234);
            encounterController.StartPendingWaveNow();
            waveController.ReportEnemyDeath(enemyDefinition);

            waveController.CompleteActiveWave();

            Assert.That(encounterController.CurrentWaveNumber, Is.EqualTo(2));
            Assert.That(encounterController.PendingWave.EffectiveWave, Is.EqualTo(2f + PerformanceBonus));
        }

        [Test]
        public void CompletedWave_ReturnsToIntermission()
        {
            encounterController.StartEncounter(1234);
            encounterController.StartPendingWaveNow();

            waveController.CompleteActiveWave();

            Assert.That(encounterController.State, Is.EqualTo(EncounterState.Intermission));
            Assert.That(encounterController.IntermissionTimeRemaining, Is.EqualTo(IntermissionDuration));
        }

        [Test]
        public void EndEncounter_CancelsActiveWaveAndEntersEnded()
        {
            encounterController.StartEncounter(1234);
            encounterController.StartPendingWaveNow();

            encounterController.EndEncounter();

            Assert.That(waveController.CancelCount, Is.EqualTo(1));
            Assert.That(encounterController.State, Is.EqualTo(EncounterState.Ended));
        }

        [Test]
        public void EndEncounter_IgnoresStaleWaveCompletion()
        {
            encounterController.StartEncounter(1234);
            encounterController.StartPendingWaveNow();
            encounterController.EndEncounter();

            waveController.CompleteActiveWave();

            Assert.That(encounterController.State, Is.EqualTo(EncounterState.Ended));
            Assert.That(encounterController.CurrentWaveNumber, Is.EqualTo(1));
        }

        [Test]
        public void StartEncounter_AfterEndStartsANewRun()
        {
            encounterController.StartEncounter(1234);
            encounterController.EndEncounter();

            encounterController.StartEncounter(5678);

            Assert.That(encounterController.State, Is.EqualTo(EncounterState.Intermission));
            Assert.That(encounterController.EncounterSeed, Is.EqualTo(5678));
            Assert.That(encounterController.CurrentWaveNumber, Is.EqualTo(1));
        }

        [Test]
        public void EndEncounter_PreservesScoreUntilTheNextRunStarts()
        {
            encounterController.StartEncounter(1234);
            encounterController.StartPendingWaveNow();
            waveController.ReportEnemyDeath(enemyDefinition);

            encounterController.EndEncounter();

            Assert.That(scoreSystem.CurrentScore, Is.EqualTo(enemyDefinition.Cost));

            encounterController.StartEncounter(5678);

            Assert.That(scoreSystem.CurrentScore, Is.Zero);
        }

        private void ConfigureDefinitions()
        {
            SerializedObject enemyDefinitionObject = new(enemyDefinition);
            enemyDefinitionObject.FindProperty("cost").intValue = 7;
            enemyDefinitionObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject scalingConfigObject = new(waveScalingConfig);
            scalingConfigObject.FindProperty("budgetCurve").animationCurveValue =
                AnimationCurve.Constant(1f, 10f, enemyDefinition.Cost);
            scalingConfigObject.FindProperty("performanceWaveBonusCurve").animationCurveValue =
                AnimationCurve.Constant(0f, 1f, PerformanceBonus);
            scalingConfigObject.FindProperty("maxPerformanceWaveBonus").floatValue = PerformanceBonus;
            scalingConfigObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    public sealed class EncounterWaveControllerSpy : EncounterWaveController
    {
        private Action<WaveData> completed;
        private Action<EnemyDefinition> enemyDied;

        public ScoreSystem ObservedScoreSystem { get; set; }
        public WaveData LastStartedWave { get; private set; }
        public float WaveBaseScoreWhenStarted { get; private set; }
        public int CancelCount { get; private set; }

        public override void StartWave(
            WaveData waveData,
            Action<WaveData> onCompleted,
            Action<EnemyDefinition> onEnemyDied = null)
        {
            LastStartedWave = waveData;
            completed = onCompleted;
            enemyDied = onEnemyDied;
            WaveBaseScoreWhenStarted = ObservedScoreSystem.CurrentWaveBaseScore;
        }

        public override void CancelWave()
        {
            CancelCount++;
        }

        public void ReportEnemyDeath(EnemyDefinition enemyDefinition)
        {
            enemyDied?.Invoke(enemyDefinition);
        }

        public void CompleteActiveWave()
        {
            completed?.Invoke(LastStartedWave);
        }
    }
}
