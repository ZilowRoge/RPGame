using TMPro;
using RPGame.Encounter;
using UnityEngine;

namespace RPGame.UI.Encounter
{
    public sealed class WaveTimerUI : MonoBehaviour
    {
        [SerializeField] private EncounterRuntime runtime;
        [SerializeField] private TMP_Text timerText;

        private void Update()
        {
            EncounterController encounterController = runtime != null ? runtime.EncounterController : null;
            if (timerText == null)
            {
                return;
            }

            if (encounterController == null || encounterController.State != EncounterState.Intermission)
            {
                timerText.text = string.Empty;
                return;
            }

            timerText.text = $"Next wave: {Mathf.CeilToInt(encounterController.IntermissionTimeRemaining)}";
        }
    }
}
