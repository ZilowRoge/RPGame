using UnityEngine;

namespace RPGame.Core.Interaction
{
    public interface IInteractable
    {
        Transform InteractionTransform { get; }
        bool CanInteract(InteractionContext context);
        void Interact(InteractionContext context);
        string GetInteractionText();
    }
}
