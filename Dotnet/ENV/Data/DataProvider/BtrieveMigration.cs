using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ENV.Utilities;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.UnderConstruction;
using ENV.Data.Storage;

namespace ENV.Data.DataProvider
{
    public class BtrieveMigration
    {
        public static ContextStatic<Firefly.Box.Data.DataProvider.IEntityDataProvider> MockDB = new ContextStatic<IEntityDataProvider>(() => null);
        public static bool UseBtrieve = true;



        public static IColumnStorageSrategy<Number> ShortNumberStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.ShortNumberStorage();
            else
                return null;
        }
        public static IColumnStorageSrategy<Number> IntNumberStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.IntNumberStorage();
            else
                return null;
        }



        public static IColumnStorageSrategy<Bool> ShortNumberBoolStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.ShortNumberBoolStorage();
            else
                return null;
        }
        public static IColumnStorageSrategy<Bool> ByteArrayBoolStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.ByteArrayBoolStorage();
            else
                return null;
        }
        public static IColumnStorageSrategy<Number> LegacyNumberStorage(int length, bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.LegacyNumberStorage(length);
            return null;
        }

        public static IColumnStorageSrategy<Number> UShortNumberStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.UShortNumberStorage();
            else
                return new ENV.Data.Storage.PositiveNumberStorage();
        }
        public static IColumnStorageSrategy<Number> StringNumberStorage(int length, int decimalDigits, bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.StringNumberStorage(length, decimalDigits);
            else
                return null;
        }
        public static IColumnStorageSrategy<Number> ZeroPadStringNumberStorage(int length, int decimalDigits, bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.ZeroPadStringNumberStorage(length, decimalDigits);
            else
                return null;
        }
        public static IColumnStorageSrategy<Number> CharacterNumberStorage(int length, int decimalDigits, bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.CharacterNumberStorage(length, decimalDigits);
            else
                return null;
        }
        public static IColumnStorageSrategy<Number> BtrieveNumericNumberStorage(int length, int decimalDigits, bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.BtrieveNumericNumberStorage(length, decimalDigits);
            else
                return null;
        }
        public static IColumnStorageSrategy<Number> UIntNumberStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.UIntNumberStorage();
            else
                return null;
        }
        public static IColumnStorageSrategy<byte[]> AnsiStringByteArrayStorage(bool isUsingBtrieve = false)
        {
            return new Storage.AnsiStringByteArrayStorage();
        }
        public static IColumnStorageSrategy<Number> ByteNumberStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.ByteNumberStorage();
            else
                return null;
        }
        public static IColumnStorageSrategy<Number> SByteNumberStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.SByteNumberStorage();
            else
                return null;
        }
        public static IColumnStorageSrategy<Number> SingleDecimalNumberStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.SingleDecimalNumberStorage();
            else
                return null;
        }
        public static IColumnStorageSrategy<Number> DoubleNumberStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.DoubleNumberStorage();
            else
                return null;
        }
        public static IColumnStorageSrategy<Text> ByteArrayTextStorage(int p, bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.ByteArrayTextStorage(p);
            else
                return null;
        }
        public static IColumnStorageSrategy<Text> AnsiStringTextStorageWithManualSize(int p, bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.AnsiStringTextStorageWithManualSize(p,true);
            else
                return null;
        }
        public static IColumnStorageSrategy<Text> HebrewOemByteArrayStorage(int p, bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new HebrewOemByteArrayStorage( new ENV.Data.Storage.ByteArrayTextStorage(p));
            else
                return null;
        }

        public static IColumnStorageSrategy<Text> LegacyMemoTextStorage(int p, bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.LegacyMemoTextStorage(p);
            else
                return null;
        }
        public static IColumnStorageSrategy<Text> NullTerminatorTextStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.NullTerminatorTextStorage();
            else
                return null;
        }
        public static IColumnStorageSrategy<Text> LStringTextStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.LStringTextStorage();
            else
                return null;
        }
        public static IColumnStorageSrategy<Time> StringTimeStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.StringTimeStorage();
            else
                return null;
        }
        public static IColumnStorageSrategy<Time> HMSHTimeStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.HMSHTimeStorage();
            else
                return null;
        }
        public static IColumnStorageSrategy<Time> LegacyTimeStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.LegacyTimeStorage();
            else
                return null;
        }
        public static IColumnStorageSrategy<Date> LegacyDateStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.LegacyDateStorage();
            else
                return new ENV.Data.Storage.StringDateStorage();
        }
        public static IColumnStorageSrategy<Date> BtrieveDateStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.BtrieveDateStorage();
            else
                return new ENV.Data.Storage.StringDateStorage();
        }
        public static IColumnStorageSrategy<Date> NumberDateStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.NumberDateStorage();
            else
                return new ENV.Data.Storage.StringDateStorage();
        }
        public static IColumnStorageSrategy<Date> ByteArrayDateStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.ByteArrayDateStorage();
            else
                return new ENV.Data.Storage.StringDateStorage();
        }

        public static IColumnStorageSrategy<Date> Number1901DateStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.Number1901DateStorage();
            else
                return new ENV.Data.Storage.StringDateStorage();
        }

        public static IColumnStorageSrategy<Number> PackedDecimalNumberStorage(int i, int decimalDigits, bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.PackedDecimalNumberStorage(i, decimalDigits);
            else
                return null;
        }
        public static IColumnStorageSrategy<Number> FloatDecimalNumberStorage(int i, bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.FloatDecimalNumberStorage(i);
            else
                return null;
        }
        public static IColumnStorageSrategy<Number> ExtendedFloatNumberStorage(bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.ExtendedFloatNumberStorage();
            else
                return null;
        }
        public static IColumnStorageSrategy<Number> CISAMDecimalNumberStorage(int i, bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.CISAMDecimalNumberStorage(i);
            else
                return null;
        }
        public static IColumnStorageSrategy<Number> FloatMSBasicNumberStorage(int i, bool isUsingBtrieve = false)
        {
            if (UseBtrieve || isUsingBtrieve)
                return new ENV.Data.Storage.FloatMSBasicNumberStorage(i);
            else
                return null;
        }
        public static void SwitchToSQL()
        {

            UseBtrieve = false;
        }

        class EntityEntry
        {
            public BtrieveEntity _entity;
            public int _index;

            public EntityEntry(BtrieveEntity entity, int index)
            {
                _entity = entity;
                _index = index;
            }

            public override string ToString()
            {
                return _entity.SqlName;
            }
        }
        public static void AnalyzeTables(params ApplicationControllerBase[] args)
        {
            var allTablesBtrieveName = new Dictionary<string, List<EntityEntry>>();
            var allTablesSQLName = new Dictionary<string, List<EntityEntry>>();

            foreach (var app in args)
            {
                foreach (var val in app.AllEntities._entities)
                {
                    var e = System.Activator.CreateInstance(val.Value) as BtrieveEntity;

                    if (e != null && e.ShoudBeMigrated)
                    {
                        var ee = new EntityEntry(e, val.Key);
                        {
                            var n = e.BtrieveName.Trim().ToUpper();
                            List<EntityEntry> l;
                            if (!allTablesBtrieveName.TryGetValue(n, out l))
                            {
                                l = new List<EntityEntry>();
                                allTablesBtrieveName.Add(n, l);
                            }
                            l.Add(new EntityEntry(e, val.Key));
                        }
                        {
                            var n = e.SqlName.Trim().ToUpper();
                            List<EntityEntry> l;
                            if (!allTablesSQLName.TryGetValue(n, out l))
                            {
                                l = new List<EntityEntry>();
                                allTablesSQLName.Add(n, l);
                            }
                            l.Add(new EntityEntry(e, val.Key));
                        }
                    }
                }
            }
            var multipleBtrieve = new Dictionary<string, List<EntityEntry>>();
            var multipleSql = new Dictionary<string, List<EntityEntry>>();

            {
                foreach (var allTable in allTablesBtrieveName)
                {
                    if (allTable.Value.Count > 1)
                        multipleBtrieve.Add(allTable.Key, allTable.Value);
                }
            }
            {
                foreach (var allTable in allTablesSQLName)
                {
                    if (allTable.Value.Count > 1)
                        multipleSql.Add(allTable.Key, allTable.Value);
                }
            }

            foreach (var t in multipleBtrieve)
            {
                var first = t.Value[0];
                for (int i = 1; i < t.Value.Count; i++)
                {
                    Action<string> writeReport =
                        x =>
                        {
                            Console.WriteLine();
                            Console.WriteLine("Entity:\r\n{2} (#{1}), {0}\r\n in reference to \r\n{5} (#{4}), {3}:",
                                t.Value[i]._entity,
                                t.Value[i]._index,
                                t.Value[i]._entity.GetType().FullName,
                                first._entity,
                                t.Value[0]._index,
                                first._entity.GetType().FullName);
                            writeReport = y => Console.WriteLine(y);
                            writeReport(x);
                        };
                    var current = t.Value[i]._entity;
                    if (current.Columns.Count > first._entity.Columns.Count)
                        writeReport(string.Format("Has more columns then first {0},{1}", current.Columns.Count,
                                                  first._entity.Columns.Count));
                    for (int j = 0; j < first._entity.Columns.Count; j++)
                    {
                        var fc = first._entity.Columns[j];
                        if (current.Columns.Count <= j)
                            writeReport(string.Format("Missing column {0}(#{1})", fc.Caption, j));
                        else
                        {
                            var cc = current.Columns[j];
                            if (cc.Name != fc.Name)
                                writeReport(
                                    string.Format("Column {0}(#{1}) has a deferent name ({2}) then the original ({3})",
                                                  cc.Caption, j, cc.Name, fc.Name));
                            var fv = fc.GetType().GetProperty("DefaultValue").PropertyType;
                            var cv = cc.GetType().GetProperty("DefaultValue").PropertyType;
                            if (fv != cv)
                                writeReport(
                                    string.Format(
                                        "Column {0} (#{1}) has a different return type ({2}) then the original ({3})",
                                        cc.Caption, j, cv.Name, fv.Name));
                        }

                    }
                }

            }
            Console.WriteLine();
            Console.WriteLine("SQL Duplicates:");
            foreach (var ms in multipleSql)
            {
                EntityEntry first = null;
                foreach (var entityEntry in ms.Value)
                {
                    if (first == null)
                        first = entityEntry;
                    else
                    {
                        if (first._entity.BtrieveName.ToUpper() != entityEntry._entity.BtrieveName.ToUpper())
                        {
                            Console.WriteLine("Entity:\r\n{2} (#{1}), {0}\r\n in reference to \r\n{5} (#{4}), {3}:",
                                entityEntry._entity,
                                entityEntry._index,
                                entityEntry._entity.GetType().FullName,
                                first._entity,
                                first._index,
                                first._entity.GetType().FullName);
                        }
                    }
                }
            }

        }
        public static void MigrateToSQL(ApplicationControllerBase app, params int[] number)
        {
            if (number.Length == 0)
                MigrateToSQL(new[] { app });
            else
                MigrationToSql.MigrateToSQL(x =>
                {
                    foreach (var i in number)
                    {
                        x(app.AllEntities._entities[i], i);
                    }

                }, new BtrieveMigrationToSQLHelper());
        }




        public static void MigrateToSQL(params ApplicationControllerBase[] args)
        {
            MigrationToSql.MigrateToSQL(x =>
            {
                foreach (var app in args)
                {
                    foreach (var e in app.AllEntities._entities)
                    {

                        x(e.Value, e.Key);
                    }
                }
            }, new BtrieveMigrationToSQLHelper());


        }

        internal static Dictionary<string, string[]> _divertingLogicalNames = new Dictionary<string, string[]>();
        public static void AddLogicalNameMap(string logicalName, params string[] pathes)
        {
            _divertingLogicalNames.Add(logicalName, pathes);
        }

        public static IEntityDataProvider GetDataProvider(string databaseEntryName)
        {
            if (MockDB.Value != null)
                return MockDB.Value;
            if (BtrieveMigration.UseBtrieve)
                return ConnectionManager.GetDataProvider(databaseEntryName);
            else
            {
                string key = "BtrieveMigrationDB" + databaseEntryName;
                IEntityDataProvider result = Firefly.Box.Context.Current[key] as IEntityDataProvider;
                if (result == null)
                {
                    result = CreateSQLDataProvider();
                    Firefly.Box.Context.Current[key] = result;
                }
                return result;
            }
        }


        public static Func<IEntityDataProvider> CreateSQLDataProvider;

        private static int smaxValue = ushort.MaxValue + 1;
        public static void ShowTableWithPosition(BtrieveEntity e)
        {
            var p = new BtrievePositionColumn() { Caption = "position" };
            e.Columns.Add(p);
            var eb = new EntityBrowser(e);
            eb.AddColumns(e.Columns.ToArray());
            var a = new NumberColumn() { Caption = "a" };
            a.BindValue(() => (p % smaxValue) * smaxValue + (int)(p / smaxValue));

            eb.AddColumns(a);
            eb.Run();


        }

        public static Action<Entity, DataTable> InsertToSQLDatabase =
            (Entity targetSql, DataTable dataTable) =>
            {
                System.Data.SqlClient.SqlConnection con = null;
                var ds = targetSql.DataProvider as DynamicSQLSupportingDataProvider;
                if (ds != null)
                    using (var c = ds.CreateCommand())
                    {
                        con = (System.Data.SqlClient.SqlConnection)c.Connection;
                    }
                var scc = targetSql.DataProvider as BtrieveMigrationToSQLHelper.CanProvideConnection;
                if (scc != null)
                    con = scc.GetConnection();



                using (var trans = con.BeginTransaction())
                {
                    var bc = new System.Data.SqlClient.SqlBulkCopy(con,
                                                                   System.Data.SqlClient.SqlBulkCopyOptions.
                                                                       KeepIdentity, trans);

                    bc.BulkCopyTimeout = 0;
                    bc.DestinationTableName = targetSql.EntityName;
                    bc.WriteToServer(dataTable);
                    trans.Commit();
                }
            };
    }
    public class ThreadOperator
    {

        private Queue<Action> _actions = new Queue<Action>();
        public void AddAction(Action what)
        {
            _actions.Enqueue(what);
        }

        private List<System.Threading.Thread> _threads = new List<Thread>();
        public static bool NoThreads;

        public void StartThread()
        {
            if (NoThreads)
            {
                DoThreadWork();
                return;
            }

            for (int j = 0; j < System.Environment.ProcessorCount; j++)
            {
                var c = Context.Current;
                var t = new System.Threading.Thread(
                    () =>
                    {
                        Context.Current.AttachToUIContext(c);
                        try
                        {

                            DoThreadWork();
                        }
                        catch (Exception)
                        {


                        }
                        finally
                        {
                            Context.Current.Dispose();
                        }


                    })
                { Name = "Worker" + j };

                _threads.Add(t);
                t.Start();
            }

        }
        void DoThreadWork()
        {
            while (_actions.Count > 0)
            {
                Action a;
                lock (_actions)
                {
                    if (_actions.Count > 0)
                        a = _actions.Dequeue();
                    else
                        return;
                }
                a();
            }
        }

        public void Join()
        {
            if (_threads.Count == 0)
            {
                while (_actions.Count > 0)
                    _actions.Dequeue()();
                return;
            }
            else
            {
                while (true)
                {
                    bool done = true;
                    foreach (var thread in _threads)
                    {
                        if (thread.ThreadState == ThreadState.Running)
                        {
                            done = false;
                            break;
                        }

                    }
                    if (!done)
                        Context.Current.Suspend(1);
                    else
                    {
                        _threads.Clear();
                        return;

                    }
                }
            }

        }
    }

    public class RowId : NumberColumn
    {
        public RowId()
            : base("rowid__", "15")
        {
            DbReadOnly = true;
        }
    }
}