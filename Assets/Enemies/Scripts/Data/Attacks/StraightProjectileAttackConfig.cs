using UnityEngine;

namespace RPGame.Enemies
{
    [CreateAssetMenu(
        fileName = "StraightProjectileAttackConfig",
        menuName = "RPGame/Enemies/Attacks/Straight Projectile Attack Config")]
    public sealed class StraightProjectileAttackConfig : RangedAttackConfig
    {
        [SerializeField] private EnemyStraightProjectile projectilePrefab;
        [SerializeField] private float projectileSpeed = 8f;

        public EnemyStraightProjectile ProjectilePrefab => projectilePrefab;
        public float ProjectileSpeed => projectileSpeed;

        protected override void OnValidate()
        {
            base.OnValidate();
            projectileSpeed = Mathf.Max(0f, projectileSpeed);
        }
    }
}
