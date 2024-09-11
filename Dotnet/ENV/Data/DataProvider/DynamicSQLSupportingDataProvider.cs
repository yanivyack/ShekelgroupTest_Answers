using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Web.UI;
using System.Windows.Forms.VisualStyles;
using ENV.Data.Storage;

using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;


namespace ENV.Data.DataProvider
{

    public interface ICanGetDecoratedISSQLEntityDataProvider
    {
        ISQLEntityDataProvider GetDecorated();
    }

    public class DynamicSQLSupportingDataProvider : ISQLEntityDataProvider, ISupportsGetDefinition, ICanGetDecoratedISSQLEntityDataProvider
    {
        Func<ISQLEntityDataProvider> _original;
        internal static Action<Firefly.Box.Data.TextColumn, IDbDataParameter> _processTextParameter = delegate { };
        internal static Action<ENV.Data.ByteArrayColumn, IDbDataParameter> _processByteArrayParameter = delegate { };
        internal static Func<object, object> _translateResultParamValue = x => x;
        public interface ColumnInQuery
        {
            void Load(Action<IValue> to, IDataReader r);
            bool IsString(IDataReader reader);
            bool IsDatetime(IDataReader dataReader);
            void LoadCompleteDateTime(Action<IValue> to, IDataReader r);
            void SetValue(IRowStorage rowStorage, IDataReader dataReader);
        }
        public SqlCommandFilterBuilder CreateSqlCommandFilterBuilder(IDbCommand command, Entity entity)
        {
            return _original().CreateSqlCommandFilterBuilder(command, entity);
        }
        bool ISQLEntityDataProvider.IsClosed()
        {
            return _original().IsClosed();
        }
        IValueLoader ISQLEntityDataProvider.GetDataReaderValueLoader(IDataReader reader, int columnIndexInSelect, IDateTimeCollector dtc)
        {
            return _original().GetDataReaderValueLoader(reader, columnIndexInSelect, dtc);
        }
        public ISupportsGetDefinition GetSupportGetDefinition()
        {
            return _original().GetSupportGetDefinition();
        }
        public bool RequiresTransactionForLocking
        {
            get
            {
                return _original().RequiresTransactionForLocking;
            }
        }

        public static ISQLEntityDataProvider GetDeepDecorated(ISQLEntityDataProvider dataProvider)
        {
            var x = dataProvider as ICanGetDecoratedISSQLEntityDataProvider;
            if (x != null)
                return GetDeepDecorated(x.GetDecorated());
            return dataProvider;
        }

        static Executer _executer = new Executer();
        public static Executer ExecutionStrategy
        {
            get { return _executer; }
            set { _executer = value; }
        }

        internal static Exception DbExceptionToIgnore = new System.Exception();
        public static Date DateForNullWhenNotNull = Date.Empty;
        public class Executer
        {
            public virtual IRowsSource ReadSQLResult(Firefly.Box.Data.Entity entity, Dictionary<ColumnBase, ColumnInQuery> columnIndexes, IDbCommand c, MemoryDatabase db, bool directRead)
            {
                IRowsSource datasetRowSource;
                var r = c.ExecuteReader();
                {
                    int q = 0;
                    foreach (var column in columnIndexes.Keys)
                    {
                        if (object.ReferenceEquals(column, entity.IdentityColumn))
                            continue;
                        var tc = column as TimeColumn;
                        bool timePartOfDateColumn = tc != null && tc.DateColumnForDateTimeStorage != null;
                        if (timePartOfDateColumn)
                        {
                            column.Name = tc.DateColumnForDateTimeStorage.Name;
                            continue;
                        }
                        else
                            column.Name = "C" + q++;
                        {
                            var dc = column as Firefly.Box.Data.DateColumn;
                            if (dc != null)
                            {
                                if (columnIndexes[column].IsString(r) && !(dc.Storage is NumberDateStorage))
                                    dc.Storage = new myStringDateStorage();
                                else if (!dc.AllowNull && (dc.Storage is DateTimeDateStorage || dc.Storage is DateDateStorage))
                                {
                                    dc.Storage = new FixNullToEmpty(dc.Storage);
                                }



                            }
                        }
                        {
                            var dc = column as ENV.Data.TextColumn;
                            if (dc != null)
                            {
                                if (columnIndexes[column].IsDatetime(r))
                                    dc.Storage = new DateTimeTextStorage();
                                else if (dc.Storage is DateTimeTextStorage && columnIndexes[column].IsString(r))
                                    dc.StorageType = dc.StorageType;

                            }
                        }


                    }
                    var allColumns = new List<ColumnBase>();
                    var columns = new List<ColumnBase>();
                    var datetimeSpecialColumns = new List<DateColumn>();
                    foreach (var column in entity.Columns)
                    {
                        if (object.ReferenceEquals(column, entity.IdentityColumn))
                            continue;
                        var dc = column as DateColumn;
                        if (dc != null && dc.TimeColumnForDateTimeStorage != null)
                        {
                            datetimeSpecialColumns.Add(dc);

                            continue;
                        }
                        var tc = column as TimeColumn;
                        if (tc != null && tc.DateColumnForDateTimeStorage != null)
                            continue;
                        columns.Add(column);

                    }
                    allColumns.AddRange(columns);
                    allColumns.AddRange(datetimeSpecialColumns);



                    var res = new DynamicSqlRowsSource(entity, columnIndexes, db, r, columns, datetimeSpecialColumns, allColumns, this);

                    datasetRowSource = res;
                    if (!directRead)
                        res.CreateDatatableRowSource();
                }
                return datasetRowSource;
            }
            class FixNullToEmpty : IColumnStorageSrategy<Date>
            {
                IColumnStorageSrategy<Date> _orig;

                public FixNullToEmpty(IColumnStorageSrategy<Date> orig)
                {
                    _orig = orig;
                }

                public Date LoadFrom(IValueLoader loader)
                {
                    if (loader.IsNull())
                        return DateForNullWhenNotNull;
                    else
                        return _orig.LoadFrom(loader);
                }

                public void SaveTo(Date value, IValueSaver saver)
                {
                    if (value == Date.Empty)
                        saver.SaveNull();
                    else
                        _orig.SaveTo(value, saver);
                }
            }

            public class DynamicSqlRowsSource : IRowsSource
            {
                Firefly.Box.Data.Entity _entity;
                Dictionary<ColumnBase, ColumnInQuery> _columnIndexes;
                MemoryDatabase _db;
                IDataReader _r;
                List<ColumnBase> _columns;
                List<DateColumn> _datetimeSpecialColumns;
                List<ColumnBase> _allColumns;
                Executer _parent;
                IRowsSource _source;

                public DynamicSqlRowsSource(Firefly.Box.Data.Entity entity, Dictionary<ColumnBase, ColumnInQuery> columnIndexes, MemoryDatabase db, IDataReader r, List<ColumnBase> columns, List<DateColumn> datetimeSpecialColumns, List<ColumnBase> allColumns, Executer parent)
                {
                    _parent = parent;
                    _entity = entity;
                    _columnIndexes = columnIndexes;
                    _db = db;
                    _r = r;
                    _columns = columns;
                    _datetimeSpecialColumns = datetimeSpecialColumns;
                    _allColumns = allColumns;
                }

                public IRowsSource CreateDatatableRowSource()
                {
                    if (_source != null)
                        return _source;
                    using (_r)
                    {
                        if (_entity.IdentityColumn == null)
                        {
                            _entity.IdentityColumn = new NumberColumn();
                        }
                        IRowsSource datasetRowSource;
                        datasetRowSource = ((IEntityDataProvider)_db).ProvideRowsSource(_entity);
                        foreach (DataTable table in _db.DataSet.Tables)
                        {
                            table.BeginLoadData();
                        }


                        try
                        {
                            while (_parent.Read(_r))
                            {
                                var values = new List<IValue>();
                                foreach (var column in _columns)
                                {
                                    _columnIndexes[column].Load(values.Add, _r);
                                }
                                foreach (var dc in _datetimeSpecialColumns)
                                {
                                    _columnIndexes[dc].LoadCompleteDateTime(values.Add, _r);
                                }

                                datasetRowSource.Insert(_allColumns, values, new DummyRowStorage(), _allColumns);
                            }
                            foreach (var columnBase in _columns)
                            {
                                var tc = columnBase as TextColumn;
                                if (tc != null && !(tc.Storage is DateTimeTextStorage) &&
                                    (tc.StorageType == TextStorageType.AnsiFixedLength ||
                                     tc.StorageType == TextStorageType.NullPaddedAnsiFixedLength))
                                    tc.Storage = new myTextStorage(tc, false);
                            }
                        }
                        finally
                        {
                            foreach (DataTable table in _db.DataSet.Tables)
                            {
                                table.EndLoadData();
                            }
                        }

                        return _source = datasetRowSource;
                    }
                }
                public void Dispose()
                {
                    _r.Dispose();
                    if (_source != null)
                        _source.Dispose();
                }
                class myTextStorage : IColumnStorageSrategy<Text>
                {
                    Firefly.Box.Data.TextColumn _column;
                    bool _fixedLength;
                    public myTextStorage(Firefly.Box.Data.TextColumn column, bool fixedLength)
                    {
                        _column = column;
                        _fixedLength = fixedLength;
                    }

                    public Text LoadFrom(IValueLoader loader)
                    {
                        return loader.GetString();


                    }

                    public void SaveTo(Text value, IValueSaver saver)
                    {
                        if (ReferenceEquals(value, null))
                            saver.SaveNull();
                        else
                            saver.SaveAnsiString(value, _column.FormatInfo.MaxDataLength, false);
                    }

                }

                public IRowsProvider CreateReader(IEnumerable<ColumnBase> selectedColumns, IFilter @where, Sort sort, IEnumerable<IJoin> joins, bool disableCache)
                {
                    return CreateDatatableRowSource().CreateReader(selectedColumns, where, sort, joins, disableCache);
                }

                public IRowsReader ExecuteReader(IEnumerable<ColumnBase> selectedColumns, IFilter @where, Sort sort, IEnumerable<IJoin> joins, bool lockAllRows)
                {
                    if (_source != null)
                        return _source.ExecuteReader(selectedColumns, where, sort, joins, lockAllRows);
                    return new DirectReadRowsReader(this);
                }

                class DirectReadRowsReader : IRowsReader
                {
                    DynamicSqlRowsSource _parent;

                    public DirectReadRowsReader(DynamicSqlRowsSource parent)
                    {
                        _parent = parent;
                    }

                    public void Dispose()
                    {

                    }

                    public bool Read()
                    {
                        return _parent._r.Read();
                    }

                    public IRow GetRow(IRowStorage c)
                    {

                        foreach (var item in _parent._columnIndexes.Values)
                        {
                            item.SetValue(c, _parent._r);
                        }

                        return new myRow();
                    }

                    class myRow : IRow
                    {
                        public void Delete(bool verifyRowHasNotChangedSinceLoaded)
                        {

                        }

                        public void Update(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, bool verifyRowHasNotChangedSinceLoaded)
                        {

                        }

                        public void Lock()
                        {

                        }

                        public void ReloadData()
                        {

                        }

                        public bool IsEqualTo(IRow row)
                        {
                            return row == this;
                        }

                        public void Unlock()
                        {

                        }
                    }

                    public IRow GetJoinedRow(Firefly.Box.Data.Entity e, IRowStorage c)
                    {
                        throw new NotImplementedException();
                    }
                }

                public IRowsReader ExecuteCommand(IEnumerable<ColumnBase> selectedColumns, IFilter filter, Sort sort, bool firstRowOnly,
                    bool shouldBeOnlyOneRowThatMatchesTheFilter, bool lockAllRows)
                {
                    return CreateDatatableRowSource()
                        .ExecuteCommand(selectedColumns, filter, sort, firstRowOnly,
                            shouldBeOnlyOneRowThatMatchesTheFilter, lockAllRows);
                }

                public IRow Insert(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, IRowStorage storage, IEnumerable<ColumnBase> selectedColumns)
                {
                    return CreateDatatableRowSource().Insert(columns, values,
                        storage, selectedColumns);
                }

                public bool IsOrderBySupported(Sort sort)
                {
                    return false;
                }
            }





            bool Read(IDataReader r)
            {
                using (Utilities.Profiler.StartContext("Read"))
                    return r.Read();
            }
            public virtual void ExecuteNonQuery(IDbCommand c)
            {
                c.ExecuteNonQuery();
            }

            class myStringDateStorage : IColumnStorageSrategy<Date>
            {
                public Date LoadFrom(IValueLoader loader)
                {
                    if (loader.IsNull())
                        return null;

                    try
                    {
                        var s = loader.GetString();
                        if (s.Length == 10)
                            if (s[4] == '-' || s[4] == '/')
                                return Date.Parse(s, "YYYY-MM-DD");
                            else
                                return Date.Parse(s, "DD/MM/YYYY");
                        else if (s.Length == 19)
                            return Date.Parse(s, "YYYY-MM-DD");//W6134
                        else if (s.Length != 8 || s == "79080104")
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

            internal virtual string PreProcessDynamicSQL(string sql)
            {
                return sql;
            }

            internal virtual DynamicSQLEntity.ISQLBuilder GetSQLBuilder(string sql)
            {
                return new DynamicSQLEntity.SQLBuilder(sql);
            }
        }

        public string Name { get; set; }



        public DynamicSQLSupportingDataProvider(SQLClientEntityDataProvider p) :
            this(() => new BridgeToISQLEntityDataProvider(p))
        {
        }
        public DynamicSQLSupportingDataProvider(ISQLEntityDataProvider p) :
            this(() => p)
        {
        }
        public DynamicSQLSupportingDataProvider(OracleClientEntityDataProvider p) :
            this(() => new BridgeToISQLEntityDataProvider(p))
        {
        }
        public DynamicSQLSupportingDataProvider(OdbcEntityDataProvider p) :
            this(() => new BridgeToISQLEntityDataProvider(p))
        {
        }

        internal static DynamicSQLSupportingDataProvider CreateForMicrosoftSQL(string connectionString)
        {
            return
                new DynamicSQLSupportingDataProvider(
                    new ENV.Data.DataProvider.SQLClientEntityDataProvider(new System.Data.SqlClient.SqlConnection(connectionString)));
        }
        class ErrorISQLEntityDataProvider : ISQLEntityDataProvider
        {
            Func<ISQLEntityDataProvider> _original;
            public ErrorISQLEntityDataProvider(Func<ISQLEntityDataProvider> original)
            {
                _original = () =>
                {
                    var r = original();
                    _original = () => r;
                    return r;
                };
            }
            bool ISQLEntityDataProvider.IsClosed()
            {
                return true;
            }
            public ISupportsGetDefinition GetSupportGetDefinition()
            {
                return _original().GetSupportGetDefinition();
            }

            public bool IsOracle { get { return _original().IsOracle; } }

            public bool AutoCreateTables { get { return _original().AutoCreateTables; } set { _original().AutoCreateTables = value; } }

            public bool SupportsTransactions
            {
                get { return true; }
            }
            public bool RequiresTransactionForLocking
            {
                get
                {
                    return true;
                }
            }
            public ITransaction BeginTransaction()
            {
                return _original().BeginTransaction();
            }

            public bool Contains(Firefly.Box.Data.Entity entity)
            {
                return _original().Contains(entity);
            }

            public long CountRows(Firefly.Box.Data.Entity entity)
            {
                return _original().CountRows(entity);
            }

            public IDbCommand CreateCommand()
            {
                return _original().CreateCommand();
            }

            public SqlScriptTableCreator CreateScriptGeneratorTable(Firefly.Box.Data.Entity entity)
            {
                return _original().CreateScriptGeneratorTable(entity);
            }

            public void Dispose()
            {
                _original().Dispose();
            }

            public void Drop(Firefly.Box.Data.Entity entity)
            {
                _original().Drop(entity);
            }

            public IValueLoader GetDataReaderValueLoader(IDataReader reader, int columnIndexInSelect, IDateTimeCollector dtc)
            {
                return _original().GetDataReaderValueLoader(reader, columnIndexInSelect, dtc);
            }

            public string GetEntityName(Firefly.Box.Data.Entity entity)
            {
                return entity.EntityName;
            }

            public UserDbMethods.IUserDbMethodImplementation GetUserMethodsImplementation()
            {
                return _original().GetUserMethodsImplementation();
            }

            public Exception ProcessException(Exception e, Firefly.Box.Data.Entity entity, IDbCommand c)
            {
                return _original().ProcessException(e, entity, c);
            }

            public IRowsSource ProvideRowsSource(Firefly.Box.Data.Entity entity)
            {
                return _original().ProvideRowsSource(entity);
            }

            public void Truncate(Firefly.Box.Data.Entity entity)
            {
                _original().Truncate(entity);
            }

            public SqlCommandFilterBuilder CreateSqlCommandFilterBuilder(IDbCommand command, Entity entity)
            {
                return _original().CreateSqlCommandFilterBuilder(command, entity);
            }
        }
        Action _dispose = delegate { };
        internal DynamicSQLSupportingDataProvider(Func<ISQLEntityDataProvider> original)
        {

            _original = () =>
            {
                try
                {
                    var result = original();

                    _dispose = result.Dispose;
                    return result;
                }
                catch
                {
                    var result = new ErrorISQLEntityDataProvider(original);
                    _original = () => result;
                    _dispose = result.Dispose;
                    return result;
                }
            };

        }
        public IDbCommand CreateCommand()
        {
            return _original().CreateCommand();
        }
        public void Dispose()
        {
            try
            {
                _dispose();
            }
            catch
            {
                if (ThrowExceptionsOnDisposeForTesting)
                    throw;
            }
        }
        internal bool ThrowExceptionsOnDisposeForTesting = false;



        public IRowsSource ProvideRowsSource(Firefly.Box.Data.Entity entity)
        {
            var e = entity as Entity;
            if (e != null && e.AutoCreateTable)
            {
                using (Utilities.Profiler.StartContext("Table Exists"))
                    if (!Contains(entity))
                        using (Utilities.Profiler.StartContext("Create Table"))
                            CreateTable(e);
            }
            return _original().ProvideRowsSource(entity);
        }

        public bool Contains(Firefly.Box.Data.Entity entity)
        {
            return _original().Contains(entity);
        }



        public long CountRows(Firefly.Box.Data.Entity entity)
        {
            return _original().CountRows(entity);
        }

        public void Drop(Firefly.Box.Data.Entity entity)
        {
            _original().Drop(entity);
        }

        public void Truncate(Firefly.Box.Data.Entity entity)
        {
            _original().Truncate(entity);
        }

        public ITransaction BeginTransaction()
        {
            return _original().BeginTransaction();
        }

        public bool SupportsTransactions
        {
            get
            {
                try
                {
                    return _original().SupportsTransactions;
                }
                catch
                {
                    return true;
                }

            }
        }


        internal IEntityDataProvider ProvideDynamicSqlEntityDataProvider()
        {
            return new mySqlDataProviderForSqlEntity(this);
        }

        class mySqlDataProviderForSqlEntity : IEntityDataProvider
        {
            DynamicSQLSupportingDataProvider _parent;


            public mySqlDataProviderForSqlEntity(DynamicSQLSupportingDataProvider parent)
            {
                _parent = parent;
            }

            public ITransaction BeginTransaction()
            {
                return _parent.BeginTransaction();
            }

            public bool SupportsTransactions
            {
                get { return true; }
            }
            public bool RequiresTransactionForLocking
            {
                get
                {
                    return true;
                }
            }

            class myValueReader : IValueLoader
            {
                IValueLoader _source;

                public myValueReader(IValueLoader source)
                {
                    _source = source;
                }

                public bool IsNull()
                {
                    return _source.IsNull();
                }

                public Number GetNumber()
                {
                    return _source.GetNumber();
                }

                public string GetString()
                {
                    var x = _source.GetString();
                    if (x == null)
                        return x;
                    int i = x.IndexOf('\0');
                    if (i == -1)
                        return x;
                    return x.Remove(i);
                }

                public DateTime GetDateTime()
                {
                    return _source.GetDateTime();
                }

                public TimeSpan GetTimeSpan()
                {
                    return _source.GetTimeSpan();
                }

                public bool GetBoolean()
                {
                    return _source.GetBoolean();
                }

                public byte[] GetByteArray()
                {
                    return _source.GetByteArray();
                }
            }

            class RemoveNullCharsTextColumnInQuery : NormalColumnInQuery
            {
                public RemoveNullCharsTextColumnInQuery(ColumnBase column, int index, ISQLEntityDataProvider edp)
                    : base(column, index, edp)
                {

                }
                protected override IValueLoader GetValueLoader(IDataReader r)
                {

                    return new myValueReader(base.GetValueLoader(r));
                }
            }
            class TruncateDecimalDigitsColumnInQuerty : NormalColumnInQuery
            {
                private decimal _multiplier;
                public TruncateDecimalDigitsColumnInQuerty(ColumnBase column, int index, ISQLEntityDataProvider edp, decimal multiplier)
                    : base(column, index, edp)
                {
                    _multiplier = multiplier;
                }
                class myValueReader : IValueLoader
                {
                    IValueLoader _source;
                    private decimal _multiplier;

                    public myValueReader(IValueLoader source, decimal multiplier)
                    {
                        _multiplier = multiplier;
                        _source = source;
                    }

                    public bool IsNull()
                    {
                        return _source.IsNull();
                    }

                    public Number GetNumber()
                    {

                        return (Math.Floor(_source.GetNumber().ToDecimal() * _multiplier)) / _multiplier;
                    }

                    public string GetString()
                    {
                        return _source.GetString();
                    }

                    public DateTime GetDateTime()
                    {
                        return _source.GetDateTime();
                    }

                    public TimeSpan GetTimeSpan()
                    {
                        return _source.GetTimeSpan();
                    }

                    public bool GetBoolean()
                    {
                        return _source.GetBoolean();
                    }

                    public byte[] GetByteArray()
                    {
                        return _source.GetByteArray();
                    }
                }
                protected override IValueLoader GetValueLoader(IDataReader r)
                {

                    return new myValueReader(base.GetValueLoader(r),_multiplier);
                }
            }


            class NormalColumnInQuery : ColumnInQuery
            {
                int _index;
                ColumnBase _column;
                ISQLEntityDataProvider _edp;
                public NormalColumnInQuery(ColumnBase column, int index, ISQLEntityDataProvider edp)
                {
                    _column = column;
                    _edp = edp;

                    _index = index;
                }

                public void Load(Action<IValue> to, IDataReader r)
                {
                    if (_index < r.FieldCount)
                    {

                        {
                            to(_column.LoadFrom(GetValueLoader(r)));
                        }
                    }
                    else
                    {
                        _column.ResetToDefaultValue();
                        to(new ColumnBridgeToIValue(_column));
                    }
                }

                protected virtual IValueLoader GetValueLoader(IDataReader r)
                {
                    return _edp.GetDataReaderValueLoader(r, _index, DummyDateTimeCollector.Instance);
                }

                public bool IsString(IDataReader reader)
                {
                    if (_index >= reader.FieldCount) return false;
                    return reader.GetFieldType(_index) == typeof(string);
                }

                public bool IsDatetime(IDataReader reader)
                {
                    if (_index >= reader.FieldCount) return false;
                    return reader.GetFieldType(_index) == typeof(DateTime);

                }

                public void LoadCompleteDateTime(Action<IValue> to, IDataReader r)
                {
                    if (r.IsDBNull(_index))
                        to(new NullValue());
                    else
                    {
                        to(new DateTimeValue(r.GetDateTime(_index)));
                    }
                }

                public void SetValue(IRowStorage rowStorage, IDataReader dataReader)
                {
                    rowStorage.SetValue(_column, this.GetValueLoader(dataReader));
                }
            }

            class DateTimeValue : IValue
            {
                DateTime _dt;

                public DateTimeValue(DateTime dt)
                {
                    _dt = dt;
                }

                public void SaveTo(IValueSaver saver)
                {
                    saver.SaveDateTime(_dt);
                }
            }

            class NullValue : IValue
            {
                public void SaveTo(IValueSaver saver)
                {
                    saver.SaveNull();
                }
            }

            class DateTimeTimeColumnInQuery : ColumnInQuery
            {
                int _index;
                TimeColumn _tc;

                public DateTimeTimeColumnInQuery(TimeColumn tc, int index)
                {
                    _tc = tc;
                    _index = index;
                }

                public void Load(Action<IValue> to, IDataReader r)
                {
                    var t = Time.StartOfDay;
                    if (!r.IsDBNull(_index))
                    {
                        var x = r.GetDateTime(_index);
                        t = Time.FromDateTime(x);
                    }
                    to(new myValueSaver(_tc, t));

                }
                class myValueSaver : IValue
                {
                    TimeColumn _tc;
                    Time _value;

                    public myValueSaver(TimeColumn tc, Time value)
                    {
                        _tc = tc;
                        _value = value;
                    }

                    public void SaveTo(IValueSaver saver)
                    {
                        _tc.Storage.SaveTo(_value, saver);
                    }
                }


                public bool IsString(IDataReader reader)
                {
                    return false;
                }

                public bool IsDatetime(IDataReader dataReader)
                {
                    return false;
                }


                public void LoadCompleteDateTime(Action<IValue> to, IDataReader r)
                {
                    if (r.IsDBNull(_index))
                        to(new NullValue());
                    else
                    {
                        to(new DateTimeValue(r.GetDateTime(_index)));
                    }

                }

                public void SetValue(IRowStorage rowStorage, IDataReader dataReader)
                {
                    rowStorage.SetValue(_tc, new DataReaderValueLoader(dataReader, _index, DummyDateTimeCollector.Instance));
                }
            }

            public IRowsSource ProvideRowsSource(Firefly.Box.Data.Entity entity)
            {
                bool isBatch = false;
                bool isParentInTransaction = false;
                var t = Firefly.Box.Context.Current.ActiveTasks;
                if (t.Count > 0)
                {
                    var task = t[t.Count - 1];
                    isBatch = task is BusinessProcess;
                    if (t.Count > 1)

                        isParentInTransaction = t[t.Count - 2].InTransaction;
                }
                IDbCommand c = null;
                Func<IDbCommand> commandFactory =
                    () =>
                    {
                        IDbCommand z;
                        if (c != null)
                        {
                            z = c;
                            c = null;
                            return z;
                        }
                        try
                        {
                            z = _parent.CreateCommand();
                            z.CommandText =
                                ((DynamicSQLEntity)entity).GetCompletedSQL(
                                () =>
                                {
                                    var p = z.CreateParameter();
                                    z.Parameters.Add(p);
                                    return p;
                                });
                        }
                        catch (DatabaseErrorException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            throw new DatabaseErrorException(
                                DatabaseErrorType.UnknownError, e,
                                DatabaseErrorHandlingStrategy.AbortAllTasks);
                        }
                        return z;
                    };
                c = commandFactory();
                if (isBatch && entity.Columns.Count > 0)
                    return new DelayedRowSource(() => InternalProvideSource(entity, isParentInTransaction, isBatch, commandFactory), entity);
                var x = InternalProvideSource(entity, isParentInTransaction, isBatch, commandFactory);
                if (x == null)
                    return null;
                return new PreloadedRowSourceThatSupportsReload(() => InternalProvideSource(entity, isParentInTransaction, isBatch, commandFactory), x);
            }

            class PreloadedRowSourceThatSupportsReload : IRowsSource
            {
                IRowsSource _source;
                Func<IRowsSource> _factory;
                bool _wasUsed = false;

                public PreloadedRowSourceThatSupportsReload(Func<IRowsSource> factory, IRowsSource source)
                {
                    _factory = factory;
                    _source = source;
                }
                void Prepare()
                {
                    if (!_wasUsed || DontRunSql)
                    {
                        _wasUsed = true;
                        return;
                    }
                    _source = _factory();
                }

                public void Dispose()
                {
                    _source.Dispose();
                }

                public IRowsProvider CreateReader(IEnumerable<ColumnBase> selectedColumns, IFilter @where, Sort sort, IEnumerable<IJoin> joins, bool disableCache)
                {
                    Prepare();
                    return _source.CreateReader(selectedColumns, @where, sort, joins, disableCache);
                }

                public IRowsReader ExecuteReader(IEnumerable<ColumnBase> selectedColumns, IFilter @where, Sort sort, IEnumerable<IJoin> joins, bool lockAllRows)
                {
                    Prepare();
                    return _source.ExecuteReader(selectedColumns, @where, sort, joins, lockAllRows);
                }

                public IRowsReader ExecuteCommand(IEnumerable<ColumnBase> selectedColumns, IFilter filter, Sort sort, bool firstRowOnly, bool shouldBeOnlyOneRowThatMatchesTheFilter, bool lockAllRows)
                {
                    Prepare();
                    return _source.ExecuteCommand(selectedColumns, filter, sort, firstRowOnly, shouldBeOnlyOneRowThatMatchesTheFilter, lockAllRows);
                }

                public IRow Insert(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, IRowStorage storage, IEnumerable<ColumnBase> selectedColumns)
                {
                    _wasUsed = true;
                    return _source.Insert(columns, values, storage, selectedColumns);
                }

                public bool IsOrderBySupported(Sort sort)
                {
                    return true;
                }
            }

            class DelayedRowSource : IRowsSource
            {
                IRowsSource _source;
                Func<IRowsSource> _factory;
                class InfiniteRowsSource : IRowsSource, IRowsReader, IRow
                {
                    Firefly.Box.Data.Entity _parent;
                    bool _hasRows;
                    public InfiniteRowsSource(Firefly.Box.Data.Entity parent, bool hasRows)
                    {
                        _hasRows = hasRows;
                        _parent = parent;
                    }

                    public void Dispose()
                    {

                    }

                    public IValueLoader Load(ColumnBase column)
                    {
                        throw new System.NotSupportedException();
                    }


                    public IRowsProvider CreateReader(IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort, IEnumerable<IJoin> joins, bool disableCache)
                    {
                        return new IRowsReaderProviderBridgeToNormalReader(_parent, where, sort,
                                                                           (filter, sort1) => this, (row, columnBase) =>
                                                                           {
                                                                               throw
                                                                                   new InvalidOperationException
                                                                                       ();
                                                                           }

                );
                    }

                    public IRowsReader ExecuteReader(IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort, IEnumerable<IJoin> joins, bool lockAllRows)
                    {
                        return this;
                    }

                    public IRowsReader ExecuteCommand(IEnumerable<ColumnBase> selectedColumns, IFilter filter, Sort sort, bool firstRowOnly, bool shouldBeOnlyOneRowThatMatchesTheFilter, bool lockAllRows)
                    {
                        return this;
                    }

                    public IRow Insert(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, IRowStorage storage, IEnumerable<ColumnBase> selectedColumns)
                    {
                        throw new System.NotSupportedException();
                    }

                    public bool IsOrderBySupported(Sort sort)
                    {
                        return true;
                    }

                    public void Delete(bool verifyRowHasNotChangedSinceLoaded)
                    {

                    }

                    public void Update(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, bool verifyRowHasNotChangedSinceLoaded)
                    {

                    }

                    public void Lock()
                    {

                    }

                    public void ReloadData()
                    {

                    }

                    public bool IsEqualTo(IRow row)
                    {
                        return false;
                    }

                    public void Unlock()
                    {
                    }

                    public bool Read()
                    {
                        return _hasRows;
                    }

                    public IRow GetRow(IRowStorage c)
                    {

                        return this;
                    }

                    public IRow GetJoinedRow(Firefly.Box.Data.Entity e, IRowStorage c)
                    {
                        return this;
                    }

                }
                public DelayedRowSource(Func<IRowsSource> factory, Firefly.Box.Data.Entity entity)
                {
                    _factory = factory;
                    _entity = entity;
                }

                Firefly.Box.Data.Entity _entity;
                void Prepare()
                {
                    if (_source == null)
                    {
                        _source = new InfiniteRowsSource(_entity, _entity.Columns.Count == 0);
                        var x = _factory();
                        if (x != null)
                            _source = x;
                        _toDispose.Add(_source);
                    }
                }
                List<IDisposable> _toDispose = new List<IDisposable>();
                public void Dispose()
                {
                    _toDispose.ForEach(x => x.Dispose());
                }

                public IRowsProvider CreateReader(IEnumerable<ColumnBase> selectedColumns, IFilter @where, Sort sort, IEnumerable<IJoin> joins, bool disableCache)
                {
                    Prepare();
                    return _source.CreateReader(selectedColumns, @where, sort, joins, disableCache);
                }

                public IRowsReader ExecuteReader(IEnumerable<ColumnBase> selectedColumns, IFilter @where, Sort sort, IEnumerable<IJoin> joins, bool lockAllRows)
                {
                    Prepare();
                    var r = _source.ExecuteReader(selectedColumns, @where, sort, joins, lockAllRows);
                    _source = null;
                    return r;
                }

                public IRowsReader ExecuteCommand(IEnumerable<ColumnBase> selectedColumns, IFilter filter, Sort sort, bool firstRowOnly, bool shouldBeOnlyOneRowThatMatchesTheFilter, bool lockAllRows)
                {
                    Prepare();
                    return _source.ExecuteCommand(selectedColumns, filter, sort, firstRowOnly, shouldBeOnlyOneRowThatMatchesTheFilter, lockAllRows);
                }

                public IRow Insert(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, IRowStorage storage, IEnumerable<ColumnBase> selectedColumns)
                {
                    Prepare();
                    return _source.Insert(columns, values, storage, selectedColumns);
                }

                public bool IsOrderBySupported(Sort sort)
                {
                    return true;

                }
            }

            IRowsSource InternalProvideSource(Firefly.Box.Data.Entity entity, bool isParentInTransaction, bool isBatch,
                Func<IDbCommand> commandFactory)
            {
                var de = entity as DynamicSQLEntity;
                using (Profiler.DynamicSQLEntity(de))
                {
                    var c = commandFactory();



                    Action<bool> result = x =>
                    {
                        if (de != null && de.SuccessColumn != null)
                        {
                            de.SuccessColumn.Value = x;
                            var t = Firefly.Box.Context.Current.ActiveTasks;
                            if (t.Count > 0)
                            {
                                ControllerBase.SendInstanceBasedOnTaskAndCallStack(t[t.Count - 1], cont =>
                                {
                                    if (cont.Columns.Contains(de.SuccessColumn))
                                    {
                                        foreach (var item in cont._boundParameters)
                                        {
                                            if (item.SetDynamicSQLSuccessColumnValueForAndReturnTrueIfYouHaveDoneIt(de.SuccessColumn, x))
                                                return;
                                        }
                                        de.SuccessColumn.Value = de.SuccessColumn.DefaultValue;
                                    }
                                });

                            }
                        }
                    };
                    var db = new MemoryDatabase();
                    if (de != null)
                        de.InternalDataSet = db.DataSet;
                    IRowsSource datasetRowSource = null;
                    var _columnIndexes = new Dictionary<ColumnBase, ColumnInQuery>();

                    int ofset = 0;
                    var usedNames = new HashSet<string>();
                    foreach (var column in entity.Columns)
                    {
                        if (object.ReferenceEquals(column, entity.IdentityColumn))
                            continue;
                        while (usedNames.Contains(column.Name))
                        {
                            column.Name += "1";
                        }
                        usedNames.Add(column.Name);
                        var tc = column as TimeColumn;
                        if (tc != null && tc.DateColumnForDateTimeStorage != null)
                        {
                            ofset -= 1;
                            _columnIndexes.Add(column, new DateTimeTimeColumnInQuery(tc, _columnIndexes.Count + ofset));
                        }
                        else
                        {
                            var nc = column as NumberColumn;
                            if (nc != null && nc.FormatInfo.DecimalDigits > 0)
                            {
                                _columnIndexes.Add(column, new TruncateDecimalDigitsColumnInQuerty(column, _columnIndexes.Count + ofset, _parent, ENV.UserMethods.Instance.Pow(10, nc.FormatInfo.DecimalDigits)));
                            }
                            else
                            {
                                var textColumn = column as ENV.Data.TextColumn;
                                if (textColumn != null && textColumn.StorageType != TextStorageType.NullPaddedAnsiFixedLength)
                                    _columnIndexes.Add(column, new RemoveNullCharsTextColumnInQuery(column, _columnIndexes.Count + ofset, _parent));
                                else
                                    _columnIndexes.Add(column, new NormalColumnInQuery(column, _columnIndexes.Count + ofset, _parent));
                            }
                        }
                    }


                    try
                    {
                        if (((IEntityDataProvider)db).Contains(entity))
                            ((IEntityDataProvider)db).Drop(entity);
                        using (c)
                        {
                            try
                            {
                                var cmd = c.CommandText.ToUpper(CultureInfo.InvariantCulture).TrimStart();
                                if (_parent._original().IsOracle &&
                                    cmd.StartsWith("EXEC"))
                                {
                                    datasetRowSource = new ProcedureRunner(entity, c, db).Execute();
                                }
                                else if (entity.Columns.Count > 0)
                                {
                                    if (cmd.StartsWith("CALL ") && cmd.Contains("?"))
                                        datasetRowSource = new CallProcedure(entity, c, db).Execute();
                                    else
                                    {
                                        if (cmd.StartsWith("EXEC"))
                                        {
                                            cmd = cmd.Substring(4).TrimStart();
                                            if (cmd.StartsWith("?"))
                                            {
                                                cmd = cmd.Substring(1).TrimStart();
                                                if (cmd.StartsWith("="))
                                                {
                                                    cmd = cmd.Substring(1).TrimStart();
                                                    c.CommandText = c.CommandText.Substring(c.CommandText.Length - cmd.Length);
                                                }
                                            }
                                        }
                                        datasetRowSource = _executer.ReadSQLResult(entity, _columnIndexes, c, db, isBatch && _parent.IsOracle);
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        _executer.ExecuteNonQuery(c);
                                    }
                                    catch (SqlException ex)
                                    {
                                        if (ex.Number != 266)
                                            throw;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    throw _parent._original().ProcessException(ex, entity, c);
                                }
                                catch (Exception exx)
                                {
                                    de.ExceptionHappend(exx);
                                    throw;
                                }

                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        if (!ex.Message.StartsWith("ORA-01403") && ex != DbExceptionToIgnore)
                        {
                            result(false);


                            var x = ((DynamicSQLEntity)entity).OnError;
                            if (entity.Columns.Count == 0 && isBatch && !isParentInTransaction &&
                                x == DatabaseErrorHandlingStrategy.Rollback)
                                x = DatabaseErrorHandlingStrategy.Ignore;

                            var dee = ex as DatabaseErrorException;
                            if (dee != null && dee.ErrorType != DatabaseErrorType.UnknownError)
                                throw ex;
                            throw new DatabaseErrorException(DatabaseErrorType.UnknownError, ex, x);
                        }
                    }



                    result(true);
                    return datasetRowSource;
                }
            }

            class ProcedureRunner
            {
                Firefly.Box.Data.Entity entity;
                IDbCommand c;
                MemoryDatabase db;
                StringBuilder currentArg;
                int j = 1;

                public ProcedureRunner(Firefly.Box.Data.Entity entity, IDbCommand c, MemoryDatabase db)
                {
                    this.entity = entity;
                    this.c = c;
                    this.db = db;
                }
                List<ColumnBase> columns = new List<ColumnBase>();
                List<Func<IValue>> valueFactories = new List<Func<IValue>>();
                StringBuilder resultArgs = new StringBuilder();
                HashSet<ColumnBase> usedColumns = new HashSet<ColumnBase>();
                void AddColumn(ColumnBase col)
                {
                    bool foundReadyParam = false;
                    IDbDataParameter p = null;
                    if (currentArg.ToString().Trim().StartsWith(":p"))
                    {
                        foreach (IDbDataParameter par in c.Parameters)
                        {
                            if (par.ParameterName ==
                                currentArg.ToString().Trim())
                            {
                                p = par;
                                foundReadyParam = true;
                                break;
                            }
                        }
                    }

                    if (p == null)
                    {
                        p = c.CreateParameter();
                        p.ParameterName = ":fld" + j;


                        c.Parameters.Add(p);
                    }
                    p.Direction = ParameterDirection.InputOutput;
                    if (!foundReadyParam)
                        if (col is Firefly.Box.Data.NumberColumn)
                        {
                            var n = col as Firefly.Box.Data.NumberColumn;
                            var en = n as ENV.Data.NumberColumn;
                            if (en != null && en.DbType != null && en.DbType.IndexOf("SMALLINT", StringComparison.InvariantCultureIgnoreCase) > -1)
                            {
                                p.DbType = DbType.Int16;
                            }
                            else
                            {

                                p.DbType = DbType.Decimal;
                                p.Precision = (byte)n.FormatInfo.Precision;
                                p.Scale = (byte)n.FormatInfo.Scale;
                                if (p.Precision == 0)
                                {
                                    p.Precision = 36;
                                    p.Scale = 18;
                                }
                            }
                            if (p.Value == null)
                            {
                                decimal d;
                                if (currentArg.Length > 0 &&
                                    currentArg.ToString().Trim() != "" &&
                                    currentArg.ToString().Trim() != "NULL" &&
                                    decimal.TryParse(currentArg.ToString(),
                                        NumberStyles.Number, CultureInfo.InvariantCulture,
                                        out d))
                                    p.Value = d;
                                else
                                    p.Value = 0;
                            }
                        }
                        else if (col is Firefly.Box.Data.TextColumn)
                        {
                            var n = col as Firefly.Box.Data.TextColumn;
                            p.DbType = DbType.String;
                            p.Size = 8000;
                            var ca = currentArg.ToString().Trim();
                            if (ca.Length > 0 && ca.StartsWith("\'"))
                            {
                                ca = ca.Substring(1);
                                ca = ca.Remove(ca.Length - 1);
                                p.Value = ca.Replace("''", "'").TrimStart();
                            }
                            else
                            {
                                p.Value = "";
                            }
                            _processTextParameter(n, p);
                        }
                        else if (col is ENV.Data.ByteArrayColumn)
                        {
                            var n = col as ENV.Data.ByteArrayColumn;
                            p.DbType = DbType.String;
                            p.Size = 8000;
                            var ca = currentArg.ToString().Trim();
                            if (ca.Length > 0 && ca.StartsWith("\'"))
                            {
                                ca = ca.Substring(1);
                                ca = ca.Remove(ca.Length - 1);
                                p.Value = ca.Replace("''", "'").TrimStart();
                            }
                            else
                            {
                                p.Value = "";
                            }
                            _processByteArrayParameter(n, p);
                        }
                        else if (col is Firefly.Box.Data.DateColumn)
                        {
                            var n = col as Firefly.Box.Data.DateColumn;
                            if (n.Storage is DateTimeDateStorage || n.Storage is DateTimeDateStorageThatSupportsEmptyDate)
                            {
                                p.DbType = DbType.DateTime;
                            }
                            else
                            {
                                p.DbType = DbType.AnsiString;
                                p.Size = 8;
                                var ca = currentArg.ToString().Trim();
                                if (p.Value == null)
                                {
                                    if (ca.Length > 0)
                                    {
                                        ca = ca.Substring(1);
                                        ca = ca.Remove(ca.Length - 1);
                                        p.Value =
                                            ca.Replace("''", "'").TrimStart();
                                    }
                                    else
                                    {
                                        p.Value = "";
                                    }
                                }
                            }
                        }
                        else if (col is Firefly.Box.Data.TimeColumn)
                        {
                            var n = col as Firefly.Box.Data.TimeColumn;
                            if (n.Storage is DateTimeTimeStorage)
                            {
                                p.DbType = DbType.Date;
                            }
                            else
                            {
                                p.DbType = DbType.Decimal;
                                p.Size = 8;
                                var ca = currentArg.ToString().Trim();
                                if (p.Value == null)
                                {
                                    if (ca.Length > 0)
                                    {
                                        p.Value = ca;
                                    }
                                    else
                                    {
                                        p.Value = "0";
                                    }
                                }
                            }
                        }
                        else if (col is Firefly.Box.Data.BoolColumn)
                        {
                            var n = col as Firefly.Box.Data.BoolColumn;
                            p.DbType = DbType.Int32;
                            p.Size = 8;
                            var ca = currentArg.ToString().Trim();
                            if (p.Value == null)
                            {
                                p.Value = ca == "1";
                            }
                        }
                        else
                            throw new NotSupportedException(
                                "Columns other then number and text in dynamic sql procedure result - " +
                                col.GetType().FullName);
                    currentArg = new StringBuilder(p.ParameterName);
                    columns.Add(col);
                    valueFactories.Add(
                        () =>
                            col.LoadFrom(
                                new IValueBridgeToParameter(() => _translateResultParamValue(p.Value))));
                }

                void AddCurrentArg()
                {
                    foreach (var s in ((DynamicSQLEntity)entity)._boundInOutParam)
                    {
                        if (s.Value == j)
                        {
                            AddColumn(s.Key);
                            usedColumns.Add(s.Key);
                            break;
                        }
                    }
                    if (resultArgs.Length > 0)
                        resultArgs.Append(",");
                    resultArgs.Append(currentArg);
                    j++;
                    currentArg = new StringBuilder();
                }

                public IRowsSource Execute()
                {
                    c.CommandType = CommandType.Text;
                    c.CommandText = c.CommandText.TrimStart().Substring(4);
                    if (c.CommandText.ToUpper(CultureInfo.InvariantCulture).StartsWith("UTE"))
                        c.CommandText = c.CommandText.Substring(3);
                    if (entity.Columns.Count > 0)
                    {
                        string start, args, end;

                        {
                            var indexOfP = c.CommandText.IndexOf("(");
                            if (indexOfP >= 0)
                            {
                                var s = c.CommandText.Remove(indexOfP);
                                if (string.IsNullOrEmpty(s))
                                    throw new Exception("Invalid sql statement");
                                start = s;
                                var s2 = c.CommandText.Substring(s.Length + 1);
                                var lastParense = FindCloseParense(s2);
                                if (lastParense >= 0)
                                {
                                    args = s2.Remove(lastParense);
                                    end = s2.Substring(args.Length + 1);
                                }
                                else
                                {
                                    args = s2;
                                    end = "";
                                }

                            }
                            else
                            {
                                start = c.CommandText;
                                args = "";
                                end = "";
                            }
                        }
                        using (var sr = new System.IO.StringReader(args))
                        {
                            int ch;
                            currentArg = new StringBuilder();
                            bool runLoop = true;
                            while (runLoop)
                            {
                                ch = sr.Read();
                                switch (ch)
                                {
                                    case (int)',':
                                        AddCurrentArg();
                                        break;
                                    case -1:
                                        AddCurrentArg();
                                        runLoop = false;
                                        break;
                                    case '\'':
                                        {
                                            currentArg.Append((char)ch);
                                            bool instring = true;
                                            while (instring)
                                            {
                                                ch = sr.Read();
                                                switch (ch)
                                                {
                                                    case -1:
                                                        break;
                                                    case '\'':
                                                        {
                                                            currentArg.Append((char)ch);
                                                            instring = false;
                                                            break;
                                                        }
                                                    default:
                                                        currentArg.Append((char)ch);
                                                        break;
                                                }
                                            }
                                        }
                                        break;
                                    case '(':
                                        ReadParense(sr, currentArg);
                                        break;
                                    default:
                                        currentArg.Append((char)ch);
                                        break;
                                }
                            }
                            foreach (var col in entity.Columns)
                            {
                                if (!usedColumns.Contains(col))
                                {
                                    AddColumn(col);
                                    if (resultArgs.Length > 0)
                                        resultArgs.Append(",");
                                    resultArgs.Append(currentArg);
                                    j++;
                                    currentArg = new StringBuilder();
                                }
                            }
                            args = resultArgs.ToString();
                            c.CommandText = start + "(" + args + ")" + end;
                        }
                    }

                    if (!c.CommandText.TrimEnd().EndsWith(";"))
                        c.CommandText += ";";

                    c.CommandText = "begin " + c.CommandText + " end;";
                    c.ExecuteNonQuery();
                    var datasetRowSource = ((IEntityDataProvider)db).ProvideRowsSource(entity);

                    var values = new List<IValue>();

                    foreach (var x in valueFactories)
                    {
                        values.Add(x());
                    }
                    datasetRowSource.Insert(columns, values, new DummyRowStorage(), columns);
                    return datasetRowSource;
                }

                int FindCloseParense(string s2)
                {
                    int depth = 0;
                    bool disable = false;
                    for (int i = 0; i < s2.Length; i++)
                    {
                        switch (s2[i])
                        {
                            case '(':
                                if (!disable)
                                {
                                    depth++;
                                }
                                break;
                            case '\'':
                                disable = !disable;
                                break;
                            case ')':
                                if (!disable)
                                {
                                    if (depth == 0)
                                        return i;
                                    else
                                        depth--;
                                }
                                break;


                        }

                    }
                    return -1;
                }

                void ReadParense(StringReader sr, StringBuilder currentArg)
                {
                    currentArg.Append('(');
                    int ch;
                    while (true)
                    {
                        ch = sr.Read();
                        switch (ch)
                        {
                            case -1:
                                return;
                            case ')':
                                currentArg.Append(')');
                                return;
                            case '(':
                                ReadParense(sr, currentArg);
                                break;
                            default:
                                currentArg.Append((char)ch);
                                break;
                        }
                    }
                }
            }








            class ColumnBridgeToIValue : IValue
            {
                ColumnBase _column;

                public ColumnBridgeToIValue(ColumnBase column)
                {
                    _column = column;
                }

                public void SaveTo(IValueSaver saver)
                {
                    _column.SaveYourValueToDb(saver);
                }
            }


            public bool Contains(Firefly.Box.Data.Entity entity)
            {
                throw new System.NotImplementedException();
            }

            public long CountRows(Firefly.Box.Data.Entity entity)
            {
                throw new System.NotImplementedException();
            }

            public void Drop(Firefly.Box.Data.Entity entity)
            {
                throw new System.NotImplementedException();
            }

            public void Truncate(Firefly.Box.Data.Entity entity)
            {
                throw new System.NotImplementedException();
            }

            public void Dispose()
            {

            }
        }

        internal bool IsClosed()
        {
            return _original().IsClosed();
        }

        public void Execute(string sql)
        {
            using (var c = CreateCommand())
            {
                c.CommandText = sql;
                try
                {
                    c.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    using (var s = new StringWriter())
                    {
                        s.WriteLine(c.CommandText);
                        foreach (IDbDataParameter p in c.Parameters)
                        {
                            s.WriteLine(string.Format("{0}({2}) = {1}", p.ParameterName, p.Value, p.DbType));
                        }
                        ex.Data.Add("SQL", s.ToString());
                    }
                    throw;
                }

            }
        }

        public void CreateTable(Entity entity)
        {
            var x = new EntityScriptGenerator(new SqlScriptGenerator(_original().CreateScriptGeneratorTable));
            x.AddEntity(entity);
            x.Execute(Execute);

        }
        /// <summary>
        /// If you want to get results from db for production code see: http://doc.fireflymigration.com/accessing-the-database-directly.html
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public string GetHtmlTableBasedOnSQLResultForDebugPurposes(string sql)
        {
            using (var c = CreateCommand())
            {
                c.CommandText = sql;
                var st = new System.Diagnostics.Stopwatch();
                int j = 0;
                using (var sw = new StringWriter())
                {
                    using (var h = new HtmlTextWriter(sw))
                    {
                        st.Start();
                        using (var r = c.ExecuteReader())
                        {
                            st.Stop();
                            h.WriteBeginTag("Table");
                            h.WriteAttribute("border", "1");
                            h.Write(HtmlTextWriter.TagRightChar);
                            h.EndRender();
                            h.WriteFullBeginTag("Tr");
                            for (int i = 0; i < r.FieldCount; i++)
                            {
                                h.WriteFullBeginTag("th");
                                h.WriteEncodedText(r.GetName(i));
                                h.WriteLine("<br/>");
                                h.WriteEncodedText(r.GetDataTypeName(i));
                                h.WriteEndTag("th");
                            }
                            h.WriteEndTag("tr");


                            while (r.Read())
                            {
                                if (j++ > 1000)
                                    continue;
                                h.WriteFullBeginTag("tr");
                                for (int i = 0; i < r.FieldCount; i++)
                                {
                                    h.WriteFullBeginTag("td");
                                    if (r.IsDBNull(i))
                                        h.WriteEncodedText("NULL");
                                    else
                                    {
                                        var x = r[i];
                                        if (x is byte[])
                                        {
                                            var s = "";
                                            var y = (byte[])x;
                                            int z = 0;
                                            foreach (var item in y)
                                            {
                                                if (s.Length > 0)
                                                    s += ",";
                                                s += item.ToString();
                                                if (z++ > 100)
                                                    break;
                                            }
                                            x = string.Format("new byte[]{{{0}}}", s);
                                        }
                                        h.WriteEncodedText(x.ToString().Replace("\0", @"\0"));
                                    }
                                    h.WriteEndTag("td");
                                }
                                h.WriteEndTag("tr");
                            }
                            h.WriteEndTag("table");


                        }
                    }

                    return j + " rows: in " + ((Number)(st.ElapsedMilliseconds) / 1000).ToString("6.3C").Trim() + " Seconds" + sw.ToString();
                }
            }
        }

        public Exception ProcessException(Exception exception, Firefly.Box.Data.Entity entity, IDbCommand dbCommand)
        {
            return _original().ProcessException(exception, entity, dbCommand);
        }
        internal SQLDataProviderHelperClient _internalGetClient()

        {
            return (SQLDataProviderHelperClient)((BridgeToISQLEntityDataProvider)GetDeepDecorated(this)).DataProvider;
        }

        public bool IsOracle
        {
            get
            {
                return _original().IsOracle;
            }
        }
        public bool AutoCreateTables
        {
            get { return _original().AutoCreateTables; }
            set
            {
                _original().AutoCreateTables = value;
            }
        }
        public SqlScriptTableCreator CreateScriptGeneratorTable(Firefly.Box.Data.Entity entity)
        {
            return _original().CreateScriptGeneratorTable(entity);
        }

        bool ISupportsGetDefinition.Available
        {
            get { return true; }
        }

        internal static bool DontRunSql { get; set; }

        void ISupportsGetDefinition.SendTables(IEntityDataProvider dp, Action<GetDefinition.TableBase, FilterBase> to)
        {
            var x = _original() as ICanGetDecoratedISSQLEntityDataProvider;
            if (x != null)
            {
                var d = x.GetDecorated();
                var supp = d.GetSupportGetDefinition();
                if (supp != null)
                {
                    supp.SendTables(dp, to);
                    return;
                }
            }

            if (IsOracle)
                to(new GetDefinition.OracleTables(dp), null);
            else
                to(new GetDefinition.Tables(dp), null);
        }


        public UserDbMethods.IUserDbMethodImplementation GetUserMethodsImplementation()
        {
            return _original().GetUserMethodsImplementation();
        }

        public ISQLEntityDataProvider GetDecorated()
        {
            return _original();
        }

        public string GetEntityName(Firefly.Box.Data.Entity entity)
        {
            return _original().GetEntityName(entity);
        }
    }

    class IValueBridgeToParameter : IValueLoader
    {
        Func<object> _getValue;

        public IValueBridgeToParameter(Func<object> getValue)
        {
            _getValue = getValue;
        }



        public bool IsNull()
        {
            return _getValue() == null || _getValue() is System.DBNull;
        }

        public Number GetNumber()
        {
            var x = _getValue();
            if (x is System.DBNull || ReferenceEquals(x, null))
                return null;
            if (x is string)
                return Number.Parse((string)x);
            return Number.Cast(x);
        }

        public string GetString()
        {
            var x = _getValue();
            if (x is System.DBNull || ReferenceEquals(x, null))
                return null;
            return x.ToString();
        }

        public DateTime GetDateTime()
        {
            var x = _getValue();
            if (x is System.DBNull || ReferenceEquals(x, null))
                return DateTime.MinValue;
            return (DateTime)x;
        }

        public TimeSpan GetTimeSpan()
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean()
        {
            return (int)_getValue() == 1;
        }

        public byte[] GetByteArray()
        {
            throw new NotImplementedException();
        }
    }

    class DummyRowStorage : IRowStorage
    {
        Dictionary<ColumnBase, IValue> _vals = new Dictionary<ColumnBase, IValue>();
        public IValue GetValue(ColumnBase column)
        {
            return _vals[column];
        }


        public void SetValue(ColumnBase column, IValueLoader value)
        {
            if (_vals.ContainsKey(column))
                _vals[column] = column.LoadFrom(value);
            else
                _vals.Add(column, column.LoadFrom(value));

        }


    }

    class CallProcedure
    {
        IDbCommand _command;
        MemoryDatabase _db;
        Firefly.Box.Data.Entity _entity;
        List<ColumnBase> _columns = new List<ColumnBase>();
        List<Func<IValue>> _valueFactories = new List<Func<IValue>>();
        public CallProcedure(Firefly.Box.Data.Entity entity, IDbCommand dbCommand, MemoryDatabase db)
        {
            _command = dbCommand;
            this._db = db;
            this._entity = entity;
            foreach (var col in entity.Columns)
            {
                var p = _command.CreateParameter();
                p.ParameterName = "?";

                p.Direction = ParameterDirection.InputOutput;
                if (col is Firefly.Box.Data.NumberColumn)
                {
                    var c = (Firefly.Box.Data.NumberColumn)col;
                    var nc = c as ENV.Data.NumberColumn;
                    if (nc != null && nc.DbType != null && nc.DbType.IndexOf("SMALLINT", StringComparison.InvariantCultureIgnoreCase) > -1)
                        p.DbType = DbType.Int16;
                    else if (c.FormatInfo.DecimalDigits == 0)
                        if (c.Storage is ENV.Data.Storage.SmallIntNumberStorage)
                            p.DbType = DbType.Int16;
                        else
                            p.DbType = DbType.Int32;
                    else p.DbType = DbType.Decimal;

                }
                else if (col is Firefly.Box.Data.TextColumn)
                    p.DbType = DbType.String;
                else if (col is Firefly.Box.Data.DateColumn)
                    p.DbType = DbType.DateTime;
                else throw new NotImplementedException("Call Procedure with out paramater:" + col.GetType());
                _command.Parameters.Add(p);

                _columns.Add(col);
                _valueFactories.Add(
                            () =>
                                col.LoadFrom(
                                    new IValueBridgeToParameter(() => p.Value)));
            }

        }

        public IRowsSource Execute()
        {

            _command.ExecuteNonQuery();
            var datasetRowSource = ((IEntityDataProvider)_db).ProvideRowsSource(_entity);

            var values = new List<IValue>();

            foreach (var x in _valueFactories)
            {
                values.Add(x());
            }
            datasetRowSource.Insert(_columns, values, new DummyRowStorage(), _columns);
            return datasetRowSource;
        }
    }

    public interface ISQLEntityDataProvider : IEntityDataProvider
    {
        IDbCommand CreateCommand();

        Exception ProcessException(Exception e, Firefly.Box.Data.Entity entity, IDbCommand c);

        bool IsOracle { get; }
        bool AutoCreateTables { get; set; }

        SqlScriptTableCreator CreateScriptGeneratorTable(Firefly.Box.Data.Entity entity);
        UserDbMethods.IUserDbMethodImplementation GetUserMethodsImplementation();
        IValueLoader GetDataReaderValueLoader(IDataReader reader, int columnIndexInSelect, IDateTimeCollector dtc);
        string GetEntityName(Firefly.Box.Data.Entity entity);
        ISupportsGetDefinition GetSupportGetDefinition();
        bool IsClosed();
        SqlCommandFilterBuilder CreateSqlCommandFilterBuilder(IDbCommand command, Entity entity);
    }
    delegate IValueLoader GetDataReaderValueLoaderDelegate(IDataReader reader, int columnIndexInSelect, IDateTimeCollector dtc);

    class BridgeToISQLEntityDataProvider : ISQLEntityDataProvider
    {
        internal IEntityDataProvider DataProvider;
        Func<IDbCommand> _createCommand;
        Func<bool> _isClosed;
        Func<Firefly.Box.Data.Entity, string> _getEntityName;
        Func<IDbCommand, Entity, SqlCommandFilterBuilder> _createFilterBuilder;


        bool ISQLEntityDataProvider.IsClosed()
        {
            return _isClosed();
        }

        public IRowsSource ProvideRowsSource(Firefly.Box.Data.Entity entity)
        {
            return DataProvider.ProvideRowsSource(entity);
        }

        public ITransaction BeginTransaction()
        {
            return DataProvider.BeginTransaction();
        }
        public bool RequiresTransactionForLocking
        {
            get
            {
                return DataProvider.RequiresTransactionForLocking;
            }
        }
        public bool SupportsTransactions
        {
            get { return DataProvider.SupportsTransactions; }
        }

        public bool Contains(Firefly.Box.Data.Entity entity)
        {
            return DataProvider.Contains(entity);
        }

        public long CountRows(Firefly.Box.Data.Entity entity)
        {
            return DataProvider.CountRows(entity);
        }

        public void Drop(Firefly.Box.Data.Entity entity)
        {
            DataProvider.Drop(entity);
        }

        public void Truncate(Firefly.Box.Data.Entity entity)
        {
            DataProvider.Truncate(entity);
        }

        Func<Firefly.Box.Data.Entity, SqlScriptTableCreator> _createScriptGeneratorTable;


        public BridgeToISQLEntityDataProvider(SQLClientEntityDataProvider p) :
            this(p, p.CreateCommand, s => new MssqlTable(p.GetEntityName(s), s), false, p.ProcessException, new UserDbMethods.Default(), ((SQLDataProviderHelperClient)p).GetDataReaderValueLoader, p.GetEntityName, p.IsClosed, p.CreateSqlCommandFilterBuilder)
        {
        }


        public BridgeToISQLEntityDataProvider(OracleClientEntityDataProvider p) :
            this(p, p.CreateCommand, s => new OracleTable(p.GetEntityName(s)), true, p.ProcessException, new UserDbMethods.OracleDbMethods(), ((SQLDataProviderHelperClient)p).GetDataReaderValueLoader, p.GetEntityName, p.IsClosed, p.CreateSqlCommandFilterBuilder)
        {
            _userMethods = new UserDbMethods.OracleDbMethods();
        }
        public BridgeToISQLEntityDataProvider(OdbcEntityDataProvider p) :
            this(p, p.CreateCommand, s => new OdbcTable(p.GetEntityName(s), p.ColumnNameWrapper), false, p.ProcessException, new UserDbMethods.MySql(), ((SQLDataProviderHelperClient)p).GetDataReaderValueLoader, p.GetEntityName, p.IsClosed, p.CreateSqlCommandFilterBuilder)
        {
        }

        UserDbMethods.IUserDbMethodImplementation _userMethods = new UserDbMethods.Default();
        GetDataReaderValueLoaderDelegate _getDataReaderValueLoader;
        internal BridgeToISQLEntityDataProvider(IEntityDataProvider dataProvider, Func<IDbCommand> createCommand, Func<Firefly.Box.Data.Entity, SqlScriptTableCreator> createScriptGeneratorTable, bool isOracle, Func<Exception, Firefly.Box.Data.Entity, IDbCommand, Exception> processException, UserDbMethods.IUserDbMethodImplementation dbMethods, GetDataReaderValueLoaderDelegate getDataReaderValueLoader, Func<Firefly.Box.Data.Entity, string> getEntityName, Func<bool> isClosed, Func<IDbCommand, Entity, SqlCommandFilterBuilder> createFilterBuilder)
        {
            _createFilterBuilder = createFilterBuilder;
            DataProvider = dataProvider;
            _createCommand = createCommand;
            _createScriptGeneratorTable = createScriptGeneratorTable;
            IsOracle = isOracle;
            _processException = processException;
            _userMethods = dbMethods;
            _getDataReaderValueLoader = getDataReaderValueLoader;
            _getEntityName = getEntityName;
            _isClosed = isClosed;


        }
        public void Dispose()
        {
            DataProvider.Dispose();
            if (Disposed != null)
                Disposed();
        }


        public event Action Disposed;

        Func<Exception, Firefly.Box.Data.Entity, IDbCommand, Exception> _processException;

        public IDbCommand CreateCommand()
        {
            return _createCommand();
        }


        public Exception ProcessException(Exception e, Firefly.Box.Data.Entity entity, IDbCommand c)
        {
            return _processException(e, entity, c);
        }

        public bool IsOracle { get; private set; }

        public bool AutoCreateTables
        {
            get; set;
        }

        public SqlScriptTableCreator CreateScriptGeneratorTable(Firefly.Box.Data.Entity entity)
        {
            return _createScriptGeneratorTable(entity);
        }

        public UserDbMethods.IUserDbMethodImplementation GetUserMethodsImplementation()
        {
            return _userMethods;
        }

        public IValueLoader GetDataReaderValueLoader(IDataReader reader, int columnIndexInSelect, IDateTimeCollector dtc)
        {
            return _getDataReaderValueLoader(reader, columnIndexInSelect, dtc);
        }

        public string GetEntityName(Firefly.Box.Data.Entity entity)
        {
            return _getEntityName(entity);
        }

        public ISupportsGetDefinition GetSupportGetDefinition()
        {
            return DataProvider as ISupportsGetDefinition;
        }

        public SqlCommandFilterBuilder CreateSqlCommandFilterBuilder(IDbCommand command, Entity entity)
        {
            var r = _createFilterBuilder(command, entity);
            if (r == null)
            {
                var p = new NoParametersFilterItemSaver(!this.IsOracle, this.IsOracle ? OracleClientEntityDataProvider.DateTimeStringFormat : SQLClientEntityDataProvider.DateTimeStringFormat, DummyDateTimeCollector.Instance, this.IsOracle ? OracleClientEntityDataProvider.DateTimeStringFormatForToString : null);
                var x =
                    new SQLFilterConsumer(
                       p, y => y.Name, false, new dummySqlFilterHelper(p))
                    { NewLinePrefix = "" };
                return x;
            }
            return r;
        }
    }
}