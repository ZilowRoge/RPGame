using RPGame.Combat.Damage;
using RPGame.Core.Statistics;
using RPGame.Core.Targeting;
using UnityEngine;

namespace RPGame.Enemies
{
    [RequireComponent(typeof(StatisticsController))]
    [RequireComponent(typeof(Detection))]
    [RequireComponent(typeof(Movement))]
    [RequireComponent(typeof(Attack))]
    [RequireComponent(typeof(EnemyTargetable))]
    [RequireComponent(typeof(DamageReceiver))]
    [RequireComponent(typeof(Death))]
    public sealed class Controller : MonoBehaviour
    {
        [SerializeField] private Detection detection;
        [SerializeField] private Movement movement;
        [SerializeField] private Attack attack;

        private IEnemyBehaviour behaviour;

        private void Start()
        {
            CacheRequiredComponents();
            if (!HasRequiredComponents())
            {
                enabled = false;
                return;
            }

            behaviour = new MeleeEnemyBehaviour(detection, movement, attack);
        }

        private void Update()
        {
            behaviour?.Tick(Time.deltaTime);
        }

        internal void Tick()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            behaviour?.Tick(Time.deltaTime);
        }

        private void CacheRequiredComponents()
        {
            if (detection == null)
            {
                detection = GetComponent<Detection>();
            }

            if (movement == null)
            {
                movement = GetComponent<Movement>();
            }

            if (attack == null)
            {
                attack = GetComponent<Attack>();
            }
        }

        private bool HasRequiredComponents()
        {
            if (detection == null)
            {
                Debug.LogError("Missing field detection.", this);
                return false;
            }

            if (movement == null)
            {
                Debug.LogError("Missing field movement.", this);
                return false;
            }

            if (attack == null)
            {
                Debug.LogError("Missing field attack.", this);
                return false;
            }

            return true;
        }
    }
}
