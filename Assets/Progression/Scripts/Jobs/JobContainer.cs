using System;
using System.Collections.Generic;

namespace RPGame.Progression
{
    [Serializable]
    public sealed class JobContainer
    {
        private readonly Dictionary<string, JobInstance> jobs = new();

        public IReadOnlyDictionary<string, JobInstance> JobsById => jobs;

        public void AddJob(JobDefinition definition, Action<JobInstance> onAdvanced = null)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.JobId) || jobs.ContainsKey(definition.JobId))
            {
                return;
            }

            jobs.Add(definition.JobId, new JobInstance(definition, onAdvanced));
        }

        public bool HasJob(string jobId)
        {
            return jobs.ContainsKey(jobId);
        }

        public JobInstance GetJob(string jobId)
        {
            return jobs.TryGetValue(jobId, out JobInstance job) ? job : null;
        }

        public IEnumerable<JobInstance> GetAllJobs()
        {
            return jobs.Values;
        }

        public int AddExperience(string jobId, int amount)
        {
            if (jobs.TryGetValue(jobId, out JobInstance job))
            {
                return job.AddExperience(amount);
            }

            return amount > 0 ? amount : 0;
        }

        public void Clear()
        {
            jobs.Clear();
        }
    }
}
