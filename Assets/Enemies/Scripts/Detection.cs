using System.Collections;
using RPGame.Core.Targeting;
using UnityEngine;

namespace RPGame.Enemies
{
    public sealed class Detection : MonoBehaviour
    {
        [SerializeField] private float detectionRange = 6f;
        [SerializeField] private float detectionInterval = 0.2f;

        private Coroutine detectionCoroutine;

        public float DetectionRange => detectionRange;
        public ITargetable CurrentTarget { get; private set; }
        public Transform CurrentTargetPoint => CurrentTarget?.TargetPoint;
        public bool HasTarget => CurrentTargetPoint != null;

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

            CurrentTarget = null;
        }

        public void RefreshDetection()
        {
            CurrentTarget = TargetSelector.SelectNearest(
                TargetRegistry.PlayerTargets,
                transform,
                detectionRange);
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
