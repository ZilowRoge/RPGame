using System;
using System.Collections.Generic;
using RPGame.Combat.Projectiles;
using RPGame.Core.Damage;
using UnityEngine;

namespace RPGame.Enemies
{
    [RequireComponent(typeof(ParabolicProjectileMover))]
    public sealed class EnemyParabolicProjectile : EnemyProjectile, IEnemyProjectile
    {
        private const float TelegraphGroundOffset = 0.02f;

        [SerializeField] private float projectileLifetime = 5f;
        [SerializeField] private float arcHeight = 3f;
        [SerializeField] private float ascentDuration = 0.75f;
        [SerializeField] private float descentDuration = 0.5f;
        [SerializeField] private GameObject telegraphPrefab;
        [SerializeField] private float aoeRadius = 2f;

        private ParabolicProjectileMover mover;
        private bool trajectoryCompleted;
        private GameObject activeTelegraph;

        public event Action<EnemyParabolicProjectile> ApexReached;

        internal Vector3 ApexPoint => mover.ApexPoint;
        internal Vector3 ImpactPoint => mover.ImpactPoint;
        internal float CurrentSpeed => mover.CurrentSpeed;
        internal ParabolicProjectilePhase Phase => mover.Phase;
        internal bool HasReachedApex => mover.HasReachedApex;
        internal int ApexReachedCount { get; private set; }
        internal EnemyProjectileHit LastImpact { get; private set; }
        internal bool HasImpact { get; private set; }
        internal GameObject ActiveTelegraph => activeTelegraph;

        private void Start()
        {
            CacheMover();
        }

        public void Initialize(
            Vector3 targetPosition,
            IDamageable targetDamageable,
            IReadOnlyList<PartialDamage> damageParts,
            GameObject source)
        {
            InitializeProjectile(damageParts, source, projectileLifetime);
            trajectoryCompleted = false;
            HasImpact = false;
            LastImpact = default;
            ApexReachedCount = 0;
            DestroyTelegraph();
            CacheMover();
            mover.ApexReached -= OnMoverApexReached;
            mover.ApexReached += OnMoverApexReached;
            mover.InitializeTrajectory(
                transform.position,
                targetPosition,
                arcHeight,
                ascentDuration,
                descentDuration);
        }

        protected override void Move(float deltaTime)
        {
            mover.Tick(deltaTime);
            trajectoryCompleted = mover.IsComplete;
        }

        protected override void AfterMove()
        {
            if (trajectoryCompleted)
            {
                FinishAt(mover.ImpactPoint, Vector3.up);
            }
        }

        protected override void OnImpact(EnemyProjectileHit hit, IDamageable damageable)
        {
            HasImpact = true;
            LastImpact = hit;
        }

        protected override void Cleanup()
        {
            DestroyTelegraph();
        }

        private void OnDestroy()
        {
            DestroyTelegraph();
        }

        private void CacheMover()
        {
            if (mover != null)
            {
                return;
            }

            mover = GetComponent<ParabolicProjectileMover>();
        }

        private void OnMoverApexReached()
        {
            ApexReachedCount++;
            SpawnTelegraph();
            ApexReached?.Invoke(this);
        }

        private void SpawnTelegraph()
        {
            if (activeTelegraph != null || telegraphPrefab == null)
            {
                return;
            }

            activeTelegraph = Instantiate(
                telegraphPrefab,
                mover.ImpactPoint + Vector3.up * TelegraphGroundOffset,
                Quaternion.identity);

            float diameter = aoeRadius * 2f;
            activeTelegraph.transform.localScale = new Vector3(diameter, activeTelegraph.transform.localScale.y, diameter);
        }

        private void DestroyTelegraph()
        {
            if (activeTelegraph == null)
            {
                return;
            }

            GameObject telegraph = activeTelegraph;
            activeTelegraph = null;
            if (Application.isPlaying)
            {
                Destroy(telegraph);
            }
            else
            {
                DestroyImmediate(telegraph);
            }
        }

        private void OnValidate()
        {
            projectileLifetime = Mathf.Max(0f, projectileLifetime);
            arcHeight = Mathf.Max(0f, arcHeight);
            ascentDuration = Mathf.Max(0.001f, ascentDuration);
            descentDuration = Mathf.Max(0.001f, descentDuration);
            aoeRadius = Mathf.Max(0f, aoeRadius);
        }
    }
}
