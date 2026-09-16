namespace RPGame.Core.Spells
{
    public interface ISpellActivationHandle
    {
        void Activate(ISpellActivationService service, CasterData casterData);
        void Deactivate(ISpellActivationService service);
        bool TryCreateCasterData(CasterData casterData, out CasterData activatedCasterData);
    }
}
