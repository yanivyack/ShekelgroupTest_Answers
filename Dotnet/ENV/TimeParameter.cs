using System;
using Firefly.Box;
using Firefly.Box.Data.Advanced;

namespace ENV
{
    public class TimeParameter : ParameterBase<Time>
    {
        public TimeParameter(int hours,int minutes,int seconds):base(new Time(hours,minutes,seconds))
        {
        }
        internal TimeParameter(ObjectParameterBridge<Time> x) : base(x)
        {
        }

        public TimeParameter(Time value)
            : base(value)
        {
        }
        internal TimeParameter(string value)
            : base(column=> Time.Parse(value, column.Format))
        {
        }

        public TimeParameter(TypedColumnBase<Time> column)
            : base(column)
        {
        }

        TimeParameter(Func<ColumnBase,Time> getValue, Action<Time> setReturnValue,ColumnBase column) : base(getValue, setReturnValue,column)
        {
        }

        public static implicit operator TimeParameter(Number value)
        {
            return new TimeParameter(ENV.UserMethods.Instance.ToTime(value));
        }
        public static implicit operator TimeParameter(Firefly.Box.Data.NumberColumn value)
        {
            if (value == null)
                return null;
            return new TimeParameter((s) => ENV.UserMethods.Instance.ToTime(value), a => value.Value = ENV.UserMethods.Instance.ToNumber(a), value);
        }
        public static implicit operator TimeParameter(Firefly.Box.Data.DateColumn value)
        {
            if (value == null)
                return null;
            return new TimeParameter((s) => ENV.UserMethods.Instance.ToTime(value.Value), a => value.Value = ENV.UserMethods.Instance.ToDate(a), value);
        }
        public static implicit operator TimeParameter(int value)
        {
            return new TimeParameter(ENV.UserMethods.Instance.ToTime(value));
        }
        public static implicit operator TimeParameter(Time value)
        {
            return new TimeParameter(value);
        }
        public static implicit operator TimeParameter(Date value)
        {
            return new TimeParameter(ENV.UserMethods.Instance.ToTime(value));
        }

        public static implicit operator TimeParameter(TypedColumnBase<Time> column)
        {
            if (column == null)
                return null;
            return new TimeParameter(column);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
        
    }
}