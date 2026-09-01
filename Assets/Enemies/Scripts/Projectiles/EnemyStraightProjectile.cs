using RPGame.Combat.Projectiles;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Enemies
{
    [RequireComponent(typeof(StraightProjectileMover))]
    public sealed class EnemyStraightProjectile : EnemyProjectile, IProjectileMovementSource
    {
        private StraightProjectileMover mover;
        private IDamageable targetDamageable;
        private float currentSpeed;

        public float CurrentSpeed => currentSpeed;

        private void Awake()
        {
            mover = EnsureMover();
        }

        public void Initialize(
            IDamageable targetDamageable,
            System.Collections.Generic.IReadOnlyList<PartialDamage> damageParts,
            GameObject source,
            float projectileSpeed,
            float projectileLifetime)
        {
            this.targetDamageable = targetDamageable;
            currentSpeed = projectileSpeed;
            InitializeProjectile(damageParts, source, projectileLifetime);

            mover = EnsureMover();
            mover.Initialize(this);
        }

        protected override void Move(float deltaTime)
        {
            mover = EnsureMover();
            mover.Tick(deltaTime);
        }

        protected override void OnImpact(EnemyProjectileHit hit, IDamageable damageable)
        {
            if (ReferenceEquals(damageable, targetDamageable))
            {
                damageable.ApplyDamage(new DamageData(DamageParts, Source));
            }
        }

        private StraightProjectileMover EnsureMover()
        {
            return mover != null
                ? mover
                : GetComponent<StraightProjectileMover>() ?? gameObject.AddComponent<StraightProjectileMover>();
        }
    }
}
