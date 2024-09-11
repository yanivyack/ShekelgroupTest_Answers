using System.Globalization;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using System;

namespace ENV
{
    public class BoolParameter : ParameterBase<Bool>
    {
        public BoolParameter(Bool value)
            : base(value)
        {
        }
        internal BoolParameter(ObjectParameterBridge<Bool> x) : base(x)
        {
        }

        public BoolParameter(TypedColumnBase<Bool> column)
            : base(column)
        {
        }
        public BoolParameter(Func<ColumnBase, Bool> getValue, Action<Bool> setReturnValue, ColumnBase column) : base(getValue, setReturnValue, column) { }
        public static implicit operator BoolParameter(Bool value)
        {
            return new BoolParameter(value);
        }
        
        public static implicit operator BoolParameter(bool value)
        {
            return new BoolParameter(value);
        }
        public static implicit operator BoolParameter(TypedColumnBase<Bool> column)
        {
            if (column == null)
                return null;
            return new BoolParameter(column);
        }

        public static BoolParameter FromString(string s)
        {
            if (s == null)
                return new BoolParameter((Bool)null);
            if (s.Length > 0)
                if (s == "")
                    return new BoolParameter(false);
                else
                    return new BoolParameter(c =>
                    {
                        if (s[0].ToString().ToUpper(CultureInfo.InvariantCulture) == "T")
                            return true;
                        var nc = c as Firefly.Box.Data.BoolColumn;
                        if (nc != null)
                            return (Bool)nc.Parse(s, nc.Format);
                        return false;

                    }, delegate { }, null);
            
            return false;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}