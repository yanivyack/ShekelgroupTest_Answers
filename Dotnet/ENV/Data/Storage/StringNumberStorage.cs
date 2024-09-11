using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class StringNumberStorage : IColumnStorageSrategy<Number>
    {
        int _size;
        int _decimalDigits;

        public StringNumberStorage(int size, int decimalDigits) 
        {
            _size = size;
            _decimalDigits = decimalDigits;
        }

        public Number LoadFrom(IValueLoader loader)
        {
            return Number.Parse(loader.GetString());
        }

        public void SaveTo(Number value, IValueSaver saver)
        {
            var s = value.ToString((_decimalDigits > 0 ? (_size - 1 - _decimalDigits) + "." + _decimalDigits 
                : _size.ToString()) + "N");
            if (s.Length > _size) s = s.Remove(0, s.Length - _size);
            saver.SaveAnsiString(s, _size, false);
        }
    }
    public class ZeroPadStringNumberStorage : IColumnStorageSrategy<Number>
    {
        int _size;
        int _decimalDigits;

        public ZeroPadStringNumberStorage(int size, int decimalDigits)
        {
            _size = size;
            _decimalDigits = decimalDigits;
        }

        public Number LoadFrom(IValueLoader loader)
        {
            return Number.Parse(loader.GetString());
        }

        public void SaveTo(Number value, IValueSaver saver)
        {
            var s = value.ToString((_decimalDigits > 0 ? (_size - 1 - _decimalDigits) + "." + _decimalDigits
                : _size.ToString()) + "NP0");
            if (s.Length > _size) s = s.Remove(0, s.Length - _size);
            saver.SaveAnsiString(s, _size, false);
        }
    }
    public class ByteArrayNumberStorage : IColumnStorageSrategy<Number>, IStorageScriptCreator
    {
        int _size;
        int _decimalDigits;

        public ByteArrayNumberStorage(int size, int decimalDigits)
        {
            _size = size;
            _decimalDigits = decimalDigits;
        }

        public Number LoadFrom(IValueLoader loader)
        {
            return Number.Parse(LocalizationInfo.Current.OuterEncoding.GetString( loader.GetByteArray()));
        }

        public void SaveTo(Number value, IValueSaver saver)
        {
            saver.SaveByteArray(
                LocalizationInfo.Current.OuterEncoding .GetBytes(value.ToString(_decimalDigits > 0 ? (_size - 2 - _decimalDigits) + "." + _decimalDigits + "N"
                    : (_size - 1) + "N")));
        }


        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddBinary(name, caption, _size);
        }
    }

    public class CharacterNumberStorage : IColumnStorageSrategy<Number>
    {
        int _size;
        int _decimalDigits;

        public CharacterNumberStorage(int size, int decimalDigits)
        {
            _size = size;
            _decimalDigits = decimalDigits;
        }

        public Number LoadFrom(IValueLoader loader)
        {
            var x = Number.Parse(loader.GetString(), string.Format("{0}NP0+,+;-,-;", _size - 1));
            return _decimalDigits > 0 ? (Number)decimal.Divide(x.ToDecimal(), (decimal)System.Math.Pow(10, _decimalDigits)) : x;
        }

        public void SaveTo(Number value, IValueSaver saver)
        {
            var x = _decimalDigits > 0 ? (Number)((long)(value * System.Math.Pow(10, _decimalDigits))) : value;
            saver.SaveAnsiString(x.ToString(string.Format("{0}NP0+,+;-,-;", _size - 1)), _size, false);
        }
    }
}
