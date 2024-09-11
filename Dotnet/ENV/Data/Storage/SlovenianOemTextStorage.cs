using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class SlovenianOemTextStorage : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>
    {
        Firefly.Box.Data.TextColumn _column;
        static System.Text.Encoding _ansi = System.Text.Encoding.GetEncoding(852);
        internal static byte[] _from = new byte[256], _to = new byte[256];
        int _maxDataLength = -1;
        static SlovenianOemTextStorage()
        {
            for (int i = 0; i < 255; i++)
            {
                _from[i] = (byte)i;
                _to[i] = (byte)i;
            }
            foreach (var item in new Dictionary<byte, byte>() { { 200, 69 },{ 232, 101 } })
            {
                _from[item.Key] = item.Value;
                _to[item.Value] = item.Key;
            }
        }
        public SlovenianOemTextStorage(Firefly.Box.Data.TextColumn column)
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
    public class V8OemSlovenianEncoding : System.Text.Encoding
    {
        internal static byte[] _from = new byte[256], _to = new byte[256];
        static V8OemSlovenianEncoding()
        {
            for (int i = 0; i < 255; i++)
            {
                _from[i] = (byte)i;
                _to[i] = (byte)i;
            }
            foreach (var item in new Dictionary<byte, byte>() { { 172, 200 }, { 159, 232 }, { 230, 138 }, { 231, 154 }, { 166, 142 }, { 167, 158 }, { 7, 149 }, { 134, 230 }, { 143, 198 }, { 208, 240 }, { 209, 208 } })
            {
                _from[item.Key] = item.Value;
                _to[item.Value] = item.Key;
            }
        }
        System.Text.Encoding _ansi = System.Text.Encoding.GetEncoding(852);
        public override int GetByteCount(char[] chars, int index, int count)
        {
            return count;
        }



        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {

            return _ansi.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
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
