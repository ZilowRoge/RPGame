using System;
using RPGame.Progression;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPGame.UI.Jobs
{
    public sealed class PerkNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private Color lockedColor = new(0.35f, 0.35f, 0.35f, 1f);
        [SerializeField] private Color availableColor = new(0.95f, 0.82f, 0.35f, 1f);
        [SerializeField] private Color pendingColor = new(0.35f, 0.65f, 1f, 1f);
        [SerializeField] private Color unlockedColor = new(0.35f, 0.85f, 0.48f, 1f);
        [SerializeField] private Color hoverColor = Color.white;

        private PerkDefinition perk;
        private Func<PerkDefinition, PerkUnlockState> getUnlockState;
        private Func<PerkDefinition, bool> isPerkPending;
        private Action<PerkDefinition> togglePendingPerk;
        private Action<PerkDefinition, Vector2> showTooltip;
        private Action hideTooltip;
        private Action refreshRequested;
        private bool isPointerInside;

        public void Initialize(
            PerkDefinition perk,
            Func<PerkDefinition, PerkUnlockState> getUnlockState,
            Func<PerkDefinition, bool> isPerkPending,
            Action<PerkDefinition> togglePendingPerk,
            Action<PerkDefinition, Vector2> showTooltip,
            Action hideTooltip,
            Action refreshRequested)
        {
            this.perk = perk;
            this.getUnlockState = getUnlockState;
            this.isPerkPending = isPerkPending;
            this.togglePendingPerk = togglePendingPerk;
            this.showTooltip = showTooltip;
            this.hideTooltip = hideTooltip;
            this.refreshRequested = refreshRequested;

            ResolveReferences();
            Refresh();
        }

        public void Refresh()
        {
            PerkUnlockState unlockState = GetUnlockState();
            bool isPending = IsPending();

            if (targetGraphic != null)
            {
                targetGraphic.color = isPointerInside && unlockState == PerkUnlockState.Available && !isPending
                    ? hoverColor
                    : GetColor(unlockState, isPending);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerInside = true;
            showTooltip?.Invoke(perk, eventData != null ? eventData.position : Vector2.zero);
            Refresh();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerInside = false;
            hideTooltip?.Invoke();
            Refresh();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (togglePendingPerk == null || perk == null)
            {
                return;
            }

            PerkUnlockState unlockState = GetUnlockState();
            if (unlockState != PerkUnlockState.Available && !IsPending())
            {
                return;
            }

            togglePendingPerk(perk);
            refreshRequested?.Invoke();
        }

        private PerkUnlockState GetUnlockState()
        {
            return getUnlockState != null
                ? getUnlockState(perk)
                : PerkUnlockState.Locked;
        }

        private bool IsPending()
        {
            return isPerkPending != null && isPerkPending(perk);
        }

        private Color GetColor(PerkUnlockState unlockState, bool isPending)
        {
            if (isPending)
            {
                return pendingColor;
            }

            return unlockState switch
            {
                PerkUnlockState.Available => availableColor,
                PerkUnlockState.Unlocked => unlockedColor,
                _ => lockedColor
            };
        }

        private void ResolveReferences()
        {
            if (targetGraphic == null)
            {
                targetGraphic = GetComponent<Graphic>();
            }
        }
    }
}
