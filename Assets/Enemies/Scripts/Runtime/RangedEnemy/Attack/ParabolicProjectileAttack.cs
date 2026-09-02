using System;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Enemies
{
    public sealed class ParabolicProjectileAttack : IEnemyAttack
    {
        private readonly ParabolicProjectileAttackConfig config;
        private readonly IEnemyLineOfSight lineOfSight;
        private readonly IEnemyGroundProjection groundProjection;
        private readonly IProjectileLauncher projectileLauncher;
        private readonly Func<GameObject> sourceProvider;
        private readonly IRandomPointInCircle randomPointProvider;
        private float remainingCooldown;

        public ParabolicProjectileAttack(
            ParabolicProjectileAttackConfig config,
            IEnemyLineOfSight lineOfSight,
            IEnemyGroundProjection groundProjection,
            IProjectileLauncher projectileLauncher,
            Func<GameObject> sourceProvider)
            : this(
                config,
                lineOfSight,
                groundProjection,
                projectileLauncher,
                sourceProvider,
                new UnityRandomPointInCircle())
        {
        }

        internal ParabolicProjectileAttack(
            ParabolicProjectileAttackConfig config,
            IEnemyLineOfSight lineOfSight,
            IEnemyGroundProjection groundProjection,
            IProjectileLauncher projectileLauncher,
            Func<GameObject> sourceProvider,
            IRandomPointInCircle randomPointProvider)
        {
            this.config = config;
            this.lineOfSight = lineOfSight;
            this.groundProjection = groundProjection;
            this.projectileLauncher = projectileLauncher;
            this.sourceProvider = sourceProvider;
            this.randomPointProvider = randomPointProvider;
        }

        public float Range => float.PositiveInfinity;

        public void Tick(float deltaTime)
        {
            remainingCooldown -= deltaTime;
        }

        public bool IsInRange(SelectedTarget target)
        {
            return target.IsValid;
        }

        public bool TryAttack(SelectedTarget target)
        {
            if (!CanAttack(target))
            {
                return false;
            }

            Vector3 candidatePosition = CreateImpactCandidate(target.Position);
            if (!groundProjection.TryProjectToGround(candidatePosition, out Vector3 impactPoint))
            {
                return false;
            }

            ProjectileLaunchData launchData = new(
                config.ProjectilePrefab,
                impactPoint,
                null,
                DamageRangeRoller.Roll(config.Damage),
                sourceProvider?.Invoke());

            if (!projectileLauncher.Launch(launchData))
            {
                return false;
            }

            remainingCooldown = config.AttackInterval;
            return true;
        }

        private bool CanAttack(SelectedTarget target)
        {
            return remainingCooldown <= 0f
                && target.IsValid
                && config != null
                && config.ProjectilePrefab != null
                && lineOfSight != null
                && lineOfSight.HasLineOfSight(target.Position)
                && groundProjection != null
                && projectileLauncher != null;
        }

        private Vector3 CreateImpactCandidate(Vector3 targetPosition)
        {
            Vector2 offset = randomPointProvider.NextPoint(config.TargetRandomRadius);
            return targetPosition + new Vector3(offset.x, 0f, offset.y);
        }
    }

    internal interface IRandomPointInCircle
    {
        Vector2 NextPoint(float radius);
    }

    internal sealed class UnityRandomPointInCircle : IRandomPointInCircle
    {
        public Vector2 NextPoint(float radius)
        {
            return UnityEngine.Random.insideUnitCircle * Mathf.Max(0f, radius);
        }
    }
}
