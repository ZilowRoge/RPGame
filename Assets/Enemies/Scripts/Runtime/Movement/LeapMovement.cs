using System;
using UnityEngine;

namespace RPGame.Enemies
{
    internal sealed class LeapMovement
    {
        private readonly Transform transform;
        private readonly Func<MovementAgentState> beginMovement;
        private readonly Action<MovementAgentState, Vector3, bool> finishMovement;

        private bool isActive;
        private bool traversesOffMeshLink;
        private Vector3 startPosition;
        private Vector3 landingPoint;
        private float duration;
        private float elapsed;
        private float arcHeight;
        private MovementAgentState agentState;

        public bool IsActive => isActive;
        public bool TraversesOffMeshLink => traversesOffMeshLink;

        public LeapMovement(
            Transform transform,
            Func<MovementAgentState> beginMovement,
            Action<MovementAgentState, Vector3, bool> finishMovement)
        {
            this.transform = transform;
            this.beginMovement = beginMovement;
            this.finishMovement = finishMovement;
        }

        public void Start(Vector3 destination, float speed, float arcHeight, bool traversesOffMeshLink)
        {
            startPosition = transform.position;
            landingPoint = destination;
            duration = Vector3.Distance(startPosition, landingPoint) / speed;
            elapsed = 0f;
            this.arcHeight = arcHeight;
            this.traversesOffMeshLink = traversesOffMeshLink;
            agentState = beginMovement();
            isActive = true;
        }

        public void Tick(float deltaTime)
        {
            if (!isActive)
            {
                return;
            }

            if (elapsed >= duration)
            {
                Finish(landingPoint, true);
                return;
            }

            elapsed += deltaTime;
            float progress = duration > Mathf.Epsilon ? Mathf.Clamp01(elapsed / duration) : 1f;
            transform.position = Vector3.Lerp(startPosition, landingPoint, progress)
                + Vector3.up * Mathf.Sin(Mathf.PI * progress) * arcHeight;
        }

        public void Cancel()
        {
            if (!isActive)
            {
                return;
            }

            Finish(startPosition, false);
        }

        private void Finish(Vector3 position, bool completeTraversal)
        {
            bool shouldCompleteTraversal = completeTraversal && traversesOffMeshLink;
            isActive = false;
            traversesOffMeshLink = false;
            finishMovement(agentState, position, shouldCompleteTraversal);
        }
    }
}
