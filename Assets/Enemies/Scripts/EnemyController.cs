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

        private void Awake()
        {
            ResolveComponents();
        }

        private void Update()
        {
            Tick();
        }

        public void Tick()
        {
            ResolveComponents();

            if (detection == null || movement == null || attack == null || !detection.HasTarget)
            {
                movement?.Stop();
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

        private void ResolveComponents()
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
    }
}
