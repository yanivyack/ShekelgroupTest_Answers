using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class BtrieveDateStorage : IColumnStorageSrategy<Date>
    {
        static StringDateStorage _stringDateStorage = new StringDateStorage();

        public Date LoadFrom(IValueLoader loader)
        {
            var btr = loader as IBtrieveValueLoader;
            if (btr != null)
                return btr.GetDate();
            return _stringDateStorage.LoadFrom(loader);
        }

        public void SaveTo(Date value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else
            {
                var btr = saver as IBtrieveValueSaver;
                if (btr != null)
                    btr.SaveDate(value);
                else
                    _stringDateStorage.SaveTo(value, saver);
            }
        }
    }
}
