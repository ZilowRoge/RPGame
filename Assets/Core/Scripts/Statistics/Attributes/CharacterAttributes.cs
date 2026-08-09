using System;
using System.Collections.Generic;
using RPGame.Core.Effects;
using UnityEngine;

namespace RPGame.Core.Statistics.Attributes
{
    public sealed class CharacterAttributes : MonoBehaviour, ICharacterAttributes
    {
        [SerializeField] private CharacterAttributesConfig config;
        [SerializeField] private EffectAggregator effectAggregator;

        private readonly Dictionary<CharacterAttributeType, int> purchasedPoints = new();

        public event Action ValuesChanged;

        public int Strength => GetValue(CharacterAttributeType.Strength);
        public int Dexterity => GetValue(CharacterAttributeType.Dexterity);
        public int Endurance => GetValue(CharacterAttributeType.Endurance);
        public int Vitality => GetValue(CharacterAttributeType.Vitality);
        public int Intelligence => GetValue(CharacterAttributeType.Intelligence);
        public int Power => GetValue(CharacterAttributeType.Power);

        public int GetValue(CharacterAttributeType attributeType)
        {
            ValidateConfig();
            return config.GetValue(attributeType) + GetPurchasedPoints(attributeType) + GetFlatBonus(attributeType);
        }

        public int GetBaseValue(CharacterAttributeType attributeType)
        {
            ValidateConfig();
            return config.GetValue(attributeType);
        }

        public int GetPurchasedPoints(CharacterAttributeType attributeType)
        {
            return purchasedPoints.TryGetValue(attributeType, out int points) ? points : 0;
        }

        internal void AddPurchasedPoints(CharacterAttributeType attributeType, int points)
        {
            if (points <= 0)
            {
                return;
            }

            purchasedPoints[attributeType] = GetPurchasedPoints(attributeType) + points;
            ValuesChanged?.Invoke();
        }

        private void ValidateConfig()
        {
            if (config == null)
            {
                throw new InvalidOperationException($"{nameof(CharacterAttributes)} on '{name}' requires a {nameof(CharacterAttributesConfig)}.");
            }
        }

        private int GetFlatBonus(CharacterAttributeType attributeType)
        {
            ResolveEffectAggregator();
            return effectAggregator != null
                ? Mathf.RoundToInt(effectAggregator.GetEffectValue(GetEffectStat(attributeType), EffectModifierType.Flat))
                : 0;
        }

        private void ResolveEffectAggregator()
        {
            if (effectAggregator == null)
            {
                TryGetComponent(out effectAggregator);
            }
        }

        private static EffectStat GetEffectStat(CharacterAttributeType attributeType)
        {
            return attributeType switch
            {
                CharacterAttributeType.Strength => EffectStat.Strength,
                CharacterAttributeType.Dexterity => EffectStat.Dexterity,
                CharacterAttributeType.Endurance => EffectStat.Endurance,
                CharacterAttributeType.Vitality => EffectStat.Vitality,
                CharacterAttributeType.Intelligence => EffectStat.Intelligence,
                CharacterAttributeType.Power => EffectStat.Power,
                _ => throw new ArgumentOutOfRangeException(nameof(attributeType), attributeType, null)
            };
        }
    }
}
