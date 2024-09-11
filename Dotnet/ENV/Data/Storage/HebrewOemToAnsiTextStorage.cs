using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class HebrewOemToAnsiTextStorage : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>
    {
        Firefly.Box.Data.TextColumn _column;

        public HebrewOemToAnsiTextStorage(Firefly.Box.Data.TextColumn column)
        {
            _column = column;
        }
        public static bool DoNotDoAnsiToOEMOnLoad = false;

        public Text LoadFrom(IValueLoader loader)
        {
            if (DoNotDoAnsiToOEMOnLoad)
                return UserDbMethods.InternalAdjustOem(loader.GetString(), false);
            else
                return loader.GetString();
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            saver.SaveAnsiString(UserDbMethods.InternalAdjustOem(value, true), _column.FormatInfo.MaxDataLength,false);
        }
    }
}
