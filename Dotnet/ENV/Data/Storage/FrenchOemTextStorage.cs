using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class FrenchOemTextStorage : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>
    {
        Firefly.Box.Data.TextColumn _column;
        static System.Text.Encoding _ansi = System.Text.Encoding.GetEncoding(1252);
        internal static byte[] _from = new byte[256], _to = new byte[256];
        int _maxDataLength = -1;
        static FrenchOemTextStorage()
        {
            for (int i = 0; i < 255; i++)
            {
                _from[i] = (byte)i;
                _to[i] = (byte)i;
            }
            foreach (var item in new Dictionary<byte, byte>() { { 176, 248 } })
            {
                _from[item.Key] = item.Value;
                _to[item.Value] = item.Key;
            }
        }
        public FrenchOemTextStorage(Firefly.Box.Data.TextColumn column)
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
            return _to[b];
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
            return _from[b];
        }

        public Text LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            return Decode(loader.GetString());
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            if (object.ReferenceEquals(value, null))
                saver.SaveNull();
            else
            {
                if (_maxDataLength == -1) _maxDataLength = _column.FormatInfo.MaxDataLength;
                saver.SaveAnsiString(Encode(value), _maxDataLength, false);
            }
        }
    }
    public class V8OemFrenchEncoding : System.Text.Encoding
    {
        internal static byte[] _from = new byte[256], _to = new byte[256];
        static V8OemFrenchEncoding()
        {
            for (int i = 0; i < 255; i++)
            {
                _from[i] = (byte)i;
                _to[i] = (byte)i;
            }
            foreach (var item in new Dictionary<byte, byte>() { { 176, 248 }, { 224, 133 }, { 225, 160 }, { 226, 131 }, { 228, 132 }, { 229, 134 }, { 230, 145 }, { 231, 135 }, { 232, 138 }, { 233, 130 }, { 234, 136 }, { 235, 137 }, { 238, 140 }, { 239, 139 }, { 249, 151 } })
            {
                _from[item.Key] = item.Value;
                _to[item.Value] = item.Key;
            }
        }
        System.Text.Encoding _ansi = System.Text.Encoding.GetEncoding(1252);
        public override int GetByteCount(char[] chars, int index, int count)
        {
            return count;
        }



        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {

            var result =  _ansi.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = _from[bytes[i]];
            }
            return result;
        }



        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            var ba = new byte[byteCount];
            for (int i = byteIndex; i < byteCount; i++)
            {
                ba[i] = _to[bytes[i]];

            }

            var r = _ansi.GetChars(ba, 0, byteCount, chars, charIndex);
            return r;
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }

    }

}
