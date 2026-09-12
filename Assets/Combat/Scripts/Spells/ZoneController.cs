using System.Collections;
using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ZoneController : MonoBehaviour
    {
        private SphereCollider zoneCollider;
        private Rigidbody zoneRigidbody;
        private IZoneBehaviour zoneBehaviour;
        private Coroutine lifecycleRoutine;

        private void Awake()
        {
            ResolveComponents();
        }

        public void Initialize(CasterData casterData, float radius, float activationDelay, float activeDuration)
        {
            ResolveComponents();
            ResolveZoneBehaviour();

            zoneCollider.isTrigger = true;
            zoneCollider.radius = radius;
            zoneCollider.enabled = false;

            zoneRigidbody.useGravity = false;
            zoneRigidbody.isKinematic = true;

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

            zoneCollider.enabled = true;
            zoneBehaviour?.Activate();

            if (activeDuration > 0f)
            {
                yield return new WaitForSeconds(activeDuration);
            }

            zoneBehaviour?.Deactivate();
            zoneCollider.enabled = false;
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

        private void ResolveComponents()
        {
            if (zoneCollider == null)
            {
                zoneCollider = GetComponent<SphereCollider>();
            }

            if (zoneRigidbody == null)
            {
                zoneRigidbody = GetComponent<Rigidbody>();
            }
        }
    }
}
