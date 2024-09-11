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
    

    class ImapMailClient : MailClient
    {
        
        static Encoding _mailEncoding { get { return LocalizationInfo.Current.OuterEncoding; } }

        protected string ReadResponse()
        {
            var responseLine = "*";
            var _lastResponse = "";
            while (responseLine.StartsWith("*") && !responseLine.Contains("OK") || _socket.Available > 0)
            {

                responseLine = ReadLineFromSocket();
                _lastResponse += responseLine;
            }
            return _lastResponse;
        }

        
        string  AssertServerOK()
        {
            var l = ReadResponse();
                if (!l.ToString().Contains("OK"))
                    throw new Exception(string.Format("IMAP Error - '{0}'", l.ToString()));
                return l.ToString();
            
            
            
        }

      

        int _commandCounter;

        void SendCommand(string commandText)
        {
            SendLine(string.Format("{0} {1}", ++_commandCounter, commandText));
        }
        string SendAndAssertOk(string commandText)
        {
            SendCommand(commandText);
            return AssertServerOK();
        }

        public override void Connect(string host, string username, string password)
        {
            OpenSocket(host, 143);

            AssertServerOK();
            SendCommand(string.Format("LOGIN {0} {1}", username, password));
            AssertServerOK();

            var m = Regex.Match(SendAndAssertOk("SELECT \"INBOX\""), @"(\d+)\sEXISTS");
            if (m.Success)
                _totalMessages = int.Parse(m.Groups[1].Value);
        }

        protected override MailClient.Message GetMessageFromServer(int index)
        {
            return new Message(index, this);
        }

        public override void Disconnect()
        {
            AssertSocketConnected();

            SendCommand("LOGOUT");
            AssertServerOK();
            CloseSocket();
            
            
            
            _commandCounter = 0;
        }





        public class Message : MailClient.Message
        {
            ImapMailClient _parent;

            public Message(int index, ImapMailClient parent)
                : base(index)
            {
                _parent = parent;
            }

            bool _headerFetched;
            protected override void FetchHeader()
            {
                if (_headerFetched) return;
                var s =
                    ReadMultiline(
                        string.Format("FETCH {0} (RFC822.SIZE FLAGS UID INTERNALDATE BODY.PEEK[HEADER])", _index + 1));
                ParseHeader(new System.IO.StringReader(s));
                _headerFetched = true;
            }
            string ReadMultiline(string command)
            {
                _parent.SendCommand(command);
                var r = _parent.ReadLineFromSocket();
                var rs = r.Split(' ');
                if (!(rs[0] == "*" && rs[2].ToUpper() == "FETCH"))
                {
                    throw new Exception("Imap error: " + r);
                }
                var sw = new StringWriter();
                sw.WriteLine(r);
                bool done = false;
                while (!done)
                {
                    r = _parent.ReadLineFromSocket();
                    if (r == ")")
                        done = true;
                    else
                    {
                        sw.WriteLine(r);
                    }
                }
                r = _parent.ReadLineFromSocket();
                if (!r.ToUpper().Contains("OK FETCH COMPLETED"))
                    throw new Exception("Imap Error: " + r);
                return sw.ToString();

            }

            protected override string GetMessageId(TextReader headerReader)
            {
                var idLine = headerReader.ReadLine();
                return Regex.Match(idLine, @"UID (\d+)").Groups[1].Value;
            }

            protected override string GetBody()
            {
                return ReadMultiline(string.Format("FETCH {0} (RFC822.SIZE FLAGS UID INTERNALDATE BODY.PEEK[])", _index + 1));


            }
        }
            

        

      

        public override void ClearMailbox()
        {
            Delete("1:*");
        }

        public override void DeleteMessage(int index)
        {
            Delete((index + 1).ToString());
        }

        void Delete(string s)
        {
            SendCommand(string.Format(@"STORE {0} +FLAGS (\Deleted)", s));
            AssertServerOK();
            SendCommand("EXPUNGE");
            AssertServerOK();
        }
    }
}
