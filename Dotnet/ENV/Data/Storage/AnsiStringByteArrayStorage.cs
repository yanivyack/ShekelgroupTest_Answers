using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Utilities;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class AnsiStringByteArrayStorage : IColumnStorageSrategy<byte[]>, IStorageScriptCreator
    {
        public byte[] LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            var s = loader.GetString();
            return ByteArrayColumn.ToAnsiByteArray(s);
        }

        public void SaveTo(byte[] value, IValueSaver saver)
        {
            if (value == null)
            {
                saver.SaveNull();
                return;
            }
            var s = ByteArrayColumn.AnsiByteArrayToString(value);
            if (UserMethods.TrimTrailingNulls)
                s = UserMethods.Instance.RemoveZeroChar(s);
            saver.SaveAnsiString(s, s.Length, true);//did true for W10867
        }
        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddText(name, caption, "9999", allowNull, false, false, helper);
        }
    }
}
