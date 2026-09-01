using UnityEngine;

namespace RPGame.Enemies
{
    [CreateAssetMenu(
        fileName = "StraightProjectileAttackConfig",
        menuName = "RPGame/Enemies/Attacks/Straight Projectile Attack Config")]
    public sealed class StraightProjectileAttackConfig : RangedAttackConfig
    {
        [SerializeField] private GameObject projectilePrefab;

        public GameObject ProjectilePrefab => projectilePrefab;
    }
}
