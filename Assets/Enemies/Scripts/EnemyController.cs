using RPGame.Core.Targeting;
using UnityEngine;

namespace RPGame.Enemies
{
    [RequireComponent(typeof(Detection))]
    [RequireComponent(typeof(Movement))]
    [RequireComponent(typeof(Attack))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private Detection detection;
        [SerializeField] private Movement movement;
        [SerializeField] private Attack attack;

        private void Start()
        {
            CacheRequiredComponents();
            if (!HasRequiredComponents())
            {
                enabled = false;
            }
        }

        private void Update()
        {
            Tick();
        }

        public void Tick()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (!detection.HasTarget)
            {
                movement.Stop();
                return;
            }

            ITargetable target = detection.CurrentTarget;
            if (target == null || target.TargetPoint == null)
            {
                movement.Stop();
                return;
            }

            if (attack.IsInRange(target))
            {
                movement.Stop();
                attack.TryAttack(target);
                return;
            }

            movement.MoveTo(target.TargetPoint.position);
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
