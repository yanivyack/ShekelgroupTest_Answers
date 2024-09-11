using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ENV.Data.DataProvider;
using Firefly.Box.Data;
using Firefly.Box.Data.Advanced;
using ENV.Data;
using BoolColumn = ENV.Data.BoolColumn;
using ByteArrayColumn = ENV.Data.ByteArrayColumn;
using DateColumn = ENV.Data.DateColumn;
using Entity = ENV.Data.Entity;
using NumberColumn = ENV.Data.NumberColumn;
using TextColumn = ENV.Data.TextColumn;
using TimeColumn = ENV.Data.TimeColumn;
using System.Runtime.InteropServices;

namespace ENV.Utilities
{
    public enum ScriptType
    {
        Full,
        TableOnly,
        IndexOnly,
        AddColumns
    }
    public class EntityScriptGenerator
    {
        SqlScriptGenerator _sql;

        public EntityScriptGenerator(SqlScriptGenerator sql)
        {
            _sql = sql;
        }
        public ScriptType ScriptType = ScriptType.Full;

        public EntityScriptGenerator(bool oracle)
        {
            _sql = new SqlScriptGenerator(oracle);
        }

        public void ToClipBoard()
        {

            using (var s = new System.IO.StringWriter())
            {
                ToWriter(s);
                ENV.UserMethods.Instance.ClipAdd(s.ToString());
                ENV.UserMethods.Instance.ClipWrite();
            }

        }
        public void ToWriter(System.IO.TextWriter writer)
        {
            _sql.WriteTo(writer, ScriptType);
        }
        public void Execute(Action<string> executer)
        {
            _sql.Execute(executer, ScriptType);
        }

        public void ToFile(string fileName)
        {
            using (var sw = new System.IO.StreamWriter(fileName, false, System.Text.Encoding.Default))
            {
                ToWriter(sw);
            }
        }

        private static HashSet<string> _invalidNames =
            new HashSet<string>(new[] { "GRANT", "CASE", "SELECT", "TABLE", "USER" });
        public static bool ignoreDbTypeProperty { get; set; }
        public static bool CreatePrimaryKeyAsPrimaryKeyInsteadOfUniqueIndex { get; set; }

        public static string FixNameForDb(string name, int maxLength = int.MaxValue, string numberedNamePrefix = "_")
        {
            var newNameCharArray = name.ToArray();
            for (int i = 0; i < newNameCharArray.Length; i++)
            {
                if (!"ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".Contains(newNameCharArray[i].ToString().ToUpper()))
                {
                    newNameCharArray[i] = '_';
                }
            }
            var newName = new string(newNameCharArray);
            while (newName.Contains("__"))
                newName = newName.Replace("__", "_");
            if (newName.Length == 0 || "1234567890".Contains(newName[0]))
                newName = numberedNamePrefix + newName;
            if (_invalidNames.Contains(newName.ToUpper()))
                newName += "_";
            if (newName.Length > maxLength)
                newName = newName.Remove(maxLength);
            var result = newName;
            return result;
        }
        public static string GenerateOKIndexName(string caption, HashSet<string> usedNames, int maxLength, string numberedNamePrefix)
        {
            var newName = FixNameForDb(caption, maxLength, numberedNamePrefix);
            var result = newName;
            int j = 0;
            while (usedNames.Contains(result.ToUpper()))
            {
                j++;
                var s = j.ToString();
                if (s.Length + newName.Length > maxLength)
                    newName = newName.Remove(maxLength - s.Length);
                result = newName + j;
            }
            usedNames.Add(result.ToUpper());
            return result;
        }

        public void AddEntity(Firefly.Box.Data.Entity e)
        {
            var t = _sql.CreateTable(e, "");
            foreach (var c in e.Columns)
            {
                AddColumnToTableCreator(t, c);
            }

            bool needPKIndex = true;
            var usedNames = new HashSet<string>();
            foreach (var indexxxx in e.Indexes)
            {
                var index = indexxxx as Index;
                if (index == null)
                    continue;
                if (!index.AutoCreate)
                    continue;
                if (index.Unique && CreatePrimaryKeyAsPrimaryKeyInsteadOfUniqueIndex && index.Segments.Count == e.PrimaryKeyColumns.Length)
                {
                    bool allOk = true;
                    var hash = new HashSet<ColumnBase>(e.PrimaryKeyColumns);
                    for (int i = 0; i < index.Segments.Count; i++)
                    {
                        if (!hash.Contains(index.Segments[i].Column))
                        {
                            allOk = false;
                            break;
                        }
                    }
                    if (allOk && needPKIndex)
                    {
                        var x = new List<string>();
                        foreach (var list in index.Segments)
                        {
                            var tc = list.Column as TimeColumn;
                            if (tc != null && tc.DateColumnForDateTimeStorage != null)
                                continue;
                            x.Add(list.Column.Name);
                        }
                        if (x.Count > 0)
                            t.AddPrimaryKey(x);
                        needPKIndex = false;
                        continue;
                    }
                }
                string name = PathDecoder.DecodePath(index.Name) ?? GenerateOKIndexName(e.EntityName + index.Caption, usedNames, t.MaxNameLength, "Ix_");
                HashSet<ColumnBase> usedColumns = AddIndexToTableCreator(t, index, name);
                if (e.PrimaryKeyColumns.Length - usedColumns.Count <= 1)
                {
                    bool allAreHere = index.Unique;
                    foreach (var c in e.PrimaryKeyColumns)
                    {
                        if (IsPartOfDateTime(c))
                            continue;

                        if (!usedColumns.Contains(c))
                        {
                            allAreHere = false;
                            break;
                        }
                    }
                    if (allAreHere && e.PrimaryKeyColumns.Length == indexxxx.Segments.Count)
                        needPKIndex = false;
                }
            }
            if (needPKIndex)
            {
                var x = new List<string>();
                foreach (var list in e.PrimaryKeyColumns)
                {
                    var tc = list as TimeColumn;
                    if (tc != null && tc.DateColumnForDateTimeStorage != null)
                        continue;
                    x.Add(list.Name);
                }
                if (x.Count > 0)
                    t.AddPrimaryKey(x);
            }


        }

        static HashSet<ColumnBase> AddIndexToTableCreator(SqlScriptTableCreator t, Index index, string name)
        {
            var usedColumns = new HashSet<ColumnBase>();
            var items = new List<string>();
            foreach (var segment in index.Segments)
            {
                if (IsPartOfDateTime(segment.Column))
                    continue;
                items.Add(segment.Column.Name +
                          (segment.Direction == Firefly.Box.SortDirection.Descending ? " desc" : ""));
                usedColumns.Add(segment.Column);
            }

            t.AddIndex(name, index.Unique, items.ToArray());
            return usedColumns;
        }

        static void AddColumnToTableCreator(SqlScriptTableCreator t, ColumnBase c)
        {
            if (c.DbReadOnly && c.Name.StartsWith("="))
                return;
            string dbType = null;
            string dbDefault = null;
            var col = c as SqlScriptGenerator.IColumn;
            if (col != null)
            {
                if (!ignoreDbTypeProperty)
                {
                    dbType = col.DbType;
                    if (dbType == "SQL_INTEGER")
                        dbType = "integer";
                    if (dbType == "SQL_VARCHAR")
                        dbType = "varchar";
                }
                dbDefault = col.DbDefault;
            }
            var h = new ScriptHelper(c, dbDefault);
            if (!string.IsNullOrEmpty(dbType))
                t.AddCustomColumn(c.Name, c.Caption, dbType, dbDefault, c.AllowNull);
            else if (c == c.Entity.IdentityColumn)
                t.AddIdentityColumn(c.Name, c.Caption);
            else if (c is Firefly.Box.Data.NumberColumn)
                AddToT((Firefly.Box.Data.NumberColumn)c, t, () => t.AddNumber(c.Name, c.Caption, c.Format, c.AllowNull), h);
            else if (c is Firefly.Box.Data.TextColumn)
                AddToT((Firefly.Box.Data.TextColumn)c, t, () => t.AddText(c.Name, c.Caption, c.Format, c.AllowNull, true, false, h), h);
            else if (c is Firefly.Box.Data.BoolColumn)
                AddToT((Firefly.Box.Data.BoolColumn)c, t, () => t.AddBoolInt(c.Name, c.Caption, c.AllowNull), h);
            else if (c is Firefly.Box.Data.DateColumn)
                AddToT((Firefly.Box.Data.DateColumn)c, t, () =>
                {
                    if (((DateColumn)c).TimeColumnForDateTimeStorage != null)
                        t.AddDateTime(c.Name, c.Caption, c.AllowNull, h);
                    else
                        t.AddStringDate(c.Name, c.Caption, c.AllowNull, h);
                }, h);
            else if (c is Firefly.Box.Data.DateTimeColumn)
                AddToT((Firefly.Box.Data.DateTimeColumn)c, t, () => t.AddDateTime(c.Name, c.Caption, c.AllowNull, h), h);
            else if (c is Firefly.Box.Data.TimeColumn)
            {
                var tc = c as Firefly.Box.Data.TimeColumn;
                if (tc.DateColumnForDateTimeStorage == null)
                    AddToT((Firefly.Box.Data.TimeColumn)c, t, () => t.AddIntegerTime(c.Name, c.Caption, c.AllowNull), h);
            }
            else if (c is Firefly.Box.Data.ByteArrayColumn)
                AddToT((Firefly.Box.Data.ByteArrayColumn)c, t, () => t.AddBinary(c.Name, c.Caption, 0), h);
        }
        public static void AddColumn(ColumnBase columnToAdd, DynamicSQLSupportingDataProvider db = null)
        {
            db = GetDb(db, columnToAdd.Entity);
            var t = db.CreateScriptGeneratorTable(columnToAdd.Entity);
            AddColumnToTableCreator(t, columnToAdd);
            t.WriteTo(db.Execute, ScriptType.AddColumns);
        }
        public static void AddIndex(Entity e, Index index, DynamicSQLSupportingDataProvider db = null)
        {
            db = GetDb(db, e);
            var t = db.CreateScriptGeneratorTable(e);
            AddIndexToTableCreator(t, index, index.Name);
            t.WriteTo(db.Execute, ScriptType.IndexOnly);
        }
        public static void AddIndexIfNotFound(Entity e, Index index, DynamicSQLSupportingDataProvider db = null)
        {
            db = GetDb(db, e);
            if (db.IsOracle)
            {
                var idx = new GetDefinition.OracleTables.Indexes(e.DataProvider);
                if (idx.Contains(EntityNameFilter(idx.TableName, e).And(idx.Name.IsEqualTo(index.Name))))
                    return;
            }
            else
            {
                var idx = new GetDefinition.Tables.Indexes(e.DataProvider);
                if (idx.Contains(EntityNameFilter(idx.TableName, e).And(idx.Name.IsEqualTo(index.Name))))
                    return;
            }
            var t = db.CreateScriptGeneratorTable(e);
            AddIndexToTableCreator(t, index, index.Name);
            t.WriteTo(db.Execute, ScriptType.IndexOnly);
        }
        public static void AddColumnIfNotFound(ColumnBase columnToAdd, DynamicSQLSupportingDataProvider db = null)
        {
            db = GetDb(db, columnToAdd.Entity);
            var colName = columnToAdd.Name;
            if (colName.StartsWith("["))
                colName = colName.Substring(1, colName.Length - 2);
            if (db.IsOracle)
            {
                var cols = new GetDefinition.OracleTables.Columns(db);
                var fc = new FilterCollection();
                if (cols.Contains(EntityNameFilter(cols.TableName, columnToAdd.Entity).And(cols.ColumnName.IsEqualTo(colName))))
                    return;
            }
            else
            {
                var cols = new GetDefinition.Tables.Columns(db);
                var fc = new FilterCollection();
                if (cols.Contains(EntityNameFilter(cols.TableName, columnToAdd.Entity).And(cols.ColumnName.IsEqualTo(colName))))
                    return;
            }
            var t = db.CreateScriptGeneratorTable(columnToAdd.Entity);
            AddColumnToTableCreator(t, columnToAdd);
            t.WriteTo(db.Execute, ScriptType.AddColumns);
        }
        static FilterBase EntityNameFilter(TextColumn col, Firefly.Box.Data.Entity e)
        {
            var entityName = e.EntityName.Replace("dbo.", "").Replace("[", "").Replace("]", "");

            return col.IsEqualTo(entityName);
        }
        public static void VerifyEntityStructure(ENV.Data.Entity e)
        {
            if (!e.Exists())
            {
                var db = GetDb(null, e);
                db.CreateTable(e);
                return;
            }
            foreach (var column in e.Columns)
            {
                AddColumnIfNotFound(column);
            }
            foreach (var index in e.Indexes)
            {
                var idx = index as Index;
                if (idx != null && idx.AutoCreate)
                {

                    AddIndexIfNotFound(e, idx);
                }
            }
        }



        private static DynamicSQLSupportingDataProvider GetDb(DynamicSQLSupportingDataProvider db, Firefly.Box.Data.Entity e)
        {
            if (db == null)
            {
                var envE = e as ENV.Data.Entity;
                if (envE == null)
                    throw new InvalidOperationException(nameof(db) + " should be provided or Entity should be " + typeof(Entity).FullName);
                db = envE.DataProvider as DynamicSQLSupportingDataProvider;
                if (db == null)
                {
                    if (envE == null)
                        throw new InvalidOperationException(nameof(db) + " should be provided or Entity Database should be " + typeof(DynamicSQLSupportingDataProvider).FullName);
                }
            }

            return db;
        }

        static bool IsPartOfDateTime(ColumnBase column)
        {
            var tc = column as TimeColumn;
            return tc != null && tc.DateColumnForDateTimeStorage != null;
        }

        public static void AddToT<T>(TypedColumnBase<T> c, SqlScriptTableCreator t, Action ifDoesntHaveStorage, ScriptHelper helper)
        {
            var s = (c).Storage as IStorageScriptCreator;
            if (s != null)
                s.AddTo(t, c.Name, c.Caption, c.AllowNull, helper);
            else
                ifDoesntHaveStorage();
        }
    }
    interface IStorageScriptCreator
    {
        void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper);
    }

    public interface SqlScriptTableCreator
    {
        void WriteTo(Action<string> commandExecutioner, ScriptType scriptType);
        void AddIdentityColumn(string name, string caption);
        void AddNumber(string name, string caption, string format, bool allowNull);
        void AddStringDate(string name, string caption, bool allowNull, ScriptHelper helper);
        void AddIntegerTime(string name, string caption, bool allowNull);
        void AddText(string name, string caption, string format, bool allowNull, bool unicode, bool fixedWidth, ScriptHelper helper);
        void AddBoolInt(string name, string caption, bool allowNull);
        void AddBinary(string name, string caption, int i);
        void AddStringTime(string name, string caption, bool allowNull);
        void AddDateTime(string name, string caption, bool allowNull, ScriptHelper helper);
        void AddBoolString(string name, string caption, bool allowNull);
        string CommandSeparator { get; }
        int MaxNameLength { get; }
        void AddPrimaryKey(IEnumerable<string> columns);
        void AddIndex(string name, bool unique, IEnumerable<string> items);

        void AddDate(string name, string caption, bool allowNull, ScriptHelper helper);
        void AddTimeSpan(string name, string caption, bool allowNull, ScriptHelper helper);
        void AddCustomColumn(string name, string caption, string dbType, string dbDefault, bool allowNull);
    }
    public class ScriptHelper
    {
        private ColumnBase _column;
        private string _dbDefault;

        public ScriptHelper(ColumnBase c, string dbDefault)
        {
            this._column = c;
            this._dbDefault = dbDefault;
        }

        internal string GetStringDefault()
        {
            string r = _dbDefault;
            if (r == null)
            {
                if (MssqlTable.UseDefaultsAsDBDefaults)
                {
                    var c = _column as Firefly.Box.Data.TextColumn;
                    if (c != null)
                    {
                        r = c.DefaultValue;
                        if (r == null)
                            if (c.AllowNull)
                                return "'null'";
                            else
                                r = "";
                        else
                            r = r.TrimEnd();


                    }
                }
                else
                    return "' '";
            }

            if (r == "")
                r = " ";
            if (r.StartsWith("'") && r.EndsWith("'"))
                return r;
            return "'" + r.Replace("'", "''") + "'";
        }

        internal void ProvideDefault(Action<string> to)
        {
            if (MssqlTable.UseDefaultsAsDBDefaults)
            {
                var x = new NoParametersFilterItemSaver(true, SQLClientEntityDataProvider.DateTimeStringFormat, DummyDateTimeCollector.Instance);
                _column.SaveYourValueToDb(x);
                to(x.Result);
            }
        }
    }

    interface SqlScriptGeneratorHelper
    {
        string GetEntityName();
    }

    public class SqlScriptGenerator
    {
        public interface IColumn
        {
            string DbType { get; set; }
            string DbDefault { get; set; }
        }
        Dictionary<string, List<SqlScriptTableCreator>> _tables = new Dictionary<string, List<SqlScriptTableCreator>>();
        Func<Firefly.Box.Data.Entity, SqlScriptTableCreator> _createTable;
        public SqlScriptGenerator()
            : this(false)
        {
        }
        public SqlScriptGenerator(Func<Firefly.Box.Data.Entity, SqlScriptTableCreator> createTable)
        {
            _createTable = createTable;
        }

        static string GetEntityName(Firefly.Box.Data.Entity e)
        {
            var h = e as SqlScriptGeneratorHelper;
            if (h != null)
                return h.GetEntityName();
            return e.EntityName;

        }

        public SqlScriptGenerator(bool oracle)
        {
            if (oracle)
                _createTable = s => new OracleTable(GetEntityName(s));
            else
                _createTable = s => new MssqlTable(GetEntityName(s), s);
        }

        public SqlScriptTableCreator CreateTable(Firefly.Box.Data.Entity dbname, string databaseName)
        {
            var result = _createTable(dbname);
            List<SqlScriptTableCreator> t;
            if (!_tables.TryGetValue(databaseName, out t))
            {
                t = new List<SqlScriptTableCreator>();
                _tables.Add(databaseName, t);
            }
            t.Add(result);
            return result;
        }

        public void WriteTo(TextWriter writer, ScriptType scriptType)
        {

            foreach (var l in _tables)
            {


                foreach (var table in l.Value)
                {
                    table.WriteTo(x =>
                    {
                        writer.WriteLine(x);
                        writer.WriteLine(table.CommandSeparator);
                    }, scriptType);
                }
            }

        }

        public void Execute(Action<string> executeScript, ScriptType scriptType)
        {
            foreach (var t in _tables.Values)
                foreach (var table in t)
                {
                    table.WriteTo(executeScript, scriptType);
                }
        }


    }
    public class TableBase
    {
        string _dbname;
        protected List<string> _fields = new List<string>();
        public TableBase(string dbname)
        {
            _dbname = dbname;
        }
        protected virtual string WrapName(string name)
        {
            return name;
        }

        public virtual void WriteTo(Action<string> script, ScriptType scriptType)
        {
            using (var sw = new StringWriter())
            {
                if (scriptType == ScriptType.AddColumns)
                {
                    foreach (var field in _fields)
                    {
                        script("alter table " + _dbname + " add " + field);
                    }

                }
                else
                {
                    if (scriptType != ScriptType.IndexOnly)
                    {
                        sw.WriteLine("create table {0} (", _dbname);
                        bool first = true;
                        foreach (var field in _fields)
                        {
                            if (first)
                                first = false;
                            else
                                sw.Write(",");
                            sw.WriteLine(field);
                        }
                        sw.WriteLine(")");
                        script(sw.ToString());
                    }
                    if (scriptType != ScriptType.TableOnly)
                    {
                        if (!string.IsNullOrEmpty(_pkColumns))
                            CreatePrimaryKeyIndex(script);
                        foreach (var action in _createIndexes)
                        {
                            action(script);
                        }
                    }
                    if (scriptType != ScriptType.IndexOnly)
                        AdditionalTableInfo(script);
                }

            }
        }
        public virtual void AddCustomColumn(string name, string caption, string dbType, string dbDefault, bool allowNull)
        {


            string s = WrapName(name) + " " + dbType;
            if (!string.IsNullOrEmpty(dbDefault))
                s += " default " + dbDefault + "";
            if (!allowNull)
                s += " not null";


            _fields.Add(s);
        }

        protected virtual void AdditionalTableInfo(Action<String> script)
        {
        }

        protected virtual void CreatePrimaryKeyIndex(Action<string> script)
        {
            string indexName = _dbname + "_pk";
            if (indexName.IndexOf('.') > -1)
                indexName = indexName.Substring(indexName.LastIndexOf('.') + 1);
            if (_pkColumns != "")
                script(string.Format("create unique index {0} on {2} ({1})", WrapName(indexName), _pkColumns, WrapName(_dbname)));
        }
        public virtual void AddIndex(string indexName, bool unique, IEnumerable<string> items)
        {

            if (indexName.IndexOf('.') > -1)
                indexName = indexName.Substring(indexName.LastIndexOf('.') + 1);
            var columns = "";
            foreach (var item in items)
            {
                if (columns.Length > 0)
                    columns += ", ";
                columns += WrapName(item);
            }
            _createIndexes.Add(script =>
                script(string.Format("create {3}index {0} on {2} ({1})", indexName, columns, _dbname, unique ? "Unique " : "")));
        }
        private List<Action<Action<string>>> _createIndexes = new List<Action<Action<string>>>();


        protected string _pkColumns = "";
        public void AddPrimaryKey(IEnumerable<string> columns)
        {
            foreach (var column in columns)
            {
                if (_pkColumns.Length > 0)
                    _pkColumns += ", ";
                _pkColumns += WrapName(column);
            }

        }
    }
    class OdbcTable : TableBase, SqlScriptTableCreator
    {
        protected override string WrapName(string name)
        {
            return _nameWrapper + base.WrapName(name) + _nameWrapper;
        }
        string _nameWrapper;
        public OdbcTable(string dbName, string nameWrapper)
            : base(dbName)
        {
            _nameWrapper = nameWrapper;
        }

        public void AddIdentityColumn(string name, string caption)
        {
            throw new NotSupportedException();
        }

        public void AddNumber(string name, string caption, string format, bool allowNull)
        {
            var f = new NumberFormatInfo(format);
            if (f.Precision == 0)
                f = new NumberFormatInfo("15.3");
            if (f.Scale == 0 && f.Precision <= 9)
                Add(name, " int" + (allowNull ? "" : " NOT NULL"));
            else
                Add(name, string.Format(" numeric({0},{1})", f.Precision, f.Scale) + (allowNull ? "" : " NOT NULL"));

        }
        void Add(string name, string info)
        {
            _fields.Add(string.Format("{0} {1}", WrapName(name), info));
        }

        public void AddStringDate(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            Add(name, " char(8) ");
        }

        public void AddIntegerTime(string name, string caption, bool allowNull)
        {
            Add(name, " integer ");
        }

        public void AddText(string name, string caption, string format, bool allowNull, bool unicode, bool fixedWidth, ScriptHelper helper)
        {
            var f = new TextFormatInfo(format);
            var s = string.Format(" nchar ({0}) ", f.MaxDataLength);
            if (f.MaxDataLength > 4000)
                s = "ntext";
            if (!unicode)
            {
                s = string.Format(" char ({0}) ", f.MaxDataLength);
                if (f.MaxDataLength > 4000)
                    s = "text";
            }
            s += (allowNull ? "" : (allowNull ? "" : " default " + helper.GetStringDefault() + " not null"));
            Add(name, s);
        }

        public void AddBoolInt(string name, string caption, bool allowNull)
        {
            Add(name, " integer ");
        }

        public void AddBinary(string name, string caption, int i)
        {
            Add(name, " long varbinary ");
        }

        public void AddStringTime(string name, string caption, bool allowNull)
        {
            Add(name, " char(6) ");
        }

        public void AddDateTime(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            Add(name, " datetime" + (allowNull ? "" : " not null"));
        }

        public void AddBoolString(string name, string caption, bool allowNull)
        {
            Add(name, " char(1)");
        }

        public string CommandSeparator
        {
            get { return "\r\ngo"; }
        }

        public int MaxNameLength
        {
            get { return 30; }
        }

        public void AddDate(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            Add(name, " date ");
        }

        public void AddTimeSpan(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            Add(name, " time ");
        }
    }
    public class MssqlTable : TableBase, SqlScriptTableCreator
    {
        Firefly.Box.Data.Entity _entity;
        bool _additionalInfo = false;

        public MssqlTable(string dbName, Firefly.Box.Data.Entity entity)
            : this(dbName, entity, false)
        {
        }

        public MssqlTable(string dbName, Firefly.Box.Data.Entity entity, bool doNotAddAdditionalInfo)
            : base(dbName)
        {
            _additionalInfo = !doNotAddAdditionalInfo;
            _entity = entity;
        }
        public static Boolean UseIntForPrimaryKey;
        public virtual void AddIdentityColumn(string name, string caption)
        {
            _fields.Add(name + (UseIntForPrimaryKey ? " int " : " bigint ") + "IDENTITY(1,1)");
        }
        public bool UsePrimaryKeys = UseIdentityAsPrimaryKey || EntityScriptGenerator.CreatePrimaryKeyAsPrimaryKeyInsteadOfUniqueIndex;
        protected override void CreatePrimaryKeyIndex(Action<string> script)
        {
            if (!UsePrimaryKeys)
                base.CreatePrimaryKeyIndex(script);
            else
                script(string.Format("alter table {0} add primary key ({1})", _entity.EntityName, _pkColumns));
        }

        public void AddNumber(string name, string caption, string format, bool allowNull)
        {
            var f = new NumberFormatInfo(format);

            string dataType = string.Format("decimal ({0},{1})", 18,
                                            f.Scale);
            if (f.Scale == 0)
            {
                if (f.Precision < 5 && !UseDecimalTypes && f.Precision > 0)
                    dataType = "smallint";
                else if (f.Precision < 10)
                    dataType = "int";
                else if (f.Precision < 16)
                    dataType = "bigint";
            }
            else
            {
                if (!UseDecimalTypes)
                {
                    if (f.Precision < 6)
                        dataType = "real";
                    else if (f.Precision < 16)
                        dataType = "float";
                }
            }
            _fields.Add(name + " " + dataType + (allowNull ? "" : " default 0 not null"));

        }

        public void AddStringDate(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            string theDefault = "'00000000'";
            helper.ProvideDefault(x => theDefault = x);

            _fields.Add(name + " char(8)" + (allowNull ? "" : " default " + theDefault + " not null"));
        }

        public void AddIntegerTime(string name, string caption, bool allowNull)
        {
            _fields.Add(name + " int" + (allowNull ? "" : " default 0 not null"));
        }

        public void AddText(string name, string caption, string format, bool allowNull, bool unicode, bool fixedWidth, ScriptHelper helper)
        {
            var f = new TextFormatInfo(format);

            _fields.Add(name + string.Format(" " + (unicode ? "n" : "") + (fixedWidth ? "char" : "varchar") + " ({0})" + (allowNull ? "" : " default " + helper.GetStringDefault() + " not null"), f.MaxDataLength == 0 || f.MaxDataLength > 4000 ? "max" : f.MaxDataLength.ToString()));
        }

        public void AddBoolInt(string name, string caption, bool allowNull)
        {
            string type = " smallint";
            if (UseDecimalTypes)
                type = " bit";
            _fields.Add(name + type + (allowNull ? "" : " default 0 not null"));
        }

        public void AddBinary(string name, string caption, int i)
        {
            if (i == 0)
                _fields.Add(name + " image");
            else
                _fields.Add(name + " binary (" + i + ")");
        }

        public void AddStringTime(string name, string caption, bool allowNull)
        {
            _fields.Add(name + " char(6)" + (allowNull ? "" : " default '000000' not null"));
        }

        public void AddDateTime(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            _fields.Add(name + " datetime" + (UseDatetime2 ? "2" : "") + (allowNull ? "" : " not null"));
        }

        public void AddBoolString(string name, string caption, bool allowNull)
        {
            _fields.Add(name + " char(1)" + (allowNull ? "" : " default 'F' not null"));
        }

        public string CommandSeparator
        {
            get { return "\r\ngo"; }
        }

        public int MaxNameLength
        {
            get { return 128; }
        }
        public static bool UseDecimalTypes { get; set; }//float is evil!!!! it's not accurate
        public static bool UseIdentityAsPrimaryKey { get; set; }
        public static bool UseDefaultsAsDBDefaults { get; set; }
        public static bool UseDatetime2 = false;



        public void AddDate(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            string column = name + " date";
            if (!allowNull)
                column += " not null";
            helper.ProvideDefault(d =>
            {
                if (!string.IsNullOrEmpty(d))
                    column += " default " + d;
            });
            _fields.Add(column);
        }

        public void AddTimeSpan(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            var column = name + " time";
            if (!allowNull)
                column += " not null";
            helper.ProvideDefault(d =>
            {
                if (!string.IsNullOrEmpty(d))
                    column += " default " + d;
            });
            _fields.Add(column);
        }



        public override void WriteTo(Action<string> script, ScriptType scriptType)
        {
            using (var sw = new StringWriter())
            {
                base.WriteTo(y =>
                {
                    sw.WriteLine(y);
                    sw.WriteLine(";");
                }, scriptType);
                script(sw.ToString());
            }

        }

        protected override void AdditionalTableInfo(Action<string> script)
        {
            if (_entity.EntityName.Contains("#") || !_additionalInfo || true)
                return;
            if (_entity.Caption != _entity.EntityName)
                script(string.Format(@"EXEC sys.sp_addextendedproperty 
   @name=N'MS_Description', @value=N'{0}' ,
	 @level0type=N'SCHEMA',@level0name=N'dbo', 
	  @level1type=N'TABLE'
	 ,@level1name=N'{1}'", _entity.Caption.Replace("'", "''"), _entity.EntityName.Replace("'", "''")));
            script(string.Format(@"EXEC sys.sp_addextendedproperty 
   @name=N'AppClassName', @value=N'{0}' ,
	 @level0type=N'SCHEMA',@level0name=N'dbo', 
	  @level1type=N'TABLE'
	 ,@level1name=N'{1}'", _entity.GetType().FullName, _entity.EntityName.Replace("'", "''")));

            var be = _entity as BtrieveEntity;
            if (be != null)
                script(string.Format(@"EXEC sys.sp_addextendedproperty 
   @name=N'Btrieve Name', @value=N'{0}' ,
	 @level0type=N'SCHEMA',@level0name=N'dbo', 
	  @level1type=N'TABLE'
	 ,@level1name=N'{1}'", be.BtrieveName, _entity.EntityName.Replace("'", "''")));
        }
    }
    class OracleTable : TableBase, SqlScriptTableCreator
    {
        public OracleTable(string dbname)
            : base(dbname)
        {
        }
        public void AddIdentityColumn(string name, string caption)
        {
            if (name.ToUpper().Trim() != "ROWID")
                AddNumber(name, caption, "15", false);
        }
        protected override void CreatePrimaryKeyIndex(Action<string> script)
        {

        }

        public void AddNumber(string name, string caption, string format, bool allowNull)
        {

            _fields.Add(name + string.Format(" Number" + (allowNull ? "" : " default 0 not null")));

        }

        public void AddStringDate(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            _fields.Add(name + " char(8)" + (allowNull ? "" : " default '00000000' not null"));
        }

        public void AddIntegerTime(string name, string caption, bool allowNull)
        {
            _fields.Add(name + " Number" + (allowNull ? "" : " default 0 not null"));
        }

        public void AddText(string name, string caption, string format, bool allowNull, bool unicode, bool fixedWidth, ScriptHelper helper)
        {
            var f = new TextFormatInfo(format);
            var dbType = "Clob";
            if (f.MaxDataLength < 4001 && f.MaxDataLength > 0)
                dbType = string.Format((unicode ? "n" : "") + (fixedWidth ? "char" : "varchar2") + " ({0})", f.MaxDataLength);
            _fields.Add(name + " " + dbType + (allowNull ? "" : " default " + helper.GetStringDefault() + " not null"));
        }

        public void AddBoolInt(string name, string caption, bool allowNull)
        {
            _fields.Add(name + " Number" + (allowNull ? "" : " default 0 not null"));
        }

        public void AddBinary(string name, string caption, int i)
        {
            if (i == 1)
                _fields.Add(name + " RAW(1)");
            else
                _fields.Add(name + " blob");
        }

        public void AddStringTime(string name, string caption, bool allowNull)
        {
            _fields.Add(name + " char(6)" + (allowNull ? "" : " default '000000' not null"));
        }

        public void AddDateTime(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            _fields.Add(name + " timestamp" + (allowNull ? "" : " not null"));
        }

        public void AddBoolString(string name, string caption, bool allowNull)
        {
            _fields.Add(name + " char(1)" + (allowNull ? "" : " default 'F' not null"));
        }

        public string CommandSeparator
        {
            get { return ";"; }
        }

        public int MaxNameLength
        {
            get { return 30; }
        }

        public void AddDate(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            _fields.Add(name + " date");
        }

        public void AddTimeSpan(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            throw new NotImplementedException();
        }
    }
    class Db2Table : TableBase, SqlScriptTableCreator
    {
        public Db2Table(string dbName)
            : base(dbName)
        {
        }

        public virtual void AddIdentityColumn(string name, string caption)
        {
            _fields.Add(name + " bigint NOT NULL GENERATED ALWAYS AS IDENTITY");
        }

        public void AddNumber(string name, string caption, string format, bool allowNull)
        {
            var f = new NumberFormatInfo(format);
            if (f.Precision == 0)
                f = new NumberFormatInfo("15.3");
            string dataType = string.Format("decimal ({0},{1})", f.Precision,
                                            f.Scale);
            if (f.Scale == 0)
            {
                if (f.Precision < 5 && f.Precision > 0)
                    dataType = "smallint";
                else if (f.Precision < 10)
                    dataType = "int";
                else if (f.Precision < 16)
                    dataType = "float";
            }
            else
            {
                if (f.Precision < 6)
                    dataType = "real";
                else if (f.Precision < 16)
                    dataType = "float";
            }
            _fields.Add(name + " " + dataType + (allowNull ? "" : " default 0 not null"));

        }

        public void AddStringDate(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            _fields.Add(name + " char(8)" + (allowNull ? "" : " default '00000000' not null"));
        }

        public void AddIntegerTime(string name, string caption, bool allowNull)
        {
            _fields.Add(name + " int" + (allowNull ? "" : " default 0 not null"));
        }

        public void AddText(string name, string caption, string format, bool allowNull, bool unicode, bool fixedWidth, ScriptHelper helper)
        {
            var f = new TextFormatInfo(format);

            _fields.Add(name + string.Format(" " + (unicode ? "n" : "") + (fixedWidth ? "char" : "varchar") + " ({0})" + (allowNull ? "" : " default " + helper.GetStringDefault() + " not null"), f.MaxDataLength == 0 || f.MaxDataLength > 4000 ? "max" : f.MaxDataLength.ToString()));
        }

        public void AddBoolInt(string name, string caption, bool allowNull)
        {
            _fields.Add(name + " smallint" + (allowNull ? "" : " default 0 not null"));
        }

        public void AddBinary(string name, string caption, int i)
        {
            if (i == 0)
                _fields.Add(name + " image");
            else
                _fields.Add(name + " varbinary");
        }

        public void AddStringTime(string name, string caption, bool allowNull)
        {
            _fields.Add(name + " char(6)" + (allowNull ? "" : " default '000000' not null"));
        }

        public void AddDateTime(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            _fields.Add(name + " timestamp" + (allowNull ? "" : " not null"));
        }

        public void AddBoolString(string name, string caption, bool allowNull)
        {
            _fields.Add(name + " char(1)" + (allowNull ? "" : " default 'F' not null"));
        }

        public string CommandSeparator
        {
            get { return ";"; }
        }

        public int MaxNameLength
        {
            get { return 128; }
        }

        public void AddDate(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            _fields.Add(name + " date");
        }

        public void AddTimeSpan(string name, string caption, bool allowNull, ScriptHelper helper)
        {
            _fields.Add(name + " time");
        }
    }
}
