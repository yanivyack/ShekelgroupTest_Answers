using Firefly.Box.Data.DataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENV.Data.Storage
{
    public class Base64ByteArrayStorage : IColumnStorageSrategy<byte[]>
    {
        public byte[] LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            return Convert.FromBase64String(loader.GetString());
        }

        public void SaveTo(byte[] value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else
                saver.SaveString(Convert.ToBase64String(value), 0, false);
        }
    }
}
