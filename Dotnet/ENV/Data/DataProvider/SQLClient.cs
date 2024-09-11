using Firefly.Box.Data.DataProvider;
using Firefly.Box;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data;
using Firefly.Box.Testing;
using ENV.Data.Storage;

namespace ENV.Data.DataProvider
{

    public interface IConnectionWrapper
    {
        IDbConnection GetInnerConnection();
    }

    public class OdbcEntityDataProvider : IEntityDataProvider, SQLDataProviderHelperClient
    {
        SQLDataProviderHelper _helper;
        public string ColumnNameWrapper = "\"";
        public bool RequiresTransactionForLocking
        {
            get
            {
                return true;
            }
        }
        public SqlCommandFilterBuilder CreateSqlCommandFilterBuilder(IDbCommand command, Firefly.Box.Data.Entity entity)
        {
            return _helper.CreateSqlCommandFilterBuilder(command, entity);
        }
        public bool UseParameters { get; set; }
        public Exception ProcessException(Exception ex, Firefly.Box.Data.Entity e, IDbCommand c)
        {
            return _helper.ProcessException(ex, e, c);
        }
        IRowsReader SQLDataProviderHelperClient.ProvideAlternateExecuteCommandWithLock(bool firstRowOnly, bool shouldBeOnlyOneRowThatMatchesTheFilter, bool lockAllrows, SQLDataProviderHelper.SQLSelectCommand.AlternateExecuteCommandHelper helper)
        {
            return null;
        }
        public void AddToCommandAfterOrderBy(StringBuilder commandText, bool firstRowOnly, Firefly.Box.Data.Entity entity, Sort sort, bool isFilterUnique, bool hasAccentCharsInNonEqualOperators, bool optimizeForFirstRows, bool fromRelation)
        {

        }
        public OdbcEntityDataProvider(IDbConnection connection)
        {
            _helper = new SQLDataProviderHelper(connection, this);

            var x = connection as System.Data.Odbc.OdbcConnection;
            if (x == null)
            {
                var y = connection as IConnectionWrapper;
                while (y != null)
                {
                    x = y.GetInnerConnection() as System.Data.Odbc.OdbcConnection;
                    if (x == null)
                    {
                        y = y.GetInnerConnection() as IConnectionWrapper;
                    }
                    else
                        y = null;
                }

            }
            if (x == null)
                x = (System.Data.Odbc.OdbcConnection)connection;


            try
            {


                var r = x.GetSchema("DataSourceInformation");
                if (r != null)
                {
                    var s = r.Rows[0]["QuotedIdentifierPattern"].ToString();
                    ColumnNameWrapper = "";
                    if (s.ToString().Length > 0)
                        ColumnNameWrapper = s[0].ToString();
                    if (r.Rows[0][1].ToString() == "OPENEDGE")
                        ColumnNameWrapper = "";


                }

            }
            catch
            {
            }


        }
        public IDbCommand CreateCommand()
        {
            return _helper.CreateCommand();
        }


        public static bool DefaultSupportsMultipleReaders = false;
        public bool SupportsMultipleReaders { get; set; } = DefaultSupportsMultipleReaders;
        IRowsReader SQLDataProviderHelperClient.Execute(SQLDataProviderHelper.SQLSelectCommand.ExecuteReaderHelper helper, bool optimizeForFirstRows, IEnumerable<ColumnBase> selectedColumns, Firefly.Box.Data.Entity entity, bool disableCache, bool lockAllRows, string entityName)
        {
            SQLDataProviderHelper.ColumnInSelect[] columnsIndexes;

            bool isUnique = helper.IsUniqueFilter();
            var command = helper.CreateCommand(out columnsIndexes, lockAllRows, isUnique, optimizeForFirstRows);
            if (SupportsMultipleReaders)
            {
                var x = new SQLDataProviderHelper.DataReaderEntitySelectReader(_helper,
              command.ExecuteReader(),
              selectedColumns, columnsIndexes, entity, lockAllRows, command, entityName);
                command.Dispose();
                return x;
            }
            else
            {
                var x = SQLDataProviderHelper.RowsCacheDataReader.LoadAllData(_helper,
                    command.ExecuteReader(),
                    selectedColumns, columnsIndexes, entity, lockAllRows, entityName);
                command.Dispose();
                return x;
            }

        }

        IRowsSource IEntityDataProvider.ProvideRowsSource(Firefly.Box.Data.Entity entity)
        {
            return _helper.ProvideTaskRowSourceFor(entity);
        }


        bool IEntityDataProvider.Contains(Firefly.Box.Data.Entity entity)
        {
            {
                return _helper.Contains(entity);
            }
        }

        long IEntityDataProvider.CountRows(Firefly.Box.Data.Entity entity)
        {
            return _helper.CountRows(entity);
        }

        void IEntityDataProvider.Drop(Firefly.Box.Data.Entity entity)
        {
            _helper.Drop(entity);
        }

        void IEntityDataProvider.Truncate(Firefly.Box.Data.Entity entity)
        {
            throw new NotSupportedException();

        }

        Exception SQLDataProviderHelperClient.ProcessException(Firefly.Box.Data.Entity entity, Exception e)
        {
            var odbce = e as System.Data.Odbc.OdbcException;

            if (odbce != null)
            {
                foreach (var er in odbce.Errors)
                {
                    var oerr = er as System.Data.Odbc.OdbcError;
                    if (oerr != null)
                    {
                        switch (oerr.SQLState)
                        {
                            case "23000":
                                return new DatabaseErrorException(DatabaseErrorType.DuplicateIndex, e);

                        }
                    }
                }
            }
            return e;

        }

        void SQLDataProviderHelperClient.AddLockSyntaxToSqlCommand(StringBuilder commandText, IEnumerable<IJoin> joins, Func<ColumnBase, string> getColumnAlias, bool hasNonRowLockingEntitiesInJoin, IEnumerable<ColumnBase> selectedColumns, Firefly.Box.Data.Entity table)
        {

        }



        public bool AllowEmptyStringParameter()
        {
            return false;
        }
        public static bool EscapeStringsInDbParameters = false;

        EntityDataProviderFilterItemSaver SQLFilterHelper.GetNoParameterFilterItemSaver(IDateTimeCollector dtc)
        {
            return new NoParametersFilterItemSaver(false, SQLClientEntityDataProvider.DateTimeStringFormat, dtc) { EscapeString = EscapeStringsInDbParameters };
        }


        EntityDataProviderFilterItemSaver SQLDataProviderHelperClient.CreateFilterItemSaverForInsert(IDbCommand dbCommand, Firefly.Box.Data.Entity entity, ColumnBase column,
            IRowStorage storage, IDateTimeCollector dtc)
        {
            return ((SQLDataProviderHelperClient)this).CreateFilterItemSaver(dbCommand, dtc);
        }

        public string GetColumnNameForSelect(ColumnBase column, string aliasGiven)
        {
            return aliasGiven;
        }

        public IValueLoader GetDataReaderValueLoader(IDataReader reader, int columnIndexInSelect, IDateTimeCollector dtc)
        {
            return new DataReaderValueLoader(reader, columnIndexInSelect, dtc);
        }

        void SQLDataProviderHelperClient.AddLockSyntaxAfterTableDefinitionInFrom(StringBuilder commandText)
        {

        }


        EntityDataProviderFilterItemSaver SQLDataProviderHelperClient.CreateFilterItemSaver(IDbCommand command, IDateTimeCollector dtc)
        {
            if (UseParameters)
            {
                return new OleDbDataParameterSaver(command, "?", dtc);
            }
            return ((SQLDataProviderHelperClient)this).GetNoParameterFilterItemSaver(dtc);

        }

        string SQLDataProviderHelperClient.GetSelectAddition(bool onlyOneRow, Firefly.Box.Data.Entity entity, Sort sort, bool uniqueFilter, bool hasAccentCharsInNonEqualOperators, IEnumerable<ColumnBase> columns, bool optimiseForFirstRows, bool isForRelation)
        {
            return "";
        }

        bool SQLDataProviderHelperClient.ReuseParameters
        {
            get { return false; }
        }

        bool SQLDataProviderHelperClient.KeepLockDataReaderOpenUntilUnlock { get { return false; } }

        void SQLDataProviderHelperClient.ExecuteInsert(Firefly.Box.Data.Entity entity, IDbCommand insertCommand, IRowStorage storage)
        {
            insertCommand.ExecuteNonQuery();
        }

        string SQLDataProviderHelperClient.WrapColumnName(string name)
        {
            return ColumnNameWrapper + name + ColumnNameWrapper;
        }
        internal bool IsClosed()
        {
            return _helper.IsClosed();
        }
        public string GetEntityName(Firefly.Box.Data.Entity entity)
        {
            var sb = new StringBuilder();
            foreach (var item in entity.EntityName.Split('.'))
            {
                if (sb.Length > 0)
                    sb.Append('.');
                sb.Append(ColumnNameWrapper);
                sb.Append(item);
                sb.Append(ColumnNameWrapper);
            }
            return sb.ToString();
        }

        IDbCommand SQLDataProviderHelperClient.CreateCommand(IDbConnection connection)
        {
            return connection.CreateCommand();
        }

        public bool ShouldLockAllRowsInExecuteCommand(bool firstRowOnly, bool shouldBeOnlyOneRowThatMatchesTheFilter)
        {
            return false;
        }

        void SQLDataProviderHelperClient.AddJoin(Firefly.Box.Data.Entity entity, string entityAlias, IFilter joinFilter, bool outerJoin, StringBuilder fromText, StringBuilder whereText, SQLFilterConsumer sqlFilterConsumerForJoinFilter, Action<StringBuilder> callMeAfterTableAlias, Firefly.Box.Data.Entity mainSelectEntity)
        {
            AnsiJoin(entity, entityAlias, joinFilter, outerJoin, fromText, sqlFilterConsumerForJoinFilter, ((SQLDataProviderHelperClient)this).GetEntityName);
        }

        internal static void AnsiJoin(Firefly.Box.Data.Entity entity, string entityAlias, IFilter joinFilter, bool outerJoin, StringBuilder fromText, SQLFilterConsumer sqlFilterConsumerForJoinFilter, Func<Firefly.Box.Data.Entity, string> wrapName)
        {
            joinFilter.AddTo(sqlFilterConsumerForJoinFilter);
            if (sqlFilterConsumerForJoinFilter.HasFilter)
                fromText.Append(outerJoin ? " left outer join " : " inner join ");
            else
                fromText.Append(", ");
            fromText.Append(wrapName(entity));
            fromText.Append(" ");
            fromText.Append(entityAlias);
            if (sqlFilterConsumerForJoinFilter.HasFilter)
            {
                fromText.Append(" on ");
                fromText.Append(sqlFilterConsumerForJoinFilter.Result);
            }
        }



        ITransaction IEntityDataProvider.BeginTransaction()
        {
            return _helper.BeginTransaction();
        }

        bool IEntityDataProvider.SupportsTransactions
        {
            get { return true; }
        }

        public void Dispose()
        {
            _helper.Dispose();
        }

        void SQLFilterHelper.AddLessOrEqualWithWildcard(Firefly.Box.Data.TextColumn column, Text value, IFilterItem filterItem, IFilterBuilder to, string operatorPrefix)
        {
            int i = column.FormatInfo.MaxDataLength - value.TrimEnd().Length;
            value = value.ToString().PadRight(column.FormatInfo.MaxDataLength, ENV.UserMethods.Instance.Chr(255)[0]);
            to.AddWhere("{0} <= {1}", column, new val(value));
        }
        class val : IFilterItem
        {
            string _s;
            public val(string s)
            {
                _s = s;
            }
            public bool IsAColumn()
            {
                return false;
            }

            public void SaveTo(IFilterItemSaver saver)
            {
                saver.SaveAnsiString(_s, _s.Length, false);
            }
        }

        public void SaveTimePartOfDateTimeTo(Time t, IValueSaver valueSaver)
        {
            throw new NotImplementedException();
        }

        public string GetTimeExtractionSyntaxFromDateTimeColumn()
        {
            throw new NotImplementedException();
        }
        public ICustomLock UseCustomRowLocking(StringBuilder commandText, IFilter primaryKeyFilter, IDbCommand c, Firefly.Box.Data.Entity e)
        {
            return null;
        }
    }
    public class SQLClientEntityDataProvider : IEntityDataProvider, SQLDataProviderHelperClient
    {
        internal static string TranslateParameterTypeToSQLType(IDbDataParameter p)
        {
            switch (p.DbType)
            {
                case DbType.Int16:
                    return "int";
                case DbType.Int32:
                    return "int";
                case DbType.Int64:
                    return "bigint";
                case DbType.Decimal:
                    return string.Format("decimal ({0},{1})", p.Precision, p.Scale);
                case DbType.DateTime:
                    return "datetime";
                case DbType.DateTime2:
                    return "datetime2";
                case DbType.Date:
                    return "date";
                case DbType.AnsiString:
                    return "varchar(" + (p.Size == -1 ? "max" : p.Size.ToString()) + ")";
                case DbType.String:
                    return "nvarchar(" + (p.Size == -1 ? "max" : p.Size.ToString()) + ")";
                case DbType.Binary:
                    if (p.Value == DBNull.Value)
                        return "varbinary(8000)";
                    return "varbinary(" + (((byte[])p.Value).Length > 8000 ? ((byte[])p.Value).Length.ToString() : "8000") + ")";
                default:
                    return "varchar(max)";
            }
        }
        public static bool DoNotUseBoundParametersForDynamicWhere { get; set; }
        public static bool SuppressLocking { get; set; }
        public static bool UseOptimisticLocking { get; set; }
        SQLDataProviderHelper _helper;
        public bool RequiresTransactionForLocking
        {
            get
            {
                return !UseCursorLocking;
            }
        }
        public bool UseParameters { get; set; }

        public interface IEntityAditionalInfoForSQL
        {
            CursorOptions Cursor { get; }
        }
        /// <summary>
        /// in ms
        /// </summary>
        public static int DefaultLockTimeout = 500;
        public static bool DefaultUseCursorLocking = false;

        bool _useCursorLocking = DefaultUseCursorLocking;
        public bool UseCursorLocking
        {
            get { return _useCursorLocking; }
            set
            {
                _useCursorLocking = value;
                using (var c = CreateCommand())
                {
                    c.CommandText = "SET LOCK_TIMEOUT " + DefaultLockTimeout;
                    c.ExecuteNonQuery();
                }

            }
        }
        public void AddToCommandAfterOrderBy(StringBuilder commandText, bool firstRowOnly, Firefly.Box.Data.Entity entity, Sort sort, bool isFilterUnique, bool hasAccentCharsInNonEqualOperators, bool optimizeForFirstRows, bool fromRelation)
        {

        }
        public ICustomLock UseCustomRowLocking(StringBuilder commandText, IFilter primaryKeyFilter, IDbCommand c, Firefly.Box.Data.Entity e)
        {
            if (!UseCursorLocking)
                return null;
            else
            {
                _helper.AppendWhere(commandText, primaryKeyFilter, c, e, false);
                c.CommandText = commandText.ToString();
                return new CursorLock(c, this);
            }
        }
        public bool ShouldLockAllRowsInExecuteCommand(bool firstRowOnly, bool uniqueByPrimaryKey)
        {
            if (UseCursorLocking)
                return false;
            return uniqueByPrimaryKey;
        }
        class LockOnReadRowsReader : IRowsReader
        {
            IRowsReader _orig;

            public LockOnReadRowsReader(IRowsReader orig)
            {
                _orig = orig;
            }

            public void Dispose()
            {
                _orig.Dispose();
            }

            public IRow GetJoinedRow(Firefly.Box.Data.Entity e, IRowStorage c)
            {
                return _orig.GetJoinedRow(e, c);
            }
            IRow _lastRow;
            public IRow GetRow(IRowStorage c)
            {
                _lastRow = _orig.GetRow(c);
                _lastRow.Lock();
                return _lastRow;
            }

            public bool Read()
            {
                if (_lastRow != null)
                    _lastRow.Unlock();
                return _orig.Read();
            }
        }
        IRowsReader SQLDataProviderHelperClient.ProvideAlternateExecuteCommandWithLock(bool firstRowOnly, bool uniqueByPrimaryKey, bool lockAllrows, SQLDataProviderHelper.SQLSelectCommand.AlternateExecuteCommandHelper helper)
        {
            if (SuppressLocking)
                return null;
            if (UseCursorLocking && (uniqueByPrimaryKey))
            {
                SQLDataProviderHelper.ColumnInSelect[] columnsIndexes;


                IDbCommand c = helper.CreateCommand(out columnsIndexes);
                var cursor = new CursorLock(c, this);
                IRowsReader result = new NoRows();
                cursor.LockTheRow(r =>
                {
                    result = helper.LoadRow(r, columnsIndexes, cursor.Unlock, true);
                });
                return result;
            }
            if (firstRowOnly && !uniqueByPrimaryKey)
            {
                SQLDataProviderHelper.ColumnInSelect[] columnsIndexes;
                var c = helper.CreateCommand(out columnsIndexes);
                IRowsReader result = new NoRows();
                using (var r = c.ExecuteReader())
                {
                    if (r.Read())
                        result = new LockOnReadRowsReader(helper.LoadRow(r, columnsIndexes, delegate { }, false));
                }
                return result;

            }
            return null;
        }

        internal bool TrackCursorsForTesting = false;
        internal List<object> _trackedCursors = new List<object>();


        class CursorLock : ICustomLock
        {
            IDbCommand c;
            SQLClientEntityDataProvider _parent;



            public CursorLock(IDbCommand c, SQLClientEntityDataProvider parent)
            {

                this.c = c;
                _parent = parent;

            }
            Action _unlock = () => { };

            public void LockTheRow(Action<IDataReader> andCallMeToReadTheValues)
            {
                var origString = c.CommandText;
                var origType = c.CommandType;
                c.CommandText += " /* using Cursor Locks */";
                var builder = new ServerSideCursorBuilder(c, false, false, true);
                builder.ccopt.Value = 0x00002;

                using (var r = c.ExecuteReader())
                {
                    r.Read();
                }
                if (_parent.TrackCursorsForTesting)
                    _parent._trackedCursors.Add(this);
                var cursorHandle = (int)builder.cursor.Value;

                c.CommandText = origString;
                c.CommandType = origType;
                builder.RemoveParametersFromCommand();
                using (var r = c.ExecuteReader())// second query to get an up to date data - the cursor sometimes return stale data
                {
                    if (r.Read())
                        andCallMeToReadTheValues(r);
                }

                _unlock = () =>
                {
                    if (_parent.TrackCursorsForTesting)
                        _parent._trackedCursors.Remove(this);
                    using (var c2 = _parent.CreateCommand())
                    {
                        var cursorForFetch = c2.CreateParameter();
                        cursorForFetch.DbType = DbType.Int32;
                        cursorForFetch.ParameterName = "@cursor";
                        cursorForFetch.Value = cursorHandle;
                        c2.Parameters.Add(cursorForFetch);
                        c2.CommandType = CommandType.StoredProcedure;
                        c2.CommandText = "sp_cursorclose";
                        c2.ExecuteNonQuery();
                    }
                };



            }

            public void Unlock()
            {
                _unlock();
            }
        }

        public class CursorOptions
        {

            /// <summary>
            /// Disables the usage of dynamic cursor and forces the use of keyset-driven cursor.
            /// see: https://technet.microsoft.com/en-us/library/ms188644(v=sql.105).aspx
            /// </summary>
            public bool DisableFirstRowOptimization { get; set; }
            /// <summary>
            /// Determines that if the First Row Optimized cursor (dynamic cursor) fails, use static cursor instead of the default which is keyset-driven cursor.
            /// see: https://technet.microsoft.com/en-us/library/ms188644(v=sql.105).aspx
            /// </summary>
            public bool UseStaticCursorAsFallbackCursor { get; set; }
            internal bool PagedCommandInsteadOfCursor { get; set; }
            internal int PagedCommandInsteadOfCursorRowsPerPage { get; set; }
        }

        public SQLClientEntityDataProvider(IDbConnection connection)
        {
            _helper = new SQLDataProviderHelper(connection, this);

            UseParameters = true;
        }

        public string GetColumnNameForSelect(ColumnBase column, string aliasGiven)
        {
            return aliasGiven;
        }

        public Exception ProcessException(Exception ex, Firefly.Box.Data.Entity e, IDbCommand c)
        {
            return _helper.ProcessException(ex, e, c);
        }

        string SQLDataProviderHelperClient.WrapColumnName(string name)
        {
            return name;
        }

        public string GetEntityName(Firefly.Box.Data.Entity entity)
        {
            var x = entity.EntityName;
            var pos = x.LastIndexOf('\\');
            return entity.EntityName.Substring(pos+1);
        }

        IDbCommand SQLDataProviderHelperClient.CreateCommand(IDbConnection connection)
        {
            return connection.CreateCommand();
        }



        public IDbCommand CreateCommand()
        {
            return _helper.CreateCommand();
        }
        public SqlCommandFilterBuilder CreateSqlCommandFilterBuilder(IDbCommand command, Firefly.Box.Data.Entity entity)
        {
            return _helper.CreateSqlCommandFilterBuilder(command, entity);
        }
        public static bool DisableAutoFetch { get; set; } = true;//See W9972. Internal firefly cursor thoughts docs at: c:\dotnet2\WizardOfOz\TestFirefly.Box\DataBase\CursorInvestigation
        IRowsReader SQLDataProviderHelperClient.Execute(SQLDataProviderHelper.SQLSelectCommand.ExecuteReaderHelper helper, bool optimizeForFirstRows, IEnumerable<ColumnBase> selectedColumns, Firefly.Box.Data.Entity entity, bool disableCache, bool lockAllRows, string entityName)
        {

            SQLDataProviderHelper.ColumnInSelect[] columnsIndexes;

            bool isUnique = helper.IsUniqueFilter();
            if (lockAllRows)
            {
                lockAllRows = isUnique;
            }
            if (lockAllRows)
            {
                if (UseCursorLocking)
                {

                    var com = helper.CreateCommand(out columnsIndexes, false, true, false);
                    var c = new CursorLock(com, this);
                    IRowsReader result = new NoRows();
                    c.LockTheRow(r =>
                    {
                        result = helper.LoadRow(r, columnsIndexes, c.Unlock);
                    });
                    com.Dispose();
                    return result;
                }



            }
            var sql = entity as SqlEntity;
            if (sql != null)
            {
                if (sql.Cursor.PagedCommandInsteadOfCursor && !isUnique)
                {

                    return helper.PagedReader(lockAllRows, optimizeForFirstRows, sql.Cursor.PagedCommandInsteadOfCursorRowsPerPage, (x, r) => "select top " + r + " " + x.Substring(6));
                }
            }
            var command = helper.CreateCommand(out columnsIndexes, lockAllRows, isUnique, optimizeForFirstRows);
            if (!isUnique)
                return new ServerSideCursorEntitySelectReader(command, optimizeForFirstRows, _helper, selectedColumns, columnsIndexes, entity, disableCache, lockAllRows, entityName, this);
            else
            {
                var x = SQLDataProviderHelper.RowsCacheDataReader.LoadAllData(_helper,
                    command.ExecuteReader(),
                    selectedColumns, columnsIndexes, entity, lockAllRows, entityName);
                command.Dispose();
                return x;
            }

        }

        IRowsSource IEntityDataProvider.ProvideRowsSource(Firefly.Box.Data.Entity entity)
        {
            return _helper.ProvideTaskRowSourceFor(entity);
        }
        public IsolationLevel IsolationLevel
        {
            set
            {
                _helper.IsolationLevel = value;
                if (value == System.Data.IsolationLevel.ReadUncommitted)
                {
                    using (var c = CreateCommand())
                    {
                        c.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED";
                        c.ExecuteNonQuery();
                    }
                }
            }
            get
            {
                return _helper.IsolationLevel;
            }
        }

        bool IEntityDataProvider.Contains(Firefly.Box.Data.Entity entity)
        {
            using (var c = CreateCommand())
            {
                c.CommandText = "select object_id(@p1) ";//  Select count (*) From sysobjects Where name='" +
                //   entity.EntityName.ToUpper().ToString().Replace("'", "''") + "'";
                var p = c.CreateParameter();
                p.ParameterName = "@p1";
                var s = GetEntityName(entity);
                if (s.StartsWith("#"))
                    s = "tempdb.." + s;
                if (s.StartsWith("dbo.#"))
                    s = "tempdb.." + s.Substring(4);
                p.Value = s;
                p.Size = 256;
                p.DbType = DbType.AnsiString;
                c.Parameters.Add(p);
                var i = c.ExecuteScalar();
                return i != System.DBNull.Value;
            }
        }
        long IEntityDataProvider.CountRows(Firefly.Box.Data.Entity entity)
        {
            return _helper.CountRows(entity);
        }

        void IEntityDataProvider.Drop(Firefly.Box.Data.Entity entity)
        {
            _helper.Drop(entity);
        }

        void IEntityDataProvider.Truncate(Firefly.Box.Data.Entity entity)
        {
            _helper.Truncate(entity);
        }

        Exception SQLDataProviderHelperClient.ProcessException(Firefly.Box.Data.Entity entity, Exception e)
        {
            var ex = e as System.Data.SqlClient.SqlException;
            if (ex != null)
            {
                if (ex.Number == 2601)
                    return new DatabaseErrorException(DatabaseErrorType.DuplicateIndex, e);
                if (ex.Number == 547 || ex.Number == 2627 || ex.Number == 515)
                    return new DatabaseErrorException(DatabaseErrorType.ConstraintFailed, e);
                if (ex.Number == 1222)
                    throw new DatabaseErrorException(DatabaseErrorType.LockedRow, e);
                if (ex.Number == 1205)
                    throw new DatabaseErrorException(DatabaseErrorType.Deadlock, e);
            }
            return e;
        }

        void SQLDataProviderHelperClient.AddLockSyntaxToSqlCommand(StringBuilder commandText, IEnumerable<IJoin> joins, Func<ColumnBase, string> getColumnAlias, bool hasNonRowLockingEntitiesInJoin, IEnumerable<ColumnBase> selectedColumns, Firefly.Box.Data.Entity table)
        {

        }



        public bool AllowEmptyStringParameter()
        {
            return true;
        }

        internal interface SQLServerEntityDataProviderFilterItemSaver : EntityDataProviderFilterItemSaver
        {
            void SaveNewGUIDOnInsert(string value);
        }

        EntityDataProviderFilterItemSaver SQLFilterHelper.GetNoParameterFilterItemSaver(IDateTimeCollector dtc)
        {
            return new NoParametersFilterItemSaver(true, DateTimeStringFormat, dtc);

        }

        EntityDataProviderFilterItemSaver SQLDataProviderHelperClient.CreateFilterItemSaverForInsert(IDbCommand dbCommand, Firefly.Box.Data.Entity entity, ColumnBase column,
           IRowStorage storage, IDateTimeCollector dtc)
        {
            return new SQLServerEntityDataProviderFilterItemSaverCore(((SQLDataProviderHelperClient)this).CreateFilterItemSaver(dbCommand, dtc), column, storage, entity);
        }



        class SQLServerEntityDataProviderFilterItemSaverCore : SQLServerEntityDataProviderFilterItemSaver, Storage.CanForceDateTime
        {
            EntityDataProviderFilterItemSaver _orig;
            ColumnBase _column;
            IRowStorage _storage;
            Firefly.Box.Data.Entity _entity;

            public SQLServerEntityDataProviderFilterItemSaverCore(EntityDataProviderFilterItemSaver orig, ColumnBase column, IRowStorage storage, Firefly.Box.Data.Entity entity)
            {
                _orig = orig;
                _column = column;
                _storage = storage;
                _entity = entity;
            }

            public void SaveInt(int value)
            {
                _orig.SaveInt(value);
            }

            public void SaveDecimal(decimal value, byte precision, byte scale)
            {
                _orig.SaveDecimal(value, precision, scale);
            }

            public void SaveString(string value, int length, bool fixedWidth)
            {
                _orig.SaveString(value, length, fixedWidth);
            }

            public void SaveAnsiString(string value, int length, bool fixedWidth)
            {
                _orig.SaveAnsiString(value, length, fixedWidth);
            }

            public void SaveNull()
            {
                _orig.SaveNull();
            }

            public void SaveDateTime(DateTime value)
            {
                _orig.SaveDateTime(value);
            }

            public void SaveTimeSpan(TimeSpan value)
            {
                _orig.SaveTimeSpan(value);
            }

            public void SaveBoolean(bool value)
            {
                _orig.SaveBoolean(value);
            }

            public void SaveByteArray(byte[] value)
            {
                _orig.SaveByteArray(value);
            }

            public string TextForCommand
            {
                get { return _orig.TextForCommand; }
            }

            public void SaveNewGUIDOnInsert(string value)
            {
                var x = "{" + Guid.NewGuid().ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
                SaveAnsiString(x, x.Length, false);
                _storage.SetValue(_column, new myIValue(x));
            }

            public void SaveEmptyDateTime()
            {
                _orig.SaveEmptyDateTime();
            }

            public void ForceDateTime2()
            {
                ((CanForceDateTime)_orig).ForceDateTime2();
            }

            class myIValue : IValueLoader
            {
                string _x;

                public myIValue(string x)
                {
                    _x = x;
                }

                public bool IsNull()
                {
                    throw new NotImplementedException();
                }

                public Number GetNumber()
                {
                    throw new NotImplementedException();
                }

                public string GetString()
                {
                    return _x;
                }

                public DateTime GetDateTime()
                {
                    throw new NotImplementedException();
                }

                public TimeSpan GetTimeSpan()
                {
                    throw new NotImplementedException();
                }

                public bool GetBoolean()
                {
                    throw new NotImplementedException();
                }

                public byte[] GetByteArray()
                {
                    throw new NotImplementedException();
                }
            }
        }

        public IValueLoader GetDataReaderValueLoader(IDataReader reader, int columnIndexInSelect, IDateTimeCollector dtc)
        {
            return new DataReaderValueLoader(reader, columnIndexInSelect, dtc);
        }

        void SQLDataProviderHelperClient.AddLockSyntaxAfterTableDefinitionInFrom(StringBuilder commandText)
        {
            if (SuppressLocking)
                return;
            if (UseCursorLocking)
                throw new InvalidOperationException();
            commandText.Append(" with (UPDLOCK,NOWAIT) ");
        }

        class DecimalIdentityValue : IValueLoader
        {
            decimal _value;

            public DecimalIdentityValue(decimal value)
            {
                _value = value;
            }

            public bool IsNull()
            {
                return false;
            }

            public Number GetNumber()
            {
                return _value;
            }

            public string GetString()
            {
                throw new NotImplementedException();
            }

            public DateTime GetDateTime()
            {
                throw new NotImplementedException();
            }

            public TimeSpan GetTimeSpan()
            {
                throw new NotImplementedException();
            }

            public bool GetBoolean()
            {
                throw new NotImplementedException();
            }

            public byte[] GetByteArray()
            {
                throw new NotImplementedException();
            }
        }

        public const string DateTimeStringFormat = "'{0}'";
        EntityDataProviderFilterItemSaver SQLDataProviderHelperClient.CreateFilterItemSaver(IDbCommand command, IDateTimeCollector dtc)
        {

            if (UseParameters)
            {
                return new MssqlDbDataParameterFilterItemSaver(command, dtc);

            }

            return ((SQLDataProviderHelperClient)this).GetNoParameterFilterItemSaver(dtc);
        }
        class MssqlDbDataParameterFilterItemSaver : DbDataParameterFilterItemSaver, Storage.CanForceDateTime
        {
            public MssqlDbDataParameterFilterItemSaver(IDbCommand command, IDateTimeCollector dtc)
                : base(command, "@", true, dtc)
            {
            }
            public override void SaveString(string value, int length, bool fixedWidth)
            {
                if (length > 4000)
                    base.SaveString(value, -1, fixedWidth);
                else
                    base.SaveString(value, length, fixedWidth);
            }
            public override void SaveAnsiString(string value, int length, bool fixedWidth)
            {
                if (length > 4000)
                    base.SaveAnsiString(value, -1, fixedWidth);
                else
                    base.SaveAnsiString(value, length, fixedWidth);
            }
            protected override DbType GetDateTimeDbType(DateTime value)
            {
                if (value.Year < 1800 || _forceDatetime2||value.Ticks%10000!=0)
                    return DbType.DateTime2;
                return DbType.DateTime;
            }
            bool _forceDatetime2;
            public void ForceDateTime2()
            {
                _forceDatetime2 = true;
            }
        }


        string SQLDataProviderHelperClient.GetSelectAddition(bool onlyOneRow, Firefly.Box.Data.Entity entity, Sort sort, bool uniqueFilter, bool hasAccentCharsInNonEqualOperators, IEnumerable<ColumnBase> columns, bool optimiseForFirstRows, bool isForRelation)
        {
            if (onlyOneRow && !uniqueFilter && !hasAccentCharsInNonEqualOperators)
            {
                foreach (var item in columns)
                {
                    if (item.Name.Trim().StartsWith("top ", StringComparison.InvariantCultureIgnoreCase))
                        return string.Empty;
                    break;
                }
                return "top 1 ";
            }
            return string.Empty;
        }

        bool SQLDataProviderHelperClient.ReuseParameters
        {
            get { return true; }
        }

        bool SQLDataProviderHelperClient.KeepLockDataReaderOpenUntilUnlock { get { return false; } }

        void SQLDataProviderHelperClient.ExecuteInsert(Firefly.Box.Data.Entity entity, IDbCommand insertCommand, IRowStorage storage)
        {
            if (!(entity.IdentityColumn is Firefly.Box.Data.Advanced.TypedColumnBase<Number>))
                insertCommand.ExecuteNonQuery();
            else
            {
                insertCommand.CommandText += ";select SCOPE_IDENTITY()";
                decimal identity = 0;

                object result = 0;
                using (var r = insertCommand.ExecuteReader())
                {

                    do
                    {
                        while (r.Read())
                        {
                            result = r[0];
                        }
                    }
                    while (r.NextResult());//sql triggers can add more results to the insert statement -and we need to find and read the last one

                }
                try
                {
                    identity = (decimal)result;
                }
                catch { }
                storage.SetValue(entity.IdentityColumn, new DecimalIdentityValue(identity));

            }
        }
        internal delegate string GetEntityNameForJoinsDelegate(Firefly.Box.Data.Entity joinEntity, Firefly.Box.Data.Entity mainSelectEntity, string entityName);
        internal static GetEntityNameForJoinsDelegate _getEntityNameForJoins = (a, b, c) => c;
        void SQLDataProviderHelperClient.AddJoin(Firefly.Box.Data.Entity entity, string entityAlias, IFilter joinFilter, bool outerJoin, StringBuilder fromText, StringBuilder whereText, SQLFilterConsumer sqlFilterConsumerForJoinFilter, Action<StringBuilder> callMeAfterTableAlias, Firefly.Box.Data.Entity mainSelectEntity)
        {
            AnsiJoin(_getEntityNameForJoins(entity, mainSelectEntity, GetEntityName(entity)), entityAlias, joinFilter, outerJoin, fromText, sqlFilterConsumerForJoinFilter, callMeAfterTableAlias);
        }

        internal static void AnsiJoin(string entityName, string entityAlias, IFilter joinFilter, bool outerJoin, StringBuilder fromText, SQLFilterConsumer sqlFilterConsumerForJoinFilter, Action<StringBuilder> callMeAfterTableAlias)
        {
            joinFilter.AddTo(sqlFilterConsumerForJoinFilter);
            if (sqlFilterConsumerForJoinFilter.HasFilter)
                fromText.Append(outerJoin ? " " + SQLDataProviderHelper.NewLineInSQL + "  left outer join " : " " + SQLDataProviderHelper.NewLineInSQL + "  inner join ");
            else
                fromText.Append(", " + SQLDataProviderHelper.NewLineInSQL + "  ");
            fromText.Append(entityName);
            fromText.Append(" ");
            fromText.Append(entityAlias);
            callMeAfterTableAlias(fromText);
            if (sqlFilterConsumerForJoinFilter.HasFilter)
            {
                fromText.Append(" on ");
                fromText.Append(sqlFilterConsumerForJoinFilter.Result);
            }
        }
        class ServerSideCursorBuilder
        {
            public IDataParameter handle, ccopt, stmt, paramdef, cursor, scrollopt, rowcount;
            public int baseCursorType, cursorType;
            internal void RemoveParametersFromCommand()
            {
                foreach (var p in new[] { handle, ccopt, stmt, paramdef, cursor, scrollopt, rowcount })
                {
                    _command.Parameters.Remove(p);
                }
            }
            IDbCommand _command;

            public ServerSideCursorBuilder(IDbCommand c, bool optimizeForFirstRows, bool useStatusCursorAsFallbackCursor, bool autoFetch)
            {
                _command = c;
                handle = c.CreateParameter();
                ccopt = c.CreateParameter();
                stmt = c.CreateParameter();
                paramdef = c.CreateParameter();
                cursor = c.CreateParameter();
                scrollopt = c.CreateParameter();
                rowcount = c.CreateParameter();
                handle.ParameterName = "@handle";
                handle.DbType = DbType.Int32;
                handle.Direction = System.Data.ParameterDirection.Output;

                cursor.ParameterName = "@cursor";
                cursor.DbType = DbType.Int32;
                cursor.Direction = System.Data.ParameterDirection.Output;
                cursor.Value = null;
                scrollopt.ParameterName = "@scrollopt";
                scrollopt.DbType = DbType.Int32;
                bool hasParameters = c.Parameters.Count > 0;



                //https://technet.microsoft.com/en-us/library/ms188644(v=sql.105).aspx
                //https://msdn.microsoft.com/en-us/library/ff848775.aspx
                //http://jtds.sourceforge.net/apiCursors.html


                baseCursorType = hasParameters ? 0x1000 : 0;
                if (autoFetch)
                    baseCursorType += 0x2000;
                cursorType = baseCursorType;
                if (optimizeForFirstRows)
                {
                    if (useStatusCursorAsFallbackCursor)
                        cursorType += 0x28002;//dynamic cursor
                    else
                        cursorType += 0x20002;
                }
                else
                    cursorType += 0x1;//keyset-driven cursor

                scrollopt.Value = cursorType;
                ccopt.ParameterName = "@ccopt";
                ccopt.DbType = DbType.Int32;
                ccopt.Value = 0x18001;
                rowcount.ParameterName = "@rowcount";
                rowcount.DbType = DbType.Int32;
                rowcount.Direction = System.Data.ParameterDirection.InputOutput;


                paramdef.ParameterName = "@paramdef";
                paramdef.DbType = DbType.String;
                stmt.ParameterName = "@stmt";
                stmt.DbType = DbType.String;

                string parameterDefinition = "";
                string parameterSuffix = "";
                foreach (IDbDataParameter p in c.Parameters)
                {
                    if (parameterDefinition != "")
                        parameterDefinition += ", ";
                    parameterDefinition += p.ParameterName + " ";
                    parameterDefinition += TranslateParameterTypeToSQLType(p);

                    parameterSuffix += ", " + p.ParameterName;

                }

                int i = 0;
                foreach (var par in new[]
                                        {
                                            handle,
                                            cursor,
                                            paramdef,
                                            stmt,
                                            scrollopt,
                                            ccopt,
                                            rowcount
                                        })
                {
                    c.Parameters.Insert(i++, par);
                }
                paramdef.Value = parameterDefinition == "" ? (string)null : parameterDefinition;
                stmt.Value = c.CommandText;
                c.CommandText = "sp_cursorprepexec";
                c.CommandType = CommandType.StoredProcedure;
            }


        }
#if DEBUG
        internal static int TestNumberOfServerSideFeches = 0;
#endif


        class ServerSideCursorEntitySelectReader : IRowsReader
        {
            SQLDataProviderHelper _helper;
            SQLClientEntityDataProvider _parent;
            int _cursorHandle;
            SQLDataProviderHelper.ColumnInSelect[] _columnsIndexes;
            IEnumerable<ColumnBase> _selectedColumns;

            Firefly.Box.Data.Entity _entity;
            string _entityName;

            public ServerSideCursorEntitySelectReader(IDbCommand command, bool optimizeForFirstRows, SQLDataProviderHelper helper, IEnumerable<ColumnBase> selectedColumns, SQLDataProviderHelper.ColumnInSelect[] columnsIndexes, Firefly.Box.Data.Entity entity, bool disableCache, bool queryLocksAllRows, string entityName, SQLClientEntityDataProvider parent)
            {
                _parent = parent;
                _selectedColumns = selectedColumns;
                _entity = entity;
                CursorOptions cursorOptions = new DataProvider.SQLClientEntityDataProvider.CursorOptions();
                _entityName = entityName;
                {
                    var ex = entity as IEntityAditionalInfoForSQL;
                    if (ex != null)
                    {
                        cursorOptions = ex.Cursor;
                    }
                }
                if (cursorOptions.DisableFirstRowOptimization)
                {
                    optimizeForFirstRows = false;
                    disableCache = false;
                }
                bool autoFetch = !queryLocksAllRows;
                if (DisableAutoFetch)
                    autoFetch = false;
                if (queryLocksAllRows)
                    disableCache = true;
                _numOfRowsInFetch = disableCache ? 1 : optimizeForFirstRows ? 40 : columnsIndexes.Length > 10 ? 40 : 400;


                _columnsIndexes = columnsIndexes;
                _helper = helper;
                var c = command;


                var builder = new ServerSideCursorBuilder(command, optimizeForFirstRows, cursorOptions.UseStaticCursorAsFallbackCursor, autoFetch);

                _cache = new RowsCache(_numOfRowsInFetch, _selectedColumns, _columnsIndexes, queryLocksAllRows);

                try
                {
                    Execute(autoFetch, c, builder.rowcount);
                    if (_parent.TrackCursorsForTesting)
                        _parent._trackedCursors.Add(this);

                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    if (e.Number == 16955)
                    {

                        builder.cursorType = builder.baseCursorType;
                        if (cursorOptions.UseStaticCursorAsFallbackCursor)
                            builder.cursorType += 0x88008;//static cursor
                        else
                        {
                            builder.cursorType += 0x1;//keyset driven cursor
                            builder.scrollopt.Value = 8193;
                        }

                        builder.scrollopt.Value = builder.cursorType;
                        Execute(autoFetch, c, builder.rowcount);
                        if (_parent.TrackCursorsForTesting)
                            _parent._trackedCursors.Add(this);
                    }
                    else
                        throw;
                }
                if (builder.cursor.Value != null)
                    _cursorHandle = (int)builder.cursor.Value;





            }

            void Execute(bool autoFetch, IDbCommand c, IDataParameter rowcount)
            {
                if (autoFetch)
                {
                    rowcount.Value = _numOfRowsInFetch;
                    using (var r = c.ExecuteReader())
                        ReadFromCursor(r);
                }
                else
                {
                    c.ExecuteNonQuery();
                    if (rowcount.Value != null)
                        _totalRows = (int)rowcount.Value;
                    if (_totalRows == 0)
                        _noMoreRows = true;
                    else
                        if (_totalRows < _numOfRowsInFetch && _totalRows > 0)
                        _numOfRowsInFetch = (int)_totalRows;
                }
            }

            RowsCache _cache;

            IDbCommand CreateCommand()
            {
                var result = _helper.CreateCommand();
                var cursorForFetch = result.CreateParameter();
                cursorForFetch.DbType = DbType.Int32;
                cursorForFetch.ParameterName = CursorParameterName;
                cursorForFetch.Value = _cursorHandle;
                result.Parameters.Add(cursorForFetch);
                return result;
            }


            public void Dispose()
            {
                if (_parent.TrackCursorsForTesting)
                    _parent._trackedCursors.Remove(this);
                using (var c = CreateCommand())
                {
                    c.CommandText = "sp_cursorclose";
                    c.CommandType = CommandType.StoredProcedure;
                    c.ExecuteNonQuery();
                }
            }

            int _numOfRowsInFetch;
            const int NumOfRowsInFetch = 40;
            const string CursorParameterName = "@cursor";
            bool _noMoreRows;
            int _totalRows;
            int _fetchedRows = 0;


            public bool Read()
            {
                if (_cache.Read())
                    return true;
                else
                {
                    if (_noMoreRows)
                        return false;

                    using (var c = CreateCommand())
                    {

                        try
                        {
                            var fetchtype = c.CreateParameter();
                            fetchtype.DbType = DbType.Int32;
                            fetchtype.Value = 2;
                            fetchtype.ParameterName = "@fetchtype";
                            c.Parameters.Add(fetchtype);

                            var rownum = c.CreateParameter();
                            rownum.ParameterName = "@rownum";
                            rownum.DbType = DbType.Int32;
                            rownum.Value = 1;
                            c.Parameters.Add(rownum);

                            var nrows = c.CreateParameter();
                            nrows.ParameterName = "@nrows";
                            nrows.DbType = DbType.Int32;
                            nrows.Value = _numOfRowsInFetch;


                            c.Parameters.Add(nrows);

#if DEBUG
                            TestNumberOfServerSideFeches++;
#endif

                            c.CommandText = "sp_cursorfetch";
                            c.CommandType = CommandType.StoredProcedure;

                            using (var reader = c.ExecuteReader())
                            {
                                ReadFromCursor(reader);
                                if (_cache.Read())
                                    return true;
                                return false;

                            }
                        }
                        catch (Exception e)
                        {
                            throw _helper.ProcessException(e, _entity, c);
                        }
                    }
                }
            }

            private void ReadFromCursor(IDataReader reader)
            {
                int fetched = 0;

                while (reader.Read())
                {
                    _cache.AddRow(reader, _entityName, _parent);

                    fetched++;
                    _fetchedRows++;
                }
                if (fetched < _numOfRowsInFetch)
                    _noMoreRows = true;
                if (_totalRows > 0 && _fetchedRows == _totalRows)
                    _noMoreRows = true;
            }

            public IRow GetRow(IRowStorage c)
            {
                return _cache.GetRow(c, _entity, _helper);
            }

            public IRow GetJoinedRow(Firefly.Box.Data.Entity e, IRowStorage c)
            {
                return _cache.GetJoinedRow(e, c, _helper);
            }
        }

        ITransaction IEntityDataProvider.BeginTransaction()
        {
            return _helper.BeginTransaction();
        }

        bool IEntityDataProvider.SupportsTransactions
        {
            get { return true; }
        }

        public void Dispose()
        {
            if (TrackCursorsForTesting)
                _trackedCursors.ToArray().ShouldBeArray(new object[0]);
            _helper.Dispose();
        }

        void SQLFilterHelper.AddLessOrEqualWithWildcard(Firefly.Box.Data.TextColumn column, Text value, IFilterItem filterItem, IFilterBuilder to, string operatorPrefix)
        {
            int i = column.FormatInfo.MaxDataLength - value.TrimEnd().Length;
            to.AddWhere("{0} <= {1}+replicate (char(255)," + i + ")", column, filterItem);
        }

        void SQLFilterHelper.SaveTimePartOfDateTimeTo(Time t, IValueSaver valueSaver)
        {
            valueSaver.SaveAnsiString(t.ToString("HH:MM:SS") + ":000", 12, false);
        }

        string SQLFilterHelper.GetTimeExtractionSyntaxFromDateTimeColumn()
        {
            return "CONVERT (CHAR, {0}, 114)";
        }

        internal bool IsClosed()
        {
            return _helper.IsClosed();
        }
    }

    public class OracleClientEntityDataProvider : IEntityDataProvider, SQLDataProviderHelperClient
    {
        SQLDataProviderHelper _helper;
        public const string DateTimeStringFormat = "to_date('{0}','yyyy-MM-dd HH24:mi:ss')";
        public const string DateTimeStringFormatForToString = "yyyy-MM-dd HH:mm:ss";

        public bool RequiresTransactionForLocking
        {
            get
            {
                return true;
            }
        }
        public SqlCommandFilterBuilder CreateSqlCommandFilterBuilder(IDbCommand command, Firefly.Box.Data.Entity entity)
        {
            return _helper.CreateSqlCommandFilterBuilder(command, entity);
        }
        public OracleClientEntityDataProvider(IDbConnection connection)
        {
            _helper = new SQLDataProviderHelper(connection, this);
            UseNamedParameters = true;
            ParameterNamePrefix = ":p";
            _parameterFactory = (command, dtc) =>
            {
                if (_useParameters)
                {

                    return new OracleParameterFilterItemSaver(command, ParameterNamePrefix, UseNamedParameters, dtc);
                }
                else
                    return ((SQLDataProviderHelperClient)this).GetNoParameterFilterItemSaver(dtc);
            };
        }
        public ICustomLock UseCustomRowLocking(StringBuilder commandText, IFilter primaryKeyFilter, IDbCommand c, Firefly.Box.Data.Entity e)
        {
            return null;
        }

        IRowsReader SQLDataProviderHelperClient.ProvideAlternateExecuteCommandWithLock(bool firstRowOnly, bool shouldBeOnlyOneRowThatMatchesTheFilter, bool lockAllrows, SQLDataProviderHelper.SQLSelectCommand.AlternateExecuteCommandHelper helper)
        {
            return null;
        }
        internal interface IRowIdColumn { }
        public string GetColumnNameForSelect(ColumnBase column, string aliasGiven)
        {
            if (column is IRowIdColumn)
                return "rowidtochar(" + aliasGiven + ")";
            else return aliasGiven;
        }
        public Exception ProcessException(Exception ex, Firefly.Box.Data.Entity e, IDbCommand c)
        {
            return _helper.ProcessException(ex, e, c);
        }
        class OracleParameterFilterItemSaver : DbDataParameterFilterItemSaver
        {
            string _prefix;
            bool _useNames;
            public OracleParameterFilterItemSaver(IDbCommand command, string prefix, bool useName, IDateTimeCollector dtc)
                : base(command, prefix, false, dtc)
            {
                _prefix = prefix;
                _useNames = useName;
            }
            protected override void SaveDecimalNumber(IDbDataParameter x, decimal value, byte precision, byte scale)
            {
                x.DbType = DbType.Decimal;
                x.Value = value;
            }
        }
        string SQLDataProviderHelperClient.WrapColumnName(string name)
        {
            return name;
        }

        public string GetEntityName(Firefly.Box.Data.Entity entity)
        {
            return entity.EntityName;
        }

        internal bool IsClosed()
        {
            return _helper.IsClosed();
        }
        IDbCommand SQLDataProviderHelperClient.CreateCommand(IDbConnection connection)
        {
            return connection.CreateCommand();
        }

        public bool ShouldLockAllRowsInExecuteCommand(bool firstRowOnly, bool shouldBeOnlyOneRowThatMatchesTheFilter)
        {
            return true;
        }

        public IsolationLevel IsolationLevel { set { _helper.IsolationLevel = value; } get { return _helper.IsolationLevel; } }
        public IDbCommand CreateCommand()
        {
            return _helper.CreateCommand();
        }

        public IRowsSource ProvideRowsSource(Firefly.Box.Data.Entity entity)
        {
            return _helper.ProvideTaskRowSourceFor(entity);
        }

        bool IEntityDataProvider.Contains(Firefly.Box.Data.Entity entity)
        {
            try
            {

                using (var c = CreateCommand())
                {
                    string s = "*";
                    if (entity.IdentityColumn != null)
                        s = entity.IdentityColumn.Name;
                    c.CommandText = "Select " + s + " " + "From " + GetEntityName(entity);
                    using (var r = c.ExecuteReader(CommandBehavior.SchemaOnly))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("ORA-00942"))
                    return false;
                if (ex.Message.StartsWith("ORA-01446") && entity.IdentityColumn != null)
                {
                    Sort shortesIndex = null;
                    foreach (var item in entity.Indexes)
                    {
                        if (item.Unique)
                        {
                            if (shortesIndex == null || item.Segments.Count < shortesIndex.Segments.Count)
                                shortesIndex = item;
                        }
                    }

                    if (shortesIndex != null)
                    {
                        var cols = new List<ColumnBase>();
                        foreach (var item in shortesIndex.Segments)
                        {
                            cols.Add(item.Column);
                        }
                        entity.IdentityColumn = null;
                        entity.SetPrimaryKey(cols.ToArray());

                    }
                }
                return true;
            }


        }

        long IEntityDataProvider.CountRows(Firefly.Box.Data.Entity entity)
        {
            return _helper.CountRows(entity);
        }

        void IEntityDataProvider.Drop(Firefly.Box.Data.Entity entity)
        {
            _helper.Drop(entity);
        }

        void IEntityDataProvider.Truncate(Firefly.Box.Data.Entity entity)
        {
            _helper.Truncate(entity);
        }

        Exception SQLDataProviderHelperClient.ProcessException(Firefly.Box.Data.Entity entity, Exception e)
        {
            if (e.Message.StartsWith("ORA-00001"))
                throw new DatabaseErrorException(DatabaseErrorType.DuplicateIndex, e);
            if (e.Message.StartsWith("ORA-00054") || e.Message.StartsWith("ORA-30006"))
                throw new DatabaseErrorException(DatabaseErrorType.LockedRow, e);
            if (e.Message.StartsWith("ORA-00060"))
                throw new DatabaseErrorException(DatabaseErrorType.Deadlock, e);
            if (e.Message.StartsWith("ORA-01400") || e.Message.StartsWith("ORA-02292"))
                throw new DatabaseErrorException(DatabaseErrorType.ConstraintFailed, e);
            if (e.Message.StartsWith("ORA-01403"))
                throw new DatabaseErrorException(DatabaseErrorType.RowWasChangedSinceLoaded, e, DatabaseErrorHandlingStrategy.Rollback);
            if (e.Message.StartsWith("ORA-02091"))
                throw new DatabaseErrorException(DatabaseErrorType.TransactionRolledBack, e);
            if (e.Message.StartsWith("ORA-01410") || e.Message.StartsWith("ORA-08103"))
                throw new DatabaseErrorException(DatabaseErrorType.AllErrors, e);
            if (e.Message.StartsWith("ORA-03135") || e.Message.Contains("ORA-12571") || e.Message.Contains("ORA-12570"))
                _helper.KillConnection();

            return e;
        }

        static int _lockWaitTimeout = 0;
        static string _waitSyntax = "nowait";

        public static int LockWaitTimeout
        {
            get
            {
                return _lockWaitTimeout;
            }
            set
            {
                _lockWaitTimeout = value;
                if (_lockWaitTimeout == 0)
                    _waitSyntax = "nowait";
                else
                {
                    _waitSyntax = "wait " + _lockWaitTimeout;
                }
            }
        }

        void SQLDataProviderHelperClient.AddLockSyntaxToSqlCommand(StringBuilder commandText, IEnumerable<IJoin> joins, Func<ColumnBase, string> getColumnAlias, bool hasNonRowLockingEntitiesInJoin, IEnumerable<ColumnBase> selectedColumns, Firefly.Box.Data.Entity table)
        {
            if (!hasNonRowLockingEntitiesInJoin)
                commandText.Append(" for update " + _waitSyntax);
            else
            {
                var lockedEntities = new HashSet<Firefly.Box.Data.Entity>();
                foreach (var @join in joins)
                {
                    if (@join.Entity.AllowRowLocking)
                    {
                        lockedEntities.Add(@join.Entity);
                    }
                }
                if (table.AllowRowLocking)
                    lockedEntities.Add(table);


                commandText.Append(" for update of ");
                bool first = true;
                foreach (var c in selectedColumns)
                {
                    if (!c.DbReadOnly && lockedEntities.Contains(c.Entity))
                    {
                        if (!first)
                            commandText.Append(", ");
                        else
                        {
                            first = false;
                        }
                        commandText.Append(getColumnAlias(c));
                    }
                }
                commandText.Append(" " + _waitSyntax);
            }
        }


        public bool AllowEmptyStringParameter()
        {
            return false;
        }
        internal static Func<IDateTimeCollector, EntityDataProviderFilterItemSaver> _createNoParameterFilterItemSaver = dtc => new NoParametersFilterItemSaver(false, DateTimeStringFormat, dtc, DateTimeStringFormatForToString);
        EntityDataProviderFilterItemSaver SQLFilterHelper.GetNoParameterFilterItemSaver(IDateTimeCollector dtc)
        {
            return _createNoParameterFilterItemSaver(dtc);
        }


        public IValueLoader GetDataReaderValueLoader(IDataReader reader, int columnIndexInSelect, IDateTimeCollector dtc)
        {
            return _valueLoaderFactory(reader, columnIndexInSelect, dtc);
        }

        void SQLDataProviderHelperClient.AddLockSyntaxAfterTableDefinitionInFrom(StringBuilder commandText)
        {

        }

        IRowsReader SQLDataProviderHelperClient.Execute(SQLDataProviderHelper.SQLSelectCommand.ExecuteReaderHelper helper, bool optimizeForFirstRows, IEnumerable<ColumnBase> selectedColumns, Firefly.Box.Data.Entity entity, bool disableCache, bool lockAllRows, string entityName)
        {
            SQLDataProviderHelper.ColumnInSelect[] columnsIndexes;

            bool isUnique = helper.IsUniqueFilter();
            var command = helper.CreateCommand(out columnsIndexes, lockAllRows, isUnique, optimizeForFirstRows);
            var x = new SQLDataProviderHelper.DataReaderEntitySelectReader(_helper, command.ExecuteReader(),
                selectedColumns, columnsIndexes, entity, lockAllRows, command, entityName);
            command.Dispose();
            return x;

        }
        bool _useParameters = true;
        public bool UseParameters { get { return _useParameters; } set { _useParameters = value; } }
        public bool UseNamedParameters { get; set; }
        public string ParameterNamePrefix { get; set; }
        Func<IDbCommand, IDateTimeCollector, EntityDataProviderFilterItemSaver> _parameterFactory;
        Func<IDataReader, int, IDateTimeCollector, IValueLoader> _valueLoaderFactory = (reader, columnIndexInSelect, dtc) => new DataReaderValueLoader(reader, columnIndexInSelect, dtc);

        internal void SetFactories(Func<IDbCommand, IDateTimeCollector, EntityDataProviderFilterItemSaver> parameterFactory, Func<IDataReader, int, IDateTimeCollector, IValueLoader> dataReaderValueLoaderFactory)
        {
            _parameterFactory = parameterFactory;
            _valueLoaderFactory = dataReaderValueLoaderFactory;
        }

        EntityDataProviderFilterItemSaver SQLDataProviderHelperClient.CreateFilterItemSaver(IDbCommand command, IDateTimeCollector dtc)
        {
            return _parameterFactory(command, dtc);

        }
        EntityDataProviderFilterItemSaver SQLDataProviderHelperClient.CreateFilterItemSaverForInsert(IDbCommand dbCommand, Firefly.Box.Data.Entity entity, ColumnBase column,
           IRowStorage storage, IDateTimeCollector dtc)
        {
            return ((SQLDataProviderHelperClient)this).CreateFilterItemSaver(dbCommand, dtc);
        }

        HintProviderDelegate _hintProvider = delegate { return ""; };

        internal void SetHintProvider(HintProviderDelegate hintProvider)
        {
            _hintProvider = hintProvider;
        }
        public delegate string HintProviderDelegate(bool onlyOneRow, Firefly.Box.Data.Entity entity, Sort sort, bool uniqueFilter, IEnumerable<ColumnBase> columns, bool optimiseForFirstRows, bool isForRelation);

        string SQLDataProviderHelperClient.GetSelectAddition(bool onlyOneRow, Firefly.Box.Data.Entity entity, Sort sort, bool uniqueFilter, bool hasAccentCharsInNonEqualOperators, IEnumerable<ColumnBase> columns, bool optimiseForFirstRows, bool isForRelation)
        {
            return _hintProvider(onlyOneRow, entity, sort, uniqueFilter, columns, optimiseForFirstRows, isForRelation);
        }

        bool SQLDataProviderHelperClient.ReuseParameters
        {
            get { return UseNamedParameters; }
        }

        bool SQLDataProviderHelperClient.KeepLockDataReaderOpenUntilUnlock { get { return false; } }

        void SQLDataProviderHelperClient.ExecuteInsert(Firefly.Box.Data.Entity entity, IDbCommand insertCommand, IRowStorage storage)
        {
            if (entity.IdentityColumn == null)
                insertCommand.ExecuteNonQuery();
            else
            {
                var p = insertCommand.CreateParameter();
                insertCommand.Parameters.Add(p);
                p.Direction = ParameterDirection.Output;
                p.Size = 30;
                p.ParameterName = ":" + insertCommand.Parameters.Count;
                insertCommand.CommandText += "RETURNING rowid INTO " + p.ParameterName;//the upper case is critical due to oracle bug in turkish
                insertCommand.ExecuteNonQuery();
                storage.SetValue(entity.IdentityColumn, new rowIdValueLoader(p.Value.ToString()));
            }
        }
        class rowIdValueLoader : IValueLoader
        {
            string _rowid;

            public rowIdValueLoader(string rowid)
            {
                _rowid = rowid;
            }

            public bool IsNull()
            {
                return false;
            }

            public Number GetNumber()
            {
                throw new NotImplementedException();
            }

            public string GetString()
            {
                return _rowid;
            }

            public DateTime GetDateTime()
            {
                throw new NotImplementedException();
            }

            public TimeSpan GetTimeSpan()
            {
                throw new NotImplementedException();
            }

            public bool GetBoolean()
            {
                throw new NotImplementedException();
            }

            public byte[] GetByteArray()
            {
                throw new NotImplementedException();
            }
        }





        bool _ansiJoin = true;
        public bool AnsiJoin { get { return _ansiJoin; } set { _ansiJoin = value; } }
        void SQLDataProviderHelperClient.AddJoin(Firefly.Box.Data.Entity entity, string entityAlias, IFilter joinFilter, bool outerJoin, StringBuilder fromText, StringBuilder whereText, SQLFilterConsumer sqlFilterConsumerForJoinFilter, Action<StringBuilder> callMeAfterTableAlias, Firefly.Box.Data.Entity mainSelectEntity)
        {
            if (AnsiJoin)
                SQLClientEntityDataProvider.AnsiJoin(GetEntityName(entity), entityAlias, joinFilter, outerJoin, fromText,
                                                     sqlFilterConsumerForJoinFilter, callMeAfterTableAlias);
            else
            {
                fromText.Append(", " + SQLDataProviderHelper.NewLineInSQL + "  ");
                fromText.Append(GetEntityName(entity));
                fromText.Append(" ");
                fromText.Append(entityAlias);
                callMeAfterTableAlias(fromText);

                if (!outerJoin)
                    joinFilter.AddTo(sqlFilterConsumerForJoinFilter);
                else
                {
                    if (OuterJoinOnlyOnIsEqualToUsingPlusSyntax)
                        joinFilter.AddTo(new OuterJoinPlusOperatorOnlyOnEqualTo(sqlFilterConsumerForJoinFilter));
                    else
                    {
                        sqlFilterConsumerForJoinFilter.OperatorPrefix = "(+)";
                        joinFilter.AddTo(sqlFilterConsumerForJoinFilter);

                    }
                }
                if (sqlFilterConsumerForJoinFilter.HasFilter)
                {
                    if (whereText.Length > 0)
                    {
                        whereText.Append(" And ");
                    }
                    whereText.Append(sqlFilterConsumerForJoinFilter.Result);

                }
            }
        }
        public static bool OuterJoinOnlyOnIsEqualToUsingPlusSyntax { get; set; }
        class OuterJoinPlusOperatorOnlyOnEqualTo : IFilterBuilder
        {
            SQLFilterConsumer _originalFilter;

            public OuterJoinPlusOperatorOnlyOnEqualTo(SQLFilterConsumer originalFilter)
            {
                _originalFilter = originalFilter;
            }



            public void AddBetween(ColumnBase column, IFilterItem from, IFilterItem to)
            {
                _originalFilter.AddBetween(column, from, to);
            }

            public void AddEqualTo(ColumnBase column, IFilterItem item)
            {

                _originalFilter.OperatorPrefix = "(+)";
                try
                {
                    _originalFilter.AddEqualTo(column, item);
                }
                finally
                {
                    _originalFilter.OperatorPrefix = "";
                }
            }

            public void AddDifferentFrom(ColumnBase column, IFilterItem item)
            {
                _originalFilter.AddDifferentFrom(column, item);
            }

            public void AddStartsWith(ColumnBase column, IFilterItem item)
            {
                _originalFilter.AddStartsWith(column, item);
            }

            public void AddGreaterOrEqualTo(ColumnBase column, IFilterItem item)
            {
                _originalFilter.AddGreaterOrEqualTo(column, item);
            }

            public void AddLessOrEqualTo(ColumnBase column, IFilterItem item)
            {
                _originalFilter.AddLessOrEqualTo(column, item);
            }

            public void AddLessThan(ColumnBase column, IFilterItem item)
            {
                _originalFilter.AddLessThan(column, item);
            }

            public void AddGreaterThan(ColumnBase column, IFilterItem item)
            {
                _originalFilter.AddGreaterThan(column, item);
            }

            public void AddWhere(string filterText, params IFilterItem[] formatItems)
            {
                _originalFilter.AddWhere(filterText, formatItems);
            }

            public void AddOr(IFilter a, IFilter b)
            {
                _originalFilter.AddOr(a, b);
            }

            public void AddTrueCondition()
            {
                _originalFilter.AddTrueCondition();
            }

            public void AddLessOrEqualWithWildcard(Firefly.Box.Data.TextColumn column, Text value, IFilterItem filterItem)
            {
                _originalFilter.AddLessOrEqualWithWildcard(column, value, filterItem);
            }


        }

        ITransaction IEntityDataProvider.BeginTransaction()
        {
            return _helper.BeginTransaction();
        }

        bool IEntityDataProvider.SupportsTransactions
        {
            get { return true; }
        }

        public void Dispose()
        {
            _helper.Dispose();
        }
        class isStringFilterItemSaver : IFilterItemSaver
        {
            public bool isString = false;
            public void SaveAnsiString(string value, int length, bool fixedWidth)
            {
                isString = true;
            }

            public void SaveBoolean(bool value)
            {

            }
            public PrepareBytes PrepareBetween;
            public void SaveByteArray(byte[] value)
            {
                PrepareBetween = new PrepareBytes(value);
            }

            public void SaveColumn(ColumnBase column)
            {

            }

            public void SaveDateTime(DateTime value)
            {

            }

            public void SaveDecimal(decimal value, byte precision, byte scale)
            {
            }

            public void SaveEmptyDateTime()
            {
            }

            public void SaveInt(int value)
            {
            }

            public void SaveNull()
            {
            }

            public void SaveString(string value, int length, bool fixedWidth)
            {
                isString = true;
            }

            public void SaveTimeSpan(TimeSpan value)
            {
            }
        }
        void SQLFilterHelper.AddLessOrEqualWithWildcard(Firefly.Box.Data.TextColumn column, Text value, IFilterItem filterItem, IFilterBuilder to, string operatorPrefix)
        {
            var isString = new isStringFilterItemSaver();
            filterItem.SaveTo(isString);
            if (isString.isString)
            {
                int i = column.FormatInfo.MaxDataLength;
                to.AddWhere("{0} " + operatorPrefix + "<= rpad ({1}," + i.ToString() + ",chr(255))", column, filterItem);
            }
            else
            {
                if (isString.PrepareBetween != null)
                {
                    to.AddLessOrEqualTo(column, new byteFilterItem(isString.PrepareBetween.betweenTo));
                }
            }
        }

        void SQLFilterHelper.SaveTimePartOfDateTimeTo(Time t, IValueSaver valueSaver)
        {
            valueSaver.SaveAnsiString(t.ToString("HHMMSS"), 6, false);
        }

        string SQLFilterHelper.GetTimeExtractionSyntaxFromDateTimeColumn()
        {
            return "TO_CHAR ({0}, 'HH24MISS')";
        }
        public static bool EnableFetchFirstRow = true;
        public void AddToCommandAfterOrderBy(StringBuilder commandText, bool firstRowOnly, Firefly.Box.Data.Entity entity, Sort sort, bool isFilterUnique, bool hasAccentCharsInNonEqualOperators, bool optimizeForFirstRows, bool fromRelation)
        {
            if (EnableFetchFirstRow && firstRowOnly && !commandText.ToString().Contains("for update"))
                commandText.Append(" FETCH FIRST ROW ONLY");
        }
    }

    class OleDbDataParameterSaver : DbDataParameterFilterItemSaver
    {
        string _indicator;

        public OleDbDataParameterSaver(IDbCommand command, string indicator, IDateTimeCollector dtc)
            : base(command, indicator, false, dtc)
        {
            _indicator = indicator;
        }

        protected override string CreateParameterName(int parameterCount)
        {
            return _indicator;
        }
    }
    interface SQLDataProviderHelperClient : SQLFilterHelper
    {
        IRowsReader Execute(SQLDataProviderHelper.SQLSelectCommand.ExecuteReaderHelper command, bool optimizeForFirstRows, IEnumerable<ColumnBase> selectedColumns, Firefly.Box.Data.Entity entity, bool disableCache, bool lockAllRows, string entityName);
        Exception ProcessException(Firefly.Box.Data.Entity entity, Exception e);
        void AddLockSyntaxAfterTableDefinitionInFrom(StringBuilder commandText);
        void AddJoin(Firefly.Box.Data.Entity entity, string entityAlias, IFilter joinFilter, bool outerJoin, StringBuilder fromText, StringBuilder whereText, SQLFilterConsumer sqlFilterConsumerForJoinFilter, Action<StringBuilder> callMeAfterTableAlias, Firefly.Box.Data.Entity mainSelectEntity);
        EntityDataProviderFilterItemSaver CreateFilterItemSaver(IDbCommand command, IDateTimeCollector dtc);
        string GetSelectAddition(bool onlyOneRow, Firefly.Box.Data.Entity entity, Sort sort, bool uniqueFilter, bool hasAccentCharsInNonEqualOperators, IEnumerable<ColumnBase> columns, bool optimiseForFirstRows, bool isForRelation);
        bool ReuseParameters { get; }
        bool KeepLockDataReaderOpenUntilUnlock { get; }
        void ExecuteInsert(Firefly.Box.Data.Entity entity, IDbCommand insertCommand, IRowStorage storage);
        string WrapColumnName(string name);
        string GetEntityName(Firefly.Box.Data.Entity entity);
        IDbCommand CreateCommand(IDbConnection connection);
        bool ShouldLockAllRowsInExecuteCommand(bool firstRowOnly, bool uniqueByPrimaryKey);
        void AddLockSyntaxToSqlCommand(StringBuilder commandText, IEnumerable<IJoin> joins, Func<ColumnBase, string> getColumnAlias, bool hasNonRowLockingEntitiesInJoin, IEnumerable<ColumnBase> selectedColumns, Firefly.Box.Data.Entity table);

        ICustomLock UseCustomRowLocking(StringBuilder commandText, IFilter primaryKeyFilter, IDbCommand c, Firefly.Box.Data.Entity e);
        bool AllowEmptyStringParameter();

        IValueLoader GetDataReaderValueLoader(IDataReader reader, int columnIndexInSelect, IDateTimeCollector dtc);

        EntityDataProviderFilterItemSaver CreateFilterItemSaverForInsert(IDbCommand dbCommand, Firefly.Box.Data.Entity entity, ColumnBase column, IRowStorage storage, IDateTimeCollector dtc);
        string GetColumnNameForSelect(ColumnBase column, string aliasGiven);
        IRowsReader ProvideAlternateExecuteCommandWithLock(bool firstRowOnly, bool uniqueByPrimaryKey, bool lockAllrows, SQLDataProviderHelper.SQLSelectCommand.AlternateExecuteCommandHelper helper);
        void AddToCommandAfterOrderBy(StringBuilder commandText, bool firstRowOnly, Firefly.Box.Data.Entity entity, Sort sort, bool isFilterUnique, bool hasAccentCharsInNonEqualOperators, bool optimizeForFirstRows, bool fromRelation);
    }

    public interface ICustomLock
    {
        void Unlock();
        void LockTheRow(Action<IDataReader> andCallMeToReadTheValues);
    }
    class SQLDataProviderHelper
    {

        System.Data.IDbConnection ______neverUseMeConnection;
        System.Data.IDbConnection _connection
        {
            get { return _wrapConnection(______neverUseMeConnection); }
        }
        public static Func<IDbConnection, IDbConnection> _wrapConnection = c => c;
        internal Exception ProcessException(Exception e, Firefly.Box.Data.Entity entity, IDbCommand command)
        {
            if (command != null)
            {
                if (!e.Data.Contains("SQL"))
                    using (var s = new StringWriter())
                    {
                        s.WriteLine(command.CommandText);
                        s.WriteLine(LogDatabaseWrapper.ParameterInfo("Parameter Info", command.Parameters));

                        e.Data.Add("SQL", s.ToString());
                    }
                command.Dispose();
            }

            var r = _client.ProcessException(entity, e);

            if (r == e)
            {
                if (e is DatabaseErrorException)
                    return e;
                var result = new DatabaseErrorException(DatabaseErrorType.UnknownError, e, DatabaseErrorHandlingStrategy.AbortAllTasks);
                if (e.Data.Contains("SQL"))
                    result.Data.Add("SQL", e.Data["SQL"]);
                return result;

            }
            return r;

        }

        string WrapColumnName(ColumnBase x)
        {
            if (x.Name[0] == '=')
                return _client.WrapColumnName(x.Name.Substring(1));
            return _client.WrapColumnName(x.Name);
        }

        SQLDataProviderHelperClient _client;
        public SQLDataProviderHelper(IDbConnection connection, SQLDataProviderHelperClient client)
        {
            _client = client;
            ______neverUseMeConnection = connection;
            if (_connection.State != ConnectionState.Open)
                _connection.Open();



        }
        public void Dispose()
        {
            _connection.Close();
        }

        class SQLRowsSource : IRowsSource
        {
            Firefly.Box.Data.Entity _entity;
            string _entityName;
            SQLDataProviderHelper _parent;

            public SQLRowsSource(Firefly.Box.Data.Entity entity, SQLDataProviderHelper parent)
            {
                _entity = entity;
                _parent = parent;
                _entityName = _parent._client.GetEntityName(_entity);
            }

            public void Dispose()
            {

            }

            public IRowsProvider CreateReader(IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort, IEnumerable<IJoin> joins, bool disableCache)
            {
                return new IRowsReaderProviderBridgeToNormalReader(_entity, where, sort,
                                                                    (filter, sort1) =>

                                                                    new SQLSelectCommand(_entity, _parent, selectedColumns, filter, sort1, joins, disableCache, _entityName).ExecuteReader(true, false), (row, columnBase) => ((SQLRow)row).GetValueOf(columnBase));
            }

            public IRowsReader ExecuteReader(IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort, IEnumerable<IJoin> joins, bool lockAllRows)
            {
                return
                    new SQLSelectCommand(_entity, _parent, selectedColumns, where, sort, joins, false, _entityName).ExecuteReader(
                        false, lockAllRows);
            }

            public IRowsReader ExecuteCommand(IEnumerable<ColumnBase> selectedColumns, IFilter filter, Sort sort, bool firstRowOnly, bool shouldBeOnlyOneRowThatMatchesTheFilter, bool lockAllRows)
            {
                return
                    new SQLSelectCommand(_entity, _parent, selectedColumns, filter, sort, new List<IJoin>(), false, _entityName).
                        ExecuteCommand(firstRowOnly, shouldBeOnlyOneRowThatMatchesTheFilter, lockAllRows);
            }

            public IRow Insert(IEnumerable<ColumnBase> columns, IEnumerable<IValue> valuesV, IRowStorage storage, IEnumerable<ColumnBase> selectedColumns)
            {
                StringBuilder cols = new StringBuilder(), values = new StringBuilder();
                bool first = true;
                var dtc = new DateTimeCollector();
                using (var c = _parent.CreateCommand())
                {
                    var vr = valuesV.GetEnumerator();
                    foreach (var column in columns)
                    {
                        vr.MoveNext();
                        if (first)
                            first = false;
                        else
                        {
                            cols.Append(", ");
                            values.Append(", ");
                        }

                        cols.Append(_parent.WrapColumnName(column));
                        var x = dtc.CreateFilterItemSaverForInsert(_parent._client, c, _entity, column, storage);
                        vr.Current.SaveTo(x);
                        values.Append(x.TextForCommand ?? "null");
                    }
                    c.CommandText += string.Format(NewLineInSQL + "Insert into {0} ({1}) " + NewLineInSQL + " values ({2})" + NewLineInSQL, _entityName, cols, values);

                    try
                    {
                        _parent._client.ExecuteInsert(_entity, c, storage);
                    }
                    catch (Exception e)
                    {
                        throw _parent.ProcessException(e, _entity, c);
                    }

                }
                return new SQLRow(_parent, _entity, storage, selectedColumns, _entityName, dtc);

            }

            public bool IsOrderBySupported(Sort sort)
            {
                return true;
            }
        }




        static ColumnInSelect[] AppendSelectPortion(StringBuilder commandText, IEnumerable<ColumnBase> selectedColumns, Func<ColumnBase, string> wrapName)
        {
            return AppendSelectPortion(commandText, selectedColumns, wrapName, string.Empty);
        }
        public class ColumnInSelect
        {
            public readonly ColumnBase Column;
            public readonly int IndexInSelect;

            public ColumnInSelect(ColumnBase column, int indexInSelect)
            {
                Column = column;
                IndexInSelect = indexInSelect;
            }
        }
        static ColumnInSelect[] AppendSelectPortion(StringBuilder commandText, IEnumerable<ColumnBase> selectedColumns, Func<ColumnBase, string> getColumnName, string selectAdition)
        {
            commandText.Append("Select ");
            commandText.Append(selectAdition);
            bool first = true;
            var columnsPosition = new List<ColumnInSelect>();
            int i = 0;
            foreach (var column in selectedColumns)
            {
                var tc = column as Firefly.Box.Data.TimeColumn;
                if (!ReferenceEquals(tc, null) && !ReferenceEquals(tc.DateColumnForDateTimeStorage, null))
                {
                    bool found = false;
                    foreach (var columnInSelect in columnsPosition)
                    {
                        if (ReferenceEquals(columnInSelect.Column, tc.DateColumnForDateTimeStorage))
                        {
                            found = true;
                            columnsPosition.Add(new ColumnInSelect(tc, columnInSelect.IndexInSelect));
                            break;
                        }
                    }
                    if (found)
                        continue;
                }


                if (first)
                    first = false;
                else
                    commandText.Append(", ");
                commandText.Append(getColumnName(column));
                columnsPosition.Add(new ColumnInSelect(column, i++));
            }
            return columnsPosition.ToArray();
        }

        public static string NewLineInSQL = "\r\n";
        EntityDataProviderFilterItemSaver CreateFilterItemSaver(IDbCommand command, Firefly.Box.Data.Entity fromEntity, bool forInsert, IDateTimeCollector dtc, bool forceNoParams)
        {
            if (fromEntity is IDontUseDbParameters || forceNoParams)
                return _client.GetNoParameterFilterItemSaver(dtc);
            return _client.CreateFilterItemSaver(command, dtc);
        }
        internal void AppendWhere(StringBuilder commandText, IFilter theFilter, IDbCommand command, Firefly.Box.Data.Entity fromEntity, bool forceNoParams)
        {
            var filter = new SQLFilterConsumer(CreateFilterItemSaver(command, fromEntity, false, DummyDateTimeCollector.Instance, forceNoParams), WrapColumnName, _client.ReuseParameters, _client);
            theFilter.AddTo(filter);
            if (filter.HasFilter)
                commandText.Append(" " + NewLineInSQL + "Where " + filter.Result);
            else
                throw new InvalidOperationException("Entity must have a primary key " + fromEntity.GetType());
        }
        internal class RowsCacheDataReader : IRowsReader
        {


            Firefly.Box.Data.Entity _entity;

            SQLDataProviderHelper _helper;
            RowsCache _cache;

            public static RowsCacheDataReader LoadAllData(SQLDataProviderHelper helper, IDataReader reader, IEnumerable<ColumnBase> selectedColumns, ColumnInSelect[] columnsIndexes, Firefly.Box.Data.Entity entity, bool suppressRowLocks, string entityName)
            {

                var cache = new RowsCache(1000, selectedColumns, columnsIndexes, suppressRowLocks);
                using (reader)
                {
                    while (reader.Read())
                    {
                        cache.AddRow(reader, entityName, helper._client);
                    }
                }
                return new RowsCacheDataReader(helper, cache, entity);

            }
            public RowsCacheDataReader(SQLDataProviderHelper helper, RowsCache cache, Firefly.Box.Data.Entity entity)
            {

                _entity = entity;
                _cache = cache;
                _helper = helper;


            }
            public void Dispose()
            {
            }

            public bool Read()
            {

                return _cache.Read();
            }

            public IRow GetRow(IRowStorage c)
            {
                return _cache.GetRow(c, _entity, _helper);
            }

            public IRow GetJoinedRow(Firefly.Box.Data.Entity e, IRowStorage c)
            {
                return _cache.GetJoinedRow(e, c, _helper);

            }
        }
        internal class DataReaderEntitySelectReader : IRowsReader
        {
            IDataReader _reader;

            Firefly.Box.Data.Entity _entity;
            string _entityName;

            SQLDataProviderHelper _helper;
            IEnumerable<ColumnBase> _selectedColumns;
            ColumnInSelect[] _columnsIndexes;
            bool _suppressLocks;
            IDbCommand _command;
            public DataReaderEntitySelectReader(SQLDataProviderHelper helper, IDataReader reader, IEnumerable<ColumnBase> selectedColumns, ColumnInSelect[] columnsIndexes, Firefly.Box.Data.Entity entity, bool suppressLocks, IDbCommand command, string entityName)
            {
                _entityName = entityName;
                _command = command;
                _suppressLocks = suppressLocks;
                _entity = entity;
                _reader = reader;
                _helper = helper;
                _selectedColumns = selectedColumns;
                _columnsIndexes = columnsIndexes;
            }
            public void Dispose()
            {

                _reader.Close();
                _reader.Dispose();
            }

            public void ReadRow(IRowStorage rowStorageProvider)
            {

                if (_reader.Read())
                {

                }
            }


            public bool Read()
            {

                try
                {
                    return _reader.Read();
                }
                catch (Exception e)
                {
                    throw _helper.ProcessException(e, _entity, _command);
                }
            }

            public IRow GetRow(IRowStorage c)
            {
                var dtc = new DateTimeCollector();
                foreach (var columnsIndex in _columnsIndexes)
                {
                    dtc.SetValue(c, columnsIndex.Column, _helper._client, _reader, columnsIndex.IndexInSelect);
                }
                var row = new SQLRow(_helper, _entity, c, _selectedColumns, _suppressLocks, _entityName, dtc);

                return row;
            }

            public IRow GetJoinedRow(Firefly.Box.Data.Entity e, IRowStorage c)
            {
                var pkColumns = new HashSet<ColumnBase>(e.PrimaryKeyColumns);
                foreach (var columnInSelect in _columnsIndexes)
                {
                    if (pkColumns.Contains(columnInSelect.Column))
                        if (_reader.IsDBNull(columnInSelect.IndexInSelect))
                            return null;
                }
                var dtc = new DateTimeCollector();
                foreach (var columnsIndex in _columnsIndexes)
                {
                    dtc.SetValue(c, columnsIndex.Column, _helper._client, _reader, columnsIndex.IndexInSelect);
                }
                var row = new SQLRow(_helper, e, c, _selectedColumns, _suppressLocks, _helper._client.GetEntityName(e)/*this is a compromise - what I should have done is a dictionary based on the joins*/, dtc);

                return row;

            }
        }

        internal class SQLRow : IRow
        {
            SQLDataProviderHelper _helper;
            Firefly.Box.Data.Entity _entity;
            IRowStorage _storage;
            IEnumerable<ColumnBase> _selectedColumns;
            string _entityName;
            DateTimeCollector _dtc;
            public SQLRow(SQLDataProviderHelper helper, Firefly.Box.Data.Entity entity, IRowStorage rowStorageProvider, IEnumerable<ColumnBase> selectedColumns, string entityName, DateTimeCollector dtc)
            {
                _dtc = dtc;
                _helper = helper;
                _entity = entity;
                _storage = rowStorageProvider;
                _selectedColumns = selectedColumns;
                _entity = entity;
                _entityName = entityName;
            }

            bool _suppressLocks = false;
            public SQLRow(SQLDataProviderHelper helper, Firefly.Box.Data.Entity entity, IRowStorage rowStorageProvider, IEnumerable<ColumnBase> selectedColumns, bool suppressLocks, string entityName, DateTimeCollector dtc, Action onUnLock = null)
                : this(helper, entity, rowStorageProvider, selectedColumns, entityName, dtc)
            {
                _onUnlock = onUnLock;
                _suppressLocks = suppressLocks;

            }


            public void Update(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, bool verifyRowHasNotChangedSinceLoaded)
            {
                var x = _dtc.GetUpdateContext();
                using (var c = new NonQueryCommand(_helper, _entity, "Update "))
                {
                    try
                    {
                        c.CommandText.Append(_entityName);
                        c.CommandText.Append(" " + NewLineInSQL + "Set ");
                        bool first = true;
                        var vr = values.GetEnumerator();
                        foreach (var column in columns)
                        {
                            vr.MoveNext();
                            if (first)
                                first = false;
                            else
                                c.CommandText.Append(", ");
                            c.CommandText.Append(_helper.WrapColumnName(column));
                            c.CommandText.Append("=");
                            c.CommandText.Append(x.ExtractValue(c, vr.Current, column) ?? "null");

                        }
                        c.AppendWhere(new PrimaryKeyFitler(_entity, GetValueOf, _useOptimisticLocking ? GetOptimisticLockingColumns() : verifyRowHasNotChangedSinceLoaded ? columns : null));
                    }
                    catch (Exception ex)
                    {
                        c.SetException(ex);
                        throw;
                    }
                }
                x.Done();
            }


            class PrimaryKeyFitler : IFilter
            {
                Firefly.Box.Data.Entity _entity;
                Func<ColumnBase, IValue> _getValue;
                IEnumerable<ColumnBase> _additionalColumns;

                public PrimaryKeyFitler(Firefly.Box.Data.Entity entity, Func<ColumnBase, IValue> getValue)
                    : this(entity, getValue, null)
                {

                }
                public PrimaryKeyFitler(Firefly.Box.Data.Entity entity, Func<ColumnBase, IValue> getValue, IEnumerable<ColumnBase> additionalColumns)
                {
                    _entity = entity;
                    _getValue = getValue;
                    _additionalColumns = additionalColumns;
                }

                public void AddTo(IFilterBuilder builder)
                {

                    foreach (var column in _entity.PrimaryKeyColumns)
                    {
                        AddEqualToColumn(builder, column);
                    }
                    if (_additionalColumns != null)
                    {
                        var done = new HashSet<ColumnBase>(_entity.PrimaryKeyColumns);
                        foreach (var column in _additionalColumns)
                        {
                            if (done.Contains(column))
                                continue;
                            done.Add(column);
                            if (!column.ExcludeFromDbWhere)
                                AddEqualToColumn(builder, column);
                        }
                    }

                }

                void AddEqualToColumn(IFilterBuilder builder, ColumnBase column)
                {
                    var dc = column as DateColumn;
                    var tc = column as TimeColumn;
                    if (tc != null && tc.DateColumnForDateTimeStorage != null)
                        return;
                    if (dc != null && dc.TimeColumnForDateTimeStorage != null)
                    {
                        var d = _getValue(dc);
                        var dv = new myValueSave();
                        d.SaveTo(dv);
                        if (dv.IsNull)
                            builder.AddEqualTo(dc, new NullFilterItem());
                        else
                        {
                            var t = _getValue(dc.TimeColumnForDateTimeStorage);
                            var tv = new myValueSave();
                            t.SaveTo(tv);
                            builder.AddEqualTo(dc, new DateTimeFilterItem(new DateTime(
                                dv._dt.Year,
                                dv._dt.Month,
                                dv._dt.Day,
                                tv._dt.Hour,
                                tv._dt.Minute,
                                tv._dt.Second
                                )));
                        }
                    }
                    else
                        builder.AddEqualTo(column, new FilterItemBridgeToIValue(_getValue(column)));
                }
            }
            class NullFilterItem : IFilterItem
            {
                public bool IsAColumn()
                {
                    return false;
                }

                public void SaveTo(IFilterItemSaver saver)
                {
                    saver.SaveNull();
                }
            }

            class DateTimeFilterItem : IFilterItem
            {
                DateTime _dt;

                public DateTimeFilterItem(DateTime dt)
                {
                    _dt = dt;
                }

                public void SaveTo(IFilterItemSaver saver)
                {
                    saver.SaveDateTime(_dt);
                }
                public bool IsAColumn()
                {
                    return false;
                }
            }

            class myValueSave : IValueSaver
            {
                public void SaveInt(int value)
                {

                }

                public void SaveDecimal(decimal value, byte precision, byte scale)
                {

                }

                public void SaveString(string value, int length, bool fixedWidth)
                {

                }

                public void SaveAnsiString(string value, int length, bool fixedWidth)
                {
                }

                public void SaveNull()
                {
                    IsNull = true;
                }
                public bool IsNull = false;
                public DateTime _dt;
                public void SaveDateTime(DateTime value)
                {
                    _dt = value;
                }

                public void SaveTimeSpan(TimeSpan value)
                {
                }

                public void SaveBoolean(bool value)
                {
                }

                public void SaveByteArray(byte[] value)
                {
                }

                public void SaveEmptyDateTime()
                {

                }
            }


            public void Delete(bool verifyRowHasNotChangedSinceLoaded)
            {
                using (var c = new NonQueryCommand(_helper, _entity, "Delete from "))
                {
                    c.CommandText.Append(_entityName);
                    c.AppendWhere(new PrimaryKeyFitler(_entity, GetValueOf, verifyRowHasNotChangedSinceLoaded || _useOptimisticLocking ? GetOptimisticLockingColumns() : null));

                }
                Unlock();
            }
            static IJoin[] _emptyJoins = new IJoin[0];
            bool _useOptimisticLocking = false;

            Action _onUnlock;
            public void Lock()
            {
                if (SQLClientEntityDataProvider.SuppressLocking)
                {
                    if (SQLClientEntityDataProvider.UseOptimisticLocking)
                        _useOptimisticLocking = true;
                    return;
                }
                if (_suppressLocks)
                    return;
                using (var c = _helper.CreateCommand())
                {
                    var selectedColumns = GetSelectedColumns();
                    var commandText = new StringBuilder();
                    var columnsPosition = AppendSelectPortion(commandText, selectedColumns, _helper.WrapColumnName);
                    commandText.Append(" " + NewLineInSQL + "From ");
                    commandText.Append(_entityName);
                    var pkFilter = new PrimaryKeyFitler(_entity, GetValueOf);
                    if (_onUnlock != null)
                        _onUnlock();
                    var customLock = _helper._client.UseCustomRowLocking(commandText, pkFilter, c, _entity);
                    if (customLock == null)
                    {
                        _helper._client.AddLockSyntaxAfterTableDefinitionInFrom(commandText);

                        _helper.AppendWhere(commandText, pkFilter, c, _entity, false);
                        _helper._client.AddLockSyntaxToSqlCommand(commandText, _emptyJoins, _helper.WrapColumnName, false, _selectedColumns, _entity);

                        c.CommandText = commandText.ToString();
                        customLock = new DefaultCustomLock(c, _helper._client.KeepLockDataReaderOpenUntilUnlock);
                    }

                    try
                    {
                        bool foundRow = false;
                        _onUnlock = customLock.Unlock;
                        customLock.LockTheRow(r =>
                        {
                            foreach (var column in columnsPosition)
                            {
                                _storage.SetValue(column.Column, _dtc.GetLoader(column.Column, _helper._client, r, column.IndexInSelect));
                            }
                            foundRow = true;
                        });

                        if (!foundRow)
                            throw new DatabaseErrorException(DatabaseErrorType.RowDoesNotExist, DatabaseErrorHandlingStrategy.Rollback);

                    }
                    catch (System.Exception e)
                    {
                        throw _helper.ProcessException(e, _entity, c);
                    }
                }
                _suppressLocks = true;
            }
            class DefaultCustomLock : ICustomLock
            {
                IDbCommand c;

                IDataReader _lockReader;
                bool _keepDataReaderOpenUntilUnlock;
                public DefaultCustomLock(IDbCommand c, bool keepDataReaderOpenUntilUnlock)
                {
                    this.c = c;
                    _keepDataReaderOpenUntilUnlock = keepDataReaderOpenUntilUnlock;
                }
                public void LockTheRow(Action<IDataReader> andCallMeToReadTheValues)
                {
                    IDataReader r = c.ExecuteReader();
                    try
                    {
                        if (r.Read())
                        {
                            andCallMeToReadTheValues(r);
                            if (_keepDataReaderOpenUntilUnlock)
                            {
                                _lockReader = r;
                            }
                            else r.Dispose();
                        }
                        else
                        {
                            r.Dispose();

                        }
                    }
                    catch
                    {
                        r.Dispose();
                        throw;
                    }

                }

                public void Unlock()
                {
                    if (_lockReader != null)
                        _lockReader.Dispose();
                    _lockReader = null;
                }
            }
            ColumnBase[] GetSelectedColumns()
            {
                var selectedColumns = new List<ColumnBase>();
                foreach (var column in _selectedColumns)
                {
                    if (column.Entity == _entity)
                        selectedColumns.Add(column);
                }
                return selectedColumns.ToArray();
            }
            ColumnBase[] GetOptimisticLockingColumns()
            {
                var selectedColumns = new List<ColumnBase>();
                foreach (var column in _selectedColumns)
                {
                    if (column.Entity == _entity && !column.ExcludeFromDbWhere && !(column is ByteArrayColumn))
                        selectedColumns.Add(column);
                }
                return selectedColumns.ToArray();
            }

            public void ReloadData()
            {
                LoadData(GetSelectedColumns(), (columnBase, loader) => _storage.SetValue(columnBase, loader));
            }

            void LoadData(IEnumerable<ColumnBase> selectedColumns, Action<ColumnBase, IValueLoader> setColumnValue)
            {
                using (var c = _helper.CreateCommand())
                {
                    var commandText = new StringBuilder();
                    var columnsPosition = AppendSelectPortion(commandText, selectedColumns, _helper.WrapColumnName);
                    commandText.Append(" " + NewLineInSQL + "From ");
                    commandText.Append(_entityName);


                    _helper.AppendWhere(commandText, new PrimaryKeyFitler(_entity, GetValueOf), c, _entity, false);

                    c.CommandText = commandText.ToString();
                    try
                    {
                        using (var r = c.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                foreach (var column in columnsPosition)
                                    setColumnValue(column.Column, _dtc.GetLoader(column.Column, _helper._client, r, column.IndexInSelect));
                            }
                            else
                                throw new DatabaseErrorException(DatabaseErrorType.RowDoesNotExist, DatabaseErrorHandlingStrategy.Ignore);
                        }
                    }
                    catch (System.Exception e)
                    {
                        throw _helper.ProcessException(e, _entity, c);
                    }
                }
            }

            public bool IsEqualTo(IRow row)
            {
                var x = row as SQLRow;
                if (x != null)
                {
                    foreach (var column in _entity.PrimaryKeyColumns)
                    {
                        var a = new NoParametersFilterItemSaver(_helper._client.AllowEmptyStringParameter(), "{0}", DummyDateTimeCollector.Instance);
                        var b = new NoParametersFilterItemSaver(_helper._client.AllowEmptyStringParameter(), "{0}", DummyDateTimeCollector.Instance);
                        _storage.GetValue(column).SaveTo(a);
                        x._storage.GetValue(column).SaveTo(b);
                        if (!string.Equals(a.Result, b.Result, StringComparison.Ordinal))
                            return false;
                    }
                    return true;
                }
                return false;
            }

            public void Unlock()
            {

                if (_onUnlock != null)
                {
                    _onUnlock();
                    _onUnlock = null;
                }
                _suppressLocks = false;
            }


            public IValue GetValueOf(ColumnBase columnBase)
            {
                var r = _dtc.GetValue(_storage, columnBase);
                if (r == null && columnBase.Entity == _entity)
                    LoadData(new[] { columnBase },
                        (columnBase1, valueLoader) =>
                        {
                            if (columnBase1 == columnBase)
                                r = columnBase1.LoadFrom(valueLoader);
                        });
                return r;
            }
        }




        internal class SQLSelectCommand
        {
            Firefly.Box.Data.Entity _entity;
            string _entityName;
            SQLDataProviderHelper _parent;

            IEnumerable<ColumnBase> _columns;

            IFilter _where;
            Sort _sort;
            bool _disableCache;

            public SQLSelectCommand(Firefly.Box.Data.Entity entity, SQLDataProviderHelper parent, IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort, IEnumerable<IJoin> joins, bool disableCache, string entityName)
            {
                _entityName = entityName;

                _entity = entity;

                _parent = parent;
                _where = where;
                _sort = sort;

                _columns = selectedColumns;
                _disableCache = disableCache;
                _getColumnAlias = _parent.WrapColumnName;

                _getColumnAliasForSelect = _getColumnAlias;
                foreach (var j in joins)
                {

                    if (_aliases == null)
                    {
                        _aliases = new Dictionary<Firefly.Box.Data.Entity, string> { { _entity, "A" } };
                        _getColumnAlias = column =>
                        {
                            if (column.Name[0] == '=')
                                return _parent._client.WrapColumnName(column.Name.Substring(1));
                            return _parent._client.WrapColumnName(_aliases[column.Entity]) + "." + _parent._client.WrapColumnName(column.Name);

                        };
                        _getColumnAliasForSelect =
                            column => _parent._client.GetColumnNameForSelect(column, _getColumnAlias(column));
                    }
                    int i = _aliases.Count;
                    int modVal = (int)'Z' - 'A' + 1;
                    string entityAlias = "";
                    while (i > 0)
                    {
                        if (entityAlias.Length > 0)
                            i--;
                        var k = i % modVal;
                        i -= k;
                        entityAlias = ((char)('A' + k)).ToString() + entityAlias; ;
                        i /= modVal;

                    }
                    _aliases.Add(j.Entity, entityAlias);

                }
                _joins = joins;

            }

            public class ExecuteReaderHelper
            {
                IDbCommand _command;

                private SQLSelectCommand _parent;

                public ExecuteReaderHelper(IDbCommand c, SQLSelectCommand parent)
                {
                    this._command = c;
                    this._parent = parent;
                }

                public IDbCommand CreateCommand(out ColumnInSelect[] columnsIndexes, bool addLockSyntax, bool noOrderByRequired, bool optimizeForFirstRows, bool forceNoParams = false)
                {
                    var x = false;
                    _command.CommandText = _parent.CreateCommand(out columnsIndexes, _command, addLockSyntax, false, noOrderByRequired, out x, optimizeForFirstRows, false, forceNoParams);
                    return _command;
                }

                public bool IsUniqueFilter()
                {
                    var x = new IsUniqeFilterClass();
                    _parent._where.AddTo(x);
                    return x.IsUniqueFor(_parent._entity, false);

                }


                internal IRowsReader LoadRow(IDataReader r, ColumnInSelect[] columnsIndexes, Action unlock)
                {
                    var rowsCache = new RowsCache(1, _parent._columns, columnsIndexes, true);
                    rowsCache.AddRow(r, _parent._entityName, _parent._parent._client, unlock);
                    return new RowsCacheDataReader(_parent._parent, rowsCache, _parent._entity);
                }

                internal IRowsReader PagedReader(bool addLockSyntax, bool optimizeForFirstRows, int rowsPerPage, Func<string, int, string> processCommand)
                {
                    if (rowsPerPage <= 0)
                    {
                        rowsPerPage = optimizeForFirstRows ? 40 : 1000;
                    }
                    return new PagedRowsReader(
                    new IRowsReaderProviderBridgeToNormalReader(_parent._entity, _parent._where, _parent._sort, (f, s) =>
                    {

                        ColumnInSelect[] columnsIndexes;
                        var x = false;
                        var select = new SQLSelectCommand(_parent._entity, _parent._parent, _parent._columns, f, s, _parent._joins, _parent._disableCache, _parent._entityName);
                        var c = _parent._parent.CreateCommand();
                        c.CommandText = processCommand(select.CreateCommand(out columnsIndexes, c, addLockSyntax, false, false, out x, optimizeForFirstRows, false, false), rowsPerPage);

                        var result = RowsCacheDataReader.LoadAllData(_parent._parent,
                          c.ExecuteReader(),
                          _parent._columns, columnsIndexes, _parent._entity, addLockSyntax, _parent._entityName);
                        c.Dispose();
                        return result;
                    }, (x, c) => ((SQLRow)x).GetValueOf(c)), rowsPerPage);



                }
                class PagedRowsReader : IRowsReader
                {

                    IRowsReader _rowsReader;
                    IRowsProvider _rowsProvider;
                    IRow _lastRow;
                    int _rows;

                    public PagedRowsReader(IRowsProvider provider, int rows)
                    {

                        _rowsProvider = provider;
                        _rowsReader = provider.FromStart();
                        _rows = rows;

                    }

                    public void Dispose()
                    {
                        _rowsReader.Dispose();
                    }

                    public IRow GetJoinedRow(Firefly.Box.Data.Entity e, IRowStorage c)
                    {
                        return _rowsReader.GetJoinedRow(e, c);
                    }

                    public IRow GetRow(IRowStorage c)
                    {
                        return _lastRow = _rowsReader.GetRow(c);
                    }
                    int i = 0;
                    public bool Read()
                    {
                        if (i++ >= _rows)
                        {
                            i = 1;
                            _rowsReader.Dispose();
                            _rowsReader = _rowsProvider.After(_lastRow, false);
                        }
                        return _rowsReader.Read();
                    }
                }
            }
            public IRowsReader ExecuteReader(bool optimizeForFirstRows, bool lockAllRows)
            {
                var c = _parent.CreateCommand();
                try
                {



                    var r = _parent._client.Execute(new ExecuteReaderHelper(c, this), optimizeForFirstRows, _columns, _entity,
                        _disableCache, lockAllRows, _entityName);
                    c.Dispose();
                    return r;
                }
                catch (Exception e)
                {
                    throw _parent.ProcessException(e, _entity, c);
                }

            }



            string CreateCommand(out ColumnInSelect[] columnsIndexes, IDbCommand command, bool addLockSyntax, bool firstRowOnly, bool isFilterUnique, out bool limitToOneRowResult, bool optimizeForFirstRows, bool fromRelation, bool forceNoParams = false)
            {
                StringBuilder commandText = new StringBuilder(), fromText = new StringBuilder();
                var filter = new SQLFilterConsumer(_parent.CreateFilterItemSaver(command, _entity, false, DummyDateTimeCollector.Instance, forceNoParams), _getColumnAlias, _parent._client.ReuseParameters, _parent._client);
                var hasJoins = false;
                fromText.Append(" " + NewLineInSQL + "From " + _entityName);
                bool hasNonRowLockingEntitiesInJoin = false;
                if (_joins.GetEnumerator().MoveNext())
                {
                    hasJoins = true;
                    fromText.Append(" A");
                    if (addLockSyntax && _entity.AllowRowLocking)
                        _parent._client.AddLockSyntaxAfterTableDefinitionInFrom(fromText);
                    foreach (var j in _joins)
                    {
                        if (j.Entity.AllowRowLocking != _entity.AllowRowLocking)
                            hasNonRowLockingEntitiesInJoin = true;
                        _parent._client.AddJoin(j.Entity, _aliases[j.Entity], j.Where, j.IsOuterJoin, fromText, filter.Result, new SQLFilterConsumer(_parent.CreateFilterItemSaver(command, _entity, false, DummyDateTimeCollector.Instance, false), _getColumnAlias, _parent._client.ReuseParameters, _parent._client),
                            y =>
                            {
                                if (addLockSyntax && j.Entity.AllowRowLocking)
                                    _parent._client.AddLockSyntaxAfterTableDefinitionInFrom(y);
                            }, _entity);

                    }
                }
                else
                {
                    if (addLockSyntax)
                        _parent._client.AddLockSyntaxAfterTableDefinitionInFrom(fromText);
                }
                _where.AddTo(filter);

                string selectAdition = string.Empty;
                if (firstRowOnly && !filter.HasAccentCharsInNonEqualOperators)
                    limitToOneRowResult = true;
                else
                    limitToOneRowResult = false;
                selectAdition = _parent._client.GetSelectAddition(firstRowOnly, _entity, _sort, isFilterUnique, filter.HasAccentCharsInNonEqualOperators, _columns, optimizeForFirstRows, fromRelation);
                columnsIndexes = AppendSelectPortion(commandText, _columns, _getColumnAliasForSelect, selectAdition);
                commandText.Append(fromText);



                if (filter.HasFilter)
                {
                    commandText.Append(" " + NewLineInSQL + "Where " + filter.Result);
                }
                //order by
                if (!isFilterUnique || ConnectionManager.DisableUniqueFilterOrderbyOptimization)
                {
                    var columns = new HashSet<string>();
                    var descending = !_sort.Reversed ? SortDirection.Descending : SortDirection.Ascending;
                    var segments = _sort.Segments;
                    bool started = false;
                    for (int i = 0; i < segments.Count; i++)
                    {
                        var columnAlias = _getColumnAlias(segments[i].Column);
                        if (columns.Contains(columnAlias))
                            continue;
                        columns.Add(columnAlias);
                        if (!started)
                        {
                            commandText.Append(" " + NewLineInSQL + "Order by ");
                            started = true;
                        }
                        else
                            commandText.Append(", ");
                        commandText.Append(columnAlias);
                        if (segments[i].Direction == descending)
                            commandText.Append(" desc");
                    }
                }

                if (addLockSyntax)
                {
                    _parent._client.AddLockSyntaxToSqlCommand(commandText, _joins, _getColumnAlias, hasNonRowLockingEntitiesInJoin, _columns, _entity);
                }
                _parent._client.AddToCommandAfterOrderBy(commandText, firstRowOnly, _entity, _sort, isFilterUnique, filter.HasAccentCharsInNonEqualOperators, optimizeForFirstRows, fromRelation);
                return commandText.ToString();
            }
            internal class AlternateExecuteCommandHelper
            {
                SQLSelectCommand _parent;
                IDbCommand _command;
                bool _uniqueFilter;


                public AlternateExecuteCommandHelper(SQLSelectCommand parent, IDbCommand command, bool uniqueFilter)
                {
                    _parent = parent;
                    _command = command;
                    _uniqueFilter = uniqueFilter;

                }

                internal IDbCommand CreateCommand(out ColumnInSelect[] columnsIndexes, bool forceNoParams = false)
                {

                    bool y = false;
                    _command.CommandText = _parent.CreateCommand(out columnsIndexes, _command, false, false, _uniqueFilter, out y, false, true, forceNoParams);
                    return _command;
                }

                internal IRowsReader LoadRow(IDataReader r, ColumnInSelect[] columnsIndexes, Action unlock, bool suppressRowLock)
                {
                    var rowsCache = new RowsCache(1, _parent._columns, columnsIndexes, suppressRowLock);
                    rowsCache.AddRow(r, _parent._entityName, _parent._parent._client, unlock);
                    return new RowsCacheDataReader(_parent._parent, rowsCache, _parent._entity);
                }
            }
            public IRowsReader ExecuteCommand(bool firstRowOnly, bool shouldBeOnlyOneRowThatMatchesTheFilter, bool lockAllrows)
            {


                using (var c = _parent.CreateCommand())
                {
                    try
                    {
                        ColumnInSelect[] columnsIndexes;

                        bool x = shouldBeOnlyOneRowThatMatchesTheFilter;

                        bool limitToOneResult;
                        if (lockAllrows)
                        {
                            var filterAnalyzer = new IsUniqeFilterClass();
                            _where.AddTo(filterAnalyzer);
                            var isUnique = filterAnalyzer.IsUniqueFor(_entity, true);

                            var result = _parent._client.ProvideAlternateExecuteCommandWithLock(firstRowOnly, isUnique, lockAllrows, new AlternateExecuteCommandHelper(this, c, shouldBeOnlyOneRowThatMatchesTheFilter));
                            if (result != null)
                                return result;
                            lockAllrows = _parent._client.ShouldLockAllRowsInExecuteCommand(firstRowOnly,
                                                                                            isUnique);
                        }

                        var commandText = CreateCommand(out columnsIndexes, c, lockAllrows, firstRowOnly, x, out limitToOneResult, false, true);
                        c.CommandText = commandText;
                        return new DataReaderEntitySelectReader(_parent,
                                                                c.ExecuteReader(limitToOneResult
                                                                                    ? CommandBehavior.SingleRow
                                                                                    : CommandBehavior.Default),
                                                                _columns, columnsIndexes, _entity, lockAllrows, c, _entityName);
                    }
                    catch (Exception e)
                    {
                        throw _parent.ProcessException(e, _entity, c);
                    }
                }

            }



            IEnumerable<IJoin> _joins;
            Dictionary<Firefly.Box.Data.Entity, string> _aliases;
            Func<ColumnBase, string> _getColumnAlias, _getColumnAliasForSelect;
        }



        public IRowsSource ProvideTaskRowSourceFor(Firefly.Box.Data.Entity entity)
        {
            if (entity.PrimaryKeyColumns == null || entity.PrimaryKeyColumns.Length == 0)
                throw new InvalidOperationException("An SQL Entity must have a Primary Key");
            return new SQLRowsSource(entity, this);
        }




        public IDbCommand CreateCommand()
        {
            var result = _client.CreateCommand(_connection);
            if (_activeTransaction != null)
            {
                if (_activeTransaction.Transaction == null)
                {
                    if (result.Transaction != null)
                        _activeTransaction.Transaction = result.Transaction;
                    else
                    {
                        if (IsolationLevel == IsolationLevel.Unspecified)
                            _activeTransaction.Transaction = _connection.BeginTransaction();
                        else
                            _activeTransaction.Transaction = _connection.BeginTransaction(IsolationLevel);
                    }
                }
                var trans = _activeTransaction.Transaction;
                var decorator = trans as ITransactionDecorator;
                if (decorator != null)
                    trans = decorator.GetDecoratedTransaction();
                result.Transaction = trans;
            }
            result.CommandTimeout = 0;
            return result;
        }
        System.Data.IsolationLevel _isolationLevel = IsolationLevel.Unspecified;
        public System.Data.IsolationLevel IsolationLevel { get { return _isolationLevel; } set { _isolationLevel = value; } }
        internal class NonQueryCommand : IDisposable
        {
            IDbCommand _command;
            SQLDataProviderHelper _parent;
            Firefly.Box.Data.Entity _entity;
            public NonQueryCommand(SQLDataProviderHelper parent, Firefly.Box.Data.Entity entity, string sql)
            {
                _parent = parent;
                _command = _parent.CreateCommand();
                CommandText = new StringBuilder(sql);
                _entity = entity;
            }

            public readonly StringBuilder CommandText;
            public void Dispose()
            {


                using (_command)
                {
                    int rowsAffected;

                    _command.CommandText = NewLineInSQL + CommandText.ToString() + NewLineInSQL;
                    if (_exception != null)
                        throw _exception;

                    try
                    {
                        rowsAffected = _command.ExecuteNonQuery();
                        if (rowsAffected < 1)
                            throw new DatabaseErrorException(DatabaseErrorType.UpdatedRowWasChangedSinceLoaded);

                    }
                    catch (System.Exception e)
                    {
                        try
                        {
                            throw _parent.ProcessException(e, _entity, _command);
                        }
                        catch (Exception ex)
                        {
                            if (!(ex is DatabaseErrorException))
                                throw new DatabaseErrorException(DatabaseErrorType.DataChangeFailed, ex);
                            else throw ex;
                        }

                    }

                }
            }

            public string ExtractValue(IValue column, IDateTimeCollector updateContext)
            {
                var x = _parent.CreateFilterItemSaver(_command, _entity, false, updateContext, false);
                column.SaveTo(x);
                return x.TextForCommand;
            }

            public void AppendWhere(IFilter filter)
            {
                _parent.AppendWhere(CommandText, filter, _command, _entity, false);
            }
            Exception _exception;
            internal void SetException(Exception ex)
            {
                _exception = ex;
            }
        }




        public long CountRows(Firefly.Box.Data.Entity entity)
        {
            using (var c = CreateCommand())
            {
                c.CommandText = "Select count(*) " + NewLineInSQL + "From " + _client.GetEntityName(entity);
                return Number.Cast(c.ExecuteScalar());
            }
        }

        public void Drop(Firefly.Box.Data.Entity entity)
        {
            using (var c = CreateCommand())
            {
                c.CommandText = "drop table " + _client.GetEntityName(entity);
                c.ExecuteNonQuery();
            }
        }

        public void Truncate(Firefly.Box.Data.Entity entity)
        {
            using (var c = CreateCommand())
            {
                c.CommandText = "truncate table " + _client.GetEntityName(entity);
                c.ExecuteNonQuery();
            }
        }
        public ITransaction BeginTransaction()
        {
            if (_activeTransaction == null)
                _activeTransaction = new ActiveTransaction(this);
            return _activeTransaction;
        }

        ActiveTransaction _activeTransaction;
        class ActiveTransaction : ITransaction
        {
            readonly SQLDataProviderHelper _parent;
            public System.Data.IDbTransaction Transaction;

            public ActiveTransaction(SQLDataProviderHelper parent)
            {
                _parent = parent;
            }

            public void Commit()
            {
                try
                {
                    if (Transaction != null)
                    {
                        if (Transaction.Connection != null)
                            Transaction.Commit();
                        Transaction = null;
                    }
                    _parent._activeTransaction = null;
                }
                catch (Exception e)
                {
                    throw _parent.ProcessException(e, null, null);
                }
            }

            public void Rollback()
            {
                if (Transaction != null)
                {
                    if (Transaction.Connection != null)
                        Transaction.Rollback();
                    Transaction = null;
                }
                _parent._activeTransaction = null;
            }
        }






        internal static Sort Duplicate(Sort s)
        {
            return s.Clone();
        }


        public bool Contains(Firefly.Box.Data.Entity entity)
        {
            try
            {

                using (var c = CreateCommand())
                {
                    string s = "*";
                    if (entity.IdentityColumn != null)
                        s = entity.IdentityColumn.Name;
                    c.CommandText = "Select " + s + " " + NewLineInSQL + "From " + _client.GetEntityName(entity);
                    using (var r = c.ExecuteReader(CommandBehavior.SchemaOnly))
                    {
                        if (r.FieldCount > 0)
                            return true;
                    }
                }
            }
            catch
            {

            }
            return false;

        }

        internal bool IsClosed()
        {
            return _connection.State != ConnectionState.Open;
        }

        internal void KillConnection()
        {
            _connection.Close();

        }

        public SqlCommandFilterBuilder CreateSqlCommandFilterBuilder(IDbCommand command, Firefly.Box.Data.Entity entity)
        {
            return new SQLFilterConsumer(CreateFilterItemSaver(command, entity, false, DummyDateTimeCollector.Instance, false), x => x.Name, _client.ReuseParameters, _client);
        }
    }
    interface SQLFilterHelper
    {
        void AddLessOrEqualWithWildcard(Firefly.Box.Data.TextColumn column, Text value, IFilterItem filterItem, IFilterBuilder to, string operatorPrefix);
        void SaveTimePartOfDateTimeTo(Time t, IValueSaver valueSaver);
        string GetTimeExtractionSyntaxFromDateTimeColumn();
        EntityDataProviderFilterItemSaver GetNoParameterFilterItemSaver(IDateTimeCollector dtc);
    }


    class dummySqlFilterHelper : SQLFilterHelper
    {
        public dummySqlFilterHelper(EntityDataProviderFilterItemSaver saver)
        {
            _saver = saver;
        }
        public void AddLessOrEqualWithWildcard(Firefly.Box.Data.TextColumn column, Text value, IFilterItem filterItem, IFilterBuilder to, string operatorPrefix)
        {
            to.AddWhere("{0} <= {1}+'*'", column, filterItem);
        }

        public void SaveTimePartOfDateTimeTo(Time t, IValueSaver valueSaver)
        {
            valueSaver.SaveAnsiString(t.ToString("HH:MM:SS") + ":000", 12, false);
        }

        public string GetTimeExtractionSyntaxFromDateTimeColumn()
        {
            return "CONVERT (CHAR, {0}, 114)";
        }
        EntityDataProviderFilterItemSaver _saver;
        public EntityDataProviderFilterItemSaver GetNoParameterFilterItemSaver(IDateTimeCollector dtc)
        {
            return _saver;
        }
    }
    public interface SqlCommandFilterBuilder : IFilterBuilder
    {
        string GetSql();
    }
    class SQLFilterConsumer : IFilterBuilder, SqlCommandFilterBuilder
    {

        public StringBuilder Result = new StringBuilder();
        SQLFilterHelper _client;

        bool _reuseParameter = true;

        public static string DisplayFilter(IFilter filter, bool isOracle)
        {

            var p = new NoParametersFilterItemSaver(!isOracle, isOracle ? OracleClientEntityDataProvider.DateTimeStringFormat : SQLClientEntityDataProvider.DateTimeStringFormat, DummyDateTimeCollector.Instance, isOracle ? OracleClientEntityDataProvider.DateTimeStringFormatForToString : null);
            var x =
                new SQLFilterConsumer(p, y => y.Name, false, new dummySqlFilterHelper(p));
            filter.AddTo(x);
            return x.Result.ToString();
        }
        public static string DisplayFilterInSingleLine(IFilter filter, bool isOracle)
        {
            var p = new NoParametersFilterItemSaver(!isOracle, isOracle ? OracleClientEntityDataProvider.DateTimeStringFormat : SQLClientEntityDataProvider.DateTimeStringFormat, DummyDateTimeCollector.Instance, isOracle ? OracleClientEntityDataProvider.DateTimeStringFormatForToString : null);
            var x =
                new SQLFilterConsumer(
                   p, y => y.Name, false, new dummySqlFilterHelper(p))
                { NewLinePrefix = "" };
            filter.AddTo(x);
            return x.Result.ToString();
        }

        public bool HasFilter
        {
            get { return Result.Length > 0; }
        }

        EntityDataProviderFilterItemSaver _source;
        Func<ColumnBase, string> _getColumnAlias;
        public bool HasOrsInIt { get; private set; }

        public bool HasAccentCharsInNonEqualOperators { get; set; }

        public SQLFilterConsumer(EntityDataProviderFilterItemSaver source, Func<ColumnBase, string> getColumnAlias, bool reuseParameter, SQLFilterHelper client)
        {
            _source = source;
            _getColumnAlias = getColumnAlias;
            _client = client;

            _reuseParameter = reuseParameter;
        }
        class FilterConsumerFilterItemSaver : IFilterItemSaver
        {
            EntityDataProviderFilterItemSaver _source;
            Func<ColumnBase, string> _getColumnAlias;
            public string TextForCommand;
            public string StringFilter;
            public FilterConsumerFilterItemSaver(EntityDataProviderFilterItemSaver source, Func<ColumnBase, string> getColumnAlias)
            {
                _source = source;
                _getColumnAlias = getColumnAlias;

            }
            public void SaveInt(int value)
            {
                _source.SaveInt(value);
                TextForCommand = _source.TextForCommand;
            }

            public void SaveDecimal(decimal value, byte precision, byte scale)
            {
                _source.SaveDecimal(value, precision, scale);
                TextForCommand = _source.TextForCommand;
            }

            public void SaveString(string value, int length, bool fixedWidth)
            {
                _source.SaveString(value, length, fixedWidth);
                TextForCommand = _source.TextForCommand;
                StringFilter = value;
            }

            public void SaveAnsiString(string value, int length, bool fixedWidth)
            {
                _source.SaveAnsiString(value, length, fixedWidth);
                TextForCommand = _source.TextForCommand;
                StringFilter = value;
            }

            public void SaveNull()
            {
                TextForCommand = "null";
                /*_source.SaveNull();
                TextForCommand = _source.TextForCommand;*/
            }

            public void SaveDateTime(DateTime value)
            {
                _source.SaveDateTime(value);
                TextForCommand = _source.TextForCommand;
            }

            public void SaveTimeSpan(TimeSpan value)
            {
                _source.SaveTimeSpan(value);
                TextForCommand = _source.TextForCommand;
            }

            public void SaveBoolean(bool value)
            {
                _source.SaveBoolean(value);
                TextForCommand = _source.TextForCommand;
            }


            public void SaveByteArray(byte[] value)
            {
                _source.SaveByteArray(value);
                TextForCommand = _source.TextForCommand;
            }

            public void SaveColumn(ColumnBase column)
            {
                TextForCommand = _getColumnAlias(column);
            }

            public void SaveEmptyDateTime()
            {
                _source.SaveEmptyDateTime();
                TextForCommand = _source.TextForCommand;
            }
        }
        class NewFilterConsumerFilterItemSaver : IFilterItemSaver, CanForceDateTime
        {
            SQLFilterConsumer _parent;
            ColumnBase _column;
            string _operator;
            public string StringFilter { get; private set; }
            public NewFilterConsumerFilterItemSaver(SQLFilterConsumer parent, ColumnBase column, string @operator)
            {
                _parent = parent;
                _column = column;
                _operator = @operator;
            }

            public void SaveInt(int value)
            {
                _parent._source.SaveInt(value);
                AddToWhere(_parent._source.TextForCommand);
            }

            void AddToWhere(string valueAfterParameter)
            {
                _parent.InternalAddWhere(false, "{0} " + _parent.OperatorPrefix + _operator + " " + valueAfterParameter.Replace("{", "{{").Replace("}", "}}"), _column);
            }

            public void SaveDecimal(decimal value, byte precision, byte scale)
            {
                _parent._source.SaveDecimal(value, precision, scale);
                AddToWhere(_parent._source.TextForCommand);

            }

            public void SaveString(string value, int length, bool fixedWidth)
            {
                _parent._source.SaveString(value, length, fixedWidth);
                AddToWhere(_parent._source.TextForCommand);
                StringFilter = value;

            }

            public void SaveAnsiString(string value, int length, bool fixedWidth)
            {
                _parent._source.SaveAnsiString(value, length, fixedWidth);
                AddToWhere(_parent._source.TextForCommand);
                StringFilter = value;
            }

            public void SaveNull()
            {
                if (_operator == "<>")
                    _parent.InternalAddWhere(false, "{0} is not null", _column);
                else
                    _parent.AddNullEquality(_column);
                /*_source.SaveNull();
                TextForCommand = _source.TextForCommand;*/
            }

            public void SaveDateTime(DateTime value)
            {
                if (_column is TimeColumn && ((TimeColumn)_column).DateColumnForDateTimeStorage != null)
                {
                    _parent._client.SaveTimePartOfDateTimeTo(Time.FromDateTime(value), _parent._source);

                    _parent.InternalAddWhere(false, _parent._client.GetTimeExtractionSyntaxFromDateTimeColumn() + " " + _parent.OperatorPrefix + _operator + " " + _parent._source.TextForCommand.Replace("{", "{{").Replace("}", "}}"), _column);
                    return;

                }
                _parent._source.SaveDateTime(value);
                AddToWhere(_parent._source.TextForCommand);
            }

            public void SaveTimeSpan(TimeSpan value)
            {
                _parent._source.SaveTimeSpan(value);
                AddToWhere(_parent._source.TextForCommand);
            }

            public void SaveBoolean(bool value)
            {
                _parent._source.SaveBoolean(value);
                AddToWhere(_parent._source.TextForCommand);
            }


            public void SaveByteArray(byte[] value)
            {
                _parent._source.SaveByteArray(value);
                AddToWhere(_parent._source.TextForCommand);
            }

            public void SaveColumn(ColumnBase column)
            {
                AddToWhere(_parent._getColumnAlias(column));

            }

            public void SaveEmptyDateTime()
            {
                _parent._source.SaveEmptyDateTime();
                AddToWhere(_parent._source.TextForCommand);
            }

            public void ForceDateTime2()
            {
                ((CanForceDateTime)_parent._source).ForceDateTime2();
            }
        }

        public void AddBetween(ColumnBase column, IFilterItem from, IFilterItem to)
        {
            if (from == to)
                _equalToColumns.Add(column);
            var nullTo = new NullAndDatetimeChecker();
            var nullFrom = new NullAndDatetimeChecker();

            from.SaveTo(nullFrom);
            to.SaveTo(nullTo);
            if (nullTo.IsNull || nullFrom.IsNull)
                AddNullEquality(column);

            else if (nullTo.HasDateTime && column is Firefly.Box.Data.TimeColumn)
            {
                _client.SaveTimePartOfDateTimeTo(Time.FromDateTime(nullFrom.TheDateTime), _source);
                var x = _source.TextForCommand;
                _client.SaveTimePartOfDateTimeTo(Time.FromDateTime(nullTo.TheDateTime), _source);
                InternalAddWhere(false,
                    _client.GetTimeExtractionSyntaxFromDateTimeColumn() + " " + OperatorPrefix + "between " + x.Replace("{", "{{").Replace("}", "}}") + " and " +
                    _source.TextForCommand.Replace("{", "{{").Replace("}", "}}"), column);
            }
            else
            {
                var toItem = new FilterConsumerFilterItemSaver(_source, _getColumnAlias);
                var fromItem = new FilterConsumerFilterItemSaver(_source, _getColumnAlias);
                from.SaveTo(fromItem);
                to.SaveTo(toItem);

                CheckAccent(fromItem.StringFilter);

                InternalAddWhere(false,
                    "{0} " + OperatorPrefix + "between " + fromItem.TextForCommand.Replace("{", "{{").Replace("}", "}}") + " and " +
                    toItem.TextForCommand.Replace("{", "{{").Replace("}", "}}"), column);
            }


        }

        void CheckAccent(string stringFilter)
        {
            if (!string.IsNullOrEmpty(stringFilter) && stringFilter.Contains("-"))
                HasAccentCharsInNonEqualOperators = true;

        }

        HashSet<ColumnBase> _equalToColumns = new HashSet<ColumnBase>();
        public string OperatorPrefix = "";

        public void AddEqualTo(ColumnBase column, IFilterItem item)
        {
            _equalToColumns.Add(column);
            AddOperator(column, "=", item, false);

        }

        public void AddDifferentFrom(ColumnBase column, IFilterItem item)
        {
            AddOperator(column, "<>", item, true);
        }


        void AddOperator(ColumnBase column, string op, IFilterItem to, bool checkAccentChars)
        {
            var newFilterConsumerFilterItemSaver = new NewFilterConsumerFilterItemSaver(this, column, op);
            to.SaveTo(newFilterConsumerFilterItemSaver);
            if (checkAccentChars)
            {
                CheckAccent(newFilterConsumerFilterItemSaver.StringFilter);
            }
        }


        void AddNullEquality(IFilterItem column)
        {
            InternalAddWhere(false, "{0} is null", column);
        }

        public void AddStartsWith(ColumnBase column, IFilterItem item)
        {
            var toItem = new FilterConsumerFilterItemSaver(_source, _getColumnAlias); ;
            var sw = new StartsWithFilterItemDecorator(toItem);
            item.SaveTo(sw);
            if (sw.PrepareBetween != null)
            {
                AddBetween(column, new byteFilterItem(sw.PrepareBetween.betweenFrom), new byteFilterItem(sw.PrepareBetween.betweenTo));
            }
            else if (toItem.TextForCommand == null)
            {
                AddNullEquality(column);
            }
            else
                InternalAddWhere(false, "{0} " + OperatorPrefix + "like " + toItem.TextForCommand, column);
        }

        class StartsWithFilterItemDecorator : IFilterItemSaver
        {
            IFilterItemSaver _saver;

            public StartsWithFilterItemDecorator(IFilterItemSaver saver)
            {
                _saver = saver;
            }



            public void SaveInt(int value)
            {
                var x = value.ToString().TrimEnd(' ');
                _saver.SaveString(x + "%", x.Length + 1, false);
            }

            public void SaveDecimal(decimal value, byte precision, byte scale)
            {
                var x = value.ToString(CultureInfo.InvariantCulture).TrimEnd(' ');
                _saver.SaveString(x + "%", x.Length + 1, false);
            }

            public void SaveString(string value, int length, bool fixedWidth)
            {
                _saver.SaveString(value.ToString().TrimEnd(' ').Replace("[", "[[]") + "%", length, fixedWidth);
            }

            public void SaveAnsiString(string value, int length, bool fixedWidth)
            {
                _saver.SaveAnsiString(value.ToString().TrimEnd(' ').Replace("[", "[[]") + "%", length, fixedWidth);
            }

            public void SaveNull()
            {
                _saver.SaveNull();
            }

            public void SaveDateTime(DateTime value)
            {
                var x = value.ToString().TrimEnd(' ');
                _saver.SaveString(x + "%", x.Length + 1, false);
            }

            public void SaveTimeSpan(TimeSpan value)
            {
                var x = value.ToString().TrimEnd(' ');
                _saver.SaveString(x + "%", x.Length + 1, false);
            }

            public void SaveBoolean(bool value)
            {
                SaveInt(value ? 1 : 0);
            }

            public PrepareBytes PrepareBetween;


            public void SaveByteArray(byte[] value)
            {
                PrepareBetween = new PrepareBytes(value);

            }

            public void SaveColumn(ColumnBase column)
            {
                _saver.SaveColumn(column);
            }

            public void SaveEmptyDateTime()
            {
                _saver.SaveEmptyDateTime();
            }
        }


        public void AddGreaterOrEqualTo(ColumnBase column, IFilterItem item)
        {
            AddOperator(column, ">=", item, true);
        }

        public void AddLessOrEqualTo(ColumnBase column, IFilterItem item)
        {
            AddOperator(column, "<=", item, true);
        }

        public void AddLessThan(ColumnBase column, IFilterItem item)
        {
            AddOperator(column, "<", item, true);
        }

        public void AddGreaterThan(ColumnBase column, IFilterItem item)
        {
            AddOperator(column, ">", item, true);
        }

        public string NewLinePrefix = " " + SQLDataProviderHelper.NewLineInSQL + " ";
        public void AddWhere(string filterText, params IFilterItem[] formatItems)
        {
            InternalAddWhere(true, filterText, formatItems);
        }
        void InternalAddWhere(bool dynamic, string filterText, params IFilterItem[] formatItems)
        {
            if (!_reuseParameter && formatItems.Length > 0)
            {
                var f = new FormatParser(filterText);
                var newArgs = new List<IFilterItem>();
                for (int i = 0; i < f.Tokens.Length; i++)
                {
                    newArgs.Add(formatItems[f.Tokens[i]]);
                    if (i != f.Tokens[i])
                    {
                        f.Tokens[i] = i;
                    }
                }
                formatItems = newArgs.ToArray();
                filterText = f.ToString();
            }
            var args = new object[formatItems.Length];
            for (int i = 0; i < formatItems.Length; i++)
            {
                var item = new FilterConsumerFilterItemSaver(dynamic && SQLClientEntityDataProvider.DoNotUseBoundParametersForDynamicWhere ? _client.GetNoParameterFilterItemSaver(DummyDateTimeCollector.Instance) : _source, _getColumnAlias);
                formatItems[i].SaveTo(item);
                if (filterText == "({0})" && item.StringFilter != null && item.StringFilter.TrimEnd() == "")
                    return;
                args[i] = item.TextForCommand;
            }
            if (Result.Length > 0)
                Result.Append(NewLinePrefix + " And ");
            if (args.Length > 0)
                filterText = string.Format(filterText, args);
            if (dynamic)
                filterText = PreprocessDynamicWhere(filterText);
            Result.Append(filterText);
        }
        internal static Func<string, string> PreprocessDynamicWhere = s => s;
        public void AddOr(IFilter a, IFilter b)
        {
            SQLFilterConsumer left = new SQLFilterConsumer(_source, _getColumnAlias, _reuseParameter, _client) { OperatorPrefix = OperatorPrefix },
                              right = new SQLFilterConsumer(_source, _getColumnAlias, _reuseParameter, _client) { OperatorPrefix = OperatorPrefix };
            a.AddTo(left);
            b.AddTo(right);
            if (!HasNonDbCondition && (left.HasNonDbCondition || right.HasNonDbCondition))
                HasNonDbCondition = true;
            if (left.Result.Length > 0 && right.Result.Length > 0)
            {
                InternalAddWhere(false, "(" + left.Result + NewLinePrefix + " Or " + right.Result + ")");
                HasOrsInIt = true;
            }
            else if (left.Result.Length > 0)
                InternalAddWhere(false, left.Result.ToString());
            else if (right.Result.Length > 0)
                InternalAddWhere(false, right.Result.ToString());


        }
        public bool HasNonDbCondition { get; private set; }
        public void AddTrueCondition()
        {
            HasNonDbCondition = true;
            InternalAddWhere(false, "1=1 /*Non Db Where*/");
        }

        public void AddLessOrEqualWithWildcard(Firefly.Box.Data.TextColumn column, Text value, IFilterItem filterItem)
        {
            _client.AddLessOrEqualWithWildcard(column, value, filterItem, this, OperatorPrefix);
        }

        public string GetSql()
        {
            if (!HasFilter)
                return null;
            return Result.ToString();
        }
    }
    class IsUniqeFilterClass : IFilterBuilder
    {

        public bool IsUniqueFor(Firefly.Box.Data.Entity entity, bool primaryKeyOnly)
        {
            bool uniqe = false;
            foreach (var primaryKeyColumn in entity.PrimaryKeyColumns)
            {
                uniqe = _equalToColumns.Contains(primaryKeyColumn);
                if (!uniqe)
                    break;
            }
            if (uniqe)
                return true;
            if (primaryKeyOnly)
                return false;
            foreach (var index in entity.Indexes)
            {
                if (index.Unique)
                {
                    foreach (var segment in index.Segments)
                    {
                        uniqe = _equalToColumns.Contains(segment.Column);
                        if (!uniqe)
                            break;
                    }
                    if (uniqe)
                        return true;
                }
            }
            return false;
        }
        HashSet<ColumnBase> _equalToColumns = new HashSet<ColumnBase>();
        public void AddBetween(ColumnBase column, IFilterItem from, IFilterItem to)
        {
            if (from == to)
                _equalToColumns.Add(column);
        }

        public void AddDifferentFrom(ColumnBase column, IFilterItem item)
        {

        }

        public void AddEqualTo(ColumnBase column, IFilterItem item)
        {
            _equalToColumns.Add(column);
        }

        public void AddGreaterOrEqualTo(ColumnBase column, IFilterItem item)
        {

        }

        public void AddGreaterThan(ColumnBase column, IFilterItem item)
        {
        }

        public void AddLessOrEqualTo(ColumnBase column, IFilterItem item)
        {
        }

        public void AddLessOrEqualWithWildcard(Firefly.Box.Data.TextColumn column, Text value, IFilterItem filterItem)
        {
        }

        public void AddLessThan(ColumnBase column, IFilterItem item)
        {
        }

        public void AddOr(IFilter a, IFilter b)
        {
        }

        public void AddStartsWith(ColumnBase column, IFilterItem item)
        {
        }

        public void AddTrueCondition()
        {
        }

        public void AddWhere(string filterText, params IFilterItem[] formatItems)
        {
        }
    }

    class NullAndDatetimeChecker : IFilterItemSaver
    {
        public bool IsNull = false;
        public void SaveInt(int value)
        {
        }

        public void SaveDecimal(decimal value, byte precision, byte scale)
        {
        }

        public void SaveString(string value, int length, bool fixedWidth)
        {
        }

        public void SaveAnsiString(string value, int length, bool fixedWidth)
        {
        }

        public void SaveNull()
        {
            IsNull = true;
        }

        public DateTime TheDateTime;
        public bool HasDateTime;

        public void SaveDateTime(DateTime value)
        {
            HasDateTime = true;
            TheDateTime = value;
        }

        public void SaveTimeSpan(TimeSpan value)
        {

        }

        public void SaveBoolean(bool value)
        {
        }

        public void SaveByteArray(byte[] value)
        {
        }

        public void SaveColumn(ColumnBase column)
        {
        }

        public void SaveEmptyDateTime()
        {

        }
    }

    interface EntityDataProviderFilterItemSaver : IValueSaver
    {
        string TextForCommand { get; }
    }

    class NoParametersFilterItemSaver : EntityDataProviderFilterItemSaver, CanForceDateTime
    {
        public string Result;

        readonly bool _allowEmptyString;
        string _dateTimeFormatInSql;
        string _dateTimeFormatForToString;
        IDateTimeCollector _dtc;
        public bool EscapeString = false;
        public NoParametersFilterItemSaver(bool allowEmptyString, string dateTimeFormat, IDateTimeCollector dtc, string dateTimeFormatForToString = null)
        {
            if (dateTimeFormatForToString == null)
                dateTimeFormatForToString = "yyyy-MM-ddTHH:mm:ss";
            _dateTimeFormatForToString = dateTimeFormatForToString;
            _allowEmptyString = allowEmptyString;
            _dateTimeFormatInSql = dateTimeFormat;
            _dtc = dtc;
        }

        public void SaveInt(int value)
        {
            Result = value.ToString(CultureInfo.InvariantCulture);
        }

        public void SaveDecimal(decimal value, byte precision, byte scale)
        {
            Result = value.ToString(CultureInfo.InvariantCulture);
        }

        public virtual void SaveString(string value, int length, bool fixedWidth)
        {
            if (value == null)
                return;
            var s = value.ToString().TrimEnd(' ');
            if (EscapeString)
                s = s.Replace("\\", "\\\\");
            if (s.Length == 0 && !_allowEmptyString)
                Result = "N' '";
            else
                Result = "N'" + s.Replace("'", "''") + "'";
        }

        public virtual void SaveAnsiString(string value, int length, bool fixedWidth)
        {
            if (value == null)
                return;
            var s = value.ToString();
            if (!fixedWidth)
                s = s.TrimEnd(' ');
            if (EscapeString)
                s = s.Replace("\\", "\\\\");
            if (s.Length == 0 && !_allowEmptyString)
                Result = "' '";
            else
                Result = "'" + s.Replace("'", "''") + "'";
        }

        public void SaveNull()
        {
            Result = null;
        }

        public virtual void SaveDateTime(DateTime value)
        {
            Result = string.Format(_dateTimeFormatInSql, value.ToString(_dateTimeFormatForToString));
            _dtc.SetDateTime(value);

        }

        public virtual void SaveTimeSpan(TimeSpan value)
        {
            Result = string.Format("'" + value.ToString()) + "'";
        }

        public virtual void SaveBoolean(bool value)
        {

            SaveInt(value ? 1 : 0);

        }

        public virtual void SaveByteArray(byte[] value)
        {
            if (value == null)
                return;
            var sb = new StringBuilder();
            sb.Append("0x");
            foreach (byte b in value)
            {

                sb.Append(b.ToString("x").PadLeft(2, '0'));
            }
            Result = sb.ToString();
        }

        public void SaveEmptyDateTime()
        {
            Result = "EmptyDate";
        }

        public void ForceDateTime2()
        {

        }

        public string TextForCommand
        {
            get { return Result; }
        }
    }


    internal class DataReaderValueLoader : IValueLoader,CanForceDateTime
    {
        protected IDataReader _reader;
        protected int _position;
        protected IDateTimeCollector _dtc;

        public DataReaderValueLoader(IDataReader reader, int position, IDateTimeCollector dtc)
        {
            _reader = reader;
            _position = position;
            _dtc = dtc;
        }

        public bool IsNull()
        {
            return _reader.IsDBNull(_position);
        }

        public Number GetNumber()
        {
            try
            {
                Number result;
                object o = _reader[_position];
                if (Number.TryCast(o, out result))
                {
                    return result;
                }
                if (o is bool)
                    return (bool)o ? 1 : 0;

                var s = o as string;

                if (s != null && s.Trim() != "")
                    return decimal.Parse(s, CultureInfo.InvariantCulture);
            }
            catch
            {
            }
            return Number.Zero;
        }

        public string GetString()
        {
            var o = _reader[_position];
            return GetStringFromReaderObject(o);

        }

        internal static string GetStringFromReaderObject(object o)
        {
            var s = o as string;
            if (s != null)
                return s;
            if (o is Guid)
                return "{" + o.ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
            else if (o is DateTime)
            {
                return ((DateTime)o).ToString("yyyy-MM-dd HH:mm:ss");
            }
            {
                var ba = o as byte[];
                if (ba != null)
                {
                    var result = new StringBuilder();
                    foreach (var b in ba)
                    {
                        result.Append(b.ToString("X").PadLeft(2, '0'));
                    }
                    return result.ToString();

                }
            }

            return Text.Cast(o);
        }

        public DateTime GetDateTime()
        {
            var r = _reader.GetDateTime(_position);
            _dtc.SetDateTime(r);
            return r;
        }

        public TimeSpan GetTimeSpan()
        {
            return (TimeSpan)_reader[_position];
        }

        public bool GetBoolean()
        {
            var value = _reader[_position];
            Number r2;
            if (Number.TryCast(value, out r2))
                return r2 != Number.Zero;
            var s = value as string;
            if (!string.IsNullOrEmpty(s))
            {

                return s[0] == '1' || s[0] == (char)1 || s.ToLowerInvariant()=="true";
            }
            return Bool.Cast(value);
        }

        public byte[] GetByteArray()
        {
            if (typeof(byte[]) == _reader.GetFieldType(_position) && _reader.GetBytes(_position, 0, null, 0, 0) == 0)
                return new byte[0];
            var result = _reader[_position];
            var ba = result as byte[];
            if (ba != null)
            {
                return ba;
            }
            var sa = result as string;
            if (sa != null)
            {
                return ByteArrayColumn.ToAnsiByteArray(sa);
            }
            return null;
        }

        public void ForceDateTime2()
        {
        }
    }

    class DbDataParameterFilterItemSaver : EntityDataProviderFilterItemSaver
    {
        IDbCommand _command;
        string _prefix;
        readonly bool _allowEmptyString;
        IDateTimeCollector _dtc;
        public DbDataParameterFilterItemSaver(IDbCommand command, string prefix, bool allowEmptyString, IDateTimeCollector dtc)
        {
            _dtc = dtc;
            _allowEmptyString = allowEmptyString;
            _command = command;
            _prefix = prefix;
        }

        public void SaveInt(int value)
        {
            var p = CreateParameter();
            p.DbType = DbType.Int32;
            p.Value = value;
        }
        protected IDbDataParameter CreateParameter()
        {
            var p = _command.CreateParameter();
            p.ParameterName = CreateParameterName(_command.Parameters.Count);
            _result = p.ParameterName;
            _command.Parameters.Add(p);
            return p;
        }
        protected virtual string CreateParameterName(int parameterCount)
        {
            return _prefix + parameterCount;
        }
        const decimal IntMaxValue = int.MaxValue, IntMinValue = int.MinValue;
        public void SaveDecimal(decimal value, byte precision, byte scale)
        {
            var x = CreateParameter();
            if (scale == 0)
            {
                if (value > IntMinValue && value < IntMaxValue)
                {
                    x.DbType = DbType.Int32;
                    x.Value = (int)value;
                }
                else
                {
                    x.DbType = DbType.Int64;
                    x.Value = (long)value;
                }

            }
            else
            {
                SaveDecimalNumber(x, value, precision, scale);
            }

        }

        protected virtual void SaveDecimalNumber(IDbDataParameter x, decimal value, byte precision, byte scale)
        {
            x.DbType = DbType.Decimal;

            x.Value = value;
            if (precision != 0)
            {
                if (precision > x.Precision)
                    x.Precision = precision;
                if (scale > x.Scale)
                {
                    x.Precision += (byte)(scale - x.Scale);
                    x.Scale = scale;
                }
            }
        }

        protected IDataParameter PrepareStringParameter(string value, int length, bool fixedWidth)
        {
            var p = CreateParameter();
            if (value == null)
                p.Value = System.DBNull.Value;
            else
            {
                string val = value;
                if (!fixedWidth)
                    val = val.TrimEnd(' ');

                if (val.Length == 0 && !_allowEmptyString)
                {
                    val = " ";
                }
                if (length > 0)
                    p.Size = length;
                else if (val.Length > 0)
                    p.Size = val.Length;
                else
                    p.Size = -1;
                p.Value = val;
            }
            return p;
        }

        public virtual void SaveString(string value, int length, bool fixedWidth)
        {
            PrepareStringParameter(value, length, fixedWidth).DbType = DbType.String;
        }

        public virtual void SaveAnsiString(string value, int length, bool fixedWidth)
        {
            PrepareStringParameter(value, length, fixedWidth).DbType = DbType.AnsiString;
        }

        public void SaveNull()
        {
            CreateParameter().Value = System.DBNull.Value;
        }

        public virtual void SaveDateTime(DateTime value)
        {
            var p = CreateParameter();
            p.DbType = GetDateTimeDbType(value);
            p.Value = value;
            _dtc.SetDateTime(value);
        }

        public virtual void SaveTimeSpan(TimeSpan value)
        {
            var p = CreateParameter();

            p.Value = value;
            if (p.DbType != DbType.Time)//donot remove - handles an sql server bug.
                p.DbType = DbType.Time;
        }

        protected virtual DbType GetDateTimeDbType(DateTime value)
        {
            return DbType.DateTime;
        }

        public virtual void SaveBoolean(bool value)
        {
            var p = CreateParameter();
            p.DbType = DbType.Int16;
            p.Value = value ? 1 : 0;
        }

        public void SaveByteArray(byte[] value)
        {
            var p = CreateParameter();
            p.DbType = DbType.Binary;
            if (value == null)
                p.Value = System.DBNull.Value;
            else
                p.Value = value;
        }

        public virtual void SaveEmptyDateTime()
        {
            throw new NotImplementedException("This database doesn't support dateempty");
        }

        string _result;
        public string TextForCommand
        {
            get { return _result; }
            protected set { _result = value; }
        }
    }
    class RowsCache
    {
        Queue<CachedRow> _rows;
        IEnumerable<ColumnBase> _selectedColumns;
        SQLDataProviderHelper.ColumnInSelect[] _columnsIndexes;
        bool _suppressRowLocks;
        public RowsCache(int capicity, IEnumerable<ColumnBase> selectedColumns, SQLDataProviderHelper.ColumnInSelect[] columnsIndexes, bool suppressRowLocks)
        {
            _suppressRowLocks = suppressRowLocks;
            _rows = new Queue<CachedRow>(capicity);
            _columnsIndexes = columnsIndexes;
            _selectedColumns = selectedColumns;
        }

        CachedRow _current;
        public bool Read()
        {
            if (_rows.Count > 0)
            {
                _current = _rows.Dequeue();
                return true;
            }
            return false;
        }

        class CachedRow
        {
            Dictionary<ColumnBase, IValueLoader> _columnData = new Dictionary<ColumnBase, IValueLoader>();
            bool _suppressRowLocks;
            Action _onUnLock;
            string _entityName;
            public CachedRow(Dictionary<ColumnBase, IValueLoader> columnData, bool suppressRowLocks, string entityName, Action onUnlock)
            {
                _entityName = entityName;
                _suppressRowLocks = suppressRowLocks;
                _columnData = columnData;
                _onUnLock = onUnlock;

            }





            public IValueLoader Load(ColumnBase column)
            {
                return _columnData[column];
            }

            public IRow GetRow(IRowStorage c, Firefly.Box.Data.Entity e, SQLDataProviderHelper helper, IEnumerable<ColumnBase> selectedColumns)
            {
                var dtc = new DateTimeCollector();
                foreach (var selectedColumn in selectedColumns)
                {
                    c.SetValue(selectedColumn, dtc.DecorateLoader(selectedColumn, Load(selectedColumn)));
                }
                return new SQLDataProviderHelper.SQLRow(helper, e, c, selectedColumns, _suppressRowLocks, _entityName, dtc, _onUnLock);
            }

            public IRow GetJoinedRow(IRowStorage c, Firefly.Box.Data.Entity e, SQLDataProviderHelper helper, IEnumerable<ColumnBase> columns)
            {
                foreach (var column in e.PrimaryKeyColumns)
                {
                    if (_columnData[column].IsNull())
                    {
                        return null;
                    }
                }
                var dtc = new DateTimeCollector();
                foreach (var columnBase in columns)
                {
                    c.SetValue(columnBase, dtc.DecorateLoader(columnBase, Load(columnBase)));
                }
                return new SQLDataProviderHelper.SQLRow(helper, e, c, columns, _suppressRowLocks, e.EntityName, dtc);
            }
        }




        public void AddRow(IDataReader reader, string entityName, SQLDataProviderHelperClient client, Action onUnLock = null)
        {
            var columnData = new Dictionary<ColumnBase, IValueLoader>();
            foreach (var column in _columnsIndexes)
            {
                var cd = new myValueLoader();
                if (reader.IsDBNull(column.IndexInSelect))
                    cd.SetToNull();
                else
                {
                    column.Column.LoadFrom(cd.TempLoader(client.GetDataReaderValueLoader(reader, column.IndexInSelect, DummyDateTimeCollector.Instance)));
                }
                columnData.Add(column.Column, cd);
            }
            _rows.Enqueue(new CachedRow(columnData, _suppressRowLocks, entityName, onUnLock));
        }
        class myValueLoader : IValueLoader, CanForceDateTime
        {
            bool _boolValue;
            byte[] _byteArray;
            DateTime _dateTime;
            string _string;
            Number _number;
            bool _isNull;
            TimeSpan _timeSpan;
            public IValueLoader TempLoader(IValueLoader orig)
            {
                return new childValueLoader(this, orig);
            }
            class childValueLoader : IValueLoader, CanForceDateTime
            {
                myValueLoader _parent;
                IValueLoader _orig;

                public childValueLoader(myValueLoader parent, IValueLoader orig)
                {
                    _parent = parent;
                    _orig = orig;
                }

                public void ForceDateTime2()
                {

                }

                public bool GetBoolean()
                {
                    return _parent._boolValue = _orig.GetBoolean();
                }

                public byte[] GetByteArray()
                {
                    return _parent._byteArray = _orig.GetByteArray();
                }

                public DateTime GetDateTime()
                {
                    return _parent._dateTime = _orig.GetDateTime();
                }

                public Number GetNumber()
                {
                    return _parent._number = _orig.GetNumber();
                }

                public string GetString()
                {
                    return _parent._string = _orig.GetString();
                }

                public TimeSpan GetTimeSpan()
                {
                    return _parent._timeSpan = _orig.GetTimeSpan();
                }

                public bool IsNull()
                {
                    return _parent._isNull = _orig.IsNull();
                }
            }

            public bool GetBoolean()
            {
                return _boolValue;
            }

            public byte[] GetByteArray()
            {
                return _byteArray;
            }

            public DateTime GetDateTime()
            {
                return _dateTime;
            }

            public Number GetNumber()
            {
                return _number;
            }

            public string GetString()
            {
                return _string;
            }

            public TimeSpan GetTimeSpan()
            {
                return _timeSpan;
            }

            public bool IsNull()
            {
                return _isNull;
            }

            internal void SetToNull()
            {
                _isNull = true;
            }

            public void ForceDateTime2()
            {

            }
        }

        internal IRow GetRow(IRowStorage c, Firefly.Box.Data.Entity e, SQLDataProviderHelper helper)
        {
            return _current.GetRow(c, e, helper, _selectedColumns);
        }

        internal IRow GetJoinedRow(Firefly.Box.Data.Entity e, IRowStorage c, SQLDataProviderHelper helper)
        {
            return _current.GetJoinedRow(c, e, helper, _selectedColumns);
        }
    }
    public class ValueLoaderAndSaver : IValueLoader, IValueSaver, IFilterItemSaver
    {
        object _value;

        public Number GetNumber()
        {
            return (Number)_value;
        }

        public string GetString()
        {
            return DataReaderValueLoader.GetStringFromReaderObject(_value);

        }

        public DateTime GetDateTime()
        {
            return (DateTime)_value;
        }

        public TimeSpan GetTimeSpan()
        {
            return (TimeSpan)_value;
        }

        public bool GetBoolean()
        {
            return (bool)_value;
        }

        public byte[] GetByteArray()
        {
            return (byte[])_value;
        }

        public bool IsNull()
        {
            return _value == null || _value == DBNull.Value;

        }

        public void SaveInt(int value)
        {
            _value = (Number)value;
        }

        public void SaveDecimal(decimal value, byte precision, byte scale)
        {
            _value = (Number)value;
        }

        public void SaveString(string value, int length, bool fixedWidth)
        {
            _value = value;
        }

        public void SaveAnsiString(string value, int length, bool fixedWidth)
        {
            SaveString(value, length, fixedWidth);
        }

        public void SaveNull()
        {
            _value = null;
        }

        public void SaveDateTime(DateTime value)
        {
            _value = value;
        }

        public void SaveTimeSpan(TimeSpan value)
        {
            _value = value;
        }

        public void SaveBoolean(bool value)
        {
            _value = value;
        }

        public void SaveByteArray(byte[] value)
        {
            _value = value;
        }

        public void SaveEmptyDateTime()
        {
            _value = Date.Empty;
        }

        public void SaveColumn(ColumnBase column)
        {
            column.SaveYourValueToDb(this);
        }
    }

    class FormatParser
    {
        int[] _tokens;
        string[] _strings;
        public FormatParser(string originalString)
        {
            new DoParse(new StringReader(originalString), this);

        }
        class DoParse
        {
            readonly List<string> _strings = new List<string>();
            readonly List<int> _tokens = new List<int>();
            StringBuilder _currentParsedValue = new StringBuilder();

            public DoParse(TextReader reader, FormatParser parent)
            {
                _procesChar = ReadString;
                int i;
                Finish = FinishString;
                while ((i = reader.Read()) != -1)
                {
                    _procesChar((char)i);
                }
                Finish();



                parent._strings = _strings.ToArray();
                parent._tokens = _tokens.ToArray();
            }

            Action Finish;
            void FinishString()
            {
                _strings.Add(_currentParsedValue.ToString());
            }

            void ReadString(char c)
            {
                switch (c)
                {
                    case '{':
                        _procesChar = FoundStartCandidate;
                        Finish = delegate
                        {
                            _currentParsedValue.Append('{');
                            FinishString();
                        };
                        break;
                    default:
                        _currentParsedValue.Append(c);
                        break;
                }
            }
            void FoundStartCandidate(char c)
            {

                switch (c)
                {
                    case '{':
                        _procesChar = ReadString;
                        Finish = FinishString;
                        _currentParsedValue.Append("{{");
                        break;
                    default:
                        _procesChar = ReadToken;
                        Finish = FinishString;
                        _strings.Add(_currentParsedValue.ToString());
                        _currentParsedValue = new StringBuilder();
                        _currentParsedValue.Append(c);
                        break;
                }
            }
            void ReadToken(char c)
            {
                switch (c)
                {
                    case '}':
                        _procesChar = FoundEndCandidate;
                        Finish = delegate
                        {
                            _tokens.Add(int.Parse(_currentParsedValue.ToString()));
                        };
                        break;
                    default:
                        _currentParsedValue.Append(c);
                        break;

                }
            }
            void FoundEndCandidate(char c)
            {

                switch (c)
                {
                    case '}':
                        _procesChar = ReadToken;
                        Finish = FinishString;
                        _currentParsedValue.Append("}}");
                        break;
                    default:
                        try
                        {
                            _tokens.Add(int.Parse(_currentParsedValue.ToString()));
                            _currentParsedValue = new StringBuilder();
                            _currentParsedValue.Append(c);
                            _procesChar = ReadString;
                            Finish = FinishString;
                        }
                        catch
                        {
                            throw new FormatException();
                        }
                        break;
                }
            }



            Action<char> _procesChar;

        }
        public int[] Tokens { get { return _tokens; } }
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < _strings.Length; i++)
            {
                if (i > 0)
                {
                    result.Append("{");
                    result.Append(_tokens[i - 1]);
                    result.Append("}");
                }
                result.Append(_strings[i]);
            }
            if (_strings.Length == _tokens.Length)
            {
                result.Append("{");
                result.Append(_tokens[_tokens.Length - 1]);
                result.Append("}");
            }
            return result.ToString();
        }
        public string ToString(object[] args)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < _strings.Length; i++)
            {
                if (i > 0)
                {
                    try
                    {
                        result.Append(args[_tokens[i - 1]]);
                    }
                    catch
                    {
                        throw new FormatException();
                    }
                }
                result.Append(_strings[i]);
            }
            if (_strings.Length == _tokens.Length)
            {
                result.Append(args[_tokens[_tokens.Length - 1]]);
            }
            return result.ToString();
        }
    }
    public interface IDontUseDbParameters { }

    public interface IDateTimeCollector
    {
        void SetDateTime(DateTime dt);
    }
    class DummyDateTimeCollector : IDateTimeCollector
    {
        public static IDateTimeCollector Instance = new DummyDateTimeCollector();
        public void SetDateTime(DateTime dt)
        {

        }
    }
    internal class DateTimeCollector
    {
        Dictionary<ColumnBase, DateTime> _columns;
        public DateTimeCollector()
        {
        }
        public void SetValue(IRowStorage c, ColumnBase col, SQLDataProviderHelperClient _client, IDataReader _reader, int indexInSelect)
        {
            c.SetValue(col, GetLoader(col, _client, _reader, indexInSelect));
        }
        public IValueLoader GetLoader(ColumnBase col, SQLDataProviderHelperClient _client, IDataReader _reader, int indexInSelect)
        {
            return _client.GetDataReaderValueLoader(_reader, indexInSelect, new myUpdate(this, col));
        }

        private void SetDateTime(ColumnBase col, DateTime r)
        {
            if (_columns == null)
            {
                _columns = new Dictionary<ColumnBase, DateTime>();
                _columns.Add(col, r);
            }
            else if (!_columns.ContainsKey(col))
                _columns.Add(col, r);
            else
                _columns[col] = r;
        }

        internal EntityDataProviderFilterItemSaver CreateFilterItemSaverForInsert(SQLDataProviderHelperClient _client, IDbCommand c, Firefly.Box.Data.Entity _entity, ColumnBase column, IRowStorage storage)
        {
            return _client.CreateFilterItemSaverForInsert(c, _entity, column, storage, new myUpdate(this, column));
        }
        class myValueLoader : IValueLoader, CanForceDateTime
        {
            IValueLoader _original;
            DateTimeCollector _parent;
            ColumnBase _col;
            public myValueLoader(IValueLoader original, DateTimeCollector parent, ColumnBase col)
            {
                _original = original;
                _parent = parent;
                _col = col;
            }

            public void ForceDateTime2()
            {
                ((CanForceDateTime)_original).ForceDateTime2();
            }

            public bool GetBoolean()
            {
                return _original.GetBoolean();
            }

            public byte[] GetByteArray()
            {
                return _original.GetByteArray();
            }

            public DateTime GetDateTime()
            {

                var r = _original.GetDateTime();
                _parent.SetDateTime(_col, r);
                return r;
            }

            public Number GetNumber()
            {
                return _original.GetNumber();
            }

            public string GetString()
            {
                return _original.GetString();
            }

            public TimeSpan GetTimeSpan()
            {
                return _original.GetTimeSpan();
            }

            public bool IsNull()
            {
                return _original.IsNull();
            }
        }
        internal UpdateContext GetUpdateContext()
        {
            return new UpdateContext(this);
        }

        internal IValue GetValue(IRowStorage _storage, ColumnBase columnBase)
        {
            DateTime dt;
            if (_columns != null && _columns.TryGetValue(columnBase, out dt))
            {
                return new DateTimeIValue(dt);
            }
            return _storage.GetValue(columnBase);
        }

        internal IValueLoader DecorateLoader(ColumnBase selectedColumn, IValueLoader valueLoader)
        {
            return new myValueLoader(valueLoader, this, selectedColumn);
        }

        class DateTimeIValue : IValue
        {
            DateTime _dt;
            public DateTimeIValue(DateTime dt)
            {
                _dt = dt;
            }

            public void SaveTo(IValueSaver saver)
            {
                saver.SaveDateTime(_dt);
            }
        }

        internal class UpdateContext
        {
            DateTimeCollector _parent;
            DateTimeCollector _clone;
            public UpdateContext(DateTimeCollector parent)
            {
                _parent = parent;
                _clone = new DateTimeCollector();
                if (_parent._columns != null)
                    _clone._columns = new Dictionary<ColumnBase, DateTime>(_parent._columns);
            }

            internal void Done()
            {
                _parent._columns = _clone._columns;
            }

            internal string ExtractValue(SQLDataProviderHelper.NonQueryCommand c, IValue current, ColumnBase col)
            {
                return c.ExtractValue(current, new myUpdate(_clone, col));
            }



        }
        class myUpdate : IDateTimeCollector
        {
            DateTimeCollector _dtc;
            ColumnBase _col;
            public myUpdate(DateTimeCollector dtc, ColumnBase col)
            {
                _dtc = dtc;
                _col = col;
            }
            public void SetDateTime(DateTime dt)
            {
                _dtc.SetDateTime(_col, dt);
            }
        }
    }
    class PrepareBytes
    {
        public byte[] betweenFrom, betweenTo;
        public PrepareBytes(byte[] value)
        {
            betweenFrom = new byte[value.Length];
            betweenTo = new byte[value.Length];
            int lastMeaningfulChar = -1;
            for (int j = value.Length - 1; j >= 0; j--)
            {
                if (value[j] != 32)
                {
                    lastMeaningfulChar = j + 1;
                    break;

                }
            }
            for (int i = 0; i < lastMeaningfulChar; i++)
            {
                betweenFrom[i] = betweenTo[i] = value[i];
            }
            for (int i = lastMeaningfulChar; i < value.Length; i++)
            {
                betweenFrom[i] = 0;
                betweenTo[i] = 255;
            }
        }
    }
    class byteFilterItem : IFilterItem
    {
        byte[] _bytes;
        public byteFilterItem(byte[] bytes)
        {
            _bytes = bytes;
        }

        public bool IsAColumn()
        {
            return false;
        }

        public void SaveTo(IFilterItemSaver saver)
        {
            saver.SaveByteArray(_bytes);
        }
    }

    class NoRows : IRowsReader
    {
        public void Dispose()
        {

        }

        public IRow GetJoinedRow(Firefly.Box.Data.Entity e, IRowStorage c)
        {
            throw new NotImplementedException();
        }

        public IRow GetRow(IRowStorage c)
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            return false;
        }
    }
    interface ITransactionDecorator
    {
        IDbTransaction GetDecoratedTransaction();
    }

}