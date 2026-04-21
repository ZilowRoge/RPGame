using UnityEngine;

namespace RPGame.Core.Interaction
{
    public abstract class InteractionBase : MonoBehaviour, IInteractable
    {
        public virtual Transform InteractionTransform => transform;

        public abstract bool CanInteract(InteractionContext context);
        public abstract void Interact(InteractionContext context);
        public abstract string GetInteractionText();

        protected virtual void OnTriggerEnter(Collider other)
        {
            Interactor interactor = other.GetComponentInParent<Interactor>();

            if (interactor != null)
            {
                interactor.RegisterInteractable(this);
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            Interactor interactor = other.GetComponentInParent<Interactor>();

            if (interactor != null)
            {
                interactor.UnregisterInteractable(this);
            }
        }
    }
}
