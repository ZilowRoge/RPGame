using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPGame.Encounter.Tests
{
    public sealed class WaveGeneratorTests
    {
        private readonly List<ScriptableObject> createdAssets = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < createdAssets.Count; i++)
            {
                Object.DestroyImmediate(createdAssets[i]);
            }

            createdAssets.Clear();
        }

        [Test]
        public void GenerateWave_WhenPerformanceBonusIsProvided_AddsItToEffectiveWave()
        {
            WaveGenerator generator = CreateGenerator(CreateScalingConfig(AnimationCurve.Constant(0f, 10f, 3f)), Enemy(1, 1));

            WaveData wave = generator.GenerateWave(4, 1.5f, 123);

            Assert.AreEqual(5.5f, wave.EffectiveWave);
        }

        [Test]
        public void GenerateWave_WhenBudgetCurveEvaluates_UsesCurveValueForBudget()
        {
            WaveGenerator generator = CreateGenerator(CreateScalingConfig(AnimationCurve.Linear(1f, 2f, 3f, 6f)), Enemy(1, 1));

            WaveData wave = generator.GenerateWave(2, 0f, 123);

            Assert.AreEqual(4, wave.Budget);
        }

        [Test]
        public void GenerateWave_WhenBudgetCurveValueIsFractional_RoundsBudgetDown()
        {
            WaveGenerator generator = CreateGenerator(CreateScalingConfig(AnimationCurve.Constant(0f, 10f, 3.9f)), Enemy(1, 1));

            WaveData wave = generator.GenerateWave(1, 0f, 123);

            Assert.AreEqual(3, wave.Budget);
        }

        [Test]
        public void GenerateWave_WhenBudgetCurveValueIsBelowOne_ClampsBudgetToOne()
        {
            WaveGenerator generator = CreateGenerator(CreateScalingConfig(AnimationCurve.Constant(0f, 10f, 0.2f)), Enemy(1, 1));

            WaveData wave = generator.GenerateWave(1, 0f, 123);

            Assert.AreEqual(1, wave.Budget);
        }

        [Test]
        public void GenerateWave_WhenEnemyUnlockWaveIsAboveRealWave_ExcludesEnemy()
        {
            EnemyDefinition unlockedEnemy = Enemy(1, 1);
            EnemyDefinition lockedEnemy = Enemy(1, 3);
            WaveGenerator generator = CreateGenerator(
                CreateScalingConfig(AnimationCurve.Constant(0f, 10f, 5f)),
                unlockedEnemy,
                lockedEnemy);

            WaveData wave = generator.GenerateWave(2, 0f, 123);

            Assert.IsTrue(wave.Enemies.All(enemy => enemy == unlockedEnemy));
        }

        [Test]
        public void GenerateWave_WhenEffectiveWaveReachesUnlockButRealWaveDoesNot_ExcludesEnemy()
        {
            EnemyDefinition unlockedEnemy = Enemy(1, 1);
            EnemyDefinition lockedEnemy = Enemy(1, 3);
            WaveGenerator generator = CreateGenerator(
                CreateScalingConfig(AnimationCurve.Constant(0f, 10f, 5f)),
                unlockedEnemy,
                lockedEnemy);

            WaveData wave = generator.GenerateWave(2, 5f, 123);

            Assert.AreEqual(7f, wave.EffectiveWave);
            Assert.IsTrue(wave.Enemies.All(enemy => enemy == unlockedEnemy));
        }

        [Test]
        public void GenerateWave_WhenWaveIsGenerated_EnemyCostsSumExactlyToBudget()
        {
            WaveGenerator generator = CreateGenerator(
                CreateScalingConfig(AnimationCurve.Constant(0f, 10f, 7f)),
                Enemy(1, 1),
                Enemy(2, 1),
                Enemy(3, 1));

            WaveData wave = generator.GenerateWave(1, 0f, 123);

            Assert.AreEqual(wave.Budget, wave.Enemies.Sum(enemy => enemy.Cost));
        }

        [Test]
        public void GenerateWave_WhenDefinitionsContainNullOrInvalidCost_IgnoresInvalidEntries()
        {
            EnemyDefinition validEnemy = Enemy(1, 1);
            EnemyDefinition invalidEnemy = Enemy(0, 1);
            WaveGenerator generator = CreateGenerator(
                CreateScalingConfig(AnimationCurve.Constant(0f, 10f, 4f)),
                null,
                invalidEnemy,
                validEnemy);

            WaveData wave = generator.GenerateWave(1, 0f, 123);

            Assert.IsTrue(wave.Enemies.All(enemy => enemy == validEnemy));
        }

        [Test]
        public void GenerateWave_WhenInputsAreSame_ProducesIdenticalCompositionOrder()
        {
            EnemyDefinition cheapEnemy = Enemy(1, 1);
            EnemyDefinition expensiveEnemy = Enemy(2, 1);
            WaveScalingConfig config = CreateScalingConfig(AnimationCurve.Constant(0f, 10f, 8f));
            WaveGenerator firstGenerator = CreateGenerator(config, cheapEnemy, expensiveEnemy);
            WaveGenerator secondGenerator = CreateGenerator(config, cheapEnemy, expensiveEnemy);

            WaveData firstWave = firstGenerator.GenerateWave(2, 0.5f, 999);
            WaveData secondWave = secondGenerator.GenerateWave(2, 0.5f, 999);

            CollectionAssert.AreEqual(firstWave.Enemies, secondWave.Enemies);
            Assert.AreEqual(firstWave.Seed, secondWave.Seed);
        }

        [Test]
        public void GenerateWave_WhenWaveNumbersDiffer_DerivesDifferentWaveSeeds()
        {
            WaveGenerator generator = CreateGenerator(CreateScalingConfig(AnimationCurve.Constant(0f, 10f, 3f)), Enemy(1, 1));

            WaveData firstWave = generator.GenerateWave(1, 0f, 123);
            WaveData secondWave = generator.GenerateWave(2, 0f, 123);

            Assert.AreNotEqual(firstWave.Seed, secondWave.Seed);
        }

        [Test]
        public void Constructor_WhenConfigIsMissing_ThrowsClearly()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new WaveGenerator(null, new[] { Enemy(1, 1) }));

            Assert.AreEqual("scalingConfig", exception.ParamName);
        }

        [Test]
        public void GenerateWave_WhenWaveNumberIsBelowOne_ThrowsClearly()
        {
            WaveGenerator generator = CreateGenerator(CreateScalingConfig(AnimationCurve.Constant(0f, 10f, 3f)), Enemy(1, 1));

            Assert.Throws<ArgumentOutOfRangeException>(() => generator.GenerateWave(0, 0f, 123));
        }

        [Test]
        public void GenerateWave_WhenNoUnlockedValidEnemies_ThrowsClearly()
        {
            WaveGenerator generator = CreateGenerator(
                CreateScalingConfig(AnimationCurve.Constant(0f, 10f, 3f)),
                Enemy(1, 2),
                Enemy(0, 1));

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => generator.GenerateWave(1, 0f, 123));

            StringAssert.Contains("unlocked valid enemies", exception.Message);
        }

        [Test]
        public void GenerateWave_WhenBudgetCannotBeSpentExactly_ThrowsClearly()
        {
            WaveGenerator generator = CreateGenerator(
                CreateScalingConfig(AnimationCurve.Constant(0f, 10f, 3f)),
                Enemy(2, 1));

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => generator.GenerateWave(1, 0f, 123));

            StringAssert.Contains("remaining wave budget", exception.Message);
        }

        private WaveGenerator CreateGenerator(WaveScalingConfig config, params EnemyDefinition[] enemies)
        {
            return new WaveGenerator(config, enemies);
        }

        private WaveScalingConfig CreateScalingConfig(AnimationCurve budgetCurve)
        {
            WaveScalingConfig config = ScriptableObject.CreateInstance<WaveScalingConfig>();
            createdAssets.Add(config);
            SetPrivateField(config, "budgetCurve", budgetCurve);
            return config;
        }

        private EnemyDefinition Enemy(int cost, int unlockWave)
        {
            EnemyDefinition enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            createdAssets.Add(enemy);
            SetPrivateField(enemy, "cost", cost);
            SetPrivateField(enemy, "unlockWave", unlockWave);
            return enemy;
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(target, value);
        }
    }
}
