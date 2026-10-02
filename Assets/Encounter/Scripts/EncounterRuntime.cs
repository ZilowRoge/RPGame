using System;
using System.Collections.Generic;
using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Encounter
{
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

        public EncounterController EncounterController => encounterController;
        public ScoreSystem ScoreSystem => scoreSystem;

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
                enemyPool.ReleaseAllActive);
            startWaveInteractable.Initialize(encounterController);
            enemyPool.Prewarm(enemyDefinitions);
            SubscribeToPlayerDeath();
            encounterController.StartEncounter(Guid.NewGuid().GetHashCode());
        }

        private void Update()
        {
            encounterController?.Tick(Time.deltaTime);
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
            encounterController?.EndEncounter();
        }
    }
}
