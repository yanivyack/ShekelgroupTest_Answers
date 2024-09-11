using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class ByteArrayTextStorage : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>, ENV.Utilities.IStorageScriptCreator
    {
        System.Text.Encoding _e = ENV.LocalizationInfo.Current.OuterEncoding;

        int _maxLength = -1;

        public ByteArrayTextStorage()
        {
        }

        public ByteArrayTextStorage(int maxLength)
        {
            _maxLength = maxLength;
        }

        public Text LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            try
            {
                var ba = loader.GetByteArray();
                if (ba != null) return Decode(ba);
                var s = loader.GetString();
                if (s == null)
                    return Text.Empty;
                string num = "";
                var bytes = new List<byte>();
                foreach (var c in s)
                {
                    num += c;
                    if (num.Length == 2)
                    {
                        bytes.Add(byte.Parse(num, System.Globalization.NumberStyles.HexNumber));
                        num = "";
                    }
                }
                return Decode(bytes.ToArray());
            }
            catch
            {
                return Text.Empty;
            }

        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            if (object.ReferenceEquals(value, null))
                saver.SaveByteArray(null);
            else
            {
                var v = value.TrimEnd().ToString();
                if (_maxLength >= 0 && _maxLength < v.Length)
                    v = v.Substring(0, _maxLength);
                saver.SaveByteArray(Encode(v));
            }
        }

        public byte[] Encode(string s)
        {
            var result = new List<byte>();
            byte a = (byte)(s.Length % 256);
            byte b = (byte)(s.Length / 256);
            result.Add(a);
            result.Add(b);
            result.AddRange(_e.GetBytes(s));
            return result.ToArray();
        }
        public string Decode(byte[] s)
        {
            if (s == null)
                return "";
            if (s.Length < 2)
                return "";
            var x = new List<byte>(s);
            x.RemoveAt(0);
            x.RemoveAt(0);
            return new string(_e.GetChars(x.ToArray()));
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddBinary(name, caption, 0);
        }
    }
    public class RawByteArrayTextStorage : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>,
       ENV.Utilities.IStorageScriptCreator
    {
        System.Text.Encoding _e = ENV.LocalizationInfo.Current.OuterEncoding;
        int _maxLength = -1;

        public RawByteArrayTextStorage(int maxLength)
        {
            _maxLength = maxLength;
        }

        public RawByteArrayTextStorage()
        {
            _maxLength = -1;
        }

        public Text LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            var ba = loader.GetByteArray();
            if (ba == null)
            {
                var s = loader.GetString();//magic returns garbage here because of hexa stuff - I decided to return string.
                ba = new byte[s.Length / 2];
                for (int i = 0; i < s.Length; i += 2)
                {
                    ba[i / 2] = byte.Parse(s.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
                }
            }
            return _e.GetString(ba);
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            if (object.ReferenceEquals(value, null))
                saver.SaveByteArray(null);
            else
            {
                var v = value.TrimEnd().ToString();
                if (_maxLength >= 0 && _maxLength < v.Length)
                    v = v.Substring(0, _maxLength);
                if (v.Length < _maxLength)
                    v = v.PadRight(_maxLength);
                saver.SaveByteArray(_e.GetBytes(v));
            }
        }
        public bool StoreAsBinary { get; set; }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            if (StoreAsBinary)
                sql.AddBinary(name, caption, _maxLength);
            else
                sql.AddCustomColumn(name, caption, "raw (" + this._maxLength + ")", "", false);
        }
    }
    public class HebrewOemByteArrayStorage : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>
    {
        Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text> _storage;

        public HebrewOemByteArrayStorage(Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text> storage)
        {
            _storage = storage;
        }

        public Text LoadFrom(IValueLoader loader)
        {
            return HebrewOemTextStorage.Decode(_storage.LoadFrom(loader));
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            _storage.SaveTo(HebrewOemTextStorage.Encode(value), saver);
        }
    }
    public class V8HebrewOemByteArrayStorage : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>
    {
        Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text> _storage;
        static System.Text.Encoding _ansi = System.Text.Encoding.GetEncoding(1255);
        HebrewTextTools.V8OemEncoding _enc = new HebrewTextTools.V8OemEncoding();
        public V8HebrewOemByteArrayStorage(Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text> storage)
        {
            _storage = storage;
        }

        public Text LoadFrom(IValueLoader loader)
        {
            return _enc.GetString(_ansi.GetBytes(_storage.LoadFrom(loader)));
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            _storage.SaveTo(_ansi.GetString(_enc.GetBytes(value)), saver);
        }
    }
}