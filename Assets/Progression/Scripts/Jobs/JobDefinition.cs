using UnityEngine;

namespace RPGame.Progression
{
    [CreateAssetMenu(fileName = "JobDefinition", menuName = "RPGame/Progression/Job Definition")]
    public sealed class JobDefinition : ScriptableObject
    {
        [SerializeField] private string jobId = "Wizard";
        [SerializeField] private string displayName = "Wizard";
        [TextArea]
        [SerializeField] private string description;
        [SerializeField] private JobTier tier = JobTier.Basic;
        [SerializeField] private int maxLevel = 10;
        [SerializeField] private int baseXP = 100;
        [SerializeField] private float xpGrowthRate = 1.5f;

        public string JobId => jobId;
        public string DisplayName => displayName;
        public string Description => description;
        public JobTier Tier => tier;
        public int MaxLevel => maxLevel;
        public int BaseXP => baseXP;
        public float XPGrowthRate => xpGrowthRate;

        public int GetRequiredExperience(int level)
        {
            float requiredExperience = baseXP * Mathf.Pow(xpGrowthRate, Mathf.Max(1, level));
            if (float.IsInfinity(requiredExperience) || requiredExperience >= int.MaxValue)
            {
                return int.MaxValue;
            }

            return Mathf.Max(1, Mathf.RoundToInt(requiredExperience));
        }

        private void OnValidate()
        {
            jobId = string.IsNullOrWhiteSpace(jobId) ? name : jobId.Trim();
            displayName = string.IsNullOrWhiteSpace(displayName) ? jobId : displayName.Trim();
            maxLevel = Mathf.Max(1, maxLevel);
            baseXP = Mathf.Max(1, baseXP);
            xpGrowthRate = Mathf.Max(0.01f, xpGrowthRate);
        }
    }
}
