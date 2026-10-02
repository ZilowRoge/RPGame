using System;

namespace RPGame.Encounter
{
    public enum EncounterState
    {
        Idle,
        Intermission,
        WaveActive,
        Ended
    }

    public sealed class EncounterController
    {
        public const float DefaultIntermissionDuration = 15f;

        private readonly EncounterWaveController waveController;
        private readonly WaveGenerator waveGenerator;
        private readonly ScoreSystem scoreSystem;
        private readonly float intermissionDuration;

        private int runVersion;
        private WaveData activeWave;
        private bool isStartingWave;

        public EncounterController(
            EncounterWaveController waveController,
            WaveGenerator waveGenerator,
            ScoreSystem scoreSystem,
            float intermissionDuration = DefaultIntermissionDuration)
        {
            this.waveController = waveController ?? throw new ArgumentNullException(nameof(waveController));
            this.waveGenerator = waveGenerator ?? throw new ArgumentNullException(nameof(waveGenerator));
            this.scoreSystem = scoreSystem ?? throw new ArgumentNullException(nameof(scoreSystem));

            if (float.IsNaN(intermissionDuration) || float.IsInfinity(intermissionDuration)
                || intermissionDuration < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(intermissionDuration));
            }

            this.intermissionDuration = intermissionDuration;
        }

        public EncounterState State { get; private set; } = EncounterState.Idle;
        public int CurrentWaveNumber { get; private set; }
        public int EncounterSeed { get; private set; }
        public WaveData PendingWave { get; private set; }
        public float IntermissionTimeRemaining { get; private set; }
        public float IntermissionDuration => intermissionDuration;

        public void StartEncounter(int encounterSeed)
        {
            if (State != EncounterState.Idle && State != EncounterState.Ended)
            {
                throw new InvalidOperationException("An encounter can only start from the idle or ended state.");
            }

            runVersion++;
            scoreSystem.ResetRun();
            EncounterSeed = encounterSeed;
            CurrentWaveNumber = 1;
            activeWave = null;
            PendingWave = waveGenerator.GenerateWave(CurrentWaveNumber, 0f, EncounterSeed);
            IntermissionTimeRemaining = intermissionDuration;
            State = EncounterState.Intermission;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            if (State == EncounterState.Intermission || State == EncounterState.WaveActive)
            {
                scoreSystem.Tick(deltaTime);
            }

            if (State != EncounterState.Intermission)
            {
                return;
            }

            IntermissionTimeRemaining -= deltaTime;
            if (IntermissionTimeRemaining <= 0f)
            {
                StartPendingWaveNow();
            }
        }

        public void StartPendingWaveNow()
        {
            if (State != EncounterState.Intermission)
            {
                throw new InvalidOperationException("A pending wave can only start during intermission.");
            }

            if (PendingWave == null)
            {
                throw new InvalidOperationException("There is no pending wave to start.");
            }

            WaveData waveToStart = PendingWave;
            int waveRunVersion = runVersion;

            PendingWave = null;
            IntermissionTimeRemaining = 0f;
            activeWave = waveToStart;
            scoreSystem.BeginWave();
            isStartingWave = true;
            try
            {
                waveController.StartWave(
                    waveToStart,
                    completedWave => HandleWaveCompleted(waveRunVersion, waveToStart, completedWave),
                    enemyDefinition => HandleEnemyDied(waveRunVersion, waveToStart, enemyDefinition));
            }
            finally
            {
                isStartingWave = false;
            }

            if (State == EncounterState.Intermission && ReferenceEquals(activeWave, waveToStart))
            {
                State = EncounterState.WaveActive;
            }
        }

        public void EndEncounter()
        {
            if (State == EncounterState.Ended)
            {
                return;
            }

            runVersion++;

            if (State == EncounterState.WaveActive)
            {
                waveController.CancelWave();
            }

            activeWave = null;
            PendingWave = null;
            IntermissionTimeRemaining = 0f;
            State = EncounterState.Ended;
        }

        private void HandleEnemyDied(int callbackRunVersion, WaveData callbackWave, EnemyDefinition enemyDefinition)
        {
            if (callbackRunVersion != runVersion || (State != EncounterState.WaveActive && !isStartingWave)
                || !ReferenceEquals(callbackWave, activeWave) || enemyDefinition == null)
            {
                return;
            }

            scoreSystem.RegisterKill(enemyDefinition.Cost);
        }

        private void HandleWaveCompleted(int callbackRunVersion, WaveData callbackWave, WaveData completedWave)
        {
            if (callbackRunVersion != runVersion || (State != EncounterState.WaveActive && !isStartingWave)
                || !ReferenceEquals(callbackWave, activeWave) || !ReferenceEquals(completedWave, activeWave))
            {
                return;
            }

            scoreSystem.EndWave();
            CurrentWaveNumber++;
            activeWave = null;
            PendingWave = waveGenerator.GenerateWave(
                CurrentWaveNumber,
                scoreSystem.LastWavePerformanceBonus,
                EncounterSeed);
            IntermissionTimeRemaining = intermissionDuration;
            State = EncounterState.Intermission;
        }
    }
}
