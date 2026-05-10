using RPGame.Core.Progression;
using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Progression
{
    public sealed class CharacterProgression : MonoBehaviour, IExperienceReceiver
    {
        [SerializeField] private List<JobDefinition> startingJobs = new();
        [SerializeField] private int availableXP;

        private readonly JobContainer jobContainer = new();
        private readonly PerkProgression perks = new();
        private JobProgression jobs;

        public JobContainer JobContainer => jobContainer;
        public JobProgression Jobs => GetJobs();

        private void Awake()
        {
            for (int i = 0; i < startingJobs.Count; i++)
            {
                if (startingJobs[i] != null)
                {
                    Jobs.UnlockJob(startingJobs[i]);
                }
            }
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            availableXP = amount > int.MaxValue - availableXP ? int.MaxValue : availableXP + amount;
        }

        public int GetAvailableXP()
        {
            return availableXP;
        }

        public PerkUnlockState GetPerkUnlockState(JobInstance job, PerkDefinition perk)
        {
            return perks.GetUnlockState(job, perk);
        }

        public bool TryUnlockPerk(JobInstance job, PerkDefinition perk)
        {
            return perks.TryUnlockPerk(job, perk);
        }

        private void OnValidate()
        {
            availableXP = Mathf.Max(0, availableXP);
        }

        private JobProgression GetJobs()
        {
            jobs ??= new JobProgression(jobContainer, GetAvailableXP, SpendExperience);
            return jobs;
        }

        private void SpendExperience(int amount)
        {
            availableXP = Mathf.Max(0, availableXP - amount);
        }
    }
}
