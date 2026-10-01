using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Encounter
{
    public sealed class WaveGenerator
    {
        private readonly WaveScalingConfig scalingConfig;
        private readonly List<EnemyDefinition> enemyDefinitions;

        public WaveGenerator(WaveScalingConfig scalingConfig, IEnumerable<EnemyDefinition> enemyDefinitions)
        {
            this.scalingConfig = scalingConfig != null
                ? scalingConfig
                : throw new ArgumentNullException(nameof(scalingConfig));
            this.enemyDefinitions = new List<EnemyDefinition>(enemyDefinitions ?? Array.Empty<EnemyDefinition>());
        }

        public WaveData GenerateWave(int waveNumber, float performanceBonus, int encounterSeed)
        {
            if (waveNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(waveNumber), "Wave number must be at least 1.");
            }

            float effectiveWave = waveNumber + performanceBonus;
            int budget = CalculateBudget(effectiveWave);
            List<EnemyDefinition> unlockedEnemies = GetUnlockedEnemies(waveNumber);
            if (unlockedEnemies.Count == 0)
            {
                throw new InvalidOperationException("Cannot generate wave without unlocked valid enemies.");
            }

            int waveSeed = DeriveWaveSeed(encounterSeed, waveNumber);
            List<EnemyDefinition> enemies = GenerateEnemies(budget, unlockedEnemies, waveSeed);
            return new WaveData(waveNumber, effectiveWave, budget, enemies, waveSeed);
        }

        private int CalculateBudget(float effectiveWave)
        {
            if (scalingConfig.BudgetCurve == null)
            {
                throw new InvalidOperationException("Wave scaling config is missing a budget curve.");
            }

            int budget = Mathf.FloorToInt(scalingConfig.BudgetCurve.Evaluate(effectiveWave));
            return Mathf.Max(1, budget);
        }

        private List<EnemyDefinition> GetUnlockedEnemies(int waveNumber)
        {
            List<EnemyDefinition> unlockedEnemies = new();
            for (int i = 0; i < enemyDefinitions.Count; i++)
            {
                EnemyDefinition definition = enemyDefinitions[i];
                if (definition != null && definition.Cost > 0 && definition.UnlockWave <= waveNumber)
                {
                    unlockedEnemies.Add(definition);
                }
            }

            return unlockedEnemies;
        }

        private static List<EnemyDefinition> GenerateEnemies(
            int budget,
            IReadOnlyList<EnemyDefinition> unlockedEnemies,
            int waveSeed)
        {
            System.Random random = new(waveSeed);
            List<EnemyDefinition> selectedEnemies = new();
            int remainingBudget = budget;

            while (remainingBudget > 0)
            {
                List<EnemyDefinition> affordableEnemies = GetAffordableEnemies(unlockedEnemies, remainingBudget);
                if (affordableEnemies.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"Cannot spend remaining wave budget exactly. Remaining budget: {remainingBudget}.");
                }

                EnemyDefinition selectedEnemy = affordableEnemies[random.Next(affordableEnemies.Count)];
                selectedEnemies.Add(selectedEnemy);
                remainingBudget -= selectedEnemy.Cost;
            }

            return selectedEnemies;
        }

        private static List<EnemyDefinition> GetAffordableEnemies(
            IReadOnlyList<EnemyDefinition> unlockedEnemies,
            int remainingBudget)
        {
            List<EnemyDefinition> affordableEnemies = new();
            for (int i = 0; i < unlockedEnemies.Count; i++)
            {
                EnemyDefinition definition = unlockedEnemies[i];
                if (definition.Cost <= remainingBudget)
                {
                    affordableEnemies.Add(definition);
                }
            }

            return affordableEnemies;
        }

        private static int DeriveWaveSeed(int encounterSeed, int waveNumber)
        {
            unchecked
            {
                return (encounterSeed * 397) ^ waveNumber;
            }
        }
    }
}
