using System;

namespace RPGame.Core.Progression
{
    public interface IExperienceProvider
    {
        event Action AvailableExperienceChanged;

        int AvailableExperience { get; }
    }
}
