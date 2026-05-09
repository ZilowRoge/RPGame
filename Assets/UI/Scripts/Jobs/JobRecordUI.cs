using System;
using RPGame.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPGame.UI.Jobs
{
    public sealed class JobRecordUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TMP_Text jobNameText;
        [SerializeField] private TMP_Text jobLevelText;
        [SerializeField] private Image expBar;
        [SerializeField] private Button levelUpButton;

        private JobInstance job;
        private Action<JobInstance> experienceAssignmentRequested;

        public event Action<JobInstance> Selected;

        private void Awake()
        {
            if (levelUpButton != null)
            {
                levelUpButton.onClick.AddListener(HandleLevelUpClicked);
            }
        }

        private void OnDestroy()
        {
            if (levelUpButton != null)
            {
                levelUpButton.onClick.RemoveListener(HandleLevelUpClicked);
            }
        }

        public void Initialize(JobInstance job, Action<JobInstance> experienceAssignmentRequested)
        {
            this.job = job;
            this.experienceAssignmentRequested = experienceAssignmentRequested;
        }

        public void Refresh(int availableXP)
        {
            if (job == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if (jobNameText != null)
            {
                jobNameText.text = job.Definition != null ? job.Definition.DisplayName : job.JobId;
            }

            if (jobLevelText != null)
            {
                jobLevelText.text = $"Level {job.CurrentLevel}";
            }

            int requiredXP = job.GetXPToNextLevel();

            if (expBar != null)
            {
                expBar.fillAmount = requiredXP > 0 ? Mathf.Clamp01((float)job.CurrentXP / requiredXP) : 1f;
            }

            if (levelUpButton != null)
            {
                levelUpButton.gameObject.SetActive(!job.IsMaxLevel && availableXP > 0 && job.GetRemainingXPToMaxLevel() > 0);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (job != null)
            {
                Selected?.Invoke(job);
            }
        }

        private void HandleLevelUpClicked()
        {
            if (job != null)
            {
                experienceAssignmentRequested?.Invoke(job);
            }
        }

        private void OnValidate()
        {
            if (expBar != null)
            {
                expBar.type = Image.Type.Filled;
                expBar.fillMethod = Image.FillMethod.Horizontal;
            }
        }
    }
}
