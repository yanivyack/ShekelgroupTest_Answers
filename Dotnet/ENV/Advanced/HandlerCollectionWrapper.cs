using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.Flow;
using Firefly.Box.UI.Advanced;

namespace ENV.Advanced
{
    public class HandlerCollectionWrapper
    {
        Firefly.Box.Advanced.HandlerCollection _root;
        LevelProvider _levelProvider;
        Func<bool> _disableExpressionHandlersWhileCommandsArePending;
        Func<bool> _disableHandlersWhileInvoked;
        CachedControllerManager _cachedControllerManager;
        Func<ProgramCollection> _progs;
        Action<Action> _invokeInController;
        internal HandlerCollectionWrapper(HandlerCollection root, LevelProvider levelProvider, Func<bool> disableExpressionHandlersWhileCommandsArePending,
            Func<bool> disableHandlersWhileInvoked, CachedControllerManager cachedControllerManager, Func<ProgramCollection> progs, Action<Action> invokeInController)
        {
            _invokeInController = invokeInController;
            _progs = progs;
            _cachedControllerManager = cachedControllerManager;
            _root = root;
            _levelProvider = levelProvider;
            _disableExpressionHandlersWhileCommandsArePending = disableExpressionHandlersWhileCommandsArePending;
            _disableHandlersWhileInvoked = disableHandlersWhileInvoked;
        }

        public HandlerWrapper Add<T>(Firefly.Box.Interop.ComColumn<T> column, string eventName, HandlerScope scope)
            where T : class
        {
            var x = Add(column, eventName);
            x.Scope = scope;
            return x;
        }

        public HandlerWrapper Add<T>(Firefly.Box.Interop.ComColumn<T> column, string eventName) where T : class
        {
            if (eventName == "DragDrop" && typeof(Control).IsAssignableFrom(typeof(T)))
            {
                column.ValueChanged += delegate
                {
                    var c = column.Value as Control;
                    if (c != null)
                        c.DragEnter += (s, e) => e.Effect = e.AllowedEffect;
                };
            }
            return new HandlerWrapper(_levelProvider, "HX_" + eventName, _root.Add(column, eventName), null, _disableExpressionHandlersWhileCommandsArePending, _disableHandlersWhileInvoked, _cachedControllerManager, this);
        }

        public HandlerWrapper Add<T>(string eventName, HandlerScope scope) where T : class
        {
            var x = Add<T>(eventName);
            x.Scope = scope;
            return x;
        }

        public HandlerWrapper Add<T>(string eventName) where T : class
        {
            return new HandlerWrapper(_levelProvider, "HX_" + eventName, _root.Add<T>(eventName), null, _disableExpressionHandlersWhileCommandsArePending, _disableHandlersWhileInvoked, _cachedControllerManager, this);
        }

        public HandlerWrapper Add(Keys keyCombination, HandlerScope scope)
        {
            var x = Add(keyCombination);
            x.Scope = scope;
            return x;
        }

        public HandlerWrapper Add(Keys keyCombination)
        {
            return new HandlerWrapper(_levelProvider, "HS_" + keyCombination, _root.Add(keyCombination), null, _disableExpressionHandlersWhileCommandsArePending, _disableHandlersWhileInvoked, _cachedControllerManager, this);
        }

        public HandlerWrapper Add(Keys keyCombination, ControlBase triggeringControl, HandlerScope scope)
        {
            var x = Add(keyCombination, triggeringControl);
            x.Scope = scope;
            return x;
        }

        public HandlerWrapper Add(Keys keyCombination, ControlBase triggeringControl)
        {
            return new HandlerWrapper(_levelProvider, "HS_" + keyCombination, _root.Add(keyCombination, triggeringControl), null, _disableExpressionHandlersWhileCommandsArePending, _disableHandlersWhileInvoked, _cachedControllerManager, this);
        }

        public HandlerWrapper Add(Keys keyCombination, Predicate<ControlBase> ifControlMatches, HandlerScope scope)
        {
            var x = Add(keyCombination, ifControlMatches);
            x.Scope = scope;
            return x;
        }

        public HandlerWrapper Add(Keys keyCombination, Predicate<ControlBase> ifControlMatches)
        {
            return new HandlerWrapper(_levelProvider, "HS_" + keyCombination, _root.Add(keyCombination, ifControlMatches), null, _disableExpressionHandlersWhileCommandsArePending, _disableHandlersWhileInvoked, _cachedControllerManager, this);
        }

        public HandlerWrapper Add(Keys keyCombination, string triggeringControlTag, HandlerScope scope)
        {
            var x = Add(keyCombination, triggeringControlTag);
            x.Scope = scope;
            return x;
        }

        public HandlerWrapper Add(Keys keyCombination, string triggeringControlTag)
        {
            return new HandlerWrapper(_levelProvider, "HS_" + keyCombination, _root.Add(keyCombination, triggeringControlTag), null, _disableExpressionHandlersWhileCommandsArePending, _disableHandlersWhileInvoked, _cachedControllerManager, this);
        }
        string GetStringForCommand(Command c)
        {
            if (c == null)
                return "null";
            if (c is CustomCommand)
                if (c.Name != null && c.Name.StartsWith("User Action "))
                    return "HI_" + c.Name;
                else
                    return "HU_" + c.Name;
            if (c.ToString().StartsWith("Exp"))
                return "HE";
            if (c.ToString().StartsWith("Tim"))
                return "HT_" + c.ToString().Substring(13);
            return "HI_" + c.Name;
        }

        public HandlerWrapper Add(Command triggeringCommand, HandlerScope scope)
        {
            var x = Add(triggeringCommand);
            x.Scope = scope;
            return x;
        }

        public HandlerWrapper Add(Command triggeringCommand)
        {
            return new HandlerWrapper(_levelProvider, GetStringForCommand(triggeringCommand), _root.Add(triggeringCommand), triggeringCommand, _disableExpressionHandlersWhileCommandsArePending, _disableHandlersWhileInvoked, _cachedControllerManager, this);
        }

        public HandlerWrapper Add(Command triggeringCommand, ControlBase triggeringControl, HandlerScope scope)
        {
            var x = Add(triggeringCommand, triggeringControl);
            x.Scope = scope;
            return x;
        }

        public HandlerWrapper Add(Command triggeringCommand, ControlBase triggeringControl)
        {
            return new HandlerWrapper(_levelProvider, GetStringForCommand(triggeringCommand), _root.Add(triggeringCommand, triggeringControl), triggeringCommand, _disableExpressionHandlersWhileCommandsArePending, _disableHandlersWhileInvoked, _cachedControllerManager, this);
        }

        public HandlerWrapper Add(Command triggeringCommand, string triggeringControlTag, HandlerScope scope)
        {
            var x = Add(triggeringCommand, triggeringControlTag);
            x.Scope = scope;
            return x;
        }

        public HandlerWrapper Add(Command triggeringCommand, string triggeringControlTag)
        {
            return new HandlerWrapper(_levelProvider, GetStringForCommand(triggeringCommand), _root.Add(triggeringCommand, triggeringControlTag), triggeringCommand, _disableExpressionHandlersWhileCommandsArePending, _disableHandlersWhileInvoked, _cachedControllerManager, this);
        }

        public HandlerWrapper Add(Command triggeringCommand, Predicate<ControlBase> ifControlMatches, HandlerScope scope)
        {
            var x = Add(triggeringCommand, ifControlMatches);
            x.Scope = scope;
            return x;
        }

        public HandlerWrapper Add(Command triggeringCommand, Predicate<ControlBase> ifControlMatches)
        {
            return new HandlerWrapper(_levelProvider, GetStringForCommand(triggeringCommand), _root.Add(triggeringCommand, ifControlMatches), triggeringCommand, _disableExpressionHandlersWhileCommandsArePending, _disableHandlersWhileInvoked, _cachedControllerManager, this);
        }

        public DataBaseErrorHandlerWrapper AddDatabaseErrorHandler(DatabaseErrorType triggeringError, HandlerScope scope)
        {
            var x = AddDatabaseErrorHandler(triggeringError);
            x.Scope = scope;
            return x;
        }

        public DataBaseErrorHandlerWrapper AddDatabaseErrorHandler(DatabaseErrorType triggeringError)
        {
            return new DataBaseErrorHandlerWrapper(_levelProvider, "HE_" + triggeringError, _root, triggeringError);
        }

        public int Count
        {
            get { return _root.Count; }
        }

        public static event Action<object> BeforeHandler;
        public class DataBaseErrorHandlerWrapper
        {
            List<DatabaseErrorHandler> _root = new List<DatabaseErrorHandler>();
            LevelProvider _levelProvider;
            string _levelString;

            static ContextStatic<int> _errorHandlerStackCount = new ContextStatic<int>(() => 0);

            internal DataBaseErrorHandlerWrapper(LevelProvider levelProvider, string levelString, Firefly.Box.Advanced.HandlerCollection handlerCollection, DatabaseErrorType errorType)
            {
                _root.Add(handlerCollection.AddDatabaseErrorHandler(errorType));
                if (errorType == DatabaseErrorType.UnknownError)
                    _root.Add(handlerCollection.AddDatabaseErrorHandler(DatabaseErrorType.FailedToInitializeEntity));
                _levelProvider = levelProvider;
                _levelString = levelString;
                DatabaseErrorEventHandler whatToDo =
                 e =>
                {

                    if (e.ErrorType == DatabaseErrorType.DuplicateIndex)
                    {
                        if (e.Exception == null)
                            return;
                        var ex = e.Exception as DatabaseErrorException;
                        if (ex != null && ex.InnerException == null)
                            return;


                    }
                    if (e.ErrorType == DatabaseErrorType.UnknownError &&
                       e.HandlingStrategy == DatabaseErrorHandlingStrategy.AbortAllTasks
                       && !(e.Entity is BtrieveEntity))
                    {
                        e.HandlingStrategy = DatabaseErrorHandlingStrategy.Rollback;
                    }


                    using (_levelProvider.StartContext(_levelString))
                    {
                        using (ENV.Utilities.Profiler.StartContext("Exception " + (e.Exception != null ? e.Exception.Message : "") + Common.WriteDatabaseErrorToLog(e, true)))
                        {
                            if (Invokes != null)
                            {
                                if (BeforeHandler != null)
                                    BeforeHandler(this);
                                if (_errorHandlerStackCount.Value < 3)
                                {
                                    try
                                    {
                                        _errorHandlerStackCount.Value++;
                                        Common._wasThereADatabaseErrorHander.Value = true;

                                        Context.Current.DiscardPendingCommandsIfOnUIThread();
                                        UserMethods.SetCurrentHandledDatabaseError(e);
                                        CallInvokes(e);
                                        if (e.HandlingStrategy == DatabaseErrorHandlingStrategy.Retry && e.ErrorType != DatabaseErrorType.LockedRow)
                                            if (Firefly.Box.Context.Current.ActiveTasks.Count <= 1)
                                                e.HandlingStrategy = DatabaseErrorHandlingStrategy.Throw;
                                        UserMethods.SetCurrentHandledDatabaseError(null);
                                    }
                                    catch
                                    {
                                        e.Handled = true;
                                        throw;
                                    }
                                    finally
                                    {
                                        _errorHandlerStackCount.Value--;
                                    };
                                }
                                else
                                {
                                    using (ENV.Utilities.Profiler.StartContext("Database Error Stack Overflow"))
                                    {
                                        throw new InvalidOperationException("Database Error Handler within Database Error Handler within Database Error Handler");
                                    }
                                }
                            }
                        }
                    }
                    if (e.ErrorType == DatabaseErrorType.FailedToInitializeEntity &&
                 e.HandlingStrategy == DatabaseErrorHandlingStrategy.AbortAllTasks)
                    {
                        e.HandlingStrategy = DatabaseErrorHandlingStrategy.Rollback;
                        var at = Firefly.Box.Context.Current.ActiveTasks;
                        if (at.Count > 0)
                        {
                            var t = at[at.Count - 1];
                            ControllerBase.SendInstanceBasedOnTaskAndCallStack(t,
                                c =>
                                {
                                    c.ThrowRollbackExcpetionAfterExecute();
                                });
                        }

                    }
                };
                _root.ForEach(x => x.Invokes += whatToDo);
            }
            void CallInvokes(DatabaseErrorEventArgs e)
            {
                Invokes(e);
            }

            public HandlerScope Scope
            {
                get { return _root[0].Scope; }
                set { _root.ForEach(x => x.Scope = value); }
            }

            public void BindEnabled(Func<bool> condition)
            {
                _root.ForEach(x => x.BindEnabled(condition));
            }

            public event DatabaseErrorEventHandler Invokes;




        }


        List<HandlerWrapper> _handlers = new List<HandlerWrapper>();
        public class HandlerWrapper
        {
            Handler _root;
            LevelProvider _levelProvider;
            string _levelString;
            bool _isTemplate;
            bool _isUIControllerCustomCommand;
            bool _isExpression;
            Func<bool> _disableExpressionHandlersWhileCommandsArePending;
            Func<bool> _disableHandlersWhileInvoked;
            bool _isBeingInvoked = false;
            CachedControllerManager _cachedControllerManager;
            Func<ProgramCollection> _progs;
            internal HandlerWrapper(LevelProvider levelProvider, string levelString, Handler root, Command triggeringCommand, Func<bool> disableExpressionHandlersWhileCommandsArePending,
                Func<bool> disableHandlersWhileInvoked, CachedControllerManager cachedControllerManager, HandlerCollectionWrapper parent)
            {
                _progs = parent._progs;
                parent._handlers.Add(this);
                _cachedControllerManager = cachedControllerManager;
                _root = root;
                _isTemplate = triggeringCommand is TemplateModeCommand;
                _isUIControllerCustomCommand = triggeringCommand is UIControllerCustomCommand;
                if (!_isTemplate)
                    _root.BindEnabled(() => !Commands.TemplateExit.Enabled);
                _isExpression = levelString == "HE";

                _levelProvider = levelProvider;
                _levelString = levelString;
                _disableExpressionHandlersWhileCommandsArePending = disableExpressionHandlersWhileCommandsArePending;
                _disableHandlersWhileInvoked = disableHandlersWhileInvoked;
                _root.Invokes += e =>
                {
                    if (_disableHandlersWhileInvoked() && _isBeingInvoked) return;

                    if (_isTemplate)
                    {
                        if (!Commands.TemplateExit.Enabled)
                            return;
                    }
                    else
                        if (Commands.TemplateExit.Enabled)
                        return;

                    if (_isExpression && _disableExpressionHandlersWhileCommandsArePending() && Context.Current.CommandsPending) return;

                    Action invoke =
                        () =>
                        {
                            using (_levelProvider.StartContext(_levelString))
                            {
                                _isBeingInvoked = true;
                                using (new CurrentHandlerStackScope())
                                {
                                    CurrentHandler.Value = _isUIControllerCustomCommand ? null : this;
                                    _throwException = null;
                                    try
                                    {
                                        if (Invokes != null)
                                        {
                                            if (BeforeHandler != null)
                                                BeforeHandler(this);
                                            if (UserSettings.Version8Compatible && !_isUIControllerCustomCommand)
                                                ControllerBase._runAsInvokeV8.Value = parent._invokeInController;
                                            CallInvokes(e);
                                            if (_throwException != null)
                                                throw _throwException;
                                        }
                                    }
                                    finally
                                    {
                                        if (UserSettings.Version8Compatible)
                                            ControllerBase._runAsInvokeV8.Value = null;
                                        _isBeingInvoked = false;
                                    }
                                }
                            }
                        };

                    if (UserSettings.AsyncComEvents && _levelString.StartsWith("HX_"))
                        Context.Current.BeginInvoke(invoke);
                    else
                        invoke();
                };
            }
            void CallInvokes(HandlerInvokeEventArgs e)
            {
                Invokes(e);
            }

            class CurrentHandlerStackScope : IDisposable
            {
                HandlerWrapper _prev;

                public CurrentHandlerStackScope()
                {
                    _prev = CurrentHandler.Value;
                }

                public void Dispose()
                {
                    CurrentHandler.Value = _prev;
                }
            }
            public static IDisposable StartTaskScope()
            {
                var x = new CurrentHandlerStackScope();
                CurrentHandler.Value = null;
                return x;

            }


            static ContextStatic<HandlerWrapper> CurrentHandler = new ContextStatic<HandlerWrapper>(() => null);
            FlowAbortException _throwException;

            internal static void ThrowFlowAbortException(FlowAbortException exception)
            {
                if (CurrentHandler.Value != null && !CurrentHandler.Value._levelProvider.IsTheTopGenerationController() && ENV.UserMethods.Instance.MainLevel(0) == "RM")
                {
                    CurrentHandler.Value._throwException = exception;
                    return;
                }
                throw exception;
            }

            public HandlerScope Scope
            {
                get { return _root.Scope; }
                set { _root.Scope = value; }
            }

            public void BindEnabled(Func<bool> condition)
            {
                _root.BindEnabled(() =>
                                  {
                                      if (Commands.TemplateExit.Enabled)
                                          return false;
                                      bool result = false;
                                      bool done = false;
                                      var at = Firefly.Box.Context.Current.ActiveTasks;
                                      if (at.Count > 0)
                                      {
                                          ControllerBase.SendInstanceBasedOnTaskAndCallStack(at[at.Count - 1],
                                              cb =>
                                              {
                                                  cb._myNullStrategy.OverrideAndCalculate(() =>
                                                                                          {
                                                                                              done = true;
                                                                                              result = condition();
                                                                                          });
                                              });
                                      }
                                      if (done)
                                          return result;
                                      return condition();
                                  });
            }

            public event HandlerInvokeHandler Invokes;


            public ColumnCollection Parameters
            {
                get { return _root.Parameters; }
            }

            public void RegisterCallByPublicName(Func<Text> publicName)
            {
                var c = _progs().GetCachedByPublicName(publicName, null);
                _root.RegisterCalledTask(c);
            }
            public void RegisterCallByPublicName(Func<Text> publicName, Func<bool> condition)
            {
                var c = _progs().GetCachedByPublicName(publicName, null);
                _root.RegisterCalledTask(c, condition);
            }
            public void RegisterCallByIndex(Func<Number> programNumber)
            {
                var c = _progs().GetCachedByIndex(programNumber, null);
                _root.RegisterCalledTask(c);
            }
            public void RegisterCallByIndex(Func<Number> programNumber, Func<bool> condition)
            {
                var c = _progs().GetCachedByIndex(programNumber, null);
                _root.RegisterCalledTask(c, condition);
            }

            public void RegisterCached<T>() where T : class
            {
                _root.RegisterCalledTask((CachedTask<T>)_cachedControllerManager.GetTheCachedController<T>());
            }
            public void RegisterCached<T>(Func<bool> condition) where T : class
            {
                _root.RegisterCalledTask((CachedTask<T>)_cachedControllerManager.GetTheCachedController<T>(), condition);
            }

            public void RegisterCalledTask<taskType>(CachedTask<taskType> cachedTaskBase) where taskType : class
            {
                _root.RegisterCalledTask(cachedTaskBase);
            }
            public void RegisterCalledTask<taskType>(CachedTask<taskType> cachedTaskBase, Func<bool> condition) where taskType : class
            {
                _root.RegisterCalledTask(cachedTaskBase, condition);
            }
        }

        internal void SendColumnsTo(Action<ColumnBase> add)
        {
            foreach (var handlerWrapper in _handlers)
            {
                foreach (var c in handlerWrapper.Parameters)
                {
                    if (c is UserMethods.NotIncludedInVarIndexCalculations)
                        continue;
                    add(c);
                }
            }
        }

        public static string FixCustomCommandKey(string key)
        {
            if (key == null)
                return null;
            if (UserSettings.Version10Compatible)
            {
                key = key.TrimEnd();
            }
            return key;
        }

    }

}
