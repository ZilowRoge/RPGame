using UnityEngine;

namespace RPGame.Core.Interaction
{
    public sealed class DebugInteractable : InteractionBase
    {
        [SerializeField] private string interactionText = "Interact";
        [SerializeField] private bool canInteract = true;

        public override bool CanInteract(InteractionContext context)
        {
            return canInteract;
        }

        public override void Interact(InteractionContext context)
        {
            Debug.Log($"{context.InteractorObject.name} interacted with {name}.", this);
        }

        public override string GetInteractionText()
        {
            return interactionText;
        }
    }
}
