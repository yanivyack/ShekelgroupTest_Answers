using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ENV.Data;
using ENV.Data.DataProvider;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box;

namespace ENV.Utilities
{
    public class GetDefinition
    {
        public abstract class TableBase : ENV.Data.Entity
        {
            protected IEntityDataProvider _dataProvider;
            public TableBase(string name, IEntityDataProvider dataProvider)
                : base(name, dataProvider)
            {
                _dataProvider = dataProvider;
                AutoCreateTable = false;
            }

            public abstract string GetTableName();

            internal abstract void WriteTo(EntityWriter ew);

        }
        public class Tables : TableBase
        {

            [PrimaryKey]
            public readonly TextColumn TableName = new TextColumn("Table_Name", "100", "Name");


            public readonly TextColumn Type = new TextColumn("Table_Type", "50", "Type");
            [PrimaryKey]
            public readonly TextColumn TableCatalog = new TextColumn("Table_Catalog", "100", "Catalog");
            [PrimaryKey]
            public readonly TextColumn TableSchema = new TextColumn("Table_Schema", "100", "Schema");
            public Tables(IEntityDataProvider dataProvider)
                : base("INFORMATION_SCHEMA.TABLES", dataProvider)
            {
                AutoCreateTable = false;
            }

            public override string GetTableName()
            {
                return TableName;
            }
            class PrimaryKeys : ENV.Data.Entity
            {
                [PrimaryKey]
                internal readonly TextColumn TableCatalog = new TextColumn("Table_Catalog", "100", "Catalog");
                [PrimaryKey]
                internal readonly TextColumn TableSchema = new TextColumn("Table_Schema", "100", "Schema");
                [PrimaryKey]
                internal readonly TextColumn TableName = new TextColumn("Table_Name", "100", "Table Name");
                [PrimaryKey]
                internal readonly TextColumn ColumnName = new TextColumn("Column_Name", "100", "Name");
                public PrimaryKeys(IEntityDataProvider dataProvider)
                    : base("INFORMATION_SCHEMA.KEY_COLUMN_USAGE", dataProvider)
                {
                    AutoCreateTable = false;
                }
            }
            public class Indexes : ENV.Data.Entity
            {
                [PrimaryKey]
                public readonly TextColumn TableName = new TextColumn("TABLE_NAME", "100", "Table Name");
                public readonly TextColumn Name = new TextColumn("NAME", "100", "Name");
                [PrimaryKey]
                public readonly NumberColumn TableID = new NumberColumn("TABLE_ID", "18");
                [PrimaryKey]
                public readonly NumberColumn IndexId = new NumberColumn("INDEX_ID", "18");

                public readonly NumberColumn NumberOfColumns = new NumberColumn("NUMBEROFCOLUMNS", "18");
                public readonly BoolColumn IsUnique = new BoolColumn("IS_UNIQUE", "18");
                public Indexes(IEntityDataProvider dataProvider)
                    : base("(SELECT T.NAME AS TABLE_NAME,I.NAME,I.INDEX_ID,T.OBJECT_ID AS TABLE_ID,COUNT(*) NUMBEROFCOLUMNS,I.IS_UNIQUE AS IS_UNIQUE  FROM sys.tables T,sys.indexes I,sys.index_columns IC WHERE  T.OBJECT_ID=I.OBJECT_ID AND IC.OBJECT_ID=I.OBJECT_ID AND IC.INDEX_ID=I.INDEX_ID GROUP BY T.NAME,I.NAME,I.INDEX_ID,T.OBJECT_ID,I.IS_UNIQUE)  INDEXES", dataProvider)
                {
                    AutoCreateTable = false;
                }
            }
            public class IndexColumns : ENV.Data.Entity
            {
                [PrimaryKey]
                public readonly NumberColumn TableID = new NumberColumn("OBJECT_ID", "18");
                [PrimaryKey]
                public readonly NumberColumn IndexId = new NumberColumn("INDEX_ID", "18");
                [PrimaryKey]
                public readonly TextColumn ColumnName = new TextColumn("NAME", "100", "Name");

                public readonly BoolColumn IsDescending = new BoolColumn("IS_DESCENDING_KEY", "18");
                public IndexColumns(IEntityDataProvider dataProvider)
                    : base("( SELECT C.NAME,I.INDEX_ID,I.OBJECT_ID,I.IS_DESCENDING_KEY FROM sys.index_columns I,sys.columns C WHERE I.OBJECT_ID=C.OBJECT_ID AND C.COLUMN_ID=I.COLUMN_ID ) as columnIndexes", dataProvider)
                {
                    AutoCreateTable = false;
                }
            }

            public class Columns : ENV.Data.Entity
            {
                [PrimaryKey]
                internal readonly TextColumn TableCatalog = new TextColumn("Table_Catalog", "100", "Catalog");
                [PrimaryKey]
                internal readonly TextColumn TableSchema = new TextColumn("Table_Schema", "100", "Schema");
                [PrimaryKey]
                internal readonly TextColumn TableName = new TextColumn("Table_Name", "100", "Table Name");
                [PrimaryKey]
                internal readonly NumberColumn OrdinalPosition = new NumberColumn("Ordinal_Position", "3", "Order");
                internal readonly TextColumn ColumnName = new TextColumn("Column_Name", "100", "Name");
                internal readonly TextColumn DefaultValue = new TextColumn("Column_Default", "100");
                internal readonly TextColumn DataType = new TextColumn("Data_Type", "100");
                internal readonly NumberColumn MaxLength = new NumberColumn("Character_maximum_length", "4");
                internal readonly TextColumn AllowNull = new TextColumn("Is_Nullable", "2");
                internal readonly NumberColumn NumericPresision = new NumberColumn("Numeric_precision", "4");
                internal readonly NumberColumn NumericScale = new NumberColumn("Numeric_Scale", "4");



                public Columns(IEntityDataProvider dataProvider)
                    : base("INFORMATION_SCHEMA.COLUMNS", "Columns", dataProvider)
                {
                    AutoCreateTable = false;
                }


                internal void WriteTo(EntityWriter ew)
                {
                    ew.WriteColumn(ColumnName, DataType, NumericPresision, NumericScale, MaxLength, DefaultValue,
                                   AllowNull != "NO", null);
                }
            }
            internal override void WriteTo(EntityWriter ew)
            {
                var foundPrimaryKey = false;
                {
                    var pk = new PrimaryKeys(_dataProvider);
                    var bp = new BusinessProcess { From = pk };
                    bp.Where.Add(
                        pk.TableCatalog.IsEqualTo(TableCatalog).And(
                            pk.TableSchema.IsEqualTo(TableSchema).And(pk.TableName.IsEqualTo(TableName))));
                    bp.ForEachRow(() => ew.AddPrimaryKey(pk.ColumnName));
                    foundPrimaryKey = bp.Counter > 0;
                }
                {
                    var ind = new Indexes(_dataProvider);
                    var bp = new BusinessProcess { From = ind };
                    bp.OrderBy.Add(ind.NumberOfColumns);
                    bp.Where.Add(ind.TableName.IsEqualTo(TableName));
                    bp.ForFirstRow(() =>
                    {
                        var cols = new IndexColumns(_dataProvider);
                        var bp2 = new BusinessProcess { From = cols };
                        bp2.Where.Add(
                            cols.IndexId.IsEqualTo(ind.IndexId).And(
                                cols.TableID.IsEqualTo(ind.TableID)));
                        var idxWriter = ew.AddIndex(ind.Name, ind.IsUnique);
                        bp2.ForEachRow(() =>
                        {
                            if (!foundPrimaryKey && ind.IsUnique)
                                ew.AddPrimaryKey(cols.ColumnName);
                            idxWriter.Add(cols.ColumnName, cols.IsDescending);

                        });
                        if (ind.IsUnique)
                            foundPrimaryKey = true;
                    });
                }
                {
                    var cols = new Columns(_dataProvider);
                    var bp = new BusinessProcess { From = cols };
                    bp.Where.Add(
                        cols.TableCatalog.IsEqualTo(TableCatalog).And(
                            cols.TableSchema.IsEqualTo(TableSchema).And(cols.TableName.IsEqualTo(TableName))));
                    bp.ForEachRow(() =>
                    {
                        cols.WriteTo(ew);
                    });
                }
            }
        }
        public static string FixName(string name)
        {
            if (name.ToUpper() != name)
                return name;
            var x = name.Split('_');
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i].Length > 1)
                {
                    x[i] = x[i][0] + x[i].Substring(1).ToLower();
                }
            }
            return String.Join("", x);

        }
        internal class OracleTables : TableBase
        {

            [PrimaryKey]
            internal readonly TextColumn TableName = new TextColumn("tname", "100", "Name");


            internal readonly TextColumn Type = new TextColumn("tabtype", "50", "Type");
            public OracleTables(IEntityDataProvider dataProvider)
                : base("tab", dataProvider)
            {
                AutoCreateTable = false;
            }

            public override string GetTableName()
            {
                return TableName;
            }

            internal override void WriteTo(EntityWriter ew)
            {
                var cols = new Columns(_dataProvider);
                var bp = new BusinessProcess { From = cols };
                bp.OrderBy.Add(cols.OrdinalPosition);
                bp.Where.Add(cols.TableName.IsEqualTo(TableName));
                bp.ForEachRow(() => { cols.WriteTo(ew); });
            }
            public class Columns : ENV.Data.Entity
            {

                internal readonly TextColumn TableName = new TextColumn("Table_Name", "100", "Table Name");
                [PrimaryKey]
                internal readonly NumberColumn OrdinalPosition = new NumberColumn("Column_id", "3", "Order");
                internal readonly TextColumn ColumnName = new TextColumn("Column_Name", "100", "Name");
                internal readonly TextColumn DefaultValue = new TextColumn("data_default", "100");
                internal readonly TextColumn DataType = new TextColumn("Data_Type", "100");
                internal readonly NumberColumn MaxLength = new NumberColumn("data_length", "4");
                internal readonly TextColumn AllowNull = new TextColumn("Nullable", "1");
                internal readonly NumberColumn NumericPresision = new NumberColumn("data_precision", "4");
                internal readonly NumberColumn NumericScale = new NumberColumn("data_Scale", "4");
                public Columns(IEntityDataProvider dataProvider)
                    : base("user_tab_columns", "Columns", dataProvider)
                {
                    AutoCreateTable = false;
                }

                public void WriteTo(EntityWriter ew)
                {
                    ew.WriteColumn(ColumnName, DataType, NumericPresision, NumericScale, MaxLength, DataType == "CHAR" && MaxLength == 8 ? "('00000000')" : DefaultValue,
                                   AllowNull != "N", null);
                }
            }
            public class Indexes : ENV.Data.Entity
            {
                [PrimaryKey]
                public readonly TextColumn TableName = new TextColumn("TABLE_NAME", "100", "Table Name");
                [PrimaryKey]
                public readonly TextColumn Name = new TextColumn("index_name", "100", "Name");
                
                public Indexes(IEntityDataProvider dataProvider)
                    : base("user_indexes", dataProvider)
                {
                    AutoCreateTable = false;
                }
            }
        }

        internal class EntityWriter : IDisposable
        {
            IndentedTextWriter _writer;
            string _tableName;
            string _datasourceName;
            Type _datasourceClassType;
            public EntityWriter(IndentedTextWriter writer, string tableName, string datasourceName, Type datasourceClassType)
            {
                _datasourceClassType = datasourceClassType;
                _datasourceName = datasourceName;
                _writer = writer;
                _tableName = tableName;
                writer.WriteLine("/* auto generated entity code, {0}*/", DateTime.Now);
                writer.WriteLine("using {0};", typeof(ENV.Data.Entity).Namespace);
                writer.WriteLine();
                writer.WriteLine("namespace {0}.Models", System.Reflection.Assembly.GetEntryAssembly().GetName().Name);
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine("public class {0} : {1}", FixName( tableName.Trim()), typeof(ENV.Data.Entity).Name);
                writer.WriteLine("{");
                writer.Indent++;
            }

            class ColumnWriter
            {
                IndentedTextWriter _writer;
                Text columnName, columnCaption;
                string dataType;
                Number numericPresision;
                Number NumericScale;
                Number MaxLength;
                string DefaultValue;
                bool allowNull;
                EntityWriter _parent;

                public ColumnWriter(EntityWriter parent, IndentedTextWriter writer, string columnName, string dataType, Number numericPresision, Number numericScale, Number maxLength, string defaultValue, bool allowNull, string columnCaption)
                {
                    _parent = parent;
                    _writer = writer;
                    this.columnName = columnName.Trim();
                    this.columnCaption = columnCaption ?? this.columnName;
                    this.dataType = dataType;
                    this.numericPresision = numericPresision;
                    NumericScale = numericScale;
                    MaxLength = maxLength;
                    DefaultValue = defaultValue;
                    if (!string.IsNullOrEmpty(DefaultValue))
                        DefaultValue = DefaultValue.Trim();
                    this.allowNull = allowNull;
                }

                void WriteColumn(Type type)
                {
                    WriteColumn(type, null);
                }

                void WriteColumn(Type type, string format)
                {
                    WriteColumn(type, format, delegate { });
                }
                public void WriteColumn()
                {
                    switch (dataType.Trim().ToLower())
                    {
                        case "decimal":
                        case "real":
                        case "int":
                        case "integer":
                        case "smallint":
                        case "tinyint":
                        case "bigint":
                        case "float":
                        case "numeric":
                        case "number":
                        case "money":
                            var p = numericPresision.ToDecimal();
                            p = Math.Min(p, 20);
                            string format = null;
                            if (p > 0)
                                format = NumericScale > 0 ? (p - NumericScale).ToString().Trim() + "." + NumericScale.ToString().Trim() : p.ToString(CultureInfo.InvariantCulture).Trim();
                            WriteColumn(typeof(NumberColumn), format);
                            break;
                        case "nchar":
                        case "nvarchar":
                        case "ntext":
                        case "nvarchar2":
                            WriteColumn(typeof(TextColumn), MaxLength.ToString().Trim(),
                                        x =>
                                        x("StorageType",
                                          typeof(TextStorageType).Name + "." + TextStorageType.Unicode.ToString()));
                            break;
                        case "text":
                        case "varchar":
                        case "varchar2":
                            WriteColumn(typeof(TextColumn), MaxLength.ToString().Trim());
                            break;
                        case "char":
                            if (MaxLength == 8 && DefaultValue == "('00000000')")
                                WriteColumn(typeof(DateColumn));
                            else
                                WriteColumn(typeof(TextColumn), MaxLength.ToString().Trim());
                            break;
                        case "date":
                            WriteColumn(typeof(DateColumn), null,
                                        x =>
                                        x("Storage", "new " + typeof(ENV.Data.Storage.DateDateStorage).FullName + "()"));
                            break;
                        case "datetime":
                            WriteColumn(typeof(DateColumn), null,
                                        x =>
                                        x("Storage", "new " + typeof(ENV.Data.Storage.DateTimeDateStorage).FullName + "()"));
                            break;
                        case "time":
                            WriteColumn(typeof(TimeColumn),null, x =>
                                       x("Storage", "new " + typeof(ENV.Data.Storage.TimeSpanTimeStorage).FullName + "()"));
                            break;
                        case "bit":
                            WriteColumn(typeof(BoolColumn));
                            break;
                        case "image":
                        case "binary":
                        case "blob":
                        case "varbinary":
                            WriteColumn(typeof(ByteArrayColumn));
                            break;
                        case "rowid":
                            break;
                        default:

                            _writer.WriteLine(
                                "public readonly {0} {1} = new {0}(\"{8}\",\"100\"); /* unknown datatype:{2}, default value:{3} max_length:{4} precision:{5} scale:{6} allowNull:{7}*/",
                                typeof(TextColumn).Name, FixName(columnName.Trim()), dataType.Trim(), DefaultValue.Trim(), MaxLength,
                                numericPresision, NumericScale, allowNull, columnName);
                            break;

                    }
                }

                void WriteColumn(Type type, string format, Action<Action<string, string>> setProperties)
                {
                    if (_parent._pkColumns.Contains(columnName))
                        _writer.WriteLine("[PrimaryKey]");
                    _writer.Write("public readonly {0} {1} = new {0}(\"{4}\"{2}{3})",
                        type.Name, FixName( columnName), format != null ? ", \"" + format + "\"" : "", (!Text.IsNullOrEmpty(columnCaption) && columnCaption != columnName) ? (string)(", \"" + columnCaption.TrimEnd() + "\"") : "",columnName);
                    bool hadProperties = false;
                    Action<string, string> propertySetter = (name, value) =>
                    {
                        if (!hadProperties)
                        {
                            
                            _writer.Write(" { ");
                            
                            hadProperties = true;
                        }
                        else
                            _writer.Write(", ");
                        _writer.Write("{0} = {1}", name, value);
                    };
                    if (allowNull && type != typeof(ByteArrayColumn))
                        propertySetter("AllowNull", "true");
                    setProperties(propertySetter);
                    if (hadProperties)
                    {
                        _writer.WriteLine(" };");
                    }
                    else
                        _writer.WriteLine(";");
                }
            }
            public void WriteColumn(string columnName, string dataType, Number numericPresision, Number numericScale, Number maxLength, string defaultValue, bool allowNull, string columnCaption)
            {
                new ColumnWriter(this, _writer, columnName, dataType, numericPresision, numericScale, maxLength, defaultValue,
                                 allowNull, columnCaption).WriteColumn();
            }

            public void Dispose()
            {
                if (_indexes.Count > 0)
                {
                    _writer.WriteLine();
                    _writer.Write("#region Indexes");
                    foreach (var idx in _indexes)
                    {
                        _writer.WriteLine();
                        _writer.Write("public readonly Index SortBy{0} = new Index",FixName( idx.Name));
                        _writer.Write(" {");
                        
                        _writer.Write(" Name = \"{0}\"{1}", idx.Name, idx.Unique ? ", " : "");
                        if (idx.Unique)
                            _writer.Write("Unique = true");
                        _writer.WriteLine(" };");
                    }
                    _writer.Write("#endregion");
                }
                _writer.WriteLine();
                _writer.WriteLine();
                _writer.WriteLine("public {0}() : base(\"{3}\", {1}.{2})",FixName( _tableName), _datasourceClassType.FullName,
                                  _datasourceName,_tableName);
                _writer.WriteLine("{");
                _writer.Indent++;
                foreach (var index in _indexes)
                {
                    index.Write(_writer);
                }
                _writer.Indent--;
                _writer.WriteLine("}");
                _writer.Indent--;
                _writer.WriteLine("}");
                _writer.Indent--;
                _writer.WriteLine("}");
            }

            HashSet<string> _pkColumns = new HashSet<string>();
            public void AddPrimaryKey(string columnName)
            {
                _pkColumns.Add(columnName.Trim());
            }

            internal class Idx
            {
                public string Name;
                public bool Unique;
                public List<IdxCol> Cols = new List<IdxCol>();
                bool _hasDesc = false;
                public void Add(string name, bool descending)
                {
                    if (descending)
                        _hasDesc = true;
                    Cols.Add(new IdxCol { Name = name.TrimEnd(), Descending = descending });
                }

                internal class IdxCol
                {
                    public string Name;
                    public bool Descending;
                }

                public void Write(IndentedTextWriter writer)
                {
                    if (!_hasDesc)
                    {
                        var sb = new StringBuilder();
                        foreach (var idxCol in Cols)
                        {
                            if (sb.Length > 0)
                                sb.Append(", ");
                            sb.Append(FixName( idxCol.Name));
                        }
                        writer.WriteLine("SortBy{0}.Add({1});", FixName(Name), sb);
                    }
                    else
                    {
                        foreach (var idxCol in Cols)
                        {
                            writer.WriteLine("SortBy{0}.Add({1}{2});", FixName(Name),FixName( idxCol.Name), idxCol.Descending ? ", SortDirection.Descending" : "");
                        }
                    }
                }
            }

            List<Idx> _indexes = new List<Idx>();
            public Idx AddIndex(string name, bool isUnique)
            {
                Idx result = new Idx { Name = name.TrimEnd(), Unique = isUnique };
                _indexes.Add(result);
                return result;
            }
        }


        string _datasourceName;
        Type _datasourceClassType;
        IEntityDataProvider _dp;
        ISupportsGetDefinition _gd;
        internal GetDefinition(string datasourceName, Type datasourceClassType, IEntityDataProvider dp, ISupportsGetDefinition gd)
        {
            _dp = dp;
            _gd = gd;
            _datasourceClassType = datasourceClassType;
            _datasourceName = datasourceName;


        }
        string GenerateCsharpCode(TableBase table)
        {
            using (var ms = new StringWriter())
            {
                using (var writer = new IndentedTextWriter(ms))
                {
                    using (var ew = new EntityWriter(writer, table.GetTableName().Trim(), _datasourceName, _datasourceClassType))
                    {
                        table.WriteTo(ew);
                    }


                }
                return ms.ToString();

            }
        }

        public void Show()
        {
            _gd.SendTables(_dp, (tables, where) =>
            {
                var eb = new EntityBrowser(tables);
                if (where != null)
                    eb.Where.Add(where);
                eb.AddAction("Generate C# Code", () =>
                {
                    try
                    {
                        EntityBrowser.ShowString("Entity Code", GenerateCsharpCode(tables));
                    }
                    catch (Exception e)
                    {
                        Common.ShowExceptionDialog(e, true, "");
                    }
                }, true);
                eb.Run();
            });


        }


        public static void Run(Type dataSourceClassType)
        {
            var e = new Entity("Available Connections", new DataSetDataProvider());
            var dataSourceName = new TextColumn("Name", "50");
            e.Columns.Add(dataSourceName);
            e.SetPrimaryKey(dataSourceName);

            var definitionGetters = new Dictionary<string, GetDefinition>();

            foreach (var propertyInfo in dataSourceClassType.GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
            {
                var x = propertyInfo.GetValue(null, new object[0]) as IEntityDataProvider;
                if (x != null)
                {
                    var name = propertyInfo.Name;
                    Action<GetDefinition> add = g =>
                    {
                        new BusinessProcess { From = e, Activity = Activities.Insert }.
                            ForFirstRow(() =>
                            {
                                dataSourceName.Value = name;
                            });
                        definitionGetters.Add(name, g);
                    };
                    try
                    {
                        var z = x as ISupportsGetDefinition;
                        if (z != null && z.Available)
                            add(new GetDefinition(name, dataSourceClassType, x, z));



                    }
                    catch
                    {
                    }

                }
            }
            var eb = new EntityBrowser(e);
            eb.AddAction("View Tables", () =>
            {
                definitionGetters[dataSourceName.Trim()].Show();
            }, true);
            eb.Run();
        }


    }

    public interface ISupportsGetDefinition
    {
        bool Available { get; }
        void SendTables(IEntityDataProvider dp, Action<GetDefinition.TableBase, FilterBase> to);

    }
}
