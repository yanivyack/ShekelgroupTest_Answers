using System;
using System.Globalization;
using System.Text;
using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class PackedDecimalNumberStorage : IColumnStorageSrategy<Number>
    {
        int _size;
        int _decimalDigits;

        public PackedDecimalNumberStorage() : this(10, 0) { }

        public PackedDecimalNumberStorage(int size, int decimalDigits)
        {
            _size = size;
            _decimalDigits = decimalDigits;
        }

        public Number LoadFrom(IValueLoader loader)
        {
            var bArr = loader.GetByteArray();

            decimal result = 0;
            var negative = false;

            for (int i = 0; i < bArr.Length - 1; i++)
            {
                result *= 100;
                int rem = 0;
                result += Math.DivRem(bArr[i], 16, out rem) * 10 + rem;
            }
            {
                result *= 10;
                int rem = 0;
                result += Math.DivRem(bArr[bArr.Length - 1], 16, out rem);
                negative = rem == 13;
            }
            if (_decimalDigits > 0)
                result = result * (decimal)Math.Pow(0.1, _decimalDigits);
            return result * (negative ? -1 : 1);
        }

        public void SaveTo(Number value, IValueSaver saver)
        {
            var d = value.ToDecimal();
            var x = (long)Math.Abs(Math.Truncate(d * (decimal)Math.Pow(10, _decimalDigits)));

            var result = new byte[_size];
            long r;
            x = Math.DivRem(x, 10, out r);
            result[_size - 1] = (byte)(r * 16 + (d >= 0 ? 15 : 13));

            var i = _size - 2;
            while (x > 0 && i >= 0)
            {
                long t;
                x = Math.DivRem(x, 100, out t);
                t = Math.DivRem(t, 10, out r);
                result[i] = (byte)(t * 16 + r);
                i--;
            }

            var btrieveSaver = saver as IBtrieveValueSaver;
            if (btrieveSaver != null)
                btrieveSaver.SaveByteArray(result, null, 5);
            else
                saver.SaveByteArray(result);
        }
    }
}
