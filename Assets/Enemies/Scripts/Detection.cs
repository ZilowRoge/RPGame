using System.Collections;
using RPGame.Core.Targeting;
using UnityEngine;

namespace RPGame.Enemies
{
    public sealed class Detection : MonoBehaviour, IEnemyDetection
    {
        [SerializeField] private float detectionRange = 6f;
        [SerializeField] private float detectionInterval = 0.2f;

        private Coroutine detectionCoroutine;

        internal ITargetable CurrentTarget { get; private set; }
        internal Transform CurrentTargetPoint => CurrentTarget?.TargetPoint;
        internal bool HasTarget => CurrentTargetPoint != null;

        private void OnEnable()
        {
            RefreshDetection();

            if (Application.isPlaying)
            {
                detectionCoroutine = StartCoroutine(DetectionRoutine());
            }
        }

        private void OnDisable()
        {
            if (detectionCoroutine != null)
            {
                StopCoroutine(detectionCoroutine);
                detectionCoroutine = null;
            }

            ClearCurrentTarget();
        }

        internal void ClearCurrentTarget()
        {
            CurrentTarget = null;
        }

        internal void RefreshDetection()
        {
            CurrentTarget = TargetSelector.SelectNearest(
                TargetRegistry.PlayerTargets,
                transform,
                detectionRange);
        }

        bool IEnemyDetection.TryGetTarget(out SelectedTarget target)
        {
            target = default;
            if (!HasTarget)
            {
                return false;
            }

            target = new SelectedTarget(CurrentTarget, CurrentTargetPoint.position);
            return true;
        }

        private IEnumerator DetectionRoutine()
        {
            WaitForSeconds wait = new(detectionInterval);

            while (true)
            {
                yield return wait;
                RefreshDetection();
            }
        }

        private void OnValidate()
        {
            detectionRange = Mathf.Max(0f, detectionRange);
            detectionInterval = Mathf.Max(0.02f, detectionInterval);
        }
    }
}
