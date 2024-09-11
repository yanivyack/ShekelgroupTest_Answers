using System.Windows.Forms;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using System;

namespace ENV
{
    public class ArrayParameter<dataType> : ParameterBase<dataType[]>
    {
        public ArrayParameter(dataType[] value)
            : base(value)
        {
        }

        public ArrayParameter(TypedColumnBase<dataType[]> column)
            : base(column)
        {
        }
        internal ArrayParameter(Func<ColumnBase, dataType[]> getValue, Action<ColumnBase, dataType[]> setReturnValue, ColumnBase column) : base(getValue, setReturnValue, column)
        { }
        public static implicit operator ArrayParameter<dataType>(dataType[] value)
        {
            return new ArrayParameter<dataType>(value);
        }

        public static implicit operator ArrayParameter<dataType>(TypedColumnBase<dataType[]> column)
        {
            if (column == null)
                return null;
            return new ArrayParameter<dataType>(column);
        }
        public static implicit operator ArrayParameter<dataType>(ENV.Data.ByteArrayColumn column)
        {
            if (column == null)
                return null;
            return new ArrayParameter<dataType>(x =>
            {
                var ar = x as ENV.Data.ArrayColumn<dataType>;
                return ar.FromByteArray(column.Value);
            }, (col, b) =>
            {
                var ar = col as ENV.Data.ArrayColumn<dataType>;
                column.Value = ar.ToByteArray();
            }, column);
        }
    }
}