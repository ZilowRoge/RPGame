namespace RPGame.UI.Statistics
{
    public readonly struct StatisticRecordData
    {
        public StatisticRecordData(string label, string valueText)
        {
            Label = label;
            ValueText = valueText;
        }

        public string Label { get; }
        public string ValueText { get; }
    }
}
