using System;
using System.Collections.Generic;

namespace RPGame.Progression
{
    [Serializable]
    public sealed class JobProgression
    {
        private readonly JobContainer jobs;
        private readonly Func<int> getAvailableXP;
        private readonly Action<int> spendXP;

        public JobProgression(JobContainer jobs, Func<int> getAvailableXP, Action<int> spendXP)
        {
            this.jobs = jobs;
            this.getAvailableXP = getAvailableXP;
            this.spendXP = spendXP;
        }

        public IReadOnlyDictionary<string, JobInstance> JobsById => jobs.JobsById;

        public void UnlockJob(JobDefinition definition, Action<JobInstance> onAdvanced = null)
        {
            jobs.AddJob(definition, onAdvanced);
        }

        public bool HasJob(string jobId)
        {
            return jobs.HasJob(jobId);
        }

        public JobInstance GetJob(string jobId)
        {
            return jobs.GetJob(jobId);
        }

        public IEnumerable<JobInstance> GetAllJobs()
        {
            return jobs.GetAllJobs();
        }

        public bool AddXPToJob(string jobId, int amount)
        {
            if (!CanAddXPToJob(jobId, amount))
            {
                return false;
            }

            int unspentXP = jobs.AddExperience(jobId, amount);
            int spentXP = amount - unspentXP;

            if (spentXP <= 0)
            {
                return false;
            }

            spendXP(spentXP);
            return true;
        }

        public bool CanAddXPToJob(string jobId, int amount)
        {
            JobInstance job = jobs.GetJob(jobId);
            return amount > 0
                && amount <= getAvailableXP()
                && job != null
                && !job.IsMaxLevel
                && amount <= job.GetRemainingXPToMaxLevel();
        }

        public int GetJobLevel(string jobId)
        {
            JobInstance job = jobs.GetJob(jobId);
            return job != null ? job.CurrentLevel : 0;
        }

        public void Clear()
        {
            jobs.Clear();
        }
    }
}
