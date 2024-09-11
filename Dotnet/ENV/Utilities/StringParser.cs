using System;
using System.IO;
using System.Text;

namespace ENV.Utilities
{
    public class StringParser
    {
        public void Parse(string s, CharProcessor processor)
        {
            Parse(new StringReader(s), processor);

        }
        internal static void Debug(object what, string prefix = "")
        {
            return;
#if DEBUG


            if (!(what is string) && !(what is char))
                what = what.GetType().Name;
            System.Diagnostics.Debug.WriteLine(prefix + "  " + what);
#endif
        }

        public void Parse(TextReader tr, CharProcessor processor)
        {
            int code;
            CharProcessor state = processor;
            SetCharProcessor setState = delegate { };
            while ((code = tr.Read()) != -1)
            {
                char c = (char)code;
                Debug(c.ToString());
                setState = delegate (CharProcessor value, bool sendCharToIt)
                {
                    state = value;
                    Debug(value, "charProcessor=");
                    if (sendCharToIt)
                        state.Process(c, setState);
                };
                state.Process(c, setState);

            }
            state.Finish();
        }
    }

    public interface CharProcessor
    {
        void Process(char c, SetCharProcessor setState);
        void Finish();
    }

    public delegate void SetCharProcessor(CharProcessor value, bool sendCharToIt);

    class ContainedString : CharProcessor
    {
        char _stringContainer;
        StringBuilder _sb = new StringBuilder();
        Action<string> _whatToDoWithTheString;
        CharProcessor _nextProcessor;

        public ContainedString(char stringContainer, CharProcessor nextProcessor, Action<string> whatToDoWithTheString)
        {
            _stringContainer = stringContainer;
            _nextProcessor = nextProcessor;
            _whatToDoWithTheString = whatToDoWithTheString;
        }

        public void Finish()
        {
            _whatToDoWithTheString(_sb.ToString());
            _nextProcessor.Finish();
        }
        public void Process(char c, SetCharProcessor setState)
        {
            if (_stringContainer == c)
                setState(new StringThatWasTerminated(this), false);
            else
                _sb.Append(c);
        }

        class StringThatWasTerminated : CharProcessor
        {
            ContainedString _containedString;

            public StringThatWasTerminated(ContainedString containedString)
            {
                _containedString = containedString;
            }
            public void Finish()
            {
                _containedString._whatToDoWithTheString(_containedString._sb.ToString());
                _containedString._nextProcessor.Finish();
            }
            public void Process(char c, SetCharProcessor setState)
            {
                if (_containedString._stringContainer == c)
                {
                    _containedString._sb.Append(c);
                    setState(_containedString, false);
                }
                else
                {
                    _containedString._whatToDoWithTheString(_containedString._sb.ToString());
                    setState(_containedString._nextProcessor, true);
                }
            }

        }
    }
    class EscapeCharacter : CharProcessor
    {
        Action<char> _whatToDoWithNextChar;
        CharProcessor _nextState;

        public EscapeCharacter(CharProcessor nextState, Action<char> whatToDoWithNextChar)
        {
            _nextState = nextState;
            _whatToDoWithNextChar = whatToDoWithNextChar;
        }

        public void Process(char c, SetCharProcessor setState)
        {
            _whatToDoWithNextChar(c);
            setState(_nextState, false);
        }

        public void Finish()
        {
            _nextState.Finish();
        }
    }
    class ReadToEnd : CharProcessor
    {
        Action<string> _whatToDoWithString;
        StringBuilder _sb = new StringBuilder();
        public ReadToEnd(Action<string> whatToDoWithString)
        {
            _whatToDoWithString = whatToDoWithString;
        }

        public void Process(char c, SetCharProcessor setState)
        {
            _sb.Append(c);
        }

        public void Finish()
        {
            _whatToDoWithString(_sb.ToString());
        }
    }
    class ReadUntilChars : CharProcessor
    {
        Action<string> _whatToDoWithString;
        Func<CharProcessor> _nextProcessor;
        char[] _theChar;
        int _position = 0;
        StringBuilder _sb = new StringBuilder();


        public ReadUntilChars(Func<CharProcessor> nextProcessor, char[] theChar, Action<string> whatToDoWithString)
        {
            _nextProcessor = nextProcessor;
            _theChar = theChar;
            _whatToDoWithString = whatToDoWithString;
        }
        public ReadUntilChars(CharProcessor nextProcessor, char theChar, Action<string> whatToDoWithString)
        {
            _nextProcessor = () => nextProcessor;
            _theChar = new[] { theChar };
            _whatToDoWithString = whatToDoWithString;
        }

        public void Process(char c, SetCharProcessor setState)
        {
            if (c == _theChar[_position])
            {
                _position++;
                if (_position == _theChar.Length)
                {
                    if (_sb.Length > 0)
                        _whatToDoWithString(_sb.ToString());
                    setState(_nextProcessor(), false);
                }
            }
            else
            {
                if (_position > 0)
                {
                    for (int i = 0; i < _position; i++)
                    {
                        _sb.Append(_theChar[i]);
                    }
                    _position = 0;
                    Process(c, setState);
                }
                else
                    _sb.Append(c);

            }
        }

        public void Finish()
        {
            if (_sb.Length > 0)
                _whatToDoWithString(_sb.ToString());
        }
    }
    class ReadUntilAnyChars : CharProcessor
    {
        Action<string> _whatToDoWithString;
        CharProcessor _nextProcessor;
        System.Collections.Generic.HashSet<char> _theChar;

        StringBuilder _sb = new StringBuilder();


        public ReadUntilAnyChars(CharProcessor nextProcessor, char[] theChar, Action<string> whatToDoWithString)
        {
            _nextProcessor = nextProcessor;
            _theChar = new System.Collections.Generic.HashSet<char>(theChar);
            _whatToDoWithString = whatToDoWithString;
        }


        public void Process(char c, SetCharProcessor setState)
        {
            if (_theChar.Contains(c))
            {
                _whatToDoWithString(_sb.ToString());
                setState(_nextProcessor, true);
            }
            _sb.Append(c);
        }

        public void Finish()
        {
            if (_sb.Length > 0)
                _whatToDoWithString(_sb.ToString());
        }
    }
    class ReadTillSpaceOrGrashaim : CharProcessor
    {
        Action<string> _whatToDoWithString;
        CharProcessor _nextProcessor;


        StringBuilder _sb = new StringBuilder();



        public ReadTillSpaceOrGrashaim(CharProcessor nextProcessor, Action<string> whatToDoWithString)
        {
            _nextProcessor = nextProcessor;

            _whatToDoWithString = whatToDoWithString;
        }

        public void Process(char c, SetCharProcessor setState)
        {
            if (c == '\"' && _sb.Length == 0)
            {
                setState(new ReadUntilChars(_nextProcessor, '\"', _whatToDoWithString), false);
            }
            else if (c == ' ')
            {
                if (_sb.Length > 0)
                    _whatToDoWithString(_sb.ToString());
                setState(_nextProcessor, false);

            }
            else
            {
                _sb.Append(c);

            }
        }

        public void Finish()
        {
            if (_sb.Length > 0)
                _whatToDoWithString(_sb.ToString());
        }
    }
    public class ReadValueThatMayOrMayNotBeQuoted : CharProcessor
    {
        Action<string> _whatToDoWithString;
        CharProcessor _nextProcessor;
        char[] _theChar;


        public ReadValueThatMayOrMayNotBeQuoted(CharProcessor nextProcessor, char[] theChar, Action<string> whatToDoWithString)
        {
            _nextProcessor = nextProcessor;
            _theChar = theChar;
            _whatToDoWithString = whatToDoWithString;
        }
        public ReadValueThatMayOrMayNotBeQuoted(CharProcessor nextProcessor, char theChar, Action<string> whatToDoWithString)
        {
            _nextProcessor =  nextProcessor;
            _theChar = new[] { theChar };
            _whatToDoWithString = whatToDoWithString;
        }
        public void Finish()
        {

        }

        public void Process(char c, SetCharProcessor setState)
        {
            switch (c)
            {
                case '\'':
                case '\"':
                    setState(new ContainedString(c, _nextProcessor, _whatToDoWithString), false);
                    break;
                default:
                    setState(new ReadUntilAnyChars(_nextProcessor, _theChar, _whatToDoWithString), true);
                    break;
                case ' ':
                case '\r':
                case '\n':
                    break;

            }
        }
    }
    public class EndOfString : CharProcessor
    {
        public void Finish()
        {
        }

        public void Process(char c, SetCharProcessor setState)
        {
            throw new System.InvalidOperationException("recieved char " + c + " at end of string");
        }
    }
   
}