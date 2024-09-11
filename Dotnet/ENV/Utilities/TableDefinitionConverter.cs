using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ENV.Data;
using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;

namespace ENV.Utilities
{
    public class TableDefinitionConverter
    {
        static bool AutoConversionEnabled;

        public static TableDefinition DefOf(BtrieveEntity e)
        {
            return new TableDefinition
            {
                TypeName = e.GetType().FullName,
                SqlName = e.SqlName,
                Caption = e.Caption,
                SqlSchema = e.SqlSchema,
                BtrieveName = e.BtrieveName,
                Owner = e.Owner,
                Columns = e.Columns.Select(c => DefOf(c)).ToArray(),
                Indexes = e.Indexes.Select(i => DefOf(i)).ToArray()
            };
        }

        public static string DefOfAsBase64(BtrieveEntity e)
        {
            return System.Convert.ToBase64String(UserMethods.Instance.Serialize(DefOf(e)));
        }

        public static TableDefinition FromBase64(string s)
        {
            return UserMethods.Instance.Deserialize<TableDefinition>(System.Convert.FromBase64String(s));
        }

        internal static ColumnDefinition DefOf(ColumnBase col)
        {
            var colDef = new ColumnDefinition
            {
                Name = col.Name,
                Format = col.Format,
            };
            UserMethods.CastColumn(col, new ColumnDefinitionSaver(colDef));
            return colDef;
        }


        class ColumnDefinitionSaver : UserMethods.IColumnSpecifier
        {
            readonly ColumnDefinition _colDef;

            public ColumnDefinitionSaver(ColumnDefinition colDef)
            {
                _colDef = colDef;
            }

            public void DoOnColumn(TypedColumnBase<Text> column)
            {
                _colDef.TypeName = typeof(Text).Name;
                _colDef.StorageTypeName = column.Storage.GetType().FullName + ", " + column.Storage.GetType().Assembly.GetName().Name;
                if (column.DefaultValue != null && !String.IsNullOrEmpty(column.DefaultValue.ToString().Trim()))
                    _colDef.DefaultValue = System.Convert.ToBase64String(UserMethods.Instance.Serialize(new Remoting.ClientParameterManager().Pack(column.DefaultValue)));
            }

            public void DoOnColumn(TypedColumnBase<Number> column)
            {
                _colDef.TypeName = typeof(Number).Name;
                _colDef.StorageTypeName = column.Storage.GetType().FullName + ", " + column.Storage.GetType().Assembly.GetName().Name;
                if (column.Storage is ENV.Data.Storage.LegacyNumberStorage)
                {
                    _colDef.StorageTypeName += "|" + ((ENV.Data.Storage.LegacyNumberStorage)column.Storage).GetSizeString();
                }
                if (column.DefaultValue != null && !String.IsNullOrEmpty(column.DefaultValue.ToString().Trim()))
                    _colDef.DefaultValue = System.Convert.ToBase64String(UserMethods.Instance.Serialize(new Remoting.ClientParameterManager().Pack(column.DefaultValue)));
            }

            public void DoOnColumn(TypedColumnBase<Date> column)
            {
                _colDef.TypeName = typeof(Date).Name;
                _colDef.StorageTypeName = column.Storage.GetType().FullName + ", " + column.Storage.GetType().Assembly.GetName().Name;
                if (column.DefaultValue != null && !String.IsNullOrEmpty(column.DefaultValue.ToString().Trim()))
                    _colDef.DefaultValue = System.Convert.ToBase64String(UserMethods.Instance.Serialize(new Remoting.ClientParameterManager().Pack(column.DefaultValue)));
            }

            public void DoOnColumn(TypedColumnBase<Time> column)
            {
                _colDef.TypeName = typeof(Time).Name;
                _colDef.StorageTypeName = column.Storage.GetType().FullName + ", " + column.Storage.GetType().Assembly.GetName().Name;
                if (column.DefaultValue != null && !String.IsNullOrEmpty(column.DefaultValue.ToString().Trim()))
                    _colDef.DefaultValue = System.Convert.ToBase64String(UserMethods.Instance.Serialize(new Remoting.ClientParameterManager().Pack(column.DefaultValue)));
            }

            public void DoOnColumn(TypedColumnBase<Bool> column)
            {
                _colDef.TypeName = typeof(Bool).Name;
                _colDef.StorageTypeName = column.Storage.GetType().FullName + ", " + column.Storage.GetType().Assembly.GetName().Name;
                if (column.DefaultValue != null && !String.IsNullOrEmpty(column.DefaultValue.ToString().Trim()))
                    _colDef.DefaultValue = System.Convert.ToBase64String(UserMethods.Instance.Serialize(new Remoting.ClientParameterManager().Pack(column.DefaultValue)));
            }

            public void DoOnColumn(TypedColumnBase<byte[]> column)
            {
                _colDef.TypeName = typeof(byte[]).Name;
                _colDef.StorageTypeName = column.Storage.GetType().FullName + ", " + column.Storage.GetType().Assembly.GetName().Name;
                if (column.DefaultValue != null && !String.IsNullOrEmpty(column.DefaultValue.ToString().Trim()))
                    _colDef.DefaultValue = System.Convert.ToBase64String(UserMethods.Instance.Serialize(column.DefaultValue));
            }

            public void DoOnUnknownColumn(ColumnBase column)
            {
                throw new Exception("Unknown column type");
            }
        }

        internal static IndexDefinition DefOf(Sort idx)
        {
            return new IndexDefinition { Unique = idx.Unique, Name = idx.Name, Segments = idx.Segments.Select(s => DefOf(s, idx)).ToArray() };
        }

        static SegmentDefinition DefOf(SortSegment seg, Sort sort)
        {
            var bs = sort as BtrieveSort;
            var size = 0;
            if (bs != null)
            {
                size = bs.GetSize(seg);
            }
            return new SegmentDefinition
            {
                ColumnName = seg.Column.Name,
                Descending = (seg.Direction == SortDirection.Descending),
                Size = size
            };
        }

        internal static void CreateEntitiesBasedOnDefinition(TableDefinition targetDef, TableDefinition sourceDef, IEntityDataProvider dataProvider, Action<BtrieveEntity, BtrieveEntity> doOnEntities)
        {
            var sourceEntity = new BtrieveEntity(sourceDef.SqlName, sourceDef.Caption, dataProvider,
                sourceDef.SqlSchema, sourceDef.BtrieveName);
            var targetEntity = new MyBtrieveEntity(targetDef.SqlName, targetDef.Caption, dataProvider,
                targetDef.SqlSchema, targetDef.BtrieveName);

            Action<BtrieveEntity, TableDefinition> init = (e, def) =>
            {
                e.Owner = def.Owner;
                Array.ForEach(def.Columns, c => e.Columns.Add(CreateColumn(c)));
                Array.ForEach(def.Indexes, i => e.Indexes.Add(CreateIndex(i, e)));
            };

            init(sourceEntity, sourceDef);
            init(targetEntity, targetDef);

            doOnEntities(sourceEntity, targetEntity);
        }

        // used to differentiate types so that we can use the same btrieve files by 2 table definitions
        class MyBtrieveEntity : BtrieveEntity
        {
            public MyBtrieveEntity(string sqlName, string caption, IEntityDataProvider dataProvider, string sqlSchema, string btrieveName) : base(sqlName, caption, dataProvider, sqlSchema, btrieveName)
            {
            }
        }

        internal static IColumnStorageSrategy<T> CreateStorage<T>(TypedColumnBase<T> c, ColumnDefinition colDef)
        {
            if (colDef.StorageTypeName == "ENV.Data.Storage.AnsiStringTextStorageThatRemovesNullChars, ENV")
                return (IColumnStorageSrategy<T>)Activator.CreateInstance(Type.GetType(colDef.StorageTypeName), c, false);
            if (colDef.StorageTypeName.StartsWith("ENV.Data.Storage.LegacyNumberStorage, ENV"))
                return (IColumnStorageSrategy<T>)new ENV.Data.Storage.LegacyNumberStorage(Number.Parse(colDef.StorageTypeName.Split('|')[1]));
            if (colDef.StorageTypeName == "Firefly.Box.Data.NumberColumn+DefaultNumberStorage, Firefly.Box")
                return null;
            try
            {
                return (IColumnStorageSrategy<T>)Activator.CreateInstance(Type.GetType(colDef.StorageTypeName));
            }
            catch (MissingMethodException)
            {
                try
                {
                    return (IColumnStorageSrategy<T>)Activator.CreateInstance(Type.GetType(colDef.StorageTypeName), c);
                }
                catch (Exception e)
                {
                    throw new Exception(
                        String.Format("Failed to created Storage based on type: {0}", colDef.StorageTypeName), e);
                }
            }
        }

        static ColumnBase CreateColumn(ColumnDefinition colDef)
        {
            var u = UserMethods.Instance;

            if (colDef.TypeName == typeof(Number).Name)
            {
                var typedCol = new NumberColumn(colDef.Name, colDef.Format);
                typedCol.Storage = CreateStorage(typedCol, colDef);
                if (typedCol.DefaultValue != null && !String.IsNullOrEmpty(typedCol.DefaultValue.ToString().Trim()))
                    typedCol.DefaultValue = (Number)new Remoting.ServerParameterManager().UnPack(u.Deserialize<object>(System.Convert.FromBase64String(colDef.DefaultValue)));
                return typedCol;
            }

            if (colDef.TypeName == typeof(Text).Name)
            {
                var typedCol = new TextColumn(colDef.Name, colDef.Format);
                typedCol.Storage = CreateStorage(typedCol, colDef);
                if (colDef.DefaultValue != null && !String.IsNullOrEmpty(colDef.DefaultValue.Trim()))
                    typedCol.DefaultValue = (Text)new Remoting.ServerParameterManager().UnPack(u.Deserialize<object>(System.Convert.FromBase64String(colDef.DefaultValue)));
                return typedCol;
            }

            if (colDef.TypeName == typeof(Date).Name)
            {
                var typedCol = new DateColumn(colDef.Name, colDef.Format);
                typedCol.Storage = CreateStorage(typedCol, colDef);
                if (typedCol.DefaultValue != null && typedCol.DefaultValue != Date.Empty)
                    typedCol.DefaultValue = (Date)new Remoting.ServerParameterManager().UnPack(u.Deserialize<object>(System.Convert.FromBase64String(colDef.DefaultValue)));
                return typedCol;
            }

            if (colDef.TypeName == typeof(Time).Name)
            {
                var typedCol = new TimeColumn(colDef.Name, colDef.Format);
                typedCol.Storage = CreateStorage(typedCol, colDef);
                if (typedCol.DefaultValue != null && typedCol.DefaultValue != Time.StartOfDay)
                    typedCol.DefaultValue = (Time)new Remoting.ServerParameterManager().UnPack(u.Deserialize<object>(System.Convert.FromBase64String(colDef.DefaultValue)));
                return typedCol;
            }

            if (colDef.TypeName == typeof(Bool).Name)
            {
                var typedCol = new BoolColumn(colDef.Name, colDef.Format);
                typedCol.Storage = CreateStorage(typedCol, colDef);
                if (typedCol.DefaultValue == true)
                    typedCol.DefaultValue = (Bool)new Remoting.ServerParameterManager().UnPack(u.Deserialize<object>(System.Convert.FromBase64String(colDef.DefaultValue)));
                return typedCol;
            }


            if (colDef.TypeName == typeof(byte[]).Name)
            {
                var typedCol = new ByteArrayColumn(colDef.Name, colDef.Format);
                typedCol.Storage = CreateStorage(typedCol, colDef);
                if (typedCol.DefaultValue != null && !String.IsNullOrEmpty(typedCol.DefaultValue.ToString().Trim()))
                    typedCol.DefaultValue = System.Convert.FromBase64String(colDef.DefaultValue);
                return typedCol;
            }

            throw new Exception("Unknown column type name");
        }

        static Sort CreateIndex(IndexDefinition idxDef, Entity e)
        {
            var idx = new BtrieveSort { Unique = idxDef.Unique, Name = idxDef.Name };
            Array.ForEach(idxDef.Segments, s =>

            {
                idx.Segments.Add(new SortSegment(e.Columns[s.ColumnName],
                  s.Descending ? SortDirection.Descending : SortDirection.Ascending));
                if (s.Size > 0)
                    idx.ModifySegmentLength(e.Columns[s.ColumnName], s.Size);

            });
            return idx;
        }

        public static void CompareForTesting(BtrieveEntity e)
        {
            string folderA = "Old", folderB = "New";
            foreach (var d in new[] { folderA, folderB })
            {
                if (!Directory.Exists(d))
                    Directory.CreateDirectory(d);
            }

            var fileName = e.GetType().FullName;
            ExportTable(e, Path.Combine(folderA, fileName + ".txt"));
            var def = DefOf(e);
            CreateEntitiesBasedOnDefinition(def, def, e.DataProvider, (a, b) => ExportTable(a, Path.Combine(folderB, fileName + ".txt")));
            Windows.OSCommand(String.Format(@"""C:\Program Files\Beyond Compare 4\BCompare.exe"" {0} {1}", folderA, folderB));
        }

        static void ExportTable(BtrieveEntity e, string fileName)
        {
            using (var fw = new StreamWriter(fileName, false, LocalizationInfo.Current.OuterEncoding))
            {
                e.ForEachRow(() => fw.WriteLine(String.Join(",", e.Columns.ToArray().Select(c => c.ToString()))));
            }
        }

        static void Convert(BtrieveEntity e, string newDefBase64, string oldDefBase64)
        {
            Convert(newDefBase64, oldDefBase64, e.DataProvider, BtrieveEntity.GetFullBtrievePathAndName(e));
        }

        public static void Convert(string newDefAsBase64, string oldDefAsBase64, IEntityDataProvider dataProvider, string fileName)
        {
            Convert(FromBase64(newDefAsBase64), FromBase64(oldDefAsBase64), dataProvider, fileName);
        }

        internal static void Convert(TableDefinition target, TableDefinition source, IEntityDataProvider dataProvider, string fileName)
        {
            CreateEntitiesBasedOnDefinition(target, source, dataProvider, (s, t) => Convert(s, t, fileName));
        }

        static void Convert(BtrieveEntity source, BtrieveEntity target, string fileName = null)
        {
            Compare(target, source, compareResults =>
            {
                if (compareResults.HasChanges)
                {
                    source.EntityName = fileName ?? target.EntityName;
                    target.EntityName = target.EntityName + "_new";
                    if (target.Exists())
                        target.Drop();
                    target.FileSharing = BtrieveFileSharing.None;
                    target.OpenMode = BtrieveOpenMode.Reindex;

                    var bp = new BusinessProcess { From = source };
                    bp.Relations.Add(target, RelationType.Insert);
                    bp.ForEachRow(() =>
                    {
                        foreach (var col in target.Columns)
                        {
                            var isNewColumn = compareResults.GetColumnStatus(col.Name) == CompareStatus.Added;
                            if (!isNewColumn)
                            {
                                SetValue(col, source.Columns[col.Name]);
                            }
                        }
                    });

                    var sourceFile = BtrieveEntity.GetFullBtrievePathAndName(source);
                    var targetFile = BtrieveEntity.GetFullBtrievePathAndName(target);
                    File.Copy(sourceFile, sourceFile + "_bck_" + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss"));
                    source.Drop();
                    File.Copy(targetFile, sourceFile);
                    target.Drop();
                }
            });

        }

        static void SetValue(ColumnBase to, ColumnBase value)
        {
            switch (UserMethods.GetAttribute(to))
            {
                case "L":
                    {
                        Bool result;
                        if (Bool.TryCast(value, out result))
                            to.Value = result;
                        break;
                    }
                case "T":
                    {
                        Time result;
                        if (Time.TryCast(value, out result))
                            to.Value = result;
                        break;
                    }
                case "N":
                    {
                        if (value is TypedColumnBase<Text>)
                        {
                            to.Value = Number.Parse(value.ToString(),
                                value.Format);
                        }
                        else
                        {
                            Number result;
                            if (Number.TryCast(value, out result))
                                to.Value = result;
                        }

                        break;
                    }
                case "A":
                    to.Value = value.ToString();
                    break;
                case "D":
                    {
                        Date result;
                        if (Date.TryCast(value, out result))
                            to.Value = result;
                        break;
                    }
                default:
                    to.Value = value;
                    break;
            }
        }

        internal static void Compare(string newDefAsBase64, string oldDefAsBase64, IEntityDataProvider dataProvider, Action<CompareResults> doOnResults)
        {
            Compare(FromBase64(newDefAsBase64), FromBase64(oldDefAsBase64), dataProvider, doOnResults);
        }
        static void Compare(TableDefinition newDef, TableDefinition oldDef, IEntityDataProvider dataProvider, Action<CompareResults> doOnResults)
        {
            CreateEntitiesBasedOnDefinition(newDef, oldDef, dataProvider, (source, target) => Compare(target, source, doOnResults));
        }

        internal static void Compare(Entity target, Entity source, Action<CompareResults> DoOnResults)
        {
            var results = new CompareResults();
            CompareColumns(target, source, results);
            CompareIndexes(target, source, results);

            DoOnResults(results);
        }

        static void CompareIndexes(Entity target, Entity source, CompareResults results)
        {
            var sourceIndexes = source.Indexes.OrderBy(idx => idx.Name).ToArray();
            var targetIndexes = target.Indexes.OrderBy(idx => idx.Name).ToArray();

            int i = 0;
            int j = 0;
            while (i < sourceIndexes.Length)
            {
                if (j >= targetIndexes.Length)
                {
                    results.AddIndex(sourceIndexes[i].Name, CompareStatus.Deleted);
                    i++;
                }
                else
                {
                    if (sourceIndexes[i].Name.CompareTo(targetIndexes[j].Name) == 0)
                    {
                        results.AddIndex(targetIndexes[j].Name,
                            CompareIndexProperties(DefOf(targetIndexes[j]), DefOf(sourceIndexes[j])));
                        i++;
                        j++;
                    }
                    else if (sourceIndexes[i].Name.CompareTo(targetIndexes[j].Name) < 0)
                    {
                        results.AddIndex(sourceIndexes[i].Name, CompareStatus.Deleted);
                        i++;
                    }
                    else
                    {
                        results.AddIndex(targetIndexes[j].Name, CompareStatus.Added);
                        j++;
                    }
                }
            }
            while (j < targetIndexes.Length)
            {
                results.AddIndex(targetIndexes[j++].Name, CompareStatus.Added);
            }
        }

        static CompareStatus CompareIndexProperties(IndexDefinition target, IndexDefinition source)
        {
            if (target.Unique != source.Unique)
                return CompareStatus.Changed;
            return source.Segments.SequenceEqual(target.Segments, new SegmentEqualityComparer())
                ? CompareStatus.Same
                : CompareStatus.Changed;
        }

        class SegmentEqualityComparer : IEqualityComparer<SegmentDefinition>
        {
            public bool Equals(SegmentDefinition x, SegmentDefinition y)
            {
                if (x == null && y == null)
                    return true;
                if (x == null | y == null)
                    return false;
                if (x.ColumnName == y.ColumnName && x.Descending == y.Descending)
                    return true;
                return false;
            }

            public int GetHashCode(SegmentDefinition obj)
            {
                return obj.ColumnName.GetHashCode() + obj.Descending.GetHashCode();
            }
        }

        static void CompareColumns(Entity target, Entity source, CompareResults results)
        {
            var sourceColumns = source.Columns.OrderBy(c => c.Name).ToArray();
            var targetColumns = target.Columns.OrderBy(c => c.Name).ToArray();
            int i = 0;
            int j = 0;
            while (i < sourceColumns.Length)
            {
                if (j >= targetColumns.Length)
                {
                    results.AddColumn(sourceColumns[i].Name, CompareStatus.Deleted);
                    i++;
                }
                else
                {
                    if (sourceColumns[i].Name.CompareTo(targetColumns[j].Name) == 0)
                    {
                        results.AddColumn(targetColumns[j].Name, CompareColumnProperties(targetColumns[j], sourceColumns[i]));
                        i++;
                        j++;
                    }
                    else if (sourceColumns[i].Name.CompareTo(targetColumns[j].Name) < 0)
                    {
                        results.AddColumn(sourceColumns[i].Name, CompareStatus.Deleted);
                        i++;
                    }
                    else
                    {
                        results.AddColumn(targetColumns[j].Name, CompareStatus.Added);
                        j++;
                    }
                }
            }

            while (j < targetColumns.Length)
            {
                results.AddColumn(targetColumns[j++].Name, CompareStatus.Added);
            }
        }

        static CompareStatus CompareColumnProperties(ColumnBase target, ColumnBase source)
        {
            var targetDef = DefOf(target);
            var sourceDef = DefOf(source);

            return targetDef.TypeName == sourceDef.TypeName &&
                   targetDef.Format == sourceDef.Format &&
                   targetDef.StorageTypeName == sourceDef.StorageTypeName
                ? CompareStatus.Same
                : CompareStatus.Changed;
        }

        internal static void CheckTableDefinitionChanges(Entity entity, Action ok)
        {
            if (!AutoConversionEnabled)
                return;

            var be = entity as BtrieveEntity;
            if (be != null)
            {
                var oldDefinitionFile = Path.Combine(Application.UserAppDataPath, be.GetType().FullName);

                if (!File.Exists(oldDefinitionFile))
                {
                    SaveBtrieveDefinition(entity);
                }
                else
                {
                    var oldDefBase64 = File.ReadAllText(oldDefinitionFile);
                    var newDefBase64 = DefOfAsBase64(be);

                    Compare(newDefBase64, oldDefBase64, be.DataProvider, results =>
                    {
                        if (results.HasChanges)
                        {
                            if (MessageBox.Show(
                                    "The following definition changes were detected.\nWould you like to try converting the file?" +
                                    results,
                                    "Conversion", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                                DialogResult.Yes)
                            {
                                try
                                {
                                    Convert(be, newDefBase64, oldDefBase64);
                                    SaveBtrieveDefinition(entity);
                                    if (MessageBox.Show(
                                            "Converted Successfully!\nWould you like to see the code that does the conversion?",
                                            "Conversion Succeeded!", MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question) == DialogResult.Yes)
                                    {
                                        DisplayCode(oldDefBase64, newDefBase64, be);
                                    }

                                }
                                catch
                                {
                                    MessageBox.Show(
                                        "Conversion failed. Nothing was changed.\nCheck the log file for details",
                                        "Conversion Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    });
                }
            }
        }

        static void DisplayCode(string oldDefBase64, string newDefBase64, BtrieveEntity e)
        {
            var code = new StringBuilder();
            code.AppendLine("// Copy this code to a database migration program");
            code.AppendLine();
            code.AppendLine(String.Format("var oldDef = \"{0}\";", oldDefBase64));
            code.AppendLine();
            code.AppendLine(String.Format("var newDef = \"{0}\";", newDefBase64));
            code.AppendLine();
            code.AppendLine(String.Format("var {0} = new {1}();", e.GetType().Name.ToLower(), e.GetType().FullName));
            code.AppendLine();
            code.AppendLine(
                String.Format("ENV.Utilities.TableDefinitionConverter.Convert(newDef, oldDef, {0}.DataProvider, @\"{1}\");", e.GetType().Name.ToLower(), e.EntityName));
            EntityBrowser.ShowString("Here is the code", code.ToString());
        }

        public static void EnableBtrieveAutoConversion()
        {
            AutoConversionEnabled = true;
        }

        public static void SaveBtrieveDefinition(Entity e)
        {
            var fileName = Path.Combine(Application.UserAppDataPath, e.GetType().FullName);

            var be = e as BtrieveEntity;
            if (be != null)
            {
                try
                {
                    File.WriteAllText(fileName, DefOfAsBase64(be));
                }
                catch
                { }
            }
        }
    }

    [Serializable]
    public class TableDefinition
    {
        public string TypeName;
        public string SqlName;
        public string Caption;
        public string SqlSchema;
        public string BtrieveName;
        public string Owner;
        public ColumnDefinition[] Columns;
        public IndexDefinition[] Indexes;

    }

    [Serializable]
    public class ColumnDefinition
    {
        public string TypeName;
        public string Name;
        public string Format;
        public string StorageTypeName;
        public string DefaultValue;
    }

    [Serializable]
    public class IndexDefinition
    {
        public string Name;
        public bool Unique;
        public SegmentDefinition[] Segments;
    }

    [Serializable]
    public class SegmentDefinition
    {
        public string ColumnName;
        public bool Descending;
        public int Size;

    }

    class CompareResults
    {
        internal bool HasChanges;
        readonly Dictionary<string, CompareStatus> _columns = new Dictionary<string, CompareStatus>();
        readonly Dictionary<string, CompareStatus> _indexes = new Dictionary<string, CompareStatus>();

        internal void AddColumn(string name, CompareStatus status)
        {
            _columns.Add(name, status);
            if (status != CompareStatus.Same)
                HasChanges = true;
        }

        public CompareStatus GetColumnStatus(string name)
        {
            return _columns[name];
        }

        public CompareStatus GetIndexStatus(string name)
        {
            return _indexes[name];
        }

        public void AddIndex(string name, CompareStatus status)
        {
            _indexes.Add(name, status);
            if (status != CompareStatus.Same)
                HasChanges = true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine();
            foreach (var col in _columns)
            {
                if (col.Value != CompareStatus.Same)
                    sb.AppendLine(string.Format("Column \"{0}\" was {1}", col.Key, col.Value));
            }

            foreach (var idx in _indexes)
            {
                if (idx.Value != CompareStatus.Same)
                    sb.AppendLine(string.Format("Index: \"{0}\" was {1}", idx.Key, idx.Value));
            }

            return sb.ToString();
        }
    }

    enum CompareStatus
    {
        Same,
        Added,
        Deleted,
        Changed
    }

}
