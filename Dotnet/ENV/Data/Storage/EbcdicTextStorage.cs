using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.Storage
{
    public class EbcdicTextStorage : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>
    {
        Encoding _e = Encoding.GetEncoding(500),_ansi = LocalizationInfo.Current.OuterEncoding;
        ENV.Data.DataProvider.FrenchCanadianDatabaseStringFixer _fixer = new DataProvider.FrenchCanadianDatabaseStringFixer();

        public Text LoadFrom(Firefly.Box.Data.DataProvider.IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            return _ansi.GetString( _e.GetBytes(_fixer.toDb(loader.GetString())));
        }

        public void SaveTo(Text value, Firefly.Box.Data.DataProvider.IValueSaver saver)
        {
            if (object.ReferenceEquals(value, null))
                saver.SaveNull();
            else
                saver.SaveString(_fixer.fromDb( _e.GetString(_ansi.GetBytes(value))),value.Length,false);
        }
    }

    internal static class EbcdicTextComparer
    {
        static System.Text.Encoding ebsidic = Encoding.GetEncoding(500);
        static byte[] _ebsidicVals = new byte[char.MaxValue];
        static byte GetVal(Char c)
        {

            var r = _ebsidicVals[c];
            if (r == default(byte))
            {
                _ebsidicVals[c] = r = ebsidic.GetBytes(new[] { c })[0];
            }
            return r;
        }
        public static int Compare(Text a, Text b)
        {
            a = a.TrimEnd(' ').ToString();
            b = b.TrimEnd(' ').ToString();
            int maxLength = a.Length;
            if (b.Length > maxLength)
                maxLength = b.Length;
            for (int i = 0; i < maxLength; i++)
            {
                if (i >= a.Length)
                    return -1;
                if (i >= b.Length)
                    return 1;
                if (a[i] == b[i])
                    continue;
                return GetVal(a[i]).CompareTo(GetVal(b[i]));

            }
            return 0;
        }
    }
    class EbcdicByteArrayTextStorage : Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text>
    {
        System.Text.Encoding _e = System.Text.Encoding.GetEncoding(500);
        public Text LoadFrom(Firefly.Box.Data.DataProvider.IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            return _e.GetString(loader.GetByteArray());
        }

        public void SaveTo(Text value, Firefly.Box.Data.DataProvider.IValueSaver saver)
        {
            if (object.ReferenceEquals(value, null))
                saver.SaveNull();
            else
                saver.SaveByteArray(_e.GetBytes(value));
        }

    }
    public class EbcdicStringBoolStorage : IColumnStorageSrategy<Bool>, ENV.Utilities.IStorageScriptCreator

    {
        public Bool LoadFrom(IValueLoader loader)
        {
            if (loader.IsNull())
                return null;
            return loader.GetString() == "è";
        }

        public void SaveTo(Bool value, IValueSaver saver)
        {
            if (value == null)
                saver.SaveNull();
            else
                saver.SaveAnsiString(value ? "è" : "ã", 1, false);
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddBoolString(name, caption, allowNull);
        }
    }
}
