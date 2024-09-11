using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENV.Data;

namespace ENV.Utilities
{
    public class ApplicationSelectionManager
    {
        static ApplicationSelectionManager()
        {

        }
        public static bool AddEmptyLineForKbPut = true;
        static List<string> _applications = new List<string>();
        internal static void UserAskedToSelectApplication(Action raiseExitApplication, Firefly.Box.Text appName)
        {
            bool done = false;
            if (_applications.Count > 0)
            {
                done = true;
                var e = new Entity("Applications", new ENV.Data.DataProvider.MemoryDatabase());
                var number = new NumberColumn("Number", "2Z");
                var Name = new TextColumn("Name", "30");
                e.SetPrimaryKey(number);
                e.Columns.Add(Name);
                var num = 0;
                if (AddEmptyLineForKbPut)
                    e.Insert(() => number.Value = 0);
                else num= 1;
                foreach (var item in _applications)
                {
                    e.Insert(() =>
                    {
                        number.Value = num++;
                        Name.Value = UserSettings.Get("MAGIC_SYSTEMS", "SYSTEM" + number.Value.ToString().Trim()).Split(',')[0];
                    });
                }
                var selected = new NumberColumn() { Value = UserSettings._startApplication };
                Utilities.SelectionList.Show(selected, e, number, Name);
                try
                {
                    var m = GetRunMethod(selected);
                    if (m != null)
                    {
                        ENV.UserSettings.ParseAndSet("STARTAPPLICATION=" + selected.ToString().Trim(), false, false, true);
                        _applicationRunMethod = m;
                        _rerun = true;
                        raiseExitApplication();
                    }
                }
                catch (Exception ex)
                {
                    ENV.Common.ShowExceptionDialog(ex, true, "");
                }
            }

            var y = ENV.UserSettings.ParseAndGet("StartApplication");
            if (!string.IsNullOrEmpty(y) && appName.ToLower() == y.ToLower())
            {
                done = true;
                _rerun = true;
            }
            if (!done)
                if (!Firefly.Box.Text.IsNullOrEmpty(appName))
                {
                    done = RunAppByFileName(raiseExitApplication, appName);
                    if (done)
                    {
                        ENV.UserSettings.ParseAndSet("StartApplication=*" + appName, false, false, true);
                        return;
                    }
                }
            var executingPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            while (!done)
            {

                var s = ENV.UserMethods.Instance.FileDlg("Application", System.IO.Path.Combine(executingPath, "*.exe"));
                if (Firefly.Box.Text.IsNullOrEmpty(s))
                    return;
                if (!string.Equals(executingPath, System.IO.Path.GetDirectoryName(s), StringComparison.InvariantCultureIgnoreCase))
                {
                    ENV.Message.ShowWarning("Can only open an application from the same directory: " + executingPath);
                    continue;
                }
                done = RunAppByFileName(raiseExitApplication, s);
                if (!done)
                    ENV.Message.ShowWarning("Failed to open " + s);
            }




        }

        private static bool RunAppByFileName(Action raiseExitApplication, Firefly.Box.Text appName)
        {
            bool done = false;
            ApplicationControllerBase.InternalRunProgramFromAnUnreferencedApplication(appName, "Application", app =>
            {
                try
                {
                    var m = app.GetType().GetMethod("Run");
                    if (m != null)
                    {

                        _applicationRunMethod = m;
                        _rerun = true;
                        raiseExitApplication();
                    }

                }
                catch { }
                done = true;


            }, x => { });
            return done;
        }

        static System.Reflection.MethodInfo _applicationRunMethod;
        static bool _rerun = true;
        public static void RunAndAllowSelectFrom(Action runMethod, params string[] applications)
        {
            _applicationRunMethod = runMethod.Method;
            if (applications.Length > 0)
                ENV.UserSettings.ParseAndSet("STARTAPPLICATION=" + (Array.IndexOf(applications, runMethod.Method.DeclaringType.Namespace) + 1).ToString().Trim(), false, false, true);
            RunAndAllowSelectFrom(applications);
        }
        public static void RunAndAllowSelectFrom(params string[] applications)
        {
            if (applications != null)
                _applications.AddRange(applications);
            if (_applications.Count == 0)
            {
                var i = 1;
                string systemLine;
                while (!string.IsNullOrWhiteSpace(systemLine = UserSettings.Get("MAGIC_SYSTEMS", "SYSTEM" + i.ToString().Trim())))
                {
                    var args = systemLine.Split(',');
                    var app = System.IO.Path.GetFileNameWithoutExtension(args[1]) + "ctl";
                    if (!string.IsNullOrWhiteSpace(args[2]))
                    {
                        var FullAppName = System.IO.Path.GetFileNameWithoutExtension(args[2]);
                        string[] appName = FullAppName.Split(new Char[] { '%', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                        app = appName[appName.Length - 1];
                    }
                    foreach (var item in ApplicationControllerBase._unreferencedEcfMap)
                    {
                        if (item.Key.Equals(app, StringComparison.InvariantCultureIgnoreCase))
                            app = item.Value;
                    }
                    _applications.Add(app);
                    i++;
                }
            }
            if (_applicationRunMethod == null)
            {
                _applicationRunMethod = GetRunMethod(UserSettings._startApplication);
                if (_applicationRunMethod == null)
                    throw new Exception("Couldn't find the configuration for application " + UserSettings._startApplication);
            }
            ApplicationControllerBase._afterEnd = () =>
            {
                var bp = new Firefly.Box.BusinessProcess { UserInterfaceRefreshRowsInterval = 1, UserInterfaceRefreshInterval = 1, AllowUserAbort = true };
                var tc = new TextColumn();
                var h = bp.Handlers.Add(Commands.SelectApplicationFromList);
                h.Parameters.Add(tc);
                h.Invokes += e =>
                {
                    e.Handled = true;
                    UserAskedToSelectApplication(() => { }, tc.Trim());
                };
                bp.ForEachRow(() =>
                {
                    if (bp.Counter == 5)
                        bp.Exit();
                });
            };
            while (_rerun)
            {
                _rerun = false;
                Firefly.Box.Context.Current[_applicationRunMethod.DeclaringType] = null;
                Firefly.Box.Context.Current[_applicationRunMethod.DeclaringType.BaseType] = null;
                _applicationRunMethod.Invoke(null, new object[0]);
            }
        }

        private static System.Reflection.MethodInfo GetRunMethod(int appNumber)
        {
            var index = appNumber - 1;
            if (index < 0 || index >= _applications.Count)
                return null;
            else
            {

                var ass = System.Reflection.Assembly.Load(_applications[index]);
                foreach (var item in ass.GetTypes())
                {
                    if ((item.Name == "Application" || item.Name == "ApplicationCore") && !item.IsAbstract)
                    {
                        return item.GetMethod("Run");
                    }
                }
            }
            return null;
        }


    }
}
