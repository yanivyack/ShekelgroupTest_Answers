using System;
using System.Globalization;
using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class FloatDecimalNumberStorage : IColumnStorageSrategy<Number>
    {
        int _size;

        public FloatDecimalNumberStorage(int size)
        {
            _size = size;
        }

        public Number LoadFrom(IValueLoader loader)
        {
            var bytes = loader.GetByteArray();
            if (bytes[0] == 0) return 0;
            var s = "";
            for (var i = 1; i < bytes.Length; i++)
                s += bytes[i].ToString("X").PadLeft(2, '0');
            return decimal.Parse("." + s.TrimEnd('0')) * (decimal)Math.Pow(10, ((bytes[0] & ~128) - 64)) * ((bytes[0] & 128) > 0 ? -1 : 1);
        }

        public void SaveTo(Number value, IValueSaver saver)
        {
            if (ReferenceEquals(value, null))
            {
                saver.SaveNull();
                return;
            }

            var bytes = new byte[_size];
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
                else i = s.Length;
                bytes[0] = (byte)(64 + i + (negative ? 128 : 0));
                s = s.PadRight((_size - 1) * 2, '0');
                for (var j = 1; j < _size && (j - 1) * 2 < s.Length; j++)
                    bytes[j] = byte.Parse(s.Substring((j - 1) * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }

            var btrieveSaver = saver as IBtrieveValueSaver;
            if (btrieveSaver != null)
                btrieveSaver.SaveByteArray(bytes, value.ToDecimal());
            else
                saver.SaveByteArray(bytes);
        }
    }

    public class ExtendedFloatNumberStorage : IColumnStorageSrategy<Number>
    {
        public Number LoadFrom(IValueLoader loader)
        {
            var ba = loader.GetByteArray();
            var x = BitConverter.ToUInt16(ba, 0);
            if (x > 32)
                x -= 32;
            var xb = BitConverter.GetBytes(x);
            var nba = new byte[] { ba[6], ba[7], ba[4], ba[5], ba[2], ba[3], xb[0], xb[1] };
            return BitConverter.ToDouble(nba, 0);
        }

        public void SaveTo(Number value, IValueSaver saver)
        {
            var ba = BitConverter.GetBytes((double)value);
            var x = BitConverter.ToUInt16(ba, 6);
            if (x != 0)
                x += 32;
            var xb = BitConverter.GetBytes(x);
            saver.SaveByteArray(new byte[] { xb[0], xb[1], ba[4], ba[5], ba[2], ba[3], ba[0], ba[1] });
        }
    }

}
