using RPGame.Combat.Projectiles;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Enemies
{
    [RequireComponent(typeof(StraightProjectileMover))]
    public sealed class EnemyStraightProjectile : EnemyProjectile, IEnemyProjectile, IProjectileMovementSource
    {
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private float projectileLifetime = 5f;

        private StraightProjectileMover mover;
        private IDamageable targetDamageable;

        public float CurrentSpeed => projectileSpeed;

        private void Start()
        {
            CacheMover();
        }

        public void Initialize(
            Vector3 targetPosition,
            IDamageable targetDamageable,
            System.Collections.Generic.IReadOnlyList<PartialDamage> damageParts,
            GameObject source)
        {
            this.targetDamageable = targetDamageable;
            InitializeProjectile(damageParts, source, projectileLifetime);

            CacheMover();
            mover.Initialize(this);
        }

        protected override void Move(float deltaTime)
        {
            mover.Tick(deltaTime);
        }

        protected override void OnImpact(EnemyProjectileHit hit, IDamageable damageable)
        {
            if (ReferenceEquals(damageable, targetDamageable))
            {
                damageable.ApplyDamage(new DamageData(DamageParts, Source));
            }
        }

        private void CacheMover()
        {
            if (mover != null)
            {
                return;
            }

            mover = GetComponent<StraightProjectileMover>();
        }

        private void OnValidate()
        {
            projectileSpeed = Mathf.Max(0f, projectileSpeed);
            projectileLifetime = Mathf.Max(0f, projectileLifetime);
        }
    }
}
