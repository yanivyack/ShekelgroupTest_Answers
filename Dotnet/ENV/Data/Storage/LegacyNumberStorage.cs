using System;
using System.Globalization;
using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class LegacyNumberStorage : IColumnStorageSrategy<Number>
    {
        int _size;

        public LegacyNumberStorage(int size)
        {
            _size = size;
        }

        public Number LoadFrom(IValueLoader loader)
        {
            return FromBytes(loader.GetByteArray());
        }

        public void SaveTo(Number value, IValueSaver saver)
        {
            if (ReferenceEquals(value, null))
            {
                saver.SaveNull();
                return;
            }

            var btrieveSaver = saver as IBtrieveValueSaver;
            if (btrieveSaver != null)
                btrieveSaver.SaveByteArray(ToBytes(value, _size), value.ToDecimal());
            else
                saver.SaveByteArray(ToBytes(value, _size));
        }

        internal static Number FromBytes(byte[] bytes)
        {
            if (bytes[0] == 128 || bytes[0] == 80) return 0;

            var neg = bytes[0] < 100;
            var valueBytesLength = neg ? 63 - bytes[0] : bytes[0] - 192;
            Number r = 0;
            var i = 0;
            for (; i < valueBytesLength; i++)
            {
                var x = neg ? 255 - bytes[i + 1] : bytes[i + 1];
                r += x * Math.Pow(100, valueBytesLength - i - 1);
            }
            for (; i < bytes.Length - 1; i++)
            {
                var x = neg ? 255 - bytes[i + 1] : bytes[i + 1];
                r += x * Math.Pow(0.01, i + 1 - valueBytesLength);
            }
            return neg ? r * -1 : r;
        }

        internal static byte[] ToBytes(Number value, int size)
        {
            var bytes = new byte[size];

            if (value == 0)
                bytes[0] = 128;
            else
            {
                var neg = value < 0;
                decimal absVal = Math.Abs(value.ToDecimal());
                var s = absVal.ToString(CultureInfo.InvariantCulture).Split('.');
                var sizeForWholePart = s[0] == "0" ? 0 : (s[0].Length + 1) / 2;
                var totalSize = sizeForWholePart + (s.Length > 1 ? (s[1].Length + 1) / 2 : 0);
                bytes[0] = (byte)(neg ? 63 - sizeForWholePart : 192 + sizeForWholePart);

                var i = 0;
                for (; i < sizeForWholePart; i++)
                {
                    var x = (byte)Math.Floor(absVal / (decimal)Math.Pow(100, sizeForWholePart - i - 1));
                    bytes[i + 1] = neg ? (byte)(255 - x) : x;
                    absVal -= x * (decimal)Math.Pow(100, sizeForWholePart - i - 1);
                }
                for (; i < totalSize; i++)
                {
                    var x = (byte)Math.Floor(absVal * (decimal)Math.Pow(100, i - sizeForWholePart + 1));
                    bytes[i + 1] = neg ? (byte)(255 - x) : x;
                    absVal -= x * (decimal)Math.Pow(0.01, i - sizeForWholePart + 1);
                }
                if (neg)
                    for (int j = totalSize + 1; j < size; j++)
                        bytes[j] = 255;
            }

            return bytes;
        }

        internal string GetSizeString()
        {
            return _size.ToString();
        }
    }
}
