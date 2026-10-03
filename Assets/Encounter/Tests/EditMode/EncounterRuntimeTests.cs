using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RPGame.Core.Statistics;
using RPGame.Enemies;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPGame.Encounter.Tests.EditMode
{
    public sealed class EncounterRuntimeTests
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
        public void Start_ConstructsDependenciesStartsEncounterAndInitializesInteractable()
        {
            EncounterRuntime runtime = CreateRuntime(out StartWaveInteractable interactable, out _, out _);

            StartRuntime(runtime);

            Assert.That(runtime.EncounterController, Is.Not.Null);
            Assert.That(runtime.ScoreSystem, Is.Not.Null);
            Assert.That(runtime.EncounterController.State, Is.EqualTo(EncounterState.Intermission));
            Assert.That(interactable.CanInteract(default), Is.True);
        }

        [Test]
        public void PlayerDeath_EndsEncounterAndReleasesActiveEnemies()
        {
            EncounterRuntime runtime = CreateRuntime(out _, out EnemyPool pool, out StatisticsController playerStatistics);
            StartRuntime(runtime);
            EnemyDefinition definition = CreateEnemyDefinition();
            PooledEnemy activeEnemy = pool.Acquire(definition, Vector3.zero, Quaternion.identity);

            RaisePlayerDeath(playerStatistics);

            Assert.That(runtime.EncounterController.State, Is.EqualTo(EncounterState.Ended));
            Assert.That(activeEnemy.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void Destroy_UnsubscribesFromPlayerDeath()
        {
            EncounterRuntime runtime = CreateRuntime(out _, out EnemyPool pool, out StatisticsController playerStatistics);
            StartRuntime(runtime);
            EnemyDefinition definition = CreateEnemyDefinition();
            PooledEnemy activeEnemy = pool.Acquire(definition, Vector3.zero, Quaternion.identity);

            typeof(EncounterRuntime).GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(runtime, null);
            Object.DestroyImmediate(runtime.gameObject);
            RaisePlayerDeath(playerStatistics);

            Assert.That(activeEnemy.gameObject.activeSelf, Is.True);
        }

        private EncounterRuntime CreateRuntime(
            out StartWaveInteractable interactable,
            out EnemyPool pool,
            out StatisticsController playerStatistics)
        {
            GameObject runtimeObject = CreateObject("EncounterRuntime");
            EncounterRuntime runtime = runtimeObject.AddComponent<EncounterRuntime>();
            GameObject waveControllerObject = CreateObject("EncounterWaveController");
            EncounterWaveController waveController = waveControllerObject.AddComponent<EncounterWaveController>();
            pool = CreateObject("EnemyPool").AddComponent<EnemyPool>();
            interactable = CreateObject("StartWaveInteractable").AddComponent<StartWaveInteractable>();
            playerStatistics = CreateObject("PlayerStatistics").AddComponent<StatisticsController>();
            WaveScalingConfig scalingConfig = CreateAsset<WaveScalingConfig>();
            ScoreConfig scoreConfig = CreateAsset<ScoreConfig>();
            EnemyDefinition definition = CreateEnemyDefinition();
            ConfigureDefinitions(definition, scalingConfig);

            SetPrivateField(runtime, "waveController", waveController);
            SetPrivateField(runtime, "enemyPool", pool);
            SetPrivateField(runtime, "waveScalingConfig", scalingConfig);
            SetPrivateField(runtime, "scoreConfig", scoreConfig);
            SetPrivateField(runtime, "enemyDefinitions", new List<EnemyDefinition> { definition });
            SetPrivateField(runtime, "playerStatistics", playerStatistics);
            SetPrivateField(runtime, "startWaveInteractable", interactable);
            SetPrivateField(runtime, "intermissionDuration", 1f);
            return runtime;
        }

        private EnemyDefinition CreateEnemyDefinition()
        {
            GameObject prefab = CreateObject("EnemyPrefab");
            prefab.AddComponent<PooledEnemy>();
            EnemyDefinition definition = CreateAsset<EnemyDefinition>();
            SetPrivateField(definition, "prefab", prefab);
            SetPrivateField(definition, "cost", 1);
            SetPrivateField(definition, "unlockWave", 1);
            return definition;
        }

        private void ConfigureDefinitions(EnemyDefinition definition, WaveScalingConfig scalingConfig)
        {
            SerializedObject scalingConfigObject = new(scalingConfig);
            scalingConfigObject.FindProperty("baseBudget").floatValue = definition.Cost;
            scalingConfigObject.FindProperty("budgetPerWave").floatValue = 0f;
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

        private static void StartRuntime(EncounterRuntime runtime)
        {
            typeof(EncounterRuntime).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(runtime, null);
        }

        private static void RaisePlayerDeath(StatisticsController playerStatistics)
        {
            FieldInfo diedField = typeof(StatisticsController).GetField(
                "Died",
                BindingFlags.Instance | BindingFlags.NonPublic);
            ((Action)diedField.GetValue(playerStatistics))?.Invoke();
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);
        }
    }
}
