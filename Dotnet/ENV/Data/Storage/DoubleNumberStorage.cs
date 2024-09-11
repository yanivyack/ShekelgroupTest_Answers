using System;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class DoubleNumberStorage : IColumnStorageSrategy<Number>
    {
        public Number LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull()) return null;
            return loader.GetNumber();
        }

        public void SaveTo(Number value, IValueSaver saver)
        {
            if (value == null) saver.SaveNull();
            else saver.SaveDecimal(value, 0, 0);
        }
    }
}
