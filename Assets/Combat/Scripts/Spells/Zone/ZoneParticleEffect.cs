using System;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    [Serializable]
    public sealed class ZoneParticleEffect
    {
        [SerializeField] private ParticleSystem particleSystem;
        [SerializeField] private bool matchZoneRadius = true;
        [SerializeField] private float radiusMultiplier = 1f;

        public ParticleSystem ParticleSystem => particleSystem;
        public bool MatchZoneRadius => matchZoneRadius;
        public float RadiusMultiplier => Mathf.Max(0f, radiusMultiplier);
    }
}
