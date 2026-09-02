using RPGame.Core.Statistics;
using RPGame.Core.Targeting;
using UnityEngine;

namespace RPGame.Enemies
{
    public sealed class Death : MonoBehaviour
    {
        [SerializeField] private StatisticsController deathSource;
        [SerializeField] private Movement movement;
        [SerializeField] private Controller controller;
        [SerializeField] private Detection detection;
        [SerializeField] private EnemyTargetable targetable;

        private StatisticsController subscribedDeathSource;

        internal bool IsDead { get; private set; }

        private void Start()
        {
            CacheRequiredComponents();
            if (!HasRequiredComponents())
            {
                enabled = false;
                return;
            }

            SubscribeDeathSource();
        }

        private void OnDisable()
        {
            UnsubscribeDeathSource();
        }

        private void HandleDeath()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            movement.Stop();
            controller.enabled = false;
            detection.enabled = false;
            detection.ClearCurrentTarget();
            targetable.enabled = false;
        }

        private void SubscribeDeathSource()
        {
            if (subscribedDeathSource != null && subscribedDeathSource != deathSource)
            {
                UnsubscribeDeathSource();
            }

            deathSource.Died -= HandleDeath;
            deathSource.Died += HandleDeath;
            subscribedDeathSource = deathSource;
        }

        private void UnsubscribeDeathSource()
        {
            if (subscribedDeathSource != null)
            {
                subscribedDeathSource.Died -= HandleDeath;
                subscribedDeathSource = null;
            }
        }

        private void CacheRequiredComponents()
        {
            if (deathSource == null)
            {
                deathSource = GetComponentInParent<StatisticsController>();
            }

            if (movement == null)
            {
                movement = GetComponent<Movement>();
            }

            if (controller == null)
            {
                controller = GetComponent<Controller>();
            }

            if (detection == null)
            {
                detection = GetComponent<Detection>();
            }

            if (targetable == null)
            {
                targetable = GetComponent<EnemyTargetable>();
            }
        }

        private bool HasRequiredComponents()
        {
            if (deathSource == null)
            {
                Debug.LogError("Missing field deathSource.", this);
                return false;
            }

            if (movement == null)
            {
                Debug.LogError("Missing field movement.", this);
                return false;
            }

            if (controller == null)
            {
                Debug.LogError("Missing field controller.", this);
                return false;
            }

            if (detection == null)
            {
                Debug.LogError("Missing field detection.", this);
                return false;
            }

            if (targetable == null)
            {
                Debug.LogError("Missing field targetable.", this);
                return false;
            }

            return true;
        }
    }
}
