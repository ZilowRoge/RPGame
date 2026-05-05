using System;
using UnityEngine;

namespace RPGame.Progression
{
    [Serializable]
    public sealed class JobInstance
    {
        private const int StartingLevel = 1;

        [SerializeField] private JobDefinition definition;
        [SerializeField] private string jobId;
        [SerializeField] private int currentLevel;
        [SerializeField] private int currentXP;
        [SerializeField] private int totalInvestedXP;

        private readonly Action<JobInstance> onAdvanced;

        public JobInstance(JobDefinition definition, Action<JobInstance> onAdvanced = null)
            : this(definition, StartingLevel, 0, 0, onAdvanced)
        {
        }

        public JobInstance(
            JobDefinition definition,
            int currentLevel,
            int currentXP,
            int totalInvestedXP,
            Action<JobInstance> onAdvanced = null)
        {
            this.definition = definition;
            jobId = definition.JobId;
            this.currentLevel = Mathf.Max(0, currentLevel);
            this.currentXP = Mathf.Max(0, currentXP);
            this.totalInvestedXP = Mathf.Max(0, totalInvestedXP);
            this.onAdvanced = onAdvanced;
        }

        public JobDefinition Definition => definition;
        public string JobId => jobId;
        public int CurrentLevel => currentLevel;
        public int CurrentXP => currentXP;
        public int TotalInvestedXP => totalInvestedXP;
        public bool IsMaxLevel => currentLevel >= definition.MaxLevel;

        public int AddExperience(int amount)
        {
            if (amount <= 0 || IsMaxLevel)
            {
                return amount > 0 ? amount : 0;
            }

            int acceptedXP = Mathf.Min(amount, GetRemainingXPToMaxLevel());
            if (acceptedXP <= 0)
            {
                return amount;
            }

            currentXP = AddClamped(currentXP, acceptedXP);
            totalInvestedXP = AddClamped(totalInvestedXP, acceptedXP);
            ApplyLevelUps();

            return amount - acceptedXP;
        }

        public int GetXPToNextLevel()
        {
            return IsMaxLevel ? 0 : definition.GetRequiredExperience(currentLevel);
        }

        private void ApplyLevelUps()
        {
            while (!IsMaxLevel)
            {
                int requiredXP = GetXPToNextLevel();
                if (currentXP < requiredXP)
                {
                    return;
                }

                currentXP = Mathf.Max(0, currentXP - requiredXP);
                currentLevel++;
                onAdvanced?.Invoke(this);
            }
        }

        private int GetRemainingXPToMaxLevel()
        {
            long remainingXP = 0;
            int simulatedLevel = currentLevel;
            int simulatedCurrentXP = currentXP;

            while (simulatedLevel < definition.MaxLevel)
            {
                int requiredXP = definition.GetRequiredExperience(simulatedLevel);
                remainingXP += Math.Max(0, requiredXP - simulatedCurrentXP);
                simulatedCurrentXP = 0;
                simulatedLevel++;
            }

            return remainingXP >= int.MaxValue ? int.MaxValue : (int)remainingXP;
        }

        private static int AddClamped(int currentValue, int amount)
        {
            return amount > int.MaxValue - currentValue ? int.MaxValue : currentValue + amount;
        }
    }
}
