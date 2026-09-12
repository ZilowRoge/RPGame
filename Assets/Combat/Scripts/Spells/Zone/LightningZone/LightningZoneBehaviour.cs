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
            if (stunEffect == null)
            {
                return;
            }

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

                statusEffectReceiver.ApplyStatusEffect(stunEffect, stunDuration);
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
        }
    }
}
