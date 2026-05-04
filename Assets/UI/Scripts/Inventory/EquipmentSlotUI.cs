using System;
using RPGame.Core.Inventory.Data;
using RPGame.Core.Inventory.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGame.UI.Inventory
{
    public sealed class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI slotNameText;

        private float doubleClickThreshold;
        private float lastClickTime = -1f;

        public event Action<EquipmentSlotType> DoubleClicked;

        public EquipmentSlotType SlotType { get; private set; }

        public void Initialize(EquipmentSlotType slotType, float doubleClickThreshold)
        {
            SlotType = slotType;
            this.doubleClickThreshold = doubleClickThreshold;

            if (slotNameText != null)
            {
                slotNameText.text = slotType.ToString();
            }
        }

        public void SetItem(ItemInstance item)
        {
            bool hasItem = item != null && item.Definition != null;

            if (iconImage != null)
            {
                iconImage.enabled = hasItem && item.Definition.Icon != null;
                iconImage.sprite = hasItem ? item.Definition.Icon : null;
            }

        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsLeftPointerClick(eventData))
            {
                return;
            }

            float currentTime = Time.unscaledTime;
            if (currentTime - lastClickTime <= doubleClickThreshold)
            {
                lastClickTime = -1f;
                Debug.Log($"Equipment slot double clicked: {SlotType}", this);
                DoubleClicked?.Invoke(SlotType);
                return;
            }

            lastClickTime = currentTime;
        }

        private static bool IsLeftPointerClick(PointerEventData eventData)
        {
            return eventData.button == PointerEventData.InputButton.Left
                && (Mouse.current != null || Touchscreen.current != null || Pen.current != null || Pointer.current != null);
        }
    }
}
