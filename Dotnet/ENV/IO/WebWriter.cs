using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using ENV.IO.Advanced;
using ENV.IO.Html;
using ENV.Utilities;
using Firefly.Box;
using Writer = ENV.IO.Advanced.Writer;

namespace ENV.IO
{
    public class WebWriter : Writer, IOByName, ITemplateEnabled
    {
        Func<ITextWriter> _createWriter = () => new DummyITextWriter();
        public WebWriter(string fileName, Encoding encoding = null)
        {
            if (Text.IsNullOrEmpty(fileName))
                InitResponseWriter(encoding);
            else
                _createWriter = () => new ITextWriterBridgeToTextWriter(fileName, false, LocalizationInfo.Current.OuterEncoding);
            
            _helper = new HebrewTextTools.TextWritingHelper(this);
        }
        public WebWriter(Encoding encoding = null)
        {
            _helper = new HebrewTextTools.TextWritingHelper(this);
            InitResponseWriter(encoding);
        }

        private void InitResponseWriter(Encoding encoding)
        {
            var w = FixedWriter;
            if (w != null)
            {
                _createWriter = () => w;
            }
            else
                _createWriter = () => new ITextWriterBridgeToResponse(System.Web.HttpContext.Current.Response,  encoding );
        }

        internal static ITextWriter FixedWriter
        {
            get
            {

                return Context.Current[typeof(WebWriter)] as ITextWriter;
            }
            set
            {
                Context.Current[typeof(WebWriter)] = value;

            }
        }
        #region Hebrew OEM issues

        HebrewTextTools.TextWritingHelper _helper;



        protected override string ProcessLine(string originalLine, int width, bool donotTrim)
        {
            return _helper.ProcessLine(originalLine, width,donotTrim);
        }

        protected override string ProcessControlData(string originalData, bool rightToLeft, bool hebrewDosCompatibleEditing)
        {
            return _helper.ProcessControlData(originalData, rightToLeft, hebrewDosCompatibleEditing);
        }

        public bool PerformRightToLeftManipulations
        {
            get { return _helper.PerformRightToLeftManipulations; }
            set { _helper.PerformRightToLeftManipulations = value; }
        }

        public bool RightToLeftFlipLine
        {
            get { return _helper.RightToLeftFlipLine; }
            set { _helper.RightToLeftFlipLine = value; }
        }

        public bool Oem
        {
            get { return _helper.Oem; }
            set { _helper.Oem = value; }
        }

        public bool OemForNonRightToLeftColumns
        {
            get { return _helper.OemForNonRightToLeftColumns; }
            set { _helper.OemForNonRightToLeftColumns = value; }
        }

        #endregion
        public bool V8Compatible { get { return _helper.V8Compatible; } set { _helper.V8Compatible = value; } }
        ITextWriter _writer;
        protected override void OnOpen()
        {
            try
            {
                _writer = _createWriter();
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                _writer = new DummyITextWriter();
            }
            finally
            {
                base.OnOpen();
            }
        }
        protected override void OnWrite(string text)
        {
            _writer.Write(text);
        }
        internal string _name = null;
        public string Name { set { _name = PathDecoder.DecodePath(value); } get { return _name; } }
        class ITextWriterBridgeToResponse : ITextWriter, ResponseWriter
        {
            System.Web.HttpResponse _response;
            System.IO.StringWriter _sw;
            public ITextWriterBridgeToResponse(HttpResponse response, Encoding encoding )
            {
                _response = response;
                if (encoding != null)
                    _response.ContentEncoding = encoding;
                _sw = new System.IO.StringWriter();
            }

            public void SetCookie(HttpCookie c)
            {
                _response.SetCookie(c);
                WebWriter.ThereWasAnOutput();
            }

            public void WriteContent(string what)
            {
                _response.Output.Write(what);
                WebWriter.ThereWasAnOutput();
            }

            public void WriteLine(string what)
            {
                _response.Output.WriteLine();
                WebWriter.ThereWasAnOutput();
            }

            public void Write(string s)
            {
                _sw.Write(s);
                WebWriter.ThereWasAnOutput();
            }

            public void WriteInitBytes(byte[] obj)
            {
                if (obj.Length>0&&obj[0]== 239)
                    _response.BinaryWrite(obj);
                //_response.ContentEncoding = Encoding.UTF8;
            }

            public void Dispose()
            {
                if (WebWriter.WasThereAnOutput())
                    ParseOutCookies(_sw.ToString(), this);
            }
        }
        internal interface ResponseWriter
        {
            void SetCookie(HttpCookie c);
            void WriteContent(string what);
            void WriteLine(string what);

        }
        internal static void ParseOutCookies(string content, ResponseWriter httpResponse)
        {
            while (content.StartsWith("set-cookie:"))
            {
                content = content.Substring(11);
                string cookieS = content;
                int i = 0;
                while (i < content.Length)
                {
                    if (content[i] == '\n' || content[i] == '\r')
                    {
                        cookieS = content.Remove(i);
                        if (content[i] == '\r' && content.Length > i + 1 && content[i + 1] == '\n')
                            i++;
                        
                        break;
                    }
                    i++;
                }
                var s = cookieS.Split('=');
                httpResponse.SetCookie(new HttpCookie(s[0], s[1]));
                if (i >= content.Length)
                    return;
                else
                {
                    content = content.Substring(i + 1);
                }

            }
            httpResponse.WriteContent(content);


        }

        void ITemplateEnabled.WriteTextTemplate(Func<TemplateWriter> createTemplateWriter, Action<TemplateValues> provideTokens)
        {
            if (_template == null)
                _template = createTemplateWriter();
            var v = new TemplateValues();
            provideTokens(v);
            _template.MergeTokens(v);
        }

        TemplateWriter _template = null;

        public static WebWriter FindIOByName(Text baseStreamName)
        {
            return IOFinder.FindByName<WebWriter>(baseStreamName);
        }
        internal static Func<TemplateWriter,Action<string>,bool> BypassTemplateWrite = (t,w)=>false;
        int _usagesByName = 0;
        public override void Dispose()
        {
            if (_usagesByName == 0)
            {
                if (_template != null)
                {
                    if (_writer == null)
                        Open();
                    bool donotWrite = BypassTemplateWrite(_template,OnWrite);
                    
                    if (!donotWrite)
                    _template.WriteTo(_writer.WriteInitBytes, a => OnWrite(a));
                }
                if (_htmlContentWriter != null)
                    _htmlContentWriter.Dispose();
                if (_writer != null)
                    _writer.Dispose();
                base.Dispose();

            }
            else
                _usagesByName--;
        }

        void IOByName.AddToUsageCount()
        {
            _usagesByName++;
        }
        public HttpResponse Response { get { return System.Web.HttpContext.Current.Response; } }
        public HttpRequest Request { get { return System.Web.HttpContext.Current.Request; } }
        public static HashSet<string> HTTPServerVariablesBlackList = new HashSet<string>();

        IO.Html.HtmlContentWriter _htmlContentWriter;
        internal void Write(HtmlSection htmlSection)
        {
            if (_htmlContentWriter == null)
            {
                _htmlContentWriter = new HtmlContentWriter(OnWrite);
                Open();
            }
            _htmlContentWriter.Write(htmlSection);
        }

        internal static Firefly.Box.ContextStatic<IRequestInfo> RequestInfo
            =new ContextStatic<IRequestInfo>(()=>new HttpContextRequestInfo()); 
        
        static ContextStatic<bool> _hasOutput = new ContextStatic<bool>(()=>false);
        public static void ThereWasAnOutput()
        {
            _hasOutput.Value = true;
        }
        public static bool WasThereAnOutput()
        {
            return _hasOutput.Value;
        }
        public static bool FixHebrewUTF8InUrlParameters = false;
        class HttpContextRequestInfo : IRequestInfo
        {
            NameValueCollection _badPostValues;
            public string GetPrgName()
            {
                var req = System.Web.HttpContext.Current.Request;
                var prgName = req.Form["PRGNAME"];
                if (string.IsNullOrEmpty(prgName))
                    prgName = req["PRGNAME"];
                if (string.IsNullOrEmpty(prgName) &&
                  IsMethodPost &&
                  req.Form.Count == 0)
                {
                    try
                    {
                        var x = req.ContentEncoding.GetString(UserMethods.StreamToByteArray(req.InputStream));
                        _badPostValues = HttpUtility.ParseQueryString(x);
                        _badPostValues.Add(req.Params);
                        prgName = _badPostValues["PRGNAME"];
                    }
                    catch
                    {
                    }

                }
                return prgName;
            }

            public HttpPostedFile GetFile(string key)
            {
               return  System.Web.HttpContext.Current.Request.Files[key];
                
            }

            public HttpCookie GetCookie(string key)
            {
                return System.Web.HttpContext.Current.Request.Cookies[key];
            }

            public string[] GetValues(string key)
            {
                if (HTTPServerVariablesBlackList.Contains(key))
                {
                    var x = System.Web.HttpContext.Current.Request.Params[key];
                    var y = System.Web.HttpContext.Current.Request.ServerVariables[key];
                    if (x == y)
                        return null;
                }
                if (_badPostValues != null)
                    return _badPostValues.GetValues(key);
                var r = System.Web.HttpContext.Current.Request.Params.GetValues(key);
                FixHebrewUTFStuff(r);
                return r;
            }

            private static void FixHebrewUTFStuff(string[] r)
            {
                if (FixHebrewUTF8InUrlParameters && r != null)
                {
                    for (int i = 0; i < r.Length; i++)
                    {
                        if (r[i].IndexOf((char)1523) >= 0)
                        {
                            r[i] = Encoding.UTF8.GetString(Encoding.GetEncoding(1255).GetBytes(r[i]));
                        }
                    }
                }
            }

            public string[] GetFormValues(string key)
            {
                if (HTTPServerVariablesBlackList.Contains(key))
                {
                    var x = System.Web.HttpContext.Current.Request.Params[key];
                    var y = System.Web.HttpContext.Current.Request.ServerVariables[key];
                    if (x == y)
                        return null;
                }
                if (_badPostValues != null)
                    return _badPostValues.GetValues(key);
                var z = System.Web.HttpContext.Current.Request.Form.GetValues(key);
                if (z == null)
                    z = HttpContext.Current.Request.ServerVariables.GetValues(key);
                FixHebrewUTFStuff(z);
                return z;
            }

            public bool IsMethodPost {
                get
                {
                return     System.Web.HttpContext.Current.Request.HttpMethod.Equals("POST",
                        StringComparison.InvariantCultureIgnoreCase);
                }
            }
        
        }

        internal interface IRequestInfo
        {
            string GetPrgName();

            HttpPostedFile GetFile(string key);
            HttpCookie GetCookie(string key);
            string[] GetValues(string key);
            string[] GetFormValues(string key);
            bool IsMethodPost { get; }
        }
        public static string[] GetRequestValues(string key)
        {
            IRequestInfo requestInfo = RequestInfo.Value;
            var cookie = requestInfo.GetCookie(key);
            if (cookie != null)
            {
                var input = cookie.Values;

                var result = new string[input.Count];
                for (int i = 0; i < input.Count; i++)
                {
                    if (HttpContext.Current != null)
                        result[i] = ProcessWebString(HttpContext.Current.Server.UrlDecode(input[i]));
                    else
                        result[i] = input[i];
                }
                return result;
            }
            {
                var input = requestInfo.IsMethodPost?requestInfo.GetFormValues(key): requestInfo.GetValues(key);
                if (input == null)
                    return null;
                var result = new string[input.Length];
                for (int i = 0; i < input.Length; i++)
                {
                    result[i] = ProcessWebString(input[i]);
                }
                return result;
            }
        }

        internal static string ProcessWebString(string s)
        {
            // 
            // To introducs XSS protection, please download the HtmlSanitizationLibrary Library from http://www.microsoft.com/en-us/download/details.aspx?id=28589
            // It's in the C:\Program Files (x86)\Microsoft Information Security\AntiXSS Library v4.2\SANITIZER directory post installation.,
            // Make sure to perform considerable testing, as your application may count on illegal values that are being sent in.
            // 
            //return Microsoft.Security.Application.Sanitizer.GetSafeHtmlFragment(s);
            //
            return s;
        }


        internal  string Visit(ITemplateTokensWriter mv)
        {
            var result = _template.GetTypeScriptTypeDefinition();
            _template.Visit(mv);
            return result;

        }
    }
    public static class WebConsole
    {
        public static void WriteLine(string text)
        {
            Response.Write(text + "<br/>\n");
        }
        public static void Write(string text)
        {
            Response.Write(text);
        }

        public static void BeginHtmlAndTable(string title, params Firefly.Box.Data.Advanced.ColumnBase[] columns)
        {
            Write("<html>\n<Head>\n<title>" + title + "</title>\n</head>\n<body>\n<h1>" + title + "</h1>\n<table border=1>\n");
            Write("\n\t<tr>");
            foreach (var item in columns)
            {
                Write("\n\t\t<th>" + item.Caption + "</th>");
            }
            Write("\n\t</tr>");
        }


        public static void WriteTableLine(params Firefly.Box.Data.Advanced.ColumnBase[] columns)
        {
            Write("\n\t<tr>");
            foreach (var item in columns)
            {
                Write("\n\t\t<td>" + item.ToString() + "</td>");
            }
            Write("\n\t</tr>");
        }
        public static void EndTableAndHtml()
        {
            Write("</table>\n</body>\n</html>");
        }
        public static HttpResponse Response { get { return System.Web.HttpContext.Current.Response; } }
        public static HttpRequest Request { get { return System.Web.HttpContext.Current.Request; } }
    }
    public class HtmlTableWriter<T> : IDisposable
    {
        List<string> _headers = new List<string>();
        List<Func<T, string>> _data = new List<Func<T, string>>();
        Action<string> _write;
        bool _printedHeaders = false;
        public HtmlTableWriter(string title, string additionalHtml, Action<string> write,string refresh)
        {
            _write = write;
            if (refresh.ToUpper().Trim() == "N")
                refresh = "";
            else
                refresh = "<meta http-equiv=\"refresh\" content=\"" + refresh + "\">";
            write(string.Format("<html>\n<Head>\n<title>{0}</title>\n{2}</head>\n<body>\n<h1>{0}</h1>{1}\n<table border=1>\n", title, additionalHtml, refresh));
        }
        public void AddColumn(string caption, Func<T, string> valueFactory)
        {
            if (_printedHeaders)
                throw new InvalidOperationException();
            _headers.Add(caption);
            _data.Add(valueFactory);
        }
        public void WriteLine(T rowItem)
        {
            if (!_printedHeaders)
            {
                _write("<tr>");
                foreach (var header in _headers)
                {
                    _write(string.Format("<th>{0}</th>", header));
                }
                _write("</tr>\n");
                _printedHeaders = true;
            }
            _write("<tr>");
            foreach (var valueFactory in _data)
            {
                var s = valueFactory(rowItem);
                if (s != null)
                    s = s.Replace("\n", "<br/>");
                _write(string.Format("<td>{0}</td>", s));
            }
            _write("</tr>\n");
        }

        public void Dispose()
        {
            _write("</table>\n</body>\n</html>");
        }
    }
}
