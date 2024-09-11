using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.Protocols;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ENV.Data;
using ENV.Data.DataProvider;
using ENV.Security;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.Printing;
using Firefly.Box.Printing.Advanced;
using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;
using BoolColumn = Firefly.Box.Data.BoolColumn;
using DateColumn = Firefly.Box.Data.DateColumn;
using Form = System.Windows.Forms.Form;
using NumberColumn = ENV.Data.NumberColumn;
using NumberFormatInfo = Firefly.Box.Data.Advanced.NumberFormatInfo;
using TextColumn = Firefly.Box.Data.TextColumn;
using TimeColumn = Firefly.Box.Data.TimeColumn;
using ENV.Labs;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using ENV.Advanced;

namespace ENV
{
    public static class Common
    {
        static System.Windows.Forms.Form RootMDI = null;
        static Firefly.Box.Context _rootMdiContext;
        internal static bool IsRootMdiContext { get { return Firefly.Box.Context.Current == _rootMdiContext; } }
        internal static void SetRootMDI(Form mdiForm)
        {
            RootMDI = mdiForm;
            _rootMdiContext = Firefly.Box.Context.Current;
            if (RootMDI != null)
            {
                if (ShowSpecificMDIText && !string.IsNullOrEmpty(SpecificMDIText))
                    RootMDI.Text = SpecificMDIText;
                else
                    RootMDI.Text = ApplicationTitle;
            }
            if (RootMDIChanged != null)
                RootMDIChanged(RootMDI);
        }

        public static event Action<Form> RootMDIChanged;

        internal static bool ShowSpecificMDIText = false;
        internal static string SpecificMDIText = null;

        static ContextStatic<System.Windows.Forms.Form> _callingContextTopMostForm = new ContextStatic<Form>(() => null);
        static ContextStatic<System.Windows.Forms.Form> _contextTopMostForm = new ContextStatic<Form>(() => null);

        internal static System.Windows.Forms.Form ContextTopMostForm
        {
            get
            {
                var x = _contextTopMostForm.Value;
                if (x != null && !x.IsDisposed)
                    return x;
                x = _callingContextTopMostForm.Value;
                if (x != null && !x.IsDisposed)
                    return x;
                return RootMDI;
            }
        }

        internal static void SetContextTopMostFormIfNull(System.Windows.Forms.Form form)
        {
            if (_contextTopMostForm.Value != null && !_contextTopMostForm.Value.IsDisposed)
                return;
            _contextTopMostForm.Value = form;
            ENV.MenuManager.DoOnMenuManagers(m => m.SetCurrentStateAsDefault());
            ENV.MenuManager.DoOnMenuManagers(m => m.ContextState.Apply());

        }

        internal static bool ContextTopMostFormWasSet { get { return _contextTopMostForm.Value != null && !_contextTopMostForm.Value.IsDisposed; } }

        static BoundStatusBar _mainStatusBar;
        static BoundStatusBar StatusBar
        {
            get
            {
                var sb = Context.Current["BoundStatusBar"] as BoundStatusBar;
                return sb ?? _mainStatusBar ?? new BoundStatusBar();
            }
        }

        internal static event Action<string> StatusTextChanged
        {
            add { StatusBar.StatusTextChanged += value; }
            remove { StatusBar.StatusTextChanged -= value; }
        }

        public static bool ShowDatabaseErrorMessage = true;
        internal static ContextStatic<bool> _wasThereADatabaseErrorHander = new ContextStatic<bool>();


        static string _logFileName = null;
        public static bool LockLogFileName { get; set; }

        public static string LogFileName
        {
            get
            {
                if (_logFileName == null)
                    return null;
                if (_logFileName.Contains("<D>"))
                    return _logFileName.Replace("<D>", Date.Now.ToString("YYYY_MM_DD"));
                return _logFileName;
            }
            set
            {
                if (LockLogFileName)
                    return;
                _logFileName = value;
            }
        }

        public static ContextAndSharedValue<string> TraceFileName = new ContextAndSharedValue<string>(null);
        public static bool IncludeProcessAndThreadInTraceFileName { get; set; }
        public static bool IncludeProcessDateAndTimeInTraceFileName { get; set; }
        public static bool IncludeAppNameInTraceFileName { get; set; }

        public static string GetShortStackTrace()
        {
            string reuslt = "";
            bool done = false;

            Common.RunOnContextTopMostForm(y =>
            {
                if (y.ActiveControl != null)
                {
                    done = true;
                    Common.RunOnLogicContext(y.ActiveControl, () => reuslt = Environment.StackTrace);
                }

            });
            if (!done)
                reuslt = Environment.StackTrace;
            return GetShortStackTrace(reuslt);
        }

        public static string GetShortStackTrace(string stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace))
                return null;
            string stack = "";
            using (var st = new System.IO.StringReader(stackTrace))
            {
                string l;
                while ((l = st.ReadLine()) != null)
                {
                    bool c = true;
                    foreach (var s in new string[] { " System.", " Microsoft.", " WizardOfOz.", " Firefly.", " ENV.", "Oracle.", "OracleInternal.", "GECapital.JobQueue.ISeriesFunctionJobQueuePlugin" })
                    {
                        if (l.Contains(s))
                        {
                            c = false;
                            break;
                        }
                    }
                    if (c)
                        stack += l + "\r\n";

                }
            }
            if (stack.EndsWith("\r\n"))
                stack = stack.Remove(stack.Length - 2);
            return stack;
        }
        internal static bool DisableWaitOnRetry = false;
        public static int RetryTimeoutInSeconds { get; set; }
        internal static void DatabaseErrorOccurred(DatabaseErrorEventArgs e, RelationCollection relations, LockingStrategy lockingStrategy)
        {

            string info = "";

            using (ENV.Utilities.Profiler.StartContext("Database Error " + e.ErrorType.ToString() + " HandlingStrategy:" + e.HandlingStrategy))
            {
                if (e.Exception != null)
                {
                    var ex = e.Exception;
                    if (ex.InnerException != null)
                        ex = ex.InnerException;
                    info = WriteDatabaseErrorToLog(e, e.HandlingStrategy == DatabaseErrorHandlingStrategy.Ignore);
                    Profiler.WriteToLogFile(ex, info, new object[0]);
                }
            }

            {
                var sqle = e.Entity as DynamicSQLEntity;
                if (sqle != null && sqle.SuccessColumn != null)
                    ShowDatabaseErrorMessage = false;
            }


            UserMethods.SetCurrentHandledDatabaseError(null);
            bool nonSqlDatabase = false;
            var x = e.Entity as ENV.Data.Entity;
            if (x != null)
            {
                try
                {
                    if (!x.DataProvider.SupportsTransactions)
                        nonSqlDatabase = true;
                }
                catch
                {
                }
            }
            {
                DenyRollbackAndRecoverForBusinessProcessWhenHandlerWasExecuted(e);
                if (e.ErrorType == DatabaseErrorType.DuplicateIndex && e.HandlingStrategy == DatabaseErrorHandlingStrategy.Retry)
                {
                    var activeControllers = Firefly.Box.Context.Current.ActiveTasks;
                    if (activeControllers[activeControllers.Count - 1] is BusinessProcess)
                        SetRollbackOrRecoverBasedOnStack(e);
                }
                if (e.ErrorType == DatabaseErrorType.FailedToInitializeEntity && e.Entity is DynamicSQLEntity && e.HandlingStrategy == DatabaseErrorHandlingStrategy.Ignore)
                    e.HandlingStrategy = DatabaseErrorHandlingStrategy.Rollback;
                else if (e.ErrorType == DatabaseErrorType.FailedToInitializeEntity && e.HandlingStrategy == DatabaseErrorHandlingStrategy.Throw)
                    e.HandlingStrategy = DatabaseErrorHandlingStrategy.Rollback;
                if (e.ErrorType == DatabaseErrorType.LockedRow &&
                    e.HandlingStrategy == DatabaseErrorHandlingStrategy.Ignore && !nonSqlDatabase)
                {
                    if (lockingStrategy == LockingStrategy.OnRowLoading)
                    {
                        e.HandlingStrategy = DatabaseErrorHandlingStrategy.Retry;
                        if (RetryTimeoutReached(e))
                        {
                            SetRollbackOrRecoverBasedOnStack(e);

                        }
                        else
                        {
                            Context.Current.Suspend(500);
                            return;
                        }
                    }
                    else
                    {
                        SetRollbackOrRecoverBasedOnStack(e);

                        return;
                    }
                }
                if (e.HandlingStrategy == DatabaseErrorHandlingStrategy.Ignore &&
                    e.ErrorType == DatabaseErrorType.UnknownError && !(e.Entity is DynamicSQLEntity))
                    SetRollbackOrRecoverBasedOnStack(e);
                if (nonSqlDatabase && e.ErrorType == DatabaseErrorType.RowDoesNotExist)
                    e.HandlingStrategy = DatabaseErrorHandlingStrategy.Ignore;

                if (!e.Handled || e.ErrorType == DatabaseErrorType.LockedRow || e.ErrorType == DatabaseErrorType.ReadOnlyEntityUpdate)
                    switch (e.ErrorType)
                    {

                        case DatabaseErrorType.DuplicateIndex:
                            if (ShowDatabaseErrorMessage)
                            {
                                if (!_suppressDialogForTesting && !Message.DisableBeep)
                                    SystemSounds.Beep.Play();
                                SetTemporaryStatusMessage(LocalizationInfo.Current.DuplicateIndex + " " + e.Entity.ToString());
                            }
                            break;
                        case DatabaseErrorType.NullInOnlyOnePartOfDateTimePair:
                            {
                                if (ShowDatabaseErrorMessage)
                                {
                                    if (!_suppressDialogForTesting && !Message.DisableBeep)
                                        SystemSounds.Beep.Play();
                                    SetTemporaryStatusMessage(LocalizationInfo.Current.NullInOnlyOnePartOfDateTimePair + " " + e.Entity.ToString());
                                }
                                break;
                            }
                        case DatabaseErrorType.RowDoesNotExist:
                            if (ShowDatabaseErrorMessage && e.HandlingStrategy != DatabaseErrorHandlingStrategy.Ignore)
                                SetTemporaryStatusMessage(LocalizationInfo.Current.RowDoesNotExist + " " + e.Entity.ToString());
                            break;
                        case DatabaseErrorType.UpdatedRowWasChangedSinceLoaded:
                            if (ShowDatabaseErrorMessage)
                                SetTemporaryStatusMessage(LocalizationInfo.Current.RowWasChanged + " " + e.Entity.ToString());
                            break;
                        case DatabaseErrorType.RowWasChangedSinceLoaded:
                            if (ShowDatabaseErrorMessage)
                                SetTemporaryStatusMessage(LocalizationInfo.Current.RowWasChanged + " " + e.Entity.ToString());
                            break;
                        case DatabaseErrorType.ReadOnlyEntityUpdate:
                            if (ShowDatabaseErrorMessage)
                                SetTemporaryStatusMessage(LocalizationInfo.Current.ReadOnlyEntityUpdate + " " + e.Entity.ToString());
                            break;

                        case DatabaseErrorType.LockedRow:

                            if (ShowDatabaseErrorMessage)
                            {
                                SetTemporaryStatusMessage(LocalizationInfo.Current.LockedRow + " " + e.Entity.ToString());
                                if (!_suppressDialogForTesting && !Message.DisableBeep)
                                    SystemSounds.Beep.Play();
                            }
                            break;

                        case DatabaseErrorType.TransactionRolledBack:
                            if (ShowDatabaseErrorMessage)
                            {
                                SetTemporaryStatusMessage(LocalizationInfo.Current.TransactionRolledBack);
                                if (!_suppressDialogForTesting && !Message.DisableBeep)
                                    SystemSounds.Beep.Play();
                            }
                            break;

                        default:
                            if (DisplayDatabaseErrors && ShowDatabaseErrorMessage || AllwaysShowDBErrors)
                            {
                                string s = "";
                                if (e.Entity != null)
                                    s = e.Entity.ToString();
                                SetTemporaryStatusMessage(LocalizationInfo.Current.ErrorOpeningTable + " " + e.Exception.Message + " " + s);
                                if ((((!(e.Entity is BtrieveEntity) || !((BtrieveEntity)e.Entity).IsUsingBtrieve) && !(e.Entity is XmlEntity)) || AllwaysShowDBErrors) || e.HandlingStrategy == DatabaseErrorHandlingStrategy.Throw)
                                    ShowExceptionDialog(e.Exception, e.HandlingStrategy != DatabaseErrorHandlingStrategy.Throw, info);
                            }
                            break;
                    }
                else if (DisplayDatabaseErrors && ShowDatabaseErrorMessage || AllwaysShowDBErrors)
                {
                    switch (e.HandlingStrategy)
                    {
                        case DatabaseErrorHandlingStrategy.Retry:
                            break;
                        default:
                            switch (e.ErrorType)
                            {
                                case DatabaseErrorType.FailedToInitializeEntity:
                                case DatabaseErrorType.ConstraintFailed://W7503
                                case DatabaseErrorType.InvalidSQLStatement:
                                case DatabaseErrorType.UnknownError:
                                case DatabaseErrorType.DataChangeFailed:
                                    string s = "";
                                    if (e.Entity != null)
                                        s = e.Entity.ToString();
                                    SetTemporaryStatusMessage(LocalizationInfo.Current.ErrorOpeningTable + " " + e.Exception.Message + " " + s);
                                    if (((!(e.Entity is BtrieveEntity) && !(e.Entity is XmlEntity)) || AllwaysShowDBErrors) || e.HandlingStrategy == DatabaseErrorHandlingStrategy.Throw)
                                        ShowExceptionDialog(e.Exception, e.HandlingStrategy != DatabaseErrorHandlingStrategy.Throw, info);
                                    break;
                                case DatabaseErrorType.DuplicateIndex:
                                    if (ShowDatabaseErrorMessage)
                                    {
                                        if (!_suppressDialogForTesting && !Message.DisableBeep)
                                            SystemSounds.Beep.Play();
                                        SetTemporaryStatusMessage(LocalizationInfo.Current.DuplicateIndex + " " + e.Entity.ToString());
                                    }
                                    break;
                            }
                            break;
                    }

                }


                if (e.HandlingStrategy == DatabaseErrorHandlingStrategy.Retry)
                {
                    var allowAbort = true;
                    var curr = Context.Current.ActiveTasks[Context.Current.ActiveTasks.Count - 1] as BusinessProcess;
                    if (curr != null && !curr.AllowUserAbort)
                        allowAbort = false;
                    if (RetryTimeoutReached(e) && allowAbort)
                        SetRollbackOrRecoverBasedOnStack(e);
                    else
                    {
                        Keys key = Keys.None;
                        bool first = true;
                        while ((key & Keys.Control) == Keys.Control || first)
                        {
                            first = false;
                            if (!DisableWaitOnRetry)
                                key = Context.Current.Suspend(2000);
                            SetTemporaryStatusMessage("");
                            if (allowAbort)
                            {
                                if (key == Keys.Escape || key == Command.UndoChangesInRow.Shortcut)
                                {
                                    SetRollbackOrRecoverBasedOnStack(e);
                                    if (key == Keys.Escape && e.Entity is BtrieveEntity)
                                        e.HandlingStrategy = DatabaseErrorHandlingStrategy.AbortTask;
                                    Context.Current.DiscardPendingCommands();
                                    break;
                                }
                                if (key == Keys.Up || key == Keys.Down || key == Keys.PageUp || key == Keys.PageDown)
                                {
                                    SetRollbackOrRecoverBasedOnStack(e);
                                    break;
                                }
                                if ((key & Keys.Control) != Keys.Control)
                                    break;
                            }

                        }
                    }
                }
            }
        }
        public static bool CanShowDeveloperTools()
        {
            if (EnableDeveloperTools)
                return true;
            if (ShouldAllowDeveloperToolsForCurrentUser == null)
                return false;
            return ShouldAllowDeveloperToolsForCurrentUser();
        }
        public static void WriteCurrentDatabaseErrorToLog()
        {
            var x = UserMethods._currentDatabaseError;
            if (x != null)
                WriteDatabaseErrorToLog(x, false);
        }

        public static string WriteDatabaseErrorToLog(DatabaseErrorEventArgs e, bool forceNotWriteToFile)
        {
            string info = "";
            var ex = e.Exception;
            if (ex.InnerException != null)
                ex = ex.InnerException;
            using (var r = new StringWriter())
            {
                if (e.Entity != null)
                {
                    if (!(e.Entity is DynamicSQLEntity))
                        r.WriteLine("Entity: {0} ({1}), {2}", e.Entity.Caption,
                            e.Entity.EntityName, e.Entity.GetType().FullName);
                    else
                        r.WriteLine("DynamicSQLEntity: {0} \n\n{1}", e.Entity.EntityName,
                            e.Entity.GetType().FullName);
                }

                if (ex.Data.Contains("SQL"))
                {
                    var y = ex.Data["SQL"] as string;
                    if (y != null)
                    {
                        r.WriteLine();
                        r.WriteLine("SQL:");
                        r.WriteLine(Regex.Replace(y, @"\r\n?|\n", "\r\n"));
                    }
                }
                info = r.ToString();
            }
            if (ShowDatabaseErrorMessage && !forceNotWriteToFile)
                ErrorLog.WriteToLogFile(ex, info);
            return info;
        }

        static bool RetryTimeoutReached(DatabaseErrorEventArgs e)
        {
            if (RetryTimeoutInSeconds <= 0)
                return false;
            return (DateTime.Now - e.OperationTime).TotalSeconds > RetryTimeoutInSeconds;
        }
        public static bool DisplayDatabaseErrors = true;
        public static void PreviewDatabaseError(DatabaseErrorEventArgs e)
        {
            ShowDatabaseErrorMessage = true;
            _wasThereADatabaseErrorHander.Value = false;

            switch (e.ErrorType)
            {
                case DatabaseErrorType.DuplicateIndex:
                case DatabaseErrorType.ConstraintFailed:
                case DatabaseErrorType.NullInOnlyOnePartOfDateTimePair:
                case DatabaseErrorType.DataChangeFailed:
                    if ((e.Entity is DynamicSQLEntity))
                    {
                        e.HandlingStrategy = DatabaseErrorHandlingStrategy.Rollback;
                    }
                    else SetRollbackOrRecoverBasedOnStack(e);
                    break;
                case DatabaseErrorType.LockedRow:
                    e.HandlingStrategy = DatabaseErrorHandlingStrategy.Retry;
                    break;
                case DatabaseErrorType.Deadlock:
                case DatabaseErrorType.ReadOnlyEntityUpdate:
                    e.HandlingStrategy = DatabaseErrorHandlingStrategy.Rollback;
                    break;

                case DatabaseErrorType.UpdatedRowWasChangedSinceLoaded:
                    if (e.Entity is BtrieveEntity)
                        e.HandlingStrategy = DatabaseErrorHandlingStrategy.Ignore;
                    else
                        e.HandlingStrategy = DatabaseErrorHandlingStrategy.Rollback;
                    break;

                case DatabaseErrorType.UnknownError:
                    if (e.HandlingStrategy == DatabaseErrorHandlingStrategy.Throw && !(e.Entity is BtrieveEntity))
                        e.HandlingStrategy = DatabaseErrorHandlingStrategy.AbortAllTasks;
                    break;
                default:
                    if (!(e.Entity is DynamicSQLEntity))
                    {
                        if (e.HandlingStrategy == DatabaseErrorHandlingStrategy.RollbackAndRecover ||
                            e.HandlingStrategy == DatabaseErrorHandlingStrategy.Rollback)
                            SetRollbackOrRecoverBasedOnStack(e);
                    }
                    break;
            }
            UserMethods.SetCurrentHandledDatabaseError(e);
        }

        internal static Dictionary<ITask, bool> _databaseErrorRetryValuesForTesting = new Dictionary<ITask, bool>();
        internal static bool InDatabaseErrorRetry(ITask c)
        {
            bool x;
            if (_databaseErrorRetryValuesForTesting.TryGetValue(c, out x))
                return x;
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(c, cb => x = cb.OnDatabaseErrorRetry);
            return x;
        }



        internal static void SetRollbackOrRecoverBasedOnStack(DatabaseErrorEventArgs e)
        {
            ITask c = GetTransactionOpenController();
            if (c != null)
            {
                e.HandlingStrategy = InDatabaseErrorRetry(c)
                    ? DatabaseErrorHandlingStrategy.RollbackAndRecover
                    : DatabaseErrorHandlingStrategy.Rollback;
                if (!c.InTransaction && e.HandlingStrategy == DatabaseErrorHandlingStrategy.Rollback)
                    ControllerBase.SendInstanceBasedOnTaskAndCallStack(c,
                        cb =>
                        {
                            var bpb = cb as BusinessProcessBase;
                            if (bpb != null && bpb.OnRollbackWithoutTransactionAbortTask)
                                e.HandlingStrategy = DatabaseErrorHandlingStrategy.AbortTask;
                        });
            }
        }

        internal static ITask GetTransactionOpenController()
        {
            var t = Firefly.Box.Context.Current.ActiveTasks;
            ITask c = null;
            for (int i = 0; i < t.Count; i++)
            {
                c = t[i];
                if (c.InTransaction)
                {
                    break;
                }
            }

            return c;
        }

        internal static void DenyRollbackAndRecoverForBusinessProcessWhenHandlerWasExecuted(DatabaseErrorEventArgs e)
        {
            if (e.HandlingStrategy != DatabaseErrorHandlingStrategy.RollbackAndRecover)
                return;
            if (!_wasThereADatabaseErrorHander.Value)
                return;
            var t = Firefly.Box.Context.Current.ActiveTasks;
            ITask c = null;
            for (int i = 0; i < t.Count; i++)
            {
                c = t[i];
                if (c.InTransaction)
                {
                    break;
                }
            }
            if (c != null)
            {
                if (c is BusinessProcess)
                    e.HandlingStrategy = DatabaseErrorHandlingStrategy.Rollback;
            }
        }

        internal static bool _suppressDialogForTesting = !Environment.UserInteractive;
        public static void SuppressDialogs()
        {
            _suppressDialogForTesting = true;
        }

        public static void ShowExceptionDialog(Exception e)
        {
            ShowExceptionDialog(e, "");
        }
        static Action<Exception, string, string[]> _callMeWhenShowingExceptionDialogWithoutContinue = delegate { };
        internal static void CallThisWhenShowingExceptionDialogWithoutContinue(Action<Exception, string, string[]> a)
        {
            _callMeWhenShowingExceptionDialogWithoutContinue = a;
        }
        public static void ShowExceptionDialog(Exception e, string aditionalInfo, params string[] args)
        {
            ShowExceptionDialog(e, false, aditionalInfo, args);
        }
        internal static bool _FORTESTINGONLYdisplayedExceptionDialog = false;

        public static void ShowExceptionDialog(Exception e, bool allowContinue, string aditionalInfo, params string[] args)
        {
            if (!allowContinue)
                _callMeWhenShowingExceptionDialogWithoutContinue(e, aditionalInfo, args);
            UI.SplashScreen.Stop();
            _FORTESTINGONLYdisplayedExceptionDialog = true;
            if (_suppressDialogForTesting)
                return;

            //ThreadExceptionDialog d = new ThreadExceptionDialog(e);
            var d = new UI.ExceptionDialog(e, aditionalInfo, args);
            d.AllowContinue = allowContinue;
            var result = d.ShowDialog();
            if (allowContinue)
            {
            }
            else
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            // if (d.DialogResult == System.Windows.Forms.DialogResult.Abort)

        }

        internal static void SetCurrentContextAsRoot()
        {
            _rootMdiContext = Context.Current;
        }

        static bool _enableDeveloperTools;
        public static Func<bool> ShouldAllowDeveloperToolsForCurrentUser = null;
        public static bool EnableDeveloperTools
        {
            get { return _enableDeveloperTools; }
            set
            {
                _enableDeveloperTools = value;
                UserMethods.SetRunMode(0);
                if (_enableDeveloperTools)
                {
                    UserMethods.SetRunMode(Number.Parse(UserMethods.Instance.IniGet("RUNMODE")));
                }
            }
        }

        internal static void SetActivityText(string activityText)
        {
            StatusBar.SetActivityText(activityText);
        }

        public static void SetDefaultStatusText(string text)
        {
            StatusBar.SetDefaultStatusText(text);
        }

        public static void PushStatusText(string text)
        {
            StatusBar.PushStatusText(text);
        }
        public static void PopStatusText()
        {
            StatusBar.PopStatusText();
        }
        public static void ResetStatusText()
        {
            StatusBar.ResetStatusText();
        }
        static string _applicationTitle;
        public static string ApplicationTitle
        {
            get
            {
                if (HebrewTextTools.V8HebrewOem)
                    return _applicationTitle;
                return ENV.PathDecoder.DecodePath(_applicationTitle);
            }
            set { _applicationTitle = value; }
        }
        public static void BindStatusBar(ToolStripStatusLabel mainStatusLabel, ToolStripStatusLabel userStatusLabel, ToolStripStatusLabel activityStatusLabel, ToolStripStatusLabel expandStatusLabel, ToolStripStatusLabel expandTextBoxStatusLabel, ToolStripStatusLabel insertOverwriteIndicator, ToolStripStatusLabel versionIndicator)
        {
            _mainStatusBar = new BoundStatusBar(mainStatusLabel, userStatusLabel, activityStatusLabel, expandStatusLabel, expandTextBoxStatusLabel, insertOverwriteIndicator, versionIndicator);
            _mainStatusBar.Bind();

            Command.Expand.EnabledChanged += () => StatusBar.ExpandEnabledChanged();
            Command.ExpandTextBox.EnabledChanged += () => StatusBar.ExpandTextBoxEnabledChanged();
        }

        public static void RunOnLogicContext(System.Windows.Forms.Control ofControl, Action what)
        {
            var f = ofControl == null ? null : ofControl.FindForm();
            if (f == null)
            {
                var cm = ofControl as ContextMenuStrip;
                if (cm != null && cm.SourceControl != null)
                    f = cm.SourceControl.FindForm();
            }
            if (f == null)
            {
                var cm = ofControl as ENV.UI.SuspendedToolStripMenuItem.myDropDown;
                if (cm != null && cm.SourceControl != null)
                    f = cm.SourceControl.FindForm();
            }
            if (f == null)
            {
                var c = ofControl;
                while (c is ToolStripDropDown && ((ToolStripDropDown)c).OwnerItem != null)
                    c = ((ToolStripDropDown)c).OwnerItem.Owner;
                f = c == null ? null : c.FindForm();
            }
            if (f != null)
            {
                var ff = f as Firefly.Box.UI.Form;
                if (ff != null)
                    ff.RunInLogicContext(what);
                else
                {
                    var actionRunned = false;
                    var tlc = f.TopLevelControl;
                    Firefly.Box.UI.Form.RunInActiveLogicContext(() => { if (tlc == ContextTopMostForm) { actionRunned = true; what(); } });
                    if (!actionRunned)
                        RunOnContextTopMostForm(x => what());
                }
            }
        }

        public static void BindSDIStatusBar(Firefly.Box.UI.Form sdiForm, ToolStripStatusLabel mainStatusLabel, ToolStripStatusLabel userStatusLabel, ToolStripStatusLabel activityStatusLabel, ToolStripStatusLabel expandStatusLabel, ToolStripStatusLabel expandTextBoxStatusLabel, ToolStripStatusLabel insertOverwriteIndicator, ToolStripStatusLabel versionIndicator)
        {
            var strip = mainStatusLabel.GetCurrentParent();
            if (strip != null && !strip.Visible) return;
            sdiForm.RunInLogicContext(
                () =>
                {
                    if (sdiForm != ContextTopMostForm) return;
                    var sb = new BoundStatusBar(mainStatusLabel, userStatusLabel, activityStatusLabel, expandStatusLabel, expandTextBoxStatusLabel, insertOverwriteIndicator, versionIndicator);
                    sb.Bind();
                    Context.Current["BoundStatusBar"] = sb;
                });
        }

        internal static void UpdateInsertOverwriteIndicator()
        {
            StatusBar.UpdateInsertOverwriteIndicator();
        }


        internal static string _statusText;
        internal static event Action<string> StatusBarTextLabelTextChanged;

        internal static void SetTemporaryStatusMessage(string text)
        {
            StatusBar.SetTemporaryStatusMessage(text);
        }
        public static Image GetImage(string imageLocation)
        {
            return GetImage(imageLocation, Size.Empty);
        }

        public static Image GetImage(string imageLocation, Size preferredSize)
        {
            if (string.IsNullOrEmpty(imageLocation)) return null;
            string path = PathDecoder.DecodePath(imageLocation);
            try
            {
                return Utilities.ImageLoader.Load(path, preferredSize);
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
            }
            return null;
        }

        internal static void BindForms(Firefly.Box.UI.Form taskForm, Firefly.Box.UI.Form parameterForm)
        {
            if (parameterForm != null && taskForm != null && !taskForm.IsHandleCreated)
            {
                taskForm.ChildWindow = parameterForm.ChildWindow;
                taskForm.Location = parameterForm.Location;
                taskForm.Border = parameterForm.Border;
                taskForm.TitleBar = parameterForm.TitleBar;
                taskForm.MinimizeBox = parameterForm.MinimizeBox;
                taskForm.MaximizeBox = parameterForm.MaximizeBox;
                taskForm.SystemMenu = parameterForm.SystemMenu;
                taskForm.StartPosition = parameterForm.StartPosition;
                if (!UserSettings.Version10Compatible && parameterForm.Location != Point.Empty)
                    taskForm.StartPosition = WindowStartPosition.Custom;
                taskForm.BackColor = parameterForm.BackColor;
                taskForm.BackgroundImage = parameterForm.BackgroundImage;
                taskForm.AdvancedAnchor = parameterForm.AdvancedAnchor;
                taskForm.AutoScroll = true;
                taskForm.FitToMDI = parameterForm.FitToMDI;
                taskForm.BindFitToMDI += (sender, args) => args.Value = parameterForm.FitToMDI;
                taskForm.BindChildWindow += (sender, args) => args.Value = parameterForm.ChildWindow;
                taskForm.BindText += (sender, args) => args.Value = parameterForm.Text;

                var horizontalScale = 1d;
                var verticalScale = 1d;

                Action setTaskFormBounds = () => { };
                setTaskFormBounds = () =>
                {
                    taskForm.Location = new Point((int)(parameterForm.Left * horizontalScale), (int)(parameterForm.Top * verticalScale));
                    taskForm.ClientSize = new Size((int)(parameterForm.ClientSize.Width * horizontalScale), (int)(parameterForm.ClientSize.Height * verticalScale));
                    setTaskFormBounds = () => { };
                };

                var envForm = parameterForm as ENV.UI.Form;
                if (envForm != null)
                {
                    horizontalScale = envForm.ScalingFactorInternal.Width;
                    verticalScale = envForm.ScalingFactorInternal.Height;
                }

                var taskEnvForm = taskForm as ENV.UI.Form;
                if (taskEnvForm != null)
                {
                    if (taskEnvForm.OnBindFormSetClientSizeBeforeAdvancedAnchorLayout)
                        setTaskFormBounds();
                }

                taskForm.AdvancedAnchorLayoutInitialized += () => setTaskFormBounds();

                if (UserSettings.VersionXpaCompatible)
                    taskForm.RemoveAllLocationAndSizeBindings();
            }
        }




        internal static void ErrorInStatusBar(string text)
        {
            if (!_suppressDialogForTesting && !string.IsNullOrEmpty(text) && !Message.DisableBeep)
            {
                SystemSounds.Beep.Play();
                SystemSounds.Beep.Play();
            }
            SetTemporaryStatusWarningOrError(text);
            Advanced.HandlerCollectionWrapper.HandlerWrapper.ThrowFlowAbortException(new FlowAbortException(text, text));
        }

        static void SetTemporaryStatusWarningOrError(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (ENV.UserSettings.Version8Compatible && text != null && text.Length > 80)
                text = text.Substring(0, 80).TrimEnd();
            SetTemporaryStatusMessage(text);
        }

        internal static void WarningInStatusBar(string text)
        {
            if (!_suppressDialogForTesting && !string.IsNullOrEmpty(text) && !Message.DisableBeep)
                SystemSounds.Beep.Play();
            SetTemporaryStatusWarningOrError(text);
        }
        public static DialogResult ShowMessageBox(string title, System.Windows.Forms.MessageBoxIcon icon, string message)
        {
            return ShowMessageBox(title, icon, message, System.Windows.Forms.MessageBoxButtons.OK, MessageBoxDefaultButton.Button1);
        }

        public static void RunOnRootMDI(Action<System.Windows.Forms.Form> command)
        {
            if (RootMDI != null)
                Context.Current.InvokeUICommand(
                    () =>
                    {
                        Action a = () => command(RootMDI);
                        if (RootMDI.InvokeRequired)
                            RootMDI.Invoke(a);
                        else
                            a();
                    });
        }

        public static void RunOnContextTopMostForm(Action<System.Windows.Forms.Form> command)
        {
            var mdi = ContextTopMostForm;
            if (mdi != null)
                Context.Current.InvokeUICommand(() => command(mdi));
        }

        public static DialogResult ShowMessageBox(string title, System.Windows.Forms.MessageBoxIcon icon, string message, MessageBoxButtons messageBoxButtons, MessageBoxDefaultButton messageBoxDefaultButton)
        {
            if (_suppressDialogForTesting)
                return 0;
            if (message != string.Empty)
                return ShowDialog(owner => MessageBox.Show(owner, Languages.Translate(message.TrimEnd()), Languages.Translate(title.TrimEnd()), messageBoxButtons, icon, messageBoxDefaultButton, LocalizationInfo.Current.MessageBoxOptions));
            return 0;
        }
        public static bool ShowYesNoMessageBox(string title, string message, bool defaultYes)
        {
            if (_suppressDialogForTesting)
                return false;
            if (message != string.Empty)
            {
                if (UserSettings.Version8Compatible)
                    return new UI.BackwardCompatibleMessageBox(title, message, defaultYes).ShowDialog();

                return
                    ShowDialog(
                        owner =>
                            System.Windows.Forms.MessageBox.Show(owner, message, title,
                                System.Windows.Forms.MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                defaultYes ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2,
                                LocalizationInfo.Current.MessageBoxOptions)) == DialogResult.Yes;
            }
            return false;
        }
        public static DialogResult ShowYesNoCancelMessageBox(string message, string title)
        {
            if (_suppressDialogForTesting)
                return 0;
            return ShowDialog(
                owner => MessageBox.Show(owner, message, title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, LocalizationInfo.Current.MessageBoxOptions));
        }

        public static DialogResult ShowDialog(Func<Form, DialogResult> showTheDialog)
        {
            Firefly.Box.UI.Form.CompleteUIChanges();
            var owner = ContextTopMostForm;
            if (owner != null && !owner.IsMdiContainer)
            {
                for (var i = Context.Current.ActiveTasks.Count - 1; i >= 0; i--)
                {
                    var t = Context.Current.ActiveTasks[i];
                    if (t.View == null || t.View.IsDisposed || !t.View.Visible) continue;
                    owner = t.View;
                    break;
                }
            }
            var result = DialogResult.None;
            Context.Current.InvokeUICommand(() => result = showTheDialog(owner));
            return result;
        }

        class ValueChangeColumnScope
        {
            public object PreviousValue;
            public int lastChangeReason = -1;

        }
        static readonly Firefly.Box.ContextStatic<Dictionary<ColumnBase, ValueChangeColumnScope>> _scopeOfValueChanges = new ContextStatic<Dictionary<ColumnBase, ValueChangeColumnScope>>(() => new Dictionary<ColumnBase, ValueChangeColumnScope>());

        public class WhenValueChange<DataType>
        {
            public event ValueChangedHandler<DataType> Invokes;

            internal void Run(ValueChangedEventArgs<DataType> args)
            {
                if (Invokes != null)
                    Invokes(args);
            }
        }
        public static bool UseOriginalValueChangePreviousValue = true;
        internal static void MonitorValueChanged<DataType>(TypedColumnBase<DataType> c, ValueChangedHandler<DataType> handler, NumberColumn changeReasonColumn, TypedColumnBase<DataType> previousValueColumn, Action<Action<SubForm>> provideSubForms, Func<bool> condition = null)
        {
            if (provideSubForms == null)
                provideSubForms = delegate { };
            Func<ValueChangedEventArgs<DataType>, bool> fireEventAndReturnFalseIfConditionIsFalse =

                delegate (ValueChangedEventArgs<DataType> e)
                {
                    if (condition != null && !condition())
                        return false;
                    if (_suppressColumnChange)
                        return true;
                    var chgReason = e.ChangedByUser ? 0 : 1;
                    var previousValue = e.PreviousValue;
                    List<Action> after = new List<Action>();
                    ValueChangeColumnScope scope;
                    var dictionary = _scopeOfValueChanges.Value;
                    if (!dictionary.TryGetValue(c, out scope))
                    {
                        scope = new ValueChangeColumnScope();
                        scope.PreviousValue = previousValue;
                        dictionary.Add(c, scope);
                        after.Add(() => dictionary.Remove(c));

                    }
                    if (UseOriginalValueChangePreviousValue)
                        if (scope.lastChangeReason == 0 && chgReason == 1)
                            previousValue = (DataType)scope.PreviousValue;
                    scope.lastChangeReason = chgReason;

                    var y = scope.PreviousValue;
                    var isParameterFirstUpdate = false;
                    var u = ENV.UserMethods.Instance;
                    {
                        var pc = c as ControllerBase.ParameterColumn;
                        if (pc != null)
                        {
                            u = pc._getUserMethods();
                            if (pc._fireOnChangeEventIfEqual)
                            {
                                isParameterFirstUpdate = true;
                                pc._fireOnChangeEventIfEqual = false;
                            }

                        }
                    }
                    try
                    {
                        if (u.Equals(y, c.Value) && !isParameterFirstUpdate && scope.lastChangeReason >= 0)
                            if (ENV.BackwardCompatible.NullBehaviour.IsNull(y) == ENV.BackwardCompatible.NullBehaviour.IsNull(c.Value))
                                return true;
                        scope.PreviousValue = previousValue;
                        after.Add(() => scope.PreviousValue = y);
                        if (!object.ReferenceEquals(changeReasonColumn, null))
                        {

                            changeReasonColumn.SilentSet(chgReason);
                        }
                        if (!object.ReferenceEquals(previousValueColumn, null))
                            previousValueColumn.SilentSet(previousValue);
                        if (u.Equals(previousValue, c.Value) && !isParameterFirstUpdate)
                        {
                            var ec = c as IENVColumn;
                            if (ec != null && ec.CompareForNullValue(previousValue))
                                return true;


                        }







                        var restoreSubforms = new List<Action>();
                        provideSubForms(f =>
                        {
                            var form = f as ENV.UI.SubForm;
                            if (form != null)
                            {
                                var prevSuspendAutoRefresh = form.SuspendAutoRefresh;
                                restoreSubforms.Add(() => form.SuspendAutoRefresh = prevSuspendAutoRefresh);
                                form.SuspendAutoRefresh = true;
                            }
                        });
                        try
                        {
                            handler(e);
                        }
                        finally
                        {
                            restoreSubforms.ForEach(action => action());

                        }
                    }
                    finally
                    {
                        after.ForEach(x => x());
                    }
                    return true;
                };
            var envCol = c as IENVColumn;
            if (envCol != null)
            {
                var eventList = envCol._internalValueChangeStore as List<Func<ValueChangedEventArgs<DataType>, bool>>;
                if (eventList == null)
                {
                    eventList = new List<Func<ValueChangedEventArgs<DataType>, bool>>();
                    envCol._internalValueChangeStore = eventList;
                    c.ValueChanged += e =>
                    {
                        for (int i = eventList.Count - 1; i >= 0; i--)
                        {

                            {
                                if (eventList[i](e))
                                    return;
                            }
                        }
                    };
                }
                eventList.Add(fireEventAndReturnFalseIfConditionIsFalse);

            }
            else
                c.ValueChanged += e =>
                {
                    fireEventAndReturnFalseIfConditionIsFalse(e);
                };
        }
        static bool _suppressColumnChange = false;
        public static void DoWhileColumnChangeIsSuppressed(Action action)
        {
            _suppressColumnChange = true;
            try
            {
                action();
            }
            finally
            {
                _suppressColumnChange = false;
            }
        }

        public static void CallCom<T>(this Firefly.Box.Interop.ComColumn<T> col, System.Action<T> operationToRun) where T : class
        {
            Context.Current.InvokeUICommand(
                () =>
                {
                    var prevCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
                    try
                    {
                        //Used to solve a specific Excel com interop issue
                        if (AlwaysUseEnglishUSCultureForTry)
                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                        operationToRun(col.Value);

                    }
                    catch (System.Runtime.InteropServices.COMException e)
                    {
                        ENV.UserMethods.SetLastComError(e);
                        ENV.ErrorLog.WriteToLogFile(e, "");
                    }
                    catch (Exception e)
                    {
                        ENV.ErrorLog.WriteToLogFile(e, "");
                    }
                    finally
                    {
                        System.Threading.Thread.CurrentThread.CurrentCulture = prevCulture;
                    }
                });
        }
        public static void CallCom<T>(this Firefly.Box.Interop.ComColumn<T> col, System.Action<T> operationToRun, NumberColumn returnCode) where T : class
        {
            Try(() => operationToRun(col.Value), returnCode);
        }

        public static void Try(Action operationToRun)
        {
            Try(operationToRun, new NumberColumn());
        }

        [ThreadStatic]
        static CultureInfo _threadCulture;
        internal static CultureInfo ThreadCulture
        {
            get
            {
                if (_threadCulture != null)
                    return _threadCulture;
                return Thread.CurrentThread.CurrentCulture;
            }
            set { _threadCulture = value; }
        }
        public static bool AlwaysUseEnglishUSCultureForTry = false;// I think that this is old and no longer relevant
        public static void Try(Action operationToRun, NumberColumn returnCode)
        {
            Exception ex = null;
            Context.Current.InvokeUICommand(
                () =>
                {
                    var prevCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
                    _threadCulture = prevCulture;
                    try
                    {
                        //Used to solve a specific Excel com interop issue
                        if (AlwaysUseEnglishUSCultureForTry)
                            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                        operationToRun();
                        returnCode.SilentSet(0);
                    }
                    catch (System.Runtime.InteropServices.COMException e)
                    {
                        returnCode.SilentSet(e.ErrorCode);
                        ENV.UserMethods.SetLastComError(e);
                        ENV.ErrorLog.WriteToLogFile(e, "");
                    }
                    catch (Exception e)
                    {
                        ex = e;
                        ENV.ErrorLog.WriteToLogFile(e, "");
                        returnCode.SilentSet(-1);
                    }
                    finally
                    {
                        System.Threading.Thread.CurrentThread.CurrentCulture = prevCulture;
                        var dispose = Context.Current["Common.DisposeAfterTry"] as List<Action>;
                        if (dispose != null)
                        {
                            dispose.ForEach(action =>
                            {
                                try
                                {
                                    action();
                                }
                                catch (Exception ex1)
                                {
                                    ENV.ErrorLog.WriteToLogFile(ex1, "");
                                }
                            });
                            dispose.Clear();
                        }
                    }
                });
            UserMethods.SetLastDotnetError(ex);
        }

        public static void DisposeAfterTry(Action action)
        {
            var list = Context.Current["Common.DisposeAfterTry"] as List<Action>;
            if (list == null)
            {
                list = new List<Action>();
                Context.Current["Common.DisposeAfterTry"] = list;
            }
            list.Add(action);
        }


        public static void SilentSet<Type>(this TypedColumnBase<Type> column, Type value)
        {
            var x = column.OnChangeMarkRowAsChanged;
            column.OnChangeMarkRowAsChanged = false;
            try
            {
                column.Value = value;
            }
            finally
            {
                column.OnChangeMarkRowAsChanged = x;
            }
        }
        public static void SilentSet(this TypedColumnBase<Text> column, Text value)
        {
            SilentSet<Text>(column, value);
        }
        public static void SilentSet(this TypedColumnBase<Number> column, Number value)
        {
            SilentSet<Number>(column, value);
        }
        public static void SilentSet(this TypedColumnBase<Bool> column, Bool value)
        {
            SilentSet<Bool>(column, value);
        }
        public static void SilentSet(this TypedColumnBase<Date> column, Date value)
        {
            SilentSet<Date>(column, value);
        }
        public static void SilentSet(this TypedColumnBase<Time> column, Time value)
        {
            SilentSet<Time>(column, value);
        }












        static internal ColumnBase GetColumn(this Firefly.Box.UI.Advanced.InputControlBase c)
        {
            if (c == null) return null;
            try
            {
                var cd = c.GetType().GetProperty("Data").GetValue(c, new object[0]) as ControlData;
                if (cd != null)
                    return cd.Column;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
            }
            return null;
        }
        static Rectangle _mdiBounds;

        public static void MDILoad(System.Windows.Forms.Form mdi)
        {
            try
            {
                RootMDI = mdi;

                mdi.LocationChanged += (a, b) =>
                {
                    if (mdi.WindowState == FormWindowState.Normal)
                        _mdiBounds = mdi.Bounds;
                };
                mdi.SizeChanged += (a, b) =>
                {
                    if (mdi.WindowState == FormWindowState.Normal)
                        _mdiBounds = mdi.Bounds;
                };
                _mdiBounds = mdi.Bounds;
                if (DoNotSaveMdiDimensions)
                {
                    mdi.WindowState = FormWindowState.Maximized;
                    return;
                }
                if (!System.IO.File.Exists(MDIFileName))
                {
                    mdi.WindowState = FormWindowState.Maximized;
                    return;
                }
                var s = System.IO.File.ReadAllText(MDIFileName).Split(',');

                mdi.Left = int.Parse(s[0]);
                mdi.Top = int.Parse(s[1]);
                var w = int.Parse(s[2]);
                var h = int.Parse(s[3]);
                if (w > 0)
                    mdi.Width = w;
                if (h > 0)
                    mdi.Height = h;
                mdi.StartPosition = FormStartPosition.Manual;
                int x = 0, width = 0, y = 0, height = 0;
                foreach (var item in Screen.AllScreens)
                {
                    x = Math.Min(item.Bounds.X, x);
                    width = Math.Max(item.Bounds.Right, width);
                    y = Math.Min(item.Bounds.Y, y);
                    height = Math.Max(item.Bounds.Height, height);
                }
                if (mdi.Right > width)
                    mdi.Left = width - mdi.Width;
                if (mdi.Bottom > height)
                    mdi.Top = height - mdi.Height;
                if (mdi.Left < x)
                    mdi.Left = x;
                if (mdi.Top < y)
                    mdi.Top = y;

                if (s[4] == "1")
                    mdi.WindowState = FormWindowState.Maximized;
                else
                {
                    mdi.WindowState = FormWindowState.Normal;
                    _mdiBounds = mdi.Bounds;
                }

            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                mdi.WindowState = FormWindowState.Maximized;
            }
        }
        public static bool DoNotSaveMdiDimensions = false;
        static string MDIFileName { get { return System.Windows.Forms.Application.UserAppDataPath + "\\" + PathDecoder.FixFileName(ENV.UserMethods.Instance.Sys()) + ".mdiSettings"; } }

        public static void MDIClose(System.Windows.Forms.Form mdi)
        {
            try
            {
                if (mdi.WindowState != FormWindowState.Minimized)
                    System.IO.File.WriteAllText(MDIFileName, string.Format("{0},{1},{2},{3},{4}", _mdiBounds.Left, _mdiBounds.Top, _mdiBounds.Width, _mdiBounds.Height, mdi.WindowState == FormWindowState.Maximized ? "1" : "0"));
            }
            catch
            { }
        }
        public static void MDIScale(System.Windows.Forms.Form f, SizeF factor)
        {
            foreach (var c in f.Controls)
            {
                var s = c as StatusStrip;
                if (s != null)
                {
                    foreach (var st in s.Items)
                    {
                        var stl = st as ToolStripStatusLabel;
                        if (stl != null)
                        {
                            stl.Size = new Size((int)(stl.Size.Width * factor.Width), stl.Height);
                        }
                    }
                }
            }
        }

        static void MDIResizeEnd(System.Windows.Forms.Form mdi)
        {
            foreach (var mdiChild in mdi.MdiChildren)
            {
                var f = mdiChild as Firefly.Box.UI.Form;
                if (f != null && f.Dock == DockStyle.Fill)
                {
                    f.RunInLogicContext(() => Raise(Command.WindowResize));
                    return;
                }
            }
            foreach (var mdiChild in mdi.Controls)
            {
                var f = mdiChild as Firefly.Box.UI.Form;
                if (f != null && f.Dock == DockStyle.Fill)
                {
                    f.RunInLogicContext(() => Raise(Command.WindowResize));
                    return;
                }
            }
        }

        static void MDIMoveEnd(System.Windows.Forms.Form mdi)
        {
            Common.Raise(Command.WindowMove);
        }

        public static void MDIPaintBackgroundImage(Control f, PaintEventArgs e, Image image, Firefly.Box.UI.ImageLayout imageLayout)
        {
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(f, true, null);

            using (var b = new SolidBrush(f.BackColor))
                e.Graphics.FillRectangle(b, e.ClipRectangle);

            if (imageLayout == Firefly.Box.UI.ImageLayout.Tile)
            {
                for (var left = 0; left < f.ClientRectangle.Width; left += image.Width)
                    for (var top = 0; top < f.ClientRectangle.Height; top += image.Height)
                        e.Graphics.DrawImageUnscaled(image, new Point(left, top));
                return;
            }

            var x = (f.ClientRectangle.Width - image.Width) / 2;
            var y = (f.ClientRectangle.Height - image.Height) / 2;

            Rectangle srcRect, destRect;
            switch (imageLayout)
            {
                case Firefly.Box.UI.ImageLayout.ScaleToFill:
                    srcRect = new Rectangle(0, 0, image.Width, image.Height);
                    destRect = f.ClientRectangle;
                    break;
                default:
                    destRect = Rectangle.Intersect(f.ClientRectangle, new Rectangle(x, y, image.Width, image.Height));
                    srcRect = Rectangle.Intersect(f.ClientRectangle, new Rectangle(x, y, image.Width, image.Height));
                    srcRect.X = x < 0 ? -x : 0;
                    srcRect.Y = y < 0 ? -y : 0;
                    break;
            }

            e.Graphics.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);
        }

        public static Size GetMDIMinimumSize(System.Windows.Forms.Form mdi, Size defaultMinimumSize)
        {
            var result = defaultMinimumSize;
            foreach (var mdiChild in mdi.MdiChildren)
            {
                var f = mdiChild as Firefly.Box.UI.Form;
                if (f != null && f.FitToMDI)
                {
                    result.Width = System.Math.Max(result.Width, f.MinimumSize.Width + mdi.Width - mdiChild.Parent.Width + f.Left);
                    result.Height = System.Math.Max(result.Height, f.MinimumSize.Height + mdi.Height - mdiChild.Parent.Height + f.Top);
                }
            }
            return result;
        }

        public static ImageList GetImageList(string imageListLocation)
        {
            System.Drawing.Image image = ENV.Common.GetImage(imageListLocation);
            if (image == null)
                return null;
            System.Windows.Forms.ImageList result = new System.Windows.Forms.ImageList();
            var width = 16;
            result.ImageSize = new System.Drawing.Size(width, image.Height);
            var bmp = new System.Drawing.Bitmap((1 + image.Width / width) * width, image.Height);
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.DrawImageUnscaledAndClipped(image, new System.Drawing.Rectangle(0, 0, image.Width, image.Height));
                g.DrawImageUnscaledAndClipped(image, new System.Drawing.Rectangle(width, 0, image.Width, image.Height));
            }
            result.Images.AddStrip(bmp);
            result.TransparentColor = bmp.GetPixel(width, 0);
            return result;
        }
        public static void Raise(Command command, params object[] args)
        {
            RaiseOnContext(Context.Current, command, args);
        }
        public static void Raise(Text command, params object[] args)
        {
            RaiseOnContext(Context.Current, command, args);
        }

        public static void RaiseOnContext(Context context, Command command, object[] args)
        {
            ControllerBase.RaiseHappened(command);
            if (context.ActiveTasks.Count > 0)
            {
                var x = context.ActiveTasks[context.ActiveTasks.Count - 1];
                {
                    var uic = x as UIController;
                    if (uic != null)
                        uic.Raise(command, args);
                }
                {
                    var uic = x as BusinessProcess;
                    if (uic != null)
                        uic.Raise(command, args);
                }
                {
                    var uic = x as ModuleController;
                    if (uic != null)
                        uic.Raise(command, args);
                }
            }
        }
        public static void RaiseOnContext(Context context, Text command, object[] args)
        {
            ControllerBase.RaiseHappened(command);
            if (context.ActiveTasks.Count > 0)
            {
                var x = context.ActiveTasks[context.ActiveTasks.Count - 1];
                {
                    var uic = x as UIController;
                    if (uic != null)
                        uic.Raise(HandlerCollectionWrapper.FixCustomCommandKey(command), args);
                }
                {
                    var uic = x as BusinessProcess;
                    if (uic != null)
                        uic.Raise(HandlerCollectionWrapper.FixCustomCommandKey(command), args);
                }
                {
                    var uic = x as ModuleController;
                    if (uic != null)
                        uic.Raise(HandlerCollectionWrapper.FixCustomCommandKey(command), args);
                }
            }
        }
        public static void Invoke(Command command, params object[] args)
        {
            InvokeOnContext(Context.Current, command, args);
        }

        public static void InvokeOnContext(Context context, Command command, object[] args)
        {
            if (context.ActiveTasks.Count > 0)
            {
                var x = context.ActiveTasks[context.ActiveTasks.Count - 1];
                {
                    var uic = x as UIController;
                    if (uic != null)
                        uic.Invoke(command, args);
                }
                {
                    var uic = x as BusinessProcess;
                    if (uic != null)
                        uic.Invoke(command, args);
                }
                {
                    var uic = x as ModuleController;
                    if (uic != null)
                        uic.Invoke(command, args);
                }
            }
        }




        static Icon _defaultIcon = null;
        static bool _tried = false;
        public static bool AllwaysShowDBErrors { get; set; }

        public static Icon DefaultIcon
        {
            get
            {
                if (_defaultIcon == null && !_tried)
                    try
                    {
                        _tried = true;
                        var a = Assembly.GetEntryAssembly();
                        if (a != null)
                        {
                            DefaultIcon = GetIconForFile(Assembly.GetEntryAssembly().Location);
                            if (_defaultIcon == null)
                                DefaultIcon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
                        }
                    }
                    catch
                    {
                    }
                return _defaultIcon;
            }
            set
            {
                _defaultIcon = value;
                if (_defaultIcon != null && _defaultIcon.Size.Width > 16)
                {
                    var bmp = new Bitmap(16, 16);
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawIcon(_defaultIcon, new Rectangle(0, 0, 16, 16));
                    }
                    _defaultIcon = Icon.FromHandle(bmp.GetHicon());
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        static Icon GetIconForFile(string filename)
        {
            var shinfo = new SHFILEINFO();
            SHGetFileInfo(filename, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), 0x101);

            if (shinfo.hIcon.ToInt32() == 0) return null;
            var icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
            DestroyIcon(shinfo.hIcon);
            return icon;
        }

        [DllImport("shell32.dll")]
        static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);

        public static bool IncludeThreadInTraceFileName { get; set; }

        public static void SetColumnsReadonlyAcordingToActivity(ColumnCollection columns, Activities activity)
        {
            foreach (var column in columns)
            {
                var c = column as IENVColumn;
                if (c != null && column.Entity != null)
                    c._internalReadOnly = UserSettings.SuppressUpdatesInBrowseActivity && activity == Activities.Browse;
            }
        }

        public static System.Threading.Thread RunOnNewThread(Action run, bool syncUI = true)
        {
            var user = UserManager.CurrentUser;
            Context uiContext = null;
            Form f = null;
            Context.Current.InvokeUICommand(
                () =>
                {
                    uiContext = Context.Current;
                    f = ContextTopMostForm;
                });
            var t = new System.Threading.Thread(
                () =>
                {
                    Context.Current.BeforeExit += e => { Raise(Commands.CloseAllTasks); e.Cancel = true; };
                    if (_suppressDialogForTesting || !syncUI)
                        Context.Current.SetNonUIThread();
                    else
                    {
                        Context.Current.AttachToUIContext(uiContext);
                        _callingContextTopMostForm.Value = f;
                    }
                    try
                    {
                        UserManager.SetCurrentUser(user);
                        run();
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorLog.WriteToLogFile(e);
                        ShowExceptionDialog(e);
                    }
                    finally
                    {
                        _callingContextTopMostForm.Value = null;
                        Profiler.ThreadIsClosing();
                        Context.Current.Dispose();
                    }
                });
            if (!_suppressDialogForTesting)
                t.SetApartmentState(ApartmentState.STA);
            t.Start();
            return t;
        }

        public static IDisposable SetAsCaseInsensitive()
        {
            return new CaseInsensitiveDisposable();
        }
        class CaseInsensitiveDisposable : IDisposable
        {
            List<Action> _onTearDown = new List<Action>();
            public CaseInsensitiveDisposable()
            {
                {
                    var x = MemoryDatabase.DefaultCaseSensitive;
                    _onTearDown.Add(() => MemoryDatabase.DefaultCaseSensitive = x);
                    MemoryDatabase.DefaultCaseSensitive = false;
                }
                {
                    var x = Text.TextComparer;
                    _onTearDown.Add(() => Text.TextComparer = x);
                    Text.TextComparer = LocalizationInfo.Current.GetCaseInsensitiveTextComparer();
                }
            }

            public void Dispose()
            {
                foreach (var action in _onTearDown)
                {
                    action();
                }
            }
        }
        public class CultureAwareIgnoreCaseTextComparer : Text.ITextComparer
        {
            public static IgnoreCaseTextComparer Instance = new IgnoreCaseTextComparer();
            public bool StartsWith(string theString, string theSubstring)
            {
                return theString.StartsWith(theSubstring, StringComparison.CurrentCultureIgnoreCase);
            }

            public int TrimAndCompare(string a, int additionalSpacesA, string b, int additionalSpacesB)
            {
                var at = a.TrimEnd(' ');
                var bt = b.TrimEnd(' ');
                var al = at.Length;
                var bl = bt.Length;
                if (al > bl)
                    bt = bt.PadRight(al, ' ');
                else if (al < bl)
                    at = at.PadRight(bl, ' ');
                return string.Compare(at, bt, StringComparison.CurrentCultureIgnoreCase);


            }

            public bool AreEqualTrim(string a, string b)
            {
                return string.Equals(a.TrimEnd(' '), b.TrimEnd(' '), StringComparison.CurrentCultureIgnoreCase);

            }

            public bool AreEqualOrdinalTrim(string a, string b)
            {
                return string.Equals(a.TrimEnd(' '), b.TrimEnd(' '), StringComparison.Ordinal);
            }
        }
        public class IgnoreCaseTextComparer : Text.ITextComparer
        {
            public static IgnoreCaseTextComparer Instance = new IgnoreCaseTextComparer();
            public bool StartsWith(string theString, string theSubstring)
            {
                return theString.StartsWith(theSubstring, StringComparison.CurrentCultureIgnoreCase);
            }
            static Text.ITextComparer _default = new Text.DefaultTextComparer();

            public int TrimAndCompare(string a, int additionalSpacesA, string b, int additionalSpacesB)
            {
                return _default.TrimAndCompare(a.ToLower(), additionalSpacesA, b.ToLower(), additionalSpacesB);



            }

            public bool AreEqualTrim(string a, string b)
            {
                return string.Equals(a.TrimEnd(' '), b.TrimEnd(' '), StringComparison.CurrentCultureIgnoreCase);

            }

            public bool AreEqualOrdinalTrim(string a, string b)
            {
                return string.Equals(a.TrimEnd(' '), b.TrimEnd(' '), StringComparison.Ordinal);
            }
        }


        class BoundStatusBar : IDisposable
        {
            System.Windows.Forms.ToolStripStatusLabel _statusBarTextLabel = null;
            System.Windows.Forms.ToolStripStatusLabel _userStatusLabel = null;
            System.Windows.Forms.ToolStripStatusLabel _insertOverwriteIndicator = null;
            ContextStatic<Stack<string>> _statusTextStack = new ContextStatic<Stack<string>>(() => new Stack<string>(new[] { "" }));
            System.Windows.Forms.ToolStripStatusLabel _activityStatusLabel = null;
            ToolStripStatusLabel _expandStatusLabel = null;
            ToolStripStatusLabel _expandTextBoxStatusLabel = null;
            ToolStripStatusLabel _versionIndicator;
            public BoundStatusBar(ToolStripStatusLabel mainStatusLabel, ToolStripStatusLabel userStatusLabel, ToolStripStatusLabel activityStatusLabel, ToolStripStatusLabel expandStatusLabel, ToolStripStatusLabel expandTextBoxStatusLabel, ToolStripStatusLabel insertOverwriteIndicator, ToolStripStatusLabel versionIndicator)
            {
                _activityStatusLabel = activityStatusLabel;
                _statusBarTextLabel = mainStatusLabel;
                _insertOverwriteIndicator = insertOverwriteIndicator;
                _expandStatusLabel = expandStatusLabel;
                _expandTextBoxStatusLabel = expandTextBoxStatusLabel;
                _versionIndicator = versionIndicator;
                _userStatusLabel = userStatusLabel;
            }

            string CurrentStatusText { get { return _statusTextStack.Value.Peek(); } }

            public void PushStatusText(string text)
            {
                var x = CurrentStatusText;
                _statusTextStack.Value.Push(text);
                if (_statusBarTextLabel != null)
                    if (x == _statusBarTextLabel.Text)
                    {
                        ResetStatusText();
                    }
            }

            public void SetDefaultStatusText(string text)
            {
                var tempStack = new Stack<string>();
                while (_statusTextStack.Value.Count > 0)
                    tempStack.Push(_statusTextStack.Value.Pop());
                _statusTextStack.Value = new Stack<string>();
                _statusTextStack.Value.Push("");
                _statusTextStack.Value.Push(text);
                tempStack.Pop();
                if (tempStack.Count > 0)
                    tempStack.Pop();
                while (tempStack.Count > 0)
                    _statusTextStack.Value.Push(tempStack.Pop());
                ResetStatusText();
            }

            public void ResetStatusText()
            {
                if (_statusBarTextLabel != null)
                    SetStatusText(CurrentStatusText);
            }

            public void PopStatusText()
            {
                _statusTextStack.Value.Pop();
                ResetStatusText();
            }

            public void SetActivityText(string activityText)
            {
                if (_activityStatusLabel == null) return;
                Context.Current.InvokeUICommand(() => _activityStatusLabel.Text = activityText);
            }

            public void UpdateInsertOverwriteIndicator()
            {
                if (_insertOverwriteIndicator == null) return;
                Context.Current.InvokeUICommand(() => _insertOverwriteIndicator.Text = Firefly.Box.UI.TextBox.InsertKeyMode == InsertKeyMode.Overwrite ? LocalizationInfo.Current.TextboxOverwriteMode : LocalizationInfo.Current.TextboxInsertMode);
            }

            List<Action> _unbind = new List<Action>();

            public BoundStatusBar()
            {
            }

            public void ExpandEnabledChanged()
            {
                if (_expandStatusLabel == null) return;
                var text = Command.Expand.Enabled ? LocalizationInfo.Current.Expand : "";
                Context.Current.InvokeUICommand(() => _expandStatusLabel.Text = text);
            }

            public void ExpandTextBoxEnabledChanged()
            {
                if (_expandTextBoxStatusLabel == null) return;
                var text = Command.ExpandTextBox.Enabled ? LocalizationInfo.Current.ExpandTextBox : "";
                Context.Current.InvokeUICommand(() => _expandTextBoxStatusLabel.Text = text);
            }

            public void Bind()
            {
                ExpandEnabledChanged();
                ExpandTextBoxEnabledChanged();
                Context.Current.InvokeUICommand(
                    () =>
                    {
                        if (_statusBarTextLabel != null)
                        {
                            if (this == _mainStatusBar)
                            {
                                if (!string.IsNullOrEmpty((ApplicationTitle ?? "").Trim()))
                                {
                                    if (!ENV.UserSettings.Version10Compatible)
                                        _statusBarTextLabel.Text = ApplicationTitle;
                                    if (RootMDI != null)
                                    {
                                        if (ShowSpecificMDIText && !string.IsNullOrEmpty(SpecificMDIText))
                                            RootMDI.Text = SpecificMDIText;
                                        else
                                            RootMDI.Text = ApplicationTitle;
                                    }
                                }
                                PushStatusText(_statusBarTextLabel.Text);
                            }
                            else
                            {
                                SetDefaultStatusText(_mainStatusBar.CurrentStatusText);
                            }
                            Firefly.Box.UI.Form.UserInput += ResetStatusText;
                            _statusBarTextLabel.TextChanged +=
                                (sender, args) =>
                                {
                                    if (StatusBarTextLabelTextChanged != null)
                                        StatusBarTextLabelTextChanged(_statusBarTextLabel.Text);
                                };
                            _unbind.Add(() => Firefly.Box.UI.Form.UserInput -= ResetStatusText);
                        }
                        if (_userStatusLabel != null)
                        {
                            Action setUserLabel = () => _userStatusLabel.Text = ENV.Security.UserManager.CurrentUser.Name;
                            ENV.Security.UserManager.CurrentUserChanged += setUserLabel;
                            _unbind.Add(() => ENV.Security.UserManager.CurrentUserChanged -= setUserLabel);
                            setUserLabel();
                        }
                        if (_versionIndicator != null)
                        {
                            _versionIndicator.Text = "Version: " +
                                                              (System.Reflection.Assembly.GetEntryAssembly()
                                                               ?? System.Reflection.Assembly.GetCallingAssembly()
                                                               ?? System.Reflection.Assembly.GetExecutingAssembly()).
                                                                  GetName().Version;
                        }
                        UpdateInsertOverwriteIndicator();
                    });
            }

            public void SetTemporaryStatusMessage(string text)
            {

                _statusText = text;
                SetStatusText(Languages.Translate(text));
            }

            void SetStatusText(string text)
            {
                if (_statusBarTextLabel != null && _statusBarTextLabel.Text != text)
                    Context.Current.InvokeUICommand(() => { _statusBarTextLabel.Text = text; });
                if (StatusTextChanged != null)
                    StatusTextChanged(text);
            }

            public void Dispose()
            {
                foreach (var action in _unbind)
                    action();
            }

            public event Action<string> StatusTextChanged;
        }

        public static Image GetTabControlImage(string imageLocation)
        {
            var i = GetImage(imageLocation);
            var bmp = i as Bitmap;
            if (bmp != null)
                try
                {
                    bmp.MakeTransparent(bmp.GetPixel(0, 0));
                }
                catch
                {
                }
            return i;
        }


        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public static void ProcessMDIMessage(System.Windows.Forms.Form mdi, System.Windows.Forms.Message m)
        {
            if (UserSettings.Version8Compatible)
            {
                // Exit UserMethods.Delay when mdi is activated
                if (m.Msg == 0x1c /* WM_ACTIVATEAPP */ && (int)m.WParam == 1)
                {
                    PostMessage(mdi.Handle, 0x444, IntPtr.Zero, IntPtr.Zero);
                }
                if (m.Msg == 0x444)
                {
                    Context.Current.BeginInvoke(() => { });
                }
            }
            if (m.Msg == 0x4A) // WM_COPYDATA 
            {
                var ds = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT));
                var args = new object[ds.dwData.ToInt32()];
                var ofset = 0;
                for (int i = 0; i < args.Length; i++)
                {
                    var bytes = new List<byte>();
                    var b = (byte)1;
                    while (ofset < ds.cbData && (b = Marshal.ReadByte(ds.lpData, ofset++)) != 0)
                        bytes.Add(b);
                    var s = LocalizationInfo.Current.OuterEncoding.GetString(bytes.ToArray());
                    args[i] = s;
                    if (s.Length > 1 && s.StartsWith("-"))
                    {
                        args[i] = s.Substring(2);
                        switch (s.ToUpper(CultureInfo.InvariantCulture)[1])
                        {
                            case 'N':
                            case 'F':
                                args[i] = Number.Parse(args[i].ToString());
                                break;
                            case 'L':
                                args[i] = args[i].ToString() == "T";
                                break;
                        }
                    }
                }
                var c = _rootMdiContext ?? Context.Current;
                c.BeginInvoke(() => Context.Current.Suspend(0));
                RaiseOnContext(c, Commands.ExternalEvent, args);
            }

            if (m.Msg == 0x112) //WM_SYSCOMMAND
            {
                var scCommand = ((int)m.WParam & 0xfff0);
                if (scCommand == 0xF030 || scCommand == 0xF120 || scCommand == 0xF000 || scCommand == 0xF010)
                    Context.Current["MDI_SIZE_BEFORE_SYSCOMMAND" + mdi.GetHashCode()] = mdi.Size;
            }

            if (m.Msg == 0x1f) // WM_CANCELMODE
                Firefly.Box.UI.Form.CompleteUIChanges();

            if (ProcessingMdiMessage != null)
                ProcessingMdiMessage(mdi, m);
        }
        public static void ProcessMDIMessageAfterMDI(System.Windows.Forms.Form mdi, System.Windows.Forms.Message m)
        {
            ProcessMDIMessageAfterMDI(mdi, m,
                clientSize => clientSize + SystemInformation.FrameBorderSize + SystemInformation.FrameBorderSize + new Size(0, SystemInformation.CaptionHeight));
        }
        public static void ProcessMDIMessageAfterMDI(System.Windows.Forms.Form mdi, System.Windows.Forms.Message m, Func<Size, Size> sizeFromClientSize)
        {
            if (m.Msg == 0x24) // WM_GETMINMAXINFO
            {
                var mmi = (MINMAXINFO)Marshal.PtrToStructure(m.LParam, typeof(MINMAXINFO));

                foreach (var child in mdi.MdiChildren)
                {
                    var f = child as Firefly.Box.UI.Form;
                    if (f != null && (f.WindowState == FormWindowState.Maximized || f.Dock == DockStyle.Fill) && !f.MinimumTrackSize.IsEmpty)
                    {
                        var s = sizeFromClientSize(f.MinimumTrackSize);
                        foreach (Control c in mdi.Controls)
                        {
                            if (c.Dock == DockStyle.Top | c.Dock == DockStyle.Bottom)
                                s.Height += c.Height;
                            if (c.Dock == DockStyle.Left | c.Dock == DockStyle.Right)
                                s.Width += c.Width;
                        }
                        mmi.ptMinTrackSize.x = Math.Min(mdi.Width, Math.Max(mmi.ptMinTrackSize.x, s.Width));
                        mmi.ptMinTrackSize.y = Math.Min(mdi.Height, Math.Max(mmi.ptMinTrackSize.y, s.Height));
                    }
                }
                Marshal.StructureToPtr(mmi, m.LParam, false);
                m.Result = IntPtr.Zero;
            }

            if (m.Msg == 0x112) //WM_SYSCOMMAND
            {
                var scCommand = ((int)m.WParam & 0xfff0);
                if ((scCommand == 0xF030 || scCommand == 0xF120 || scCommand == 0xF000 || scCommand == 0xF010) && ((Size)(Context.Current["MDI_SIZE_BEFORE_SYSCOMMAND" + mdi.GetHashCode()]) != mdi.Size))
                    MDIResizeEnd(mdi);
                if (scCommand == 0xF010)
                    MDIMoveEnd(mdi);

            }
        }

        [StructLayout(LayoutKind.Sequential)]
        class MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        class POINT
        {
            public int x;
            public int y;
        }

        public static event Action<System.Windows.Forms.Form, System.Windows.Forms.Message> ProcessingMdiMessage;

        [StructLayout(LayoutKind.Sequential)]
        struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        public static bool IsRootMDI(Form form)
        {
            return form == RootMDI;
        }

        public static void RunPreviewInSameUIThread()
        {
            Firefly.Box.Printing.Advanced.PrintDocumentPrintJob.SetPreviewOwnerForm =
                (f) =>
                {
                    RunOnContextTopMostForm(f);
                };
        }

        public static bool FileExists(string filePath)
        {
            var x = new WIN32_FIND_DATA();//watch out - if the filepath is greater than 260 chars - it returned false in testing with blanks
            using (var h = FindFirstFile(filePath.TrimEnd(), out x))
                return !h.IsInvalid;
        }

        sealed class SafeFindHandle : Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
        {
            // Methods
            [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.LinkDemand, UnmanagedCode = true)]
            internal SafeFindHandle()
            : base(true)
            {
            }

            public SafeFindHandle(IntPtr preExistingHandle, bool ownsHandle) : base(ownsHandle)
            {
                base.SetHandle(preExistingHandle);
            }

            protected override bool ReleaseHandle()
            {
                if (!(IsInvalid || IsClosed))
                {
                    return FindClose(this);
                }
                return (IsInvalid || IsClosed);
            }

            protected override void Dispose(bool disposing)
            {
                if (!(IsInvalid || IsClosed))
                {
                    FindClose(this);
                }
                base.Dispose(disposing);
            }
        }


        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern SafeFindHandle FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll")]
        public static extern bool FindClose(SafeHandle hFindFile);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WIN32_FIND_DATA
        {
            public FileAttributes dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternate;
        }
        internal static bool OKToUpdateColumn(IENVColumn col)
        {
            CheckReadOnlyForExistingRows(col);

            return col._internalReadOnly;
        }

        private static void CheckReadOnlyForExistingRows(IENVColumn col)
        {
            if (col.ReadOnlyForExistingRows)
            {
                var c = col as ColumnBase;
                if (c != null && c.Entity != null)
                {
                    var at = Firefly.Box.Context.Current.ActiveTasks;
                    for (int i = at.Count - 1; i >= 0; i--)
                    {
                        var uic = at[i] as UIController;
                        if (uic != null)
                        {
                            if (CheckController(uic.From, uic.Activity, uic.Relations, c))
                                return;
                        }
                        var bp = at[i] as BusinessProcess;
                        if (bp != null)
                        {
                            if (CheckController(bp.From, bp.Activity, bp.Relations, c))
                                return;
                        }
                    }

                }
            }
        }
        static bool CheckController(Firefly.Box.Data.Entity from, Activities activity, RelationCollection relations, ColumnBase c)
        {
            if (from == c.Entity)
            {
                if (activity != Activities.Insert)
                    throw new ReadOnlyForExistingRowsException(c);
                return true;
            }
            foreach (var r in relations)
            {
                if (r.From == c.Entity)
                {
                    if (r.Enabled && r.RowFound)
                        throw new ReadOnlyForExistingRowsException(c);
                    return true;
                }
            }
            return false;
        }
        public class ReadOnlyForExistingRowsException : RollbackException
        {
            public ReadOnlyForExistingRowsException(ColumnBase c) : base(false, "Can't set the value for column " + c.Caption + " (" + c.Name + ")" + " on existing row, because it's ReadOnlyForExistingRows property is set to true", null) { }
        }
    }
}
