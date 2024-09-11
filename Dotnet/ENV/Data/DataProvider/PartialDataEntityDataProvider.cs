using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.DataProvider
{
    public class PartialDataEntityDataProvider<T> : IEntityDataProvider where T : Firefly.Box.Data.Entity
    {
        public bool RequiresTransactionForLocking
        {
            get
            {
                return _source.RequiresTransactionForLocking;
            }
        }
        public abstract class Helper
        {
            public void Bind<dataType>(TypedColumnBase<dataType> column, dataType value)
            {
                var x = new myIValue<dataType>(column, value);
                UseValue(column, x);
                UseFilterItem(column, x);

            }
            protected virtual void UseValue(ColumnBase column, IValue value)
            {
            }
            protected virtual void UseFilterItem(ColumnBase column, IFilterItem value)
            {
            }



            class myIValue<dataType> : IFilterItem, IValue
            {
                TypedColumnBase<dataType> _column;
                dataType _value;

                public myIValue(TypedColumnBase<dataType> column, dataType value)
                {
                    _column = column;
                    _value = value;
                }
                public bool IsAColumn()
                {
                    return false;
                }

                public void SaveTo(IFilterItemSaver saver)
                {
                    _column.Storage.SaveTo(_value, saver);
                }

                public void SaveTo(IValueSaver saver)
                {
                    _column.Storage.SaveTo(_value, saver);
                }
            }
            public void Bind(NumberColumn column, Number value)
            {
                Bind<Number>(column, value);
            }


        }

        IEntityDataProvider _source;
        Action<T, Helper> _applyRules;
        public PartialDataEntityDataProvider(IEntityDataProvider dataProvider, Action<T, Helper> applyRules)
        {
            _source = dataProvider;
            _applyRules = applyRules;
        }



        public ITransaction BeginTransaction()
        {
            return _source.BeginTransaction();
        }

        public bool Contains(Firefly.Box.Data.Entity entity)
        {
            return _source.Contains(entity);
        }

        public void Dispose()
        {
            _source.Dispose();
        }

        public long CountRows(Firefly.Box.Data.Entity entity)
        {
            throw new NotSupportedException("Count rows for partial data provider");
            
        }



        public void Drop(Firefly.Box.Data.Entity entity)
        {
            throw new NotSupportedException("drop for partial data provider");
            
        }


        public IRowsSource ProvideRowsSource(Firefly.Box.Data.Entity entity)
        {
            return new myRowsSource(_source.ProvideRowsSource(entity), entity, this);
        }
        class myRowsSource : IRowsSource
        {
            IRowsSource _source;
            Firefly.Box.Data.Entity _entity;
            PartialDataEntityDataProvider<T> _parent;
            public myRowsSource(IRowsSource source, Firefly.Box.Data.Entity entity, PartialDataEntityDataProvider<T> parent)
            {
                _source = source;
                _entity = entity;
                _parent = parent;
            }
            IFilter DecorateWhere(IFilter val)
            {
                return new myIFilter(val, this);
            }


            class myIFilter : IFilter
            {
                IFilter _filter;
                myRowsSource _parent;
                public myIFilter(IFilter filter, myRowsSource parent)
                {
                    _filter = filter;
                    _parent = parent;
                }

                public void AddTo(IFilterBuilder builder)
                {
                    _filter.AddTo(builder);
                    _parent._parent._applyRules((T)_parent._entity, new myHelper(builder));
                }
                class myHelper : Helper
                {
                    IFilterBuilder _filter;

                    public myHelper(IFilterBuilder filter)
                    {
                        _filter = filter;
                    }
                    protected override void UseFilterItem(ColumnBase column, IFilterItem value)
                    {
                        _filter.AddEqualTo(column, value);
                    }
                }
            }

            public IRowsProvider CreateReader(IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort, IEnumerable<IJoin> joins, bool disableCache)
            {
                return new myRowsProvider(_source.CreateReader(selectedColumns, DecorateWhere(where), sort, (joins), disableCache), this);
            }
            class myRowsProvider : IRowsProvider
            {
                IRowsProvider _provider;
                myRowsSource _parent;
                public myRowsProvider(IRowsProvider provider, myRowsSource parent)
                {
                    _provider = provider;
                    _parent = parent;
                }

                public IRowsReader After(IRow row, bool reverse)
                {
                    return new myRowsReader(_provider.After(((myRow)row)._row, reverse), _parent);
                }

                public IRowsReader Find(IFilter filter, bool reverse)
                {
                    return new myRowsReader(_provider.Find(filter, reverse), _parent);
                }

                public IRowsReader From(IRow row, bool reverse)
                {
                    return new myRowsReader(_provider.From(((myRow)row)._row, reverse), _parent);
                }

                public IRowsReader From(IFilter filter, bool reverse)
                {
                    return new myRowsReader(_provider.From(filter, reverse), _parent);
                }

                public IRowsReader FromEnd()
                {
                    return new myRowsReader(_provider.FromEnd(), _parent);
                }

                public IRowsReader FromStart()
                {
                    return new myRowsReader(_provider.FromStart(), _parent);
                }
            }

            public void Dispose()
            {
                _source.Dispose();
            }

            public IRowsReader ExecuteCommand(IEnumerable<ColumnBase> selectedColumns, IFilter filter, Sort sort, bool firstRowOnly, bool shouldBeOnlyOneRowThatMatchesTheFilter, bool lockAllRows)
            {
                return new myRowsReader(_source.ExecuteCommand(selectedColumns, DecorateWhere(filter), sort, firstRowOnly, shouldBeOnlyOneRowThatMatchesTheFilter, lockAllRows), this);
            }

            public IRowsReader ExecuteReader(IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort, IEnumerable<IJoin> joins, bool lockAllRows)
            {
                return new myRowsReader(_source.ExecuteReader(selectedColumns, DecorateWhere(where), sort, (joins), lockAllRows), this);
            }
            class myRowsReader : IRowsReader
            {
                IRowsReader _source;
                myRowsSource _parent;
                public myRowsReader(IRowsReader source, myRowsSource parent)
                {
                    _source = source;
                    _parent = parent;
                }

                public void Dispose()
                {
                    _source.Dispose();
                }

                public IRow GetJoinedRow(Firefly.Box.Data.Entity e, IRowStorage c)
                {
                    return new myRow(_source.GetJoinedRow(e, c), c, e, _parent);
                }

                public IRow GetRow(IRowStorage c)
                {
                    return new myRow(_source.GetRow(c), c, _parent._entity, _parent);
                }

                public bool Read()
                {
                    return _source.Read();
                }
            }
            class myRow : IRow
            {
                internal IRow _row;
                IRowStorage _storage;
                Firefly.Box.Data.Entity _e;
                myRowsSource _parent;
                public myRow(IRow source, IRowStorage storage, Firefly.Box.Data.Entity e, myRowsSource parent)
                {
                    _parent = parent;
                    _row = source;
                    _storage = storage;
                    _e = e;
                }

                public void Delete(bool verifyRowHasNotChangedSinceLoaded)
                {
                    _row.Delete(verifyRowHasNotChangedSinceLoaded);
                }

                public bool IsEqualTo(IRow row)
                {
                    return _row.IsEqualTo(((myRow)row)._row);
                }

                public void Lock()
                {
                    _row.Lock();
                }


                public void ReloadData()
                {
                    _row.ReloadData();
                }

                public void Unlock()
                {
                    _row.Unlock();
                }

                public void Update(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, bool verifyRowHasNotChangedSinceLoaded)
                {
                    var x = new ValuesHelper(columns, values, false);
                    _parent._parent._applyRules((T)_e, x);
                    if (x._columns.Count > 0)
                        _row.Update(x._columns, x._values, verifyRowHasNotChangedSinceLoaded);
                }
            }

            public IRow Insert(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, IRowStorage storage, IEnumerable<ColumnBase> selectedColumns)
            {
                var x = new ValuesHelper(columns, values, true);
                _parent._applyRules((T)_entity, x);
                return new myRow(_source.Insert(x._columns, x._values, storage,selectedColumns), storage, _entity, this);


            }
            class ValuesHelper : Helper
            {
                public List<ColumnBase> _columns;
                public List<IValue> _values;
                bool _addFixedValuesToValues;
                public ValuesHelper(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, bool addFixedValuesToValues)
                {
                    _columns = new List<ColumnBase>(columns);
                    _values = new List<IValue>(values);
                    _addFixedValuesToValues = addFixedValuesToValues;
                }
                protected override void UseValue(ColumnBase column, IValue value)
                {
                    var x = _columns.IndexOf(column);
                    if (x >= 0)
                    {
                        _columns.RemoveAt(x);
                        _values.RemoveAt(x);
                    }
                    if (_addFixedValuesToValues)
                    {
                        _columns.Add(column);
                        _values.Add(value);
                    }
                }

            }

            public bool IsOrderBySupported(Sort sort)
            {
                return _source.IsOrderBySupported(sort);
            }
        }

        public bool SupportsTransactions
        {
            get { return _source.SupportsTransactions; }
        }

        public void Truncate(Firefly.Box.Data.Entity entity)
        {
            throw new NotSupportedException("drop for partial data provider");
        }


    }
}
