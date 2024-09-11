using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firefly.Box.Data.Advanced;

namespace ENV
{
    public class ActiveXParameter<T> : ParameterBase<T>
        where T : System.Windows.Forms.AxHost, new()
    {
        public ActiveXParameter(T value)
           : base(value)
        {
        }

        ActiveXParameter(Func<ColumnBase, T> getValue, Action<T> setReturnValue,
   ColumnBase column)
  : base(getValue, setReturnValue, column)
        {
        }

        public ActiveXParameter(TypedColumnBase<T> column) : base(column)
        {
        }
        public static implicit operator ActiveXParameter<T>(ActiveXColumn<T> column)
        {
            if (column == null)
                return null;
            return new ActiveXParameter<T>(column);
        }
        public static ActiveXParameter<T> Cast<N>(ActiveXColumn<N> column) where N : System.Windows.Forms.AxHost, new()
        {
            return new ActiveXParameter<T>(x => (T)(object)column.Value, v => column.Value = (N)(object)v, column);
        }

    }
    public class ComParameter<T> : ParameterBase<T> where T : class

    {
        public ComParameter(TypedColumnBase<T> column)
            : base(column)
        {
        }
        ComParameter(Func<ColumnBase, T> getValue, Action<T> setReturnValue,
           ColumnBase column)
          : base(getValue, setReturnValue, column)
        {
        }
        public static implicit operator ComParameter<T>(ComColumn<T> column)
        {
            if (column == null)
                return null;
            return new ComParameter<T>(column);
        }
        public ComParameter(T value)
            : base(value)
        {
        }
        public static implicit operator ComParameter<T>(T value)
        {
            return new ComParameter<T>(value);
        }
        public static ComParameter<T> Cast<N>(ComColumn<N> column)
        {
            return new ComParameter<T>(x => (T)(object)column.Value, v => column.Value = (N)(object)v, column);
        }
    }

    public class DotnetParameter<T> : ParameterBase<T>

    {
        public DotnetParameter(TypedColumnBase<T> column)
            : base(column)
        {
        }
        DotnetParameter(Func<ColumnBase, T> getValue, Action<T> setReturnValue,
            ColumnBase column)
           : base(getValue, setReturnValue, column)
        {
        }
        public static implicit operator DotnetParameter<T>(DotnetColumn<T> column)
        {
            if (column == null)
                return null;
            return new DotnetParameter<T>(column);
        }
        public DotnetParameter(T value)
            : base(value)
        {
        }
        public DotnetParameter(T value, Action<T> setReturnValue)
            : base(x=>value,setReturnValue,null)
        {
        }
        public static implicit operator DotnetParameter<T>(T value)
        {
            return new DotnetParameter<T>(value);
        }
        public static DotnetParameter<T> Cast<N>(DotnetColumn<N> column)
        {
            return new DotnetParameter<T>(x => (T)(object)column.Value, v => column.Value = (N)(object)v, column);
        }
    }
}
