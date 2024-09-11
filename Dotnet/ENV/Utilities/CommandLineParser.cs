using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ENV.Utilities
{
    class CommandLineParser
    {
        Action<string> _value;
        Action<string> _externalIdentifier;
        class Start : CharProcessor
        {
            protected CommandLineParser _parent;

            public Start(CommandLineParser parent)
            {
                _parent = parent;
            }
            public void Process(char c, SetCharProcessor setState)
            {
                switch (c)
                {
                    case ' ':
                        break;
                    case '/':
                        setState(new Value(_parent), false);
                        break;
                    case '-':
                        setState(new Value(_parent), false);
                        break;
                    case '\'':
                        setState(new ContainedString('\'', this, s => _parent._value("'" + s + "'")), false);
                        break;
                    case '@':
                        setState(new ReadTillSpaceOrGrashaim(new Start(_parent), _parent._externalIdentifier),
                                 false);
                        break;
                    default:
                        setState(new Value(_parent), true);
                        break;
                }
            }

            public virtual void Finish()
            {

            }
        }


        class Value : CharProcessor
        {
            protected CommandLineParser _parent;
            public Value(CommandLineParser parent)
            {
                _parent = parent;
            }

            StringBuilder _sb = new StringBuilder();


            public void Process(char c, SetCharProcessor setState)
            {
                switch (c)
                {
                    case ' ':
                        setState(new spaceInValue(this), false);
                        break;
                    case '\'':
                        setState(new ContainedString('\'', this, s => _sb.Append("'" + s + "'")), false);
                        break;
                    case '*':
                        setState(new ReadToEnd(delegate (string s)
                                                   {
                                                       _sb.Append('*');
                                                       _sb.Append(s);
                                                       Finish();
                                                   }), false);
                        break;
                    case '\\':
                        setState(new EscapeCharacter(this, delegate (char c2)
                                                               {
                                                                   _sb.Append(c);
                                                                   _sb.Append(c2);
                                                               }), false);
                        break;
                    case '@':
                        if (_sb.Length == 0)
                        {
                            setState(new ReadTillSpaceOrGrashaim(new Start(_parent), _parent._externalIdentifier),
                                 false);
                        }
                        else
                            _sb.Append(c);
                        break;
                    default:
                        _sb.Append(c);
                        break;
                }

            }

            public void Finish()
            {
                _parent._value(_sb.ToString());
            }
            class spaceInValue : CharProcessor
            {
                Value _parent;

                public spaceInValue(Value parent)
                {
                    _parent = parent;
                }

                public void Process(char c, SetCharProcessor setState)
                {
                    switch (c)
                    {
                        case '/':
                            {
                                _parent.Finish();
                                setState(new Value(_parent._parent), false);
                                break;
                            }
                        case '@':
                            if (_parent._sb.ToString().TrimEnd().EndsWith("="))
                            {
                                _parent._sb.Append(' ');
                                setState(_parent, true);
                                break;
                            }
                            else
                            {
                                _parent.Finish();
                                setState(new ReadTillSpaceOrGrashaim(new Start(_parent._parent), _parent._parent._externalIdentifier),
                                         false);
                            }
                            break;
                        default:
                            _parent._sb.Append(' ');
                            setState(_parent, true);
                            break;
                    }
                }

                public void Finish()
                {
                    _parent._sb.Append(' ');
                    _parent.Finish();
                }
            }
        }



        public CommandLineParser(Action<string> value, Action<string> externalIdentifier)
        {
            _externalIdentifier = externalIdentifier;
            _value = value;
        }

        public void Parse(System.IO.TextReader command)
        {
            string line;
            while ((line = command.ReadLine()) != null)
            {
                while (line.TrimEnd(' ').EndsWith("+"))
                {
                    line = line.TrimEnd(' ');
                    line = line.Remove(line.Length - 1);
                    line += command.ReadLine();
                }
                new StringParser().Parse(line, new Start(this));
            }
        }
    }
}
