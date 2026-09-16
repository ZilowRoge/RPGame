using RPGame.Core.Spells;
using RPGame.Combat.Projectiles;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    public sealed class OrbitController : MonoBehaviour
    {
        [SerializeField] private OrbitProjectileController projectilePrefab;

        private Transform center;
        private int projectileCount;
        private float orbitRadius;
        private float angularSpeed;
        private float lifetime;
        private float damageCapacity;
        private CasterData casterData;
        private bool isInitialized;

        public void Initialize(
            Transform center,
            int projectileCount,
            float orbitRadius,
            float angularSpeed,
            float lifetime,
            float damageCapacity,
            CasterData casterData)
        {
            this.center = center;
            this.projectileCount = Mathf.Max(1, projectileCount);
            this.orbitRadius = Mathf.Max(0f, orbitRadius);
            this.angularSpeed = angularSpeed;
            this.lifetime = Mathf.Max(0f, lifetime);
            this.damageCapacity = Mathf.Max(0f, damageCapacity);
            this.casterData = casterData;

            transform.SetPositionAndRotation(center != null ? center.position : transform.position, Quaternion.identity);
            if (center == null || projectilePrefab == null || this.lifetime <= 0f || this.damageCapacity <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            SpawnProjectiles();
            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized || center == null)
            {
                Destroy(gameObject);
                return;
            }

            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = center.position;
            transform.Rotate(Vector3.up, angularSpeed * Time.deltaTime);
        }

        private void SpawnProjectiles()
        {
            float angleStep = 360f / projectileCount;
            for (int i = 0; i < projectileCount; i++)
            {
                OrbitProjectileController projectile = Instantiate(projectilePrefab, transform);
                float angle = angleStep * i * Mathf.Deg2Rad;
                projectile.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * orbitRadius,
                    0f,
                    Mathf.Sin(angle) * orbitRadius);
                projectile.transform.localRotation = Quaternion.identity;
                projectile.Initialize(casterData, damageCapacity);
            }
        }
    }
}
