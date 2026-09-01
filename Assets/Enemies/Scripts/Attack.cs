using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Enemies
{
    public sealed class Attack : MonoBehaviour, IEnemyAttack
    {
        [SerializeField] private Config config;
        [SerializeField] private AttackType attackType = AttackType.Melee;
        [SerializeField] private LineOfSight lineOfSight;
        [SerializeField] private ProjectileLauncher projectileLauncher;

        private IEnemyAttack runtimeAttack;

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
            return runtimeAttack != null && runtimeAttack.TryAttack(target);
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

            try
            {
                runtimeAttack = CreateRuntimeAttack();
            }
            catch (System.InvalidOperationException exception)
            {
                Debug.LogError(exception.Message, this);
                return;
            }
        }

        private IEnemyAttack CreateRuntimeAttack()
        {
            return attackType switch
            {
                AttackType.Melee => new MeleeAttack(
                    config.GetAttack<MeleeAttackConfig>(AttackType.Melee),
                    () => transform.position,
                    GetDamageable,
                    () => gameObject),

                AttackType.StraightProjectile => new StraightProjectileAttack(
                    config.GetAttack<StraightProjectileAttackConfig>(AttackType.StraightProjectile),
                    GetLineOfSight(),
                    GetProjectileLauncher(),
                    GetDamageable,
                    () => gameObject),

                _ => throw new System.InvalidOperationException($"Unsupported attack type '{attackType}'.")
            };
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

        private static IDamageable GetDamageable(SelectedTarget target)
        {
            return TryGetDamageable(target, out IDamageable damageable) ? damageable : null;
        }

        private IEnemyLineOfSight GetLineOfSight()
        {
            if (lineOfSight == null)
            {
                lineOfSight = GetComponent<LineOfSight>();
            }

            if (lineOfSight == null)
            {
                throw new System.InvalidOperationException("Missing field lineOfSight.");
            }

            return lineOfSight;
        }

        private IProjectileLauncher GetProjectileLauncher()
        {
            if (projectileLauncher == null)
            {
                projectileLauncher = GetComponent<ProjectileLauncher>();
            }

            if (projectileLauncher == null)
            {
                throw new System.InvalidOperationException("Missing field projectileLauncher.");
            }

            return projectileLauncher;
        }
    }
}
