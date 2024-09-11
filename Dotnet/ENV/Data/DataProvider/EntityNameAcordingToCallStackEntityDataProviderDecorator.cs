using System;
using System.Collections.Generic;
using ENV.Data;
using Firefly.Box;
using Firefly.Box.Data;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;


namespace ENV.Data.DataProvider
{
    class EntityNameAcordingToCallStackEntityDataProviderDecorator : IEntityDataProvider
    {
        IEntityDataProvider _source;
        static IEntityDataProvider _inMemoryData = new MemoryDatabase();

        static ContextStatic<MemoryDatabase> _inMemoryContextData =
            new ContextStatic<MemoryDatabase>(() => new MemoryDatabase());
        internal static void ClearInMemoryData()
        {
            _inMemoryData = new DataSetDataProvider();
            _inMemoryContextData.Value.DataSet.Clear();
        }
        internal static void ClearInMemoryData(Entity e)
        {
            if (_inMemoryData.Contains(e))
                _inMemoryData.Drop(e);
        }

        public EntityNameAcordingToCallStackEntityDataProviderDecorator(IEntityDataProvider source)
        {
            _source = source;
        }

        public void Dispose()
        {
            _source.Dispose();
        }

        public bool RequiresTransactionForLocking
        {
            get
            {
                return _source.RequiresTransactionForLocking;
            }
        }
        public IRowsSource ProvideRowsSource(Firefly.Box.Data.Entity entity)
        {
            var envEntity = entity as Entity;
            if (envEntity != null && envEntity.ShouldDetermineNameAcordingToCallStack)
            {
                var tasks = Firefly.Box.Context.Current.ActiveTasks;
                bool foundEntity = false;
                bool first = true;
                CrossTaskCache tc = null;
                for (var q = 0; q < tasks.Count - 1; q++)
                {
                    var task = tasks[q];
                    var ent = task.Entities;
                    for (int i = 0; i < ent.Count; i++)
                    {
                        var e = ent[i];
                        if (e.GetType() == entity.GetType())
                        {
                            foundEntity = true;
                            var be = e as BtrieveEntity;
                            if (be != null)
                            {
                                var bEntity = (BtrieveEntity)entity;
                                bEntity.BtrieveName = be.BtrieveName;
                            }
                            else
                                entity.EntityName = ((Firefly.Box.Data.Entity)e).EntityName;
                            var xe = e as XmlEntity;
                            if (xe != null)
                            {
                                xe.SetSourceTo((XmlEntity)entity);
                            }
                            if (e.AllowRowLocking)
                                entity.AllowRowLocking = true;
                            envEntity.AutoCreateTable = false;


                        }
                    }
                   

                    if (foundEntity && envEntity.Cached && envEntity.KeepCacheAliveAfterExit)
                    {
                        ControllerBase.SendInstanceBasedOnTaskAndCallStack(
                           task,
                            r =>
                            {
                                if (!r.KeepChildRelationCacheAlive || first)
                                    tc = r._crossTaskCache;
                                first = false;
                            });
                    }
                }
                if (tc != null)
                {
                    tc.AddClearCacheOnEnd(envEntity);

                }
            }
            using (var r = ENV.Utilities.Profiler.OpenRowSource(entity))
            {
                return r.ProvideRowsSource(LoadEntity(entity).ProvideRowsSource(entity));
            }
        }
        class LoadInMemoryDataProvider : IEntityDataProvider
        {
            IEntityDataProvider _mem;
            Action _load;
            public LoadInMemoryDataProvider(IEntityDataProvider mem,Action load,ENV.Data.Entity entity)
            {
                _mem = mem;
                _load = load;
                entity.AfterDisplayFormEvent += DoLoad;

            }
            bool _loaded = false;
            private void DoLoad()
            {
                if (_loaded)
                    return;
                _loaded = true;
                _load();
            }

            public bool SupportsTransactions { get { return _mem.SupportsTransactions; } }
            public bool RequiresTransactionForLocking
            {
                get
                {
                    return _mem.RequiresTransactionForLocking;
                }
            }

            public ITransaction BeginTransaction()
            {
                return _mem.BeginTransaction();
            }

            public bool Contains(Firefly.Box.Data.Entity entity)
            {
                DoLoad();
                return _mem.Contains(entity);
            }

            public long CountRows(Firefly.Box.Data.Entity entity)
            {
                DoLoad();
                return _mem.CountRows(entity);
            }

            public void Dispose()
            {
                _mem.Dispose();
            }

            public void Drop(Firefly.Box.Data.Entity entity)
            {
                
                _mem.Drop(entity);
            }

            public IRowsSource ProvideRowsSource(Firefly.Box.Data.Entity entity)
            {
                return _mem.ProvideRowsSource(entity);
            }

            public void Truncate(Firefly.Box.Data.Entity entity)
            {
                _mem.Truncate(entity);
            }
        }
        IEntityDataProvider LoadEntity(Firefly.Box.Data.Entity e)
        {
            var env = e as ENV.Data.Entity;
            if (env != null)
            {
                env.FixTextStorageForBtrieveAndMemory();
            }
            if (env != null && env.InMemory != InMemoryScope.None&&!DisableInMemoryEntityLoading)
            {
                if (!(env.IdentityColumn is Firefly.Box.Data.NumberColumn) || env.IdentityColumn is BtrievePositionColumn)
                    env.IdentityColumn = null;
                IEntityDataProvider mem = env.InMemory == InMemoryScope.Shared
                    ? _inMemoryData
                    : _inMemoryContextData.Value;

                if (!mem.Contains(e))
                {
                    return new LoadInMemoryDataProvider(mem,()=> {
                        if (mem.CountRows(e) == 0)
                        {
                            lock (mem)
                            {
                                if (mem.CountRows(e) == 0)
                                {
                                    using (var rrr = ENV.Utilities.Profiler.OpenRowSource(e))
                                    {
                                        using (var org = rrr.ProvideRowsSource(_source.ProvideRowsSource(e)))
                                        {
                                            using (var target = mem.ProvideRowsSource(e))
                                            {
                                                using (
                                                    var r = org.ExecuteCommand(e.Columns.ToArray(), new dummyFilter(), new Sort(),
                                                        false,
                                                        false, false))
                                                {
                                                    while (r.Read())
                                                    {
                                                        var s = new myStorage();
                                                        r.GetRow(s);
                                                        s.InsertTo(target);

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },env);
                   


                }

                return mem;
            }
            return _source;

        }
        class dummyFilter : IFilter
        {
            public void AddTo(IFilterBuilder builder)
            {

            }
        }
        class myStorage : IRowStorage
        {
            public IValue GetValue(ColumnBase column)
            {
                throw new NotImplementedException();
            }

            List<ColumnBase> _columns = new List<ColumnBase>();
            List<IValue> _values = new List<IValue>();

            public void SetValue(ColumnBase column, IValueLoader value)
            {
                _columns.Add(column);
                _values.Add(column.LoadFrom(value));
            }

            public void InsertTo(IRowsSource target)
            {
                target.Insert(_columns.ToArray(), _values.ToArray(), new myStorage(),_columns);
            }
        }
        public ITransaction BeginTransaction()
        {
            return _source.BeginTransaction();
        }

        public bool SupportsTransactions
        {
            get { return _source.SupportsTransactions; }
        }

        public static bool DisableInMemoryEntityLoading { get; internal set; }

        public bool Contains(Firefly.Box.Data.Entity entity)
        {
            using (Utilities.Profiler.EntityCommand(entity, "Table Exists"))
                return LoadEntity(entity).Contains(entity);
        }

        public long CountRows(Firefly.Box.Data.Entity entity)
        {
            using (Utilities.Profiler.EntityCommand(entity, "CountRows"))
                return LoadEntity(entity).CountRows(entity);
        }

        public void Drop(Firefly.Box.Data.Entity entity)
        {
            using (Utilities.Profiler.EntityCommand(entity, "Drop"))
                LoadEntity(entity).Drop(entity);
        }

        public void Truncate(Firefly.Box.Data.Entity entity)
        {
            using (Utilities.Profiler.EntityCommand(entity, "Truncate"))
                LoadEntity(entity).Truncate(entity);
        }
    }
    class CrossTaskCache
    {
        List<WeakReference> _entites = new List<WeakReference>();

        public void AddClearCacheOnEnd(ENV.Data.Entity entity)
        {
            entity._supressCacheClear = true;
            _entites.Add(new WeakReference(entity));
        }

        public void Clear()
        {
            foreach (var item in _entites)
            {
                if (item.IsAlive)
                {
                    var z = (ENV.Data.Entity)item.Target;
                    z._supressCacheClear = false;
                    z.ClearRelationCache();
                }

            }
            _entites.Clear();
        }
    }
}