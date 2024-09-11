using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using ENV.Data;
using Firefly.Box;
using Firefly.Box.Data.Advanced;

namespace ENV.WebServices
{
    public class WebService
    {
        string _url;
        bool _addNamespaceToAction;
        public WebService(string url)
        {
            _url = url;
        }
        Dictionary<string, string> _actionMap = new Dictionary<string, string>();
        public WebService(WebServiceInfo info)
        {
            _url = info.Url;
            _addNamespaceToAction = info.AddNamespaceToAction;
            _actionMap = info.GetOperationToSoapActionMap();
        }
        public static string envelopeNs = "http://schemas.xmlsoap.org/soap/envelope/";
        internal const string xsiNS = "http://www.w3.org/2001/XMLSchema-instance";

        public WebServiceResult InternalRun(Text soapAction, Text header, Text body, Action<string> resultColumn)
        {
            if (body == null)
                body = "";
            return InternalRun(soapAction, header,
                (writer, sendNs) =>
                {if (body != "")
                    {
                        var s = body.ToString();
                        var p = s.IndexOf("<");
                        if (p > 0)
                            s = s.Substring(p);

                        try
                        {
                            var xd = new XmlDocument();
                            xd.LoadXml(s);
                            if (!string.IsNullOrEmpty(xd.DocumentElement.NamespaceURI))
                                sendNs(xd.DocumentElement.NamespaceURI);
                        }
                        catch
                        {
                        }

                        using (var bodyXmlReader = new System.Xml.XmlTextReader(new StringReader(s)))
                        {
                            bodyXmlReader.MoveToContent();
                            writer.WriteRaw(bodyXmlReader.ReadOuterXml());
                        }
                    }
                }, resultColumn);
        }
        public WebServiceResult Run(Text soapAction, Text header, Text body, ByteArrayColumn resultColumn)
        {
            return InternalRun(soapAction, header, body,
                result =>
                {
                    if (resultColumn != null)
                    {
                        if (resultColumn.ContentType == ByteArrayColumnContentType.BinaryUnicode)
                            resultColumn.Value = ByteArrayColumn.ToAnsiByteArray(result);
                        else
                            resultColumn.Value = resultColumn.FromString(result);
                    }
                });
        }
        public WebServiceResult Run(Text soapAction, byte[] header, Text body, ByteArrayColumn resultColumn)
        {
            return InternalRun(soapAction, TextColumn.FromByteArray(header), body,
                result =>
                {
                    if (resultColumn != null)
                    {
                        if (resultColumn.ContentType == ByteArrayColumnContentType.BinaryUnicode)
                            resultColumn.Value = ByteArrayColumn.ToAnsiByteArray(result);
                        else
                            resultColumn.Value = resultColumn.FromString(result);
                    }
                });
        }
        public WebServiceResult Run(Text soapAction, ByteArrayColumn header, Text body, ByteArrayColumn resultColumn)
        {
            return InternalRun(soapAction, header.ToString(), body,
                result =>
                {
                    if (resultColumn != null)
                    {
                        if (resultColumn.ContentType == ByteArrayColumnContentType.BinaryUnicode)
                            resultColumn.Value = ByteArrayColumn.ToAnsiByteArray(result);
                        else
                            resultColumn.Value = resultColumn.FromString(result);
                    }
                });
        }
        public WebServiceResult Run(Text soapAction, HeaderInfo header, byte[] body, ByteArrayColumn resultColumn)
        {
            return InternalRun(soapAction, header.ToString(), TextColumn.FromByteArray(body),
                result =>
                {
                    if (resultColumn != null)
                    {
                        if (resultColumn.ContentType == ByteArrayColumnContentType.BinaryUnicode)
                            resultColumn.Value = ByteArrayColumn.ToAnsiByteArray(result);
                        else
                            resultColumn.Value = resultColumn.FromString(result);
                    }
                });
        }
        public WebServiceResult Run(Text soapAction, HeaderInfo header, ByteArrayColumn body, ByteArrayColumn resultColumn)
        {
            return InternalRun(soapAction, header.ToString(), body,
                  result =>
                  {
                      if (resultColumn != null)
                      {
                          if (resultColumn.ContentType == ByteArrayColumnContentType.BinaryUnicode)
                              resultColumn.Value = ByteArrayColumn.ToAnsiByteArray(result);
                          else
                              resultColumn.Value = resultColumn.FromString(result);
                      }
                  });
        }
        public WebServiceResult Run(Text soapAction, RemoteProcedureCallInfo rpc, ByteArrayColumn resultColumn)
        {

            return InternalRun(soapAction, rpc, result =>
            {
                if (resultColumn != null)
                {
                    if (resultColumn.ContentType == ByteArrayColumnContentType.BinaryUnicode)
                        resultColumn.Value = ByteArrayColumn.ToAnsiByteArray(result);
                    else
                        resultColumn.Value = resultColumn.FromString(result);
                }
            });
        }
        public WebServiceResult Run(Text soapAction, ByteArrayColumn resultColumn)
        {
            return Run(soapAction, "", "", resultColumn);
        }
        public WebServiceResult InternalRun(Text soapAction, RemoteProcedureCallInfo rpc, Action<string> resultColumn)
        {
            return InternalRun(soapAction, "", (writer, sendNs) => rpc.WriteTo(writer), x => resultColumn(rpc.ParseResult(x)));
        }

        public class WebServiceResult
        {
            public WebServiceResult(bool successful, string errorText)
            {
                Successful = successful;
                ErrorText = errorText;
            }

            public bool Successful { get; private set; }
            public string ErrorText { get; private set; }

        }

        public static bool DisableProxy = false;

        delegate void WriteSoapBody(XmlWriter writer, Action<string> sendNamespace);

        WebServiceResult InternalRun(Text soapAction, Text header, WriteSoapBody writeBody, Action<string> resultColumn)
        {
            string url = PathDecoder.DecodePath(_url);
            using (ENV.Utilities.Profiler.StartContext("WSCall " + url))
            {
                try
                {
                    System.Net.WebRequest request = System.Net.WebRequest.Create(url);
                    request.Timeout = UserSettings.HttpWebRequestTimeoutInSeconds * 1000;
                    request.Method = "POST";
                    request.ContentType = "text/xml; charset=utf-8";
                    var httpRequest = request as HttpWebRequest;
                    if (httpRequest != null)
                        httpRequest.UserAgent = "EasySoap++/0.6";
                    var proxyAddress = ENV.UserMethods.Proxy.Value;
                    if (Firefly.Box.Text.IsNullOrEmpty(proxyAddress) || DisableProxy)
                        request.Proxy = null;
                    else
                        request.Proxy = new WebProxy(proxyAddress);


                    System.IO.Stream requestStream = request.GetRequestStream();

                    System.Xml.XmlWriter writerForMessage =
                        new System.Xml.XmlTextWriter(requestStream, System.Text.Encoding.UTF8);

                    var bodyNs = "";
                    WriteSoapMessage(writerForMessage, header, writer => writeBody(writer, s => bodyNs = s + "/"));
                    var sa = soapAction.ToString();
                    if (_actionMap.TryGetValue(sa, out sa))
                        soapAction = sa;
                    else if (_addNamespaceToAction && !soapAction.Contains("/"))
                        soapAction = string.Format("\"{0}\"", bodyNs.TrimEnd() + soapAction);
                    request.Headers.Add("SOAPAction", soapAction);

                    requestStream.Close();

                    var response = request.GetResponse();
                    using (var reader = new System.Xml.XmlTextReader(response.GetResponseStream()))
                    {
                        string result = GetResultFromSoap(reader);

                        resultColumn(result);
                        return new WebServiceResult(true, null);
                    }

                }
                catch (Exception e)
                {
                    ErrorLog.WriteToLogFile(e, "");


                    try
                    {
                        var ex = e as WebException;
                        if (ex != null)
                        {
                            var reader = new System.Xml.XmlTextReader(ex.Response.GetResponseStream());
                            reader.MoveToContent();
                            reader.ReadStartElement("Envelope", envelopeNs);
                            reader.MoveToContent();
                            reader.ReadStartElement("Body", envelopeNs);
                            reader.MoveToContent();

                            var result = reader.ReadOuterXml();
                            var d = new System.Xml.XmlDocument();
                            d.LoadXml(result);
                            foreach (var item in d.FirstChild.ChildNodes)
                            {
                                var el = item as System.Xml.XmlElement;
                                if (el.Name == "faultstring")
                                {
                                    return new WebServiceResult(false, el.InnerText);

                                }


                            }
                        }
                    }
                    catch (Exception e2)
                    {
                        ErrorLog.WriteToLogFile(e2, "");
                    }

                    return new WebServiceResult(false, e.Message);
                }
            }
        }

        public static ContextStatic<string> Username = new ContextStatic<string>(() => "");
        public static ContextStatic<string> Password = new ContextStatic<string>(() => "");

        public static void WriteSoapMessage(XmlWriter w, Text header, Action<XmlWriter> writeBody)
        {
            var envelopeNsAlias = "SOAP-ENV";
            w.WriteStartElement(envelopeNsAlias, "Envelope", envelopeNs);
            w.WriteAttributeString("xmlns", envelopeNsAlias, null, envelopeNs);
            w.WriteAttributeString("xmlns", "SOAPEnc", null,
                                                  "http://schemas.xmlsoap.org/soap/encoding/");
            w.WriteAttributeString("xmlns", "xsi", null, xsiNS);
            w.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
            w.WriteAttributeString(envelopeNsAlias, "encodingStyle", null, "http://schemas.xmlsoap.org/soap/encoding/");

            var username = Username.Value.Trim();
            if (!string.IsNullOrWhiteSpace( header) || !string.IsNullOrWhiteSpace(username ))
            {
                w.WriteStartElement(envelopeNsAlias, "Header", envelopeNs);
                try
                {






                    //validate xml to prevet partial writing of the header element, when the header is invalid.
                    {
                        var xmlr = XmlReader.Create(new StringReader(header.Trim()),
                            new XmlReaderSettings { IgnoreWhitespace = true, ConformanceLevel = ConformanceLevel.Fragment });
                        var sw = new StringWriter();
                        var xmlw = XmlWriter.Create(sw, new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment });
                        xmlw.WriteNode(xmlr, true);
                    }
                    {
                        var xmlr = XmlReader.Create(new StringReader(header.Trim()),
                            new XmlReaderSettings
                            {
                                IgnoreWhitespace = true,
                                ConformanceLevel = ConformanceLevel.Fragment
                            });
                        w.WriteNode(xmlr, true);
                    }
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrWhiteSpace(header))
                        w.WriteRaw(header.Trim());

                }
                /*if (header.Trim() != "")
                {
                    using (var headerXmlReader = new System.Xml.XmlTextReader(new StringReader(header.ToString())))
                    {
                        headerXmlReader.MoveToContent();
                        w.WriteRaw(headerXmlReader.ReadOuterXml());
                    }
                }*/
                if (username != "")
                {
                    var securityNsAlias = "wsse";
                    var securityNs = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
                    var securityUtilityAlias = "wsu";
                    var securityUtilityNs = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
                    w.WriteStartElement(securityNsAlias, "Security", securityNs);
                    w.WriteAttributeString("mustUnderstand", envelopeNs, "1");
                    w.WriteAttributeString("xmlns", securityNsAlias, null, securityNs);
                    w.WriteAttributeString("xmlns", securityUtilityAlias, null, securityUtilityNs);

                    w.WriteStartElement("Timestamp", securityUtilityNs);
                    w.WriteAttributeString("Id", securityUtilityNs, "TS1");
                    var now = System.DateTime.UtcNow;
                    w.WriteElementString("Created", securityUtilityNs, now.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    w.WriteElementString("Expires", securityUtilityNs, now.AddMinutes(5).ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    w.WriteEndElement();

                    w.WriteStartElement("UsernameToken", securityNs);
                    w.WriteAttributeString("Id", securityUtilityNs, "UT1");
                    w.WriteElementString("Username", securityNs, username);
                    w.WriteStartElement("Password", securityNs);
                    w.WriteAttributeString("Type", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText");
                    w.WriteString(Password.Value);
                    w.WriteEndElement();
                    w.WriteElementString("Created", securityUtilityNs, now.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    w.WriteElementString("Nonce", securityNs, System.Convert.ToBase64String(System.Guid.NewGuid().ToByteArray()));
                    w.WriteEndElement();
                    w.WriteEndElement();
                }
                w.WriteEndElement();
            }

            w.WriteStartElement(envelopeNsAlias, "Body", envelopeNs);
            writeBody(w);
            w.WriteEndElement();

            w.WriteEndElement();
            w.Flush();
        }

        public static string GetResultFromSoap(XmlTextReader reader)
        {
            string s;
            return GetResultFromSoap(reader, out s);
        }

        public static string GetResultFromSoap(XmlTextReader reader, out string header)
        {
            header = null;
            reader.MoveToContent();
            reader.ReadStartElement("Envelope", envelopeNs);
            reader.MoveToContent();
            if (reader.IsStartElement("Header", envelopeNs))
            {
                header = reader.ReadInnerXml();
            }
            if (!reader.IsStartElement("Body", envelopeNs))
                reader.ReadToFollowing("Body", envelopeNs);
            reader.ReadStartElement("Body", envelopeNs);
            reader.MoveToContent();

            var result = reader.ReadOuterXml();
            return result;
        }

        public delegate void SoapRequestPrgHandler(string prgName, Action<Action<string, string>> content, string nameSpace);
        public static void SendSoapRequestInfoTo(string s, SoapRequestPrgHandler to)
        {
            var x = new XmlDocument();
            x.LoadXml(s);
            to(x.ChildNodes[0].LocalName, send =>
            {
                foreach (XmlNode childNode in x.ChildNodes[0].ChildNodes)
                {
                    send(childNode.Name, childNode.InnerText);
                }

            }, x.ChildNodes[0].NamespaceURI);
        }

        public WebServiceResult Run(Text soapAction, Text body, ByteArrayColumn resultColumn)
        {
            return Run(soapAction, "", body, resultColumn);
        }

        public WebServiceResult Run(Text soapAction, Text body, BoolColumn resultColumn)
        {
            return InternalRun(soapAction, "", body, x =>
            {
                try
                {
                    resultColumn.Value = x == "true";
                }
                catch
                {
                }
            });
        }

        public WebServiceResult Run(Text soapAction, byte[] body, ByteArrayColumn resultColumn)
        {
            return Run(soapAction, "", TextColumn.FromByteArray(body), resultColumn);
        }
        public WebServiceResult Run(Text soapAction, ByteArrayColumn body, ByteArrayColumn resultColumn)
        {
            return Run(soapAction, "", body.ToString(), resultColumn);
        }




        public WebServiceResult Run(Text soapAction, Text body, TextColumn resultColumn)
        {
            ByteArrayColumn c = new ByteArrayColumn();
            var result = Run(soapAction, body, c);
            resultColumn.Value = c.ToString();
            return result;
        }
        public WebServiceResult Run(Text soapAction, Text body)
        {
            ByteArrayColumn c = new ByteArrayColumn();
            var result = Run(soapAction, body, c);

            return result;
        }




        public WebServiceResult Run(Text soapAction, RemoteProcedureCallInfo body, NumberColumn resultColumn)
        {
            body.UseOutParameter = true;
            return InternalRun(soapAction, body, x =>
            {
                try
                {
                    resultColumn.Value = Number.Parse(x);
                }
                catch
                {
                }
            });
        }
        public WebServiceResult Run(Text soapAction, RemoteProcedureCallInfo body, BoolColumn resultColumn)
        {
            body.UseOutParameter = true;
            return InternalRun(soapAction, body, x =>
            {
                try
                {
                    resultColumn.Value = x == "true";
                }
                catch
                {
                }
            });
        }

        public WebServiceResult Run(Text soapAction, RemoteProcedureCallInfo body)
        {
            body.UseOutParameter = true;
            return Run(soapAction, body, new ByteArrayColumn());
        }

        public WebServiceResult Run(Text soapAction, Text header, Text body, TextColumn resultColumn)
        {
            return InternalRun(soapAction, header, body, x => resultColumn.Value = x);
        }
        public WebServiceResult Run(Text soapAction, HeaderInfo header, Text body, TextColumn resultColumn)
        {
            return InternalRun(soapAction, header.ToString(), body, x => resultColumn.Value = x);
        }
        public WebServiceResult Run(Text soapAction, HeaderInfo header, Text body)
        {
            return InternalRun(soapAction, header.ToString(), body, x => { });
        }
        public WebServiceResult Run(Text soapAction, HeaderInfo header, ByteArrayColumn body)
        {
            return InternalRun(soapAction, header.ToString(), TextColumn.FromByteArray(body), x => { });
        }
        public WebServiceResult Run(Text soapAction, RemoteProcedureCallInfo body, TextColumn resultColumn)
        {
            return InternalRun(soapAction, body, x => resultColumn.Value = x);
        }
        public WebServiceResult Run(Text soapAction, TextColumn resultColumn)
        {
            return Run(soapAction, "", "", resultColumn);
        }
        public WebServiceResult Run(Text soapAction)
        {
            return Run(soapAction, "", "", new TextColumn());
        }
    }
    public class WebServiceInfo
    {
        public string Url { get; private set; }
        internal bool AddNamespaceToAction { get; private set; }
        string _wsdl;
        internal WebServiceInfo(string url, string wsdl)
        {
            Url = url;
            AddNamespaceToAction = wsdl != null;
            _wsdl = wsdl;
        }
        string _lastWsdl = null;
        Dictionary<string, string> _map = new Dictionary<string, string>();
        public Dictionary<string, string> GetOperationToSoapActionMap()
        {
            var wsdl = ENV.PathDecoder.DecodePath(_wsdl);
            if (wsdl != _lastWsdl)
            {
                _lastWsdl = wsdl;
                try
                {
                    _map = GetOperationToSoapActionMap(wsdl);
                }
                catch (Exception ex)
                {
                    ErrorLog.WriteToLogFile(ex, "Loading the wsdl of a soap web service");
                    _map = new Dictionary<string, string>();
                }
            }
            return _map;
        }

        private static Dictionary<string, string> GetOperationToSoapActionMap(string wsdl)
        {
            var translateOperation = new Dictionary<string, string>();
            var req = System.Net.WebRequest.Create(wsdl);
            var res = req.GetResponse();
            var xdom = new System.Xml.XmlDataDocument();
            xdom.Load(res.GetResponseStream());
            foreach (var item in xdom.DocumentElement.ChildNodes)
            {
                var e = item as System.Xml.XmlElement;
                if (e != null && e.LocalName == "binding")
                {
                    foreach (var item2 in e.ChildNodes)
                    {
                        var operation = item2 as System.Xml.XmlElement;
                        if (operation != null && operation.LocalName == "operation")
                        {
                            var opName = operation.GetAttribute("name");
                            foreach (var item3 in operation.ChildNodes)
                            {
                                var soapOp = item3 as System.Xml.XmlElement;
                                if (soapOp != null && soapOp.LocalName == "operation")
                                {
                                    var soapAction = soapOp.GetAttribute("soapAction");
                                    if (soapAction != null)
                                    {
                                        if (!translateOperation.ContainsKey(opName))
                                            translateOperation.Add(opName, soapAction);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return translateOperation;
        }

    }
    public class HeaderInfo
    {
        StringBuilder _result = new StringBuilder();
        public void Add(string @namespace, string name, Text value)
        {
            if (value == null)
                value = "";
            _result.AppendFormat("<{0} xmlns=\"{1}\">{2}</{0}>", name, @namespace, value.TrimEnd());
        }
        public override string ToString()
        {
            return _result.ToString();
        }
    }

    public class RemoteProcedureCallInfo
    {
        string _name;
        string _nameSpace;

        public RemoteProcedureCallInfo(string name, string nameSpace)
        {
            _name = name;
            _nameSpace = nameSpace;
        }

        class RemoteProcedureCallParameter
        {
            string _name;
            string _xsdType;
            string _value;
            Action<string> _parseResult;
            public RemoteProcedureCallParameter(string name, string xsdType, string value, Action<string> parseResult)
            {
                _parseResult = parseResult;
                _name = name;
                _xsdType = xsdType;
                _value = value;
            }

            public void WriteTo(XmlWriter x)
            {
                x.WriteStartElement(_name);
                x.WriteAttributeString("type", WebService.xsiNS, "xsd:" + _xsdType);
                if (_value == null)
                    x.WriteAttributeString("nil", WebService.xsiNS, "true");
                else
                {
                    if (_xsdType == "any")
                        x.WriteRaw(_value.TrimEnd());
                    else
                        x.WriteString(_value.TrimEnd());
                }
                x.WriteFullEndElement();
            }

            public void SetResult(string val)
            {
                _parseResult(val);
            }
        }

        List<RemoteProcedureCallParameter> _parameters = new List<RemoteProcedureCallParameter>();
        Dictionary<string, RemoteProcedureCallParameter> _parametersByName = new Dictionary<string, RemoteProcedureCallParameter>();

        public bool UseOutParameter { get; set; }


        public void AddParameter(string name, string xsdType, Text value)
        {
            AddParameterInternal(name, xsdType, value, Dummy);
        }
        public void AddParameter(string name, string xsdType, object value)
        {
            AddParameterInternal(name, xsdType, value, Dummy);
        }
        public void AddParameter(string name, string xsdType, Bool value)
        {
            AddParameterInternal(name, xsdType, value, Dummy);
        }
        public void AddParameter(string name, string xsdType, Firefly.Box.Data.TextColumn column)
        {
            AddParameterInternal(name, xsdType, column, y => column.Value = y);
        }
        public void AddParameter(string name, string xsdType, Firefly.Box.Data.ByteArrayColumn column)
        {
            AddParameterInternal(name, xsdType, xsdType.ToUpperInvariant() == "BASE64BINARY" ? (column.Value == null ? "" : Convert.ToBase64String(column)) : column.ToString(), y => column.Value = column.FromString(y));
        }
        public void AddParameter(string name, string xsdType, ByteArrayColumn column)
        {
            AddParameter(name, xsdType, (Firefly.Box.Data.ByteArrayColumn)column);
        }
        public void AddParameter(string name, string xsdType, NumberColumn column)
        {
            if (xsdType.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                xsdType = "int";
            AddParameterInternal(name, xsdType, column, y => column.Value = Number.Parse(y));
        }
        public void AddParameter(string name, string xsdType, Number value)
        {
            if (xsdType.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                xsdType = "int";
            AddParameterInternal(name, xsdType, value, Dummy);
        }

        static Action<string> Dummy = delegate { };
        public void AddParameter(string name, string xsdType, Date value)
        {
            AddParameterInternal(name, xsdType, value.ToString("YYYY-MM-DD"), Dummy);
        }
        public void AddParameter(string name, string xsdType, Firefly.Box.Data.DateColumn column)
        {
            AddParameterInternal(name, xsdType, column.ToString("YYYY-MM-DD"), y => column.Value = Date.Parse(y, "YYYY-MM-DD"));
        }

        void AddParameterInternal(string name, string xsdType, object value, Action<String> parseResult)
        {

            var cb = value as ColumnBase;
            if (cb != null)
                value = cb.Value;

            string s = null;
            if (value != null)
            {
                s = value.ToString();
                if (xsdType.ToUpper().Contains("STRING"))
                    s = s.Trim();
            }
            var x = new RemoteProcedureCallParameter(name, xsdType, s, parseResult);
            _parameters.Add(x);
            _parametersByName.Add(name.ToUpper(), x);
        }

        public void WriteTo(XmlWriter x)
        {
            x.WriteStartElement("m", _name, _nameSpace);
            x.WriteAttributeString("xmlns", "m", null, _nameSpace);

            foreach (var p in _parameters)
                p.WriteTo(x);

            x.WriteEndElement();
        }

        public string ParseResult(string resultXml)
        {
            try
            {
                string result = "";
                var d = new XmlDocument();
                d.LoadXml(resultXml);
                int i = 0;
                if (d.FirstChild.HasChildNodes)
                {

                    foreach (var childNode in d.FirstChild.ChildNodes)
                    {
                        var e = childNode as XmlElement;
                        if (e != null)
                        {
                            RemoteProcedureCallParameter p;
                            if (_parametersByName.TryGetValue(e.Name.ToUpper(), out p))
                                p.SetResult(e.InnerText);
                            else if (i == 0)
                                result = e.InnerText;

                            i++;
                        }

                    }
                    return result;
                }
                return d.FirstChild.InnerText;
            }
            catch
            {
                return "";
            }
        }


    }
}
