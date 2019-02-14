using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OneOf;

namespace TabStats
{
    using Key = OneOf<ColumnKey, AggregateKey>;

    public abstract class Column<TInput>
    {
        protected Column(ColumnKey key, ImmutableArray<ColumnKey> columnDependencies, ImmutableArray<AggregateKey> aggregateDependencies)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            if (columnDependencies.Contains(null))
                throw new ArgumentException("Must not contain a null value.", nameof(columnDependencies));
            if (aggregateDependencies.Contains(null))
                throw new ArgumentException("Must not contain a null value.", nameof(aggregateDependencies));

            ColumnDependencies = columnDependencies;
            AggregateDependencies = aggregateDependencies;
        }

        public ColumnKey Key { get; }

        public ImmutableArray<ColumnKey> ColumnDependencies { get; }

        public ImmutableArray<AggregateKey> AggregateDependencies { get; }

        internal IList<Key> AllDependencies() => ColumnDependencies.Select(ak => (Key)ak).Concat(AggregateDependencies.Select(ak => (Key)ak)).ToList();

        public abstract Object Calculate(AggregateValueBag aggregateValues, ColumnValueBag<TInput> columnValues);
    }
}
