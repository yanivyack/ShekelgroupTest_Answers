using System;
using Firefly.Box;
using Firefly.Box.Data;
using Firefly.Box.Data.Advanced;
using ByteArrayColumn = ENV.Data.ByteArrayColumn;

namespace ENV
{
    public class ByteArrayParameter : ParameterBase<byte[]>
    {

        public ByteArrayParameter(byte[] value)
            : base(col =>
            {
                if (value == null)
                    return null;
                var bac = col as ByteArrayColumn;
                if (bac == null)
                    return value;

                if (bac.ContentType == Data.ByteArrayColumnContentType.Unicode)
                {
                    if (!IsUnicodeValue(value)) {
                        return ByteArrayColumn.ToUnicodeByteArray(ByteArrayColumn.AnsiByteArrayToString(value));
                    }
                }
                return value;
            })
        {
        }
        static bool IsUnicodeValue(byte[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == 0)
                    return true;

            }
            return false;
        }

        public ByteArrayParameter(TypedColumnBase<byte[]> column)
            : base(column)
        {
        }

        public ByteArrayParameter(Func<ColumnBase, byte[]> getValue, Action<ColumnBase, byte[]> setReturnValue, ColumnBase value)
            : base(getValue, setReturnValue, value)
        {
        }
        Func<object> _rawValue;
        protected override object _GetRawValue()
        {
            if (_rawValue != null)
                return _rawValue();
            return base._GetRawValue();
        }
        public ByteArrayParameter(Func<ColumnBase, byte[]> getValue, object rawValue)
            : base(getValue)
        {
            _rawValue = () => rawValue;
        }

        public ByteArrayParameter(object o)
            : base(@base => { return ((ByteArrayColumn)@base).Cast(o); })
        {
        }

        public static implicit operator ByteArrayParameter(byte[] value)
        {
            return new ByteArrayParameter(value);
        }
        public static implicit operator ByteArrayParameter(Firefly.Box.Data.ByteArrayColumn column)
        {
            if (column == null)
                return null;
            return new ByteArrayParameter(t =>
                                          {
                                              var z = t as ENV.Data.ByteArrayColumn;
                                              if (z != null)
                                                  return z.ToByteArray(column);
                                              return column.Value;
                                          }, (c, v) =>
                                             {
                                                 var z = column as ByteArrayColumn;
                                                 var z2 = c as ByteArrayColumn;
                                                 if (z != null && z2 != null)
                                                     z.Value = z.ToByteArray(z2);
                                                 else
                                                 {
                                                     z.Value = v;
                                                 }
                                             }, column);
        }
        public static implicit operator ByteArrayParameter(Text value)
        {
            return new ByteArrayParameter(c =>
                                          {
                                              if (value == null)
                                                  return null;
                                              return ((ByteArrayColumn)c).FromString(value);
                                          }, value);
        }
        public static implicit operator ByteArrayParameter(TextColumn value)
        {
            if (value == null)
                return null;
            return new ByteArrayParameter(c =>
            {
                var bc = c as ByteArrayColumn;
                if (!ReferenceEquals(bc, null))
                    return bc.FromString(value);
                return null;
            }, (c, bytes) => value.Value = bytes == null ? null : ((ByteArrayColumn)c).ToString(bytes), value);
        }
        public static implicit operator ByteArrayParameter(DotnetColumn<string> value)
        {
            if (value == null)
                return null;
            return new ByteArrayParameter(c => ((ByteArrayColumn)c).FromString(value), (c, bytes) => value.Value = bytes == null ? null : ((ByteArrayColumn)c).ToString(bytes), value);
        }
        public static implicit operator ByteArrayParameter(string value)
        {
            return new ByteArrayParameter(c =>
                                          {
                                              if (value == null)
                                                  return null;
                                              return ((ByteArrayColumn)c).FromString(value);
                                          }, value);
        }
        public static implicit operator ByteArrayParameter(ArrayColumn<Text> value)
        {
            return new ByteArrayParameter(c =>
            {
                if (value == null)
                    return null;
                return value.ToByteArray();
            }, value.ToByteArray());
        }
    }
}