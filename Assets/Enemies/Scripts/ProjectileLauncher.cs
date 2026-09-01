using UnityEngine;

namespace RPGame.Enemies
{
    public sealed class ProjectileLauncher : MonoBehaviour, IProjectileLauncher
    {
        [SerializeField] private Transform projectileSpawnPoint;

        public bool Launch(ProjectileLaunchData data)
        {
            if (data.ProjectilePrefab == null || data.TargetDamageable == null)
            {
                return false;
            }

            Transform spawnPoint = projectileSpawnPoint != null ? projectileSpawnPoint : transform;
            Vector3 direction = data.TargetPosition - spawnPoint.position;
            Quaternion rotation = direction.sqrMagnitude > 0f
                ? Quaternion.LookRotation(direction.normalized)
                : spawnPoint.rotation;

            EnemyStraightProjectile projectile = Instantiate(data.ProjectilePrefab, spawnPoint.position, rotation);
            projectile.Initialize(
                data.TargetDamageable,
                data.DamageParts,
                data.Source,
                data.ProjectileSpeed,
                data.ProjectileLifetime);

            return true;
        }
    }
}
