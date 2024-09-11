using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class StringTimeStorage : IColumnStorageSrategy<Time>, ENV.Utilities.IStorageScriptCreator
    {
        public Time LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            string t = loader.GetString();
            if (t.Length > 6)
                t = t.TrimEnd();
          //  if (t.Length == 8 && t[2] == ':' && t[5] == ':')
          //      return Time.Parse(t, "HH:MM:SS");
            try
            {
                if (string.IsNullOrEmpty(t) || t != null && t.Length > 6)
                {
                    try
                    {
                        var dt = loader.GetDateTime();
                        return Time.FromDateTime(dt);
                    }
                    catch
                    {
                    }
                }

                return Time.Parse(t.Substring(t.Length - 6), "HHMMSS");

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
            else
                saver.SaveAnsiString(Time.ToString(value, "HHMMSS"), 6, true);
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddStringTime(name, caption, allowNull);
        }
    }
    public class ByteArrayTimeStorage : IColumnStorageSrategy<Time>, ENV.Utilities.IStorageScriptCreator
    {
        public Time LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            var t = System.Text.Encoding.Default.GetString(loader.GetByteArray());
            if (t.Length > 6)
                t = t.TrimEnd();

            try
            {
                if (string.IsNullOrEmpty(t) || t != null && t.Length > 6)
                {
                    try
                    {
                        var dt = loader.GetDateTime();
                        return Time.FromDateTime(dt);
                    }
                    catch
                    {
                    }
                }

                return Time.Parse(t.Substring(t.Length - 6), "HHMMSS");

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
            else
                saver.SaveByteArray(Encoding.Default.GetBytes(Time.ToString(value, "HHMMSS")));
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddStringTime(name, caption, allowNull);
        }
    }
    public class StringTimeHHMMStorage : IColumnStorageSrategy<Time>, ENV.Utilities.IStorageScriptCreator
    {
        public Time LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            string t = loader.GetString();

            try
            {
                if (string.IsNullOrEmpty(t) || t != null && t.Length > 6)
                {
                    try
                    {
                        var dt = loader.GetDateTime();
                        return Time.FromDateTime(dt);
                    }
                    catch
                    {
                    }
                }

                return Time.Parse(t.Substring(t.Length - 4), "HHMM");

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
            else
                saver.SaveAnsiString(Time.ToString(value, "HHMM"), 4, true);
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddStringTime(name, caption, allowNull);
        }
    }
    public class XmlStringTimeStorage : IColumnStorageSrategy<Time>
    {
        public Time LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            string t = loader.GetString().TrimEnd();

            try
            {
                if (t.EndsWith("Z"))
                    t= t.Remove(t.Length - 5);
                if (t.Length>=8)
                    return Time.Parse(t.Substring(t.Length - 8), "HH:MM:SS");

            }
            catch
            {
            }
            return Time.StartOfDay;
        }

        public void SaveTo(Time value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else
                saver.SaveAnsiString(Time.ToString(value, "HH:MM:SS"), 8, false);
        }

    }
}
