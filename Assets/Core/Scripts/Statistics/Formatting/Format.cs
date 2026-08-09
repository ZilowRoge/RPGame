using System;

namespace RPGame.Core.Statistics
{
    [Serializable]
    public struct Format
    {
        public int DecimalPlaces;
        public bool ShowAsPercentage;
        public bool ShowPlusSign;
        public string Suffix;
        public string PairSeparator;

        public static Format Integer => new()
        {
            DecimalPlaces = 0
        };

        public static Format Decimal(int decimalPlaces) => new()
        {
            DecimalPlaces = decimalPlaces
        };

        public static Format CurrentAndMax => new()
        {
            DecimalPlaces = 0,
            PairSeparator = " / "
        };

        public static Format Percentage(int decimalPlaces = 0, bool showPlusSign = false) => new()
        {
            DecimalPlaces = decimalPlaces,
            ShowAsPercentage = true,
            ShowPlusSign = showPlusSign,
            Suffix = "%"
        };

        public static Format Range(int decimalPlaces = 0) => new()
        {
            DecimalPlaces = decimalPlaces,
            PairSeparator = " - "
        };

        public static Format PerSecond(int decimalPlaces = 1) => new()
        {
            DecimalPlaces = decimalPlaces,
            Suffix = "/s"
        };
    }
}
