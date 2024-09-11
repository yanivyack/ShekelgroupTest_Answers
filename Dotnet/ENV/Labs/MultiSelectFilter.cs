using System;
using System.Collections.Generic;
using System.Text;
using ENV.Data.DataProvider;
using ENV.UI;
using Firefly.Box;
using ENV.Data;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.Data.UnderConstruction;

namespace ENV.Labs
{
    class MultiSelectFilter<T> : UIControllerBase
    {
        internal readonly MultiSelectFilterEntity<T> MultiSelectFilterEntity;

        TypedColumnBase<T> _column;

        UI.MultiSelectFilterUI _form;
        bool _isBtrieve = false;
       
        public MultiSelectFilter(UIController uic, TypedColumnBase<T> column, System.Drawing.Point location,TypedColumnBase<T> columnToCollectValues,Firefly.Box.UI.Advanced.InputControlBase icb)
        {
            MultiSelectFilterEntity = new MultiSelectFilterEntity<T>(columnToCollectValues);
            _column = column;

            Func<T, ValueInfo<T>> createValue = null;

            var cb = icb as Firefly.Box.UI.ComboBox;
            
            if (cb != null)
            {
                createValue = y => new ValueInfo<T> { DescriptionValue = cb.Text ,Value = y};
            }
            else
            {
                createValue = y => new ValueInfo<T> {  Value = y};
            }

            var values = new Dictionary<string, ValueInfo<T>>();
            var be = uic.From as BtrieveEntity;
            if (be != null && be.IsUsingBtrieve)
                _isBtrieve = true;
            ShowProgressInNewThread.ReadAllRowsWithProgress(uic,"Collect filter info",
                () =>
                {
                    if (icb!=null)
                        icb.ToString();
                    var val = "";
                    if (column.Value != null)
                        val = column.Value.ToString().ToUpperInvariant();
                    if (!values.ContainsKey(val))
                    {
                        values.Add(val, createValue(column.Value));
                    }
                    else
                    {
                        values[val].Count++;
                    }
                });
            MultiSelectFilterEntity.Truncate();
            MultiSelectFilterEntity.Populate(values);

            From = MultiSelectFilterEntity;



            View = ()=>_form = new UI.MultiSelectFilterUI(Ok,MultiSelectFilterEntity.Checked,(cb!=null?(ColumnBase)MultiSelectFilterEntity.Description: MultiSelectFilterEntity.Value),MultiSelectFilterEntity.NumberOfRows){Location = location};
            

            Activity = Activities.Browse;
            AllowUpdate = false;
            AllowInsert = false;
            AllowDelete = false;
        }

        internal override bool SuppressUpdatesInBrowseActivity
        {
            get
            {
                return false;
            }
        }
        public FilterBase Run()
        {
            Execute();
            FilterBase filter = null;
            if (_ok)
            {
                
                var bp = new BusinessProcess { From = MultiSelectFilterEntity };
                bp.Where.Add(MultiSelectFilterEntity.Checked.IsEqualTo(true));
                var values = new List<T>();
                bp.ForEachRow(
                    () =>
                    {
                        values.Add(MultiSelectFilterEntity.Value.Value);
                        if (filter == null)
                            filter = _column.IsEqualTo(MultiSelectFilterEntity.Value.Value);
                        else
                            filter = filter.Or(_column.IsEqualTo(MultiSelectFilterEntity.Value.Value));
                    });
                if (_isBtrieve)
                {
                    var fc = new FilterCollection();
                    fc.Add(() => values.Contains(_column.Value));
                    return fc;
                }
            }
            return filter;
        }


        bool _ok = false;
        public void Ok()
        {
            _ok = true;



            Exit();
        }
    }

    class ValueInfo<T>
    {
        public int Count = 1;
        public string DescriptionValue;
        public T Value;
    }
    class MultiSelectFilterEntity<T> : Entity
    {
        internal readonly TypedColumnBase<T> Value;
        internal readonly TextColumn Description = new TextColumn("Description","100");
        internal readonly NumberColumn NumberOfRows = new NumberColumn();
        internal readonly BoolColumn Checked = new BoolColumn();

        public MultiSelectFilterEntity(TypedColumnBase<T> valueColumn)
            : base("MultiSelectFilterEntity", new MemoryDatabase())
        {
            Value = valueColumn;
            Columns.Add(Value,Description, NumberOfRows, Checked);
            SetPrimaryKey(Value);
        }
        protected override void PopulateColumns()
        {
            
        }

        public void Populate(Dictionary<string, ValueInfo<T>> values)
        {
            var i = new Iterator(this);

            foreach (var pair in values)
            {
                var r = i.CreateRow();
                r.Set(Value, pair.Value.Value);
                r.Set(Checked, false);
                r.Set(Description, pair.Value.DescriptionValue);
                r.Set(NumberOfRows, pair.Value.Count);
                r.UpdateDatabase();
            }
        }
    }

  
}
