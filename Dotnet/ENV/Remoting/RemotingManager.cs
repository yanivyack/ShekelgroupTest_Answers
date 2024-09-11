using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using ENV.Data;
using ENV.IO;
using Firefly.Box;


namespace ENV.Remoting
{
    public interface RemotingProvider<T>
    {
        object ExecuteOnServer(RemoteCommand<T> command);
    }

    public class HttpRemotingProvider<T> : RemotingProvider<T>
    {
        urlManager _url;

        class urlManager
        {
            List<string> _availableUrls = new List<string>();
            public urlManager(string url)
            {
                _availableUrls.AddRange(url.Split(','));
            }


            [ThreadStatic]
            static Dictionary<string, urlManager> _managers = new Dictionary<string, urlManager>();
            public static urlManager GetUrlManagerFor(string url)
            {
                if (_managers == null)
                    _managers = new Dictionary<string, urlManager>();
                urlManager result;
                if (!_managers.TryGetValue(url, out result))
                {
                    result = new urlManager(url);
                    _managers.Add(url, result);
                }
                return result;
            }

            int _lastOkItem = 0;
            public WebResponse GetResponse(Action<WebRequest> whatToDoToTheRequest)
            {

                Exception lastException = null;
                for (int i = 0; i < _availableUrls.Count; i++)
                {
                    try
                    {
                        var r = WebRequest.Create(_availableUrls[_lastOkItem]);
                        whatToDoToTheRequest(r);
                        return r.GetResponse();
                    }
                    catch (Exception e)
                    {
                        lastException = e;
                        ErrorLog.WriteToLogFile(e, "Remote Request Failed For URL - " + _availableUrls[_lastOkItem]);
                        _lastOkItem++;
                        if (_lastOkItem >= _availableUrls.Count)
                            _lastOkItem = 0;
                    }
                }
                throw lastException;



            }
        }

        public HttpRemotingProvider(string url)
        {
            _url = urlManager.GetUrlManagerFor(url);
        }
        class IFormatterBridgeToFormatter : Formatter
        {
            System.Runtime.Serialization.IFormatter _original;

            public IFormatterBridgeToFormatter(IFormatter original)
            {
                _original = original;
            }

            public object Deserialize(Stream stream)
            {
                return _original.Deserialize(stream);
            }

            public void Serialize(Stream stream, object item)
            {
                _original.Serialize(stream, item);
            }
        }
        public static Formatter _serialiser = new IFormatterBridgeToFormatter(new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter());
        public object ExecuteOnServer(RemoteCommand<T> command)
        {
            var ms = new System.IO.MemoryStream();

            _serialiser.Serialize(ms, command);

            var webResponse = _url.GetResponse(rr =>
            {
                rr.Method = "POST";
                rr.Proxy = null;
                rr.Timeout = System.Threading.Timeout.Infinite;
                using (var content = rr.GetRequestStream())
                {
                    ms.WriteTo(content);

                }
            });

            var rs = webResponse.GetResponseStream();
            int count = 0;
            if (true)
            {
                var ms2 = new MemoryStream();
                int v;

                while ((v = rs.ReadByte()) > -1)
                {
                    count++;
                    ms2.WriteByte((byte)v);
                }
                ms2.Position = 0;
                rs = ms2;
            }
            var result = _serialiser.Deserialize(rs);
            if (true)
            {
                var n = command.ToString();
                if (n.Contains("+"))
                    n = n.Substring(n.LastIndexOf("+") + 1);
                if (n.Contains("."))
                    n = n.Substring(n.LastIndexOf(".") + 1);

            }
            var e = result as TransfarableException;
            if (e != null)
                throw e.CreateException();
            if (result is NullResult)
                return null;
            return result;
        }

        public static void Process(T param)
        {
            Process(o =>
            {
                var t = (RemoteCommand<T>)o;
                return t.Execute(param);

            });
        }
        class As400RemoteCommand : RemoteCommand<IRemoteApplication>, HttpApplication.HasToDoWithRequest
        {
            string _operation;
            bool _async;
            List<object> _arguments = new List<object>();

            public As400RemoteCommand(HttpRequest req)
            {
                Func<int, string> getParameter = y => req.Form["P" + y.ToString()];
                _operation = getParameter(2);
                _async = getParameter(4) == "A";
                int i = 5;
                string type;
                while ((type = getParameter(i)) != null)
                {
                    switch (type)
                    {
                        case "C":
                            _arguments.Add(Text.Cast(getParameter(i + 2)));
                            i += 3;
                            break;
                        case "D":
                            _arguments.Add(Number.Parse(getParameter(i + 3)));
                            i += 4;
                            break;
                        case "B":
                            _arguments.Add((Bool)(Number.Parse(getParameter(i + 2)) == 1));
                            i += 3;
                            break;
                        default:
                            throw new InvalidOperationException("Unknown as400 type " + getParameter(i) + " " +
                                                                getParameter(i + 1) + " " + getParameter(i + 2));
                    }
                }


            }

            public object Execute(IRemoteApplication param)
            {
                if (!_async)
                    WebWriter.FixedWriter = null;
                param.Run(_operation, _arguments.ToArray());
                return "";

            }

            public bool Async { get { return _async; } }
            public void ApplyTo(HttpApplicationServer.RequestInfo info)
            {
                info.Operation = _operation;
                System.Web.HttpContext.Current.Response.AddHeader("Request-ID", info.GetId());
                info.SetHostName(System.Web.HttpContext.Current.Request.UserHostAddress);
                if (_async)
                    System.Web.HttpContext.Current.Response.Write(@"Request-ID:" + info.GetId());
            }
        }
        class ComRemoteCommand : RemoteCommand<IRemoteApplication>, HttpApplication.HasToDoWithRequest
        {
            string _operation;
            bool _async;
            List<object> _arguments = new List<object>();

            public ComRemoteCommand(HttpRequest req)
            {
                Func<int, string> getParameter = y => req.Form["P" + y.ToString()];
                _operation = getParameter(1);
                _arguments.Add(new ProgramCollection.StringOrByteArrayArg((string)getParameter(2)));



            }

            public object Execute(IRemoteApplication param)
            {
                if (!_async)
                    WebWriter.FixedWriter = null;
                param.Run(_operation, _arguments.ToArray());
                return "";

            }

            public bool Async { get { return _async; } }
            public void ApplyTo(HttpApplicationServer.RequestInfo info)
            {
                info.Operation = _operation;
                System.Web.HttpContext.Current.Response.AddHeader("Request-ID", info.GetId());
                info.SetHostName(System.Web.HttpContext.Current.Request.UserHostAddress);
                if (_async)
                    System.Web.HttpContext.Current.Response.Write(@"Request-ID:" + info.GetId());
            }
        }

        class HttpRemoteCommand : RemoteCommand<IRemoteApplication>, HttpApplication.HasToDoWithRequest
        {
            string _operation;
            bool _async;
            string _arguments;

            public HttpRemoteCommand(string operation, HttpRequest req)
            {
                _operation = operation;
                _async = req["Wait"] == "N";
                _arguments = req["ARGUMENTS"];

            }

            public object Execute(IRemoteApplication param)
            {
                if (!_async)
                    WebWriter.FixedWriter = null;
                param.Run(_operation, HttpApplication.ParseArguments(_arguments));
                return "";
            }

            public bool Async { get { return _async; } }
            public void ApplyTo(HttpApplicationServer.RequestInfo info)
            {
                info.Operation = _operation;
                System.Web.HttpContext.Current.Response.AddHeader("Request-ID", info.GetId());
                info.SetHostName(System.Web.HttpContext.Current.Request.UserHostAddress);
                if (_async)
                    System.Web.HttpContext.Current.Response.Write(@"Request-ID:" + info.GetId());
            }
        }










        public static void Process(Func<object, object> performWork)
        {
            var Request = HttpContext.Current.Request;
            var Response = HttpContext.Current.Response;
            var soapAction = Request["HTTP_SOAPACTION"];
            if (soapAction == null)
            {
                var s = "action=\"";
                var i = Request.ContentType.IndexOf(s);
                if (i >= 0)
                {
                    soapAction = Request.ContentType.Substring(i + s.Length);
                    soapAction = soapAction.Remove(soapAction.IndexOf('\"'));
                }
            }
            var prgName = Request["PRGNAME"];
            var fromAs400 = Request.Headers["FromAs400"];
            var fromCom = Request.Headers["FromCOM"];
            if (string.IsNullOrEmpty(soapAction))
            {
                if (string.IsNullOrEmpty(prgName))
                {
                    if ((fromAs400 ?? "") == "Y")
                    {
                        performWork(new As400RemoteCommand(Request));
                    }
                    else if ((fromCom ?? "") == "Y")
                    {
                        performWork(new ComRemoteCommand(Request));
                    }
                    else
                    {
                        object s = _serialiser.Deserialize(Request.InputStream);
                        object result = performWork(s);
                        var ms = new MemoryStream();
                        if (ReferenceEquals(result, null))
                            _serialiser.Serialize(ms, new NullResult());
                        else
                            _serialiser.Serialize(ms, result);
                        ms.WriteTo(Response.OutputStream);
                    }
                }
                else
                {
                    var x = performWork(new HttpRemoteCommand(prgName, Request));
                }
            }
            else
            {
                SoapRemoteCommand.Process(soapAction, Request, Response, performWork);
            }

            /*     System.Diagnostics.Debug.WriteLine(
                     string.Format("{0} in={1} out={2}", s.GetType().Name, Request.InputStream.Length, ms.Length));*/
        }
        public static void Process(HttpListenerContext c, T param)
        {
            Process(c, o =>
            {
                var t = (RemoteCommand<T>)o;
                return t.Execute(param);

            });
        }
        public static void Process(HttpListenerContext c, Func<object, object> performWork)
        {
            var Request = c.Request;
            var Response = c.Response;
            object s = _serialiser.Deserialize(Request.InputStream);
            object result = performWork(s);
            var ms = new MemoryStream();
            if (ReferenceEquals(result, null))
                _serialiser.Serialize(ms, new NullResult());
            else
                _serialiser.Serialize(ms, result);
            ms.WriteTo(Response.OutputStream);

            /*     System.Diagnostics.Debug.WriteLine(
                     string.Format("{0} in={1} out={2}", s.GetType().Name, Request.InputStream.Length, ms.Length));*/
        }

    }
    [Serializable]
    internal class NullResult
    { }
    public class SoapRemoteCommand : RemoteCommand<IRemoteApplication>
    {
        string _operation;
        string _nameSpace;
        bool _hasReturnValue;
        protected List<object> _args = new List<object>();
        List<string> _argNames = new List<string>();
        int[] _outputArgumentIndices = new int[0];

        protected SoapRemoteCommand()
        {
        }
        SystinetRequestReponseNamespaceMapping _transformer;
        string _sig;
        XmlDocument _xd;
        private void setTransformer(SystinetRequestReponseNamespaceMapping defaultTransformer, string sig, XmlDocument xd)
        {
            _transformer = defaultTransformer;
            _sig = sig;
            _xd = xd;
        }

        public virtual object Execute(IRemoteApplication param)
        {
            if (_transformer != null)
                _transformer.CreateRequest(_sig, _xd, x =>
                {
                    if (x is string)
                        x = new ProgramCollection.StringOrByteArrayArg(x.ToString());
                    this._args.Add(x);
                }, param.ProvideArgumentParserFor(_operation));




            var result = new SoapResult();

            var outputColumns = new List<ByteArrayColumn>();
            foreach (var outputArgumentIndex in _outputArgumentIndices)
            {
                var c = new ByteArrayColumn() { ContentType = ByteArrayColumnContentType.Ansi };
                (_args[outputArgumentIndex] as ProgramCollection.StringOrByteArrayArg).SetColumn(c);
                outputColumns.Add(c);
            }
            result.OutputColumns = outputColumns.ToArray();

            WebWriter.FixedWriter = new DummyITextWriter();
            try
            {
                result.ReturnValue = param.Run(_operation, _args.ToArray());
            }
            finally
            {
                WebWriter.FixedWriter = null;
            }
            return result;
        }

        public bool Async
        {

            get { return false; ; }
        }


        internal class SoapResult
        {
            public ByteArrayColumn[] OutputColumns;
            public object ReturnValue;
            public string SoapHeader;
        }

        public static SystinetSoapRequestResponseXmlTransformer DefaultTransformer = new SystinetSoapRequestResponseXmlTransformer();

        public static void Process(string soapAction, HttpRequest request, HttpResponse response, Func<object, object> performWork)
        {
            using (var z = new System.IO.StreamReader(request.InputStream))
            {
                using (var reader = new System.Xml.XmlTextReader(new StringReader(z.ReadToEnd())))
                {
                    string header;
                    var m = WebServices.WebService.GetResultFromSoap(reader, out header);

                    var xd = new XmlDocument();
                    var cmd = new SoapRemoteCommand();
                    xd.LoadXml(m);
                    soapAction = soapAction.Trim('\"');
                    if (HttpApplicationServer.SoapRequestsFormat == "ibolt")
                    {
                        cmd = IBoltReplacerController.GetCommandFor(soapAction);
                        if (!string.IsNullOrWhiteSpace(header))
                        {
                            ENV.UserMethods.Instance.SetParam("SOAP_HEADER",
                                "<SOAP_HEADER>" + header.Trim() + "</SOAP_HEADER>");
                        }
                    }
                    if (HttpApplicationServer.SoapRequestsFormat == "systinet")
                    {
                        cmd._operation = DefaultTransformer.GetSystinetOperationPublicName(soapAction);
                        var ns = xd.ChildNodes[0].NamespaceURI;
                        var sig = Encoding.ASCII.GetString(Convert.FromBase64String(soapAction.Substring(soapAction.LastIndexOf('?') + 1)));
                        var mapping = DefaultTransformer.GetMapping(sig, cmd._operation);
                        cmd.setTransformer(mapping, sig, xd);



                        var result = performWork(cmd) as HttpApplicationServer.RequestResult;
                        if (result != null)
                        {
                            var resultData = result.ResultData as SoapResult;
                            response.ContentType = "text/xml; charset=UTF-8";
                            mapping.CreateResponse(ns, sig, response.OutputStream, resultData != null ? resultData.ReturnValue : null, SystinetSoapRequestResponseXmlTransformer.GetWnNamespace(soapAction));
                        }
                    }
                    else
                    {
                        var o = GetSoapActionString(soapAction);
                        if (o.StartsWith("doc:"))
                        {
                            cmd._operation = o.Substring(o.IndexOf("/") + 1);
                            if (cmd._operation.EndsWith("/1,0"))
                                cmd._operation = cmd._operation.Remove(cmd._operation.Length - 4);
                            cmd._argNames.Add("arg");
                            cmd._args.Add(new ProgramCollection.StringOrByteArrayArg(xd.OuterXml));

                            var result = performWork(cmd) as HttpApplicationServer.RequestResult;
                            if (result != null)
                            {
                                response.ContentType = "text/xml; charset=UTF-8";
                                using (var sw = new StreamWriter(response.OutputStream, Encoding.UTF8))
                                {
                                    var r = (SoapResult)result.ResultData;
                                    using (var xw1 = XmlWriter.Create(sw, new XmlWriterSettings
                                    {
                                        Indent = true,
                                        IndentChars = "  ",
                                        NewLineChars = "\r\n",
                                        NewLineHandling = NewLineHandling.Replace
                                    }))
                                        WebServices.WebService.WriteSoapMessage(xw1, r.SoapHeader,
                                            xw =>
                                            {
                                                var bytes = r.ReturnValue as byte[];
                                                if (bytes != null)
                                                {

                                                    var xmlr = XmlReader.Create(new StringReader(ByteArrayColumn.AnsiByteArrayToString(bytes)), new XmlReaderSettings { IgnoreWhitespace = true, ConformanceLevel = ConformanceLevel.Fragment });
                                                    xmlr.MoveToContent();
                                                    xw.WriteNode(xmlr, true);

                                                }
                                            });

                                }
                            }
                        }
                        else
                        {
                            cmd._operation = xd.ChildNodes[0].LocalName;
                            cmd._nameSpace = xd.ChildNodes[0].NamespaceURI;

                            if (soapAction.StartsWith("13C21302"))
                            {
                                cmd._outputArgumentIndices = new int[] { 0 };
                            }
                            else if (soapAction.StartsWith("13C2330263C203"))
                            {
                                cmd._hasReturnValue = true;
                                cmd._outputArgumentIndices = new int[] { 2 };
                            }

                            foreach (XmlNode childNode in xd.ChildNodes[0].ChildNodes)
                            {
                                cmd._argNames.Add(childNode.Name);
                                cmd._args.Add(new ProgramCollection.StringOrByteArrayArg(childNode.InnerText));
                            }
                            if (cmd._outputArgumentIndices.Length > 0)
                            {
                                while (cmd._argNames.Count < cmd._outputArgumentIndices[cmd._outputArgumentIndices.Length - 1] + 1)
                                {
                                    cmd._argNames.Add("out");
                                    cmd._args.Add(new ProgramCollection.StringOrByteArrayArg(""));
                                }
                            }

                            var result = performWork(cmd) as HttpApplicationServer.RequestResult;
                            if (result != null)
                            {
                                response.ContentType = "text/xml; charset=UTF-8";
                                using (var sw = new StreamWriter(response.OutputStream))
                                {
                                    using (var xw1 = new System.Xml.XmlTextWriter(sw))
                                    {
                                        WebServices.WebService.WriteSoapMessage(xw1, "",
                                            xw =>
                                            {
                                                xw.WriteStartElement("m", cmd._operation + "Response", cmd._nameSpace);
                                                xw.WriteAttributeString("xmlns", "m", null, cmd._nameSpace);
                                                var r = result.ResultData as SoapResult;

                                                if (cmd._hasReturnValue && r.ReturnValue != null)
                                                {
                                                    xw.WriteStartElement("Return");
                                                    xw.WriteAttributeString("type", WebServices.WebService.xsiNS,
                                                        r.ReturnValue is Number ? "xsd:double" : "xsd:string");
                                                    xw.WriteString(r.ReturnValue.ToString());
                                                    xw.WriteEndElement();
                                                }

                                                var i = 0;
                                                foreach (var outputArgumentIndex in cmd._outputArgumentIndices)
                                                {
                                                    xw.WriteStartElement(cmd._argNames[outputArgumentIndex]);
                                                    xw.WriteAttributeString("type", WebServices.WebService.xsiNS,
                                                        "xsd:string");
                                                    xw.WriteString(r.OutputColumns[i].ToString());
                                                    xw.WriteEndElement();
                                                    i++;
                                                }
                                                xw.WriteEndElement();
                                            });
                                    }

                                }
                            }
                        }
                    }
                }
            }


        }



        static string GetSoapActionString(string orig)
        {
            var ca = orig.ToCharArray();
            Array.Reverse(ca);
            orig = new string(ca);
            var decripted = "";
            for (int j = 0; j < orig.Length; j += 2)
            {
                decripted += (char)int.Parse(orig.Substring(j, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            return decripted;
        }


    }

    public class SystinetRequestReponseNamespaceMapping
    {
        WsdlSystinetTranslator _wsdl;
        string _outputType;
        public SystinetRequestReponseNamespaceMapping(WsdlSystinetTranslator wsdl, string outputType)
        {

            _wsdl = wsdl;
            _outputType = outputType;
        }
        public string Prefix { get; set; }
        public string RequestNamespace { get; set; }
        public string ReponseNamespace { get; set; }
        public string RequestName { get; set; }
        class myXmlWriterForChangingNamespace : XmlTextWriter
        {
            string _oldNamespace;
            string _newNamespace;

            public myXmlWriterForChangingNamespace(TextWriter w, string oldNamespace, string newNamespace) : base(w)
            {
                _oldNamespace = oldNamespace;
                _newNamespace = newNamespace;
            }

            public override void WriteStartElement(string prefix, string localName, string ns)
            {
                if (ns == _oldNamespace)
                {
                    ns = _newNamespace;

                    var p = LookupPrefix(_newNamespace);
                    if (p != "")
                        prefix = p;
                }

                base.WriteStartElement(prefix, localName, ns.Replace(_oldNamespace, _newNamespace));
            }
        }
        internal void CreateResponse(string ns, string sig, Stream output, object controllerReturnValue, string wnNamespace)
        {
#if DEBUG

            if (controllerReturnValue is byte[])
                System.Diagnostics.Debug.WriteLine(Encoding.UTF8.GetString((byte[])controllerReturnValue));
            else
                System.Diagnostics.Debug.WriteLine(controllerReturnValue);
#endif


            using (var sw = new StreamWriter(output))
            {

                using (var outputStream = ReponseNamespace != null ? (XmlTextWriter)new myXmlWriterForChangingNamespace(sw, ReponseNamespace, wnNamespace) : new XmlTextWriter(sw))
                {
                    outputStream.WriteStartElement("e", "Envelope", XmlSchema.Namespace);
                    outputStream.WriteAttributeString("xmlns", "d", null, XmlSchema.Namespace);
                    outputStream.WriteAttributeString("xmlns", "e", null, WebServices.WebService.envelopeNs);
                    outputStream.WriteAttributeString("xmlns", Prefix, null, wnNamespace);
                    if (ns != wnNamespace)
                        outputStream.WriteAttributeString("xmlns", "wn2", null, ns);
                    outputStream.WriteAttributeString("xmlns", Prefix == "wn1" ? "wn0" : "wn1", null, "http://idoox.com/interface");
                    outputStream.WriteAttributeString("xmlns", "i", null, XmlSchema.InstanceNamespace);

                    outputStream.WriteStartElement("e", "Body", WebServices.WebService.envelopeNs);

                    if (controllerReturnValue != null)
                    {
                        var returnType = sig.Substring(sig.LastIndexOf(')') + 1).TrimEnd(';');
                        
                        var outputDocument = new XmlDocument();
                        var rootType = _wsdl.GetComplexType(_outputType);
                        var rootResponse = outputDocument.CreateElement(_outputType, ReponseNamespace);
                        outputDocument.AppendChild(rootResponse);
                        if (rootType.Count > 0)
                        {
                            var root = outputDocument.CreateElement(rootType[0].Name, ReponseNamespace);
                            _wsdl.setTypeAttribute(root, rootType[0].Type, Prefix);
                            rootResponse.AppendChild(root);
                            if (returnType.StartsWith("L") && !returnType.EndsWith("String"))
                            {
                                var controllerOutputAsXml = new XmlDocument();
                                controllerOutputAsXml.LoadXml(ByteArrayColumn.AnsiByteArrayToString(controllerReturnValue as byte[]));
                                _wsdl.PopulateBasedOn(_wsdl.GetComplexType(rootType[0].Type), root, controllerOutputAsXml.DocumentElement);

                            }
                            else
                            {
                                if (controllerReturnValue is byte[])
                                {
                                    root.AppendChild(outputDocument.CreateCDataSection(ByteArrayColumn.AnsiByteArrayToString(controllerReturnValue as byte[])));
                                }
                                root.InnerText = controllerReturnValue.ToString();

                            }
                        }
                        outputDocument.WriteContentTo(outputStream);
                    }

                    outputStream.WriteEndElement();
                    outputStream.WriteEndElement();
                    outputStream.Flush();
                }
            }
        }


     


        
        internal void CreateRequest(string sig, XmlDocument xd, Action<object> addArg, Action<Action<string, Func<string, object>>> provideControllerArgumentParser)
        {
            var sWriter = new StringWriter();




            if (!sig.StartsWith("(Ljava") && !sig.StartsWith("(DLjava") && !sig.StartsWith("Lorg/") && !sig.StartsWith("()"))
            {
                using (var xWriter = new XmlTextWriter(sWriter))
                {
                    xWriter.WriteRaw("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
                    xWriter.WriteStartElement(Prefix, RequestName, RequestNamespace);
                    foreach (XmlNode childNode in xd.ChildNodes[0].ChildNodes)
                    {
                        xWriter.WriteStartElement(Prefix, childNode.LocalName, RequestNamespace);
                        xWriter.WriteValue(childNode.InnerText);
                        xWriter.WriteEndElement();
                    }
                    xWriter.WriteEndElement();
                }

                addArg(sWriter.ToString());
            }
            else
            {
                provideControllerArgumentParser((paramName, argParser) =>
                {
                    foreach (XmlNode item in xd.ChildNodes[0].ChildNodes)
                    {
                        if (item.LocalName == paramName)
                        {
                            addArg(argParser(item.InnerText));
                        }
                    }
                });
            }
        }

    }
    public delegate void SetMandatoryFields(string parentPath, params string[] mandatoryChildren);
    public delegate void AddArray(string parentPath, string childName, string newChildType = null);

    public class SystinetSoapRequestResponseXmlTransformer
    {


        readonly Dictionary<string, SystinetRequestReponseNamespaceMapping> _sigToNs =
            new Dictionary<string, SystinetRequestReponseNamespaceMapping>(),
            _opToNs = new Dictionary<string, SystinetRequestReponseNamespaceMapping>();

        //string _magicInternalNs = "urn:magic_In_out";



        public void AddOperationMapping(string operation, SystinetRequestReponseNamespaceMapping mapping)
        {
            _opToNs.Add(operation, mapping);
        }
        public void AddSigToNamespaceMapping(string sig, SystinetRequestReponseNamespaceMapping mapping)
        {
            _sigToNs.Add(sig, mapping);
        }




        internal void CreateRequest(string sig, XmlDocument xd, Action<object> addArg, Action<Action<string, Func<string, object>>> provideControllerArgumentParser, string operation)
        {


            GetMapping(sig, operation).CreateRequest(sig, xd, addArg, provideControllerArgumentParser);
        }
        public SystinetRequestReponseNamespaceMapping GetMapping(string sig, string operation)
        {
            SystinetRequestReponseNamespaceMapping mapping;
            if (!_sigToNs.TryGetValue(sig, out mapping))
                mapping = _opToNs[operation];
            return mapping;
        }

        internal void CreateResponse(string ns, string sig, Stream output, string operation, object controllerReturnValue, string wnNamespace)
        {

            GetMapping(sig, operation).CreateResponse(ns, sig, output, controllerReturnValue, wnNamespace);

        }

        readonly Dictionary<string, string> _operationNameToPublicName = new Dictionary<string, string>();

        public void AddOperationToPublicNameMapping(string operationName, string publicName)
        {
            _operationNameToPublicName[operationName.ToUpper()] = publicName;
        }

        internal string GetSystinetOperationPublicName(string soapAction)
        {
            var operationName = new Regex(@"\#(.+)\?").Match(soapAction).Groups[1].Value;
            string publicName;
            _operationNameToPublicName.TryGetValue(operationName.ToUpper(), out publicName);

            return publicName ?? operationName;
        }

        internal static string GetWnNamespace(string soapAction)
        {
            return new Regex(@"http:(.+)/").Match(soapAction).Value;
        }













    }

    public interface Formatter
    {
        object Deserialize(Stream stream);
        void Serialize(Stream stream, object item);
    }

    public interface RemoteCommand<T>
    {
        object Execute(T param);
        bool Async { get; }

    }


    public class LocalDataServer<T> : RemotingProvider<T>
    {
        T _param;

        public LocalDataServer(T param)
        {
            _param = param;
        }

        public object ExecuteOnServer(RemoteCommand<T> command)
        {
            return command.Execute(_param);
        }
    }
    [Serializable]
    class TransfarableException
    {
        string _message;
        TransfarableException _innerException;
        string _stackTrace;


        public string StackTrace
        {
            get { return _stackTrace; }
        }

        public string Message
        {
            get { return _message; }
        }

        public TransfarableException InnerException
        {
            get { return _innerException; }
        }
        public TransfarableException(string message, TransfarableException innerException, string stackTrace)
        {
            _message = message;
            _innerException = innerException;
            _stackTrace = stackTrace;
        }

        public TransfarableException(Exception e)
        {
            _message = e.Message;
            if (e.InnerException != null)
                _innerException = new TransfarableException(e.InnerException);
            _stackTrace = e.StackTrace;

        }

        public virtual Exception CreateException()
        {
            Exception e = null;
            if (_innerException != null)
                e = _innerException.CreateException();
            return new Exception(_message, e);
        }
    }
    public class IBoltReplacerHelper
    {
        private IRemoteApplication _app;
        internal IBoltReplacerHelper(IRemoteApplication app)
        {
            _app = app;
        }

        public readonly NumberColumn FlowSequenceID = new NumberColumn("P_FlowSequenceID", "12");
        public readonly NumberColumn FlowSequenceStep = new NumberColumn("P_FlowSequenceStep", "12");
        public readonly TextColumn ActivationString = new TextColumn("P_ActivationString", "1000");
        public readonly ByteArrayColumn ActivationBlob = new ByteArrayColumn("P_ActivationBlob");
        public readonly NumberColumn FlowID = new NumberColumn("P_FlowID", "12");
        public readonly BoolColumn Transcation = new BoolColumn("P_Transcation");
        public readonly BoolColumn SyncCall = new BoolColumn("P_SyncCall");
        public readonly NumberColumn UserStatusCode = new NumberColumn("P_UserStatusCode", "12");
        public readonly TextColumn UserString = new TextColumn("P_UserString", "1000");
        public readonly ByteArrayColumn UserBlob = new ByteArrayColumn("P_UserBlob");
        public readonly TextColumn ReturnAction = new TextColumn("P_ReturnAction", "1");
        public readonly NumberColumn ErrorCode = new NumberColumn("P_ErrorCode", "12");
        public readonly ByteArrayColumn UserXML = new ByteArrayColumn("P_UserXML");
        public readonly BoolColumn ReturnSetupData = new BoolColumn("P_ReturnSetupData");

        public void RunController(string publicName)
        {
            _app.Run(publicName, this.FlowSequenceID, this.FlowSequenceStep, this.ActivationString, this.ActivationBlob,
                this.FlowID, this.Transcation, this.SyncCall, this.UserStatusCode, this.UserString, this.UserBlob,
                this.ReturnAction, this.ErrorCode, this.UserXML, this.ReturnSetupData);
        }
        public XMLToParamMapper GetMapper()
        {
            return new XMLToParamMapper(UserBlob);
        }

        public class XMLToParamMapper
        {
            private readonly ENV.IO.ByteArrayReader _requestXML;
            private readonly ENV.UserMethods u = new ENV.UserMethods();
            public XMLToParamMapper(ByteArrayColumn column)
            {
                _requestXML = new ENV.IO.ByteArrayReader(column);
            }
            public void Map(string xmlPath, string paramName)
            {
                u.SetParam(paramName, _requestXML.Get(xmlPath));
            }

            public void MapNumber(string xmlPath, string paramName)
            {
                u.SetParam(paramName, Number.Parse(_requestXML.Get(xmlPath)));
            }
        }

    }

    public abstract class IBoltReplacerController
    {
        class IboltReplacerRemoteCommand : SoapRemoteCommand
        {
            private IBoltReplacerController _controller;

            public IboltReplacerRemoteCommand(IBoltReplacerController controller)
            {
                _controller = controller;
            }

            public override object Execute(IRemoteApplication param)
            {
                var soapResult = new SoapResult();
                var replacerHelper = new IBoltReplacerHelper(param);
                replacerHelper.UserBlob.Value = ((ProgramCollection.StringOrByteArrayArg)_args[0]).GetByteArrayParameter().Value;
                _controller.Execute(replacerHelper);
                soapResult.ReturnValue = replacerHelper.UserBlob.Value;
                var soapHeader = ENV.UserMethods.Instance.GetTextParam("SOAP_HEADER");
                if (!Text.IsNullOrEmpty(soapHeader))
                {
                    var dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(soapHeader);
                        soapResult.SoapHeader = dom.DocumentElement.InnerXml;
                    }
                    catch (Exception ex)
                    {
                        soapHeader = soapHeader.Trim();//to simulate the stupid behaviour where there are two soap_header, the need to be returned in a corrupt way - with a closing tag without an opening tag
                        soapResult.SoapHeader = soapHeader.Substring(13, soapHeader.Length - 27).TrimEnd();
                    }

                }
                return soapResult;
            }
        }
        private static Dictionary<string, IBoltReplacerController> _registeredControllers =
            new Dictionary<string, IBoltReplacerController>();
        public static void InitBasedOnControllersFromAssembly(Assembly assembly)
        {
            HttpApplicationServer.SoapRequestsFormat = "ibolt";

            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IBoltReplacerController).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    try
                    {
                        var x = (IBoltReplacerController)System.Activator.CreateInstance(type);
                        _registeredControllers.Add(x.GetSoapAction(), x);
                    }
                    catch (Exception ex)
                    {
                        ENV.ErrorLog.WriteToLogFile(ex);
                    }
                }
            }
        }
        public abstract void Execute(IBoltReplacerHelper h);

        public abstract string GetSoapAction();

        internal static SoapRemoteCommand GetCommandFor(string soapAction)
        {
            return new IboltReplacerRemoteCommand(_registeredControllers[soapAction.Replace("\"", "")]);
        }
    }

}
