namespace RPGame.Core.Statistics
{
    public readonly struct DataEntry
    {
        public DataEntry(RecordId id, string valueText)
        {
            Id = id;
            ValueText = valueText ?? string.Empty;
        }

        public RecordId Id { get; }
        public string ValueText { get; }
    }
}
