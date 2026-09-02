using System;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Enemies
{
    public sealed class MeleeAttack : IEnemyAttack
    {
        private readonly MeleeAttackConfig config;
        private readonly Func<Vector3> attackerPositionProvider;
        private readonly Func<SelectedTarget, IDamageable> damageableProvider;
        private readonly Func<GameObject> sourceProvider;
        private float remainingCooldown;

        public MeleeAttack(
            MeleeAttackConfig config,
            Func<Vector3> attackerPositionProvider,
            Func<SelectedTarget, IDamageable> damageableProvider = null,
            Func<GameObject> sourceProvider = null)
        {
            this.config = config;
            this.attackerPositionProvider = attackerPositionProvider;
            this.damageableProvider = damageableProvider;
            this.sourceProvider = sourceProvider;
        }

        public float Range => config.AttackRange;

        public void Tick(float deltaTime)
        {
            remainingCooldown -= deltaTime;
        }

        public bool IsInRange(SelectedTarget target)
        {
            if (!target.IsValid)
            {
                return false;
            }

            float attackRangeSqr = Range * Range;
            return (target.Position - attackerPositionProvider()).sqrMagnitude <= attackRangeSqr;
        }

        public bool TryAttack(SelectedTarget target)
        {
            return TryAttack(target, damageableProvider?.Invoke(target), sourceProvider?.Invoke());
        }

        internal bool TryAttack(SelectedTarget target, IDamageable damageable, GameObject source)
        {
            if (!CanAttack(target, damageable))
            {
                return false;
            }

            DamageResult result = damageable.ApplyDamage(new DamageData(DamageRangeRoller.Roll(config.Damage), source));
            if (!result.WasApplied)
            {
                return false;
            }

            remainingCooldown = config.AttackInterval;
            return true;
        }

        private bool CanAttack(SelectedTarget target, IDamageable damageable)
        {
            return remainingCooldown <= 0f
                && IsInRange(target)
                && damageable != null
                && damageable.CanReceiveDamage;
        }
    }
}
