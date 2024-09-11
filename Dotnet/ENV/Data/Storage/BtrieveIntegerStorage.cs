using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public abstract class BtrieveIntegerStorage : IColumnStorageSrategy<Number>
    {
        public Number LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            var ba = loader.GetByteArray();
            if (ba != null&&ba.Length>0)
                return GetNumber(loader.GetByteArray());
            return loader.GetNumber();

        }

        protected abstract Number GetNumber(byte[] bytes);

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
                btrieveSaver.SaveInteger(value, GetBytes((long)value), DataTypeCode);
                return;
            }
            saver.SaveByteArray(GetBytes((long)value));
        }

        protected abstract byte[] GetBytes(long value);

        protected virtual int DataTypeCode { get { return 1; } }
    }
}