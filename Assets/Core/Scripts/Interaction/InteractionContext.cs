using UnityEngine;

namespace RPGame.Core.Interaction
{
    public readonly struct InteractionContext
    {
        public InteractionContext(GameObject interactorObject, Transform interactorTransform)
        {
            InteractorObject = interactorObject;
            InteractorTransform = interactorTransform;
        }

        public GameObject InteractorObject { get; }
        public Transform InteractorTransform { get; }
    }
}
