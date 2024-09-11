using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.DataProvider
{
    public class MemoryDatabase : IEntityDataProvider
    {
        public MemoryDatabase()
        {
            DataSet.Tables.CollectionChanged += new System.ComponentModel.CollectionChangeEventHandler(Tables_CollectionChanged);

        }

        static bool _defaultCaseSensitive = true;
        public static bool DefaultCaseSensitive { get { return _defaultCaseSensitive; } set { _defaultCaseSensitive = value; DataSetDataProvider.IgnoreCase = !value; } }

        static bool _defaultOrdinalStringComparison = true;
        public static bool DefaultOrdinalStringComparison { get { return _defaultOrdinalStringComparison; } set { _defaultOrdinalStringComparison = value; } }

        void Tables_CollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
        {
            if (!DefaultOrdinalStringComparison) return;
            if (e.Action == System.ComponentModel.CollectionChangeAction.Add)
            {
                var dt = e.Element as DataTable;
                if (dt != null)
                {
                    var f = typeof(DataTable).GetField("_compareFlags",
                                              System.Reflection.BindingFlags.NonPublic |
                                              System.Reflection.BindingFlags.Instance);
                    f.SetValue(dt, _defaultCaseSensitive ? CompareOptions.Ordinal : CompareOptions.OrdinalIgnoreCase);
                }
            }
        }

        DataSetDataProvider _dataProvider = new DataSetDataProvider();
        public static MemoryDatabase Instance
        {
            get
            {
                var x = Firefly.Box.Context.Current["MemoryDatabase"] as MemoryDatabase;
                if (x == null)
                {
                    x = new MemoryDatabase();

                    //      x = new Firefly.Box.Data.DataProvider.UnderConstruction.InMemoryDatabase();
                    Firefly.Box.Context.Current["MemoryDatabase"] = x;
                }
                return x;
            }
        }

        ITransaction IEntityDataProvider.BeginTransaction()
        {
            return ((IEntityDataProvider)_dataProvider).BeginTransaction();
        }

        bool IEntityDataProvider.Contains(Firefly.Box.Data.Entity entity)
        {
            return ((IEntityDataProvider)_dataProvider).Contains(entity);
        }

        long IEntityDataProvider.CountRows(Firefly.Box.Data.Entity entity)
        {
            return ((IEntityDataProvider)_dataProvider).CountRows(entity);
        }



        void IEntityDataProvider.Truncate(Firefly.Box.Data.Entity entity)
        {
            ((IEntityDataProvider)_dataProvider).Truncate(entity);
        }

        public DataSet DataSet
        {
            get { return _dataProvider.DataSet; }
        }

        bool IEntityDataProvider.SupportsTransactions
        {
            get { return ((IEntityDataProvider)_dataProvider).SupportsTransactions; }
        }
        public bool RequiresTransactionForLocking
        {
            get
            {
                return _dataProvider.RequiresTransactionForLocking;
            }
        }

        public void Dispose()
        {
            _dataProvider.Dispose();
        }


        IRowsSource IEntityDataProvider.ProvideRowsSource(Firefly.Box.Data.Entity entity)
        {
            var result = new EntityInUse(entity, ((IEntityDataProvider)_dataProvider).ProvideRowsSource(entity), this);
            lock (_entitiesInUse)
                _entitiesInUse.Add(result);
            return result;
        }

        List<EntityInUse> _entitiesInUse = new List<EntityInUse>();
        class EntityInUse : IRowsSource
        {
            Firefly.Box.Data.Entity _entity;
            IRowsSource _original;
            MemoryDatabase _parent;

            public EntityInUse(Firefly.Box.Data.Entity entity, IRowsSource original, MemoryDatabase parent)
            {
                _entity = entity;
                _original = original;
                _parent = parent;
            }

            public void Dispose()
            {
                lock (_parent._entitiesInUse)
                    _parent._entitiesInUse.Remove(this);
                _original.Dispose();
            }

            public IRowsProvider CreateReader(IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort, IEnumerable<IJoin> joins, bool disableCache)
            {
                return _original.CreateReader(selectedColumns, where, sort, joins, disableCache);
            }

            public IRowsReader ExecuteReader(IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort, IEnumerable<IJoin> joins, bool lockAllRows)
            {
                return _original.ExecuteReader(selectedColumns, where, sort, joins, lockAllRows);
            }

            public IRowsReader ExecuteCommand(IEnumerable<ColumnBase> selectedColumns, IFilter filter, Sort sort, bool firstRowOnly, bool shouldBeOnlyOneRowThatMatchesTheFilter, bool lockAllRows)
            {
                return _original.ExecuteCommand(selectedColumns, filter, sort, firstRowOnly, shouldBeOnlyOneRowThatMatchesTheFilter, lockAllRows);
            }

            public IRow Insert(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, IRowStorage storage, IEnumerable<ColumnBase> selectedColumns)
            {
                return _original.Insert(columns, values, storage,selectedColumns);
            }

            public bool IsOrderBySupported(Sort sort)
            {
                return _original.IsOrderBySupported(sort) && (sort.Segments.Count == 0 || _entity.Indexes.IndexOf(sort) != -1 || (sort.Segments.Count == 1 && sort.Segments[0].Column == _entity.IdentityColumn));
            }

            public bool Is(string name)
            {
                return _entity.EntityName == name;
            }
        }
        void IEntityDataProvider.Drop(Firefly.Box.Data.Entity entity)
        {
            if (_dataProvider.DataSet.Tables.Contains(entity.EntityName))
                lock (_entitiesInUse)
                    foreach (var e in _entitiesInUse)
                    {
                        if (e.Is(entity.EntityName))
                            throw new EntityInUseAndCannotBeDroppedException(entity);
                    }
            ((IEntityDataProvider)_dataProvider).Drop(entity);
        }

        public class EntityInUseAndCannotBeDroppedException : Exception
        {
            public EntityInUseAndCannotBeDroppedException(Firefly.Box.Data.Entity e) : base(string.Format("Entity {0}({1}) in use and cannot be dropped", e.Caption, e.EntityName))
            {
            }
        }

        public void Clear()
        {
            _dataProvider = new DataSetDataProvider();
        }
    }

    public class MockTestingDatabase : DynamicSQLSupportingDataProvider
    {
        class MemoryMockDatabase : ISQLEntityDataProvider
        {
            IEntityDataProvider _source;
            public IEntityDataProvider Source { get { return _source; } }
            public MemoryMockDatabase(IEntityDataProvider source, bool isOracle)
            {
                _source = source;
                IsOracle = isOracle;
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
                return _source as ISupportsGetDefinition ;
            }
            public bool AutoCreateTables { get; set; }

            public ITransaction BeginTransaction()
            {
                return _source.BeginTransaction();
            }

            public bool Contains(Firefly.Box.Data.Entity entity)
            {
                return _source.Contains(entity);
            }

            public long CountRows(Firefly.Box.Data.Entity entity)
            {
                return _source.CountRows(entity);
            }

            public void Dispose()
            {
                _source.Dispose();
            }

            public void Drop(Firefly.Box.Data.Entity entity)
            {
                _source.Drop(entity);
            }

            public IRowsSource ProvideRowsSource(Firefly.Box.Data.Entity entity)
            {
                return _source.ProvideRowsSource(entity);
            }

            public bool SupportsTransactions
            {
                get { return _source.SupportsTransactions; }
            }
            public bool RequiresTransactionForLocking
            {
                get
                {
                    return _source.RequiresTransactionForLocking;
                }
            }

            public void Truncate(Firefly.Box.Data.Entity entity)
            {
                _source.Truncate(entity);
            }

            public IDbCommand CreateCommand()
            {
                throw new NotImplementedException();


            }

            public Exception ProcessException(Exception e, Firefly.Box.Data.Entity entity, IDbCommand c)
            {
                throw new NotImplementedException();
            }

            public bool IsOracle { get; private set; }
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

            public string GetEntityName(Firefly.Box.Data.Entity entity)
            {
                return entity.EntityName;
            }
        }

        MemoryMockDatabase _source;

        public MockTestingDatabase()
            : this(new MemoryMockDatabase(new MemoryDatabase(), false))
        {

        }

        public MockTestingDatabase(IEntityDataProvider dp, bool isOracle) : this(new MemoryMockDatabase(dp, isOracle))
        {

        }

        MockTestingDatabase(MemoryMockDatabase x)
            : base(x)
        {
            _source = x;


        }

        public DataSet DataSet { get { return ((DataSetDataProvider)_source.Source).DataSet; } }
    }
}