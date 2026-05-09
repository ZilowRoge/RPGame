using RPGame.Progression;
using TMPro;
using UnityEngine;

namespace RPGame.UI.Jobs
{
    public sealed class JobPerksUI : MonoBehaviour
    {
        [SerializeField] private CharacterProgression progression;
        [SerializeField] private string jobId;
        [SerializeField] private TMP_Text jobNameText;
        [SerializeField] private TMP_Text availableJobPointsText;

        private JobInstance job;

        private void Awake()
        {
            ResolveProgression();
        }

        private void OnEnable()
        {
            ResolveProgression();
            ResolveJob();
            Refresh();
        }

        public void SetProgression(CharacterProgression progression)
        {
            if (this.progression == progression)
            {
                return;
            }

            this.progression = progression;
            ResolveJob();
            Refresh();
        }

        public void SetJob(JobInstance job)
        {
            this.job = job;
            jobId = job != null ? job.JobId : string.Empty;
            ResolveProgression();
            Refresh();
        }

        public void SetJobId(string jobId)
        {
            this.jobId = jobId;
            ResolveJob();
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
                availableJobPointsText.text = $"Job Points: {availableJobPoints}";
            }
        }

        private void ResolveProgression()
        {
            if (progression == null)
            {
                progression = FindAnyObjectByType<CharacterProgression>();
            }
        }

        private void ResolveJob()
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                return;
            }

            if (job != null && job.JobId == jobId)
            {
                return;
            }

            job = progression != null ? progression.Jobs.GetJob(jobId) : null;
        }
    }
}
