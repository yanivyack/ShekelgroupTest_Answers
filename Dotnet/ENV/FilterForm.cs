using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.UI;
using Firefly.Box;
using Firefly.Box.UI.Advanced;
using Button = Firefly.Box.UI.Button;
using CheckBox = Firefly.Box.UI.CheckBox;
using ComboBox = Firefly.Box.UI.ComboBox;
using DateColumn = ENV.Data.DateColumn;
using Entity = ENV.Data.Entity;
using Form = Firefly.Box.UI.Form;
using NumberColumn = ENV.Data.NumberColumn;
using TextBox = Firefly.Box.UI.TextBox;
using TextColumn = ENV.Data.TextColumn;
using TimeColumn = ENV.Data.TimeColumn;
using Firefly.Box.Data;

namespace ENV
{
    public class FilterForm : IUIControllerFilterUI
    {
        DataSetDataProvider _dataProvider = new DataSetDataProvider();
        ColumnsEntity GetColumns()
        {
            return new ColumnsEntity(_dataProvider);
        }

        int _lastId = 0;
        FilterEntity GetFilter()
        {
            return new FilterEntity(_dataProvider, () => ++_lastId, () => new ColumnsEntity(_dataProvider));
        }

        class ColumnType : TextColumn
        {
            public ColumnType()
                : base("ColumnType")
            {
            }

            public void Set(Type type)
            {
                Value = type.Name;
            }

            public Bool Is(Type type)
            {
                return type.Name == Value;
            }
        }
        class ColumnId : NumberColumn
        {
            readonly Func<ColumnsEntity> _columns;
            public ColumnId(Func<ColumnsEntity> columns)
                : base("ColumnID")
            {
                _columns = columns;
            }

            public void AttachTo(ComboBox comboBox)
            {
                comboBox.Data = this;
                comboBox.BindListSource +=
                    (sender, e) =>
                    {
                        var columnsEntity = _columns();
                        comboBox.ListSource = columnsEntity;
                        comboBox.ValueColumn = columnsEntity.Id;
                        comboBox.DisplayColumn = columnsEntity.Name;
                    };
            }


        }
        class ColumnsEntity : Entity
        {
            [PrimaryKey]
            internal readonly ColumnId Id = new ColumnId(() => null);
            internal readonly TextColumn Name = new TextColumn("Name");
            internal readonly ColumnType Type = new ColumnType();
            internal readonly TextColumn Format = new TextColumn("Format");
            internal readonly Data.BoolColumn isCombo = new Data.BoolColumn("IsCombo");
            internal readonly Data.BoolColumn isCheckBox = new Data.BoolColumn("IsCheckBox");

            public ColumnsEntity(IEntityDataProvider dataProvider)
                : base("ColumnsEntity", dataProvider)
            {
            }
        }
        class FilterType : NumberColumn
        {
            static FilterType()
            {
                Action<int, string> insert =
                    delegate(int i, string s)
                    {
                        var bp = new BusinessProcess();
                        var e = new FilterTypeEntity();
                        bp.From = e;
                        bp.Activity = Activities.Insert;
                        bp.ForFirstRow(delegate
                        {
                            e.FilterType.Value = i;
                            e.Description.Value = s;
                        });
                    };
                insert(Equals, LocalizationInfo.Current.IsEqualTo);
                insert(GreaterOrEqual, LocalizationInfo.Current.GreaterOrEqual);
                insert(Greater, LocalizationInfo.Current.Greater);
                insert(LesserOrEqual, LocalizationInfo.Current.LessOrEqual);
                insert(Lesser, LocalizationInfo.Current.LessThen);
                insert(Between, LocalizationInfo.Current.Between);
                //insert(StartsWith, "Starts With");
            }
            public FilterType()
                : base("FilterType")
            {
                BindValue(() => Equals);
            }

            internal new static readonly Number
                Equals = 1;
            internal static readonly Number
                GreaterOrEqual = 2,
                Greater = 3,
                LesserOrEqual = 4,
                Lesser = 5,
                Between = 6,
                StartsWith = 7;

            public void AttachTo(ComboBox comboBox, ColumnType type)
            {
                comboBox.Data = this;
                comboBox.BindListSource +=
                    (sender, e) =>
                    {
                        var ex = new FilterTypeEntity();
                        comboBox.ListSource = ex;
                        comboBox.ValueColumn = ex.FilterType;
                        comboBox.DisplayColumn = ex.Description;
                        var f = new FilterCollection();
                        f.Add(() => type.Is(typeof(Text)));
                        comboBox.ListWhere.Add(ex.FilterType.IsLessOrEqualTo(StartsWith).Or(f));
                    };
            }

            public bool ShowToField()
            {
                return Value == Between;
            }

            public void AddFilterTo<T>(FilterCollection filter, TypedColumnBase<T> columnBase, T from, T to)
            {
                if (Value == Equals)
                    filter.Add(columnBase.IsEqualTo(from));
                else if (Value == GreaterOrEqual)
                    filter.Add(columnBase.IsGreaterOrEqualTo(from));
                else if (Value == Greater)
                    filter.Add(columnBase.IsGreaterThan(from));
                else if (Value == LesserOrEqual)
                    filter.Add(columnBase.IsLessOrEqualTo(from));
                else if (Value == Lesser)
                    filter.Add(columnBase.IsLessThan(from));
                else if (Value == Between)
                    filter.Add(columnBase.IsBetween(from, to));
            }
        }
        class FilterTypeEntity : Entity
        {
            static readonly DataSetDataProvider myProvider = new DataSetDataProvider();


            [PrimaryKey]
            internal readonly FilterType FilterType = new FilterType();
            internal readonly TextColumn Description = new TextColumn("Description");
            public FilterTypeEntity()
                : base("FilterType", myProvider)
            {
            }
        }
        class FilterEntity : Entity
        {
            [PrimaryKey]
            internal readonly NumberColumn RowId = new NumberColumn("RowId");
            internal readonly ColumnId Column;
            internal readonly FilterType FilterType = new FilterType();
            internal readonly NumberColumn FromNumber = new NumberColumn("FN","15.10N");
            internal readonly NumberColumn ToNumber = new NumberColumn("TN","15.10N");
            internal readonly NumberColumn FromNumberCombo = new NumberColumn("FNC", "15.10N");
            internal readonly NumberColumn ToNumberCombo = new NumberColumn("TNC", "15.10N");
            internal readonly TextColumn FromText = new TextColumn("FT");
            internal readonly TextColumn ToText = new TextColumn("TT");
            internal readonly TextColumn FromTextCombo = new TextColumn("FTC");
            internal readonly TextColumn ToTextCombo = new TextColumn("TTC");
            internal readonly Data.BoolColumn FromBool = new Data.BoolColumn("FB");
            internal readonly Data.BoolColumn ToBool = new Data.BoolColumn("TB");
            internal readonly Data.BoolColumn FromBoolCombo = new Data.BoolColumn("FBC");
            internal readonly Data.BoolColumn ToBoolCombo = new Data.BoolColumn("TBC");
            internal readonly Data.BoolColumn FromBoolCheckBox = new Data.BoolColumn("FBCB");
            internal readonly Data.BoolColumn ToBoolCheckBox = new Data.BoolColumn("TBCB");

            internal readonly DateColumn FromDate = new DateColumn("FD");
            internal readonly DateColumn ToDate = new DateColumn("TD");
            internal readonly TimeColumn FromTime = new TimeColumn("FTi");
            internal readonly TimeColumn ToTime = new TimeColumn("TTi");


            public FilterEntity(IEntityDataProvider dataProvider, Func<Number> idProvider, Func<ColumnsEntity> getColumns)
                : base("Filter", dataProvider)
            {
                Column = new ColumnId(getColumns);
                Columns.Add(Column);
                RowId.BindValue(idProvider);
            }
        }
        public bool Run(UIOptions neverUsed, ColumnBase defaultColumn)
        {
            return Run(defaultColumn);
        }
        public bool Run(ColumnBase defaultColumn)
        {
            var uic = new UIController();
            uic.SwitchToInsertWhenNoRows = true;
            var filterEntity = GetFilter();
            if (defaultColumn != null && _columns.ContainsKey(defaultColumn))
                filterEntity.Column.DefaultValue = _columns[defaultColumn];
            uic.From = filterEntity;
            var f = new Form();
            f.RightToLeftLayout = true;
            f.RightToLeft = LocalizationInfo.Current.RightToLeft;
            f.StartPosition = WindowStartPosition.CenterMDI;
            f.Size = new System.Drawing.Size(640, 470);

            uic.View = f;
            var column = GetColumns();
            uic.Relations.Add(column, column.Id.IsEqualTo(filterEntity.Column));
            var controlLocation = new System.Drawing.Point(2, 2);
            var g = new Grid();
            g.Height = f.ClientSize.Height - 30;
            g.Width = f.ClientSize.Width;
            f.Controls.Add(g);
            var gcc = new GridColumn();

            var cbColumn = new ComboBox() { Location = controlLocation, Style = ControlStyle.Flat };
            gcc.Text = LocalizationInfo.Current.Column;
            filterEntity.Column.AttachTo(cbColumn);
            gcc.Controls.Add(cbColumn);
            gcc.Width = cbColumn.Right + 3;
            g.Controls.Add(gcc);
            var gcFilterType = new GridColumn();

            gcFilterType.Width = 150;
            gcFilterType.Text = LocalizationInfo.Current.FilterType;
            var cbFilterType = new ComboBox() { Location = controlLocation, Style = ControlStyle.Flat };
            gcFilterType.Controls.Add(cbFilterType);
            cbFilterType.Width = gcFilterType.Width - 10;
            filterEntity.FilterType.AttachTo(cbFilterType, column.Type);
            g.Controls.Add(gcFilterType);
            var gcValues = new GridColumn();
            gcValues.Text = LocalizationInfo.Current.Values;
            gcValues.Width = g.Width - 20 - gcFilterType.Width - gcc.Width;

            g.Controls.Add(gcValues);

            var cc = new ControlsCreator(controlLocation, column, filterEntity, gcValues, cbColumn.Height, this,
                                         f.RightToLeft == System.Windows.Forms.RightToLeft.Yes);

            cc.AddTextbox(filterEntity.FromNumber, filterEntity.ToNumber);
            cc.AddComboBox(filterEntity.FromNumberCombo, filterEntity.ToNumberCombo);
            cc.AddComboBox(filterEntity.FromTextCombo, filterEntity.ToTextCombo);
            cc.AddComboBox(filterEntity.FromBoolCombo, filterEntity.ToBoolCombo);
            cc.AddCheckBox(filterEntity.FromBoolCheckBox, filterEntity.ToBoolCheckBox);
            cc.AddTextbox(filterEntity.FromText, filterEntity.ToText);
            cc.AddTextbox(filterEntity.FromBool, filterEntity.ToBool);
            cc.AddTextbox(filterEntity.FromDate, filterEntity.ToDate);
            cc.AddTextbox(filterEntity.FromTime, filterEntity.ToTime);
            bool result = false;
            var bOk = new Button();
            bOk.Text = LocalizationInfo.Current.Ok;
            bOk.Location = new System.Drawing.Point(f.ClientSize.Width - bOk.Width - 4,
                                                    f.ClientSize.Height - bOk.Height - 4);
            f.Controls.Add(bOk);
            f.AcceptButton = bOk;
            bOk.Click += delegate
            {
                Filter.Clear();
                uic.SaveRowAndDo(
                    delegate
                    {
                        var bp = new BusinessProcess();
                        var fe = GetFilter();
                        bp.From = fe;
                        bp.ForEachRow(delegate
                        {
                            _filterActions[fe.Column](fe);
                        });
                        if (!string.IsNullOrEmpty(_expressionString))
                        {
                            var e = new Utilities.EvaluateExpressions(ENV.UserMethods.Instance,()=>Firefly.Box.Context.Current.ActiveTasks);
                            Filter.Add(() =>
                            {
                                try
                                {
                                    return e.Evaluate<Bool>(_expressionString);
                                }
                                catch { return false; }
                            });

                        }
                        result = true;
                        uic.Exit();
                    });

            };

            var bCancel = new Button();
            bCancel.Text = LocalizationInfo.Current.Cancel;
            bCancel.Location = new System.Drawing.Point(bOk.Left - bCancel.Width - 5, bOk.Top);
            bCancel.Click += (a, b) => uic.Exit();
            f.CancelButton = bCancel;
            f.Controls.Add(bCancel);

            var bExpression = new Button { Text = LocalizationInfo.Current.FilterExpression, Top = bOk.Top };
            bExpression.Left = bCancel.Left - bExpression.Width - 5;
            f.Controls.Add(bExpression);
            bExpression.Click += (sender, args) =>
            {
                var d = new UI.FilterExpressionForm(Context.Current.ActiveTasks, true,_userMethods);
                d.Expression = _expressionString;
                if (d.ShowDialog(Common.ContextTopMostForm) == DialogResult.OK)
                    _expressionString = d.Expression;
            };

            f.Text = _caption;
            uic.Start += () =>
            {
                uic.Raise(Command.GoToNextControl);
                uic.Raise(Command.GoToNextControl);
            };
            uic.Run();
            return result;

        }

        string _expressionString = "";
        class ControlsCreator
        {
            Point _controlLocation;
            ColumnsEntity _column;
            FilterEntity _filterEntity;
            GridColumn _valuesColumn;
            int _controlHeight;
            FilterForm _parent;
            bool _rightToLeft;

            public ControlsCreator(Point controlLocation, ColumnsEntity column, FilterEntity filterEntity, GridColumn valuesColumn, int controlHeight, FilterForm parent, bool rightToLeft)
            {
                _controlLocation = controlLocation;
                _column = column;
                _filterEntity = filterEntity;
                _valuesColumn = valuesColumn;
                _controlHeight = controlHeight;
                _parent = parent;
                _rightToLeft = rightToLeft;
            }
            public void AddTextbox<T>(TypedColumnBase<T> fromColumn, TypedColumnBase<T> toColumn)
            {
                var tbFrom = new TextBox { Data = fromColumn };
                var tbTo = new TextBox { Data = toColumn };
                tbFrom.BindFormat += (sender, e) => e.Value = _column.Format;
                tbTo.BindFormat += (sender, e) => e.Value = _column.Format;
                Add(typeof(T), tbFrom, tbTo, () => !_column.isCombo && !_column.isCheckBox);
            }
            public void AddComboBox<T>(TypedColumnBase<T> fromColumn, TypedColumnBase<T> toColumn)
            {
                var tbFrom = new ComboBox { Data = fromColumn };
                var tbTo = new ComboBox { Data = toColumn };

                Add(typeof(T), tbFrom, tbTo, () =>
                {
                    if (_column.isCombo)
                    {
                        _parent._whatToDoToCombo[_filterEntity.Column](tbFrom);
                        _parent._whatToDoToCombo[_filterEntity.Column](tbTo);

                        return true;
                    }
                    return false;
                });
            }

            void Add(Type type, InputControlBase tbFrom, InputControlBase tbTo, Func<Bool> aditionalVisibleExpresion)
            {


                tbFrom.Location = _controlLocation;
                tbFrom.Style = ControlStyle.Flat;
                tbFrom.BindVisible += (sender, e) => e.Value = _column.Type.Is(type) && aditionalVisibleExpresion();
                tbFrom.Width = _valuesColumn.Width / 2 - 10;
                tbFrom.Height = _controlHeight;

                tbFrom.Expand += () => _parent._fromExpandActions[_column.Id](_filterEntity);




                tbTo.Location = _controlLocation;
                tbTo.Style = ControlStyle.Flat;
                tbTo.BindVisible += (sender, e) => e.Value = _filterEntity.FilterType.ShowToField() && _column.Type.Is(type) && aditionalVisibleExpresion();
                tbTo.Left = tbFrom.Right + 5;
                tbTo.Width = tbFrom.Width;
                tbTo.Height = tbFrom.Height;
                tbTo.Expand += () => _parent._fromExpandActions[_column.Id](_filterEntity);



                if (_rightToLeft)
                {
                    tbTo.Left = tbFrom.Left;
                    tbFrom.Left = tbTo.Right + 5;
                }

                _valuesColumn.Controls.Add(tbFrom);
                _valuesColumn.Controls.Add(tbTo);
            }

            public void AddCheckBox(Data.BoolColumn fromColumn, Data.BoolColumn toColumn)
            {
                var tbFrom = new CheckBox { Data = fromColumn };
                var tbTo = new CheckBox { Data = toColumn };

                Add(typeof(Bool), tbFrom, tbTo, () =>
                {
                    if (_column.isCheckBox)
                    {
                        return true;
                    }
                    return false;
                });
                foreach (var box in new[] { tbFrom, tbTo })
                {
                    box.Style = ControlStyle.Standard;
                    box.Size = new Size(20, 20);
                }
            }
        }


        string _caption;

        Dictionary<int, Action<FilterEntity>> _filterActions = new Dictionary<int, Action<FilterEntity>>();
        Dictionary<int, Action<FilterEntity>> _fromExpandActions = new Dictionary<int, Action<FilterEntity>>();
        Dictionary<int, Action<FilterEntity>> _toExpandActions = new Dictionary<int, Action<FilterEntity>>();
        Dictionary<int, Action<ComboBox>> _whatToDoToCombo = new Dictionary<int, Action<ComboBox>>();
        int i = 0;
        readonly FilterCollection _filter = new FilterCollection();
        public FilterCollection Filter { get { return _filter; } }

        public delegate void AddColumnToFilterForm(ColumnBase column, InputControlBase icb);
        UserMethods _userMethods;
        public FilterForm(string caption, Action<AddColumnToFilterForm> columnsProvider,UserMethods userMethods)
        {
            _userMethods = userMethods;
            _caption = caption;


            columnsProvider(
                delegate(ColumnBase column, InputControlBase icb)
                {
                    if (icb is ComboBox && (
                        column is TypedColumnBase<Number> ||
                        column is TypedColumnBase<Text> ||
                        column is TypedColumnBase<Bool>))
                    {
                        AddComboColumn(icb as ComboBox, column as TypedColumnBase<Number>, f => f.FromNumberCombo, f => f.ToNumberCombo);
                        AddComboColumn(icb as ComboBox, column as TypedColumnBase<Text>, f => f.FromTextCombo, f => f.ToTextCombo);
                        AddComboColumn(icb as ComboBox, column as TypedColumnBase<Bool>, f => f.FromBoolCombo, f => f.ToBoolCombo);
                    }
                    else if (icb is CheckBox && column is TypedColumnBase<Bool>)
                    {
                        AddColumn(icb, column as TypedColumnBase<Bool>, f => f.FromBoolCheckBox, f => f.ToBoolCheckBox,
                                  x => x.isCheckBox.Value = true);
                    }
                    else
                    {
                        AddColumn(icb, column as TypedColumnBase<Text>, f => f.FromText, f => f.ToText);
                        AddColumn(icb, column as TypedColumnBase<Number>, f => f.FromNumber, f => f.ToNumber);
                        AddColumn(icb, column as TypedColumnBase<Bool>, f => f.FromBool, f => f.ToBool);
                        AddColumn(icb, column as TypedColumnBase<Date>, f => f.FromDate, f => f.ToDate);
                        AddColumn(icb, column as TypedColumnBase<Time>, f => f.FromTime, f => f.ToTime);
                    }
                });


        }
        void AddComboColumn<T>(ComboBox icb, TypedColumnBase<T> column, Func<FilterEntity, TypedColumnBase<T>> from, Func<FilterEntity, TypedColumnBase<T>> to)
        {
            AddColumn(icb, column, from, to, c =>
            {
                c.isCombo.Value = true;
                _whatToDoToCombo.Add(c.Id, combo =>
                {


                    combo.Values = icb.Values;
                    combo.DisplayValues = icb.DisplayValues;
                    Firefly.Box.UI.Advanced.BindingEventHandler
                        <EventArgs> x =
                            (sender, e) =>
                            {


                            };
                    combo.BindListSource += x;
                    combo.ListSource = icb.ListSource;
                    combo.DisplayColumn = icb.DisplayColumn;
                    combo.ValueColumn = icb.ValueColumn;
                    combo.ListOrderBy = icb.ListOrderBy;
                    combo.ListWhere.Clear();
                    combo.ListWhere.Add(icb.ListWhere);
                    combo.BindListSource -= x;
                    if (combo.Values == null &&
                        combo.ListSource == null)
                        combo.Values = column.InputRange;
                });


            }
                );

        }
        void AddColumn<T>(InputControlBase icb, TypedColumnBase<T> column, Func<FilterEntity, TypedColumnBase<T>> from, Func<FilterEntity, TypedColumnBase<T>> to)
        {
            AddColumn<T>(icb, column, from, to, c => c.isCombo.Value = false);

        }
        Dictionary< ColumnBase,int> _columns = new Dictionary<ColumnBase,int>();
        void AddColumn<T>(InputControlBase icb, TypedColumnBase<T> column, Func<FilterEntity, TypedColumnBase<T>> from, Func<FilterEntity, TypedColumnBase<T>> to, Action<ColumnsEntity> doOnColumnsEntity)
        {
            if (column == null || icb == null)
                return;

            string format = null;
            {
                var tb = icb as TextBox;
                if (tb != null)
                    format = tb.Format;
            }
            if (string.IsNullOrEmpty(format))
                format = column.Format;
            Action expand = icb.InvokeExpand;
            int key = i++;
            var bp = new BusinessProcess();
            var c = GetColumns();
            bp.From = c;
            bp.Activity = Activities.Insert;
            bp.ForFirstRow(delegate
            {
                c.Id.Value = key;
                c.Name.Value = column.Caption;
                c.Type.Set(typeof(T));
                c.Format.Value = format;
                doOnColumnsEntity(c);

            });
            _filterActions.Add(key, f => f.FilterType.AddFilterTo(Filter, column, from(f), to(f)));
            _columns.Add( column,key);
            _fromExpandActions.Add(key, delegate(FilterEntity f)
            {
                T value = column.Value;
                column.Value = from(f).Value;
                expand();
                from(f).Value = column.Value;
                column.Value = value;
            });
            _toExpandActions.Add(key, delegate(FilterEntity f)
            {
                T value = column.Value;
                column.Value = to(f).Value;
                expand();
                to(f).Value = column.Value;
                column.Value = value;
            });
        }

        public void ChooseIndex(Firefly.Box.Data.Entity from, Action<Sort> sort)
        {
            
        }
    }
}
