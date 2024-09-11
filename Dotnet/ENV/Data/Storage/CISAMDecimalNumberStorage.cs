using System;
using System.Globalization;
using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class CISAMDecimalNumberStorage : IColumnStorageSrategy<Number>
    {
        int _size;

        public CISAMDecimalNumberStorage(int size)
        {
            _size = size;
        }

        public Number LoadFrom(IValueLoader loader)
        {
            var bytes = loader.GetByteArray();
            if ((bytes[0] & ~128) == 0) return 0;
            var s = "";
            for (var i = 1; i < bytes.Length; i++)
                s += bytes[i].ToString().PadLeft(2, '0');
            return (decimal.Parse("." + s.TrimEnd('0')) - ((bytes[0] & 128) > 0 ? 0 : 1)) * 
                (decimal)Math.Pow(10, Math.Abs((bytes[0] & ~128) - 63.5) * 2 - 1);
        }

        public void SaveTo(Number value, IValueSaver saver)
        {
            if (ReferenceEquals(value, null))
            {
                saver.SaveNull();
                return;
            }

            var bytes = new byte[_size];
            bytes[0] = 128;
            if (value != 0)
            {
                var negative = value < 0;
                var s = value.ToDecimal().ToString(CultureInfo.InvariantCulture).TrimStart('-', '0');
                var i = s.IndexOf('.');
                if (i >= 0)
                {
                    s = s.Remove(i, 1);
                    if (i == 0)
                    {
                        i = s.Length;
                        s = s.TrimStart('0');
                        i = s.Length - i;
                    }
                }
                else
                {
                    if (s.Length % 2 != 0) s = "0" + s;
                    i = s.Length;
                }
                bytes[0] = (byte)(64 + (negative ? -1 : 1) * i / 2 + (negative ? -1 : 128));
                if (negative)
                    s = ((decimal)Math.Pow(10, i) + value.ToDecimal()).ToString(CultureInfo.InvariantCulture);
                s = s.PadRight((_size - 1) * 2, '0');
                for (var j = 1; j < _size && (j - 1) * 2 < s.Length; j++)
                    bytes[j] = byte.Parse(s.Substring((j - 1) * 2, 2));
            }

            var btrieveSaver = saver as IBtrieveValueSaver;
            if (btrieveSaver != null)
                btrieveSaver.SaveByteArray(bytes, value.ToDecimal());
            else
                saver.SaveByteArray(bytes);
        }
    }
}
