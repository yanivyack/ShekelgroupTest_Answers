using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using ENV.Data;
using ENV.Data.DataProvider;
using ENV.UI;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Utilities
{
    public class ProgramRunner
    {
        Func<object> _factory;
        Type _type;
        string _name;

        bool _profile;
        public ProgramRunner(Func<object> factory, string name, bool profile)
        {
            _profile = profile;
            _factory = factory;
            _type = factory().GetType();
            _name = name;
            if (string.IsNullOrEmpty(_name))
                _name = _type.Name;
        }
        public static void ShowAllPrograms(ApplicationControllerBase app)
        {
            EntityBrowser.ShowApplicationComponentsAndDo(app, "Show Programs",
                x => new ShowPrograms(app).Run(x),
                x => new ShowPrograms(app).Run(x));
        }

        class ShowPrograms
        {
            class programsListEntity : ENV.Data.Entity
            {
                [PrimaryKey]
                internal readonly NumberColumn id = new NumberColumn("ID", "5");
                internal readonly NumberColumn nc = new NumberColumn("index", "5Z");
                internal readonly TextColumn caption = new TextColumn("caption", "100");
                internal readonly TextColumn publicName = new TextColumn("publicName", "100");
                internal readonly TextColumn TypeName = new TextColumn("TypeName", "40", "ClassName");
                internal readonly TextColumn LongTypeName = new TextColumn("LongTypeName", "200", "LongClassName");
                public programsListEntity(IEntityDataProvider source)
                    : base("ProgramList", "Controllers (Programs)", source)
                {
                }

            }

            ApplicationControllerBase _app;

            programsListEntity e = new programsListEntity(new MemoryDatabase());

            Dictionary<int, Func<object>> tasks = new Dictionary<int, Func<object>>();

            RecentManager _recent;

            public ShowPrograms(ApplicationControllerBase app)
            {
                this._app = app;


                _recent = new RecentManager(_app.GetType().FullName + ".Programs.recent", e.LongTypeName);


            }



            public void Run(ApplicationControllerBase app)
            {

                _recent = new RecentManager(_app.GetType().FullName + ".Programs.recent", e.LongTypeName);
                var dict = new Dictionary<string, ProgramInfo>();

                app.AllPrograms.ProvideProgramsTo(
                    (i, s, name1, factory, typeFullName) =>
                    {
                        if (!dict.ContainsKey(typeFullName))
                            dict.Add(typeFullName, new ProgramInfo { Number = i, Caption = s, PublicName = name1 });
                        AddOneController(i, s, name1, factory, typeFullName);

                    });

                new AssemblySearcher(typeof(ControllerBase),
                    item =>
                    {

                        var key = item.FullName;
                        if (key.EndsWith("Core"))
                            key = key.Remove(key.Length - 4);
                        ProgramInfo i = null;
                        if (!dict.TryGetValue(key, out i))
                            AddOneController(0, item.Name, "", () => System.Activator.CreateInstance(item), item.FullName);


                    }).Search(app.GetType().Assembly);

                ShowList();
            }
            public void Run(Assembly ass)
            {

                _recent = new RecentManager(ass.GetName() + ".dll.Programs.recent", e.LongTypeName);
                var dict = new Dictionary<string, ProgramInfo>();

                _app.AllPrograms.ProvideProgramsTo(
                    (i, s, name1, factory, typeFullName) =>
                    {
                        if (!dict.ContainsKey(typeFullName))
                            dict.Add(typeFullName, new ProgramInfo { Number = i, Caption = s, PublicName = name1 });


                    });

                new AssemblySearcher(typeof(ControllerBase),
                    item =>
                    {

                        var key = item.FullName;
                        if (key.EndsWith("Core"))
                            key = key.Remove(key.Length - 4);
                        ProgramInfo i = null;
                        if (!dict.TryGetValue(key, out i))
                        {
                            foreach (var intr in item.GetInterfaces())
                            {
                                if (i == null && intr.Assembly != typeof(ProgramRunner).Assembly)
                                {
                                    dict.TryGetValue(intr.FullName, out i);
                                }
                            }


                            if (i == null)
                                AddOneController(0, item.Name, "", () => System.Activator.CreateInstance(item), item.FullName);
                        }
                        if (i != null)
                            AddOneController(i.Number, i.Caption, i.PublicName, () => System.Activator.CreateInstance(item), item.FullName);


                    }).Search(ass);

                ShowList();
            }

            private void ShowList()
            {
                var eb = new EntityBrowser(e);
                eb.OrderBy.Add(e.nc, e.LongTypeName);
                Action<bool> runTheProgram = profile =>
                {
                    _recent.SaveRecent();
                    try { new ProgramRunner(tasks[e.id], e.caption.Trim(), profile).ShowOverloads(); }
                    catch { }
                };
                eb.AddAction("Run", () => runTheProgram(false), true);
                eb.AddAction("Profile", () => runTheProgram(true));
                eb.AddAction("Copy code path", () =>
                {
                    Clipboard.SetText(e.LongTypeName.Trim().Replace("+", "."));
                });
                eb.AddColumns(e.nc, e.caption, e.publicName, e.TypeName, e.LongTypeName);

                eb.MapKey(Keys.F7, () => runTheProgram(false));
                eb.MapKey(Keys.F8, () => runTheProgram(true));
                _recent.StartOnRow(eb.StartOnRowWhere);

                eb.Run();
            }

            int j = 0;


            class ProgramInfo
            {
                public int Number { get; set; }
                public string Caption { get; set; }
                public string PublicName { get; set; }
            }

            private void AddOneController(int programNumber, string desc, string publicName, Func<object> factory, string typeFullName)
            {
                var z = ++j;
                tasks.Add(j, factory);
                new BusinessProcess { From = e, Activity = Activities.Insert }.
                    ForFirstRow(
                        () =>
                        {
                            e.id.Value = z;
                            e.nc.Value = programNumber;
                            e.caption.Value = desc;
                            this.e.publicName.Value = publicName;
                            e.LongTypeName.Value = typeFullName;
                            e.TypeName.Value = typeFullName.Substring(typeFullName.LastIndexOf('.') + 1);


                        });
            }
        }


        void ShowOverloads()
        {

            try
            {
                var runMethods = new List<MethodInfo>();
                foreach (MethodInfo method in _type.GetMethods())
                {
                    if (method.Name == "Run")
                    {
                        runMethods.Add(method);
                    }
                }
                switch (runMethods.Count)
                {
                    case 0:
                        if (Common.ShowYesNoMessageBox("", "Run The Program?", true))
                        {
                            var bs = _factory() as ControllerBase;
                            if (bs != null)
                                Run(y => bs.Execute(), false);
                        }
                        break;
                    case 1:

                        UseRunMethod(runMethods[0], true);

                        break;
                    default:
                        {
                            var e = new Entity("overlloads", _name + " Overloads", new MemoryDatabase());
                            var nc = new NumberColumn("index", "5");
                            var numOfParameters = new NumberColumn("Parameters", "5");
                            var caption = new TextColumn("description", "100");
                            e.Columns.Add(nc, numOfParameters, caption);
                            e.SetPrimaryKey(nc);
                            int i = 0;
                            foreach (var methodInfo in runMethods)
                            {
                                new BusinessProcess { From = e, Activity = Activities.Insert }.ForFirstRow(
                                    () =>
                                    {
                                        nc.Value = i++;
                                        var pars = methodInfo.GetParameters();
                                        numOfParameters.Value = pars.Length;
                                        var s = "";
                                        foreach (var parameterInfo in pars)
                                        {
                                            if (s.Length > 0)
                                                s += ", ";
                                            s += parameterInfo.Name;
                                        }
                                        caption.Value = s;
                                    });
                            }
                            var eb = new EntityBrowser(e);
                            eb.AddAction("Run", () =>
                            {
                                UseRunMethod(runMethods[nc], false);
                            }, true);
                            eb.Run();
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Common.ShowExceptionDialog(e, true, "");
            }
        }

        void Run(Action<Action<object>> what, bool askToRun)
        {
            if (!askToRun || Common.ShowYesNoMessageBox("", "Run The Program?", true))
            {
                if (_profile)
                    ENV.Utilities.Profiler.Start();
                bool showDone = !_profile;
                Action<UIController> uicStarts = x => { showDone = false; };
                AbstractUIController.StartOfInstance += uicStarts;
                try
                {
                    object result = null;
                    var sw = new System.Diagnostics.Stopwatch();

                    sw.Start();

                    what(y => result = y);
                    sw.Stop();

                    var s = "Done in " + sw.Elapsed.ToString();
                    if (result != null)
                    {
                        s += "\r\nResult: " + result;
                    }
                    if (showDone)
                        ENV.Common.ShowMessageBox(_name, MessageBoxIcon.Information, s);
                }
                catch (Exception ex)
                {

                    ENV.Common.ShowExceptionDialog(ex, true, _name);
                }
                finally
                {
                    AbstractUIController.StartOfInstance -= uicStarts;
                }
                if (_profile)
                    ENV.Utilities.Profiler.EndProfilingSession();
            }

        }

        void UseRunMethod(MethodInfo runMethod, bool askToRun)
        {
            if (runMethod.GetParameters().Length == 0)
            {
                Run(y => RunMethod(runMethod, new object[0], y), askToRun);
            }
            else
                ShowParameters(runMethod);
        }
        void RunMethod(MethodInfo method, object[] args, Action<object> callWithResult)
        {
            object result = method.Invoke(_factory(), args);
            if (method.ReturnType != typeof(void))
            {
                if (result == null)
                    result = "NULL";
                callWithResult(result);
            }
        }



        void ShowParameters(MethodInfo info)
        {
            string paramsFileName = ParamsFileName(_type);
            var fb = new FormBuilder("Run " + _name);
            var parameters = new List<Func<object>>();
            var columns = new List<Firefly.Box.Data.Advanced.ColumnBase>();
            foreach (var par in info.GetParameters())
            {
                if (par.ParameterType == typeof(NumberParameter))
                {
                    var x = new NumberColumn(par.Name);
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => new NumberParameter(x));
                }
                else if (par.ParameterType == typeof(Number))
                {
                    var x = new NumberColumn(par.Name);
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => x.Value);
                }
                else if (typeof(NumberColumn).IsAssignableFrom(par.ParameterType))
                {
                    var x = new NumberColumn(par.Name);
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => x);
                }
                else if (par.ParameterType == typeof(int))
                {
                    var x = new NumberColumn(par.Name, "12N");
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => (int)x.Value);
                }
                else if (par.ParameterType == typeof(decimal))
                {
                    var x = new NumberColumn(par.Name);
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => (decimal)x.Value);
                }
                else if (par.ParameterType == typeof(TextParameter))
                {
                    var x = new TextColumn(par.Name);
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => new TextParameter(x));
                }
                else if (par.ParameterType == typeof(Text))
                {
                    var x = new TextColumn(par.Name);
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => x.Value);
                }
                else if (typeof(TextColumn).IsAssignableFrom(par.ParameterType))
                {
                    var x = new TextColumn(par.Name);
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => x);
                }
                else if (par.ParameterType == typeof(string))
                {
                    var x = new TextColumn(par.Name);
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => (string)x.Value);
                }
                else if (par.ParameterType == typeof(ByteArrayParameter))
                {
                    var x = new TextColumn(par.Name) { AllowNull = true, DefaultValue = null };
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => (ByteArrayParameter)x);
                }
                else if (par.ParameterType == typeof(DateParameter) || typeof(DateColumn).IsAssignableFrom(par.ParameterType))
                {
                    var x = new DateColumn(par.Name);
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => new DateParameter(x));
                }
                else if (par.ParameterType == typeof(Date))
                {
                    var x = new DateColumn(par.Name);
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => x.Value);
                }
                else if (par.ParameterType == typeof(TimeParameter) || typeof(TimeColumn).IsAssignableFrom(par.ParameterType))
                {
                    var x = new TimeColumn(par.Name);
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => new TimeParameter(x));
                }
                else if (par.ParameterType == typeof(Time))
                {
                    var x = new TimeColumn(par.Name);
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => x.Value);
                }
                else if (par.ParameterType == typeof(BoolParameter) || typeof(BoolColumn).IsAssignableFrom(par.ParameterType))
                {
                    var x = new BoolColumn(par.Name);
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => new BoolParameter(x));
                }
                else if (par.ParameterType == typeof(Bool))
                {
                    var x = new BoolColumn(par.Name);
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => x.Value);
                }
                else if (par.ParameterType == typeof(bool))
                {
                    var x = new BoolColumn(par.Name);
                    columns.Add(x);
                    fb.AddColumn(x);
                    parameters.Add(() => (bool)x.Value);
                }

                else
                {
                    parameters.Add(() => null);

                }
            }


            fb.AddAction("Clear",
                () =>
                {

                    foreach (var column in columns)
                    {
                        column.Value = null;
                    }
                    File.Delete(paramsFileName);


                });
            Action<bool> saveParameters = (askFileName) =>
            {
                var saveValues = new List<string>();
                foreach (var item in columns)
                {
                    saveValues.Add(item.ToString());
                }
                SaveParameters(saveValues.ToArray(), _type, askFileName);
            };
            Action<string> loadParamFile = fn =>
            {

                try
                {
                    if (!File.Exists(fn))
                        return;
                    var ba = File.ReadAllBytes(fn);
                    using (var ms = new MemoryStream(ba))
                    {
                        var f = new System.Xml.Serialization.XmlSerializer(typeof(string[]));
                        var vals = (string[])f.Deserialize(ms);
                        int i = 0;
                        foreach (var column in columns)
                        {
                            column.Value = column.Parse(vals[i++], column.Format);
                        }
                    }
                }
                catch { }
            };

            fb.AddAction("Load Parameters", () =>
            {
                {
                    var fdg = new OpenFileDialog();
                    fdg.InitialDirectory = Path.GetDirectoryName(paramsFileName);
                    fdg.FileName = Path.GetFileName(paramsFileName);

                    if (fdg.ShowDialog() == DialogResult.OK)
                    {
                        loadParamFile(fdg.FileName);
                    }
                    else

                        return;
                }
            });
            fb.AddAction("Save Parameters", () =>
            {
                saveParameters(true);
            });
            fb.AddAction("Run",
                () =>
                {
                    saveParameters(false);
                    new BusinessProcess().ForFirstRow(() =>
                    Run(y => RunMethod(info, parameters.ConvertAll(yy => yy()).ToArray(), y), false));
                });

            loadParamFile(paramsFileName);
            fb.Run();

        }

        public static void SaveParameters(string[] saveValues, Type t, bool showDialog = false)
        {
            string paramsFileName = ParamsFileName(t);
            if (showDialog)
            {
                var fdg = new System.Windows.Forms.SaveFileDialog();
                fdg.InitialDirectory = Path.GetDirectoryName(paramsFileName);
                fdg.FileName = Path.GetFileName(paramsFileName);
                if (fdg.ShowDialog() == DialogResult.OK)
                {
                    paramsFileName = fdg.FileName;
                }
                else

                    return;
            }
            using (var sw = new MemoryStream())
            {
                var f = new System.Xml.Serialization.XmlSerializer(typeof(string[]));
                f.Serialize(sw, saveValues);
                System.IO.File.WriteAllBytes(paramsFileName, sw.ToArray());
            }
        }
        internal const string paramFileSuffix = ".Program.params";
        internal static string ParamsFileName(Type t)
        {
            return Path.Combine(Application.UserAppDataPath, t.FullName + paramFileSuffix);
        }
    }
}