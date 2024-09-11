using System;
using System.Collections.Generic;

using System.Diagnostics;
using System.Windows.Forms;
using ENV.Advanced;
using ENV.Data.DataProvider;
using ENV.UI;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.Data.UnderConstruction;
using Firefly.Box.Flow;
using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;
using iTextSharp.text.pdf;
using Form = Firefly.Box.UI.Form;
using ENV.BackwardCompatible;
using Grid = ENV.UI.Grid;
using GridColumn = ENV.UI.GridColumn;
using SubForm = Firefly.Box.UI.SubForm;
using Firefly.Box.Data;

namespace ENV
{
    [Firefly.Box.UI.Advanced.ContainsData]
    public abstract class AbstractUIController : ControllerBase, IHaveUserMethods
    {
        internal UIController _uiController;
        public static event Action<UIController> StartOfInstance;
        internal int _subformExecMode = -1;
        protected internal ENV.UserMethods u;
        UserMethods IHaveUserMethods.GetUserMethods()
        {
            return u;
        }
        internal bool _inFilterTemplateMode = false, _inFindRowsTemplateMode = false;
        internal static int SortingScreenInterval = 0, FilterRowsScreenInterval = 0;
        public static bool UndoChangesInRowRevertsControllerState = false;
        public static bool DefaultAllowSelectOrderBy = true;

        public AbstractUIController()
        {
            UIController.AdvancedSettings advancedSettings = new UIController.AdvancedSettings();
            advancedSettings.ConditionalTransaction = UserSettings.ConditionalTransaction;
            advancedSettings.UseChildOpenTransaction = true;
            advancedSettings.AccumulateChangeEventOnReevaluation = true;
            advancedSettings.UndoChangesInRowRevertsControllerState = UndoChangesInRowRevertsControllerState;



            advancedSettings.SuppressRowLockingInBrowseActivity = UserSettings.SuppressUpdatesInBrowseActivity;

            if (UserSettings.Version8Compatible)
            {
                advancedSettings.ParentViewAccordingToHandlerContext = true;
                advancedSettings.ProcessExpressionCommandHandlersInOrderWithOtherHandlers = true;
                advancedSettings.EnableGotoFirstRowCommandWhenCurrentRowIsFirst = true;
                advancedSettings.EnableGotoLastRowCommandWhenCurrentRowIsLast = true;
            }

            if (UserSettings.VersionXpaCompatible)
            {
                advancedSettings.KeepNewRowValuesVisibleAfterExit =
                    KeepNewRowValuesVisibleAfterExit.Always;

                _ensureResponsiveUIWhileSortingOrFindingRowsStopWatch = Stopwatch.StartNew();
                advancedSettings.FindingRows += EnsureResponsiveUIWhileSortingOrFindingRows;
                advancedSettings.SortingRows += EnsureResponsiveUIWhileSortingOrFindingRows;

                advancedSettings.CheckExitBeforeRowWhenNoRows = true;
                advancedSettings.ForceInputValidationOfFocusedControlWhenLeavingRow = true;
            }
            else if (UserSettings.AlwaysKeepNewRowValuesVisibleAfterExit)
                advancedSettings.KeepNewRowValuesVisibleAfterExit = KeepNewRowValuesVisibleAfterExit.Yes;
            else if (UserSettings.Version10Compatible)
                advancedSettings.KeepNewRowValuesVisibleAfterExit = KeepNewRowValuesVisibleAfterExit.IfRowWasEntered;

            if (UserSettings.Version10Compatible)
            {
                advancedSettings.StayInInsertActivityIfUpdateNotAllowed = true;
                advancedSettings.SuppressEndOnExitAfterRollbackExceptionWithoutTransactionWhileInInsertActivity = true;
            }

            if (UserSettings.Version10Compatible && !UserSettings.VersionXpaCompatible)
            {
                advancedSettings.BindValuesInActiveControllerContext = true;
            }

            advancedSettings.ShowEmptyRowValuesWhenNoRows = true;
            if (DisableClosingByClickOnParent)
                advancedSettings.DisableClosingByClickOnParent = true;
            if (SuppressInputValidationOfFocusedControlWhenClosingByClickOnParentAndRowHasNotChanged)
                advancedSettings.SuppressInputValidationOfFocusedControlWhenClosingByClickOnParentAndRowHasNotChanged = true;
            if (HideReloadedRowsNotMatchingWhere)
                advancedSettings.HideReloadedRowsNotMatchingWhere = true;

            if (UserSettings.ProcessFlowBeforeQueuedEventsAfterExitingSubForm)
                advancedSettings.ProcessFlowBeforeQueuedEventsAfterExitingSubForm = true;

            _doOnAdvancedSettings(advancedSettings);


            if (SortingScreenInterval > 0)
                advancedSettings.SortingRows += new SimpleProgress("Sorting rows", () => _uiController.View, SortingScreenInterval).Progress;
            if (FilterRowsScreenInterval > 0)
                advancedSettings.FindingRows += new SimpleProgress("Finding matching rows", () => _uiController.View, SortingScreenInterval).Progress;


            _uiController = new UIController(advancedSettings);

            ControllerStart(this);
            {
                var reloadDataMode = new NumberColumn();

                var h = _uiController.Handlers.Add(Command.ReloadData, HandlerScope.CurrentTaskOnly);
                h.Parameters.Add(reloadDataMode);
                h.Invokes +=
                    e =>
                    {
                        if (_useUserMethodsLocateOnNextReloadData && _userMethodsLocate != null)
                            if (!_userMethodsLocate.IsEmpty)
                            {
                                e.Handled = true;
                                SaveRowAndDo(uio =>
                                {
                                    if (SortOnIncrementalSearch)
                                    {
                                        var newSort = TemplateModeFilter.InternalChooseIndexBasedOnColumns(OrderBy, From, new List<ColumnBase>(_locateFilterPerColumn.Keys));
                                        if (newSort != OrderBy)
                                        {
                                            newSort.Reversed = OrderBy.Reversed;
                                            this.OrderBy = newSort;

                                        }
                                    }
                                    uio.ReloadData(true);
                                    uio.GoToRow(_userMethodsLocate);
                                });
                            }
                        _useUserMethodsLocateOnNextReloadData = false;
                    };
            }
            {
                var indexNumber = new NumberColumn();
                var h = _uiController.Handlers.Add(Commands.ChangeOrderBy, HandlerScope.CurrentTaskOnly);
                h.Parameters.Add(indexNumber);
                h.Invokes += e =>
                {
                    Sort ob = null;
                    var x = OrderBy.Reversed;
                    try
                    {

                        var ent = From as ENV.Data.Entity;
                        ob = ent.Indexes[indexNumber];
                    }
                    catch { }
                    this.SaveRowAndDo(o =>
                    {
                        OrderBy = ob;
                        if (x)
                            OrderBy.Reversed = true;
                        o.ReloadData(true);
                    });



                };
            }

            _levelProvider = new LevelProvider(false, _uiController, this);

            _uiController.ReloadDataAfterUpdatingOrderByColumns = UserSettings.ReloadDataAfterUpdatingOrderByColumns;
            _uiController.Where.Add(_where);
            _uiController.EnterRow += MyEnterRow;
            _uiController.SavingRow += MySavingRow;
            _uiController.LeaveRow += () =>
            {
                OnLeaveRow();
                if (LeaveRow != null)
                    LeaveRow();
            };
            _uiController.Start += () =>
            {
                if (_subformExecMode != 0 || _uiController.ReloadDataOnReEntry)
                {
                    UserMethodsRangeReset();
                    UserMethodsLocateReset();
                    _uiController.RefreshWhere();
                }
                InitNullToColumns(true, Entities, u, _uiController.View);
                _beforeControllerStart();
                using (_levelProvider.Start())
                {
                    _enterRow = 0;
                    if (StartOfInstance != null)
                        StartOfInstance(_uiController);
                    OnStart();
                    if (Start != null)
                        Start();
                }
            };
            _uiController.End += () =>
            {
                using (_levelProvider.End())
                {
                    _withinOnEnd = true;
                    try
                    {
                        OnEnd();
                        if (End != null)
                            End();
                        if (_uiController.CurrentHandledCommand == Command.Select)
                            u._alternativeCurrentCommand = Command.Select;
                    }
                    finally
                    {
                        _withinOnEnd = false;
                    }
                }
            };
            _uiController.Load += MyLoad;
            _uiController.DatabaseErrorOccurred +=
                e =>
                {
                    if (e.ErrorType == DatabaseErrorType.LockedRow && e.HandlingStrategy == DatabaseErrorHandlingStrategy.Ignore)
                        e.HandlingStrategy = DatabaseErrorHandlingStrategy.Retry;
                    Common.DatabaseErrorOccurred(e, Relations, this.RowLocking);
                };
            _uiController.PreviewDatabaseError += Common.PreviewDatabaseError;
            _uiController.Deleting += new CancelEventHandler(_uiController_Deleting);
            _uiController.Undoing += new CancelEventHandler(_uiController_Undoing);
            _uiController.ActivityChanged += () => ApplyActivityToColumns(Activity);
            _uiController.StartOnRowWhereError += args =>
            {
                if (!DisableStartOnRowNotFoundError)
                    Common.WarningInStatusBar(args.RepositionToFirstRow ?
                    LocalizationInfo.Current.StartOnRowWhereErrorRepositionToFirstRow :
                    LocalizationInfo.Current.StartOnRowWhereError);
            };

            _handlers = new HandlerCollectionWrapper(_uiController.Handlers, _levelProvider, () => UserSettings.Version8Compatible,
                () => UserSettings.Version8Compatible, _cachedControllerManager, () => _application.AllPrograms, x => _uiController.Invoke(x));
            if (UserSettings.Version8Compatible)
            {
                var h = Handlers.Add(Keys.PageUp);
                h.BindEnabled(() =>
                {
                    if (Command.GoToPreviousPage.Enabled)
                        return false;
                    foreach (var c in this._uiController.View.Controls)
                    {
                        if (c is ENV.UI.Grid)
                            return true;
                    }
                    return false;
                });
                h.Invokes += e =>
                {
                    Raise(Command.GoToFirstRow);
                };
            }

            u = new UserMethods(this);
            _customOrderBy = MenuActionInstance.Create<CustomOrderByDialog>(
                () =>
                {
                    CustomOrderByDialog result = null;
                    Firefly.Box.Context.Current
            .InvokeUICommand(() => result = new CustomOrderByDialog());
                    return result;
                }
                            , d => d.ShowDialog(Common.ContextTopMostForm, _uiController), this, Commands.CustomOrderBy);
            _orderBy = MenuActionInstance.Create<SelectOrderByDialog>(
                            () =>
                            {
                                SelectOrderByDialog result = null;
                                Firefly.Box.Context.Current.InvokeUICommand(() => result = new SelectOrderByDialog());
                                return result;
                            }, (d) =>
                                d.ShowDialog(Common.ContextTopMostForm, _uiController), this, Commands.SelectOrderBy);
            if (!DefaultAllowSelectOrderBy)
                AllowSelectOrderBy = false;
            _exportData = MenuActionInstance.Create<ENV.UI.ExportDataDialog>(() => new ExportDataDialog(this),
                                                                                         dialog =>
                                                                                                 dialog.ShowDialog(Common.ContextTopMostForm)
                                                                                             , this, Commands.ExportData);
            bool canRun = true, wasInCreate = false; ;
            _filterRows = MenuActionInstance.Create<IUIControllerFilterUI>(
                            delegate
                            {
                                var result = CreateFilterUI(LocalizationInfo.Current.FilterRows, Where);
                                _filterRowsFilter = result.Filter;
                                Where.Add(_filterRowsFilter);
                                return result;
                            }, delegate (IUIControllerFilterUI form)
                            {
                                wasInCreate = (Activity == Activities.Insert);
                                if (!canRun)
                                    return;
                                canRun = false;
                                var filterApplied = false;
                                _uiController.SaveRowAndDo(
                                            delegate (UIOptions options)
                                            {
                                                _inFilterTemplateMode = true;
                                                var runAgain = false;
                                                do
                                                {
                                                    runAgain = false;
                                                    filterApplied = false;
                                                    if (form.Run(options, _currentColumn()))
                                                    {
                                                        if (_uiController.SortOnIncrementalSearch)
                                                            form.ChooseIndex(From, s => OrderBy = s);
                                                        _inFilterTemplateMode = false;
                                                        options.ReloadData();
                                                        options.GoToDefaultRow();
                                                        filterApplied = true;
                                                    }
                                                    if (filterApplied && _uiController.CachedRowsInfo.Count == 0)
                                                    {
                                                        runAgain = true;
                                                        Context.Current.BeginInvokeDelayed(() => Message.ShowWarningInStatusBar(LocalizationInfo.Current.NoRowsMatchFilter));
                                                    }
                                                } while (runAgain);
                                                _inFilterTemplateMode = false;
                                                SetActivityAcordingToMe();
                                            });
                                if (wasInCreate)
                                    SwitchBackToOriginalActivity();
                                canRun = true;
                            }, this, Commands.FilterRows);
            _findRow = MenuActionInstance.Create<IUIControllerFilterUI>(
                            () => CreateFilterUI(LocalizationInfo.Current.FindARow, StartOnRowWhere),
                            (d) =>
                            {
                                wasInCreate = (Activity == Activities.Insert);
                                if (!canRun)
                                    return;
                                canRun = false;
                                _uiController.SaveRowAndDo(o =>
                                {
                                    _findRowFilter = d.Filter;
                                    _wasSearch = true;
                                    _inFindRowsTemplateMode = true;
                                    if (d.Run(o, _currentColumn()))
                                    {
                                        if (_uiController.SortOnIncrementalSearch)
                                            d.ChooseIndex(From, s => OrderBy = s);
                                        _inFindRowsTemplateMode = false;
                                        o.ReloadData();
                                        o.GoToRow(d.Filter);
                                    }
                                    _inFindRowsTemplateMode = false;
                                    SetActivityAcordingToMe();
                                });
                                if (wasInCreate)
                                    SwitchBackToOriginalActivity();
                                canRun = true;
                            },
                            this, Commands.FindRows);
            _findNextRow = MenuActionInstance.Create<FilterForm>(() => null,
                            delegate
                            {
                                if (_findRowFilter == null && StartOnRowWhere.IsEmpty)
                                {
                                    Raise(Command.GoToNextRow);
                                    return;
                                }

                                _uiController.SaveRowAndDo(
                                    o =>
                                    {
                                        var fc = new FilterCollection();
                                        if (_findRowFilter != null)
                                            fc.Add(_findRowFilter);
                                        else
                                            fc.Add(StartOnRowWhere);
                                        FilterBase afterRowFilter = null;

                                        var eq = new List<ColumnBase>();
                                        Action<ColumnBase, int> addToAfterRowFilter = (c, d) =>
                                        {
                                            if (eq.Contains(c))
                                                return;
                                            var myf = new FilterCollection();
                                            if (OrderBy.Reversed)
                                                d *= -1;
                                            foreach (var item in eq)
                                            {
                                                AddFilter(item, myf, 0);
                                            }
                                            eq.Add(c);
                                            AddFilter(c, myf, d);
                                            if (afterRowFilter == null)
                                                afterRowFilter = myf;
                                            else
                                                afterRowFilter = afterRowFilter.Or(myf);
                                        };
                                        foreach (var s in OrderBy.Segments)
                                        {
                                            addToAfterRowFilter(s.Column, s.Direction == SortDirection.Descending ? -1 : 1);
                                        }
                                        if (!OrderBy.Unique)
                                            foreach (var item in From.PrimaryKeyColumns)
                                            {
                                                addToAfterRowFilter(item, 1);
                                            }
                                        if (afterRowFilter != null)
                                        {

                                            fc.Add(() => FilterBase.IsTrue(afterRowFilter));
                                        }
                                        o.ReloadData();
                                        o.GoToRow(fc, true);


                                    });
                            }, this, Commands.FindNextRow);


            _exportData.Enabled = false;

            _uiController.Activated +=
                            delegate
                            {
                                SetActivityAcordingToMe();
                                _uiController.ActivityChanged += SetActivityAcordingToMe;
                            };
            _uiController.Deactivated +=
                delegate ()
                {
                    Common.SetActivityText("");
                    _uiController.ActivityChanged -= SetActivityAcordingToMe;
                };


            OnDatabaseErrorRetry = true;
            _uiController.ExitingAfterFailedReactivatingInvalidatedControl += _uiController_Undoing;
            _uiController.NoDataStateEntered += () => _levelProvider.StartContext("RM", "RM", null);
            _uiController.IncrementalSearch += args => _findRowFilter = new FilterCollection(args.Filter);
            _uiController.IllegalActivity += () => Common.ShowMessageBox(LocalizationInfo.Current.IllegalActivity, MessageBoxIcon.Error, LocalizationInfo.Current.IllegalActivity);
        }

        internal override void _MarkRowAsChanged()
        {
            _uiController.MarkRowAsChanged();
        }
        void SwitchBackToOriginalActivity()
        {
            if (_originalActivity == Activities.Browse && AllowBrowse)
                Raise(Command.SwitchToBrowseActivity);
            else if (Activity != Activities.Update && AllowUpdate)
                Raise(Command.SwitchToUpdateActivity);
        }

        protected event Action RowChanging { add { _uiController.RowChanging += value; } remove { _uiController.RowChanging -= value; } }
        protected event Action AfterSavingRow { add { _uiController.AfterSavingRow += value; } remove { _uiController.AfterSavingRow -= value; } }
        internal override ITask GetITask()
        {
            return _uiController;
        }


        protected void RegisterCachedController<controllerClass>(System.Action<controllerClass> runTheController)
            where controllerClass : class
        {
            var c = (CachedTask<controllerClass>)_cachedControllerManager.GetTheCachedController<controllerClass>();
            var h = Handlers.Add(new CustomCommand());
            h.RegisterCalledTask(c);
            h.Invokes += e => ((Action)(() => runTheController(c.Instance)))();
        }
        protected void RegisterCachedController<controllerClass>(System.Action runTheController)
           where controllerClass : class
        {
            RegisterCachedController<controllerClass>(x => runTheController());
        }
        protected bool DisableStartOnRowNotFoundError { get; set; }
        ColumnBase _currentColumn()
        {
            var parkedColumn = _uiController.View.LastFocusedControl;
            if (parkedColumn != null && parkedColumn.GetColumn() != null)
                return parkedColumn.GetColumn();
            return null;
        }

        bool _wasSearch = false;
        internal void MatchController(AbstractUIController parentController)
        {
            Activity = parentController.Activity;
            AllowUpdate = parentController.AllowUpdate;
            AllowDelete = parentController.AllowDelete;
            AllowInsert = parentController.AllowInsert;
        }
        internal bool UseWildcardForContainsInTextColumnFilter = false;
        internal virtual IUIControllerFilterUI CreateFilterUI(string caption, FilterCollection originalFilter)
        {
            if (BackwardCompatibleFilterUI)
                return new TemplateModeFilter(_uiController, this, caption, () => ApplyActivityToColumns(Activity), UseWildcardForContainsInTextColumnFilter,
                    action => _provideColumnsForFilter((column, icb) => action(column, icb)), originalFilter);
            else
                return new FilterForm(caption, _provideColumnsForFilter, this.u);

        }
        Action _setOrderBy;
        Func<Sort> _currentBindOrderBy, _originalBindOrderBy;
        Sort _lastSetOrderBy;
        protected internal void BindOrderBy(Func<Sort> orderBy)
        {
            _currentBindOrderBy = orderBy;
            Action setOrderBy = delegate
            {
                var reversed = OrderBy.Reversed;
                _uiController.OrderBy = _currentBindOrderBy();
                _lastSetOrderBy = _uiController.OrderBy;
                OrderBy.Reversed = reversed;

                var isamEntity = From as ENV.Data.IsamEntity;
                if (isamEntity != null)
                {
                    if (isamEntity.Indexes.IndexOf(OrderBy) != 0)
                        isamEntity.FetchIndex = OrderBy;
                }
                var idx = _uiController.OrderBy as Data.Index;
                if (idx != null && idx.ForceGridStartOnRowPositionTopRow)
                {
                    foreach (var item in _uiController.View.Controls)
                    {
                        var g = item as Grid;
                        if (g != null)
                            g.StartOnRowPosition = GridStartOnRowPosition.TopRow;
                    }
                }
            };
            if (_setOrderBy != null)
            {
                _setOrderBy = setOrderBy;
                return;
            }
            _setOrderBy = setOrderBy;
            _uiController.Load += () => _setOrderBy();
            var x = _uiController.Handlers.Add(Command.ReloadData, HandlerScope.CurrentTaskOnly);
            x.Scope = HandlerScope.CurrentTaskOnly;
            var reloadDataMode = new Firefly.Box.Data.NumberColumn();
            var keepModifiedSort = new Firefly.Box.Data.BoolColumn();
            x.Parameters.Add(reloadDataMode, keepModifiedSort);
            x.Invokes +=
                y =>
                {
                    if (!keepModifiedSort.Value || _lastSetOrderBy == OrderBy)
                        _setOrderBy();
                    if (_userMethodsSort != null && OrderBy == _userMethodsSort)
                    {
                        y.Handled = true;
                        SaveRowAndDo(uio =>
                        {
                            uio.ReloadData();
                            uio.GoToDefaultRow();
                        });
                    }
                };
        }


        internal event Action SavingRow, EnterRow, Start, End, Load;
        internal protected event Action LeaveRow;

        void AddFilter(ColumnBase col, FilterCollection f, int i)
        {
            AddFilter<Number>(col, f, i);
            AddFilter<Date>(col, f, i);
            AddFilter<Time>(col, f, i);
            AddFilter<Text>(col, f, i);
            AddFilter<Bool>(col, f, i);
            AddFilter<byte[]>(col, f, i);

        }
        void AddFilter<T>(ColumnBase col, FilterCollection f, int i)
        {
            {
                var c = col as TypedColumnBase<T>;
                if (c != null)
                    switch (i)
                    {
                        case 0:
                            f.Add(c.IsEqualTo(c.Value));
                            break;
                        case -1:
                            f.Add(c.IsLessThan(c.Value));
                            break;
                        case 1:
                            f.Add(c.IsGreaterThan(c.Value));
                            break;
                    }
            }
        }

        internal FilterCollection _findRowFilter = null;
        FilterCollection _filterRowsFilter = null;

        void _uiController_Undoing(CancelEventArgs e)
        {
            if (_uiController.CurrentHandledCommand == Command.UndoChangesInRowAndExit)
                return;
            if (!_uiController.RowChanged)
                return;
            if (Common._suppressDialogForTesting)
                return;
            if (!_confirmUndo())
                return;
            e.Cancel = !Common.ShowYesNoMessageBox(ENV.LocalizationInfo.Current.Undo, ENV.LocalizationInfo.Current.DoYouWantToUndo, false);
        }




        internal abstract void _doOnAdvancedSettings(UIController.AdvancedSettings settings);

        void SetActivityAcordingToMe()
        {
            switch (Activity)
            {
                case Firefly.Box.Activities.Update:
                    Common.SetActivityText(ENV.LocalizationInfo.Current.Update);
                    break;
                case Firefly.Box.Activities.Insert:
                    Common.SetActivityText(ENV.LocalizationInfo.Current.Insert);
                    break;
                case Firefly.Box.Activities.Delete:
                    Common.SetActivityText(ENV.LocalizationInfo.Current.Delete);

                    break;
                case Firefly.Box.Activities.Browse:
                    Common.SetActivityText(ENV.LocalizationInfo.Current.Browse);
                    break;
                default:
                    Common.SetActivityText("");
                    break;
            }
        }


        MenuActionInstance _customOrderBy;
        MenuActionInstance _orderBy;
        MenuActionInstance _filterRows;
        MenuActionInstance _findRow;
        MenuActionInstance _findNextRow;
        MenuActionInstance _exportData;

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


            public static MenuActionInstance Create<T>(Func<T> factory, Action<T> whatToDo, AbstractUIController uic, UIControllerCustomCommand staticItem)
            {
                T _instance = default(T);
                uic.End += () => _instance = default(T);
                return new MenuActionInstance(e =>
                {
                    if (_instance == null)
                        _instance = factory();
                    whatToDo(_instance);
                    e.Handled = true;
                }, uic, staticItem);

            }

            UIControllerCustomCommand _staticItem;
            Action _activate;
            public void ActivateIfActive()
            {
                if (_isActive)
                    _activate();
            }
            public MenuActionInstance(Action<HandlerInvokeEventArgs> whatToDo, AbstractUIController uic, UIControllerCustomCommand staticItem)
            {
                _staticItem = staticItem;
                Action activate = delegate
                {
                    _activate();
                    _isActive = true;
                };
                _activate = () => staticItem.SetAction(whatToDo
                                         , _enabled && uic.From is Firefly.Box.Data.Entity && uic.AllowActivitySwitch);

                uic.BecomingTheCurrentTask += activate;
                uic.NoLongerTheCurrentTask += delegate { staticItem.ClearAction(); _isActive = false; };
            }
            bool _isActive = false;
        }

        void _uiController_Deleting(CancelEventArgs e)
        {
            if (Common._suppressDialogForTesting)
                return;
            try
            {
                _forceDeleteActivity = true;
                if (!_confirmDelete())
                    return;
            }
            finally
            {
                _forceDeleteActivity = false;
            }
            e.Cancel = !Common.ShowYesNoMessageBox(ENV.LocalizationInfo.Current.Delete,
                                                  ENV.LocalizationInfo.Current.DoYouWantToDelete, false);

        }
        internal virtual bool SuppressUpdatesInBrowseActivity { get { return UserSettings.SuppressUpdatesInBrowseActivity; } }
        internal FlowAbortException LastFlowAbortException;
        void MySavingRow(SavingRowEventArgs e)
        {
            LastFlowAbortException = null;
            try
            {
                using (_levelProvider.SavingRow())
                {
                    if (_confirmUpdate() && Activity != Activities.Delete && _subformExecMode != 1)
                    {
                        DialogResult r =
                            Common.ShowYesNoCancelMessageBox(LocalizationInfo.Current.ConfirmUpdateMessage,
                                LocalizationInfo.Current.ConfirmUpdateTitle);
                        if (r == DialogResult.Cancel)
                            throw new FlowAbortException();
                        if (r == DialogResult.No)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                    OnSavingRow();
                    if (SavingRow != null)
                        SavingRow();

                    if (_forceExitAsSoonAsPossible && ForceSaveRow && !RowChanged)
                        e.Cancel = true;
                    else if (SuppressUpdatesInBrowseActivity && Activity == Firefly.Box.Activities.Browse)
                    {
                        e.Cancel = true;
                        if (AnyColumnWithEntityChanged || _AnyColumnOfRelationSetToBoundValue(Relations))
                        {
                            Context.Current.DiscardPendingCommands();
                            Common.WarningInStatusBar(LocalizationInfo.Current.UpdateNotAllowedInBrowseMode);
                        }
                    }
                }
                if (!e.Cancel)
                    _levelProvider.AfterSavingRow();
            }
            catch (FlowAbortException ex)
            {
                LastFlowAbortException = ex;
                _forceExitAsSoonAsPossible = false;
                throw;
            }
        }



        internal void ResetSort()
        {
            var reversed = OrderBy.Reversed;
            OrderBy = _originalBindOrderBy();
            OrderBy.Reversed = reversed;
        }
        internal void AddUserMethodsSort(ColumnBase columnBase, Bool ascending)
        {
            if (OrderBy != _userMethodsSort)
            {
                OrderBy = _userMethodsSort = new Sort();
            }
            var be = From as BtrieveEntity;
            if (be != null && !be.IsUsingBtrieve)
            {
                var old = OrderBy;
                OrderBy = _userMethodsSort = new Sort();
                for (int i = 0; i < old.Segments.Count - 1; i++)
                {
                    var s = old.Segments[i];
                    OrderBy.Add(s.Column, s.Direction);
                }
                OrderBy.Segments.Add(columnBase, ascending ? SortDirection.Ascending : SortDirection.Descending);
                OrderBy.Segments.Add(be.IdentityColumn, ascending ? SortDirection.Ascending : SortDirection.Descending);
            }
            else
                OrderBy.Segments.Add(columnBase, ascending ? SortDirection.Ascending : SortDirection.Descending);
            if (_enterRow == 0)
                _uiController.RefreshWhere();
        }

        Sort _userMethodsSort;
        internal bool _inOnLoad;
        void MyLoad()
        {
            AllowInsertInUpdateActivity = UserSettings.DefaultAllowInsertInUpdateActivity;
            var z = _originalBindOrderBy;
            if (z != null)
            {
                ResetSort();
                BindOrderBy(z);
            }
            else if (_currentBindOrderBy != null)
                _originalBindOrderBy = _currentBindOrderBy;
            else
            {
                var x = OrderBy;
                _originalBindOrderBy = () => x;
            }

            InitNullToColumns(false, Entities, u, _uiController.View);
            using (ENV.Utilities.Profiler.Level(this, "OnLoad"))
            {
                _inOnLoad = true;
                try
                {
                    OnLoad();
                    if (Load != null)
                        Load();
                }
                finally
                {
                    _inOnLoad = false;
                }
                if (UserSettings.Version8Compatible)
                    OnDatabaseErrorRetry = false;

                if (_uiController.View == null && !Common._suppressDialogForTesting)
                    Common.ErrorInStatusBar(LocalizationInfo.Current.UIControllerWithoutView);
                _bindView(_uiController.View);
                _bindView = delegate { };
                ApplyActivityToColumns(Activity);

                if (false)//W4956 disabled for now along with testes in TestClass59 as I think it's better to determine this per client and not per database
                {
                    var e = _uiController.From as ENV.Data.Entity;
                    if (_uiController.View != null && e != null && !(e.DataProvider is DynamicSQLSupportingDataProvider))
                    {
                        foreach (var g in _uiController.View.Controls)
                        {
                            var grid = g as Grid;
                            if (g != null)
                            {
                                foreach (var control in grid.Controls)
                                {
                                    var gc = control as Firefly.Box.UI.GridColumn;
                                    if (gc != null)
                                        gc.ParkOnFirstRowOnSort = true;
                                }
                            }
                        }
                    }
                }
            }
            _originalActivity = Activity;
            _AfterOnLoad();
        }

        protected virtual void OnSavingRow() { ENV.Utilities.Profiler.DontWriteIfEmpty(); }
        protected virtual void OnLeaveRow() { ENV.Utilities.Profiler.DontWriteIfEmpty(); }
        protected virtual void OnEnd() { ENV.Utilities.Profiler.DontWriteIfEmpty(); }
        protected virtual void OnStart() { ENV.Utilities.Profiler.DontWriteIfEmpty(); }
        void MyEnterRow()
        {
            _levelProvider.StartContext("RM", "RM", null);
            _enterRow++;
            _callMeOnEnterRow();
            using (_levelProvider.EnterRow())
            {

                OnEnterRow();
                if (EnterRow != null)
                    EnterRow();
                var ff = _uiController.View as ENV.UI.Form;
                if (ff != null)
                {
                    ff.ControllerEnterRow();
                }
            }
        }
        internal override Activities GetActivity()
        {
            return Activity;
        }
        internal override RelationCollection GetRelations()
        {
            return Relations;
        }
        internal int _enterRow = 0;

        protected virtual void OnEnterRow() { ENV.Utilities.Profiler.DontWriteIfEmpty(); }
        protected virtual void OnLoad() { ENV.Utilities.Profiler.DontWriteIfEmpty(); }

        internal override void RunTheTask()
        {

            _uiController.Run();

        }



        Action<Firefly.Box.UI.Form> _bindView = delegate { };
        protected override sealed void BindView(Form form)
        {
            if (form != null)
                form.EvaluateFormCreationBindings();

            _bindView = delegate (Form taskForm)
            {
                Common.BindForms(taskForm, form);
                _bindView = delegate { };
            };
        }

        internal override void ProvideSubForms(Action<SubForm> runForEachSubForm)
        {
            if (_uiController.View == null) return;
            foreach (Control control in _uiController.View.Controls)
            {
                var sf = control as SubForm;
                if (sf != null)
                    runForEachSubForm(sf);
            }
        }


        protected internal void AddAllColumns()
        {
            _uiController.AddAllColumns();
        }


        protected Number DeltaOf(Func<Number> expression)
        {
            return _uiController.DeltaOf(expression);
        }
        protected Time DeltaOf(Func<Time> expression)
        {
            return u.ToTime(_uiController.DeltaOf(() => u.ToNumber(expression())));
        }
        protected Number DeltaOf(Func<Date> expression)
        {
            return _uiController.DeltaOf(() => u.ToNumber(expression()));
        }



        internal protected void Raise(Command command, params object[] untypedArgs)
        {
            ControllerBase.RaiseHappened(command);
            _uiController.Raise(command, untypedArgs);
        }

        protected void Raise(Keys keys, params object[] untypedArgs)
        {
            ControllerBase.RaiseHappened(keys);
            _uiController.Raise(keys, untypedArgs);
        }
        protected void Raise(string customCommandKey, params object[] untypedArgs)
        {
            ControllerBase.RaiseHappened(customCommandKey);
            _uiController.Raise(HandlerCollectionWrapper.FixCustomCommandKey(customCommandKey), untypedArgs);
        }


        internal protected void Invoke(CommandWithArgs command)
        {

            _uiController.Invoke(command);
        }
        internal protected void Raise(CommandWithArgs command)
        {

            _uiController.Raise(command);
        }
        protected void Invoke(string customCommandKey, params object[] untypedArgs)
        {
            _uiController.Invoke(HandlerCollectionWrapper.FixCustomCommandKey(customCommandKey), untypedArgs);
        }



        internal protected void Invoke(Command command, params object[] untypedArgs)
        {
            _uiController.Invoke(command, untypedArgs);
        }

        protected void Invoke(System.Windows.Forms.Keys keyCombination, params object[] untypedArgs)
        {
            _uiController.Invoke(keyCombination, untypedArgs);
        }
        internal protected void Raise<T>(Command command, ArrayColumn<T> untypedArgs)
        {
            ControllerBase.RaiseHappened(command);
            _uiController.Raise(command, untypedArgs);
        }

        protected void Raise<T>(Keys keys, ArrayColumn<T> untypedArgs)
        {
            ControllerBase.RaiseHappened(keys);
            _uiController.Raise(keys, untypedArgs);
        }
        protected void Raise<T>(string customCommandKey, ArrayColumn<T> untypedArgs)
        {
            ControllerBase.RaiseHappened(customCommandKey);
            _uiController.Raise(HandlerCollectionWrapper.FixCustomCommandKey(customCommandKey), untypedArgs);
        }


        protected void Invoke<T>(string customCommandKey, ArrayColumn<T> untypedArgs)
        {
            _uiController.Invoke(HandlerCollectionWrapper.FixCustomCommandKey(customCommandKey), untypedArgs);
        }



        internal protected void Invoke<T>(Command command, ArrayColumn<T> untypedArgs)
        {
            _uiController.Invoke(command, untypedArgs);
        }

        protected void Invoke<T>(System.Windows.Forms.Keys keyCombination, ArrayColumn<T> untypedArgs)
        {
            _uiController.Invoke(keyCombination, untypedArgs);
        }

        protected internal T Invoke<T>(Func<T> func)
        {
            T r = default(T);
            _uiController.Invoke(() => r = func());
            return r;
        }
        protected internal HandlerCollectionWrapper Handlers
        {
            get { return _handlers; }
        }

        protected Bool CalcExpression(object expression)
        {
            return false;
        }

        protected Bool CalcExpression(Func<Bool> expression)
        {
            if (expression != null)
                return expression();
            return false;
        }


        internal protected LockingStrategy RowLocking
        {
            get { return _uiController.RowLocking; }
            set { _uiController.RowLocking = value; }
        }
        internal protected TransactionType TransactionType
        {
            get { return _uiController.TransactionType; }
            set { _uiController.TransactionType = value; }
        }
        internal protected TransactionScopes TransactionScope
        {
            get { return _uiController.TransactionScope; }
            set { _uiController.TransactionScope = value; }
        }



        internal protected Func<Form> View
        {
            set
            {
                if (Common._suppressDialogForTesting)
                {
                    if (_uiController.View == null)
                        _uiController.View = new Form();
                    return;
                }

                SetView(value, v => _uiController.View = v,
                    f =>
                    {
                        if (f.ContextMenuStrip == null && _subformExecMode == -1)
                        {
                            if (_application != null && _application._defaultContextMenuType != null)
                            {
                                var comp = new System.ComponentModel.Container();
                                var cm = (ContextMenuStrip)System.Activator.CreateInstance(_application._defaultContextMenuType, comp);
                                if (cm.Items.Count > 0)
                                {
                                    f.Disposed += delegate { comp.Dispose(); };
                                    f.ContextMenuStrip = cm;
                                }
                            }
                            else if (_application != null && !_application.RunnedAsApplication)
                                f.ContextMenuStrip = new ENV.UI.Menus.ContextMenuStripBase();
                        }
                    }, true);
            }
        }
        public sealed override string ToString()
        {
            return base.ToString();
        }
        public sealed override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public sealed override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        protected bool AllowInsertInUpdateActivity
        {
            get { return _uiController.AllowInsertInUpdateActivity; }
            set { _uiController.AllowInsertInUpdateActivity = value; }
        }

        internal protected string Title
        {
            get { return _uiController.Title; }
            set { _uiController.Title = value; }
        }

        internal sealed override protected Firefly.Box.Data.Entity From
        {
            set
            {
                _uiController.From = value;
            }
            get { return _uiController.From; }
        }

        protected internal Sort OrderBy
        {
            set
            {
                _uiController.OrderBy = value;
                BindOrderBy(() => value);
            }
            get { return _uiController.OrderBy; }
        }

        bool _forceDeleteActivity = false;
        Activities _originalActivity;
        internal protected Activities Activity
        {
            get
            {
                if (_forceDeleteActivity)
                    return Activities.Delete;
                if (_inFilterTemplateMode)
                    return (Activities)(-1);
                else if (_inFindRowsTemplateMode)
                    return (Activities)(-2);
                else if (_uiController.ExitedBecauseThereAreNoRowsAndInsertActivityIsNotAllowed)
                    return (Activities)(-9);
                return _uiController.Activity;
            }
            set
            {
                _uiController.Activity = value;
            }
        }


        FilterCollection _where = new FilterCollection();

        internal protected FilterCollection Where
        {
            get { return _where; }
        }

        /// <summary>
        /// This Where will always be evaluated in memory and may have performance implications
        /// </summary>
        protected FilterCollection NonDbWhere
        {
            get { return _uiController.NonDbWhere; }
        }

        internal protected FilterCollection StartOnRowWhere
        {
            get { return _uiController.StartOnRowWhere; }
        }


        protected void BindAllowDelete(Func<bool> condition)
        {
            _uiController.BindAllowDelete(condition);
        }
        protected internal void BindAllowActivitySwitch(Func<bool> condition)
        {
            _uiController.BindAllowActivitySwitch(condition);
        }
        protected internal bool AllowDelete
        {
            set { _uiController.AllowDelete = value; }
            get { return _uiController.AllowDelete; }
        }

        protected internal bool ReevaluateBindValueAndRelationsOnEnterRow
        {
            set { _uiController.ReevaluateBindValueAndRelationsOnEnterRow = value; }
            get { return _uiController.ReevaluateBindValueAndRelationsOnEnterRow; }
        }



        internal protected bool AllowActivitySwitch
        {
            get { return _uiController.AllowActivitySwitch; }
            set
            {
                _uiController.AllowActivitySwitch = value;
                _customOrderBy.ActivateIfActive();
                _orderBy.ActivateIfActive();
                _filterRows.ActivateIfActive();
                _findRow.ActivateIfActive();
                _findNextRow.ActivateIfActive();
                _exportData.ActivateIfActive();


            }
        }


        protected internal bool AllowInsert
        {
            set { _uiController.AllowInsert = value; }
            get { return _uiController.AllowInsert; }
        }
        protected internal void BindAllowInsert(Func<bool> condition)
        {
            _uiController.BindAllowInsert(condition);
        }


        protected internal bool AllowUpdate
        {
            set { _uiController.AllowUpdate = value; }
            get { return _uiController.AllowUpdate; }
        }
        protected void BindAllowUpdate(Func<bool> condition)
        {
            _uiController.BindAllowUpdate(condition);
        }

        protected bool AllowBrowse
        {
            set { _uiController.AllowBrowse = value; }
            get { return _uiController.AllowBrowse; }
        }
        protected void BindAllowBrowse(Func<bool> expression)
        {
            _uiController.BindAllowBrowse(expression);
        }


        protected bool AllowIncrementalSearch
        {
            set { _uiController.AllowIncrementalSearch = value; }
            get { return _uiController.AllowIncrementalSearch; }
        }
        protected void BindAllowIncrementalSearch(Func<bool> condition)
        {
            _uiController.BindAllowIncrementalSearch(condition);
        }

        internal protected bool GoToToNextRowAfterLastControl
        {
            set { _uiController.GoToToNextRowAfterLastControl = value; }
            get { return _uiController.GoToToNextRowAfterLastControl; }
        }
        protected void BindGoToToNextRowAfterLastControl(Func<bool> condition)
        {
            _uiController.BindGoToToNextRowAfterLastControl(condition);
        }
        protected void BindForceSaveRow(Func<bool> condition)
        {
            _uiController.BindForceSaveRow(condition);
        }
        internal protected bool ForceSaveRow
        {
            get { return _uiController.ForceSaveRow; }
            set { _uiController.ForceSaveRow = value; }
        }


        internal protected void DeleteRowAfterLeavingItIf(Func<bool> condition)
        {
            _uiController.DeleteRowAfterLeavingItIf(() => (!UserSettings.SuppressUpdatesInBrowseActivity || Activity != Activities.Browse) && condition());
        }

        protected internal bool AllowSelect
        {
            set { _uiController.AllowSelect = value; }
            get { return _uiController.AllowSelect; }
        }

        protected bool StartFromLastRow
        {
            set { _uiController.StartFromLastRow = value; }
            get { return _uiController.StartFromLastRow; }
        }

        protected bool StartFromFirstRowIfStartOnRowWhereFails
        {
            set { _uiController.StartFromFirstRowIfStartOnRowWhereFails = value; }
            get { return _uiController.StartFromFirstRowIfStartOnRowWhereFails; }
        }

        internal protected bool KeepViewVisibleAfterExit
        {
            set { _uiController.KeepViewVisibleAfterExit = value; }
            get { return _uiController.KeepViewVisibleAfterExit; }
        }
        internal protected void BindKeepViewVisibleAfterExit(Func<bool> condition)
        {
            _uiController.BindKeepViewVisibleAfterExit(condition);
        }

        protected internal bool ReloadDataAfterSavingRow
        {
            get { return _uiController.ReloadDataAfterSavingRow; }
            set { _uiController.ReloadDataAfterSavingRow = value; }
        }
        protected void BindReloadDataAfterSavingRow(Func<bool> condition)
        {
            _uiController.BindReloadDataAfterSavingRow(condition);
        }






        protected internal sealed override ColumnCollection Columns
        {
            get { return _uiController.Columns; }
        }
        protected EntityCollection Entities { get { return _uiController.Entities; } }

        protected void LockCurrentRowIfItWasChanged()
        {
            if (RowChanged)
                LockCurrentRow();
        }

        internal protected void LockCurrentRow()
        {
            try
            {
                _uiController.LockCurrentRow();
            }
            catch (RollbackException)
            {
                if (!u.InTrans() && !_uiController.Equals(Context.Current.ActiveTasks[Context.Current.ActiveTasks.Count - 1]))
                    Raise(Command.UndoChangesInRow);
                throw;
            }
        }

        class RelationCollectionDebugProxy
        {
            public RelationCollectionDebugProxy(object o)
            {
                MessageBox.Show(o.GetType().ToString());
            }
        }


        internal protected RelationCollection Relations
        {
            get { return _uiController.Relations; }
        }




        protected bool SortOnIncrementalSearch
        {
            get { return _uiController.SortOnIncrementalSearch; }
            set { _uiController.SortOnIncrementalSearch = value; }
        }
        protected void BindSortOnIncrementalSearch(Func<bool> condition)
        {
            _uiController.BindSortOnIncrementalSearch(condition);
        }


        protected bool AllowCustomOrderBy
        {
            get { return _customOrderBy.Enabled; }
            set
            {
                _customOrderBy.Enabled = value;

            }
        }
        protected bool AllowExportData
        {
            get
            {
                return _exportData.Enabled;
            }
            set
            {
                _exportData.Enabled = value;
            }
        }
        protected bool AllowSelectOrderBy
        {
            get
            {
                return _orderBy.Enabled;
            }
            set
            {
                _orderBy.Enabled = value;
            }
        }
        protected bool AllowFilterRows
        {
            get { return _filterRows.Enabled; }
            set { _filterRows.Enabled = value; }
        }
        protected bool AllowFindRow
        {
            get
            {
                return _findRow.Enabled;
            }
            set { _findRow.Enabled = value; }
        }

        protected void BindAllowFindRow(Func<bool> condition)
        {
            EnterRow += () => AllowFindRow = condition();
        }
        protected void BindAllowExportData(Func<bool> condition)
        {
            EnterRow += () => AllowExportData = condition();
        }
        protected void BindSwitchToInsertWhenNoRows(Func<bool> condition)
        {
            Start += () => SwitchToInsertWhenNoRows = condition();
            EnterRow += () => SwitchToInsertWhenNoRows = condition();
        }
        protected void BindStartFromLastRow(Func<bool> condition)
        {
            Start += () => StartFromLastRow = condition();
        }
        protected void BindStartFromFirstRowIfStartOnRowWhereFails(Func<bool> condition)
        {
            Start += () => StartFromFirstRowIfStartOnRowWhereFails = condition();
        }


        Func<bool> _asSoonAsPossibleExitCondition = () => false;
        internal override sealed protected void Exit(ExitTiming timing, Func<bool> condition)
        {
            if (timing == ExitTiming.AsSoonAsPossible)
            {
                _asSoonAsPossibleExitCondition = condition;
                _forceExitAsSoonAsPossible = false;
                _uiController.Exit(ExitTiming.AsSoonAsPossible, ShouldExitAsSoonAsPossible);
            }
            else
            {
                _asSoonAsPossibleExitCondition = () => false;
                _uiController.Exit(timing, condition);
            }
        }

        internal bool ShouldExitAsSoonAsPossible()
        {
            if (_forceExitAsSoonAsPossible) return true;
            if (_asSoonAsPossibleExitCondition())
            {
                _forceExitAsSoonAsPossible = true;
                return true;
            }
            return false;
        }

        bool _forceExitAsSoonAsPossible = false;
        internal sealed override protected void CheckExit()
        {
            ShouldExitAsSoonAsPossible();
        }
        protected void Exit(ExitTiming timing)
        {
            Exit(timing, () => true);

        }


        protected void Exit()
        {
            _uiController.Exit();
        }

        internal void EndBeforeEnterRow(bool endTask)
        {

            Exit(ExitTiming.BeforeRow, () => endTask);
        }
        internal void YouAreADetailTask(System.Windows.Forms.Control positionedAt)
        {
            KeepViewVisibleAfterExit = true;
            _uiController.View.TitleBar = false;
            _uiController.View.Border = ControlBorderStyle.None;
            _uiController.View.ChildWindow = true;
            _uiController.View.Bounds = positionedAt.Bounds;
            positionedAt.Parent.Controls.Remove(positionedAt);
            positionedAt.Dispose();
        }
        internal protected bool InTransaction
        {
            get { return _uiController.InTransaction; }
        }
        Func<bool> _confirmUpdate = () => false;
        Func<bool> _confirmDelete = () => true;
        Func<bool> _confirmUndo = () => false;

        protected bool ConfirmUpdate
        {
            get { return _confirmUpdate(); }
            set { _confirmUpdate = () => value; }
        }
        protected void BindConfirmUpdate(Func<bool> condition)
        {
            _confirmUpdate = condition;
        }


        protected bool ConfirmDelete
        {
            get { return _confirmDelete(); }
            set { _confirmDelete = () => value; }
        }
        protected void BindConfirmDelete(Func<bool> condition)
        {
            _confirmDelete = condition;
        }
        protected bool ConfirmUndo
        {
            get { return _confirmUndo(); }
            set { _confirmUndo = () => value; }
        }
        protected void BindConfirmUndo(Func<bool> condition)
        {
            _confirmUndo = condition;
        }
        protected internal bool SwitchToInsertWhenNoRows
        {
            get { return _uiController.SwitchToInsertWhenNoRows; }
            set { _uiController.SwitchToInsertWhenNoRows = value; }
        }

        internal protected void DenyUndoForCurrentRow()
        {
            _uiController.DenyUndoForCurrentRow();
        }
        protected internal void setApplication(ApplicationControllerBase application)
        {
            if (application == null)
                return;
            _application = application;
            u.SetApplication(application);
            _uiController.Module = _application._moduleController;
            _myNullStrategy = application._nullStrategy;
        }

        protected sealed internal override void Execute()
        {
            _originalActivity = Activity;
            if (_filterRowsFilter != null)
                _filterRowsFilter.Clear();
            var be = From as BtrieveEntity;
            if (be != null)
                be.ReloadRowDataBeforeUpdate = true;
            if (_inSubformReload)
                return;
            try
            {
                _levelProvider.BeforeStart();
                lock (_activeUIControllers)
                    _activeUIControllers.Add(_uiController, this);

                {
                    base.Execute();
                }
            }
            finally
            {
                lock (_activeUIControllers)
                    _activeUIControllers.Remove(_uiController);
                u.DoEndOfTaskCleanup();
            }
        }
        protected internal ColumnBase CurrentColumn
        {
            get
            {
                var parkedColumn = _uiController.View.FocusedControl as InputControlBase;
                if (parkedColumn != null && parkedColumn.GetColumn() != null)
                    return parkedColumn.GetColumn();
                return null;
            }
        }

        protected internal bool RowChanged
        {
            get { return _uiController.RowChanged; }
        }
        internal override bool GetRowChanged()
        {
            return RowChanged;
        }

        internal protected bool PreloadData
        {
            get { return _uiController.PreloadData; }
            set { _uiController.PreloadData = value; }
        }
        protected void ReadAllRows(Action runForEachRow)
        {
            _uiController.ReadAllRows(runForEachRow);
        }

        protected void SaveRowAndDo(Action<UIOptions> action)
        {
            _uiController.SaveRowAndDo(action);
        }


        internal static Dictionary<UIController, AbstractUIController> _activeUIControllers = new Dictionary<UIController, AbstractUIController>();
        static bool _backwardCompatibleFilterUi = true;

        public static bool BackwardCompatibleFilterUI
        {
            get { return _backwardCompatibleFilterUi; }
            set { _backwardCompatibleFilterUi = value; }
        }

        FilterCollection _userMethodsLocate = null, _userMethodsRange, _userMethodNonDbWhereRange;
        bool _useUserMethodsLocateOnNextReloadData;
        Dictionary<ColumnBase, FilterCollection> _locateFilterPerColumn;
        internal void UserMethodsLocateReset()
        {
            if (_userMethodsLocate != null)
            {
                _userMethodsLocate.Clear();
                _locateFilterPerColumn.Clear();
            }
        }

        internal void AddUserMethodLocate(ColumnBase column, FilterBase obj)
        {
            if (_userMethodsLocate == null)
            {
                _userMethodsLocate = new FilterCollection();
                StartOnRowWhere.Add(_userMethodsLocate);
            }
            if (_locateFilterPerColumn == null)
                _locateFilterPerColumn = new Dictionary<ColumnBase, FilterCollection>();
            FilterCollection f;
            if (!_locateFilterPerColumn.TryGetValue(column, out f))
            {
                _locateFilterPerColumn.Add(column, f = new FilterCollection());
                _userMethodsLocate.Add(f);
            }
            else
            {
                f.Clear();
            }
            f.Add(obj);
            _useUserMethodsLocateOnNextReloadData = true;
        }
        internal void AddUserMethodRange(ColumnBase refColumn, FilterBase obj)
        {
            AddUserMethodRange(obj);
            var l = new List<ColumnBase>();
            l.Add(refColumn);
            var index = TemplateModeFilter.InternalChooseIndex(OrderBy, From, l, l, x => true);
            if (index != OrderBy)
            {
                var reversed = OrderBy.Reversed;
                OrderBy = index;
                OrderBy.Reversed = reversed;
            }
        }
        internal void AddUserMethodRange(FilterBase obj)
        {
            if (_userMethodsRange == null)
            {
                _userMethodsRange = new FilterCollection();
                _uiController.Where.Add(_userMethodsRange);
            }
            _userMethodsRange.Add(obj);
            if (_enterRow == 0)
                _uiController.RefreshWhere();
        }
        internal void AddUserMethodNonDbRange(FilterCollection obj)
        {

            if (_userMethodNonDbWhereRange == null)
            {
                _userMethodNonDbWhereRange = new FilterCollection();
                _uiController.NonDbWhere.Add(_userMethodNonDbWhereRange);
            }
            _userMethodNonDbWhereRange.Add(obj);
        }


        internal void UserMethodsRangeReset()
        {
            if (_userMethodsRange != null)
                _userMethodsRange.Clear();
            if (_userMethodNonDbWhereRange != null)
                _userMethodNonDbWhereRange.Clear();
        }
        protected internal Firefly.Box.Flow.Direction CurrentFlowDirection
        {
            get
            {
                var fv = new FlowVisitor();
                _uiController.VisitFlow(fv);
                switch (fv.Result)
                {
                    case "P":
                    case "R":
                        return Direction.Backward;
                }
                return Direction.Forward;
            }
        }
        protected internal Firefly.Box.Flow.FlowMode CurrentFlowMode
        {
            get
            {
                var fv = new FlowVisitor();
                _uiController.VisitFlow(fv);
                switch (fv.Result)
                {
                    case "N":
                    case "P":
                        return FlowMode.Tab;
                }
                return FlowMode.Skip;
            }
        }

        public static bool SuppressInputValidationOfFocusedControlWhenClosingByClickOnParentAndRowHasNotChanged { get; set; }

        public static bool HideReloadedRowsNotMatchingWhere { get; set; }

        internal class FlowVisitor : IFlowVisitor
        {
            public Text Result = "";

            public void TabForward()
            {
                Result = "N";
            }

            public void TabBackward()
            {
                Result = "P";
            }

            public void SkipForward()
            {
                Result = "F";
            }

            public void SkipBackward()
            {
                Result = "R";
            }

            public void Select()
            {
                Result = "S";
            }

            public void Cancel()
            {
                Result = "C";
            }

            public void None()
            {
                Result = "";
            }
        }

        bool _withinOnEnd;

        protected void RunInSubForm<T>(string subFormControlTag, bool focusOnSubform, Action runTask) where T : class
        {
            if (!RunInSubform(subFormControlTag, focusOnSubform,
                subform =>
                {
                    if (_cachedControllerManager.GetCachedController<T>() is AbstractUIController)
                    {
                        subform.Clear();
                        subform.SetController(_cachedControllerManager.GetCachedController<T>(), runTask);
                    }
                    else runTask();
                }))
                runTask();
        }
        protected void RunInSubForm<T>(string subFormControlTag, bool focusOnSubform, Action<T> runTask) where T : class
        {
            if (!RunInSubform(subFormControlTag, focusOnSubform,
                subform =>
                {
                    var x = _cachedControllerManager.GetCachedController<T>();
                    if (x is AbstractUIController)
                    {
                        subform.Clear();
                        x = _cachedControllerManager.GetCachedController<T>();
                        subform.SetController(x, () => runTask(x));
                    }
                    else runTask(x);
                }))
                runTask(_cachedControllerManager.GetCachedController<T>());
        }
        bool RunInSubform(string subFormControlTag, bool focusOnSubForm, Action<UI.SubForm> what)
        {
            if (_withinOnEnd) return false;

            var form = _uiController.View as UI.Form;
            if (form != null)
            {
                var subform = form.FindControlByTag(subFormControlTag) as UI.SubForm;
                if (subform != null)
                {
                    if (subform.IsControllerRunning())
                    {
                        _uiController.Raise(Command.RefreshSubForm, subFormControlTag);
                        return true;
                    }
                    var x = subform.TryFocusOnControllerChanged;
                    subform.TryFocusOnControllerChanged = focusOnSubForm;
                    try
                    {
                        _uiController.Invoke(() => what(subform));
                        return true;
                    }
                    finally
                    {
                        subform.TryFocusOnControllerChanged = x;
                    }
                }
            }
            return false;
        }
        protected void RunInSubForm(string subFormControlTag, bool focusOnSubform, Number programNumber, params object[] args)
        {
            if (!RunInSubform(subFormControlTag, focusOnSubform, subform => _application.AllPrograms.RunOnSubformByIndex(subform, programNumber, args)))
                _application.AllPrograms.RunByIndex(programNumber, args);
        }
        protected void RunInSubForm(string subFormControlTag, bool focusOnSubform, Text programPublicName, params object[] args)
        {
            if (!RunInSubform(subFormControlTag, focusOnSubform, subform => _application.AllPrograms.RunOnSubformByPublicName(subform, programPublicName, args)))
                _application.AllPrograms.RunByPublicName(programPublicName, args);
        }
        protected void RunInSubFormControllerFromAnUnreferencedApplication(string subFormControlTag, bool focusOnSubform, Text applicationKey, Text publicName, params object[] args)
        {
            if (!RunInSubform(subFormControlTag, focusOnSubform, subform => ApplicationControllerBase.InternalRunProgramFromAnUnreferencedApplication(applicationKey, publicName, app => app.AllPrograms.RunOnSubformByPublicName(subform, publicName, args), x => x(_application))))
                _application.RunControllerFromAnUnreferencedApplication(applicationKey, publicName, args);
        }

        protected internal void RegisterCachedControllerCalledByControlEvent<controllerClass>(string triggeringControlTag) where controllerClass : class
        {
            RegisterCachedControllerCalledByControlEvent(triggeringControlTag, (CachedTask<controllerClass>)_cachedControllerManager.GetTheCachedController<controllerClass>(), () => true);
        }
        protected internal void RegisterCachedControllerCalledByControlEvent<controllerClass>(string triggeringControlTag,
           Func<bool> condition) where controllerClass : class
        {
            RegisterCachedControllerCalledByControlEvent(triggeringControlTag, (CachedTask<controllerClass>)_cachedControllerManager.GetTheCachedController<controllerClass>(), condition);
        }
        protected internal void RegisterCachedControllerCalledByControlEvent<controllerClass>(string triggeringControlTag,
            CachedTask<controllerClass> cachedTaskBase) where controllerClass : class
        {
            RegisterCachedControllerCalledByControlEvent(triggeringControlTag, cachedTaskBase, () => true);
        }

        protected internal void RegisterCachedControllerCalledByControlEvent<controllerClass>(string triggeringControlTag,
             CachedTask<controllerClass> cachedTaskBase, Func<bool> condition) where controllerClass : class
        {
            var h = Handlers.Add(Command.GoToNextControl, triggeringControlTag);
            h.RegisterCalledTask(cachedTaskBase, condition);
            h.BindEnabled(() => string.IsNullOrEmpty(_uiController.CurrentHandledKey) &&
                (!UserSettings.Version10Compatible || !IsBeforeFocusedControlInTabOrder(triggeringControlTag)));

            if (UserSettings.Version10Compatible)
            {
                var h1 = Handlers.Add(Command.GoToPreviousControl, triggeringControlTag);
                h1.RegisterCalledTask(cachedTaskBase, condition);
                h1.BindEnabled(() => string.IsNullOrEmpty(_uiController.CurrentHandledKey) &&
                    IsBeforeFocusedControlInTabOrder(triggeringControlTag));
            }
        }

        bool IsBeforeFocusedControlInTabOrder(string triggeringControlTag)
        {
            var form = _uiController.View as UI.Form;
            if (form == null) return false;
            var tc = form.FindControlByTag(triggeringControlTag);
            if (tc == null) return false;
            var fc = ((Firefly.Box.UI.Form)form).FocusedControl;
            if (fc == null) return false;
            var result = false;
            var stop = false;
            form.ForEachControlInTabOrder(
                c =>
                {
                    if (stop) return;

                    if (fc == c)
                        stop = true;
                    else if (c == tc)
                        result = true;
                });
            return result;
        }
        protected void RegisterCachedControllerCalledByPublicNameByControlEvent(string triggeringControlTag, Func<Text> publicName)
        {
            var c = _application.AllPrograms.GetCachedByPublicName(publicName, null);
            RegisterCachedControllerCalledByControlEvent(triggeringControlTag, c);
        }
        protected void RegisterCachedControllerCalledByPublicNameByControlEvent(string triggeringControlTag, Func<Text> publicName, Func<bool> condition)
        {
            var c = _application.AllPrograms.GetCachedByPublicName(publicName, null);
            RegisterCachedControllerCalledByControlEvent(triggeringControlTag, c, condition);
        }
        protected void RegisterCachedControllerCalledByIndexByControlEvent(string triggeringControlTag, Func<Number> programNumber)
        {
            var c = _application.AllPrograms.GetCachedByIndex(programNumber, null);
            RegisterCachedControllerCalledByControlEvent(triggeringControlTag, c);
        }
        protected void RegisterCachedControllerCalledByIndexByControlEvent(string triggeringControlTag, Func<Number> programNumber, Func<bool> condition)
        {
            var c = _application.AllPrograms.GetCachedByIndex(programNumber, null);
            RegisterCachedControllerCalledByControlEvent(triggeringControlTag, c, condition);
        }

        internal void YouAreSetToRunWithinSubForm()
        {
            _uiController.ReloadDataOnReEntry = From is ENV.Data.DynamicSQLEntity;
        }

        static DateTime _lastInput = DateTime.Now;

        static AbstractUIController()
        {
            FilterCollection.FilterToDebugString = (filter, e) =>
            {
                var f = FilterBase.GetIFilter(filter, false, e);
                return SQLFilterConsumer.DisplayFilterInSingleLine(f, true);
            };

            Form.UserInput += () => _lastInput = DateTime.Now;
        }

        Stopwatch _ensureResponsiveUIWhileSortingOrFindingRowsStopWatch;
        void EnsureResponsiveUIWhileSortingOrFindingRows(RowsProgressEventArgs e)
        {
            if (_ensureResponsiveUIWhileSortingOrFindingRowsStopWatch.ElapsedMilliseconds > 3000)
            {
                _ensureResponsiveUIWhileSortingOrFindingRowsStopWatch.Restart();
                if ((DateTime.Now - _lastInput).TotalMilliseconds > 3000)
                    Context.Current.Suspend(10, true);
            }
        }

        public static bool DisableClosingByClickOnParent;
        class SimpleProgress
        {
            ENV.UI.Form sortForm = null;
            System.Windows.Forms.Label l = null;
            Stopwatch sortStart = null;
            Stopwatch lastSortUpdate = null;
            string _title;
            int _interval;
            Func<Form> _view;
            public SimpleProgress(string title, Func<Form> view, int interval)
            {
                _interval = interval;
                _title = title;
                _view = view;
            }
            public void Progress(RowsProgressEventArgs e)
            {

                if (sortStart == null)
                {
                    sortStart = new Stopwatch();
                    sortStart.Start();
                }
                else
                {
                    if (sortForm == null)
                    {
                        if (!e.Done && sortStart.ElapsedMilliseconds > _interval)
                        {
                            lastSortUpdate = new Stopwatch();
                            lastSortUpdate.Start();
                            Common.ShowDialog(owner =>
                            {
                                sortForm = new UI.Form { Text = _title, Owner = owner };

                                var f = sortForm;

                                l = new System.Windows.Forms.Label();
                                f.Height = 100;
                                f.Width = 400;
                                l.AutoSize = false;
                                l.Dock = DockStyle.Fill;
                                l.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                                l.Font = new System.Drawing.Font(l.Font.Name, 12);

                                l.Top = (int)((sortForm.ClientSize.Height - l.Height) / 2);

                                f.Controls.Add(l);

                                var parent = _view();
                                if (parent == null)
                                    sortForm.StartPosition = WindowStartPosition.CenterParent;
                                else
                                {
                                    sortForm.StartPosition = WindowStartPosition.Custom;
                                    sortForm.Location = parent.PointToScreen(new System.Drawing.Point(((parent.Width - sortForm.Width) / 2), ((parent.Height - sortForm.Height) / 2)));
                                }
                                sortForm.Show();
                                l.Text = "Processed " + e.Rows.ToString("###,###,###");

                                return DialogResult.None;
                            });
                            ENV.UserMethods.Instance.Delay(0);
                            Firefly.Box.Context.Current.InvokeUICommand(() =>
                            {
                                if (sortForm != null)
                                    sortForm.Refresh();
                            });
                        }
                    }
                    else
                    {
                        if (e.Done)
                            Firefly.Box.Context.Current.InvokeUICommand(() =>
                            {
                                if (sortForm != null)
                                    sortForm.Hide();
                            });
                        else
                        {
                            if (lastSortUpdate.ElapsedMilliseconds > _interval)
                            {
                                lastSortUpdate.Reset();
                                lastSortUpdate.Start();
                                ENV.UserMethods.Instance.Delay(0);
                                Context.Current.InvokeUICommand(() =>
                                {
                                    if (sortForm != null)
                                    {
                                        l.Text = "Processed " + e.Rows.ToString("###,###,###");
                                        sortForm.Refresh();
                                    }
                                });
                            }
                        }

                    }
                }
                if (e.Done)
                {
                    sortForm = null;
                    sortStart = null;
                }
            }

        }
        internal protected void RunRevertableCodeFlow(Action<RevertableCodeBlock> what)
        {
            var r = new RevertableCodeBlock();
            what(r);
            r.Run();
        }

    }
    public class RevertableCodeBlock
    {

        void AddItem(ItemInCodeBlock item)
        {
            if (_blocks.Count == 0)
                _blockToRun.Add(item);
            else
                _blocks.Peek().CodeBlock.Add(item);
        }
        void AddBlock(Block b)
        {
            AddItem(b);
            _blocks.Push(b);

        }
        void AddBlockElse(Block e)
        {
            if (_blocks.Count == 0)
                throw new InvalidOperationException("Block else without Start Block");
            var b = _blocks.Pop();
            b.Else = e;
            _blocks.Push(e);


        }
        public void EndBlock()
        {
            if (_blocks.Count == 0)
                throw new InvalidOperationException("EndBlock without Start Block");
            _blocks.Pop();
        }
        Stack<Block> _blocks = new Stack<Block>();


        class CodeBlock
        {
            List<ItemInCodeBlock> _items = new List<ItemInCodeBlock>();
            public void Add(ItemInCodeBlock item)
            {
                _items.Add(item);
            }

            internal void Run(RunHelper rh)
            {
                int i = 0;
                if (rh.Revert)
                    i = _items.Count - 1;
                while (i >= 0 && i < _items.Count)
                {
                    _items[i].Run(rh);
                    if (rh.Revert)
                        i--;
                    else
                        i++;
                }
            }
        }

        CodeBlock _blockToRun = new CodeBlock();


        public void Add(Action what, Func<Bool> condition, FlowMode mode, Direction direction)
        {
            AddItem(new ItemInCodeBlock
            {
                What = what,
                Mode = mode,
                Direction = direction,
                Condition = condition
            });
        }
        public void Add(Action what, FlowMode mode)
        {
            AddItem(new ItemInCodeBlock
            {
                What = what,
                Mode = mode,
            });
        }
        public void Add(Action what, Func<Bool> condition, FlowMode mode)
        {
            AddItem(new ItemInCodeBlock
            {
                What = what,
                Mode = mode,
                Condition = condition
            });
        }
        public void Add(Action what, Func<Bool> condition)
        {
            AddItem(new ItemInCodeBlock
            {
                What = what,
                Condition = condition
            });
        }
        public void Add(Action what, Func<Bool> condition, Direction direction)
        {
            AddItem(new ItemInCodeBlock
            {
                What = what,
                Direction = direction,
                Condition = condition
            });
        }
        public void Add(Action what, FlowMode mode, Direction direction)
        {
            AddItem(new ItemInCodeBlock
            {
                What = what,
                Mode = mode,
                Direction = direction,
            });
        }
        public void Add(Action what, Direction direction)
        {
            AddItem(new ItemInCodeBlock
            {
                What = what,
                Direction = direction,
            });
        }
        public void Add(Action what)
        {
            AddItem(new ItemInCodeBlock
            {
                What = what,
            });
        }
        public void AddRevert(Action what, Func<Bool> condition, FlowMode mode, Direction direction)
        {
            AddItem(new RevertItemInCodeBlock
            {
                What = what,
                Mode = mode,
                Direction = direction,
                Condition = condition
            });
        }
        public void AddRevert(Action what, FlowMode mode)
        {
            AddItem(new RevertItemInCodeBlock
            {
                What = what,
                Mode = mode,
            });
        }
        public void AddRevert(Action what, Func<Bool> condition, FlowMode mode)
        {
            AddItem(new RevertItemInCodeBlock
            {
                What = what,
                Mode = mode,
                Condition = condition
            });
        }
        public void AddRevert(Action what, Func<Bool> condition)
        {
            AddItem(new RevertItemInCodeBlock
            {
                What = what,
                Condition = condition
            });
        }
        public void AddRevert(Action what, Func<Bool> condition, Direction direction)
        {
            AddItem(new RevertItemInCodeBlock
            {
                What = what,
                Direction = direction,
                Condition = condition
            });
        }
        public void AddRevert(Action what, FlowMode mode, Direction direction)
        {
            AddItem(new RevertItemInCodeBlock
            {
                What = what,
                Mode = mode,
                Direction = direction,
            });
        }
        public void AddRevert(Action what, Direction direction)
        {
            AddItem(new RevertItemInCodeBlock
            {
                What = what,
                Direction = direction,
            });
        }
        public void AddRevert(Action what)
        {
            AddItem(new RevertItemInCodeBlock
            {
                What = what,
            });
        }

        public void StartBlock(Func<Bool> condition, FlowMode mode, Direction direction)
        {
            AddBlock(new Block
            {
                Mode = mode,
                Direction = direction,
                Condition = condition
            });
        }
        public void StartBlock(FlowMode mode)
        {
            AddBlock(new Block
            {
                Mode = mode,
            });
        }
        public void StartBlock(Func<Bool> condition, FlowMode mode)
        {
            AddBlock(new Block
            {
                Mode = mode,
                Condition = condition
            });
        }
        public void StartBlock(Func<Bool> condition)
        {
            AddBlock(new Block
            {
                Condition = condition
            });
        }
        public void StartBlock(Func<Bool> condition, Direction direction)
        {
            AddBlock(new Block
            {
                Direction = direction,
                Condition = condition
            });
        }
        public void StartBlock(FlowMode mode, Direction direction)
        {
            AddBlock(new Block
            {
                Mode = mode,
                Direction = direction,
            });
        }
        public void StartBlock(Direction direction)
        {
            AddBlock(new Block
            {
                Direction = direction,
            });
        }
        public void StartBlock()
        {
            AddBlock(new Block
            {
            });
        }





        public void StartBlockElse(Func<Bool> condition, FlowMode mode, Direction direction)
        {
            AddBlockElse(new Block
            {
                Mode = mode,
                Direction = direction,
                Condition = condition
            });
        }
        public void StartBlockElse(FlowMode mode)
        {
            AddBlockElse(new Block
            {
                Mode = mode,
            });
        }
        public void StartBlockElse(Func<Bool> condition, FlowMode mode)
        {
            AddBlockElse(new Block
            {
                Mode = mode,
                Condition = condition
            });
        }
        public void StartBlockElse(Func<Bool> condition)
        {
            AddBlockElse(new Block
            {
                Condition = condition
            });
        }
        public void StartBlockElse(Func<Bool> condition, Direction direction)
        {
            AddBlockElse(new Block
            {
                Direction = direction,
                Condition = condition
            });
        }
        public void StartBlockElse(FlowMode mode, Direction direction)
        {
            AddBlockElse(new Block
            {
                Mode = mode,
                Direction = direction,
            });
        }
        public void StartBlockElse(Direction direction)
        {
            AddBlockElse(new Block
            {
                Direction = direction,
            });
        }
        public void StartBlockElse()
        {
            AddBlockElse(new Block
            {
            });
        }




        internal void Run()
        {
            var rh = new RunHelper();
            _blockToRun.Run(rh);
            if (rh.Revert && rh.Exception != null)
                throw rh.Exception;

        }
        class RunHelper
        {
            public bool Revert = false;

            public FlowAbortException Exception { get; internal set; }
        }
        class RevertItemInCodeBlock : ItemInCodeBlock
        {
            protected override void DoIt(RunHelper rh)
            {
                try
                {
                    rh.Revert = true;
                    base.DoIt(rh);
                }
                catch (FlowAbortException e)
                {
                    e.DoNotClearEvents();
                    Firefly.Box.Context.Current.DiscardPendingCommands();
                    rh.Exception = e;
                }
            }
        }
        class ItemInCodeBlock
        {
            public Action What;
            public FlowMode Mode = FlowMode.TabOrSkip;
            public Direction Direction = Direction.Any;
            public Func<Bool> Condition = () => true;



            internal void Run(RunHelper rh)
            {
                if (Calc(rh) && Condition())
                {
                    DoIt(rh);
                }
                else ConditionFailed(rh);
            }

            protected virtual void ConditionFailed(RunHelper rh)
            {

            }

            protected virtual void DoIt(RunHelper rh)
            {
                What();
            }

            bool Calc(RunHelper rh)
            {
                switch (Mode)
                {
                    case FlowMode.TabOrSkip:
                        var back = _flow("PR");
                        if (rh.Revert)
                            back = !back;
                        switch (Direction)
                        {
                            case Direction.Forward:
                                return !back;
                            case Direction.Backward:
                                return back;
                            case Direction.Any:
                            default:
                                return true;
                        }
                    case FlowMode.Tab:

                        switch (Direction)
                        {
                            case Direction.Forward:
                                return _flow("N") && !rh.Revert;
                            case Direction.Backward:
                                return _flow("P") || rh.Revert;
                            case Direction.Any:
                            default:
                                return _flow("NP");

                        }

                    case FlowMode.Skip:

                        switch (Direction)
                        {
                            case Direction.Forward:
                                return _flow("F") && !rh.Revert;
                            case Direction.Backward:
                                return _flow("R") || rh.Revert;
                            case Direction.Any:
                                return _flow("FRS");
                            default:
                                return true;
                        }
                    case FlowMode.ExpandBefore:
                    case FlowMode.ExpandAfter:
                    default:
                        return false;
                }
            }

            private bool _flow(string v)
            {
                return ENV.UserMethods.Instance.Flow(v);
            }
        }
        class Block : ItemInCodeBlock
        {
            public CodeBlock CodeBlock = new CodeBlock();
            public Block Else;
            protected override void DoIt(RunHelper rh)
            {
                CodeBlock.Run(rh);
            }
            protected override void ConditionFailed(RunHelper rh)
            {
                if (Else != null)
                    Else.Run(rh);
            }
        }
    }
}
