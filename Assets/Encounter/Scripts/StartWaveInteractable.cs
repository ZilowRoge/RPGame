using RPGame.Core.Interaction;

namespace RPGame.Encounter
{
    public sealed class StartWaveInteractable : InteractionBase
    {
        private EncounterController encounterController;

        public void Initialize(EncounterController encounterController)
        {
            this.encounterController = encounterController;
        }

        public override bool CanInteract(InteractionContext context)
        {
            return encounterController != null && encounterController.State == EncounterState.Intermission;
        }

        public override void Interact(InteractionContext context)
        {
            if (encounterController == null || encounterController.State != EncounterState.Intermission)
            {
                return;
            }

            encounterController.StartPendingWaveNow();
        }

        public override string GetInteractionText()
        {
            return "Start wave";
        }
    }
}
