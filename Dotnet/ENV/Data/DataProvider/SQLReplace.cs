using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;
using ENV.IO;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data;



namespace ENV.Data.DataProvider
{
    public class SQLReplace : ENV.Data.Entity
    {
        public NumberColumn Id = new NumberColumn("ID", "8");
        public DateTimeColumn LastRun = new DateTimeColumn("LastRun") { DefaultValue = new DateTime(1901, 1, 1) };
        [PrimaryKey]
        public TextColumn OriginalSQLTemplateKey = new TextColumn("OriginalSQLTemplate", "4000");
        public TextColumn OriginalSQLTemplate = new TextColumn("OriginalSQLTemplateFull");
        public TextColumn TargetSQLTemplate = new TextColumn("TargetSQLTemplate");
        public TextColumn LastError = new TextColumn("LastError");
        public TextColumn Comments = new TextColumn("Comments");
        public TextColumn Fixed1 = new TextColumn("Fixed1", "U");
        public TextColumn Fixed2 = new TextColumn("Fixed2", "U");
        public TextColumn OriginalMergedSQL = new TextColumn("OriginalMergedSQL");
        public TextColumn TargetMergedSQL = new TextColumn("TargetMergedSQL");
        public TextColumn TheParams = new TextColumn("TheParams");
        public TextColumn TheClass = new TextColumn("TheClass");
        public TextColumn LocationInMagic = new TextColumn("LocationInMagic");
        public BoolColumn Refresh = new BoolColumn("Refresh");

        

        public static void Init(Func<DynamicSQLSupportingDataProvider> sqlReplaceConnection,
            Func<DynamicSQLSupportingDataProvider> originalConnection,
            Func<DynamicSQLSupportingDataProvider> targetConnection, bool backupOracle)
        {
            _SQLReplaceConnection = sqlReplaceConnection;
            _targetConnection = targetConnection;
            _originalConnection = originalConnection;
            var e = new SQLReplace();
            if (!e.Exists())
                _SQLReplaceConnection().CreateTable(e);
            if (backupOracle && e.CountRows() > 0)
            {
                var orgName = e.EntityName;
                e.EntityName += "_" + Date.Now.ToString("YYYYMMDD");
                if (!e.Exists())
                {
                    _SQLReplaceConnection().Execute(string.Format("create table {0} as select * from {1}", e.EntityName, orgName));
                }
            }
            Reload();
        }

        static Func<DynamicSQLSupportingDataProvider> _SQLReplaceConnection,
            _targetConnection,
            _originalConnection;
        public SQLReplace()
            : base("SQLReplace", _SQLReplaceConnection())
        {
        }

        public string Simulate(string @select, bool fixSQL)
        {

            var sb = new DynamicSQLEntity.SQLBuilder(@select);
            if (!Text.IsNullOrEmpty(TheParams.Value))
            {
                var xs = new XmlSerializer(typeof(Param[]));
                using (var sr = new StringReader(TheParams.Value))
                {
                    var p = (Param[])xs.Deserialize(sr);
                    foreach (var param in p)
                    {
                        sb.AddValueParameter(param.Value, param.IsNull, () => null);
                    }

                }
            }
            if (fixSQL)
                return SQLReplace.FixSQL(sb.GetResult());
            return sb.GetResult();

        }
        static Dictionary<string, string> _replaceInQuery = new Dictionary<string, string>();
        static List<Func<string, string>> _replaceInQueryRegex = new List<Func<string, string>>();
        public static void ReplaceInSQL(string what, string with)
        {
            _replaceInQuery.Add(what, with);
        }

        public static void ReplaceInSQLRegex(string regex, string replaceWithThis)
        {
            var re = new Regex(regex, RegexOptions.IgnoreCase);
            _replaceInQueryRegex.Add(s => re.Replace(s, replaceWithThis));
        }

        public static void ReplaceInSQLRegex(string regex, System.Text.RegularExpressions.MatchEvaluator m, RegexOptions options)
        {
            var re = new Regex(regex, options);
            _replaceInQueryRegex.Add(s => re.Replace(s, m));
        }


        public static string FixSQL(string commandText)
        {

            if (_replaceInQuery.Count > 0 || _replaceInQueryRegex.Count > 0)
            {
                var s = commandText;
                _replaceInQueryRegex.ForEach(func => s = func(s));
                foreach (var x in _replaceInQuery)
                {
                    int i = -1;
                    while ((i = s.IndexOf(x.Key, StringComparison.InvariantCultureIgnoreCase)) >= 0)
                    {
                        s = s.Remove(i, x.Key.Length).Insert(i, x.Value);
                    }
                }

                return s;
            }
            else return commandText;
        }

        public static void ShowUI()
        {
            new SqlReplaceManager().Run();

            Reload();
        }

        public void ShowMergedSQL()
        {
            var y = this.TargetSQLTemplate.Value;
            if (Text.IsNullOrEmpty(y))
                y = this.OriginalSQLTemplate.Value;

            EntityBrowser.ShowString("Merged", this.Simulate(y, true));
        }

        public void RunSQL()
        {
            string targetSQL = "";
            try
            {
                var x = TargetSQLTemplate.Value;
                if (Text.IsNullOrEmpty(x))
                    x = OriginalSQLTemplate.Value;
                targetSQL = Simulate(x, true);
                var targetSw = new Stopwatch();
                var originalSw = new Stopwatch();
                var targetFile = CreateFile(targetSQL, Id.ToString().Trim() + "_target", _targetConnection(), targetSw);
                var originalFile = CreateFile(Simulate(OriginalSQLTemplate, false), Id.ToString().Trim() + "_original",
                    _originalConnection(), originalSw);

                var org = File.ReadAllText(originalFile);
                var tar = File.ReadAllText(targetFile);
                bool ok = true;
                bool onlySortDifference = false;
                if (org.Length != tar.Length)
                {
                    ok = false;
                }
                else
                {
                    ok = org == tar;
                    if (!ok)
                    {
                        org = Sort(org);
                        tar = Sort(tar);
                        if (org == tar)
                            onlySortDifference = true;
                    }
                }
                var moreInfo = string.Format(@"
Original:{0}
  Target:{1}", originalSw.Elapsed, targetSw.Elapsed);
                if (ok)
                    MessageBox.Show("Seems ok - great" + moreInfo);
                else
                {
                    if (onlySortDifference)
                        MessageBox.Show("There is only a sort difference" + moreInfo);
                    else
                        MessageBox.Show("There is a difference!!" + moreInfo);
                    Windows.OSCommand(GetCompareResultFolder());
                }
                //Windows.OSCommand(string.Format(@"""C:\Program Files (x86)\Beyond Compare 3\bcompare.exe"" {0} {1}",targetFile,originalFile));
            }
            catch (Exception e)
            {
                Common.ShowExceptionDialog(e, true, "sql: {0}", targetSQL);
            }
        }
        static string Sort(string s)
        {
            var l = new List<string>();
            using (var sw = new StringReader(s))
            {
                string line;
                while (((line = sw.ReadLine()) != null))
                {
                    l.Add(line);
                }
            }
            l.Sort();
            using (var sw = new StringWriter())
            {
                sw.WriteLine(l);
                return sw.ToString();
            }
        }

        public static void Reload()
        {
            SQLReplaceBuilder.Preload();
        }

        static string CreateFile(string sql, string name, DynamicSQLSupportingDataProvider connection, Stopwatch watch)
        {
            var fileName = Path.Combine(GetCompareResultFolder(),
                   name + ".txt");
            using (var sw = new StreamWriter(fileName))
            {
                using (var c = connection.CreateCommand())
                {
                    c.CommandText = sql;
                    watch.Start();
                    using (var r = c.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            for (int i = 0; i < r.FieldCount; i++)
                            {
                                object val = null;
                                if (r.IsDBNull(i))
                                    val = "null";
                                else if (r.GetDataTypeName(i) == "Date")
                                    val = r.GetDateTime(i);
                                else
                                {
                                    val = r[i];
                                    if (val is string)
                                    {
                                        val = "\"" + val.ToString().TrimEnd().Replace("\"", "\"\"") + "\"";
                                    }
                                    else if (val is decimal)
                                    {
                                        var s = ((decimal)val).ToString(CultureInfo.InvariantCulture);
                                        if (s.Contains(".") && s.EndsWith("0"))
                                        {
                                            while (s.EndsWith("0"))
                                            {
                                                s = s.Remove(s.Length - 1);
                                            }
                                            if (s.EndsWith("."))
                                                s = s.Remove(s.Length - 1);
                                            val = s;
                                        }
                                    }
                                }
                                if (i > 0)
                                    sw.Write(",");
                                sw.Write(val);
                            }
                            sw.WriteLine();
                        }
                    }
                    watch.Stop();
                }
            }



            return fileName;
        }

        static string GetCompareResultFolder()
        {
            var result = Path.Combine(Path.GetTempPath(), "SQLCompare");
            if (!Directory.Exists(result))
                Directory.CreateDirectory(result);
            return result;

        }


    
internal string GetSQLToRun()
{
 	var x= TargetSQLTemplate.TrimEnd();
    if (string.IsNullOrEmpty(x))
        x = OriginalSQLTemplate.TrimEnd();
    return x;
}}
    class SQLReplaceBuilder : DynamicSQLEntity.ISQLBuilder
    {
        static bool _preLoad = false;
        static Dictionary<string, string> _sqlReplace = new Dictionary<string, string>();
        public static void Preload()
        {
            _preLoad = true;
            lock (_sqlReplace)
            {
                _sqlReplace.Clear();
                var sq = new SQLReplace();
                var bp = new BusinessProcess { From = sq };
                bp.Where.Add(sq.Refresh.IsEqualTo(false));
                bp.Columns.Add(sq.Refresh, sq.OriginalSQLTemplateKey, sq.TargetSQLTemplate,sq.Id,sq.OriginalSQLTemplate);
                bp.ForEachRow(() =>
                {
                    var key = sq.OriginalSQLTemplateKey.TrimEnd().ToString();
                    if (key.Length > keySize)
                        key = key.Remove(keySize);
                    if (!_sqlReplace.ContainsKey(key)) 
                        _sqlReplace.Add(key, sq.GetSQLToRun());
                });
            }
        }
        string _sql;
        string _sqlKey;
        public SQLReplaceBuilder(string sql)
        {
            _sql = sql;
            _sqlKey = _sql;
            if (_sqlKey.Length > keySize)
                _sqlKey = _sqlKey.Remove(keySize);
        }

        List<Action<Action<string, bool, Func<IDbDataParameter>>>> _list =
            new List<Action<Action<string, bool, Func<IDbDataParameter>>>>();

        List<Param> _params = new List<Param>();


        public void AddValueParameter(string s, bool isNull, Func<IDbDataParameter> createParamAndSetItsValue)
        {
            _list.Add(action =>
            {
                action(s, isNull, createParamAndSetItsValue);
            });
            _params.Add(new Param
            {
                Value = s,
                IsNull = isNull
            });
        }

        public void BeginPrepare()
        {

        }
        public static void RunOnAnotherThread(Action what)
        {
            var t = new System.Threading.Thread(() =>
            {
                try
                {
                    Firefly.Box.Context.Current.SetNonUIThread();
                    what();

                }
                catch (Exception ex)
                {
                    ErrorLog.WriteToLogFile(ex);
                    Common.ShowExceptionDialog(ex);
                }
                finally
                {
                    Firefly.Box.Context.Current.Dispose();
                }
            });
            t.Start();
            t.Join();
        }

        internal const int keySize = 3990;
        string lastSql = null, lastOriginal = null, lastParameters = null;
        public string GetResult()
        {
            lastSql = _sql;
            bool interactWithDb = !_preLoad;
            if (_preLoad)
            {
                string targetSql = "";
                if (_sqlReplace.TryGetValue(_sqlKey, out targetSql))
                {
                    if (!string.IsNullOrEmpty(targetSql))
                        lastSql = targetSql;
                }
                else
                {
                    interactWithDb = true;
                }
            }
            if (interactWithDb)
            {
                var callstack = ENV.ErrorLog.GetCurrentLocation();
                RunOnAnotherThread(
                    () =>
                    {
                        var s = new SQLReplace();
                        var bp = new BusinessProcess { };
                        bp.Relations.Add(s, RelationType.InsertIfNotFound,
                            s.OriginalSQLTemplateKey.BindEqualTo(_sqlKey));
                        bp.ForFirstRow(() =>
                                       {
                                           s.OriginalSQLTemplate.Value = _sql;
                                           if (s.Id.Value == 0)
                                               s.Id.Value = s.Max(s.Id) + 1;


                                           lastSql = s.GetSQLToRun();
                                           BuildTheSqls();
                                           lock (_sqlReplace)
                                           {
                                               if (!_sqlReplace.ContainsKey(_sqlKey))
                                                   _sqlReplace.Add(_sqlKey, s.TargetSQLTemplate.TrimEnd());
                                           }
                                           s.TheParams.Value = lastParameters;
                                           s.TargetMergedSQL.Value = lastSql;
                                           s.OriginalMergedSQL.Value = lastOriginal;
                                           s.LastRun.Value = DateTime.Now;
                                           s.TheClass.Value = callstack;
                                           s.LastError.Value = "";
                                           s.Refresh.Value = false;
                                       });
                    });
            }
            else
            {
                BuildTheSqls();
            }
            return lastSql;
        }

        class DummyDbParameter : IDbDataParameter
        {
            public byte Precision
            {
                get;
                set;
            }
            public byte Scale
            {
                get;
                set;
            }
            public int Size
            {
                get;
                set;
            }
            public DbType DbType
            {
                get;
                set;
            }
            public ParameterDirection Direction
            {
                get;
                set;
            }
            public bool IsNullable
            {
                get;
                set;
            }
            public string ParameterName
            {
                get;
                set;
            }
            public string SourceColumn
            {
                get;
                set;
            }
            public DataRowVersion SourceVersion
            {
                get;
                set;
            }
            public object Value
            {
                get;
                set;
            }
        }
        void BuildTheSqls()
        {
            var sb = new DynamicSQLEntity.SQLBuilder(lastSql);
            var sbOriginal = new DynamicSQLEntity.SQLBuilder(_sql);
            foreach (var action in _list)
            {
                action(sb.AddValueParameter);
                action((s1, b, arg3) => { sbOriginal.AddValueParameter(s1, b, () => { return new DummyDbParameter(); }); });
            }
            lastSql = SQLReplace.FixSQL(sb.GetResult());
            lastOriginal = sbOriginal.GetResult();
            var xs = new XmlSerializer(typeof(Param[]));
            using (var sw = new StringWriter())
            {
                xs.Serialize(sw, _params.ToArray());
                lastParameters = sw.ToString();
            }
        }

        public void Restore()
        {
            _list.Clear();
            _params.Clear();
        }

        public string ToOriginalString()
        {
            return _sql;
        }

        public void ExceptionHappend(Exception exx)
        {
            var ss = exx.Message + "\r\n" + Common.GetShortStackTrace();
            var callstack = ENV.ErrorLog.GetCurrentLocation();
            RunOnAnotherThread(
                () =>
                {
                    var s = new SQLReplace();
                    var bp = new BusinessProcess { };
                    bp.Relations.Add(s, RelationType.InsertIfNotFound,
                        s.OriginalSQLTemplateKey.BindEqualTo(_sqlKey));
                    bp.ForFirstRow(() =>
                    {
                        s.OriginalSQLTemplate.Value = _sql;
                        s.LastError.Value = ss;
                        s.TheParams.Value = lastParameters;
                        s.TargetMergedSQL.Value = lastSql;
                        s.OriginalMergedSQL.Value = lastOriginal;
                        s.TheClass.Value = callstack;
                        s.LastRun.Value = DateTime.Now;

                    });

                });
        }
    }
    public class Param
    {
        public string Value { get; set; }
        public bool IsNull { get; set; }
    }
}