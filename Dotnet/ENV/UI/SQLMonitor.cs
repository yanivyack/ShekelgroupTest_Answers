using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using ENV.Data;
using ENV.Data.DataProvider;
using Firefly.Box;

namespace ENV.UI
{
    public partial class SQLMonitor : System.Windows.Forms. Form
    {
        DataSet _logDataSet;

        public SQLMonitor(DataSet logDataSet)
        {
            _logDataSet = logDataSet;
            InitializeComponent();
            var logDataBindingSource = new BindingSource
            {

                DataSource = _logDataSet.Tables[0].DefaultView,
                RaiseListChangedEvents = true
            };
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.DataSource = logDataBindingSource;

            dataGridView1.Columns.Add(
                new DataGridViewColumn
                {
                    CellTemplate = new DataGridViewTextBoxCell(),
                    Width = 70,
                    Name = "Sequence",
                    DataPropertyName = "Sequence"
                });
            dataGridView1.Columns.Add(
                new DataGridViewColumn
                {
                    CellTemplate = new DataGridViewTextBoxCell(),
                    Width = 500,
                    Name = "Description",
                    DataPropertyName = "Description"
                });
            dataGridView1.Columns.Add(
                new DataGridViewColumn
                {
                    CellTemplate = new DataGridViewTextBoxCell(),
                    Width = 70,
                    Name = "Status",
                    DataPropertyName = "Status"
                });
            dataGridView1.Columns.Add(
                new DataGridViewColumn
                {
                    CellTemplate = new DataGridViewTextBoxCell(),
                    Name = "Duration (ms)",
                    DataPropertyName = "Duration (ms)"
                });
            dataGridView1.Columns.Add(
                new DataGridViewColumn
                {
                    CellTemplate = new DataGridViewTextBoxCell(),
                    Name = "Operation",
                    DataPropertyName = "Operation"
                });
            textBox2.DataBindings.Add("Text", logDataBindingSource, "SQL Text", true);
            textBox1.DataBindings.Add("Text", logDataBindingSource, "TranslatedSQL", true);

            DataTableNewRowEventHandler a =
                delegate
                {

                    dataGridView1.Invoke(new Action(() =>
                    {
                        lock (this)
                        {


                            dataGridView1.DataSource = null;
                            dataGridView1.DataSource = logDataBindingSource;
                            foreach (var tb in new[] { textBox1, textBox2 })
                            {
                                var dataBind = tb.DataBindings[0];
                                tb.DataBindings.Clear();
                                tb.DataBindings.Add(dataBind);
                            }
                            /*  if (_autoScrollEnabled)
                                  dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count -
                                                                                  dataGridView1.DisplayedRowCount(
                                                                                      false);*/
                        }
                    }));
                };
            _logDataSet.Tables[0].TableNewRow += a;
            dataGridView1.Disposed += delegate { _logDataSet.Tables[0].TableNewRow -= a; };
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

            string saveFileName = null;
            Action what = () =>
            {
                var sfd = new SaveFileDialog { DefaultExt = "xml" };


                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    saveFileName = sfd.FileName;
                }
            };
            Context.Current.InvokeUICommand(what);
            if (!string.IsNullOrEmpty(saveFileName))
                _logDataSet.WriteXml(saveFileName);
            ;
        }

        bool _autoScrollEnabled = true;


        private void autoScrollButton_Click(object sender, EventArgs e)
        {

            _autoScrollEnabled = !_autoScrollEnabled;
            /*if (_autoScrollEnabled)
            {
                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count -
                                                                        dataGridView1.DisplayedRowCount(false);
            }*/
        }
        static ENV.UI.SQLMonitor _viewer = null;
        public static void Run()
        {
            if (_viewer != null)
            {
                _viewer.BeginInvoke(new Action(() => _viewer.Activate()));
                return;
            }
            DatasetLogWriter.Init();
            //Create the datatable in the dataset

            var t = new System.Threading.Thread(() =>
            {
                try
                {
                    {
                        var original = SQLDataProviderHelper._wrapConnection;
                        SQLDataProviderHelper._wrapConnection =
                            c => new LogDatabaseWrapper(c, new DatasetLogWriter());

                        try
                        {

                            _viewer = new ENV.UI.SQLMonitor(_logDatabase.DataSet);
                            _viewer.ShowDialog();
                        }
                        finally
                        {
                            _viewer = null;
                            SQLDataProviderHelper._wrapConnection = original;
                        }
                    }

                }
                finally
                {
                    Firefly.Box.Context.Current.Dispose();
                }
            });
            t.Start();

        }
        static MemoryDatabase _logDatabase = new MemoryDatabase();
        class DatasetLogWriter : ILogWriter
        {
            public static void Init()
            {
                var e = new DbLogData();
                if (!e.Exists())
                {
                    e.Insert("");
                    e.Truncate();
                }
            }

            class DbLogData : Entity
            {
                static int counter = 0;
                [PrimaryKey]
                NumberColumn Sequence = new NumberColumn("Sequence", "6");

                TextColumn SQLTitle = new TextColumn("Description", "2000");
                TextColumn Status = new TextColumn("Status", "10");
                NumberColumn Duration = new NumberColumn("Duration (ms)", "50");
                TextColumn Operation = new TextColumn("Operation", "50");
                TextColumn SQLText = new TextColumn("SQL Text", "8000");
                TextColumn TranslatedSQL = new TextColumn("TranslatedSQL", "8000");


                public DbLogData()
                    : base("DbLogData", _logDatabase)
                {
                }
                public void Insert(string operation)
                {
                    Insert(operation, 0, "Success", "", operation, "");
                }

                public void Insert(string operation, double duration, string status, string sqlText, string sqlTitle, string translatedQuery)
                {
                    var bp = new BusinessProcess { From = this, Activity = Activities.Insert };
                    bp.ForFirstRow(() =>
                    {
                        Sequence.Value = counter++;
                        Operation.Value = operation;
                        Duration.Value = duration;
                        Status.Value = status;
                        SQLText.Value = sqlText;
                        SQLTitle.Value = sqlTitle;
                        TranslatedSQL.Value = translatedQuery;
                    });
                }
            }



            static DatasetLogWriter()
            {
                //Create the datatable in the dataset
                var e = new DbLogData();
                e.Insert("");
                e.Truncate();
            }


            public Type1 ExecuteOperation<Type1>(string description, Func<Type1> whatToRun, IDbCommand _command)
            {
                string status = "Success";
                string commandText = _command.CommandText;
                commandText += " \r\n" + LogDatabaseWrapper.ParameterInfo("Query Parameters", _command.Parameters);

                DateTime start = DateTime.Now;
                try
                {
                    return whatToRun();
                }
                catch (Exception e)
                {
                    status = "Failed";
                    using (var sw = new StringWriter())
                    {
                        sw.WriteLine();
                        sw.WriteLine("DATABASE ERROR - " + e.Message);
                        sw.WriteLine(UserMethods.Instance.Prog());
                        sw.WriteLine(Common.GetShortStackTrace());
                        commandText += sw.ToString();
                    }
                    throw;
                }
                finally
                {
                    var duration = (DateTime.Now - start).TotalMilliseconds;
                    new DbLogData().Insert(description, duration, status, commandText, GetTitle(_command), GetPlainSql(_command));
                }
            }

            string GetTitle(IDbCommand command)
            {
                string sqlTitle = "";

                switch (command.CommandText)
                {
                    case "sp_cursorclose":
                        sqlTitle = "Close Cursor";
                        break;
                    case "sp_cursorfetch":
                        sqlTitle = "Fetch From Cursor";
                        break;
                    case "sp_cursorprepexec":
                        sqlTitle = "Cursor: ";
                        break;
                }
                sqlTitle += MakeCommandTextShorterForTitle(GetPlainSql(command));
                return sqlTitle;
            }
            string GetPlainSql(IDbCommand command)
            {
                string query = command.CommandText;
                if (command.Parameters.Contains("@stmt"))
                {
                    query = ((SqlParameter)command.Parameters["@stmt"]).Value.ToString();
                }
                return GetSqlWithInlineParameters(command, query).Trim();
            }
            string GetSqlWithInlineParameters(IDbCommand command, string sql)
            {
                foreach (SqlParameter p in command.Parameters)
                {
                    if (p.Value != null)
                        sql = sql.Replace(p.ParameterName, ParameterValueForSQL(p));
                }
                return sql;
            }

            string ParameterValueForSQL(IDbDataParameter sp)
            {
                String retval = "";

                switch (sp.DbType)
                {
                    case DbType.AnsiString:
                    case DbType.String:
                    case DbType.AnsiStringFixedLength:
                    case DbType.StringFixedLength:
                    case DbType.Time:
                    case DbType.Xml:
                    case DbType.Date:
                    case DbType.DateTime:
                    case DbType.DateTime2:
                    case DbType.DateTimeOffset:
                        retval = "'" + sp.Value.ToString().Replace("'", "''") + "'";
                        break;

                    case  DbType.Byte:
                        retval = ((bool)sp.Value) ? "1" : "0";
                        break;

                    default:
                        retval = sp.Value.ToString().Replace("'", "''");
                        break;
                }

                return retval;
            }

            string MakeCommandTextShorterForTitle(string selectStatement)
            {
                int indexOfFrom = selectStatement.IndexOf("From");
                if (indexOfFrom > 0)
                {
                    selectStatement = "Select ... " + selectStatement.Substring(indexOfFrom);
                }
                int indexOfQuery = selectStatement.IndexOf("Query Parameters");
                if (indexOfQuery > 0)
                {
                    selectStatement = selectStatement.Remove(indexOfQuery);
                }
                return selectStatement;
            }
        }
    }

}
