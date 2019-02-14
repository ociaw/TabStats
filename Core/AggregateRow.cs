using System;

namespace TabStats
{
    internal sealed class AggregateRow
    {
        private AggregateRow(Object[] columnValues)
        {
            ColumnValues = columnValues;
        }

        private Object[] ColumnValues { get; }

        public Object this[Int32 index]
        {
            get
            {
                if (index < 0 || ColumnValues == null || index >= ColumnValues.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return ColumnValues[index];
            }
            set
            {
                if (index < 0 || ColumnValues == null || index >= ColumnValues.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                ColumnValues[index] = value;
            }
        }

        public T Get<T>(Int32 index)
        {
            Object value = this[index];

            if (value == null)
                return default;

            if (!(value is T t))
                throw new ArgumentException($"Value at {index} is not of type T.");

            return t;
        }

        public static AggregateRow Create(Int32 columnCount)
        {
            if (columnCount < 0)
                throw new ArgumentOutOfRangeException(nameof(columnCount));

            return columnCount == 0 ? new AggregateRow(null) : new AggregateRow(new Object[columnCount]);
        }
    }
}
