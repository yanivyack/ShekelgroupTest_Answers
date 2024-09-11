using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using System.Windows.Forms;
using ENV.Data.Storage;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.Data.UnderConstruction;
using Time = Firefly.Box.Time;

namespace ENV.Data.DataProvider
{
    class RowBridgeToTargetRow : TableMigrator.TargetRow
    {
        Row _row;

        public RowBridgeToTargetRow(Row row)
        {
            _row = row;
        }

        public void Set<T>(TypedColumnBase<T> column, T value)
        {
            _row.Set(column, value);
        }
    }
    class DummySqlDataProvider : ISQLEntityDataProvider
    {
        IEntityDataProvider _dp;
        public bool AutoCreateTables { get; set; }
        public DummySqlDataProvider(IEntityDataProvider dp)
        {
            _dp = dp;
        }
        bool ISQLEntityDataProvider.IsClosed()
        {
            return false;
        }
        public SqlCommandFilterBuilder CreateSqlCommandFilterBuilder(IDbCommand command, Entity entity)
        {
            return null;
        }
        public ISupportsGetDefinition GetSupportGetDefinition()
        {
            return _dp as ISupportsGetDefinition;
        }
        public void Dispose()
        {
            _dp.Dispose();
        }

        public IRowsSource ProvideRowsSource(Firefly.Box.Data.Entity entity)
        {
            return _dp.ProvideRowsSource(entity);
        }

        public ITransaction BeginTransaction()
        {
            return _dp.BeginTransaction();
        }

        public bool Contains(Firefly.Box.Data.Entity entity)
        {
            return _dp.Contains(entity);
        }

        public long CountRows(Firefly.Box.Data.Entity entity)
        {
            return _dp.CountRows(entity);
        }

        public void Drop(Firefly.Box.Data.Entity entity)
        {
            _dp.Drop(entity);
        }

        public void Truncate(Firefly.Box.Data.Entity entity)
        {
            _dp.Truncate(entity);
        }

        public bool SupportsTransactions
        {
            get { return _dp.SupportsTransactions; }
        }
        public bool RequiresTransactionForLocking
        {
            get
            {
                return _dp.RequiresTransactionForLocking;
            }
        }

        public IDbCommand CreateCommand()
        {
            throw new NotImplementedException();
        }

        public string GetEntityName(Firefly.Box.Data.Entity entity)
        {
            return entity.EntityName;
        }

        public Exception ProcessException(Exception e, Firefly.Box.Data.Entity entity, IDbCommand c)
        {
            throw e;
        }

        public bool IsOracle { get { return false; } }
        public SqlScriptTableCreator CreateScriptGeneratorTable(Firefly.Box.Data.Entity entity)
        {
            throw new NotImplementedException();
        }

        public UserDbMethods.IUserDbMethodImplementation GetUserMethodsImplementation()
        {
            return new UserDbMethods.Default();
        }

        public IValueLoader GetDataReaderValueLoader(IDataReader reader, int columnIndexInSelect, IDateTimeCollector dtc)
        {
            throw new NotImplementedException();
        }
    }

    public interface MigrationToSQLHelper
    {
        void UseOriginalDB();
        Entity CreateTargetSQLBasedOn(Type type, Entity source);
        Entity CreateTargetMemBasedOn(Type type, Entity source, DataSetDataProvider memdb);
        void AddEntity(Type type, Action<Entity> addEntity);
        void SwitchToSql();
        string GetUniqueIdentifierForDuplicateMigrationPrevention(Entity e);
        void InsertToDatabase(Entity targetSql, DataTable dataTable);
    }
    public class BtrieveMigrationToSQLHelper : MigrationToSQLHelper
    {

        public void UseOriginalDB()
        {
            BtrieveMigration.UseBtrieve = true;
        }
        const string lockReference = "CreateMemTable";
        public Entity CreateTargetSQLBasedOn(Type type, Entity source)
        {
            lock (lockReference)
            {
                var result = Activator.CreateInstance(type) as BtrieveEntity;
                result.SqlSchema = ((BtrieveEntity)source).SqlSchema;
                result.BtrieveName = ((BtrieveEntity)source).BtrieveName;
                if (result.DataProvider is BtrieveDataProvider)
                    throw new Exception("Btrieve table that has no SQL database");
                return result;
            }
        }

        public Entity CreateTargetMemBasedOn(Type type, Entity source, DataSetDataProvider memdb)
        {
            lock (lockReference)
            {
                BtrieveMigration.MockDB.Value = memdb;
                Action afterCreate = () => { };
                if (BtrieveEntity.UseOracleRowIdAsIdentityInSQL)
                {
                    var x = BtrieveEntity.CreateRowIdColumn;
                    BtrieveEntity.CreateRowIdColumn = () => null;
                    afterCreate = () => BtrieveEntity.CreateRowIdColumn = x;
                }
                var targetMem = Activator.CreateInstance(type) as BtrieveEntity;
                afterCreate();
                targetMem.BtrieveName = ((BtrieveEntity)source).BtrieveName;
                BtrieveMigration.MockDB.Value = null;
                return targetMem;
            }
        }

        HashSet<string> _migratedNames = new HashSet<string>();
        public void AddEntity(Type type, Action<Entity> addEntity)
        {
            var e = Activator.CreateInstance(type) as BtrieveEntity;
            if (e != null && e.DataProvider is BtrieveDataProvider && e.ShoudBeMigrated)
            {
                if (!BtrieveEntity.UseOracleRowIdAsIdentityInSQL)
                    e.Columns.Add(new BtrievePositionColumn());


                Action<BtrieveEntity> addTheEntity = x =>
                {

                    var p =
                        x.GetFullBtrievePathAndName();
                    if (_migratedNames.Contains(p))
                        return;
                    _migratedNames.Add(p);
                    addEntity(x);
                };
                string[] pathes;
                string sqlScehma = e.SqlSchema ?? "";
                if (string.IsNullOrEmpty(sqlScehma) && e.BtrieveName.StartsWith("%"))
                {
                    var i = e.BtrieveName.IndexOf("%", 1) + 1;
                    sqlScehma = e.BtrieveName.Substring(0, i);
                    e.BtrieveName = e.BtrieveName.Substring(i);
                }

                if (BtrieveMigration._divertingLogicalNames.TryGetValue(sqlScehma, out pathes))
                {
                    foreach (var path in pathes)
                    {
                        var en = Activator.CreateInstance(type) as BtrieveEntity;
                        //en.SqlSchema = path;
                        en.BtrieveName = Path.Combine(path, e.BtrieveName);


                        addTheEntity(en);
                    }
                }
                else
                {

                    addTheEntity(e);
                }
            }
        }

        public void SwitchToSql()
        {
            BtrieveMigration.SwitchToSQL();
        }

        public string GetUniqueIdentifierForDuplicateMigrationPrevention(Entity e)
        {
            var x = e as BtrieveEntity;
            return x.GetFullBtrievePathAndName();
        }
        internal interface CanProvideConnection
        {
            SqlConnection GetConnection();
        }


        public void InsertToDatabase(Entity targetSql, DataTable dataTable)
        {
            BtrieveMigration.InsertToSQLDatabase(targetSql, dataTable);

        }
    }


    public class MigrationToSql
    {
        public static bool LogWarnings = false;
        public static bool LogCallStack = false;

        public static string LogFileName = "Migration.log";
        public static bool AppendLog = false;

        public static void MigrateToSQL(Action<Action<Type, int>> entityType, MigrationToSQLHelper helper)
        {
            var migratedTables = new HashSet<string>();
            BtrieveMigration.UseBtrieve = true;
            var x = new List<TableMigrator>();
            long totalRows = 0;

            string endMessage = "";
            using (var sw = new System.IO.StreamWriter(LogFileName, AppendLog, LocalizationInfo.Current.OuterEncoding) { AutoFlush = true })
            {
                var to1 = new ThreadOperator();
                var last = Environment.TickCount;
                using (var sourceTables = new StreamWriter("SourceTables.txt", AppendLog) { AutoFlush = true })
                {
                    entityType((z, number) =>
                    {

                        Action<Entity> addEntity =
                            entr =>
                            {


                                var n = helper.GetUniqueIdentifierForDuplicateMigrationPrevention(entr);
                                var m = new TableMigrator(z, entr, number, helper);
                                to1.AddAction(
                                    () =>
                                    {
                                        try
                                        {

                                            var rows = entr.CountRows();
                                            if (
                                                !migratedTables.Contains(n))
                                            {

                                                lock ("Counting")
                                                {
                                                    totalRows += rows;
                                                    m.SetTotalRows(rows, n);
                                                    sourceTables.WriteLine("{0} ({1})",
                                                                           m.ToString(), rows.ToString("###,###,###"));
                                                }


                                                Common.SetTemporaryStatusMessage(
                                                    "Calculate total records " +
                                                    totalRows.ToString("###,###,###") + " " +
                                                    entr.Caption);
                                                if (Environment.TickCount - last > 1000)
                                                {
                                                    Context.Current.Suspend(1);
                                                    last = Environment.TickCount;
                                                }
                                                migratedTables.Add(n);
                                            }
                                            else
                                                m.DoNotMigrate = true;
                                            lock ("ListAdd")
                                                x.Add(m);
                                        }
                                        catch (Exception e)
                                        {

                                            lock (sw)
                                            {
                                                sw.WriteLine(
                                                    string.Format("\t{0} ({1},{4})#{3} - {2}",
                                                                  entr.Caption,
                                                                  entr.GetType().FullName,
                                                                  e.Message, number,
                                                                  entr.EntityName));
                                                if (LogCallStack)
                                                    sw.WriteLine(e.StackTrace);
                                            }



                                        }
                                    });



                            };
                        helper.AddEntity(z, addEntity);

                    });

                    to1.StartThread();
                    to1.Join();
                    x.Sort((a, b) => b.GetTotalRows().CompareTo(a.GetTotalRows()));
                    helper.SwitchToSql();
                    long totalRowsDone = 0;
                    long newTotalRowsDone = 0;
                    var start = DateTime.Now;
                    var to2 = new ThreadOperator();
                    using (var targetWriter = new StreamWriter("TargetTables.txt", AppendLog) { AutoFlush = true })
                    {
                        foreach (var mx in x)
                        {
                            var m = mx;
                            if (mx.GetTotalRows() == 0)
                                continue;
                            to2.AddAction(() =>
                            {
                                long mid = 0;
                                var done = m.DoMigration(delegate (string s, long i)
                                {

                                    lock ("done")
                                    {
                                        totalRowsDone += i - mid;
                                        mid = i;
                                    }
                                    var duration = DateTime.Now - start;
                                    decimal progress =
                                        (decimal)(totalRowsDone) /
                                        totalRows;
                                    if (progress == 0)
                                        progress = (decimal)0.0000001;
                                    var ms =
                                        (long)
                                        (duration.Ticks / progress -
                                         duration.Ticks);
                                    ms -= ms % 10000000;
                                    var remains = new TimeSpan(ms);
                                    Common.SetTemporaryStatusMessage(
                                        string.Format(
                                            "{2}% - {3} - {0} -rows -  {1}({4})",
                                            s,
                                            i.ToString("###,###,###"),
                                            (int)(progress * 100),
                                            remains.ToString(),
                                            m.GetTotalRows().ToString("###,###,###")));
                                    Context.Current.Suspend(1);
                                }, error =>
                                {
                                    lock (sw)
                                    {
                                        sw.WriteLine(error);
                                    }
                                });

                                lock ("done")
                                {
                                    targetWriter.WriteLine("{0} ({1})", m, done.ToString("###,###,###"));

                                    totalRowsDone += done - mid;
                                    try
                                    {
                                        newTotalRowsDone += m.GetTotalRows();
                                    }
                                    catch (Exception e)
                                    {
                                        lock (sw)
                                        {
                                            sw.WriteLine("{0}, {1}", m, e.Message);
                                            if (LogCallStack)
                                                sw.WriteLine(e.StackTrace);
                                        }
                                    }

                                }
                            });


                        }

                        to1.Join();
                        to2.StartThread();

                        to2.Join();
                    }

                    endMessage = string.Format("Completed {0} out of {1}. Total Time: {2}",
                                               totalRowsDone.ToString("###,###,###"),
                                               totalRows.ToString("###,###,###"),
                                               DateTime.Now - start);

                    sw.WriteLine(endMessage);
                }

            }
            if (!Silence)
                MessageBox.Show(endMessage);

        }

        public static bool Silence { get; set; }




        public static bool CompareRowCount;
    }
    class TableMigrator
    {
        Type _type;
        Entity _source;
        int _number;
        public bool DoNotMigrate = false;
        MigrationToSQLHelper _helper;
        public override string ToString()
        {
            return _type.FullName + "{" + _number + "," + _fullBtrieveName + "}";
        }
        public TableMigrator(Type type, Entity source, int number, MigrationToSQLHelper helper)
        {
            _helper = helper;
            _type = type;
            _source = source;
            _source.ShouldDetermineNameAcordingToCallStack = false;
            _number = number;
        }

        internal interface TargetColumnUpdater
        {
            void Set(SourceRow source, TargetRow target, Action<string> reportMessage);
            void TransferData(IValueLoader loader, object[] rowValues);
            int ColumnIndex { get; }
        }

        internal interface TargetRow
        {
            void Set<T>(TypedColumnBase<T> column, T value);
        }

        internal interface SourceRow
        {
            T Get<T>(TypedColumnBase<T> column);
        }

        class TargetolumnUpdater<T> : TargetColumnUpdater
        {
            TypedColumnBase<T> _sourceColumn, _targetColumn;
            int _index;
            public int ColumnIndex { get { return _index; } }
            public TargetolumnUpdater(TypedColumnBase<T> sourceColumn, TypedColumnBase<T> targetColumn, int index)
            {
                _index = index;
                _sourceColumn = sourceColumn;
                _targetColumn = targetColumn;
            }

            public void Set(SourceRow source, TargetRow target, Action<string> reportMessage)
            {
                var x = FixValue(source.Get(_sourceColumn));

                target.Set(_targetColumn, x);
            }

            T FixValue(T val)
            {
                if (ReferenceEquals(val, null) && !_targetColumn.AllowNull)
                    val = _targetColumn.DefaultValue;
                return val;
            }

            public void TransferData(IValueLoader loader, object[] rowValues)
            {
                _targetColumn.Value = FixValue(_sourceColumn.Storage.LoadFrom(loader));
                _targetColumn.SaveYourValueToDb(new myValueSaver(rowValues, _index));
            }
        }
        private static int smaxValue = ushort.MaxValue + 1;
        class BtrievePositionTargetSetter : TargetColumnUpdater
        {
            NumberColumn _sourceColumn, _targetColumn;
            int _index;
            public int ColumnIndex { get { return _index; } }
            public BtrievePositionTargetSetter(NumberColumn sourceColumn, NumberColumn targetColumn, int index)
            {
                _index = index;
                _sourceColumn = sourceColumn;
                _targetColumn = targetColumn;
            }

            public void Set(SourceRow source, TargetRow target, Action<string> reportMessage)
            {
                var result = FixValue(source.Get(_sourceColumn));
                target.Set(_targetColumn, result);
            }

            Number FixValue(Number value)
            {
                var result = (value % smaxValue) * smaxValue + (int)Math.Floor((value.ToDecimal() / smaxValue));
                return result;
            }

            public void TransferData(IValueLoader loader, object[] rowValues)
            {
                _targetColumn.Storage.SaveTo(FixValue(_sourceColumn.Storage.LoadFrom(loader)), new myValueSaver(rowValues, _index));
            }
        }
        class NumberSetter : TargetColumnUpdater
        {
            NumberColumn _source, _target;
            Entity _entity;
            decimal _minVal, _maxVal;
            int _targetWholeDigits, _sourceDecimalDigits, _targetDecimalDigits;
            int _index;
            public int ColumnIndex { get { return _index; } }
            public NumberSetter(NumberColumn source, NumberColumn target, Entity entity, int index)
            {
                _index = index;
                _source = source;
                _target = target;
                _entity = entity;
                ResetMinMax();

            }
            void ResetMinMax()
            {
                _targetWholeDigits = _target.FormatInfo.WholeDigits;
                _targetDecimalDigits = _target.FormatInfo.DecimalDigits;
                _sourceDecimalDigits = _source.FormatInfo.DecimalDigits;
                _maxVal = (decimal)Math.Pow(10, _targetWholeDigits);
                _minVal = -_maxVal;
            }

            public void Set(SourceRow source, TargetRow target, Action<string> reportMessage)
            {
                var sourceValue = FixValue(reportMessage, source.Get(_source));
                target.Set(_target, sourceValue);
            }

            public void TransferData(IValueLoader loader, object[] rowValues)
            {
                _target.Storage.SaveTo(FixValue(delegate { }, _source.Storage.LoadFrom(loader)), new myValueSaver(rowValues, _index));
            }

            decimal FixValue(Action<string> reportMessage, Number value)
            {
                var sourceValue = System.Math.Round(value.ToDecimal(), _sourceDecimalDigits,
                    MidpointRounding.AwayFromZero);
                if (sourceValue < _minVal || sourceValue >= _maxVal)
                {
                    if (MigrationToSql.LogWarnings)
                        reportMessage(string.Format("Column {0} ({1}) value ({2}) Exceeds the format ({3})",
                            _target.Caption, _target.Name, sourceValue, _target.Format));
                    var s = sourceValue.ToString(CultureInfo.InvariantCulture);
                    var whole = s.IndexOf('.');
                    if (whole == -1)
                        whole = s.Length;
                    var f = whole.ToString();
                    if (_targetDecimalDigits > 0)
                        f += "." + _targetDecimalDigits;
                    _target.Format = f;
                    ResetMinMax();
                    var dp = ((DynamicSQLSupportingDataProvider)_entity.DataProvider);
                    var indexName = _entity.EntityName + "_pk";
                    if (indexName.IndexOf('.') > -1)
                        indexName = indexName.Substring(indexName.LastIndexOf('.') + 1);
                    try
                    {
                        dp.Execute(string.Format("drop index {0} on {1}", indexName, _entity.EntityName));
                    }
                    catch
                    {
                    }
                    dp.Execute(
                        string.Format(
                            "alter table {0} alter column  {1} decimal ({2},{3}) not null",
                            _entity.EntityName, _target.Name, _target.FormatInfo.Precision,
                            _target.FormatInfo.Scale));
                    {
                        var pkColumns = "";
                        foreach (var c in _entity.PrimaryKeyColumns)
                        {
                            if (pkColumns.Length > 0)
                                pkColumns += ", ";
                            pkColumns += c.Name;
                        }

                        if (pkColumns != "")
                            dp.Execute(string.Format("create unique index {0} on {2} ({1})", indexName, pkColumns,
                                _entity.EntityName));
                    }
                }
                return sourceValue;
            }
        }
        class TextSetter : TargetColumnUpdater
        {
            TextColumn _source, _target;
            int _index;
            public int ColumnIndex { get { return _index; } }
            public TextSetter(TextColumn source, TextColumn target, int index)
            {
                _index = index;
                _source = source;
                _target = target;
            }

            public void Set(SourceRow source, TargetRow target, Action<string> reportMessage)
            {
                var x = FixValue(source.Get(_source));
                target.Set(_target, x);
            }

            Text FixValue(Text value)
            {
                if (value == null)
                {
                    if (!_target.AllowNull)
                        value = _target.DefaultValue;
                }
                else
                    value = value.Replace("\0", " ");
                return value;
            }

            public void TransferData(IValueLoader loader, object[] rowValues)
            {
                var x = Text.Empty;
                try
                {
                    x = FixValue(_source.Storage.LoadFrom(loader));
                }
                catch
                {
                }
                _target.Storage.SaveTo(x, new myValueSaver(rowValues, _index));
            }
        }



        public long DoMigration(Action<string, long> rowsDone, Action<string> report)
        {
            long sourceRows = -1;
            Action<string> reportMessage =
                m => report(string.Format("\t{0} ({1},{4})#{3} - {2}", _source.Caption, _type.FullName, m, _number, _source.EntityName));
            Action<Exception> reportException =
                ex =>
                {

                    reportMessage(ex.Message);
                    if (MigrationToSql.LogCallStack)
                        report(ex.StackTrace);
                    var x = ex;
                    while (x.InnerException != null)
                    {
                        x = x.InnerException;
                        if (ex.Message != x.Message)
                        {
                            report("Inner Exception:" + x.Message);
                            if (MigrationToSql.LogCallStack)
                                report(x.StackTrace);
                        }
                    }
                    if (x != null)
                    {
                        var sql = x.Data["SQL"] as string;
                        if (!string.IsNullOrEmpty(sql))
                        {
                            report("sql: " + sql);
                        }
                    }
                };
            try
            {
                sourceRows = _source.CountRows();
                if (sourceRows == 0)
                    return 0;

                long bpCounter = 0;
                long duplicateIndexes = 0;
                Entity targetSql;



                targetSql = _helper.CreateTargetSQLBasedOn(_type, _source);

                targetSql.ShouldDetermineNameAcordingToCallStack = false;
                targetSql.AutoCreateTable = true;


                var memdb = new DataSetDataProvider();
                memdb.DataSet.CaseSensitive = true;
                Entity targetMem;


                targetMem = _helper.CreateTargetMemBasedOn(_type, _source, memdb);


                targetMem.Truncate();
                targetMem.ShouldDetermineNameAcordingToCallStack = false;
                targetMem.AutoCreateTable = false;
                rowsDone(_source.ToString() + " => " + targetSql.EntityName, 0);








                lock (targetSql.EntityName)
                {
                    if (targetSql.Exists())
                    {

                        long y = 0;
                        if (MigrationToSql.CompareRowCount)
                            y = targetSql.CountRows();
                        if ((sourceRows == y && MigrationToSql.CompareRowCount) || DoNotMigrate)
                        {

                            try
                            {
                                using (
                                    var c =
                                        ((DynamicSQLSupportingDataProvider)targetSql.DataProvider).CreateCommand())
                                {
                                    c.CommandText = "";
                                    foreach (var col in targetSql.Columns)
                                    {
                                        if (c.CommandText.Length != 0)
                                            c.CommandText += ", ";
                                        c.CommandText += col.Name;
                                    }
                                    c.CommandText = "select " + c.CommandText + " from " + targetSql.EntityName;
                                    using (c.ExecuteReader(CommandBehavior.SchemaOnly))
                                    {
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                reportException(e);
                            }
                            if (DoNotMigrate)
                                return 0;
                            return targetSql.CountRows();
                        }
                        if (MigrationToSql.CompareRowCount)
                            reportMessage(string.Format(
                                "Table truncated because of difference between btrieve rows ({0}) and sql rows ({1})",
                            sourceRows, y));
                        targetSql.Truncate();
                    }
                    else
                    {
                        new Iterator(targetSql).Select();
                    }
                }
                var sourceIterator = new Iterator(_source);
                var targetIterator = new Iterator(targetMem);




                var targetSetter = CreateTargetSetter(targetMem, _source, targetSql);
                int i = 0;


                foreach (var row in sourceIterator.SelectCommand())
                {
                    var targetRow = targetIterator.CreateRow();
                    var tr = new RowBridgeToTargetRow(targetRow);
                    if (i++ % 10000 == 0)
                        rowsDone(_source.ToString() + " => " + targetSql.EntityName, i);

                    var rr = new RowBridgeToSourceRow(row);
                    foreach (var targetColumnUpdater in targetSetter.Values)
                    {
                        targetColumnUpdater.Set(rr, tr, reportMessage);
                    }
                    try
                    {
                        targetRow.UpdateDatabase();
                    }
                    catch (DatabaseErrorException ex)
                    {
                        if (ex.ErrorType == DatabaseErrorType.DuplicateIndex)
                        {
                            duplicateIndexes++;
                            if (MigrationToSql.LogWarnings)
                                reportException(ex);
                        }
                        else
                            throw;

                    }

                    if (i % 100000 == 0)
                    {
                        _helper.InsertToDatabase(targetSql, memdb.DataSet.Tables[0]);
                        memdb.DataSet.Tables[0].Rows.Clear();
                    }
                }

                rowsDone(_source.ToString() + " => " + targetSql.EntityName, i);
                bpCounter = i;


                {
                    _helper.InsertToDatabase(targetSql, memdb.DataSet.Tables[0]);
                }
                memdb.DataSet.Tables.Clear();
                memdb.DataSet.Clear();
                memdb.DataSet.Dispose();
                if (bpCounter != sourceRows)
                    reportMessage(string.Format("Deference between counter {1} and source rows {0}", sourceRows.ToString("###,###"), bpCounter.ToString("###,###")));
                var targetRows = targetSql.CountRows();
                if (targetRows != sourceRows)
                    reportMessage(string.Format("Deference between Target Rows {1} and source rows {0}, duplicate rows - {2}", sourceRows.ToString("###,###"), targetRows.ToString("###,###"), duplicateIndexes.ToString("###,###")));
                _source = null;

                return sourceRows;
            }
            catch (Exception e)
            {
                reportException(e);
                return sourceRows;

            }


        }

        class DateTimeSetter : TargetColumnUpdater
        {
            TypedColumnBase<Date> _sourceColumn, _targetColumn;
            int _index;
            public int ColumnIndex { get { return _index; } }
            public DateTimeSetter(TypedColumnBase<Date> sourceColumn, TypedColumnBase<Date> targetColumn, int index)
            {
                _index = index;
                _sourceColumn = sourceColumn;
                _targetColumn = targetColumn;
            }

            public void Set(SourceRow source, TargetRow target, Action<string> reportMessage)
            {

            }

            DateTime FixValue(DateTime val)
            {
                if (ReferenceEquals(val, null) && !_targetColumn.AllowNull)
                    val = _targetColumn.DefaultValue.ToDateTime();
                return val;
            }

            public void TransferData(IValueLoader loader, object[] rowValues)
            {
                if (loader.IsNull())
                    rowValues[_index] = null;
                else
                    rowValues[_index] = FixValue(loader.GetDateTime());

            }
        }

        class DummySetter : TargetColumnUpdater
        {
            public int ColumnIndex { get { return -1; } }
            public void Set(SourceRow source, TargetRow target, Action<string> reportMessage)
            {

            }

            public void TransferData(IValueLoader loader, object[] rowValues)
            {

            }
        }

        internal static Dictionary<ColumnBase, TargetColumnUpdater> CreateTargetSetter(Entity targetMem, Entity source, Entity targetSQL)
        {
            var targetSetter = new Dictionary<ColumnBase, TargetColumnUpdater>();

            {

                int i = -1;
                int j = 0;
                foreach (var targetColumn in targetMem.Columns)
                {
                    i++;
                    if (targetColumn is UserMethods.NotIncludedInVarIndexCalculations)
                        continue;
                    if (source.Columns.Count > j)
                    {
                        var sourceColumn = source.Columns[j++];
                        {
                            var s = sourceColumn as TextColumn;
                            if (s != null)
                            {
                                targetSetter.Add(sourceColumn, new TextSetter(s, (TextColumn)targetColumn, i));
                                continue;
                            }
                        }
                        {
                            var s = sourceColumn as DateColumn;
                            if (s != null)
                            {
                                if (s.TimeColumnForDateTimeStorage != null)
                                    targetSetter.Add(sourceColumn, new DateTimeSetter(s, (DateColumn)targetColumn, i));
                                else
                                    targetSetter.Add(sourceColumn, new TargetolumnUpdater<Date>(s, (DateColumn)targetColumn, i));
                                continue;
                            }
                        }
                        {
                            var s = sourceColumn as TimeColumn;
                            if (s != null)
                            {
                                if (s.DateColumnForDateTimeStorage != null)
                                {
                                    targetSetter.Add(sourceColumn, new DummySetter());
                                    i--;
                                }
                                else
                                    targetSetter.Add(sourceColumn, new TargetolumnUpdater<Time>(s, (TimeColumn)targetColumn, i));
                                continue;
                            }
                        }
                        {
                            var s = sourceColumn as NumberColumn;
                            if (s != null)
                            {
                                var bp = s as BtrievePositionColumn;
                                if (bp != null)
                                {
                                    targetSetter.Add(sourceColumn, new BtrievePositionTargetSetter(bp, (NumberColumn)targetColumn, i));
                                }

                                else
                                    targetSetter.Add(sourceColumn, new TargetolumnUpdater<Number>(s,
                                        (NumberColumn)targetColumn, i));
                                continue;
                            }
                        }
                        {
                            var s = sourceColumn as BoolColumn;
                            if (s != null)
                            {
                                targetSetter.Add(sourceColumn, new TargetolumnUpdater<Bool>(s, (BoolColumn)targetColumn, i));
                                continue;
                            }
                        }
                        {
                            var s = sourceColumn as ByteArrayColumn;
                            if (s != null)
                            {
                                targetSetter.Add(sourceColumn, new TargetolumnUpdater<byte[]>(s, (ByteArrayColumn)targetColumn, i));
                                continue;
                            }
                        }
                        throw new Exception("Unknown column type");
                    }
                }
            }
            return targetSetter;
        }


        private long _totalRows = 0;
        private string _fullBtrieveName = "";
        public void SetTotalRows(long rows, string fullName)
        {
            _totalRows = rows;
            _fullBtrieveName = fullName;
        }

        public long GetTotalRows()
        {
            return _totalRows;
        }
    }

    class RowBridgeToSourceRow : TableMigrator.SourceRow
    {
        Row _row;

        public RowBridgeToSourceRow(Row row)
        {
            _row = row;
        }

        public T Get<T>(TypedColumnBase<T> column)
        {
            return _row.Get(column);
        }
    }

    class myValueSaver : IValueSaver, CanForceDateTime
    {
        object[] _parent;
        int _position;
        static FixDB2Chars _charFixes = new FixDB2Chars();
        public myValueSaver(object[] parent, int position)
        {

            _parent = parent;
            _position = position;
        }

        public void SaveInt(int value)
        {
            _parent[_position] = value;
        }

        public void SaveDecimal(decimal value, byte precision, byte scale)
        {
            _parent[_position] = value;
        }

        public void SaveString(string value, int length, bool fixedWidth)
        {
            if (!fixedWidth)
                value = value.TrimEnd(' ');
            if (value.Length == 0)
                value = " ";
            else
            {
                value = _charFixes.Fix(value);
            }
            _parent[_position] = value;
        }

        public void SaveAnsiString(string value, int length, bool fixedWidth)
        {
            SaveString(value, length, fixedWidth);
        }

        public void SaveNull()
        {
            _parent[_position] = DBNull.Value;
        }

        public void SaveDateTime(DateTime value)
        {
            _parent[_position] = value;
        }

        public void SaveTimeSpan(TimeSpan value)
        {
            _parent[_position] = value;
        }

        public void SaveBoolean(bool value)
        {
            _parent[_position] = value ? 1 : 0;
        }

        public void SaveByteArray(byte[] value)
        {
            _parent[_position] = (object)value ?? DBNull.Value;
        }

        public void SaveEmptyDateTime()
        {
            _parent[_position] = Date.Empty;
        }

        public void ForceDateTime2()
        {

        }
    }
    class FixDB2Chars
    {
        char[] _replace = new char[255];
        char _maxChar;
        public FixDB2Chars()
        {
            var x = System.Text.Encoding.GetEncoding(1252);
            foreach (var i in new byte[]{128,
130,
131,
132,
133,
134,
135,
136,
137,
138,
139,
140,
142,
145,
146,
147,
148,
149,
150,
151,
152,
153,
154,
155,
156,
158,
159
})
            {
                _maxChar = (char)i;
                _replace[i] = x.GetChars(new byte[] { i })[0];
            }
        }

        public string Fix(string s)
        {
            var r = s.ToCharArray();
            for (int i = 0; i < r.Length; i++)
            {
                var z = r[i];
                if (z <= _maxChar)
                {
                    var c = _replace[z];
                    if (c != char.MinValue)
                    {
                        r[i] = c;
                    }
                }
            }
            return new string(r);
        }
    }
}


