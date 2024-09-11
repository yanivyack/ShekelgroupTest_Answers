using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Firefly.Box;

namespace ENV.Utilities
{
    interface IKBPutParserClient
    {
        void Keys(string keys);
        void Command(string commandName);
        void Text(string text);
    }
    class KBPutParser
    {
        IKBPutParserClient _client;
        System.IO.StringReader _reader;

        public KBPutParser(string text, IKBPutParserClient client)
        {
            _reader = new System.IO.StringReader(text);
            _client = client;
        }

        public void Parse()
        {
            int i;
            StringBuilder sb = new StringBuilder();
            Action endStringReading = delegate
            {
                if (sb.Length != 0)
                    _client.Text(sb.ToString());
                sb = new StringBuilder();
            };
            while (true)
            {
                switch (i = _reader.Peek())
                {
                    case '[':
                        endStringReading();
                        ReadCommand();
                        break;
                    case '<':

                        endStringReading();
                        ReadKeyboard();
                        break;
                    case -1:
                        endStringReading();
                        _reader.Dispose();
                        return;
                    default:
                        sb.Append((char)_reader.Read());
                        break;
                }
            }
        }
        void ReadCommand()
        {
            int i;
            StringBuilder sb = new StringBuilder();
            _reader.Read();
            while (true)
            {
                switch (i = _reader.Peek())
                {
                    case -1:
                        _client.Text("[" + sb);
                        return;

                    case ']':
                        _reader.Read();
                        if (sb.Length > 0)
                        {
                            _client.Command(sb.ToString());
                        }
                        return;
                    default:
                        sb.Append((char)_reader.Read());
                        break;
                }
            }
        }
        void ReadKeyboard()
        {
            int i;
            StringBuilder sb = new StringBuilder();
            _reader.Read();
            while (true)
            {
                switch (i = _reader.Peek())
                {
                    case -1:
                        _client.Text("<" + sb);
                        return;
                    case '>':
                        _reader.Read();
                        if (sb.Length > 0)
                        {
                            _client.Keys(sb.ToString());
                        }
                        return;
                    default:
                        sb.Append((char)_reader.Read());
                        break;
                }
            }
        }
    }
    class KBPutParserClient : IKBPutParserClient
    {
        public static System.Windows.Forms.Keys FindKey(string p)
        {
            System.Windows.Forms.Keys result = System.Windows.Forms.Keys.None;
            if (string.IsNullOrEmpty(p))
                return result;
            foreach (var s in p.Split('+'))
            {
                p = s.ToUpper(CultureInfo.InvariantCulture);
                if ("1234567890".Contains(p)) p = "D" + p;
                switch (p)
                {
                    case "CTRL":
                        result|= System.Windows.Forms.Keys.Control;
                        break;
                    case "ENTER":
                        result |= System.Windows.Forms.Keys.Enter;
                        break;
                    case "ESC":
                        result |= System.Windows.Forms.Keys.Escape;
                        break;
                    case "RGHT":
                        result |= System.Windows.Forms.Keys.Right;
                        break;
                    case "PGDN":
                        result |= System.Windows.Forms.Keys.PageDown;
                        break;
                    case "PGUP":
                        result |= System.Windows.Forms.Keys.PageUp;
                        break;
                    case "DEL":
                        result |= System.Windows.Forms.Keys.Delete;
                        break;
                    case "INS":
                        result |= System.Windows.Forms.Keys.Insert;
                        break;
                    case "PLUS":
                        result |= System.Windows.Forms.Keys.Add;
                        break;
                    case "MINUS":
                        result |= System.Windows.Forms.Keys.Subtract;
                        break;
                    default:
                        bool done = false;
                        foreach (Keys k in Enum.GetValues(typeof(Keys)))
                        {
                            if (k.ToString().ToUpper(CultureInfo.InvariantCulture) == p)
                            {
                                result |= k;
                                done = true;
                                break;
                            }

                        }
                        if (!done)
                            throw new Exception("Couldn't find Key " + p);
                        break;
                }
                
            }
            return result;
            
        }

        static Dictionary<string, Command> _commandsByName = new Dictionary<string, Command>();
        static KBPutParserClient()
        {
            LoadFromType(typeof(Firefly.Box.Command));
            LoadFromType(typeof(ENV.Commands));
        }
        static void LoadFromType(Type t)
        {
            foreach (var info in t.GetFields())
            {
                if (info.IsStatic)
                {
                    var c = info.GetValue(null) as Command;
                    if (c != null && !string.IsNullOrEmpty(c.Name))
                    {
                        if (!_commandsByName.ContainsKey(c.Name))
                            _commandsByName.Add(c.Name, c);
                    }
                }
            }
        }


        public void Keys(string keys)
        {
            try
            {
                ENV.UserMethods.Instance.KBPut(FindKey(keys));
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "Kbput " + keys);
            }
        }

        public void Command(string commandName)
        {
            if (_commandsByName.ContainsKey(commandName))
                ENV.UserMethods.Instance.KBPut(_commandsByName[commandName]);
            else
                Text("["+commandName+"]");
        }

        public void Text(string text)
        {
            ENV.UserMethods.Instance.KBPut(Firefly.Box.Command.InputText(text));
        }
    }
}
