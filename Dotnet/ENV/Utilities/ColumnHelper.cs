using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ENV.BackwardCompatible;
using ENV.UI;
using Firefly.Box.Data.Advanced;
using Firefly.Box.UI.Advanced;
using Microsoft.SqlServer.Server;

namespace ENV.Utilities
{

    interface IENVColumn : ICanShowCustomHelp
    {
        void ClearExpandEvent();
        string StatusTip { get; set; }
        string ToolTip { get; set; }
        bool IsParameter { get; set; }
        void EnterOnControl();
        void SetNullStrategy(INullStrategy instance);
        bool CompareForNullValue(object previousValue);
        void UpdateParameterReturnValue(Action performUpdate);
        void InternalPerformExpandOperation(Action expand);
        bool _internalReadOnly { get; set; }
        bool ReadOnlyForExistingRows { get; set; }
        Type ControlType { get; set; }
        Type ControlTypeOnGrid { get; set; }
        Type ControlTypePrinting { get; set; }
        Type ControlTypePrintingOnGrid { get; set; }
        object _internalValueChangeStore { get; set; }
    }


    class ColumnHelper
    {
        ColumnBase _column;

        public ColumnHelper(ColumnBase column)
        {
            _column = column;
        }

        Action _expand;
        bool _autoExpand = false;
        public bool AutoExpand
        {
            get { return _autoExpand; }
            set
            {
                if (_autoExpand != value)
                {
                    if (value)
                    {
                        _column.Expand -= MyExpand;
                    }
                    else
                    {
                        if (_expand != null)
                            _column.Expand += MyExpand;
                    }

                    _autoExpand = value;
                }

            }
        }
        /// <summary>
        /// Only used for the UserMethods.ControlSelectProgram method - backward compatability only
        /// </summary>

        public Type ExpandClassType { get; set; }

        bool _disableParametersReturnValue = false;
        bool _lockOnParameterChangeValue = false;
        internal void UpdateParameterReturnValue(Action performUpdate)
        {
            if (_disableParametersReturnValue)
                return;

            if (_lockOnParameterChangeValue && Firefly.Box.Context.Current.ActiveTasks.Count > 0)
            {
                var t = Firefly.Box.Context.Current.ActiveTasks[Firefly.Box.Context.Current.ActiveTasks.Count - 1] as Firefly.Box.UIController;
                if (t != null && t.RowLocking == Firefly.Box.LockingStrategy.OnUserEdit)
                    t.LockCurrentRow();
            }
            var x = (IENVColumn)_column;
            var y = x._internalReadOnly;
            x._internalReadOnly = false;
            var z = x.ReadOnlyForExistingRows;
            x.ReadOnlyForExistingRows = false;
            try
            {

                performUpdate();
            }
            finally
            {
                x._internalReadOnly = y;
                x.ReadOnlyForExistingRows = z;
            }
        }
        internal void InternalPerformExpandOperation(Action expand)
        {
            if (expand == null)
                return;
            if (ENV.UserMethods.Instance.Stat(0, "Q") && !_column.AfterExpandGoToNextControl)
                _disableParametersReturnValue = true;
            else
            {

                var ec = _column as IENVColumn;
                if (ec != null)
                {
                    if (ec.ReadOnlyForExistingRows && !ENV.UserMethods.Instance.Stat(0, "C"))
                        _disableParametersReturnValue = true;
                }
            }
            try
            {
                if (_column.Entity != null)
                    _lockOnParameterChangeValue = true;
                expand();
            }
            finally
            {
                _lockOnParameterChangeValue = false;
                _disableParametersReturnValue = false;
            }


        }
        internal static void DoExpand(Action expand, ColumnBase column)
        {
            if (expand == null)
                return;

            var c = column as IENVColumn;
            if (c != null)
            {
                c.InternalPerformExpandOperation(expand);
            }
        }

        public event Action Expand
        {
            add
            {
                if (_expand == null && !AutoExpand)
                    _column.Expand += MyExpand;
                _expand = value;
            }
            remove { }
        }
        void MyExpand()
        {
            DoExpand(_expand, _column);

        }


        public void ClearExpandEvent()
        {
            _expand = null;
            _column.Expand -= MyExpand;
            ExpandClassType = null;
        }

        public void EnterOnControl()
        {
            if (AutoExpand && _expand != null)
                _expand();
        }
    }
    class ControlHelper
    {
        class MyControl : IControlHelperControl
        {
            InputControlBase _control;
            public MyControl(InputControlBase control)
            {
                _control = control;
            }

            public bool ReadOnly { get { return _control.ReadOnly; } set { _control.ReadOnly = value; } }

            public event Action Load
            {
                add
                {
                    _control.Load += value;
                }
                remove
                {
                    _control.Load -= value;
                }
            }
            public event Action Expand
            {
                add
                {
                    _control.Expand += value;
                }
                remove
                {
                    _control.Expand -= value;
                }
            }

            public string ToolTip { get { return _control.ToolTip; } set { _control.ToolTip = value; } }

            public ColumnBase GetColumn()
            {
                return Common.GetColumn(_control);
            }
            public Control GetControl()
            {
                return _control;
            }
            public Control GetControlForFocus()
            {
                if (_control.Controls.Count == 0)
                    return null;
                return _control.Controls[0];
            }
        }
        IControlHelperControl _control;
        bool _readonlyFoeExistingRows;
        public ControlHelper(InputControlBase control) : this(new MyControl(control)) { }
        public ControlHelper(IControlHelperControl control)
        {
            _control = control;

            _control.Load += () =>
            {
                var col = _control.GetColumn();
                if (col != null)
                    switch (AfterExpandGoToNextControl)
                    {
                        case AfterExpandGoToNextControlOptions.True:
                            col.AfterExpandGoToNextControl = true;
                            break;
                        case AfterExpandGoToNextControlOptions.False:
                            col.AfterExpandGoToNextControl = false;
                            break;
                    }
                var x = col as IENVColumn;
                if (x != null)
                {
                    if (_wasCleared)
                    {

                        x.ClearExpandEvent();
                    }
                    if (ExpandClassType == null)
                        ExpandClassType = x.ExpandClassType;
                    if (!string.IsNullOrEmpty(x.ToolTip))
                        _control.ToolTip = x.ToolTip;
                    if (x.ReadOnlyForExistingRows)
                        _readonlyFoeExistingRows = true;

                }
            };
        }
        bool _autoExpand = false;
        public bool AutoExpand
        {
            get { return _autoExpand; }
            set
            {
                if (_autoExpand != value)
                {
                    if (value)
                    {
                        _control.Expand -= MyExpand;
                    }
                    else
                    {
                        if (_expand != null)
                            _control.Expand += MyExpand;
                    }

                    _autoExpand = value;
                }

            }
        }
        Action _expand;
        public event Action Expand
        {
            add
            {
                ClearExpandEvent();
                if (!AutoExpand)
                    _control.Expand += MyExpand;

                _expand = value;
            }
            remove { }
        }
        AfterExpandGoToNextControlOptions _afterExpandGoToNextControlOptions = AfterExpandGoToNextControlOptions.Default;
        public AfterExpandGoToNextControlOptions AfterExpandGoToNextControl { get { return _afterExpandGoToNextControlOptions; } set { _afterExpandGoToNextControlOptions = value; } }
        public Type ExpandClassType { set; get; }
        void MyExpand()
        {
            ColumnHelper.DoExpand(_expand, _control.GetColumn());
        }
        bool _wasCleared = false;
        public void ClearExpandEvent()
        {
            _wasCleared = true;
            _expand = null;
            _control.Expand -= MyExpand;
            var x = _control.GetColumn() as IENVColumn;
            if (x != null)
                x.ClearExpandEvent();
        }
        void ProvideStatusTip(Action<string> to)
        {
            if (_wasStatusTipChanged)
            {
                if (!string.IsNullOrEmpty(_statusTip))
                    to(_statusTip);
            }
            else
            {
                var x = _control.GetColumn() as IENVColumn;
                if (x != null)
                {
                    if (!string.IsNullOrEmpty(x.StatusTip))
                        to(x.StatusTip);
                }

            }

        }

        Firefly.Box.UI.TextBox GetControlAsTextboxWhichIsNotInAGrid()
        {
            var tb = _control.GetControl() as Firefly.Box.UI.TextBox;
            if (tb != null)
            {
                var p = tb.Parent;
                while (p != null)
                {
                    if (p is Firefly.Box.UI.Grid)
                        return null;
                    p = p.Parent;
                }
                return tb;
            }
            return null;
        }

        void AdjustTextBoxHeightToFitFontHeight()
        {
            var tb = GetControlAsTextboxWhichIsNotInAGrid();
            if (tb != null && tb.DeferredHeight > 2)
            {
                var autoHeight = tb.Font.Height + 1;
                var style = tb.Style;
                var envtb = tb as ENV.UI.TextBox;
                if (envtb != null)
                    style = envtb.GetOriginalStyle();
                if (style == Firefly.Box.UI.ControlStyle.Standard ||
                    (style == Firefly.Box.UI.ControlStyle.Flat && tb.Border == Firefly.Box.UI.ControlBorderStyle.Thick))
                    autoHeight += 2;
                else if (style == Firefly.Box.UI.ControlStyle.Flat && tb.Border == Firefly.Box.UI.ControlBorderStyle.None)
                    autoHeight -= 1;
                if (tb.DeferredHeight < autoHeight)
                {
                    _originalHeight = tb.DeferredHeight;
                    tb.DeferredHeight = autoHeight;
                }
            }
        }

        void RestoreTextBoxOriginalHeight()
        {
            var tb = GetControlAsTextboxWhichIsNotInAGrid();
            if (tb != null)
            {
                if (_originalHeight > 0)
                {
                    tb.DeferredHeight = _originalHeight;
                    _originalHeight = -1;
                }
            }

        }

        int _originalHeight = -1;
        System.Windows.Forms.Form _form;
        void SetFocusedControlOfParentForm(System.Windows.Forms.Control c)
        {
            if (_form == null)
                _form = _control.GetControl().FindForm();
            if (_form == null) return;
            var envForm = _form as ENV.UI.Form;
            if (envForm != null) envForm.SetFocusedControl(c);
        }
        List<Action> _onLeave = new List<Action>();
        public void ControlEnter(Action controlEvent)
        {
            SetFocusedControlOfParentForm(_control.GetControl());
            if (UserSettings.ForceTextBoxMinimumHeightWhileFocused)
                AdjustTextBoxHeightToFitFontHeight();
            {
                var x = _control.GetColumn() as IENVColumn;
                if (x != null)
                    x.EnterOnControl();
            }
            if (AutoExpand && _expand != null)
                _expand();
            ProvideStatusTip(x =>
            {
                if (_hasStatusTip)
                    return;
                _hasStatusTip = true;
                Common.PushStatusText(_displayedStatusTip = Languages.Translate(x));
                var fc = _control.GetControlForFocus();
                if (!_registeredFocusEvents && fc != null)
                {
                    fc.GotFocus += ControlHelper_GotFocus;
                    fc.LostFocus += ControlHelper_LostFocus;
                    _registeredFocusEvents = true;
                }
            });


            ENV.Advanced.LevelProvider.StartEnterControlContext(controlEvent, _control.GetControl());
            if (_readonlyFoeExistingRows && !_control.ReadOnly && !ENV.UserMethods.Instance.Stat(0, "C"))
            {
                var cb = _control.GetControl() as ENV.UI.ComboBox;
                if (cb == null || !cb.AllowChangeInBrowse)
                {
                    _control.ReadOnly = true;
                    _onLeave.Add(() => _control.ReadOnly = false);
                }
            }
        }
        bool _registeredFocusEvents = false;
        string _displayedStatusTip = null;
        private void ControlHelper_LostFocus(object sender, EventArgs e)
        {
            ProvideStatusTip(x =>
            {

                if (!_hasStatusTip)
                    return;
                _hasStatusTip = false;
                Common.PopStatusText();
            });
        }

        private void ControlHelper_GotFocus(object sender, EventArgs e)
        {
            ProvideStatusTip(x =>
            {
                if (_hasStatusTip)
                    return;
                _hasStatusTip = true;
                Common.PushStatusText(_displayedStatusTip = Languages.Translate(x));

            });
        }

        bool _hasStatusTip;
        public void ControlLeave(Action controlEvent)
        {
            var at = Firefly.Box.Context.Current.ActiveTasks;
            var resetStatusText = true;
            if (at.Count > 0)
            {
                ControllerBase.SendInstanceBasedOnTaskAndCallStack(at[at.Count - 1],
                    t =>
                    {
                        var uic = t as AbstractUIController;
                        if (uic != null && uic.ShouldExitAsSoonAsPossible())
                            resetStatusText = false;
                    });

            }
            if (resetStatusText)
                Common.ResetStatusText();
            if (UserSettings.ForceTextBoxMinimumHeightWhileFocused)
                RestoreTextBoxOriginalHeight();


            ProvideStatusTip(x =>
            {

                if (!_hasStatusTip)
                    return;
                _hasStatusTip = false;
                Common.PopStatusText();
            });
            ENV.Advanced.LevelProvider.StartLeaveControlContext(controlEvent, _control.GetControl());
            SetFocusedControlOfParentForm(null);
            foreach (var item in _onLeave)
            {
                item();
            }
            _onLeave.Clear();
        }

        public void ControlChange(Action controlEvent)
        {
            ENV.Advanced.LevelProvider.StartChangeControlContext(controlEvent, _control.GetControl());
        }
        public void ControlInputValidation(Action controlEvent)
        {
            ENV.Advanced.LevelProvider.StartInputValidationControlContext(controlEvent, _control.GetControl());
        }


        string _statusTip = string.Empty;
        bool _wasStatusTipChanged = false;

        public string StatusTip { get { return _statusTip; } set { _statusTip = value; _wasStatusTipChanged = true; } }

        public Size ToleratedContainerOverflow
        {
            get
            {
                if (_originalHeight > 0)
                {
                    var tb = GetControlAsTextboxWhichIsNotInAGrid();
                    if (tb != null && tb.DeferredHeight > _originalHeight)
                        return new Size(0, tb.DeferredHeight - _originalHeight);
                }
                return Size.Empty;
            }
        }
    }
    internal interface IControlHelperControl
    {
        bool ReadOnly { get; set; }
        string ToolTip { get; set; }

        event Action Expand;
        event Action Load;

        ColumnBase GetColumn();
        Control GetControl();
        Control GetControlForFocus();
    }
}
