using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ProfilerUI;

namespace ENV.Utilities
{
    public class ProfilerCore
    {
        public static void Open()
        {
            var d = new System.Windows.Forms.OpenFileDialog();
            d.DefaultExt = "prof";
            d.Filter = "Profiler Files|*.prof;*.proftrace|All files|*.*";
            d.AddExtension = true;
            d.RestoreDirectory = true;
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Open(d.FileName);
            }
        }
        public static void Open(string fileName)
        {
            try
            {


                if (System.IO.Path.GetExtension(fileName).Equals(".proftrace", StringComparison.InvariantCultureIgnoreCase))
                {
                    new TraceViewer(fileName).Show();
                }
                else
                {
                    char[] chars = new char[50];
                    using (var r = new System.IO.StreamReader(fileName, System.Text.Encoding.UTF8))
                    {

                        r.ReadBlock(chars, 0, 50);

                    }
                    if (new string(chars).StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>"))
                    {
                        Entry.Load(fileName).Display(true, fileName);
                        return;
                    }





                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
    }
    internal class Entry
    {
        static long startTicks = DateTime.Now.Ticks - Stopwatch.GetTimestamp();
        static long currentTicks() => startTicks + Stopwatch.GetTimestamp();
        public string Name;
        public long Count;
        public long Duration;
        public bool IsUserTime;
        public void AddDuration(long what)
        {
            Duration += what;
            _endTicks = currentTicks();
        }
        long _startTicks = currentTicks(), _endTicks;
        private void merge(Entry e)
        {
            Count += e.Count;
            Duration += e.Duration;
            _endTicks = e._endTicks;
        }

        public const string CommitTransaction = "Commit Transaction", RollbackTransaction = "Rollback Transaction", CnstBeginTransaction = "Begin Transaction";


        internal Dictionary<string, Entry> _children = new Dictionary<string, Entry>();
        [NonSerialized]
        internal long _calculatedCount;
        [NonSerialized]
        long _childDuration;
        [NonSerialized]
        long _dbDuration;

        [NonSerialized]
        string _shortName;

        public void Display(bool saved, string name = null)
        {
            bool wasSaved = saved;
            Show2(() => wasSaved = Save(), () => wasSaved, name);
        }
        string GetName(string parentName)
        {
            return GetShortName(parentName, Name);
        }
        internal static string GetShortName(string parentName, string Name)
        {
            string name = Name;
            if (parentName != null && name.StartsWith(parentName) && name.Length > parentName.Length + 1)
                name = name.Substring(parentName.Length + 1);
            name = name.Trim();


            {
                if (name.Contains("+"))
                {
                    name = name.Substring(name.LastIndexOf('+'));
                }
                if (name.StartsWith("M:"))
                {
                    name = "M: " + name.Substring(name.LastIndexOf('.') + 1);
                }
                if (name.StartsWith("DynamicSQLEntity"))
                { }
                else if (name.Contains("."))
                {
                    int dot = name.LastIndexOf('.');
                    int parens = name.IndexOf('(');
                    var enter = name.IndexOf("\n");
                    if ((dot < parens || parens < 0) && (dot < enter || enter < 0) && !name.Contains(" "))
                        name = name.Substring(dot + 1);
                }
            }
            return name;
        }

        const int _maxChildren = 100;
        internal Entry Get(string entry, out bool foundInParent)
        {
            Entry child;

            if (!(foundInParent = _children.TryGetValue(entry, out child)))
            {
                child = new Entry() { Name = entry };
            }

            return child;
        }
        Entry _fastest, _moreOfTheSame;
        void findFastest()
        {
            _fastest = null;
            foreach (var item in _children.Values)
            {
                if (item != _moreOfTheSame)
                {
                    if (_fastest == null || _fastest.Duration > item.Duration)
                        _fastest = item;


                }

            }
        }

        public void AddItem(Entry entry)
        {

            lock (_children)
            {
                if (_children.Count >= _maxChildren)
                {

                    if (_moreOfTheSame == null)
                    {
                        if (entry.Duration < _fastest.Duration)
                        {
                            entry.Name = "more of the same";
                            _moreOfTheSame = entry;
                        }
                        else
                        {
                            _fastest.Name = "more of the same";
                            _moreOfTheSame = _fastest;
                            findFastest();
                        }
                    }
                    else
                    {
                        if (_fastest.Duration > entry.Duration)
                        {
                            _moreOfTheSame.merge(entry);
                            return;
                        }
                        else
                        {
                            _moreOfTheSame.merge(_fastest);
                            _children.Remove(_fastest.Name);
                            findFastest();
                        }
                    }
                }
                if (_fastest == null)
                {
                    _fastest = entry;
                }
                else
                {
                    if (entry.Duration < _fastest.Duration && entry != _moreOfTheSame)
                        _fastest = entry;
                }
                _children.Add(entry.Name, entry);
            }

        }



        static internal Action<Action> _wrapCallToShow = y => y();
        public void Show2(Func<bool> save, Func<bool> saved, string name = null)
        {
            _wrapCallToShow(() =>
            {
                var st = new ProfilerTree("Firefly", save, saved, name);

                foreach (var c in _children.Values.OrderByDescending(i => i.Duration))
                {
                    st.AddChild(x => c.PopulateTreeItem(x, Name, Duration), Duration);
                }

                st.Show();
            });
        }









        private void PopulateTreeItem(TreeItem tn, string parentName, long totalDuration)
        {
            tn.Set(Name, GetName(parentName), Count, Duration, Name.StartsWith("M:") || Name.Contains("DynamicSQLEntity") || Name == CommitTransaction || parentName == RollbackTransaction || parentName == CnstBeginTransaction, _startTicks, _endTicks, IsUserTime);

            foreach (var item in _children.Values)
            {
                tn.AddChild(x => item.PopulateTreeItem(x, Name, totalDuration));
            }
            if (Name.StartsWith("Exception"))
                tn.PaintNode(System.Drawing.Color.Red, false);
        }




        public bool HasChildern()
        {
            return _children.Count > 0;
        }



        bool Save()
        {
            var d = new System.Windows.Forms.SaveFileDialog();
            d.DefaultExt = "prof";
            d.Filter = "Profiling session|*.prof";
            d.AddExtension = true;
            d.RestoreDirectory = true;
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    Save(d.FileName);
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return false;
        }

        internal void Save(string fileName)
        {
            lock (this)
            {
                using (var wr = System.Xml.XmlWriter.Create(fileName))
                {
                    WriteTo(wr);
                }
            }

            return;

            var b = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var ms = new System.IO.MemoryStream())
            {
                b.Serialize(ms, this);

                System.IO.File.WriteAllBytes(fileName, ms.ToArray());
            }
        }
        public static Entry Load(string fileName)
        {
            using (var sr = System.Xml.XmlReader.Create(fileName))
            {
                sr.Read();
                sr.Read();
                var ag = new Entry();
                try
                {
                    ag.ReadFrom(sr);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
                return ag;

            }
        }

        void WriteTo(XmlWriter wr)
        {

            wr.WriteStartElement("I");


            var x = Name;
            try { x = XmlConvert.VerifyXmlChars(Name); }
            catch
            {
                var sb = new StringBuilder();
                foreach (var item in Name.ToCharArray())
                {
                    try
                    {
                        sb.Append(XmlConvert.VerifyXmlChars(item.ToString()));
                    }
                    catch
                    {
                        sb.Append("<Char_" + (int)item + ">");
                    }
                }
                sb.Append("    >> Note some invalid characters where encoded by the profiler");
                x = sb.ToString();
            }



            wr.WriteAttributeString("N", x);

            wr.WriteAttributeString("D", Duration.ToString());
            wr.WriteAttributeString("C", Count.ToString());
            wr.WriteAttributeString("S", _startTicks.ToString());
            wr.WriteAttributeString("E", _endTicks.ToString());
            if (IsUserTime)
                wr.WriteAttributeString("U", "T");
            lock (_children)
                foreach (var child in _children)
                {
                    child.Value.WriteTo(wr);
                }
            wr.WriteEndElement();
        }
        static long getLongAttribute(XmlReader sr, string key)
        {

            var x = sr.GetAttribute(key);
            if (!string.IsNullOrEmpty(x))
                return long.Parse(x);
            return 0;
        }

        public void ReadFrom(XmlReader sr)
        {
            Name = sr.GetAttribute("N");
            Duration = long.Parse(sr.GetAttribute("D"));
            Count = long.Parse(sr.GetAttribute("C"));
            _startTicks = getLongAttribute(sr, "S");
            _endTicks = getLongAttribute(sr, "E");
            IsUserTime = sr.GetAttribute("U") == "T";
            long childDuration = 0;
            if (!sr.IsEmptyElement)

                while (sr.Read())
                {
                    switch (sr.NodeType)
                    {

                        case XmlNodeType.EndElement:
                            if (childDuration > Duration)
                                Duration = childDuration;
                            return;
                        case XmlNodeType.Element:
                            {
                                var ag = new Entry();
                                try
                                {
                                    ag.ReadFrom(sr);
                                }
                                finally
                                {
                                    _children.Add(ag.Name, ag);
                                    childDuration += ag.Duration;
                                }
                            }
                            break;

                    }

                }
        }
    }
    public class TraceViewer
    {
        class TraceItem
        {
            public string Name;
            public long Id;
            public DateTime Start, End;
            public TraceItem Parent;
            public List<TraceItem> Children = new List<TraceItem>();
            long Duration { get { return (long)((End - Start).TotalMilliseconds * TimeSpan.TicksPerMillisecond); } }
            public override string ToString()
            {
                var pn = "";
                if (Parent != null)
                    pn = Parent.Name;
                return Entry.GetShortName(pn, Name);
            }

            internal void Populate(ProfilerTree st)
            {
                foreach (var c in Children)
                {
                    st.AddChild(x => c.PopulateTreeItem(x, Name, c.Duration), c.Duration);
                }
            }
            string GetName()
            {
                var r = Name;
                r += "\r\nStart: " + Start.ToString("hh:mm:ss.ffffff");
                r += "\r\nEnd: " + End.ToString("hh:mm:ss.ffffff");
                return r;
            }
            private void PopulateTreeItem(TreeItem tn, string parentName, long duration)
            {
                tn.Set(GetName(), Entry.GetShortName(parentName, Name), 0, Duration, Name.StartsWith("M:") || Name.Contains("DynamicSQLEntity") || Name == Entry.CommitTransaction || parentName == Entry.RollbackTransaction || parentName == Entry.CnstBeginTransaction, 0, 0, false);

                foreach (var item in Children)
                {
                    tn.AddChild(x => item.PopulateTreeItem(x, Name, Duration));
                }
                if (Name.StartsWith("Exception"))
                    tn.PaintNode(System.Drawing.Color.Red, false);
            }
        }
        public static long TicksPerMillisecond = TimeSpan.TicksPerMillisecond;
        DateTime _start;
        TraceItem _root;
        string _fileName;
        public TraceViewer(string fileName)
        {
            _fileName = fileName;
            _root = new Utilities.TraceViewer.TraceItem();
            var current = _root;

            var xml = string.Format("<root>{0}</root>", System.IO.File.ReadAllText(fileName));
            using (var sr = new System.Xml.XmlTextReader(new System.IO.StringReader(xml)))
            {
                sr.Read();
                try
                {
                    while (sr.Read())
                    {
                        switch (sr.NodeType)
                        {
                            case XmlNodeType.Element:
                                switch (sr.Name)
                                {
                                    case "Sync":
                                        try
                                        {
                                            _start = DateTime.Parse(sr.GetAttribute("datetime"));
                                        }
                                        catch { }
                                        break;
                                    case "s":
                                        {
                                            string name = sr.GetAttribute("n");
                                            long id = long.Parse(sr.GetAttribute("id"));
                                            long ticks = long.Parse(sr.GetAttribute("t"));
                                            var e = new TraceItem
                                            {
                                                Name = name,
                                                Start = _start.AddMilliseconds(ticks / TicksPerMillisecond),
                                                Id = id,
                                                Parent = current
                                            };
                                            current.Children.Add(e);
                                            current = e;

                                        }
                                        break;
                                    case "e":
                                        {
                                            long id = long.Parse(sr.GetAttribute("id"));
                                            long ticks = long.Parse(sr.GetAttribute("t"));

                                            current.End = _start.AddMilliseconds(ticks / TicksPerMillisecond);
                                            current = current.Parent;
                                            if (current == null)
                                                current = new TraceItem() { Name = "unknown" };
                                        }
                                        break;

                                }
                                break;
                        }
                    }
                }
                catch { }
            }
        }

        public void Show()
        {

            Entry._wrapCallToShow(() =>
            {
                var st = new ProfilerTree("Firefly", null, null, _fileName) { Trace = true };
                _root.Populate(st);
                st.Show();

            });

        }
    }
}
