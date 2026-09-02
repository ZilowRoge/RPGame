using UnityEngine;

namespace RPGame.Enemies
{
    public sealed class RangedEnemyBehaviour : IEnemyBehaviour
    {
        private const float MinDirectionSqrMagnitude = 0.0001f;
        private const float MinRepositionSearchInterval = 0.001f;
        private const int RepositionCandidateCount = 8;
        private const float FullCircleDegrees = 360f;

        private readonly IEnemyDetection detection;
        private readonly IEnemyMovement movement;
        private readonly IRangedEnemyBehaviourConfig config;
        private readonly IEnemyLineOfSight lineOfSight;
        private readonly IEnemyAttack straightAttack;
        private readonly IEnemyAttack parabolicAttack;
        private float repositionSearchCooldown;

        internal RangedBehaviourState State { get; private set; } = RangedBehaviourState.Idle;

        public RangedEnemyBehaviour(
            IEnemyDetection detection,
            IEnemyMovement movement,
            IRangedEnemyBehaviourConfig config,
            IEnemyLineOfSight lineOfSight,
            IEnemyAttack straightAttack,
            IEnemyAttack parabolicAttack)
        {
            this.detection = detection;
            this.movement = movement;
            this.config = config;
            this.lineOfSight = lineOfSight;
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

            repositionSearchCooldown = Mathf.Max(0f, repositionSearchCooldown - deltaTime);

            float distance = Vector3.Distance(movement.Position, target.Position);
            RangedBehaviourState previousState = State;
            RangedBehaviourState nextState = SelectState(distance, target.Position);
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

            if (nextState == RangedBehaviourState.Reposition)
            {
                RepositionForLineOfSight(target.Position, previousState);
                return;
            }

            movement.Stop();
            parabolicAttack?.TryAttack(target);
        }

        private RangedBehaviourState SelectState(float distance, Vector3 targetPosition)
        {
            if (State == RangedBehaviourState.Reposition)
            {
                if (distance > config.MaxRange)
                {
                    return RangedBehaviourState.Approach;
                }

                if (distance < config.MinRange)
                {
                    return RangedBehaviourState.Retreat;
                }

                return HasLineOfSight(targetPosition)
                    ? RangedBehaviourState.Hold
                    : RangedBehaviourState.Reposition;
            }

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

            return HasLineOfSight(targetPosition)
                ? RangedBehaviourState.Hold
                : RangedBehaviourState.Reposition;
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

        private bool HasLineOfSight(Vector3 targetPosition)
        {
            return lineOfSight == null || lineOfSight.HasLineOfSight(targetPosition);
        }

        private void RepositionForLineOfSight(Vector3 targetPosition, RangedBehaviourState previousState)
        {
            bool enteredReposition = previousState != RangedBehaviourState.Reposition;
            if (!enteredReposition && repositionSearchCooldown > 0f)
            {
                return;
            }

            repositionSearchCooldown = Mathf.Max(MinRepositionSearchInterval, config.RepositionSearchInterval);

            if (!TryFindRepositionPoint(targetPosition, out Vector3 repositionPoint))
            {
                movement.Stop();
                return;
            }

            movement.MoveTo(repositionPoint);
        }

        private bool TryFindRepositionPoint(Vector3 targetPosition, out Vector3 repositionPoint)
        {
            repositionPoint = default;
            float preferredDistance = (config.MinRange + config.MaxRange) * 0.5f;
            Vector3 baseDirection = movement.Position - targetPosition;
            baseDirection.y = 0f;
            if (baseDirection.sqrMagnitude <= MinDirectionSqrMagnitude)
            {
                baseDirection = Vector3.forward;
            }

            baseDirection.Normalize();
            float closestDistanceSqr = float.MaxValue;
            bool foundPoint = false;

            for (int i = 0; i < RepositionCandidateCount; i++)
            {
                Vector3 desiredPosition = targetPosition + GetRepositionDirection(baseDirection, i) * preferredDistance;
                if (!movement.TryResolvePosition(desiredPosition, out Vector3 validPosition)
                    || !IsInValidRange(Vector3.Distance(validPosition, targetPosition))
                    || lineOfSight == null
                    || !lineOfSight.HasLineOfSightFrom(validPosition, targetPosition))
                {
                    continue;
                }

                float distanceSqr = (validPosition - movement.Position).sqrMagnitude;
                if (distanceSqr >= closestDistanceSqr)
                {
                    continue;
                }

                closestDistanceSqr = distanceSqr;
                repositionPoint = validPosition;
                foundPoint = true;
            }

            return foundPoint;
        }

        private bool IsInValidRange(float distance)
        {
            return distance >= config.MinRange && distance <= config.MaxRange;
        }

        private static Vector3 GetRepositionDirection(Vector3 baseDirection, int candidateIndex)
        {
            int step = (candidateIndex + 1) / 2;
            int side = candidateIndex % 2 == 0 ? 1 : -1;
            float angle = FullCircleDegrees / RepositionCandidateCount * step * side;
            return Quaternion.AngleAxis(angle, Vector3.up) * baseDirection;
        }
    }
}
