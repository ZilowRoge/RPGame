using System.Collections.Generic;
using RPGame.Inventory;
using RPGame.Inventory.Data;
using RPGame.Inventory.Logic;
using UnityEngine;

namespace RPGame.UI.Inventory
{
    public sealed class EquipmentUI : MonoBehaviour
    {
        [SerializeField] private ItemManagementController controller;
        [SerializeField] private EquipmentSlotUI slotPrefab;
        [SerializeField] private Transform slotsRoot;
        [SerializeField] private ConsumableSlotUI consumableSlotPrefab;
        [SerializeField] private Transform consumableSlotsRoot;
        [SerializeField] private ItemTooltipUI tooltip;
        [SerializeField] private ItemDragDropUI dragDrop;
        [SerializeField] private float doubleClickThreshold = 0.3f;

        private readonly List<EquipmentSlotUI> slots = new();
        private readonly List<ConsumableSlotUI> consumableSlots = new();

        private void OnEnable()
        {
            if (controller == null)
            {
                return;
            }

            controller.OnEquipmentChanged += Refresh;
            controller.OnConsumableSlotsChanged += Refresh;
            RebuildSlotsIfNeeded();
            Refresh();
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.OnEquipmentChanged -= Refresh;
                controller.OnConsumableSlotsChanged -= Refresh;
            }

            tooltip?.Hide();
        }

        private void RebuildSlotsIfNeeded()
        {
            RebuildEquipmentSlotsIfNeeded();
            RebuildConsumableSlotsIfNeeded();
        }

        private void RebuildEquipmentSlotsIfNeeded()
        {
            if (slotPrefab == null || slotsRoot == null || controller.Equipment == null || slots.Count == controller.Equipment.Slots.Count)
            {
                return;
            }

            ClearEquipmentSlots();

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

        private void RebuildConsumableSlotsIfNeeded()
        {
            if (consumableSlotPrefab == null
                || consumableSlotsRoot == null
                || controller.ConsumableSlots == null
                || consumableSlots.Count == controller.ConsumableSlots.Slots.Count)
            {
                return;
            }

            ClearConsumableSlots();

            foreach (ConsumableSlot consumableSlot in controller.ConsumableSlots.Slots)
            {
                ConsumableSlotUI slot = Instantiate(consumableSlotPrefab, consumableSlotsRoot);
                slot.Initialize(consumableSlot.SlotType, doubleClickThreshold);
                slot.DoubleClicked += HandleConsumableSlotDoubleClicked;
                slot.PointerEntered += HandleSlotPointerEntered;
                slot.PointerExited += HandleSlotPointerExited;
                slot.DragStarted += HandleSlotDragStarted;
                slot.Dragged += HandleSlotDragged;
                slot.DragEnded += HandleSlotDragEnded;
                slot.Dropped += HandleSlotDropped;
                consumableSlots.Add(slot);
            }
        }

        private void Refresh()
        {
            if (controller == null || controller.Equipment == null || controller.ConsumableSlots == null)
            {
                return;
            }

            RebuildSlotsIfNeeded();

            for (int i = 0; i < slots.Count; i++)
            {
                EquipmentSlot equipmentSlot = controller.Equipment.GetSlot(slots[i].SlotType);
                slots[i].SetItem(equipmentSlot?.Item);
            }

            for (int i = 0; i < consumableSlots.Count; i++)
            {
                ConsumableSlot consumableSlot = controller.ConsumableSlots.GetSlot(consumableSlots[i].SlotType);
                consumableSlots[i].SetItem(consumableSlot?.Item);
            }
        }

        private void HandleSlotDoubleClicked(EquipmentSlotType slotType)
        {
            tooltip?.Hide();
            controller.UnequipToInventory(slotType);
        }

        private void HandleConsumableSlotDoubleClicked(ConsumableSlotType slotType)
        {
            tooltip?.Hide();
            controller.UseConsumableSlot(slotType);
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

        private void ClearEquipmentSlots()
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

        private void ClearConsumableSlots()
        {
            for (int i = 0; i < consumableSlots.Count; i++)
            {
                if (consumableSlots[i] != null)
                {
                    consumableSlots[i].DoubleClicked -= HandleConsumableSlotDoubleClicked;
                    consumableSlots[i].PointerEntered -= HandleSlotPointerEntered;
                    consumableSlots[i].PointerExited -= HandleSlotPointerExited;
                    consumableSlots[i].DragStarted -= HandleSlotDragStarted;
                    consumableSlots[i].Dragged -= HandleSlotDragged;
                    consumableSlots[i].DragEnded -= HandleSlotDragEnded;
                    consumableSlots[i].Dropped -= HandleSlotDropped;
                    Destroy(consumableSlots[i].gameObject);
                }
            }

            consumableSlots.Clear();
        }
    }
}
