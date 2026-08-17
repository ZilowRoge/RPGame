using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RPGame.Combat.Spells;
using RPGame.Core.Damage;
using RPGame.Core.Spells;
using RPGame.Core.Spells.Symbols;
using RPGame.Core.Statistics;
using RPGame.Core.Targeting;
using RPGame.Player.Spells;
using TargetingController = RPGame.Player.Targeting.TargetingController;
using UnityEngine;

namespace RPGame.Player.Tests
{
    public sealed class CastControllerTests
    {
        private GameObject playerObject;
        private TargetingController targeting;
        private CastController controller;
        private CaptureCasterDataSpell spell;

        [SetUp]
        public void SetUp()
        {
            playerObject = new GameObject("CastControllerTests");
            targeting = playerObject.AddComponent<TargetingController>();
            controller = playerObject.AddComponent<CastController>();
            spell = ScriptableObject.CreateInstance<CaptureCasterDataSpell>();

            SetField(controller, "targeting", targeting);
            SetField(controller, "casterObject", playerObject);
            SetField(controller, "castOrigin", playerObject.transform);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(spell);
        }

        [Test]
        public void CastSpell_BuildsCasterDataWithCurrentTarget()
        {
            GameObject targetObject = new("Target");
            try
            {
                SetCurrentTarget(new TestTargetable(targetObject.transform));

                InvokeCastSpell(spell);

                Assert.AreSame(targetObject.transform, spell.LastCasterData.Target);
            }
            finally
            {
                Object.DestroyImmediate(targetObject);
            }
        }

        [Test]
        public void CastSpell_WithNoCurrentTarget_BuildsCasterDataWithNullTarget()
        {
            SetCurrentTarget(null);

            InvokeCastSpell(spell);

            Assert.IsNull(spell.LastCasterData.Target);
        }

        [Test]
        public void CreateCasterData_TargetIsSnapshotOfCurrentCast()
        {
            GameObject firstTargetObject = new("FirstTarget");
            GameObject secondTargetObject = new("SecondTarget");
            try
            {
                SetCurrentTarget(new TestTargetable(firstTargetObject.transform));

                CasterData casterData = InvokeCreateCasterData();
                SetCurrentTarget(new TestTargetable(secondTargetObject.transform));

                Assert.AreSame(firstTargetObject.transform, casterData.Target);
            }
            finally
            {
                Object.DestroyImmediate(firstTargetObject);
                Object.DestroyImmediate(secondTargetObject);
            }
        }

        [Test]
        public void CreateCasterData_IncludesPlayerStatistics()
        {
            StatisticsController statisticsController = playerObject.AddComponent<StatisticsController>();

            CasterData casterData = InvokeCreateCasterData();

            Assert.AreSame(statisticsController, casterData.Statistics);
        }

        [Test]
        public void SpellSymbolCaster_DoesNotDependOnTargeting()
        {
            FieldInfo[] fields = typeof(SpellSymbolCaster).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int i = 0; i < fields.Length; i++)
            {
                Assert.AreNotSame(typeof(TargetingController), fields[i].FieldType);
            }
        }

        [Test]
        public void SpellSymbolCaster_EmitsSelectedSpell()
        {
            SpellSymbolCaster symbolCaster = playerObject.AddComponent<SpellSymbolCaster>();
            SetSpellSymbolEntries(symbolCaster, symbolId: 3, spell);
            Spell selectedSpell = null;
            symbolCaster.SpellSelected += emittedSpell => selectedSpell = emittedSpell;

            symbolCaster.ReceiveSymbol(new SymbolRecognitionResult(3, 1f));

            Assert.AreSame(spell, selectedSpell);
        }

        [Test]
        public void CastController_CastsSpellSelectedBySpellSymbolCaster()
        {
            SpellSymbolCaster symbolCaster = playerObject.AddComponent<SpellSymbolCaster>();
            SetSpellSymbolEntries(symbolCaster, symbolId: 5, spell);
            SetField(controller, "spellSymbolCaster", symbolCaster);

            InvokeLifecycle("OnEnable");
            symbolCaster.ReceiveSymbol(new SymbolRecognitionResult(5, 1f));
            InvokeLifecycle("OnDisable");

            Assert.AreSame(playerObject, spell.LastCasterData.CasterObject);
        }

        [Test]
        public void CastSpell_WhenSuccessful_UpdatesLastUsedSpellDamageRanges()
        {
            DamageRangeSpell damageRangeSpell = ScriptableObject.CreateInstance<DamageRangeSpell>();
            try
            {
                InvokeCastSpell(damageRangeSpell);

                Assert.IsTrue(controller.TryGetLastUsedSpellDamageRanges(out IReadOnlyList<PartialDamageRange> damageRanges));
                Assert.AreEqual(1, damageRanges.Count);
                Assert.AreEqual(3f, damageRanges[0].MinDamage);
                Assert.AreEqual(7f, damageRanges[0].MaxDamage);
            }
            finally
            {
                Object.DestroyImmediate(damageRangeSpell);
            }
        }

        [Test]
        public void CastSpell_WhenLastUsedSpellChanges_RaisesDamageRangeChanged()
        {
            int changedCount = 0;
            controller.LastUsedSpellDamageRangeChanged += () => changedCount++;

            InvokeCastSpell(spell);

            Assert.AreEqual(1, changedCount);
        }

        private void InvokeCastSpell(Spell selectedSpell)
        {
            MethodInfo method = typeof(CastController).GetMethod("CastSpell", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(controller, new object[] { selectedSpell });
        }

        private CasterData InvokeCreateCasterData()
        {
            MethodInfo method = typeof(CastController).GetMethod("CreateCasterData", BindingFlags.Instance | BindingFlags.NonPublic);
            return (CasterData)method.Invoke(controller, null);
        }

        private void InvokeLifecycle(string methodName)
        {
            MethodInfo method = typeof(CastController).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(controller, null);
        }

        private void SetCurrentTarget(ITargetable target)
        {
            FieldInfo field = typeof(TargetingController).GetField("<CurrentTarget>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(targeting, target);
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(target, value);
        }

        private static void SetSpellSymbolEntries(SpellSymbolCaster symbolCaster, int symbolId, Spell configuredSpell)
        {
            System.Type entryType = typeof(SpellSymbolCaster).GetNestedType("SpellSymbolEntry", BindingFlags.NonPublic);
            object entry = System.Activator.CreateInstance(entryType, nonPublic: true);
            SetField(entry, "symbolId", symbolId);
            SetField(entry, "spell", configuredSpell);

            System.Array entries = System.Array.CreateInstance(entryType, 1);
            entries.SetValue(entry, 0);
            SetField(symbolCaster, "spellsBySymbol", entries);
        }

        private sealed class TestTargetable : ITargetable
        {
            public TestTargetable(Transform targetPoint)
            {
                TargetPoint = targetPoint;
            }

            public Transform TargetPoint { get; }
        }

        private sealed class CaptureCasterDataSpell : Spell
        {
            public CasterData LastCasterData { get; private set; }

            public override void OnCast(CasterData casterData)
            {
                LastCasterData = casterData;
            }
        }

        private sealed class DamageRangeSpell : Spell, ICasterDamageRangeProvider
        {
            private static readonly IReadOnlyList<PartialDamageRange> DamageRanges = new[]
            {
                new PartialDamageRange(3f, 7f, DamageType.Magical, DamageElement.Fire)
            };

            public override void OnCast(CasterData casterData)
            {
            }

            public IReadOnlyList<PartialDamageRange> GetDamageRanges(CasterData casterData)
            {
                return DamageRanges;
            }
        }
    }
}

