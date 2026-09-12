using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    public abstract class ZoneSpell : Spell, IIndicatorSpell
    {
        [SerializeField] private float placementRange;
        [SerializeField] private float radius;
        [SerializeField] private float activationDelay;
        [SerializeField] private float activeDuration;

        public float PlacementRange => placementRange;
        public float PlacementRadius => radius;

        public override void OnCast(CasterData casterData)
        {
            if (!casterData.TargetPosition.HasValue)
            {
                Debug.LogWarning($"{name} cannot cast because target position is missing.", this);
                return;
            }

            if (SpellPrefab == null)
            {
                Debug.LogWarning($"{name} cannot cast because spell prefab is missing.", this);
                return;
            }

            GameObject zoneObject = Instantiate(SpellPrefab, casterData.TargetPosition.Value, Quaternion.identity);
            if (!zoneObject.TryGetComponent(out ZoneController zoneController))
            {
                Debug.LogWarning($"{name} spawned a prefab without {nameof(ZoneController)}.", zoneObject);
                Destroy(zoneObject);
                return;
            }

            zoneController.Initialize(casterData, radius, activationDelay, activeDuration);
        }

        private void OnValidate()
        {
            placementRange = Mathf.Max(0f, placementRange);
            radius = Mathf.Max(0f, radius);
            activationDelay = Mathf.Max(0f, activationDelay);
            activeDuration = Mathf.Max(0f, activeDuration);
        }
    }
}
