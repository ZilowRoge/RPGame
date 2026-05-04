using System;
using RPGame.Core.Inventory.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGame.UI.Inventory
{
    public sealed class InventorySlotUI :
        MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI stackText;
        [SerializeField] private GameObject stackTextBackground;

        private int slotIndex;
        private float doubleClickThreshold;
        private float lastClickTime = -1f;
        private ItemInstance currentItem;

        public event Action<int> DoubleClicked;
        public event Action<ItemInstance, Vector2> PointerEntered;
        public event Action PointerExited;
        public event Action<ItemSlotReference, ItemInstance, Vector2> DragStarted;
        public event Action<Vector2> Dragged;
        public event Action DragEnded;
        public event Action<ItemSlotReference> Dropped;

        public void Initialize(int slotIndex, float doubleClickThreshold)
        {
            this.slotIndex = slotIndex;
            this.doubleClickThreshold = doubleClickThreshold;
        }

        public void SetItem(ItemInstance item)
        {
            currentItem = item;
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentItem != null && currentItem.Definition != null)
            {
                PointerEntered?.Invoke(currentItem, eventData.position);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerExited?.Invoke();
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

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left
                || currentItem == null
                || currentItem.Definition == null)
            {
                return;
            }

            DragStarted?.Invoke(ItemSlotReference.Inventory(slotIndex), currentItem, eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Dragged?.Invoke(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DragEnded?.Invoke();
        }

        public void OnDrop(PointerEventData eventData)
        {
            Dropped?.Invoke(ItemSlotReference.Inventory(slotIndex));
        }

        private static bool IsLeftPointerClick(PointerEventData eventData)
        {
            return eventData.button == PointerEventData.InputButton.Left
                && (Mouse.current != null || Touchscreen.current != null || Pen.current != null || Pointer.current != null);
        }
    }
}
