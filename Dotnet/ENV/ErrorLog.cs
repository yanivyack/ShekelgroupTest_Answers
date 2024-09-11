using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.UnderConstruction;

namespace ENV
{
    public class ErrorLog
    {
        public static void WriteToLogFile(Exception e)
        {
            WriteToLogFile(e, "");
        }

        public static void WriteToLogFile(Exception e, Action<TextWriter> moreInfo)
        {

            using (var sw = new StringWriter())
            {
                moreInfo(sw);
                WriteToLogFile(e, sw.ToString());
            }
        }

        public static bool WriteLogEntriesToTraceFile { get; set; }

        static System.Diagnostics.Process _currentProcess;

        public static void WriteTrace(Func<string> what)
        {
            WriteTrace("Info", what);
        }

        public static void WriteTrace(string type, Func<string> what)
        {

            if (!string.IsNullOrEmpty(Common.TraceFileName.Value))
            {
                var p = PathDecoder.DecodePath(Common.TraceFileName.Value);
                if (p.Contains("<D>"))
                    p = p.Replace("<D>", Date.Now.ToString("YYYY_MM_DD"));
                if (Common.IncludeProcessAndThreadInTraceFileName)
                {
                    if (_currentProcess == null)
                        _currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                    var processAddition = "";
                    if (Common.IncludeAppNameInTraceFileName)
                        processAddition = (ENV.UserMethods.Instance.GetTextParam("APPNAME") ?? Text.Empty);
                    if (Common.IncludeProcessDateAndTimeInTraceFileName)
                    {
                        if (!string.IsNullOrEmpty(processAddition))
                            processAddition += "_";
                        processAddition += _currentProcess.StartTime.ToString("yyyyMMddHHmmss");
                    }
                    processAddition += "_" + _currentProcess.Id + "_" +
                         System.Threading.Thread.CurrentThread.ManagedThreadId;
                    int i = p.LastIndexOf('.');
                    if (i < 0)
                        p += processAddition;
                    else
                        p = p.Insert(i, processAddition);
                }
                WriteTrace(p, type, what);

            }

        }

        public static void WriteTrace(string traceFileName, string type, Func<string> what)
        {
            if (!string.IsNullOrEmpty(traceFileName))
            {
                var p = PathDecoder.DecodePath(traceFileName).Trim();
                lock (p)
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(p));
                        using (
                          var sw = new System.IO.StreamWriter(p, true,
                                                              ENV.LocalizationInfo.Current.OuterEncoding))
                        {
                            var sb = new StringBuilder();
                            sb.Append(DateTime.Now.ToString("dd/MM/yy HH:mm:ss.fff"));
                            sb.Append("  ");
                            sb.Append("[" + type.PadRight(7) + "] - ");
                            var count = Firefly.Box.Context.Current.ActiveTasks.Count;
                            if (count > 0)
                                sb.Append(new string(' ', count * 4 - 4));

                            sb.Append(what());
                            sw.WriteLine(sb);
                        }
                    }
                    catch
                    {
                    }
                }

            }

        }

        [ThreadStatic]
        public static string AdditionalInfo;

        public interface IDoNotWriteMeToLog
        {
        }

        static ContextStatic<Exception> LastWrittenException = new ContextStatic<Exception>(() => null);
        public static void WriteToLogFile(Exception e, string extraInfo, params object[] args)
        {
            if (e is IDoNotWriteMeToLog)
                return;
            if (LastWrittenException.Value == e)
                return;
            Profiler.WriteToLogFile(e, extraInfo, args);
            LastWrittenException.Value = e;
            if (WriteLogEntriesToTraceFile)
                WriteTrace("Exception", () => CreateErrorDescription(e, extraInfo, args));
            bool writtenToDebugOutput = false;
            if (Common.LogFileName != null)
            {
                var decodePath = ENV.PathDecoder.DecodePath(Common.LogFileName);
                if (Firefly.Box.Text.IsNullOrEmpty(decodePath))
                    return;
                lock (decodePath)
                {
                    try
                    {

                        using (
                            var sw = new System.IO.StreamWriter(decodePath, true,
                                                                ENV.LocalizationInfo.Current.OuterEncoding))
                        {
                            if (!string.IsNullOrEmpty(AdditionalInfo))
                                sw.WriteLine(AdditionalInfo);
                            sw.Write("{0} - >> ERROR >> ", DateTime.Now);
                            var desc = CreateErrorDescription(e, extraInfo, args);
                            sw.WriteLine(desc);
                            {
                                if (System.Diagnostics.Debugger.IsAttached)
                                    System.Diagnostics.Debug.WriteLine(desc);
                                writtenToDebugOutput = true;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            if (!writtenToDebugOutput)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debug.WriteLine(CreateErrorDescription(e, extraInfo, args));
            }

        }
        public static string CreateErrorDescriptionWithSQL(Exception ex)
        {
            var x = ex;
            string sql = null;
            while (x != null)
            {
                sql = x.Data["SQL"] as string;
                if (!string.IsNullOrEmpty(sql))
                {
                    break;
                }
                x = x.InnerException;
            }
            var orig = CreateErrorDescription(ex, "", new object[0]);
            if (sql != null)
                orig = "SQL:" + sql + orig;
            return orig;

        }
        public static string CreateErrorDescription(Exception e, string extraInfo, object[] args)
        {

            using (var errorReportBuilder = new StringWriter())
            {
                var uie = e as Firefly.Box.Advanced.InUIException;
                if (uie != null)
                    e = e.InnerException;
                var te = e as TargetInvocationException;
                if (te != null)
                    e = e.InnerException;
                errorReportBuilder.WriteLine(e.GetType() + " - " +
                    e.Message);
                {
                    var sea = e as System.Runtime.InteropServices.ExternalException;
                    if (sea != null)
                    {
                        errorReportBuilder.WriteLine("External error code: " + sea.ErrorCode);
                    }
                }
                if (!extraInfo.Contains("SQL:"))
                    AddSqlInfo(e, errorReportBuilder);
                if (ENV.Security.UserManager.CurrentUser != null &&
                    !string.IsNullOrEmpty(ENV.Security.UserManager.CurrentUser.Name))
                    errorReportBuilder.WriteLine("Application User: " +
                                                 ENV.Security.UserManager.
                                                     CurrentUser.Name);
                if (!string.IsNullOrEmpty(extraInfo))
                {
                    if (args.Length > 0)
                        errorReportBuilder.WriteLine(extraInfo, args);
                    else
                    {
                        errorReportBuilder.WriteLine(extraInfo);
                    }
                }
                errorReportBuilder.WriteLine();
                errorReportBuilder.WriteLine("Short Callstack:");
                {
                    errorReportBuilder.WriteLine(Common.GetShortStackTrace(e.StackTrace));
                    if (uie != null)
                        errorReportBuilder.WriteLine(Common.GetShortStackTrace(uie.StackTrace));
                    errorReportBuilder.WriteLine(Common.GetShortStackTrace());
                    var inner = e.InnerException;
                    while (inner != null)
                    {
                        errorReportBuilder.WriteLine("Inner Error : " +
                                                     inner.GetType() + " - " + inner.Message);
                        var sea = inner as System.Runtime.InteropServices.ExternalException;
                        if (sea != null)
                        {
                            errorReportBuilder.WriteLine("External error code: " + sea.ErrorCode);
                        }
                        errorReportBuilder.WriteLine("Inner Trace : ");
                        errorReportBuilder.WriteLine(Common.GetShortStackTrace(inner.StackTrace));
                        inner = inner.InnerException;
                    }

                    errorReportBuilder.WriteLine();
                }
                {
                    errorReportBuilder.WriteLine("Callstack:");
                    errorReportBuilder.WriteLine(e.StackTrace);
                    if (uie != null)
                        errorReportBuilder.WriteLine(uie.StackTrace);
                    var inner = e.InnerException;
                    while (inner != null)
                    {
                        errorReportBuilder.WriteLine("Inner Error : " +
                                                     inner.Message);
                        errorReportBuilder.WriteLine("Inner Trace : ");
                        errorReportBuilder.WriteLine(inner.StackTrace);
                        inner = inner.InnerException;
                    }
                    errorReportBuilder.WriteLine();
                }
                CurrentLocationInfo(errorReportBuilder, false);

                errorReportBuilder.WriteLine();



                return errorReportBuilder.ToString();

            }

        }

        private static void AddSqlInfo(Exception e, StringWriter errorReportBuilder)
        {

            if (e.Data.Contains("SQL"))
            {
                var y = e.Data["SQL"] as string;
                if (y != null)
                {
                    errorReportBuilder.WriteLine();
                    errorReportBuilder.WriteLine("SQL:");
                    errorReportBuilder.WriteLine(Regex.Replace(y, @"\r\n?|\n", "\r\n"));
                }
                return;
            }
            if (e.InnerException != null)
                AddSqlInfo(e.InnerException, errorReportBuilder);

        }

        public static void ShowCurrentLocation()
        {
            using (var sw = new StringWriter())
            {
                CurrentLocationInfo(sw, true);
                EntityBrowser.ShowString("Current Location", sw.ToString());
            }
        }

        public static string GetCurrentLocation()
        {
            using (var sw = new StringWriter())
            {
                CurrentLocationInfo(sw, true);
                return sw.ToString();
            }
        }
        static void CurrentLocationInfo(StringWriter errorReportBuilder, bool includeShortCallStack)
        {
            var v = System.Reflection.Assembly.GetEntryAssembly();

            var m = ENV.UserMethods._menu;
            if (m != null)
            {
                var l = new List<string>();
                while (m != null)
                {
                    var a = MenuManager.GetMenuText(m);
                    var b = MenuManager.GetOriginalMenuText(m);
                    if (a != b)
                    {
                        a += " (" + b + ")";
                    }
                    l.Insert(0, a);
                    m = m.OwnerItem;
                }

                errorReportBuilder.WriteLine("Last Menu Path: ");
                int i = 0;
                foreach (var VARIABLE in l)
                {
                    errorReportBuilder.WriteLine("{0}) {1}", ++i, VARIABLE);
                }

                errorReportBuilder.WriteLine();
            }

            if (includeShortCallStack)
            {
                errorReportBuilder.WriteLine("Short Callstack:");
                errorReportBuilder.WriteLine(Common.GetShortStackTrace());
                errorReportBuilder.WriteLine();
            }
            errorReportBuilder.WriteLine("Application Callstack:");
            WriteApplicationStack(errorReportBuilder);
            errorReportBuilder.WriteLine();


            ENV.Data.DataProvider.ConnectionManager.AddToErrorLog(errorReportBuilder);

            var mainVersion = "";
            var envVersion = typeof(ErrorLog).Assembly.GetName().Version.ToString();
            if (v != null)
            {
                var asm = System.Reflection.Assembly.GetEntryAssembly().GetName();
                mainVersion = asm.Version.ToString();
                errorReportBuilder.WriteLine(asm.Name + " Version: " + mainVersion);
            }
            if (mainVersion != envVersion)
                errorReportBuilder.WriteLine("ENV Version: " + envVersion);
            errorReportBuilder.WriteLine("Firefly Version: " + typeof(Firefly.Box.Number).Assembly.GetName().Version.ToString());
            MemoryTracker.AddMemoryInfo(errorReportBuilder);
            Profiler.AddInfoToErrorLog(errorReportBuilder);
        }

        static void WriteApplicationStack(StringWriter errorReportBuilder)
        {
            var x = Firefly.Box.Context.Current.ActiveTasks;
            for (int i = x.Count - 1; i >= 0; i--)
            {
                ControllerBase.SendInstanceBasedOnTaskAndCallStack(x[i],
                    c =>
                    {
                        string name = ENV.UserMethods.GetControllerName(x[i]);
                        var t = c.GetType();
                        try
                        {
                            if (c._application != null)
                            {
                                int num = c._application.AllPrograms.IndexOf(t);
                                var publicName = c._application.AllPrograms.GetPublicNameOf(c);

                                if (num > 0)
                                {
                                    if (!string.IsNullOrEmpty(publicName))
                                        publicName += " ";
                                    publicName += "P#" + num.ToString().Trim();

                                    name += " (" + publicName + ")";
                                }
                            }
                        }
                        catch
                        {
                        }
                        //errorReportBuilder.WriteLine(name.PadRight(40,'.') + t.FullName);
                        errorReportBuilder.WriteLine(name + " - " + t.FullName);
                        bool first = true;
                        c.SendParametersTo((s, s1) =>
                        {
                            string prefix = "  Parameters: ";
                            if (first)
                            {
                                first = false;
                            }
                            else
                                prefix = new string(' ', prefix.Length);
                            errorReportBuilder.WriteLine(prefix + s.Trim() + " = " + s1.Trim());
                        });
                        if (!first)
                            errorReportBuilder.WriteLine();
                    });

            }
        }


        public static void Test()
        {
            var decodePath = ENV.PathDecoder.DecodePath(Common.LogFileName);
            if (Firefly.Box.Text.IsNullOrEmpty(decodePath))
            {
                Common.ShowMessageBox("Test Error Log", MessageBoxIcon.Error, "GeneralErrorLog entry was not set in the ini");
                return;
            }
            try
            {
                using (var sw = new StreamWriter(decodePath))
                {
                    sw.WriteLine("Test Error Log");
                }
                Common.ShowMessageBox("Test Error Log", MessageBoxIcon.Information,
                    "'Test Error Log' was successfully written to the log file at '" +
                    System.IO.Path.GetFullPath(decodePath) + "'");
            }
            catch (Exception m)
            {
                Common.ShowExceptionDialog(m, true, "Failed to write to log file at '" + System.IO.Path.GetFullPath(decodePath) + "'");

            }

        }
        public static bool DisableDiagnosticsDebug { get; set; }
        public static void WriteDebugLine(string s)
        {
            if (!DisableDiagnosticsDebug)
                System.Diagnostics.Debug.WriteLine(s);
        }
        public static void WriteDebugLine(string format, params object[] args)
        {
            if (!DisableDiagnosticsDebug)
                System.Diagnostics.Debug.WriteLine(format, args);
        }
        public static void WriteTraceLine(string s)
        {
            if (!DisableDiagnosticsDebug)
                System.Diagnostics.Trace.WriteLine(s);
        }
    }
    enum TrackMemoryStrategies
    {
        None,
        InstanceOnly,
        IncludeAllocationStack
    }
    class MemoryTracker
    {
        public static TrackMemoryStrategies TrackStrategy { get; set; }

        public static void Track(object instance, string key = null)
        {
            if (TrackStrategy == TrackMemoryStrategies.None)
                return;
            _trackedInstances.Add(new InstanceInfo(instance, key));
        }
        static List<InstanceInfo> _trackedInstances = new List<InstanceInfo>();
        static Dictionary<string, string> _stackTraceCache = new Dictionary<string, string>();
        class InstanceInfo
        {
            WeakReference _instance;
            string _key;
            string _allocationShortStack;
            public InstanceInfo(object instance, string key)
            {
                _instance = new WeakReference(instance);
                _key = key;
                if (TrackStrategy == TrackMemoryStrategies.IncludeAllocationStack)
                {
                    var tempStack = "\r\nAllocation Stack:\r\n" + Common.GetShortStackTrace(Environment.StackTrace) + "\r\n";
                    //reduce memory consumption of identical callstacks used many times
                    lock (_stackTraceCache)
                    {
                        if (!_stackTraceCache.TryGetValue(tempStack, out _allocationShortStack))
                            _stackTraceCache.Add(tempStack, _allocationShortStack = tempStack);
                    }
                }

            }

            internal string GetKeyIfAlive()
            {
                if (!_instance.IsAlive)
                    return null;

                if (_key == null)
                {
                    var t = _instance.Target;
                    if (t == null)
                        return null;
                    _key = t.GetType().FullName + _allocationShortStack;
                }
                return _key;
            }
        }

        internal class AggregateStackInfo
        {
            public string Key;
            public int Count;
        }
        static object LockReference = new object();
        static List<AggregateStackInfo> Collect()
        {
            var x = _trackedInstances;
            lock (LockReference)
            {
                _trackedInstances = new List<InstanceInfo>(x.Capacity);
                var activeItems = new List<InstanceInfo>();
                var result = new List<AggregateStackInfo>();
                var dic = new Dictionary<string, AggregateStackInfo>();
                foreach (var item in x.ToArray())
                {
                    var k = item.GetKeyIfAlive();
                    if (k != null)
                    {
                        activeItems.Add(item);
                        AggregateStackInfo i;
                        if (!dic.TryGetValue(k, out i))
                        {
                            i = new ENV.MemoryTracker.AggregateStackInfo { Key = k };
                            result.Add(i);
                            dic.Add(k, i);
                        }
                        i.Count++;
                    }
                }
                result.Sort((a, b) => b.Count.CompareTo(a.Count));
                _trackedInstances.AddRange(activeItems);
                return result;
            }
        }

        internal static void AddMemoryInfo(StringWriter errorReportBuilder)
        {
            if (TrackStrategy != TrackMemoryStrategies.None)
            {

                errorReportBuilder.WriteLine("Memory Usage:");
                try
                {
                    errorReportBuilder.WriteLine("***Before GC Collect:");
                    var totalDotNetBytes = GC.GetTotalMemory(false);
                    var x = Collect();
                    errorReportBuilder.WriteLine("Controllers: " + ItemCount(x));
                    errorReportBuilder.WriteLine(".Net Memory: " + totalDotNetBytes / 1024 / 1024);
                    var p = System.Diagnostics.Process.GetCurrentProcess();
                    p.Refresh();
                    errorReportBuilder.WriteLine("WorkingSet: " + p.WorkingSet64 / 1024 / 1024);
                    errorReportBuilder.WriteLine("PeakWorkingSet: " + p.PeakWorkingSet64 / 1024 / 1024);
                    errorReportBuilder.WriteLine("PrivateMemorySize: " + p.PrivateMemorySize64 / 1024 / 1024);
                    errorReportBuilder.WriteLine("VirtualMemorySize: " + p.VirtualMemorySize64 / 1024 / 1024);
                    errorReportBuilder.WriteLine("Handles: " + p.HandleCount);

                    if (false)
                    {
                        errorReportBuilder.WriteLine();
                        errorReportBuilder.WriteLine("Tracked Instances Information:");

                        foreach (var item in x)
                        {
                            if (item.Count >= InstanceCountToShow)
                            {
                                errorReportBuilder.WriteLine("{0} - {1}", item.Count, item.Key);
                            }
                        }
                    }

                    errorReportBuilder.WriteLine("**************************************************************************************************");
                    errorReportBuilder.WriteLine("  After GC Collect:");
                    totalDotNetBytes = GC.GetTotalMemory(true);
                    x = Collect();
                    errorReportBuilder.WriteLine("Controllers: " + ItemCount(x));
                    errorReportBuilder.WriteLine(".Net Memory: " + totalDotNetBytes / 1024 / 1024);
                    p.Refresh();
                    errorReportBuilder.WriteLine("WorkingSet: " + p.WorkingSet64 / 1024 / 1024);
                    errorReportBuilder.WriteLine("PeakWorkingSet: " + p.PeakWorkingSet64 / 1024 / 1024);
                    errorReportBuilder.WriteLine("PrivateMemorySize: " + p.PrivateMemorySize64 / 1024 / 1024);
                    errorReportBuilder.WriteLine("VirtualMemorySize: " + p.VirtualMemorySize64 / 1024 / 1024);
                    errorReportBuilder.WriteLine("Handles: " + p.HandleCount);
                    errorReportBuilder.WriteLine();
                    errorReportBuilder.WriteLine("Machine's Memory {0}:", System.Environment.MachineName);
                    var oMemoryInfo = new NativeMethods();
                    if (oMemoryInfo.isMemoryTight())
                        errorReportBuilder.WriteLine("*** Machine Memory Is Tight ***");
                    errorReportBuilder.WriteLine("MemoryLoad: " + oMemoryInfo.MemoryLoad + "%");
                    errorReportBuilder.WriteLine("Available Physical: " + oMemoryInfo.msex.ullAvailPhys / 1024 / 1024);
                    errorReportBuilder.WriteLine("Available Virtual: " + oMemoryInfo.msex.ullAvailVirtual / 1024 / 1024);
                    errorReportBuilder.WriteLine("Total Physical: " + oMemoryInfo.msex.ullTotalPhys / 1024 / 1024);
                    errorReportBuilder.WriteLine("Total Virtual: " + oMemoryInfo.msex.ullTotalVirtual / 1024 / 1024);
                    errorReportBuilder.WriteLine();
                    errorReportBuilder.WriteLine("Tracked Instances Information:");
                    foreach (var item in x)
                    {
                        if (item.Count >= InstanceCountToShow)
                        {
                            errorReportBuilder.WriteLine("{0} - {1}", item.Count, item.Key);
                        }
                    }
                }
                catch
                {
                }


            }
        }

        internal static long ItemCount(List<AggregateStackInfo> collection = null)
        {
            if (collection == null)
                collection = Collect();
            var r = 0;
            foreach (var item in collection)

            {
                r += item.Count;
            }
            return r;
        }

        public static int InstanceCountToShow = 2;
        [CLSCompliant(false)]
        public class NativeMethods
        {

            public MEMORYSTATUSEX msex;
            private uint _MemoryLoad;
            const int MEMORY_TIGHT_CONST = 80;

            public bool isMemoryTight()
            {
                if (_MemoryLoad > MEMORY_TIGHT_CONST)
                    return true;
                else
                    return false;
            }

            public uint MemoryLoad
            {
                get { return _MemoryLoad; }
                internal set { _MemoryLoad = value; }
            }

            public NativeMethods()
            {

                msex = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(msex))
                {

                    _MemoryLoad = msex.dwMemoryLoad;
                    //etc.. Repeat for other structure members         

                }
                else
                    // Use a more appropriate Exception Type. 'Exception' should almost never be thrown
                    throw new Exception("Unable to initalize the GlobalMemoryStatusEx API");
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public class MEMORYSTATUSEX
            {
                public uint dwLength;
                public uint dwMemoryLoad;
                public ulong ullTotalPhys;
                public ulong ullAvailPhys;
                public ulong ullTotalPageFile;
                public ulong ullAvailPageFile;
                public ulong ullTotalVirtual;
                public ulong ullAvailVirtual;
                public ulong ullAvailExtendedVirtual;
                public MEMORYSTATUSEX()
                {
                    this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                }
            }


            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        }
    }

}
