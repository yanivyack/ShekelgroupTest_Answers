using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Firefly.Box;

namespace ENV
{
    public class Pop3MailClient : MailClient
    {
        protected override MailClient.Message GetMessageFromServer(int index)
        {
            return new myMessage(index, this);
        }
        class myMessage : Message
        {
            Pop3MailClient _parent;

            public myMessage(int index, Pop3MailClient parent)
                : base(index)
            {
                _parent = parent;
            }

            protected override void FetchHeader()
            {
                FetchBody();
            }

            protected override string GetMessageId(TextReader headerReader)
            {

                using (var sr = new StringReader(_parent.SendLineAndAssertOk("UIDL " + (_index + 1))))
                {
                    return sr.ReadLine().Split(' ')[2];
                }

            }

            protected override string GetBody()
            {
                _parent.SendLine("RETR " + (_index + 1).ToString());
                bool first = true;
                bool done = false;
                using (var sw = new StringWriter())
                {
                    string line = _parent.ReadLineFromSocket();
                    if (!line.StartsWith("+OK"))
                        throw new Exception(string.Format("Pop3 Error - '{0}'", line));
                    while (!done)
                    {
                        line = _parent.ReadLineFromSocket();
                        if (line == ".")
                            done = true;
                        else
                            if (line.StartsWith(".."))
                            sw.WriteLine(line.Substring(1));
                        else
                            sw.WriteLine(line);
                    }
                    return sw.ToString();
                }
            }


        }



        string SendLineAndAssertOk(string command)
        {
            SendLine(command);
            return AssertOk();
        }

        public override void Connect(string host, string username, string password)
        {
            OpenSocket(host, 110);
            AssertOk();
            SendLineAndAssertOk("user " + username);
            SendLineAndAssertOk("pass " + password);
            _totalMessages = int.Parse(SendLineAndAssertOk("stat").Split(' ')[1]);


        }


        string AssertOk()
        {
            var l = ReadLineFromSocket();
            if (!l.StartsWith("+OK"))
                throw new Exception(string.Format("Pop3 Error - '{0}'", l));
            return l;
        }

        public override void Disconnect()
        {
            AssertSocketConnected();
            SendLineAndAssertOk("quit");
            CloseSocket();
        }

        public override void ClearMailbox()
        {
            for (int i = 1; i < _totalMessages + 1; i++)
            {
                SendLineAndAssertOk("dele " + i.ToString());
            }


        }

        public override void DeleteMessage(int index)
        {
            SendLineAndAssertOk("dele " + (index + 1));
        }
    }
    public abstract class MailClient
    {
        protected int _totalMessages = 0;
        public int TotalMessages { get { return _totalMessages; } }
        protected string ReadLineFromSocket()
        {

            var result = new List<byte>();
            while (true)
            {

                var b = new byte[1];
                while (_socket.Available > 0)
                {
                    _socket.Receive(b);
                    if (b[0] == '\n')
                    {
                        if (result[result.Count - 1] == '\r')
                        {
                            result.RemoveAt(result.Count - 1);
                            return _mailEncoding.GetString(result.ToArray());
                        }
                    }
                    result.Add(b[0]);

                }
            }
        }

        protected void CloseSocket()
        {
            _socket.Close();
            _socket = null;

            _totalMessages = 0;
            _messageCache.Clear();
        }
        void SendData(string data)
        {
            AssertSocketConnected();
            _socket.Send(_mailEncoding.GetBytes(data));
        }

        protected void SendLine(string line)
        {
            SendData(string.Format("{0}\r\n", line));
        }

        protected void AssertSocketConnected()
        {
            if (_socket == null || !_socket.Connected)
                throw new System.Net.Mail.SmtpException("Client Not Connected");
        }
        protected void OpenSocket(string host, int port)
        {
            if (_socket != null && _socket.Connected)
                Disconnect();
            var s = host.Split(':');
            var IPhst = Dns.GetHostEntry(s[0]);
            var addr = Array.Find(IPhst.AddressList, address => address.AddressFamily == AddressFamily.InterNetwork);
            var endPt = new IPEndPoint(addr, s.Length > 1 ? int.Parse(s[1]) : port);
            _socket = new Socket(endPt.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = 10000 };
            _socket.Connect(endPt);
        }



        internal static void ReadHeader(TextReader sr, Action<string, string> andSendItTo)
        {
            string line, header = null, value = null;
            Action send = () =>
            {
                if (header != null)
                    andSendItTo(header, value.Trim());
                header = null;
                value = null;
            };
            while (!string.IsNullOrEmpty(line = sr.ReadLine()))
            {
                if (line.StartsWith("\t") || line.StartsWith(" ") || !line.Contains(":"))
                {
                    if (!string.IsNullOrEmpty(value))
                        value += "\r\n";
                    value += line;
                }
                else
                {
                    send();
                    int i = line.IndexOf(":");
                    header = line.Remove(i);
                    value = line.Substring(i + 1);
                }
            }
            send();
        }

        public abstract void Connect(string host, string username, string password);
        public abstract class Message
        {
            protected int _index;

            public Message(int index)
            {

                _index = index;
            }

            protected abstract void FetchHeader();
            bool _bodyFetched;

            protected abstract string GetMessageId(System.IO.TextReader headerReader);
            Dictionary<string, string> _allHeaders = new Dictionary<string, string>();

            protected void ParseHeader(System.IO.TextReader tr)
            {
                _id = GetMessageId(tr);
                ReadHeader(tr, (headerName, headerValue) =>
                {
                    switch (headerName.ToUpper())
                    {
                        case "FROM":
                            _from = headerValue;
                            break;
                        case "TO":
                            _to = headerValue;
                            break;
                        case "CC":
                            _cc = headerValue;
                            break;
                        case "BCC":
                            _bcc = headerValue;
                            break;
                        case "SUBJECT":
                            _subject = headerValue;
                            break;
                        case "DATE":
                            _date = headerValue;
                            break;
                        case "CONTENT-TYPE":
                            _contentType = headerValue;
                            break;
                        case "CONTENT-TRANSFER-ENCODING":
                            _contentTransferEncoding = headerValue;
                            break;
                    }
                    _allHeaders[headerName] = headerValue;

                });



            }

            string _contentType;
            string _contentTransferEncoding;

            string _id;
            string _from;
            string _cc;
            string _bcc;
            string _subject;
            string _date;
            string _text;
            string _to;

            public string Id
            {
                get { FetchHeader(); return _id; }
            }

            public string From
            {
                get { FetchHeader(); return _from; }
            }
            public string To
            {
                get { FetchHeader(); return _to; }
            }

            public string Cc
            {
                get { FetchHeader(); return _cc; }
            }

            public string Bcc
            {
                get { FetchHeader(); return _bcc; }
            }

            public string Subject
            {
                get { FetchHeader(); return _subject; }
            }

            public string Date
            {
                get { FetchHeader(); return _date; }
            }

            public string Text
            {
                get { FetchBody(); return _text; }
            }

            List<Attachment> _attachments = new List<Attachment>();
            public Attachment[] Attachments
            {
                get
                {
                    FetchBody();
                    return _attachments.ToArray();
                }
            }
            public class Attachment
            {
                string _name;
                byte[] _data;

                public Attachment(string name, byte[] data)
                {
                    _name = name;
                    _data = data;
                }

                public string Name { get { return _name; } }

                public byte[] Data
                {
                    get { return _data; }
                }
            }

            static string FromQEncoded(string s)
            {
                var re = new Regex(@"=(.{2})");
                s = re.Replace(s,
                           match =>
                           {
                               byte b;
                               if (byte.TryParse(match.Groups[1].Value, NumberStyles.HexNumber, null, out b))
                                   return _mailEncoding.GetString(new[] { b });
                               return "";
                           });
                return s.Replace("=\r\n", "");
            }

            protected abstract string GetBody();


            protected void FetchBody()
            {
                if (_bodyFetched) return;


                string rawMessage = GetBody();
                var sr = new System.IO.StringReader(rawMessage);
                ParseHeader(sr);

                Func<string, string, string> getText =
                    (text, contentTransferEncoding) =>
                    {
                        if (!string.IsNullOrEmpty(contentTransferEncoding) && contentTransferEncoding.ToUpper(CultureInfo.InvariantCulture) == "QUOTED-PRINTABLE")
                            return FromQEncoded(text);
                        return text;
                    };

                Action<string> addMultipart = delegate { };
                addMultipart =
                    (contentType) =>
                    {
                        var boundry = new Regex("boundary=(.+)").Match(contentType).Groups[1].Value.Replace("\"", "");
                        var currentPartContentType = "";
                        var currentPartContentTransferEncoding = "";
                        var currentPartContentDisposition = "";
                        var currentPartData = new StringBuilder();

                        var line = "";

                        Action addPart =
                            () =>
                            {
                                string name = "";
                                Action<string, string> getFileName =
                                    (propertyName, stringToLookFor) =>
                                    {
                                        if (!string.IsNullOrEmpty(name))
                                            return;
                                        var fileNameMatch = new Regex(propertyName + "=(.+)", RegexOptions.IgnoreCase).Match(stringToLookFor);
                                        name = fileNameMatch.Success ? fileNameMatch.Groups[1].Value : "";

                                        if (name.IndexOf(';') > -1)
                                        {
                                            name = name.Remove(name.IndexOf(';'));
                                        }
                                        name = name.Replace("\"", "");
                                    };
                                getFileName("name", currentPartContentType);
                                getFileName("file", currentPartContentType);
                                getFileName("filename", currentPartContentDisposition);

                                if (name == "" && currentPartContentType.ToUpper(CultureInfo.InvariantCulture).Contains("TEXT"))
                                {
                                    if (string.IsNullOrEmpty(_text))
                                        _text = getText(currentPartData.ToString(), currentPartContentTransferEncoding);
                                }
                                else
                                {
                                    try
                                    {
                                        _attachments.Add(new Attachment(name,
                                            currentPartContentType.ToUpper(CultureInfo.InvariantCulture).Contains("TEXT") || 
                                            currentPartContentTransferEncoding.Equals("quoted-printable", StringComparison.InvariantCultureIgnoreCase) ?
                                            MailClient._mailEncoding.GetBytes(getText(currentPartData.ToString(), currentPartContentTransferEncoding)) :
                                            System.Convert.FromBase64String(currentPartData.ToString())));
                                    }
                                    catch (Exception e)
                                    {
                                        ErrorLog.WriteToLogFile(e, "");
                                    }
                                }
                            };

                        while ((line = sr.ReadLine()) != null && !line.StartsWith(")"))
                        {
                            if (line.StartsWith("--" + boundry))
                            {
                                if (currentPartContentType != "")
                                    addPart();
                                if (line.EndsWith("--")) return;

                                currentPartContentType = "";
                                currentPartContentTransferEncoding = "";
                                currentPartContentDisposition = "";
                                currentPartData = new StringBuilder();

                                ReadHeader(sr, (headerName, headerValue) =>
                                {
                                    switch (headerName.ToUpper())
                                    {
                                        case "CONTENT-TYPE":
                                            currentPartContentType = headerValue;
                                            break;
                                        case "CONTENT-TRANSFER-ENCODING":
                                            currentPartContentTransferEncoding = headerValue;
                                            break;
                                        case "CONTENT-DISPOSITION":
                                            currentPartContentDisposition = headerValue;
                                            break;
                                    }
                                });

                                if (string.IsNullOrEmpty(currentPartContentType)) break;

                                if (currentPartContentType.ToUpper(CultureInfo.InvariantCulture).Contains("MULTIPART"))
                                {
                                    addMultipart(currentPartContentType);
                                    currentPartContentType = "";
                                }
                            }
                            else
                            {
                                if (currentPartData.Length > 0 && currentPartContentType.ToUpper(CultureInfo.InvariantCulture).Contains("TEXT")) currentPartData.AppendLine();
                                currentPartData.AppendLine(line);
                            }
                        }
                        if (currentPartContentType != "")
                            addPart();
                    };

                if (!string.IsNullOrEmpty(_contentType) && _contentType.ToUpper(CultureInfo.InvariantCulture).Contains("MULTIPART"))
                    addMultipart(_contentType);
                else
                {
                    var body = sr.ReadToEnd();

                    _text = getText(body, _contentTransferEncoding);
                }

                _bodyFetched = true;
            }




            public Text GetHeader(Text headerName)
            {
                FetchHeader();
                var s = headerName.TrimEnd().ToUpper(CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(s))
                {
                    Func<string, int> order = (h) =>
                    {
                        switch (h.ToUpper())
                        {
                            case "MESSAGE-ID":
                                return 10;
                            case "TO":
                                return 20;
                            case "FROM":
                                return 30;
                            case "SUBJECT":
                                return 40;
                            case "DATE":
                                return 50;
                            case "RETURN-PATH":
                                return 60;
                            case "RECEIVED":
                                return 70;
                            case "MIME-VERSION":
                                return 75;
                            case "CONTENT-TYPE":
                                return 80;
                            default:
                                return 100;
                        }
                    };

                    using (var sw = new StringWriter())
                    {
                        var ordered = new List<string>(_allHeaders.Keys);
                        ordered.Sort((a, b) => order(a).CompareTo(order(b)));
                        foreach (var h in ordered)
                        {
                            using (var sr = new StringReader(_allHeaders[h]))
                            {
                                string l;
                                while ((l = sr.ReadLine()) != null)
                                {
                                    sw.WriteLine(h + ":" + l);
                                }
                            }
                        }
                        return sw.ToString();
                    }

                }
                else
                {
                    foreach (var allHeader in _allHeaders)
                    {
                        if (allHeader.Key.ToUpper(CultureInfo.InvariantCulture) == headerName.ToUpper(CultureInfo.InvariantCulture))
                            return allHeader.Value;
                    }
                }
                return "";
            }

        }
        Dictionary<int, Message> _messageCache = new Dictionary<int, Message>();
        public MailClient.Message GetMessage(int index)
        {
            Message m;
            if (!_messageCache.TryGetValue(index, out m))
            {
                m = GetMessageFromServer(index);
                _messageCache.Add(index, m);
            }
            return m;
        }

        protected abstract Message GetMessageFromServer(int index);

        public abstract void Disconnect();

        protected Socket _socket;
        protected static Encoding _mailEncoding { get { return LocalizationInfo.Current.OuterEncoding; } }

        public abstract void ClearMailbox();


        public abstract void DeleteMessage(int index);
    }
}