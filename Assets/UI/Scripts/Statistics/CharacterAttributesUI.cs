using System.Collections.Generic;
using RPGame.Core.Statistics.Attributes;
using RPGame.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGame.UI.Statistics
{
    public sealed class CharacterAttributesUI : MonoBehaviour
    {
        [SerializeField] private CharacterProgression progression;
        [SerializeField] private CharacterAttributes attributes;
        [SerializeField] private Transform recordsRoot;
        [SerializeField] private List<CharacterAttributeRecordUI> records = new();
        [SerializeField] private AttributeCostTooltipUI costTooltip;
        [SerializeField] private TMP_Text availableExpText;
        [SerializeField] private TMP_Text pendingCostText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private readonly Dictionary<CharacterAttributeType, int> pendingPoints = new();

        private void Awake()
        {
            ResolveReferences();
            SubscribeButtons();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeButtons();
            ClearPendingPoints();
            Refresh();
        }

        private void OnDisable()
        {
            ClearPendingPoints();
            HideCostTooltip();
        }

        private void OnDestroy()
        {
            UnsubscribeButtons();
        }

        public void SetAttributes(CharacterAttributes attributes)
        {
            if (this.attributes == attributes)
            {
                return;
            }

            this.attributes = attributes;
            ClearPendingPoints();
            Refresh();
        }

        public void SetProgression(CharacterProgression progression)
        {
            if (this.progression == progression)
            {
                return;
            }

            this.progression = progression;
            ClearPendingPoints();
            Refresh();
        }

        public void Refresh()
        {
            ResolveReferences();
            SubscribeButtons();

            ICharacterAttributes attributeSource = attributes;
            int pendingCost = GetPendingCost();
            int remainingXP = progression != null ? Mathf.Max(0, progression.GetAvailableXP() - pendingCost) : 0;

            for (int i = 0; i < records.Count; i++)
            {
                if (records[i] != null)
                {
                    CharacterAttributeType attributeType = records[i].AttributeType;
                    int attributePendingPoints = GetPendingPoints(attributeType);
                    int nextPointCost = GetNextPendingPointCost(attributeType);
                    records[i].Refresh(
                        attributeSource,
                        attributePendingPoints,
                        progression != null && attributeSource != null && nextPointCost <= remainingXP);
                }
            }

            if (availableExpText != null)
            {
                availableExpText.text = remainingXP.ToString();
            }

            if (pendingCostText != null)
            {
                pendingCostText.text = $"XP to spend: {pendingCost}";
            }

            bool hasPendingPoints = HasPendingPoints();
            if (confirmButton != null)
            {
                confirmButton.interactable = hasPendingPoints && progression != null && pendingCost <= progression.GetAvailableXP();
            }

            if (cancelButton != null)
            {
                cancelButton.interactable = hasPendingPoints;
            }
        }

        private void ResolveReferences()
        {
            if (progression == null)
            {
                progression = FindAnyObjectByType<CharacterProgression>();
            }

            if (attributes == null)
            {
                attributes = FindAnyObjectByType<CharacterAttributes>();
            }

            if (recordsRoot == null)
            {
                recordsRoot = transform.Find("Atributes/Content") ?? transform.Find("Attributes/Content");
            }

            ResolveRecords();
            InitializeRecords();
        }

        private void ResolveRecords()
        {
            if (records.Count > 0 || recordsRoot == null)
            {
                return;
            }

            for (int i = 0; i < recordsRoot.childCount; i++)
            {
                Transform child = recordsRoot.GetChild(i);
                CharacterAttributeRecordUI record = child.GetComponent<CharacterAttributeRecordUI>();
                if (record != null)
                {
                    record.Initialize(
                        HandleIncreaseRequested,
                        HandleDecreaseRequested,
                        ShowCostTooltip,
                        HideCostTooltip);
                    records.Add(record);
                }
            }
        }

        private void InitializeRecords()
        {
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i] != null)
                {
                    records[i].Initialize(
                        HandleIncreaseRequested,
                        HandleDecreaseRequested,
                        ShowCostTooltip,
                        HideCostTooltip);
                }
            }
        }

        private void SubscribeButtons()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(ConfirmPendingPoints);
                confirmButton.onClick.AddListener(ConfirmPendingPoints);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(CancelPendingPoints);
                cancelButton.onClick.AddListener(CancelPendingPoints);
            }
        }

        private void UnsubscribeButtons()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(ConfirmPendingPoints);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(CancelPendingPoints);
            }
        }

        private void HandleIncreaseRequested(CharacterAttributeType attributeType)
        {
            if (progression == null)
            {
                return;
            }

            int pendingCost = GetPendingCost();
            int nextCost = GetNextPendingPointCost(attributeType);
            if (pendingCost + nextCost > progression.GetAvailableXP())
            {
                return;
            }

            pendingPoints[attributeType] = GetPendingPoints(attributeType) + 1;
            Refresh();
        }

        private void HandleDecreaseRequested(CharacterAttributeType attributeType)
        {
            int points = GetPendingPoints(attributeType);
            if (points <= 0)
            {
                return;
            }

            if (points == 1)
            {
                pendingPoints.Remove(attributeType);
            }
            else
            {
                pendingPoints[attributeType] = points - 1;
            }

            Refresh();
        }

        private void ConfirmPendingPoints()
        {
            if (progression == null || !HasPendingPoints())
            {
                return;
            }

            AttributePurchaseResult result = progression.TryBuyAttributePoints(pendingPoints);
            if (!result.Success)
            {
                Refresh();
                return;
            }

            pendingPoints.Clear();
            Refresh();
        }

        private void CancelPendingPoints()
        {
            if (!HasPendingPoints())
            {
                return;
            }

            ClearPendingPoints();
            Refresh();
        }

        private void ClearPendingPoints()
        {
            pendingPoints.Clear();
        }

        private void ShowCostTooltip(CharacterAttributeType attributeType, Vector2 screenPosition)
        {
            if (costTooltip != null)
            {
                costTooltip.Show(GetNextPendingPointCost(attributeType), screenPosition);
            }
        }

        private void HideCostTooltip()
        {
            if (costTooltip != null)
            {
                costTooltip.Hide();
            }
        }

        private int GetPendingCost()
        {
            return progression != null ? progression.GetAttributePointsCost(pendingPoints) : 0;
        }

        private int GetNextPendingPointCost(CharacterAttributeType attributeType)
        {
            if (progression == null)
            {
                return 0;
            }

            int currentPendingPoints = GetPendingPoints(attributeType);
            if (currentPendingPoints <= 0)
            {
                return progression.GetNextAttributePointCost(attributeType);
            }

            Dictionary<CharacterAttributeType, int> pointsWithNext = new(pendingPoints)
            {
                [attributeType] = currentPendingPoints + 1
            };

            return progression.GetAttributePointsCost(pointsWithNext) - GetPendingCost();
        }

        private int GetPendingPoints(CharacterAttributeType attributeType)
        {
            return pendingPoints.TryGetValue(attributeType, out int points) ? points : 0;
        }

        private bool HasPendingPoints()
        {
            foreach (int points in pendingPoints.Values)
            {
                if (points > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
