using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGame.Core.Interaction
{
    public sealed class Interactor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform interactionOrigin;
        [SerializeField] private PlayerInput playerInput;

        [Header("Input")]
        [SerializeField] private InputActionProperty interactAction;
        [SerializeField] private string interactActionName = "Interact";

        [Header("Selection")]
        [SerializeField, Range(-1f, 1f)] private float minimumForwardDot;
        [SerializeField] private float forwardWeight = 2f;
        [SerializeField] private float distanceWeight = 1f;

        private readonly List<IInteractable> candidates = new();
        private InputAction resolvedInteractAction;

        public event Action<IInteractable> CurrentInteractableChanged;

        public IInteractable CurrentInteractable { get; private set; }
        public string CurrentInteractionText => !IsInvalidInteractable(CurrentInteractable)
            ? CurrentInteractable.GetInteractionText()
            : string.Empty;
        public bool HasInteractable => !IsInvalidInteractable(CurrentInteractable);

        private void Awake()
        {
            if (interactionOrigin == null)
            {
                interactionOrigin = transform;
            }

            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }

            ResolveInputActions();
        }

        private void OnEnable()
        {
            ResolveInputActions();
            EnableAction(resolvedInteractAction);
        }

        private void OnDisable()
        {
            DisableAction(resolvedInteractAction);
        }

        private void Update()
        {
            if (candidates.Count <= 0)
            {
                SetCurrentInteractable(null);
                return;
            }

            RefreshCurrentInteractable();

            if (CurrentInteractable != null && WasInteractPressedThisFrame())
            {
                CurrentInteractable.Interact(CreateContext());
            }
        }

        public void RegisterInteractable(IInteractable interactable)
        {
            if (IsInvalidInteractable(interactable) || ContainsInteractable(interactable))
            {
                return;
            }

            candidates.Add(interactable);
            RefreshCurrentInteractable();
        }

        public void UnregisterInteractable(IInteractable interactable)
        {
            if (interactable == null)
            {
                return;
            }

            for (int i = candidates.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(candidates[i], interactable))
                {
                    candidates.RemoveAt(i);
                }
            }

            if (ReferenceEquals(CurrentInteractable, interactable))
            {
                RefreshCurrentInteractable();
            }
        }

        public void RefreshCurrentInteractable()
        {
            SetCurrentInteractable(candidates.Count > 0 ? FindBestInteractable() : null);
        }

        private IInteractable FindBestInteractable()
        {
            for (int i = candidates.Count - 1; i >= 0; i--)
            {
                IInteractable candidate = candidates[i];

                if (IsInvalidInteractable(candidate))
                {
                    candidates.RemoveAt(i);
                }
            }

            return SelectionUtility.SelectBest(
                candidates,
                CreateContext(),
                interactionOrigin.position,
                interactionOrigin.forward,
                minimumForwardDot,
                forwardWeight,
                distanceWeight);
        }

        private bool ContainsInteractable(IInteractable interactable)
        {
            for (int i = candidates.Count - 1; i >= 0; i--)
            {
                if (IsInvalidInteractable(candidates[i]))
                {
                    candidates.RemoveAt(i);
                    continue;
                }

                if (ReferenceEquals(candidates[i], interactable))
                {
                    return true;
                }
            }

            return false;
        }

        private void SetCurrentInteractable(IInteractable interactable)
        {
            if (ReferenceEquals(CurrentInteractable, interactable))
            {
                return;
            }

            CurrentInteractable = interactable;
            CurrentInteractableChanged?.Invoke(CurrentInteractable);
        }

        private bool WasInteractPressedThisFrame()
        {
            if (resolvedInteractAction != null)
            {
                return resolvedInteractAction.WasPressedThisFrame();
            }

            return Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame
                || Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame;
        }

        private InteractionContext CreateContext()
        {
            return new InteractionContext(gameObject, transform);
        }

        private void ResolveInputActions()
        {
            resolvedInteractAction = ResolveAction(interactAction, interactActionName);
        }

        private InputAction ResolveAction(InputActionProperty actionProperty, string actionName)
        {
            if (playerInput != null && playerInput.actions != null && !string.IsNullOrWhiteSpace(actionName))
            {
                InputAction playerAction = playerInput.actions.FindAction(actionName, false);

                if (playerAction != null)
                {
                    return playerAction;
                }
            }

            return HasBindings(actionProperty.action) ? actionProperty.action : null;
        }

        private static void EnableAction(InputAction action)
        {
            if (action != null)
            {
                action.Enable();
            }
        }

        private static void DisableAction(InputAction action)
        {
            if (action != null)
            {
                action.Disable();
            }
        }

        private static bool HasBindings(InputAction action)
        {
            return action != null && action.bindings.Count > 0;
        }

        private static bool IsInvalidInteractable(IInteractable interactable)
        {
            if (interactable == null || interactable is UnityEngine.Object unityObject && unityObject == null)
            {
                return true;
            }

            return interactable.InteractionTransform == null;
        }

        private void OnValidate()
        {
            if (interactionOrigin == null)
            {
                interactionOrigin = transform;
            }

            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }

            forwardWeight = Mathf.Max(0f, forwardWeight);
            distanceWeight = Mathf.Max(0f, distanceWeight);
        }
    }
}
