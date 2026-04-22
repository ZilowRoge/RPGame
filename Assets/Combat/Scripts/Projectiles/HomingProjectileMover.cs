using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Projectiles
{
    public sealed class HomingProjectileMover : ProjectileMover
    {
        [SerializeField] private float turnSpeed = 360f;
        [SerializeField] private bool lockDirectionAfterPassingTarget = true;

        private Vector3 lastDirection;
        private bool isDirectionLocked;

        public override void Initialize(ProjectileController projectile, CasterData casterData)
        {
            base.Initialize(projectile, casterData);
            lastDirection = transform.forward;
        }

        public override void Tick(float deltaTime)
        {
            if (isDirectionLocked)
            {
                MoveForward(deltaTime);
                return;
            }

            if (lockDirectionAfterPassingTarget && HasPassedTarget())
            {
                isDirectionLocked = true;
                MoveForward(deltaTime);
                return;
            }

            Vector3 desiredDirection = GetDesiredDirection();
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(desiredDirection),
                turnSpeed * deltaTime);

            MoveForward(deltaTime);
        }

        private Vector3 GetDesiredDirection()
        {
            Transform target = CasterData.Target;
            if (target == null)
            {
                return lastDirection.sqrMagnitude > 0f ? lastDirection.normalized : transform.forward;
            }

            Vector3 directionToTarget = target.position - transform.position;
            if (directionToTarget.sqrMagnitude <= Mathf.Epsilon)
            {
                return transform.forward;
            }

            return directionToTarget.normalized;
        }

        private bool HasPassedTarget()
        {
            Transform target = CasterData.Target;
            if (target == null)
            {
                return false;
            }

            Vector3 directionToTarget = target.position - transform.position;
            if (directionToTarget.sqrMagnitude <= Mathf.Epsilon)
            {
                return false;
            }

            return Vector3.Dot(transform.forward, directionToTarget.normalized) <= 0f;
        }

        private void MoveForward(float deltaTime)
        {
            lastDirection = transform.forward;
            transform.position += transform.forward * Projectile.CurrentSpeed * deltaTime;
        }

        private void OnValidate()
        {
            turnSpeed = Mathf.Max(0f, turnSpeed);
        }
    }
}
