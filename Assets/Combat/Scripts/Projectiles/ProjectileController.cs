using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Projectiles
{
    public sealed class ProjectileController : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float acceleration;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float maxLifetime = 5f;

        private ProjectileMover mover;
        private float currentSpeed;
        private float remainingLifetime;

        public CasterData CasterData { get; private set; }
        public float CurrentSpeed => currentSpeed;
        public float Damage => damage;
        public bool IsInitialized { get; private set; }

        public void Initialize(CasterData casterData)
        {
            CasterData = casterData;
            currentSpeed = speed;
            remainingLifetime = maxLifetime;
            mover = GetComponent<ProjectileMover>();

            if (mover == null)
            {
                Debug.LogWarning($"{name} cannot move because no {nameof(ProjectileMover)} is attached.", this);
                Destroy(gameObject);
                return;
            }

            mover.Initialize(this, casterData);
            IsInitialized = true;
        }

        private void Update()
        {
            if (!IsInitialized)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            remainingLifetime -= deltaTime;
            if (remainingLifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            currentSpeed = Mathf.Max(0f, currentSpeed + acceleration * deltaTime);
            mover.Tick(deltaTime);
        }

        private void OnValidate()
        {
            speed = Mathf.Max(0f, speed);
            damage = Mathf.Max(0f, damage);
            maxLifetime = Mathf.Max(0.01f, maxLifetime);
        }
    }
}
