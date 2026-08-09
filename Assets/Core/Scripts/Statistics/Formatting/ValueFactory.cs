using System;
using System.Globalization;

namespace RPGame.Core.Statistics
{
    public static class ValueFactory
    {
        public const string EmptyValueText = "\u2014";

        public static Value None()
        {
            return new EmptyValue();
        }

        public static Value Single(float value)
        {
            return new SingleValue(value);
        }

        public static Value CurrentAndMax(float currentValue, float maxValue)
        {
            return new PairValue(currentValue, maxValue, " / ");
        }

        public static Value Range(float minValue, float maxValue)
        {
            return new PairValue(minValue, maxValue, " - ");
        }

        private static string FormatNumber(float rawValue, Format format)
        {
            float value = format.ShowAsPercentage ? rawValue * 100f : rawValue;
            value = ApplyPrecision(value, format.DecimalPlaces);

            string number = value.ToString(GetNumberFormat(format.DecimalPlaces), CultureInfo.InvariantCulture);
            if (format.ShowPlusSign && value > 0f)
            {
                number = $"+{number}";
            }

            return $"{number}{format.Suffix}";
        }

        private static float ApplyPrecision(float value, int decimalPlaces)
        {
            int places = Math.Max(0, decimalPlaces);
            return (float)Math.Round(value, places, MidpointRounding.AwayFromZero);
        }

        private static string GetNumberFormat(int decimalPlaces)
        {
            int places = Math.Max(0, decimalPlaces);
            return places == 0 ? "0" : $"0.{new string('0', places)}";
        }

        private sealed class EmptyValue : Value
        {
            public override string Format(Format format)
            {
                return EmptyValueText;
            }
        }

        private sealed class SingleValue : Value
        {
            private readonly float value;

            public SingleValue(float value)
            {
                this.value = value;
            }

            public override string Format(Format format)
            {
                return FormatNumber(value, format);
            }
        }

        private sealed class PairValue : Value
        {
            private readonly float firstValue;
            private readonly float secondValue;
            private readonly string defaultSeparator;

            public PairValue(float firstValue, float secondValue, string defaultSeparator)
            {
                this.firstValue = firstValue;
                this.secondValue = secondValue;
                this.defaultSeparator = defaultSeparator;
            }

            public override string Format(Format format)
            {
                string separator = string.IsNullOrEmpty(format.PairSeparator)
                    ? defaultSeparator
                    : format.PairSeparator;

                return $"{FormatNumber(firstValue, format)}{separator}{FormatNumber(secondValue, format)}";
            }
        }
    }
}
