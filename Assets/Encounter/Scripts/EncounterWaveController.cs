using System;
using UnityEngine;

namespace RPGame.Encounter
{
    public class EncounterWaveController : MonoBehaviour
    {
        [SerializeField] private SpawnManager spawnManager;

        private WaveData activeWave;
        private Action<WaveData> activeCompleted;
        private Action<EnemyDefinition> activeEnemyDied;
        private bool spawningFinished;
        private int activeWaveId;

        public bool IsWaveActive { get; private set; }
        public int RemainingEnemies { get; private set; }

        public virtual void StartWave(
            WaveData waveData,
            Action<WaveData> onCompleted,
            Action<EnemyDefinition> onEnemyDied = null)
        {
            if (waveData == null)
            {
                throw new ArgumentNullException(nameof(waveData));
            }

            if (spawnManager == null)
            {
                throw new InvalidOperationException("Cannot start an encounter wave without a spawn manager.");
            }

            if (IsWaveActive)
            {
                throw new InvalidOperationException("Cannot start an encounter wave while another wave is active.");
            }

            int waveId = ++activeWaveId;
            bool isStarting = true;
            bool spawningCompletedDuringStart = false;

            activeWave = waveData;
            activeCompleted = onCompleted;
            activeEnemyDied = onEnemyDied;
            RemainingEnemies = 0;
            spawningFinished = false;
            IsWaveActive = true;

            int scheduledEnemyCount = spawnManager.StartSpawning(
                waveData,
                enemyDefinition => HandleEnemyDied(waveId, enemyDefinition),
                () =>
                {
                    if (isStarting)
                    {
                        spawningCompletedDuringStart = true;
                        return;
                    }

                    HandleSpawningCompleted(waveId);
                });

            RemainingEnemies = scheduledEnemyCount;
            isStarting = false;

            if (spawningCompletedDuringStart)
            {
                HandleSpawningCompleted(waveId);
            }
        }

        public virtual void CancelWave()
        {
            if (!IsWaveActive)
            {
                return;
            }

            spawnManager?.CancelSpawning();
            activeWaveId++;
            ClearState();
        }

        private void HandleEnemyDied(int waveId, EnemyDefinition enemyDefinition)
        {
            if (!IsWaveActive || waveId != activeWaveId)
            {
                return;
            }

            activeEnemyDied?.Invoke(enemyDefinition);
            RemainingEnemies = Math.Max(0, RemainingEnemies - 1);
            TryCompleteWave();
        }

        private void HandleSpawningCompleted(int waveId)
        {
            if (!IsWaveActive || waveId != activeWaveId)
            {
                return;
            }

            spawningFinished = true;
            TryCompleteWave();
        }

        private void TryCompleteWave()
        {
            if (!IsWaveActive || !spawningFinished || RemainingEnemies > 0)
            {
                return;
            }

            WaveData completedWave = activeWave;
            Action<WaveData> completed = activeCompleted;
            activeWaveId++;
            ClearState();
            completed?.Invoke(completedWave);
        }

        private void ClearState()
        {
            activeWave = null;
            activeCompleted = null;
            activeEnemyDied = null;
            RemainingEnemies = 0;
            spawningFinished = false;
            IsWaveActive = false;
        }
    }
}
