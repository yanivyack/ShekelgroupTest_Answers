using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Data.DataProvider;
using ENV.Data.Storage;
using ENV.Security.UI;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data
{
    public class UserDbMethods
    {
        internal class UnTypedUserDbMethods
        {

            public ICustomFilterMember Upper(object column)
            {
                return new myCustomFilterDelegateDecorator(collector => collector("Upper ({0})", column));
            }

            public ICustomFilterMember Lower(object column)
            {
                return new myCustomFilterDelegateDecorator(collector => collector("Lower ({0})", column));
            }
            public ICustomFilterMember Asc(object column)
            {
                return new myCustomFilterDelegateDecorator(collector => collector("Ascii ({0})", column));
            }
            public ICustomFilterMember Chr(object column)
            {
                return new myCustomFilterDelegateDecorator(collector => collector("CHAR ({0})", column));
            }
            public ICustomFilterMember ASCIIChr(object column)
            {
                return Chr(column);
            }
            public ICustomFilterMember Val(object column)
            {
                return new myCustomFilterDelegateDecorator(collector => collector("Cast ({0} as float)", column));
            }



            public ICustomFilterMember InStr(object text, object textToLookFor)
            {
                return _impl().InStr(text, textToLookFor);
            }


            public ICustomFilterMember Mid(object text, object position, object length)
            {
                return new myCustomFilterDelegateDecorator(a => a(_impl().GetSubstringSyntax() + " ({0}, {1}, {2})", text, position, length));
            }

            public ICustomFilterMember Str(object number, object format)
            {
                return _impl().Str(number, format);
            }

            public ICustomFilterMember DStr(object date, object format)
            {
                return _impl().DStr(date, format);
            }

            public ICustomFilterMember Like(object column, object text)
            {
                return new myCustomFilterDelegateDecorator(a => a("{0} Like {1} escape '\\'", column, FixLikeValue(text)));
            }
            private static object FixLikeValue(object o)
            {
                if (o is Func<string>)
                    return MatchValueForLike(((Func<string>)o)());
                if (o is Func<Text>)
                    return MatchValueForLike(((Func<Text>)o)());
                if (o is string)
                    return MatchValueForLike((string)o);
                if (o is Text)
                    return MatchValueForLike((Text)o);
                if (o is ICustomFilterMember)
                    o = new LikeCustomFilterWrapper((ICustomFilterMember)o);
                else if (o is ColumnBase)
                    o = new ColumnLikeFilterItem((ColumnBase)o);
                else if (o is IFilterItem)
                    o = new LikeFilterItem((IFilterItem)o);
                return o;
            }
            class LikeFilterItem : IFilterItem
            {
                IFilterItem _wrapped;

                public LikeFilterItem(IFilterItem wrapped)
                {
                    _wrapped = wrapped;
                }

                public void SaveTo(IFilterItemSaver saver)
                {
                    _wrapped.SaveTo(new LikeFilterItemSaver(saver));
                }
                public bool IsAColumn()
                {
                    return _wrapped.IsAColumn();
                }

            }
            class LikeCustomFilterWrapper : ICustomFilterMember
            {
                ICustomFilterMember _wrapped;

                public LikeCustomFilterWrapper(ICustomFilterMember wrapped)
                {
                    _wrapped = wrapped;
                }

                public void SendFilterTo(CustomFilterCollector sendFilterString)
                {
                    _wrapped.SendFilterTo((filter, args) =>
                    {
                        for (int i = 0; i < args.Length; i++)
                        {
                            var o = args[i];
                            args[i] = FixLikeValue(args[i]);
                        }
                        sendFilterString(filter, args);
                    });

                }





            }
            class ColumnLikeFilterItem : IFilterItem
            {
                ColumnBase _column;

                public ColumnLikeFilterItem(ColumnBase column)
                {
                    _column = column;
                }

                public void SaveTo(IFilterItemSaver saver)
                {
                    _column.SaveYourValueToDb(new LikeFilterItemSaver(saver));
                }
                public bool IsAColumn()
                {
                    return false;
                }
            }
            class LikeFilterItemSaver : IFilterItemSaver
            {
                IFilterItemSaver _wrapped;

                public LikeFilterItemSaver(IFilterItemSaver wrapped)
                {
                    _wrapped = wrapped;
                }

                public void SaveInt(int value)
                {
                    _wrapped.SaveInt(value);
                }

                public void SaveDecimal(decimal value, byte precision, byte scale)
                {
                    _wrapped.SaveDecimal(value, precision, scale);
                }

                public void SaveString(string value, int length, bool fixedWidth)
                {
                    _wrapped.SaveString(MatchValueForLike(value), length, fixedWidth);
                }

                public void SaveAnsiString(string value, int length, bool fixedWidth)
                {
                    _wrapped.SaveAnsiString(MatchValueForLike(value), length, fixedWidth);
                }

                public void SaveNull()
                {
                    _wrapped.SaveNull();
                }

                public void SaveDateTime(DateTime value)
                {
                    _wrapped.SaveDateTime(value);
                }

                public void SaveTimeSpan(TimeSpan value)
                {
                    _wrapped.SaveTimeSpan(value);
                }

                public void SaveBoolean(bool value)
                {
                    _wrapped.SaveBoolean(value);
                }

                public void SaveByteArray(byte[] value)
                {
                    _wrapped.SaveByteArray(value);
                }

                public void SaveColumn(ColumnBase column)
                {
                    _wrapped.SaveColumn(column);
                }

                public void SaveEmptyDateTime()
                {
                    _wrapped.SaveEmptyDateTime();
                }
            }

            static string MatchValueForLike(string value)
            {
                return value.Replace("*", "%").Replace("?", "_");
            }




            public ICustomFilterMember In(object column, params object[] args)
            {
                string s = "";
                var l = new ArrayList();
                l.Add(column);
                foreach (var o in args)
                {
                    if (s.Length > 0)
                        s += ", ";
                    s += "{" + l.Count + "}";
                    l.Add(o);
                }

                return new myCustomFilterDelegateDecorator(x => x("{0} in (" + s + ")", l.ToArray()));
            }

            public ICustomFilterMember Trim(object column)
            {
                return _impl().Trim(column);
            }

            public ICustomFilterMember RTrim(object column)
            {
                return new myCustomFilterDelegateDecorator(a => a("RTrim ({0})", column));
            }

            public ICustomFilterMember Len(object column)
            {
                return new myCustomFilterDelegateDecorator(a => a("Len ({0})", column));
            }

            public ICustomFilterMember LTrim(object column)
            {
                return new myCustomFilterDelegateDecorator(a => a("LTrim ({0})", column));
            }
            public ICustomFilterMember Not(object expression)
            {
                return new myCustomFilterDelegateDecorator(a => a("Not ({0})", expression));
            }




            public ICustomFilterMember ABS(object number)
            {
                return new myCustomFilterDelegateDecorator(a => a("ABS({0})", number));
            }

            public ICustomFilterMember Date()
            {
                return _impl().Date();

            }

            public ICustomFilterMember ToDateTime(int year, int month, int day)
            {
                return ToDateTime(new Date(year, month, day));
            }

            public ICustomFilterMember ToDateTime(Date date)
            {
                return new myCustomFilterDelegateDecorator(y =>
                {
                    y("{0}", new DateColumn { Storage = new DateDateStorage(), Value = date });
                });
            }
            public ICustomFilterMember Month(object date)
            {
                return _impl().Month(date);
            }

            public ICustomFilterMember Year(object date)
            {
                return _impl().Year(date);
            }

            public ICustomFilterMember Time()
            {
                return new myCustomFilterDelegateDecorator(a => a("CONVERT(INT,(SUBSTRING((CONVERT(CHAR,GETDATE(),8)),1,2) + SUBSTRING((CONVERT(CHAR,GETDATE(),8)),4,2) + SUBSTRING((CONVERT(CHAR,GETDATE(),8)),7,2)))"));

            }

            public ICustomFilterMember AddDate(object date, object years, object month, object days)
            {
                return _impl().AddDate(date, years, month, days);
            }


            public ICustomFilterMember IsNull(object column)
            {
                return new myCustomFilterDelegateDecorator(a => a("{0} is NULL", column));
            }
            public ICustomFilterMember CastNumberToVarchar(NumberColumn column, int length)
            {
                return new myCustomFilterDelegateDecorator(a => a("CAST({0} AS varchar(" + length + "))", column));
            }


            public ICustomFilterMember SQL(Text sql, params object[] args)
            {
                return new myCustomFilterDelegateDecorator(a => a(TranslateSql(sql), args));
            }








            Func<IUserDbMethodImplementation> _impl;
            public UnTypedUserDbMethods(Func<Firefly.Box.Data.Entity> getEntity)
            {
                _impl = () =>
                {
                    IUserDbMethodImplementation result;
                    var e = getEntity() as ENV.Data.Entity;
                    if (e == null)
                        result = new Default();
                    else
                    {
                        var dp = e.DataProvider as DynamicSQLSupportingDataProvider;
                        if (dp == null)
                            result = new Default();
                        else
                            result = dp.GetUserMethodsImplementation();

                    }
                    _impl = () => result;
                    return result;
                };
            }
            internal UnTypedUserDbMethods(Func<bool> isOracle)
            {
                if (isOracle())
                    _impl = () => new OracleDbMethods();
                else
                {
                    _impl = () => new Default();
                }
            }

            internal class myCustomFilterDelegateDecorator : ICustomFilterMember
            {
                Action<CustomFilterCollector> _to;

                public myCustomFilterDelegateDecorator(Action<CustomFilterCollector> to)
                {
                    _to = to;
                }

                public void SendFilterTo(CustomFilterCollector sendFilterString)
                {
                    _to(sendFilterString);
                }
            }


        }

        public ICustomFilterMember Upper(Firefly.Box.Data.TextColumn column)
        {
            return _db.Upper(column);
        }
        public ICustomFilterMember Upper(Firefly.Box.Data.ByteArrayColumn column)
        {
            return _db.Upper(column);
        }
        public ICustomFilterMember Upper(ICustomFilterMember column)
        {
            return _db.Upper(column);
        }
        public ICustomFilterMember Lower(Firefly.Box.Data.TextColumn column)
        {
            return _db.Lower(column);
        }
        public ICustomFilterMember Lower(ICustomFilterMember column)
        {
            return _db.Lower(column);
        }
        public ICustomFilterMember Asc(Firefly.Box.Data.TextColumn column)
        {
            return _db.Asc(column);
        }
        public ICustomFilterMember Chr(Number column)
        {
            return _db.Chr(column);
        }
        public ICustomFilterMember ASCIIChr(Number column)
        {
            return Chr(column);
        }
        public ICustomFilterMember Val(Firefly.Box.Data.TextColumn column)
        {
            return _db.Val(column);
        }
        public ICustomFilterMember Val(ICustomFilterMember column)
        {
            return _db.Val(column);
        }

        public ICustomFilterMember InStr(TextColumn text, TextColumn textToLookFor)
        {
            return _db.InStr(text, textToLookFor);
        }
        public ICustomFilterMember InStr(Text text, ICustomFilterMember textToLookFor)
        {
            return _db.InStr(text, textToLookFor);
        }



        public ICustomFilterMember InStr(TextColumn text, ICustomFilterMember textToLookFor)
        {
            return _db.InStr(text, textToLookFor);
        }
        public ICustomFilterMember InStr(ICustomFilterMember text, TextColumn textToLookFor)
        {
            return _db.InStr(text, textToLookFor);
        }
        public ICustomFilterMember InStr(Text text, TextColumn textToLookFor)
        {
            return _db.InStr(text, textToLookFor);
        }
        public ICustomFilterMember InStr(TextColumn text, Text textToLookFor)
        {
            return _db.InStr(text, textToLookFor);
        }
        public ICustomFilterMember InStr(ICustomFilterMember text, Text textToLookFor)
        {
            return _db.InStr(text, textToLookFor);
        }
        public ICustomFilterMember InStr(ICustomFilterMember text, ICustomFilterMember textToLookFor)
        {
            return _db.InStr(text, textToLookFor);
        }

        public ICustomFilterMember Mid(ICustomFilterMember text, Number position, ICustomFilterMember length)
        {
            return _db.Mid(text, position, length);
        }
        public ICustomFilterMember Mid(ICustomFilterMember text, Number position, Number length)
        {
            return _db.Mid(text, position, length);
        }
        public ICustomFilterMember Mid(TextColumn text, NumberColumn position, NumberColumn length)
        {
            return _db.Mid(text, position, length);
        }
        public ICustomFilterMember Mid(TextColumn text, Number position, Number length)
        {
            return _db.Mid(text, position, length);
        }
        public ICustomFilterMember Mid(TextColumn text, Number position, ICustomFilterMember length)
        {
            return _db.Mid(text, position, length);
        }
        public ICustomFilterMember Mid(TextColumn text, NumberColumn position, Number length)
        {
            return _db.Mid(text, position, length);
        }

        public ICustomFilterMember Str(NumberColumn number, Text format)
        {
            return _db.Str(number, format);
        }
        public ICustomFilterMember Str(ICustomFilterMember number, Text format)
        {
            return _db.Str(number, format);
        }
        public ICustomFilterMember DStr(ICustomFilterMember date, Text format)
        {
            return _db.DStr(date, format);
        }
        public ICustomFilterMember DStr(Date date, Text format)
        {
            return _db.DStr(date, format);
        }
        public ICustomFilterMember DStr(DateColumn date, Text format)
        {
            return _db.DStr(date, format);
        }
        public ICustomFilterMember Like(TextColumn column, Text text)
        {
            return _db.Like(column, text);
        }
        public ICustomFilterMember Like(ICustomFilterMember column, Text text)
        {
            return _db.Like(column, text);
        }
        public ICustomFilterMember Like(ICustomFilterMember column, TextColumn text)
        {
            return _db.Like(column, text);
        }
        public ICustomFilterMember Like(ByteArrayColumn column, Text text)
        {
            return _db.Like(column, text);
        }
        public ICustomFilterMember Like(ByteArrayColumn column, ByteArrayColumn text)
        {
            return _db.Like(column, text);
        }
        public ICustomFilterMember Like(ICustomFilterMember column, ICustomFilterMember text)
        {
            return _db.Like(column, text);
        }
        public ICustomFilterMember Like(TextColumn column, ICustomFilterMember text)
        {
            return _db.Like(column, text);
        }
        public ICustomFilterMember Like(TextColumn column, TextColumn text)
        {
            return _db.Like(column, text);
        }
        public ICustomFilterMember Like(ICustomFilterMember column, Func<Text> text)
        {
            return _db.Like(column, text);
        }
        public ICustomFilterMember Like(TextColumn column, Func<Text> text)
        {
            return _db.Like(column, text);
        }
        public ICustomFilterMember In(ColumnBase column, params object[] args)
        {
            return _db.In(column, args);
        }
        public FilterBase IsInSelect(ColumnBase col, ColumnBase colInOtherTable, FilterBase filter)
        {
            return new DynamicFilter(where =>
            {
                var stringFilter = ENV.Utilities.FilterHelper.ToSQLWhere
                       (filter, false, colInOtherTable.Entity);
                where.Add("{0} in " +
                       "(select " + colInOtherTable.Name +

                       " from " + Entity.GetEntityName(colInOtherTable.Entity) +
                       " where " + stringFilter + ")", col);
            });
        }


        public FilterBase IsNotInSelect(ColumnBase col, ColumnBase colInOtherTable, FilterBase filter)
        {
            return new DynamicFilter(where =>
            {
                var stringFilter = ENV.Utilities.FilterHelper.ToSQLWhere
                   (filter, false, colInOtherTable.Entity);
                where.Add("{0} not in " +
                       "(select " + colInOtherTable.Name +

                       " from " + Entity.GetEntityName(colInOtherTable.Entity) +
                       " where " + stringFilter + ")", col);
            });
        }

        public void InsertAsSelect(ENV.Data.Entity sourceEntity, ENV.Data.Entity targetEntity, FilterBase filter = null)
        {
            var dataProvider = (DynamicSQLSupportingDataProvider)sourceEntity.DataProvider;
            if (!targetEntity.Exists())
                dataProvider.CreateTable(targetEntity);

            Func<Entity, string> getColumns = entity => string.Join(",", entity.Columns.Where(c => !c.DbReadOnly).Select(c => c.Name).ToArray());
            string sql = "Insert Into " + Entity.GetEntityName(targetEntity) + " (" + getColumns(targetEntity) + ") Select " + getColumns(sourceEntity) + " From " + ENV.Data.Entity.GetEntityName(sourceEntity);
            if (filter != null)
                sql += " Where " + Utilities.FilterHelper.ToSQLWhere(filter, false);
            dataProvider.Execute(sql);
        }

        public void CreateTable(ENV.Data.Entity entity)
        {
            ((DynamicSQLSupportingDataProvider)entity.DataProvider).CreateTable(entity);
        }

        public ICustomFilterMember In(ICustomFilterMember column, params object[] args)
        {
            return _db.In(column, args);

        }

        ICustomFilterMember Trim(object column)
        {
            return _db.Trim(column);
        }

        public ICustomFilterMember Trim(TextColumn column)
        {
            return _db.Trim(column);
        }
        public ICustomFilterMember Trim(ICustomFilterMember column)
        {
            return _db.Trim(column);
        }

        public ICustomFilterMember Trim(Text column)
        {
            return _db.Trim(column);
        }

        public ICustomFilterMember RTrim(TextColumn column)
        {
            return _db.RTrim(column);
        }
        public ICustomFilterMember RTrim(ICustomFilterMember column)
        {
            return _db.RTrim(column);
        }
        public ICustomFilterMember Len(TextColumn column)
        {
            return _db.Len(column);
        }
        public ICustomFilterMember Len(ICustomFilterMember column)
        {
            return _db.Len(column);
        }
        public ICustomFilterMember LTrim(TextColumn column)
        {
            return _db.LTrim(column);
        }
        public ICustomFilterMember Not(ICustomFilterMember expression)
        {
            return _db.Not(expression);
        }
        public ICustomFilterMember ContentAsIs(Firefly.Box.Data.TextColumn column)
        {
            return new myCustomFilterDelegateDecorator(a => a(column.Value.TrimEnd()));
        }
        public ICustomFilterMember ContentAsIs(Firefly.Box.Data.NumberColumn column)
        {
            return new myCustomFilterDelegateDecorator(a => a(column.ToString().TrimEnd()));
        }


        public ICustomFilterMember ABS(NumberColumn number)
        {
            return _db.ABS(number);
        }
        public ICustomFilterMember ABS(ICustomFilterMember number)
        {
            return _db.ABS(number);
        }

        public ICustomFilterMember Date()
        {
            return _db.Date();

        }

        public ICustomFilterMember ToDateTime(int year, int month, int day)
        {
            return _db.ToDateTime(year, month, day);
        }

        public ICustomFilterMember ToDateTime(Date date)
        {
            return _db.ToDateTime(date);
        }
        public ICustomFilterMember Month(Date date)
        {
            return _db.Month(date);
        }
        public ICustomFilterMember Month(ICustomFilterMember date)
        {
            return _db.Month(date);
        }
        public ICustomFilterMember Year(Date date)
        {
            return _db.Year(date);
        }
        public ICustomFilterMember Year(ICustomFilterMember date)
        {
            return _db.Year(date);
        }
        public ICustomFilterMember Year(DateColumn date)
        {
            return _db.Year(date);
        }
        public ICustomFilterMember Month(DateColumn date)
        {
            return _db.Month(date);
        }
        public ICustomFilterMember Time()
        {
            return _db.Time();

        }

        public ICustomFilterMember AddDate(ICustomFilterMember date, int years, int month, int days)
        {
            return _db.AddDate(date, years, month, days);
        }
        public ICustomFilterMember AddDate(DateColumn date, int years, int month, int days)
        {
            return _db.AddDate(date, years, month, days);
        }
        public ICustomFilterMember AddDate(DateColumn date, int years, ICustomFilterMember month, int days)
        {
            return _db.AddDate(date, years, month, days);
        }
        public ICustomFilterMember AddDate(ICustomFilterMember date, int years, ICustomFilterMember month, int days)
        {
            return _db.AddDate(date, years, month, days);
        }
        public ICustomFilterMember AddDate(DateColumn date, int years, int month, ICustomFilterMember days)
        {
            return _db.AddDate(date, years, month, days);
        }

        public ICustomFilterMember IsNull(ColumnBase column)
        {
            return _db.IsNull(column);
        }
        public ICustomFilterMember IsNull(ICustomFilterMember column)
        {
            return _db.IsNull(column);
        }
        public ICustomFilterMember CastNumberToVarchar(NumberColumn column, int length)
        {
            return _db.CastNumberToVarchar(column, length);
        }
        internal static string InternalAdjustOem(Text t, bool ragil)
        {

            var ansi = System.Text.Encoding.GetEncoding(1255);
            var oem = ENV.LocalizationInfo.Current.InnerEncoding;
            if (t == null)
                return t;
            byte[] bytes = oem.GetBytes(t.ToString().ToCharArray());
            if (ragil)
            {

                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = HebrewOemTextStorage.DecodeByte(bytes[i]);
                }
            }
            return new string(ansi.GetChars(bytes));

        }

        public ICustomFilterMember AdjustOem(Firefly.Box.Data.TextColumn column, bool ragil)
        {
            return new myCustomFilterDelegateDecorator(a => a("{0}", InternalAdjustOem(column.Value, ragil)));
        }
        public ICustomFilterMember AdjustOemContentAsIs(Firefly.Box.Data.TextColumn column, bool ragil)
        {
            return new myCustomFilterDelegateDecorator(a => a(InternalAdjustOem(column.Value.TrimEnd(), ragil)));
        }

        public ICustomFilterMember SQL(Text sql, params object[] args)
        {
            return _db.SQL(sql, args);
        }

        public static Func<string, string> TranslateSql = sql => sql;

        public interface IUserDbMethodImplementation
        {
            string GetSubstringSyntax();
            ICustomFilterMember Date();
            ICustomFilterMember Month(object date);
            ICustomFilterMember Year(object date);
            ICustomFilterMember Str(object number, object format);
            ICustomFilterMember DStr(object date, object format);
            ICustomFilterMember InStr(object text, object textToLookFor);
            ICustomFilterMember Trim(object column);
            ICustomFilterMember AddDate(object date, object years, object month, object days);
        }
        public class MySql : Default
        {
            public override ICustomFilterMember Date()
            {
                return new myCustomFilterDelegateDecorator(a => a("{{fn CURDATE()}}"));
            }
        }
        public class Default : IUserDbMethodImplementation
        {
            public virtual string GetSubstringSyntax()
            {
                return "Substring";
            }
            public virtual ICustomFilterMember AddDate(object date, object years, object month, object days)
            {
                return new myCustomFilterDelegateDecorator(a =>
                a("DATEADD (year, {0}, DATEADD (month, {1}, DATEADD (day, {2}, {3})))", years, month, days, date));
            }

            public virtual ICustomFilterMember Date()
            {
                return new myCustomFilterDelegateDecorator(a => a("CAST (CONVERT (CHAR, GETDATE(), 112) AS DATETIME)"));
            }

            public virtual ICustomFilterMember Str(object number, object format)
            {
                return new myCustomFilterDelegateDecorator(a => a("RTRIM(CONVERT(CHAR,{0}, 0))", number));
            }


            public virtual ICustomFilterMember DStr(object date, object format)
            {
                Text t;
                if (Text.TryCast(format, out t) && t == "YYYYMMDD")
                    return new myCustomFilterDelegateDecorator(a => a("CONVERT(CHAR,{0}, 112)", date));
                else
                    return new myCustomFilterDelegateDecorator(a => a("RTRIM(CONVERT(CHAR,{0}, 0))", date));
            }

            public virtual ICustomFilterMember InStr(object text, object textToLookFor)
            {
                return new myCustomFilterDelegateDecorator(a => a("CHARINDEX ({0}, {1})", textToLookFor, text));
            }

            public virtual ICustomFilterMember Trim(object column)
            {
                return new myCustomFilterDelegateDecorator(a => a("LTrim (RTrim ({0}))", column));
            }

            public virtual ICustomFilterMember Month(object date)
            {
                if (date is ColumnBase || date is ICustomFilterMember)
                    return new myCustomFilterDelegateDecorator(a => a("DATEPART(mm,{0})", date));
                else
                    return new myCustomFilterDelegateDecorator(a => a("DATEPART(mm,{0})", Firefly.Box.Date.Cast(date).ToDateTime()));
            }

            public virtual ICustomFilterMember Year(object date)
            {
                if (date is ColumnBase || date is ICustomFilterMember)
                    return new myCustomFilterDelegateDecorator(a => a("DATEPART(yyyy,{0})", date));
                else
                    return new myCustomFilterDelegateDecorator(a => a("DATEPART(yyyy,{0})", Firefly.Box.Date.Cast(date).ToDateTime()));

            }

        }
        public class OracleDbMethods : IUserDbMethodImplementation
        {
            public string GetSubstringSyntax()
            {
                return "SUBSTR";
            }
            public ICustomFilterMember AddDate(object date, object years, object month, object days)
            {
                return new myCustomFilterDelegateDecorator(a =>
                a("DATEADD (year, {0}, DATEADD (month, {1}, DATEADD (day, {2}, {3})))", years, month, days, date));
            }

            public ICustomFilterMember Date()
            {
                if (!UserSettings.VersionXpaCompatible)
                    return new myCustomFilterDelegateDecorator(a => a("to_date(sysdate,'YYYY-MM-DD')"));
                return new myCustomFilterDelegateDecorator(a => a("sysdate"));
            }

            public ICustomFilterMember Str(object number, object format)
            {
                return new myCustomFilterDelegateDecorator(a => a("TO_CHAR({0})", number));
            }


            public ICustomFilterMember DStr(object date, object format)
            {
                return new myCustomFilterDelegateDecorator(a => a("TO_CHAR({0}, {1})", date, format));
            }

            public ICustomFilterMember InStr(object text, object textToLookFor)
            {
                return new myCustomFilterDelegateDecorator(a => a("INSTR ({0}, {1})", text, textToLookFor));
            }

            public ICustomFilterMember Trim(object column)
            {
                return new myCustomFilterDelegateDecorator(a => a("Trim ({0})", column));
            }

            public ICustomFilterMember Month(object date)
            {
                if (date is ColumnBase || date is ICustomFilterMember)
                    return new myCustomFilterDelegateDecorator(a => a("TO_NUMBER(TO_CHAR ({0}, 'MM'))", date));
                else
                    return new myCustomFilterDelegateDecorator(a => a("TO_NUMBER(TO_CHAR ({0}, 'MM'))", new DateColumn { Storage = new DateDateStorage(), Value = Firefly.Box.Date.Cast(date).ToDateTime() }));
            }


            public ICustomFilterMember Year(object date)
            {
                if (date is ColumnBase || date is ICustomFilterMember)
                    return new myCustomFilterDelegateDecorator(a => a("TO_NUMBER(TO_CHAR ({0}, 'YYYY'))", date));
                else
                    return new myCustomFilterDelegateDecorator(a => a("TO_NUMBER(TO_CHAR ({0}, 'YYYY'))", new DateColumn { Storage = new DateDateStorage(), Value = Firefly.Box.Date.Cast(date).ToDateTime() }));
            }

        }

        internal UnTypedUserDbMethods _db;
        public UserDbMethods(Func<Firefly.Box.Data.Entity> getEntity)
        {
            _db = new UnTypedUserDbMethods(getEntity);
        }
        internal UserDbMethods(Func<bool> isOracle)
        {
            _db = new UnTypedUserDbMethods(isOracle);
        }

        internal class myCustomFilterDelegateDecorator : ICustomFilterMember
        {
            Action<CustomFilterCollector> _to;

            public myCustomFilterDelegateDecorator(Action<CustomFilterCollector> to)
            {
                _to = to;
            }

            public void SendFilterTo(CustomFilterCollector sendFilterString)
            {
                _to(sendFilterString);
            }
        }


    }
}