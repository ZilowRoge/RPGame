using UnityEngine;

namespace RPGame.Enemies
{
    public sealed class RangedEnemyBehaviour : IEnemyBehaviour
    {
        private const float MinDirectionSqrMagnitude = 0.0001f;

        private readonly IEnemyDetection detection;
        private readonly IEnemyMovement movement;
        private readonly IRangedEnemyBehaviourConfig config;
        private readonly IEnemyAttack straightAttack;
        private readonly IEnemyAttack parabolicAttack;

        internal RangedBehaviourState State { get; private set; } = RangedBehaviourState.Idle;

        public RangedEnemyBehaviour(
            IEnemyDetection detection,
            IEnemyMovement movement,
            IRangedEnemyBehaviourConfig config,
            IEnemyAttack straightAttack,
            IEnemyAttack parabolicAttack)
        {
            this.detection = detection;
            this.movement = movement;
            this.config = config;
            this.straightAttack = straightAttack;
            this.parabolicAttack = parabolicAttack;
        }

        public void Tick(float deltaTime)
        {
            straightAttack?.Tick(deltaTime);
            parabolicAttack?.Tick(deltaTime);

            if (!detection.TryGetTarget(out SelectedTarget target) || !target.IsValid)
            {
                State = RangedBehaviourState.Idle;
                movement.Stop();
                return;
            }

            float distance = Vector3.Distance(movement.Position, target.Position);
            RangedBehaviourState nextState = SelectState(distance);
            State = nextState;

            if (nextState == RangedBehaviourState.Approach)
            {
                movement.MoveTo(target.Position);
                return;
            }

            if (nextState == RangedBehaviourState.Retreat)
            {
                RetreatFrom(target.Position);
                straightAttack?.TryAttack(target);
                return;
            }

            movement.Stop();
            parabolicAttack?.TryAttack(target);
        }

        private RangedBehaviourState SelectState(float distance)
        {
            if (State == RangedBehaviourState.Approach && distance > config.MaxRange - config.RangeHysteresis)
            {
                return RangedBehaviourState.Approach;
            }

            if (State == RangedBehaviourState.Retreat && distance < config.MinRange + config.RangeHysteresis)
            {
                return RangedBehaviourState.Retreat;
            }

            if (distance > config.MaxRange)
            {
                return RangedBehaviourState.Approach;
            }

            if (distance < config.MinRange)
            {
                return RangedBehaviourState.Retreat;
            }

            return RangedBehaviourState.Hold;
        }

        private void RetreatFrom(Vector3 targetPosition)
        {
            Vector3 retreatDirection = movement.Position - targetPosition;
            if (retreatDirection.sqrMagnitude <= MinDirectionSqrMagnitude)
            {
                movement.Stop();
                return;
            }

            float desiredDistance = config.MinRange + config.RangeHysteresis;
            Vector3 desiredPosition = targetPosition + retreatDirection.normalized * desiredDistance;
            if (!movement.TryResolvePosition(desiredPosition, out Vector3 validPosition))
            {
                movement.Stop();
                return;
            }

            movement.MoveTo(validPosition);
        }
    }
}
