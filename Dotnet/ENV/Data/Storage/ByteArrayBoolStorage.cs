using System;
using ENV.Data.DataProvider;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class ByteArrayBoolStorage:IColumnStorageSrategy<Bool>,ENV.Utilities.IStorageScriptCreator
    {
        static byte[] True = new byte[] { (byte)'T' }, False = new byte[] { (byte)'F' };
        public Bool LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            Byte[] source = loader.GetByteArray() as byte[];
            if (source == null)
                return null;
            return (source.Length >= 1 && source[0] == True[0]);
        }

        public void SaveTo(Bool value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else
            {
                var btrieveSaver = saver as IBtrieveValueSaver;
                if (btrieveSaver != null)
                    btrieveSaver.SaveByteArray(value ? True : False, value);
                else
                    saver.SaveByteArray(value ? True : False);
            }
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddBinary(name, caption, 1);
        }
    }
}
