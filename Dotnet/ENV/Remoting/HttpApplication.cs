using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ENV.BackwardCompatible;
using Firefly.Box;
using Firefly.Box.Data;

namespace ENV.Remoting
{
    public class HttpApplication
    {
        HttpRemotingProvider<IRemoteApplication> _server;
        string _url;
        public HttpApplication(string url)
        {
            _url = url;
            _server = new HttpRemotingProvider<IRemoteApplication>(_url);
        }
        public NumberColumn ReturnCodeColumn { get; set; }
        public NumberColumn ErrorCodeColumn { get; set; }
        public TextColumn MessageIDColumn { get; set; }
        public Text ResultFileName { get; set; }
        bool _successful;
        public bool Successful
        {
            get { return _successful; }
        }

        byte[] _resultStream;
        public byte[] ResultStream
        {
            get { return _resultStream; }
        }

        long _messageId;
        public long MessageId
        {
            get { return _messageId; }
        }

        string _exceptionText;
        bool _writeToConsole = false;
        bool _throw = false;
        public string ExceptionText { get { return _exceptionText; } }
        public object Run(string publicName, params object[] args)
        {
            return InternalRun(publicName, false, args);
        }
        public static int RunCmdl()
        {
            try
            {
                var u = ENV.UserMethods.Instance;

                bool wait = true;
                if (u.IniGet("WAIT").ToUpper() == "N")
                    wait = false;
                string serviceName = u.IniGet("SERVICE");
                string url = u.IniGet("URL");
                if (String.IsNullOrEmpty(url))
                    url = UserSettings.GetApplicationServerUrl(serviceName);
                string prgName = u.IniGet("PRGNAME");
                string arguments = u.IniGet("ARGUMENTS");
                var argumentsArray = ENV.Remoting.HttpApplication.ParseArguments(arguments);

                var ha = new HttpApplication(url);
                ha._writeToConsole = true;
                ha._throw = true;

                if (wait)
                    ha.Run(prgName, argumentsArray);
                else
                    ha.RunAsync(prgName, argumentsArray);
                return 0;
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "Cmdl");
                Console.WriteLine("Failed: " + e.Message);
                return -1;
            }
        }

        public static object[] ParseArguments(string argumentsString)
        {
            if (string.IsNullOrEmpty(argumentsString))
                return new object[0];
            var result = new List<object>();
            foreach (var s in argumentsString.Split(','))
            {
                var modifier = s.Substring(0, 2);
                var value = s.Substring(2);
                switch (modifier)
                {
                    case "-A":
                        result.Add(new TextColumn { Value = value.ToString() });
                        break;
                    case "-L":
                        result.Add(new BoolColumn { Value = value.Trim().ToUpper().StartsWith("T") });
                        break;
                    case "-N":
                        result.Add(new NumberColumn { Value = Number.Parse(value) });
                        break;
                    default:
                        throw new Exception("Unknown arguments modifier");
                }
            }
            return result.ToArray();
        }

        public object InternalRun(string publicName, bool async, object[] args)
        {
            try
            {
                var start = DateTime.Now;
                var x = new ClientParameterManager();
                var result = (HttpApplicationServer.RequestResult)_server.ExecuteOnServer(new MyRequest(publicName, args, x, async));
                ApplyResult(result);
                if (result.Successful)
                {
                    if (!async)
                    {
                        var r = (ServerParameterByRefResultInformation)result.ResultData;
                        r.ApplyTo(x);
                        if (_writeToConsole)
                        {
                            if (result.ResultStream.Length > 0)
                            {
                            Console.WriteLine("Result :");
                            Console.WriteLine("========");
                            Console.WriteLine(System.Text.Encoding.Default.GetString(result.ResultStream));
                            }
                            x.ReportColumnResultToConsole();

                        }
                    }
                    if (_writeToConsole)
                    {
                        Console.WriteLine("Reqid {0} : Elapsed execution time : {1}", result.MessageId,
                                          DateTime.Now - start);
                    }
                    if (async)
                        return true;
                    else
                        return x._returnValue;


                }
                else
                {
                    throw new Exception(result.ExceptionMessage);
                }

            }
            catch (Exception e)
            {
                _successful = false;
                _exceptionText = e.Message;
                if (ErrorCodeColumn != null)
                    ErrorCodeColumn.Value = 9999;
                if (ReturnCodeColumn != null)
                    ReturnCodeColumn.Value = 9999;
                ErrorLog.WriteToLogFile(e, "Remote Request Failed For Public Name - " + publicName + ". URL - " + _url);
                if (_writeToConsole)
                    throw;
                return false;
            }
            return null;
        }

        T RunFunction<T>(RemoteCommand<IRemoteApplication> command, T errorValue)
        {
            try
            {
                var result = _server.ExecuteOnServer(command) as HttpApplicationServer.RequestResult;
                return (T)result.ResultData;

            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "Remote Request Failed For " + command.GetType() + " Function. URL - " + _url);
                return errorValue;
            }
        }

        public Text GetRqRtInf(Number engineNumber)
        {
            return RunFunction(new GetRqRtInfClass(engineNumber), string.Empty);

        }
        public Number GetRqRts()
        {
            return RunFunction(new RqRtsClass(), 0);

        }
        public HttpApplicationServer.RequestInfo[] GetRequests(long fromId, long toId, bool pendingOnly)
        {
            return RunFunction(new GetRequestsClass(fromId, toId, pendingOnly), new HttpApplicationServer.RequestInfo[0]);
        }
        public string GetRqLoad()
        {
            return RunFunction(new GetRqLoadClass(), string.Empty);
        }
        public bool AbortRequest(long requestId)
        {
            return RunFunction(new AbortRequestClass(requestId), false);
        }
        [Serializable]
        class GetRqRtAppClass : InternalServerCommand
        {
            

            public override object Execute(IRemoteApplication param)
            {
                return (ENV.UserMethods.Instance.IniGet("ApplicationPublicName")??ENV.UserMethods.Instance.Sys()).ToString();
            }

        }
        [Serializable]

        internal abstract class InternalServerCommand : RemoteCommandBase
        {
            
        }
        internal Text RqRtApp()
        {
            return RunFunction(new GetRqRtAppClass(), string.Empty);
        }
        public Number RQStat(long requestId)
        {
            return RunFunction(new RQStatClass(requestId), 0);
        }
        [Serializable]
        class RQStatClass : InternalServerCommand
        {
            long _requestId;

            public RQStatClass(long requestId)
            {
                _requestId = requestId;
            }

            public override object Execute(IRemoteApplication param)
            {
                return param.RQStat(_requestId);
            }

        }
        [Serializable]
        class AbortRequestClass : InternalServerCommand
        {
            long _requestId;

            public AbortRequestClass(long requestId)
            {
                _requestId = requestId;
            }

            public override object Execute(IRemoteApplication param)
            {
                return param.AbortRequest(_requestId);
            }

        }

        public  interface HasToDoWithRequest
        {
            void ApplyTo(HttpApplicationServer.RequestInfo info);
        }

        [Serializable]
        public abstract class RemoteCommandBase :HasToDoWithRequest, RemoteCommand<IRemoteApplication>
        {
            string _hostName;
            public RemoteCommandBase()
            {
                _hostName = System.Environment.MachineName;
            }

            public abstract object Execute(IRemoteApplication param);


            public virtual bool Async
            {
                get { return false; }
            }

            public virtual string GetOperation()
            {
                return GetType().Name.Replace("Class", "");
            }

             void HasToDoWithRequest.ApplyTo(HttpApplicationServer.RequestInfo info)
            {
                info.Operation = GetOperation();
                info.SetHostName(_hostName);
            }
        }

        [Serializable]
        class GetRqLoadClass : InternalServerCommand
        {
            public override object Execute(IRemoteApplication param)
            {
                return param.GetRqLoad();
            }

        }

        [Serializable]
        class GetRequestsClass : InternalServerCommand
        {
            long _from, _to;
            bool _pendingOnly;

            public GetRequestsClass(long @from, long to, bool pendingOnly)
            {
                _from = from;
                _to = to;
                _pendingOnly = pendingOnly;
            }

            public override object Execute(IRemoteApplication param)
            {
                return param.GetRequests(_from, _to, _pendingOnly);
            }

        }
        [Serializable]
        class GetRqRtInfClass : InternalServerCommand
        {
            int _engineNumber;

            public GetRqRtInfClass(int engineNumber)
            {
                _engineNumber = engineNumber;
            }

            public override object Execute(IRemoteApplication param)
            {
                return param.GetRqRtInf(_engineNumber);
            }

        }
        [Serializable]
        class RqRtsClass : InternalServerCommand
        {
            public override object Execute(IRemoteApplication param)
            {
                return param.GetRqRts();
            }

        }

        void ApplyResult(HttpApplicationServer.RequestResult result)
        {
            _messageId = result.MessageId;
            if (MessageIDColumn != null)
                MessageIDColumn.Value = result.MessageId.ToString();
            if (result.Successful)
            {
                _successful = true;
                _resultStream = result.ResultStream;
                if (ResultFileName != null && ResultStream != null)
                    System.IO.File.WriteAllBytes(PathDecoder.DecodePath(ResultFileName), _resultStream);
                if (ErrorCodeColumn != null)
                    ErrorCodeColumn.Value = 0;
                if (ReturnCodeColumn != null)
                    ReturnCodeColumn.Value = 0;
            }
            else
            {
                _successful = false;
                _exceptionText = result.ExceptionMessage;
                if (ErrorCodeColumn != null)
                    ErrorCodeColumn.Value = 9999;
                if (ReturnCodeColumn != null)
                    ReturnCodeColumn.Value = 9999;
            }
        }

        public object RunAsync(string publicName, params object[] args)
        {
            return InternalRun(publicName, true, args);
            

        }

        [Serializable]
        class MyRequest : RemoteCommandBase
        {
            string _publicName;
            object[] _args;
            bool _async;
            public override bool Async
            {
                get { return _async; }
            }

            public override string GetOperation()
            {
                return _publicName;
            }

            public MyRequest(string publicName, object[] args, ClientParameterManager m, bool async)
            {
                _async = async;
                _publicName = publicName;
                _args = new object[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    _args[i] = m.Pack(args[i]);
                }
            }



            public override object Execute(IRemoteApplication param)
            {
                var x = new ServerParameterManager();
                var args = new object[_args.Length];
                for (int i = 0; i < _args.Length; i++)
                {
                    args[i] = x.UnPack(_args[i]);
                }
                object ret = param.Run(_publicName, args);
                return x.CreateResult(ret);
                
            }
        }



    }
    public interface IRemoteApplication
    {
        object Run(string publicName, params object[] args);
        int GetRqRts();
        string GetRqRtInf(int engineNumber);
        HttpApplicationServer.RequestInfo[] GetRequests(long @from, long to, bool pendingOnly);
        string GetRqLoad();
        bool AbortRequest(long requestId);
        int RQStat(long requestId);
        Action<Action<string, Func<string, object>>> ProvideArgumentParserFor(string publicName);
    }

}
