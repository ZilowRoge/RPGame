using System;

namespace RPGame.Core.Statuses
{
    public readonly struct StatusSourceId : IEquatable<StatusSourceId>
    {
        private const string NoneValue = "None";

        public StatusSourceId(string value)
        {
            Value = string.IsNullOrWhiteSpace(value) ? NoneValue : value;
        }

        public string Value { get; }

        public bool Equals(StatusSourceId other)
        {
            return string.Equals(ToString(), other.ToString(), StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is StatusSourceId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(ToString());
        }

        public override string ToString()
        {
            return Value ?? NoneValue;
        }

        public static bool operator ==(StatusSourceId left, StatusSourceId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StatusSourceId left, StatusSourceId right)
        {
            return !left.Equals(right);
        }
    }
}
