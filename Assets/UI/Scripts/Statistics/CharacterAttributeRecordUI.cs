using System;
using RPGame.Core.Statistics.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPGame.UI.Statistics
{
    public sealed class CharacterAttributeRecordUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private CharacterAttributeType attributeType;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private Button increaseButton;
        [SerializeField] private Button decreaseButton;

        public CharacterAttributeType AttributeType => attributeType;

        private Action<CharacterAttributeType> increaseRequested;
        private Action<CharacterAttributeType> decreaseRequested;
        private Action<CharacterAttributeType, Vector2> showCostTooltip;
        private Action hideCostTooltip;
        private bool isCostTooltipVisible;
        private Vector2 costTooltipPosition;

        private void Awake()
        {
            ResolveReferences();
            SubscribeButtons();
        }

        private void OnDestroy()
        {
            UnsubscribeButtons();
        }

        public void Initialize(
            Action<CharacterAttributeType> increaseRequested,
            Action<CharacterAttributeType> decreaseRequested,
            Action<CharacterAttributeType, Vector2> showCostTooltip,
            Action hideCostTooltip)
        {
            this.increaseRequested = increaseRequested;
            this.decreaseRequested = decreaseRequested;
            this.showCostTooltip = showCostTooltip;
            this.hideCostTooltip = hideCostTooltip;
            ResolveReferences();
            SubscribeButtons();
        }

        public void Refresh(
            ICharacterAttributes attributes,
            int pendingPoints,
            bool canIncrease)
        {
            ResolveReferences();

            if (valueText != null)
            {
                valueText.text = attributes != null
                    ? (attributes.GetValue(attributeType) + pendingPoints).ToString()
                    : string.Empty;
            }

            if (increaseButton != null)
            {
                increaseButton.interactable = canIncrease;
            }

            if (decreaseButton != null)
            {
                decreaseButton.interactable = pendingPoints > 0;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (IsPointerOnIncreaseButton(eventData))
            {
                ShowCostTooltip(eventData != null ? eventData.position : Vector2.zero);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isCostTooltipVisible = false;
            hideCostTooltip?.Invoke();
        }

        private void ResolveReferences()
        {
            if (valueText == null)
            {
                valueText = transform.Find("Content/Value")?.GetComponent<TMP_Text>();
            }

            if (increaseButton == null)
            {
                increaseButton = transform.Find("Plus/Button")?.GetComponent<Button>();
            }

            if (decreaseButton == null)
            {
                decreaseButton = transform.Find("Minus/Button")?.GetComponent<Button>();
            }
        }

        private void SubscribeButtons()
        {
            if (increaseButton != null)
            {
                increaseButton.onClick.RemoveListener(HandleIncreaseClicked);
                increaseButton.onClick.AddListener(HandleIncreaseClicked);
            }

            if (decreaseButton != null)
            {
                decreaseButton.onClick.RemoveListener(HandleDecreaseClicked);
                decreaseButton.onClick.AddListener(HandleDecreaseClicked);
            }
        }

        private void UnsubscribeButtons()
        {
            if (increaseButton != null)
            {
                increaseButton.onClick.RemoveListener(HandleIncreaseClicked);
            }

            if (decreaseButton != null)
            {
                decreaseButton.onClick.RemoveListener(HandleDecreaseClicked);
            }
        }

        private void HandleIncreaseClicked()
        {
            increaseRequested?.Invoke(attributeType);

            if (isCostTooltipVisible)
            {
                showCostTooltip?.Invoke(attributeType, costTooltipPosition);
            }
        }

        private void HandleDecreaseClicked()
        {
            decreaseRequested?.Invoke(attributeType);
        }

        private bool IsPointerOnIncreaseButton(PointerEventData eventData)
        {
            return increaseButton != null
                && eventData?.pointerEnter != null
                && eventData.pointerEnter.transform.IsChildOf(increaseButton.transform);
        }

        private void ShowCostTooltip(Vector2 screenPosition)
        {
            isCostTooltipVisible = true;
            costTooltipPosition = screenPosition;
            showCostTooltip?.Invoke(attributeType, costTooltipPosition);
        }
    }
}
