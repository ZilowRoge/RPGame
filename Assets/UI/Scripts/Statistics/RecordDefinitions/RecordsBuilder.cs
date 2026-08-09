using System;
using System.Collections.Generic;
using RPGame.Core.Statistics;

namespace RPGame.UI.Statistics
{
    [Serializable]
    public sealed class RecordsBuilder
    {
        public List<RecordDefinition> Definitions = CreateDefaultDefinitions();

        private readonly Dictionary<RecordId, string> valuesById = new();
        private readonly List<StatisticRecordData> records = new();

        public IReadOnlyList<StatisticRecordData> Build(IReadOnlyList<DataEntry> entries)
        {
            EnsureDefinitions();
            valuesById.Clear();
            records.Clear();

            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    DataEntry entry = entries[i];
                    if (entry.Id != RecordId.None)
                    {
                        valuesById[entry.Id] = entry.ValueText;
                    }
                }
            }

            for (int i = 0; i < Definitions.Count; i++)
            {
                RecordDefinition definition = Definitions[i];
                if (definition.Id == RecordId.None || string.IsNullOrWhiteSpace(definition.Label))
                {
                    continue;
                }

                if (valuesById.TryGetValue(definition.Id, out string valueText))
                {
                    records.Add(new StatisticRecordData(definition.Label, valueText));
                }
                else if (definition.ShowWhenMissing)
                {
                    records.Add(new StatisticRecordData(definition.Label, ValueFactory.EmptyValueText));
                }
            }

            return records;
        }

        private void EnsureDefinitions()
        {
            if (Definitions == null || Definitions.Count == 0)
            {
                Definitions = CreateDefaultDefinitions();
            }
        }

        private static List<RecordDefinition> CreateDefaultDefinitions()
        {
            return new List<RecordDefinition>
            {
                RecordDefinition.Health(),
                RecordDefinition.Mana(),
                RecordDefinition.Stamina(),
                RecordDefinition.HealthRegeneration(),
                RecordDefinition.ManaRegeneration(),
                RecordDefinition.StaminaRegeneration(),
                RecordDefinition.WeaponDamage(),
                RecordDefinition.LastSpell()
            };
        }
    }
}
