using RPGame.Inventory.Logic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGame.Inventory
{
    public sealed class ConsumableQuickUseController : MonoBehaviour
    {
        [SerializeField] private ItemManagementController itemManagementController;
        [SerializeField] private InputActionReference useConsumable1Action;
        [SerializeField] private InputActionReference useConsumable2Action;
        [SerializeField] private InputActionReference useConsumable3Action;
        [SerializeField] private InputActionReference useConsumable4Action;

        private void Awake()
        {
            itemManagementController ??= GetComponent<ItemManagementController>();
        }

        private void OnEnable()
        {
            RegisterAction(useConsumable1Action, UseConsumable1);
            RegisterAction(useConsumable2Action, UseConsumable2);
            RegisterAction(useConsumable3Action, UseConsumable3);
            RegisterAction(useConsumable4Action, UseConsumable4);
        }

        private void OnDisable()
        {
            UnregisterAction(useConsumable1Action, UseConsumable1);
            UnregisterAction(useConsumable2Action, UseConsumable2);
            UnregisterAction(useConsumable3Action, UseConsumable3);
            UnregisterAction(useConsumable4Action, UseConsumable4);
        }

        private void UseConsumable1(InputAction.CallbackContext context)
        {
            UseConsumable(ConsumableSlotType.Slot1);
        }

        private void UseConsumable2(InputAction.CallbackContext context)
        {
            UseConsumable(ConsumableSlotType.Slot2);
        }

        private void UseConsumable3(InputAction.CallbackContext context)
        {
            UseConsumable(ConsumableSlotType.Slot3);
        }

        private void UseConsumable4(InputAction.CallbackContext context)
        {
            UseConsumable(ConsumableSlotType.Slot4);
        }

        private void UseConsumable(ConsumableSlotType slotType)
        {
            if (itemManagementController == null)
            {
                return;
            }

            itemManagementController.UseConsumableSlot(slotType);
        }

        private static void RegisterAction(
            InputActionReference actionReference,
            System.Action<InputAction.CallbackContext> callback)
        {
            if (actionReference?.action == null)
            {
                return;
            }

            actionReference.action.performed += callback;
            actionReference.action.Enable();
        }

        private static void UnregisterAction(
            InputActionReference actionReference,
            System.Action<InputAction.CallbackContext> callback)
        {
            if (actionReference?.action == null)
            {
                return;
            }

            actionReference.action.performed -= callback;
        }
    }
}
