using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Encounter
{
    public sealed class SpawnManager : MonoBehaviour
    {
        [SerializeField] private List<SpawnPoint> spawnPoints = new();
        [SerializeField] private EnemyPool enemyPool;
        [SerializeField] private float spawnDelay;

        private readonly HashSet<SpawnPoint> activeSpawnPoints = new();
        private int activeOperationId;
        private bool isSpawning;

        public bool AllEnemiesSpawned { get; private set; } = true;
        public bool IsSpawning => isSpawning;

        public void StartSpawning(WaveData waveData, Action<EnemyDefinition> onEnemyDied = null)
        {
            if (waveData == null)
            {
                throw new ArgumentNullException(nameof(waveData));
            }

            if (isSpawning)
            {
                throw new InvalidOperationException("Cannot start spawning while another spawn operation is active.");
            }

            List<EnemyDefinition> validEnemies = GetValidEnemies(waveData.Enemies);
            if (validEnemies.Count == 0)
            {
                AllEnemiesSpawned = true;
                return;
            }

            List<SpawnPoint> validSpawnPoints = GetValidSpawnPoints();
            if (validSpawnPoints.Count == 0)
            {
                throw new InvalidOperationException("Cannot spawn a non-empty wave without valid spawn points.");
            }

            if (enemyPool == null)
            {
                throw new InvalidOperationException("Cannot spawn a non-empty wave without an enemy pool.");
            }

            Dictionary<SpawnPoint, List<EnemyDefinition>> assignments = AssignEnemiesToSpawnPoints(
                validEnemies,
                validSpawnPoints,
                waveData.Seed);

            List<KeyValuePair<SpawnPoint, List<EnemyDefinition>>> usedAssignments = new();
            foreach (KeyValuePair<SpawnPoint, List<EnemyDefinition>> assignment in assignments)
            {
                if (assignment.Value.Count > 0)
                {
                    usedAssignments.Add(assignment);
                }
            }

            activeOperationId++;
            isSpawning = true;
            AllEnemiesSpawned = false;
            activeSpawnPoints.Clear();

            for (int i = 0; i < usedAssignments.Count; i++)
            {
                activeSpawnPoints.Add(usedAssignments[i].Key);
            }

            for (int i = 0; i < usedAssignments.Count; i++)
            {
                KeyValuePair<SpawnPoint, List<EnemyDefinition>> assignment = usedAssignments[i];
                assignment.Key.StartSpawnQueue(
                    assignment.Value,
                    Mathf.Max(0f, spawnDelay),
                    enemyPool,
                    onEnemyDied,
                    this,
                    activeOperationId);
            }

            if (activeSpawnPoints.Count == 0)
            {
                FinishSpawning();
            }
        }

        public void CancelSpawning()
        {
            if (!isSpawning)
            {
                return;
            }

            List<SpawnPoint> pointsToCancel = new(activeSpawnPoints);
            for (int i = 0; i < pointsToCancel.Count; i++)
            {
                pointsToCancel[i].CancelSpawning();
            }

            activeSpawnPoints.Clear();
            isSpawning = false;
            activeOperationId++;
        }

        internal void NotifySpawnPointFinished(SpawnPoint spawnPoint, int operationId)
        {
            if (!isSpawning || operationId != activeOperationId)
            {
                return;
            }

            activeSpawnPoints.Remove(spawnPoint);
            if (activeSpawnPoints.Count == 0)
            {
                FinishSpawning();
            }
        }

        internal static Dictionary<SpawnPoint, List<EnemyDefinition>> AssignEnemiesToSpawnPoints(
            IReadOnlyList<EnemyDefinition> enemies,
            IReadOnlyList<SpawnPoint> spawnPoints,
            int seed)
        {
            Dictionary<SpawnPoint, List<EnemyDefinition>> assignments = new();
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                assignments[spawnPoints[i]] = new List<EnemyDefinition>();
            }

            System.Random random = new(seed);
            for (int i = 0; i < enemies.Count; i++)
            {
                SpawnPoint spawnPoint = spawnPoints[random.Next(spawnPoints.Count)];
                assignments[spawnPoint].Add(enemies[i]);
            }

            return assignments;
        }

        private void FinishSpawning()
        {
            activeSpawnPoints.Clear();
            isSpawning = false;
            AllEnemiesSpawned = true;
        }

        private List<EnemyDefinition> GetValidEnemies(IReadOnlyList<EnemyDefinition> enemies)
        {
            List<EnemyDefinition> validEnemies = new();
            if (enemies == null)
            {
                return validEnemies;
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyDefinition enemy = enemies[i];
                if (enemy != null && enemy.Prefab != null)
                {
                    validEnemies.Add(enemy);
                }
            }

            return validEnemies;
        }

        private List<SpawnPoint> GetValidSpawnPoints()
        {
            List<SpawnPoint> validSpawnPoints = new();
            if (spawnPoints == null)
            {
                return validSpawnPoints;
            }

            for (int i = 0; i < spawnPoints.Count; i++)
            {
                SpawnPoint spawnPoint = spawnPoints[i];
                if (spawnPoint != null)
                {
                    validSpawnPoints.Add(spawnPoint);
                }
            }

            return validSpawnPoints;
        }
    }
}
