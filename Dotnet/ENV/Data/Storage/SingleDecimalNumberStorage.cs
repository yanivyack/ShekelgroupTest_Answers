using System;
using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class SingleDecimalNumberStorage : IColumnStorageSrategy<Number>
    {
        public Number LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            return loader.GetNumber();
        }

        public void SaveTo(Number value, IValueSaver saver)
        {
            if (ReferenceEquals(value, null))
            {
                saver.SaveNull();
                return;
            }

            var btrieveSaver = saver as IBtrieveValueSaver;
            if (btrieveSaver != null)
            {
                btrieveSaver.SaveSingleDecimal(value);
                return;
            }
            
            saver.SaveDecimal(value, 18, 5);
        }
    }
}
