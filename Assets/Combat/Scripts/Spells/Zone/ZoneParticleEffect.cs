using System;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    [Serializable]
    public sealed class ZoneParticleEffect
    {
        [SerializeField] private ParticleSystem particleSystem;
        [SerializeField] private bool matchEmissionRadius = true;
        [SerializeField] private float emissionRadiusMultiplier = 1f;
        [SerializeField] private bool matchParticleSize;
        [SerializeField] private float particleSizeMultiplier = 1f;

        public ParticleSystem ParticleSystem => particleSystem;
        public bool MatchEmissionRadius => matchEmissionRadius;
        public float EmissionRadiusMultiplier => Mathf.Max(0f, emissionRadiusMultiplier);
        public bool MatchParticleSize => matchParticleSize;
        public float ParticleSizeMultiplier => Mathf.Max(0f, particleSizeMultiplier);
    }
}
