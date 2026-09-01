using System;
using System.Collections.Generic;
using RPGame.Combat.Projectiles;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Enemies
{
    [RequireComponent(typeof(ParabolicProjectileMover))]
    public sealed class EnemyParabolicProjectile : EnemyProjectile
    {
        private ParabolicProjectileMover mover;
        private bool trajectoryCompleted;

        public event Action<EnemyParabolicProjectile> ApexReached;

        internal Vector3 ApexPoint => mover.ApexPoint;
        internal Vector3 ImpactPoint => mover.ImpactPoint;
        internal float CurrentSpeed => mover.CurrentSpeed;
        internal ParabolicProjectilePhase Phase => mover.Phase;
        internal bool HasReachedApex => mover.HasReachedApex;
        internal int ApexReachedCount { get; private set; }
        internal EnemyProjectileHit LastImpact { get; private set; }
        internal bool HasImpact { get; private set; }

        private void Start()
        {
            CacheMover();
        }

        public void Initialize(
            Vector3 impactPoint,
            IReadOnlyList<PartialDamage> damageParts,
            GameObject source,
            float projectileLifetime,
            float arcHeight,
            float ascentDuration,
            float descentDuration)
        {
            InitializeProjectile(damageParts, source, projectileLifetime);
            trajectoryCompleted = false;
            HasImpact = false;
            LastImpact = default;
            ApexReachedCount = 0;
            CacheMover();
            mover.ApexReached -= OnMoverApexReached;
            mover.ApexReached += OnMoverApexReached;
            mover.InitializeTrajectory(
                transform.position,
                impactPoint,
                arcHeight,
                ascentDuration,
                descentDuration);
        }

        protected override void Move(float deltaTime)
        {
            mover.Tick(deltaTime);
            trajectoryCompleted = mover.IsComplete;
        }

        protected override void AfterMove()
        {
            if (trajectoryCompleted)
            {
                FinishAt(mover.ImpactPoint, Vector3.up);
            }
        }

        protected override void OnImpact(EnemyProjectileHit hit, IDamageable damageable)
        {
            HasImpact = true;
            LastImpact = hit;
        }

        private void CacheMover()
        {
            if (mover != null)
            {
                return;
            }

            mover = GetComponent<ParabolicProjectileMover>();
        }

        private void OnMoverApexReached()
        {
            ApexReachedCount++;
            ApexReached?.Invoke(this);
        }
    }
}
