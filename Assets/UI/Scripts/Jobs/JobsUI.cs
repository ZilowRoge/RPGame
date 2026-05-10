using System.Collections.Generic;
using RPGame.Progression;
using UnityEngine;

namespace RPGame.UI.Jobs
{
    public sealed class JobsUI : MonoBehaviour
    {
        [SerializeField] private CharacterProgression progression;
        [SerializeField] private JobRecordUI recordPrefab;
        [SerializeField] private Transform recordsRoot;
        [SerializeField] private JobExperienceAssignmentUI assignmentWindow;
        [SerializeField] private JobPerksUI jobPerksUI;

        private readonly Dictionary<string, JobRecordUI> recordsByJobId = new();

        private void Awake()
        {
            ResolveProgression();
        }

        private void OnEnable()
        {
            ResolveProgression();
            Rebuild();
        }

        public void SetProgression(CharacterProgression progression)
        {
            if (this.progression == progression)
            {
                return;
            }

            this.progression = progression;
            Rebuild();
        }

        private void ResolveProgression()
        {
            if (progression == null)
            {
                progression = FindAnyObjectByType<CharacterProgression>();
            }
        }

        private void Rebuild()
        {
            ClearRecords();

            if (progression == null || recordPrefab == null || recordsRoot == null)
            {
                return;
            }

            foreach (JobInstance job in progression.Jobs.GetAllJobs())
            {
                AddRecord(job);
            }

            RefreshAll();
        }

        private void AddRecord(JobInstance job)
        {
            if (job == null || string.IsNullOrWhiteSpace(job.JobId) || recordsByJobId.ContainsKey(job.JobId))
            {
                return;
            }

            JobRecordUI record = Instantiate(recordPrefab, recordsRoot);
            record.Initialize(job, HandleExperienceAssignmentRequested);
            record.Selected += HandleJobSelected;
            recordsByJobId.Add(job.JobId, record);
        }

        private void RefreshAll()
        {
            RefreshAll(progression != null ? progression.GetAvailableXP() : 0);
        }

        private void RefreshAll(int availableXP)
        {
            foreach (JobRecordUI record in recordsByJobId.Values)
            {
                if (record != null)
                {
                    record.Refresh(availableXP);
                }
            }
        }

        private void HandleExperienceAssignmentRequested(JobInstance job)
        {
            if (progression == null || job == null || job.IsMaxLevel)
            {
                return;
            }

            if (assignmentWindow != null)
            {
                assignmentWindow.Open(progression, job, () => RefreshAfterJobChanged(job));
            }
        }

        private void HandleJobSelected(JobInstance job)
        {
            if (jobPerksUI != null)
            {
                jobPerksUI.SetJob(job);
            }
        }

        private void RefreshAfterJobChanged(JobInstance job)
        {
            RefreshAll();

            if (jobPerksUI != null)
            {
                jobPerksUI.SetJob(job);
            }
        }

        private void ClearRecords()
        {
            foreach (JobRecordUI record in recordsByJobId.Values)
            {
                if (record != null)
                {
                    record.Selected -= HandleJobSelected;
                    Destroy(record.gameObject);
                }
            }

            recordsByJobId.Clear();
        }
    }
}
