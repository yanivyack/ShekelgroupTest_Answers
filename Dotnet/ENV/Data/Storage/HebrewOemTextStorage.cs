using System;
using System.Collections.Generic;
using System.Text;
using Firefly.Box;
using Firefly.Box.Data;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class HebrewOemTextStorage : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>
    {
        Firefly.Box.Data.TextColumn _column;
        static System.Text.Encoding _ansi = System.Text.Encoding.GetEncoding(1255);
        static Dictionary<char, char> _fromDb = new Dictionary<char, char>(), _toDb = new Dictionary<char, char>();
        static HebrewOemTextStorage()
        {

            for (byte i = 128; i < 160; i++)
            {
                var low = _ansi.GetString(new byte[] { i })[0];
                var high = (char)('א' + i - 128);
                _fromDb.Add(low, high);
                _toDb.Add(high, low);


            }
        }
        static string Trans(string s, Dictionary<char, char> x)
        {
            var c = s.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                char r;
                if (x.TryGetValue(c[i], out r))
                    c[i] = r;
            }
            return new string(c);
        }
        int _maxDataLength = -1;
        public HebrewOemTextStorage(Firefly.Box.Data.TextColumn column)
        {
            _column = column;
        }

        public static Text Decode(Text text)
        {
            byte[] bytes = _ansi.GetBytes(text.ToString().ToCharArray());
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = DecodeByte(bytes[i]);
            }
            return new string(_ansi.GetChars(bytes));
        }

        public static byte DecodeByte(byte b)
        {
            if (b > 127 && b <= 159)
            {
                b = (byte)(b + 96);
            }
            return b;
        }

        public static Text Encode(Text text)
        {
            byte[] bytes = _ansi.GetBytes(text.ToString().ToCharArray());
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = EncodeByte(bytes[i]);
            }
            return new string(_ansi.GetChars(bytes));
        }

        public static byte EncodeByte(byte b)
        {
            if (b > 223 && b < 251)
            {
                b = (byte)(b - 96);
            }
            if (b >= 155 && b <= 159)
            {
                b = (byte)(b + 96);
            }
            return b;
        }

        public Text LoadFrom(IValueLoader loader)
        {
            
            if (loader.IsNull())
                return null;
            return Trans(loader.GetString(),_fromDb);
         //   return Decode(loader.GetString());
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            if (object.ReferenceEquals(value, null))
                saver.SaveNull();
            else
            {
                if (_maxDataLength == -1) _maxDataLength = _column.FormatInfo.MaxDataLength;
                saver.SaveAnsiString(Trans(value, _toDb), _maxDataLength, false);
                //saver.SaveAnsiString(Encode(value), _maxDataLength, false);
            }
        }
    }
    
    public class V8HebrewOemTextStorage : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>
    {
        Firefly.Box.Data.TextColumn _column;
        int _maxDataLength = -1;
        static System.Text.Encoding _ansi = System.Text.Encoding.GetEncoding(1255);

        HebrewTextTools.V8OemEncoding _enc = new HebrewTextTools.V8OemEncoding();
        public V8HebrewOemTextStorage(Firefly.Box.Data.TextColumn column)
        {
            _column = column;
        }
        public Text LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            return _enc.GetString(_ansi.GetBytes(loader.GetString()));
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            if (object.ReferenceEquals(value, null))
                saver.SaveNull();
            else
            {
                if (_maxDataLength == -1) _maxDataLength = _column.FormatInfo.MaxDataLength;
                saver.SaveAnsiString(_ansi.GetString(_enc.GetBytes(value)), _maxDataLength, false);
            }
        }
    }
}