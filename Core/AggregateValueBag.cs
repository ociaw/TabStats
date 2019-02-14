using System;
using System.Collections.Immutable;

namespace TabStats
{
    public struct AggregateValueBag
    {
        internal AggregateValueBag(AggregateRow row, ImmutableArray<Int32> indexMappings)
        {
            Row = row;
            IndexMappings = indexMappings;
        }

        private AggregateRow Row { get; }

        private ImmutableArray<Int32> IndexMappings { get; }

        public Int32 ColumnCount => IndexMappings.Length;

        public T Get<T>(Int32 index)
        {
            if (index < 0 || index >= IndexMappings.Length)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Must be non-negative and less than {nameof(ColumnCount)}.");

            return Row.Get<T>(IndexMappings[index]);
        }
    }
}
