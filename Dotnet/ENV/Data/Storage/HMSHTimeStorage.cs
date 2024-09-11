using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class HMSHTimeStorage : IColumnStorageSrategy<Time>
    {
        static StringTimeStorage _stringTimeStorage = new StringTimeStorage();

        public Time LoadFrom(IValueLoader loader)
        {
            var btr = loader as IBtrieveValueLoader;
            if (btr != null)
                return btr.GetTime();
            return _stringTimeStorage.LoadFrom(loader);
        }

        public void SaveTo(Time value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else
            {
                var btr = saver as IBtrieveValueSaver;
                if (btr != null)
                    btr.SaveTime(value);
                else
                    _stringTimeStorage.SaveTo(value, saver);
            }
        }
    }
}
