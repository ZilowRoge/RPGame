using System;
using RPGame.Core.Statistics;

namespace RPGame.UI.Statistics
{
    [Serializable]
    public struct RecordDefinition
    {
        public RecordDefinition(
            RecordId id,
            string label,
            bool showWhenMissing = false)
        {
            Id = id;
            Label = label;
            ShowWhenMissing = showWhenMissing;
        }

        public RecordId Id;
        public string Label;
        public bool ShowWhenMissing;

        public static RecordDefinition Health()
        {
            return new RecordDefinition(RecordId.Health, "Health");
        }

        public static RecordDefinition Mana()
        {
            return new RecordDefinition(RecordId.Mana, "Mana");
        }

        public static RecordDefinition Stamina()
        {
            return new RecordDefinition(RecordId.Stamina, "Stamina");
        }

        public static RecordDefinition ManaRegeneration()
        {
            return new RecordDefinition(RecordId.ManaRegeneration, "Mana Regeneration");
        }

        public static RecordDefinition HealthRegeneration()
        {
            return new RecordDefinition(RecordId.HealthRegeneration, "Health Regeneration");
        }

        public static RecordDefinition StaminaRegeneration()
        {
            return new RecordDefinition(RecordId.StaminaRegeneration, "Stamina Regeneration");
        }

        public static RecordDefinition LastSpell()
        {
            return new RecordDefinition(RecordId.LastSpellDamage, "Last spell", showWhenMissing: true);
        }

        public static RecordDefinition WeaponDamage()
        {
            return new RecordDefinition(RecordId.WeaponDamage, "Weapon damage", showWhenMissing: true);
        }
    }
}
