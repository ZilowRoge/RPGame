using System;
using System.Collections.Generic;

namespace RPGame.Encounter
{
    public sealed class WaveData
    {
        public WaveData(
            int waveNumber,
            float effectiveWave,
            int budget,
            IEnumerable<EnemyDefinition> enemies,
            int seed)
        {
            WaveNumber = waveNumber;
            EffectiveWave = effectiveWave;
            Budget = budget;
            Enemies = new List<EnemyDefinition>(enemies ?? Array.Empty<EnemyDefinition>()).AsReadOnly();
            Seed = seed;
        }

        public int WaveNumber { get; }
        public float EffectiveWave { get; }
        public int Budget { get; }
        public IReadOnlyList<EnemyDefinition> Enemies { get; }
        public int Seed { get; }
    }
}
