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
        [SerializeField] private float arcHeight = 3f;
        [SerializeField] private float ascentDuration = 0.75f;
        [SerializeField] private float descentDuration = 0.5f;

        public float TargetRandomRadius => targetRandomRadius;
        public float AoERadius => aoeRadius;
        public float ArcHeight => arcHeight;
        public float AscentDuration => ascentDuration;
        public float DescentDuration => descentDuration;

        protected override void OnValidate()
        {
            base.OnValidate();
            targetRandomRadius = Mathf.Max(0f, targetRandomRadius);
            aoeRadius = Mathf.Max(0f, aoeRadius);
            arcHeight = Mathf.Max(0f, arcHeight);
            ascentDuration = Mathf.Max(0.001f, ascentDuration);
            descentDuration = Mathf.Max(0.001f, descentDuration);
        }
    }
}
