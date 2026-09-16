using System.Collections;
using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    public sealed class ZoneController : MonoBehaviour
    {
        private IZoneBehaviour zoneBehaviour;
        private Coroutine lifecycleRoutine;

        public void Initialize(CasterData casterData, float radius, float activationDelay, float activeDuration)
        {
            ResolveZoneBehaviour();

            if (lifecycleRoutine != null)
            {
                StopCoroutine(lifecycleRoutine);
                zoneBehaviour?.Deactivate();
            }

            zoneBehaviour?.Initialize(casterData, radius);
            lifecycleRoutine = StartCoroutine(RunLifecycle(activationDelay, activeDuration));
        }

        private IEnumerator RunLifecycle(float activationDelay, float activeDuration)
        {
            if (activationDelay > 0f)
            {
                yield return new WaitForSeconds(activationDelay);
            }

            zoneBehaviour?.Activate();

            if (activeDuration > 0f)
            {
                yield return new WaitForSeconds(activeDuration);
            }

            zoneBehaviour?.Deactivate();
            Destroy(gameObject);
        }

        private void ResolveZoneBehaviour()
        {
            zoneBehaviour = null;
            int behaviourCount = 0;
            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is not IZoneBehaviour behaviour)
                {
                    continue;
                }

                zoneBehaviour = behaviour;
                behaviourCount++;
            }

            if (behaviourCount != 1)
            {
                Debug.LogWarning($"{name} requires exactly one {nameof(IZoneBehaviour)} component.", this);
                zoneBehaviour = null;
            }
        }

    }
}
