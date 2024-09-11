using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using ENV.BackwardCompatible;
using ENV.Data.DataProvider;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.Data.UnderConstruction;
using SQLClientEntityDataProvider = ENV.Data.DataProvider.SQLClientEntityDataProvider;
using SortCollection = ENV.Data.SortCollection;

namespace ENV.Data
{
    public class Entity : Firefly.Box.Data.Entity
    {
        public static event Action<Entity, IEntityOnSavingRowEventArgs> EntitySavingRow;
        protected static string AppPrefix { get { return ENV.UserMethods.Instance.Pref(); } }
        public Entity(string entityName, string caption, IEntityDataProvider dataProvider)
            : base(entityName, caption, new EntityNameAcordingToCallStackEntityDataProviderDecorator(dataProvider))
        {
            _dataProvider = dataProvider;
            Init();
        }

        public Entity(string entityName, IEntityDataProvider dataProvider)
            : base(entityName, new EntityNameAcordingToCallStackEntityDataProviderDecorator(dataProvider))
        {
            _dataProvider = dataProvider;
            Init();
        }

        void Init()
        {
            _indexes = new SortCollection(base.Indexes);
            ShouldDetermineNameAcordingToCallStack = true;
            _autoCreateTable =
               () =>
               {
                   var sql = _dataProvider as ISQLEntityDataProvider;
                   if (sql != null && sql.AutoCreateTables)
                   {
                       AutoCreateTable = true;
                       return true;
                   }
                   return false;
               };
        }
        IEntityDataProvider _dataProvider;
        public IEntityDataProvider DataProvider
        {
            get
            {
                return _dataProvider;
            }
        }
        public bool KeepCacheAliveAfterExit { get; set; }
        internal bool _supressCacheClear = false;
        public override void ClearRelationCache()
        {
            if (!_supressCacheClear)
                base.ClearRelationCache();
        }
        public override string EntityName
        {
            get
            {
                return PathDecoder.DecodePath(base.EntityName);
            }
            set
            {if (value == null)
                    return;
                base.EntityName = value;
            }
        }
        /// <summary>
        /// When set to true, the Data will be loaded Once, and kept in memory. It can be cleared by using the ClearInMemoryData method
        /// </summary>
        public InMemoryScope InMemory { get; set; }

        public void ClearInMemoryData()
        {
            ENV.Data.DataProvider.EntityNameAcordingToCallStackEntityDataProviderDecorator.ClearInMemoryData(this);
        }

        protected override void OnSavingRow(IEntityOnSavingRowEventArgs e)
        {
            if (EntitySavingRow != null)
                EntitySavingRow(this, e);
            base.OnSavingRow(e);
        }

        SortCollection _indexes;
        public new SortCollection Indexes
        {
            get { return _indexes; }
        }
        public bool ShouldDetermineNameAcordingToCallStack { get; set; }

        Func<bool> _autoCreateTable = () => false;
        /// <summary>
        /// If set to true, whenever this table is queried, there will be a test if this table exists, and if not it'll be created.
        /// </summary>
        public bool AutoCreateTable
        {
            get { return _autoCreateTable(); }
            set { _autoCreateTable = () => value; }
        }
        INullStrategy _nullStrategy =  NullStrategy.GetStrategy(false);
        UserMethods _uInstance;
        protected UserMethods u
        {
            get
            {
                if (_uInstance == null)
                {
                    _uInstance = new UserMethods();
                    _nullStrategy.ApplyTo(_uInstance);
                }
                return _uInstance;
            }
        }
        public long ForEachRow(FilterBase where,Sort orderBy, Action what)
        {
            var bp = new BusinessProcess { From = this };
            if (where != null)
                bp.Where.Add(where);
            if (orderBy != null)
                bp.OrderBy = orderBy;
            return bp.ForEachRow(what);
        }
        public long ForEachRow(FilterBase where, Action what)
        {
            return ForEachRow(where,null,what);
        }
        public long ForEachRow(Action what)
        {
            return ForEachRow(null, null, what);
        }
        public void Delete(FilterBase where)
        {
            OperateOnRows(where, c =>
                                    {
                                        c.CommandText = "Delete" + c.CommandText;
                                        c.ExecuteNonQuery();
                                    }, r => r.Delete());
        }
        Dictionary<object, Dictionary<string, object>> _getCache =
            new Dictionary<object, Dictionary<string, object>>();
        public T GetValue<T>(TypedColumnBase<T> column, FilterBase where, Sort s = null)
        {
            string cacheKey = null;
            Dictionary<string, object> specificCache = null;
            cacheKey = FilterHelper.ToSQLWhere(where, false, this);

            if (!_getCache.TryGetValue(column, out specificCache))
            {
                specificCache = new Dictionary<string, object>();
                _getCache.Add(column, specificCache);
            }
            else
            {
                object cachedResult;
                if (specificCache.TryGetValue(cacheKey, out cachedResult))
                    return (T)cachedResult;
            }


            var i = new Iterator(this, column);
            T result;
            if (s != null)
            {
                bool valueFound;

                result = i.GetValue(column, where, s, out valueFound);
            }
            else
            {
                result = i.GetValue(column, where);
            }
            if (specificCache != null)
                specificCache.Add(cacheKey, result);
            return result;
        }
        bool _alreadyFixedTextStorage;
        internal void FixTextStorageForBtrieveAndMemory()
        {
            if (_alreadyFixedTextStorage)
                return;
            _alreadyFixedTextStorage = true;
            if (this is BtrieveEntity || this.DataProvider is MemoryDatabase)
            {
                for (int i = 0; i < Columns.Count; i++)
                {
                    var c = Columns[i] as TextColumn;
                    if (c == null)
                        continue;
                    if (c.StorageType == TextStorageType.Ansi && c.Storage is ENV.Data.Storage.AnsiStringTextStorageThatRemovesNullChars)
                    {
                        c.Storage = new Storage.AnsiStringTextStorage(c);
                    }
                }
            }
        }

        public T GetValue<T>(Func<T> expression, FilterBase where, params ColumnBase[] selectColumns)
        {
            return GetValue(expression, where, null, selectColumns);
        }
        public T GetValue<T>(Func<T> expression, FilterBase where, Sort s = null, params ColumnBase[] selectColumns)
        {
            string cacheKey = null;
            Dictionary<string, object> specificCache = null;
            object id = expression;
            cacheKey = FilterHelper.ToSQLWhere(where, false, this);

            if (!_getCache.TryGetValue(id, out specificCache))
            {
                specificCache = new Dictionary<string, object>();
                _getCache.Add(id, specificCache);
            }
            else
            {
                object cachedResult;
                if (specificCache.TryGetValue(cacheKey, out cachedResult))
                    return (T)cachedResult;
            }


            var bp = new BusinessProcess { };
            if (s == null)
                bp.Relations.Add(this, where);
            else
                bp.Relations.Add(this, where, s);
            if (selectColumns != null && selectColumns.Length > 0)
                bp.Columns.Add(selectColumns);

            T result = default(T);
            bp.ForFirstRow(() => result = expression());

            if (specificCache != null)
                specificCache.Add(cacheKey, result);
            return result;
        }
        public static string GetEntityName(Firefly.Box.Data.Entity entity)
        {

            var ee = entity as ENV.Data.Entity;
            if (ee != null)
            {
                var ds = ee.DataProvider as DynamicSQLSupportingDataProvider;
                if (ds != null)
                {
                    return ds.GetEntityName(entity);
                }
            }
            return entity.EntityName;

        }
        void OperateOnRows(FilterBase where, Action<IDbCommand> doOnSqlCommand, Action<Row> doOnRowForNonSQL)
        {
            var ds = DataProvider as DynamicSQLSupportingDataProvider;
            if (ds != null && !(ds is MockTestingDatabase))
            {
                using (var c = ds.CreateCommand())
                {
                    c.CommandText = " from " + ds.GetEntityName(this);
                    if (where != null)
                    {
                        var filter = ds.CreateSqlCommandFilterBuilder(c, this);
                        FilterBase.GetIFilter(where, false, this).AddTo(filter);
                        string whereString = filter.GetSql();
                        if (!string.IsNullOrEmpty(whereString))
                            c.CommandText += " where " + whereString;
                    }
                    doOnSqlCommand(c);


                }
            }
            else
            {
                var it = new Iterator(this);
                
                foreach (var row in where!=null? (Indexes.Count==0? it.SelectCommand(where): it.SelectCommand(where,Indexes[1])) :it.SelectCommand())
                {
                    doOnRowForNonSQL(row);
                }

            }
        }
        protected override bool CacheAFilterBasedOnTheseColumns(HashSet<ColumnBase> columns, Sort relationSort)
        {
            var idx = relationSort as Index;
            if (idx != null && idx.ColumnsAddedToAutomaticallyMakeSortUnique.Count > 0)
            {
                columns = new HashSet<ColumnBase>(columns);
                foreach (var c in idx.ColumnsAddedToAutomaticallyMakeSortUnique)
                {
                    columns.Add(c);
                }
            }
            return base.CacheAFilterBasedOnTheseColumns(columns, relationSort);
        }

        public void Insert(Action setValues)
        {
            new BusinessProcess { From = this, Activity = Activities.Insert }
                .ForFirstRow(() => setValues());
        }
        public void InsertIfNotFound(FilterBase where, Action setValues)
        {
            var bp = new BusinessProcess();
            bp.Relations.Add(this, RelationType.InsertIfNotFound, where);
            bp.ForFirstRow(() => setValues());
        }

        public void Update(FilterBase where, Action SetValues)
        {
            new BusinessProcess { From = this }.
                ForEachRow(where, SetValues);
        }

        public Bool Contains(FilterBase where)
        {
            return CountRows(where) != 0;
        }

        public Number CountRows(FilterBase where)
        {
            Number result = 0;
            OperateOnRows(where, c =>
                                     {
                                         c.CommandText = "select count(*)" + c.CommandText;
                                         result = Number.Cast(c.ExecuteScalar());
                                     }, y =>
                                     {
                                         result++;
                                     });
            return result ?? 0;
        }
        public Number Sum(Firefly.Box.Data.NumberColumn column)
        {
            return Sum(column, null);
        }

        public Number Sum(Firefly.Box.Data.NumberColumn column, FilterBase where)
        {
            Number result = 0;
            OperateOnRows(where, c =>
            {
                c.CommandText = string.Format("select sum ({0})", column.Name) + c.CommandText;
                result = Number.Cast(c.ExecuteScalar());
            }, y =>
                   {
                       result += y.Get(column);
                   });
            return result ?? 0;
        }
        public Number Max(Firefly.Box.Data.NumberColumn column)
        {
            return Max(column, null);
        }

        public Number Max(Firefly.Box.Data.NumberColumn column, FilterBase where)
        {
            Number result = null;
            OperateOnRows(where, c =>
            {
                c.CommandText = string.Format("select max ({0})", column.Name) + c.CommandText;
                result = Number.Cast(c.ExecuteScalar());
            }, y =>
            {
                var x = y.Get(column);
                if (result == null || x > result)
                    result = x;
            });
            return result ?? 0;
        }
        public Number Min(Firefly.Box.Data.NumberColumn column)
        {
            return Min(column, null);
        }
        protected override void AfterDisplayForm()
        {
            if (AfterDisplayFormEvent != null)
                AfterDisplayFormEvent();
        }
        internal event Action AfterDisplayFormEvent;

        public Number Min(Firefly.Box.Data.NumberColumn column, FilterBase where)
        {
            Number result = null;
            OperateOnRows(where, c =>
            {
                c.CommandText = string.Format("select min ({0})", column.Name) + c.CommandText;
                result = Number.Cast(c.ExecuteScalar());
            }, y =>
            {
                var x = y.Get(column);
                if (result == null || x < result)
                    result = x;
            });
            return result ?? 0;
        }

        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }
        internal void SetNullStrategy(INullStrategy instance)
        {
            _nullStrategy = instance;
            if (_uInstance != null)
                u.SetNullStrategy(instance);
        }

        internal void ClearGetValueCache()
        {
            _getCache.Clear();
        }
    }

    public class SqlEntity : Entity,  SQLClientEntityDataProvider.IEntityAditionalInfoForSQL
    {
        public SqlEntity(string entityName, string caption, IEntityDataProvider dataProvider)
            : base(entityName, caption, dataProvider)
        {
        }

        public SqlEntity(string entityName, IEntityDataProvider dataProvider)
            : base(entityName, dataProvider)
        {
        }
    
        public bool DisableFirstRowOptimization
        {
            get { return Cursor.DisableFirstRowOptimization; }
            set
            {
                Cursor.DisableFirstRowOptimization = value;
            }
        }

        SQLClientEntityDataProvider.CursorOptions _cursor = new SQLClientEntityDataProvider.CursorOptions();
        public SQLClientEntityDataProvider.CursorOptions Cursor
        {
            get { return _cursor; }
        }

    }


    public class ERROREntity : Entity
    {
        public Sort IndexOfEntityWithErrors = new Sort();
        public ERROREntity()
            : base("ERROR ENTITY", null)
        {
            throw new Exception("ERROR ENTITY");
        }
        public readonly NumberColumn ErrorField = new NumberColumn();
    }
    public class Index : Firefly.Box.Sort
    {
        public Index(params ColumnBase[] columns)
            : base(columns)
        {

        }
        public bool ForceGridStartOnRowPositionTopRow { get; set; }

        public bool AutoCreate { get; set; }

        internal readonly List<ColumnBase> ColumnsAddedToAutomaticallyMakeSortUnique = new List<ColumnBase>();

    }

    public enum InMemoryScope
    {
        None,
        Shared,
        Context
    }
    public class IsamEntity : Entity
    {
        public IsamEntity(string entityName, string caption, IEntityDataProvider dataProvider)
            : base(entityName, caption, dataProvider)
        {
        }

        public IsamEntity(string entityName, IEntityDataProvider dataProvider)
            : base(entityName, dataProvider)
        {
        }
        Sort _fetchIndex;

        public Sort FetchIndex
        {
            get { return _fetchIndex; }
            set { _fetchIndex = value; }
        }
        
    }
}