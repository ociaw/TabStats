using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OneOf;
using Dagger;

namespace TabStats
{
    using Key = OneOf<ColumnKey, AggregateKey>;

    public sealed class TableEngine<TInput>
    {
        private TableEngine(
            List<List<Key>> layers,
            Dictionary<ColumnKey, Int32> columnIndexes,
            Dictionary<AggregateKey, Int32> aggregateIndexes,
            Dictionary<ColumnKey, Column<TInput>> columns,
            Dictionary<AggregateKey, Aggregate<TInput>> aggregates,
            Dictionary<Key, ImmutableArray<Int32>> columnMappings,
            Dictionary<Key, ImmutableArray<Int32>> aggregateMappings,
            ImmutableArray<AggregateKey> requestedAggregateKeys,
            AggregateRow aggregateRow
        )
        {
            Layers = layers;
            ColumnIndexes = columnIndexes;
            AggregateIndexes = aggregateIndexes;
            Columns = columns;
            Aggregates = aggregates;
            ColumnDependencyMappings = columnMappings;
            AggregateDependencyMappings = aggregateMappings;
            RequestedAggregateKeys = requestedAggregateKeys;
            AggregateRow = aggregateRow;
        }

        private List<List<Key>> Layers { get; }

        private Dictionary<ColumnKey, Int32> ColumnIndexes { get; }

        private Dictionary<AggregateKey, Int32> AggregateIndexes { get; }

        private Dictionary<ColumnKey, Column<TInput>> Columns { get; }

        private Dictionary<AggregateKey, Aggregate<TInput>> Aggregates { get; }

        private Dictionary<Key, ImmutableArray<Int32>> ColumnDependencyMappings { get; }

        private Dictionary<Key, ImmutableArray<Int32>> AggregateDependencyMappings { get; }

        private ImmutableArray<AggregateKey> RequestedAggregateKeys { get; }

        private AggregateRow AggregateRow { get; }

        private List<Row<TInput>> Rows { get; } = new List<Row<TInput>>();

        public void AddRow(TInput input) => Rows.Add(Row<TInput>.Create(input, Columns.Count));

        public List<Object> Aggregate()
        {
            foreach (var layer in Layers)
            {
                foreach (var key in layer)
                {
                    key.Switch(CalculateColumn, CalculateAggregate);
                }
            }

            List<Object> values = new List<Object>(RequestedAggregateKeys.Length);
            foreach (AggregateKey key in RequestedAggregateKeys)
                values.Add(AggregateRow[AggregateIndexes[key]]);
            return values;
        }

        private void CalculateColumn(ColumnKey key)
        {
            ImmutableArray<Int32> columnMapping = ColumnDependencyMappings[key];
            ImmutableArray<Int32> aggregateMapping = AggregateDependencyMappings[key];
            AggregateValueBag aggregateBag = new AggregateValueBag(AggregateRow, aggregateMapping);
            Column<TInput> column = Columns[key];
            Int32 columnIndex = ColumnIndexes[key];
            foreach (var row in Rows)
            {
                var bag = new ColumnValueBag<TInput>(row, columnMapping);
                row[columnIndex] = column.Calculate(aggregateBag, new ColumnValueBag<TInput>(row, columnMapping));
            }
        }

        private void CalculateAggregate(AggregateKey key)
        {
            ImmutableArray<Int32> columnMapping = ColumnDependencyMappings[key];
            ImmutableArray<Int32> aggregateMapping = AggregateDependencyMappings[key];
            AggregateValueBag aggregateBag = new AggregateValueBag(AggregateRow, aggregateMapping);
            Aggregate<TInput> aggregate = Aggregates[key];
            Int32 aggregateIndex = AggregateIndexes[key];
            AggregateRow[aggregateIndex] = aggregate.Calculate(aggregateBag, Rows.Select(row => new ColumnValueBag<TInput>(row, columnMapping)));
        }

        internal static TableEngine<TInput> Create(Graph<Key, Int32> dependencyGraph, Dictionary<ColumnKey, Column<TInput>> columns, Dictionary<AggregateKey, Aggregate<TInput>> aggregates, ImmutableArray<AggregateKey> requestedAggregates)
        {
            if (dependencyGraph == null)
                throw new ArgumentNullException(nameof(dependencyGraph));

            var (layers, detached) = dependencyGraph.TopologicalSort();

            var columnIndexes = new Dictionary<ColumnKey, Int32>();
            var aggregateIndexes = new Dictionary<AggregateKey, Int32>();

            foreach (var layer in layers)
            {
                foreach (var key in layer)
                {
                    key.Switch
                    (
                        columnKey => columnIndexes.Add(columnKey, columnIndexes.Count),
                        aggregateKey => aggregateIndexes.Add(aggregateKey, aggregateIndexes.Count)
                    );
                }
            }

            Dictionary<Key, ImmutableArray<Int32>> columnMappings = new Dictionary<Key, ImmutableArray<Int32>>();
            Dictionary<Key, ImmutableArray<Int32>> aggregateMappings = new Dictionary<Key, ImmutableArray<Int32>>();

            foreach (ColumnKey columnKey in columns.Keys)
            {
                Column<TInput> column = columns[columnKey];

                columnMappings.Add(columnKey, column.ColumnDependencies.Select(ck => columnIndexes[ck]).ToImmutableArray());
                aggregateMappings.Add(columnKey, column.AggregateDependencies.Select(ak => aggregateIndexes[ak]).ToImmutableArray());
            }

            foreach (AggregateKey aggregateKey in aggregates.Keys)
            {
                Aggregate<TInput> aggregate = aggregates[aggregateKey];

                columnMappings.Add(aggregateKey, aggregate.ColumnDependencies.Select(ck => columnIndexes[ck]).ToImmutableArray());
                aggregateMappings.Add(aggregateKey, aggregate.AggregateDependencies.Select(ak => aggregateIndexes[ak]).ToImmutableArray());
            }

            return new TableEngine<TInput>(
                layers,
                columnIndexes,
                aggregateIndexes,
                columns,
                aggregates,
                columnMappings,
                aggregateMappings,
                requestedAggregates,
                AggregateRow.Create(aggregateIndexes.Count)
            );
        }
    }
}
