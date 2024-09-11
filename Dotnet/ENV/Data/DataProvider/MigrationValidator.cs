using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.DataProvider
{
    public class MigrationValidator
    {
        static MigrationValidator()
        {
            _memory.DataSet.CaseSensitive = true;
        }

        private static MemoryDatabase _memory = new MemoryDatabase();

        public class EntityList : ENV.Data.Entity
        {
            internal readonly TextColumn TypeName = new TextColumn("TypeName", "4000");
            [PrimaryKey]
            internal readonly TextColumn Applic = new TextColumn("Applic", "30");
            [PrimaryKey]
            internal readonly NumberColumn Index = new NumberColumn("IndexX", "5");
            internal readonly TextColumn BtrieveName = new TextColumn("BtrieveName", "1000");
            public readonly TextColumn SqlName = new TextColumn("SqlName", "300");
            internal readonly BoolColumn WorksInBtrieve = new BoolColumn("WorksInBtrieve");
            public readonly TextColumn SqlColumnNames = new TextColumn("SqlColumnNames", "4000");
            public readonly TextColumn BtrieveColumns = new TextColumn("BtrieveColumns", "4000");

            internal readonly TextColumn TypeFullName = new TextColumn("TypeFullName", "4000");
            public EntityList()
                : base("AllEntities", _memory)
            {
                AutoCreateTable = true;
            }
        }
        class mySaver : IBtrieveValueSaver
        {
            int i = 0;
            public StringBuilder Result = new StringBuilder();
            public void SaveInt(int value)
            {
                Add("Int");
            }
            void Add(string what)
            {
                Result.Append(++i + "-" + what + ", ");
            }

            public void SaveDecimal(decimal value, byte precision, byte scale)
            {
                Add(string.Format("Decimal({0},{1})", precision, scale));
            }

            public void SaveString(string value, int length, bool fixedWidth)
            {
                Add(string.Format("String ({0})", length));
            }

            public void SaveAnsiString(string value, int length, bool fixedWidth)
            {
                Add(string.Format("AnsiString ({0})", length));
            }

            public void SaveNull()
            {
                Add("Null");
            }

            public void SaveDateTime(DateTime value)
            {
                Add("DateTime");
            }

            public void SaveTimeSpan(TimeSpan value)
            {
                Add("Time");
            }

            public void SaveBoolean(bool value)
            {
                Add("Bool");
            }

            public void SaveByteArray(byte[] value)
            {
                var s = "unknown";
                if (_column is Firefly.Box.Data.TextColumn)
                    s = _column.FormatInfo.MaxLength.ToString();
                Add(string.Format("ByteArray({0})", s));
            }

            public void SaveInteger(Number value, byte[] valueBytes, int dataTypeCode)
            {
                Add("BtrieveInteger");
            }

            public void SaveSingleDecimal(Number value)
            {
                Add("SingleDecimal");
            }

            public void SaveByteArray(byte[] value, IComparable comparable)
            {
                Add("BtrieveByteArray");
            }

            public void SaveTime(Time value)
            {
                Add("Time");
            }

            public void SaveDate(Date value)
            {
                Add("Date");
            }

            public void SaveByteArray(byte[] value, IComparable comparable, short btrieveExtendedDataType)
            {
                SaveByteArray(value, comparable);
            }

            private ColumnBase _column;
            public void SetColumn(ColumnBase column)
            {
                _column = column;
            }

            public void SaveEmptyDateTime()
            {
                Add("DateTime");
            }
        }
        public static void Populate(params ApplicationControllerBase[] apps)
        {
            foreach (var applicationControllerBase in apps)
            {
                Populate(applicationControllerBase);
            }
            _memory.DataSet.WriteXml(@"d:\westchester\project\entityMap.xml", XmlWriteMode.WriteSchema);
        }
        public static void Load()
        {
            _memory.DataSet.ReadXml(@"d:\westchester\project\entityMap.xml");
        }
        public static void Show()
        {
            Show(delegate { });
        }

        public static void Show(Action<EntityList, Utilities.EntityBrowser> where)
        {
            var e = new EntityList();
            var eb = new Utilities.EntityBrowser(e);
            where(e, eb);

            eb.AddAction("View", () =>
            {
                var ee = System.Activator.CreateInstance(Type.GetType(e.TypeFullName.Trim())) as Entity;
                try
                {
                    new EntityBrowser(ee).Run();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);


                }

            });
            eb.AddAction("Migrate",
                         () =>
                         MigrationToSql.MigrateToSQL(x => x(Type.GetType(e.TypeFullName), e.Index),
                                                     new BtrieveMigrationToSQLHelper()));
            eb.Run();
        }
        public static void ShowDifferent(Func<EntityList, TextColumn> whichColumn)
        {
            var btrieveName = new TextColumn("BtrieveName", "3000");

            var differentVariations = new NumberColumn("Variations", "5");
            var ds = new DataSetDataProvider();
            ds.DataSet.CaseSensitive = true;
            var e = new Entity("e", ds);
            e.Columns.Add(btrieveName, differentVariations);
            e.SetPrimaryKey(btrieveName);

            var el = new EntityList();
            var bp = new BusinessProcess { From = el };
            bp.OrderBy.Add(el.BtrieveName, whichColumn(el));
            var variationCount = 0;
            var g = bp.Groups.Add(el.BtrieveName);
            g.Enter += () => variationCount = 0;
            bp.Groups.Add(whichColumn(el)).Enter += () => variationCount++;
            g.Leave += () =>
            {
                if (variationCount > 1)
                    new BusinessProcess { From = e, Activity = Activities.Insert }
                        .ForFirstRow(() =>
                        {
                            btrieveName.Value = el.BtrieveName;
                            differentVariations.Value = variationCount;
                        });
            };
            bp.Run();
            var eb = new EntityBrowser(e);
            eb.AddAction("Show", () => Show(
                (entityListOfDifferentVariations, b) =>
                {
                    b.Where.Add(entityListOfDifferentVariations.BtrieveName.IsEqualTo(btrieveName));
                    b.AddAction("Show Diff",
                        () =>
                        {
                            Show((selectEntity, ebOfSelect) =>
                            {
                                ebOfSelect.Where.Add(selectEntity.BtrieveName.IsEqualTo(btrieveName));
                                ebOfSelect.Where.Add(whichColumn(selectEntity).IsDifferentFrom(whichColumn(entityListOfDifferentVariations)));
                                ebOfSelect.AddAction("Show Column Diff", () =>
                                {
                                    ShowColumnDiff(
                                        entityListOfDifferentVariations,
                                        selectEntity);
                                }, true);
                            });

                        }, true);
                }), true);
            eb.Run();

        }

        private static void ShowColumnDiff(EntityList entityListOfDifferentVariations, EntityList selectEntity)
        {
            var sourceEntity =
                System.Activator.CreateInstance(Type.GetType(entityListOfDifferentVariations.TypeFullName)) as Entity;
            var targetEntity = System.Activator.CreateInstance(Type.GetType(selectEntity.TypeFullName)) as Entity;

            var e = new Entity("", new DataSetDataProvider());
            var columnIndex = new NumberColumn("Column Index", "3");
            var columnCaption = new TextColumn("Column Caption", "30");
            var columnName = new TextColumn("ColumnName", "100");
            var columnShouldBeName = new TextColumn("ColumnShouldBeName", "100");
            var additionalErrors = new TextColumn("AdditionalErrors", "1000");
            if (sourceEntity.Columns.Count != targetEntity.Columns.Count)
            {
                MessageBox.Show("Column Count Doesnt Match");
                return;
            }
            e.Columns.Add(columnIndex, columnCaption, columnName, columnShouldBeName, additionalErrors);
            e.SetPrimaryKey(columnIndex);

            for (int i = 0; i < sourceEntity.Columns.Count; i++)
            {
                var sourceColumn = sourceEntity.Columns[i];
                var targetColumn = targetEntity.Columns[i];
                string additionalError = "";
                if (sourceColumn.Format != targetColumn.Format)
                    additionalError = string.Format("Format, {0} - {1}", sourceColumn.Format, targetColumn.Format);
                bool nameDifferent = sourceColumn.Name != targetColumn.Name;
                if (nameDifferent || additionalError != "")
                    new BusinessProcess { From = e, Activity = Activities.Insert }
                        .ForFirstRow(() =>
                        {
                            columnIndex.Value = i + 1;
                            columnCaption.Value = sourceColumn.Caption;
                            columnName.Value = sourceColumn.Name;
                            if (nameDifferent)
                                columnShouldBeName.Value = targetColumn.Name;
                            additionalErrors.Value = additionalError;
                        });
            }
            new EntityBrowser(e).Run();
        }

        public static Type FilterByType;
        public static void Populate(ApplicationControllerBase app)
        {
            var appName = app.GetType().Namespace;

            ENV.Data.DataProvider.BtrieveMigration.UseBtrieve = true;
            foreach (var entity in app.AllEntities._entities)
            {
                var el = new EntityList();
                var bp = new Firefly.Box.BusinessProcess();
                bp.Relations.Add(el, Firefly.Box.RelationType.InsertIfNotFound,
                                 el.Applic.BindEqualTo(appName).And(el.Index.BindEqualTo(entity.Key)));

                var e = System.Activator.CreateInstance(entity.Value) as BtrieveEntity;
                if (e != null &&
                    ((FilterByType == null && e.ShoudBeMigrated)
                    || (FilterByType != null && FilterByType.IsAssignableFrom(entity.Value))))
                    bp.ForFirstRow(() =>
                    {
                        el.BtrieveName.Value = e.GetFullBtrievePathAndName();

                        var x = new mySaver();
                        foreach (var column in e.Columns)
                        {
                            x.SetColumn(column);
                            {
                                var t = column as Firefly.Box.Data.TextColumn;
                                if (t != null)
                                    t.AllowNull = false;
                            }
                            {
                                var t = column as Firefly.Box.Data.DateColumn;
                                if (t != null)
                                    t.AllowNull = false;
                            }
                            {
                                var t = column as Firefly.Box.Data.NumberColumn;
                                if (t != null)
                                    t.AllowNull = false;
                            }
                            column.SaveYourValueToDb(x);
                        }
                        el.BtrieveColumns.Value = x.Result.ToString();
                        el.TypeName.Value = e.GetType().FullName + "..ctor()";
                        el.TypeFullName.Value = e.GetType().AssemblyQualifiedName;

                        try
                        {
                            var bp2 = new Firefly.Box.BusinessProcess { From = e };
                            bp2.ForFirstRow(() => { });
                            el.WorksInBtrieve.Value = true;
                        }
                        catch
                        {
                        }
                    });


            }
            if (FilterByType != null)
                return;
            ENV.Data.DataProvider.BtrieveMigration.UseBtrieve = false;
            foreach (var entity in app.AllEntities._entities)
            {
                var el = new EntityList();
                var bp = new Firefly.Box.BusinessProcess();
                bp.Relations.Add(el, Firefly.Box.RelationType.InsertIfNotFound,
                                 el.Applic.BindEqualTo(appName).And(el.Index.BindEqualTo(entity.Key)));
                try
                {
                    var e = System.Activator.CreateInstance(entity.Value) as BtrieveEntity;
                    if (e != null && e.ShoudBeMigrated)
                        bp.ForFirstRow(() =>
                        {
                            el.SqlName.Value = e.EntityName;
                            var sb = new StringBuilder();
                            foreach (var ent in e.Columns)
                            {
                                sb.Append(ent.Name + ",");
                            }
                            el.SqlColumnNames.Value = sb.ToString();
                        });
                }
                catch
                {
                }
            }
            ENV.Data.DataProvider.BtrieveMigration.UseBtrieve = true;
        }
    }
}