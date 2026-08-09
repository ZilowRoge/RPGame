using System.Collections.Generic;
using RPGame.Core.Damage;
using RPGame.Core.Progression;
using RPGame.Core.Spells;
using RPGame.Core.Statistics.Attributes;
using RPGame.Core.Statistics.CombatStats;
using UnityEngine;

namespace RPGame.Core.Statistics
{
    public sealed class CharacterStatisticsDataProvider : StatisticsDataProviderBase
    {
        [SerializeField] private StatisticsController statisticsController;
        [SerializeField] private CharacterAttributes characterAttributes;
        [SerializeField] private CombatStatsProvider weaponDamageProvider;

        private readonly List<DataEntry> entries = new();
        private ILastUsedSpellDamageRangeProvider lastUsedSpellDamageRangeProvider;
        private IExperienceProvider experienceProvider;
        private IStatisticsController statistics;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        public override IReadOnlyList<DataEntry> GetStatistics()
        {
            ResolveReferences();
            entries.Clear();

            AddVitals();
            AddAvailableExperience();
            AddLastSpellDamage();
            AddWeaponDamage();

            return entries;
        }

        public void SetStatisticsController(StatisticsController controller)
        {
            if (statisticsController == controller)
            {
                return;
            }

            UnsubscribeStatistics();
            statisticsController = controller;
            statistics = statisticsController;
            SubscribeStatistics();
            NotifyChanged();
        }

        public void SetCharacterAttributes(CharacterAttributes attributes)
        {
            if (characterAttributes == attributes)
            {
                return;
            }

            UnsubscribeAttributes();
            characterAttributes = attributes;
            SubscribeAttributes();
            NotifyChanged();
        }

        public void SetWeaponDamageProvider(CombatStatsProvider provider)
        {
            if (weaponDamageProvider == provider)
            {
                return;
            }

            UnsubscribeWeaponDamageProvider();
            weaponDamageProvider = provider;
            SubscribeWeaponDamageProvider();
            NotifyChanged();
        }

        public void SetLastUsedSpellDamageRangeProvider(ILastUsedSpellDamageRangeProvider provider)
        {
            if (lastUsedSpellDamageRangeProvider == provider)
            {
                return;
            }

            UnsubscribeLastUsedSpellDamageRangeProvider();
            lastUsedSpellDamageRangeProvider = provider;
            SubscribeLastUsedSpellDamageRangeProvider();
            NotifyChanged();
        }

        public void SetExperienceProvider(IExperienceProvider provider)
        {
            if (experienceProvider == provider)
            {
                return;
            }

            UnsubscribeExperienceProvider();
            experienceProvider = provider;
            SubscribeExperienceProvider();
            NotifyChanged();
        }

        private void ResolveReferences()
        {
            if (statisticsController == null)
            {
                statisticsController = GetComponentInParent<StatisticsController>();
            }

            if (characterAttributes == null)
            {
                characterAttributes = GetComponentInParent<CharacterAttributes>();
            }

            if (weaponDamageProvider == null)
            {
                weaponDamageProvider = GetComponentInParent<CombatStatsProvider>();
            }

            if (lastUsedSpellDamageRangeProvider == null)
            {
                lastUsedSpellDamageRangeProvider = ResolveLastUsedSpellDamageRangeProvider();
            }

            if (experienceProvider == null)
            {
                experienceProvider = ResolveExperienceProvider();
            }

            statistics = statisticsController;
        }

        private void Subscribe()
        {
            SubscribeStatistics();
            SubscribeAttributes();
            SubscribeWeaponDamageProvider();
            SubscribeLastUsedSpellDamageRangeProvider();
            SubscribeExperienceProvider();
        }

        private void Unsubscribe()
        {
            UnsubscribeStatistics();
            UnsubscribeAttributes();
            UnsubscribeWeaponDamageProvider();
            UnsubscribeLastUsedSpellDamageRangeProvider();
            UnsubscribeExperienceProvider();
        }

        private void SubscribeStatistics()
        {
            if (statistics == null)
            {
                return;
            }

            statistics.HealthChanged -= OnStatisticsChanged;
            statistics.StaminaChanged -= OnStatisticsChanged;
            statistics.OnManaChanged -= OnStatisticsChanged;
            statistics.HealthChanged += OnStatisticsChanged;
            statistics.StaminaChanged += OnStatisticsChanged;
            statistics.OnManaChanged += OnStatisticsChanged;
        }

        private void UnsubscribeStatistics()
        {
            if (statistics == null)
            {
                return;
            }

            statistics.HealthChanged -= OnStatisticsChanged;
            statistics.StaminaChanged -= OnStatisticsChanged;
            statistics.OnManaChanged -= OnStatisticsChanged;
        }

        private void SubscribeAttributes()
        {
            if (characterAttributes != null)
            {
                characterAttributes.ValuesChanged -= OnAttributesChanged;
                characterAttributes.ValuesChanged += OnAttributesChanged;
            }
        }

        private void UnsubscribeAttributes()
        {
            if (characterAttributes != null)
            {
                characterAttributes.ValuesChanged -= OnAttributesChanged;
            }
        }

        private void SubscribeWeaponDamageProvider()
        {
            if (weaponDamageProvider != null)
            {
                weaponDamageProvider.Changed -= OnWeaponDamageProviderChanged;
                weaponDamageProvider.Changed += OnWeaponDamageProviderChanged;
            }
        }

        private void UnsubscribeWeaponDamageProvider()
        {
            if (weaponDamageProvider != null)
            {
                weaponDamageProvider.Changed -= OnWeaponDamageProviderChanged;
            }
        }

        private void SubscribeLastUsedSpellDamageRangeProvider()
        {
            if (lastUsedSpellDamageRangeProvider != null)
            {
                lastUsedSpellDamageRangeProvider.LastUsedSpellDamageRangeChanged -= OnLastUsedSpellChanged;
                lastUsedSpellDamageRangeProvider.LastUsedSpellDamageRangeChanged += OnLastUsedSpellChanged;
            }
        }

        private void UnsubscribeLastUsedSpellDamageRangeProvider()
        {
            if (lastUsedSpellDamageRangeProvider != null)
            {
                lastUsedSpellDamageRangeProvider.LastUsedSpellDamageRangeChanged -= OnLastUsedSpellChanged;
            }
        }

        private void SubscribeExperienceProvider()
        {
            if (experienceProvider != null)
            {
                experienceProvider.AvailableExperienceChanged -= OnAvailableExperienceChanged;
                experienceProvider.AvailableExperienceChanged += OnAvailableExperienceChanged;
            }
        }

        private void UnsubscribeExperienceProvider()
        {
            if (experienceProvider != null)
            {
                experienceProvider.AvailableExperienceChanged -= OnAvailableExperienceChanged;
            }
        }

        private void OnStatisticsChanged(float currentValue, float maxValue)
        {
            NotifyChanged();
        }

        private void OnAttributesChanged()
        {
            NotifyChanged();
        }

        private void OnWeaponDamageProviderChanged()
        {
            NotifyChanged();
        }

        private void OnLastUsedSpellChanged()
        {
            NotifyChanged();
        }

        private void OnAvailableExperienceChanged()
        {
            NotifyChanged();
        }

        private void AddVitals()
        {
            if (statistics == null)
            {
                return;
            }

            entries.Add(new DataEntry(
                RecordId.Health,
                FormatCurrentAndMax(statistics.CurrentHealth, statistics.MaxHealth)));
            entries.Add(new DataEntry(
                RecordId.Mana,
                FormatCurrentAndMax(statistics.CurrentMana, statistics.MaxMana)));
            entries.Add(new DataEntry(
                RecordId.Stamina,
                FormatCurrentAndMax(statistics.CurrentStamina, statistics.MaxStamina)));
            entries.Add(new DataEntry(
                RecordId.HealthRegeneration,
                FormatPerSecond(statistics.HealthRegenerationPerSecond)));
            entries.Add(new DataEntry(
                RecordId.ManaRegeneration,
                FormatPerSecond(statistics.ManaRegenerationPerSecond)));
            entries.Add(new DataEntry(
                RecordId.StaminaRegeneration,
                FormatPerSecond(statistics.StaminaRegenerationPerSecond)));
        }

        private void AddLastSpellDamage()
        {
            if (lastUsedSpellDamageRangeProvider == null
                || !lastUsedSpellDamageRangeProvider.TryGetLastUsedSpellDamageRanges(out IReadOnlyList<PartialDamageRange> damageRanges))
            {
                return;
            }

            if (TrySumDamageRanges(damageRanges, out float minDamage, out float maxDamage))
            {
                entries.Add(new DataEntry(
                    RecordId.LastSpellDamage,
                    FormatRange(minDamage, maxDamage)));
            }
        }

        private void AddAvailableExperience()
        {
            if (experienceProvider == null)
            {
                return;
            }

            entries.Add(new DataEntry(
                RecordId.AvailableExperience,
                FormatInteger(experienceProvider.AvailableExperience)));
        }

        private void AddWeaponDamage()
        {
            if (weaponDamageProvider == null)
            {
                return;
            }

            if (TrySumDamageRanges(weaponDamageProvider.GetDamageRanges(), out float minDamage, out float maxDamage))
            {
                entries.Add(new DataEntry(
                    RecordId.WeaponDamage,
                    FormatRange(minDamage, maxDamage)));
            }
        }

        private ILastUsedSpellDamageRangeProvider ResolveLastUsedSpellDamageRangeProvider()
        {
            MonoBehaviour[] behaviours = GetComponentsInParent<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is ILastUsedSpellDamageRangeProvider provider)
                {
                    return provider;
                }
            }

            return null;
        }

        private IExperienceProvider ResolveExperienceProvider()
        {
            MonoBehaviour[] behaviours = GetComponentsInParent<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IExperienceProvider provider)
                {
                    return provider;
                }
            }

            return null;
        }

        private static bool TrySumDamageRanges(
            IReadOnlyList<PartialDamageRange> damageRanges,
            out float minDamage,
            out float maxDamage)
        {
            minDamage = 0f;
            maxDamage = 0f;

            if (damageRanges == null || damageRanges.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < damageRanges.Count; i++)
            {
                minDamage += damageRanges[i].MinDamage;
                maxDamage += damageRanges[i].MaxDamage;
            }

            return maxDamage > 0f;
        }

        private static string FormatCurrentAndMax(float currentValue, float maxValue)
        {
            return ValueFactory.CurrentAndMax(currentValue, maxValue).Format(Format.CurrentAndMax);
        }

        private static string FormatRange(float minValue, float maxValue)
        {
            return ValueFactory.Range(minValue, maxValue).Format(Format.Range());
        }

        private static string FormatPerSecond(float value)
        {
            return ValueFactory.Single(value).Format(Format.PerSecond());
        }

        private static string FormatInteger(float value)
        {
            return ValueFactory.Single(value).Format(Format.Integer);
        }
    }
}
