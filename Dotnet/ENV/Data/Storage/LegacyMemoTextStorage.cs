using System;
using System.Collections.Generic;
using System.IO;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class LegacyMemoTextStorage : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>
    {
        System.Text.Encoding _e = ENV.LocalizationInfo.Current.OuterEncoding;

        int _maxLength = -1;

        public LegacyMemoTextStorage(int maxLength)
        {
            _maxLength = maxLength;
        }

        public Text LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            return Decode(loader.GetByteArray());
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            if (object.ReferenceEquals(value, null))
                saver.SaveNull();
            else
            {
                var b = Encode(value);
                if (_maxLength >= 0 && _maxLength + 2 < b.Length)
                    Array.Resize(ref b, _maxLength + 2);
                saver.SaveByteArray(b);
            }
        }

        public byte[] Encode(string s)
        {
            if (string.IsNullOrEmpty(s))
                return new byte[0];
            var bytes = _e.GetBytes(s);
            byte currentByte = bytes[0];
            byte count = 1;

            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            var compressionUsed = false;
            for (int i = 1; i < bytes.Length + 1; i++)
            {
                if (i == bytes.Length || bytes[i] != currentByte || count == 255)
                {
                    if (count < 4)
                        while (count-- > 0)
                            bw.Write(currentByte);
                    else
                    {
                        bw.Write(new byte[] { 0, count, currentByte });
                        compressionUsed = true;
                    }
                    count = 1;
                    if (i < bytes.Length)
                        currentByte = bytes[i];
                }
                else
                    count++;
            }

            var result = new byte[ms.Length + 2];
            var size = BitConverter.GetBytes((short)(ms.Length * (compressionUsed ? 1 : -1)));
            result[0] = size[1];
            result[1] = size[0];
            Array.Copy(ms.GetBuffer(), 0, result, 2, ms.Length);
            return result;
        }

        public string Decode(byte[] s)
        {
            if (s == null)
                return "";
            if (s.Length < 2)
                return "";

            var size = BitConverter.ToInt16(new[] {s[1], s[0]}, 0);
            
            if (size < 0)
                return new string(_e.GetChars(s, 2, s.Length - 2));

            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);

            for (int i = 2; i < s.Length; i++)
            {
                if (s[i] == 0)
                {
                    for (int j = 0; j < s[i + 1]; j++)
                        bw.Write(s[i + 2]);
                    i += 2;
                }
                else
                    bw.Write(s[i]);
            }

            return new string(_e.GetChars(ms.ToArray()));
        }

    }
}
