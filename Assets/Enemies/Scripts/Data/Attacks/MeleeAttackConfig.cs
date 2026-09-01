using UnityEngine;

namespace RPGame.Enemies
{
    [CreateAssetMenu(
        fileName = "MeleeAttackConfig",
        menuName = "RPGame/Enemies/Attacks/Melee Attack Config")]
    public sealed class MeleeAttackConfig : AttackConfig
    {
        [SerializeField] private float attackRange = 1.5f;

        public float AttackRange => attackRange;

        protected override void OnValidate()
        {
            base.OnValidate();
            attackRange = Mathf.Max(0f, attackRange);
        }
    }
}
