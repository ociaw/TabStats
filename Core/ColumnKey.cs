using System;

namespace TabStats
{
    public sealed class ColumnKey : IEquatable<ColumnKey>
    {
        public ColumnKey(String value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public String Value { get; }

        public override String ToString() => Value;

        public override Int32 GetHashCode() => Value.GetHashCode();

        public override Boolean Equals(object obj)
        {
            if (!(obj is ColumnKey key))
                return false;

            return Equals(key);
        }

        public Boolean Equals(ColumnKey other) => Value.Equals(other?.Value);

        public static Boolean operator ==(ColumnKey left, ColumnKey right) => left?.Value == right?.Value;

        public static Boolean operator !=(ColumnKey left, ColumnKey right) => left?.Value != right?.Value;
    }
}
