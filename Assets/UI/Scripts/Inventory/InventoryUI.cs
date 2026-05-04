using System.Collections.Generic;
using RPGame.Core.Inventory;
using RPGame.Core.Inventory.Logic;
using UnityEngine;

namespace RPGame.UI.Inventory
{
    public sealed class InventoryUI : MonoBehaviour
    {
        [SerializeField] private ItemManagementController controller;
        [SerializeField] private InventorySlotUI slotPrefab;
        [SerializeField] private Transform slotsRoot;
        [SerializeField] private ItemTooltipUI tooltip;
        [SerializeField] private ItemDragDropUI dragDrop;
        [SerializeField] private float doubleClickThreshold = 0.3f;

        private readonly List<InventorySlotUI> slots = new();

        private void OnEnable()
        {
            if (controller == null)
            {
                return;
            }

            controller.OnInventoryChanged += Refresh;
            RebuildSlotsIfNeeded();
            Refresh();
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.OnInventoryChanged -= Refresh;
            }

            tooltip?.Hide();
        }

        private void RebuildSlotsIfNeeded()
        {
            if (slotPrefab == null || slotsRoot == null || controller.Inventory == null || slots.Count == controller.Inventory.Size)
            {
                return;
            }

            ClearSlots();

            for (int i = 0; i < controller.Inventory.Size; i++)
            {
                InventorySlotUI slot = Instantiate(slotPrefab, slotsRoot);
                slot.Initialize(i, doubleClickThreshold);
                slot.DoubleClicked += HandleSlotDoubleClicked;
                slot.PointerEntered += HandleSlotPointerEntered;
                slot.PointerExited += HandleSlotPointerExited;
                slot.DragStarted += HandleSlotDragStarted;
                slot.Dragged += HandleSlotDragged;
                slot.DragEnded += HandleSlotDragEnded;
                slot.Dropped += HandleSlotDropped;
                slots.Add(slot);
            }
        }

        private void Refresh()
        {
            if (controller == null || controller.Inventory == null)
            {
                return;
            }

            RebuildSlotsIfNeeded();

            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlot slot = controller.Inventory.GetSlot(i);
                slots[i].SetItem(slot?.Item);
            }
        }

        private void HandleSlotDoubleClicked(int slotIndex)
        {
            tooltip?.Hide();
            controller.EquipFromInventory(slotIndex);
        }

        private void HandleSlotPointerEntered(ItemInstance item, Vector2 screenPosition)
        {
            tooltip?.Show(item, screenPosition);
        }

        private void HandleSlotPointerExited()
        {
            tooltip?.Hide();
        }

        private void HandleSlotDragStarted(ItemSlotReference source, ItemInstance item, Vector2 screenPosition)
        {
            tooltip?.Hide();
            dragDrop?.BeginDrag(source, item, screenPosition);
        }

        private void HandleSlotDragged(Vector2 screenPosition)
        {
            dragDrop?.Move(screenPosition);
        }

        private void HandleSlotDragEnded()
        {
            dragDrop?.EndDrag();
        }

        private void HandleSlotDropped(ItemSlotReference target)
        {
            dragDrop?.Drop(target, controller);
        }

        private void ClearSlots()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].DoubleClicked -= HandleSlotDoubleClicked;
                    slots[i].PointerEntered -= HandleSlotPointerEntered;
                    slots[i].PointerExited -= HandleSlotPointerExited;
                    slots[i].DragStarted -= HandleSlotDragStarted;
                    slots[i].Dragged -= HandleSlotDragged;
                    slots[i].DragEnded -= HandleSlotDragEnded;
                    slots[i].Dropped -= HandleSlotDropped;
                    Destroy(slots[i].gameObject);
                }
            }

            slots.Clear();
        }
    }
}
