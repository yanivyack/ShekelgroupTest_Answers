using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ENV.Data;
using ENV.Data.DataProvider;
using ENV.UI;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box;
using Entity = Firefly.Box.Data.Entity;
using TreeView = System.Windows.Forms.TreeView;
using XmlWriter = System.Xml.XmlWriter;
using ProfilerUI;
using System.IO;

namespace ENV.Utilities
{
    public class Profiler
    {
        static Profiler()
        {
            Entry._wrapCallToShow = y => Firefly.Box.Context.Current.InvokeUICommand(y);
        }
        internal class DummyDisposable : IDisposable
        {
            public static DummyDisposable Instance = new DummyDisposable();
            public void Dispose()
            {

            }
        }
        public static bool TrackCommandsAndReaders = false;
        internal static void AddInfoToErrorLog(StringWriter errorReportBuilder)
        {
            if (TrackCommandsAndReaders)
            {
                _trackedCommandsAndReaders.Value.AddToErrorLog(errorReportBuilder);

            }
        }

        internal class ItemTracker
        {
            Dictionary<object, TrackInfo> _items = new Dictionary<object, TrackInfo>();
            class TrackInfo
            {
                private object t;
                System.Diagnostics.Stopwatch _st = new Stopwatch();
                IProfilerEntry _context = CurrentEntry.Value;

                public TrackInfo(object t)
                {
                    this.t = t;
                    _st.Start();
                }

                internal void WriteReportTo(StringWriter errorReportBuilder)
                {

                    errorReportBuilder.WriteLine(t.ToString() + " - Age, " + _st.Elapsed);
                    var x = _context;
                    while (x != null)
                    {
                        errorReportBuilder.WriteLine(x.Name);
                        x = x.Parent;
                    }
                    errorReportBuilder.WriteLine();
                }
            }
            public void Add(object t)
            {
                _items.Add(t, new TrackInfo(t));
                if (_items.Count % 50 == 0)
                {
                    WriteCommandsAndReadersToTrace();
                }
            }

            public void WriteCommandsAndReadersToTrace()
            {
                ErrorLog.WriteTrace("Info", () =>
                {

                    using (var sw = new StringWriter())
                    {
                        AddToErrorLog(sw);
                        return sw.ToString();
                    }

                });
            }

            public void Remove(object t)
            {
                _items.Remove(t);
            }

            internal void AddToErrorLog(StringWriter errorReportBuilder)
            {
                errorReportBuilder.WriteLine();
                errorReportBuilder.WriteLine("Tracked Readers and Commands " + _items.Count + ":");
                foreach (var item in _items.Values)
                {
                    item.WriteReportTo(errorReportBuilder);
                }
            }
        }
        internal static ContextStatic<ItemTracker> _trackedCommandsAndReaders = new ContextStatic<ItemTracker>(() => new ItemTracker());
        static event Action ProfilingStateChanged;
        static string _profilerFile;
        public static void WriteCommandsAndReadersToTrace()
        {
            if (TrackCommandsAndReaders)
                _trackedCommandsAndReaders.Value.WriteCommandsAndReadersToTrace();
        }
        public static string ProfilerFile
        {
            get
            {
                return _profilerFile;
            }
            set
            {
                _profilerFile = value;
                if (!_profilerFile.EndsWith("\\"))
                    if (DoNotProfile())
                    {
                        Start();
                        AutoSaveTimer(_profilerFile);
                    }
            }
        }
        class TraceEntry
        {
            EntryDisposable _entry;
            bool _end;
            long _ticks;
            public TraceEntry(EntryDisposable entry, bool end)
            {
                _entry = entry;
                _end = end;
                _ticks = _traceTicks.Elapsed.Ticks;
            }

            internal void WriteTo(XmlTextWriter sw)
            {
                _entry.WriteTraceTo(sw, _end, _ticks);
            }
        }

        public static bool Tracing { get; set; }
        static object _lockTrace = new object();
        static List<TraceEntry> _traceEntries = new List<TraceEntry>();

        static Stopwatch _traceTicks;
        static DateTime _traceStart;
        static long _traceStartTicks;
        static bool _writeTraceStart;
        static void Trace(EntryDisposable entry, bool end)
        {
            if (Tracing)
                lock (_lockTrace)
                {
                    if (_traceTicks == null)
                    {
                        _traceStart = DateTime.Now;
                        _traceTicks = new Stopwatch();
                        _traceTicks.Start();
                        _traceStartTicks = _traceTicks.Elapsed.Ticks;
                        _writeTraceStart = true;
                    }
                    _traceEntries.Add(new TraceEntry(entry, end));
                }
        }
        static string _lastAutoSaveTimerFileName;
        static ContextStatic<string> _currentSessionFileName = new ContextStatic<string>(() => null);
        static void AutoSaveTimer(string fileName)
        {
            _currentSessionFileName.Value = fileName;
            bool saving = false;
            var x = Firefly.Box.Context.Current;
            ;
            _lastAutoSaveTimerFileName = fileName;
            _timer.Value = new System.Threading.Timer(state =>
            {
                if (saving)
                    return;
                saving = true;
                try
                {
                    if (_memoryTracker != null)
                    {
                        using (var y = _memoryTracker.CreateChild(((int)(DateTime.Now - _memoryTrackerStart).TotalSeconds).ToString().PadLeft(3, '0')))
                        {
                            y.CreateChild(ErrorLog.GetCurrentLocation()).Dispose();
                        }
                    }
                    var e = CurrentEntry.GetValueFromContext(x);
                    if (e != null)
                        e.SaveFromTimer(fileName);
                    if (Tracing)
                    {
                        SaveTraceFile(fileName);
                    }
                }
                catch (Exception e)
                {
                    ENV.ErrorLog.WriteToLogFile(e,
                        string.Format("Failed to save Profiler file '{0}', {1}", fileName, e.Message));
                }
                finally
                {
                    saving = false;
                }
            }, null, 5000, 5000);
        }
        public static void ForceSaveCurrentProfilerFile()
        {
            if (_currentSessionFileName.Value != null && CurrentEntry.Value != null)
            {
                try
                {
                    CurrentEntry.Value.SaveFromTimer(_currentSessionFileName.Value);
                }
                catch { }
            }
        }

        static void SaveTraceFile(string fileName)
        {
            List<TraceEntry> te;
            lock (_lockTrace)
            {
                te = _traceEntries;
                _traceEntries = new List<TraceEntry>(te.Capacity);
            }
            if (te.Count > 0)
                using (var sw = new System.IO.StreamWriter(fileName + "trace", true))
                {
                    using (var xml = new System.Xml.XmlTextWriter(sw))
                    {
                        if (_writeTraceStart)
                        {
                            _writeTraceStart = false;
                            xml.WriteStartElement("Sync");
                            xml.WriteAttributeString("ticks", _traceStartTicks.ToString());
                            xml.WriteAttributeString("datetime", _traceStart.ToString());
                            xml.WriteEndElement();

                        }
                        foreach (var item in te)
                        {
                            item.WriteTo(xml);
                        }
                    }
                }
        }

        static ContextStatic<System.Threading.Timer> _timer = new ContextStatic<System.Threading.Timer>(() => null);
        static ContextStatic<long> _profilingSessionID = new ContextStatic<long>(() => 0);
        static EntryDisposable _memoryTracker;
        static DateTime _memoryTrackerStart;
        public static void Start()
        {
            _generalDoNotProfile = false;
            _contextDoNotProfile.Value = false;
            CurrentEntry.Value = null;
            _profilingSessionID.Value++;
            if (ProfilingStateChanged != null)
                ProfilingStateChanged();
            var r = new EntryDisposable("Entire Session");
            if (ENV.MemoryTracker.TrackStrategy != TrackMemoryStrategies.None)
            {
                _memoryTracker = new EntryDisposable("Memory Tracker") { JustATitle = true };
                _memoryTracker.Dispose();
                _memoryTrackerStart = DateTime.Now;
            }
        }

        static bool _generalDoNotProfile = true;
        static ContextStatic<bool> _contextDoNotProfile = new ContextStatic<bool>(() => true);
        static bool _disableMenuSet = false;
        public static bool DoNotProfile()
        {
            if (_generalDoNotProfile)
                return true;
            if (_userIsProfiling)
            {
                if (_contextDoNotProfile.Value)
                {
                    var x = _disableMenuSet;
                    _disableMenuSet = true;
                    try
                    {
                        StartUserProfilingSession();
                    }
                    finally
                    {
                        _disableMenuSet = x;
                    }
                }
                return false;
            }
            else
                return _contextDoNotProfile.Value;
        }

        public static void EndProfilingSession(string file = null)
        {
            var e = End1();
            if (string.IsNullOrEmpty(file))
            {
                e.Display(false, Text.Cast(Context.Current[UserMethods.CONTEXT_NAME_KEY]) ?? System.Threading.Thread.CurrentThread.Name ?? ENV.UserMethods.Instance.CtxGetName());
                if (Tracing)
                {
                    if (_lastAutoSaveTimerFileName != null)
                    {
                        SaveTraceFile(_lastAutoSaveTimerFileName);
                        ProfilerCore.Open(_lastAutoSaveTimerFileName + "trace");
                    }

                }
            }
            else e.Save(file);
        }
        public static event Action<IProfilerEntry> OnStartContext;

        static Entry End1()
        {
            try
            {
                _contextDoNotProfile.Value = true;
                if (ProfilingStateChanged != null)
                    ProfilingStateChanged();
                if (_timer.Value != null)
                    _timer.Value.Dispose();
                return CurrentEntry.Value.End();
            }
            finally
            {
                _profilingSessionID.Value++;
            }
        }

        internal static Entry End()
        {
            return End1();
        }



        static Firefly.Box.ContextStatic<EntryDisposable> CurrentEntry = new Firefly.Box.ContextStatic<EntryDisposable>(() => null);
        class EntryDisposable : IDisposable, IProfilerEntry
        {

            static long LastId;
            long _id;
            Entry _e;
            EntryDisposable _previous, _parent;
            Stopwatch _ticks;

            public bool JustATitle { set { _e.Count = -1; } }

            public string Name
            {
                get
                {
                    return _e.Name;
                }
            }

            public long Count { get { return _e.Count; } }

            public IProfilerEntry Parent { get { return _parent; } }

            public override string ToString()
            {
                return _e.ToString();
            }
            public EntryDisposable(string e, bool isUserTime = false)
                : this(e, CurrentEntry.Value, isUserTime)
            {
            }

            long _myProfilingSessionID;
            string _name;
            bool _isUserTime = false;
            public EntryDisposable(string e, EntryDisposable parent, bool isUserTime = false)
            {
                _isUserTime = isUserTime;
                _id = System.Threading.Interlocked.Increment(ref LastId);
                _name = e;
                _ticks = new Stopwatch();
                _ticks.Start();
                _myProfilingSessionID = _profilingSessionID.Value;
                if (parent == null)
                    _e = new Entry() { Name = e };
                else
                    _e = parent._e.Get(e, out _foundInParent);

                _e.IsUserTime = isUserTime;

                _parent = parent;
                _previous = CurrentEntry.Value;
                CurrentEntry.Value = this;
                Trace(this, false);
                if (OnStartContext != null)
                    OnStartContext(this);
            }
            bool _foundInParent = false;
            public EntryDisposable CreateChild(string name)
            {
                return new EntryDisposable(name, this);
            }



            bool _done = false;
            public virtual void Dispose()
            {
                if (_profilingSessionID.Value != _myProfilingSessionID)
                    return;
                lock (_e)
                {
                    CurrentEntry.Value = _previous;
                    _ticks.Stop();
                    _done = true;
                    _e.AddDuration(_ticks.Elapsed.Ticks);


                    if (_parent != null)
                        if (!_doNotWriteIfEmpty || _e.HasChildern() || _e.Duration > TimeSpan.TicksPerSecond)
                        {
                            if (_e.Count > -1)
                                _e.Count++;
                            if (!_foundInParent)
                            {
                                _parent._e.AddItem(_e);
                                _foundInParent = true;
                            }

                            AddDurationToParent(_ticks.Elapsed.Ticks);
                        }
                    Trace(this, true);
                }

            }

            void AddDurationToParent(long duration)
            {
                if (_parent != null)
                {
                    if (_parent._done)
                    {
                        _parent._e.AddDuration(duration);
                        _parent.AddDurationToParent(duration);
                    }
                }
            }


            public Entry End()
            {
                Dispose();
                if (CurrentEntry.Value != null)
                    return CurrentEntry.Value.End();
                return _e;

            }



            bool _doNotWriteIfEmpty;
            public void DoNoWriteIfEmpty()
            {
                _doNotWriteIfEmpty = true;
            }


            public void SaveFromTimer(string profilerFile)
            {
                if (_parent == null)
                {
                    if (_e.HasChildern())
                        _e.Save(profilerFile);
                }
                else
                {
                    lock (_e)
                    {
                        if (!_foundInParent)
                        {
                            _parent._e.AddItem(_e);
                            _foundInParent = true;
                        }


                        var y = _e.Duration;
                        var z = _e.Count;
                        _e.Duration += _ticks.Elapsed.Ticks;
                        _e.Count++;
                        try
                        {
                            _parent.SaveFromTimer(profilerFile);

                        }
                        finally
                        {
                            _e.Duration = y;
                            _e.Count = z;
                        }
                    }

                }
            }

            internal void WriteTraceTo(XmlTextWriter sw, bool end, long tickCount)
            {
                if (end)
                {
                    sw.WriteStartElement("e");

                }
                else
                {
                    sw.WriteStartElement("s");
                    sw.WriteAttributeString("n", _name);

                }
                sw.WriteAttributeString("id", _id.ToString());
                sw.WriteAttributeString("t", tickCount.ToString());
                if (_isUserTime)
                    sw.WriteAttributeString("d", "t");
                sw.WriteEndElement();
            }
        }



        public static IDisposable Controller(ControllerBase controllerBase)
        {
            if (DoNotProfile())
                return DummyDisposable.Instance;
            return StartContext(controllerBase.GetType().FullName, controllerBase is AbstractUIController);
        }

        public static IDisposable StartContext(string name, bool isUserTime = false)
        {
            if (DoNotProfile())
                return DummyDisposable.Instance;
            return new EntryDisposable(name, isUserTime);
        }


        public static IDisposable Level(object controller, string name)
        {
            if (DoNotProfile())
                return DummyDisposable.Instance;
            if (name == null)
                return DummyDisposable.Instance;
            if (name.StartsWith("GP_"))
                name = "Group Enter " + name.Substring(3);
            else if (name.StartsWith("GS_"))
                name = "Group Leave " + name.Substring(3);
            return StartContext(controller.GetType().FullName + "." + name + "()");
        }



        static string DurationToString(long duration)
        {
            var x = ((Number)duration / TimeSpan.TicksPerMillisecond);
            if (x < 10)
                return x.ToString("1.2") + " ms";
            return x.ToString("7C").Trim() + " ms";
        }






        public static void DontWriteIfEmpty()
        {
            if (DoNotProfile())
                return;
            CurrentEntry.Value.DoNoWriteIfEmpty();
        }



        class MyRow : IRow
        {
            IRow _row;
            EntryDisposable _parent;
            Entity _entity;

            public IRow OriginalRow()
            {
                return _row;
            }
            public MyRow(EntryDisposable parent, IRow row, Entity entity)
            {
                _row = row;
                _parent = parent;
                _entity = entity;
            }

            public void Delete(bool verifyRowHasNotChangedSinceLoaded)
            {
                using (var x = _parent.CreateChild("Delete " + _entity.GetType().Name))
                {
                    using (x.CreateChild("DeleteRow " + getPositionWhere()))
                        try
                        {
                            _row.Delete(verifyRowHasNotChangedSinceLoaded);

                        }
                        catch (Exception ex)
                        {
                            using (StartContext("Exception: " + ex.Message))
                            {
                            };
                            throw;
                        }
                }
            }

            public bool IsEqualTo(IRow row)
            {
                var z = row as MyRow;
                if (z != null)
                    row = z.OriginalRow();
                return _row.IsEqualTo(row);
            }
            string getPositionWhere()
            {
                var fc = new FilterCollection();
                var cols = _entity.PrimaryKeyColumns;
                if (cols.Length == 0 || cols.Length == 1 && (cols[0] is BtrievePositionColumn || cols[0] is UserMethods.NotIncludedInVarIndexCalculations))
                {
                    Sort s = null;
                    foreach (var item in _entity.Indexes)
                    {
                        if (s == null)
                            s = item;
                        else if (!s.Unique && item.Unique)
                        {
                            s = item;
                        }
                        else if (s.Unique == item.Unique && s.Segments.Count > item.Segments.Count)
                            s = item;
                    }
                    if (s != null)
                    {
                        var l = new List<ColumnBase>();
                        foreach (var item in s.Segments)
                        {
                            l.Add(item.Column);
                        }
                        cols = l.ToArray();
                    }

                }
                if (cols.Length == 0)
                    cols = _entity.Columns.ToArray();

                var sb = new StringBuilder();
                foreach (var item in cols)
                {
                    if (sb.Length > 0)
                        sb.Append(" and ");
                    var x = TranslateValue(item.OriginalValue);
                    sb.Append(item.Name + " = " + x);
                }
                return "Where " + sb.ToString();

            }



            public void Lock()
            {

                using (var x = _parent.CreateChild("Lock " + _entity.GetType().Name))
                {
                    using (x.CreateChild("LockRow " + getPositionWhere()))
                        _row.Lock();
                }
            }

            public void ReloadData()
            {
                using (var x = _parent.CreateChild("Reload Row From " + _entity.GetType().Name))
                {
                    using (x.CreateChild("ReloadRow " + getPositionWhere()))
                        _row.ReloadData();
                }
            }

            public void Unlock()
            {
                using (var x = _parent.CreateChild("Unlock " + _entity.GetType().Name))
                {
                    ENV.Utilities.Profiler.DontWriteIfEmpty();
                    using (x.CreateChild("Unlock " + getPositionWhere()))
                    {
                        ENV.Utilities.Profiler.DontWriteIfEmpty();
                        _row.Unlock();
                    }
                }
            }


            public void Update(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, bool verifyRowHasNotChangedSinceLoaded)
            {
                using (var x = _parent.CreateChild("Update " + _entity.GetType().Name))
                {
                    using (x.CreateChild("Update " + getUpdateString(columns) + " " + getPositionWhere()))
                        try
                        {
                            _row.Update(columns, values, verifyRowHasNotChangedSinceLoaded);
                        }
                        catch (Exception ex)
                        {
                            using (StartContext("Exception: " + ex.Message))
                            {
                            };
                            throw;
                        }
                }
            }
        }
        static string getUpdateString(IEnumerable<ColumnBase> cols)
        {
            var sb = new StringBuilder();
            foreach (var item in cols)
            {
                if (sb.Length != 0)
                    sb.Append(", ");
                sb.Append(item.Name + " = " + TranslateValue(item.Value));
            }
            return "Set " + sb.ToString();

        }
        private static string TranslateValue(object x)
        {

            if (!(x is Number))
            {
                if (x == null)
                    x = "null";
                else
                    x = "'" + x.ToString().TrimEnd().Replace("'", "''") + "'";
            }

            return x.ToString();
        }
        internal static void ToggleProfilerWithMessage()
        {
            if (DoNotProfile())
            {
                if (ENV.Common.ShowYesNoMessageBox("Profiler", "Start Profiling", true))
                    ToggleProfiling();
            }
            else
                ToggleProfiling();
        }

        class ProfiledRowsreader : IRowsReader
        {
            IRowsReader _source;
            EntryDisposable _parent;
            EntryDisposable _entityParent;
            Entity _entity;

            public ProfiledRowsreader(IRowsReader source, EntryDisposable parent, Entity entity, EntryDisposable entityParent)
            {
                _entity = entity;
                _source = source;
                _parent = parent;
                _entityParent = entityParent;
            }

            public void Dispose()
            {
                using (_parent.CreateChild("Close Reader"))
                {
                    _source.Dispose();
                    DontWriteIfEmpty();
                }
            }

            public IRow GetJoinedRow(Firefly.Box.Data.Entity e, IRowStorage c)
            {
                var x = _source.GetJoinedRow(e, c);
                if (x != null)
                    return new MyRow(_entityParent, x, e);
                return null;
            }

            public IRow GetRow(IRowStorage c)
            {
                return new MyRow(_entityParent, _source.GetRow(c), _entity);
            }
            bool _everRowFound = false;
            public bool Read()
            {
                using (var x = _parent.CreateChild("Fetch Row"))
                {
                    var r = _source.Read();
                    if (r)
                        _everRowFound = true;
                    else if (!r && !_everRowFound)
                    {
                        using (StartContext("No rows"))
                        {
                        }
                    }
                    return r;
                }
            }
        }

        public interface IRowsSourceProviderProfiler : IDisposable
        {
            IRowsSource ProvideRowsSource(IRowsSource originalSource);
        }

        class DummyRowsSourceProviderProfiler : IRowsSourceProviderProfiler
        {
            public static DummyRowsSourceProviderProfiler Instane = new DummyRowsSourceProviderProfiler();
            public void Dispose()
            {

            }

            public IRowsSource ProvideRowsSource(IRowsSource originalSource)
            {
                return originalSource;
            }
        }



        class RowsSourceProviderProfiler : IRowsSourceProviderProfiler
        {
            EntryDisposable _myEntry;
            Entity _entity;
            public RowsSourceProviderProfiler(EntryDisposable e, Entity entity)
            {
                _entity = entity;
                _myEntry = e;

            }

            public IRowsSource ProvideRowsSource(IRowsSource originalSource)
            {
                return new ProfiledRowSource(originalSource, _myEntry, _entity);
            }



            public void Dispose()
            {
                _myEntry.Dispose();
            }
        }





        class ProfilerRowsReaderProvider : IDisposable
        {
            EntryDisposable _profiler, _profilerParent, _entityParent;
            Entity _entity;

            public ProfilerRowsReaderProvider(string name, string moreInfo, EntryDisposable parent, Entity entity)
            {
                _entityParent = parent;
                _profilerParent = parent.CreateChild(name);
                if (moreInfo != null)
                    _profiler = _profilerParent.CreateChild(moreInfo);
                _entity = entity;
            }

            public void Dispose()
            {
                if (_profiler != null)
                    _profiler.Dispose();
                _profilerParent.Dispose();
            }

            public IRowsReader GetRowsReader(IRowsReader executeCommand)
            {

                return new ProfiledRowsreader(executeCommand, _profiler ?? _profilerParent, _entity, _entityParent);
            }
        }



        public static IRowsSourceProviderProfiler OpenRowSource(Firefly.Box.Data.Entity entity)
        {
            if (DoNotProfile())
                return DummyRowsSourceProviderProfiler.Instane;
            return new RowsSourceProviderProfiler(new EntryDisposable("M:" + entity.GetType().FullName) { JustATitle = true }, entity);
        }
        class ProfiledRowSource : IRowsSource
        {
            IRowsSource _source;
            EntryDisposable _parent;
            Entity _entity;
            public ProfiledRowSource(IRowsSource source, EntryDisposable parent, Entity entity)
            {
                _source = source;
                _parent = parent;
                _entity = entity;
            }

            static ProfilerRowsReaderProvider ProfiledRowsReader(string name, string moreInfo, EntryDisposable parent, Entity entity)
            {
                return new ProfilerRowsReaderProvider(name, moreInfo, parent, entity);
            }

            static string FilterToString(IFilter filter)
            {
                var x = SQLFilterConsumer.DisplayFilterInSingleLine(filter, true);
                if (string.IsNullOrEmpty(x))
                    x = "All Rows";
                else
                    x = "Where " + x;
                return x;
            }

            public IRowsProvider CreateReader(IEnumerable<ColumnBase> selectedColumns, IFilter @where, Sort sort, IEnumerable<IJoin> joins, bool disableCache)
            {
                using (var x = new ProfiledRowsProviderProvider(_parent.CreateChild("Rows Provider, " + FilterToString(@where)), _entity))
                    return x.GetRowsProvider(_source.CreateReader(selectedColumns, @where, sort, joins, disableCache));
            }
            class ProfiledRowsProviderProvider : IDisposable
            {
                EntryDisposable _profiler;
                Entity _entity;
                public ProfiledRowsProviderProvider(EntryDisposable profiler, Entity entity)
                {
                    _entity = entity;
                    _profiler = profiler;
                }

                public void Dispose()
                {
                    _profiler.Dispose();
                }

                class ProfiledRowsProvider : IRowsProvider
                {
                    IRowsProvider _provider;
                    EntryDisposable _parent;
                    Entity _entity;

                    public ProfiledRowsProvider(IRowsProvider provider, EntryDisposable parent, Entity entity)
                    {
                        _provider = provider;
                        _parent = parent;
                        _entity = entity;
                    }

                    public IRowsReader After(IRow row, bool reverse)
                    {
                        var r = row as MyRow;
                        if (r != null)
                            row = r.OriginalRow();
                        using (var x = ProfiledRowsReader("AfterRow " + (reverse ? "Descending" : ""), null, _parent, _entity))
                            return x.GetRowsReader(_provider.After(row, reverse));
                    }

                    public IRowsReader Find(IFilter filter, bool reverse)
                    {
                        using (var x = ProfiledRowsReader("Find " + (reverse ? "Descending" : "") + " - " + FilterToString(filter), null, _parent, _entity))
                            return x.GetRowsReader(_provider.Find(filter, reverse));
                    }

                    public IRowsReader From(IFilter filter, bool reverse)
                    {
                        using (var x = ProfiledRowsReader("From " + (reverse ? "Descending" : "") + " - " + FilterToString(filter), null, _parent, _entity))
                            return x.GetRowsReader(_provider.From(filter, reverse));
                    }

                    public IRowsReader From(IRow row, bool reverse)
                    {
                        var r = row as MyRow;
                        if (r != null)
                            row = r.OriginalRow();
                        using (var x = ProfiledRowsReader("From Row " + (reverse ? "Descending" : ""), null, _parent, _entity))
                            return x.GetRowsReader(_provider.From(row, reverse));
                    }

                    public IRowsReader FromEnd()
                    {
                        using (var x = ProfiledRowsReader("From End", null, _parent, _entity))
                            return x.GetRowsReader(_provider.FromEnd());
                    }

                    public IRowsReader FromStart()
                    {
                        using (var x = ProfiledRowsReader("From Start", null, _parent, _entity))
                            return x.GetRowsReader(_provider.FromStart());
                    }
                }


                public IRowsProvider GetRowsProvider(IRowsProvider provider)
                {
                    return new ProfiledRowsProvider(provider, _profiler, _entity);
                }
            }


            public void Dispose()
            {
                var x = CurrentEntry.Value;
                try
                {
                    CurrentEntry.Value = _parent;
                    _source.Dispose();
                }
                finally
                {
                    CurrentEntry.Value = x;
                }
            }

            public IRowsReader ExecuteCommand(IEnumerable<ColumnBase> selectedColumns, IFilter filter, Sort sort, bool firstRowOnly,
                bool shouldBeOnlyOneRowThatMatchesTheFilter, bool lockAllRows)
            {
                using (var r = ProfiledRowsReader("Relation to " + _entity.GetType().Name, FilterToString(filter), _parent, _entity))
                    return r.GetRowsReader(_source.ExecuteCommand(selectedColumns, filter, sort, firstRowOnly, shouldBeOnlyOneRowThatMatchesTheFilter, lockAllRows));
            }

            public IRowsReader ExecuteReader(IEnumerable<ColumnBase> selectedColumns, IFilter @where, Sort sort, IEnumerable<IJoin> joins, bool lockAllRows)
            {
                using (var r = ProfiledRowsReader("From " + _entity.GetType().Name, FilterToString(@where), _parent, _entity))
                    return r.GetRowsReader(_source.ExecuteReader(selectedColumns, @where, sort, joins, lockAllRows));
            }

            public IRow Insert(IEnumerable<ColumnBase> columns, IEnumerable<IValue> values, IRowStorage storage, IEnumerable<ColumnBase> selectedColumns)
            {
                using (var x = _parent.CreateChild("Insert into " + _entity.GetType().Name))
                {
                    using (x.CreateChild(getUpdateString(columns)))
                        try
                        {
                            return new MyRow(_parent, _source.Insert(columns, values, storage, selectedColumns), _entity);
                        }
                        catch (Exception ex)
                        {
                            using (StartContext("Exception: " + ex.Message))
                            {
                            };
                            throw;
                        }
                }
            }

            public bool IsOrderBySupported(Sort sort)
            {
                return _source.IsOrderBySupported(sort);
            }
        }

        public static IDisposable DynamicSQLEntity(DynamicSQLEntity de)
        {
            return StartContext("DynamicSQLEntity, SQL=" + de.EntityName);
        }

        public static IDbConnection DecorateConnection(IDbConnection connection)
        {
            return new ProfiledConnection(connection);
        }


        class ProfiledTransaction : IDbTransaction, ITransactionDecorator
        {
            IDbTransaction _trans;

            public ProfiledTransaction(IDbTransaction trans)
            {
                _trans = trans;
            }

            public void Dispose()
            {
                _trans.Dispose();
            }

            public void Commit()
            {
                using (StartContext(Entry.CommitTransaction))
                    _trans.Commit();
            }

            public void Rollback()
            {
                using (StartContext(Entry.RollbackTransaction))
                    _trans.Rollback();
            }

            public IDbConnection Connection
            {
                get { return _trans.Connection; }
            }

            public IsolationLevel IsolationLevel
            {
                get { return _trans.IsolationLevel; }
            }

            public IDbTransaction GetDecoratedTransaction()
            {
                return _trans;
            }
        }
        class ProfiledConnection : IDbConnection, IConnectionWrapper
        {
            IDbConnection _connection;

            public ProfiledConnection(IDbConnection connection)
            {
                _connection = connection;

            }

            public IDbTransaction BeginTransaction()
            {
                if (DoNotProfile())
                    return _connection.BeginTransaction();
                else
                    using (StartContext(Entry.CnstBeginTransaction))
                        return new ProfiledTransaction(_connection.BeginTransaction());
            }



            public IDbTransaction BeginTransaction(IsolationLevel il)
            {
                if (DoNotProfile())
                    return _connection.BeginTransaction(il);
                else
                    using (StartContext(Entry.CnstBeginTransaction))
                        return new ProfiledTransaction(_connection.BeginTransaction(il));

            }

            public void ChangeDatabase(string databaseName)
            {
                _connection.ChangeDatabase(databaseName);
            }

            public void Close()
            {
                _connection.Close();
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

            public IDbCommand CreateCommand()
            {
                if (DoNotProfile())
                    return _connection.CreateCommand();
                else
                    return new ProfilingDbCommand(_connection.CreateCommand());
            }

            class ProfilingDbCommand : IDbCommand, IDbCommandWrapper
            {
                IDbCommand _command;

                public ProfilingDbCommand(IDbCommand command)
                {
                    _command = command;
                    if (TrackCommandsAndReaders)
                    {
                        _trackedCommandsAndReaders.Value.Add(this);
                    }
                }


                public void Cancel()
                {
                    _command.Cancel();
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

                public IDbConnection Connection
                {
                    get { return _command.Connection; }
                    set { _command.Connection = value; }
                }

                public IDbDataParameter CreateParameter()
                {
                    return _command.CreateParameter();
                }

                public void Dispose()
                {
                    _command.Dispose();
                    if (TrackCommandsAndReaders)
                    {
                        _trackedCommandsAndReaders.Value.Remove(this);
                    }
                }

                IDisposable StartContext(string what)
                {

                    return Profiler.StartContext(what + "\r\n" + CommandToString());
                }

                private string CommandToString()
                {
                    return _command.CommandText + "\r\n" +
                                                            LogDatabaseWrapper.ParameterInfo("Parameters",
                                                                _command.Parameters);
                }
                public override string ToString()
                {
                    return "Command: " + CommandToString();
                }

                public int ExecuteNonQuery()
                {
                    using (StartContext("SQL"))
                        try
                        {
                            return _command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            using (StartContext("Exception: " + ex.Message))
                            {
                            };
                            throw;
                        }
                }

                public IDataReader ExecuteReader()
                {
                    using (StartContext("SQL"))
                        try
                        {
                            return WrapReader(_command.ExecuteReader());
                        }
                        catch (Exception ex)
                        {
                            using (StartContext("Exception: " + ex.Message))
                            {
                            };
                            throw;
                        }
                }
                IDataReader WrapReader(IDataReader r)
                {
                    if (TrackCommandsAndReaders)
                        return new TrackedDataReader(r, CommandToString());
                    return r;
                }

                public IDataReader ExecuteReader(CommandBehavior behavior)
                {
                    if (behavior == CommandBehavior.Default)
                        using (StartContext("SQL"))
                            try
                            {
                                return WrapReader(_command.ExecuteReader(behavior));
                            }
                            catch (Exception ex)
                            {
                                using (StartContext("Exception: " + ex.Message))
                                {
                                };
                                throw;
                            }
                    else
                        using (StartContext("SQL" + behavior.ToString()))
                            try
                            {
                                return WrapReader(_command.ExecuteReader(behavior));
                            }
                            catch (Exception ex)
                            {
                                using (StartContext("Exception: " + ex.Message))
                                {
                                };
                                throw;
                            }
                }
                public class TrackedDataReader : IDataReader
                {
                    IDataReader _reader;
                    string _query;



                    public TrackedDataReader(IDataReader original, string query)
                    {
                        _query = query;
                        _reader = original;
                        if (TrackCommandsAndReaders)
                            _trackedCommandsAndReaders.Value.Add(this);


                    }
                    public override string ToString()
                    {
                        return "Reader: " + _query;
                    }

                    public object this[string name]
                    {
                        get
                        {
                            return _reader[name];
                        }
                    }

                    public object this[int i]
                    {
                        get
                        {
                            return _reader[i];
                        }
                    }

                    public int Depth
                    {
                        get
                        {
                            return _reader.Depth;
                        }
                    }

                    public int FieldCount
                    {
                        get
                        {
                            return _reader.FieldCount;
                        }
                    }

                    public bool IsClosed
                    {
                        get
                        {
                            return _reader.IsClosed;
                        }
                    }

                    public int RecordsAffected
                    {
                        get
                        {
                            return _reader.RecordsAffected;
                        }
                    }

                    public void Close()
                    {
                        _reader.Close();
                    }

                    public void Dispose()
                    {
                        _reader.Dispose();
                        if (TrackCommandsAndReaders)
                            _trackedCommandsAndReaders.Value.Remove(this);
                    }

                    public bool GetBoolean(int i)
                    {
                        return _reader.GetBoolean(i);
                    }

                    public byte GetByte(int i)
                    {
                        return _reader.GetByte(i);
                    }

                    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
                    {
                        return _reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
                    }

                    public char GetChar(int i)
                    {
                        return _reader.GetChar(i);
                    }

                    public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
                    {
                        return _reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
                    }

                    public IDataReader GetData(int i)
                    {
                        return _reader.GetData(i);
                    }

                    public string GetDataTypeName(int i)
                    {
                        return _reader.GetDataTypeName(i);
                    }

                    public DateTime GetDateTime(int i)
                    {
                        return _reader.GetDateTime(i);
                    }

                    public decimal GetDecimal(int i)
                    {
                        return _reader.GetDecimal(i);
                    }

                    public double GetDouble(int i)
                    {
                        return _reader.GetDouble(i);
                    }

                    public Type GetFieldType(int i)
                    {
                        return _reader.GetFieldType(i);
                    }

                    public float GetFloat(int i)
                    {
                        return _reader.GetFloat(i);
                    }

                    public Guid GetGuid(int i)
                    {
                        return _reader.GetGuid(i);
                    }

                    public short GetInt16(int i)
                    {
                        return _reader.GetInt16(i);
                    }

                    public int GetInt32(int i)
                    {
                        return _reader.GetInt32(i);
                    }

                    public long GetInt64(int i)
                    {
                        return _reader.GetInt64(i);
                    }

                    public string GetName(int i)
                    {
                        return _reader.GetName(i);
                    }

                    public int GetOrdinal(string name)
                    {
                        return _reader.GetOrdinal(name);
                    }

                    public DataTable GetSchemaTable()
                    {
                        return _reader.GetSchemaTable();
                    }

                    public string GetString(int i)
                    {
                        return _reader.GetString(i);
                    }

                    public object GetValue(int i)
                    {
                        return _reader.GetValue(i);
                    }

                    public int GetValues(object[] values)
                    {
                        return _reader.GetValues(values);
                    }

                    public bool IsDBNull(int i)
                    {
                        return _reader.IsDBNull(i);
                    }

                    public bool NextResult()
                    {
                        return _reader.NextResult();
                    }

                    public bool Read()
                    {
                        return _reader.Read();
                    }




                }
                public object ExecuteScalar()
                {
                    using (StartContext("SQL"))
                        try
                        {
                            return _command.ExecuteScalar();
                        }
                        catch (Exception ex)
                        {
                            using (StartContext("Exception: " + ex.Message))
                            {
                            };
                            throw;
                        }
                }

                public IDataParameterCollection Parameters
                {
                    get { return _command.Parameters; }
                }

                public void Prepare()
                {
                    _command.Prepare();
                }

                public IDbTransaction Transaction
                {
                    get { return _command.Transaction; }
                    set
                    {
                        var val = value as ProfiledTransaction;
                        if (val != null)
                            value = val.GetDecoratedTransaction();
                        _command.Transaction = value;
                    }
                }

                public UpdateRowSource UpdatedRowSource
                {
                    get { return _command.UpdatedRowSource; }
                    set { _command.UpdatedRowSource = value; }
                }

                public IDbCommand GetOriginalCommand()
                {
                    return _command;
                }


            }

            public string Database
            {
                get { return _connection.Database; }
            }

            public void Dispose()
            {
                _connection.Dispose();
            }

            public void Open()
            {
                using (StartContext("Open Connection"))
                    _connection.Open();
            }

            public IDbConnection GetInnerConnection()
            {
                return _connection;
            }

            public ConnectionState State
            {
                get { return _connection.State; }
            }
        }

        public static void InitMenu(System.Windows.Forms.ToolStripMenuItem profiling)
        {
            if (profiling.DropDownItems.Count > 0)
                return;
            var start = new ENV.UI.SuspendedToolStripMenuItem();
            profiling.DropDownItems.Add(start);

            var open = new ENV.UI.SuspendedToolStripMenuItem() { Text = "Open Profiling Session" };
            profiling.DropDownItems.Add(open);
            open.Click += delegate
            {
                ProfilerCore.Open();
            };




            start.Click += delegate
            {
                Firefly.Box.UI.Form.RunInActiveLogicContext(() => ToggleProfiling());
            };


            SetToolstripText(start);
            Action x = () => SetToolstripText(start);
            ProfilingStateChanged += x;
            start.Disposed += delegate { ProfilingStateChanged -= x; };

        }

        static string SetToolstripText(ToolStripItem y)
        {
            if (_disableMenuSet)
                return "";
            string result = "";
            Firefly.Box.Context.Current.InvokeUICommand(() => result = y.Text = DoNotProfile() ? "Start Profiling" : "End Profiling");
            return result;
        }
        public static string TempProfilerFilesPath { get; set; }
        static bool _userIsProfiling = false;
        static void ToggleProfiling()
        {
            if (DoNotProfile())
            {
                _userIsProfiling = true;
                foreach (var c in Firefly.Box.Context.ActiveContexts)
                {
                    c.BeginInvoke(StartUserProfilingSession);
                }
            }
            else
                foreach (var c in Firefly.Box.Context.ActiveContexts)
                {
                    _userIsProfiling = false;
                    c.BeginInvoke(() => EndProfilingSession());
                }
        }
        internal static void ThreadIsClosing()
        {
            if (_userIsProfiling)
                EndProfilingSession();
        }

        private static void StartUserProfilingSession()
        {
            Start();
            var x = TempProfilerFilesPath ?? "";
            x += "Profiler_" + Date.Now.ToString("YYYYMMDD") + "T" + Time.Now.ToString("HHMMSS") + ENV.UserMethods.Instance.CtxGetName() + ".prof";
            x = PathDecoder.DecodePath(x);
            AutoSaveTimer(x);
        }

        static bool _setupToolstrip = false;
        public static void InitToolStrip(ToolStrip toolbar)
        {
            Action setup = () =>
            {
                if (_setupToolstrip)
                    return;
                Firefly.Box.Context.Current.InvokeUICommand(() =>
                {
                    _setupToolstrip = true;
                    var tsb = new ToolStripButton() { Text = "ASFDSA" };
                    tsb.Click += delegate
                    {
                        Firefly.Box.UI.Form.RunInActiveLogicContext(() => ToggleProfiling());
                    };
                    toolbar.Items.Add(tsb);
                    SetToolstripText(tsb);
                    Action setState = () => SetToolstripText(tsb);
                    ProfilingStateChanged += setState;
                    toolbar.Disposed += delegate { ProfilingStateChanged -= setState; };
                });
            };
            ProfilingStateChanged += setup;
            toolbar.Disposed += delegate { ProfilingStateChanged -= setup; };
            if (!DoNotProfile())
                ProfilingStateChanged();

        }









        public static void CloseApplication()
        {
            if (DoNotProfile())
                return;
            EndProfilingSession(ProfilerFile);
        }


        public static IDisposable EntityCommand(Entity entity, string command)
        {
            if (DoNotProfile())
                return DummyDisposable.Instance;
            return new DoubleDisposable(new EntryDisposable("M:" + entity.GetType().FullName) { JustATitle = true }, StartContext(command));
        }

        class DoubleDisposable : IDisposable
        {
            IDisposable a;
            IDisposable b;

            public DoubleDisposable(IDisposable a, IDisposable b)
            {
                this.a = a;
                this.b = b;
            }



            public void Dispose()
            {

                b.Dispose();
                a.Dispose();
            }
        }

        public static void Clear()
        {
            _contextDoNotProfile.Value = true;
            _generalDoNotProfile = true;
        }

        public static IDisposable StartContextAndSaveOnEnd(Func<string> topNodeInfo, Func<string> addToFileName)
        {
            if (string.IsNullOrEmpty(_profilerFile))
                return DummyDisposable.Instance;
            var x = _profilerFile;
            var dir = System.IO.Path.GetDirectoryName(x);
            var ext = System.IO.Path.GetExtension(x);
            if (string.IsNullOrEmpty(ext))
                ext = "prof";
            x = System.IO.Path.GetFileNameWithoutExtension(x);
            x = System.IO.Path.Combine(dir, x + addToFileName() + "." + ext);
            Start();
            AutoSaveTimer(x);
            return new saveSessionDisposable(x, StartContext(topNodeInfo()));
        }

        class saveSessionDisposable : IDisposable
        {
            string _fileName;
            IDisposable _disp;

            public saveSessionDisposable(string fileName, IDisposable disp)
            {
                _fileName = fileName;
                _disp = disp;
            }



            public void Dispose()
            {
                _disp.Dispose();
                var e = End1();
                try
                {
                    e.Save(_fileName);
                }
                catch (Exception ex)
                {
                    ENV.ErrorLog.WriteToLogFile(ex,
                        string.Format("Failed to save Profiler file '{0}', {1}", _fileName, ex.Message));

                }

            }
        }

        static bool _writingToLog = false;
        static ContextStatic<Exception> LastWrittenException = new ContextStatic<Exception>(() => null);
        public static void WriteToLogFile(Exception exception, string extraInfo, object[] args)
        {
            if (DoNotProfile())
                return;
            if (_writingToLog)
                return;
            if (LastWrittenException.Value == exception)
                return;
            LastWrittenException.Value = exception;
            try
            {

                _writingToLog = true;
                using (StartContext("Exception"))
                {
                    StartContext(ErrorLog.CreateErrorDescription(exception, extraInfo, args)).Dispose();
                }
                ForceSaveCurrentProfilerFile();

            }
            finally
            {
                _writingToLog = false;
            }

        }

    }
    public interface IProfilerEntry
    {
        string Name { get; }
        long Count { get; }
        IProfilerEntry Parent { get; }
    }
}
