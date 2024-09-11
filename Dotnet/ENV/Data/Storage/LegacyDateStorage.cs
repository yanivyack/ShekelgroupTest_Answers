using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class LegacyDateStorage : IColumnStorageSrategy<Date>
    {
        public Date LoadFrom(IValueLoader loader)
        {
            return new Date().AddDays(LegacyNumberStorage.FromBytes(loader.GetByteArray()));
        }

        public void SaveTo(Date value, IValueSaver saver)
        {
            if (ReferenceEquals(value, null))
            {
                saver.SaveNull();
                return;
            }

            var n = value - new Date();

            var btrieveSaver = saver as IBtrieveValueSaver;
            if (btrieveSaver != null)
                btrieveSaver.SaveByteArray(LegacyNumberStorage.ToBytes(n, 4), n);
            else
                saver.SaveByteArray(LegacyNumberStorage.ToBytes(n, 4));
        }
    }
}
