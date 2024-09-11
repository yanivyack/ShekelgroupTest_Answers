using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using ENV.Advanced;
using ENV.BackwardCompatible;
using ENV.Data.DataProvider;
using ENV.Printing;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;

using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;
using Form = Firefly.Box.UI.Form;
using Firefly.Box.Data;

namespace ENV
{
    internal interface IHaveUserMethods
    {
        UserMethods GetUserMethods();
    }
    [Firefly.Box.UI.Advanced.ContainsData]
    public class BusinessProcessBase : ENV.ControllerBase, IHaveUserMethods
    {
        internal BusinessProcess _businessProcess;
        public static event Action<BusinessProcess> StartOfInstance;
        internal static int DefaultUserInterfaceRefreshInterval = 1000;
        protected internal ENV.UserMethods u;
        UserMethods IHaveUserMethods.GetUserMethods()
        {
            return u;
        }
        bool _executionNotConfirmedKillController = false;
        internal bool _leaveRowHappened;
        public BusinessProcessBase()
        {

            BusinessProcess.AdvancedSettings advancedSettings = new BusinessProcess.AdvancedSettings();
            advancedSettings.SuppressValueBindingInInsertActivity = true;
            advancedSettings.UseChildOpenTransaction = true;
            advancedSettings.ConditionalTransaction = UserSettings.ConditionalTransaction;
            advancedSettings.AccumulateChangeEventOnReevaluation = true;
            advancedSettings.SuppressRowLockingInBrowseActivity = UserSettings.SuppressUpdatesInBrowseActivity;
            advancedSettings.ParentViewAccordingToHandlerContext = UserSettings.Version8Compatible;
            advancedSettings.ProcessExpressionCommandHandlersInOrderWithOtherHandlers = UserSettings.Version8Compatible;
            advancedSettings.ForceResponsiveUserInterface = UserSettings.Version8Compatible || UserSettings.VersionXpaCompatible;

            if (UserSettings.VersionXpaCompatible)
                advancedSettings.UserInterfaceUpdateInterval = 100;

            if (UserSettings.Version10Compatible && !UserSettings.VersionXpaCompatible)
            {
                OnRollbackWithoutTransactionAbortTask = true;
            }

            if (UserSettings.Version10Compatible && !UserSettings.VersionXpaCompatible)
            {
                advancedSettings.BindValuesInActiveControllerContext = true;
            }

            _businessProcess = new BusinessProcess(advancedSettings);
            ControllerStart(this);
            _levelProvider = new LevelProvider(false, _businessProcess, this);
            _groups = new GroupCollectionWrapper(_businessProcess.Groups, _levelProvider);
            _businessProcess.UserInterfaceRefreshInterval = DefaultUserInterfaceRefreshInterval;
            _businessProcess.EnterRow += delegate
                                             {
                                                 _callMeOnEnterRow();
                                                 using (_levelProvider.EnterRow())
                                                 {
                                                     OnEnterRow();
                                                     if (EnterRow != null)
                                                         EnterRow();
                                                 }
                                             };
            _businessProcess.LeaveRow += delegate
                                             {
                                                 using (_levelProvider.LeaveRow())
                                                 {
                                                     OnLeaveRow();
                                                     if (LeaveRow != null)
                                                         LeaveRow();
                                                 }
                                                 _leaveRowHappened = true;
                                             };
            _businessProcess.Start += delegate
                                      {
                                          if (_executionNotConfirmedKillController)
                                              return;
                                          InitNullToColumns(true, Entities, u, _businessProcess.View);
                                          _beforeControllerStart();
                                          using (_levelProvider.Start())
                                          {
                                              if (StartOfInstance != null)
                                                  StartOfInstance(_businessProcess);
                                              OnStart(); if (Start != null)
                                                  Start();
                                          }
                                      };
            _businessProcess.End += delegate
                                        {
                                            if (_executionNotConfirmedKillController)
                                                return;
                                            using (_levelProvider.End())
                                            {
                                                OnEnd();
                                                if (End != null)
                                                    End();
                                                foreach (var io in Streams)
                                                {

                                                    var pw = io as PrinterWriter;
                                                    if (pw != null)
                                                        pw.BusinessProcessIsEnding();
                                                }
                                                _disposeStreams();
                                            }

                                            if (ConfirmExecution)
                                            {

                                                Common.ShowMessageBox(LocalizationInfo.Current.ExecutionCompleted,
                                                                      MessageBoxIcon.Information,
                                                                      LocalizationInfo.Current.ExecutionCompleted);
                                            }

                                        };
            _businessProcess.Load += () =>
                                     {
                                         InitNullToColumns(false, Entities, u, _businessProcess.View);
                                         using (ENV.Utilities.Profiler.Level(this, "OnLoad"))
                                         {
                                             OnLoad();
                                             if (_executionNotConfirmedKillController)
                                                 _executionNotConfirmedKillController = false;
                                             if (ConfirmExecution)
                                                 if (AllowFilterAndSortInConfirmExecution)
                                                 {
                                                     ConfirmExecutionWithFilter();
                                                 }

                                             if (Activity == Activities.Browse && (UserSettings.SuppressUpdatesInBrowseActivity || !AllowUpdate))
                                             {
                                                 RowLocking = LockingStrategy.None;
                                             }
                                             if (!UserSettings.DoNotDisplayUI)
                                                 switch (Activity)
                                                 {
                                                     case Firefly.Box.Activities.Update:
                                                         if (!AllowUpdate && !ConfirmExecution)
                                                             throw new IllegalActivityException(Activity);
                                                         break;
                                                     case Firefly.Box.Activities.Insert:
                                                         if (!AllowInsert)
                                                             throw new IllegalActivityException(Activity);
                                                         break;
                                                     case Firefly.Box.Activities.Delete:
                                                         if (!AllowDelete)
                                                             throw new IllegalActivityException(Activity);
                                                         break;
                                                 }
                                             if (Activity == Activities.Browse && !UserSettings.SuppressUpdatesInBrowseActivity && AllowUpdate)
                                             {
                                                 Activity = Activities.Update;
                                             }
                                             _businessProcess.ReloadRowBeforeGroupEnter = Activity == Activities.Update && RowLocking != LockingStrategy.OnRowLoading;
                                             if (Load != null)
                                                 Load();
                                             _bindView(_businessProcess.View);
                                             ApplyActivityToColumns(Activity);
                                             _AfterOnLoad();
                                         }

                                     };
            _businessProcess.SavingRow += _businessProcess_SavingRow;
            _businessProcess.DatabaseErrorOccurred += e => Common.DatabaseErrorOccurred(e, Relations, this.RowLocking);
            _businessProcess.PreviewDatabaseError += error =>
                                                         {
                                                             Common.PreviewDatabaseError(error);
                                                             if (error.ErrorType == Firefly.Box.Data.DataProvider.DatabaseErrorType.DuplicateIndex
                                                                && !OnRollbackWithoutTransactionAbortTask)
                                                             {
                                                                 var ent = error.Entity as ENV.Data.Entity;
                                                                 if (ent != null)
                                                                 {
                                                                     if (!ent.DataProvider.SupportsTransactions)
                                                                     {
                                                                         var x = Firefly.Box.Context.Current.ActiveTasks;
                                                                         if (x.Count > 0 &&
                                                                             !x[x.Count - 1].InTransaction)
                                                                         {
                                                                             error.HandlingStrategy =
                                                                                 Firefly.Box.Data.DataProvider.
                                                                                     DatabaseErrorHandlingStrategy
                                                                                     .Ignore;
                                                                             ENV.Common.ShowDatabaseErrorMessage = !(error.Entity is BtrieveEntity);
                                                                         }
                                                                     }
                                                                 }
                                                             }
                                                         };
            _businessProcess.AbortRowOccurred +=
                args =>
                {
                    if (InTransaction) return;
                    args.ContinueProcessing = true;
                    if (_exit || (_exitCondition != null && _exitCondition()))
                    {
                        return;
                    }
                    var key = Keys.None;
                    while (key == Keys.None && !Context.Current.CommandsPending)
                        key = Context.Current.Suspend(1000);
                    if (key != Keys.None && key != Keys.Escape)
                        Context.Current.DiscardPendingCommands();
                    else
                        Exit();
                };
            _handlers = new HandlerCollectionWrapper(_businessProcess.Handlers, _levelProvider, () => UserSettings.Version8Compatible,
                () => UserSettings.Version8Compatible, _cachedControllerManager, () => _application.AllPrograms, x => _businessProcess.Invoke(x));
            u = new UserMethods(this);
            OnDatabaseErrorRetry = false;

        }
        internal override void _MarkRowAsChanged()
        {
            _businessProcess.MarkRowAsChanged();
        }
        FilterForm _confirmExecutionFilterForm;
        FilterCollection _confirmExecutionFilter;
        private void ConfirmExecutionWithFilter()
        {
            var message = new UI.BackwardCompatibleMessageBox(LocalizationInfo.Current.ConfirmExecution, LocalizationInfo.Current.ConfirmExecution, true);

            var ent = new ENV.Data.Entity("xx", new MemoryDatabase());
            var nc = new NumberColumn();
            ent.Columns.Add(nc);
            ent.Insert(() => nc.Value = 1);
            message.From = ent;
            message.Handlers.Add(Commands.FilterRows).Invokes += e =>
            {
                if (_confirmExecutionFilterForm == null)
                {
                    _confirmExecutionFilterForm = new FilterForm(Title, addColumn =>
                    {
                        _businessProcess.View.ForEachControlInTabOrder(
                       delegate (System.Windows.Forms.Control control)
                       {
                           var icb = control as InputControlBase;

                           if (icb != null)
                           {
                               if (!(icb is Firefly.Box.UI.Button))
                               {
                                   var c = icb.GetColumn();
                                   if (c != null)
                                   {
                                       addColumn(c, icb);
                                   }
                               }
                           }
                       });
                    }, this.u);

                }
                if (_confirmExecutionFilterForm.Run(null))
                {
                    if (_confirmExecutionFilter == null)
                    {
                        _confirmExecutionFilter = new FilterCollection();
                        _businessProcess.Where.Add(_confirmExecutionFilter);
                    }
                    _confirmExecutionFilter.Clear();
                    _confirmExecutionFilter.Add(_confirmExecutionFilterForm.Filter);


                }
                e.Handled = true;
            };
            message.Handlers.Add(Commands.SelectOrderBy).Invokes += e =>
            {
                ENV.UI.SelectOrderByDialog s = null;
                Firefly.Box.Context.Current.InvokeUICommand(() => s = new ENV.UI.SelectOrderByDialog());
                s.ShowDialog(message._uiController.View, new MyBridgeForSelectOrderbyParent(this));
                e.Handled = true;
            };
            message.Handlers.Add(Commands.CustomOrderBy).Invokes += e =>
            {
                ENV.UI.CustomOrderByDialog s = null;
                Firefly.Box.Context.Current.InvokeUICommand(() => s = new ENV.UI.CustomOrderByDialog());
                s.ShowDialog(message._uiController.View, new MyBridgeForSelectOrderbyParent(this));
                e.Handled = true;
                e.Handled = true;
            };
            if (!message.Show())
            {
                _executionNotConfirmedKillController = true;
                Exit();

            }
        }
        class MyBridgeForSelectOrderbyParent : ENV.UI.SelectOrderByDialog.SelectOrderByParent
        {
            BusinessProcessBase _parent;


            public MyBridgeForSelectOrderbyParent(BusinessProcessBase businessProcessBase)
            {
                _parent = businessProcessBase;
            }

            public Entity From { get { return _parent.From; } }

            public Sort OrderBy { get { return _parent.OrderBy; } set { _parent.OrderBy = value; } }

            public void SaveRowAndDoAndCallBackOnOk(Action<Action<bool>> whatToDo)
            {
                whatToDo((x) => { });
            }
            public ColumnCollection Columns { get { return _parent.Columns; } }

            public string Title { get { return _parent.Title; } }
        }

        internal override Activities GetActivity()
        {
            return Activity;
        }
        internal override RelationCollection GetRelations()
        {
            return Relations;
        }
        List<Action> _onViewDispose = new List<Action>();
        internal void AddToViewDispose(Action what)
        {
            _onViewDispose.Add(what);
        }

        protected internal void AddPrimaryKeyToOrderBy()
        {
            if (OrderBy.Segments.Count == 0)
            {
                foreach (var item in From.Indexes)
                {
                    if (item.Unique)
                    {
                        OrderBy = item;
                        return;
                    }
                }
            }
            else
                AddPrimaryKeyToOrderBy(OrderBy, From);
        }
        protected int UserInterfaceRefreshInterval
        {
            set { _businessProcess.UserInterfaceRefreshInterval = value; }
        }

        internal override ITask GetITask()
        {
            return _businessProcess;
        }

        internal event Action Start, LeaveRow, End, EnterRow, Load;

        void _businessProcess_SavingRow(SavingRowEventArgs e)
        {

            if (Activity == Firefly.Box.Activities.Browse)
            {
                e.Cancel = true;
                if (AnyColumnWithEntityChanged || _AnyColumnOfRelationSetToBoundValue(Relations))
                {
                    Context.Current.DiscardPendingCommands();
                    Common.WarningInStatusBar(LocalizationInfo.Current.UpdateNotAllowedInBrowseMode);
                }
            }
            else
            {
                using (_levelProvider.SavingRow())
                {
                    OnSavingRow();
                }
            }

        }
        protected virtual void OnLeaveRow()
        {
            ENV.Utilities.Profiler.DontWriteIfEmpty();
        }
        protected virtual void OnSavingRow()
        {
            ENV.Utilities.Profiler.DontWriteIfEmpty();
        }
        protected virtual void OnEnd() { ENV.Utilities.Profiler.DontWriteIfEmpty(); }

        protected virtual void OnStart()
        {
            ENV.Utilities.Profiler.DontWriteIfEmpty();
        }
        protected virtual void OnEnterRow() { ENV.Utilities.Profiler.DontWriteIfEmpty(); }
        protected virtual void OnLoad() { ENV.Utilities.Profiler.DontWriteIfEmpty(); }


        Advanced.GroupCollectionWrapper _groups;
        internal protected Advanced.GroupCollectionWrapper Groups
        {
            get { return _groups; }
        }


        Action<Form> _bindView = delegate { };
        protected override sealed void BindView(Form form)
        {
            if (form != null) form.EvaluateFormCreationBindings();
            _bindView = delegate (Form taskForm)
            {
                Common.BindForms(taskForm, form);
                _bindView = delegate { };
            };
        }


        protected internal void AddAllColumns()
        {
            _businessProcess.AddAllColumns();
        }
        internal protected bool RowChanged { get { return _businessProcess.RowChanged; } }

        internal override bool GetRowChanged()
        {
            return RowChanged;
        }

        protected event Action AfterSavingRow { add { _businessProcess.AfterSavingRow += value; } remove { _businessProcess.AfterSavingRow -= value; } }

        protected int UserInterfaceRefreshRowsInterval { get { return _businessProcess.UserInterfaceRefreshRowsInterval; } set { _businessProcess.UserInterfaceRefreshRowsInterval = value; } }

        protected void Raise(Command command, params object[] untypedArgs)
        {
            ControllerBase.RaiseHappened(command);
            _businessProcess.Raise(command, untypedArgs);
        }

        protected void Raise(Keys keys, params object[] untypedArgs)
        {
            ControllerBase.RaiseHappened(keys);
            _businessProcess.Raise(keys, untypedArgs);
        }

        protected void Raise(string customCommandKey, params object[] untypedArgs)
        {
            ControllerBase.RaiseHappened(customCommandKey);
            _businessProcess.Raise(HandlerCollectionWrapper.FixCustomCommandKey(customCommandKey), untypedArgs);
        }


        internal protected void Invoke(string customCommandKey, params object[] untypedArgs)
        {
            _businessProcess.Invoke(HandlerCollectionWrapper.FixCustomCommandKey(customCommandKey), untypedArgs);
        }



        internal protected void Invoke(Command command, params object[] untypedArgs)
        {
            _businessProcess.Invoke(command, untypedArgs);
        }
        internal protected void Invoke(CommandWithArgs command)
        {

            _businessProcess.Invoke(command);
        }
        internal protected void Raise(CommandWithArgs command)
        {

            _businessProcess.Raise(command);
        }


        protected void Invoke(Keys keys, params object[] untypedArgs)
        {
            _businessProcess.Invoke(keys, untypedArgs);
        }
        protected void Raise<T>(Command command, ArrayColumn<T> untypedArgs)
        {
            ControllerBase.RaiseHappened(command);
            _businessProcess.Raise(command, untypedArgs);
        }

        protected void Raise<T>(Keys keys, ArrayColumn<T> untypedArgs)
        {
            ControllerBase.RaiseHappened(keys);
            _businessProcess.Raise(keys, untypedArgs);
        }

        protected void Raise<T>(string customCommandKey, ArrayColumn<T> untypedArgs)
        {
            ControllerBase.RaiseHappened(customCommandKey);
            _businessProcess.Raise(HandlerCollectionWrapper.FixCustomCommandKey(customCommandKey), untypedArgs);
        }


        protected void Invoke<T>(string customCommandKey, ArrayColumn<T> untypedArgs)
        {
            _businessProcess.Invoke(HandlerCollectionWrapper.FixCustomCommandKey(customCommandKey), untypedArgs);
        }



        internal protected void Invoke<T>(Command command, ArrayColumn<T> untypedArgs)
        {
            _businessProcess.Invoke(command, untypedArgs);
        }

        protected void Invoke<T>(Keys keys, ArrayColumn<T> untypedArgs)
        {
            _businessProcess.Invoke(keys, untypedArgs);
        }
        protected internal T Invoke<T>(Func<T> func)
        {
            T r = default(T);
            _businessProcess.Invoke(() => r = func());
            return r;
        }

        internal protected Advanced.HandlerCollectionWrapper Handlers
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
        protected Number DeltaOf(Func<Number> expression)
        {
            return _businessProcess.DeltaOf(expression);
        }
        protected Time DeltaOf(Func<Time> expression)
        {
            return u.ToTime(_businessProcess.DeltaOf(() => u.ToNumber(expression())));
        }
        protected Number DeltaOf(Func<Date> expression)
        {
            return _businessProcess.DeltaOf(() => u.ToNumber(expression()));
        }



        protected virtual internal LockingStrategy RowLocking
        {
            get { return _businessProcess.RowLocking; }
            set { _businessProcess.RowLocking = value; }
        }

        internal static long AdjustCounter(long result)
        {

            if (result <= _maxCounter)
                return result;
            {
                return (result - _minCounter) % (_maxCounter - _minCounter) + _minCounter;
            }
        }

        const long _maxCounter = int.MaxValue, _minCounter = int.MinValue;
        internal protected Number Counter
        {
            get
            {
                return AdjustCounter(_businessProcess.Counter);
            }
        }
        internal protected TransactionType TransactionType
        {
            get { return _businessProcess.TransactionType; }
            set { _businessProcess.TransactionType = value; }
        }
        protected internal TransactionScopes TransactionScope
        {
            get { return _businessProcess.TransactionScope; }
            set { _businessProcess.TransactionScope = value; }
        }

        GroupCollectionWrapper.WrappedGroup _transactionScopeGroup;
        protected internal GroupCollectionWrapper.WrappedGroup TransactionScopeGroup
        {
            get { return _transactionScopeGroup; }
            set
            {
                _transactionScopeGroup = value;
                _businessProcess.TransactionScopeGroup = value.Group;
            }
        }



        protected internal Func<Form> View
        {
            set
            {
                if (Common._suppressDialogForTesting)
                    return;

                SetView(value, v => _businessProcess.View = v, f => { }, false);
            }
        }

        internal protected string Title
        {
            get { return _businessProcess.Title; }
            set { _businessProcess.Title = value; }
        }

        internal override sealed protected Firefly.Box.Data.Entity From
        {
            set { _businessProcess.From = value; }
            get { return _businessProcess.From; }
        }

        protected Sort OrderBy
        {
            set { _businessProcess.OrderBy = value; }
            get { return _businessProcess.OrderBy; }
        }



        internal protected Activities Activity
        {
            get { return _businessProcess.Activity; }
            set { _businessProcess.Activity = value; }
        }



        protected internal FilterCollection Where
        {
            get { return _businessProcess.Where; }
        }

        /// <summary>
        /// This Where will always be evaluated in memory and may have performance implications
        /// </summary>
        protected FilterCollection NonDbWhere
        {
            get { return _businessProcess.NonDbWhere; }
        }
        bool _allowDelete = true;
        protected internal bool AllowDelete
        {
            set { _allowDelete = value; }
            get { return _allowDelete; }
        }
        bool _allowInsert = true;
        protected bool AllowInsert
        {
            set { _allowInsert = value; }
            get { return _allowInsert; }
        }
        bool _allowUpdate = true;
        internal protected bool AllowUpdate
        {
            set { _allowUpdate = value; }
            get { return _allowUpdate; }
        }
        protected internal void DeleteRowAfterLeavingItIf(Func<bool> condition)
        {
            _businessProcess.DeleteRowAfterLeavingItIf(() => (!UserSettings.SuppressUpdatesInBrowseActivity || Activity != Activities.Browse) && condition());
        }

        protected bool ShowView
        {
            set { _businessProcess.ShowView = value; }
            get { return _businessProcess.ShowView; }
        }

        internal protected bool KeepViewVisibleAfterExit
        {
            set { _businessProcess.KeepViewVisibleAfterExit = value; }
            get { return _businessProcess.KeepViewVisibleAfterExit; }
        }
        protected void BindKeepViewVisibleAfterExit(Func<bool> condition)
        {
            _businessProcess.BindKeepViewVisibleAfterExit(condition);
        }
        protected bool AllowUserAbort
        {
            set { _businessProcess.AllowUserAbort = value; }
            get { return _businessProcess.AllowUserAbort; }
        }


        Func<bool> _confirmExecution = () => false;

        protected bool ConfirmExecution { get { return !ENV.UserSettings.DoNotDisplayUI && _confirmExecution(); } set { _confirmExecution = () => value; } }
        protected void BindConfirmExecution(Func<bool> condition)
        {
            _confirmExecution = condition;
        }
        public static bool AllowFilterAndSortInConfirmExecution = false;
        internal override void RunTheTask()
        {
            bool runTask = true;

            if (ConfirmExecution && !AllowFilterAndSortInConfirmExecution && !ENV.Common._suppressDialogForTesting)
                runTask = Common.ShowYesNoMessageBox(LocalizationInfo.Current.ConfirmExecution,
                                                     LocalizationInfo.Current.ConfirmExecution, true);
            if (runTask)
            {
                _businessProcess.Run();

            }
        }

        internal protected sealed override ColumnCollection Columns
        {
            get { return _businessProcess.Columns; }
        }
        internal protected EntityCollection Entities { get { return _businessProcess.Entities; } }

        protected void LockCurrentRow()
        {
            if (RowLocking != LockingStrategy.None)
                _businessProcess.LockCurrentRow();
        }
        protected internal RelationCollection Relations
        {
            get { return _businessProcess.Relations; }
        }

        protected internal bool InTransaction
        {
            get { return _businessProcess.InTransaction; }
        }
        Func<bool> _asSoonAsPossibleExitCondition = () => false;
        bool _exit;
        Func<bool> _exitCondition;
        internal protected override sealed void Exit(ExitTiming timing, Func<bool> condition)
        {
            if (timing == ExitTiming.AsSoonAsPossible)
                _asSoonAsPossibleExitCondition = condition;
            else
                _asSoonAsPossibleExitCondition = () => false;
            _exitCondition = condition;
            _businessProcess.Exit(timing, condition);
        }
        internal protected override sealed void CheckExit()
        {
            if (_asSoonAsPossibleExitCondition())
            {
                _asSoonAsPossibleExitCondition = () => true;
                Exit();
            }
        }
        internal protected void Exit(ExitTiming timing)
        {
            _exit = true;
            _businessProcess.Exit(timing);
        }
        internal protected void Exit()
        {
            _exit = true;
            _businessProcess.Exit();
        }

        internal protected void setApplication(ApplicationControllerBase application)
        {
            if (application == null)
                return;
            _application = application;
            u.SetApplication(application);
            _businessProcess.Module = _application._moduleController;
            SetNullStrategy(application._nullStrategy);
        }

        internal void SetNullStrategy(INullStrategy nullStrategy)
        {
            _myNullStrategy = nullStrategy;
        }

        internal static Dictionary<BusinessProcess, BusinessProcessBase> _actionBusinessProcess =
            new Dictionary<BusinessProcess, BusinessProcessBase>();

        internal bool _inProcess;
        internal long _numOfExecutions = 0;
        protected internal sealed override void Execute()
        {
            _leaveRowHappened = false;
            _numOfExecutions++;
            if (_inSubformReload)
                return;
            try
            {
                _inProcess = true;
                lock (_actionBusinessProcess)
                {
                    _actionBusinessProcess.Add(_businessProcess, this);
                }
                base.Execute();
            }
            finally
            {
                lock (_actionBusinessProcess)
                {
                    _actionBusinessProcess.Remove(_businessProcess);
                }
                _inProcess = false;
                if (_businessProcess.View != null && _businessProcess.View.IsDisposed)
                {
                    _cachedControllerManager.Clear();
                    var x = _onViewDispose;
                    _onViewDispose = new List<Action>();
                    foreach (var item in x)
                    {
                        item();
                    }
                }
                u.DoEndOfTaskCleanup();
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
        internal protected bool ForFirstRow(Action action)
        {
            Exit(ExitTiming.AfterRow, () => true);
            LeaveRow += action;
            Execute();
            LeaveRow -= action;
            return Counter > 0;
        }

        internal protected long ForEachRow(Action action)
        {
            LeaveRow += action;
            Execute();
            LeaveRow -= action;
            return Counter;
        }

        protected bool AutoCompute
        {
            get { return _businessProcess.AutoCompute; }
            set { _businessProcess.AutoCompute = value; }
        }

        internal bool OnRollbackWithoutTransactionAbortTask { get; set; }

        protected void PerformCompute()
        {
            _businessProcess.PerformCompute();
        }

        internal override void ProvideSubForms(Action<SubForm> runForEachSubForm)
        {
            if (_businessProcess.View == null) return;
            foreach (Control control in _businessProcess.View.Controls)
            {
                var sf = control as SubForm;
                if (sf != null)
                    runForEachSubForm(sf);
            }
        }
        protected void ReadAllRows(Action runForEachRow)
        {
            _businessProcess.ReadAllRows(runForEachRow);
        }
    }

}