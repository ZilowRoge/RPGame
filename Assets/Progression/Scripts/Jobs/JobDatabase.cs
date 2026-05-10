using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Progression
{
    [CreateAssetMenu(fileName = "JobDatabase", menuName = "RPGame/Progression/Job Database")]
    public sealed class JobDatabase : ScriptableObject
    {
        [SerializeField] private List<JobDefinition> jobs = new();

        private Dictionary<string, JobDefinition> jobsById;

        public IReadOnlyList<JobDefinition> Jobs => jobs;

        public bool TryGetJob(string jobId, out JobDefinition job)
        {
            EnsureLookup();
            if (string.IsNullOrWhiteSpace(jobId))
            {
                job = null;
                return false;
            }

            return jobsById.TryGetValue(jobId, out job);
        }

        public JobDefinition GetJob(string jobId)
        {
            return TryGetJob(jobId, out JobDefinition job) ? job : null;
        }

        public bool Contains(string jobId)
        {
            return TryGetJob(jobId, out _);
        }

        public void Rebuild()
        {
            jobsById = new Dictionary<string, JobDefinition>();

            for (int i = 0; i < jobs.Count; i++)
            {
                JobDefinition job = jobs[i];
                if (job == null || string.IsNullOrWhiteSpace(job.JobId))
                {
                    continue;
                }

                if (!jobsById.ContainsKey(job.JobId))
                {
                    jobsById.Add(job.JobId, job);
                }
            }
        }

        private void EnsureLookup()
        {
            if (jobsById == null)
            {
                Rebuild();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            jobsById = null;
            ValidateJobs();
        }

        private void ValidateJobs()
        {
            HashSet<string> usedIds = new();

            for (int i = 0; i < jobs.Count; i++)
            {
                JobDefinition job = jobs[i];
                if (job == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(job.JobId))
                {
                    Debug.LogWarning($"Job database contains job without id: {job.name}.", this);
                    continue;
                }

                if (!usedIds.Add(job.JobId))
                {
                    Debug.LogWarning($"Job database contains duplicate job id: {job.JobId}.", this);
                }
            }
        }
#endif
    }
}
