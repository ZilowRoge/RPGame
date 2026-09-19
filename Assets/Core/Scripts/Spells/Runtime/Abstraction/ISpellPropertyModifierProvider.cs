namespace RPGame.Core.Spells
{
    public interface ISpellPropertyModifierProvider
    {
        SpellPropertyModifiers CreateSpellPropertyModifiers(Spell spell);
    }
}
