using System.Collections.Generic;
using NUnit.Framework;
using RPGame.Core.Damage;
using RPGame.Core.Spells;
using RPGame.Core.Statistics;
using UnityEngine;

namespace RPGame.Core.Tests
{
    public sealed class CasterDataTests
    {
        [Test]
        public void CasterData_WhenDamageRangesAreNotProvided_UsesEmptyDamageRanges()
        {
            CasterData casterData = new CasterData(null, null, null);

            Assert.IsNotNull(casterData.DamageRanges);
            Assert.AreEqual(0, casterData.DamageRanges.Count);
        }

        [Test]
        public void CasterDataBuilder_WithDamageRanges_BuildsCasterDataWithDamageRanges()
        {
            GameObject caster = new GameObject("CasterDataBuilderDamageRangesTests");
            List<PartialDamageRange> damageRanges = new()
            {
                new PartialDamageRange(5f, 9f, DamageType.Magical, DamageElement.Fire)
            };

            try
            {
                CasterData casterData = new CasterDataBuilder(caster, caster.transform, null)
                    .WithDamageRanges(damageRanges)
                    .Build();

                Assert.AreSame(caster, casterData.CasterObject);
                Assert.AreEqual(1, casterData.DamageRanges.Count);
                Assert.AreEqual(5f, casterData.DamageRanges[0].MinDamage);
                Assert.AreEqual(9f, casterData.DamageRanges[0].MaxDamage);
                Assert.AreEqual(DamageElement.Fire, casterData.DamageRanges[0].DamageElement);
            }
            finally
            {
                Object.DestroyImmediate(caster);
            }
        }

        [Test]
        public void CasterDataBuilder_WithStatistics_BuildsCasterDataWithStatistics()
        {
            TestStatisticsController statistics = new();

            CasterData casterData = new CasterDataBuilder(null, null, null)
                .WithStatistics(statistics)
                .Build();

            Assert.AreSame(statistics, casterData.Statistics);
        }

        private sealed class TestStatisticsController : IStatisticsController
        {
            public event System.Action<float, float> HealthChanged
            {
                add { }
                remove { }
            }

            public event System.Action<float, float> StaminaChanged
            {
                add { }
                remove { }
            }

            public event System.Action<float, float> OnManaChanged
            {
                add { }
                remove { }
            }

            public event System.Action Died
            {
                add { }
                remove { }
            }

            public float CurrentHealth => 0f;
            public float CurrentStamina => 0f;
            public float CurrentMana => 0f;
            public float MaxHealth => 0f;
            public float MaxStamina => 0f;
            public float MaxMana => 0f;
            public float HealthRegenerationPerSecond => 0f;
            public float StaminaRegenerationPerSecond => 0f;
            public float StaminaRegenerationDelay => 0f;
            public float ManaRegenerationPerSecond => 0f;
            public float ManaRegenerationDelay => 0f;
            public float HealthNormalized => 0f;
            public float StaminaNormalized => 0f;
            public float ManaNormalized => 0f;
            public bool IsAlive => true;

            public void TakeDamage(float amount)
            {
            }

            public void Heal(float amount)
            {
            }

            public bool CanSpendStamina(float amount)
            {
                return true;
            }

            public bool TrySpendStamina(float amount)
            {
                return true;
            }

            public void RestoreStamina(float amount)
            {
            }

            public bool CanSpendMana(float amount)
            {
                return true;
            }

            public bool TrySpendMana(float amount)
            {
                return true;
            }

            public void RestoreMana(float amount)
            {
            }
        }
    }
}
