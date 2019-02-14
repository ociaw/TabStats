using System;

namespace TabStats
{
    internal sealed class Row<TInput>
    {
        private Row(TInput input, Object[] columnValues)
        {
            Input = input;
            ColumnValues = columnValues;
        }

        public TInput Input { get; }

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

        public static Row<TInput> Create(TInput input, Int32 columnCount)
        {
            if (columnCount < 0)
                throw new ArgumentOutOfRangeException(nameof(columnCount));

            return columnCount == 0 ? new Row<TInput>(input, null) : new Row<TInput>(input, new Object[columnCount]);
        }
    }
}
