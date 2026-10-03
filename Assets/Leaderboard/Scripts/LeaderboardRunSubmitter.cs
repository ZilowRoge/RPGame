using RPGame.Encounter;
using UnityEngine;

namespace RPGame.Leaderboard
{
    public sealed class LeaderboardRunSubmitter : MonoBehaviour
    {
        [SerializeField] private EncounterRuntime encounterRuntime;

        private void OnEnable()
        {
            if (encounterRuntime != null)
            {
                encounterRuntime.RunEnded += HandleRunEnded;
            }
        }

        private void OnDisable()
        {
            if (encounterRuntime != null)
            {
                encounterRuntime.RunEnded -= HandleRunEnded;
            }
        }

        private void HandleRunEnded(EncounterResult result)
        {
            _ = LeaderboardService.SubmitScoreAsync(result);
        }
    }
}
