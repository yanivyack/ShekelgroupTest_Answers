using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ENV.Data;
using ENV.UI;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.UI.Advanced;
using Firefly.Box.Advanced;

namespace ENV.Utilities
{
    public class DataTableBuilder
    {
        public readonly DataTable Result = new DataTable() { TableName = "Table1" };
        List<Action<System.Data.DataRow>> _populateRow = new List<Action<System.Data.DataRow>>();
        public bool SplitDateToYearMonthDay { get; set; }
        public bool SplitHourOutOfTime { get; set; }
        public bool BackwardCompatibleTypes { get; set; }
        public DataColumn AddColumn(string caption, Func<string> getValue)
        {
            return AddColumn(caption, typeof(String), () => getValue(), () => false, (o, where) =>
                                                                                     {
                                                                                         where.Add(
                                                                                             () =>
                                                                                                 Comparer.Equals(o,
                                                                                                     getValue()));
                                                                                     });
        }
        public DataColumn AddColumn(string caption, Type type, Func<object> getValue, Func<bool> isEmpty, Action<object, FilterCollection> addFilter)
        {
            var s = caption;
            if (string.IsNullOrWhiteSpace(s))
                s = type.Name + " without a caption";
            int i = 0;
            while (Result.Columns.Contains(s))
                s = caption + i++;
            var dc = new DataColumn { ColumnName = s, Caption = caption };
            dc.DataType = type;
            _populateRow.Add(dr => dr[dc] = isEmpty() ? DBNull.Value : getValue());
            Result.Columns.Add(dc);
            _filters.Add(dc, addFilter);
            return dc;
        }


        public void AddColumns(params ColumnBase[] columns)
        {
            foreach (var c in columns)
            {
                AddColumn(c, c.Caption);
            }
        }
        public void AddColumns(IEnumerable<ColumnBase> columns)
        {
            foreach (var c in columns)
            {
                AddColumn(c, c.Caption);
            }
        }
        public DataColumn AddColumn(ColumnBase c)
        {
            return AddColumn(c, c.Caption);
        }

        public DataColumn AddColumn(ColumnBase c, string caption)
        {
            var cc = new myColumnCaster(this, caption, SplitDateToYearMonthDay, SplitHourOutOfTime);
            UserMethods.CastColumn(c, cc);
            return cc.DataColumn;
        }

        Dictionary<DataColumn, Action<object, FilterCollection>> _filters =
            new Dictionary<DataColumn, Action<object, FilterCollection>>();
        public void AddColumn(InputControlBase control, string caption)
        {
            var c = Common.GetColumn(control);

            var cb = control as Firefly.Box.UI.ComboBox; ;

            if (c != null && cb == null)
            {
                AddColumn(c, caption);
            }
            else
            {
                var cc = new myColumnCaster(this, caption, SplitDateToYearMonthDay, SplitHourOutOfTime);
                var tb = control as Firefly.Box.UI.TextBox;
                if (tb != null && tb.Data != null && tb.Data.Value is Number)
                {
                    cc.AddColumn(typeof(decimal), () => ((Number)tb.Data.Value).ToDecimal(), () => tb.Data.Value == null,
                        (o, Where) => Where.Add(() => Comparer.Equals(tb.Data.Value, o)));
                }
                else
                {
                    if (cb != null && c != null)
                    {
                        var map = new Dictionary<string, object>();
                        cc.AddColumn(typeof(string), () =>

                        {
                            if (!map.ContainsKey(control.Text))
                                map.Add(control.Text, c.Value);
                            return control.Text;
                        }, () => BackwardCompatible.NullBehaviour.IsNull(control.Text), (o, Where) => Where.Add(() => Comparer.Equals(c.Value, map[o.ToString()])));
                    }
                    else if (tb != null && tb.Data != null)
                        cc.AddColumn(typeof(string), () => control.Text, () => BackwardCompatible.NullBehaviour.IsNull(control.Text), (o, Where) => Where.Add(() => Comparer.Equals(tb.Data.Value, o)));
                }
            }
        }
        public static void ShowDataTable(DataTable obj)
        {
            var f = new System.Windows.Forms.Form();
            var dgv = new DataGridView { Dock = DockStyle.Fill };
            dgv.DataSource = obj;
            f.Controls.Add(dgv);
            f.ShowDialog();
        }

        class myColumnCaster : UserMethods.IColumnSpecifier
        {
            DataTableBuilder _parent;
            string _caption;
            bool _splitDateToYearMonthDay;
            bool _splitHoursOutOfTime;
            public myColumnCaster(DataTableBuilder parent, string caption, bool splitDateToYearMonthDay, bool splitHoursOutOfTime)
            {
                _parent = parent;
                _caption = caption;
                _splitDateToYearMonthDay = splitDateToYearMonthDay;
                _splitHoursOutOfTime = splitHoursOutOfTime;
            }
            public DataColumn DataColumn { get; set; }
            public void AddColumn(Type type, Func<object> getValue, Func<bool> isEmpty, Action<object, FilterCollection> filter)
            {
                DataColumn = _parent.AddColumn(_caption, type, getValue, isEmpty, filter);
            }



            public void DoOnColumn(TypedColumnBase<Text> column)
            {
                AddColumn(typeof(string), () => column.Value.TrimEnd(), () => BackwardCompatible.NullBehaviour.IsNull(column.Value), (o, where) => where.Add(column.IsEqualTo(Text.Cast(o))));
            }

            public void DoOnColumn(TypedColumnBase<Number> column)
            {
                var x = column as NumberColumn;
                if (x != null && x.FormatInfo.DecimalDigits == 0)
                {
                    if (_parent.BackwardCompatibleTypes)
                        AddColumn(typeof(long), () => (long)column.Value, () => BackwardCompatible.NullBehaviour.IsNull(column.Value), (o, where) => where.Add(column.IsEqualTo(Number.Cast(o))));
                    else
                        AddColumn(typeof(int), () => (int)column.Value, () => BackwardCompatible.NullBehaviour.IsNull(column.Value), (o, where) => where.Add(column.IsEqualTo(Number.Cast(o))));
                }
                else
                {
                    if (_parent.BackwardCompatibleTypes)
                        AddColumn(typeof(double), () => (double)column.Value, () => BackwardCompatible.NullBehaviour.IsNull(column.Value), (o, where) => where.Add(column.IsEqualTo(Number.Cast(o))));
                    else
                        AddColumn(typeof(decimal), () => column.Value.ToDecimal(), () => BackwardCompatible.NullBehaviour.IsNull(column.Value), (o, where) => where.Add(column.IsEqualTo(Number.Cast(o))));
                }
            }

            public void DoOnColumn(TypedColumnBase<Date> column)
            {
                Func<DateTime> getDateTime = () =>
                {
                    if (column.Value == Date.Empty ||
                        BackwardCompatible.NullBehaviour.IsNull(column.Value))
                        return new DateTime(1901, 1, 1);
                    else
                        return column.Value.ToDateTime();
                };

                AddColumn(typeof(DateTime), () => getDateTime(), () => BackwardCompatible.NullBehaviour.IsNull(column.Value),
                    (o, where) =>
                    {
                        if (o == null)
                            where.Add(column.IsEqualTo(Date.Empty));
                        else
                            where.Add(column.IsEqualTo(Date.FromDateTime((DateTime)o)));
                    });
                if (_splitDateToYearMonthDay)
                {
                    _parent.AddColumn(_caption + " Year", typeof(int), () => getDateTime().Year,
                                      () => Date.IsNullOrEmpty(column), DatePartFilter(column, y => y.Year));

                    _parent.AddColumn(_caption + " Month", typeof(int), () => getDateTime().Month,
                                      () => Date.IsNullOrEmpty(column), DatePartFilter(column, y => y.Month));
                    _parent.AddColumn(_caption + " Day Of Week", typeof(int), () => getDateTime().DayOfWeek + 1,
                                      () => Date.IsNullOrEmpty(column), DatePartFilter(column, y => (int)y.DayOfWeek + 1));
                }

                ;

            }

            static Action<object, FilterCollection> DatePartFilter(TypedColumnBase<Date> column, Func<Date, int> datePart)
            {
                return (o, @where) =>
                       {

                           if (o == null)
                               @where.Add(column.IsEqualTo(Date.Empty));
                           else
                               @where.Add(
                                   () =>
                                       !Date.IsNullOrEmpty(column) &&
                                       Comparer.Equals(datePart(column.Value), o));
                       };
            }

            public void DoOnColumn(TypedColumnBase<Time> column)
            {
                if (_parent.BackwardCompatibleTypes)
                    AddColumn(typeof(TimeSpan), () => column.ToString(), () => BackwardCompatible.NullBehaviour.IsNull(column.Value), (o, where) => where.Add(column.IsEqualTo((Time)column.Parse((string)o, column.Format))));
                else
                    AddColumn(typeof(string), () => column.ToString(), () => BackwardCompatible.NullBehaviour.IsNull(column.Value), (o, where) => where.Add(column.IsEqualTo((Time)column.Parse((string)o, column.Format))));
                if (_splitHoursOutOfTime)
                    _parent.AddColumn(_caption + " Hour", typeof(int), () => column.Value.Hour, () => BackwardCompatible.NullBehaviour.IsNull(column.Value),
                        (o, where) =>
                        {
                            if (o == null)
                                where.Add(column.IsEqualTo((Time)null));
                            else
                                where.Add(column.IsBetween(new Time((int)o, 0, 0), new Time((int)o, 59, 59)));
                        });
            }

            public void DoOnColumn(TypedColumnBase<Bool> column)
            {
                AddColumn(typeof(bool), () => (bool)column.Value, () => BackwardCompatible.NullBehaviour.IsNull(column.Value), (o, where) => where.Add(column.IsEqualTo(Bool.Cast(o))));
            }

            public void DoOnColumn(TypedColumnBase<byte[]> column)
            {
                var bac = column as ENV.Data.ByteArrayColumn;
                if (bac != null && bac.ContentType != ByteArrayColumnContentType.BinaryAnsi && bac.ContentType != ByteArrayColumnContentType.BinaryUnicode)
                    AddColumn(typeof(string), () => column.ToString(), () => BackwardCompatible.NullBehaviour.IsNull(column.Value), (o, where) => { });
                else
                    AddColumn(typeof(byte[]), () => (byte[])column.Value, () => BackwardCompatible.NullBehaviour.IsNull(column.Value), (o, where) => where.Add(column.IsEqualTo(o as byte[])));
            }

            public void DoOnUnknownColumn(ColumnBase column)
            {
                throw new NotImplementedException();
            }
        }

        public void AddRow()
        {
            var r = Result.NewRow();
            foreach (var action in _populateRow)
            {
                action(r);
            }
            Result.Rows.Add(r);
        }
        public void AddRows(AbstractUIController controller)
        {
            if (Result.Columns.Count == 0)
                AddColumns(controller._uiController.Columns);
            controller._uiController.ReadAllRows(AddRow);
        }

        public void AddRows(Firefly.Box.UIController controller)
        {
            if (Result.Columns.Count == 0)
                AddColumns(controller.Columns);
            controller.ReadAllRows(AddRow);
        }
        public void AddRows(Firefly.Box.BusinessProcess controller)
        {
            if (Result.Columns.Count == 0)
                AddColumns(controller.Columns);

            controller.ReadAllRows(AddRow);
        }

        public void BuildBy(AbstractUIController controller)
        {
            ENV.Labs.GridExports.ExportToDataTableBuilder(controller._uiController, () => { }, this);
        }

        public void ApplyFilter(Action<Action<DataColumn, object[]>> provideSelectedValues, FilterCollection fc)
        {
            provideSelectedValues((column, objects) =>
            {
                var y = _filters[column];
                FilterBase f = null;
                foreach (var value in objects)
                {
                    var cf = new FilterCollection();
                    y(value, cf);
                    if (f == null)
                        f = cf;
                    else f = f.Or(cf);
                }
                if (f != null)
                    fc.Add(f);
            });
        }
    }



}
