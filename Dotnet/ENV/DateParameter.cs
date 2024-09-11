using System;
using Firefly.Box;
using Firefly.Box.Data.Advanced;

namespace ENV
{
    public class DateParameter : ParameterBase<Date>
    {
        public DateParameter(int year, int month, int day)
            : base(new Date(year, month, day))
        {
        }
        internal DateParameter(ObjectParameterBridge<Date> x) : base(x)
        {
        }
        public DateParameter(Date value)
            : base(value)
        {
        }
        internal DateParameter(string value)
            : base(column => Date.Parse(value, column.Format))
        {
        }

        public DateParameter(TypedColumnBase<Date> column)
            : base(column)
        {
        }

        DateParameter(Func<ColumnBase, Date> getValue, Action<Date> setReturnValue, ColumnBase column)
            : base(getValue, setReturnValue, column)
        {
        }

        public static implicit operator DateParameter(Number value)
        {
            return new DateParameter(ENV.UserMethods.Instance.ToDate(value));
        }
        public static implicit operator DateParameter(Firefly.Box.Data.NumberColumn value)
        {
            if (value == null)
                return null;
            return new DateParameter((s) => ENV.UserMethods.Instance.ToDate(value), a => value.Value = ENV.UserMethods.Instance.ToNumber(a), value);
        }

        public static implicit operator DateParameter(Firefly.Box.Data.TimeColumn value)
        {
            if (value == null)
                return null;
            return new DateParameter((s) => ENV.UserMethods.Instance.ToDate(value), a => value.Value = ENV.UserMethods.Instance.ToTime(a), value);
        }
        public static implicit operator DateParameter(int value)
        {
            return new DateParameter(ENV.UserMethods.Instance.ToDate(value));
        }
        public static implicit operator DateParameter(Date value)
        {
            return new DateParameter(value);
        }
        public static implicit operator DateParameter(Time value)
        {
            return new DateParameter(ENV.UserMethods.Instance.ToDate(value));
        }
        public static implicit operator DateParameter(TypedColumnBase<Date> column)
        {
            if (column == null)
                return null;
            return new DateParameter(column);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
        
    }
}