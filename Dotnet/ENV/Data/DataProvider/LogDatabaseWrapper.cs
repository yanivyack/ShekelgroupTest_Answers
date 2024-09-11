using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data.DataProvider
{
    class LogDatabaseWrapper : System.Data.IDbConnection, IConnectionWrapper
    {
        System.Data.IDbConnection _connection;

        public DataTable GetSchema(string schemaName)
        {
            foreach (var methodInfo in _connection.GetType().GetMethods())
            {
                if (methodInfo.Name == "GetSchema" && methodInfo.GetParameters().Length == 1)
                {
                    var r = methodInfo.Invoke(_connection, new object[] { "DataSourceInformation" }) as DataTable;
                    return r;
                }
            }
            return null;
        }

        internal static Action<LogDatabaseWrapper> ConnectionOpened;
        internal static Action<LogDatabaseWrapper> ConnectionClosed;
        internal class TextLogWriter : ILogWriter
        {
            Action<string> _writeToLog;
            void WriteToLog(string what)
            {
                _writeToLog(what);
            }

            public TextLogWriter(Action<string> writeToLog)
            {
                _writeToLog = writeToLog;
            }

            public DataType ExecuteOperation<DataType>(string description, Func<DataType> whatToRun, IDbCommand command)
            {
                var pars = new List<object>();
                if (command != null)
                    using (var sw = new StringWriter())
                    {

                        sw.WriteLine(description + " - " + command.CommandText + ParameterInfo("Query Parameters", command.Parameters));
                        WriteToLog(sw.ToString());
                        foreach (System.Data.IDbDataParameter p in command.Parameters)
                        {
                            pars.Add(p.Value);
                        }
                    }
                else
                    WriteToLog(description);
                var start = new System.Diagnostics.Stopwatch();
                start.Start();
                try
                {
                    var result = whatToRun();

                    var outParams = new List<IDataParameter>();
                    int i = 0;
                    if (command != null)
                        foreach (IDataParameter parameter in command.Parameters)
                        {
                            if (parameter.Direction != ParameterDirection.Input)
                            {
                                if (parameter.Value != pars[i])
                                    outParams.Add(parameter);
                                i++;
                            }
                        }
                    if (outParams.Count > 0)
                        WriteToLog(ParameterInfo("Result values", outParams));
                    return result;
                }
                catch (Exception e)
                {
                    var se = e as SqlException;
                    if (se != null && se.Number == 16955)
                        throw;

                    using (var sw = new StringWriter())
                    {
                        sw.WriteLine("DATABASE ERROR - " + e.Message);
                        sw.WriteLine(UserMethods.Instance.Prog());
                        sw.WriteLine(Common.GetShortStackTrace());
                        WriteToLog(sw.ToString());
                    }
                    throw;
                }
                finally
                {
                    WriteToLog(description + DisplayDuration(start));

                }
            }


        }
        internal static string ParameterInfo(string title, IEnumerable parameters)
        {
            var sw = new StringWriter();
            bool first = true;
            foreach (IDbDataParameter parameter in parameters)
            {
                if (first)
                {
                    first = false;
                    sw.WriteLine("\n{0}:", title);
                }
                var x = parameter.Value;
                if (x is DBNull)
                    x = "null";
                if (x is int && (parameter.ParameterName == "@ccopt" || parameter.ParameterName == "@scrollopt"))
                {
                    x = x.ToString() + " /*" + ((int)x).ToString("X") + "*/";
                }
                else if (x is string || x is TimeSpan)
                {
                    x = "'" + x.ToString().Replace("'", "''") + "'";
                }
                else if (x is DateTime)
                {
                    x = "'" + ((DateTime)x).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                }
                else if (x is decimal)
                {
                    x = ((decimal)x).ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (x is byte[])
                {
                    var sb = new StringBuilder();
                    sb.Append("0x");
                    foreach (byte b in (byte[])x)
                    {

                        sb.Append(b.ToString("x").PadLeft(2, '0'));
                    }
                    x = sb.ToString();
                }
                if (x != null)
                    sw.WriteLine(string.Format("declare {0} {2} = {1}", parameter.ParameterName, x, SQLClientEntityDataProvider.TranslateParameterTypeToSQLType(parameter)));
            }
            return sw.ToString();
        }
        internal static string DisplayDuration(System.Diagnostics.Stopwatch start)
        {
            start.Stop();
            var duration = start.Elapsed.TotalMilliseconds;
            var s = " Duration" +
                    (duration < 10
                         ? ""
                         : duration < 20
                               ? "."
                               : duration < 50
                                     ? ".."
                                     : duration < 100
                                           ? "..."
                                           : duration < 200
                                                 ? "...."
                                                 : duration < 500
                                                       ? "....."
                                                       : duration < 1000
                                                             ? "......"
                                                             : ".......")
                    + ": " + ((Number)duration).ToString("9.1C") + " ms";
            return s;
        }
        ILogWriter _logWriter;
        public LogDatabaseWrapper(IDbConnection connection)
            : this(connection, content =>
            {
                lock ("aa")
                    System.Diagnostics.Trace.WriteLine(content);
            })
        {
        }



        public LogDatabaseWrapper(IDbConnection connection, string logFileName)
            : this(connection, content =>
            {
                lock (logFileName)
                    using (var w = new System.IO.StreamWriter(logFileName, true, ENV.LocalizationInfo.Current.OuterEncoding))
                    {
                        w.WriteLine(content);
                    }
            })
        {
        }

        Action<string> _write = delegate { };

        internal LogDatabaseWrapper(IDbConnection connection, ILogWriter writer)
        {
            _connection = connection;
            _logWriter = writer;

        }

        internal LogDatabaseWrapper(IDbConnection connection, Action<string> write)
        {
            _connection = connection;
            _write = x =>
            {
                if (System.Threading.Thread.CurrentThread.ManagedThreadId != this._thread)
                {

                }
                if (!string.IsNullOrEmpty(ErrorLog.AdditionalInfo))
                    x = ErrorLog.AdditionalInfo + " - " + x;
                write(string.Format("({0})-{1}", _connectionId, x));
            };
            _logWriter = new TextLogWriter(_write);
        }

        class myTransaction : IDbTransaction
        {
            LogDatabaseWrapper _parent;
            internal IDbTransaction _source;

            public myTransaction(LogDatabaseWrapper parent, IDbTransaction source)
            {
                _parent = parent;
                _source = source;
            }

            public void Dispose()
            {
                _source.Dispose();
            }

            public void Commit()
            {

                _parent.ExecuteOperation("Commit", () => _source.Commit());
            }

            public void Rollback()
            {
                _parent.ExecuteOperation("Rollback Transaction", () => _source.Rollback());
            }

            public IDbConnection Connection
            {
                get { return _source.Connection; }
            }

            public IsolationLevel IsolationLevel
            {
                get { return _source.IsolationLevel; }
            }
        }


        public IDbTransaction BeginTransaction()
        {
            return _logWriter.ExecuteOperation<IDbTransaction>("Begin Transaction",
                                                        () => new myTransaction(this, _connection.BeginTransaction()),
                                                        null);
        }

        static int _counter;
        int _connectionId = _counter++;
        long _thread = System.Threading.Thread.CurrentThread.ManagedThreadId;
        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return _logWriter.ExecuteOperation<IDbTransaction>("Begin Transaction", () => new myTransaction(this, _connection.BeginTransaction(il)), null);
        }
        void ExecuteOperation(string description, Action what)
        {
            _logWriter.ExecuteOperation<bool>(string.Format(description + " Thread ID={0} Connection Id={1}", System.Threading.Thread.CurrentThread.ManagedThreadId, _connectionId),
                () =>
                {
                    what();
                    return false;
                }, null);
        }

        public void Close()
        {
            ExecuteOperation("Close Connection", () => _connection.Close());
            if (ConnectionClosed != null)
                ConnectionClosed(this);
        }

        public void ChangeDatabase(string databaseName)
        {
            _connection.ChangeDatabase(databaseName);
        }

        public IDbCommand CreateCommand()
        {
            return new CommandWrapper(_connection.CreateCommand(), _logWriter);
        }
        static public T DoOnInternalCommand<T>(IDbCommand command, string whatToWriteToLog, Func<IDbCommand, T> what)
        {
            var z = command as CommandWrapper;
            if (z != null)
            {
                return z.DoOnInternalCommand(command, whatToWriteToLog, what);
            }
            var y = command as IDbCommandWrapper;
            if (y != null)
                return DoOnInternalCommand<T>(y.GetOriginalCommand(), whatToWriteToLog, what);
            return what(command);
        }

        public static event Action<IDbCommand> ExecuteReaderEvent;
        public static event Action<IDbCommand> ExecuteNonQueryEvent;
        internal class CommandWrapper : IDbCommand, IDbCommandWrapper
        {
            IDbCommand _command;
            ILogWriter _parent;
            public CommandWrapper(IDbCommand command, ILogWriter parent)
            {
                _parent = parent;
                _command = command;
            }


            public void Prepare()
            {
                _command.Prepare();
            }

            public void Cancel()
            {
                _command.Cancel();
            }

            public IDbDataParameter CreateParameter()
            {
                return _command.CreateParameter();
            }

            public int ExecuteNonQuery()
            {
                if (ExecuteNonQueryEvent != null)
                    ExecuteNonQueryEvent(_command);
                return _parent.ExecuteOperation<int>("ExecuteNonQuery", _command.ExecuteNonQuery, _command);
            }


            public IDataReader ExecuteReader()
            {
                if (ExecuteReaderEvent != null)
                    ExecuteReaderEvent(_command);
                return _parent.ExecuteOperation<IDataReader>("ExecuteReader", _command.ExecuteReader, _command);
            }

            public IDataReader ExecuteReader(CommandBehavior behavior)
            {
                if (ExecuteReaderEvent != null)
                    ExecuteReaderEvent(_command);
                return _parent.ExecuteOperation<IDataReader>("ExecuteReader", delegate { return _command.ExecuteReader(behavior); }, _command);
            }

            public object ExecuteScalar()
            {

                return _parent.ExecuteOperation<object>("ExecuteScalar", _command.ExecuteScalar, _command);
            }



            public IDbConnection Connection
            {
                get { return _command.Connection; }
                set { _command.Connection = value; }
            }

            public IDbTransaction Transaction
            {
                get { return _command.Transaction; }
                set
                {
                    var x = value as myTransaction;
                    if (x != null)
                        _command.Transaction = x._source;
                    else
                        _command.Transaction = value;
                }
            }

            public string CommandText
            {
                get { return _command.CommandText; }
                set { _command.CommandText = value; }
            }

            public int CommandTimeout
            {
                get { return _command.CommandTimeout; }
                set { _command.CommandTimeout = value; }
            }

            public CommandType CommandType
            {
                get { return _command.CommandType; }
                set { _command.CommandType = value; }
            }

            public IDataParameterCollection Parameters
            {
                get { return _command.Parameters; }
            }

            public UpdateRowSource UpdatedRowSource
            {
                get { return _command.UpdatedRowSource; }
                set { _command.UpdatedRowSource = value; }
            }

            public void Dispose()
            {
                _command.Dispose();
            }

            public IDbCommand GetInternalCommand()
            {
                return _command;
            }

            public IDbCommand GetOriginalCommand()
            {
                return _command;
            }
            public T DoOnInternalCommand<T>(IDbCommand command, string whatToWriteToLog, Func<IDbCommand, T> what)
            {
                var x = command;
                IDbCommandWrapper y;
                while ((y = x as IDbCommandWrapper) != null)
                {
                    x = y.GetOriginalCommand();
                }
                return _parent.ExecuteOperation<T>(whatToWriteToLog, () => what(x), _command);
            }
        }

        public void Open()
        {
            this.ExecuteOperation("Open Connection", () => _connection.Open());
            if (ConnectionOpened != null)
                ConnectionOpened(this);

            var z = _connection as System.Data.SqlClient.SqlConnection;
            if (z == null)
            {
                var y = _connection as IConnectionWrapper;
                while (y != null)
                {
                    z = y.GetInnerConnection() as SqlConnection;
                    if (z == null)
                    {
                        y = y.GetInnerConnection() as IConnectionWrapper;
                    }
                    else
                        y = null;
                }
            }

            if (z != null)
            {
                using (var c = CreateCommand())
                {
                    c.CommandText = "select @@spid";
                    try
                    {
                        _write("SPID=" + c.ExecuteScalar().ToString());
                    }
                    catch
                    {
                    }
                }
            }
        }

        public string ConnectionString
        {
            get { return _connection.ConnectionString; }
            set { _connection.ConnectionString = value; }
        }

        public int ConnectionTimeout
        {
            get { return _connection.ConnectionTimeout; }
        }

        public string Database
        {
            get { return _connection.Database; }
        }

        public ConnectionState State
        {
            get { return _connection.State; }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public IDbConnection GetInnerConnection()
        {
            return _connection;
        }
    }

    interface IDbCommandWrapper
    {
        IDbCommand GetOriginalCommand();
    }

    internal interface ILogWriter
    {
        Type ExecuteOperation<Type>(string description, Func<Type> whatToRun, IDbCommand _command);
    }
    class DurationLogWriter : ILogWriter
    {
        double _durationToLog;
        public DurationLogWriter(double seconds)
        {
            _durationToLog = seconds;
        }

        public Type ExecuteOperation<Type>(string description, Func<Type> whatToRun, IDbCommand command)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            try
            {
                var result = whatToRun();
                return result;
            }
            finally
            {
                sw.Stop();
                if (sw.Elapsed.TotalSeconds >= _durationToLog)
                {
                    using (var s = new StringWriter())
                    {
                        s.WriteLine(description + " - " + command.CommandText);
                        s.WriteLine(LogDatabaseWrapper.ParameterInfo("Query Parameters", command.Parameters));
                        s.WriteLine(LogDatabaseWrapper.DisplayDuration(sw));
                        ENV.ErrorLog.WriteTrace("LONGSQL", () => s.ToString());
                    }
                }
            }



        }
    }


}