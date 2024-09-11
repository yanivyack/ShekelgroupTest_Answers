using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using ENV.Data.DataProvider;
using ENV.Data.Storage;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using ENV.BackwardCompatible;
using Firefly.Box.Testing;


namespace ENV.Data
{
    public class DynamicSQLEntity : Firefly.Box.Data.Entity
    {
        static DynamicSQLEntity()
        {
            TrimExpressionValuesDefault = true;
        }

        IEntityDataProvider _originalProvider;


        internal interface ISQLBuilder
        {
            void AddValueParameter(string s,bool isNull, Func<IDbDataParameter> createParamAndSetItsValue);
            void BeginPrepare();
            string GetResult();
            void Restore();
            string ToOriginalString();
            void ExceptionHappend(Exception exx);
        }
        public DataSet InternalDataSet;
        internal class SQLBuilder : ISQLBuilder
        {


            public SQLBuilder(string sql)
            {



            _sql = sql;

            var sb = new StringBuilder();
            bool inColon = false;
            int lastPos = 0;
            int pos = 0;
            foreach (var c in _sql)
            {
                if (inColon)
                {
                    if ("1234567890".IndexOf(c) >= 0)
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        _tokens.Add(new Token(sb.ToString(), lastPos, this));
                        inColon = false;
                    }

                }
                if (c == ':' || c == '~')
                {
                    sb = new StringBuilder();
                    sb.Append(c);
                    inColon = true;
                    lastPos = pos;
                }
                pos++;
            }
            if (inColon)
            {
                _tokens.Add(new Token(sb.ToString(), lastPos, this));
            }

        }

            string _sql;
            Dictionary<int, int> _deltas = new Dictionary<int, int>();
            List<Token> _tokens = new List<Token>();
        int _paramNumber;
            class Token
            {
                public readonly string _token;
                public readonly int _position;
                SQLBuilder _parent;
                public Token(string token, int position, SQLBuilder parent)
                {
                    _token = token;
                    _position = position;
                    _parent = parent;
                }

                public int Position
        {
                    get
            {
                        int position = _position;
                        foreach (var d in _parent._deltas)
                        {
                            if (d.Key < _position)
                                position += d.Value;
                        }
                        return position;
                    }
                }

                public override string ToString()
                {
                    return _parent._sql.Substring(Position, Math.Min(10, _parent._sql.Length - Position));
                }
            }
            public void AddValueParameter(string s,bool isNull, Func<IDbDataParameter> createParamAndSetItsValue)
            {
            _paramNumber++;
            //set Value Parameter
            {
                string lookFor = ":" + _paramNumber;
                foreach (var t in _tokens)
                {
                    if (t._token == lookFor)
                    {
                        int lastIndex = t.Position;
                        int delta = 0;

                        _sql.Substring(lastIndex, lookFor.Length).ShouldBe(lookFor, "SQL Token parsing error, token was not where it should be");
                        _sql = _sql.Remove(lastIndex, lookFor.Length);
                        delta -= lookFor.Length;

                        _sql = _sql.Insert(lastIndex, s);
                        delta += s.Length;
                            if (isNull)
                        {
                            bool stopLoop = false;
                            for (int i = lastIndex - 1; i >= 0; i--)
                            {
                                switch (_sql[i])
                                {
                                    case '\n':
                                    case '\r':
                                    case ' ':
                                        break;
                                    case '=':
                                        _sql = _sql.Remove(i, 1);
                                        delta -= 1;
                                        _sql = _sql.Insert(i, " is ");
                                        delta += 4;
                                        break;
                                    case '>':
                                        if (i > 0 && _sql[i - 1] == '<')
                                        {
                                            _sql = _sql.Remove(i - 1, 2);
                                            delta -= 2;
                                            _sql = _sql.Insert(i - 1, " is not ");
                                            delta += 8;
                                        }
                                        else
                                            stopLoop = true;
                                        break;
                                    default:
                                        stopLoop = true;
                                        break;
                                }
                                if (stopLoop)
                                    break;
                            }
                        }
                        _deltas.Add(t._position, delta);


                    }
                }

            }
            //set Real Parameter
            {
                string lookFor = "~" + _paramNumber;
                string paramName = ":p" + _paramNumber;
                bool _savedParam = false; foreach (var t in _tokens)
                {
                    if (t._token == lookFor)
                    {
                        int lastIndex = t.Position;
                        int delta = 0;

                        _sql.Substring(lastIndex, lookFor.Length).ShouldBe(lookFor, "SQL Token parsing error, token was not where it should be");
                        _sql = _sql.Remove(lastIndex, lookFor.Length);
                        delta -= lookFor.Length;
                        _sql = _sql.Insert(lastIndex, paramName);
                        delta += paramName.Length;
                        if (!_savedParam)
                        {
                            _savedParam = true;
                            var p = createParamAndSetItsValue();
                            p.ParameterName = paramName;
                        }
                        _deltas.Add(t._position, delta);


                    }
                }

                }
            }
            string prevSql;
            public void BeginPrepare()
            {
                prevSql = _sql;
                _deltas = new Dictionary<int, int>();
                _paramNumber = 0;
            }
            public string GetResult()
            {
                return _sql;
            }
            public void Restore()
            {
                _sql = prevSql;
            }
            public string ToOriginalString()
            {
                return _sql;
            }

            public void ExceptionHappend(Exception exx)
            {
                
            }
        }
        public IEntityDataProvider DataProvider
        {
            get { return _originalProvider; }
        }
        ISQLBuilder _sqlBuilder
        {
            get
            {
                if (__theBuilder == null)
                    __theBuilder = _sqlBuilderFactory();
                return __theBuilder;
            }
        }
    	Func<ISQLBuilder> _sqlBuilderFactory;
        ISQLBuilder __theBuilder;
		 public DynamicSQLEntity(DynamicSQLSupportingDataProvider dataProvider, Func<string> sql)
           : this(dataProvider, "dynamicSql")
        {
            _sqlBuilderFactory = () => DynamicSQLSupportingDataProvider.ExecutionStrategy.GetSQLBuilder(sql());
        }
        public DynamicSQLEntity(DynamicSQLSupportingDataProvider dataProvider, string sql)
            : base(sql, sql, dataProvider.ProvideDynamicSqlEntityDataProvider())
        {
            _originalProvider = dataProvider;
            _sqlBuilderFactory = () => DynamicSQLSupportingDataProvider.ExecutionStrategy.GetSQLBuilder(sql);
            TrimTextColumnParameterValues = DefaultTrimTextColumnParameterValues;
            TrimExpressionParameterValues = TrimExpressionValuesDefault;
            RemoveTextAfterSemicolon = RemoveTextAfterSemicolonDefault;
        }
        public static bool OemTextParameters { get; set; }
        List<Action<Func<IDbDataParameter>>> _prepareSql = new List<Action<Func<IDbDataParameter>>>();
        void InternalAddValueParameter(string value, Func<IDbDataParameter> createParamAndSetItsValue, bool showNull)
        {
            if (!string.IsNullOrEmpty(value)&&!showNull)
            {
                var pi = value.IndexOf('\0');
                if (pi >= 0)
                    value = value.Remove(pi);
            }
            var s = value ?? (_nullStrategy.UseBlankTextInsteadOfNullInDynamicSQL() && !showNull ? "" : " NULL ");
            if (OemTextParameters)
                s = ENV.UserMethods.Instance.OEM2ANSI(s);
            _sqlBuilder.AddValueParameter(s,value==null, createParamAndSetItsValue);
        }
        INullStrategy _nullStrategy =  NullStrategy.GetStrategy(false);
        internal void SetNullStrategy(INullStrategy nullStrategy)
        {
            _nullStrategy = nullStrategy;
        }






        /// <summary>
        /// Adds a column that will be used as a parameter to the Sql statement .
        /// 
        /// </summary>
        /// <remarks>
        /// This column will be added according to it's own storage settings.<br/>
        /// Note that when using the <this/> you should be aware of possible sql injections. Monitor your paramaters closely.
        /// </remarks>
        /// <param name="column">The column to use as the parameter</param>
        public void AddParameter(ColumnBase column)
        {
       
                _prepareSql.Add(
                    createDbParameter =>
                    {
                        var tc = column as ENV.Data.TextColumn;
                        if (tc != null)
                        {
                            var at = Firefly.Box.Context.Current.ActiveTasks;
                            if (tc.AllowNull && at.Count > 0 && at[at.Count - 1].Columns.Contains(tc) && !tc.RecievedParameterValueOtherThenNull)
                            {
                                InternalAddValueParameter(" NULL  ", () =>
                                                                        {
                                                                            var p = createDbParameter();
                                                                            p.Value = DBNull.Value;
                                                                            return p;
                                                                        }, true);
                                return;
                            }
                            if (tc.Storage is DateTimeTextStorage)
                            {
                                new AnsiStringTextStorageThatRemovesNullChars(tc, true).SaveTo(tc.Value, new mySaver(this, createDbParameter, true,TrimTextColumnParameterValues));
                                return;
                            }
                        }
                        var nc = column as ENV.Data.NumberColumn;
                        if (nc != null)
                        {
                            var x = nc.Storage;
                            try { nc.Storage = null;
                                nc.SaveYourValueToDb(new mySaver(this, createDbParameter, column as Firefly.Box.Data.NumberColumn, true, TrimTextColumnParameterValues));
                                return;
                            }
                            finally {
                                nc.Storage = x;
                            }
                        }
                        var dc = column as Firefly.Box.Data.DateColumn;
                        if (dc != null && dc.Value== Date.Empty)
                            new StringDateStorage().SaveTo(dc.Value, new mySaver(this, createDbParameter, true, TrimTextColumnParameterValues));
                        else
                            column.SaveYourValueToDb(new mySaver(this, createDbParameter, column as Firefly.Box.Data.NumberColumn, true,TrimTextColumnParameterValues));
                    });
        }


        public void AddParameter(Func<object> expression)
        {
            _prepareSql.Add(createParameter =>
                                {
                                    var o = expression();
                                    var s = new mySaver(this, createParameter, false,TrimExpressionParameterValues);
                                    if (o == null)
                                    {
                                        s.SaveNull();
                                        return;
                                    }
                                    {
                                        Text t;
                                        if (Text.TryCast(o, out t))
                                        {
                                            if (t == null)
                                            {
                                                s.SaveNull();
                                                return;
                                            }
                                            if (TrimExpressionParameterValues)
                                                t = t.TrimEnd();
                                            s.SaveString(t, t.Length,false);
                                            return;
                                        }
                                    }
                                    {
                                        Time t;
                                        if (Time.TryCast(o, out t))
                                            o = ENV.UserMethods.Instance.ToNumber(t);
                                    }
                                    {
                                        Date d;
                                        if (Date.TryCast(o, out d))
                                            o = ENV.UserMethods.Instance.ToNumber(d);
                                    }
                                    {
                                        Number t;
                                        if (Number.TryCast(o, out t))
                                        {
                                            if (t == null)
                                            {
                                                s.SaveNull();
                                                return;
                                            }
                                            var n = Math.Abs(t.ToDecimal());
                                            if (n < 9999999999)
                                                s.SaveDecimal(t, 18, 8);
                                            else if (n < 99999999999)
                                                s.SaveDecimal(t, 19, 8);
                                            else if (n < 999999999999)
                                                s.SaveDecimal(t, 19, 7);
                                            else if (n < 9999999999999)
                                                s.SaveDecimal(t, 19, 6);
                                            else if (n < 99999999999999)
                                                s.SaveDecimal(t, 19, 5);
                                            else if (n < 999999999999999)
                                                s.SaveDecimal(t, 19, 4);
                                            else if (n < 9999999999999999)
                                                s.SaveDecimal(t, 19, 3);
                                            else if (n < 99999999999999999)
                                                s.SaveDecimal(t, 19, 2);
                                            else if (n < 999999999999999999)
                                                s.SaveDecimal(t, 19, 1);
                                            else s.SaveDecimal(t, 19, 0);
                                            return;
                                        }
                                    }
                                    {
                                        Bool t;
                                        if (Bool.TryCast(o, out t))
                                            s.SaveInt(t ? 1 : 0);
                                    }

                                    {
                                        var ba = o as byte[];
                                        if (ba != null)
                                            s.SaveByteArray(ba);
                                        if (ba == null)
                                        {
                                            var bac = o as ByteArrayColumn;
                                            if (bac != null)
                                                s.SaveString(bac.ToString(), -1, false);
                                        }
                                        
                                    }
                                });
        }



        class mySaver : IValueSaver
        {
            DynamicSQLEntity _parent;
            Func<IDbDataParameter> _createParameter;
            Firefly.Box.Data.NumberColumn _nc;
            bool _showNull;
            public mySaver(DynamicSQLEntity parent, Func<IDbDataParameter> createParamter, bool showNull,bool trimTextValues)
                : this(parent, createParamter, null, showNull,trimTextValues)
            { }

            bool _trimTextValues;
            public mySaver(DynamicSQLEntity parent, Func<IDbDataParameter> createParamter, Firefly.Box.Data.NumberColumn nc, bool showNull,bool trimTextValues)
            {
                _trimTextValues = trimTextValues;
                _showNull = showNull;
                _parent = parent;
                _createParameter = createParamter;
                _nc = nc;

            }

            public void SaveInt(int value)
            {
                if (_nc != null && !_nc.FormatInfo.SupportsMinus)
                    value = Math.Abs(value);
                string val = value.ToString();
                if (_nc != null)
                {
                    val = val.PadLeft(_nc.FormatInfo.WholeDigits + (_nc.FormatInfo.SupportsMinus ? 1 : 0));
                }
                if (value != 0)
                    val = val + " ";
                else
                {
                    val = "0";
                }
                _parent.InternalAddValueParameter(val, () =>
                                                            {
                                                                var p = _createParameter();
                                                                p.Value = value;
                                                                p.DbType = DbType.Int32;

                                                                return p;
                                                            }, _showNull);
            }

            public void SaveDecimal(decimal value, byte precision, byte scale)
            {
                if (_nc != null && !_nc.FormatInfo.SupportsMinus)
                    value = Math.Abs(value);
                Func<IDbDataParameter> createParamter =
                () =>
                {
                    var p = _createParameter();
                    p.DbType = DbType.Decimal;
                    p.Precision = precision;
                    p.Scale = scale;
                    p.Value = value;
                    return p;
                };
                if (value == 0)
                    _parent.InternalAddValueParameter("0", createParamter, _showNull);

                else if (scale == 0)
                    _parent.InternalAddValueParameter(value.ToString(System.Globalization.CultureInfo.InvariantCulture), createParamter, _showNull);
                else
                {
                    var s = ((Number)value).ToString((precision - scale).ToString() + "." + scale.ToString() + "N") + " ";
                    if (Firefly.Box.Number.DecimalSeparator.Value != '.')
                        s = s.Replace(Firefly.Box.Number.DecimalSeparator.Value, '.');
                    _parent.InternalAddValueParameter(s, createParamter, _showNull);
                }

            }

            public void SaveString(string value, int length, bool fixedWidth)
            {
                SaveStringParam(value, length, DbType.String);
            }

            void SaveStringParam(string value, int length, DbType dbType)
            {
                if (value != null)
                {
                    if (_trimTextValues)
                        value = value.TrimEnd(' ');
                    else
                    {
                        if (value.Length < length)
                            value = value.PadRight(length);
                    }
                }
                _parent.InternalAddValueParameter(value, () =>
                                                             {
                                                                 var p = _createParameter();
                                                                 p.DbType = dbType;
                                                                 p.Size = length;
                                                                 if (value == null)
                                                                     p.Value = DBNull.Value;
                                                                 else
                                                                 {
                                                                     value = value.TrimEnd(' ');
                                                                     if (value.Length == 0)
                                                                         p.Value = " ";
                                                                     else
                                                                         p.Value = value;
                                                                 }
                                                                 return p;
                                                             }, _showNull);

            }

            public void SaveAnsiString(string value, int length, bool fixedWidth)
            {
                SaveStringParam(value, length, DbType.AnsiString);
            }

            public void SaveNull()
            {
                _parent.InternalAddValueParameter(null, () =>
                                                           {
                                                               var p = _createParameter();
                                                               p.Value = DBNull.Value;
                                                               return p;
                                                           }, _showNull);
            }

            public void SaveDateTime(DateTime value)
            {
                _parent.InternalAddValueParameter( value.ToString("yyyyMMdd") /*string.Format("timestamp'{0}'", value.ToString("yyyy-MM-dd HH:mm:ss"))*/ , () =>
                                                        {
                                                            var p = _createParameter();
                                                            p.Value = value;
                                                            return p;
                                                        }, _showNull);
            }

            public void SaveTimeSpan(TimeSpan value)
            {
                throw new NotImplementedException();
            }

            public void SaveBoolean(bool value)
            {
                SaveInt(value ? 1 : 0);
            }

            public void SaveByteArray(byte[] value)
            {
                SaveString( TextColumn.FromByteArray(value),0,false);
                
            }

            public void SaveEmptyDateTime()
            {
                _parent.InternalAddValueParameter("EmptyDate", () => { throw new NotImplementedException(); }, _showNull);
            }
        }


        DatabaseErrorHandlingStrategy _onError = DatabaseErrorHandlingStrategy.Rollback;
        public Firefly.Box.Data.BoolColumn SuccessColumn { get; set; }

        /// <summary>
        /// Determines the database error handling strategy
        /// </summary>
        public DatabaseErrorHandlingStrategy OnError
        {
            get { return _onError; }
            set { _onError = value; }
        }

        public bool TrimTextColumnParameterValues { get; set; }
        public bool TrimExpressionParameterValues { get; set; }

        public static bool DefaultTrimTextColumnParameterValues { get; set; }
        public static bool TrimExpressionValuesDefault { get; set; }

        public bool RemoveTextAfterSemicolon { get; set; }
        public static bool RemoveTextAfterSemicolonDefault { get; set; }


        public override string ToString()
        {
            return _sqlBuilder.ToOriginalString();
        }

        public string GetCompletedSQL()
        {
            return GetCompletedSQL(() => new DummyIdbParameter());
        }
        class DummyIdbParameter : IDbDataParameter
        {
            public DbType DbType { get; set; }
            public ParameterDirection Direction { get; set; }
            public bool IsNullable { get; private set; }
            public string ParameterName { get; set; }
            public string SourceColumn { get; set; }
            public DataRowVersion SourceVersion { get; set; }
            public object Value { get; set; }
            public byte Precision { get; set; }
            public byte Scale { get; set; }
            public int Size { get; set; }
        }
        internal string GetCompletedSQL(Func<IDbDataParameter> createDbParameter)
        {
            _sqlBuilder.BeginPrepare();
            try
            {
                
                foreach (var action in _prepareSql)
                {
                    action(createDbParameter);
                }
                var result = _sqlBuilder.GetResult();
                if (RemoveTextAfterSemicolon && !result.TrimStart().StartsWith("BEGIN", StringComparison.InvariantCultureIgnoreCase))
                {
                    using (var sw = new StringWriter())
                    {
                        var y = result.Split('\'');
                        int i = 0;
                        foreach (var s in y)
                        {
                            if (i != 0)
                                sw.Write("'");
                            if (i % 2 == 0)
                            {
                                var index = s.IndexOf(';');
                                if (index >= 0)
                                    sw.Write(s.Remove(index));
                                else
                                    sw.Write(s);

                            }
                            else
                                sw.Write(s);
                            i++;
                        }
                        result = sw.ToString();
                    }
                }
                var zeroCharPosition= result.IndexOf('\0');
                if (zeroCharPosition >= 0)
                    result = result.Remove(zeroCharPosition);
                return result;
            }
            finally
            {
                _sqlBuilder.Restore();
            }
        }
        public class Helper
        {
            public Text From(Number n)
            {
                if (n == 0)
                    return "0";
                else if (n == null)
                    return null;
                else if (Math.Abs(n.ToDecimal()) < 9999999999)
                    return n.ToString("10.8N");
                else if (Math.Abs(n.ToDecimal()) < 99999999999)
                    return n.ToString("11.8N");
                else if (Math.Abs(n.ToDecimal()) < 999999999999)
                    return n.ToString("12.7N");
                else if (Math.Abs(n.ToDecimal()) < 9999999999999)
                    return n.ToString("13.6N");
                else if (Math.Abs(n.ToDecimal()) < 99999999999999)
                    return n.ToString("14.5N");
                else if (Math.Abs(n.ToDecimal()) < 999999999999999)
                    return n.ToString("15.4N");
                else if (Math.Abs(n.ToDecimal()) < 9999999999999999)
                    return n.ToString("16.3N");
                else if (Math.Abs(n.ToDecimal()) < 99999999999999999)
                    return n.ToString("17.2N");
                else if (Math.Abs(n.ToDecimal()) < 999999999999999999)
                    return n.ToString("18.1N");
                return n.ToString();

            }
            public Text From(Bool val)
            {
                if (val)
                    return "1";
                return "0";
            }

            public Text From(object o)
            {
                {
                    Text t;
                    if (Text.TryCast(o, out t))
                        return t;
                }
                {
                    Number t;
                    if (Number.TryCast(o, out t))
                        return From(t);
                }
                {
                    Time t;
                    if (Time.TryCast(o, out t))
                        return From(ENV.UserMethods.Instance.ToNumber(t));
                }
                {
                    Bool t;
                    if (Bool.TryCast(o, out t))
                        return From(t);
                }
                return o.ToString();
            }
        }


        internal Dictionary<ColumnBase, int> _boundInOutParam = new Dictionary<ColumnBase, int>();
        public void SetInOutParam(ColumnBase column, int position)
        {
            _boundInOutParam.Add(column, position);
        }


        internal void ExceptionHappend(Exception exx)
        {
            _sqlBuilder.ExceptionHappend(exx);
        }

        
    }
}
