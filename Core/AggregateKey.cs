using System;

namespace TabStats
{
    public sealed class AggregateKey : IEquatable<AggregateKey>
    {
        public AggregateKey(String value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public String Value { get; }

        public override String ToString() => Value;

        public override Int32 GetHashCode() => Value.GetHashCode();

        public override Boolean Equals(object obj)
        {
            if (!(obj is AggregateKey key))
                return false;

            return Equals(key);
        }

        public Boolean Equals(AggregateKey other) => Value.Equals(other?.Value);

        public static Boolean operator ==(AggregateKey left, AggregateKey right) => left?.Value == right?.Value;

        public static Boolean operator !=(AggregateKey left, AggregateKey right) => left?.Value != right?.Value;
    }
}
