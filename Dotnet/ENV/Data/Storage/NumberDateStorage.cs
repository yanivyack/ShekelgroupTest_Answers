using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class NumberDateStorage : IColumnStorageSrategy<Date>
    {
        public Date LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            try
            {
                return ENV.UserMethods.Instance.ToDate(loader.GetNumber());
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
            else
                saver.SaveInt(ENV.UserMethods.Instance.ToNumber(value));
        }
    }
}
