using System;
using RPGame.Inventory.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGame.UI.Inventory
{
    public sealed class ConsumableSlotUI :
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
        [SerializeField] private TextMeshProUGUI shortcutText;
        [SerializeField] private TextMeshProUGUI stackSizeText;
        [SerializeField] private Image stackSizeBackgroundImage;
        [SerializeField] private bool targetable = true;

        private float doubleClickThreshold;
        private float lastClickTime = -1f;
        private ItemInstance currentItem;

        public event Action<ConsumableSlotType> DoubleClicked;
        public event Action<ItemInstance, Vector2> PointerEntered;
        public event Action PointerExited;
        public event Action<ItemSlotReference, ItemInstance, Vector2> DragStarted;
        public event Action<Vector2> Dragged;
        public event Action DragEnded;
        public event Action<ItemSlotReference> Dropped;

        public ConsumableSlotType SlotType { get; private set; }
        public bool Targetable
        {
            get => targetable;
            set => targetable = value;
        }

        public void Initialize(ConsumableSlotType slotType, float doubleClickThreshold)
        {
            SlotType = slotType;
            this.doubleClickThreshold = doubleClickThreshold;

            if (shortcutText != null)
            {
                shortcutText.text = GetShortcutText(slotType);
            }
        }

        public void SetItem(ItemInstance item)
        {
            currentItem = item;
            bool hasItem = item != null && item.Definition != null;

            if (iconImage != null)
            {
                iconImage.enabled = hasItem && item.Definition.Icon != null;
                iconImage.sprite = hasItem ? item.Definition.Icon : null;

                Color iconColor = iconImage.color;
                iconColor.a = hasItem ? 1f : 0f;
                iconImage.color = iconColor;
            }

            if (stackSizeText != null)
            {
                stackSizeText.enabled = hasItem;
                stackSizeText.text = hasItem ? item.StackSize.ToString() : string.Empty;
            }

            if (stackSizeBackgroundImage != null)
            {
                stackSizeBackgroundImage.enabled = hasItem && stackSizeText != null;
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
            if (!targetable || !IsLeftPointerClick(eventData))
            {
                return;
            }

            float currentTime = Time.unscaledTime;
            if (currentTime - lastClickTime <= doubleClickThreshold)
            {
                lastClickTime = -1f;
                DoubleClicked?.Invoke(SlotType);
                return;
            }

            lastClickTime = currentTime;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!targetable
                || eventData.button != PointerEventData.InputButton.Left
                || currentItem == null
                || currentItem.Definition == null)
            {
                return;
            }

            DragStarted?.Invoke(ItemSlotReference.Consumable(SlotType), currentItem, eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!targetable)
            {
                return;
            }

            Dragged?.Invoke(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!targetable)
            {
                return;
            }

            DragEnded?.Invoke();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!targetable)
            {
                return;
            }

            Dropped?.Invoke(ItemSlotReference.Consumable(SlotType));
        }

        private static bool IsLeftPointerClick(PointerEventData eventData)
        {
            return eventData.button == PointerEventData.InputButton.Left
                && (Mouse.current != null || Touchscreen.current != null || Pen.current != null || Pointer.current != null);
        }

        private static string GetShortcutText(ConsumableSlotType slotType)
        {
            return slotType switch
            {
                ConsumableSlotType.Slot1 => "Q",
                ConsumableSlotType.Slot2 => "E",
                ConsumableSlotType.Slot3 => "Z",
                ConsumableSlotType.Slot4 => "C",
                _ => string.Empty
            };
        }
    }
}
