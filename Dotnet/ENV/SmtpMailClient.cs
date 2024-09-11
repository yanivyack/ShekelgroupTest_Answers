using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using Firefly.Box;

namespace ENV
{
    class SmtpMailClient
    {
        Socket _socket;

        public void Connect(string host, string username, string password)
        {

            var hostAndPort = host.Split(':');
            IPHostEntry IPhst;
            IPEndPoint endPt;
            IPAddress ip;
            if (IPAddress.TryParse(hostAndPort[0], out ip))
            {
                IPhst = new IPHostEntry();
                IPhst.HostName = hostAndPort[0];
                endPt = new IPEndPoint(ip, hostAndPort.Length > 1 ? int.Parse(hostAndPort[1]) : 25);
            }
            else
            {
                IPhst = Dns.GetHostEntry(hostAndPort[0]);
            var i = 0;
            while (i < IPhst.AddressList.Length && IPhst.AddressList[i].AddressFamily != AddressFamily.InterNetwork)
                i++;
                endPt = new IPEndPoint(IPhst.AddressList[i >= IPhst.AddressList.Length ? 0 : i], hostAndPort.Length > 1 ? int.Parse(hostAndPort[1]) : 25);
            }

            if (_socket != null && _socket.Connected && _socket.RemoteEndPoint.ToString() == endPt.ToString())
                return;

            _socket = new Socket(endPt.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = OperationTimeout, SendTimeout = OperationTimeout };
            _socket.Connect(endPt);

            var connectSucceeded = false;
            try
            {
                AssertNetworkResponse(220);

                SendLine(string.Format("EHLO {0}", IPhst.HostName));

                AssertNetworkResponse(250);

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    SendLine("AUTH LOGIN");
                    AssertNetworkResponse(334);
                    SendLine(Convert.ToBase64String(Encoding.Default.GetBytes(username)));
                    AssertNetworkResponse(334);
                    SendLine(Convert.ToBase64String(Encoding.Default.GetBytes(password)));
                    AssertNetworkResponse(235);
                }
                connectSucceeded = true;
            }
            finally
            {
                if (!connectSucceeded)
                {
                    _socket.Close();
                    _socket = null;
                }
            }
        }

        void SendData(string data)
        {
            using (Utilities.Profiler.StartContext("SmtpMailClient Sending: '" + data + "'"))
            {
                _socket.Send(_mailEncoding.GetBytes(data));
            }
        }

        void SendLine(string line)
        {
            SendData(string.Format("{0}\r\n", line));
        }

        static Encoding _mailEncoding { get { return Encoding.Default; } }

        public static int OperationTimeout { get; set; }

        string ToQEncoded(string text)
        {
            var sb = new StringBuilder();
            var charsInLine = 0;
            foreach (byte b in _mailEncoding.GetBytes(text))
            {
                if (b >= 33 && b <= 126 && b != 61)
                {
                    charsInLine++;
                    if (charsInLine >= 76)
                    {
                        sb.AppendLine("=");
                        charsInLine = 1;
                    }
                    sb.Append((char)b);
                    continue;
                }
                var x = string.Format("{0:X00}", b);
                if (x.Length < 2)
                    x = "0" + x;

                charsInLine += 3;
                if (charsInLine >= 76)
                {
                    sb.AppendLine("=");
                    charsInLine = 3;
                }
                sb.AppendFormat("={0}", x);
            }
            return sb.ToString();
        }

        public void Disconnect()
        {
            if (_socket == null || !_socket.Connected)
                throw new System.Net.Mail.SmtpException("Client Not Connected");

            SendLine("QUIT");
            //AssertNetworkResponse(221);
            _socket.Close();
            _socket = null;
        }

        static string FixAddress(string address)
        {
            var result = address.Trim().Replace(" ", "");
            if (result.Contains("<") && result.Contains(">") && result.IndexOf(">") > result.IndexOf("<"))
                return result.Substring(result.IndexOf("<") + 1, result.IndexOf(">") - result.IndexOf("<")-1);
            return result;
        }

        public void SendMail(string from, string to, string cc, string bcc, string subject,
            string messageText, string[] attachments)
        {
            if (_socket == null || !_socket.Connected)
                throw new System.Net.Mail.SmtpException("ClientNotConnectedException");

            SendLine("RSET");
            AssertNetworkResponse(250);

            SendLine(string.Format("MAIL From: <{0}>", FixAddress(from)));

            AssertNetworkResponse(250);

            var atLeastOneGoodAddress = false;
            var atLeastOneWrongAddress = false;
            string wrongRecipients = "";

            Action<string> addRecipient =
                delegate(string obj)
                {
                    if (!string.IsNullOrEmpty(obj))
                    {
                        SendLine(string.Format("RCPT TO: <{0}>", FixAddress(obj)));
                        if (GetNetworkResponse() != 250)
                        {
                            atLeastOneWrongAddress = true;
                            wrongRecipients += FixAddress(obj);
                        }
                        else
                            atLeastOneGoodAddress = true;
                    }
                };
            Array.ForEach<string>(to.Split(','), addRecipient);
            Array.ForEach<string>(cc.Split(','), addRecipient);
            Array.ForEach<string>(bcc.Split(','), addRecipient);

            if (!atLeastOneGoodAddress)
                throw new InvalidRecipientAddressException(wrongRecipients);

            var messsage = new StringBuilder();
            messsage.AppendLine(string.Format("From: {0}", from));
            messsage.AppendLine(string.Format("To: {0}", to));
            messsage.AppendLine(string.Format("Cc: {0}", cc));

            messsage.AppendLine(string.Format("Date: {0}", DateTime.Now.ToString("ddd, dd MMM yy HH:mm:ss zz", System.Globalization.CultureInfo.InvariantCulture)));
            messsage.AppendLine(string.Format("Subject: {0}", subject));
            messsage.AppendLine("MIME-Version: 1.0");

            Action<string, string, string[], Action<StringBuilder>> addPart =
                (contentType, contentTransferEncoding, extraHeaders, writePartBody) =>
                {
                    messsage.AppendLine(string.Format("Content-Type: {0}", contentType));
                    if (!string.IsNullOrEmpty(contentTransferEncoding))
                        messsage.AppendLine(string.Format("Content-Transfer-Encoding: {0}", contentTransferEncoding));
                    foreach (var extraHeader in extraHeaders)
                        messsage.AppendLine(extraHeader);
                    messsage.AppendLine();
                    writePartBody(messsage);
                    messsage.AppendLine();
                };

            var parts = new List<Action>();

            parts.Add(
                () =>
                {
                    Action<string, bool> addMessageTextPart =
                        (textType, useQuotedPrintable) =>
                        {
                            addPart(string.Format("text/{0}; charset={1}", textType, _mailEncoding.BodyName),
                                    useQuotedPrintable ? "quoted-printable" : "", new string[] { },
                                    sb =>
                                    {
                                        sb.AppendLine(useQuotedPrintable ? ToQEncoded(messageText) : messageText);
                                        if (!messageText.EndsWith("\r\n"))
                                            sb.AppendLine();
                                    });
                        };
                    if (messageText.Length > 6 && messageText.Substring(0, 6).ToUpper(CultureInfo.InvariantCulture) == "<HTML>")
                    {
                        addPart("multipart/alternative;boundary=unique-boundary-2", "quoted-printable", new string[] { },
                                sb =>
                                {
                                    sb.AppendLine("--unique-boundary-2");
                                    addMessageTextPart("plain", true);
                                    sb.AppendLine("--unique-boundary-2");
                                    addMessageTextPart("html", true);
                                    sb.AppendLine("--unique-boundary-2--");
                                });
                    }
                    else
                        addMessageTextPart("plain", false);
                });



            if (attachments != null)
            {
                foreach (var s in attachments)
                {
                    var fileName = (s ?? "").Trim();
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var f = new FileInfo(fileName);
                        parts.Add(
                            () =>
                            {
                                addPart("application/octet-stream; name=\"" + f.Name + "\";", "base64",
                                        new[] { "Content-Disposition: attachment; filename=\"" + f.Name + "\";" },
                                        sb =>
                                        {
                                            try
                                            {
                                                var fs = f.OpenRead();
                                                var binaryData = new Byte[fs.Length];
                                                fs.Read(binaryData, 0, (int)fs.Length);
                                                fs.Close();
                                                var base64String = System.Convert.ToBase64String(binaryData, 0, binaryData.Length);

                                                for (int i = 0; i < base64String.Length; )
                                                {
                                                    int nextchunk = 100;
                                                    if (base64String.Length - (i + nextchunk) < 0)
                                                        nextchunk = base64String.Length - i;
                                                    sb.Append(base64String.Substring(i, nextchunk));
                                                    sb.AppendLine();
                                                    i += nextchunk;
                                                }
                                                sb.AppendLine();
                                            }
                                            catch (IOException e)
                                            {
                                                ENV.ErrorLog.WriteToLogFile(e, "");
                                                throw new AttachmentNotFoundException();
                                            }
                                        });
                            });

                    }
                }
            }

            if (parts.Count == 1)
                parts[0]();
            else
            {
                messsage.AppendLine("Content-Type: multipart/mixed; boundary=unique-boundary-1");
                messsage.AppendLine();
                messsage.AppendLine("This is a multi-part message in MIME format.");
                parts.ForEach(
                    action =>
                    {
                        messsage.AppendLine("--unique-boundary-1");
                        action();
                    });
                messsage.AppendLine("--unique-boundary-1--");
            }

            messsage.AppendLine(".");
            //messsage.AppendLine();
            //messsage.AppendLine();

            SendLine("DATA");
            AssertNetworkResponse(354);
            SendData(messsage.ToString());

            AssertNetworkResponse(250);


            if (atLeastOneWrongAddress)
                throw new InvalidRecipientAddressException(wrongRecipients);
        }


        int GetNetworkResponse()
        {
            string response;
            int responseCodeReceived;
            var bytes = new byte[1024];
            using (Utilities.Profiler.StartContext("SmtpMailClient Received"))
            {
                _socket.Receive(bytes);
                response = ENV.LocalizationInfo.Current.OuterEncoding.GetString(bytes);
                using (Utilities.Profiler.StartContext(response.TrimEnd('\0')))
                {
                    responseCodeReceived = Convert.ToInt32(response.Substring(0, 3));
                    return responseCodeReceived;
                }
            }
        }

        void AssertNetworkResponse(int responseCode)
        {
            var responseCodeReceived = GetNetworkResponse();
            if (responseCodeReceived != responseCode)
                throw new System.Net.Mail.SmtpException((System.Net.Mail.SmtpStatusCode)responseCodeReceived);
        }

    }

    class InvalidRecipientAddressException : Exception
    {
        public InvalidRecipientAddressException(string s):base("InvalidRecipientAddress: "+s)
        {
        }
    }

    class AttachmentNotFoundException : Exception
    {
    }
}
