using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class StringBoolStorage : IColumnStorageSrategy<Bool>, ENV.Utilities.IStorageScriptCreator

    {
        public Bool LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            return loader.GetString() == "T";
        }

        public void SaveTo(Bool value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else
                saver.SaveAnsiString(value ? "T" : "F", 1,false);
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddBoolString(name, caption, allowNull);
        }
    }
}
