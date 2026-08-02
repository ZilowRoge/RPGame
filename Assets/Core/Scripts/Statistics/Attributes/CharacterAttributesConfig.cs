using UnityEngine;
using UnityEngine.Serialization;

namespace RPGame.Core.Statistics.Attributes
{
    [CreateAssetMenu(fileName = "CharacterAttributesConfig", menuName = "RPGame/Statistics/Character Attributes Config")]
    public sealed class CharacterAttributesConfig : ScriptableObject
    {
        [SerializeField] private int strength = 10;
        [SerializeField] private int dexterity = 10;
        [FormerlySerializedAs("condition")]
        [SerializeField] private int endurance = 10;
        [SerializeField] private int vitality = 10;
        [SerializeField] private int intelligence = 10;
        [SerializeField] private int power = 10;

        public int Strength => strength;
        public int Dexterity => dexterity;
        public int Endurance => endurance;
        public int Vitality => vitality;
        public int Intelligence => intelligence;
        public int Power => power;

        public int GetValue(CharacterAttributeType attributeType)
        {
            return attributeType switch
            {
                CharacterAttributeType.Strength => strength,
                CharacterAttributeType.Dexterity => dexterity,
                CharacterAttributeType.Endurance => endurance,
                CharacterAttributeType.Vitality => vitality,
                CharacterAttributeType.Intelligence => intelligence,
                CharacterAttributeType.Power => power,
                _ => throw new System.ArgumentOutOfRangeException(nameof(attributeType), attributeType, null)
            };
        }

        private void OnValidate()
        {
            strength = Mathf.Max(0, strength);
            dexterity = Mathf.Max(0, dexterity);
            endurance = Mathf.Max(0, endurance);
            vitality = Mathf.Max(0, vitality);
            intelligence = Mathf.Max(0, intelligence);
            power = Mathf.Max(0, power);
        }
    }
}
