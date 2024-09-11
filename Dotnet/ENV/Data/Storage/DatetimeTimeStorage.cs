using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class DateTimeTimeStorage : IColumnStorageSrategy<Time>, ENV.Utilities.IStorageScriptCreator
    {
        public Time LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            try
            {
                return Time.FromDateTime(loader.GetDateTime());
            }
            catch
            {
                return Time.StartOfDay;
            }
        }
        static DateTime _emptyDate = new DateTime(2000, 1, 1);
        public void SaveTo(Time value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else
                saver.SaveDateTime(value.AddToDateTime(_emptyDate));

        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddDateTime(name, caption,allowNull,helper);
        }
    }


    public class TimeSpanTimeStorage : IColumnStorageSrategy<Time>, ENV.Utilities.IStorageScriptCreator
    {
        public Time LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            try
            {
                var ts = loader.GetTimeSpan();
                return new Time((int)ts.TotalHours, ts.Minutes, ts.Seconds);
            }
            catch
            {
                return Time.StartOfDay;
            }
        }

        public void SaveTo(Time value, IValueSaver saver)
        {

            if (value == null)
                saver.SaveNull();
            else if (value.TotalSeconds < 0)
                saver.SaveTimeSpan(TimeSpan.Zero);
            else
                saver.SaveTimeSpan(new TimeSpan(0, value.Hour, value.Minute, value.Second));
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddTimeSpan(name,caption,allowNull,helper);
        }
    }
}
