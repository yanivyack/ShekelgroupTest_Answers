using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class DateTimeDateStorage : IColumnStorageSrategy<Date>, ENV.Utilities.IStorageScriptCreator
    {
        public static Date ZeroReplacement = new Date(1, 1, 16);
        public Date LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            try
            {
                var x = Date.FromDateTime(loader.GetDateTime());
                if (x == ZeroReplacement)
                    return Date.Empty;
                return x;
            }
            catch
            {
                return Date.Empty;
            }

        }

        public void SaveTo(Date value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else
                if (value <= Date.Empty)
                saver.SaveDateTime(ZeroReplacement.ToDateTime());
            else
                saver.SaveDateTime(value.ToDateTime());
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddDateTime(name, caption, allowNull, helper);
        }
    }
    public class DateTimeDateStorageThatSupportsEmptyDate : IColumnStorageSrategy<Date>, ENV.Utilities.IStorageScriptCreator
    {

        public Date LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            try
            {
                return Date.FromDateTime(loader.GetDateTime());
            }
            catch
            {
                return Date.Empty;
            }

        }

        public void SaveTo(Date value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else
                if (value <= Date.Empty)
            {

                saver.SaveEmptyDateTime();

            }
            else
                saver.SaveDateTime(value.ToDateTime());
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddDateTime(name, caption, allowNull, helper);
        }
    }

    public interface IDateSupportingValueSaver : IValueSaver
    {
        void SaveDate(DateTime value);
    }

    public class DateDateStorage : IColumnStorageSrategy<Date>, ENV.Utilities.IStorageScriptCreator
    {
        DateTimeDateStorage _datetimeStorage = new DateTimeDateStorage();
        public Date LoadFrom(IValueLoader loader)
        {
            return _datetimeStorage.LoadFrom(loader);
        }



        public void SaveTo(Date value, IValueSaver saver)
        {
            _datetimeStorage.SaveTo(value, saver);
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddDate(name, caption, allowNull, helper);
        }
    }

    public class DateTimeDateStorageThatTreatsNullAsSpecificValue : IColumnStorageSrategy<Date>, ENV.Utilities.IStorageScriptCreator
    {
        Date _specificValue;

        public DateTimeDateStorageThatTreatsNullAsSpecificValue(Date specificValue)
        {
            _specificValue = specificValue;
        }

        public Date LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return _specificValue;
            try
            {
                return Date.FromDateTime(loader.GetDateTime());
            }
            catch
            {
                return ENV.Data.DateColumn.ErrorDate;
            }

        }

        public void SaveTo(Date value, IValueSaver saver)
        {
            if (value == null || value == _specificValue)
                saver.SaveNull();
            else if (value == Date.Empty)
                saver.SaveDateTime(_specificValue.ToDateTime());
            else
                saver.SaveDateTime(value.ToDateTime());
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddDateTime(name, caption, allowNull, helper);
        }
    }

}
