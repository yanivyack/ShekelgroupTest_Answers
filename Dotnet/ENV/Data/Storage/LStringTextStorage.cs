
using System;
using System.Text;
using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class LStringTextStorage : IColumnStorageSrategy<Text>
    {
        public Text LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            var ba = loader.GetByteArray();
            var length = ba[0];
            return Encoding.Default.GetString(ba, 1, length);
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            if (ReferenceEquals(value, null))
                saver.SaveNull();
            else
            {
                var bytes = Encoding.Default.GetBytes(value);
                var ba = new byte[bytes.Length + 1];
                ba[0] = (byte)bytes.Length;
                Array.Copy(bytes, 0, ba, 1, bytes.Length);
                var btrieveSaver = saver as IBtrieveValueSaver;
                if (btrieveSaver != null)
                    btrieveSaver.SaveByteArray(ba, value, 10);
                else
                    saver.SaveByteArray(ba);
            }
        }
    }
}
