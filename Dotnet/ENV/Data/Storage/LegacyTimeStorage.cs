using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class LegacyTimeStorage : IColumnStorageSrategy<Time>
    {
        public Time LoadFrom(IValueLoader loader)
        {
            return new Time().AddSeconds(LegacyNumberStorage.FromBytes(loader.GetByteArray()));
        }

        public void SaveTo(Time value, IValueSaver saver)
        {
            if (ReferenceEquals(value, null))
            {
                saver.SaveNull();
                return;
            }

            var btrieveSaver = saver as IBtrieveValueSaver;
            if (btrieveSaver != null)
                btrieveSaver.SaveByteArray(LegacyNumberStorage.ToBytes(value.TotalSeconds, 4), value.TotalSeconds);
            else
                saver.SaveByteArray(LegacyNumberStorage.ToBytes(value.TotalSeconds, 4));
        }
    }
}
