using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    class AnsiStringFixedLengthTextStorage : IColumnStorageSrategy<Text>, IStorageScriptCreator
    {
        Firefly.Box.Data.TextColumn _column;
        public AnsiStringFixedLengthTextStorage(Firefly.Box.Data.TextColumn column)
        {
            _column = column;
        }

        public Text LoadFrom(IValueLoader loader)
        {
            var s = loader.GetString();
            if (s == null)
                return null;
            return  s;
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            if (ReferenceEquals(value, null))
                saver.SaveNull();
            else
                saver.SaveAnsiString(value, _column.FormatInfo.MaxDataLength,true);
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddText(name, caption, _column.Format, allowNull, false, true, helper);
        }
    }
    class NullPaddedAnsiStringFixedLengthTextStorage : IColumnStorageSrategy<Text>, IStorageScriptCreator
    {
        public static bool ReturnEmptyStringOnNullWhenNotAllowed = false;
        Firefly.Box.Data.TextColumn _column;
        public NullPaddedAnsiStringFixedLengthTextStorage(Firefly.Box.Data.TextColumn column)
        {
            _column = column;
        }

        public Text LoadFrom(IValueLoader loader)
        {
            var s = loader.GetString();
            if (s == null)
                if (_column.AllowNull)
                    return null;
                else
                {
                    s = "";
                    if (ReturnEmptyStringOnNullWhenNotAllowed)
                        return s;
                }
            return s.PadRight(_column.FormatInfo.MaxDataLength, '\0');
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            if (ReferenceEquals(value, null))
                saver.SaveNull();
            else
                saver.SaveAnsiString(value, _column.FormatInfo.MaxDataLength, true);
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddText(name, caption, _column.Format, allowNull, false, true, helper);
        }
    }
} 