using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using ENV.Data;
using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;
using System.Net;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace ENV.Utilities
{
    public partial class SQLQuery : ENV.UI.Form
    {
        readonly DynamicSQLSupportingDataProvider _dataProvider;
        readonly string _recentFileName;

        public static void Run(Type dataSourceClassType)
        {
            var e = new Entity("Available Connections", new DataSetDataProvider());
            var dataSourceName = new TextColumn("Name", "50");
            e.Columns.Add(dataSourceName);
            e.SetPrimaryKey(dataSourceName);

            var dataProviders = new Dictionary<string, DynamicSQLSupportingDataProvider>();

            foreach (var propertyInfo in dataSourceClassType.GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
            {try
                {
                    var x = propertyInfo.GetValue(null, new object[0]) as DynamicSQLSupportingDataProvider;
                    if (x != null)
                    {
                        var name = propertyInfo.Name;
                        new BusinessProcess { From = e, Activity = Activities.Insert }.
                            ForFirstRow(() => dataSourceName.Value = name);

                        dataProviders.Add(name, x);
                    }
                }
                catch { }
            }

            if (dataProviders.Count == 1)
            {
                Common.RunOnNewThread(() => new SQLQuery(dataProviders.First().Value,dataProviders.First().Key).ShowDialog(), false);
            }
            else
            {
                var eb = new EntityBrowser(e);
                eb.AddAction("View Tables",
                    () => Common.RunOnNewThread(() => new SQLQuery(dataProviders[dataSourceName.Trim()],dataSourceName.Trim()).ShowDialog(),
                        false), true);
                eb.Run();
            }
        }

        SQLQuery(DynamicSQLSupportingDataProvider dp,string name)
        {
            _dataProvider = dp;
            InitializeComponent();
            _recentFileName = System.IO.Path.Combine(Application.UserAppDataPath, PathDecoder.FixFileName(name) + "_sql");
            btnTopMost.CheckOnClick = true;
            btnTopMost.CheckedChanged += (sender, args) => TopMost = btnTopMost.Checked;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DoOnQ(false, q => {
                var bp = new BusinessProcess { From = q };
                bp.OrderBy.Add(q.LastRun, SortDirection.Descending);
                bp.ForFirstRow(() => { txtTheSql.Text = q.SQL; });

            });
        }

        void DoOnQ(bool save, Action<SQLHistory> x)
        {
            try
            {
                var dp = new MemoryDatabase();
                try
                {
                    dp.DataSet.ReadXml(_recentFileName);
                }
                catch { }
                x(new SQLHistory(dp));
                if (save)
                    dp.DataSet.WriteXml(_recentFileName, XmlWriteMode.WriteSchema);
            }
            catch { }
        }

        void txtTheSql_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:
                    ExecuteQuery();
                    break;
                case Keys.F6:
                    SelectOldQuery();
                    break;
                case Keys.F8:
                    FormatQuery();
                    break;
                case Keys.A:
                    if (e.Control)
                        txtTheSql.SelectAll();
                    break;
            }
        }

        void ExecuteQuery()
        {
            try
            {
                DoOnQ(true, q =>
                     q.InsertIfNotFound(q.SQL.BindEqualTo(txtTheSql.Text), () => { q.LastRun.Value = Date.Now.ToString("YYYY-MM-DD") + " " + Time.Now.ToString("HH:MM:SS"); }));

                var parser = new SqlParser(txtTheSql.Text);
                webBrowser2.DocumentText = _dataProvider.GetHtmlTableBasedOnSQLResultForDebugPurposes(parser.GetSql());
            }
            catch (Exception e)
            {
                webBrowser2.DocumentText = "<h2>" + e.Message + "</h2>";
            }
        }

        void SelectOldQuery()
        {
            var selectedValue = new TextColumn();
            DoOnQ(false, q => new SqlHistoryList(q, selectedValue, TopMost).ShowDialog());
            if(!String.IsNullOrEmpty(selectedValue.ToString()))
                txtTheSql.Text = selectedValue;
        }

        void FormatQuery()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response =
                        client.UploadValues("https://sqlformat.org/", new NameValueCollection()
                        {
                            { "data", txtTheSql.Text },
                            { "datafile", "" },
                            {"highlight","1" },
                            { "keyword_case","upper"},
                            { "identifier_case",""},
                            { "n_indents","2"},
                            { "output_format","sql"},
                            { "ajax", "1" },
                        });

                    string result = System.Text.Encoding.UTF8.GetString(response);
                    var x = @"""plain"": """;
                    result = result.Substring(result.IndexOf(x) + x.Length) + 1;
                    txtTheSql.Text= result.Remove(result.IndexOf(@""", ""output"": ""<div")).Replace(
                        "\\n", "\r\n").Replace("\\\"", "\"");
                }
            }
            catch (Exception ex){
                MessageBox.Show(ex.Message);
            }
        }

        void toolStripButton1_Click(object sender, EventArgs e)
        {
            ExecuteQuery();
        }

        void toolStripButton2_Click(object sender, EventArgs e)
        {
            FormatQuery();
        }

        void toolStripButton3_Click(object sender, EventArgs e)
        {
            SelectOldQuery();
        }

        public class SqlParser
        {
            readonly string _input;

            public SqlParser(string input)
            {
                _input = input;
            }

            public string GetSql()
            {
                if (!_input.StartsWith("Time:"))
                    return _input;

                bool inStmt = false;
                bool inParams = false;
                var stmt = new StringBuilder();
                var parameters = new List<Parameter>();

                using (var reader = new StringReader(_input))
                {
                    string lastLine = "";
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!String.IsNullOrWhiteSpace(line))
                        {
                            inStmt = IsInStmt(inStmt, line, lastLine);
                            if (inStmt)
                                stmt.AppendLine(line.Replace("@stmt(String) = ", ""));

                            inParams = IsInParams(line, lastLine);

                            if (inParams)
                                parameters.Add(ParseParameter(line));

                            lastLine = line;
                        }
                    }
                }

                parameters.Reverse();
                foreach (var p in parameters)
                {
                    stmt.Replace(p.Name, p.SqlEncode());
                }

                return stmt.ToString().Trim();
            }


            Parameter ParseParameter(string line)
            {
                var groups = new Regex(@"(.*)\((.*)\)\s=\s(.*)").Match(line).Groups;

                var p = new Parameter
                {
                    Name = groups[1].Value.Trim(),
                    Type = groups[2].Value.Trim(),
                    Value = groups[3].Value.Trim()
                };

                if (p.Type != "AnsiString")
                    p.Value = new Regex(@"\s\(.*\)").Replace(p.Value, "");

                return p;
            }

            bool IsInStmt(bool inStatement, string line, string lastLine)
            {
                if (line.StartsWith("sp_"))
                    return false;

                if (line.StartsWith("@stmt"))
                    return true;

                if (lastLine.Trim() == "SQL" || lastLine.Trim() == "SQLSingleRow")
                    return true;

                if (line.StartsWith("@scrollopt") || line.Trim() == "Parameters:")
                    return false;

                return inStatement;
            }

            bool IsInParams(string line, string lastLine)
            {
                if (line.StartsWith("@handle"))
                    return false;

                return lastLine.Trim() == "Parameters:" || lastLine.StartsWith("@rowcount");
            }

            class Parameter
            {
                public string Name;
                public string Type;
                public string Value;

                public string SqlEncode()
                {
                    if (Type == "AnsiString")
                        return string.Format(@"'{0}'", Value);

                    return Value;
                }
            }
        }
    }
}