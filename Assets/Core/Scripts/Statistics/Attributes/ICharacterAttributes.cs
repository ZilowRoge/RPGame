namespace RPGame.Core.Statistics.Attributes
{
    public interface ICharacterAttributes
    {
        int Strength { get; }
        int Dexterity { get; }
        int Endurance { get; }
        int Vitality { get; }
        int Intelligence { get; }
        int Power { get; }

        int GetValue(CharacterAttributeType attributeType);
    }
}
