using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class DateTimeTextStorage : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>, ENV.Utilities.IStorageScriptCreator
    {
        static Text ZeroReplacementText = "8080-80-80 00:00:00";
        static DateTime ZeroReplacement = DateTimeDateStorage.ZeroReplacement.ToDateTime();
        public static bool SaveEmptyAs111 = false;

        public Text LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            try
            {
                var x = loader.GetDateTime();
                if (x == ZeroReplacement)
                    return ZeroReplacementText;
                else
                    return x.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                return ZeroReplacementText;
            }

        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            var x = saver as CanForceDateTime;
            if (x != null)
                x.ForceDateTime2();
            if (object.ReferenceEquals(value, null))
                saver.SaveNull();
            else if (Text.IsNullOrEmpty(value) && DateTimeTextStorage.SaveEmptyAs111)
                saver.SaveDateTime(new DateTime(1, 1, 1));
            else
            {
                DateTime dt;
                if (!DateTime.TryParse(value, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out dt))
                {
                    if (value.Length >= 18)
                    {
                        var v = value.ToString().ToCharArray();
                        v[13] = ':';
                        v[16] = ':';
                        if (!DateTime.TryParse(new string(v), out dt))
                            dt = ZeroReplacement;
                    }
                    else if (value.Length > 10)
                    {
                        if (!DateTime.TryParse(value.Remove(10), out dt))
                            dt = ZeroReplacement;
                    }
                    else
                        dt = ZeroReplacement;
                }

                saver.SaveDateTime(dt);
            }

        }
        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddDateTime(name, caption, allowNull, helper);
        }
    }
    public class TimestampTextStorage : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>, ENV.Utilities.IStorageScriptCreator
    {
        static Text ZeroReplacementText = "8080-80-80-00:00:00.000000";
        static DateTime ZeroReplacement = DateTimeDateStorage.ZeroReplacement.ToDateTime();


        public Text LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            try
            {
                ((CanForceDateTime)loader).ForceDateTime2();
                var x = loader.GetDateTime();
                if (x == ZeroReplacement)
                    return ZeroReplacementText;
                else
                    return x.ToString("yyyy-MM-dd-HH.mm.ss.ffffff");
            }
            catch
            {
                return ZeroReplacementText;
            }

        }
        static void ForceDateTime2(IValueSaver saver)
        {
            var z = saver as CanForceDateTime;
            if (z != null)
            {
                z.ForceDateTime2();
            }
        }

        public void SaveTo(Text value, IValueSaver saver)
        {

            ForceDateTime2(saver);
            if (object.ReferenceEquals(value, null))
                saver.SaveNull();
            else if (Text.IsNullOrEmpty(value) && DateTimeTextStorage.SaveEmptyAs111)
                saver.SaveDateTime(new DateTime(1, 1, 1));
            else
            {
                try
                {
                    System.Func<int, int, int> get = (p, l) => int.Parse(value.Substring(p, l));
                    DateTime dt = new DateTime(get(0, 4), get(5, 2), get(8, 2), get(11, 2), get(14, 2), get(17, 2));
                    dt = new DateTime(dt.Ticks + get(20, 6) * 10);
                    saver.SaveDateTime(dt);
                }
                catch (Exception ex)
                {
                    throw new Exception("'" + value + "' Is not a valid Timestamp value. " + ex.Message, ex);
                }
            }

        }
        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddCustomColumn(name, caption, "datetime2", "", allowNull);
        }
    }
    interface CanForceDateTime
    {
        void ForceDateTime2();
    }
    public class SQLServerTimeStampTextStorage : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>, ENV.Utilities.IStorageScriptCreator
    {
        ByteArrayTextStorage _baseStorage = new ByteArrayTextStorage(8);
        System.Text.Encoding _e = ENV.LocalizationInfo.Current.OuterEncoding;
        public Text LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            return _e.GetString(loader.GetByteArray());

        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else
                saver.SaveByteArray(_e.GetBytes(value.TrimEnd()));

        }
        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddCustomColumn(name, caption, "timestamp", null, allowNull);
        }
    }

    public class DateTimeTextStorageWithMillisecond : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>, ENV.Utilities.IStorageScriptCreator
    {
        static Text ZeroReplacementText = "8080/80/80 00:00:00.000";
        static DateTime ZeroReplacement = DateTimeDateStorage.ZeroReplacement.ToDateTime();

        public Text LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            try
            {
                var x = loader.GetDateTime();
                if (x == ZeroReplacement)
                    return ZeroReplacementText;
                else
                    return x.ToString("yyyy/MM/dd HH:mm:ss.fff");
            }
            catch
            {
                return ZeroReplacementText;
            }

        }

        public void SaveTo(Text value, IValueSaver saver)
        {
            if (object.ReferenceEquals(value, null))
                saver.SaveNull();
            else
            {
                DateTime dt;
                var s = value.TrimEnd().ToString();
                var sp = s.Split(' ');
                if (value == "")
                    dt = new DateTime(1901, 1, 1);
                else if (sp[0].Trim().Length != 10)
                {
                    switch (sp[0].Trim().Length)
                    {
                        case 6:
                            dt = Date.Parse(sp[0], "YYMMDD").ToDateTime();
                            break;
                        case 8:
                            dt = Date.Parse(sp[0], "YYYYMMDD").ToDateTime();
                            break;
                        default:
                            throw new InvalidOperationException("Can't convert " + value + " to a valid datetime");
                    }
                    if (sp.Length > 1)
                        dt = dt.AddMilliseconds(ENV.UserMethods.Instance.MTVal(sp[1], "HH:MM:SS.mmm"));
                }
                else

                    if (!DateTime.TryParse(s, out dt))
                {
                    if (s.Length >= 18)
                    {
                        var v = s.ToCharArray();
                        v[13] = ':';
                        v[16] = ':';
                        if (!DateTime.TryParse(new string(v), out dt))
                            dt = ZeroReplacement;
                    }
                    else
                        dt = ZeroReplacement;
                }
                saver.SaveDateTime(dt);
            }

        }
        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddDateTime(name, caption, allowNull, helper);
        }
    }
}