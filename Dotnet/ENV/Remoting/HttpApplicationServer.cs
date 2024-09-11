using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using ENV.IO;
using Firefly.Box;

namespace ENV.Remoting
{
    public class HttpApplicationServer
    {
        Func<ApplicationControllerBase> _applicationProvider;

        public HttpApplicationServer(Func<ApplicationControllerBase> applicationProvider,Semaphore concurrentControl = null)
        {
            _mySemaphore = concurrentControl;
            _applicationProvider = applicationProvider;
        }

        static List<RequestInfo> _requests = new List<RequestInfo>();
        public enum RequestStatus
        {
            Pending = 1,
            Processing = 2,
            Done = 3,
            Failed = 4,
            Aborded = 5
        }
        [Serializable]
        public class RequestInfo
        {
            public RequestInfo()
            {
                if (HttpContext.Current != null)
                    _userHost = System.Web.HttpContext.Current.Request["REMOTE_HOST"];
            }
            long _id;
            RequestStatus _status = RequestStatus.Pending;
            string _operation;
            public string Operation
            {
                get { return _operation; }
                set
                {
                    _operation = value;
                    SetErrorLogInfo();
                }
            }
            DateTime _post = DateTime.Now, _start = DateTime.MinValue, _end = DateTime.MinValue;
            void RefreshStatus()
            {
                if (!string.IsNullOrEmpty(_otherServer))
                {
                    if (_status == RequestStatus.Pending || _status == RequestStatus.Processing)
                    {
                        try
                        {
                            var sa = new HttpApplication(_otherServer).GetRequests(_otherServerRequestResult.MessageId, _otherServerRequestResult.MessageId + 1, false);
                            if (sa.Length == 0)
                                throw new Exception("Couldn't find request on secondary server " + _otherServer);
                            else
                            {
                                _status = sa[0]._status;
                                if (_status != RequestStatus.Pending)
                                {
                                    _start = sa[0]._start;
                                    _end = sa[0]._end;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Failed(ex);
                        }

                    }
                }
            }
            string ExceptionMessage;
            string _userHost = "";
            
            
            public RequestStatus Status { get { return _status; } }
            bool _hideFromMonitoring;

            public RequestInfo(long id)
            {
                _id = id;
            }

            public bool StartProcessing()
            {
                lock (this)
                {
                    SetErrorLogInfo();
                    if (_status != RequestStatus.Aborded)
                    {
                        ErrorLog.WriteTrace(
                            () => "Start Remote Call " + _id + " " + _operation + " From Host:" + _userHost);
                        _status = RequestStatus.Processing;
                        _start = DateTime.Now;
                        return true;
                    }
                    return false;
                }
            }

            void SetErrorLogInfo()
            {
                ErrorLog.AdditionalInfo = _id.ToString() + "-" + Operation;
            }

            public RequestResult Completed(object resultData, byte[] resultBytes)
            {
                _status = RequestStatus.Done;
                _end = DateTime.Now;
                var r = new RequestResult(_id);
                r.Success(resultData, resultBytes);
                var duration = (DateTime.Now - _start).TotalMilliseconds;
                ErrorLog.WriteTrace(() => "Completed Remote Call " + _id + " " + _operation + " duration:" +
                    (duration < 50 ? "" :
                    duration < 100 ? "." :
                    duration < 200 ? ".." :
                    duration < 500 ? "..." :
                    duration < 1000 ? "...." :
                    duration < 2000 ? "....." :
                    duration < 3000 ? "......" : ".......")
                    + ": " + duration);


                return r;
            }

            public RequestResult Failed(Exception e)
            {
                _status = RequestStatus.Failed;
                _end = DateTime.Now;
                ExceptionMessage = e.Message;
                ErrorLog.WriteTrace(() => "Faled Remote Call " + _id + " " + _operation + " Message:" + e.Message);
#if DEBUG
                //   ExceptionMessage += "\n Stack:\n" + e.StackTrace;
#endif
                var r = new RequestResult(_id);
                r.Failed(e);
                return r;
            }
            public RequestResult Aborted()
            {
                return new RequestResult(_id);

            }

            public static void PopulateTable(HtmlTableWriter<RequestInfo> t, bool includeServer)
            {
                t.AddColumn("Id", x => x._id.ToString());
                t.AddColumn("Status", x => x._status.ToString());
                t.AddColumn("Start Time", x => x._start.ToString());
                t.AddColumn("Elapsed", x => x.ElapsedTime().TotalSeconds.ToString("###,###.000"));
                t.AddColumn("Operation", x => x.Operation);
                t.AddColumn("UserHost", x => x._userHost);
                if (includeServer)
                    t.AddColumn("Server", x =>
                    {
                        if (x._otherServer == null)
                            return "";
                        else return x._otherServer + " " + x._otherServerRequestResult.MessageId;
                    });
                t.AddColumn("Error", x => x.ExceptionMessage);
            }
            TimeSpan ElapsedTime()
            {
                switch (_status)
                {
                    case RequestStatus.Done:
                    case RequestStatus.Failed:
                        return (_end - _start);
                    case RequestStatus.Pending:
                        return DateTime.Now - _post;

                }
                return (DateTime.Now - _start);
            }


            public RequestResult AsyncProcessing()
            {
                var r = new RequestResult(_id);
                r.Success(null, null);
                return r;
            }
            string _otherServer;
            RequestResult _otherServerRequestResult;

            internal RequestResult AsyncProcessToOtherServer(string server, RequestResult otherServerRequest)
            {
                _otherServer = server;
                _otherServerRequestResult = otherServerRequest;
                return AsyncProcessing();
            }

            public void AddYourselfTo(string applicationName, string serverIP, long @from, long to, bool pendingOnly, Action<RequestInfo> add)
            {
                RefreshStatus();
                if (!_hideFromMonitoring && (pendingOnly && _status == RequestStatus.Pending ||
                    !pendingOnly && _id >= @from && _id < to))
                {
                    _applicationName = applicationName;
                    _myIp = serverIP;
                    add(this);
                }
            }
            string _applicationName;
            string _myIp;
            internal string ToReqInfString()
            {


                return (_applicationName + "," +
                    Operation + "," +
                    "?,0," +
                    _userHost + "," +
                    "?," +
                    _post.ToString("dd/MM HH:mm:ss") + "," +
                    Math.Round(ElapsedTime().TotalSeconds).ToString() + "," +
                    _id + "," +
                    ((int)_status).ToString() + "," +
                    "0,0,0," + _myIp
                    );

            }

            public long GetWaitTime()
            {
                switch (_status)
                {
                    case RequestStatus.Pending:
                        return (long)(DateTime.Now - _post).TotalMilliseconds;
                    default:
                        return (long)(_start - _post).TotalMilliseconds;
                }
            }

            public bool Is(long requestId)
            {
                return _id == requestId;
            }

            public bool Abort()
            {
                lock (this)
                {
                    if (_status == RequestStatus.Pending)
                    {
                        _status = RequestStatus.Aborded;
                        return true;
                    }
                    return false;
                }
            }


            public int Stat()
            {
                return (int)_status;
            }

            public string GetId()
            {
                return _id.ToString();
            }
            public long GetLongId()
            {
                return _id;
            }


            public void SetHostName(string hostName)
            {
                _userHost = hostName;
            }

            public bool AddYourselfToLog(Action<RequestInfo> add)
            {
                RefreshStatus();
                if (!_hideFromMonitoring)
                {
                    add(this);
                    return true;
                }
                return false;
            }

            public void HideFromMonitoring()
            {
                _hideFromMonitoring = true;
            }


        }



        static DateTime StartedOn = DateTime.Now;
        static long _lastId = 0;
        public static bool ShowRequestLogForRemoteMachines = false;
        void PrintRequests()
        {

            var r = System.Web.HttpContext.Current.Response;

            if (!HttpContext.Current.Request.IsLocal && !ShowRequestLogForRemoteMachines)
            {
                r.Write("Request info is blocked");
                return;
            }

            string refresh = System.Web.HttpContext.Current.Request["Refresh"];
            if (refresh == null)
                refresh = "5";
            var rows = Number.Parse(System.Web.HttpContext.Current.Request["Rows"] ?? "50");
            if (rows <= 0)
                rows = 50;

            string s = "<b>Version Information:</b><br/>Version = " + this.GetType().Assembly.GetName().Version.ToString() + "<br/>";
            foreach (var customAttribute in this.GetType().Assembly.GetCustomAttributes(false))
            {
                {
                    var y = customAttribute as AssemblyInformationalVersionAttribute;

                    if (y != null)
                    {
                        s += "Version Info: " + y.InformationalVersion + "<br/>";
                    }
                }


            }
            s += "TNS_ADMIN=" + (System.Environment.GetEnvironmentVariable("TNS_ADMIN") ?? "") + "<br/>";
            s += "Server Started On=" + StartedOn.ToString() + "<br/>";
            if (_otherServers.Count > 0)
                s += "Secondary Servers=" + string.Join(",", _otherServers) + "<br/>";
            s += "Current Time=" + DateTime.Now.ToString() + "<br/>";
            s += "Active Requests=" + _runningRequests + "<br/>";

            var printedRows = 0;
            using (var t = new HtmlTableWriter<RequestInfo>("Remote Requests", s, r.Write, refresh))
            {
                RequestInfo.PopulateTable(t, _otherServers.Count != 0);
                for (var i = _requests.Count - 1; i >= 0; i--)
                {
                    if (_requests[i].AddYourselfToLog(info => t.WriteLine(info)))
                        if (++printedRows >= rows)
                            break;

                }
            }
            r.Write("<br/><h3>Optional url parameters:</h3>Refresh=[seconds]<br/>Rows=[number]<br/>");// "Filter=[Pending/Processing/Done/Failed/Aborted]");

        }

        internal static void SetSecondaryServers(string value)
        {
            _otherServers.Clear();
            if (!string.IsNullOrWhiteSpace(value))
                _otherServers.AddRange(value.Split(','));
        }

        [Serializable]
        public class RequestResult
        {
            object _resultData;
            public object ResultData
            {
                get { return _resultData; }
            }

            long _messageId;
            public long MessageId
            {
                get { return _messageId; }
            }

            bool _successful;
            public bool Successful { get { return _successful; } }

            string _exceptionMessage;

            public string ExceptionMessage
            {
                get { return _exceptionMessage; }
            }

            byte[] _resultStream;
            public byte[] ResultStream
            {
                get { return _resultStream; }
            }

            public RequestResult(long messageId)
            {

                _messageId = messageId;

            }
            public void Success(object resultData, byte[] resultStream)
            {
                _resultData = resultData;
                _resultStream = resultStream;
                _successful = true;
            }

            public void Failed(Exception e)
            {

                _exceptionMessage = e.Message;
            }
        }

        internal class TextWriterBridgeToITextWriterWithoutDispose : ITextWriter
        {
            TextWriter _writer;

            public TextWriterBridgeToITextWriterWithoutDispose(TextWriter writer)
            {
                _writer = writer;
            }

            public void Dispose()
            {

            }

            public void Write(string s)
            {
                _writer.Write(s);
            }

            public void WriteInitBytes(byte[] obj)
            {

            }

            public void WriteFile(string fileName)
            {
                Write(System.IO.File.ReadAllText(PathDecoder.DecodePath(fileName)));
            }
        }

        static int _maxConcurrentRequests = int.MaxValue;
        public static int MaxConcurrentRequests
        {
            get { return _maxConcurrentRequests; }
            set
            {
                _maxConcurrentRequests = value;
                var x = _maxConcurrentRequests;
                if (x <= 0)
                    x = int.MaxValue;
                _concurentThreadControl = new Semaphore(x, x);
            }
        }
        Semaphore _mySemaphore;
        static System.Threading.Semaphore _concurentThreadControl = new Semaphore(_maxConcurrentRequests, _maxConcurrentRequests);

        public static readonly ContextStatic<long> RequestID = new ContextStatic<long>(
            () =>
            {
                long result = -1;
                lock (_requests)
                {
                    result = ++_lastId;
                }
                ENV.ParametersInMemory.Instance.Set("MGCurrentRequestID", result.ToString());
                return result;
            });

        public static string SoapRequestsFormat = "systinet";
        static int _runningRequests = 0;

        [Serializable]
        class MyRelayCommand : RemoteCommand<IRemoteApplication>, HttpApplication.HasToDoWithRequest
        {
            RemoteCommand<IRemoteApplication> _originalCommand;
            public MyRelayCommand(RemoteCommand<IRemoteApplication> orig)
            {
                _originalCommand = orig;
            }
            public bool Async
            {
                get
                {
                    return true;
                }
            }

            public void ApplyTo(RequestInfo info)
            {
                var z = _originalCommand as HttpApplication.HasToDoWithRequest;
                if (z != null)
                    z.ApplyTo(info);
            }

            public object Execute(IRemoteApplication param)
            {
                return _originalCommand.Execute(param);
            }
        }
        static List<string> _otherServers = new List<string>();
        public void Process()
        {
            try
            {
                if (System.Web.HttpContext.Current.Request.Params["List"] == "Y" || System.Web.HttpContext.Current.Request.InputStream.Length == 0 && string.IsNullOrEmpty(System.Web.HttpContext.Current.Request["PRGNAME"]))
                {
                    PrintRequests();
                    return;

                }

                ENV.Remoting.HttpRemotingProvider<IRemoteApplication>.Process(
                    o =>
                    {

                        var t = (RemoteCommand<ENV.Remoting.IRemoteApplication>)o;

                        return ProcessRemoteCommand(t);
                    });

            }
            catch (Exception ex)
            {
                ENV.ErrorLog.WriteToLogFile(ex, "");
            }
        }

        public  object ProcessRemoteCommand(RemoteCommand<IRemoteApplication> t)
        {
            var relay = t as MyRelayCommand;
            if (relay != null && _runningRequests >= _maxConcurrentRequests)
                return null;
            RequestInfo info = null;
            long requestID;
            if (!(t is HttpApplication.InternalServerCommand))
                lock (_requests)
                {
                    info = new RequestInfo(requestID = RequestID.Value);
                    _requests.Add(info);
                }
            else
                info = new RequestInfo(requestID = 0);
            try
            {
                var tBase = t as HttpApplication.HasToDoWithRequest;
                if (tBase != null)
                {
                    tBase.ApplyTo(info);


                }


                object result = null;
                Func<object> DoWork =
                        () =>
                        {
                            var z = _mySemaphore;
                            if (z == null)
                                z = _concurentThreadControl;
                            try
                            {
                                using (ProgramCollection.StartProfilerForWeb(() => requestID.ToString()))
                                {
                                    if (!(t is HttpApplication.InternalServerCommand))
                                        z.WaitOne();
                                    {
                                        Interlocked.Increment(ref _runningRequests);
                                        if (info.StartProcessing())
                                        {
                                            using (var m = new MemoryStream())
                                            {
                                                using (
                                                    var sw = new StreamWriter(m,
                                                                              LocalizationInfo.Current.
                                                                                  OuterEncoding)
                                                    )
                                                {
                                                    WebWriter.FixedWriter =
                                                        new TextWriterBridgeToITextWriterWithoutDispose(sw);
                                                    try
                                                    {
                                                        result =
                                                            t.Execute(new RemoteApplicationController(this,
                                                                info));
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        using (ENV.Utilities.Profiler.StartContext("Request failed"))
                                                        {
                                                            ErrorLog.WriteToLogFile(e);
                                                            throw;
                                                        }
                                                    }

                                                }
                                                return info.Completed(result, m.ToArray());
                                            }
                                        }
                                    }
                                    return info.Aborted();

                                }
                            }
                            catch (Exception e)
                            {
                                return info.Failed(e);

                            }
                            finally
                            {
                                Interlocked.Decrement(ref _runningRequests);
                                if (!(t is HttpApplication.InternalServerCommand))
                                    z.Release();
                                Context.Current.Dispose();
                            }
                        };
                if (t.Async)
                {


                    var newThread = new Thread(() =>
                    {
                        if (_otherServers.Count > 0)
                        {
                            while (_runningRequests >= _maxConcurrentRequests)
                            {
                                foreach (var otherServer in _otherServers)
                                {
                                    try
                                    {
                                        var r = new MyRelayCommand(t);
                                        var s = new HttpRemotingProvider<IRemoteApplication>(otherServer);
                                        var otherServerResult = s.ExecuteOnServer(r);
                                        var otherServerRequest = otherServerResult as RequestResult;
                                        if (otherServerRequest != null)
                                        {
                                            info.AsyncProcessToOtherServer(otherServer, otherServerRequest);
                                            return;
                                        }
                                    }
                                    catch { }

                                }
                                Thread.Sleep(500);
                            }
                        }

                        DoWork();
                    });
                    newThread.SetApartmentState(System.Threading.Thread.CurrentThread.GetApartmentState());
                    newThread.Start();
                    return info.AsyncProcessing();
                }
                else
                    return DoWork();
            }
            catch (Exception e)
            {
                info.Failed(e);
                throw;
            }
        }
        public static List<string> RegisteredApplications = new List<string>();

        class RemoteApplicationController : IRemoteApplication
        {
            HttpApplicationServer _parent;
            RequestInfo _info;

            public RemoteApplicationController(HttpApplicationServer parent, RequestInfo info)
            {
                _parent = parent;
                _info = info;
            }


            public object Run(string publicName, params object[] args)
            {
                UserMethods.Instance.SetParam("PRGNAME", publicName);
                _info.Operation = publicName;
                //       lock ("remote")
                {
                    Firefly.Box.Context.Current.SetNonUIThread();
                    var app = _parent._applicationProvider();
                    ApplicationControllerBase.SetRootApplicationType(app.GetType());
                    var result = app.AllPrograms.InternalRunByPublicName(publicName, args, _info.GetLongId());

                    return result;

                }
            }

            public int GetRqRts()
            {
                _info.Operation = "GetRqRts";
                _info.HideFromMonitoring();
                if (RegisteredApplications.Count == 0)
                return 1;
                return RegisteredApplications.Count;
            }
            string GetMyIP()
            {
                IPHostEntry host;
                var localIP = "?";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                    }
                }
                return localIP;
            }

            public string GetRqRtInf(int engineNumber)
            {
                _info.Operation = "GetRqRtInf";
                _info.HideFromMonitoring();

                var r = System.Environment.MachineName + "," +
                       engineNumber.ToString()+"," +
                       GetMyIP() + ",";
                if (RegisteredApplications.Count > 0)
                    r += engineNumber.ToString();
                else
                    r += System.Diagnostics.Process.GetCurrentProcess().Id;
                r+= "," +"1,";
                if (RegisteredApplications.Count == 0)
                    return r + _parent._applicationProvider().GetType().Namespace;
                return r + RegisteredApplications[engineNumber - 1];
            }

            public RequestInfo[] GetRequests(long from, long to, bool pendingOnly)
            {
                _info.Operation = "GetRqRtInf";
                _info.HideFromMonitoring();

                var result = new List<RequestInfo>();
                var ip = GetMyIP();
                var app = _parent._applicationProvider().GetType().Namespace;
                foreach (var requestInfo in _requests.ToArray())
                {
                    requestInfo.AddYourselfTo(app, ip, from, to, pendingOnly, result.Add);
                }
                return result.ToArray();
            }

            public string GetRqLoad()
            {
                long pending = 0, inProgress = 0, executed = 0, failed = 0, totalWaitTime = 0, totalRequests = 0;

                foreach (var r in _requests)
                {
                    totalRequests++;
                    totalWaitTime = r.GetWaitTime();
                    switch (r.Status)
                    {
                        case RequestStatus.Done:
                            executed++;
                            break;
                        case RequestStatus.Failed:
                            failed++;
                            break;

                        case RequestStatus.Pending:
                            pending++;
                            break;
                        case RequestStatus.Processing:
                            inProgress++;
                            break;
                    }
                }
                Number averagePending = 0;
                if (totalRequests > 0)
                    averagePending = (totalWaitTime / totalRequests) / 1000;
                return averagePending.ToString("10.2") + "," + totalRequests + "," + pending + "," + inProgress + "," + executed + "," + failed;
            }

            public bool AbortRequest(long requestId)
            {
                lock (_requests)
                {
                    foreach (var r in _requests)
                    {
                        if (r.Is(requestId))
                            return r.Abort();
                    }
                    return false;
                }
            }

            public int RQStat(long requestId)
            {
                lock (_requests)
                {
                    foreach (var r in _requests)
                    {
                        if (r.Is(requestId))
                            return r.Stat();
                    }
                    return 0;
                }
            }

            public Action<Action<string, Func<string, object>>> ProvideArgumentParserFor(string publicName)
            {
                return _parent._applicationProvider().AllPrograms.ProvideArgumentParserFor(publicName);
            }
        }
    }

}
