using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Web;
using System.Windows.Forms;
using ENV.Advanced;
using ENV.BackwardCompatible;
using ENV.Data;
using ENV.IO;
using ENV.UI;

using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using Firefly.Box.UI.Advanced;
using Form = Firefly.Box.UI.Form;
using ENV.Data.DataProvider;
using ENV.Security;

namespace ENV
{
    [Firefly.Box.UI.Advanced.ContainsData]
    public class ApplicationControllerBase : IHaveUserMethods
    {
        static Dictionary<string, string> _searchPathes = new Dictionary<string, string>();
        static ApplicationControllerBase()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                foreach (var searchPathe in _searchPathes)
                {
                    if (args.Name.StartsWith(searchPathe.Key))
                    {
                        var whereToSearch = searchPathe.Value;
                        if (string.IsNullOrEmpty(whereToSearch))
                            whereToSearch = AppDomain.CurrentDomain.BaseDirectory;
                        var fullName =
                            System.IO.Path.Combine(whereToSearch,
                                args.Name);
                        if (fullName.Contains(","))
                            fullName = fullName.Remove(fullName.IndexOf(','));
                        if (System.IO.File.Exists(fullName + ".exe"))
                            return Assembly.LoadFrom(fullName + ".exe");
                        else if (System.IO.File.Exists(fullName + ".dll"))
                            return Assembly.LoadFrom(fullName + ".dll");

                    }
                }
                return null;
            };

            try
            {
                // This is here to make sure PresentationCore.dll is loaded before any window is created
                // !!! IMPORTANT TO FIX PROCESS DPI AWARE ISSUES DO NOT REMOVE !!!
                var x = System.Windows.Media.Fonts.SystemFontFamilies;
            }
            catch { }
        }
        internal Type _defaultContextMenuType;
        protected void SetDefaultContextMenu(Type type)
        {
            _defaultContextMenuType = type;
        }
        List<ColumnBase> _additionalColumns = new List<ColumnBase>();
        internal protected List<ColumnBase> AdditionalColumns { get { return _additionalColumns; } }
        internal ModuleController _moduleController;
        protected internal ENV.UserMethods u;
        UserMethods IHaveUserMethods.GetUserMethods()
        {
            return u;
        }
        protected internal string TaskID { get; set; }
        protected internal string _applicationPrefix;
        CachedControllerManager _cachedControllerManager;
        [ThreadStatic]
        internal static Func<ApplicationControllerBase, bool> _suppressStartEvent = null;

        public static bool FirstRun = false;
        static HashSet<Type> _runnedApplications = new HashSet<Type>();
        public static bool AlwaysRunOnStartWithRunmodeMinusOne { get; set; }
        public ApplicationControllerBase()
        {
            UseInsteadOfNullBehaviour = false;
            _cachedControllerManager = new CachedControllerManager(this);
            var advancedSettings = new ModuleController.AdvancedSettings();
            advancedSettings.AccumulateChangeEventOnReevaluation = true;
            advancedSettings.ParentViewAccordingToHandlerContext = UserSettings.Version8Compatible;
            advancedSettings.ProcessExpressionCommandHandlersInOrderWithOtherHandlers = UserSettings.Version8Compatible;

            _moduleController = new ModuleController(advancedSettings);
            ControllerBase.ControllerStart(this);
            _moduleController.Start += delegate
            {
                InitNullToColumns(true);
                if (_suppressStartEvent != null && _suppressStartEvent(this))
                {
                    return;
                }

                using (_levelProvider.Start())
                {
                    try
                    {
                        InStart = true;
                        var t = GetType();
                        bool dontRunStart = false;
                        if (!AlwaysRunOnStartWithRunmodeMinusOne)
                        {
                            if (!_runnedApplications.Contains(t))
                            {
                                bool runit = false;
                                lock (_runnedApplications)
                                {
                                    if (!_runnedApplications.Contains(t))
                                    {
                                        runit = true;

                                        _runnedApplications.Add(t);
                                    }
                                }
                                lock (t)
                                {
                                    if (runit)
                                    {
                                        FirstRun = true;
                                        OnStart();
                                        FirstRun = false;
                                        dontRunStart = true;
                                    }
                                }
                            }
                            if (!dontRunStart)
                                OnStart();
                        }
                        else
                        {
                            FirstRun = true;
                            OnStart();
                            FirstRun = false;
                        }

                    }
                    finally
                    {
                        InStart = false;
                    }
                }
                ParametersInMemory.Instance.ClearParametersThatExistInOuterValueProvider();
            };
            _moduleController.End += delegate
                                     {

                                         using (_levelProvider.End())
                                         {
                                             OnEnd();
                                             if (End != null)
                                                 End();
                                             _afterEnd();
                                         }
                                         lock (_activeControllers)
                                             _activeControllers.Remove(_moduleController);
                                     };
            _moduleController.Load += myLoad;
            _moduleController.DatabaseErrorOccurred += e => Common.DatabaseErrorOccurred(e, Relations, LockingStrategy.None);
            _moduleController.PreviewDatabaseError += Common.PreviewDatabaseError;
            _levelProvider = new LevelProvider(true, _moduleController, this);
            _handlers = new HandlerCollectionWrapper(_moduleController.Handlers, _levelProvider, () => UserSettings.Version8Compatible, () => UserSettings.Version8Compatible, _cachedControllerManager, () => AllPrograms, x => _moduleController.Invoke(x));
            u = new UserMethods(this);
            u.SetApplication(this);
        }
        internal protected static void AddPrimaryKeyToOrderByOf(Relation r)
        {
            var orderBy = r.OrderBy;
            var entity = r.From;
            ControllerBase.AddPrimaryKeyToOrderBy(orderBy, entity);
        }

        internal static Dictionary<ModuleController, ApplicationControllerBase> _activeControllers =
            new Dictionary<ModuleController, ApplicationControllerBase>();
        internal event Action End;
        public T Cached<T>() where T : class
        {
            return _cachedControllerManager.GetCachedController<T>();
        }
        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }

        internal bool InStart = false;
        protected bool NewViewRequired { get { return !Common._suppressDialogForTesting; } }
        internal LevelProvider _levelProvider;
        protected virtual void OnEnd() { }

        protected Activities Activity
        {
            get { return Activities.Update; }
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

        protected virtual void OnStart() { }
        bool _everLoaded = false;
        public static Keys ControllersListShortcut = Keys.Shift | Keys.F3, ModelsListShortcut = Keys.Shift | Keys.F2,
            DebugLocationShortcut = Keys.Control | Keys.F12, BreakAllShortcut = Keys.Shift | Keys.F12;

        bool _useInsteadOfNullBehaviour;
        internal INullStrategy _nullStrategy = NullStrategy.GetStrategy(false);
        protected internal bool UseInsteadOfNullBehaviour
        {
            get { return _useInsteadOfNullBehaviour; }
            set
            {
                _useInsteadOfNullBehaviour = value;
                _nullStrategy = NullStrategy.GetStrategy(value);

            }
        }
        bool _determinedNullBehavior = false;

        internal void InitNullToColumns(bool dontDoItAgain)
        {
            if (_determinedNullBehavior)
                return;
            NullStrategy.ApplyToUserInstance(Columns, Entities, u, _moduleController.View, _nullStrategy);
            _determinedNullBehavior = dontDoItAgain;


        }
        public static Func<bool> CustomerFilterSortAndFindHandler { get; set; }
        static bool userConfirmedExit = false;
        void myLoad()
        {
            lock (_activeControllers)
                _activeControllers.Add(_moduleController, this);
            InitNullToColumns(false);
            OnLoad();

            if (_everLoaded || _processingWebRequest.Value || Context.Current.ActiveTasks.Count > 1 || !RunnedAsApplication || !System.Environment.UserInteractive)
                return;

            _everLoaded = true;

            if (CustomerFilterSortAndFindHandler == null && UserSettings.VersionXpaCompatible)
            {
                CustomerFilterSortAndFindHandler = () => false;
            }

            Commands.FindRows.AddTo(Handlers, CustomerFilterSortAndFindHandler);
            Commands.FilterRows.AddTo(Handlers, CustomerFilterSortAndFindHandler);
            Commands.FindNextRow.AddTo(Handlers);
            Commands.CustomOrderBy.AddTo(Handlers, CustomerFilterSortAndFindHandler);
            Commands.SelectOrderBy.AddTo(Handlers, CustomerFilterSortAndFindHandler);


            Commands.ExportData.AddTo(Handlers, CustomerFilterSortAndFindHandler);
            Commands.TemplateExit.AddTo(Handlers);
            Commands.ClearCurrentValueInTemplate.AddTo(Handlers);
            Commands.ClearTemplate.AddTo(Handlers);
            Commands.TemplateFromValues.AddTo(Handlers);
            Commands.TemplateToValues.AddTo(Handlers);
            Commands.TemplateOk.AddTo(Handlers);
            Commands.TemplateExpression.AddTo(Handlers);

            Handlers.Add(ENV.Commands.PrinterSettingsDialog).Invokes += a => ENV.Printing.PrinterWriter.ShowPrinterSettingsDialog();
            Handlers.Add(ENV.Commands.About).Invokes += a => new ENV.UI.AboutFirefly().ShowDialog(Common.ContextTopMostForm);
            Handlers.Add(ENV.Commands.ShellToOS).Invokes += a =>
            {
                a.Handled = true;
                Windows.OSCommand(
                    System.Environment.GetEnvironmentVariable(
                        "ComSpec"));
            };
            Handlers.Add(Commands.CloseAllWindows).Invokes += e =>
            {
                e.Handled = true;
                Firefly.Box.Context.Current.InvokeUICommand(() =>
                {
                    foreach (var item in ENV.UI.Form._windowList.ToArray())
                    {
                        try
                        {
                            item.Close();
                        }
                        catch { }
                    }
                });
            };
            Handlers.Add(Commands.NextWindow).Invokes += e =>
            {
                e.Handled = true;
                if (UI.Form._windowList.Count == 0)
                    return; var x = ENV.UI.Form._windowList.IndexOf(UI.Form._activeForm);
                x++;
                if (x == ENV.UI.Form._windowList.Count)
                    x = 0;
                ENV.UI.Form._windowList[x].TryFocus(delegate { });
            };
            Handlers.Add(Commands.PreviousWindow).Invokes += e =>
            {
                e.Handled = true;
                if (UI.Form._windowList.Count == 0)
                    return;
                var x = ENV.UI.Form._windowList.IndexOf(UI.Form._activeForm);
                x--;
                if (x < 0)
                    x = ENV.UI.Form._windowList.Count - 1;

                ENV.UI.Form._windowList[x].TryFocus(delegate { });
            };
            Handlers.Add(Commands.MoreWindows).Invokes += ee =>
            {
                ee.Handled = true;
                if (UI.Form._windowList.Count == 0)
                    return;

                var e = new Entity("More Windows", new MemoryDatabase());
                var nc = new NumberColumn("id", "3");
                var tc = new TextColumn("Window", "50");
                e.SetPrimaryKey(nc);
                e.Columns.Add(tc);
                var x = ENV.UI.Form._windowList.ToArray();
                for (int i = 0; i < x.Length; i++)
                {
                    e.Insert(() =>
                    {
                        nc.Value = i + 1;
                        tc.Value = x[i].Text;
                    });
                }
                var selected = new NumberColumn();
                selected.Value = ENV.UI.Form._windowList.IndexOf(UI.Form._activeForm) + 1;
                if (selected.Value < 1)
                    selected.Value = 1;
                var y = selected.Value;
                ENV.Utilities.SelectionList.Show(selected, e, nc, tc);
                if (selected > 0)
                {
                    try
                    {
                        if (y != selected.Value)
                            x[selected - 1].TryFocus(delegate { });
                    }
                    catch { }
                }
            };
            if (Common.EnableDeveloperTools || Common.ShouldAllowDeveloperToolsForCurrentUser != null)
            {

                var h = Handlers.Add(ControllersListShortcut);
                h.Scope = HandlerScope.UnhandledCustomCommandInModule;
                h.Invokes += e =>
                {
                    e.Handled = true;
                    if (Common.CanShowDeveloperTools())
                        ENV.Utilities.ProgramRunner.ShowAllPrograms(this);
                };
                h = Handlers.Add(ModelsListShortcut);
                h.Scope = HandlerScope.UnhandledCustomCommandInModule;
                h.Invokes += e =>
                {
                    e.Handled = true;
                    if (Common.CanShowDeveloperTools())
                        ENV.Utilities.EntityBrowser.ShowEntityBrowser(this);
                };
                h = Handlers.Add(Keys.Control | Keys.Shift | Keys.P);
                h.Scope = HandlerScope.UnhandledCustomCommandInModule;
                h.Invokes += e =>
                {
                    e.Handled = true;
                    if (Common.CanShowDeveloperTools())
                        ENV.Utilities.Profiler.ToggleProfilerWithMessage();
                };
            }

            {
                var h = Handlers.Add(DebugLocationShortcut);
                h.Invokes += e => ErrorLog.ShowCurrentLocation();
                h.Scope = HandlerScope.CurrentContext;
            }
            {
                Commands.BreakAll.Shortcut = BreakAllShortcut;
                Handlers.Add(Commands.BreakAll).Invokes += e =>
                {

                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                        e.Handled = true;
                    }
                    else
                        Message.ShowWarning("Visual Studio is not attached");
                };
            }
            Handlers.Add(ENV.Commands.PerformButtonClick).Invokes += e =>
            {
                var t = u.GetTaskByGeneration(0);
                if (t != null && t.View != null)
                {
                    var b = t.View.FocusedControl as Firefly.Box.UI.Button;
                    if (b != null)
                    {
                        b.PerformClick();
                    }
                }
            };

            var helpHamdler = Handlers.Add(Command.Help);
            helpHamdler.Scope = HandlerScope.UnhandledCustomCommandInModule;
            helpHamdler.Invokes +=
                e =>
                {
                    var t = Firefly.Box.Context.Current.ActiveTasks;
                    if (t.Count > 0)
                    {
                        var uic = t[t.Count - 1];
                        var f = uic.View;
                        if (f != null)
                        {
                            {
                                var c = f.FocusedControl as ICanShowCustomHelp;
                                if (c != null)
                                {
                                    if (c.CustomHelp != null)
                                    {
                                        c.CustomHelp.Show();
                                        e.Handled = true;
                                        return;
                                    }
                                }
                            }
                            {
                                var con = f.FocusedControl as InputControlBase;
                                if (con != null)
                                {
                                    var c = con.GetColumn() as ICanShowCustomHelp;
                                    if (c != null)
                                    {
                                        if (c.CustomHelp != null)
                                        {
                                            c.CustomHelp.Show();
                                            e.Handled = true;
                                            return;
                                        }
                                    }
                                }
                            }
                            {
                                var c = f as ICanShowCustomHelp;
                                if (c != null)
                                {
                                    if (c.CustomHelp != null)
                                    {
                                        c.CustomHelp.Show();
                                        e.Handled = true;
                                        return;
                                    }
                                }
                            }

                        }
                    }

                };

            var selectAppHandler = Handlers.Add(ENV.Commands.SelectApplicationFromList);
            {
                var appFileName = new appPathColumn();
                selectAppHandler.Parameters.Add(appFileName);
                selectAppHandler.Invokes += e =>
                {
                    e.Handled = true;
                    Utilities.ApplicationSelectionManager.UserAskedToSelectApplication(() => Raise(Command.ExitApplication), appFileName);
                };
            }

            Handlers.Add(ENV.Commands.ToggleInsertOverwriteMode).Invokes +=
                args =>
                {
                    Firefly.Box.UI.TextBox.InsertKeyMode = Firefly.Box.UI.TextBox.InsertKeyMode == InsertKeyMode.Overwrite ?
                        InsertKeyMode.Insert : InsertKeyMode.Overwrite;
                    Common.UpdateInsertOverwriteIndicator();
                };
            var ConfirmExit = Handlers.Add(Command.Exit);
            ConfirmExit.Scope = HandlerScope.CurrentTaskOnly;
            ConfirmExit.Invokes +=
                e =>
                {
                    e.Handled = true;
                    Firefly.Box.Context.Current.DiscardPendingCommands();
                    if (userConfirmedExit || ENV.Common.ShowYesNoMessageBox(LocalizationInfo.Current.ExitApplication, LocalizationInfo.Current.ConfirmExitApplication, false))
                    {
                        userConfirmedExit = true;
                        Raise(Command.ExitApplication);
                    }
                };
            if (MapCustomCommand20ToGoToNextControl)
            {
                var x = Handlers.Add(ENV.Commands.CustomCommand_20);
                x.Invokes += e => Raise(Command.GoToNextControl);
                x.Scope = HandlerScope.UnhandledCustomCommandInModule;
            }
            if (MapEnterToCustomCommand20)
            {
                var h = Handlers.Add(Keys.Enter);
                h.BindEnabled(() =>
                {

                    if (Command.Select.Enabled)
                        return false;
                    var tasks = Firefly.Box.Context.Current.ActiveTasks;
                    var uic = tasks[tasks.Count - 1] as UIController;
                    if (uic != null)
                    {
                        return !(uic.View.FocusedControl is Firefly.Box.UI.Button);
                    }
                    return true;

                });
                h.Invokes += e =>
                {
                    e.Handled = true;
                    Raise(ENV.Commands.CustomCommand_20);
                };
            }
            if (HandleF10SoIWontGetPropogatedToWindows)
                Handlers.Add(Keys.F10).Invokes += e => e.Handled = true;

            Handlers.Add(Commands.CloseAllTasks).Invokes +=
                args => Context.Current.CloseAllTasks();
        }
        public static bool MapCustomCommand20ToGoToNextControl { get; set; }
        public static bool MapEnterToCustomCommand20 { get; set; }
        public static bool HandleF10SoIWontGetPropogatedToWindows { get; set; }
        protected virtual void OnLoad() { }

        class appPathColumn : TextColumn, UserMethods.NotIncludedInVarIndexCalculations
        {
            public appPathColumn() : base("appPath") { }
        }
        internal protected void Exit(Func<bool> condition)
        {
            _moduleController.Exit(condition);
        }
        internal protected void Exit()
        {
            _moduleController.Exit(() => true);
        }

        internal List<ApplicationControllerBase> _referencedModules = new List<ApplicationControllerBase>();
        internal protected void AddReference(ApplicationControllerBase modulesApplicationController, bool loadImmediate)
        {
            _referencedModules.Add(modulesApplicationController);
            if (loadImmediate)
                _moduleController.AddReference(modulesApplicationController._moduleController);
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
                _prevIcon = ControllerBase.DefaultIcon.Value;
                ControllerBase.DefaultIcon.Value = value;
                _iconWasSet = true;
                Common.RunOnContextTopMostForm(x => ControllerBase.ApplyIcon(x));
            }
        }



        internal bool RunnedAsApplication
        {
            get { return _rootApplicationType == null || this.GetType() == _rootApplicationType; }
        }
        public static void SetRootApplicationType(Type type)
        {
            _rootApplicationType = type;
        }

        static internal string DefaultHelpFile;
        static Type _rootApplicationType;
        protected void Run(System.Windows.Forms.Form mdi, Func<ApplicationControllerBase> appFactory)
        {
            if (UserSettings.DoNotDisplayUI)
            {
                Context.Current.SetNonUIThread();
                Common.SuppressDialogs();
            }
            if (Name != null)
                UserMethods.SetSys(Name);

            _rootApplicationType = this.GetType();
            _everLoaded = false;

            Common.SetRootMDI(mdi);
            if (!string.IsNullOrEmpty(DefaultHelpFile))
            {
                var hp = new HelpProvider();
                hp.HelpNamespace = PathDecoder.DecodePath(DefaultHelpFile);
                hp.SetHelpNavigator(mdi, HelpNavigator.TableOfContents);
            }

            foreach (System.Windows.Forms.Control c in mdi.Controls)
            {
                var mdiClient = c as MdiClient;
                if (mdiClient != null)
                {
                    mdiClient.Paint +=
                        (sender, args) =>
                        {
                            if (_mdiImage != null)
                                Common.MDIPaintBackgroundImage(mdiClient, args, _mdiImage, MDIImageLayout);
                        };
                    mdiClient.Resize +=
                        (sender, args) =>
                        {
                            if (_mdiImage != null)
                                mdiClient.Refresh();
                        };
                }
            }

            Context.Current.RunAsMDI(mdi, () => PrivateRun(appFactory));

            _rootApplicationType = null;
            foreach (var app in _referencedModules)
            {
                Context.Current[app.GetType()] = null;
                Context.Current[app.GetType().BaseType] = null;
            }

        }

        void PrivateRun(Func<ApplicationControllerBase> appFactory)
        {
            if (_rootApplication != null)
                throw new InvalidOperationException("Only one root app can be used");
            _rootApplication = appFactory;
            try
            {
                ENV.MenuManager.DoOnMenuManagers(m => m.ContextState.Apply());
                Execute();
            }
            finally
            {
                _rootApplication = null;
            }
        }
        internal static bool _noMdi = false;
        protected void Run(Func<ApplicationControllerBase> appFactory)
        {
            _noMdi = true;
            if (UserSettings.DoNotDisplayUI)
            {
                Context.Current.SetNonUIThread();
                Common.SuppressDialogs();
            }
            _rootApplicationType = this.GetType();
            _everLoaded = false;
            Common.SetCurrentContextAsRoot();
            Exit();
            PrivateRun(appFactory);
        }

        public static string MDIImagePath
        {
            get { return _mdiImagePath; }
            set
            {
                _mdiImagePath = value;
                if (string.IsNullOrEmpty(_mdiImagePath))
                    _mdiImage = null;
                else
                {
                    try
                    {
                        _mdiImage = Common.GetImage(_mdiImagePath);
                    }
                    catch (Exception e)
                    {
                        ErrorLog.WriteToLogFile(e, "Mdi Image");
                    }
                }
            }
        }

        public static Firefly.Box.UI.ImageLayout MDIImageLayout { get; set; }

        static System.Drawing.Image _mdiImage;
        static Func<ApplicationControllerBase> _rootApplication = null;
        public static void SetRootApplicationFactory(Func<ApplicationControllerBase> getRootApplication)
        {
            _rootApplication = getRootApplication;
        }
        protected virtual void Execute()
        {
            ENV.UserMethods.Instance.CtxSetName("Main");
            Context.FocusedContextChanged +=
                            previousFocusedContext =>
                            {
                                Commands.FilterRows.RefreshBoundMenus();
                                Commands.CustomOrderBy.RefreshBoundMenus();
                                Commands.ExportData.RefreshBoundMenus();
                                Commands.FindNextRow.RefreshBoundMenus();
                                Commands.FindRows.RefreshBoundMenus();
                                Commands.SelectOrderBy.RefreshBoundMenus();

                                Common.ResetStatusText();

                                if (Common.IsRootMDI(Common.ContextTopMostForm))
                                    ENV.MenuManager.DoOnMenuManagers(m => m.ContextState.Apply());
                                if (previousFocusedContext != null)
                                    Common.RaiseOnContext(previousFocusedContext, Commands.ContextLostFocus,
                                        new[] { UserMethods.GetContextName(Context.Current) });
                                Common.Raise(Commands.ContextGotFocus,
                                    previousFocusedContext != null ? UserMethods.GetContextName(previousFocusedContext) : "");
                            };
            ENV.Security.UserManager.CurrentUserChanged +=
                () =>
                {
                    if (Common.IsRootMDI(Common.ContextTopMostForm))
                        ENV.MenuManager.DoOnMenuManagers(m => m.ContextState.Apply());
                };
            _currentRunningApplication = this;
            ENV.MenuManager.DoOnMenuManagers(m => m.ContextState.Apply());
            try
            {
                _moduleController.Run();
            }
            finally
            {
                if (_iconWasSet)
                {
                    ControllerBase.DefaultIcon.Value = _prevIcon;
                    _prevIcon = null;

                }
            }
            _currentRunningApplication = null;
        }
        internal static ApplicationControllerBase _currentRunningApplication = null;
        protected Number Error(Text text, Text title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, Bool appendToLog)
        {
            return new Message(text, true)
            {
                Title = title,
                Buttons = buttons,
                Icon = icon,
                DefaultButton = defaultButton,
                AppendToLog = appendToLog
            }.Show();
        }


        protected Number Error(Text text)
        {
            return new Message(text, true).Show();
        }
        protected Number Warning(Text text, Text title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, Bool appendToLog)
        {
            return new Message(text, false)
            {
                Title = title,
                Buttons = buttons,
                Icon = icon,
                DefaultButton = defaultButton,
                AppendToLog = appendToLog
            }.Show();
        }
        protected Number Warning(Text text, MessageBoxButtons buttons)
        {
            return new Message(text, false)
            {

                Buttons = buttons,
            }.Show();
        }
        protected Number Warning(Text text, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton)
        {
            return new Message(text, false)
            {
                Buttons = buttons,
                DefaultButton = defaultButton,
            }.Show();
        }
        protected Number Warning(Text text, Text title)
        {
            return new Message(text, false)
            {
                Title = title,
            }.Show();
        }


        protected Number Warning(Text text)
        {
            return new Message(text, false).Show();
        }
        protected Number Warning(Text text, MessageBoxIcon icon)
        {
            return new Message(text, false)
            {
                Icon = icon,
            }.Show();
        }
        protected Number Warning(Text text, Bool appendToLog)
        {
            return new Message(text, false)
            {
                AppendToLog = appendToLog
            }.Show();
        }

        protected void ErrorInStatusBar(Text text)
        {
            Common.ErrorInStatusBar(text);
        }

        protected void WarningInStatusBar(Text text)
        {
            Common.WarningInStatusBar(text);
        }
        internal protected void RaiseOnContext(Text contextName, Command command, params object[] untypedArgs)
        {
            ControllerBase.RaiseOnContextInternal(contextName, command, untypedArgs);
        }
        internal protected void RaiseOnContext(Text contextName, Text command, params object[] untypedArgs)
        {
            ControllerBase.RaiseOnContextInternal(contextName, command, untypedArgs);
        }
        internal protected void RaiseOnContext(Text contextName, CommandWithArgs command)
        {
            ControllerBase.RaiseOnContextInternal(contextName, command.Command, command.Argmuents);
        }
        internal protected void Invoke(CommandWithArgs command)
        {

            _moduleController.Invoke(command);
        }
        internal protected void Raise(CommandWithArgs command)
        {

            _moduleController.Raise(command);
        }
        protected void Raise(Command command, params object[] untypedArgs)
        {
            ControllerBase.RaiseHappened(command);
            _moduleController.Raise(command, untypedArgs);
        }

        protected void Raise(Keys keys, params object[] untypedArgs)
        {
            ControllerBase.RaiseHappened(keys);
            _moduleController.Raise(keys, untypedArgs);
        }
        protected void Raise(string customCommandKey, params object[] untypedArgs)
        {
            ControllerBase.RaiseHappened(customCommandKey);
            _moduleController.Raise(HandlerCollectionWrapper.FixCustomCommandKey(customCommandKey), untypedArgs);
        }

        protected void Raise(CustomCommand command, params object[] untypedArgs)
        {
            _moduleController.Raise(command, untypedArgs);
        }
        protected void Invoke(string customCommandKey, params object[] untypedArgs)
        {
            _moduleController.Invoke(HandlerCollectionWrapper.FixCustomCommandKey(customCommandKey), untypedArgs);
        }


        protected void Invoke(CustomCommand command, params object[] untypedArgs)
        {
            _moduleController.Invoke(command, untypedArgs);
        }
        protected void Invoke(Command command, params object[] untypedArgs)
        {
            _moduleController.Invoke(command, untypedArgs);
        }

        protected void Invoke(System.Windows.Forms.Keys keyCombination, params object[] untypedArgs)
        {
            _moduleController.Invoke(keyCombination, untypedArgs);
        }
        protected void Raise<T>(Command command, ArrayColumn<T> untypedArgs)
        {
            ControllerBase.RaiseHappened(command);
            _moduleController.Raise(command, untypedArgs);
        }

        protected void Raise<T>(Keys keys, ArrayColumn<T> untypedArgs)
        {
            ControllerBase.RaiseHappened(keys);
            _moduleController.Raise(keys, untypedArgs);
        }
        protected void Raise<T>(string customCommandKey, ArrayColumn<T> untypedArgs)
        {
            ControllerBase.RaiseHappened(customCommandKey);
            _moduleController.Raise(HandlerCollectionWrapper.FixCustomCommandKey(customCommandKey), untypedArgs);
        }

        protected void Invoke<T>(string customCommandKey, ArrayColumn<T> untypedArgs)
        {
            _moduleController.Invoke(HandlerCollectionWrapper.FixCustomCommandKey(customCommandKey), untypedArgs);
        }


        protected void Invoke<T>(Command command, ArrayColumn<T> untypedArgs)
        {
            _moduleController.Invoke(command, untypedArgs);
        }

        protected void Invoke<T>(System.Windows.Forms.Keys keyCombination, ArrayColumn<T> untypedArgs)
        {
            _moduleController.Invoke(keyCombination, untypedArgs);
        }
        protected internal T Invoke<T>(Func<T> func)
        {
            T r = default(T);
            _moduleController.Invoke(() => r = func());
            return r;
        }

        HandlerCollectionWrapper _handlers;
        internal protected HandlerCollectionWrapper Handlers
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


        protected Func<Form> View
        {

            set
            {
                if (Common._suppressDialogForTesting || _noMdi)
                    return;
                Form f = null;
                int i = 0;
                //disable mdi changes for non main context
                foreach (var item in Firefly.Box.Context.ActiveContexts)
                {
                    if (i++ >= 1)
                        return;

                }
                Action applyIconToMdi = () => { };
                Context.Current.InvokeUICommand(
                    () =>
                    {
                        f = value();
                        f.DisableMDIChildZOrdering = true;
                    });
                if (UserSettings.VersionXpaCompatible && RunnedAsApplication)
                {
                    var ff = f as ENV.UI.Form;
                    if (ff != null)
                    {
                        ff.FitToMDI = true;
                        ff.TitleBar = false;
                        Common.RunOnContextTopMostForm(
                            mdi =>
                            {
                                if (!mdi.IsMdiContainer) return;

                                mdi.Text = ff.Text;
                                string s = ff.GetBindTextValue();
                                if (!string.IsNullOrEmpty(s))
                                    mdi.Text = s;
                                mdi.MaximizeBox = ff.MaximizeBox;
                                ff.TextChanged += delegate
                                {
                                    if (!string.IsNullOrEmpty(ff.Text))
                                        mdi.Text = ff.Text;
                                };
                                applyIconToMdi = () => Common.RunOnContextTopMostForm(x => ControllerBase.ApplyIcon(x));
                            });
                        ff.BindClientHeight += (o, a) =>
                        {
                            if (ff.ClientSize.Height != a.Value)
                                Common.RunOnContextTopMostForm(mdi => mdi.Height = Math.Max(mdi.MinimumSize.Height, mdi.Height + a.Value - ff.ClientSize.Height));
                        };
                        ff.BindClientWidth += (o, a) =>
                        {
                            if (ff.ClientSize.Width != a.Value)
                                Common.RunOnContextTopMostForm(mdi => mdi.Width = Math.Max(mdi.MinimumSize.Width, mdi.Width + a.Value - ff.ClientSize.Width));
                        };
                        if (ff._TitleBarBound)
                        {
                            var titleBar = true;
                            ff.BindTitleBar += (o, a) =>
                            {
                                if (titleBar != a.Value)
                                {
                                    titleBar = a.Value;
                                    Common.RunOnContextTopMostForm(mdi =>
                                    {
                                        mdi.Text = titleBar ? ff.Text : "";
                                        mdi.ControlBox = titleBar;
                                    });
                                }
                                a.Value = false;
                            };
                        }
                    }
                }
                _moduleController.View = f;
                ControllerBase.ApplyIcon(f);
                applyIconToMdi();
            }
        }

        protected string Title
        {
            get { return _moduleController.Title; }
            set { _moduleController.Title = value; }
        }

        protected string Name { get; set; }


        protected bool ShowView
        {
            set { _moduleController.ShowView = value; }
            get { return _moduleController.ShowView; }
        }

        protected ApplicationEntityCollection _applicationEntities;
        internal protected RolesCollection _applicationRoles;
        public ApplicationEntityCollection AllEntities
        {
            get
            {
                if (_applicationEntities == null)
                    _applicationEntities = LoadAllEntitiesCollection();
                return _applicationEntities;
            }
        }

        protected virtual ApplicationEntityCollection LoadAllEntitiesCollection()
        {
            return new ApplicationEntityCollection();
        }
        protected virtual ProgramCollection LoadAllProgramsCollection()
        {
            return new ProgramCollection();
        }

        protected ProgramCollection _applicationPrograms;
        public ProgramCollection AllPrograms
        {
            get
            {
                if (_applicationPrograms == null)
                    _applicationPrograms = LoadAllProgramsCollection();
                return _applicationPrograms;
            }
        }



        protected EntityCollection Entities
        {
            get
            {

                return _moduleController.Entities;
            }
        }

        protected internal ColumnCollection Columns
        {
            get { return _moduleController.Columns; }
        }

        ContextStatic<bool> _processingWebRequest = new ContextStatic<bool>(() => false);

        class ProxyRequest : IO.WebWriter.IRequestInfo
        {
            Dictionary<string, string[]> _args = new Dictionary<string, string[]>();

            public ProxyRequest(object[] vals)
            {

                foreach (var item in vals)
                {
                    var sa = item as string[];
                    if (sa != null && sa.Length > 1)
                    {
                        var ssa = new string[sa.Length - 1];
                        Array.Copy(sa, 1, ssa, 0, sa.Length - 1);
                        _args.Add(sa[0].ToUpper(CultureInfo.InvariantCulture), ssa);
                    }
                }
            }

            public string GetPrgName()
            {
                return GetValues("PRGNAME")[0];
            }

            public HttpPostedFile GetFile(string key)
            {
                return null;
            }

            public HttpCookie GetCookie(string key)
            {
                return null;
            }

            public string[] GetValues(string key)
            {
                key = key.ToUpper(CultureInfo.InvariantCulture);
                string[] result = null;
                if (_args.TryGetValue(key, out result))
                    return result;
                return null;
            }

            public string[] GetFormValues(string key)
            {
                return GetValues(key);
            }

            public bool IsMethodPost { get; private set; }
        }
        public void ProcessWebRequest()
        {
            try
            {
                var x = System.Web.HttpContext.Current.Request.Headers["RequestProxy"];
                if (x == "Y")
                {
                    var s = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    var values = s.Deserialize(System.Web.HttpContext.Current.Request.InputStream) as object[];

                    IO.WebWriter.RequestInfo.Value = new ProxyRequest(values);

                }
                ProcessWebRequest(IO.WebWriter.RequestInfo.Value.GetPrgName());
            }
            catch (ProgramCollection.ProgramNameNotFoundException e)
            {
                if (System.Web.HttpContext.Current != null)
                    System.Web.HttpContext.Current.Response.Write(e.Message);
            }
            catch (ProgramCollection.NoProgramNameSpecifiedException e)
            {
                if (System.Web.HttpContext.Current != null)
                    System.Web.HttpContext.Current.Response.Write(e.Message);
            }
        }
        public void ProcessWebRequest(string prgName)
        {
            Firefly.Box.Context.Current.SetNonUIThread();
            _processingWebRequest.Value = true;


            try
            {
                RunWebProgram(prgName, GetWebRequestParameters());
                if (!WebWriter.WasThereAnOutput() && HttpContext.Current != null && HttpContext.Current.Response != null)
                {
                    if (ENV.IO.TextTemplate.TemplateError.Value != null)
                        throw new Exception(ENV.IO.TextTemplate.TemplateError.Value);
                    else
                        HttpContext.Current.Response.Write("No output returned");
                }
            }
            finally
            {
                Context.Current.Dispose();
            }
        }

        internal static WebParameterProvider GetWebRequestParameters()
        {
            return new WebParameterProvider(ExtractArgsFrom(
                key =>
                {
                    try
                    {
                        var result = IO.WebWriter.GetRequestValues(key);
                        if (result != null && result.Length > 0)
                            return result[0];
                        var file = IO.WebWriter.RequestInfo.Value.GetFile(key);

                        if (file != null)
                        {
                            return
                                UserMethods.StreamToByteArray(
                                    file.InputStream);
                        }
                        return null;
                    }
                    catch (Exception e)
                    {
                        ErrorLog.WriteToLogFile(e, "");
                        return null;
                    }
                }));
        }

        internal delegate object ValueExtractor(string key);
        internal static object[] ExtractArgsFrom(ValueExtractor extractor)
        {
            var args = new List<object>();
            string arguments = extractor("arguments") as string;
            if (arguments != null)
            {
                arguments = HttpUtility.UrlDecode(arguments);
                foreach (string var in arguments.Split(','))
                {
                    var s = var.TrimStart();
                    if (s == "")
                        continue;
                    if (s.StartsWith("-A"))
                    {
                        args.Add(s.Substring(2).Replace(@"\\", @"\"));
                    }
                    else if (s.StartsWith("-"))
                    {
                        args.Add(s.Substring(2));
                    }
                    else
                        args.Add(extractor(s));
                }
                if (args.Count == 0)
                    args.Add(null);
            }
            return args.ToArray();
        }

        internal void RunWebProgram(string publicName, IWebParametersProvider args)
        {

            AllPrograms.RunWebProgram(publicName, args);
        }



        protected RelationCollection Relations
        {
            get { return _moduleController.Relations; }
        }



        static ContextStaticDictionary<string, ApplicationControllerBase> _knownUnreferencedApplications =
            new ContextStaticDictionary<string, ApplicationControllerBase>();

        public static string DefaultUnreferencedDll;
        static string _mdiImagePath;
        static string GetFileNameWithoutExtention(string s)
        {
            if (s.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase) || s.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
                return System.IO.Path.GetFileNameWithoutExtension(s);
            return s;
        }
        [Obsolete("Use RunControllerFromAnUnreferencedApplication instead - it also handles current application when application key is empty")]
        public static object RunProgramFromAnUnreferencedApplication(Text applicationKey, string publicName, params object[] args)
        {
            object returnValue = null;
            InternalRunProgramFromAnUnreferencedApplication(applicationKey, publicName, app => returnValue = app.AllPrograms.RunByPublicName(publicName, args), DoOnEstimatedCurrentRunningApplicationController);
            return returnValue;
        }
        [Obsolete("Use RunControllerFromAnUnreferencedApplication instead - it also handles current application when application key is empty")]
        public static object RunProgramFromAnUnreferencedApplication(Text applicationKey, int index, params object[] args)
        {
            object returnValue = null;
            InternalRunProgramFromAnUnreferencedApplication(applicationKey, "index " + index, app => returnValue = app.AllPrograms.RunByIndex(index, args), DoOnEstimatedCurrentRunningApplicationController);
            return returnValue;
        }
        public object RunControllerFromAnUnreferencedApplication(Text applicationKey, string publicName, params object[] args)
        {
            object returnValue = null;
            InternalRunProgramFromAnUnreferencedApplication(applicationKey, publicName, app => returnValue = app.AllPrograms.RunByPublicName(publicName, args), x => x(this));
            return returnValue;
        }
        static void DoOnEstimatedCurrentRunningApplicationController(Action<ApplicationControllerBase> doOnProgramCollection)
        {

            var t = ENV.UserMethods.Instance.GetTaskByGeneration(0);
            ProgramCollection result = _currentRunningApplication.AllPrograms;

            ControllerBase.SendInstanceBasedOnTaskAndCallStack(t, c =>
            {
                doOnProgramCollection(c._application);
            }, app =>
            {
                doOnProgramCollection(app);
            });

        }
        public object RunControllerFromAnUnreferencedApplication(Text applicationKey, int index, params object[] args)
        {
            object returnValue = null;
            InternalRunProgramFromAnUnreferencedApplication(applicationKey, "index " + index, app => returnValue = app.AllPrograms.RunByIndex(index, args), x => x(this));
            return returnValue;
        }
        internal static Dictionary<string, string> _unreferencedEcfMap = new Dictionary<string, string>();
        public static void AddEcfMapping(string ecfName, string assemblyName)
        {
            _unreferencedEcfMap.Add(ecfName.ToUpper(), assemblyName.ToUpper());
        }
        public static bool FindUnreferencedApplicationsInOtherPaths = false;
        internal static void InternalRunProgramFromAnUnreferencedApplication(Text applicationKey, string program, Action<ApplicationControllerBase> runThis, Action<Action<ApplicationControllerBase>> provideCurrentApp)
        {
            try
            {
                if (Text.IsNullOrWhiteSpace(applicationKey))
                {

                    provideCurrentApp(runThis);
                    return;
                }
                var key = PathDecoder.DecodePath(applicationKey).TrimEnd().ToUpper(CultureInfo.InvariantCulture);
                if (!FindUnreferencedApplicationsInOtherPaths)
                    key = System.IO.Path.GetFileNameWithoutExtension(key);
                else
                    key = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(key), System.IO.Path.GetFileNameWithoutExtension(key));
                {
                    string newKey;
                    if (_unreferencedEcfMap.TryGetValue(key.ToUpper(), out newKey))
                        key = newKey;
                    if (_unreferencedEcfMap.TryGetValue(System.IO.Path.GetFileNameWithoutExtension(key).ToUpper(), out newKey))
                        key = newKey;
                }
                {
                    var iniKey = ENV.UserMethods.Instance.IniGet("[MapMff2Dll]" + key);
                    if (!Text.IsNullOrEmpty(iniKey))
                        key = iniKey;
                    else
                    {
                        iniKey = ENV.UserMethods.Instance.IniGet("[MapMff2Dll]" + System.IO.Path.GetFileNameWithoutExtension(key).ToUpper());
                        if (!Text.IsNullOrEmpty(iniKey))
                            key = iniKey;

                        else
                        {
                            key = key.Replace(" ", "");
                            if (!string.IsNullOrEmpty(DefaultUnreferencedDll))
                                key = DefaultUnreferencedDll;
                        }
                    }
                }

                if (string.IsNullOrEmpty(key))
                    throw new ArgumentException("Couldn't find assembly for " + key);
                key = PathDecoder.DecodePath(key);
                ApplicationControllerBase app;
                if (!_knownUnreferencedApplications.TryGetValue(key, out app))
                {

                    if (key.Contains("\\"))
                    {
                        var y = System.IO.Path.GetDirectoryName(key);
                        key = GetFileNameWithoutExtention(key);
                        if (!_searchPathes.ContainsKey(key))
                            _searchPathes.Add(key, y);
                    }
                    else
                        key = GetFileNameWithoutExtention(key);
                    var ass = Assembly.Load(key);
                    bool found = false;
                    foreach (var type in ass.GetTypes())
                    {
                        if (type.Name == "Application" || type.Name == "ApplicationCore")
                        {
                            var p = type.GetProperty("Instance");
                            app = p.GetValue(null, new object[0]) as ApplicationControllerBase;
                            if (app == null)
                                throw new InvalidOperationException("Failed to load assembly " + key);
                            if (!_knownUnreferencedApplications.ContainsKey(key))
                                _knownUnreferencedApplications.Add(key, app);
                            found = true;
                            break;
                        }

                    }
                    if (!found)
                        throw new Exception("Couldn't find application class");
                }
                runThis(app);
                return;
            }
            catch (Exception e)
            {
                string message = string.Format("Failed to call external program {0}: {1}, {2}", applicationKey.TrimEnd(), program.TrimEnd(), e.Message);
                ErrorLog.WriteToLogFile(e, message);
                Message.ShowWarningInStatusBar(message);
            }
        }



        internal static void RunOnRootApplication(Action action)
        {
            var mc = _rootApplication()._moduleController;
            if (mc != null)
                mc.RunAction(action);
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

        ControllerBase.ControllerColumns _columns;
        internal static System.Action _afterEnd = () => { };
        internal static void SendColumnsOf(ModuleController mc, Action<ControllerBase.ControllerColumns> to)
        {
            ApplicationControllerBase y;
            if (_activeControllers.TryGetValue(mc, out y))
            {
                if (y._columns == null)
                {
                    y._columns = new ControllerBase.ControllerColumns(y);
                }

                to(y._columns);
            }
        }

        internal static void ForEachActiveController(Action<ModuleController> doThis)
        {
            foreach (var item in new List<ModuleController>(_activeControllers.Keys))
                doThis(item);
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
                    result.Run(args);
                }
            }, changeReasonColumn, previousValueColumn, ProvideSubForms, condition);
            return result;
        }

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
            return MonitorValueChanged(c, null, null, null);
        }
        internal protected Common.WhenValueChange<DataType> MonitorValueChanged<DataType>(TypedColumnBase<DataType> c, NumberColumn changeReasonColumn, Func<bool> condition)
        {
            return MonitorValueChanged(c, changeReasonColumn, null, condition);
        }
        internal protected Common.WhenValueChange<DataType> MonitorValueChanged<DataType>(TypedColumnBase<DataType> c, Func<bool> condition)
        {
            return MonitorValueChanged(c, null, null, condition);
        }
        [Obsolete("Had spelling mistake, use GetMenusNames")]
        protected virtual string[] GetMenueNames()
        {
            return new string[0];
        }
        protected virtual string[] GetMenusNames()
        {
            return GetMenueNames();
        }

        internal string[] _getMenuNames()
        {
            return GetMenusNames();
        }

        public static void AddAssemblySearchPath(string assemblName, string searchDirectory = "")
        {
            _searchPathes.Add(assemblName, searchDirectory);
        }

        internal void addToEvaluateExpression(Action<object> add)
        {
            add(this);
            foreach (var item in _referencedModules)
            {
                item.addToEvaluateExpression(add);
            }
        }
    }
}
