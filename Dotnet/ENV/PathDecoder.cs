using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ENV.Data;
using ENV.Data.DataProvider;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.Data.UnderConstruction;
using Text = Firefly.Box.Text;

namespace ENV
{
    public class PathDecoder
    {
        Dictionary<string, string> _tokens = new Dictionary<string, string>();
        Func<PathDecoder> _parentDecoder = null;
        public PathDecoder()
        {
            NewValuesAreNotCaseSensative = true;
        }
        public PathDecoder(Func<PathDecoder> parentDecoder) : this()
        {
            _parentDecoder = parentDecoder;
        }


        static PathDecoder _sharedPathDecoder = new PathDecoder();
        public static PathDecoder SharedPathDecoder
        {
            get { return _sharedPathDecoder; }
        }

        static Firefly.Box.ContextStatic<PathDecoder> ContextInstance =
            new Firefly.Box.ContextStatic<PathDecoder>(() => new PathDecoder(() => SharedPathDecoder));

        public static PathDecoder ContextPathDecoder
        {
            get
            {
                return ContextInstance.Value;
            }
        }
        static bool _suspendChange;
        public static void Suspend()
        {
            _suspendChange = true;
        }



        public static void Resume()
        {
            _suspendChange = false;
        }

        internal static string FixFileName(string fileName)
        {
            foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }

            return fileName;
        }
        ChangeMonitor _myChangeMonitor = new ChangeMonitor();
        public void AddTokenName(string name, string value)
        {
            lock (this)
            {
                _myChangeMonitor.Change();
                if ('%' + name + '%' == value)
                    throw new Exception("Token can't be the same as it value");
                name = name.Trim();
                if (NewValuesAreNotCaseSensative)
                {
                    name = name.ToUpper(CultureInfo.InvariantCulture);
                    foreach (var token in _tokens)
                    {
                        if (token.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            _tokens.Remove(token.Key);
                            break;
                        }
                    }
                }

                if (_tokens.ContainsKey(name))
                {
                    _tokens[name] = value;
                }
                else
                    _tokens.Add(name, value);
                if (_suspendChange)
                    return;
                if (Change != null)
                    Change();
            }
        }

        public static event Action Change;


        static Dictionary<string, string> _pathReplacers = new Dictionary<string, string>();
        public bool NewValuesAreNotCaseSensative { get; set; }

        public static void AddPathReplacers(string what, string newValue)
        {
            if (_pathReplacers.ContainsKey(what))
                _pathReplacers.Remove(what);
            _pathReplacers.Add(what, newValue);
        }
        internal interface IResultProcessor
        {
            string ProcessResult(string s);
        }

        internal static IResultProcessor ResultProcessor = new DefaultImpOfIResultProcessor();
        class DefaultImpOfIResultProcessor : IResultProcessor
        {
            public string ProcessResult(string s)
            {
                return s;
            }
        }
        public static event Action<string, string> AfterDecode;
        public static string DecodePath(string source)
        {
            var r = ResultProcessor.ProcessResult(ContextPathDecoder.InternalDecodePath(source, false));
            if (AfterDecode != null)
                AfterDecode(source, r);
            return r;
        }

        public static string DecodePathAndKeepChar(string source)
        {
            var r = ResultProcessor.ProcessResult(ContextPathDecoder.InternalDecodePath(source, true));
            if (AfterDecode != null)
                AfterDecode(source, r);
            return r;
        }
        static public Func<string, string> _preprocessPath = y => y;
        internal string InternalDecodePath(string source, bool keepChar)
        {
         
            string path = source;
            if (path == null)
                return null;
            path = _preprocessPath(path);

            Action<Dictionary<string, string>> doIT = (dict) =>
            {
                foreach (var item in dict)
                {
                    var i = path.IndexOf("$" + item.Key);
                    if (i >= 0)
                    {
                        var j = i + item.Key.Length + 1;
                        if (j == path.Length || (!char.IsLetterOrDigit(path[j])))
                        {
                            path = path.Remove(i, item.Key.Length + 1).Insert(i, item.Value);
                        }
                    }
                }

            };
            if (path != null&&path.Contains("$"))
            {
                doIT(_tokens);
                if (_parentDecoder != null)
                    doIT(_parentDecoder()._tokens);
            }
            foreach (var pathReplacer in _pathReplacers)
            {
                var org = pathReplacer.Key;
                var newPath = pathReplacer.Value;
                if (path.StartsWith(org, StringComparison.InvariantCultureIgnoreCase))
                {
                    path = newPath + path.Substring(org.Length);
                }
            }
            int lastIndex = path.IndexOf('%');
            while (lastIndex > -1)
            {
                if (lastIndex + 1 > path.Length)
                    return path;
                int nextIndex = path.IndexOf('%', lastIndex + 1);
                if (nextIndex > lastIndex)
                {
                    string token = path.Substring(lastIndex + 1, nextIndex - lastIndex - 1);

                    string replacement;
                    if (GetFromDictionary(token, out replacement))
                    {
                        if ('%' + token + '%' == replacement)
                            throw new Exception("Token can't be the same as it value");
                        else
                        {
                            path = path.Remove(lastIndex, nextIndex - lastIndex + 1);
                            path = path.Insert(lastIndex, replacement);
                            lastIndex = path.IndexOf('%', lastIndex);
                        }
                    }
                    else
                    {
                        if (!keepChar)
                        {
                            path = path.Remove(lastIndex, nextIndex - lastIndex + 1);
                            lastIndex = path.IndexOf('%', lastIndex);
                        }
                        else
                            lastIndex += 1;
                    }



                }
                else
                {
                    string token = path.Substring(lastIndex + 1).TrimEnd(' ');
                    string replacement;
                    if (GetFromDictionary(token, out replacement))
                    {
                        path = path.Remove(lastIndex);
                        path += replacement;
                        lastIndex = path.IndexOf('%', lastIndex);
                    }
                    else
                        return path;
                }
            }
            return path;
        }

        bool GetFromDictionary(string token, out string replacement)
        {
            if (NewValuesAreNotCaseSensative)
            {
                var x = token.ToUpper(CultureInfo.InvariantCulture);
                if (_tokens.TryGetValue(x, out replacement))
                    return true;
            }
            if (_tokens.TryGetValue(token, out replacement))
                return true;
            if (_parentDecoder != null)
                return _parentDecoder().GetFromDictionary(token, out replacement);
            return false;
        }


        internal Text GetValueOf(string tokenName)
        {
            if (!_tokens.ContainsKey(tokenName))
                return "";
            return _tokens[tokenName];
        }
        public void Display()
        {
            var e = new ENV.Data.Entity("PathDecoder", new MemoryDatabase());
            TextColumn name, value, translatedValue;
            e.Columns.Add(name = new TextColumn("Name", "50"), value = new TextColumn("Value", "1000"), translatedValue = new TextColumn("TranslatedValue", "1000"));
            e.SetPrimaryKey(new[] { name });
            {
                var i = new Iterator(e);
                foreach (var token in _tokens)
                {
                    var r = i.CreateRow();
                    r.Set(name, token.Key);
                    r.Set(value, token.Value);
                    r.Set(translatedValue, DecodePath(token.Value));
                    r.UpdateDatabase();
                }
                if (_parentDecoder != null)
                    foreach (var token in _parentDecoder()._tokens)
                    {
                        if (!_tokens.ContainsKey(token.Key))
                        {
                            var r = i.CreateRow();
                            r.Set(name, token.Key);
                            r.Set(value, token.Value);
                            r.Set(translatedValue, DecodePath(token.Value));
                            r.UpdateDatabase();
                        }
                    }
            }
            new ENV.Utilities.EntityBrowser(e, false).Run();
        }
        public void PathDecoderTester()
        {
            var uic = new Firefly.Box.UIController { View = new ENV.UI.Form() { StartPosition = Firefly.Box.UI.WindowStartPosition.CenterMDI } };

            TextColumn value = new TextColumn("Input"), translatedValue = new TextColumn("Result");
            translatedValue.BindValue(() => DecodePath(value));
            int top = 5;
            foreach (var tc in new[] { value, translatedValue })
            {
                var tb = new ENV.UI.TextBox { Data = tc };
                var label = new ENV.UI.TextBox { Text = tc.Caption, Style = Firefly.Box.UI.ControlStyle.Flat };
                label.Left = 5;
                label.Width = 40;
                tb.Left = 50;
                tb.Width = 300;
                tb.AdvancedAnchor.RightPercentage = 100;
                tb.AdvancedAnchor.Enabled = true;
                label.Top = top;
                label.BackColor = uic.View.BackColor;
                tb.Top = top;
                top = tb.Bottom + 5;
                uic.View.Controls.Add(label);
                uic.View.Controls.Add(tb);
                uic.Columns.Add(tc);
                uic.View.ClientSize = new System.Drawing.Size(tb.Right + 5, top);
            }
            uic.Run();

        }

        public static void Clear()
        {
            _sharedPathDecoder = new PathDecoder();
            ContextInstance.DisposeAndClearValue();
        }

        internal void RegisterObserver(ChangeMonitor observingChangeMonitor)
        {
            observingChangeMonitor.Observe(_myChangeMonitor);
            if (_parentDecoder != null)
                _parentDecoder().RegisterObserver(observingChangeMonitor);
        }
    }
}

