using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace ProfilerUI
{
    interface TreeItem
    {
        void Set(string name, string shortName, long calls, long duration, bool isDb, long start, long end, bool isUserTime);

        void AddChild(Action<TreeItem> p);
        void PaintNode(Color red, bool isDb);
    }
    public class ProfilerTree
    {
        public static bool StandAloneApp = false;

        TreeView _tv = new TreeView() { Dock = DockStyle.Fill };
        Form _form;
        bool _isMagic;
        public bool Trace { get; set; }
        bool _hideUserDuration = true;
        bool _sortByTime = false;
        void setSorterAndSort()
        {
            var selectedNode = _tv.SelectedNode;
            _tv.TreeViewNodeSorter = new Sorter(_sortByTime, _hideUserDuration);
            _tv.Sort();
            _tv.SelectedNode = selectedNode;
        }


        void calcAndSetText()
        {
            _tv.BeginUpdate();

            long total = 0;

            if (_hideUserDuration)
                foreach (TreeNodeBridgeToTreeItem node in _tv.Nodes)
                {
                    total += node._duration - node._userDuration;
                }
            foreach (TreeNodeBridgeToTreeItem node in _tv.Nodes)
            {
                node.SetTextRecursive(total, _hideUserDuration);
            }

            setSorterAndSort();
            _tv.EndUpdate();
        }
        public ProfilerTree(string profilerTypeName, Func<bool> save, Func<bool> saved, string name = null)
        {
            _tv.Font = new Font(_tv.Font.Name, 12);
            _isMagic = profilerTypeName.TrimEnd().ToUpper() == "MAGIC";
            var f = new System.Windows.Forms.Form() { Width = 800, Height = 600 };
            _form = f;
            f.Load += delegate { f.Focus(); };
            f.Text = profilerTypeName + " Profiler ";
            if (!string.IsNullOrEmpty(name))
                f.Text += name;

            f.ShowInTaskbar = true;

            f.Controls.Add(_tv);

            var cms = new ContextMenuStrip();
            _tv.ContextMenuStrip = cms;

            Action view = () =>
            {
                if (_tv.SelectedNode == null)
                    return;
                var ag = _tv.SelectedNode as TreeNodeBridgeToTreeItem;
                if (ag == null)
                    return;
                var fd = new System.Windows.Forms.Form() { Text = _tv.SelectedNode.Text, Width = 800, Height = 600, ShowInTaskbar = true };
                var tb = new TextBox
                {
                    Text = ag.GetDetails(),
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Multiline = true,
                    ScrollBars = ScrollBars.Both,
                    Font = new System.Drawing.Font("Consolas", 10),
                    WordWrap = false
                };

                var b = new Button();
                b.Click += delegate { fd.Close(); };
                fd.CancelButton = b;
                fd.Controls.Add(tb);
                fd.Show();
            };

            Search search = new Search(_tv);
            _tv.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    view();
                }
                else if (e.Control && e.KeyCode == Keys.S && save != null)
                {
                    save();
                    e.SuppressKeyPress = true;
                }
                else if (e.Control && e.KeyCode == Keys.F)
                {
                    search.Find();
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.F3)
                {
                    if (e.Shift)
                        search.FindPrev();
                    else
                        search.FindNext();
                    e.SuppressKeyPress = true;
                }

            };

            _tv.ShowNodeToolTips = true;

            var ts = cms.Items.Add("Details");
            ts.Click += delegate { view(); };
            if (!_isMagic)
            {
                var copy = cms.Items.Add("Copy Path");
                copy.Click += delegate
                {
                    var ag = _tv.SelectedNode as TreeNodeBridgeToTreeItem;
                    if (ag == null)
                        return;
                    ag.CopyPath();

                };
            }

            var stats = cms.Items.Add("Statistics");
            stats.Click += delegate
            {
                if (_tv.SelectedNode == null)
                    return;
                var ag = _tv.SelectedNode as TreeNodeBridgeToTreeItem;
                if (ag == null)
                    return;
                var s = new Stats(ag._name);
                s.SearchNodes(_tv.Nodes);
                s.DisplayResult();

            };
            if (save != null)
            {
                ts = cms.Items.Add("Save");
                ts.Click += delegate { save(); };
            }


            f.FormClosing += (s, e) =>
            {
                if (save == null || saved == null || saved())
                    return;
                var r =
                System.Windows.Forms.MessageBox.Show(
                     "Would you like to save the profiling session?", "Save Profiling Session", MessageBoxButtons.YesNo);
                if (r == DialogResult.Cancel)
                    e.Cancel = true;
                else if (r == DialogResult.Yes)
                {
                    if (!save())
                        e.Cancel = true;
                }
            };


            var hideUserDurationItem = cms.Items.Add(_hideUserDuration ? "Show user duration" : "Hide user duration");
            hideUserDurationItem.Click += delegate
            {
                _hideUserDuration = !_hideUserDuration;
                hideUserDurationItem.Text = _hideUserDuration ? "Show user duration" : "Hide user duration";
                calcAndSetText();

            };


            var sortContextItem = cms.Items.Add("Sort by Chronological order");
            sortContextItem.Click += delegate
            {
                _sortByTime = !_sortByTime;
                sortContextItem.Text = "Sort by " + (_sortByTime ? "Duration" : "Chronological order");
                setSorterAndSort();

            };
            DateTime lastOneFound = DateTime.MaxValue;
            var test = cms.Items.Add("Find last Operation");
            test.Click += delegate
            {
                var start = DateTime.MinValue;
                TreeNodeBridgeToTreeItem node = null;
                Action<TreeNodeCollection> SearchNodes = null;
                SearchNodes = (TreeNodeCollection nodes) =>
                {
                    foreach (TreeNodeBridgeToTreeItem item in nodes)
                    {
                        if (item._start > start && item._start < lastOneFound)
                        {
                            start = item._start;
                            node = item;
                        }
                        SearchNodes(item.Nodes);
                    }
                };
                SearchNodes(_tv.Nodes);
                if (node != null)
                {
                    _tv.SelectedNode = node;
                    lastOneFound = node._start;
                }

            };


            var sb = cms.Items.Add("Find");
            sb.Click += delegate { search.Find(); };

            if (_isMagic)
            {
                var footerMenu = new MenuStrip();
                footerMenu.Dock = DockStyle.Bottom;
                _form.Controls.Add(footerMenu);

                footerMenu.Items.Add("Watch Video Tutorial").Click +=
                    (sender, args) => Process.Start("http://goto.fireflymigration.com/MagicProfilerVideoTutorial");
                footerMenu.Items.Add(new ToolStripSeparator());
                footerMenu.Items.Add("Check for Updates").Click +=
                    (sender, args) => Process.Start("https://github.com/FireflyMigration/MagicProfiler/releases");
                footerMenu.Items.Add(new ToolStripSeparator());
                footerMenu.Items.Add("Give Feedback").Click += (sender, args) =>
                    Process.Start("https://github.com/FireflyMigration/MagicProfiler/issues");
                var visitFirefly = footerMenu.Items.Add("Visit www.fireflymigration.com");
                visitFirefly.Alignment = ToolStripItemAlignment.Right;
                visitFirefly.Click += (sender, args) => Process.Start("http://www.fireflymigration.com/en/Default?utm_source=MagicProfiler&utm_medium=link&utm_campaign=MagicProfiler");
            }
        }
        class Sorter : IComparer
        {
            bool _byTime;
            bool _hideUserTime;
            public Sorter(bool byTime, bool hideUserTime)
            {
                _hideUserTime = hideUserTime;
                _byTime = byTime;
            }
            public int Compare(object x, object y)
            {
                var a = x as TreeNodeBridgeToTreeItem;
                if (a != null)
                {
                    return a.compareTo(y, _byTime, _hideUserTime);
                }
                return -1;
            }
        }

        internal void RunApp()
        {
            System.Windows.Forms.Application.Run(_form);
        }

        internal void AddChild(Action<TreeItem> p, long totalDuration)
        {
            var x = new TreeNodeBridgeToTreeItem(totalDuration, Trace);
            p(x);
            x.CompleteCalc();
            _tv.Nodes.Add(x);
            if (_tv.Nodes.Count == 1)
                x.Expand();


        }
        public void Show()
        {
            calcAndSetText();
            if (StandAloneApp)
                Application.Run(_form);
            else
                _form.Show();
        }

        class Search
        {
            string _searchString;
            bool _searchDetails;
            bool _prev;
            TreeView _treeView;
            bool _foundCurrent = false;
            bool _reverse = false;

            public Search(TreeView tc)
            {
                _treeView = tc;
            }

            public void Find()
            {
                var f = new System.Windows.Forms.Form() { Text = "Find", Width = 370, Height = 140 };
                var searchString = new TextBox() { Left = 71, Top = 11, Width = 191, Height = 19, Text = _searchString };
                var searchDetails = new CheckBox() { Text = "Search in details", Left = 5, Top = searchString.Bottom + 5, Checked = _searchDetails };
                var prev = new CheckBox() { Text = "Search up", Left = 5, Top = searchDetails.Bottom + 3, Checked = _prev };

                var find = new Button { Text = "Find Next", Top = 11, Left = 274 };
                var cancel = new Button { Text = "Cancel", Top = find.Bottom + 5, Left = 274 };
                f.AcceptButton = find;
                f.CancelButton = cancel;
                find.Click += delegate
                {
                    _searchString = searchString.Text;
                    _searchDetails = searchDetails.Checked;
                    _prev = prev.Checked;
                    f.Close();
                    FindNext();
                };
                f.Controls.AddRange(new System.Windows.Forms.Control[] { new Label { Text = "Find what:", Left = 5, Top = 11, AutoSize = true }, searchString, searchDetails, prev, find, cancel });
                f.ShowDialog();
            }


            public bool FindNext()
            {
                return SearchIt(false);
            }

            public bool FindPrev()
            {
                return SearchIt(true);
            }

            bool SearchIt(bool reverse)
            {
                _reverse = reverse;
                _foundCurrent = false;
                if (!string.IsNullOrEmpty(_searchString))
                    if (!SearchOn(_treeView.Nodes))
                    {
                        MessageBox.Show("Match not found");
                    }
                    else

                        return true;
                return false;
            }


            bool SearchOn(TreeNodeCollection nodes)
            {
                IEnumerable x = nodes;
                if (_prev ^ _reverse)
                {
                    var y = new ArrayList(nodes);
                    y.Reverse();
                    x = y;

                }
                foreach (TreeNode node in x)
                {
                    if (Found(node))
                        return true;
                }
                return false;

            }

            bool Found(TreeNode node)
            {
                if (!(_prev ^ _reverse))
                {
                    if (IsItMe(node))
                        return true;
                    return SearchOn(node.Nodes);
                }
                else
                {
                    if (SearchOn(node.Nodes))
                        return true;
                    return IsItMe(node);
                }

            }

            bool IsItMe(TreeNode node)
            {
                if (!_foundCurrent)
                {
                    if (node == _treeView.SelectedNode)
                        _foundCurrent = true;
                }
                else
                {
                    var ag = node as TreeNodeBridgeToTreeItem;
                    var x = ag._shortName;
                    if (_searchDetails)
                        x = ag._name;
                    if (x.IndexOf(_searchString.Trim(), StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        _treeView.SelectedNode = node;
                        return true;
                    }
                }
                return false;

            }
        }
        class Stats
        {
            string _nameToSearch;
            long Duration;
            long _dbDuration;
            long _childDuration;
            long _calculatedCount;
            long _places;
            public Stats(string nameToSearchFor)
            {
                _nameToSearch = nameToSearchFor;
            }
            public void SearchNodes(TreeNodeCollection nodes)
            {
                foreach (TreeNode item in nodes)
                {
                    var e = item as TreeNodeBridgeToTreeItem;
                    if (e != null && e._name == _nameToSearch)
                    {
                        _places++;
                        _calculatedCount += e._calculatedCount;
                        _childDuration += e._childDuration;
                        _dbDuration += e._dbDuration;
                        Duration += e._duration;
                    }
                    SearchNodes(item.Nodes);
                }
            }
            public void DisplayResult()
            {
                using (var sw = new System.IO.StringWriter())
                {
                    sw.WriteLine("Statistics for " + _nameToSearch);
                    sw.WriteLine("-----------------------------------------------");
                    sw.WriteLine(_places + " places");
                    sw.WriteLine(_calculatedCount + " calls");
                    if (Duration > 0)
                        sw.WriteLine("Time: " + DurationToString(Duration));
                    if (_dbDuration > 0 && _dbDuration != Duration)
                        sw.WriteLine("DB Time: " + DurationToString(_dbDuration) + " (" + PercentToString(_dbDuration, Duration) + ")");
                    if (_childDuration > 0)
                        sw.WriteLine("Own Time: " + DurationToString(Duration - _childDuration));
                    System.Windows.Forms.MessageBox.Show(sw.ToString(), "Statistics for " + _nameToSearch, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        public static string DurationToString(long duration)
        {
            var x = ((decimal)duration / TimeSpan.TicksPerMillisecond);
            if (x < 10)
                return x.ToString("0.00") + " ms";
            return x.ToString("0,0").Trim() + " ms";
        }
        static string PercentToString(long part, long total)
        {
            if (total == 0)
                return "";
            return Math.Round(part * 100m / total, 2) + "%";
        }
        class TreeNodeBridgeToTreeItem : System.Windows.Forms.TreeNode, TreeItem
        {


            long _totalDuration;
            bool _trace;
            public TreeNodeBridgeToTreeItem(long totalDuration, bool trace)
            {
                _totalDuration = totalDuration;
                _trace = trace;
            }

            public long _childCalls, _childDuration, _dbDuration, _userDuration, _calculatedCount;

            List<TreeNodeBridgeToTreeItem> _nodes = new List<TreeNodeBridgeToTreeItem>();

            public void AddChild(Action<TreeItem> p)
            {
                var tnb = new TreeNodeBridgeToTreeItem(_totalDuration, _trace);
                p(tnb);
                tnb.CompleteCalc();
                _nodes.Add(tnb);

            }

            public void CompleteCalc()
            {
                if (!_trace)
                    _nodes.Sort((a, b) =>
                    {
                        int i = b._duration.CompareTo(a._duration);
                        if (i != 0)
                            return i;
                        i = b._count.CompareTo(a._count);
                        if (i != 0)
                            return i;
                        return a.Name.CompareTo(b.Name);

                    });
                Nodes.AddRange(_nodes.ToArray());

                foreach (TreeNodeBridgeToTreeItem item in Nodes)
                {
                    _childCalls += item._calculatedCount;
                    _childDuration += item._duration;
                    _dbDuration += item._dbDuration;
                    _userDuration += item._userDuration;
                }
                if (_discountOwnTime)
                    _userDuration += this._duration - this._childDuration;

                if (_count == -1)
                    _calculatedCount = _childCalls;
                else
                    _calculatedCount = _count;

                SetNodeText(_totalDuration, false);
                if (_isDb)
                {
                    PaintNode(System.Drawing.Color.Brown, true);

                }


                var tt = GetDetails();
                if (tt.Length > 300)
                    tt = tt.Remove(300) + "...";
                ToolTipText = tt;


            }

            internal void SetNodeText(long totalDuration, bool ignoreUserTime)
            {
                long duration = _duration;
                if (totalDuration == 0)
                    totalDuration = _totalDuration;
                if (ignoreUserTime)
                    duration -= _userDuration;
                string statistis = "";
                if (duration > 0)
                    statistis = DurationToString(duration);
                if (!_trace)
                {
                    if (statistis.Length > 0)
                        statistis += " • ";
                    statistis += _calculatedCount + (_calculatedCount > 1 ? " calls" : " call");
                }

                string text = "";
                if (duration != totalDuration && duration > 0 && !_trace)
                    text = PercentToString(duration, totalDuration) + " • ";
                if (_shortName.Length < 100)
                    text += _shortName + " • " + statistis;
                else
                    text += statistis + " • " + _shortName;
                Text = text;
            }

            public string GetDetails()
            {
                using (var sw = new System.IO.StringWriter())
                {
                    if (_duration > 0)
                        sw.WriteLine("Time: " + DurationToString(_duration) + "    " + _start + " => " + _end);
                    if (_dbDuration > 0 && _dbDuration != _duration)
                        sw.WriteLine("DB Time: " + DurationToString(_dbDuration) + " (" + PercentToString(_dbDuration, _duration) + "%)");
                    if (_childDuration > 0)
                        sw.WriteLine("Own Time: " + DurationToString(_duration - _childDuration));
                    if (_userDuration > 0)
                        sw.WriteLine("User Wait Time: " + DurationToString(_userDuration));


                    sw.WriteLine("Calls: " + _calculatedCount);

                    sw.WriteLine();
                    sw.WriteLine(_name);

                    return sw.ToString();
                }
            }
            public void PaintNode(System.Drawing.Color color, bool isDb)
            {
                ForeColor = color;
                _isDb = true;
                foreach (TreeNodeBridgeToTreeItem item in Nodes)
                {
                    item.PaintNode(color, isDb);
                }
            }


            public string _name, _shortName;
            internal long _duration, _count;
            bool _isDb;
            internal DateTime _start, _end;
            bool _discountOwnTime;
            public void Set(string name, string shortName, long calls, long duration, bool isDb, long start, long end, bool isUserTime)
            {
                _isDb = isDb;
                _discountOwnTime = isUserTime;
                _name = name;
                _count = calls;
                _duration = duration;
                if (duration < 0)
                    duration = 0;
                if (isDb)
                    _dbDuration = duration;
                if (shortName.Length > 200)
                    shortName = shortName.Remove(200) + "...";
                if (shortName.Contains("\r\n"))
                    shortName = shortName.Remove(shortName.IndexOf("\r\n"));
                _shortName = shortName;
                _start = new DateTime(start);
                _end = new DateTime(end);

            }

            internal void CopyPath()
            {
                var x = this;
                while (x._isDb)
                {
                    x = x.Parent as TreeNodeBridgeToTreeItem;
                    if (x == null)
                        return;
                }


                Clipboard.SetText(x._name.Replace("+", "."));
            }

            internal int compareTo(object z, bool byTime, bool hideUserTime)
            {
                var y = z as TreeNodeBridgeToTreeItem;
                if (byTime)
                    return _start.CompareTo(y._start);
                else if (!hideUserTime)
                    return y._duration.CompareTo(_duration);
                else
                    return (y._duration - y._userDuration).CompareTo(_duration - _userDuration);
            }

            internal void SetTextRecursive(long total, bool hideUserDuration)
            {
                this.SetNodeText(total, hideUserDuration);
                foreach (TreeNodeBridgeToTreeItem child in this.Nodes)
                {
                    child.SetTextRecursive(total, hideUserDuration);
                }
            }
        }
    }

    class MagicLogParser
    {
        internal readonly MagicEntry _root = new MagicEntry() { Name = "Root" };
        string _line, _lastLine;
        string _dataLine;
        internal Stack<MagicEntry> _stack = new Stack<MagicEntry>();
        int i = 0;
        public MagicLogParser()
        {
            _stack.Push(_root);
        }
        public MagicLogParser(string s)
        {
            _stack.Push(_root);
            using (var sr = new System.IO.StreamReader(s, System.Text.Encoding.GetEncoding(1255)))
            {

                while (!sr.EndOfStream)
                {
                    try
                    {

                        ReadTheLine(sr.ReadLine());
                    }
                    catch { }
                }
                _line = _lastLine;
                while (_stack.Count > 1)
                    Pop();


            }

        }

        internal void ReadTheLine(string line)
        {
            _line = line;
            i++;
            while (_stack.Count > 0 && _stack.Peek().AutoPop)
                Pop();
            if (line.Trim().StartsWith("STMT: "))
            {
                _dataLine = line;
                ReadSQLStmt();
            }
            else
            {
                _lastLine = line;
                _dataLine = line;
                string type = "";
                if (_dataLine.StartsWith(" <-1>"))
                    _dataLine = line.Substring(5);
                var indexOfMinus = _dataLine.IndexOf('-');
                var indexOfSqB = _dataLine.IndexOf('[');
                if (indexOfSqB < indexOfMinus && indexOfSqB > 0)
                {
                    _dataLine = line.Substring(line.IndexOf("] -") + 3).TrimStart();
                    type = line.Substring(line.IndexOf('[') + 1);
                    type = type.Remove(type.IndexOf("]")).Trim();
                }
                else
                    _dataLine = _dataLine.Substring(indexOfMinus + 1).TrimStart();

                if (type == "Action" || type == "")
                {
                    if (CheckStartTask())
                        return;
                    if (CheckStartsWith("<<Ends Close Task", "<< Fin Fermeture Tגche", "<<endet Prozess zu schliessen", "<< סיים סגירת משימה"))
                    {
                        Pop();
                    }
                    else if (CheckStartsWith("Starts Task", "Starts Record", ">>Starts Handle "))
                    {
                        Push(_dataLine.Substring(7));
                    }

                    else if (CheckStartsWith("Dיmarre le prיfixe de tגche", "Beginnt Prozess Prהfix",
                        "התחל לפני משימה"
                        ))
                        Push("Task Prefix");
                    else if (CheckStartsWith("Dיmarre le suffixe de tגche", "Beginnt Prozess Suffix",
                        "התחל אחרי משימה"))
                        Push("Task Suffix");
                    else if (CheckStartsWith("Dיmarre le prיfixe d'enregistrement", "Beginnt Satz Prהfix",
                        "התחל לפני רשומה"))
                        Push("Record Prefix");
                    else if (CheckStartsWith("Dיmarre le suffixe d'enregistrement", "Beginnt Satz Suffix",
                        "התחל אחרי רשומה"))
                        Push("Record Suffix");

                    else if (CheckStartsWith("Ends Task", "Ends Record", "<<Ends Handle ", "Fin Prיfixe de Tגche",
                        "Fin Suffixe de Tגche", "Fin Prיfixe d'Enreg.", "Fin Suffixe d'Enreg.", "Beendet Satz", "Beendet Prozess",
                        "סיים לפני משימה", "סיים לפני רשומה", "סיים אחרי רשומה", "סיים אחרי משימה"))
                    {
                        Pop();
                    }
                    else if (_dataLine.StartsWith(","))
                    {
                        if (_dataLine.Contains("STMT:"))
                        {
                            ReadSQLStmt();
                        }
                        else
                        {
                            var z = _dataLine.IndexOf(':');
                            if (z > 0)
                            {
                                var indexOfExecute = _dataLine.IndexOf("EXECUTE");

                                if (indexOfExecute < z && indexOfExecute > 0)
                                {
                                    PushSQLStatement(_dataLine.Substring(z + 2));
                                }
                                else
                                {
                                    indexOfExecute = _dataLine.IndexOf("PREPARE");
                                    if (indexOfExecute < z && indexOfExecute > 0)
                                    {
                                        PushSQLStatement(_dataLine.Substring(z + 2));
                                    }
                                }

                            }
                        }
                    }
                    else if (_dataLine.StartsWith("PREPARE") ||
                             _dataLine.StartsWith("EXECUTE"))
                        PushSQLStatement(_dataLine.Substring(_dataLine.IndexOf("SELECT", StringComparison.InvariantCultureIgnoreCase)));



                }
                else
                {
                    Push(type + ": " + _dataLine).AutoPop = true;
                }
            }

        }

        public void Display(string name = null)
        {
            var st = new ProfilerTree("Magic", null, null, name);
            _root.Populate(st);
            st.Show();
        }

        private void ReadSQLStmt()
        {
            var sql = _dataLine.Substring(_dataLine.IndexOf("STMT:") + 6);
            PushSQLStatement(sql);
        }

        private void PushSQLStatement(string sql)
        {
            if (sql.Contains(" FROM "))
            {
                string from = sql.Substring(sql.IndexOf(" FROM ") + 1);
                if (from.Contains(" WHERE"))
                {
                    var where = from.Substring(from.IndexOf(" WHERE ") + 1);
                    where = SubstringUntil(where, "ORDER BY");

                    from = from.Remove(from.IndexOf(" WHERE"));
                    from = SubstringUntil(from, "ORDER BY");
                    from = SubstringUntil(from, " WITH ");
                    var x = Push(from.Trim(), true).AutoPop = true;
                    Push(where).AutoPop = true;
                    Push(sql).AutoPop = true;
                }
                else
                {
                    from = SubstringUntil(from, "ORDER BY");
                    from = SubstringUntil(from, " WITH ");
                    Push(from.Trim(), true).AutoPop = true;

                    Push(sql).AutoPop = true;
                }
            }
            else
            {
                Push("STMT: " + sql).AutoPop = true;
            }
        }

        string SubstringUntil(string s, string what)
        {
            if (s.Contains(what))
                return s.Remove(s.IndexOf(what));
            return s;
        }

        private void Pop()
        {
            if (_stack.Peek() != _root)
            {
                var x = _stack.Pop();
                x.Duration += Math.Max(0, (GetDateTime() - x.Start).Ticks);
            }
        }

        private MagicEntry Push(string what, bool db = false)
        {
            var e = _stack.Peek().Get(what);
            e.Start = GetDateTime();
            e.Db = db;
            _stack.Push(e);
            return e;
        }
        bool CheckStartsWith(params string[] args)
        {
            foreach (var item in args)
            {
                if (_dataLine.StartsWith(item))
                    return true;
            }
            return false;
        }

        bool CheckStartTask()
        {
            foreach (var item in new string[] {
                ">>Starts load Batch Task - '", ">>Starts load Online Task - '",
                ">> Dיbut chargement tגche Batch - '", ">> Dיbut chargement tגche Online - '",
                ">>Beginnt Laden des Prozesse Hintergrund - '",">> התחל טעינת משימה  אצווה - '",
            ">> התחל טעינת משימה  מקוונת - '"})
            {
                if (_dataLine.StartsWith(item))
                {
                    _dataLine = _dataLine.Substring(item.Length);
                    _dataLine = _dataLine.Remove(_dataLine.LastIndexOf('\''));

                    var e = _stack.Peek().Get(_dataLine);
                    e.Start = GetDateTime();
                    _stack.Push(e);
                    return true;
                }
            }
            return false;
        }

        internal DateTime GetDateTime()
        {
            if (_line.StartsWith(" <"))
            {
                var s = _line.Substring(_line.IndexOf("> ") + 2);
                Func<int, int, int> get = (p, l) => int.Parse(s.Substring(p, l));
                if (s[2] == ':')
                {
                    var n = DateTime.Now;
                    return new DateTime(n.Year, n.Month, n.Day, get(0, 2), get(3, 2), get(6, 2), get(9, 3));
                }
                else

                    return new DateTime(get(6, 4), get(3, 2), get(0, 2), get(11, 2), get(14, 2), get(17, 2), get(20, 3));
            }
            else if (_line.Length > 3 && _line[2] == ':')
            {
                var s = _line.TrimStart();
                Func<int, int, int> get = (p, l) => int.Parse(s.Substring(p, l));
                var n = DateTime.Now;
                return new DateTime(n.Year, n.Month, n.Day, get(0, 2), get(3, 2), get(6, 2), get(9, 3));
            }
            else if (_line.Length > 3 && _line[2] == '/')
            {
                var s = _line.TrimStart();
                Func<int, int, int> get = (p, l) => int.Parse(s.Substring(p, l));
                var n = DateTime.Now;
                return new DateTime(get(6, 4), get(3, 2), get(0, 2), get(11, 2), get(14, 2), get(17, 2), get(20, 3));
            }
            else
                return DateTime.Now;
        }
    }

    class MagicEntry
    {
        public string Name;
        public long Count;
        public long Duration;
        public DateTime Start;
        public bool AutoPop;
        internal Dictionary<string, MagicEntry> _children = new Dictionary<string, MagicEntry>();

        public bool Db { get; internal set; }

        void WriteTo(XmlWriter wr)
        {

            wr.WriteStartElement("I");
            wr.WriteAttributeString("N", Name);
            wr.WriteAttributeString("D", Duration.ToString());
            wr.WriteAttributeString("C", Count.ToString());
            lock (_children)
                foreach (var child in _children)
                {
                    child.Value.WriteTo(wr);
                }
            wr.WriteEndElement();
        }
        internal MagicEntry Get(string entry)
        {
            MagicEntry child;

            if (!(_children.TryGetValue(entry, out child)))
            {
                _children.Add(entry, child = new MagicEntry() { Name = entry });
            }

            child.Count++;
            return child;
        }

        internal void Save(string fileName)
        {
            using (var wr = System.Xml.XmlWriter.Create(fileName))
            {
                WriteTo(wr);
            }
        }

        internal void Populate(ProfilerTree st)
        {
            foreach (var c in _children.Values.OrderByDescending(i => i.Duration))
            {
                st.AddChild(x => c.PopulateTreeItem(x, Name, c.Duration), c.Duration);
            }
        }

        private void PopulateTreeItem(TreeItem x, string name, long totalDuration)
        {
            x.Set(Name, Name, Count, Duration, Db, 0, 0, false);
            foreach (var item in _children)
            {
                x.AddChild(y => item.Value.PopulateTreeItem(y, Name, totalDuration));
            }
            if (Name.StartsWith("Error"))
                x.PaintNode(Color.Red, false);
        }
    }
}




