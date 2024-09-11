using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;
using ENV.Utilities;
using ENV.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class StringDateStorage : IColumnStorageSrategy<Date>
    {
        public Date LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;

            try
            {
                var s = loader.GetString();
                if (string.IsNullOrWhiteSpace(s))
                    return Date.Empty;
                if (s.Length != 8 || s == "79080104")
                    return DateColumn.ErrorDate;
                return Date.Parse(s, "YYYYMMDD");
            }
            catch
            {
                return DateColumn.ErrorDate;
            }
        }

        public void SaveTo(Date value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else

                saver.SaveAnsiString(Date.ToString(value, "YYYYMMDD"), 8, true);
        }
    }
    public class StringOracleDateStorage : IColumnStorageSrategy<Date>
    {
        public Date LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;

            try
            {
                var s = loader.GetString();
                if (string.IsNullOrWhiteSpace(s))
                    return Date.Empty;

                return Date.Parse(s.Insert(7, "20"), "DD-MMM-YYYY");
            }
            catch
            {
                return DateColumn.ErrorDate;
            }
        }

        public void SaveTo(Date value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else

                saver.SaveAnsiString(Date.ToString(value, "DD-MMM-YY"), 9, true);
        }
    }
    public class ProblemStringDateStorage : IColumnStorageSrategy<Date>
    {
        string _problemValue;
        public ProblemStringDateStorage(string problemValue)
        {
            _problemValue = problemValue;
        }
        public Date LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;

            try
            {
                var s = loader.GetString();
                if (string.IsNullOrWhiteSpace(s))
                    return Date.Empty;
                if (s.Length != 8 || s == "79080104" || s == _problemValue)
                    return DateColumn.ErrorDate;
                return Date.Parse(s, "YYYYMMDD");
            }
            catch
            {
                return DateColumn.ErrorDate;
            }
        }

        public void SaveTo(Date value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else if (value == DateColumn.ErrorDate)
                saver.SaveAnsiString(_problemValue, 8, true);
            else saver.SaveAnsiString(Date.ToString(value, "YYYYMMDD"), 8, true);
        }
    }
    public class ByteArrayDateStorage : IColumnStorageSrategy<Date>
    {
        public Date LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;

            try
            {
                var b = loader.GetByteArray();
                if (b.Length != 8)
                    return DateColumn.ErrorDate;
                return FromBytes(b);
            }
            catch
            {
                return DateColumn.ErrorDate;
            }
        }

        public void SaveTo(Date value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else
            {
                var s = Date.ToString(value, "YYYYMMDD");
                var btrieveSaver = saver as IBtrieveValueSaver;
                if (btrieveSaver != null)
                    saver.SaveAnsiString(s, 8, true);
                else
                    saver.SaveByteArray(System.Text.Encoding.Default.GetBytes(s));
            }
        }
        public static Date FromBytes(byte[] bytes)
        {
            int num = 0;
            int years = 0;
            int months = 0;

            for (int i = 0; i < bytes.Length; i++)
            {



                var x = bytes[i];
                if (x >= 48 && x <= 57)
                {
                    num *= 10;
                    num += x - 48;
                    if (i == 3)
                    {
                        years = num;
                        num = 0;
                    }
                    else if (i == 5)
                    {
                        months = num;
                        num = 0;
                    }
                }

            }
            return new Date(years, months, num);

        }
    }
    public class XmlStringDateStorage : IColumnStorageSrategy<Date>
    {
        public Date LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;

            try
            {
                var s = loader.GetString();

                return Date.Parse(s, "YYYY-MM-DD");
            }
            catch
            {
                return DateColumn.ErrorDate;
            }
        }

        public void SaveTo(Date value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else if (value == Date.Empty)
                saver.SaveAnsiString("", 10, false);
            else
                saver.SaveAnsiString(Date.ToString(value, "YYYY-MM-DD"), 10, false);
        }
    }

    public class StringDateYYMMDDStorage : IColumnStorageSrategy<Date>
    {
        public Date LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;

            try
            {
                var s = loader.GetString();
                if (string.IsNullOrWhiteSpace(s))
                    return Date.Empty;
                if (s.Length != 6)
                    return DateColumn.ErrorDate;
                return Date.Parse(s, "YYMMDD");
            }
            catch
            {
                return DateColumn.ErrorDate;
            }
        }

        public void SaveTo(Date value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else

                saver.SaveAnsiString(Date.ToString(value, "YYMMDD"), 6, true);
        }
    }
    public class ByteArrayDateYYMMDDStorage : IColumnStorageSrategy<Date>, IStorageScriptCreator
    {
        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddBinary(name, caption, 6);
        }

        public Date LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;

            try
            {
                var s = ENV.LocalizationInfo.Current.OuterEncoding.GetString(loader.GetByteArray());
                if (s.Length != 6)
                    return DateColumn.ErrorDate;
                return Date.Parse(s, "YYMMDD");
            }
            catch
            {
                return DateColumn.ErrorDate;
            }
        }

        public void SaveTo(Date value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else

                saver.SaveByteArray(ENV.LocalizationInfo.Current.OuterEncoding.GetBytes(Date.ToString(value, "YYMMDD")));
        }
    }
}
