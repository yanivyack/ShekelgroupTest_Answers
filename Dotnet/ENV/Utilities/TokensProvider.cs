using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ENV.Utilities;
using Firefly.Box;

namespace ENV.Utilities
{
    interface TokensProvider
    {
        void ProvideTokens(TokensConsumer to);
    }

    interface TokensConsumer
    {
        void StringValue(string value, string descriptor);
        void DecimalValue(decimal value);
        void Identifier(string name);
        void Separator(char c);
    }

    class StringTokensProvider : TokensProvider
    {

        System.IO.TextReader _reader;
        string _expression;
        public StringTokensProvider(string expression)
        {
            _expression = expression;
            _reader = new System.IO.StringReader(expression);
        }
        static Func<char, bool> CreateList(string s)
        {
            return c => s.Contains(c.ToString());
        }

        static Func<char, bool> _numericChars,
                                _operatorChars,
                                _separatorChars,
                                _spaceChars;




        class NumericProcessor : CharProcessorImpl
        {
            public NumericProcessor(TokensConsumer parent)
                : base(parent)
            {
            }
            public override void Process(char c, SetCharProcessor setState)
            {
                if (_numericChars(c))
                    _sb.Append(c);
                else if (_operatorChars(c) || _spaceChars(c) || _separatorChars(c))
                {
                    base.Process(c, setState);
                }
                else
                    setState(new OtherChars(_to,_sb), true);
                
            }
            public override void Finish()
            {
                var c = Number.DecimalSeparator.Value;
                
                
                if (c!='.')
                {
                    Number.DecimalSeparator.SetContextValue('.');
                }
                try
                {
                    _to.DecimalValue(Number.Parse(_sb.ToString()));
                }
                finally {
                    if (c != '.')
                        Number.DecimalSeparator.SetContextValue(c);
                }
            }

        }
        class OperatorChar : CharProcessorImpl
        {
            public OperatorChar(TokensConsumer parent)
                : base(parent)
            {

            }
            public override void Process(char c, SetCharProcessor setState)
            {
                switch (c)
                {
                    case '<':
                    case '>':
                        setState(new OperatorWithTwoChars(_to, c), false);
                        break;
                    default:
                        if (_operatorChars(c)||c=='.'||c=='[')
                        {
                            _to.Identifier(c.ToString());
                            if (c == '[')
                            {
                                _to.Identifier("this");
                                _to.Separator(c);
                            }
                            setState(new CharProcessorImpl(_to), false);
                        }
                        break;
                }
            }

        }
        class OperatorWithTwoChars : CharProcessorImpl
        {
            char _firstChar;

            public OperatorWithTwoChars(TokensConsumer parent, char firstChar)
                : base(parent)
            {
                _firstChar = firstChar;
            }
            public override void Process(char c, SetCharProcessor setState)
            {
                if (c == '=' || _firstChar == '<' && c == '>')
                {
                    _to.Identifier(new string(new[] { _firstChar, c }));
                    setState(new CharProcessorImpl(_to), false);
                }
                else
                {
                    base.Process(c, setState);
                }
            }

            public override void Finish()
            {
                _to.Identifier(_firstChar.ToString());
            }
        }


        class SpaceChar : CharProcessorImpl
        {
            public SpaceChar(TokensConsumer parent)
                : base(parent)
            {
            }
            public override void Process(char c, SetCharProcessor setState)
            {
                if (_spaceChars(c))
                {
                }
                else
                    base.Process(c, setState);
            }

        }
        class OtherChars : CharProcessorImpl
        {
            public OtherChars(TokensConsumer parent,StringBuilder sb = null)
                : base(parent)
            {
                if (sb != null)
                    _sb = sb;
            }
            public override void Process(char c, SetCharProcessor setState)
            {
                if (_numericChars(c))
                    _sb.Append(c);
                else
                    base.Process(c, setState);
            }
            public override void ProcessUnProcessedChar(char c, SetCharProcessor setState)
            {
                _sb.Append(c);
            }
            public override void Finish()
            {
                _to.Identifier(_sb.ToString());
            }
        }

        class CharProcessorImpl : CharProcessor
        {
            protected TokensConsumer _to;
            protected StringBuilder _sb = new StringBuilder();

            public CharProcessorImpl(TokensConsumer to)
            {
                _to = to;
            }

            public virtual void Process(char c, SetCharProcessor setState)
            {
                if ((c == '.' || c=='[') && _sb.Length == 0)
                {
                    Finish();
                    setState(new OperatorChar(_to), true);
                }
                else if (_numericChars(c))
                {
                    Finish();
                    setState(new NumericProcessor(_to), true);
                }
                else if (_operatorChars(c))
                {
                    Finish();
                    setState(new OperatorChar(_to), true);
                }
                else if (_separatorChars(c))
                {
                    Finish();
                    _to.Separator(c);
                    setState(new CharProcessorImpl(_to), false);
                }
                else if (_spaceChars(c))
                {
                    Finish();
                    setState(new SpaceChar(_to), true);
                }
                else if ('\'' == c)
                {
                    Finish();
                    StringQualifier s = new StringQualifier(_to);
                    setState(new ContainedString('\'', s, s.SetString), false);
                }
                else
                {
                    ProcessUnProcessedChar(c, setState);
                }
            }
            public virtual void ProcessUnProcessedChar(char c, SetCharProcessor setState)
            {
                Finish();
                setState(new OtherChars(_to), true);
            }

            public virtual void Finish()
            {
            }
        }
        class StringQualifier : CharProcessorImpl
        {
            string _theString = "";
            public StringQualifier(TokensConsumer to)
                : base(to)
            {
            }
            public void SetString(string s)
            {
                _theString = s;
            }
            public override void ProcessUnProcessedChar(char c, SetCharProcessor setState)
            {
                _sb.Append(c);
            }
            public override void Finish()
            {
                _to.StringValue(_theString, _sb.ToString());
            }
        }

        static StringTokensProvider()
        {
            _numericChars = CreateList("1234567890.");
            _operatorChars = CreateList("-+*/^=&<>");
            _separatorChars = CreateList(",()[]");
            _spaceChars = CreateList(" \r\n\t");
        }


        public void ProvideTokens(TokensConsumer to)
        {
            new StringParser().Parse(_expression, new CharProcessorImpl(to));
        }
    }
}