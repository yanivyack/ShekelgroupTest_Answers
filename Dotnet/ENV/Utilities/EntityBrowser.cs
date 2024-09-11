using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using ENV.Data;
using ENV.Data.DataProvider;
using ENV.UI;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.Data.UnderConstruction;
using Entity = ENV.Data.Entity;
using NumberColumn = ENV.Data.NumberColumn;
using TextColumn = ENV.Data.TextColumn;

namespace ENV.Utilities
{
    public class EntityBrowser : UIControllerBase
    {

        RightToLeft _rtl = LocalizationInfo.Current.RightToLeft;
        internal RightToLeft rtl
        {
            get { return _rtl; }
            set
            {
                _rtl = value;
                _form.RightToLeft = _rtl;
            }
        }

        class entityListEntity : ENV.Data.Entity
        {
            [PrimaryKey]
            internal readonly NumberColumn ID = new NumberColumn("ID", "5");
            internal readonly NumberColumn Number = new NumberColumn("Number", "5Z");
            internal readonly TextColumn Caption = new TextColumn("Caption", "40");
            internal readonly TextColumn Name = new TextColumn("Name", "200");
            internal readonly TextColumn TypeName = new TextColumn("TypeName", "200", "ClassName");
            internal readonly TextColumn LongTypeName = new TextColumn("LongTypeName", "200", "LongClassName");
            internal readonly ENV.Data.BoolColumn Btrieve = new ENV.Data.BoolColumn("Btrieve");
            public entityListEntity(IEntityDataProvider source)
                : base("EntityList", "Entities (Tables & Views)", source)
            {
            }
        }
        public static void ShowEntityBrowser(ApplicationControllerBase app)
        {
            ShowEntityBrowser(app, true);
        }
        internal override bool SuppressUpdatesInBrowseActivity
        {
            get { return false; }
        }
        public new FilterCollection Where { get { return base.Where; } }


        internal class ShowEntityList
        {
            ApplicationControllerBase _app;
            internal static List<Action<Action<string, Action<Firefly.Box.Data.Entity>>>> _moreEntityBrowserActions = new List<Action<Action<string, Action<Firefly.Box.Data.Entity>>>>();
            bool _allowUpdate;
            Dictionary<int, Func<ENV.Data.Entity>> _dict = new Dictionary<int, Func<Data.Entity>>();
            entityListEntity e = new entityListEntity(new DataSetDataProvider());
            RecentManager _recent;

            public ShowEntityList(ApplicationControllerBase app, bool allowUpdate)
            {
                this._app = app;

                this._allowUpdate = allowUpdate;
            }




            public void Run(ApplicationControllerBase app)
            {

                _recent = new RecentManager(_app.GetType().FullName + ".Models.recent", e.LongTypeName);

                var alreadyAdded = new HashSet<int>();
                foreach (var ent in app.AllEntities._entitiesIndexes)
                {
                    var entity = ent;
                    var type = entity.Key;
                    int ind = ent.Value;
                    alreadyAdded.Add(ind);
                    AddOneEntity(type, ind);
                }
                new AssemblySearcher(typeof(ENV.Data.Entity), item =>
                {
                    int i = 0;
                    if (app != null)
                        if (!app.AllEntities._entitiesIndexes.TryGetValue(item, out i))
                            i = 0;
                    if (i > 0 && alreadyAdded.Contains(i))
                        return;
                    AddOneEntity(item, i);
                    alreadyAdded.Add(i);
                }).Search(app.GetType().Assembly);

                ShowTheList();
            }
            public void Run(System.Reflection.Assembly ass)
            {

                _recent = new RecentManager(ass.GetName() + "dll.Models.recent", e.LongTypeName);



                new AssemblySearcher(typeof(ENV.Data.Entity), item =>
                {
                    int i = 0;
                    if (_app != null)
                        if (!_app.AllEntities._entitiesIndexes.TryGetValue(item, out i))
                            i = 0;
                    AddOneEntity(item, i);
                }).Search(ass);

                ShowTheList();
            }

            private void ShowTheList()
            {
                var eb = new EntityBrowser(e);
                eb.OrderBy.Add(e.Number, e.LongTypeName);

                eb.AddColumns(e.Number, e.Caption, e.Name, e.TypeName, e.LongTypeName);
                eb.AddAllColumns();
                eb.AddAction("View Table", () =>
                {
                    _recent.SaveRecent();
                    try
                    {


                        TableDefinitionConverter.CheckTableDefinitionChanges(_dict[e.ID](), () => { });
                        var entity = _dict[e.ID]();//do not refactor the _dict[e.id] as the entity get's changed when the business process is run and causes this to look like changes.
                        var seb = new EntityBrowser(entity, _allowUpdate);
                        seb.AddAction("Save as SQL Insert Script", () =>
                        {

                            Func<EntityDataProviderFilterItemSaver> createSaver = () => new NoParametersFilterItemSaver(true, SQLClientEntityDataProvider.DateTimeStringFormat, DummyDateTimeCollector.Instance);
                            var dsp = entity.DataProvider as DynamicSQLSupportingDataProvider;
                            if (dsp != null)
                            {
                                var c = dsp._internalGetClient();
                                createSaver = () => c.GetNoParameterFilterItemSaver(DummyDateTimeCollector.Instance);
                            }
                            using (var sw = new StringWriter())
                            {

                                Number rows = 0;
                                seb.DoOnSelectedOrAllDataViewRows(() =>
                                {
                                    rows++;
                                    bool first = true;
                                    sw.Write("insert into " + entity.EntityName + " (");

                                    var values = new StringBuilder();
                                    foreach (var item in entity.Columns)
                                    {
                                        if (item.DbReadOnly)
                                            continue;
                                        if (first)
                                        {
                                            first = false;
                                        }
                                        else
                                        {
                                            sw.Write(", ");
                                            values.Append(", ");
                                        }
                                        sw.Write(item.Name);
                                        var p = createSaver();
                                        item.SaveYourValueToDb(p);
                                        values.Append(p.TextForCommand);
                                    }
                                    sw.WriteLine(") values (" + values.ToString() + ");");


                                });
                                ShowString("Insert SQL Script","--Insert script for "+rows.ToString("10C~")+" Rows. \r\n" +sw.ToString());
                            }




                        });
                        seb.AddAction("Save Table Data", () =>
                        {
                            try
                            {


                                var sfd = new SaveFileDialog();
                                sfd.FileName = entity.GetType().FullName;
                                if (sfd.ShowDialog() == DialogResult.OK)
                                {

                                    var rows = UserMethods.Instance.SerializeEntity(entity, long.MaxValue, ba =>
                                    {
                                        File.WriteAllBytes(sfd.FileName, ba);
                                    }, callMeForEachRow =>
                                    {
                                        seb.DoOnSelectedOrAllDataViewRows(callMeForEachRow);
                                    });
                                    Common.ShowMessageBox("Rows Saved", MessageBoxIcon.Information, "Saved " + rows + " succesfully");


                                }
                            }
                            catch (Exception ex)
                            {
                                ErrorLog.WriteToLogFile(ex);
                                Common.ShowExceptionDialog(ex, true, "");
                            }
                        });
                        seb.AddAction("Load Table Data", () =>
                        {
                            seb.SaveRowAndDo(o =>
                            {
                                try
                                {
                                    var ent = _dict[e.ID]();
                                    var ofd = new OpenFileDialog();
                                    if (ofd.ShowDialog() == DialogResult.OK)
                                    {
                                        var returnCode = UserMethods.Instance.DeserializeEntity(ent, File.ReadAllBytes(ofd.FileName));
                                        if (returnCode == -2)
                                            throw new InvalidOperationException(LocalizationInfo.Current.InvalidTableStructure);
                                        o.ReloadData();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ErrorLog.WriteToLogFile(ex);
                                    Common.ShowExceptionDialog(ex, true, "");
                                }
                            });
                        });


                        seb.AddAction("Generate Random Data", () =>
                        {
                            var rdg = new RandomDataGenerator(_dict[e.ID]());
                            rdg.Run();
                        });
                        seb.Run();
                    }
                    catch (Exception ex)
                    {
                        ENV.Common.ShowExceptionDialog(ex, true, "");
                    }


                }, true);
                eb.AddAction("Info", () =>
                {
                    Common.Try(() =>
                    {
                        var en = _dict[e.ID]();
                        bool exist = false;
                        Number numOfRows = 0;
                        try
                        {
                            exist = en.Exists();
                            if (exist)
                                numOfRows = en.CountRows();
                        }
                        catch (Exception ex)
                        {
                            ENV.Common.ShowExceptionDialog(ex, true, "");
                        }
                        var s = "";
                        foreach (var item in en.PrimaryKeyColumns)
                        {
                            if (s.Length != 0)
                                s += ", ";
                            s += item.Name;
                        }
                        using (var sw = new StringWriter())
                        {
                            sw.WriteLine("      Caption: " + en.Caption);
                            sw.WriteLine("   Name In Db: " + en.EntityName);
                            var be = en as BtrieveEntity;
                            if (be != null)
                                sw.WriteLine(" Btrieve Path: " + be.GetFullBtrievePathAndName());
                            sw.WriteLine("   Class Name: " + en.GetType().FullName);
                            sw.WriteLine("       Exists: " + exist);
                            sw.WriteLine("         Rows: " + numOfRows.ToString("10C~"));
                            sw.WriteLine("  Primary Key: " + s);
                            sw.WriteLine("      Columns: " + en.Columns.Count);
                            sw.WriteLine("      Indexes: " + en.Indexes.Count);
                            sw.WriteLine("Database Name: " + (ENV.UserMethods.Instance.InternalDbName(en, 3) ?? ""));
                            sw.WriteLine("Database Type: " + ENV.UserMethods.Instance.InternalDbName(en, 4));
                            ShowString("Entity Info", sw.ToString());
                        }


                    });

                });
                eb.AddAction("Copy Code Path", () =>
                {
                    Clipboard.SetText(e.LongTypeName.Trim().Replace("+", "."));
                });
                eb.AddAction("Create Table", () =>
                {
                    var x = _dict[e.ID]();
                    x.AutoCreateTable = true;
                    var bp = new BusinessProcess { From = x };
                    bp.ForFirstRow(() => { });
                });
                eb.AddAction("Drop Table", () =>
                {
                    if (ENV.Common.ShowYesNoMessageBox("Drop table", "Are you sure you want to drop the table?", false))
                    {
                        _dict[e.ID]().Drop();
                    }
                });
                eb.AddAction("Create Table SQL Script",
                    () =>
                    {
                        var x = _dict[e.ID]();
                        var ddp = x.DataProvider as DynamicSQLSupportingDataProvider;
                        EntityScriptGenerator tables, indexes;

                        if (ddp != null)
                        {
                            tables = new ENV.Utilities.EntityScriptGenerator(new SqlScriptGenerator(ddp.CreateScriptGeneratorTable)) { ScriptType = ScriptType.TableOnly };
                            indexes = new ENV.Utilities.EntityScriptGenerator(new SqlScriptGenerator(ddp.CreateScriptGeneratorTable)) { ScriptType = ScriptType.IndexOnly };
                        }
                        else
                        {
                            tables = new ENV.Utilities.EntityScriptGenerator(false) { ScriptType = ScriptType.TableOnly };
                            indexes = new ENV.Utilities.EntityScriptGenerator(false) { ScriptType = ScriptType.IndexOnly };
                        }

                        _recent.SaveRecent();

                        eb.ForCurrentOrSelectedRows(() =>
                        {
                            var ee = _dict[e.ID]();
                            tables.AddEntity(ee);
                            indexes.AddEntity(ee);
                        });

                        using (var sw = new StringWriter())
                        {
                            tables.ToWriter(sw);
                            sw.WriteLine("");
                            sw.WriteLine(@"/**************************************
Indexes
**************************************/");
                            indexes.ToWriter(sw);
                            ShowString("Create Table SQL Script", sw.ToString());
                        }



                    });

                eb.AddAction("Create DDF", () => CreateDDF(eb), () => e.Btrieve);
                eb.AddAction("Copy Btrieve Definition as Base64 string",
                    () => CopyBtrieveDefinitionAsBase64ToClipboard(eb), () => e.Btrieve);






                foreach (var item in _moreEntityBrowserActions)
                {
                    item((name, action) =>
                    {
                        eb.AddAction(name, () => action(_dict[e.ID]()));
                    });
                }
                _recent.StartOnRow(eb.StartOnRowWhere);
                eb.Run();
            }


            void CreateDDF(EntityBrowser eb)
            {
                var fb = new FormBuilder("DDF Builder");
                var resultPath = new TextColumn("Result Path");
                var useCaptions = new ENV.Data.BoolColumn("Use Captions instead of names");
                var filePrefix = new TextColumn("Optional prefix to all paths");

                fb.AddColumn(resultPath);
                fb.AddColumn(useCaptions);
                fb.AddColumn(filePrefix);
                fb.AddAction("Build", () =>
                {



                    var ddfBuilder = new DDFBuilder(new BtrieveDataProvider(), resultPath.Trim(), useCaptions, filePrefix);
                    if (ddfBuilder.Exists())
                    {
                        if (!ENV.Common.ShowYesNoMessageBox("DDF Files Exists", "There are already ddf files in this directory - would you like to delete them?", false))
                            return;
                    }
                    eb.ForCurrentOrSelectedRows(() =>
                    {
                        var be = _dict[e.ID]() as BtrieveEntity;
                        if (be != null)
                            ddfBuilder.AddEntity(be);
                    });
                    try
                    {
                        ddfBuilder.Build();
                    }
                    catch (Exception ex)
                    {
                        ENV.Common.ShowExceptionDialog(ex);
                    }
                    finally
                    {
                        fb.Close();
                    }
                });

                fb.Run();



            }

            void CopyBtrieveDefinitionAsBase64ToClipboard(EntityBrowser eb)
            {
                var be = _dict[e.ID]() as BtrieveEntity;
                if (be != null)
                {
                    var defAsBase64String = TableDefinitionConverter.DefOfAsBase64(be);
                    Clipboard.SetText(defAsBase64String);
                    MessageBox.Show("Btrieve Definition was Copied to Clipboard");
                }
            }



            private void AddOneEntity(Type type, int ind)
            {
                try
                {
                    var en = (ENV.Data.Entity)System.Activator.CreateInstance(type);

                    new BusinessProcess { From = e, Activity = Activities.Insert }.ForFirstRow(
                        () =>
                        {
                            e.ID.Value = _dict.Count;
                            _dict.Add(_dict.Count, () => (ENV.Data.Entity)System.Activator.CreateInstance(type));
                            e.Number.Value = ind;
                            e.Caption.Value = en.Caption;
                            e.Name.Value = en.EntityName;
                            e.TypeName.Value = type.Name;
                            e.LongTypeName.Value = type.FullName;
                            e.Btrieve.Value = en is BtrieveEntity;



                        });
                }
                catch
                {
                }
            }
        }



        private void ForCurrentOrSelectedRows(Action what)
        {

            if (_form.MultiSelectRowsCount > 0)
                _form.ReadSelectedRows(what);
            else
                what();
        }

        internal static void ShowApplicationComponentsAndDo(ApplicationControllerBase app,
            string actionName,
            Action<ApplicationControllerBase> what, Action<System.Reflection.Assembly> whatForAssembly)
        {



            var e = new ENV.Data.Entity("Choose Component " + actionName, new DataSetDataProvider());
            var nc = new NumberColumn("Index", "5");
            var tc = new TextColumn("Application", "100");
            e.Columns.Add(nc, tc);
            e.SetPrimaryKey(nc);

            var it = new Iterator(e);
            var apps = new Dictionary<int, ApplicationControllerBase>();


            IterateApplicationComponents(app, @base =>
            {
                if (apps.ContainsValue(@base))
                    return;
                var r = it.CreateRow();
                r.Set(nc, apps.Count);
                r.Set(tc, @base.GetType().Namespace);
                r.UpdateDatabase();
                apps.Add(apps.Count, @base);
            });
            {
                var r = it.CreateRow();
                r.Set(nc, apps.Count);
                r.Set(tc, "dlls and exes");
                r.UpdateDatabase();
            }
            var recent = new RecentManager(actionName + "choose component", tc);
            var eb = new EntityBrowser(e, false);
            recent.StartOnRow(eb.StartOnRowWhere);
            eb.AddAction(actionName, () =>
            {
                recent.SaveRecent();
                if (apps.Count == nc)
                {
                    var dlls = new Data.Entity("dlls and exes", new DataSetDataProvider());
                    var name = new TextColumn("Dll name", "100");
                    dlls.SetPrimaryKey(name);
                    foreach (var item in System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(app.GetType().Assembly.Location), "*.dll"))
                    {
                        try
                        {
                            var fileName = Path.GetFileNameWithoutExtension(item);
                            var excluded = new[] { "ENVDTE", "FIREFLY.", "ITEXTSHARP", "STDOLE", "AXINTEROP.", "INTEROP.", "MGCHART", "MSCOMCTL", "MICROSOFT." };
                            if (excluded.Any(x => fileName.ToUpper().StartsWith(x)))
                                continue;

                            dlls.Insert(() =>
                            {
                                name.Value = fileName;
                            });
                        }
                        catch { }
                    }
                    foreach (var item in System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(app.GetType().Assembly.Location), "*.exe"))
                    {
                        try
                        {
                            var i = Path.GetFileNameWithoutExtension(item);
                            dlls.Insert(() =>
                            {
                                name.Value = i;
                            });
                        }
                        catch { }
                    }

                    var dllRecent = new RecentManager(actionName + "dll", name);
                    var eb2 = new EntityBrowser(dlls);
                    dllRecent.StartOnRow(eb2.StartOnRowWhere);
                    eb2.AddAction(actionName, () =>
                    {

                        try
                        {
                            var asss = System.Reflection.Assembly.Load(name.Trim());
                            dllRecent.SaveRecent();
                            whatForAssembly(asss);
                        }
                        catch (Exception ex)
                        {
                            ENV.Common.ShowMessageBox("Failed to load dll " + name.Trim(), MessageBoxIcon.Error, ex.Message);
                        }
                    }, true);
                    eb2.Run();


                }
                else
                    what(apps[nc]);
            }, true);
            eb.Run();

        }

        public static void ShowEntityBrowser(ApplicationControllerBase app, bool allowUpdate)
        {
            ShowApplicationComponentsAndDo(app, "Show Entities",
                appp => new ShowEntityList(app, allowUpdate).Run(appp),
                ass => new ShowEntityList(app, allowUpdate).Run(ass));
        }
        public static void IterateApplicationComponents(ApplicationControllerBase app, Action<ApplicationControllerBase> add)
        {
            add(app);
            foreach (var refModule in app._referencedModules)
            {
                IterateApplicationComponents(refModule, add);
            }
        }

        public static void ShowString(string title, string what)
        {
            var uic = new UIController() { Activity = Activities.Browse };
            var tc = new TextColumn { Value = what };
            uic.View = new Firefly.Box.UI.Form() { FitToMDI = true, RightToLeft = RightToLeft.No, Text = title };
            var tb = new ENV.UI.TextBox
            {
                Multiline = true,
                Data = tc,
                Dock = DockStyle.Fill,
                RightToLeft = RightToLeft.No,
                Alignment = System.Drawing.ContentAlignment.TopLeft,
                WordWrap = false,
                AllowVerticalScroll = true,
                AllowHorizontalScroll = true,
                ScrollBars = true,
                Font = new System.Drawing.Font("Consolas", 10)
            };
            uic.View.Controls.Add(tb);
            uic.Run();
        }
        Dictionary<ColumnBase, Action<ENV.UI.TextBox>> _doOnTextbox = new Dictionary<ColumnBase, Action<UI.TextBox>>();
        Dictionary<ColumnBase, Action<ENV.UI.ComboBox>> _comboBoxes = new Dictionary<ColumnBase, Action<UI.ComboBox>>();
        public void AddTextBoxColumn(ColumnBase column, Action<ENV.UI.TextBox> what)
        {
            AddColumns(column);
            _doOnTextbox.Add(column, what);
        }
        public void AddComboBoxColumn(ColumnBase column, Action<UI.ComboBox> what)
        {
            AddColumns(column);
            _comboBoxes.Add(column, what);
        }
        public void AddAction(string what, Action action, Func<bool> condition = null)
        {
            AddAction(what, action, false, condition);
        }
        public void AddAction(string what, Action<UIController> action, bool isDefaultAction = false)
        {
            AddAction(what, () => action(_uiController), isDefaultAction);
        }

        public void AddAction(string what, Action action, bool isDefaultAction, Func<bool> condition = null)
        {

            _form.AddAction(what, action, condition);
            if (isDefaultAction)
                SetDefaultAction(action);



        }

        public EntityBrowser(Firefly.Box.Data.Entity entity)
            : this(entity, false)
        {

        }
        public void DoOnSelectedOrAllDataViewRows(Action what)
        {

            if (_form.MultiSelectRowsCount > 0)
            {
                _form.ReadSelectedRows(what);
            }
            else ReadAllRows(what);
        }
        public Sort OrderBy { get { return base.OrderBy; } set { base.OrderBy = value; } }

        GridView _form;
        public EntityBrowser(Firefly.Box.Data.Entity entity, bool allowUpdate)
        {
            entity.Cached = true;
            From = entity;
            if (entity.Indexes.Count > 0)
                OrderBy = entity.Indexes[0];
            this.Activity = Firefly.Box.Activities.Browse;
            AllowUpdate = allowUpdate;
            AllowDelete = allowUpdate;
            AllowInsert = allowUpdate;
            AllowExportData = true;

            Title = entity.Caption;
            Handlers.Add(Keys.Control | Keys.F5, HandlerScope.CurrentTaskOnly).Invokes +=
                e =>
                {
                    _uiController.SaveRowAndDo(o => o.ReloadData());
                };


            View = () =>
                _form = new GridView
                {
                    RightToLeft = rtl,
                    Text = entity.Caption,
                    FitToMDI = true,

                    Width = 1000,
                    Height = 700,

                };

        }

        public bool FitToMDI
        {
            get
            {
                return _form.FitToMDI;
            }
            set
            {
                _form.FitToMDI = value;
                _form.UserStateIdentifier = "EntityBrowser +" + _form.Text;
            }
        }


        List<ColumnBase> _columns = new List<ColumnBase>();
        public void AddColumns(params ColumnBase[] args)
        {
            _columns.AddRange(args);
        }
        public void AddNonDisplayColumn(params ColumnBase[] columns)
        {
            Columns.Add(columns);
        }
        public int MaxWidthINChars { get { return _form.MaxWidthINChars; } set { _form.MaxWidthINChars = value; } }

        protected override void OnLoad()
        {

        }
        bool _rightToLeftByFormat = false;
        public void Run()
        {




            if (_columns.Count == 0)
                _columns.AddRange(From.Columns);
            Columns.Add(_columns.ToArray());

            {

                var fac = _form.GetFilterOnAllColumnsClass();
                Handlers.Add(Keys.Control | Keys.Shift | Keys.R).Invokes +=
                    e =>
                    {
                        e.Handled = true;
                        fac.Filter(delegate { });
                    };


                var entityType = From.GetType().FullName;
                _rightToLeftByFormat = !(entityType.StartsWith("Firefly") || entityType.StartsWith("ENV"));

                foreach (var column in _columns)
                {
                    if (column is Firefly.Box.Data.ByteArrayColumn)
                        continue;
                    if (_doOnTextbox.ContainsKey(column))
                    {
                        var tb = new ENV.UI.TextBox() { Data = column, Style = Firefly.Box.UI.ControlStyle.Flat };
                        _doOnTextbox[column](tb);
                        _form.AddGridColumn(column.Caption, tb);
                    }
                    else if (_comboBoxes.ContainsKey(column))
                    {
                        var cb = new ENV.UI.ComboBox() { Data = column };
                        _comboBoxes[column](cb);
                        _form.AddGridColumn(column.Caption, cb);
                    }
                    else
                        _form.AddColumn(column);

                }




            }

            Execute();
        }
        protected override void OnUnLoad()
        {
            if (Activity == Activities.Browse)
                ApplyActivityToColumns(Activities.Update);
        }


        public void SetDefaultAction(Action action)
        {
            if (AllowSelect)
                throw new InvalidOperationException("Default action already set");
            AllowSelect = true;
            var a = Handlers.Add(Command.Select);
            a.Scope = HandlerScope.CurrentTaskOnly;
            a.Invokes += e =>
            {
                e.Handled = true;
                action();
            };


        }

        public void MapKey(Keys key, Action action)
        {
            Handlers.Add(key).Invokes += e =>
            {
                e.Handled = true;
                action();
            };
        }
        public void MapKey(Keys key, Action<UIController> action)
        {
            Handlers.Add(key).Invokes += e =>
            {
                e.Handled = true;
                action(_uiController);
            };
        }


    }
    class RecentManager
    {
        string _name;
        TextColumn _col;
        public RecentManager(string name, TextColumn col)
        {
            _col = col;
            _name = Path.Combine(Application.UserAppDataPath, PathDecoder.FixFileName(name));
        }
        public void StartOnRow(FilterCollection startOnRowWhere)
        {

            if (File.Exists(_name))
                try
                {
                    startOnRowWhere.Add(_col.IsEqualTo(File.ReadAllText(_name)));
                }
                catch { }
        }
        public void SaveRecent()
        {
            try
            {
                File.WriteAllText(_name, _col);
            }
            catch { }
        }
    }
    class AssemblySearcher
    {
        HashSet<string> _visited = new HashSet<string>();
        Type _baseTypeToSearchFor;
        Action<Type> _whatToDo;

        public AssemblySearcher(Type baseTypeToSearchFor,
        Action<Type> whatToDo)
        {
            this._baseTypeToSearchFor = baseTypeToSearchFor;
            this._whatToDo = whatToDo;
        }
        public void Search(System.Reflection.Assembly ass)
        {

            SearchInAssembly(ass);
        }
        void SearchInAssembly(System.Reflection.Assembly ass)
        {

            foreach (var item in ass.GetTypes())
            {
                var x = item.FullName;
                if (_baseTypeToSearchFor.IsAssignableFrom(item) && !item.IsAbstract && item.DeclaringType == null)
                {
                    _whatToDo(item);

                }
            }
            return;

            //not used
            foreach (var item in ass.GetReferencedAssemblies())
            {
                if (_visited.Contains(item.FullName) ||
                    item.Name == "mscorlib" ||
                    item.FullName.StartsWith("System") ||
                    item.FullName.StartsWith("Firefly") ||
                    item.FullName.StartsWith("ENV"))
                    continue;
                _visited.Add(item.FullName);
                try
                {
                    SearchInAssembly(System.Reflection.Assembly.Load(item));
                }
                catch { }
            }

        }
    }
}