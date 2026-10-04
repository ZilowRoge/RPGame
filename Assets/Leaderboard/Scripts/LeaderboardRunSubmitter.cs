using System.Threading.Tasks;
using RPGame.Encounter;
using UnityEngine;

namespace RPGame.Leaderboard
{
    public sealed class LeaderboardRunSubmitter : MonoBehaviour
    {
        [SerializeField] private EncounterRuntime encounterRuntime;

        private Task submissionTask = Task.CompletedTask;

        public Task SubmissionTask => submissionTask;

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
            Debug.Log(
                $"Leaderboard score submission requested: score={result.Score}, waves={result.WavesCompleted}, seed={result.EncounterSeed}.",
                this);
            submissionTask = LeaderboardService.SubmitScoreAsync(result);
        }
    }
}
