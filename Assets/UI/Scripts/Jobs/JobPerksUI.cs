using System.Collections.Generic;
using RPGame.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RPGame.UI.Jobs
{
    public sealed class JobPerksUI : MonoBehaviour
    {
        [SerializeField] private CharacterProgression progression;
        [SerializeField] private TMP_Text jobNameText;
        [SerializeField] private TMP_Text availableJobPointsText;
        [SerializeField] private RectTransform perkNodesParent;
        [SerializeField] private PerkTreeConnectionsGraphic connectionsGraphic;
        [SerializeField] private PerkTooltipUI perkTooltip;
        [FormerlySerializedAs("startingPerkNodePrefab")]
        [SerializeField] private RectTransform rootNodePrefab;
        [SerializeField] private RectTransform perkNodePrefab;
        [SerializeField] private Button confirmPerkUnlocksButton;
        [SerializeField] private Button cancelPerkUnlocksButton;

        private readonly List<GameObject> spawnedPerkNodes = new();
        private readonly PendingPerkUnlocks pendingPerkUnlocks = new();
        private JobInstance job;

        private void Awake()
        {
            ResolveProgression();

            if (confirmPerkUnlocksButton != null)
            {
                confirmPerkUnlocksButton.onClick.AddListener(ConfirmPendingPerkUnlocks);
            }

            if (cancelPerkUnlocksButton != null)
            {
                cancelPerkUnlocksButton.onClick.AddListener(CancelPendingPerkUnlocks);
            }
        }

        private void OnDestroy()
        {
            if (confirmPerkUnlocksButton != null)
            {
                confirmPerkUnlocksButton.onClick.RemoveListener(ConfirmPendingPerkUnlocks);
            }

            if (cancelPerkUnlocksButton != null)
            {
                cancelPerkUnlocksButton.onClick.RemoveListener(CancelPendingPerkUnlocks);
            }
        }

        private void OnEnable()
        {
            ResolveProgression();
            Refresh();
        }

        public void SetProgression(CharacterProgression progression)
        {
            if (this.progression == progression)
            {
                return;
            }

            this.progression = progression;
            ClearPendingPerks();
            Refresh();
        }

        public void SetJob(JobInstance job)
        {
            this.job = job;
            ClearPendingPerks();
            ResolveProgression();
            Refresh();
        }

        public void Refresh()
        {
            if (jobNameText != null)
            {
                jobNameText.text = job?.Definition != null ? job.Definition.DisplayName : job?.JobId ?? string.Empty;
            }

            if (availableJobPointsText != null)
            {
                int availableJobPoints = job != null ? job.JobPoints : 0;
                int pendingCost = pendingPerkUnlocks.GetPendingCost();
                availableJobPointsText.text = pendingCost > 0
                    ? $"Job Points: {availableJobPoints} (-{pendingCost})"
                    : $"Job Points: {availableJobPoints}";
            }

            RefreshConfirmButton();

            PerkTreeBuilder.Rebuild(
                job,
                perkNodesParent,
                connectionsGraphic,
                rootNodePrefab,
                perkNodePrefab,
                spawnedPerkNodes,
                GetPerkUnlockState,
                IsPerkPending,
                TogglePendingPerk,
                ShowPerkTooltip,
                HidePerkTooltip,
                Refresh);
        }

        private void ResolveProgression()
        {
            if (progression == null)
            {
                progression = FindAnyObjectByType<CharacterProgression>();
            }
        }

        private PerkUnlockState GetPerkUnlockState(PerkDefinition perk)
        {
            return pendingPerkUnlocks.GetPreviewUnlockState(job, perk, progression);
        }

        private bool IsPerkPending(PerkDefinition perk)
        {
            return pendingPerkUnlocks.IsPending(perk);
        }

        private void TogglePendingPerk(PerkDefinition perk)
        {
            pendingPerkUnlocks.Toggle(job, perk, progression);
        }

        private void ShowPerkTooltip(PerkDefinition perk, Vector2 screenPosition)
        {
            if (perkTooltip != null)
            {
                perkTooltip.Show(perk, screenPosition);
            }
        }

        private void HidePerkTooltip()
        {
            if (perkTooltip != null)
            {
                perkTooltip.Hide();
            }
        }

        private void ConfirmPendingPerkUnlocks()
        {
            if (progression == null || job == null || !pendingPerkUnlocks.HasPendingPerks)
            {
                return;
            }

            IReadOnlyList<PerkDefinition> pendingPerks = pendingPerkUnlocks.PendingPerks;
            for (int i = 0; i < pendingPerks.Count; i++)
            {
                progression.TryUnlockPerk(job, pendingPerks[i]);
            }

            ClearPendingPerks();
            Refresh();
        }

        private void CancelPendingPerkUnlocks()
        {
            if (!pendingPerkUnlocks.HasPendingPerks)
            {
                return;
            }

            ClearPendingPerks();
            Refresh();
        }

        private void ClearPendingPerks()
        {
            pendingPerkUnlocks.Clear();
        }

        private void RefreshConfirmButton()
        {
            bool hasPendingPerks = pendingPerkUnlocks.HasPendingPerks;

            if (confirmPerkUnlocksButton != null)
            {
                confirmPerkUnlocksButton.interactable = hasPendingPerks;
            }

            if (cancelPerkUnlocksButton != null)
            {
                cancelPerkUnlocksButton.interactable = hasPendingPerks;
            }
        }
    }
}
