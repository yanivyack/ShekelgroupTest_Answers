using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class NullTerminatorTextStorage : IColumnStorageSrategy<Text>
    {
        public Text LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            return loader.GetString().TrimEnd('\0');
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            if (ReferenceEquals(value, null))
                saver.SaveNull();
            else
                saver.SaveAnsiString(value + '\0', value.Length + 1,false);
        }
    }
}
