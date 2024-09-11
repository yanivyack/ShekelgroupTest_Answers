using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ENV.Advanced;
using ENV.BackwardCompatible;
using ENV.Data;
using ENV.Data.DataProvider;
using ENV.Printing;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;
using BoolColumn = ENV.Data.BoolColumn;
using Entity = Firefly.Box.Data.Entity;
using Form = Firefly.Box.UI.Form;

namespace ENV
{
    public abstract class ControllerBase
    {
        protected internal readonly UserDbMethods db;
        static long _lastId;
        internal readonly long _instanceId;
        public ControllerBase()
        {
            db = new UserDbMethods(() => From);
            _cachedControllerManager = new CachedControllerManager(this);
            MemoryTracker.Track(this);
            _instanceId = Interlocked.Increment(ref _lastId);
        }
        public static event Action<object, Command> OnProcessingCommand;
        public static event Action<object> OnRaise;
        internal static void RaiseHappened(object what)
        {
            if (OnRaise != null)
                OnRaise(what);
        }

        ColumnBase[] _dataViewVarsColumns;
        protected internal void SetDataViewVarsColumns(params ColumnBase[] columns)
        {
            _dataViewVarsColumns = columns;
        }
        internal virtual void _provideColumnsForFilter(FilterForm.AddColumnToFilterForm addColumn)
        {
            if (_dataViewVarsColumns != null)
            {
                foreach (var item in _dataViewVarsColumns)
                {
                    addColumn(item, new ENV.UI.TextBox { Data = item });
                }

            }
            var t = GetITask();
            if (t != null && t.View != null)
                t.View.ForEachControlInTabOrder(
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


        }
        internal static void ControllerStart(ApplicationControllerBase c)
        {
            if (OnProcessingCommand != null)
                c._moduleController.ProcessingCommand += com => DoOnProcessingCommand(c, com);
        }
        static void DoOnProcessingCommand(object c, Command com)
        {
            OnProcessingCommand(c, com);
        }
        internal void ControllerStart(AbstractUIController c)
        {
            if (OnProcessingCommand != null)
                c._uiController.ProcessingCommand += com => DoOnProcessingCommand(c, com);
        }
        internal void ControllerStart(BusinessProcessBase c)
        {
            if (OnProcessingCommand != null)
                c._businessProcess.ProcessingCommand += com => DoOnProcessingCommand(c, com);
        }

        protected internal T Cached<T>() where T : class
        {
            return _cachedControllerManager.GetCachedController<T>();
        }
        protected static T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }

        internal interface ParameterColumn
        {
            bool _fireOnChangeEventIfEqual { get; set; }
            UserMethods _getUserMethods();
        }

        List<ColumnBase> _additionalColumns = new List<ColumnBase>();
        bool _initAdditinalColumnsChangeMonitoring = false;
        internal protected List<ColumnBase> AdditionalColumns { get { return _additionalColumns; } }
        internal CachedControllerManager _cachedControllerManager;
        internal void DoBindParameter<T>(TypedColumnBase<T> column, ParameterBase<T> parameter)
        {
            if (parameter == null)
                return;
            var pc = column as ParameterColumn;
            if (pc != null)
            {
                pc._fireOnChangeEventIfEqual = true;
            }
            if (!_inSubformReload)
            {
                if (column.Entity == null || column.Entity is DynamicSQLEntity)
                {

                    NullStrategy.ApplyToUserInstance(_myNullStrategy, column);
                    _boundParameters.Add(new parameterBinding<T>(column, parameter, this));
                }
            }
            else
            {
                var x = column.OnChangeMarkRowAsChanged;
                try
                {
                    column.OnChangeMarkRowAsChanged = false;
                    if (parameter == null)
                        column.Value = default(T);
                    else
                        column.Value = parameter.GetValue(column);
                }
                finally
                {
                    column.OnChangeMarkRowAsChanged = x;
                }
            }

        }

        string _icon;
        string _prevIcon;
        bool _iconWasSet = false;
        internal protected bool OnDatabaseErrorRetry { get; set; }
        protected string Icon
        {
            get { return _icon; }
            set
            {

                _icon = value;
                if (!ENV.UserSettings.DoNotDisplayUI)
                {
                    _prevIcon = DefaultIcon.Value;
                    DefaultIcon.Value = value;
                    _iconWasSet = true;
                }
            }
        }
        internal static ContextStatic<string> DefaultIcon = new ContextStatic<string>(() => "");
        internal static void ApplyIcon(System.Windows.Forms.Form form)
        {
            System.Drawing.Icon x = null;
            try
            {
                var path = PathDecoder.DecodePath(DefaultIcon.Value);
                if (!string.IsNullOrEmpty(path))
                    x = ImageLoader.LoadIcon(path);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex);
            }
            if (x != null)
            {
                Firefly.Box.Context.Current.InvokeUICommand(() =>
                    form.Icon = x);
            }
        }
        internal CrossTaskCache _crossTaskCache = new CrossTaskCache();
        protected internal bool KeepChildRelationCacheAlive { get; set; }
        internal interface ABoundParameter : IDisposable
        {
            void SetBoundColumnAcordingToParameterValue();
            void SetReferencedColumnAcordingToBoundColumn();
            void ProvideNameAndValue(Action<string, string> to);
            bool SetDynamicSQLSuccessColumnValueForAndReturnTrueIfYouHaveDoneIt(ColumnBase c, Bool val);
            void DenyUndoFor(ColumnBase col, UserMethods u);
        }
        protected internal string TaskID { get; set; }
        internal protected static void AddPrimaryKeyToOrderByOf(Relation r)
        {
            var orderBy = r.OrderBy;
            var entity = r.From;
            AddPrimaryKeyToOrderBy(orderBy, entity);
        }

        internal static void AddPrimaryKeyToOrderBy(Sort orderBy, Entity entity)
        {
            if (orderBy.Unique)
                return;
            var idx = orderBy as Index;

            foreach (var primaryKeyColumn in entity.PrimaryKeyColumns)
            {
                if (!orderBy.Segments.Contains(primaryKeyColumn))
                {
                    orderBy.Segments.Add(primaryKeyColumn);
                    if (idx != null)
                        idx.ColumnsAddedToAutomaticallyMakeSortUnique.Add(primaryKeyColumn);
                }
            }
            orderBy.Unique = true;
        }



        internal Action _beforeControllerStart = delegate { };

        internal abstract ITask GetITask();

        public static ITask GetTask(ControllerBase controller)
        {
            return controller.GetITask();
        }

        /// <summary>
        /// Used only to support the UserMethods.VarName method. By Default the VarName would return Virtual.Caption, but for columns that were sent as paramters to the MarkParameterColumn the VarName method would return Parameter.Caption
        /// </summary>
        /// <param name="columns"></param>
        internal protected void MarkParameterColumns(params ColumnBase[] columns)
        {
            foreach (var item in columns)
            {
                var z = item as IENVColumn;
                if (z != null)
                    z.IsParameter = true;
            }
        }
        class parameterBinding<T> : ABoundParameter
        {
            TypedColumnBase<T> _column;
            ColumnBase _columnBase;
            ParameterBase<T> _parameter;
            bool _changed = false;
            ControllerBase _parent;
            public parameterBinding(TypedColumnBase<T> column, ParameterBase<T> _parameter, ControllerBase parent)
            {
                _parent = parent;
                _column = column;
                _columnBase = column;
                var cc = column as IENVColumn;

                this._parameter = _parameter;
            }

            public void SetBoundColumnAcordingToParameterValue()
            {
                Common.DoWhileColumnChangeIsSuppressed(() =>
                {
                    var v = _parameter == null ? default(T) : _parameter.GetValue(_column);


                    if (_parameter != null)
                        _parameter.SetValueToBoundColumn(v, _column);

                    else
                        _column.Value = v;
                });
                _columnBase.ValueChanged += _column_Change;

            }

            void _column_Change()
            {
                if (!_parent._inSubformReload)
                    _changed = true;
            }

            public void SetReferencedColumnAcordingToBoundColumn()
            {
                if (_changed && _parameter != null)
                    _parameter.SetReturnValue(_column, _column.Value);
            }

            public void ProvideNameAndValue(Action<string, string> to)
            {
                var x = _parameter.Value;

                to(_column.Caption ?? "", x == null ? "null" : x.ToString());
            }


            public void Dispose()
            {
                _columnBase.ValueChanged -= _column_Change;
            }

            public bool SetDynamicSQLSuccessColumnValueForAndReturnTrueIfYouHaveDoneIt(ColumnBase c, Bool val)
            {
                if (_parameter == null)
                    return false;
                if (!ReferenceEquals(_column, c))
                    return false;
                _parameter.SetReturnValue(c, (T)(object)val);
                return true;
            }

            public void DenyUndoFor(ColumnBase col, UserMethods u)
            {
                if (_columnBase == col)
                {
                    _parameter.DenyUndoForSentColumnSentAsParameter(u);
                }
            }
        }

        internal List<ABoundParameter> _boundParameters = new List<ABoundParameter>();

        internal protected void BindParameter(Firefly.Box.Data.NumberColumn column, NumberParameter parameter)
        {
            DoBindParameter(column, parameter);
        }

        internal protected void BindParameter(Firefly.Box.Data.DateColumn column, DateParameter parameter)
        {
            DoBindParameter(column, parameter);
        }

        internal protected void BindParameter(Firefly.Box.Data.TimeColumn column, TimeParameter parameter)
        {
            DoBindParameter(column, parameter);
        }

        internal protected void BindParameter(Firefly.Box.Data.TextColumn column, TextParameter parameter)
        {
            DoBindParameter(column, parameter);
        }
        internal protected void BindParameter<dataType>(Firefly.Box.Data.ArrayColumn<dataType> column, ArrayParameter<dataType> parameter)
        {
            DoBindParameter(column, parameter);
        }

        internal protected void BindParameter(Firefly.Box.Data.BoolColumn column, BoolParameter parameter)
        {
            DoBindParameter(column, parameter);
        }

        internal protected void BindParameter(Firefly.Box.Data.ByteArrayColumn column, ByteArrayParameter parameter)
        {
            DoBindParameter(column, parameter);
        }
        internal protected void BindParameter<T>(Firefly.Box.Interop.ActiveXColumn<T> column, ActiveXParameter<T> parameter)
            where T : System.Windows.Forms.AxHost, new()
        {
            DoBindParameter(column, parameter);
        }
        protected void BindParameter<T>(Firefly.Box.Interop.ComColumn<T> column, ComParameter<T> parameter) where T : class
        {
            DoBindParameter(column, parameter);
        }
        internal protected void BindParameter<T>(ENV.DotnetColumn<T> column, DotnetParameter<T> parameter)
        {
            DoBindParameter(column, parameter);
        }

        public static event Action<ControllerBase> BeforeExecute;
        public static event Action<ControllerBase> AfterExecute;
        List<IDisposable> _streams = new List<IDisposable>();
        internal protected List<IDisposable> Streams
        {
            get { return _streams; }
        }
        internal static void SendInstanceBasedOnTaskAndCallStack(ITask t, Action<ControllerBase> to, Action<ApplicationControllerBase> toApp = null)
        {
            {
                var x = t as BusinessProcess;
                if (x != null)
                {
                    BusinessProcessBase result;
                    if (BusinessProcessBase._actionBusinessProcess.TryGetValue(x, out result))
                        to(result);



                }
            }
            {
                var x = t as UIController;
                if (x != null)
                {
                    AbstractUIController result;
                    if (AbstractUIController._activeUIControllers.TryGetValue(x, out result))
                        to(result);

                }
            }
            {
                var x = t as ModuleController;
                if (x != null)
                {
                    ApplicationControllerBase result;
                    if (ApplicationControllerBase._activeControllers.TryGetValue(x, out result))
                        if (toApp != null)
                            toApp(result);

                }
            }
        }

        ColumnBase _lastSelectedColumn = null;
        HashSet<ColumnBase> _nonRowChangingColumns;
        Action<Action> _onExecute = y => y();
        static ContextStatic<int> _tdepth = new ContextStatic<int>(() => 0);
        internal static ContextStatic<Action<Action>> _runAsInvokeV8 = new ContextStatic<Action<Action>>(() => null);
        internal abstract void _MarkRowAsChanged();
        internal protected virtual void Execute()
        {
            try
            {
                foreach (ABoundParameter parameter in _boundParameters)
                {
                    parameter.SetBoundColumnAcordingToParameterValue();
                }
                try
                {
                    if (_lastSelectedColumn == null && Columns.Count > 0)
                        _lastSelectedColumn = Columns[Columns.Count - 1];
                    if (_nonRowChangingColumns == null)
                    {
                        _nonRowChangingColumns = new HashSet<ColumnBase>();
                        foreach (var c in Columns)
                        {
                            if (!c.OnChangeMarkRowAsChanged)
                                _nonRowChangingColumns.Add(c);
                        }
                    }
                    if (_tdepth.Value == 200)
                    {
                        Message.ShowWarningInStatusBar("Task too deep");
                        return;
                    }
                    if (!_initAdditinalColumnsChangeMonitoring)
                    {
                        _initAdditinalColumnsChangeMonitoring = true;
                        foreach (ColumnBase columnBase in AdditionalColumns)
                        {
                            columnBase.ValueChanged += () =>
                            {
                                if (columnBase.OnChangeMarkRowAsChanged)
                                    _MarkRowAsChanged();
                            };
                        }
                    }
                    _tdepth.Value++;
                    if (BeforeExecute != null)
                        BeforeExecute(this);
                    using (HandlerCollectionWrapper.HandlerWrapper.StartTaskScope())
                    {
                        bool done = false;
                        if (ENV.UserSettings.Version8Compatible && _runAsInvokeV8.Value != null)
                        {
                            var z = _runAsInvokeV8.Value;
                            _runAsInvokeV8.Value = null;
                            if (_boundParameters.Count > 0)
                            {
                                done = true;
                                z(() => _onExecute(RunTask));
                            }
                        }
                        if (!done)
                            _onExecute(RunTask);
                    }
                    if (_throwRollbackExceptionAfterExecute)
                        throw new RollbackException(false);
                }

                catch (IllegalActivityException)
                {
                    Common.ShowMessageBox(LocalizationInfo.Current.IllegalActivity, MessageBoxIcon.Error, LocalizationInfo.Current.IllegalActivity);
                }
                catch (RollbackException)
                {
                    throw;
                }
                catch (AbortAllTasksException)
                {
                    if (Context.Current.ActiveTasks.Count != 1 || !(Context.Current.ActiveTasks[0] is ModuleController))
                        throw;
                }
                catch (ThreadAbortException e)
                {
                    ErrorLog.WriteToLogFile(e);
                    throw;
                }
                catch (Exception e)
                {
                    ErrorLog.WriteToLogFile(e, "Task Crashed");
                    if (e is System.Runtime.InteropServices.COMException || e.InnerException is System.Runtime.InteropServices.COMException)
                    {
                        var x = Context.Current.ActiveTasks;
                        if (x.Count > 0 && x[x.Count - 1].InTransaction)
                            throw new RollbackException(true, "Com Object missing caused task to end", e);
                        return;
                    }

                    Common.ShowExceptionDialog(e, "Task Crashed");
                    throw;
                }

                finally
                {
                    if (_iconWasSet)
                    {
                        DefaultIcon.Value = _prevIcon;
                        _prevIcon = null;

                    }
                    _tdepth.Value--;
                    _crossTaskCache.Clear();

                    _disposeStreams();

                    foreach (var d in Disposables)
                    {
                        try
                        {
                            d.Dispose();
                        }
                        catch (Exception e)
                        {
                            ENV.ErrorLog.WriteToLogFile(e, "");
                        }
                    }
                    Disposables.Clear();
                    var prev = _onUnloadRunningTask.Value;
                    try
                    {
                        _onUnloadRunningTask.Value = GetITask();
                        OnUnLoad();
                        if (UnLoad != null)
                            UnLoad();
                        ((IHaveUserMethods)this).GetUserMethods()._alternativeCurrentCommand = null;
                    }
                    finally
                    {
                        _onUnloadRunningTask.Value = prev;
                    }
                }
                if (!_donotReturnParameterValue)
                {
                    _flowAbortExceptionWhenUpdatingReturnParameter.Value = null;
                    foreach (ABoundParameter parameter in _boundParameters)
                    {
                        try
                        {
                            parameter.SetReferencedColumnAcordingToBoundColumn();
                        }
                        catch (FlowAbortException ex)
                        {
                            _flowAbortExceptionWhenUpdatingReturnParameter.Value = ex;
                        }
                    }
                    if (_flowAbortExceptionWhenUpdatingReturnParameter.Value != null)
                    {
                        var x = _flowAbortExceptionWhenUpdatingReturnParameter.Value;
                        _flowAbortExceptionWhenUpdatingReturnParameter.Value = null;
                        throw x;
                    }
                }
            }
            finally
            {
                if (AfterExecute != null)
                    AfterExecute(this);

                foreach (ABoundParameter parameter in _boundParameters)
                {
                    parameter.Dispose();
                }
                _boundParameters.Clear();
                foreach (ColumnBase columnBase in Columns)
                {
                    columnBase.ResetToDefaultValue();
                }
                foreach (ColumnBase columnBase in AdditionalColumns)
                {
                    columnBase.ResetToDefaultValue();
                }

                _handlers.SendColumnsTo(c => c.ResetToDefaultValue());
            }

        }

        static ContextStatic<ITask> _onUnloadRunningTask = new ContextStatic<ITask>(() => null);
        static ContextStatic<FlowAbortException> _flowAbortExceptionWhenUpdatingReturnParameter = new ContextStatic<FlowAbortException>(() => null);

        internal event Action UnLoad;

        protected internal void BindExitAsSoonAsPossible(Func<bool> condition, params ColumnBase[] affectingColumns)
        {
            Exit(ExitTiming.AsSoonAsPossible, condition);
            foreach (var item in affectingColumns)
            {
                Disposables.Add(new MyValueChangeHandler(item, this));

            }
            Disposables.Add(new myActiveControllerHandlerChange(this));
            _checkExitAtTheEndOfOnLoad = true;
        }
        bool _checkExitAtTheEndOfOnLoad = false;
        internal void _AfterOnLoad()
        {
            if (_checkExitAtTheEndOfOnLoad)
                CheckExit();
        }
        class myActiveControllerHandlerChange : IDisposable
        {
            ControllerBase _parent;
            public myActiveControllerHandlerChange(ControllerBase parent)
            {
                _parent = parent;
                _parent.BecomingTheCurrentTask += _parent_BecomingTheCurrentTask;
            }

            public void Dispose()
            {
                _parent.BecomingTheCurrentTask -= _parent_BecomingTheCurrentTask;
            }

            private void _parent_BecomingTheCurrentTask()
            {
                _parent.CheckExit();
            }
        }
        internal virtual protected void Exit(ExitTiming timing, Func<bool> condition) { }

        class MyValueChangeHandler : IDisposable
        {
            ColumnBase _col;
            ControllerBase _parent;
            ITask _controller;
            public MyValueChangeHandler(ColumnBase col, ControllerBase parent)
            {
                col.ValueChanged += Col_ValueChanged;
                _parent = parent;
                _col = col;
                {
                    var uic = parent as AbstractUIController;
                    if (uic != null)
                        _controller = uic._uiController;
                }
                {
                    var uic = parent as BusinessProcessBase;
                    if (uic != null)
                        _controller = uic._businessProcess;
                }

            }

            public void Dispose()
            {
                _col.ValueChanged -= Col_ValueChanged;
            }

            private void Col_ValueChanged()
            {
                var x = Firefly.Box.Context.Current.ActiveTasks;
                if (x.Count > 0 && x[x.Count - 1] == _controller || ENV.Advanced.LevelProvider.IsCurrentHandlerIn(_controller))
                    _parent.CheckExit();
            }
        }
        internal protected virtual void CheckExit()
        { }
        protected virtual void OnUnLoad()
        {
        }

        internal List<IDisposable> Disposables = new List<IDisposable>();

        internal void _disposeStreams()
        {
            foreach (var stream in _streams)
            {
                try
                {
                    stream.Dispose();
                }
                catch (Exception e)
                {
                    ENV.ErrorLog.WriteToLogFile(e, "");
                }
            }
            _streams.Clear();
        }

        Stack<ControllerBase> _cachedCallStack;
        Stack<ControllerBase> _callStack
        {
            get
            {
                if (_cachedCallStack == null)
                {
                    _cachedCallStack = Context.Current["BackwardCompatibilityParameterizedTask._callStack"] as Stack<ControllerBase>;
                    if (_cachedCallStack == null)
                    {
                        _cachedCallStack = new Stack<ControllerBase>();
                        Context.Current["BackwardCompatibilityParameterizedTask._callStack"] = _cachedCallStack;
                    }
                }
                return _cachedCallStack;
            }
        }
        void PushTaskIntoCallStack(ControllerBase t)
        {
            var cs = _callStack;
            var prev = cs.Count > 0 ? cs.Peek() : null;
            cs.Push(t);
            if (prev != null && prev.NoLongerTheCurrentTask != null) prev.NoLongerTheCurrentTask();
            if (t.BecomingTheCurrentTask != null) t.BecomingTheCurrentTask();
        }

        void PopCallStack()
        {
            var cs = _callStack;
            var prev = cs.Pop();
            var curr = cs.Count > 0 ? cs.Peek() : null;
            if (prev != null && prev.NoLongerTheCurrentTask != null) prev.NoLongerTheCurrentTask();
            if (curr != null && curr.BecomingTheCurrentTask != null) curr.BecomingTheCurrentTask();
        }

        internal event Action BecomingTheCurrentTask, NoLongerTheCurrentTask;

        internal void ExecuteBasedOnWebArgs(IWebParametersProvider args)
        {
            int i = -1;
            foreach (ColumnBase column in Columns)
            {

                if (column.Entity == null)
                {
                    i++;
                    if (i >= args.Length)
                        break;
                    {
                        Firefly.Box.Data.TextColumn p = column as Firefly.Box.Data.TextColumn;
                        if (p != null)
                        {
                            var z = args.GetString(i);
                            if (z == "")
                                z = null;
                            BindParameter(p, z);
                            continue;
                        }
                    }
                    {
                        Firefly.Box.Data.NumberColumn p = column as Firefly.Box.Data.NumberColumn;
                        if (!object.ReferenceEquals(p, null))
                        {
                            string text = args.GetString(i);
                            BindParameter(p, NumberParameter.FromWeb(text));

                            continue;
                        }
                    }
                    {
                        Firefly.Box.Data.DateColumn p = column as Firefly.Box.Data.DateColumn;
                        if (!object.ReferenceEquals(p, null))
                        {
                            BindParameter(p, Date.Parse(args.GetString(i), p.Format));
                            continue;
                        }
                    }
                    {
                        Firefly.Box.Data.TimeColumn p = column as Firefly.Box.Data.TimeColumn;
                        if (p != null)
                        {
                            BindParameter(p, Time.Parse(args.GetString(i), p.Format));
                            continue;
                        }
                    }
                    {
                        Firefly.Box.Data.BoolColumn p = column as Firefly.Box.Data.BoolColumn;
                        if (p != null)
                        {
                            BindParameter(p, BoolParameter.FromString(args.GetString(i)));
                            continue;
                        }
                    }
                    {
                        Firefly.Box.Data.ByteArrayColumn p = column as Firefly.Box.Data.ByteArrayColumn;
                        if (p != null)
                        {
                            BindParameter(p, args.GetByteArray(i, p));
                            continue;
                        }
                    }
                }
            }
            Execute();
        }

        internal void ProvideArgumentParserTo(Action<string, Func<string, object>> addArgument)
        {
            foreach (var item in Columns)
            {
                var envC = item as IENVColumn;
                if (envC != null)
                {
                    if (envC.IsParameter)
                    {
                        addArgument(item.Caption, y =>
                        {

                            if (item is DateColumn)
                                return Date.Parse(y, "YYYY-MM-DD");

                            return item.Parse(y, null);
                        });
                    }
                }
            }
        }

        internal INullStrategy _myNullStrategy = NullStrategy.GetStrategy(false);
        bool _determinedNullBehavior = false;

        internal void InitNullToColumns(bool dontDoItAgain, EntityCollection entities, UserMethods u, Firefly.Box.UI.Form view)
        {
            if (_determinedNullBehavior)
                return;


            NullStrategy.ApplyToUserInstance(Columns, entities, u, view, _myNullStrategy);
            _determinedNullBehavior = dontDoItAgain;


        }

        internal void ApplyActivityToColumns(Activities activity)
        {
            Common.SetColumnsReadonlyAcordingToActivity(Columns, activity);

        }
        internal Advanced.HandlerCollectionWrapper _handlers;
        protected internal abstract ColumnCollection Columns { get; }
        protected internal abstract Firefly.Box.Data.Entity From { get; set; }
        internal protected bool AnyColumnWithEntityChanged
        {
            get
            {
                foreach (var col in Columns)
                {
                    if (col.Entity != null && !Firefly.Box.Advanced.Comparer.Equal(col.Value, col.OriginalValue))
                        return true;
                }
                return false;
            }
        }
        internal bool _AnyColumnOfRelationSetToBoundValue(RelationCollection Relations)
        {


            foreach (var col in Columns)
            {

                if (col.Entity != null && col.Entity != From)
                {
                    var r = Relations[col.Entity];
                    if (r != null && (r.Type == RelationType.Insert || r.Type == RelationType.InsertIfNotFound) && !r.RowFound
                        && !Firefly.Box.Advanced.Comparer.Equal(col.Value, col.DefaultValue))
                        return true;
                }
            }
            return false;

        }

        internal abstract void RunTheTask();

        void RunTask()
        {
            PushTaskIntoCallStack(this);
            try
            {
                using (ENV.Utilities.Profiler.Controller(this))
                    RunTheTask();
            }
            finally
            {
                PopCallStack();
            }
        }

        protected virtual void BindView(Firefly.Box.UI.Form form)
        {
        }

        internal object ExecuteBasedOnArgs(object[] args)
        {
            var paramBuilder = new ParameterHelper();
            int i = -1;
            foreach (ColumnBase column in Columns)
            {

                if (column.Entity == null)
                {
                    i++;
                    if (i >= args.Length)
                        break;
                    object argValue = args[i];
                    if (argValue != null)
                    {
                        {
                            Firefly.Box.Data.TextColumn p = column as Firefly.Box.Data.TextColumn;
                            if (p != null)
                            {
                                Firefly.Box.Data.TextColumn argP = argValue as Firefly.Box.Data.TextColumn;
                                if (argP != null)
                                {
                                    BindParameter(p, argP);
                                    continue;
                                }

                                {
                                    var z = args[i] as ProgramCollection.StringOrByteArrayArg;
                                    if (z != null)
                                    {
                                        BindParameter(p, z.GetTextParameter());
                                        continue;
                                    }
                                }
                                BindParameter(p, new TextParameter(new ObjectParameterBridge<Text>(argValue, Text.TryCast)));
                                continue;

                            }
                        }
                        {
                            Firefly.Box.Data.NumberColumn p = column as Firefly.Box.Data.NumberColumn;
                            if (!object.ReferenceEquals(p, null))
                            {
                                Firefly.Box.Data.NumberColumn argP = argValue as Firefly.Box.Data.NumberColumn;
                                if (!object.ReferenceEquals(argP, null))
                                {
                                    BindParameter(p, argP);
                                    continue;
                                }
                                BindParameter(p, new NumberParameter(new ObjectParameterBridge<Number>(argValue, Number.TryCast)));
                                continue;
                            }
                        }
                        {
                            var p = column as Firefly.Box.Data.BoolColumn;
                            if (p != null)
                            {
                                Firefly.Box.Data.BoolColumn argP = argValue as Firefly.Box.Data.BoolColumn;
                                if (argP != null)
                                {
                                    BindParameter(p, argP);
                                    continue;
                                }
                                Number n;
                                if (Number.TryCast(argValue, out n))
                                {
                                    BindParameter(p, n != 0);
                                    continue;
                                }
                                BindParameter(p, new BoolParameter(new ObjectParameterBridge<Bool>(argValue, Bool.TryCast)));
                                continue;
                            }
                        }
                        {
                            Firefly.Box.Data.DateColumn p = column as Firefly.Box.Data.DateColumn;
                            if (p != null)
                            {
                                Firefly.Box.Data.DateColumn argP = argValue as Firefly.Box.Data.DateColumn;
                                if (argP != null)
                                {
                                    BindParameter(p, argP);
                                    continue;
                                }
                                BindParameter(p, new DateParameter(new ObjectParameterBridge<Date>(argValue, Date.TryCast)));
                                continue;
                            }
                        }
                        {
                            Firefly.Box.Data.TimeColumn p = column as Firefly.Box.Data.TimeColumn;
                            if (p != null)
                            {
                                Firefly.Box.Data.TimeColumn argP = argValue as Firefly.Box.Data.TimeColumn;
                                if (argP != null)
                                {
                                    BindParameter(p, argP);
                                    continue;
                                }
                                BindParameter(p, new TimeParameter(new ObjectParameterBridge<Time>(argValue, Time.TryCast)));
                                continue;
                            }
                        }
                        {
                            var p = column as Firefly.Box.Data.ByteArrayColumn;
                            if (p != null)
                            {
                                var argP = argValue as Firefly.Box.Data.ByteArrayColumn;
                                if (argP != null)
                                {
                                    BindParameter(p, argP);
                                    continue;
                                }
                                var z = args[i] as ProgramCollection.StringOrByteArrayArg;
                                if (z != null)
                                {
                                    BindParameter(p, z.GetByteArrayParameter());
                                    continue;
                                }
                                byte[] value = argValue as byte[];
                                if (value != null)
                                {
                                    BindParameter(p, value);
                                    continue;
                                }
                            }
                        }
                        {
                            var p = column as Firefly.Box.Data.ArrayColumn<byte[]>;
                            if (p != null)
                            {
                                var argP = argValue as Firefly.Box.Data.ArrayColumn<byte[]>;
                                if (argP != null)
                                {
                                    BindParameter(p, argP);
                                    continue;
                                }
                                var z = args[i] as byte[][];
                                if (z != null)
                                {
                                    BindParameter(p, new ArrayParameter<byte[]>(z));
                                    continue;
                                }
                                var zz = args[i] as byte[];
                                if (zz != null)
                                {
                                    BindParameter(p, new ArrayParameter<byte[]>(new byte[][] { zz }));
                                    continue;
                                }


                            }
                        }
                        {
                            var p = column as Firefly.Box.Data.ArrayColumn<Text>;
                            if (p != null)
                            {

                                BindParameter(p, paramBuilder.CreateTextArrayParameter(argValue));
                                continue;




                            }
                        }
                        if (column.GetType().IsGenericType && column.GetType().GetGenericTypeDefinition() == typeof(DotnetColumn<>))
                        {
                            if (args[i].GetType().IsGenericType && args[i].GetType().GetGenericTypeDefinition() == typeof(DotnetColumn<>))
                            {
                                var pType = typeof(DotnetParameter<>).MakeGenericType(column.GetType().GetGenericArguments());
                                var method = typeof(ControllerBase).GetMethod(nameof(DoBindParameter), BindingFlags.NonPublic | BindingFlags.Instance);
                                method = method.MakeGenericMethod(column.GetType().GetGenericArguments());
                                method.Invoke(this, new object[] { column, ProgramCollection.CreateDotnetParameter(args, i, pType) });
                                continue;
                            }
                        }
                    }
                    else
                        continue;

                    //throw new System.Exception(string.Format("couldn't match parameter {1} with column of type {0}", column.GetType(), argValue));
                }
            }
            {
                if (args.Length > 0)
                {
                    var v = args[args.Length - 1] as Firefly.Box.UI.Form;
                    if (v != null)
                    {
                        BindView(v);

                    }
                }
            }
            Execute();
            var x = this.GetType().GetField("_taskResult", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (x != null)
                return x.GetValue(this);
            return true;
        }


        internal ApplicationControllerBase _application;


        protected FilterBase CndRange(bool condition, FilterBase filter)
        {
            return FilterBase.CreateConditionedFilter(() => condition, filter);
        }
        protected internal FilterBase CndRange(Func<bool> condition, FilterBase filter)
        {
            return FilterBase.CreateConditionedFilter(() => condition(), filter);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Bool> column, Func<bool> fromCondition, Bool fromValue, Func<bool> toCondition, Func<Bool> toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Bool> column, Func<bool> fromCondition, Func<Bool> fromValue, Func<bool> toCondition, Bool toValue)
        {
            return CndRangeBetween(column, fromCondition, fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Date> column, Func<bool> fromCondition, Date fromValue, Func<bool> toCondition, Date toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Date> column, Func<bool> fromCondition, TypedColumnBase<Date> fromValue, Func<bool> toCondition, Date toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Date> column, Func<bool> fromCondition, Func<Date> fromValue, Func<bool> toCondition, Date toValue)
        {
            return CndRangeBetween(column, fromCondition, fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Date> column, Func<bool> fromCondition, Func<Date> fromValue, Func<bool> toCondition, TypedColumnBase<Date> toValue)
        {
            return CndRangeBetween(column, fromCondition, fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Date> column, Func<bool> fromCondition, Date fromValue, Func<bool> toCondition, TypedColumnBase<Date> toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Date> column, Func<bool> fromCondition, Date fromValue, Func<bool> toCondition, Func<Date> toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, toValue);
        }

        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, Func<bool> fromCondition, Func<Number> fromValue, Func<bool> toCondition, Number toValue)
        {
            return CndRangeBetween(column, fromCondition, fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, Func<bool> fromCondition, Number fromValue, Func<bool> toCondition, Number toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, Func<bool> fromCondition, Firefly.Box.Data.NumberColumn fromValue, Func<bool> toCondition, Number toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, Func<bool> fromCondition, Firefly.Box.Data.NumberColumn fromValue, bool toCondition, Number toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, () => toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, Func<bool> fromCondition, Number fromValue, Func<bool> toCondition, Firefly.Box.Data.NumberColumn toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, Func<bool> fromCondition, Firefly.Box.Data.NumberColumn fromValue, Func<bool> toCondition, Firefly.Box.Data.NumberColumn toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, Func<bool> fromCondition, Number fromValue, Func<bool> toCondition, Func<Number> toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, bool fromCondition, Number fromValue, bool toCondition, Number toValue)
        {
            return CndRangeBetween(column, () => fromCondition, () => fromValue, () => toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Bool> column, Func<bool> fromCondition, Bool fromValue, Func<bool> toCondition, Bool toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Text> column, Func<bool> fromCondition, Func<Text> fromValue, Func<bool> toCondition, Text toValue)
        {
            return CndRangeBetween(column, fromCondition, fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Text> column, Func<bool> fromCondition, Text fromValue, Func<bool> toCondition, Text toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Text> column, bool fromCondition, Text fromValue, bool toCondition, Text toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, bool fromCondition, Func<Number> fromValue, bool toCondition, Number toValue)
        {
            return CndRangeBetween(column, fromCondition, fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Text> column, Func<bool> fromCondition, Text fromValue, Func<bool> toCondition, Func<Text> toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Time> column, Func<bool> fromCondition, Time fromValue, Func<bool> toCondition, TimeColumn toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Time> column, Func<bool> fromCondition, Time fromValue, Func<bool> toCondition, Time toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Time> column, Func<bool> fromCondition, Func<Time> fromValue, Func<bool> toCondition, Time toValue)
        {
            return CndRangeBetween<Time>(column, fromCondition, fromValue, toCondition, () => toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Time> column, Func<bool> fromCondition, TimeColumn fromValue, Func<bool> toCondition, TimeColumn toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, toCondition, () => toValue);
        }

        protected FilterBase CndRangeBetween<T>(TypedColumnBase<T> column, bool fromCondition, Func<T> fromValue, bool toCondition, Func<T> toValue)
        {
            return CndRangeBetween(column, () => fromCondition, fromValue, () => toCondition, toValue);
        }
        protected FilterBase CndRangeBetween<T>(TypedColumnBase<T> column, Func<bool> fromCondition, Func<T> fromValue, Func<bool> toCondition, Func<T> toValue)
        {
            return new DynamicFilter(where =>
            {
                var fromIsTrue = fromCondition();
                var toIsTrue = toCondition();
                if (fromIsTrue && toIsTrue)
                    where.Add(column.IsBetween(fromValue, toValue));
                else if (fromIsTrue)
                    where.Add(column.IsGreaterOrEqualTo(fromValue));
                else if (toIsTrue)
                    where.Add(column.IsLessOrEqualTo(toValue));
            });
        }
        protected FilterBase CndRangeBetween<T>(TypedColumnBase<T> column, Func<bool> fromCondition, TypedColumnBase<T> fromValue, Func<bool> toCondition, Func<T> toValue)
        {
            return new DynamicFilter(where =>
            {
                var fromIsTrue = fromCondition();
                var toIsTrue = toCondition();
                if (fromIsTrue && toIsTrue)
                    where.Add(column.IsBetween(fromValue, toValue));
                else if (fromIsTrue)
                    where.Add(column.IsGreaterOrEqualTo(fromValue));
                else if (toIsTrue)
                    where.Add(column.IsLessOrEqualTo(toValue));
            });
        }
        protected FilterBase CndRangeBetween<T>(TypedColumnBase<T> column, Func<bool> fromCondition, Func<T> fromValue, Func<bool> toCondition, TypedColumnBase<T> toValue)
        {
            return new DynamicFilter(where =>
            {
                var fromIsTrue = fromCondition();
                var toIsTrue = toCondition();
                if (fromIsTrue && toIsTrue)
                    where.Add(column.IsBetween(fromValue, toValue));
                else if (fromIsTrue)
                    where.Add(column.IsGreaterOrEqualTo(fromValue));
                else if (toIsTrue)
                    where.Add(column.IsLessOrEqualTo(toValue));
            });
        }
        protected FilterBase CndRangeBetween<T>(TypedColumnBase<T> column, bool fromCondition, TypedColumnBase<T> fromValue, bool toCondition, TypedColumnBase<T> toValue)
        {
            return CndRangeBetween<T>(column, () => fromCondition, fromValue, () => toCondition, toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, Func<bool> fromCondition, Number fromValue, bool toCondition, Func<Number> toValue)
        {
            return CndRangeBetween(column, fromCondition, () => fromValue, () => toCondition, toValue);
        }
        protected FilterBase CndRangeBetween(TypedColumnBase<Number> column, bool fromCondition, Number fromValue, bool toCondition, Func<Number> toValue)
        {
            return CndRangeBetween(column, () => fromCondition, () => fromValue, () => toCondition, toValue);
        }
        protected FilterBase CndRangeBetween<T>(TypedColumnBase<T> column, Func<bool> fromCondition, TypedColumnBase<T> fromValue, Func<bool> toCondition, TypedColumnBase<T> toValue)
        {
            return new DynamicFilter(where =>
            {
                var fromIsTrue = fromCondition();
                var toIsTrue = toCondition();
                if (fromIsTrue && toIsTrue)
                    where.Add(column.IsBetween(fromValue, toValue));
                else if (fromIsTrue)
                    where.Add(column.IsGreaterOrEqualTo(fromValue));
                else if (toIsTrue)
                    where.Add(column.IsLessOrEqualTo(toValue));
            });
        }


        internal Advanced.LevelProvider _levelProvider;
        int _mainDisplayIndex = 0;
        internal bool _inSubformReload = false;

        internal int GetMainDisplayIndex()
        {
            return _mainDisplayIndex;
        }

        protected void SetMainDisplayIndex(int index)
        {
            _mainDisplayIndex = index;
        }

        internal virtual void ProvideSubForms(Action<SubForm> runForEachSubForm)
        {

        }

        internal protected Common.WhenValueChange<DataType> MonitorValueChanged<DataType>(TypedColumnBase<DataType> c, NumberColumn changeReasonColumn, TypedColumnBase<DataType> previousValueColumn, Func<bool> condition)
        {
            var result = new Common.WhenValueChange<DataType>();
            Common.MonitorValueChanged(c, args =>
            {

                using (_levelProvider.ColumnChange(c))
                {
                    if (GetITask().CurrentHandledCommand == Command.UndoChangesInRow && !GetRowChanged() && _levelProvider.GetMainLevel() == "RP")
                        return;
                    _flowAbortExceptionWhenUpdatingReturnParameter.Value = null;
                    result.Run(args);
                }
            }, changeReasonColumn, previousValueColumn, ProvideSubForms, condition);
            return result;
        }

        internal abstract bool GetRowChanged();

        internal protected Common.WhenValueChange<DataType> MonitorValueChanged<DataType>(TypedColumnBase<DataType> c, NumberColumn changeReasonColumn, TypedColumnBase<DataType> previousValueColumn)
        {
            return MonitorValueChanged(c, changeReasonColumn, previousValueColumn, null);
        }
        internal protected Common.WhenValueChange<DataType> MonitorValueChanged<DataType>(TypedColumnBase<DataType> c, NumberColumn changeReasonColumn)
        {
            return MonitorValueChanged(c, changeReasonColumn, null, null);
        }
        internal protected Common.WhenValueChange<DataType> MonitorValueChanged<DataType>(TypedColumnBase<DataType> c)
        {
            return MonitorValueChanged(c, (NumberColumn)null, null, null);
        }
        internal protected Common.WhenValueChange<DataType> MonitorValueChanged<DataType>(TypedColumnBase<DataType> c, NumberColumn changeReasonColumn, Func<bool> condition)
        {
            return MonitorValueChanged(c, changeReasonColumn, null, condition);
        }
        internal protected Common.WhenValueChange<DataType> MonitorValueChanged<DataType>(TypedColumnBase<DataType> c, Func<bool> condition)
        {
            return MonitorValueChanged(c, (NumberColumn)null, null, condition);
        }

        internal protected void MonitorValueChanged<Type>(TypedColumnBase<Type> c, ValueChangedHandler<Type> handler, NumberColumn changeReasonColumn, TypedColumnBase<Type> previousValueColumn, Func<bool> condition)
        {
            Common.MonitorValueChanged(c, args =>
            {

                using (_levelProvider.ColumnChange(c))
                {
                    handler(args);
                }
            }, changeReasonColumn, previousValueColumn, ProvideSubForms, condition);
        }

        internal protected void MonitorValueChanged<Type>(TypedColumnBase<Type> c, ValueChangedHandler<Type> handler, NumberColumn changeReasonColumn, TypedColumnBase<Type> previousValueColumn)
        {
            MonitorValueChanged(c, handler, changeReasonColumn, previousValueColumn, null);
        }
        internal protected void MonitorValueChanged<Type>(TypedColumnBase<Type> c, ValueChangedHandler<Type> handler, NumberColumn changeReasonColumn)
        {
            MonitorValueChanged(c, handler, changeReasonColumn, null, null);
        }
        internal protected void MonitorValueChanged<Type>(TypedColumnBase<Type> c, ValueChangedHandler<Type> handler)
        {
            MonitorValueChanged(c, handler, null, null, null);
        }
        internal protected void MonitorValueChanged<Type>(TypedColumnBase<Type> c, ValueChangedHandler<Type> handler, NumberColumn changeReasonColumn, Func<bool> condition)
        {
            MonitorValueChanged(c, handler, changeReasonColumn, null, condition);
        }
        internal protected void MonitorValueChanged<Type>(TypedColumnBase<Type> c, ValueChangedHandler<Type> handler, Func<bool> condition)
        {
            MonitorValueChanged(c, handler, null, null, condition);
        }

        public class SingleInstanceManager
        {
            public Context Context { get; set; }
            public ControllerBase Instance { get; set; }
        }

        public class AsyncParameters
        {
            ApplicationControllerBase _appInOriginalThread;

            public AsyncParameters(ApplicationControllerBase appInOriginalThread)
            {
                _appInOriginalThread = appInOriginalThread;
            }

            public bool CopyParametersInMemory { get; set; }
            public bool DisableApplicationStart { get; set; }
            public SingleInstanceManager SingleInstanceManager { get; set; }




            internal void RunApp()
            {
                new BusinessProcess { Module = _appInOriginalThread._moduleController }.ForFirstRow(() => { });
            }
        }

        public static string RunAsync<T>(Func<T> factory, Action<T> runInstance, AsyncParameters args) where T : class
        {
            string result = "";
            if (args.SingleInstanceManager != null && args.SingleInstanceManager.Context != null)
            {
                bool wasRun = false;
                args.SingleInstanceManager.Context.BeginInvoke(
                    () =>
                    {
                        args.SingleInstanceManager.Instance._inSubformReload = true;
                        try
                        {
                            runInstance(args.SingleInstanceManager.Instance as T);
                        }
                        finally
                        {
                            args.SingleInstanceManager.Instance._inSubformReload = false;
                            wasRun = true;
                        }
                    });
                result = UserMethods.GetContextName(args.SingleInstanceManager.Context);

                var t = new List<ITask>(args.SingleInstanceManager.Context.ActiveTasks);
                t.Reverse();
                foreach (var task in t)
                {
                    if (task.View != null)
                    {
                        args.SingleInstanceManager.Context.BeginInvoke(
                            () =>
                            {
                                if (args.SingleInstanceManager.Context != null)
                                    args.SingleInstanceManager.Context.InvokeUICommand(() => task.View.TryFocus(task.View.Activate));
                            });
                        break;
                    }
                }
                ENV.Common.RaiseOnContext(args.SingleInstanceManager.Context, Commands.SingleInstanceAsyncTaskReactivated, new object[0]);
                while (args.SingleInstanceManager.Context != Context.Current && !wasRun)
                    UserMethods.Instance.Delay(1);
                return result;
            }
            var wait = new ManualResetEvent(false);
            var parInOriginalThread = ENV.ParametersInMemory.Instance;
            ENV.ParametersInMemory.Copy copy = null;
            if (args.CopyParametersInMemory)
            {
                copy = parInOriginalThread.CreateCopy();
            }
            var originalContext = Firefly.Box.Context.Current;
            args.RunApp();
            try
            {
                Common.RunOnNewThread(() =>
                {
                    var instance = factory();
                    var x = (ControllerBase)(object)instance;

                    try
                    {
                        if (args.SingleInstanceManager != null)
                        {
                            args.SingleInstanceManager.Context = Firefly.Box.Context.Current;
                            args.SingleInstanceManager.Instance = x;
                        }
                        if (args.DisableApplicationStart)
                        {
                            ApplicationControllerBase._suppressStartEvent =
                                @base =>
                                {
                                    var appInOriginalThread = originalContext[@base.GetType()] as ApplicationControllerBase;
                                    if (appInOriginalThread == null)
                                        appInOriginalThread =
                                            originalContext[@base.GetType().BaseType] as ApplicationControllerBase;
                                    if (@base._moduleController.Columns.Count >
                                        appInOriginalThread._moduleController.Columns.Count)
                                        throw new InvalidOperationException(
                                            "invalid number of columns between application in original thread and current thread");
                                    for (int i = 0; i < @base._moduleController.Columns.Count; i++)
                                    {
                                        var c = @base._moduleController.Columns[i];
                                        if (UserMethods.IsDerivedFromGenericType(c.GetType(), typeof(Firefly.Box.Interop.ComColumn<>)))
                                            c.GetType().GetMethod("SetValueWithoutEventHandling").Invoke(c, new[] { appInOriginalThread._moduleController.Columns[i].Value });
                                        else
                                            c.Value =
                                                appInOriginalThread._moduleController.Columns[i].Value;

                                    }
                                    return true;
                                };
                            x._beforeControllerStart = () =>
                            {
                                ApplicationControllerBase._suppressStartEvent = null;
                            };
                        }

                        if (args.CopyParametersInMemory)
                        {
                            copy.ApplyTo(ENV.ParametersInMemory.Instance);
                        }
                        result = ENV.UserMethods.Instance.CtxGetId("").ToString();

                        x._onExecute = y =>
                        {
                            wait.Set();
                            ApplicationControllerBase.RunOnRootApplication(() =>
                            {
                                y();
                            });

                        };
                        x.DoNotReturnParameterValues();
                        try
                        {
                            if (System.Threading.Thread.CurrentThread.Name == null)
                                System.Threading.Thread.CurrentThread.Name = x.GetType().FullName;
                        }
                        catch
                        {
                        }
                        runInstance(instance);



                    }
                    catch (Exception e)
                    {
                        ErrorLog.WriteToLogFile(e);
                    }
                    finally
                    {
                        if (args.SingleInstanceManager != null)
                        {
                            args.SingleInstanceManager.Context = null;
                            args.SingleInstanceManager.Instance = null;
                        }
                        wait.Set();
                    }
                });
                wait.WaitOne();
            }
            finally
            {

            }
            if (UserSettings.VersionXpaCompatible)
                Context.Current.Suspend(100);
            return result;

        }
        bool _donotReturnParameterValue = false;
        private void DoNotReturnParameterValues()
        {
            _donotReturnParameterValue = true;
        }

        internal protected void RaiseOnContext(Text contextName, Command command, params object[] untypedArgs)
        {
            RaiseOnContextInternal(contextName, command, untypedArgs);
        }

        internal protected void RaiseOnContext(Text contextName, CommandWithArgs command)
        {
            RaiseOnContextInternal(contextName, command.Command, command.Argmuents);
        }

        internal static void RaiseOnContextInternal(Text contextName, Command command, params object[] untypedArgs)
        {
            RaiseOnContextPrivate(contextName, untypedArgs, (args) => ENV.Common.Raise(command, args),
                (context, args) => Common.RaiseOnContext(context, command, args));
        }
        internal protected void RaiseOnContext(Text contextName, Text command, params object[] untypedArgs)
        {
            RaiseOnContextInternal(contextName, command, untypedArgs);
        }
        internal static void RaiseOnContextInternal(Text contextName, Text command, params object[] untypedArgs)
        {
            RaiseOnContextPrivate(contextName, untypedArgs, (args) => ENV.Common.Raise(command, args),
                (context, args) => Common.RaiseOnContext(context, command, args));
        }

        static void RaiseOnContextPrivate(Text contextName, object[] args, Action<object[]> raise, Action<Context, object[]> raiseOnContext)
        {
            object[] argsUnboxed = null;
            if (args != null)
            {
                argsUnboxed = new object[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    var x = args[i];
                    var cb = x as ColumnBase;
                    if (cb != null)
                        x = cb.Value;
                    argsUnboxed[i] = x;
                }
            }

            if (Text.IsNullOrEmpty(contextName))
                raise(argsUnboxed);
            contextName = contextName.TrimEnd();
            foreach (var activeContext in Firefly.Box.Context.ActiveContexts)
            {
                if (string.Compare(contextName, UserMethods.GetContextName(activeContext),
                        StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    var x = activeContext;
                    activeContext.BeginInvokeDelayed(() => raiseOnContext(x, argsUnboxed));
                }
            }
        }
        internal class ControllerColumns : IEnumerable<ColumnBase>
        {

            List<ColumnBase> _columns;
            Dictionary<string, ColumnBase> __colsByName;


            public ControllerColumns(ApplicationControllerBase y)
            {
                _columns = new List<ColumnBase>(y.Columns.Count);

                foreach (var column in y.Columns)
                {
                    if (column is UserMethods.NotIncludedInVarIndexCalculations)
                        continue;
                    _columns.Add(column);

                }
                y.Handlers.SendColumnsTo(_columns.Add);
                _columns.AddRange(y.AdditionalColumns);
            }

            public ControllerColumns(ITask task)
            {
                _columns = new List<ColumnBase>(task.Columns.Count);
                foreach (var column in task.Columns)
                {
                    if (column is UserMethods.NotIncludedInVarIndexCalculations)
                        continue;
                    _columns.Add(column);
                }
            }

            public ControllerColumns(ControllerBase y)
            {
                _columns = new List<ColumnBase>(y.Columns.Count);

                foreach (var column in y.Columns)
                {
                    if (column is UserMethods.NotIncludedInVarIndexCalculations)
                        continue;
                    _columns.Add(column);
                    if (y._lastSelectedColumn == column)
                        break;
                }
                y._handlers.SendColumnsTo(_columns.Add);
                _columns.AddRange(y.AdditionalColumns);
            }

            internal int IndexOf(ColumnBase column)
            {
                return _columns.IndexOf(column);
            }
            internal int IndexOf(string columnCaption)
            {
                var col = this[columnCaption];
                if (col == null)
                    return -1;
                return IndexOf(col);

            }



            public IEnumerator<ColumnBase> GetEnumerator()
            {
                return _columns.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _columns.GetEnumerator();
            }
            public ColumnBase this[int index] { get { return _columns[index]; } }
            public ColumnBase this[string caption]
            {
                get
                {
                    if (__colsByName == null)
                    {
                        __colsByName = new Dictionary<string, ColumnBase>(_columns.Count);
                        for (int i = _columns.Count - 1; i >= 0; i--)
                        {
                            var item = _columns[i];
                            if (item.Caption != null)
                                if (!__colsByName.ContainsKey(item.Caption))
                                    __colsByName.Add(item.Caption, item);
                        }

                    }

                    ColumnBase result = null;
                    if (__colsByName.TryGetValue(caption, out result))
                        return result;
                    return null;
                }
            }

            public int Count { get { return _columns.Count; } }
        }
        ControllerColumns _columns;
        internal static ControllerColumns GetColumnsOf(ITask task)
        {
            ControllerColumns result = null;
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(task,
                y =>
                {
                    result = y.GetMyColumns();

                });

            if (result == null)
            {
                var mc = task as ModuleController;
                if (mc != null)
                    ApplicationControllerBase.SendColumnsOf(mc, to => result = to);
            }


            if (result == null)
            {
                result = new ControllerColumns(task);
            }
            return result;
        }

        internal ControllerColumns GetMyColumns()
        {
            ControllerColumns result;
            if (this._columns == null)
            {
                this._columns = new ControllerColumns(this);
            }

            result = this._columns;
            return result;
        }

        bool _syncedXmlElements = false;
        protected void SyncCreatedXmlElements()
        {
            if (_syncedXmlElements)
                return;
            _syncedXmlElements = true;
            XmlEntity e = From as XmlEntity;
            if (e != null && GetActivity() == Activities.Insert)
            {
                e.CreateNewElementAndUpdateNodeId(Columns);
            }
            foreach (var item in GetRelations())
            {
                e = item.From as XmlEntity;
                if (e != null && (item.Type == RelationType.Insert || item.Type == RelationType.InsertIfNotFound) && item.Enabled && !item.RowFound)
                    e.CreateNewElementAndUpdateNodeId(Columns);
            }

        }
        internal abstract Activities GetActivity();
        internal abstract RelationCollection GetRelations();
        internal void _callMeOnEnterRow()
        {
            _syncedXmlElements = false;
        }
        internal bool DoesColumnValueChangeCauseRowChanged(ColumnBase column)
        {
            if (_nonRowChangingColumns.Contains(column))
                return false;
            if (From is DynamicSQLEntity && column.Entity == null && ENV.UserSettings.Version10Compatible)
            {
                HashSet<int> sqlColumnPositions = new HashSet<int>();
                int allCount = 0;
                int nonSQLCount = 0;
                foreach (var item in Columns)
                {
                    if (item.Entity != null && item.Entity != From)
                        continue;
                    allCount++;
                    if (item.Entity == From)
                    {
                        sqlColumnPositions.Add(allCount);
                    }
                    else
                    {
                        nonSQLCount++;
                        if (item == column)
                            return !sqlColumnPositions.Contains(nonSQLCount);
                    }
                }

                return false;

            }
            return true;
        }

        protected void Try(Action operationToRun)
        {
            Common.Try(operationToRun);
        }
        protected T Try<T>(Func<T> operationToRun)
        {
            T result = default(T);
            Common.Try(() => result = operationToRun());
            return result;
        }
        protected void Try(Action operationToRun, NumberColumn returnCode)
        {
            Common.Try(operationToRun, returnCode);
        }

        internal void SendParametersTo(Action<string, string> action)
        {
            foreach (var aBoundParameter in _boundParameters)
            {
                aBoundParameter.ProvideNameAndValue(action);
            }
        }
        bool _throwRollbackExceptionAfterExecute = false;
        internal void ThrowRollbackExcpetionAfterExecute()
        {
            _throwRollbackExceptionAfterExecute = true;
        }
        internal static IEnumerable<ITask> GetActiveTasks(Firefly.Box.Context context)
        {
            if (_onUnloadRunningTask.GetValueFromContext(context) == null)
                return context.ActiveTasks;
            else
            {
                var l = new List<ITask>(context.ActiveTasks);
                l.Add(_onUnloadRunningTask.Value);
                return l;
            }
        }

        protected void SetView(Func<Form> value, Action<Form> setTaskView, Action<Form> setDefaultContextMenu, bool forceApplyIcon)
        {
            var t = GetITask();
            if (t.View != null && t.View.IsDisposed)
                setTaskView(null);
            if (t.View != null)
                return;

            Form f = null;
            var currentContext = Context.Current;
            Context uiContext = null;
            ENV.UI.Form ff = null;
            currentContext.InvokeUICommand(() =>
            {
                f = value();
                ff = f as ENV.UI.Form;
                uiContext = Context.Current;
                if (f.ForceTopLevel)
                {
                    if (!UserSettings.VersionXpaCompatible && uiContext == currentContext)
                    {
                        f.ForceTopLevel = false;
                        f.ContainerForm = null;
                    }
                    else return;
                }

                setDefaultContextMenu(f);
            });
            if (ff != null && ff.GetForceTopLevel() && currentContext != uiContext)
            {
                var detachFromUIContext = true;
                foreach (var activeTask in currentContext.ActiveTasks)
                {
                    if (activeTask.View != null && activeTask.View.Visible)
                    {
                        detachFromUIContext = false;
                        break;
                    }
                }
                if (detachFromUIContext)
                {
                    uiContext.BeginInvoke(f.Dispose);
                    currentContext.DetachFromUIContext();
                    f = value();
                    ff = f as ENV.UI.Form;
                    UnLoad += () =>
                    {
                        if (f.IsDisposed)
                            Context.Current.AttachToUIContext(uiContext);
                    };
                }
            }
            setTaskView(f);
            if (ff != null && _application != null)
                ff.DoControllerLoad(_application != null && !_application.RunnedAsApplication, Columns);
            if (forceApplyIcon || f.TitleBar)
                ApplyIcon(f);
        }
    }

    public abstract class ParameterBase<type> : IParameterBase
    {
        internal Func<ColumnBase, type> _getValue;
        internal Action<ColumnBase, type> _setReturnValue;
        protected ParameterBase(Func<ColumnBase, type> getValue)
        {
            _getValue = getValue;
            _setReturnValue = delegate { };
        }
        ColumnBase IParameterBase.GetColumn()
        {
            return _col;
        }
        object IParameterBase.GetRawValue()
        {
            return _GetRawValue();
        }
        protected virtual object _GetRawValue()
        {
            return Value;
        }


        protected ParameterBase(TypedColumnBase<type> column)
            : this(delegate
                   {
                       if (column == null)
                           return default(type);
                       return column.Value;
                   }, delegate (ColumnBase c, type obj)
            {
                if (column == null)
                    return;
                if (ReferenceEquals(obj, null) && !column.AllowNull)
                {
                    column.AllowNull = true;
                    column.Value = obj;
                    column.AllowNull = false;
                }
                else
                    column.Value = obj;
            }, column)
        {
        }

        Action _onSetReturnValue = delegate { };
        internal void OnSetReturnValue(Action what)
        {
            _onSetReturnValue = what;
        }

        protected ParameterBase(Func<ColumnBase, type> getValue, Action<type> setReturnValue,
            ColumnBase column)
            : this(getValue, (c, v) => setReturnValue(v), column)
        {
        }
        internal ParameterBase(ObjectParameterBridge<type> x) : this(x.GetValue, x.SetValue, x.Column)
        {
        }

        protected ParameterBase(Func<ColumnBase, type> getValue, Action<ColumnBase, type> setReturnValue, ColumnBase column)
        {
            _getValue = getValue;
            _setReturnValue = (c, value) =>
            {
                _onSetReturnValue();
                var x = column as IENVColumn;
                if (x != null)
                {
                    x.UpdateParameterReturnValue(() => setReturnValue(c, value));
                }
                else
                {
                    setReturnValue(c, value);
                }

            };
            _col = column;
        }

        public ParameterBase(type value)
        {
            _getValue = delegate { return value; };
            _setReturnValue = delegate { };
        }
        bool _disableDenyUpdate;
        internal type GetValue(ColumnBase column)
        {
            if (_col == null)
            {
                _col = column;
                _disableDenyUpdate = true;
            }
            return _getValue(column);
        }
        ColumnBase _col;
        public type Value
        {
            get { return _getValue(_col); }
            set { SetReturnValue(_col, value); }
        }

        internal void SetReturnValue(ColumnBase column, type value)
        {
            _setReturnValue(column, value);
        }
        public override string ToString()
        {
            var x = _getValue(null);
            if (x != null)
                return x.ToString();
            return "null";
        }

        internal virtual void SetValueToBoundColumn(type value, TypedColumnBase<type> column)
        {
            column.Value = value;


        }

        internal void DenyUndoForSentColumnSentAsParameter(UserMethods u)
        {
            if (_col != null && !_disableDenyUpdate)
            {
                u.DenyUndoFor(_col);
            }
        }
    }
    interface IWebParametersProvider
    {
        string GetString(int index);
        byte[] GetByteArray(int index, Firefly.Box.Data.ByteArrayColumn column);
        int Length { get; }
    }
    class WebParameterProvider : IWebParametersProvider
    {
        object[] _args;

        public WebParameterProvider(params object[] args)
        {
            _args = args;
        }

        public string GetString(int index)
        {
            return _args[index] as string;
        }

        public byte[] GetByteArray(int index, Firefly.Box.Data.ByteArrayColumn column)
        {
            byte[] result = _args[index] as byte[];
            if (result != null)
                return result;
            var bac = column as ENV.Data.ByteArrayColumn;
            if (bac != null && bac.ContentType == ByteArrayColumnContentType.BinaryUnicode)
            {
                return ByteArrayColumn.ToAnsiByteArray(_args[index] as string);
            }
            return column.FromString(_args[index] as string);
        }

        public int Length
        {
            get { return _args.Length; }
        }
    }

    class CachedControllerManager
    {
        object _containingController;


        public CachedControllerManager(object containingController)
        {
            _containingController = containingController;
        }

        Dictionary<Type, ICachedController> _cachedController = new Dictionary<Type, ICachedController>();
        [ThreadStatic]
        static Dictionary<Type, Func<object, object>> _factories;


        public T GetCachedController<T>() where T : class
        {

            return (T)GetTheCachedController<T>().GetInstance();

        }
        internal ICachedController GetTheCachedController<T>() where T : class
        {
            ICachedController result;
            var type = typeof(T);
            if (!_cachedController.TryGetValue(type, out result))
            {
                _cachedController.Add(type, result = new CachedController<T>(() =>
                {
                    return (T)GetFactory(type)(_containingController);
                }));
            }
            return result;
        }

        internal static Func<object, object> GetFactory(Type type)
        {
            if (_factories == null)
                _factories = new Dictionary<Type, Func<object, object>>();
            Func<object, object> fact;
            if (!_factories.TryGetValue(type, out fact))
            {
                if (type.IsInterface)
                {
                    _factories.Add(type, fact = d => AbstractFactory.CreateInstance(type));
                }
                else
                if (type.IsAbstract)
                {
                    var m = type.GetMethod("Create");
                    _factories.Add(type, fact = d => m.Invoke(null, new object[0]));
                }
                else
                {
                    ConstructorInfo m = null;
                    foreach (var c in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (c.GetParameters().Length == 0)
                        {
                            _factories.Add(type, fact = d => c.Invoke(new object[0]));
                            break;
                        }
                        else if (c.GetParameters().Length == 1)
                            m = c;
                    }
                    if (fact == null)
                        if (m != null)
                            _factories.Add(type, fact = d => m.Invoke(new object[] { d }));
                        else
                            _factories.Add(type, fact = d => System.Activator.CreateInstance(type, new object[] { d }));
                }
            }
            return fact;
        }

        internal void Iterate<T>(Action<T> what) where T : class
        {
            foreach (var item in _cachedController)
            {
                var z = item.Value as ICachedController;
                if (z != null)
                {
                    var i = z.GetInstance() as T;
                    if (i != null)
                        what(i);
                }
            }

        }


        internal void Clear()
        {
            _cachedController.Clear();
        }
    }
    class CachedManager : IDisposable
    {

        object _defaultParamenterForConstructor;
        public CachedManager()
        {
        }
        public CachedManager(object defaultParameterForConstructor)
        {
            _defaultParamenterForConstructor = defaultParameterForConstructor;
        }
        public void Dispose()
        {
            if (_liveInstances == null)
                return;
            var x = _liveInstances;
            _liveInstances = null;
            foreach (var item in x.Values)
            {
                var d = item as IDisposable;
                if (d != null)
                    d.Dispose();
            }
        }
        Dictionary<Type, object> _liveInstances;

        public event Action<object> InstanceCreated;

        internal T Cached<T>()
        {
            return (T)Cached(typeof(T));
        }
        internal object Cached(Type type)
        {

            if (_liveInstances == null)
                _liveInstances = new Dictionary<Type, object>();

            object result;
            if (!_liveInstances.TryGetValue(type, out result))
            {

                result = CachedControllerManager.GetFactory(type)(_defaultParamenterForConstructor);
                if (InstanceCreated != null)
                    InstanceCreated(result);
                _liveInstances.Add(type, result);
            }
            return result;
        }

        internal void Iterate<T>(Action<T> what) where T : class
        {
            if (_liveInstances == null)
                return;
            foreach (var item in _liveInstances.Values)
            {
                var z = item as T;
                if (z != null)
                    what(z);
            }
        }
    }
    public class AsyncHelperBase
    {
        protected bool CopyParametersInMemory { get; set; }
        protected bool DisableApplicationStart { get; set; }
        protected bool SingleInstance { get; set; }
        ApplicationControllerBase _app;
        public AsyncHelperBase(ApplicationControllerBase app)
        {
            _app = app;
        }
        public AsyncHelperBase(System.Type applicationClassType)
        {
            _app = (ApplicationControllerBase)Firefly.Box.Context.Current[applicationClassType];
            if (_app == null)
                throw new InvalidOperationException("Application Class Type Was not Set");
        }
        protected string RunAsync<T>(Action<T> run) where T : class
        {
            return PrivateRunAsync<T>(run, typeof(T));
        }
        string PrivateRunAsync<T>(Action<T> run, Type controllerType) where T : class
        {

            var p = new ControllerBase.AsyncParameters(_app)
            {
                CopyParametersInMemory = CopyParametersInMemory,
                DisableApplicationStart = DisableApplicationStart
            };
            if (SingleInstance)
            {
                var type = typeof(T);
                ControllerBase.SingleInstanceManager r;
                lock (_singleInstancesManagers)
                {
                    if (!_singleInstancesManagers.TryGetValue(type, out r))
                    {
                        _singleInstancesManagers.Add(type, r = new ENV.ControllerBase.SingleInstanceManager());
                    }
                }
                p.SingleInstanceManager = r;
            }
            return ControllerBase.RunAsync<T>(() => (T)AbstractFactory.CreateInstance(controllerType), run, p);
        }

        internal string ExecuteBasedOnArgs(object[] args, Type controllerType)
        {
            return PrivateRunAsync<ControllerBase>(r => r.ExecuteBasedOnArgs(args), controllerType);
        }

        static Dictionary<Type, ControllerBase.SingleInstanceManager> _singleInstancesManagers = new Dictionary<Type, ControllerBase.SingleInstanceManager>();
    }
    delegate bool TryCast<T>(object o, out T t);
    class ObjectParameterBridge<T>
    {
        object _argValue;
        public readonly ColumnBase Column;
        TryCast<T> _tryCast;

        public ObjectParameterBridge(object argValue, TryCast<T> tryCast)
        {
            _argValue = argValue;
            Column = argValue as ColumnBase;
            if (Column != null)
                _argValue = Column.Value;
            _tryCast = tryCast;
        }

        internal T GetValue(ColumnBase arg)
        {
            T r = default(T);
            _tryCast(_argValue, out r);
            return r;
        }

        internal void SetValue(ColumnBase col, T obj)
        {
            if (Column != null)
                try
                {
                    Column.Value = obj;
                }
                catch (Exception ex)
                {
                    ErrorLog.WriteToLogFile(ex);
                }

        }
    }


}
