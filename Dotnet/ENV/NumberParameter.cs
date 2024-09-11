using System;
using Firefly.Box;
using Firefly.Box.Data;
using Firefly.Box.Data.Advanced;

namespace ENV
{
    public class NumberParameter : ParameterBase<Number>
    {
        public NumberParameter(Number value)
            : base(value)
        {
        }
        internal NumberParameter(ObjectParameterBridge<Number> x) : base(x)
        {
        }
        public static bool IgnoreFormatOnReturnValue = true;

        public NumberParameter(TypedColumnBase<Number> column)
            : base((c) =>
                   {
                       if (column == null)
                           return null;
                       return column.Value;
                   }, delegate (Number obj)
            {
                if (column == null)
                    return;
                var x = column.Format;
                if (IgnoreFormatOnReturnValue)
                    column.Format = null;

                try
                {
                    if (ReferenceEquals(obj, null) && !column.AllowNull)
                    {
                        column.AllowNull = true;
                        column.Value = obj;
                        column.AllowNull = false;
                    }
                    else
                        column.Value = obj;
                }
                finally
                {
                    column.Format = x;
                }
            }, column)
        {
            _iAmColumnBased = true;
        }

        bool _iAmColumnBased;
        NumberParameter(Func<ColumnBase, Number> getValue, Action<Number> setReturnValue, ColumnBase column) : base(getValue, setReturnValue, column)
        {
        }

        internal static NumberParameter FromWeb(string webValue)
        {
            if (webValue == "")
                return new NumberParameter((Number)null);
            else
                return new NumberParameter(c =>
                                           {
                                               var nc = c as NumberColumn;
                                               if (nc != null)
                                                   return Number.Parse(webValue, nc.Format);
                                               return Number.Parse(webValue);

                                           }, delegate { }, null);
        }

        internal override void SetValueToBoundColumn(Number value, TypedColumnBase<Number> column)
        {
            if (_iAmColumnBased)
            {
                var f = column.Format;
                column.Format = "";
                column.Value = value;

                column.Format = f;
            }
            else
                column.Value = value;
        }

        public static implicit operator NumberParameter(Number value)
        {
            return new NumberParameter(value);
        }
        public static implicit operator NumberParameter(Time value)
        {
            return new NumberParameter(ENV.UserMethods.Instance.ToNumber(value));
        }
        public static implicit operator NumberParameter(Date value)
        {
            return new NumberParameter(ENV.UserMethods.Instance.ToNumber(value));
        }
        public static implicit operator NumberParameter(Firefly.Box.Data.TimeColumn value)
        {
            if (value == null)
                return null;
            return new NumberParameter((s) => ENV.UserMethods.Instance.ToNumber(value), a => value.Value = ENV.UserMethods.Instance.ToTime(a), value);
        }
        public static implicit operator NumberParameter(Firefly.Box.Data.DateColumn value)
        {
            if (value == null)
                return null;
            return new NumberParameter((s) => ENV.UserMethods.Instance.ToNumber(value), a => value.Value = ENV.UserMethods.Instance.ToDate(a), value);
        }


        public static implicit operator NumberParameter(int value)
        {

            return new NumberParameter(value);
        }
        public static implicit operator NumberParameter(decimal value)
        {
            return new NumberParameter(value);
        }
        public static implicit operator NumberParameter(double value)
        {
            return new NumberParameter(value);
        }
        public static implicit operator NumberParameter(long value)
        {
            return new NumberParameter(value);
        }
        public static implicit operator NumberParameter(TypedColumnBase<Number> column)
        {
            if (column == null)
                return null;
            return new NumberParameter(column);
        }

        public override string ToString()
        {
            return Value.ToString();
        }


    }
}