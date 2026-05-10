using System;
using RPGame.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGame.UI.Jobs
{
    public sealed class JobExperienceAssignmentUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text jobNameText;
        [SerializeField] private TMP_Text jobLevelText;
        [SerializeField] private TMP_Text availableExpText;
        [SerializeField] private TMP_Text assignedExpText;
        [SerializeField] private Slider expSlider;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Color jobLevelChangedColor = Color.green;
        [SerializeField] private int levelThresholdSnapRange = 5;
        [SerializeField] private int levelThresholdReleaseRange = 15;

        private CharacterProgression progression;
        private JobInstance job;
        private Action confirmed;
        private int initialAvailableXP;
        private int snappedThreshold = -1;
        private int lastAssignedXP;
        private Color defaultJobLevelTextColor;
        private bool hasDefaultJobLevelTextColor;

        private void Awake()
        {
            ResolveCanvasGroup();
            CacheDefaultJobLevelTextColor();

            if (expSlider != null)
            {
                expSlider.wholeNumbers = true;
                expSlider.onValueChanged.AddListener(HandleSliderValueChanged);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(Cancel);
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(Confirm);
            }
        }

        private void OnDestroy()
        {
            if (expSlider != null)
            {
                expSlider.onValueChanged.RemoveListener(HandleSliderValueChanged);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(Cancel);
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(Confirm);
            }
        }

        public void Open(CharacterProgression progression, JobInstance job, Action confirmed)
        {
            ResolveCanvasGroup();
            CacheDefaultJobLevelTextColor();
            this.progression = progression;
            this.job = job;
            this.confirmed = confirmed;
            initialAvailableXP = progression != null ? progression.GetAvailableXP() : 0;
            snappedThreshold = -1;
            lastAssignedXP = 0;

            int maxAssignableXP = GetMaxAssignableXP();

            if (jobNameText != null)
            {
                jobNameText.text = job?.Definition != null ? job.Definition.DisplayName : job?.JobId ?? string.Empty;
            }

            if (jobLevelText != null)
            {
                SetJobLevelText(job != null ? job.CurrentLevel : 0);
            }

            if (expSlider != null)
            {
                expSlider.minValue = 0f;
                expSlider.maxValue = maxAssignableXP;
                expSlider.SetValueWithoutNotify(0f);
            }

            RefreshValues(0);
            SetCanvasGroupInteractable(true);
            gameObject.SetActive(true);
        }

        private void Cancel()
        {
            Hide();
        }

        private void Confirm()
        {
            int assignedXP = GetAssignedXP();
            if (CanAssignXP(assignedXP) && progression.Jobs.AddXPToJob(job.JobId, assignedXP))
            {
                confirmed?.Invoke();
                Hide();
            }
        }

        private void HandleSliderValueChanged(float value)
        {
            int assignedXP = Mathf.RoundToInt(value);

            if (snappedThreshold >= 0)
            {
                if (Mathf.Abs(assignedXP - snappedThreshold) <= levelThresholdReleaseRange)
                {
                    RefreshValues(assignedXP);
                    lastAssignedXP = assignedXP;
                    return;
                }

                snappedThreshold = -1;
            }

            if (TryGetSnappedValue(assignedXP, out int snappedValue))
            {
                snappedThreshold = snappedValue;
                if (expSlider != null)
                {
                    expSlider.SetValueWithoutNotify(snappedValue);
                }

                RefreshValues(snappedValue);
                lastAssignedXP = snappedValue;
                return;
            }

            RefreshValues(assignedXP);
            lastAssignedXP = assignedXP;
        }

        private int GetAssignedXP()
        {
            return expSlider != null ? Mathf.Clamp(Mathf.RoundToInt(expSlider.value), 0, GetMaxAssignableXP()) : 0;
        }

        private int GetMaxAssignableXP()
        {
            if (job == null || job.IsMaxLevel)
            {
                return 0;
            }

            return Mathf.Min(initialAvailableXP, job.GetRemainingXPToMaxLevel());
        }

        private void RefreshValues(int assignedXP)
        {
            int clampedAssignedXP = Mathf.Clamp(assignedXP, 0, GetMaxAssignableXP());

            if (availableExpText != null)
            {
                availableExpText.text = Mathf.Max(0, initialAvailableXP - clampedAssignedXP).ToString();
            }

            if (assignedExpText != null)
            {
                assignedExpText.text = clampedAssignedXP.ToString();
            }

            SetJobLevelText(GetPreviewLevel(clampedAssignedXP));

            if (confirmButton != null)
            {
                confirmButton.interactable = CanAssignXP(clampedAssignedXP);
            }
        }

        private bool CanAssignXP(int assignedXP)
        {
            return progression != null
                && job != null
                && progression.Jobs.CanAddXPToJob(job.JobId, assignedXP);
        }

        private int GetPreviewLevel(int assignedXP)
        {
            if (job == null || job.Definition == null)
            {
                return 0;
            }

            int previewLevel = job.CurrentLevel;
            int previewXP = job.CurrentXP + Mathf.Max(0, assignedXP);

            while (previewLevel < job.Definition.MaxLevel)
            {
                int requiredXP = job.Definition.GetRequiredExperience(previewLevel);
                if (previewXP < requiredXP)
                {
                    break;
                }

                previewXP = Mathf.Max(0, previewXP - requiredXP);
                previewLevel++;
            }

            return previewLevel;
        }

        private void SetJobLevelText(int level)
        {
            if (jobLevelText != null)
            {
                jobLevelText.color = defaultJobLevelTextColor;

                if (job == null || level <= 0)
                {
                    jobLevelText.text = string.Empty;
                    return;
                }

                jobLevelText.text = level != job.CurrentLevel
                    ? $"{job.CurrentLevel} -> <color=#{ColorUtility.ToHtmlStringRGBA(jobLevelChangedColor)}>{level}</color>"
                    : $"{job.CurrentLevel}";
            }
        }

        private void CacheDefaultJobLevelTextColor()
        {
            if (hasDefaultJobLevelTextColor || jobLevelText == null)
            {
                return;
            }

            defaultJobLevelTextColor = jobLevelText.color;
            hasDefaultJobLevelTextColor = true;
        }

        private bool TryGetSnappedValue(int assignedXP, out int snappedValue)
        {
            snappedValue = 0;

            if (job == null || job.Definition == null || levelThresholdSnapRange <= 0)
            {
                return false;
            }

            int maxAssignableXP = GetMaxAssignableXP();
            int currentLevel = job.CurrentLevel;
            int currentXP = job.CurrentXP;
            int cumulativeXP = 0;
            int closestDistance = int.MaxValue;

            while (currentLevel < job.Definition.MaxLevel)
            {
                int requiredXP = job.Definition.GetRequiredExperience(currentLevel);
                int xpToNextLevel = Mathf.Max(0, requiredXP - currentXP);

                if (xpToNextLevel > int.MaxValue - cumulativeXP)
                {
                    break;
                }

                cumulativeXP += xpToNextLevel;
                if (cumulativeXP > maxAssignableXP)
                {
                    break;
                }

                int distance = Mathf.Abs(assignedXP - cumulativeXP);
                if ((distance <= levelThresholdSnapRange || DidCrossThreshold(lastAssignedXP, assignedXP, cumulativeXP))
                    && distance < closestDistance)
                {
                    closestDistance = distance;
                    snappedValue = cumulativeXP;
                }

                currentLevel++;
                currentXP = 0;
            }

            return closestDistance != int.MaxValue;
        }

        private static bool DidCrossThreshold(int previousXP, int currentXP, int thresholdXP)
        {
            return previousXP < thresholdXP && currentXP > thresholdXP
                || previousXP > thresholdXP && currentXP < thresholdXP;
        }

        private void Hide()
        {
            SetCanvasGroupInteractable(false);
        }

        private void ResolveCanvasGroup()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void SetCanvasGroupInteractable(bool isInteractable)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = isInteractable ? 1f : 0f;
            canvasGroup.interactable = isInteractable;
            canvasGroup.blocksRaycasts = isInteractable;
        }

        private void OnValidate()
        {
            levelThresholdSnapRange = Mathf.Max(0, levelThresholdSnapRange);
            levelThresholdReleaseRange = Mathf.Max(levelThresholdSnapRange + 1, levelThresholdReleaseRange);
        }
    }
}
