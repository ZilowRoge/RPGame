using System.Collections.Generic;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Enemies
{
    public readonly struct ProjectileLaunchData
    {
        public ProjectileLaunchData(
            GameObject projectilePrefab,
            Vector3 targetPosition,
            IDamageable targetDamageable,
            IReadOnlyList<PartialDamage> damageParts,
            GameObject source)
        {
            ProjectilePrefab = projectilePrefab;
            TargetPosition = targetPosition;
            TargetDamageable = targetDamageable;
            DamageParts = damageParts;
            Source = source;
        }

        public GameObject ProjectilePrefab { get; }
        public Vector3 TargetPosition { get; }
        public IDamageable TargetDamageable { get; }
        public IReadOnlyList<PartialDamage> DamageParts { get; }
        public GameObject Source { get; }
    }
}
