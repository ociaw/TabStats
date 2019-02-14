using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OneOf;

namespace TabStats
{
    using Key = OneOf<ColumnKey, AggregateKey>;

    public abstract class Aggregate<TInput>
    {
        protected Aggregate(AggregateKey key, ImmutableArray<ColumnKey> columnDependencies, ImmutableArray<AggregateKey> aggregateDependencies)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            ColumnDependencies = columnDependencies;
            AggregateDependencies = aggregateDependencies;
        }

        public AggregateKey Key { get; }

        public ImmutableArray<ColumnKey> ColumnDependencies { get; }

        public ImmutableArray<AggregateKey> AggregateDependencies { get; }

        internal IList<Key> AllDependencies() => ColumnDependencies.Select(ak => (Key)ak).Concat(AggregateDependencies.Select(ak => (Key)ak)).ToList();

        public abstract Object Calculate(AggregateValueBag aggregateValues, IEnumerable<ColumnValueBag<TInput>> rows);

        public virtual String FormatResult(Object result) => result?.ToString() ?? "";
    }
}
