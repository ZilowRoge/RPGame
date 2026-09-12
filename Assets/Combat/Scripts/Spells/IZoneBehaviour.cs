using RPGame.Core.Spells;

namespace RPGame.Combat.Spells
{
    public interface IZoneBehaviour
    {
        void Initialize(CasterData casterData, float radius);
        void Activate();
        void Deactivate();
    }
}
