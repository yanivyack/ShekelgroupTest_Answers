using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class BtrieveNumericNumberStorage : IColumnStorageSrategy<Number>
    {
        int _size;
        int _decimalDigits;

        public BtrieveNumericNumberStorage(int size, int decimalDigits)
        {
            _size = size;
            _decimalDigits = decimalDigits;
        }

        public Number LoadFrom(IValueLoader loader)
        {
            var b = loader.GetByteArray();
            var l = b.Length;
            var sign = "+";
            var lastDigit = "0";
            if (b[l - 1] == 125)
                sign = "-";
            else if (b[l - 1] != 123)
            {
                if (b[l - 1] > 64)
                {
                    if (b[l - 1] < 74)
                        lastDigit = (b[l - 1] - 64).ToString();
                    else
                    {
                        sign = "-";
                        lastDigit = (b[l - 1] - 73).ToString();
                    }
                }
                else if (b[l - 1] > 48)
                    lastDigit = (b[l - 1] - 48).ToString();
            }
            var x = Number.Parse(sign + System.Text.Encoding.ASCII.GetString(b, 0, l - 1) + lastDigit);
            return _decimalDigits > 0 ? (Number)decimal.Divide(x.ToDecimal(), (decimal)System.Math.Pow(10, _decimalDigits)) : x;
        }

        public void SaveTo(Number value, IValueSaver saver)
        {
            var x = _decimalDigits > 0 ? (Number)((long)(value * System.Math.Pow(10, _decimalDigits))) : value;

            var s = x.ToString("18P0");
            s = s.Substring(System.Math.Max(s.Length - _size, 0));
            var lastChar = s[s.Length - 1];
            if (lastChar == '0')
                lastChar = value >= 0 ? '{' : '}';
            else
            {
                lastChar = value >= 0 ? System.Text.Encoding.ASCII.GetChars(new[] { (byte)(64 + short.Parse(lastChar.ToString())) })[0] :
                    System.Text.Encoding.ASCII.GetChars(new[] { (byte)(73 + short.Parse(lastChar.ToString())) })[0];
            }
            var ba = System.Text.Encoding.ASCII.GetBytes(s.Substring(0, s.Length - 1) + lastChar);
            var btrieveSaver = saver as IBtrieveValueSaver;
            if (btrieveSaver != null)
                btrieveSaver.SaveByteArray(ba, value.ToDecimal(), 8);
            else
                saver.SaveByteArray(ba);
        }
    }
}
