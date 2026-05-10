using System.Collections.Generic;
using NUnit.Framework;
using RPGame.Progression;
using RPGame.UI.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RPGame.UI.Tests
{
    public sealed class PerkNodeUITests
    {
        private readonly List<Object> objectsToDestroy = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = objectsToDestroy.Count - 1; i >= 0; i--)
            {
                if (objectsToDestroy[i] != null)
                {
                    Object.DestroyImmediate(objectsToDestroy[i]);
                }
            }

            objectsToDestroy.Clear();
        }

        [Test]
        public void Click_WhenPerkIsAvailable_TogglesPendingAndRequestsRefresh()
        {
            PerkDefinition perk = CreatePerk("start", isStartingPerk: true);
            CreateNode(out PerkNodeUI nodeUI, out _);
            PerkDefinition toggledPerk = null;
            int refreshCount = 0;

            nodeUI.Initialize(
                perk,
                perkDefinition => PerkUnlockState.Available,
                perkDefinition => false,
                perkDefinition => toggledPerk = perkDefinition,
                (perkDefinition, position) => { },
                () => { },
                () => refreshCount++);
            nodeUI.OnPointerClick(eventData: null);

            Assert.AreSame(perk, toggledPerk);
            Assert.AreEqual(1, refreshCount);
        }

        [Test]
        public void Click_WhenPerkIsLocked_DoesNotTogglePendingOrRequestRefresh()
        {
            PerkDefinition perk = CreatePerk("locked", isStartingPerk: false);
            CreateNode(out PerkNodeUI nodeUI, out _);
            PerkDefinition toggledPerk = null;
            int refreshCount = 0;

            nodeUI.Initialize(
                perk,
                perkDefinition => PerkUnlockState.Locked,
                perkDefinition => false,
                perkDefinition => toggledPerk = perkDefinition,
                (perkDefinition, position) => { },
                () => { },
                () => refreshCount++);
            nodeUI.OnPointerClick(eventData: null);

            Assert.IsNull(toggledPerk);
            Assert.AreEqual(0, refreshCount);
        }

        [Test]
        public void Hover_WhenPerkIsNotUnlocked_ChangesGraphicColor()
        {
            PerkDefinition perk = CreatePerk("start", isStartingPerk: true);
            CreateNode(out PerkNodeUI nodeUI, out Image image);

            nodeUI.Initialize(
                perk,
                perkDefinition => PerkUnlockState.Available,
                perkDefinition => false,
                perkDefinition => { },
                (perkDefinition, position) => { },
                () => { },
                refreshRequested: null);
            Color initialColor = image.color;
            nodeUI.OnPointerEnter(eventData: null);

            Assert.AreNotEqual(initialColor, image.color);
            Assert.AreEqual(Color.white, image.color);
        }

        [Test]
        public void Hover_WhenPerkIsLocked_DoesNotChangeGraphicColor()
        {
            PerkDefinition perk = CreatePerk("locked", isStartingPerk: false);
            CreateNode(out PerkNodeUI nodeUI, out Image image);

            nodeUI.Initialize(
                perk,
                perkDefinition => PerkUnlockState.Locked,
                perkDefinition => false,
                perkDefinition => { },
                (perkDefinition, position) => { },
                () => { },
                refreshRequested: null);
            Color initialColor = image.color;
            nodeUI.OnPointerEnter(eventData: null);

            Assert.AreEqual(initialColor, image.color);
        }

        private void CreateNode(out PerkNodeUI nodeUI, out Image image)
        {
            GameObject gameObject = new GameObject("PerkNode", typeof(RectTransform), typeof(Image), typeof(PerkNodeUI));
            objectsToDestroy.Add(gameObject);
            nodeUI = gameObject.GetComponent<PerkNodeUI>();
            image = gameObject.GetComponent<Image>();
        }

        private PerkDefinition CreatePerk(string perkId, bool isStartingPerk)
        {
            PerkDefinition perk = ScriptableObject.CreateInstance<PerkDefinition>();
            objectsToDestroy.Add(perk);

            SerializedObject serializedPerk = new SerializedObject(perk);
            serializedPerk.FindProperty("perkId").stringValue = perkId;
            serializedPerk.FindProperty("displayName").stringValue = perkId;
            serializedPerk.FindProperty("description").stringValue = "Test perk";
            serializedPerk.FindProperty("cost").intValue = 1;
            serializedPerk.FindProperty("isStartingPerk").boolValue = isStartingPerk;
            serializedPerk.ApplyModifiedPropertiesWithoutUndo();

            return perk;
        }
    }
}
