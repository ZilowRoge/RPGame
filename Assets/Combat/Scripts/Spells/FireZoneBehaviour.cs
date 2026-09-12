using System.Collections.Generic;
using RPGame.Core.Effects;
using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class FireZoneBehaviour : MonoBehaviour, IZoneBehaviour
    {
        [SerializeField] private BurningEffectDefinition burningEffect;
        [SerializeField] private float burningDuration = 3f;
        [SerializeField] private float reapplyInterval = 0.5f;

        private readonly Dictionary<IStatusEffectReceiver, TargetZoneState> targetsInside = new();
        private SphereCollider zoneCollider;
        private Rigidbody zoneRigidbody;
        private CasterData casterData;
        private bool isActive;

        private void Awake()
        {
            ResolveComponents();
        }

        public void Initialize(CasterData casterData, float radius)
        {
            this.casterData = casterData;
            ResolveComponents();

            zoneCollider.isTrigger = true;
            zoneCollider.radius = Mathf.Max(0f, radius);
            zoneCollider.enabled = false;

            zoneRigidbody.useGravity = false;
            zoneRigidbody.isKinematic = true;

            isActive = false;
            targetsInside.Clear();
        }

        public void Activate()
        {
            zoneCollider.enabled = true;
            isActive = true;
        }

        public void Deactivate()
        {
            isActive = false;
            if (zoneCollider != null)
            {
                zoneCollider.enabled = false;
            }

            targetsInside.Clear();
        }

        private void Update()
        {
            if (!isActive || reapplyInterval <= 0f)
            {
                return;
            }

            foreach (TargetZoneState targetState in targetsInside.Values)
            {
                targetState.ReapplyTimer += Time.deltaTime;
                if (targetState.ReapplyTimer < reapplyInterval)
                {
                    continue;
                }

                targetState.ReapplyTimer = 0f;
                ApplyBurning(targetState);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive || IsCasterCollider(other))
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
            ApplyBurning(targetState);
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

        private void ApplyBurning(TargetZoneState targetState)
        {
            if (burningEffect != null)
            {
                targetState.StatusEffectReceiver.ApplyStatusEffect(
                    burningEffect,
                    burningDuration);
            }
        }

        private bool IsCasterCollider(Collider other)
        {
            GameObject casterObject = casterData.CasterObject;
            return casterObject != null
                && other != null
                && (other.gameObject == casterObject || other.transform.IsChildOf(casterObject.transform));
        }

        private void OnValidate()
        {
            burningDuration = Mathf.Max(0f, burningDuration);
            reapplyInterval = Mathf.Max(0f, reapplyInterval);
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
