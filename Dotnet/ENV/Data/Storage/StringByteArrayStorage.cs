using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class StringByteArrayStorage : IColumnStorageSrategy<byte[]>
    {
        public byte[] LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            var s = loader.GetString();
            return ByteArrayColumn.ToUnicodeByteArray(s);
        }

        public void SaveTo(byte[] value, IValueSaver saver)
        {
            if (value == null)
            {
                saver.SaveNull();
                return;
            }
            var s = ByteArrayColumn.UnicodeWithoutGaps.GetString(value);
            saver.SaveString(s, s.Length,false);
        }
    }
    public class UTF8ByteArrayStorage : IColumnStorageSrategy<byte[]>
    {
        public byte[] LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            var s = loader.GetString();
            return Encoding.UTF8.GetBytes(s);
        }

        public void SaveTo(byte[] value, IValueSaver saver)
        {
            if (value == null)
            {
                saver.SaveNull();
                return;
            }
            var s = Encoding.UTF8.GetString(value);
            saver.SaveString(s, s.Length, false);
        }
    }
}
