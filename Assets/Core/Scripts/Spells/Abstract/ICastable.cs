namespace RPGame.Core.Spells
{
    public interface ICastable
    {
        void OnDeactivation(CasterData casterData);
        ISpellActivationHandle OnActivation(CasterData casterData);
        void OnCast(CasterData casterData);
    }
}
