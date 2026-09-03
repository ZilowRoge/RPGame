using RPGame.Inventory;
using RPGame.Inventory.Logic;
using UnityEngine;

namespace RPGame.UI.Inventory
{
    public sealed class QuickUseHUD : MonoBehaviour
    {
        [SerializeField] private ItemManagementController controller;
        [SerializeField] private ConsumableSlotUI slot1;
        [SerializeField] private ConsumableSlotUI slot2;
        [SerializeField] private ConsumableSlotUI slot3;
        [SerializeField] private ConsumableSlotUI slot4;

        private void Awake()
        {
            controller ??= GetComponentInParent<ItemManagementController>();
            InitializeSlots();
        }

        private void OnEnable()
        {
            if (controller == null)
            {
                return;
            }

            controller.OnConsumableSlotsChanged += Refresh;
            InitializeSlots();
            Refresh();
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.OnConsumableSlotsChanged -= Refresh;
            }
        }

        private void InitializeSlots()
        {
            InitializeSlot(slot1, ConsumableSlotType.Slot1);
            InitializeSlot(slot2, ConsumableSlotType.Slot2);
            InitializeSlot(slot3, ConsumableSlotType.Slot3);
            InitializeSlot(slot4, ConsumableSlotType.Slot4);
        }

        private void Refresh()
        {
            if (controller == null || controller.ConsumableSlots == null)
            {
                return;
            }

            RefreshSlot(slot1, ConsumableSlotType.Slot1);
            RefreshSlot(slot2, ConsumableSlotType.Slot2);
            RefreshSlot(slot3, ConsumableSlotType.Slot3);
            RefreshSlot(slot4, ConsumableSlotType.Slot4);
        }

        private void RefreshSlot(ConsumableSlotUI slot, ConsumableSlotType slotType)
        {
            if (slot == null)
            {
                return;
            }

            ConsumableSlot consumableSlot = controller.ConsumableSlots.GetSlot(slotType);
            slot.SetItem(consumableSlot?.Item);
        }

        private static void InitializeSlot(ConsumableSlotUI slot, ConsumableSlotType slotType)
        {
            if (slot == null)
            {
                return;
            }

            slot.Targetable = false;
            slot.Initialize(slotType, 0f);
        }
    }
}
