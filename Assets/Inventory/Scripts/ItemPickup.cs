using RPGame.Core.Interaction;
using RPGame.Inventory.Data;
using UnityEngine;

namespace RPGame.Inventory
{
    public sealed class ItemPickup : InteractionBase
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int amount = 1;

        public ItemDefinition Item => item;
        public int Amount => amount;

        public override bool CanInteract(InteractionContext context)
        {
            return item != null && amount > 0 && GetItemManagementController(context) != null;
        }

        public override void Interact(InteractionContext context)
        {
            ItemManagementController controller = GetItemManagementController(context);
            if (controller == null || item == null || amount <= 0)
            {
                return;
            }

            if (controller.AddItem(item, amount))
            {
                context.InteractorObject.GetComponentInParent<Interactor>()?.UnregisterInteractable(this);
                Destroy(gameObject);
            }
        }

        public override string GetInteractionText()
        {
            if (item == null)
            {
                return "Pick up";
            }

            return amount > 1 ? $"Pick up {item.Name} x{amount}" : $"Pick up {item.Name}";
        }

        private static ItemManagementController GetItemManagementController(InteractionContext context)
        {
            return context.InteractorObject != null
                ? context.InteractorObject.GetComponentInParent<ItemManagementController>()
                : null;
        }

        private void OnValidate()
        {
            amount = Mathf.Max(1, amount);
        }
    }
}
