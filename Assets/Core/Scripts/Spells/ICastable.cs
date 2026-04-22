namespace RPGame.Core.Spells
{
    public interface ICastable
    {
        void OnDeactivation(CasterData casterData);
        void OnActivation(CasterData casterData);
        void OnCast(CasterData casterData);
    }
}
