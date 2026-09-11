using System.Collections;
using System.Collections.Generic;
using RPGame.Core.Effects;
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
        private CasterData casterData;
        private IZoneBehaviour zoneBehaviour;
        private Coroutine lifecycleRoutine;
        private bool isActive;
        private readonly Dictionary<IStatusEffectReceiver, TargetZoneState> targetsInside = new();

        private void Awake()
        {
            ResolveComponents();
        }

        public void Initialize(CasterData casterData, float radius, float activationDelay, float activeDuration)
        {
            this.casterData = casterData;
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
            }

            isActive = false;
            targetsInside.Clear();
            lifecycleRoutine = StartCoroutine(RunLifecycle(activationDelay, activeDuration));
        }

        private void Update()
        {
            if (!isActive || zoneBehaviour == null)
            {
                return;
            }

            ZoneStatusApplication statusApplication = zoneBehaviour.StatusApplication;
            if (!statusApplication.ReapplyWhileInside || statusApplication.ReapplyInterval <= 0f)
            {
                return;
            }

            foreach (TargetZoneState targetState in targetsInside.Values)
            {
                targetState.ReapplyTimer += Time.deltaTime;
                if (targetState.ReapplyTimer < statusApplication.ReapplyInterval)
                {
                    continue;
                }

                targetState.ReapplyTimer = 0f;
                ApplyStatusEffect(targetState.StatusEffectReceiver, statusApplication);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive || zoneBehaviour == null)
            {
                return;
            }

            IStatusEffectReceiver statusEffectReceiver = other.GetComponentInParent<IStatusEffectReceiver>();
            if (statusEffectReceiver == null)
            {
                return;
            }

            if (targetsInside.TryGetValue(statusEffectReceiver, out TargetZoneState targetState))
            {
                targetState.Colliders.Add(other);
                return;
            }

            targetState = new TargetZoneState(statusEffectReceiver);
            targetState.Colliders.Add(other);
            targetsInside.Add(statusEffectReceiver, targetState);
            ApplyStatusEffect(statusEffectReceiver, zoneBehaviour.StatusApplication);
        }

        private void OnTriggerExit(Collider other)
        {
            IStatusEffectReceiver statusEffectReceiver = other.GetComponentInParent<IStatusEffectReceiver>();
            if (statusEffectReceiver == null)
            {
                return;
            }

            if (!targetsInside.TryGetValue(statusEffectReceiver, out TargetZoneState targetState))
            {
                return;
            }

            targetState.Colliders.Remove(other);
            if (targetState.Colliders.Count == 0)
            {
                targetsInside.Remove(statusEffectReceiver);
            }
        }

        private IEnumerator RunLifecycle(float activationDelay, float activeDuration)
        {
            if (activationDelay > 0f)
            {
                yield return new WaitForSeconds(activationDelay);
            }

            zoneCollider.enabled = true;
            isActive = true;

            if (activeDuration > 0f)
            {
                yield return new WaitForSeconds(activeDuration);
            }

            Destroy(gameObject);
        }

        private void ApplyStatusEffect(IStatusEffectReceiver statusEffectReceiver, ZoneStatusApplication statusApplication)
        {
            if (statusApplication.Effect == null)
            {
                return;
            }

            statusEffectReceiver.ApplyStatusEffect(statusApplication.Effect, statusApplication.Duration);
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

        private sealed class TargetZoneState
        {
            public TargetZoneState(IStatusEffectReceiver statusEffectReceiver)
            {
                StatusEffectReceiver = statusEffectReceiver;
            }

            public IStatusEffectReceiver StatusEffectReceiver { get; }
            public HashSet<Collider> Colliders { get; } = new();
            public float ReapplyTimer { get; set; }
        }
    }
}
