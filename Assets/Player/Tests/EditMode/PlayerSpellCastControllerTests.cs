using System.Reflection;
using NUnit.Framework;
using RPGame.Combat.Spells;
using RPGame.Core.Spells;
using RPGame.Core.Spells.Symbols;
using RPGame.Core.Targeting;
using RPGame.Player.Spells;
using RPGame.Player.Targeting;
using UnityEngine;

namespace RPGame.Player.Tests
{
    public sealed class PlayerSpellCastControllerTests
    {
        private GameObject playerObject;
        private PlayerTargeting playerTargeting;
        private SpellCaster spellCaster;
        private PlayerSpellCastController controller;
        private CaptureCasterDataSpell spell;

        [SetUp]
        public void SetUp()
        {
            playerObject = new GameObject("PlayerSpellCastControllerTests");
            playerTargeting = playerObject.AddComponent<PlayerTargeting>();
            spellCaster = playerObject.AddComponent<SpellCaster>();
            controller = playerObject.AddComponent<PlayerSpellCastController>();
            spell = ScriptableObject.CreateInstance<CaptureCasterDataSpell>();

            SetField(controller, "playerTargeting", playerTargeting);
            SetField(controller, "spellCaster", spellCaster);
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

                bool wasCast = InvokeCastSpell(spell);

                Assert.IsTrue(wasCast);
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

            bool wasCast = InvokeCastSpell(spell);

            Assert.IsTrue(wasCast);
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
        public void SpellSymbolCaster_DoesNotDependOnPlayerTargeting()
        {
            FieldInfo[] fields = typeof(SpellSymbolCaster).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int i = 0; i < fields.Length; i++)
            {
                Assert.AreNotSame(typeof(PlayerTargeting), fields[i].FieldType);
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
        public void PlayerSpellCastController_CastsSpellSelectedBySpellSymbolCaster()
        {
            SpellSymbolCaster symbolCaster = playerObject.AddComponent<SpellSymbolCaster>();
            SetSpellSymbolEntries(symbolCaster, symbolId: 5, spell);
            SetField(controller, "spellSymbolCaster", symbolCaster);

            InvokeLifecycle("OnEnable");
            symbolCaster.ReceiveSymbol(new SymbolRecognitionResult(5, 1f));
            InvokeLifecycle("OnDisable");

            Assert.AreSame(playerObject, spell.LastCasterData.CasterObject);
        }

        private bool InvokeCastSpell(Spell selectedSpell)
        {
            MethodInfo method = typeof(PlayerSpellCastController).GetMethod("CastSpell", BindingFlags.Instance | BindingFlags.NonPublic);
            return (bool)method.Invoke(controller, new object[] { selectedSpell });
        }

        private CasterData InvokeCreateCasterData()
        {
            MethodInfo method = typeof(PlayerSpellCastController).GetMethod("CreateCasterData", BindingFlags.Instance | BindingFlags.NonPublic);
            return (CasterData)method.Invoke(controller, null);
        }

        private void InvokeLifecycle(string methodName)
        {
            MethodInfo method = typeof(PlayerSpellCastController).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(controller, null);
        }

        private void SetCurrentTarget(ITargetable target)
        {
            FieldInfo field = typeof(PlayerTargeting).GetField("<CurrentTarget>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(playerTargeting, target);
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
    }
}
