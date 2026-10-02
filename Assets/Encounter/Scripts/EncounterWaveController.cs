using System;
using UnityEngine;

namespace RPGame.Encounter
{
    public sealed class EncounterWaveController : MonoBehaviour
    {
        [SerializeField] private SpawnManager spawnManager;

        private WaveData activeWave;
        private Action<WaveData> activeCompleted;
        private bool spawningFinished;
        private int activeWaveId;

        public bool IsWaveActive { get; private set; }
        public int RemainingEnemies { get; private set; }

        public void StartWave(WaveData waveData, Action<WaveData> onCompleted)
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
            RemainingEnemies = 0;
            spawningFinished = false;
            IsWaveActive = true;

            int scheduledEnemyCount = spawnManager.StartSpawning(
                waveData,
                _ => HandleEnemyDied(waveId),
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

        public void CancelWave()
        {
            if (!IsWaveActive)
            {
                return;
            }

            spawnManager?.CancelSpawning();
            activeWaveId++;
            ClearState();
        }

        private void HandleEnemyDied(int waveId)
        {
            if (!IsWaveActive || waveId != activeWaveId)
            {
                return;
            }

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
            RemainingEnemies = 0;
            spawningFinished = false;
            IsWaveActive = false;
        }
    }
}
