using RPGame.Core.Interaction;
using TMPro;
using UnityEngine;

namespace RPGame.UI.Interaction
{
    public sealed class InteractionPromptUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Interactor interactor;
        [SerializeField] private TMP_Text promptText;

        [Header("Text")]
        [SerializeField] private string promptFormat = "{0}";

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
            Refresh(interactor != null ? interactor.CurrentInteractable : null);
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (interactor != null)
            {
                interactor.CurrentInteractableChanged += OnCurrentInteractableChanged;
            }
        }

        private void Unsubscribe()
        {
            if (interactor != null)
            {
                interactor.CurrentInteractableChanged -= OnCurrentInteractableChanged;
            }
        }

        private void OnCurrentInteractableChanged(IInteractable interactable)
        {
            Refresh(interactable);
        }

        private void Refresh(IInteractable interactable)
        {
            if (promptText == null)
            {
                return;
            }

            string interactionText = interactable != null ? interactable.GetInteractionText() : string.Empty;
            bool shouldShow = !string.IsNullOrWhiteSpace(interactionText);

            promptText.text = shouldShow ? string.Format(promptFormat, interactionText) : string.Empty;
        }

        private void ResolveReferences()
        {
            if (interactor == null)
            {
                interactor = FindAnyObjectByType<Interactor>();
            }

            if (promptText == null)
            {
                promptText = GetComponentInChildren<TMP_Text>(true);
            }
        }

        private void OnValidate()
        {
            ResolveReferences();

            if (string.IsNullOrWhiteSpace(promptFormat))
            {
                promptFormat = "{0}";
            }
        }
    }
}
