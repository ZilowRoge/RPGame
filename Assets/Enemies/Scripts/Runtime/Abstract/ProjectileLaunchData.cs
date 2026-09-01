using System.Collections.Generic;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Enemies
{
    public readonly struct ProjectileLaunchData
    {
        public ProjectileLaunchData(
            EnemyStraightProjectile projectilePrefab,
            Vector3 targetPosition,
            IDamageable targetDamageable,
            IReadOnlyList<PartialDamage> damageParts,
            GameObject source,
            float projectileSpeed,
            float projectileLifetime)
        {
            ProjectilePrefab = projectilePrefab;
            TargetPosition = targetPosition;
            TargetDamageable = targetDamageable;
            DamageParts = damageParts;
            Source = source;
            ProjectileSpeed = projectileSpeed;
            ProjectileLifetime = projectileLifetime;
        }

        public EnemyStraightProjectile ProjectilePrefab { get; }
        public Vector3 TargetPosition { get; }
        public IDamageable TargetDamageable { get; }
        public IReadOnlyList<PartialDamage> DamageParts { get; }
        public GameObject Source { get; }
        public float ProjectileSpeed { get; }
        public float ProjectileLifetime { get; }
    }
}
