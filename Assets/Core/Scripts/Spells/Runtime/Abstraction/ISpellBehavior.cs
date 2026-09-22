namespace RPGame.Core.Spells
{
    public interface ISpellBehavior : IPhasedSpellBehavior
    {
        int Priority { get; }

        void Execute(SpellBehaviorContext context);
    }
}
