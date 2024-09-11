using System;
using System.Collections.Generic;
using System.Linq;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using System.Windows.Forms;
using Firefly.Box.UI.Advanced;
using Firefly.Box.Data;
using Firefly.Box.Data.DataProvider;
using ENV.Data.DataProvider;

namespace ENV
{
    interface IUIControllerFilterUI
    {
        FilterCollection Filter { get; }
        bool Run(UIOptions options, ColumnBase currentColumn);
        void ChooseIndex(Entity from, Action<Sort> sort);
    }
    public class TemplateModeFilter : IUIControllerFilterUI
    {
        UIController _uic;
        string _prefix;

        string _expressionString;

        MenuActionInstance _templateExit, _clearCurrentValue, _clearTemplate, _fromValue, _toValue, _ok, _expression;
        internal class MenuActionInstance
        {

            bool _enabled = true;

            public bool Enabled
            {
                get { return _enabled; }
                set
                {
                    _enabled = value;
                    if (_isActive)
                    {
                        _staticItem.Enabled = value;
                    }
                }
            }
            public bool Active = true;

            public Action Activate;


            UIControllerCustomCommand _staticItem;
            public MenuActionInstance(Action<HandlerInvokeEventArgs> whatToDo, UIController uic, AbstractUIController auic, UIControllerCustomCommand staticItem)
            {
                _staticItem = staticItem;
                Activate = delegate
                {
                    if (Active)
                    {
                        staticItem.SetAction(whatToDo
                                             ,
                                             _enabled && uic.From is Firefly.Box.Data.Entity && uic.AllowActivitySwitch);
                        _isActive = true;
                    }
                };

                Activate();
                auic.BecomingTheCurrentTask += Activate;
                auic.NoLongerTheCurrentTask += delegate { staticItem.ClearAction(); _isActive = false; };
            }
            bool _isActive = false;
        }

        Action _setColumnsReadOnlyAccordingToActivity;
        bool _wasThereAChange = false;
        bool _useWildcardForContainsInTextColumnFilter;
        Action<Action<ColumnBase, InputControlBase>> _inputControlsProvider;
        class collectExistingFilterValues : Firefly.Box.Data.DataProvider.IFilterBuilder
        {
            TemplateModeFilter _parent;
            public collectExistingFilterValues(TemplateModeFilter parent)
            {
                _parent = parent;
            }
            public void AddBetween(ColumnBase column, IFilterItem from, IFilterItem to)
            {
                var x = new ValueLoaderAndSaver();
                from.SaveTo(x);
                var val = column.GetValueFromDB(x);
                _parent._origFrom.Add(column, val);
                x = new ValueLoaderAndSaver();
                to.SaveTo(x);
                AddTo(column, column.GetValueFromDB(x));

            }

            public void AddDifferentFrom(ColumnBase column, IFilterItem item)
            {

            }

            public void AddEqualTo(ColumnBase column, IFilterItem item)
            {
                var x = new ValueLoaderAndSaver();
                item.SaveTo(x);
                var val = column.GetValueFromDB(x);
                AddFrom(column, val);
                AddTo(column, val);
            }

            public void AddGreaterOrEqualTo(ColumnBase column, IFilterItem item)
            {
                var x = new ValueLoaderAndSaver();
                item.SaveTo(x);
                var val = column.GetValueFromDB(x);
                AddFrom(column, val);

            }
            void AddFrom(ColumnBase column, object item)
            {
                if (_parent._origFrom.ContainsKey(column))
                    _parent._origFrom[column] = item;
                else
                    _parent._origFrom.Add(column, item);

            }
            void AddTo(ColumnBase column, object item)
            {
                if (_parent._origTo.ContainsKey(column))
                    _parent._origTo[column] = item;
                else
                    _parent._origTo.Add(column, item);

            }


            public void AddGreaterThan(ColumnBase column, IFilterItem item)
            {

            }

            public void AddLessOrEqualTo(ColumnBase column, IFilterItem item)
            {
                var x = new ValueLoaderAndSaver();
                item.SaveTo(x);
                var val = column.GetValueFromDB(x);
                AddTo(column, val);
            }

            public void AddLessOrEqualWithWildcard(TextColumn column, Text value, IFilterItem filterItem)
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
        public TemplateModeFilter(UIController uic, AbstractUIController auic, string prefix, Action setColumnsReadOnlyAccordingToActivity, bool useWildcardForContainsInTextColumnFilter, Action<Action<ColumnBase, InputControlBase>> inputControlsProvider, FilterCollection originalFilter)
        {
            Firefly.Box.Data.Advanced.FilterBase.GetIFilter(originalFilter, true, uic.From).AddTo(new collectExistingFilterValues(this));
            _permanentFrom = new Dictionary<ColumnBase, object>(_origFrom);
            _permanentTo = new Dictionary<ColumnBase, object>(_origTo);
            _useWildcardForContainsInTextColumnFilter = useWildcardForContainsInTextColumnFilter;
            _prefix = prefix;
            _uic = uic;
            _setColumnsReadOnlyAccordingToActivity = setColumnsReadOnlyAccordingToActivity;
            _inputControlsProvider = inputControlsProvider;
            _templateExit = new MenuActionInstance(
                e =>
                {
                    if (!_confirm)
                    {

                        foreach (var c in _uic.Columns)
                        {
                            if (_currentState.ContainsKey(c))
                            {
                                if (_options.TemplateView.IsColumnEmpty(c))
                                {
                                    _wasThereAChange = true;
                                    break;
                                }
                                else if (!_currentState[c].Equals(c.Value))
                                {
                                    _wasThereAChange = true;
                                    break;
                                }
                            }
                            else if (!_options.TemplateView.IsColumnEmpty(c))
                            {
                                _wasThereAChange = true;
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(_expressionString))
                            _wasThereAChange = true;
                        if (_permanentFrom.Count != _fromValues.Count || _permanentTo.Count != _toValues.Count)
                            _wasThereAChange = true;
                        if (_wasThereAChange)
                        {
                            AskUserToConfirm(e);
                        }


                    }
                    if (!e.Handled)
                        BeforeExit();
                }, _uic, auic, Commands.TemplateExit);
            _clearCurrentValue = new MenuActionInstance(
                e =>
                {
                    e.Handled = true;
                    var col = Common.GetColumn(_uic.View.LastFocusedControl);
                    if (col != null)
                    {
                        if (_currentState.ContainsKey(col))
                            _currentState.Remove(col);
                        if (_toValues.ContainsKey(col))
                            _toValues.Remove(col);
                        _options.TemplateView.SetEmptyColumn(col, true);
                    }
                }, _uic, auic, Commands.ClearCurrentValueInTemplate);
            _clearTemplate = new MenuActionInstance(
                                     e =>
                                     {

                                         e.Handled = true;
                                         if (_fromValues.Count == 0 && _toValues.Count == 0 && string.IsNullOrEmpty(_expressionString))
                                         {
                                             foreach (var c in _uic.Columns)
                                             {
                                                 if (!_options.TemplateView.IsColumnEmpty(c))
                                                 {
                                                     _wasThereAChange = true;
                                                     break;
                                                 }
                                             }

                                         }
                                         _wasThereAChange = true;
                                         _fromValues.Clear();
                                         _toValues.Clear();

                                         foreach (var c in _uic.Columns)
                                         {
                                             _options.TemplateView.SetEmptyColumn(c, true);
                                         }
                                         _expressionString = "";
                                         _fromValues = new Dictionary<ColumnBase, object>(_origFrom);
                                         _toValues = new Dictionary<ColumnBase, object>(_origTo);
                                         SetContextToFromValues(_options);
                                     }, _uic, auic, Commands.ClearTemplate);
            _fromValue = new MenuActionInstance(
                                     e =>
                                     {
                                         e.Handled = true;
                                         ExtractTemplateState(_options);
                                         SetContextToFromValues(_options);
                                     }, _uic, auic, Commands.TemplateFromValues);
            _toValue = new MenuActionInstance(
                                     e =>
                                     {
                                         e.Handled = true;
                                         ExtractTemplateState(_options);
                                         SetContextToToValues(_options);
                                     }, uic, auic, Commands.TemplateToValues);
            _ok = new MenuActionInstance(e =>
            {
                e.Handled = true;
                _confirm = true;
                _options.TemplateView.Exit();
            }, _uic, auic, Commands.TemplateOk);
            _expression = new MenuActionInstance(
                e =>
                {
                    var c = Context.Current;
                    Common.RunOnContextTopMostForm(
                        form =>
                        {
                            var d = new UI.FilterExpressionForm(c.ActiveTasks, false, auic.u);
                            d.Expression = _expressionString;
                            if (d.ShowDialog(form) == DialogResult.OK)
                                _expressionString = d.Expression;
                        });
                    e.Handled = true;
                }, _uic, auic, Commands.TemplateExpression);

            SetEnabled(false);
        }
        void BeforeExit()
        {
            ExtractTemplateState(_options);
            if (_confirm)
            {
                _permanentFrom = _fromValues;
                _permanentTo = _toValues;

                PrepareFilter();
            }
        }

        Dictionary<ColumnBase, object> _currentState;

        protected Dictionary<ColumnBase, object> _fromValues = new Dictionary<ColumnBase, object>();

        Dictionary<ColumnBase, object> _toValues = new Dictionary<ColumnBase, object>(),
            _permanentFrom = new Dictionary<ColumnBase, object>(),
            _permanentTo = new Dictionary<ColumnBase, object>(),
            _origFrom = new Dictionary<ColumnBase, object>(),
            _origTo = new Dictionary<ColumnBase, object>();


        UIOptions _options;
        void SetEnabled(bool val)
        {
            foreach (var o in new[] { _templateExit, _ok, _expression, _clearCurrentValue, _clearTemplate })
            {
                o.Enabled = val;
            }
        }
        void Activate(bool val)
        {
            foreach (var o in new[] { _templateExit, _clearCurrentValue, _clearTemplate, _fromValue, _toValue, _ok })
            {

                o.Active = val;
                if (val)
                    o.Activate();
            }
        }

        public bool Run(UIOptions options, ColumnBase currentColumn)
        {
            _confirm = false;
            _options = options;
            _fromValues = new Dictionary<ColumnBase, object>(_permanentFrom);
            _toValues = new Dictionary<ColumnBase, object>(_permanentTo);
            Activate(true);


            _currentState = _fromValues;

            var restoreState = new List<Action>();
            var cancelMenuChange = false;
            try
            {
                Context.Current.BeginInvoke(
                    () =>
                    {
                        if (cancelMenuChange) return;
                        Common.RunOnContextTopMostForm(
                            form =>
                            {
                                if (form != null)
                                    foreach (var item in form.Controls)
                                    {
                                        {
                                            var x = item as ToolStrip;
                                            var y = item as StatusStrip;
                                            if (x != null && y == null)
                                                SetMenusActivities(x.Items, restoreState);
                                        }
                                    }
                            });
                    });
                foreach (var item in _uic.View.Controls)
                {
                    var tb = item as Firefly.Box.UI.TextBox;
                    if (tb != null && tb.Style == Firefly.Box.UI.ControlStyle.Flat && tb.Border == Firefly.Box.UI.ControlBorderStyle.None)
                    {
                        tb.Border = Firefly.Box.UI.ControlBorderStyle.Thin;
                        restoreState.Add(() => tb.Border = Firefly.Box.UI.ControlBorderStyle.None);
                    }

                }
                SetEnabled(true);
                foreach (var column in _uic.Columns)
                {
                    var c = column as IENVColumn;
                    if (c != null)
                        c._internalReadOnly = false;
                }
                _wasThereAChange = false;
                options.TemplateView.Run(
                    () =>
                    {
                        SetContextToFromValues(options);
                        if (_uic.OrderBy.Segments.Count > 0)
                        {
                            var done = false;
                            InputControlBase afterOrderByColumn = null;

                            var columns = new List<ColumnBase>();
                            var controls = new List<InputControlBase>();
                            _inputControlsProvider(
                                (columnBase, c) =>
                                {
                                    columns.Add(columnBase);
                                    controls.Add(c);
                                });

                            Action<int> tryFocusControl =
                                i =>
                                {
                                    var x = i;
                                    while ((controls[x] == null || !controls[x].Visible) && x + 1 < controls.Count) x++;
                                    if (controls[x] == null || !controls[x].Visible) return;
                                    done = true;
                                    controls[x].TryFocus(() => { });
                                };

                            foreach (var segment in _uic.OrderBy.Segments)
                            {
                                var x = columns.IndexOf(segment.Column);
                                if (x != -1)
                                {
                                    tryFocusControl(x);
                                    if (done)
                                        break;
                                }
                            }

                            if (!done)
                                tryFocusControl(0);

                        }
                    });
                return _confirm;
            }
            finally
            {
                _setColumnsReadOnlyAccordingToActivity();
                cancelMenuChange = true;
                Common.RunOnContextTopMostForm(
                    form =>
                    {
                        foreach (var action in restoreState)
                        {
                            action();
                        }
                    });
                SetEnabled(false);
                Activate(false);

            }

        }

        void SetContextToToValues(UIOptions options)
        {

            Common.SetActivityText(_prefix + " to");
            ENV.Commands.TemplateToValues.Enabled = false;
            ENV.Commands.TemplateFromValues.Enabled = true;
            _currentState = _toValues;
            SetTemplateState(options);
        }

        void SetContextToFromValues(UIOptions options)
        {
            Common.SetActivityText(_prefix + " from");
            ENV.Commands.TemplateToValues.Enabled = true;
            ENV.Commands.TemplateFromValues.Enabled = false;
            _currentState = _fromValues;
            SetTemplateState(options);
        }

        bool SetMenusActivities(ToolStripItemCollection collection, List<Action> restoreState)
        {
            var hasActiveInIt = false;
            foreach (ToolStripItem item in new System.Collections.ArrayList(collection))
            {
                var dd = item as ToolStripDropDownItem;
                if (dd != null)
                {
                    hasActiveInIt = SetMenusActivities(dd.DropDownItems, restoreState);
                }
                var i = item;
                var x = item.Available;
                item.Available = hasActiveInIt || item.Tag == ShowInTemplateMode;
                if (item.Available)
                    hasActiveInIt = true;

                restoreState.Add(() => i.Available = x);

            }
            return hasActiveInIt;
        }


        protected virtual void AskUserToConfirm(HandlerInvokeEventArgs e)
        {
            var dialogResult = Common.ShowYesNoCancelMessageBox(LocalizationInfo.Current.ConfirmUpdateTitle,
                                                                LocalizationInfo.Current.ConfirmUpdateMessage);

            switch (dialogResult)
            {
                case DialogResult.Yes:
                    _confirm = true;
                    break;
                case DialogResult.No:
                    break;
                case DialogResult.Cancel:
                    e.Handled = true;
                    break;
            }
        }

        void PrepareFilter()
        {
            _filter.Clear();
            var nonEntityColumnsBoundToTabControls = new HashSet<ColumnBase>();
            foreach (var control in _uic.View.Controls)
            {
                var tabControl = control as Firefly.Box.UI.TabControl;
                if (tabControl == null) continue;
                var col = tabControl.GetColumn();
                if (col.Entity == null) nonEntityColumnsBoundToTabControls.Add(col);
            }
            foreach (var c in _permanentFrom)
            {
                if (nonEntityColumnsBoundToTabControls.Contains(c.Key)) continue;

                if (_permanentTo.ContainsKey(c.Key))
                {
                    if (Firefly.Box.Advanced.Comparer.Equal(c.Value, _permanentTo[c.Key]))
                        DoOnColumn(c.Key, new Equal(_useWildcardForContainsInTextColumnFilter), c.Value);
                    else
                        DoOnColumn(c.Key, new Between(), c.Value, _permanentTo[c.Key]);
                }
                else
                {
                    DoOnColumn(c.Key, new GreaterOrEqual(), c.Value);
                }
            }
            foreach (var c in _permanentTo)
            {
                if (nonEntityColumnsBoundToTabControls.Contains(c.Key)) continue;

                if (!_permanentFrom.ContainsKey(c.Key))
                    DoOnColumn(c.Key, new LessOrEqual(), c.Value);
            }
            if (!string.IsNullOrEmpty(_expressionString))
            {
                var ex = new EvaluateExpressions(ENV.UserMethods.Instance, () => Context.Current.ActiveTasks);
                _filter.Add(() =>
                {
                    try
                    {
                        return ex.Evaluate<Bool>(_expressionString);
                    }
                    catch
                    {
                        return false;
                    }
                });
            }
        }
        abstract class FilterApplyer
        {
            public abstract FilterBase Do<T>(TypedColumnBase<T> c, T[] values);
        }
        class Equal : FilterApplyer
        {
            bool _useWildcardForContains = false;
            public Equal(bool useWildCardForContains)
            {
                _useWildcardForContains = useWildCardForContains;
            }
            public override FilterBase Do<T>(TypedColumnBase<T> c, T[] values)
            {
                var val = values[0];
                var col = c as Firefly.Box.Data.TextColumn;
                if (_useWildcardForContains && col != null)
                {
                    var s = val.ToString();
                    if (s.StartsWith("*"))
                    {
                        var fc = new FilterCollection();
                        fc.Add(() => col.Value.Contains(s.Substring(1).TrimEnd()));
                        return fc;
                    }
                }
                return c.IsEqualTo(values[0]);
            }
        }
        class GreaterOrEqual : FilterApplyer
        {
            public override FilterBase Do<T>(TypedColumnBase<T> c, T[] values)
            {
                return c.IsGreaterOrEqualTo(values[0]);
            }
        }
        class LessOrEqual : FilterApplyer
        {
            public override FilterBase Do<T>(TypedColumnBase<T> c, T[] values)
            {
                return c.IsLessOrEqualTo(values[0]);
            }
        }
        class Between : FilterApplyer
        {
            public override FilterBase Do<T>(TypedColumnBase<T> c, T[] values)
            {
                return c.IsBetween(values[0], values[1]);
            }
        }



        void DoOnColumn(ColumnBase c, FilterApplyer filterApplyer, params object[] values)
        {
            DoOnColumn(c, x => Text.Cast(x), values, filterApplyer);
            DoOnColumn(c, x => Number.Cast(x), values, filterApplyer);
            DoOnColumn(c, x => Date.Cast(x), values, filterApplyer);
            DoOnColumn(c, x => Time.Cast(x), values, filterApplyer);
            DoOnColumn(c, x => Bool.Cast(x), values, filterApplyer);
        }

        void DoOnColumn<T>(ColumnBase c, Func<object, T> translateValue, object[] values, FilterApplyer filterApplyer)
        {
            var col = c as TypedColumnBase<T>;
            if (col != null)
            {
                var vals = new List<T>();
                foreach (var o in values)
                {
                    vals.Add(translateValue(o));
                }
                _filter.Add(filterApplyer.Do(col, vals.ToArray()));
            }
        }




        FilterCollection _filter = new FilterCollection();
        public FilterCollection Filter { get { return _filter; } }


        bool _confirm = false;
        void ExtractTemplateState(UIOptions x)
        {
            var prevState = new Dictionary<ColumnBase, object>(_currentState);
            _currentState.Clear();
            foreach (var c in _uic.Columns)
            {
                if (!x.TemplateView.IsColumnEmpty(c))
                {
                    _currentState.Add(c, c.Value);
                    _wasThereAChange = true;
                    if (_currentState == _fromValues &&
                        (!prevState.ContainsKey(c) || !Comparer.Equal(c.Value, prevState[c])))
                    {
                        if (!_toValues.ContainsKey(c))
                        {
                            _toValues.Add(c, c.Value);
                        }
                        else if (!prevState.ContainsKey(c) || Comparer.Equal(_toValues[c], prevState[c]) || (_permanentFrom.ContainsKey(c) && Comparer.Equal(prevState[c], _permanentFrom[c])))
                            _toValues[c] = c.Value;
                    }
                }
                else
                    x.TemplateView.SetEmptyColumn(c, false);
            }
        }

        void SetTemplateState(UIOptions x)
        {
            foreach (var c in _uic.Columns)
            {
                if (_currentState.ContainsKey(c))
                {
                    var ro = c as IENVColumn;
                    var returnToReadOnly = false;
                    if (ro != null && ro._internalReadOnly)
                    {
                        ro._internalReadOnly = false;
                        returnToReadOnly = true;
                    }
                    var returnToReadOnlyForExistingRows = false;
                    if (ro != null && ro.ReadOnlyForExistingRows)
                    {
                        ro.ReadOnlyForExistingRows = false;
                        returnToReadOnlyForExistingRows = true;
                    }

                    c.Value = _currentState[c];
                    if (returnToReadOnly)
                        ro._internalReadOnly = true;
                    if (returnToReadOnlyForExistingRows)
                        ro.ReadOnlyForExistingRows = true;
                }
                else
                    x.TemplateView.SetEmptyColumn(c, true);
            }
        }
        [Obsolete("Had a spelling mistake, please use SetTemplateModeMenus")]
        public static void SetTemplateModeMenues(params ToolStripItem[] items)
        {
            SetTemplateModeMenus(items);
        }
        public static void SetTemplateModeMenus(params ToolStripItem[] items)
        {
            foreach (var item in items)
            {
                item.Tag = ShowInTemplateMode;
            }

        }
        const string ShowInTemplateMode = "ShowInTemplateMode";

        public void ChooseIndex(Entity from, Action<Sort> sort)
        {
            if (_fromValues.Count == 0 || _toValues.Count == 0)
                return;

            var colsOnUI = new HashSet<ColumnBase>();
            _inputControlsProvider((c, y) =>
            {
                colsOnUI.Add(c);
            });

            var chosenIndex = InternalChooseIndex(_uic.OrderBy, from, new List<ColumnBase>(_fromValues.Keys), new List<ColumnBase>(_toValues.Keys), item => colsOnUI.Contains(item));
            if (chosenIndex != _uic.OrderBy)
            {
                chosenIndex.Reversed = _uic.OrderBy.Reversed;
                sort(chosenIndex);
            }
        }

        internal static Sort InternalChooseIndex(Sort currentIndex, Entity from, IEnumerable<ColumnBase> fromValues, IEnumerable<ColumnBase> toValues, Func<ColumnBase, bool> isDisplayed)
        {
            var filterColumns = fromValues.Union(toValues).Where(isDisplayed).ToList();
            return InternalChooseIndexBasedOnColumns(currentIndex, from, filterColumns);
        }

        internal static Sort InternalChooseIndexBasedOnColumns(Sort currentIndex, Entity from, List<ColumnBase> filterColumns)
        {
            var indexesStartingWithCurrent = from.Indexes.Except(new[] { currentIndex }).Where(idx => idx.Segments.Count > 0).ToList();
            indexesStartingWithCurrent.Insert(0, currentIndex);

            int maxScore = 0;
            Sort result = currentIndex;


            foreach (var index in indexesStartingWithCurrent)
            {
                var segments = index.Segments.Select(seg => seg.Column).ToList();
                var segmentScore = from.Columns.Count;
                var score = 0;

                foreach (var segment in segments)
                {
                    if (filterColumns.Contains(segment))
                    {
                        score += segmentScore;
                    }
                    segmentScore--;
                }

                if (score > maxScore)
                {
                    maxScore = score;
                    result = index;
                }
            }

            return result;
        }
    }
}