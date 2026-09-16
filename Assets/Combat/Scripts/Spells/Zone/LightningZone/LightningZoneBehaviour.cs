using System.Collections;
using System.Collections.Generic;
using RPGame.Core.Effects;
using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    public sealed class LightningZoneBehaviour : MonoBehaviour, IZoneBehaviour
    {
        [SerializeField] private StunEffectDefinition stunEffect;
        [SerializeField] private float stunDuration = 2f;
        [SerializeField] private GameObject lightningVfxPrefab;
        [SerializeField] private Vector3 lightningVfxOffset;
        [SerializeField] private Vector2 strikeDelayRange = new(0f, 0.2f);

        private readonly HashSet<IStatusEffectReceiver> hitReceivers = new();
        private CasterData casterData;
        private float radius;

        public void Initialize(CasterData casterData, float radius)
        {
            this.casterData = casterData;
            this.radius = Mathf.Max(0f, radius);
            hitReceivers.Clear();
        }

        public void Activate()
        {
            hitReceivers.Clear();
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider zoneCollider = colliders[i];
                if (IsCasterCollider(zoneCollider))
                {
                    continue;
                }

                IStatusEffectReceiver statusEffectReceiver =
                    zoneCollider.GetComponentInParent<IStatusEffectReceiver>();
                if (statusEffectReceiver == null || !hitReceivers.Add(statusEffectReceiver))
                {
                    continue;
                }

                float delay = Random.Range(strikeDelayRange.x, strikeDelayRange.y);
                DelayedLightningStrike.Run(
                    statusEffectReceiver,
                    stunEffect,
                    stunDuration,
                    lightningVfxPrefab,
                    zoneCollider.bounds.center + lightningVfxOffset,
                    delay);
            }
        }

        public void Deactivate()
        {
            hitReceivers.Clear();
        }

        private bool IsCasterCollider(Collider zoneCollider)
        {
            GameObject casterObject = casterData.CasterObject;
            return casterObject != null
                && zoneCollider != null
                && (zoneCollider.gameObject == casterObject
                    || zoneCollider.transform.IsChildOf(casterObject.transform));
        }

        private void OnValidate()
        {
            stunDuration = Mathf.Max(0f, stunDuration);
            strikeDelayRange.x = Mathf.Max(0f, strikeDelayRange.x);
            strikeDelayRange.y = Mathf.Max(strikeDelayRange.x, strikeDelayRange.y);
        }
    }

    internal sealed class DelayedLightningStrike : MonoBehaviour
    {
        private IStatusEffectReceiver statusEffectReceiver;
        private StunEffectDefinition stunEffect;
        private float stunDuration;
        private GameObject lightningVfxPrefab;
        private Vector3 strikePosition;
        private float delay;

        public static void Run(
            IStatusEffectReceiver statusEffectReceiver,
            StunEffectDefinition stunEffect,
            float stunDuration,
            GameObject lightningVfxPrefab,
            Vector3 strikePosition,
            float delay)
        {
            GameObject runnerObject = new($"{nameof(DelayedLightningStrike)}");
            DelayedLightningStrike runner = runnerObject.AddComponent<DelayedLightningStrike>();
            runner.statusEffectReceiver = statusEffectReceiver;
            runner.stunEffect = stunEffect;
            runner.stunDuration = stunDuration;
            runner.lightningVfxPrefab = lightningVfxPrefab;
            runner.strikePosition = strikePosition;
            runner.delay = Mathf.Max(0f, delay);
        }

        private IEnumerator Start()
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (stunEffect != null)
            {
                statusEffectReceiver?.ApplyStatusEffect(stunEffect, stunDuration);
            }

            if (lightningVfxPrefab != null)
            {
                Instantiate(lightningVfxPrefab, strikePosition, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
