using System.Collections.Generic;
using RPGame.Core.Damage;
using RPGame.Core.Spells;
using RPGame.Core.Statistics.Attributes;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    [CreateAssetMenu(fileName = "OrbitSpell", menuName = "RPGame/Spells/Orbit Spell")]
    public sealed class OrbitSpell : Spell, ICasterDamageRangeProvider
    {
        [SerializeField] private PartialDamageRange baseDamageRange = new(1f, 3f, DamageType.Magical, DamageElement.None);
        [SerializeField] private float powerDamageScaling;
        [SerializeField] private int projectileCount = 3;
        [SerializeField] private float orbitRadius = 2f;
        [SerializeField] private float angularSpeed = 90f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private float damageCapacity = 10f;

        public override SpellTags Tags => SpellTags.Projectile | SpellTags.Duration;

        public override void OnCast(CasterData casterData)
        {
            if (SpellPrefab == null)
            {
                Debug.LogWarning($"{name} cannot cast because spell prefab is missing.", this);
                return;
            }

            if (casterData.CasterObject == null)
            {
                Debug.LogWarning($"{name} cannot cast because caster object is missing.", this);
                return;
            }

            GameObject orbitObject = Instantiate(
                SpellPrefab,
                casterData.CasterObject.transform.position,
                Quaternion.identity);
            if (!orbitObject.TryGetComponent(out OrbitController orbitController))
            {
                Debug.LogWarning($"{name} spawned a prefab without {nameof(OrbitController)}.", orbitObject);
                Destroy(orbitObject);
                return;
            }

            orbitController.Initialize(
                casterData.CasterObject.transform,
                projectileCount,
                orbitRadius,
                angularSpeed,
                lifetime,
                damageCapacity,
                CreateOrbitCasterData(casterData));
        }

        public IReadOnlyList<PartialDamageRange> GetDamageRanges(CasterData casterData)
        {
            int power = casterData.Attributes != null ? casterData.Attributes.Power : 0;
            float powerDamageBonus = SpellDamageCalculator.CalculatePowerDamageBonus(power, powerDamageScaling);
            return new[]
            {
                new PartialDamageRange(
                    baseDamageRange.MinDamage + powerDamageBonus,
                    baseDamageRange.MaxDamage + powerDamageBonus,
                    baseDamageRange.DamageType,
                    baseDamageRange.DamageElement)
            };
        }

        private CasterData CreateOrbitCasterData(CasterData casterData)
        {
            return new CasterDataBuilder(casterData.CasterObject, casterData.CastOrigin, casterData.Target)
                .WithAttributes(casterData.Attributes)
                .WithStatistics(casterData.Statistics)
                .WithDamageRanges(GetDamageRanges(casterData))
                .Build();
        }

        private void OnValidate()
        {
            projectileCount = Mathf.Max(1, projectileCount);
            orbitRadius = Mathf.Max(0f, orbitRadius);
            lifetime = Mathf.Max(0f, lifetime);
            damageCapacity = Mathf.Max(0f, damageCapacity);
        }
    }
}
