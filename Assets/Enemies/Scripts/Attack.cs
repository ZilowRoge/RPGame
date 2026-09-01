using RPGame.Core.Damage;
using RPGame.Core.Targeting;
using UnityEngine;

namespace RPGame.Enemies
{
    public sealed class Attack : MonoBehaviour, IEnemyAttack
    {
        [SerializeField] private Config config;
        [SerializeField] private AttackType attackType = AttackType.Melee;

        private MeleeAttack runtimeAttack;

        float IEnemyAttack.Range
        {
            get => runtimeAttack != null ? runtimeAttack.Range : 0f;
        }

        private void Start()
        {
            EnsureRuntimeAttack();
            if (runtimeAttack == null)
            {
                enabled = false;
            }
        }

        void IEnemyAttack.Tick(float deltaTime)
        {
            runtimeAttack?.Tick(deltaTime);
        }

        bool IEnemyAttack.IsInRange(SelectedTarget target)
        {
            return runtimeAttack != null && runtimeAttack.IsInRange(target);
        }

        bool IEnemyAttack.TryAttack(SelectedTarget target)
        {
            if (runtimeAttack == null || !TryGetDamageable(target, out IDamageable damageable))
            {
                return false;
            }

            return runtimeAttack.TryAttack(target, damageable, gameObject);
        }

        private void EnsureRuntimeAttack()
        {
            if (runtimeAttack != null)
            {
                return;
            }

            if (config == null)
            {
                Debug.LogError("Missing field config.", this);
                return;
            }

            if (attackType != AttackType.Melee)
            {
                Debug.LogError($"Unsupported attack type '{attackType}' for melee Attack adapter.", this);
                return;
            }

            MeleeAttackConfig attackConfig;
            try
            {
                attackConfig = config.GetAttack<MeleeAttackConfig>(attackType);
            }
            catch (System.InvalidOperationException exception)
            {
                Debug.LogError(exception.Message, this);
                return;
            }

            runtimeAttack = new MeleeAttack(attackConfig, () => transform.position);
        }

        private static bool TryGetDamageable(SelectedTarget target, out IDamageable damageable)
        {
            damageable = null;
            if (!target.IsValid || target.Targetable.TargetPoint == null)
            {
                return false;
            }

            damageable = target.Targetable.TargetPoint.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                return false;
            }

            return true;
        }
    }
}
