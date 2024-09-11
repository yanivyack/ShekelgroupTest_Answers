using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class IntBoolStorage : IColumnStorageSrategy<Bool>
    {
        public Bool LoadFrom(IValueLoader loader)
        {
            return loader.GetNumber() == 1;
        }

        public void SaveTo(Bool value, IValueSaver saver)
        {
            saver.SaveInt(value ? 1 : 0);
        }
    }
}