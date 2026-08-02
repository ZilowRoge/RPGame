using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RPGame.Core.Effects;
using RPGame.Core.Statistics.Attributes;
using RPGame.Progression;
using RPGame.UI.Statistics;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPGame.UI.Tests
{
    public sealed class CharacterAttributesUITests
    {
        private GameObject attributesObject;
        private GameObject uiObject;
        private CharacterAttributesConfig config;
        private CharacterAttributes attributes;
        private CharacterProgression progression;

        [SetUp]
        public void SetUp()
        {
            config = CreateConfig(
                strength: 10,
                dexterity: 8,
                endurance: 12,
                vitality: 15,
                intelligence: 11,
                power: 6);

            attributesObject = new GameObject("Character");
            attributes = attributesObject.AddComponent<CharacterAttributes>();
            progression = attributesObject.AddComponent<CharacterProgression>();
            SetAttributesConfig(attributes, config);
            SetProgressionAttributes(progression, attributes);

            uiObject = new GameObject("StatisticsWindow");
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(uiObject);
            UnityEngine.Object.DestroyImmediate(attributesObject);
            UnityEngine.Object.DestroyImmediate(config);
        }

        [Test]
        public void Refresh_WhenRecordHasAttribute_DisplaysAttributeNameAndValue()
        {
            CharacterAttributeRecordUI record = CreateRecord("Power", CharacterAttributeType.Power, uiObject.transform);

            record.Refresh(attributes, pendingPoints: 0, canIncrease: true);

            Assert.AreEqual("Power", GetText(record, "Content/Label").text);
            Assert.AreEqual("6", GetText(record, "Content/Value").text);
            Assert.IsTrue(GetButton(record, "Plus/Button").interactable);
            Assert.IsFalse(GetButton(record, "Minus/Button").interactable);
        }

        [Test]
        public void OnEnable_WhenRecordsExistUnderRoot_RefreshesRecords()
        {
            progression.AddExperience(250);
            GameObject root = CreateAttributesRoot();
            CharacterAttributeRecordUI strengthRecord = CreateRecord("Strength", CharacterAttributeType.Strength, root.transform);
            CharacterAttributeRecordUI intelligenceRecord = CreateRecord("Intelligence", CharacterAttributeType.Intelligence, root.transform);
            CreateAttributesUI(root.transform);

            Assert.AreEqual("10", GetText(strengthRecord, "Content/Value").text);
            Assert.AreEqual("11", GetText(intelligenceRecord, "Content/Value").text);
        }

        [Test]
        public void Refresh_WhenRecordLabelDiffersFromAttributeType_KeepsConfiguredLabel()
        {
            CharacterAttributeRecordUI record = CreateRecord("Strenght", CharacterAttributeType.Strength, uiObject.transform);

            record.Refresh(attributes, pendingPoints: 0, canIncrease: true);

            Assert.AreEqual("Strenght", GetText(record, "Content/Label").text);
            Assert.AreEqual("10", GetText(record, "Content/Value").text);
        }

        [Test]
        public void IncreaseButton_WhenEnoughXP_AddsPendingPointAndShowsPreviewValue()
        {
            progression.AddExperience(250);
            GameObject root = CreateAttributesRoot();
            CharacterAttributeRecordUI record = CreateRecord("Power", CharacterAttributeType.Power, root.transform);
            CreateAttributesUI(root.transform);

            GetButton(record, "Plus/Button").onClick.Invoke();

            Assert.AreEqual("7", GetText(record, "Content/Value").text);
            Assert.AreEqual(6, attributes.Power);
            Assert.IsTrue(GetButton(record, "Minus/Button").interactable);
        }

        [Test]
        public void IncreaseButton_WhenPendingPointsExist_DisplaysPendingCostLabel()
        {
            progression.AddExperience(250);
            GameObject root = CreateAttributesRoot();
            CharacterAttributeRecordUI record = CreateRecord("Power", CharacterAttributeType.Power, root.transform);
            TextMeshProUGUI pendingCostText = CreateText("PendingCost", uiObject.transform);
            CreateAttributesUI(root.transform, pendingCostText: pendingCostText);

            GetButton(record, "Plus/Button").onClick.Invoke();

            Assert.AreEqual("XP to spend: 100", pendingCostText.text);
        }

        [Test]
        public void Show_WhenAttributeCostTooltipIsShown_DisplaysCostLabel()
        {
            AttributeCostTooltipUI tooltip = CreateAttributeCostTooltip(out TextMeshProUGUI costText);

            tooltip.Show(150, Vector2.zero);

            Assert.AreEqual("XP cost: 150", costText.text);
            UnityEngine.Object.DestroyImmediate(tooltip.gameObject);
        }

        [Test]
        public void CancelButton_WhenPendingPointsExist_ClearsPreviewWithoutSpendingXP()
        {
            progression.AddExperience(250);
            GameObject root = CreateAttributesRoot();
            CharacterAttributeRecordUI record = CreateRecord("Power", CharacterAttributeType.Power, root.transform);
            Button cancelButton = CreateButton("Cancel", uiObject.transform);
            CreateAttributesUI(root.transform, cancelButton: cancelButton);

            GetButton(record, "Plus/Button").onClick.Invoke();
            cancelButton.onClick.Invoke();

            Assert.AreEqual("6", GetText(record, "Content/Value").text);
            Assert.AreEqual(250, progression.GetAvailableXP());
            Assert.AreEqual(0, attributes.GetPurchasedPoints(CharacterAttributeType.Power));
        }

        [Test]
        public void ConfirmButton_WhenPendingPointsExist_BuysPointsAndSpendsXP()
        {
            progression.AddExperience(250);
            GameObject root = CreateAttributesRoot();
            CharacterAttributeRecordUI record = CreateRecord("Power", CharacterAttributeType.Power, root.transform);
            Button confirmButton = CreateButton("Confirm", uiObject.transform);
            CreateAttributesUI(root.transform, confirmButton: confirmButton);

            GetButton(record, "Plus/Button").onClick.Invoke();
            confirmButton.onClick.Invoke();

            Assert.AreEqual("7", GetText(record, "Content/Value").text);
            Assert.AreEqual(150, progression.GetAvailableXP());
            Assert.AreEqual(1, attributes.GetPurchasedPoints(CharacterAttributeType.Power));
        }

        [Test]
        public void OnDisable_WhenPendingPointsExist_ClearsPendingPoints()
        {
            progression.AddExperience(250);
            GameObject root = CreateAttributesRoot();
            CharacterAttributeRecordUI record = CreateRecord("Power", CharacterAttributeType.Power, root.transform);
            CharacterAttributesUI ui = CreateAttributesUI(root.transform);

            GetButton(record, "Plus/Button").onClick.Invoke();
            InvokeLifecycleMethod(ui, "OnDisable");
            InvokeLifecycleMethod(ui, "OnEnable");

            Assert.AreEqual("6", GetText(record, "Content/Value").text);
            Assert.AreEqual(250, progression.GetAvailableXP());
            Assert.AreEqual(0, attributes.GetPurchasedPoints(CharacterAttributeType.Power));
        }

        [Test]
        public void IncreaseButton_WhenNotEnoughXP_DoesNotAddPendingPoint()
        {
            progression.AddExperience(50);
            GameObject root = CreateAttributesRoot();
            CharacterAttributeRecordUI record = CreateRecord("Power", CharacterAttributeType.Power, root.transform);
            CreateAttributesUI(root.transform);

            GetButton(record, "Plus/Button").onClick.Invoke();

            Assert.AreEqual("6", GetText(record, "Content/Value").text);
            Assert.IsFalse(GetButton(record, "Plus/Button").interactable);
        }

        [Test]
        public void IncreaseButton_WhenCostTooltipIsVisible_RefreshesTooltipAfterPendingPointChanges()
        {
            CharacterAttributeRecordUI record = CreateRecord("Power", CharacterAttributeType.Power, uiObject.transform);
            List<int> shownCosts = new();
            int pendingPoints = 0;
            record.Initialize(
                attributeType => pendingPoints++,
                attributeType => pendingPoints--,
                (attributeType, position) => shownCosts.Add(pendingPoints == 0 ? 100 : 150),
                () => { });
            PointerEventData eventData = new(EventSystem.current)
            {
                pointerEnter = GetButton(record, "Plus/Button").gameObject,
                position = new Vector2(20f, 30f)
            };

            record.OnPointerEnter(eventData);
            GetButton(record, "Plus/Button").onClick.Invoke();

            CollectionAssert.AreEqual(new[] { 100, 150 }, shownCosts);
        }

        [Test]
        public void OnEnable_WhenAttributeEffectExists_RefreshesValue()
        {
            progression.AddExperience(250);
            EffectAggregator aggregator = attributesObject.AddComponent<EffectAggregator>();
            StatEffectDefinition effect = CreateStatEffect(EffectStat.Power, EffectModifierType.Flat, 5f);
            aggregator.Add(effect);
            GameObject root = CreateAttributesRoot();
            CharacterAttributeRecordUI record = CreateRecord("Power", CharacterAttributeType.Power, root.transform);
            CreateAttributesUI(root.transform);

            Assert.AreEqual("11", GetText(record, "Content/Value").text);
            UnityEngine.Object.DestroyImmediate(effect);
        }

        private GameObject CreateAttributesRoot()
        {
            GameObject attributesPanel = new GameObject("Atributes");
            attributesPanel.transform.SetParent(uiObject.transform);
            GameObject content = new GameObject("Content");
            content.transform.SetParent(attributesPanel.transform);
            return content;
        }

        private CharacterAttributesUI CreateAttributesUI(
            Transform recordsRoot,
            Button confirmButton = null,
            Button cancelButton = null,
            TMP_Text pendingCostText = null)
        {
            uiObject.SetActive(false);
            CharacterAttributesUI ui = uiObject.AddComponent<CharacterAttributesUI>();
            SetAttributesUIReferences(
                ui,
                progression,
                attributes,
                recordsRoot,
                confirmButton,
                cancelButton,
                pendingCostText);
            uiObject.SetActive(true);
            ui.enabled = false;
            ui.enabled = true;
            ui.Refresh();
            return ui;
        }

        private static CharacterAttributeRecordUI CreateRecord(
            string name,
            CharacterAttributeType attributeType,
            Transform parent = null)
        {
            GameObject recordObject = new GameObject(name);
            if (parent != null)
            {
                recordObject.transform.SetParent(parent);
            }

            GameObject content = new GameObject("Content");
            content.transform.SetParent(recordObject.transform);

            GameObject label = new GameObject("Label");
            label.transform.SetParent(content.transform);
            label.AddComponent<TextMeshProUGUI>().text = name;

            GameObject value = new GameObject("Value");
            value.transform.SetParent(content.transform);
            value.AddComponent<TextMeshProUGUI>();

            GameObject plus = new GameObject("Plus");
            plus.transform.SetParent(recordObject.transform);
            CreateButton("Button", plus.transform);

            GameObject minus = new GameObject("Minus");
            minus.transform.SetParent(recordObject.transform);
            CreateButton("Button", minus.transform);

            CharacterAttributeRecordUI record = recordObject.AddComponent<CharacterAttributeRecordUI>();
            SetRecordAttributeType(record, attributeType);
            return record;
        }

        private static TMP_Text GetText(CharacterAttributeRecordUI record, string path)
        {
            return record.transform.Find(path).GetComponent<TMP_Text>();
        }

        private static Button GetButton(CharacterAttributeRecordUI record, string path)
        {
            return record.transform.Find(path).GetComponent<Button>();
        }

        private static Button CreateButton(string name, Transform parent)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent);
            buttonObject.AddComponent<Image>();
            return buttonObject.AddComponent<Button>();
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent);
            return textObject.AddComponent<TextMeshProUGUI>();
        }

        private static AttributeCostTooltipUI CreateAttributeCostTooltip(out TextMeshProUGUI costText)
        {
            GameObject tooltipObject = new GameObject("AttributeCostTooltip", typeof(RectTransform));
            tooltipObject.AddComponent<CanvasGroup>();
            costText = CreateText("CostText", tooltipObject.transform);
            AttributeCostTooltipUI tooltip = tooltipObject.AddComponent<AttributeCostTooltipUI>();
            SerializedObject serializedTooltip = new SerializedObject(tooltip);
            serializedTooltip.FindProperty("costText").objectReferenceValue = costText;
            serializedTooltip.ApplyModifiedPropertiesWithoutUndo();
            return tooltip;
        }

        private static CharacterAttributesConfig CreateConfig(
            int strength,
            int dexterity,
            int endurance,
            int vitality,
            int intelligence,
            int power)
        {
            CharacterAttributesConfig attributesConfig = ScriptableObject.CreateInstance<CharacterAttributesConfig>();
            SerializedObject serializedConfig = new SerializedObject(attributesConfig);
            serializedConfig.FindProperty("strength").intValue = strength;
            serializedConfig.FindProperty("dexterity").intValue = dexterity;
            serializedConfig.FindProperty("endurance").intValue = endurance;
            serializedConfig.FindProperty("vitality").intValue = vitality;
            serializedConfig.FindProperty("intelligence").intValue = intelligence;
            serializedConfig.FindProperty("power").intValue = power;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            return attributesConfig;
        }

        private static StatEffectDefinition CreateStatEffect(
            EffectStat stat,
            EffectModifierType modifierType,
            float value)
        {
            StatEffectDefinition definition = ScriptableObject.CreateInstance<StatEffectDefinition>();
            SerializedObject serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("stat").enumValueIndex = (int)stat;
            serializedDefinition.FindProperty("modifierType").enumValueIndex = (int)modifierType;
            serializedDefinition.FindProperty("value").floatValue = value;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }

        private static void SetAttributesConfig(CharacterAttributes characterAttributes, CharacterAttributesConfig attributesConfig)
        {
            SerializedObject serializedAttributes = new SerializedObject(characterAttributes);
            serializedAttributes.FindProperty("config").objectReferenceValue = attributesConfig;
            serializedAttributes.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetAttributesUIReferences(
            CharacterAttributesUI ui,
            CharacterProgression progression,
            CharacterAttributes attributes,
            Transform recordsRoot,
            Button confirmButton = null,
            Button cancelButton = null,
            TMP_Text pendingCostText = null)
        {
            SerializedObject serializedUI = new SerializedObject(ui);
            serializedUI.FindProperty("progression").objectReferenceValue = progression;
            serializedUI.FindProperty("attributes").objectReferenceValue = attributes;
            serializedUI.FindProperty("recordsRoot").objectReferenceValue = recordsRoot;
            SerializedProperty records = serializedUI.FindProperty("records");
            records.arraySize = recordsRoot.childCount;
            for (int i = 0; i < recordsRoot.childCount; i++)
            {
                records.GetArrayElementAtIndex(i).objectReferenceValue =
                    recordsRoot.GetChild(i).GetComponent<CharacterAttributeRecordUI>();
            }

            serializedUI.FindProperty("confirmButton").objectReferenceValue = confirmButton;
            serializedUI.FindProperty("cancelButton").objectReferenceValue = cancelButton;
            serializedUI.FindProperty("pendingCostText").objectReferenceValue = pendingCostText;
            serializedUI.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetRecordAttributeType(
            CharacterAttributeRecordUI record,
            CharacterAttributeType attributeType)
        {
            SerializedObject serializedRecord = new SerializedObject(record);
            serializedRecord.FindProperty("attributeType").enumValueIndex = (int)attributeType;
            serializedRecord.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetProgressionAttributes(
            CharacterProgression characterProgression,
            CharacterAttributes characterAttributes)
        {
            SerializedObject serializedProgression = new SerializedObject(characterProgression);
            serializedProgression.FindProperty("attributes").objectReferenceValue = characterAttributes;
            serializedProgression.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void InvokeLifecycleMethod(MonoBehaviour behaviour, string methodName)
        {
            MethodInfo method = behaviour.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            method.Invoke(behaviour, null);
        }
    }
}
