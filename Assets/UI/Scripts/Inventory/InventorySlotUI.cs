using System;
using RPGame.Core.Inventory.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGame.UI.Inventory
{
    public sealed class InventorySlotUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI stackText;
        [SerializeField] private GameObject stackTextBackground;

        private int slotIndex;
        private float doubleClickThreshold;
        private float lastClickTime = -1f;

        public event Action<int> DoubleClicked;

        public void Initialize(int slotIndex, float doubleClickThreshold)
        {
            this.slotIndex = slotIndex;
            this.doubleClickThreshold = doubleClickThreshold;
        }

        public void SetItem(ItemInstance item)
        {
            bool hasItem = item != null && item.Definition != null;

            if (iconImage != null)
            {
                iconImage.enabled = hasItem && item.Definition.Icon != null;
                iconImage.sprite = hasItem ? item.Definition.Icon : null;
            }

            bool showStackText = hasItem && item.StackSize > 1;

            if (stackText != null)
            {
                stackText.text = showStackText ? item.StackSize.ToString() : string.Empty;
            }

            if (stackTextBackground != null)
            {
                stackTextBackground.SetActive(showStackText);
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
                Debug.Log($"Inventory slot double clicked: {slotIndex}", this);
                DoubleClicked?.Invoke(slotIndex);
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
