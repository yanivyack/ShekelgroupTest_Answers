using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ENV.Data.DataProvider;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.Testing;

namespace ENV.Data.DataProvider
{
    public class ConnectionManager
    {
        public static string NewLineInSQL
        {
            get
            {
                return SQLDataProviderHelper.NewLineInSQL;
            }
            set { SQLDataProviderHelper.NewLineInSQL = value; }
        }

        public static bool UseOracleNamedParameter = true;
        static bool _useDbParameters = true, _odbcDbParameters = true;
        public static bool UseDBParameters { get { return _useDbParameters; } set { _useDbParameters = value; } }
        public static bool OdbcDbParameters { get { return _odbcDbParameters; } set { _odbcDbParameters = value; } }

        static Dictionary<string, Func<IEntityDataProvider>> _simpleDataProviders =
        new Dictionary<string, Func<IEntityDataProvider>>();
        static Dictionary<string, Func<XmlEntityDataProvider>> _xmlDataProviders =
        new Dictionary<string, Func<XmlEntityDataProvider>>();

        static ContextStatic<ConnectionManager> _contextInstance =
            new ContextStatic<ConnectionManager>(() => new ConnectionManager());
        public static ConnectionManager Context { get { return _contextInstance.Value; } }
        public static readonly ConnectionManager Shared = new ConnectionManager();

        ConnectionManager()
        {
            InitCrossDatabaseJoins();
        }
        static public void InitCrossDatabaseJoins()
        {
            ENV.Data.DataProvider.SQLClientEntityDataProvider._getEntityNameForJoins = (entity, mainEntity, entityName) =>
            {

                var envEntity = entity as ENV.Data.Entity;
                var envMainEntity = mainEntity as ENV.Data.Entity;
                if (envEntity == null || envMainEntity == null)
                    return entityName;
                if (envEntity.DataProvider == envMainEntity.DataProvider)
                    return entityName;
                if (entityName.StartsWith("dbo.", StringComparison.InvariantCultureIgnoreCase))
                    entityName = entityName.Substring(4);
                if (entityName.IndexOfAny(new char[] { '.', '(' }) >= 0)
                    return entityName;
                var d = envEntity.DataProvider as DynamicSQLSupportingDataProvider;
                if (d == null)
                    return entityName;

                using (var c = d.CreateCommand())
                {
                    return entity.EntityName = c.Connection.Database + ".." + entityName;//Donot cache this result as it may change
                }
            };
        }

        class ActiveConnectionFactory
        {

        }

        delegate ActiveConnection ConnectionBuilder(bool ignoreTransactions);
        Dictionary<string, ConnectionBuilder> _builders = new Dictionary<string, ConnectionBuilder>();

        public void AddOracleDatabase(string name, string serverTnsName, string username, string password)
        {
            AddDatabase(name, "14", "", serverTnsName, username, password, IsolationLevel.ReadCommitted, "", "", false, false, false);
        }
        public void AddMicrosoftSQLDatabase(string name, string initialCatalog, string server)
        {
            AddMicrosoftSQLDatabase(name, initialCatalog, server, "", "");
        }
        public void AddMicrosoftSQLDatabase(string name, string initialCatalog, string server, IsolationLevel isolationLevel)
        {
            AddMicrosoftSQLDatabase(name, initialCatalog, server, "", "", isolationLevel);
        }
        public void AddMicrosoftSQLDatabase(string name, string initialCatalog, string server, string username, string password)
        {
            AddMicrosoftSQLDatabase(name, initialCatalog, server, username, password, IsolationLevel.ReadUncommitted);
        }
        public void AddMicrosoftSQLDatabase(string name, string initialCatalog, string server, string username, string password, IsolationLevel isolationLevel)
        {
            AddDatabase(name, "21", initialCatalog, server, username, password, isolationLevel, "", "", false, false, false);
        }


        static ContextStaticDictionary<string, string> _openDatabaseConnections = new ContextStaticDictionary<string, string>();
#if DEBUG
        public static int ForTestingSearchCounter = 0;
#endif
        public static int MaxPoolSize = 0;
        public void AddDatabase(string name, string dbType, string pinitialCatalog, string pdataSource, string puserName, string ppassword,
        System.Data.IsolationLevel mssqlIsolationLevel, string hint, string path, bool verifyStructure, bool autoCreateTables, bool hasAcsFile)
        {
            var origName = name;
            name = name.ToUpper(CultureInfo.InvariantCulture);
            if (_builders.ContainsKey(name))
                _builders.Remove(name);
            string flipName = ((Text)name).Reverse();
            if (_simpleDataProviders.ContainsKey(name))
            {
                _simpleDataProviders.Remove(name);

                Firefly.Box.Context.Current[name] = null;
                Firefly.Box.Context.Current[flipName] = null;
            }

            if (_simpleDataProviders.ContainsKey(flipName))
            {
                _simpleDataProviders.Remove(flipName);
                Firefly.Box.Context.Current[name] = null;
                Firefly.Box.Context.Current[flipName] = null;
            }
            if (_xmlDataProviders.ContainsKey(name))
            {
                _xmlDataProviders.Remove(name);

                Firefly.Box.Context.Current[name] = null;
                Firefly.Box.Context.Current[flipName] = null;
            }

            if (_xmlDataProviders.ContainsKey(flipName))
            {
                _xmlDataProviders.Remove(flipName);
                Firefly.Box.Context.Current[name] = null;
                Firefly.Box.Context.Current[flipName] = null;
            }
            if (dbType == "1")
            {
                _simpleDataProviders.Add(name, () => new BtrieveDataProvider() { FilesPath = path, VerifyStructure = verifyStructure, Name = origName, CaseInsensitive = hasAcsFile });
            }
            else if (dbType == "0")
            {
                _xmlDataProviders.Add(name, () => new XmlEntityDataProvider() { FilesPath = path, Name = origName });
            }
            else if (dbType == "17")
            {
                _simpleDataProviders.Add(name,
                    () =>
                    {
                        string initialCatalog = PathDecoder.DecodePath(pinitialCatalog).Trim(),
                                dataSource = PathDecoder.DecodePath(pdataSource).Trim(),
                                username = Security.Entities.SecuredValues.Decode(puserName).Trim(),
                                publicUserName = PathDecoder.DecodePath(puserName).Trim(),
                                password = Security.Entities.SecuredValues.Decode(ppassword).Trim();
                        if (string.IsNullOrEmpty(username))
                            username = _defaultUser;
                        if (string.IsNullOrEmpty(password))
                            password = _defaultpassword;
                        return GetAs400IsamProvider(dataSource, path, username, password);
                    });
            }
            else
            {
                _myChangeMonitor.Change();
                _builders.Add(name,
                    (disableTransactions) =>
                    {
#if DEBUG
                        ForTestingSearchCounter++;
#endif
                        string initialCatalog =
                            PathDecoder.DecodePath(pinitialCatalog).Trim(),
                               dataSource = PathDecoder.DecodePath(pdataSource).Trim(),
                               username = Security.Entities.SecuredValues.Decode(puserName).Trim(),
                               publicUserName = PathDecoder.DecodePath(puserName).Trim(),
                               password = Security.Entities.SecuredValues.Decode(ppassword).Trim();

                        if (string.IsNullOrEmpty(username))
                            username = _defaultUser;
                        if (string.IsNullOrEmpty(password))
                            password = _defaultpassword;



                        StringBuilder connectionString = new StringBuilder();
                        if (!string.IsNullOrEmpty(initialCatalog) && dbType != "14")
                            connectionString.AppendFormat("Initial Catalog={0};", initialCatalog);
                        if (!string.IsNullOrEmpty(dataSource))
                            connectionString.AppendFormat("Data Source={0};", dataSource);
                        if (!string.IsNullOrEmpty(username))
                            connectionString.AppendFormat("User Id={0};", username);
                        string connectionDescriptionNonSecret = string.Format(
                                            "{0} Datasource={1};Catalog={2};User={3}",
                                            name,
                                            dataSource,
                                            initialCatalog, publicUserName);
                        if (_openDatabaseConnections.ContainsKey(name))
                            _openDatabaseConnections[name] = connectionDescriptionNonSecret.ToString();
                        else
                            _openDatabaseConnections.Add(name, connectionDescriptionNonSecret.ToString());

                        if (!string.IsNullOrEmpty(password))
                            connectionString.AppendFormat("Password={0};", password);
                        if (!UseConnectionPool)
                            connectionString.Append("Pooling=false;");
                        if (ConnectionTimeout > 0)
                            connectionString.AppendFormat("Connection Timeout={0};", ConnectionTimeout);
                        if (MaxPoolSize != 0)
                            connectionString.AppendFormat("Max Pool Size={0};", MaxPoolSize);
                        System.Data.IDbConnection connection;


                        string conStringForCache = dbType + Security.Entities.SecuredValues.Decode(path) + ";;;" + connectionString.ToString() + (disableTransactions ? "NOTRANS" : "");


                        ActiveConnection actioveConnection;
                        if (_connectionStringCache.TryGetValue(conStringForCache, out actioveConnection))
                        {
                            return actioveConnection;
                        }


                        var theResult = new ActiveConnection(conStringForCache,
                            () =>
                            {
                                try
                                {

                                    ISQLEntityDataProvider result;
                                    switch (dbType)
                                    {

                                        case "20":
                                        case "4":
                                            {
                                                var sb = new StringBuilder();
                                                sb.Append("DSN=" + initialCatalog);
                                                if (!Text.IsNullOrEmpty(username))
                                                    sb.Append(";Uid=" + username);
                                                if (!Text.IsNullOrEmpty(password))
                                                    sb.Append(";Pwd=" + password);
                                                connection = DecorateConnection(new System.Data.Odbc.OdbcConnection(sb.ToString()));
                                                var dataP =
                                                    new OdbcEntityDataProvider
                                                        (connection);
                                                dataP.UseParameters = UseDBParameters;
                                                if (!OdbcDbParameters)
                                                    dataP.UseParameters = false;
                                                result =
                                                    new BridgeToISQLEntityDataProvider
                                                        (dataP);
                                            }
                                            break;
                                        case "2":
                                            {

                                                result = GetPervasiveSQLDataProvider(dataSource, initialCatalog, out connection);
                                            }
                                            break;

                                        case "7":
                                            result =
                                                GetAs400SQLProvider(
                                                    dataSource, path,
                                                    username,
                                                    password, initialCatalog, DecorateConnection);
                                            break;
                                        case "14":
                                        case "OracleNoTrans":
                                            connection =
                                                DecorateConnection(_oracleConnectionFactory(connectionString.ToString()));
                                            var client = new OracleClientEntityDataProvider
                                                    (connection)
                                            {
                                                AnsiJoin = false,
                                                UseNamedParameters = UseOracleNamedParameter,
                                                ParameterNamePrefix = ":p",
                                                IsolationLevel = IsolationLevel.ReadCommitted,

                                            };
                                            _setProperties(client);
                                            client.UseParameters =
                                                UseDBParameters;

                                            result = new BridgeToISQLEntityDataProvider(client);
                                            if (dbType == "OracleNoTrans")
                                                result = _createNoTransactionDatabase(result, true, false);

                                            break;
                                        case "10":
                                            {

                                                result = _createSQLiteDataProvider(path);

                                            }

                                            break;
                                        case "19":
                                            result =
                                                GetDb2Provider(
                                                    dataSource,
                                                    initialCatalog,
                                                    username,
                                                    password,
                                                    UseConnectionPool,
                                                    DecorateConnection);
                                            break;
                                        case "pg":
                                            result =
                                                GetPostgresDataProvider(
                                                    dataSource,
                                                    initialCatalog,
                                                    username,
                                                    password,
                                                    UseConnectionPool,
                                                    DecorateConnection);
                                            break;
                                        default:
                                            if (initialCatalog ==
                                                null ||
                                                initialCatalog.Trim()
                                                    .Length == 0)
                                                throw new InvalidOperationException
                                                    (
                                                    "Microsoft SQL Connection, no database catalog specified");
                                            StringBuilder connect =
                                                new StringBuilder(
                                                    connectionString.
                                                        ToString());
                                            if (
                                                string.IsNullOrEmpty(
                                                    username))
                                                connect.Append(
                                                    @"Integrated Security=SSPI;Persist Security Info=False;");
                                            var assembly = System.
                                                               Reflection
                                                               .
                                                               Assembly
                                                               .
                                                               GetEntryAssembly
                                                               ()
                                                           ??
                                                           System.
                                                               Reflection
                                                               .
                                                               Assembly
                                                               .
                                                               GetCallingAssembly
                                                               ()
                                                           ??
                                                           System.
                                                               Reflection
                                                               .
                                                               Assembly
                                                               .
                                                               GetExecutingAssembly
                                                               ();
                                            if (assembly != null)
                                            {
                                                var n =
                                                    assembly.GetName();
                                                if (n != null)
                                                {
                                                    var
                                                        applicationName
                                                            =
                                                            n.Name +
                                                            "_" +
                                                            n.Version
                                                                .
                                                                ToString
                                                                ();
                                                    connect.Append(
                                                        "Application Name=" +
                                                        applicationName +
                                                        ";");
                                                }
                                            }
                                            Action
                                                <
                                                    ISQLEntityDataProvider
                                                    > doOnResult =
                                                        delegate { };
                                            IDbConnection sqlConnection =
OnOpenConnection(name, connect.ToString(), cs => new System.Data.SqlClient.SqlConnection(cs));


                                            connection =
                                                DecorateConnection(
                                                    sqlConnection);
                                            {

                                                var sqlclient =
                                                    new SQLClientEntityDataProvider
                                                        (connection)
                                                    { };
                                                sqlclient.
                                                    IsolationLevel =
                                                    mssqlIsolationLevel;
                                                sqlclient.
                                                    UseParameters =
                                                    UseDBParameters;
                                                if (dbType == "SQLCursorLocks")
                                                    sqlclient.UseCursorLocking = true;
                                                if (dbType == "NoTransSQL")
                                                {
                                                    sqlclient.UseCursorLocking = true;
                                                    result = new NoTransactionEntityDataProvider(new BridgeToISQLEntityDataProvider(sqlclient));
                                                }
                                                else if (dbType == "ISAMTransCompatible")
                                                {
                                                    sqlclient.UseCursorLocking = true;
                                                    result = new IsamTransactionCompatibleEntityDataProvider(new BridgeToISQLEntityDataProvider(sqlclient));
                                                }
                                                else
                                                {

                                                    result = new BridgeToISQLEntityDataProvider(sqlclient);
                                                }
                                            }
                                            doOnResult(result);
                                            break;
                                    }
                                    if (disableTransactions)
                                        result = _createNoTransactionDatabase(result, false, true);
                                    if (autoCreateTables)
                                        result.AutoCreateTables = autoCreateTables;
                                    return result;
                                }
                                catch (Exception e)
                                {
                                    var s = "Error openning connection " + connectionDescriptionNonSecret;
                                    Common.SetTemporaryStatusMessage(
                                        s);

                                    throw CreateConnectionException(s, e);

                                }
                            });
                        _connectionStringCache.Add(conStringForCache, theResult);

                        return theResult;

                    });
            }
        }

        class NoTransactionEntityDataProvider : ISQLEntityDataProvider, ICanGetDecoratedISSQLEntityDataProvider
        {
            bool ISQLEntityDataProvider.IsClosed()
            {
                return _source.IsClosed();
            }
            public SqlCommandFilterBuilder CreateSqlCommandFilterBuilder(IDbCommand command, Entity entity)
            {
                return _source.CreateSqlCommandFilterBuilder(command, entity);
            }

            public bool AutoCreateTables { get { return _source.AutoCreateTables; } set { _source.AutoCreateTables = value; } }
            ISQLEntityDataProvider _source;
            public ISQLEntityDataProvider GetSource()
            {
                return (ISQLEntityDataProvider)_source;
            }

            public bool RequiresTransactionForLocking
            {
                get
                {
                    return false;
                }
            }


            public NoTransactionEntityDataProvider(ISQLEntityDataProvider source)
            {
                _source = source;

            }

            class DummyTransaction : ITransaction
            {
                public void Commit()
                {

                }


                public void Rollback()
                {

                }


            }

            public ITransaction BeginTransaction()
            {
                return new DummyTransaction();

            }
            public bool Contains(Firefly.Box.Data.Entity entity)
            {
                return ((IEntityDataProvider)_source).Contains(entity);
            }

            public void Dispose()
            {
                _source.Dispose();
            }

            public long CountRows(Firefly.Box.Data.Entity entity)
            {
                return ((IEntityDataProvider)_source).CountRows(entity);
            }


            public IDbCommand CreateCommand()
            {
                return _source.CreateCommand();
            }

            public Exception ProcessException(Exception e, Firefly.Box.Data.Entity entity, IDbCommand c)
            {
                return GetSource().ProcessException(e, entity, c);
            }

            public bool IsOracle { get; private set; }
            public SqlScriptTableCreator CreateScriptGeneratorTable(Firefly.Box.Data.Entity entity)
            {
                return _source.CreateScriptGeneratorTable(entity);
            }

            public UserDbMethods.IUserDbMethodImplementation GetUserMethodsImplementation()
            {
                return _source.GetUserMethodsImplementation();
            }

            public void Drop(Firefly.Box.Data.Entity entity)
            {
                ((IEntityDataProvider)_source).Drop(entity);
            }



            public IRowsSource ProvideRowsSource(Firefly.Box.Data.Entity entity)
            {

                return ((IEntityDataProvider)_source).ProvideRowsSource(entity);
            }


            public bool SupportsTransactions
            {
                get { return false; }
            }

            public void Truncate(Firefly.Box.Data.Entity entity)
            {
                ((IEntityDataProvider)_source).Truncate(entity);
            }


            public ISQLEntityDataProvider GetDecorated()
            {
                return _source;
            }

            public IValueLoader GetDataReaderValueLoader(IDataReader reader, int columnIndexInSelect, IDateTimeCollector dtc)
            {
                return _source.GetDataReaderValueLoader(reader, columnIndexInSelect, dtc);
            }

            public string GetEntityName(Firefly.Box.Data.Entity entity)
            {
                return _source.GetEntityName(entity);
            }

            public ISupportsGetDefinition GetSupportGetDefinition()
            {
                return _source.GetSupportGetDefinition();
            }
        }
        internal class IsamTransactionCompatibleEntityDataProvider : ISQLEntityDataProvider, ICanGetDecoratedISSQLEntityDataProvider
        {
            bool ISQLEntityDataProvider.IsClosed()
            {
                return _source.IsClosed();
            }
            public SqlCommandFilterBuilder CreateSqlCommandFilterBuilder(IDbCommand command, Entity entity)
            {
                return _source.CreateSqlCommandFilterBuilder(command, entity);
            }

            public bool AutoCreateTables { get { return _source.AutoCreateTables; } set { _source.AutoCreateTables = value; } }
            ISQLEntityDataProvider _source;
            public ISQLEntityDataProvider GetSource()
            {
                return (ISQLEntityDataProvider)_source;
            }

            public bool RequiresTransactionForLocking
            {
                get
                {
                    return false;
                }
            }


            public IsamTransactionCompatibleEntityDataProvider(ISQLEntityDataProvider source)
            {
                _source = source;

            }

            class DummyTransaction : ITransaction
            {
                public void Commit()
                {

                }


                public void Rollback()
                {

                }


            }

            public ITransaction BeginTransaction()
            {
                if (EnableBtrieveTransactions)
                    return _source.BeginTransaction();
                return new DummyTransaction();

            }
            public bool Contains(Firefly.Box.Data.Entity entity)
            {
                return ((IEntityDataProvider)_source).Contains(entity);
            }

            public void Dispose()
            {
                _source.Dispose();
            }

            public long CountRows(Firefly.Box.Data.Entity entity)
            {
                return ((IEntityDataProvider)_source).CountRows(entity);
            }


            public IDbCommand CreateCommand()
            {
                return _source.CreateCommand();
            }

            public Exception ProcessException(Exception e, Firefly.Box.Data.Entity entity, IDbCommand c)
            {
                return GetSource().ProcessException(e, entity, c);
            }

            public bool IsOracle { get; private set; }
            public SqlScriptTableCreator CreateScriptGeneratorTable(Firefly.Box.Data.Entity entity)
            {
                return _source.CreateScriptGeneratorTable(entity);
            }

            public UserDbMethods.IUserDbMethodImplementation GetUserMethodsImplementation()
            {
                return _source.GetUserMethodsImplementation();
            }

            public void Drop(Firefly.Box.Data.Entity entity)
            {
                ((IEntityDataProvider)_source).Drop(entity);
            }



            public IRowsSource ProvideRowsSource(Firefly.Box.Data.Entity entity)
            {

                return ((IEntityDataProvider)_source).ProvideRowsSource(entity);
            }


            public bool SupportsTransactions
            {
                get { return EnableBtrieveTransactions; }
            }

            public void Truncate(Firefly.Box.Data.Entity entity)
            {
                ((IEntityDataProvider)_source).Truncate(entity);
            }


            public ISQLEntityDataProvider GetDecorated()
            {
                return _source;
            }

            public IValueLoader GetDataReaderValueLoader(IDataReader reader, int columnIndexInSelect, IDateTimeCollector dtc)
            {
                return _source.GetDataReaderValueLoader(reader, columnIndexInSelect, dtc);
            }

            public string GetEntityName(Firefly.Box.Data.Entity entity)
            {
                return _source.GetEntityName(entity);
            }

            public ISupportsGetDefinition GetSupportGetDefinition()
            {
                return _source.GetSupportGetDefinition();
            }
        }
        internal delegate ISQLEntityDataProvider CreateNoTransactionDatabaseDelegate(ISQLEntityDataProvider source, bool oracle, bool noLocks);
        static internal CreateNoTransactionDatabaseDelegate _createNoTransactionDatabase = delegate
        {
            throw new NotImplementedException("Can't find NoTransactionEntityDataProvider. Did you call it's InitMethod");
        };
        internal static void AddToErrorLog(StringWriter errorReportBuilder)
        {
            errorReportBuilder.WriteLine("Open database connections:");
            foreach (var item in _openDatabaseConnections)
            {
                errorReportBuilder.WriteLine("{0} - {1}", item.Key, item.Value);
            }
            errorReportBuilder.WriteLine();
        }

        internal static DatabaseErrorException CreateConnectionException(string s, Exception e)
        {
            return new DatabaseErrorException(
                DatabaseErrorType.
                    FailedToInitializeEntity,
                s + "\r\n" + e.Message, e,
                DatabaseErrorHandlingStrategy.AbortAllTasks);
        }

        public delegate IDbConnection OpenConnectionDelegate(
            string key, string connectionString, Func<string, IDbConnection> factory);

        public static OpenConnectionDelegate OnOpenConnection =
            (key, connectionString, factory) => factory(connectionString);


        public static void Clear()
        {
            Shared._builders.Clear();
            _contextInstance.DisposeAndClearValue();
            _connectionStringCache.DisposeAndClearValue();
        }

        static string _oracleOleProvider = "MSDAORA.1";
        public static bool DatabaseLogToDebugOutput { get; set; }
        public static string DatabaseLogFileName { get; set; }
        public static void SetOracleOleProvider(string fullName)
        {
            _oracleOleProvider = fullName;
        }

        static Func<string, IDbConnection> _oracleConnectionFactory = DefaultOracleConnectionFactory;
        static Action<OracleClientEntityDataProvider> _setProperties = delegate { };
        public static void SetOracleConnectionFactory(Func<string, IDbConnection> connectionFactory, Action<OracleClientEntityDataProvider> setProperties)
        {
            _oracleConnectionFactory = connectionFactory;
            _setProperties = setProperties;
        }
        public static IDbConnection CreateOracleRawInternalConnection(string dataSource, string username, string password)
        {
            var connectionString = new StringBuilder();
            if (!string.IsNullOrEmpty(dataSource))
                connectionString.AppendFormat("Data Source={0};", dataSource);
            if (!string.IsNullOrEmpty(username))
                connectionString.AppendFormat("User Id={0};", username);
            if (!string.IsNullOrEmpty(password))
                connectionString.AppendFormat("Password={0};", password);
            return _oracleConnectionFactory(connectionString.ToString());
        }

        static IDbConnection DefaultOracleConnectionFactory(string connectionString)
        {
            return
                new System.Data.OleDb.OleDbConnection("Provider=" + _oracleOleProvider + ";" +
                                                      connectionString.ToString());
        }

        internal static Func<string, ISQLEntityDataProvider> _createSQLiteDataProvider = (s) => { throw new TypeLoadException("The SQLite entity dataprovider was not loaded"); };




        public static bool UseConnectionPool { get; set; }
        public static int ConnectionTimeout { get; set; }




        static ContextStaticDictionary<string, ActiveConnection> _connectionStringCache =
            new ContextStaticDictionary<string, ActiveConnection>();

        internal delegate ISQLEntityDataProvider GetAs400SQLProviderDelegate(
            string server, string path, string username, string password, string initialCatalog, Func<IDbConnection, IDbConnection> decorateConnection);

        internal static GetAs400SQLProviderDelegate GetAs400SQLProvider = delegate
        {
            throw new InvalidOperationException(
                "AS400 data provider was not set up");
        };

        internal delegate ISQLEntityDataProvider GetDb2ProviderDelegate(
            string server, string datasource, string username, string password, bool pooling, Func<IDbConnection, IDbConnection> wrapConnection);
        internal delegate ISQLEntityDataProvider GetPervasiveSQLDataProviderDelegate(string dataSource, string initialCatalog, out IDbConnection connection);
        internal static GetPervasiveSQLDataProviderDelegate GetPervasiveSQLDataProvider = (string dataSource, string initialCatalog, out IDbConnection connection) =>
        {

            connection = DecorateConnection(new System.Data.Odbc.OdbcConnection(string.Format("DRIVER={{Pervasive ODBC Client Interface}};ServerName={0};ServerDSN={1}", dataSource, initialCatalog)));
            var dataP =
                new OdbcEntityDataProvider(connection) { SupportsMultipleReaders = true };
            return new BridgeToISQLEntityDataProvider(dataP);
        };
        internal static GetDb2ProviderDelegate GetDb2Provider = delegate
        {
            throw new InvalidOperationException(
                "Db2 dataprovider wasn't set up");
        };
        internal static GetDb2ProviderDelegate GetPostgresDataProvider = delegate
        {
            throw new InvalidOperationException(
                "GetPostgresDataProvider dataprovider wasn't set up");
        };

        internal delegate IEntityDataProvider GetAs400IsamProviderDelegate(string server, string path, string username, string password);

        internal static GetAs400IsamProviderDelegate GetAs400IsamProvider = delegate
        {
            throw new InvalidOperationException(
                "AS400 data provider was not set up");
        };

        internal static Func<Firefly.Box.Data.Entity, string> GetEntityNameWrapper(string initCat)
        {
            return e =>
            {
                var cat = PathDecoder.DecodePath(initCat).Trim();
                var tn = e.EntityName;
                int dotPosition = tn.IndexOf('.');
                if (dotPosition >= 0)
                    if (tn.IndexOf('.', dotPosition) < 0)
                        return cat + "." + tn;
                    else
                        return tn;
                else
                    return cat + ".." + tn;

            };
        }
        internal static double SecondsToWriteLongRunningSQLToTrace = 0;
        internal static IDbConnection DecorateConnection(IDbConnection connection)
        {
            connection = Profiler.DecorateConnection(connection);
            if (DatabaseLogToDebugOutput)
                connection = new LogDatabaseWrapper(connection);
            if (!string.IsNullOrEmpty(DatabaseLogFileName))
            {
                connection = new LogDatabaseWrapper(connection, DatabaseLogFileName);
            }
            if (SecondsToWriteLongRunningSQLToTrace > 0)
                connection = new LogDatabaseWrapper(connection, new DurationLogWriter(SecondsToWriteLongRunningSQLToTrace));

            if (connection.State != ConnectionState.Open)
                connection.Open();
            return connection;
        }

        public static IEntityDataProvider GetDataProvider(string name)
        {
            try
            {
                var upperName = name.ToUpper(CultureInfo.InvariantCulture);
                var result = Firefly.Box.Context.Current[upperName] as IEntityDataProvider;
                if (result == null)
                {
                    Func<IEntityDataProvider> r;
                    string locateName = upperName;
                    if (!_simpleDataProviders.ContainsKey(locateName))
                        locateName = ((Text)locateName).Reverse();
                    if (_simpleDataProviders.TryGetValue(locateName, out r))
                    {
                        result = r();
                        Firefly.Box.Context.Current[upperName] = result;
                        return result;
                    }
                    else
                        return GetSQLDataProvider(name);


                }
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to load datasource - " + name + " Exception - " + e.Message, e);
            }
        }
        /// <summary>
        /// Allows for transactions in btrieve - note that this also affects databases  that were migrated from btrieve to SQL
        /// </summary>
        public static bool EnableBtrieveTransactions { get; set; }
        public static bool DisableUniqueFilterOrderbyOptimization { get; set; }

        public static XmlEntityDataProvider GetXmlDataProvider(string name)
        {
            try
            {
                var upperName = name.ToUpper(CultureInfo.InvariantCulture);
                var result = Firefly.Box.Context.Current[upperName] as XmlEntityDataProvider;
                if (result == null)
                {
                    Func<XmlEntityDataProvider> r;
                    string locateName = upperName;
                    if (!_xmlDataProviders.ContainsKey(locateName))
                        locateName = ((Text)locateName).Reverse();
                    if (_xmlDataProviders.TryGetValue(locateName, out r))
                    {
                        result = r();
                        Firefly.Box.Context.Current[upperName] = result;
                        return result;
                    }
                    return XmlEntityDataProvider.Instance;


                }
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to load datasource - " + name + " Exception - " + e.Message, e);
            }
        }

        public static ContextStatic<Func<string, DynamicSQLSupportingDataProvider>> AlternateDb = new ContextStatic<Func<string, DynamicSQLSupportingDataProvider>>(() => null);
        public static DynamicSQLSupportingDataProvider GetSQLDataProvider(string name)
        {
            if (AlternateDb.Value != null)
            {
                var result = AlternateDb.Value(name);
                if (result != null)
                    return result;
            }
            return InternalGetSqlDataProvider(name, false);
        }
        public static event Action<DynamicSQLSupportingDataProvider> OnGetNewSQLDataProvider;
        static DynamicSQLSupportingDataProvider InternalGetSqlDataProvider(string name, bool disableTransactions)
        {
            try
            {
                var upperName = name.ToUpper(CultureInfo.InvariantCulture);
                string key = upperName + (disableTransactions ? "NOTRANS" : "");
                var result = Firefly.Box.Context.Current[key] as DynamicSQLSupportingDataProvider;
                if (result == null)
                {
                    string locateName = upperName;
                    if (!Context._builders.ContainsKey(locateName) && !Shared._builders.ContainsKey(locateName))
                        locateName = ((Text)locateName).Reverse();

                    var myChangeMonitor = new ChangeMonitor();
                    Security.Entities.SecuredValues.RegisterObserver(myChangeMonitor);
                    Context.RegisterObserver(myChangeMonitor);
                    Shared.RegisterObserver(myChangeMonitor);
                    ChangeObserver observer = null;
                    ;
                    ISQLEntityDataProvider myResult = null;
                    ConnectionBuilder connectionBuilder = null;

                    result = new DynamicSQLSupportingDataProvider(
                        () =>
                        {
                            try
                            {
                                if (myResult != null && myResult.IsClosed())
                                {
                                    connectionBuilder(disableTransactions).Disconnect();
                                    myResult.Dispose();

                                    myResult = null;
                                }
                                if (myResult != null && !observer.Changed())
                                    return myResult;


                                if (Context._builders.TryGetValue(locateName, out connectionBuilder))
                                    myResult = connectionBuilder(disableTransactions).GetDataProvder();
                                else
                                    myResult = (connectionBuilder = Shared._builders[locateName])(disableTransactions).GetDataProvder();
                                observer = myChangeMonitor.GetObserver();
                                return myResult;

                            }
                            catch (KeyNotFoundException)
                            {
                                using (var sw = new System.IO.StringWriter())
                                {
                                    sw.WriteLine("The database named \"{0}\" definition was not found in the databases section of the INI or in any loaded configurations.", upperName);
                                    if (Context._builders.Count == 0 && Shared._builders.Count == 0)
                                        sw.WriteLine("No connections were loaded, are you missing the ini file?");
                                    else
                                    {
                                        sw.WriteLine("These  are the databases that exist in the INI:");
                                        foreach (var item in Context._builders)
                                        {
                                            sw.WriteLine(item.Key);
                                        }
                                        foreach (var item in Shared._builders)
                                        {
                                            if (!Context._builders.ContainsKey(item.Key))
                                                sw.WriteLine(item.Key);
                                        }
                                    }
                                    throw new KeyNotFoundException(sw.ToString());
                                }
                            }
                        });
                    result.Name = name;

                    Firefly.Box.Context.Current[key] = result;
                    if (OnGetNewSQLDataProvider != null)
                        OnGetNewSQLDataProvider(result);
                }
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to load datasource - " + name + " Exception - " + e.Message, e);
            }
        }

        public static DynamicSQLSupportingDataProvider GetNoTransactionSQLDataProvider(string name)
        {
            return InternalGetSqlDataProvider(name, true);
        }

        ChangeMonitor _myChangeMonitor = new ChangeMonitor();
        void RegisterObserver(ChangeMonitor observingMonitor)
        {
            observingMonitor.Observe(_myChangeMonitor);
        }

        class ActiveConnection : IDisposable
        {
            string _keyForCache;
            myIsqlEntityDataProvider _dataProvider;
            Func<ISQLEntityDataProvider> _myFactory;
            class myIsqlEntityDataProvider : ISQLEntityDataProvider, ICanGetDecoratedISSQLEntityDataProvider
            {
                ISQLEntityDataProvider _source;

                public myIsqlEntityDataProvider(ISQLEntityDataProvider source)
                {
                    _source = source;
                }
                public SqlCommandFilterBuilder CreateSqlCommandFilterBuilder(IDbCommand command, Entity entity)
                {
                    return _source.CreateSqlCommandFilterBuilder(command, entity);
                }
                bool ISQLEntityDataProvider.IsClosed()
                {
                    return _source.IsClosed();
                }

                public ISupportsGetDefinition GetSupportGetDefinition()
                {
                    return _source.GetSupportGetDefinition();
                }

                public void Dispose()
                {
                    if (_activeEntities.Count > 0)
                        throw new InvalidOperationException("Cannot disconnect, Entity in use - " + _activeEntities[0].Entity);
                    _source.Dispose();
                }

                public IRowsSource ProvideRowsSource(Firefly.Box.Data.Entity entity)
                {

                    var x = new ActiveEntityMonitor(_source.ProvideRowsSource(entity), this, entity);
                    _activeEntities.Add(x);
                    return x;
                }

                public ITransaction BeginTransaction()
                {
                    return _source.BeginTransaction();
                }
                public bool RequiresTransactionForLocking
                {
                    get
                    {
                        return _source.RequiresTransactionForLocking;
                    }
                }

                public bool SupportsTransactions
                {
                    get { return _source.SupportsTransactions; }
                }

                public bool Contains(Firefly.Box.Data.Entity entity)
                {
                    return _source.Contains(entity);
                }

                public long CountRows(Firefly.Box.Data.Entity entity)
                {
                    return _source.CountRows(entity);
                }

                public void Drop(Firefly.Box.Data.Entity entity)
                {
                    _source.Drop(entity);
                }

                public void Truncate(Firefly.Box.Data.Entity entity)
                {
                    _source.Truncate(entity);
                }

                public IDbCommand CreateCommand()
                {
                    return _source.CreateCommand();
                }

                public Exception ProcessException(Exception e, Firefly.Box.Data.Entity entity, IDbCommand c)
                {
                    return _source.ProcessException(e, entity, c);
                }

                public bool IsOracle
                {
                    get { return _source.IsOracle; }
                }

                public bool AutoCreateTables
                {
                    get
                    {
                        return _source.AutoCreateTables;
                    }

                    set
                    {
                        _source.AutoCreateTables = value;
                    }
                }

                public SqlScriptTableCreator CreateScriptGeneratorTable(Firefly.Box.Data.Entity entity)
                {
                    return _source.CreateScriptGeneratorTable(entity);
                }

                public UserDbMethods.IUserDbMethodImplementation GetUserMethodsImplementation()
                {
                    return _source.GetUserMethodsImplementation();
                }


                List<ActiveEntityMonitor> _activeEntities = new List<ActiveEntityMonitor>();
                class ActiveEntityMonitor : IRowsSource
                {
                    IRowsSource _source;
                    myIsqlEntityDataProvider _parent;
                    public readonly Firefly.Box.Data.Entity Entity;

                    public ActiveEntityMonitor(IRowsSource source, myIsqlEntityDataProvider parent, Firefly.Box.Data.Entity entity)
                    {
                        _source = source;
                        _parent = parent;
                        Entity = entity;
                    }


                    public void Dispose()
                    {
                        _source.Dispose();
                        _parent._activeEntities.Remove(this);
                    }

                    public IRowsProvider CreateReader(IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort, IEnumerable<IJoin> joins, bool disableCache)
                    {
                        return _source.CreateReader(selectedColumns, where, sort, joins, disableCache);
                    }

                    public IRowsReader ExecuteReader(IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort, IEnumerable<IJoin> joins, bool lockAllRows)
                    {
                        return _source.ExecuteReader(selectedColumns, where, sort, joins, lockAllRows);
                    }

                    public IRowsReader ExecuteCommand(IEnumerable<ColumnBase> selectedColumns, IFilter filter, Sort sort, bool firstRowOnly, bool shouldBeOnlyOneRowThatMatchesTheFilter, bool lockAllRows)
                    {
                        return _source.ExecuteCommand(selectedColumns, filter, sort, firstRowOnly, shouldBeOnlyOneRowThatMatchesTheFilter, lockAllRows);
                    }

                    public IRow Insert(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, IRowStorage storage, IEnumerable<ColumnBase> selectedColumns)
                    {
                        return _source.Insert(columns, values, storage, selectedColumns);
                    }

                    public bool IsOrderBySupported(Sort sort)
                    {
                        return _source.IsOrderBySupported(sort);
                    }
                }

                public ISQLEntityDataProvider GetDecorated()
                {
                    return _source;
                }

                public IValueLoader GetDataReaderValueLoader(IDataReader reader, int columnIndexInSelect, IDateTimeCollector dtc)
                {
                    return _source.GetDataReaderValueLoader(reader, columnIndexInSelect, dtc);
                }

                public string GetEntityName(Firefly.Box.Data.Entity entity)
                {
                    return _source.GetEntityName(entity);
                }
            }

            public ActiveConnection(string keyForCache, Func<ISQLEntityDataProvider> factory)
            {
                _keyForCache = keyForCache;
                _myFactory = factory;

            }

            public bool Disconnect()
            {
                if (_dataProvider != null)
                    _dataProvider.Dispose();

                _connectionStringCache.Remove(_keyForCache);

                return true;
            }

            public ISQLEntityDataProvider GetDataProvder()
            {
                if (_dataProvider == null)
                    _dataProvider = new myIsqlEntityDataProvider(_myFactory());
                return _dataProvider;
            }

            public void Dispose()
            {
                if (_dataProvider != null)
                    _dataProvider.Dispose();
                _dataProvider = null;
            }
        }


        public static bool Disconnect(string name)
        {
            name = name.ToUpper(CultureInfo.InvariantCulture);
            string locateName = name;
            ConnectionBuilder con;
            bool found = false;
            if (!(found = _contextInstance.Value._builders.TryGetValue(name, out con)))
            {
                found = _contextInstance.Value._builders.TryGetValue(((Text)locateName).Reverse(), out con);
            }
            if (!found && !(found = Shared._builders.TryGetValue(name, out con)))
            {
                found = Shared._builders.TryGetValue(((Text)locateName).Reverse(), out con);
            }

            if (!found)
                return false;
            Context._myChangeMonitor.Change();
            Shared._myChangeMonitor.Change();
            var result = con(false).Disconnect();
            con(true).Disconnect();
            return result;
        }

        static string _defaultUser = "", _defaultpassword = "";
        public static void SetDefaultUserAndPassword(string p1, string p2)
        {
            _defaultUser = p1;
            _defaultpassword = p2;

        }
    }
    class ChangeMonitor
    {
        ChangeObserver _currentObserver;
        public void Change()
        {
            _currentObserver = null;
        }
        public ChangeObserver GetObserver()
        {
            if (_currentObserver == null)
            {
                _parentObservers = new ChangeObserver[_parentMonitors.Count];
                for (int i = 0; i < _parentMonitors.Count; i++)
                {
                    _parentObservers[i] = _parentMonitors[i].GetObserver();
                }
                _currentObserver = new ChangeObserver(this);
            }
            return _currentObserver;
        }
        List<ChangeMonitor> _parentMonitors = new List<ChangeMonitor>();
        ChangeObserver[] _parentObservers;
        public void Observe(ChangeMonitor parent)
        {
            _parentMonitors.Add(parent);
        }

        internal bool IsCurrent(ChangeObserver changeObserver)
        {
            for (int i = 0; i < _parentObservers.Length; i++)
            {
                if (_parentObservers[i].Changed())
                {
                    Change();
                    return false;
                }
            }
            return ReferenceEquals(_currentObserver, changeObserver);
        }
    }

    class ChangeObserver
    {
        ChangeMonitor _parent;
        public ChangeObserver(ChangeMonitor parent)
        {
            _parent = parent;
        }

        public bool Changed()
        {
            return !_parent.IsCurrent(this);
        }
    }
    public class ConnectionPoolThatKeepsTheConnectionOpen
    {
        public void SetFor(string databaseKey)
        {
            var x = ENV.Data.DataProvider.ConnectionManager.OnOpenConnection;

            ENV.Data.DataProvider.ConnectionManager.OnOpenConnection =
               (key, connectionString, factory) =>
               {
                   if (key == databaseKey.ToUpper())
                       return GetConnection(connectionString, factory);
                   return x(key, connectionString, factory);
               };
        }

        public void Dispose()
        {
            foreach (var con in _connections)
            {
                foreach (var ci in con.Value)
                {
                    ci.Terminate();
                }
            }
        }

        /* static void Write(string s)
         {
             var x = @"c:\temp\req.log";
             lock (x)
             {
                 using (var sw = new StreamWriter(x, true))
                 {
                     sw.WriteLine(ErrorLog.AdditionalInfo + ": " + s);
                 }
             }
         }*/

        class ActiveConnection
        {
            bool _free = true;
            Stopwatch _lastUsed = new Stopwatch();
            IDbConnection _connection;
            static int _counter = 0;
            int _myId = _counter++;

            /*  void Write(string s)
              {
                  MyConnectionPool.Write("Connection " + _myId + ": " + s);
              }
  */
            public ActiveConnection(IDbConnection connection)
            {
                _connection = connection;
                //           Write("New");

            }

            public IDbConnection TryClaim()
            {
                if (!_free)
                    return null;
                lock (this)
                {
                    if (!_free)
                        return null;
                    //           Write("Claim");
                    _free = false;
                    return new ActiveConnectionInstance(this);
                }
            }



            public bool IdleForTooLong(TimeSpan time)
            {

                if (_lastUsed.Elapsed > time)
                {
                    var x = TryClaim();
                    if (x != null)
                    {
                        Terminate();
                        return true;
                    }

                }
                return false;
            }

            public void Terminate()
            {
                //Write("Terminate");
                _connection.Close();
                _connection.Dispose();
            }

            public bool IsBusy()
            {
                return !_free;
            }

            class ActiveConnectionInstance : IDbConnection
            {
                ActiveConnection _parent;



                public ActiveConnectionInstance(ActiveConnection parent)
                {
                    _parent = parent;
                }



                public IDbTransaction BeginTransaction()
                {
                    return _parent._connection.BeginTransaction();
                }

                public IDbTransaction BeginTransaction(IsolationLevel il)
                {
                    return _parent._connection.BeginTransaction(il);
                }

                public void Dispose()
                {
                    if (_parent == null)
                        return;
                    _parent._lastUsed = new Stopwatch();
                    _parent._lastUsed.Start();
                    _parent._free = true;
                    //_parent.Write("Free");
                    _parent = null;
                }

                public void Close()
                {
                    Dispose();
                }

                public void ChangeDatabase(string databaseName)
                {
                    _parent._connection.ChangeDatabase(databaseName);
                }

                public IDbCommand CreateCommand()
                {
                    return _parent._connection.CreateCommand();
                }

                public void Open()
                {
                    if (_parent._connection.State != ConnectionState.Open)
                        _parent._connection.Open();
                }

                public string ConnectionString
                {
                    get { return _parent._connection.ConnectionString; }
                    set { _parent._connection.ConnectionString = value; }
                }

                public int ConnectionTimeout
                {
                    get { return _parent._connection.ConnectionTimeout; }
                }

                public string Database
                {
                    get { return _parent._connection.Database; }
                }

                public ConnectionState State
                {
                    get { return _parent._connection.State; }
                }




            }
        }

        class Connections : IEnumerable<ActiveConnection>
        {
            List<ActiveConnection> _cons = new List<ActiveConnection>();

            public IEnumerator<ActiveConnection> GetEnumerator()
            {
                return _cons.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(ActiveConnection dbConnection)
            {
                lock (this)
                {
                    var x = new List<ActiveConnection>(_cons);
                    x.Add(dbConnection);
                    _cons = x;
                    //Write("Added one: now " + _cons.Count);
                }
            }

            public void CleanUp(TimeSpan time)
            {
                var connectionsToRemove = new HashSet<ActiveConnection>();
                int i = 0;
                foreach (var c in _cons)
                {
                    if (c.IdleForTooLong(time))
                    {

                        connectionsToRemove.Add(c);
                    }
                    i++;
                }
                if (i > 0)
                {
                    lock (this)
                    {
                        var newL = new List<ActiveConnection>(_cons.Count);
                        foreach (var c in _cons)
                        {
                            if (!connectionsToRemove.Contains(c))
                                newL.Add(c);
                        }
                        //Write("Cleanup " + _cons.Count + " => " + newL.Count);
                        _cons = newL;
                    }


                }

            }
        }

        Dictionary<string, Connections> _connections = new Dictionary<string, Connections>();
        Stopwatch _lastCleanup = new Stopwatch();

        public ConnectionPoolThatKeepsTheConnectionOpen()
        {
            _lastCleanup.Start();
        }

        TimeSpan _timeBetweenCleanups = new TimeSpan(0, 30, 0);
        TimeSpan _timeToCloseConnection = new TimeSpan(1, 0, 0);

        public TimeSpan CleanupInterval
        {
            get { return _timeBetweenCleanups; }
            set { _timeBetweenCleanups = value; }
        }

        public TimeSpan IdleTimeToCloseConnection
        {
            get { return _timeToCloseConnection; }
            set { _timeToCloseConnection = value; }
        }


        public IDbConnection GetConnection(string connectionString, Func<string, IDbConnection> createConnection)
        {
            try
            {
                Connections cons;
                if (!_connections.TryGetValue(connectionString, out cons))
                {
                    lock (_connections)
                    {
                        if (!_connections.TryGetValue(connectionString, out cons))
                        {
                            cons = new Connections();
                            _connections.Add(connectionString, cons);
                        }
                    }
                }
                foreach (var con in cons)
                {
                    var x = con.TryClaim();
                    if (x != null)
                        return x;

                }
                var r = new ActiveConnection(createConnection(connectionString + "Pooling=false;"));

                var z = r.TryClaim();
                cons.Add(r);
                return z;
            }
            finally
            {
                if (_lastCleanup.Elapsed > _timeBetweenCleanups)
                {
                    bool cleanUp = false;
                    lock (_lastCleanup)
                    {
                        if (_lastCleanup.Elapsed > _timeBetweenCleanups)
                        {
                            cleanUp = true;
                            _lastCleanup.Reset();
                            _lastCleanup.Start();
                        }
                    }
                    if (cleanUp)
                        Cleanup();
                }
            }

        }

        public void CountShouldBe(int expectedCount, int expectedBusy)
        {
            int count = 0, busy = 0;

            foreach (var con in _connections)
            {
                foreach (var c in con.Value)
                {
                    count++;
                    if (c.IsBusy())
                        busy++;
                }
            }
            count.ShouldBe(expectedCount, "Connection count");
            busy.ShouldBe(expectedBusy, "Busy Connection Count");
        }

        public void Cleanup()
        {



            foreach (var con in _connections)
            {
                con.Value.CleanUp(_timeToCloseConnection);

            }
        }
    }
    public class DatabaseStringFixer
    {

        public virtual string toDb(string s)
        {
            return s;
        }
        public virtual string fromDb(string s)
        {
            return s;
        }
    }
    public class KeyBasedStringFixer : DatabaseStringFixer
    {
        StringReplacer _fromDb = new StringReplacer(),
            _toDb = new StringReplacer();
        protected void Add(char key, char value)
        {
            _fromDb.Replace(key, value);
            _toDb.Replace(value, key);

        }
        protected void AddToDb(char key, char value)
        {
            _toDb.Replace(key, value);
        }
        public override string fromDb(string s)
        {
            return _fromDb.Fix(s);
        }
        public override string toDb(string s)
        {
            return _toDb.Fix(s);
        }
        class StringReplacer
        {
            char[] _chars = new char[char.MaxValue];
            public void Replace(char key, char value)
            {
                _chars[key] = value;

            }

            public string Fix(string val)
            {
                if (val == null)
                    return null;
                char[] result = null;
                for (int i = 0; i < val.Length; i++)
                {
                    var replace = _chars[val[i]];
                    if (replace != default(char))
                    {
                        if (result == null)
                            result = val.ToCharArray();
                        result[i] = replace;
                    }
                }
                if (result == null)
                    return val;
                return new string(result);

            }
        }
    }
    public class FrenchCanadianDatabaseStringFixer : KeyBasedStringFixer
    {
        public FrenchCanadianDatabaseStringFixer()
        {
            Add('\u0092', '’');
            Add('\u0095', '•');
            Add('\u0085', '…');
            Add('\u0097', '—');
            Add('\u0096', '–');
            Add('\u0091', '‘');
            Add('\u009c', 'œ');
            Add('\u008c', 'Œ');
        }
    }

}