using System;
using System.Collections.Generic;
using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Encounter
{
    [RequireComponent(typeof(EncounterWaveController))]
    [RequireComponent(typeof(SpawnManager))]
    [RequireComponent(typeof(EnemyPool))]
    public sealed class EncounterRuntime : MonoBehaviour
    {
        [SerializeField] private EncounterWaveController waveController;
        [SerializeField] private EnemyPool enemyPool;
        [SerializeField] private WaveScalingConfig waveScalingConfig;
        [SerializeField] private ScoreConfig scoreConfig;
        [SerializeField] private List<EnemyDefinition> enemyDefinitions = new();
        [SerializeField] private StatisticsController playerStatistics;
        [SerializeField] private StartWaveInteractable startWaveInteractable;
        [SerializeField] private float intermissionDuration = EncounterController.DefaultIntermissionDuration;

        private EncounterController encounterController;
        private ScoreSystem scoreSystem;
        private bool isSubscribedToPlayerDeath;
        private int lastLoggedWaveNumber;

        public EncounterController EncounterController => encounterController;
        public ScoreSystem ScoreSystem => scoreSystem;
        public int RemainingEnemyCount => waveController != null ? waveController.RemainingEnemies : 0;
        public event Action<EncounterResult> RunEnded;

        private void Start()
        {
            if (encounterController != null)
            {
                return;
            }

            WaveGenerator waveGenerator = new(waveScalingConfig, enemyDefinitions);
            scoreSystem = new ScoreSystem(scoreConfig, waveScalingConfig);
            encounterController = new EncounterController(
                waveController,
                waveGenerator,
                scoreSystem,
                intermissionDuration,
                ReleaseActiveEnemies);
            startWaveInteractable.Initialize(encounterController);
            enemyPool.Prewarm(enemyDefinitions);
            SubscribeToPlayerDeath();
            int runSeed = Guid.NewGuid().GetHashCode();
            encounterController.StartEncounter(runSeed);
            Debug.Log($"Encounter initialized with run seed {runSeed} and entered intermission.", this);
        }

        private void Update()
        {
            encounterController?.Tick(Time.deltaTime);
            LogWaveStart();
        }

        private void OnEnable()
        {
            SubscribeToPlayerDeath();
        }

        private void OnDisable()
        {
            UnsubscribeFromPlayerDeath();
        }

        private void OnDestroy()
        {
            UnsubscribeFromPlayerDeath();
        }

        private void SubscribeToPlayerDeath()
        {
            if (isSubscribedToPlayerDeath || playerStatistics == null || encounterController == null)
            {
                return;
            }

            playerStatistics.Died += HandlePlayerDied;
            isSubscribedToPlayerDeath = true;
        }

        private void UnsubscribeFromPlayerDeath()
        {
            if (!isSubscribedToPlayerDeath)
            {
                return;
            }

            if (playerStatistics != null)
            {
                playerStatistics.Died -= HandlePlayerDied;
            }

            isSubscribedToPlayerDeath = false;
        }

        private void HandlePlayerDied()
        {
            Debug.Log("Player died. Ending encounter.", this);
            if (encounterController == null)
            {
                return;
            }

            UnsubscribeFromPlayerDeath();
            encounterController.EndEncounter();
            RunEnded?.Invoke(new EncounterResult(
                scoreSystem.CurrentScore,
                Math.Max(0, encounterController.CurrentWaveNumber - 1),
                encounterController.EncounterSeed));
        }

        private void ReleaseActiveEnemies()
        {
            Debug.Log("Encounter cleanup: releasing active enemies.", this);
            enemyPool.ReleaseAllActive();
        }

        private void LogWaveStart()
        {
            if (encounterController == null
                || encounterController.State != EncounterState.WaveActive
                || encounterController.CurrentWaveNumber == lastLoggedWaveNumber)
            {
                return;
            }

            lastLoggedWaveNumber = encounterController.CurrentWaveNumber;
            Debug.Log($"Encounter wave {lastLoggedWaveNumber} started spawning.", this);
        }
    }
}
