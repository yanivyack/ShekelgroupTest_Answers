using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;
using SQLClientEntityDataProvider = ENV.Data.DataProvider.SQLClientEntityDataProvider;

namespace ENV.Data.Storage
{
    class AnsiStringTextStorage : IColumnStorageSrategy<Text>, IStorageScriptCreator
    {
        Firefly.Box.Data.TextColumn _column;
        public AnsiStringTextStorage(Firefly.Box.Data.TextColumn column)
        {
            _column = column;
        }

        public Text LoadFrom(IValueLoader loader)
        {
            var s = loader.GetString();
            if (ReferenceEquals(s, null))
                return s;
            return s;
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            if (ReferenceEquals(value, null))
                saver.SaveNull();
            else
            {
                saver.SaveAnsiString(value, _column.FormatInfo.MaxDataLength, false);
            }
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddText(name, caption, _column.Format, allowNull, false, false, helper);
        }
    }
    class AnsiStringTextStorageThatRemovesNullChars : IColumnStorageSrategy<Text>, IStorageScriptCreator
    {
        Firefly.Box.Data.TextColumn _column;
        bool _fixedLength;
        public AnsiStringTextStorageThatRemovesNullChars(Firefly.Box.Data.TextColumn column,bool fixedLength)
        {
            _column = column;
            _fixedLength = fixedLength;
        }

        public Text LoadFrom(IValueLoader loader)
        {
            var s =  loader.GetString();
            if (ReferenceEquals(s, null))
                return s;
            var y = s.IndexOf('\0');
            if (y >= 0)
                s = s.Remove(y);
            return s;
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            if (ReferenceEquals(value, null))
                saver.SaveNull();
            else
            {
                var i = value.IndexOf('\0');
                if (i > 0)
                    value = value.Remove(i);
                saver.SaveAnsiString(value, _column.FormatInfo.MaxDataLength, false);
            }
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddText(name, caption, _column.Format, allowNull, false, _fixedLength, helper);
        }
    }

    public class AnsiStringTextStorageWithManualSize : IColumnStorageSrategy<Text>, IStorageScriptCreator
    {
        private int _size;
        bool _fixedLength;

        public AnsiStringTextStorageWithManualSize(int size, bool fixedLength)
        {
            _size = size;
            _fixedLength = fixedLength;
        }

        public Text LoadFrom(IValueLoader loader)
        {
            var s = loader.GetString();
            if (ReferenceEquals(s, null))
                return s;
            var y = s.IndexOf('\0');
            if (y >= 0)
                s = s.Remove(y);
            return s;
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            if (ReferenceEquals(value, null))
                saver.SaveNull();
            else
            {
                var i = value.IndexOf('\0');
                if (i > 0)
                    value = value.Remove(i);
                saver.SaveAnsiString(value, _size, false);
            }
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddText(name, caption, _size.ToString(), allowNull, false, _fixedLength, helper);
        }
    }

    public class UniqueIdentifierAutoNewIDTextStorage : IColumnStorageSrategy<Text>, IStorageScriptCreator
    {
       

        public Text LoadFrom(IValueLoader loader)
        {
            return loader.GetString();
        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            var x = saver as SQLClientEntityDataProvider.SQLServerEntityDataProviderFilterItemSaver;
            if (x != null)
                x.SaveNewGUIDOnInsert(value);
            else if (ReferenceEquals(value, null))
                saver.SaveNull();
            else
                saver.SaveAnsiString(value, value.Length, false);
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddCustomColumn(name, caption, "UNIQUEIDENTIFIER","newid()", allowNull);
        }
    }
}
        