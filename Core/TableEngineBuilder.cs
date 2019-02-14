using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OneOf;
using Dagger;

namespace TabStats
{
    using Key = OneOf<ColumnKey, AggregateKey>;

    public sealed class TableEngineBuilder<TInput>
    {
        private Graph<Key, Int32> Graph { get; } = new Graph<Key, Int32>();

        private Dictionary<ColumnKey, Column<TInput>> Columns { get; } = new Dictionary<ColumnKey, Column<TInput>>();

        private Dictionary<AggregateKey, Aggregate<TInput>> Aggregates { get; } = new Dictionary<AggregateKey, Aggregate<TInput>>();

        public void AddColumn(Column<TInput> column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            Graph.AddNode(column.Key, 0, column.AllDependencies());
            Columns.Add(column.Key, column);
        }

        public void AddAggregate(Aggregate<TInput> aggregate)
        {
            if (aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));

            Graph.AddNode(aggregate.Key, 0, aggregate.AllDependencies());
            Aggregates.Add(aggregate.Key, aggregate);
        }

        public TableEngine<TInput> Build(params AggregateKey[] requestedAggregates) => Build(requestedAggregates.ToImmutableArray());

        public TableEngine<TInput> Build(ImmutableArray<AggregateKey> requestedAggregates)
        {
            if (requestedAggregates == null)
                throw new ArgumentNullException(nameof(requestedAggregates));

            var keys = requestedAggregates.Where(ak => Aggregates.ContainsKey(ak)).Select(ak => (Key)ak);
            var clone = Graph.Clone();
            clone.Trim(keys);

            var trimmedColumns = Columns.Where(columns => clone.Any(node => node.Key.IsT0 && columns.Key == node.Key.AsT0)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var trimmedAggregates = Aggregates.Where(aggregates => clone.Any(node => node.Key.IsT1 && aggregates.Key == node.Key.AsT1)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return TableEngine<TInput>.Create(clone, trimmedColumns, trimmedAggregates, requestedAggregates);
        }
    }
}
