using UnityEngine;

namespace RPGame.Enemies
{
    public abstract class RangedAttackConfig : AttackConfig
    {
        [SerializeField] private float projectileLifetime = 5f;

        public float ProjectileLifetime => projectileLifetime;

        protected override void OnValidate()
        {
            base.OnValidate();
            projectileLifetime = Mathf.Max(0f, projectileLifetime);
        }
    }
}
