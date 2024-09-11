using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;
using Firefly.Box.Advanced;

namespace ENV.Labs
{
    public class Translator
    {
        public static string FromLanguage = "es";
        public static void EnableFor(ApplicationControllerBase app, string fromLAng)
        {
            FromLanguage = fromLAng;
            if (ENV.UserMethods.Instance.IniGet("UseGoogleTranslator") == "Y")
                if (_key != null)
                {
                    app.Handlers.Add(Keys.F12).Invokes += TranslateCurrent();
                    app.Handlers.Add(Keys.Control | Keys.XButton2).Invokes += TranslateCurrent();
                }
        }

        static HandlerInvokeHandler TranslateCurrent()
        {
            return e =>
                       {
                           if (_key == null)
                           {
                               // MessageBox.Show("Google key not found");
                               return;
                           }
                           var p = System.Windows.Forms.Control.MousePosition;
                           IEnumerable controls = System.Windows.Forms.Application.OpenForms;
                           Firefly.Box.Context.Current.InvokeUICommand(() =>
                           {
                               IterateControls(controls, p);
                           });
                       };
        }

        static void IterateControls(IEnumerable controls, Point p)
        {
            foreach (System.Windows.Forms.Control f in controls)
            {

                var child = f.GetChildAtPoint(f.PointToClient(p));
                var cc = child as Firefly.Box.UI.Advanced.TextControlBase;
                if (cc != null)
                {
                    TranslateControl(cc, cc.Text);

                    return;
                }
                else if (child is Firefly.Box.UI.Grid)
                {
                    var g = (Firefly.Box.UI.Grid)child;
                    var ch = g.GetChildAtPoint(g.PointToClient(p));
                    if (ch != null)
                    {
                        IterateControls(child.Controls, p);
                    }
                    else
                    {
                        var leftOfPoint = g.PointToClient(p).X;
                        foreach (var gggc in g.Controls)
                        {
                            var gc = gggc as Firefly.Box.UI.GridColumn;
                            if (gc != null && gc.Left <= leftOfPoint && gc.Right >= leftOfPoint)
                            {
                                TranslateControl(gc, gc.Text);
                            }

                        }
                    }
                }
                else if (child != null)
                    IterateControls(child.Controls, p);
            }
        }

        static string __key;
        static bool _failed = false;
        static string _translationFile;
        static string _key
        {
            get
            {
                if (__key == null)
                {
                    if (System.IO.File.Exists(@"c:\firefly\translation\googleApi.Key"))
                    {
                        _translationFile = @"c:\firefly\translateCache";
                        __key = System.IO.File.ReadAllText(@"c:\firefly\translation\googleApi.Key");
                    }
                    else if (!_failed && System.IO.File.Exists(@"c:\firefly\translation\googleapi.key"))
                    {
                        _translationFile = @"c:\firefly\translation\translateCache";
                        __key = System.IO.File.ReadAllText(@"c:\firefly\translation\googleapi.key");
                    }
                    _failed = true;
                }
                return __key;
            }
        }
        static void TranslateControl(Firefly.Box.UI.Advanced.ControlBase cc, string text)
        {
            var r = TranslateText(text);

            cc.ToolTip = r;
            Common.SetTemporaryStatusMessage(r);
        }

        static Dictionary<string, string> _cache, _secondary = new Dictionary<string, string>();
        public static string TranslateText(string text)
        {
            var r = InternalTranslate(text);
            if (r.Length > 0 && char.IsLower(r[0]))
                return r[0].ToString().ToUpper() + r.Substring(1);
            return r;
        }
        static object lockit = new object();
        private static string InternalTranslate(string text)
        {
            if (string.IsNullOrEmpty(_translationFile))
                _key.ToString();
            var translatorFileName = _translationFile + "." + FromLanguage;
            if (_cache == null)
            {
                lock (lockit)
                {
                    _cache = ReadTranslationFile(translatorFileName);
                    _secondary = ReadTranslationFile(translatorFileName + ".secondary");
                }

            }
            text = text.Replace("&", "").Replace("\r", " ").Replace("\n", " ");
        
            string z = null;
            if (_cache.TryGetValue(text, out z))
                return z;
            lock (_cache)

            {
                if (_cache.TryGetValue(text, out z))
                    return z;
              
                var r = "";
                if (_secondary.TryGetValue(text, out r))
                {
                    if (r.Contains("&"))
                        r = "";
                }
                if (string.IsNullOrWhiteSpace( r ))
                {
                    string url =
                  @"https://www.googleapis.com/language/translate/v2?key=" + _key + "&source=" + FromLanguage + "&target=en&q=";
                    const string sToSearch = @"""translatedText"": """;
                    if (FromLanguage != "en")
                    {
                        r = System.Text.Encoding.UTF8.GetString(
                            UserMethods.Instance.HTTPGet(url + HttpUtility.UrlEncode(text.TrimEnd())));
                        r = r.Substring(r.IndexOf(sToSearch) + sToSearch.Length);
                        r = r.Remove(r.IndexOf("\"\n"));
                        r = UserMethods.Instance.XMLStr(r);
                        r = r.Replace("&#39;", "'");
                    }
                    else
                        r = text;
                }
                _cache.Add(text, r);
                     using (var sw = new StreamWriter(translatorFileName, true, System.Text.Encoding.UTF8))
                     {
                         sw.WriteLine(text);
                         sw.WriteLine(r);
                     }
                return r;
            }
        }

        private static Dictionary<string, string> ReadTranslationFile(string translatorFileName)
        {
            var result = new Dictionary<string, string>();
            if (System.IO.File.Exists(translatorFileName))
                using (var sr = new StreamReader(new MemoryStream(File.ReadAllBytes(translatorFileName)), System.Text.Encoding.UTF8))
                {

                    int i = 0;
                    string l, term = null;
                    while ((l = sr.ReadLine()) != null)
                    {
                        if (i++ % 2 == 0)
                        {
                            term = l;
                        }
                        else
                        {
                            try
                            {
                                if (!result.ContainsKey(term))
                                    result.Add(term, l);
                            }
                            catch (Exception)
                            {
                            }

                        }
                    }
                }
            return result;
        }
    }
}
