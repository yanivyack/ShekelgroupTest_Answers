using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    /// <summary>
    /// this Storage saves to a bytearray and was intended mainly for Btrieve/Pervasive isam. For other db's please use Number1901DateStorageInt
    /// </summary>
    public class Number1901DateStorage : IColumnStorageSrategy<Date>
    {
        IntNumberStorage _uintStorage = new IntNumberStorage();
        public Date LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            var value = _uintStorage.LoadFrom(loader);
            if (value == 0) return Firefly.Box.Date.Empty;
            try
            {
                return ENV.UserMethods.StartDate + loader.GetNumber() - 1;
            }
            catch
            {
                return DateColumn.ErrorDate;
            }
        }

        public void SaveTo(Date value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else if (value == Firefly.Box.Date.Empty)
                _uintStorage.SaveTo(0, saver);
            else
                _uintStorage.SaveTo(value - ENV.UserMethods.StartDate + 1, saver);
        }
    }
    public class Number1901DateStorageInt : IColumnStorageSrategy<Date>
    {
        
        public Date LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            var value = loader.GetNumber(); ;
            if (value == 0) return Firefly.Box.Date.Empty;
            try
            {
                return ENV.UserMethods.StartDate + value - 1;
            }
            catch
            {
                return DateColumn.ErrorDate;
            }
        }

        public void SaveTo(Date value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else if (value == Firefly.Box.Date.Empty)
                saver.SaveInt(0);
            else
                saver.SaveInt(value - ENV.UserMethods.StartDate + 1);
        }
    }
}
