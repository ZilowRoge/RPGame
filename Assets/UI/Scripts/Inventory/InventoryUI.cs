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
            bool equipped = controller.EquipFromInventory(slotIndex);
            Debug.Log($"Equip from inventory slot {slotIndex}: {equipped}", this);
        }

        private void HandleSlotPointerEntered(ItemInstance item, Vector2 screenPosition)
        {
            tooltip?.Show(item, screenPosition);
        }

        private void HandleSlotPointerExited()
        {
            tooltip?.Hide();
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
                    Destroy(slots[i].gameObject);
                }
            }

            slots.Clear();
        }
    }
}
