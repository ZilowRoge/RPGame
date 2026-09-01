using UnityEngine;

namespace RPGame.Enemies
{
    [CreateAssetMenu(
        fileName = "ParabolicProjectileAttackConfig",
        menuName = "RPGame/Enemies/Attacks/Parabolic Projectile Attack Config")]
    public sealed class ParabolicProjectileAttackConfig : RangedAttackConfig
    {
        [SerializeField] private float targetRandomRadius = 0.5f;
        [SerializeField] private float aoeRadius = 2f;
        [SerializeField] private GameObject projectilePrefab;

        public float TargetRandomRadius => targetRandomRadius;
        public float AoERadius => aoeRadius;
        public GameObject ProjectilePrefab => projectilePrefab;

        protected override void OnValidate()
        {
            base.OnValidate();
            targetRandomRadius = Mathf.Max(0f, targetRandomRadius);
            aoeRadius = Mathf.Max(0f, aoeRadius);
        }
    }
}
