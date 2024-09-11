using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using ENV.Data;
using ENV.Remoting;
using ENV.UI;
using Firefly.Box;
using Firefly.Box.Data.Storage;
using Firefly.Box.Flow;

namespace ENV
{
    public class ProgramCollection
    {
        Dictionary<string, int> _taskIndexesByType = new Dictionary<string, int>();

        Dictionary<int, ControllerInlist> _tasks = new Dictionary<int, ControllerInlist>();


        Dictionary<string, ControllerInlist> _tasksByName = new Dictionary<string, ControllerInlist>(),
                                         _tasksByPublicNames = new Dictionary<string, ControllerInlist>();

        internal class ControllerInlist
        {
            Func<object> _createInstance;
            bool _async;
            int _index;
            string _name, _publicName;
            string _typeFullName;
            Type _type;
            public ControllerInlist(int index, string name, string publicName, string typeFullName, Func<object> createInstance, bool async, Type type, bool webEndPoint)
            {
                _index = index;
                _name = name;
                _publicName = publicName;
                _typeFullName = typeFullName;
                _createInstance = createInstance;
                _async = async;
                _type = type;
                WebEndPoint = webEndPoint;
            }
            public bool WebEndPoint { get; private set; }

            internal object CreateInstance()
            {
                return _createInstance();
            }

            internal void SendTo(ProgramsConsumer x)
            {
                x(_index, _name, _publicName, _createInstance, _typeFullName);
            }

            internal Type GetTheType()
            {
                if (_type == null)
                    _type = CreateInstance().GetType();
                return _type;

            }

            internal AbstractUIController CreateInstanceAbstractUIController()
            {
                return CreateInstance() as AbstractUIController;
            }

            internal object CreateInstanceForRun()
            {
                if (_async)
                {

                    var s = _type.FullName;
                    if (_type.IsInterface)
                        s = s.Remove(s.LastIndexOf(".I") + 1, 1);
                    s += "Async";
                    var t = _type.Assembly.GetType(s);
                    return System.Activator.CreateInstance(t);

                }
                return CreateInstance();

            }
        }


        Dictionary<string, int> _taskIndexByName = new Dictionary<string, int>(),
                                _taskIndexByPublicName = new Dictionary<string, int>();

        public void Add(string publicName, bool webEndPoint, Type taskType)
        {
            Add(0, taskType.Name, publicName, webEndPoint, taskType);
        }
        internal void Add(string publicName, Type taskType)
        {
            Add(0, taskType.Name, publicName, true, taskType);
        }


        public object GetProgram(int taskIndex)
        {
            ControllerInlist t;
            if (_tasks.TryGetValue(taskIndex, out t))
            {
                return t.CreateInstance();
            }
            return null;
        }

        public void Add(Type taskType)
        {
            Add(0, taskType.Name, string.Empty, taskType);
        }
        public void Add(string publicName, string assemblyName, string typeFullName)
        {
            Add(0, publicName, publicName, assemblyName, typeFullName);
        }
        public void Add(int index, string name, string publicName, string typeFullName, bool async = false)
        {
            Add(index, name, publicName, DefaultWebEndPoint, typeFullName, async);
        }
        public void Add(int index, string name, string publicName, bool webEndPoint, string typeFullName, bool async = false)
        {
            string ns = null;
            Add(index, name, publicName, webEndPoint,
                () =>
                {
                    if (ns == null)
                    {
                        ns = typeFullName;
                        ns = ns.Remove(ns.LastIndexOf('.'));
                        var appProjectName = GetType().Assembly.GetName().Name;
                        {

                            if (ns.StartsWith(appProjectName) && ns != appProjectName)
                            {
                                var i = ns.IndexOf('.', appProjectName.Length + 1);
                                if (i > 0)
                                    ns = ns.Remove(i);
                            }
                        }
                    }
                    return AbstractFactory.CreateInstance(ns, typeFullName);
                }, typeFullName, async, null);
        }
        public void Add(int index, string name, string publicName, string assemblyName, string typeFullName, bool async = false)
        {
            Add(index, name, publicName, DefaultWebEndPoint, assemblyName, typeFullName, async);
        }
        public void Add(int index, string name, string publicName, bool webEndPoint, string assemblyName, string typeFullName, bool async = false)
        {
            Add(index, name, publicName, webEndPoint, () => AbstractFactory.CreateInstance(assemblyName, typeFullName), typeFullName, async, null);
        }
        public void Add(int index, string name, string publicName, Type taskType, bool async = false)
        {

            Add(index, name, publicName, DefaultWebEndPoint, taskType, async);
        }
        public void Add(int index, string name, string publicName, bool webEndPoint, Type taskType, bool async = false)
        {

            Add(index, name, publicName, webEndPoint, () =>
             {
                 if (taskType.IsInterface)
                     return AbstractFactory.CreateInstance(taskType);
                 if (taskType.IsAbstract)
                 {
                     var m = taskType.GetMethod("Create");
                     if (m != null)
                         return m.Invoke(null, new object[0]);
                 }
                 return Activator.CreateInstance(taskType);
             }, taskType.FullName, async, taskType);
        }


        List<Action<ProgramsConsumer>> _programs = new List<Action<ProgramsConsumer>>();
        public bool DefaultWebEndPoint = false;
        public void Add(int index, string name, Func<object> createTask, string typeFullName, bool async = false)
        {
            Add(index, name, "", DefaultWebEndPoint, createTask, typeFullName, async, null);
        }

        public void Add(int index, string name, string publicName, bool webEndPoint, Func<object> createTaskFactory, string typeFullName, bool async, Type type)
        {
            var createTask = new ControllerInlist(index, name, publicName, typeFullName, createTaskFactory, async, type, webEndPoint);
            if (!_taskIndexesByType.ContainsKey(typeFullName))
                _taskIndexesByType.Add(typeFullName, index);
            _programs.Add(x => createTask.SendTo(x));
            if (index != 0)
            {
                _tasks.Add(index, createTask);

                if (!String.IsNullOrEmpty(name) && !_taskIndexByName.ContainsKey(name))
                    _taskIndexByName.Add(name, index);
                if (!String.IsNullOrEmpty(publicName) && !_taskIndexByPublicName.ContainsKey(publicName))
                    _taskIndexByPublicName.Add(publicName, index);
            }
            if (name != String.Empty)
            {
                name = name.TrimEnd(' ');
                if (!_tasksByName.ContainsKey(name))
                    _tasksByName.Add(name, createTask);
            }
            if (publicName != null)
            {
                publicName = publicName.TrimEnd(' ');
                if (!_tasksByPublicNames.ContainsKey(publicName))
                    _tasksByPublicNames.Add(publicName, createTask);
            }

        }
        public int IndexOf(string publicName)
        {
            int result;
            if (_taskIndexByPublicName.TryGetValue(publicName.TrimEnd(' '), out result))
                return result;
            return 0;
        }


        public int IndexOfName(string name)
        {
            int result;
            if (_taskIndexByName.TryGetValue(name.TrimEnd(' '), out result))
                return result;
            return 0;
        }

        public Number GetProgramNumber(int programNumber, System.Type typeJustForCrossReference)
        {
            return programNumber;
        }

        public int IndexOf(System.Type t)
        {
            if (t.BaseType != null && t.BaseType.IsAbstract && t.BaseType.BaseType.Namespace != "ENV")
                t = t.BaseType;
            var i = t.GetInterfaces();
            foreach (var item in i)
            {
                if (item.Assembly != typeof(IHaveAMenu).Assembly)
                    t = item;
            }
            int result;

            if (_taskIndexesByType.TryGetValue(t.FullName, out result))
                return result;
            return 0;
        }
        internal delegate void ProgramsConsumer(int index, string name, string publicName, Func<object> factory, string typeFullName);
        internal void ProvideProgramsTo(ProgramsConsumer to)
        {
            foreach (var program in _programs)
            {
                program(to);
            }
        }

        static RequestLogger _logger = new ActionLogger(x => ErrorLog.WriteTrace(() => x));
        class DummyLogger : RequestLogger, RequestInfo
        {
            public RequestInfo ProcessRequest(string publicName)
            {
                return this;
            }

            public void Process(object runnedClass)
            {

            }

            public void Success()
            {

            }

            public void Fail(Exception e)
            {

            }

            public RequestInfo ProcessRequest(string publicName, long requestId)
            {
                return this;
            }
        }
        public static void LogToDebugOutput()
        {
            _logger = new ActionLogger(x => System.Diagnostics.Trace.WriteLine(x));
        }
        public static void LogToFile(string fileName)
        {
            _logger = new ActionLogger(x =>
            {
                var y = PathDecoder.DecodePath(fileName);
                lock (y)
                    using (var fw = new System.IO.StreamWriter(y, true))
                    {
                        fw.WriteLine(x);
                    }
                ErrorLog.WriteTrace(() => x);
            });
        }



        class ActionLogger : RequestLogger
        {
            Action<string> _writeTo;

            public ActionLogger(Action<string> writeTo)
            {
                _writeTo = writeTo;
            }

            public RequestInfo ProcessRequest(string publicName)
            {

                return new myRequest(this, publicName, HttpApplicationServer.RequestID.Value);

            }

            public RequestInfo ProcessRequest(string publicName, long requestId)
            {
                return new myRequest(this, publicName, requestId);
            }

            class myRequest : RequestInfo
            {
                ActionLogger _parent;
                string _publicName;
                string _className;
                long _requestId;
                DateTime _startTime;

                public myRequest(ActionLogger parent, string publicName, long requestId)
                {
                    _parent = parent;
                    _publicName = publicName;
                    _className = "Unknown Class";
                    _requestId = requestId;
                    ErrorLog.AdditionalInfo = string.Format("Request {0}({1})", requestId, publicName);


                }
                void Write(string what)
                {
                    _parent._writeTo(string.Format("{3} V:{4} - Request {0}({1}) - {2}", _requestId, _publicName,
                                                   what, DateTime.Now, typeof(Firefly.Box.Number).Assembly.GetName().Version));
                }

                public void Process(object runnedClass)
                {

                    _startTime = DateTime.Now;
                    _className = runnedClass.GetType().ToString();
                    string host = "Unknown";
                    if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null)
                        host = System.Web.HttpContext.Current.Request.UserHostAddress;
                    Write("From Host " + host + ", Begin Process, class " + _className);
                }

                public void Success()
                {
                    var duration = (DateTime.Now - _startTime).TotalMilliseconds;
                    Write("Success, Duration" +
                        (duration < 50 ? "" :
                        duration < 100 ? "." :
                        duration < 200 ? ".." :
                        duration < 500 ? "..." :
                        duration < 1000 ? "...." :
                        duration < 2000 ? "....." :
                        duration < 3000 ? "......" :
                        duration < 5000 ? "......." :
                        duration < 8000 ? "........" :
                                          ".........")
                        + ": " + duration);
                }

                public void Fail(Exception e)
                {
                    using (var sw = new System.IO.StringWriter())
                    {

                        var x = e;

                        while (x != null)
                        {
                            if (x != e)
                                sw.Write("Inner exception:");
                            sw.WriteLine(x.Message);
                            var sql = x.Data["SQL"] as string;
                            if (!string.IsNullOrEmpty(sql))
                            {
                                sw.Write("SQL:");
                                sw.WriteLine(sql);
                            }
                            var st = Common.GetShortStackTrace(x.StackTrace);
                            if (!string.IsNullOrEmpty(st))
                            {
                                sw.WriteLine("Stack Trace:");
                                sw.WriteLine(st);
                            }
                            x = x.InnerException;
                        }
                        string host = "Unknown";
                        if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null)
                            host = System.Web.HttpContext.Current.Request.UserHostAddress;
                        Write("Error from host: " + host + "\r\n " + sw.ToString());
                    }
                }
            }
        }
        interface RequestLogger
        {
            RequestInfo ProcessRequest(string publicName);
            RequestInfo ProcessRequest(string publicName, long requestId);
        }
        interface RequestInfo
        {
            void Process(object runnedClass);
            void Success();
            void Fail(Exception e);
        }

        internal static IDisposable StartProfilerForWeb(Func<string> requestID)
        {
            return ENV.Utilities.Profiler.StartContextAndSaveOnEnd(
                () =>
                {
                    return CollectRequestPArametersForProfiler();

                },
                    () =>
                    {
                        if (_startTime == DateTime.MinValue)
                            _startTime = DateTime.Now;


                        return _startTime.ToString("yyyyMMddHHmmss") + "_" + requestID();
                    });
        }

        public static string CollectRequestPArametersForProfiler()
        {
            using (var sw = new StringWriter())
            {
                var x = System.Web.HttpContext.Current;

                if (x != null)
                {
                    var c = x.Request;
                    var _memoryParams = new Dictionary<string, string>();

                    try
                    {
                        foreach (var item in c.Form.AllKeys)
                        {
                            if (item != null)
                                if (!_memoryParams.ContainsKey(item.ToUpper(CultureInfo.InvariantCulture)))
                                    _memoryParams.Add(item.ToUpper(CultureInfo.InvariantCulture),
                                        ParametersInMemory.Instance._outerValueProvide(item));
                        }
                        foreach (var item in c.Params.AllKeys)
                        {
                            if (item != null)
                                if (!_memoryParams.ContainsKey(item.ToUpper(CultureInfo.InvariantCulture)))
                                    _memoryParams.Add(item.ToUpper(CultureInfo.InvariantCulture),
                                        ParametersInMemory.Instance._outerValueProvide(item));
                        }
                        foreach (var item in c.Cookies.AllKeys)
                        {
                            if (item != null)
                                if (!_memoryParams.ContainsKey(item.ToUpper(CultureInfo.InvariantCulture)))
                                    _memoryParams.Add(item.ToUpper(CultureInfo.InvariantCulture),
                                        ParametersInMemory.Instance._outerValueProvide(item));
                        }
                        foreach (var mp in _memoryParams)
                        {
                            sw.WriteLine(mp.Key + ":" + mp.Value);
                        }
                    }
                    catch (Exception e)
                    {

                        sw.WriteLine(e.Message);
                    }

                }

                return "Request Parameters:\r\n" + sw.ToString();
            }
        }

        static DateTime _startTime = DateTime.MinValue;
        internal void RunWebProgram(string publicName, IWebParametersProvider args)
        {
            using (StartProfilerForWeb(() => HttpApplicationServer.RequestID.Value.ToString() + "_" + publicName))
            {
                var x = _logger.ProcessRequest(publicName);
                try
                {
                    if (publicName == null)
                    {
                        throw new NoProgramNameSpecifiedException();
                    }
                    if (publicName.Contains(","))
                        publicName = publicName.Split(',')[0];
                    ControllerInlist t;
                    if (_tasksByPublicNames.TryGetValue(publicName.ToString().TrimEnd(' '), out t))
                    {
                        if (!t.WebEndPoint)
                            throw new ControllerWebEndPointNotSet(publicName);

                        object o = t.CreateInstance();
                        x.Process(o);
#if DEBUG


#endif

                        MethodInfo runMethod = null;
                        foreach (MethodInfo method in o.GetType().GetMethods())
                        {
                            if (method.Name == "Run")
                            {
                                if (method.GetParameters().Length == args.Length)
                                {
                                    runMethod = method;
                                    break;
                                }
                                else
                                {
                                    int cParamLength = method.GetParameters().Length;
                                    int runPLength = runMethod == null ? -1 : runMethod.GetParameters().Length;
                                    if (cParamLength > args.Length &&
                                        (cParamLength < runPLength || runPLength < args.Length))
                                        runMethod = method;
                                }
                            }
                        }
                        if (runMethod != null)
                        {

                            List<object> result = new List<object>();
                            int i = 0;
                            foreach (ParameterInfo info in runMethod.GetParameters())
                            {
                                if (i >= args.Length)
                                    result.Add(null);
                                else
                                {
                                    System.Type pType = info.ParameterType;
                                    if (pType == typeof(TextParameter))
                                    {
                                        var z = args.GetString(i);
                                        if (z == "")
                                            z = null;
                                        result.Add(new TextParameter(z));
                                    }
                                    else if (pType == typeof(NumberParameter))
                                    {
                                        string text = args.GetString(i);
                                        result.Add(NumberParameter.FromWeb(text));

                                    }
                                    else if (pType == typeof(BoolParameter))
                                    {
                                        result.Add(BoolParameter.FromString(args.GetString(i)));
                                    }
                                    else if (pType == typeof(ByteArrayParameter))
                                    {
                                        var j = i;
                                        result.Add(
                                            new ByteArrayParameter(col => args.GetByteArray(j, (ByteArrayColumn)col), null));
                                    }
                                    else if (pType == typeof(DateParameter))
                                    {
                                        string text = args.GetString(i);
                                        if (text == "")
                                            result.Add((DateParameter)null);
                                        else
                                            result.Add(new DateParameter(text));
                                    }
                                    else if (pType == typeof(TimeParameter))
                                    {
                                        result.Add(
                                            new TimeParameter(args.GetString(i)));
                                    }
                                    else
                                        throw new Exception(String.Format("Type {0} is not yet supported", pType));
                                }
                                i++;
                            }
                            runMethod.Invoke(o, result.ToArray());
                        }
                        else
                        {
                            ControllerBase task = o as ControllerBase;
                            if (task != null)
                                task.ExecuteBasedOnWebArgs(args);

                        }
                    }
                    else
                    {
                        throw new ProgramNameNotFoundException(publicName);
                    }
                    x.Success();
                }
                catch (Exception e)
                {
                    x.Fail(e);
                    throw;
                }
            }
        }

        public class NoProgramNameSpecifiedException : InvalidOperationException
        {
            public NoProgramNameSpecifiedException()
                : base("No program name specified")
            {
            }
        }

        public class ProgramNameNotFoundException : InvalidOperationException
        {
            public ProgramNameNotFoundException(string name)
                : base("Program name not found - " + name)
            {
            }
        }
        public class ControllerWebEndPointNotSet : InvalidOperationException
        {
            public ControllerWebEndPointNotSet(string name)
                : base("Controller '" + name + "' WebEndPoint parameter was not set to True, Did you forget to set it in the call to the 'Add' method in the 'ApplicationPrograms' class?")
            {
            }
        }

        public object RunByIndex<T>(Number taskIndex, ArrayColumn<T> args)
        {
            return RunByIndex(taskIndex, new object[] { args });
        }
        public object RunByIndex(Number taskIndex, params object[] args)
        {
            var x = taskIndex % 1;
            if (x != 0)
            {
                var foundApps = new HashSet<ApplicationControllerBase>();
                var at = Firefly.Box.Context.Current.ActiveTasks;
                object result = null;
                for (int i = at.Count - 1; i >= 0; i--)
                {
                    bool done = false;

                    UIControllerBase.SendInstanceBasedOnTaskAndCallStack(at[i], y =>
                    {
                        if (!foundApps.Contains(y._application))
                        {
                            if (x == (Number)0.01 * foundApps.Count)
                            {
                                result = y._application.AllPrograms.InternalRunByIndex(taskIndex, args);
                                done = true;
                            }
                            else
                                foundApps.Add(y._application);
                        }

                    });
                    if (done)
                        break;
                }
                return result;

            }
            else return InternalRunByIndex(taskIndex, args);
        }
        internal object InternalRunByIndex(int taskIndex, params object[] args)
        {
            return GetCachedByIndex(() => taskIndex, args).Run(args);
        }
        internal Action<Action<string, Func<string, object>>> ProvideArgumentParserFor(string publicName)
        {
            ControllerInlist tFunc;
            if (_tasksByPublicNames.TryGetValue(publicName.ToString().TrimEnd(' '), out tFunc))
            {
                if (!tFunc.WebEndPoint)
                    throw new ControllerWebEndPointNotSet(publicName);
                var o = tFunc.CreateInstanceForRun() as ControllerBase;
                if (o != null)
                {
                    return (x) =>
                    {
                        o.ProvideArgumentParserTo(x);

                    };
                }
            }
            return (x) => { };
        }
        internal object InternalRunByPublicName(string publicName, object[] args, long requestId)
        {
            ControllerInlist tFunc;
            var x = _logger.ProcessRequest(publicName, requestId);
            try
            {
                if (_tasksByPublicNames.TryGetValue(publicName.ToString().TrimEnd(' '), out tFunc))
                {
                    if (!tFunc.WebEndPoint)
                        throw new ControllerWebEndPointNotSet(publicName);


                    object o = tFunc.CreateInstanceForRun();
                    x.Process(o);
                    var t = o.GetType();

                    if (t.Name.EndsWith("Core"))
                        t = t.BaseType;
                    var m = t.GetMethod("RunAsync");
                    if (m != null)
                    {
                        return RunTask(null, args, m, t);
                    }
                    else
                        return RunTask(o, args, null, t);
                    x.Success();
                    return o;
                }
                else
                {
                    throw new ProgramNotFoundException(publicName);
                }
            }
            catch (Exception ex)
            {
                x.Fail(ex);
                throw;
            }
        }
        public class ProgramNotFoundException : Exception
        {
            public ProgramNotFoundException(string programName)
                : base("Program not found, public name " + programName)
            {
            }
        }


        public object RunByPublicName(string publicName, params object[] args)
        {
            return GetCachedByPublicName(() => publicName, args).Run(args);
        }
        public object RunByPublicName<T>(string publicName, ArrayColumn<T> args)
        {
            return RunByPublicName(publicName, new object[] { args });
        }

        public class StringOrByteArrayArg
        {
            string _arg;

            public StringOrByteArrayArg(string arg)
            {
                _arg = arg;
            }

            public TextParameter GetTextParameter()
            {
                return new TextParameter(_arg);
            }

            static ByteArrayColumn _ansi = new ByteArrayColumn() { ContentType = ByteArrayColumnContentType.Ansi };
            public ByteArrayParameter GetByteArrayParameter()
            {
                if (_col != null)
                    return _col;
                return new ByteArrayParameter(_ansi.FromString(_arg));
            }

            ByteArrayColumn _col;
            public void SetColumn(ByteArrayColumn column)
            {
                column.Value = column.FromString(_arg);
                _col = column;

            }
        }

        internal static object RunTask(object taskObject, object[] args, MethodInfo runMethod, Type controllerType)
        {

            bool hasForm = false;
            if (args == null)
                args = new object[] { null };
            if (args.Length > 0 && args[args.Length - 1] is Firefly.Box.UI.Form)
                hasForm = true;
            if (runMethod == null)
            {
                foreach (MethodInfo method in taskObject.GetType().GetMethods())
                {
                    if (method.Name == "Run")
                    {
                        var pars = method.GetParameters();

                        if (pars.Length == 0 && args.Length == 0)
                        {
                            runMethod = method;
                            break;
                        }
                        else
                        {
                            if (pars.Length >= args.Length &&
                                (runMethod == null || pars.Length < runMethod.GetParameters().Length) &&
                                (!hasForm || pars[pars.Length - 1].ParameterType == typeof(Firefly.Box.UI.Form)))
                                runMethod = method;
                        }

                    }

                }
            }
            var paramBuilder = new ParameterHelper();
            if (runMethod != null)
            {
                List<object> parameters = new List<object>();
                int i = -1;
                Firefly.Box.UI.Form form = null;
                foreach (ParameterInfo info in runMethod.GetParameters())
                {
                    i++;

                    if (i >= args.Length)
                    {
                        if (info.ParameterType == typeof(Firefly.Box.UI.Form) && form != null)
                            parameters.Add(form);
                        else
                            parameters.Add(null);
                        continue;
                    }
                    form = args[i] as Firefly.Box.UI.Form;
                    {
                        if (info.ParameterType == typeof(NumberParameter))
                        {
                            Firefly.Box.Data.NumberColumn p = args[i] as Firefly.Box.Data.NumberColumn;
                            if (!ReferenceEquals(p, null))
                            {
                                parameters.Add((NumberParameter)p);
                                continue;
                            }

                            parameters.Add(new NumberParameter(new ObjectParameterBridge<Number>(args[i], Number.TryCast)));
                            continue;

                        }
                        if (info.ParameterType == typeof(TextParameter))
                        {
                            Firefly.Box.Data.TextColumn p = args[i] as Firefly.Box.Data.TextColumn;
                            if (p != null)
                            {
                                parameters.Add((TextParameter)p);
                                continue;
                            }

                            var z = args[i] as StringOrByteArrayArg;
                            if (z != null)
                            {
                                parameters.Add(z.GetTextParameter());
                                continue;
                            }
                            {
                                var x = args[i] as byte[];
                                if (x != null)
                                {
                                    parameters.Add((TextParameter)x);
                                    continue;
                                }

                            }
                            {
                                var x = args[i] as ByteArrayColumn;
                                if (x != null)
                                {
                                    parameters.Add((TextParameter)x);
                                    continue;
                                }

                            }

                            parameters.Add(new TextParameter(new ObjectParameterBridge<Text>(args[i], Text.TryCast)));
                            continue;

                        }
                        if (info.ParameterType == typeof(BoolParameter))
                        {
                            Firefly.Box.Data.BoolColumn p = args[i] as Firefly.Box.Data.BoolColumn;
                            if (p != null)
                            {
                                parameters.Add((BoolParameter)p);
                                continue;
                            }

                            parameters.Add(new BoolParameter(new ObjectParameterBridge<Bool>(args[i], Bool.TryCast)));
                            continue;

                        }
                        if (info.ParameterType == typeof(TimeParameter))
                        {
                            Firefly.Box.Data.TimeColumn p = args[i] as Firefly.Box.Data.TimeColumn;
                            if (!ReferenceEquals(p, null))
                            {
                                parameters.Add((TimeParameter)p);
                                continue;
                            }
                            parameters.Add(new TimeParameter(new ObjectParameterBridge<Time>(args[i], Time.TryCast)));
                            continue;

                        }
                        if (info.ParameterType == typeof(DateParameter))
                        {
                            Firefly.Box.Data.DateColumn p = args[i] as Firefly.Box.Data.DateColumn;
                            if (!ReferenceEquals(p, null))
                            {
                                parameters.Add((DateParameter)p);
                                continue;
                            }
                            parameters.Add(new DateParameter(new ObjectParameterBridge<Date>(args[i], Date.TryCast)));
                            continue;

                        }
                        if (info.ParameterType == typeof(ByteArrayParameter))
                        {
                            Firefly.Box.Data.ByteArrayColumn p = args[i] as Firefly.Box.Data.ByteArrayColumn;
                            if (!ReferenceEquals(p, null))
                            {
                                parameters.Add((ByteArrayParameter)p);
                                continue;
                            }
                            var z = args[i] as StringOrByteArrayArg;
                            if (z != null)
                            {
                                parameters.Add(z.GetByteArrayParameter());
                                continue;
                            }
                            var r = args[i] as Firefly.Box.Advanced.ICanBeTranslatedToByteArray;
                            if (r != null)
                            {
                                var col = (Firefly.Box.Data.Advanced.ColumnBase)args[i];
                                parameters.Add(new ByteArrayParameter(x => r.ToByteArray(), (c, v) => col.Value = r.FromByteArray(v), col));
                                continue;
                            }
                            parameters.Add(new ByteArrayParameter(args[i] as byte[]));
                            continue;

                        }
                        if (info.ParameterType == typeof(object))
                        {
                            parameters.Add(args[i]);
                            continue;
                        }
                        if (args[i] != null && info.ParameterType.IsAssignableFrom(args[i].GetType()))
                        {
                            parameters.Add(args[i]);
                            continue;
                        }

                        if (info.ParameterType == typeof(ArrayParameter<Text>))
                        {
                            parameters.Add(paramBuilder.CreateTextArrayParameter(args[i]));
                            continue;
                        }
                        if (info.ParameterType == typeof(ArrayParameter<Number>))
                        {
                            var p = args[i] as ArrayColumn<Number>;
                            if (!ReferenceEquals(p, null))
                            {
                                parameters.Add(new ArrayParameter<Number>(p));
                                continue;
                            }

                        }
                        if (info.ParameterType == typeof(ArrayParameter<Text[]>))
                        {
                            var p = args[i] as ArrayColumn<Text[]>;
                            if (!ReferenceEquals(p, null))
                            {
                                parameters.Add(new ArrayParameter<Text[]>(p));
                                continue;
                            }
                            {
                                var zz = args[i] as Text[][];
                                if (zz != null)
                                {
                                    parameters.Add(new ArrayParameter<Text[]>(zz));
                                    continue;
                                }
                            }
                            {
                                var zz = args[i] as Text[];
                                if (zz != null)
                                {
                                    parameters.Add(new ArrayParameter<Text[]>(new Text[][] { zz }));
                                    continue;
                                }
                                parameters.Add(null);
                                continue;
                            }

                        }
                        if (info.ParameterType == typeof(ArrayParameter<byte[]>))
                        {
                            var p = args[i] as ArrayColumn<byte[]>;
                            if (!ReferenceEquals(p, null))
                            {
                                parameters.Add(new ArrayParameter<byte[]>(p));
                                continue;
                            }
                            var z = args[i] as byte[][];
                            if (z != null)
                            {
                                parameters.Add(new ArrayParameter<byte[]>(z));
                                continue;
                            }
                            var zz = args[i] as byte[];
                            if (zz != null)
                            {
                                parameters.Add(new ArrayParameter<byte[]>(new byte[][] { zz }));
                                continue;
                            }
                            parameters.Add(null);
                            continue;

                        }
                        if (info.ParameterType == typeof(ArrayParameter<Text[][]>))
                        {
                            var p = args[i] as ArrayColumn<Text[][]>;
                            if (!ReferenceEquals(p, null))
                            {
                                parameters.Add(new ArrayParameter<Text[][]>(p));
                                continue;
                            }

                            var zz = args[i] as Text[][];
                            if (zz != null)
                            {
                                parameters.Add(new ArrayParameter<Text[]>(zz));
                                continue;
                            }

                        }
                        if (info.ParameterType.IsGenericType && info.ParameterType.GetGenericTypeDefinition() == typeof(DotnetParameter<>))
                        {
                            if (args[i] != null && args[i].GetType().IsGenericType && args[i].GetType().GetGenericTypeDefinition() == typeof(DotnetColumn<>))
                            {
                                var pType = info.ParameterType;
                                object dotnetParam = CreateDotnetParameter(args, i, pType);
                                parameters.Add(dotnetParam);
                                continue;
                            }
                            parameters.Add(null);
                        }
                        if (info.ParameterType.IsGenericType && info.ParameterType.GetGenericTypeDefinition() == typeof(ComParameter<>))
                        {
                            if (args[i] != null && args[i].GetType().IsGenericType && args[i].GetType().GetGenericTypeDefinition() == typeof(ComColumn<>))
                            {
                                var pType = info.ParameterType;
                                object dotnetParam = CreateComParameter(args, i, pType);
                                parameters.Add(dotnetParam);
                                continue;
                            }
                            parameters.Add(null);
                        }
                        if (info.ParameterType.IsGenericType && info.ParameterType.GetGenericTypeDefinition() == typeof(ActiveXParameter<>))
                        {
                            var baseType = args[i].GetType().BaseType;
                            if (args[i] != null && ((args[i].GetType().IsGenericType && args[i].GetType().GetGenericTypeDefinition() == typeof(ActiveXColumn<>))||(
                                baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(ActiveXColumn<>))))
                            {
                                var pType = info.ParameterType;
                                object dotnetParam = CreateActiveXParameter(args, i, pType);
                                parameters.Add(dotnetParam);
                                continue;
                            }
                            parameters.Add(null);
                        }

                    }
                }

                try
                {
                    var result = runMethod.Invoke(taskObject, parameters.ToArray());
                    if (runMethod.ReturnType == typeof(void))
                        return true;
                    return result;
                }
                catch (TargetInvocationException e)
                {
                    if (e.InnerException != null)
                        throw e.InnerException;
                    throw;
                }
            }

            ControllerBase task = taskObject as ControllerBase;
            if (task != null)
            {
                return task.ExecuteBasedOnArgs(args);

            }
            else if (taskObject is AsyncHelperBase)
            {
                return ((AsyncHelperBase)taskObject).ExecuteBasedOnArgs(args, controllerType);
            }
            else
                throw new System.Exception("Failed to run task " + taskObject.GetType());
            return null;
        }



        internal static object CreateDotnetParameter(object[] args, int i, Type pType)
        {
            var x = args[i].GetType();
            if (!x.IsGenericType)
                x = x.BaseType;
            var p = x.GetGenericArguments()[0];
            var m = pType.GetMethod(nameof(DotnetParameter<object>.Cast), BindingFlags.Static | BindingFlags.Public);
            var gm = m.MakeGenericMethod(p);
            var dotnetParam = gm.Invoke(null, new object[] { args[i] });
            return dotnetParam;
        }
        internal static object CreateComParameter(object[] args, int i, Type pType)
        {
            var x = args[i].GetType();
            if (!x.IsGenericType)
                x = x.BaseType;
            var p = x.GetGenericArguments()[0];
            var m = pType.GetMethod(nameof(ComParameter<object>.Cast), BindingFlags.Static | BindingFlags.Public);
            var gm = m.MakeGenericMethod(p);
            var dotnetParam = gm.Invoke(null, new object[] { args[i] });
            return dotnetParam;
        }
        internal static object CreateActiveXParameter(object[] args, int i, Type pType)
        {
            var x = args[i].GetType();
            if (!x.IsGenericType)
                x = x.BaseType;
            var p = x.GetGenericArguments()[0];
            var m = pType.GetMethod("Cast", BindingFlags.Static | BindingFlags.Public);
            var gm = m.MakeGenericMethod(p);
            var dotnetParam = gm.Invoke(null, new object[] { args[i] });
            return dotnetParam;
        }

        internal string GetPublicNameOf(ControllerBase task)
        {
            var i = IndexOf(task.GetType());
            if (i > 0)
                foreach (var tasksByPublicName in _taskIndexByPublicName)
                {
                    if (tasksByPublicName.Value == i)
                        return tasksByPublicName.Key;
                }
            return string.Empty;
        }

        Dictionary<int, Type> _controllerTypesByIndex = new Dictionary<int, Type>();
        Dictionary<string, Type> _controllerTypesByPublicName = new Dictionary<string, Type>();
        internal MyCachedController GetCachedByIndex(Func<Number> programNumber, object[] args)
        {
            return new MyCachedController(() =>
            {
                Type result;
                if (!_controllerTypesByIndex.TryGetValue(programNumber(), out result))
                {
                    lock (_controllerTypesByIndex)
                    {
                        if (!_controllerTypesByIndex.TryGetValue(programNumber(),
                                out result))
                        {
                            ControllerInlist fact;
                            if (_tasks.TryGetValue(programNumber(), out fact))
                            {
                                result = fact.GetTheType();
                            }
                            else
                            {
                                result = typeof(DummyBP);
                            }
                            _controllerTypesByIndex.Add(programNumber(), result);
                        }

                    }
                }
                return result;
            }, () =>
            {
                ControllerInlist result;
                if (_tasks.TryGetValue(programNumber(), out result))
                {
                    return result.CreateInstanceForRun();
                }
                return new DummyBP();
            }, args);
        }
        internal void RunOnSubformByIndex(SubForm subform, Number programNumber, object[] args)
        {
            ControllerInlist result;
            if (_tasks.TryGetValue(programNumber, out result))
            {
                var uic = result.CreateInstanceAbstractUIController();
                if (uic != null)
                {
                    subform.Clear();
                    uic = result.CreateInstanceAbstractUIController();
                    if (uic != null)
                        subform.SetController(uic, () => RunTask(uic, args, null, uic.GetType()));
                }
            }
        }
        internal void RunOnSubformByPublicName(SubForm subform, Text publicName, object[] args)
        {

            ControllerInlist result;
            if (_tasksByPublicNames.TryGetValue(publicName.ToString().TrimEnd(' '), out result))
            {
                var uic = result.CreateInstanceAbstractUIController();
                if (uic != null)
                {
                    subform.Clear();
                    uic = result.CreateInstanceAbstractUIController();
                    if (uic != null)
                        subform.SetController(uic, () => RunTask(uic, args, null, uic.GetType()));
                }
            }
        }
        internal MyCachedController GetCachedByPublicName(Func<Text> publicName, object[] args)
        {
            return new MyCachedController(() =>
            {
                Type result = typeof(DummyBP);
                var s = publicName().Trim();
                if (!string.IsNullOrEmpty(s))
                    if (!_controllerTypesByPublicName.TryGetValue(s, out result))
                    {
                        lock (_controllerTypesByPublicName)
                        {
                            if (!_controllerTypesByPublicName.TryGetValue(s,
                                    out result))
                            {
                                try
                                {
                                    ControllerInlist fact;
                                    if (_tasksByPublicNames.TryGetValue(s, out fact))
                                    {
                                        result = fact.GetTheType();
                                    }
                                    else
                                    {
                                        throw new Exception(LocalizationInfo.Current.ControllerNotFound + s);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    result = typeof(DummyBP);
                                    ErrorLog.WriteToLogFile(ex);
                                    ENV.Message.ShowWarningInStatusBar(ex.Message);
                                }
                                _controllerTypesByPublicName.Add(s, result);
                            }

                        }
                    }
                return result;
            }, () =>
            {
                ControllerInlist result;
                var s = publicName().Trim();
                if (!string.IsNullOrEmpty(s))
                    if (_tasksByPublicNames.TryGetValue(s, out result))
                    {
                        return result.CreateInstanceForRun();
                    }
                return new DummyBP();
            }, args);
        }

        class DummyBP : BusinessProcessBase
        {
            public DummyBP()
            {
                Exit();
            }
            protected override void OnLeaveRow()
            {
                Exit();
                base.OnLeaveRow();
            }
            public bool Run()
            {
                Exit();
                Execute();
                return false;
            }
        }

        internal class MyCachedController : CachedTask<object>, Runnable
        {
            Func<Type> _getType;
            Firefly.Box.UI.Form _form;
            public MyCachedController(Func<Type> getType, Func<object> getInstance, object[] args)
                : base(
                    () => getInstance(), y =>
                    {
                        var x = y as AbstractUIController;
                        if (x != null)
                            return x._uiController;
                        var z = y as BusinessProcessBase;
                        if (z != null)
                            return z._businessProcess;
                        return null;

                    }, () => getType())
            {
                _getType = getType;
                if (args != null)
                    foreach (var item in args)
                    {
                        var f = item as Firefly.Box.UI.Form;
                        if (f != null)
                            _form = f;
                    }
            }
            protected override object GetUniqueIdentifier()
            {
                var r = base.GetUniqueIdentifier();
                if (_form == null)
                    return r;
                return r.ToString() + _form.GetType().ToString();
            }
            public object Run<T>(ArrayColumn<T> args)
            {
                return Run(new object[] { args });
            }
            internal Type GetTheType()
            {
                return _getType();
            }
            public object Run(params object[] args)
            {

                var t = _getType();
                if (t.Name.EndsWith("Core"))
                    t = t.BaseType;
                var m = t.GetMethod("RunAsync");
                if (m != null)
                {
                    return RunTask(Instance, args, m, t);
                }
                else
                    return RunTask(Instance, args, null, t);

            }

            protected override bool IsForKeepViewVisibleAfterExitOnly()
            {
                return true;
            }
        }

        public interface Runnable
        {
            object Run(params object[] args);
            object Run<T>(ArrayColumn<T> args);
        }
    }
    public class MenuCollection
    {
        Dictionary<string, int> _byName = new Dictionary<string, int>(), _byPublic = new Dictionary<string, int>();
        public void Add(int id, string name, string publicName)
        {
            Add(_byName, id, name);
            Add(_byPublic, id, publicName);
        }

        private void Add(Dictionary<string, int> dict, int id, string name)
        {
            if (dict.ContainsKey(name))
                return;
            dict.Add(name, id);
        }
    }
    class ParameterHelper
    {
        public ArrayParameter<Text> CreateTextArrayParameter(object arg)
        {
            {
                var p = arg as ArrayColumn<Text>;
                if (!ReferenceEquals(p, null))
                {
                    return new ArrayParameter<Text>(p);

                }
                var pa = arg as Text[];
                if (pa != null)
                {
                    return new ArrayParameter<Text>(pa);

                }
                return null;

            }
        }
    }
}
