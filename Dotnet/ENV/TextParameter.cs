using System;
using System.Windows.Forms;
using Firefly.Box;
using Firefly.Box.Data.Advanced;

namespace ENV
{
    public class TextParameter : ParameterBase<Text>
    {
        public TextParameter(Text value)
            : base(value)
        {
        }

        public TextParameter(TypedColumnBase<Text> column)
            : base(column)
        {
        }

        public TextParameter(Func<ColumnBase, Text> getValue, Action<Text> setReturnValue, ColumnBase column)
            : base(getValue, setReturnValue, column)
        {
        }
        internal TextParameter(ObjectParameterBridge<Text> x) : base(x)
        {
        }


        public static implicit operator TextParameter(Text value)
        {
            return new TextParameter(value);
        }
        public static implicit operator TextParameter(string value)
        {
            return new TextParameter(value);
        }
        public static implicit operator TextParameter(Command value)
        {
            return new TextParameter(value);
        }
        public static implicit operator TextParameter(CustomCommand value)
        {
            return new TextParameter(value);
        }
        public static implicit operator TextParameter(Keys value)
        {

            return new TextParameter(value);
        }
        public static implicit operator TextParameter(TypedColumnBase<Text> column)
        {
            if (column == null)
                return null;
            return new TextParameter(column);
        }
        public static implicit operator TextParameter(Firefly.Box.Data.ByteArrayColumn column)
        {
            if (column == null)
                return null;
            return new TextParameter(x => column.ToString(),
                value =>
                {
                    var bac = column as ENV.Data.ByteArrayColumn;
                    if (bac != null && bac.ContentType == Data.ByteArrayColumnContentType.BinaryUnicode)
                        column.Value = ENV.Data.ByteArrayColumn.ToAnsiByteArray(value);
                    else
                        column.Value = column.FromString(value);
                }
                , column);
        }
        public static implicit operator TextParameter(ENV.DotnetColumn<string> column)
        {
            if (column == null)
                return null;
            return new TextParameter(x => column.Value,
                value =>
                {
                    if (value == null)
                        column.Value = null;
                    else
                        column.Value = value.TrimEnd();
                }
                , column);
        }
        public static implicit operator TextParameter(byte[] value)
        {
            if (value == null)
                return new TextParameter((Text)null);
            return new TextParameter(new Text(new string(LocalizationInfo.Current.InnerEncoding.GetChars(value))));
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        internal override void SetValueToBoundColumn(Text value, TypedColumnBase<Text> column)
        {
            base.SetValueToBoundColumn(value, column);
            var z = column as ENV.Data.TextColumn;
            if (z != null && !ReferenceEquals(value, null))
                z.RecievedParameterValueOtherThenNull = true;
        }

        
    }

}
