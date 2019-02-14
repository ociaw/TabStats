using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace TabStats.Tests
{
    public class TableEngineTests
    {
        [Fact]
        public void Test1()
        {
            TableEngineBuilder<Row> builder = new TableEngineBuilder<Row>();
            var sumKey = new AggregateKey("Sum");
            var squareSumKey = new AggregateKey("SquaredSum");
            var subtractorKey = new AggregateKey("Difference");
            var squareKey = new ColumnKey("Squared");

            builder.AddAggregate(new InputSummer(sumKey));
            builder.AddAggregate(new ColumnSummer(squareSumKey, squareKey));
            builder.AddAggregate(new AggregateSubtractor(subtractorKey, squareSumKey, sumKey));
            builder.AddColumn(new InputSquarer(squareKey));

            var table = builder.Build(sumKey, squareSumKey, subtractorKey);

            table.AddRow(new Row(1));
            table.AddRow(new Row(2));
            table.AddRow(new Row(3));

            var results = table.Aggregate();

            Assert.Equal(3, results.Count);
            Assert.IsType<Int32>(results[0]);
            Assert.Equal(6, (Int32)results[0]);
            Assert.IsType<Int32>(results[1]);
            Assert.Equal(14, (Int32)results[1]);
            Assert.IsType<Int32>(results[2]);
            Assert.Equal(8, (Int32)results[2]);
        }

        private sealed class Row
        {
            public Row(Int32 number)
            {
                Number = number;
            }

            public Int32 Number { get; }
        }

        private sealed class InputSquarer : Column<Row>
        {
            public InputSquarer(ColumnKey key)
                : base(key, ImmutableArray<ColumnKey>.Empty, ImmutableArray<AggregateKey>.Empty)
            { }

            public override Object Calculate(AggregateValueBag aggregateValues, ColumnValueBag<Row> columnValues) => columnValues.Input.Number * columnValues.Input.Number;
        }

        private sealed class InputSummer : Aggregate<Row>
        {
            public InputSummer(AggregateKey key)
                : base(key, ImmutableArray<ColumnKey>.Empty, ImmutableArray<AggregateKey>.Empty)
            { }

            public override Object Calculate(AggregateValueBag aggregateValues, IEnumerable<ColumnValueBag<Row>> rows) => rows.Sum(r => r.Input.Number);
        }

        private sealed class ColumnSummer : Aggregate<Row>
        {
            public ColumnSummer(AggregateKey key, ColumnKey sumColumn)
                : base(key, ImmutableArray.Create(sumColumn), ImmutableArray<AggregateKey>.Empty)
            { }

            public override Object Calculate(AggregateValueBag aggregateValues, IEnumerable<ColumnValueBag<Row>> rows) => rows.Sum(r => r.Get<Int32>(0));
        }

        private sealed class AggregateSubtractor : Aggregate<Row>
        {
            public AggregateSubtractor(AggregateKey key, AggregateKey minuend, AggregateKey subtrahend)
                : base(key, ImmutableArray<ColumnKey>.Empty, ImmutableArray.Create(minuend, subtrahend))
            { }

            public override Object Calculate(AggregateValueBag aggregateValues, IEnumerable<ColumnValueBag<Row>> rows) => aggregateValues.Get<Int32>(0) - aggregateValues.Get<Int32>(1);
        }
    }
}
