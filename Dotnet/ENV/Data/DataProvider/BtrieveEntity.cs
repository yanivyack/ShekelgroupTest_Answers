using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Management;
using System.Windows.Forms;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.DataProvider
{
    public class BtrieveEntity : ENV.Data.IsamEntity, SqlScriptGeneratorHelper
    {
        
        protected static string AppPrefix { get { return ENV.UserMethods.Instance.Pref(); } }





        public static void CreateDetachScript(string fileName)
        {
            using (var sw = new System.IO.StreamWriter(fileName, false))
            {
                var done = new HashSet<string>();
                foreach (var dbName in _schemaConvertor.Values)
                {
                    if (done.Contains(dbName))
                        continue;
                    done.Add(dbName);

                    sw.WriteLine("EXEC master.dbo.sp_detach_db @dbname = N'{0}', @keepfulltextindexfile=N'true' ", dbName);
                    sw.WriteLine("go");
                }
            }
        }
        public static void CreateAttachScript(string fileName)
        {
            using (var sw = new System.IO.StreamWriter(fileName, false))
            {
                var done = new HashSet<string>();
                foreach (var dbName in _schemaConvertor.Values)
                {
                    if (done.Contains(dbName))
                        continue;
                    done.Add(dbName);

                    sw.WriteLine("CREATE DATABASE [{0}] ON ", dbName);
                    sw.WriteLine(@"( FILENAME = N'{0}.mdf' ),", dbName);
                    sw.WriteLine(@"( FILENAME = N'{0}_log.ldf' )", dbName);
                    sw.WriteLine("For ATTACH");
                    sw.WriteLine("go");
                }
            }
        }

        internal virtual bool ShoudBeMigrated
        {
            get { return true; }
        }

        internal static Dictionary<string, string> _schemaConvertor = new Dictionary<string, string>();

        internal protected readonly bool IsUsingBtrieve = false;
        public override string EntityName
        {
            get
            {
                if (IsUsingBtrieve)
                    return PathDecoder.DecodePath(BtrieveName);
                else
                {
                    return GetTheNameToUseInSql();
                }
            }
            set
            {
                BtrieveName = value;
            }
        }

        bool _initializedColumns = false;
        internal string GetTheNameToUseInSql()
        {
            if (!_initializedColumns && !IsUsingBtrieve)
            {
                //BtrieveToSQLMigrationManager.AdjustColumnsImplementation(this);
                _initializedColumns = true; 
            }
            if (_sqlNameWasChanged)
                return SqlName;
            var result = GetSQLName();
            if (DetermineSqlName != null)
            {
                var args = new BtrieveEntitySqlNameArgs { Entity = this, ResultSQLTableName = result };
                DetermineSqlName(args);
                result = args.ResultSQLTableName;
            }
            if (result == "")
                return BtrieveName;
            return result;
        }

        public static event BtrieveEntitySqlNameHandlers DetermineSqlName;
         internal string GetSQLName()
        {
            switch (SQLNameStrategy)
            {
                case SQLNameStrategies.SQLName:
                    return SqlName;

                case SQLNameStrategies.BtrieveNameWithoutPath:
                    return GetBtrieveNameWithoutPath();
                case SQLNameStrategies.OriginalName:
                    return GetFullBtrievePathAndName();
                case SQLNameStrategies.SQLNameWithSchemaBasedOnFolders:
                default:
                    return GetSQLNameWithSchemaBasedOnFolders();


            }

        }

        public static string GetBtrieveNameWithoutPath(BtrieveEntity e)
        {
            return e.GetBtrieveNameWithoutPath();
        }

        private string GetBtrieveNameWithoutPath()
        {
            var result = System.IO.Path.GetFileName(GetFullBtrievePathAndName());
            var ext = Path.GetExtension(result).ToUpper();
            if (ext == ".DAT" || ext == ".BTR")
                result = result.Remove(result.Length - 4);
            return EntityScriptGenerator.FixNameForDb(result);
        }

        private string GetSQLNameWithSchemaBasedOnFolders()
        {
            string schema = "";
            var path = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(GetFullBtrievePathAndName()));
            if (!path.EndsWith("\\") && path.Length > 0)
                path += "\\";
            if (_schemaConvertor.Count > 0)
                if (!_schemaConvertor.TryGetValue(path.ToUpper(), out schema))
                {
                    throw new Exception("Path " + path + " was not mapped");
                }
            if (schema == "")
                return SqlName;
            else
                return schema + ".dbo." + SqlName;
        }

        private string GetSchemaBasedOnFolderMap()
        {
            string schema = "";
            var path = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(GetFullBtrievePathAndName()));
            if (!path.EndsWith("\\") && path.Length > 0)
                path += "\\";
            if (_schemaConvertor.Count > 0)
                if (!_schemaConvertor.TryGetValue(path.ToUpper(), out schema))
                {
                    throw new Exception("Path " + path + " was not mapped");
                }

            return schema;
        }

        public static string GetSchemaBasedOnFolderMap(BtrieveEntity e)
        {
            return e.GetSchemaBasedOnFolderMap();
        }

        SQLNameStrategies _sqlNameStrategy = DefaultSQLNameStrategy;
        public static SQLNameStrategies DefaultSQLNameStrategy { get; set; }
        protected internal SQLNameStrategies SQLNameStrategy { get { return _sqlNameStrategy; } set { _sqlNameStrategy = value; } }
        internal string GetFullBtrievePathAndName()
        {
            var path = PathDecoder.DecodePath(BtrieveName).TrimEnd();

            if (!System.IO.Path.IsPathRooted(path) && !path.StartsWith(".."))
            {
                var folder = PathDecoder.DecodePath(SqlSchema);
                var be = this.DataProvider as BtrieveDataProvider;
                if (be != null)
                    folder = PathDecoder.DecodePath(be.FilesPath);
                if (!string.IsNullOrEmpty(folder))
                    path = System.IO.Path.Combine(folder, path);
            }
            var y = path.IndexOf("\\..");
            if (y > 0)
            {
                path = path.Replace("\\\\", "\\");
                y = path.IndexOf("\\..");
                if (y > 0)
                {
                    var before = path.Remove(y);
                    var after = path.Substring(y + 3);
                    var z = before.LastIndexOf("\\");
                    before = before.Remove(z);
                    path = before + after;
                }
            }
            return path;
        }

        public static string GetFullBtrievePathAndName(BtrieveEntity be)
        {
            return be.GetFullBtrievePathAndName();
        }
        public static string GetSQLName(BtrieveEntity be)
        {
            return be.GetTheNameToUseInSql();
        }

        static BtrieveEntity()
        {
            DefaultSQLNameStrategy = SQLNameStrategies.SQLNameWithSchemaBasedOnFolders;
        }
        public BtrieveEntity(string sqlName, string caption, IEntityDataProvider dataProvider, string sqlSchema, string btrieveName) :
            base(dataProvider is BtrieveDataProvider ? btrieveName : sqlName, caption, dataProvider)
        {

            //CheckDuplicateIndex = true;
            AutoCreateTable = true;
            _sqlName = sqlName;
            _sqlSchema = sqlSchema;
            BtrieveName = btrieveName;
            IsUsingBtrieve = dataProvider is BtrieveDataProvider;
        }


        private string _sqlName;
        public string SqlName
        {
            get { return _sqlName; }
            set
            {
                _sqlName = value;
                _sqlNameWasChanged = true;
            }
        }
        bool _sqlNameWasChanged = false;
        private string _sqlSchema;
        public string SqlSchema
        {
            get { return _sqlSchema; }
            set
            {
                _sqlSchema = value;
            }
        }



        private string _btrieveName;
        public string BtrieveName
        {
            get { return _btrieveName; }
            set { _btrieveName = value; }
        }



        public static void AddSchemaMap(string btrievePath, string sqlSchema)
        {
            var key = Path.GetFullPath(PathDecoder.DecodePath(btrievePath)).ToUpper();
            if (!key.EndsWith("\\") && !string.IsNullOrEmpty(key))
                key += "\\";

            if (!_schemaConvertor.ContainsKey(key))
                _schemaConvertor.Add(key, PathDecoder.DecodePath(sqlSchema));
        }

        public static bool ReturnBtrieveName { get; set; }
        public static bool UseOracleRowIdAsIdentityInSQL { get; set; }


        public static Func<ColumnBase> CreateRowIdColumn = () => new RowId();
        protected void UseRowIdForPrimaryKey()
        {
            if (!IsUsingBtrieve)
            {
                IdentityColumn = CreateRowIdColumn();
                SetPrimaryKey(IdentityColumn);
                AddRowIdToNonUniqueIndexes(Indexes);
            }
            else if (UseBtrievePosition)
            {
                IdentityColumn = new BtrievePositionColumn() { Caption = "BtrievePositionValue", Storage = new ENV.Data.Storage.IntNumberStorage() };
                Columns.Add(IdentityColumn);
            }
        }
        public static bool UseBtrievePosition = false;
        protected void AddRowIdToNonUniqueIndexes(params Sort[] nonUniqueIndexes)
        {
            AddRowIdToNonUniqueIndexes((IEnumerable<Sort>)nonUniqueIndexes);
        }
        protected void AddRowIdToNonUniqueIndexes(IEnumerable<Sort> nonUniqueIndexes)
        {
            if (!IsUsingBtrieve)
            {
                if (UseOracleRowIdAsIdentityInSQL)
                    return;


                if (IdentityColumn == null)
                {
                    IdentityColumn = new RowId();

                    Columns.Add(IdentityColumn);
                }
                foreach (var nonUniqueIndex in nonUniqueIndexes)
                {
                    var idx = nonUniqueIndex as Index;
                    if (!nonUniqueIndex.Unique)
                    {
                        var lastSegmentDirection = SortDirection.Ascending;
                        if (nonUniqueIndex.Segments.Count > 0)
                            lastSegmentDirection = nonUniqueIndex.Segments[nonUniqueIndex.Segments.Count - 1].Direction;
                        nonUniqueIndex.Add(IdentityColumn, lastSegmentDirection);
                        if (idx != null)
                            idx.ColumnsAddedToAutomaticallyMakeSortUnique.Add(IdentityColumn);
                        nonUniqueIndex.Unique = true;
                    }
                }
            }
        }

        public BtrieveFileSharing FileSharing { get; set; }
        public BtrieveOpenMode OpenMode { get; set; }

        public string Owner { get; set; }
        public bool Encrypted { get; set; }



        internal long GetFileSize()
        {
            var dp = DataProvider as BtrieveDataProvider;
            if (dp != null)
                return dp.GetFileSize(this);
            return 0;
        }

        string SqlScriptGeneratorHelper.GetEntityName()
        {
            if (IsUsingBtrieve)
                return GetSQLName();
            return EntityName;
        }

        public static void ClearMapping()
        {
            _schemaConvertor.Clear();
        }
        
        public static void SwitchToSQL()
        {
            BtrieveMigration.UseBtrieve = !BtrieveMigration.UseBtrieve;
            Common.ApplicationTitle = UserMethods.Instance.Sys() +
                (BtrieveMigration.UseBtrieve ? "" : " SQL " );
            Common.RunOnRootMDI(
                mdi =>
                {
                    mdi.Text = Common.ApplicationTitle;
                    Common.SetDefaultStatusText(Common.ApplicationTitle);
                });
        }

        internal bool ReloadRowDataBeforeUpdate { get; set; }
    }


    public enum BtrieveFileSharing
    {
        Write,
        Read,
        None
    }

    public enum BtrieveOpenMode
    {
        Normal,
        Fast,
        Damaged,
        Reindex
    }
    public enum SQLNameStrategies
    {
        SQLName,
        BtrieveNameWithoutPath,
        SQLNameWithSchemaBasedOnFolders,
        OriginalName
    }
    public delegate void BtrieveEntitySqlNameHandlers(BtrieveEntitySqlNameArgs e);
    public class BtrieveEntitySqlNameArgs
    {
        public BtrieveEntity Entity { get; internal set; }
        public string ResultSQLTableName { get; set; }
    }
}