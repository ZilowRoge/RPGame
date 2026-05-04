using System.Collections.Generic;
using RPGame.Core.Inventory;
using RPGame.Core.Inventory.Data;
using RPGame.Core.Inventory.Logic;
using UnityEngine;

namespace RPGame.UI.Inventory
{
    public sealed class EquipmentUI : MonoBehaviour
    {
        [SerializeField] private ItemManagementController controller;
        [SerializeField] private EquipmentSlotUI slotPrefab;
        [SerializeField] private Transform slotsRoot;
        [SerializeField] private ItemTooltipUI tooltip;
        [SerializeField] private ItemDragDropUI dragDrop;
        [SerializeField] private float doubleClickThreshold = 0.3f;

        private readonly List<EquipmentSlotUI> slots = new();

        private void OnEnable()
        {
            if (controller == null)
            {
                return;
            }

            controller.OnEquipmentChanged += Refresh;
            RebuildSlotsIfNeeded();
            Refresh();
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.OnEquipmentChanged -= Refresh;
            }

            tooltip?.Hide();
        }

        private void RebuildSlotsIfNeeded()
        {
            if (slotPrefab == null || slotsRoot == null || controller.Equipment == null || slots.Count == controller.Equipment.Slots.Count)
            {
                return;
            }

            ClearSlots();

            foreach (EquipmentSlot equipmentSlot in controller.Equipment.Slots)
            {
                EquipmentSlotUI slot = Instantiate(slotPrefab, slotsRoot);
                slot.Initialize(equipmentSlot.SlotType, doubleClickThreshold);
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
            if (controller == null || controller.Equipment == null)
            {
                return;
            }

            RebuildSlotsIfNeeded();

            for (int i = 0; i < slots.Count; i++)
            {
                EquipmentSlot equipmentSlot = controller.Equipment.GetSlot(slots[i].SlotType);
                slots[i].SetItem(equipmentSlot?.Item);
            }
        }

        private void HandleSlotDoubleClicked(EquipmentSlotType slotType)
        {
            tooltip?.Hide();
            bool unequipped = controller.UnequipToInventory(slotType);
            Debug.Log($"Unequip from equipment slot {slotType}: {unequipped}", this);
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
            bool moved = dragDrop != null && dragDrop.Drop(target, controller);
            Debug.Log($"Drop to equipment slot {target.EquipmentSlotType}: {moved}", this);
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
