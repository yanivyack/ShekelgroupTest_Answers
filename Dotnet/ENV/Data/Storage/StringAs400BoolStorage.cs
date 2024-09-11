using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class StringAs400BoolStorage : IColumnStorageSrategy<Bool>, ENV.Utilities.IStorageScriptCreator
    {
        static string True = ((char) 232).ToString(), False = ((char) 227).ToString();

        public Bool LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            return loader.GetString() == True;
        }

        public void SaveTo(Bool value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else
                saver.SaveString(value ? True : False, 1,false);
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddBoolString(name, caption, allowNull);
        }
    }
}