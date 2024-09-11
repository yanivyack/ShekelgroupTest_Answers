using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ENV.Utilities
{
    class VisualFalseClass
    {
        HashSet<char> _hebrewChars;
        public VisualFalseClass(string source, bool alsoOem)
        {


            _hebrewChars = alsoOem ? HebrewTextTools._hebrewCharsWithOem : HebrewTextTools._hebrewChars;
            _state = new Start(this);
            foreach (char c in source)
            {
                if (_hebrewChars.Contains(c))
                    _state.Hebrew(c);
                else if (c == ' ')
                    _state.Space(c);
                else if (HebrewTextTools._englishChars.Contains(c))
                    _state.English(c);
                else if (HebrewTextTools._numberChars.Contains(c))
                    _state.Number(c);
                else if (HebrewTextTools._bracksChars.Contains(c))
                    _state.Brakets(c);
                else if (HebrewTextTools._numericOperators.Contains(c))
                    _state.NumericOperator(c);
                else
                    _state.Other(c);
            }
            _state.Finish(_sb);

        }
        class EnglishCharProcessor : myCharProcesser
        {
            public EnglishCharProcessor(VisualFalseClass root)
                : base(root)
            {
            }
            public override void Hebrew(char c)
            {
                var x = new HebrewCharProcessor(_root);
                x.AfterFinish = this;
                _root._state = x;
                x.Hebrew(c);
            }
            public override void Number(char c)
            {
                var x = new NumberAfterEnglish(this);
                x.AfterFinish = AfterFinish;
                x.BeforeFinish = this;
                AfterFinish = null;
                _root._state = x;
                x.Number(c);
            }

            class NumberAfterEnglish : myCharProcesser
            {
                EnglishCharProcessor _parent;
                public NumberAfterEnglish(EnglishCharProcessor parent)
                    : base(parent._root)
                {
                    _parent = parent;
                }
                public override void English(char c)
                {
                    _parent._chars.AddRange(_chars);
                    _root._state = _parent;
                    _root._state.AfterFinish = AfterFinish;
                    _root._state.English(c);

                }
         

                public override void Hebrew(char c)
                {
                    var x = new HebrewCharProcessor(_root);
                    x.Hebrew(c);
                    _chars = NumberReverser.Reverse(_chars,false);

                    x.BeforeFinish = this;
                    BeforeFinish.AfterFinish = AfterFinish;
                    x.AfterFinish = BeforeFinish;
                    BeforeFinish = null;
                    AfterFinish = null;

                    _root._state = x;
                }
            }
        }
        class HebrewCharProcessor : myCharProcesser
        {
            public HebrewCharProcessor(VisualFalseClass parent)
                : base(parent)
            {
            }
            public override void Space(char c)
            {
                SpaceAsNextChar(c);
            }
            public override void English(char c)
            {
                var x = new EnglishCharProcessor(_root);
                x.AfterFinish = this;
                _root._state = x;
                x.English(c);
            }

            public override void NumericOperator(char c)
            {
                SwitchToNumber().NumericOperator(c);

            }
            NumberAfterHebrew SwitchToNumber()
            {
                var x = new NumberAfterHebrew(this);
                x.AfterFinish = this;
                _root._state = x;
                return x;
            }

            public override void Number(char c)
            {

                SwitchToNumber().Number(c);
            }
            public override void Other(char c)
            {
                SwitchToNumber().Other(c);
            }
            class NumberAfterHebrew : myCharProcesser
            {
                HebrewCharProcessor _parent;
                public NumberAfterHebrew(HebrewCharProcessor parent)
                    : base(parent._root)
                {
                    _parent = parent;
                }
                public override void Space(char c)
                {
                    SpaceAsNextChar(c);
                }

                public override void Hebrew(char c)
                {
                    _parent._chars.AddRange(NumberReverser.Reverse(_chars,true));
                    _root._state = _parent;
                    _root._state.Hebrew(c);
                }
                public override void English(char c)
                {
                    var x = new EnglishCharProcessor(_root);
                    x.English(c);
                    x.AfterFinish = AfterFinish;
                    x.BeforeFinish = this;
                    AfterFinish = null;
                    _root._state = x;
                }
            }
            public override void Brakets(char c)
            {
                var x = new NumberAfterHebrew(this);
                x.Brakets(c);
                x.AfterFinish = this;
                _root._state = x;
            }

        }

        class Start : myCharProcesser
        {
            public Start(VisualFalseClass root)
                : base(root)
            {
            }
            public override void English(char c)
            {
                var x = new EnglishCharProcessor(_root);
                x.English(c);
                _root._state = x;
            }
            public override void Hebrew(char c)
            {
                var x = new HebrewCharProcessor(_root);
                x.Hebrew(c);
                _root._state = x;
            }
            public override void Number(char c)
            {
                var x = new NumberCharProcessor(_root);
                x.Number(c);
                _root._state = x;
            }
            public override void NumericOperator(char c)
            {
                Number(c);
            }
            public override void Brakets(char c)
            {
                var x = new BraketAfterStart(_root);
                _root._state = x;
                x.Brakets(c);
            }
            class BraketAfterStart : myCharProcesser
            {
                public BraketAfterStart(VisualFalseClass root)
                    : base(root)
                {
                }
                public override void Hebrew(char c)
                {
                    var x = new HebrewCharProcessor(_root);
                    x.Hebrew(c);
                    x.AfterFinish = this;
                    _root._state = x;
                }
                public override void Number(char c)
                {
                    _root._state = new NumberAfterBraketsAfterHebrew(this);
                    _root._state.Number(c);
                    _root._state.BeforeFinish = this;
                    _root._state.AfterFinish = AfterFinish;
                    AfterFinish = null;
                    _flip = false;
                }
                class NumberAfterBraketsAfterHebrew : myCharProcesser
                {
                    BraketAfterStart _parent;
                    public NumberAfterBraketsAfterHebrew(BraketAfterStart parent)
                        : base(parent._root)
                    {
                        _parent = parent;
                    }
                    public override void Hebrew(char c)
                    {
                        _root._state = new HebrewCharProcessor(_root);
                        _root._state.BeforeFinish = this;
                        BeforeFinish.BeforeFinish = AfterFinish;
                        AfterFinish = null;
                        _parent._flip = true;
                        _chars = NumberReverser.Reverse(_chars,true);
                        base.Hebrew(c);
                    }
                }
                public override void English(char c)
                {
                    var x = new EnglishCharProcessor(_root);
                    _root._state = x;
                    x.BeforeFinish = this;
                    x.AfterFinish = AfterFinish;
                    AfterFinish = null;
                    _flip = false;
                    x.English(c);
                }
                bool _flip = false;
                protected override void DoFinish(StringBuilder sb)
                {
                    if (_flip)
                        for (int i = 0; i < _chars.Count; i++)
                        {
                            _chars[i] = HebrewTextTools.ReverseBracks(_chars[i]);
                        }
                    base.DoFinish(sb);
                }
            }
            public override void Other(char c)
            {
                SwitchToEnglish().Other(c);
            }
            public override void Space(char c)
            {
                SwitchToEnglish().Space(c);
            }
            EnglishCharProcessor SwitchToEnglish()
            {
                var x = new EnglishCharProcessor(_root);
                _root._state = x;
                return x;
            }
        }
        class NumberCharProcessor : myCharProcesser
        {
            public NumberCharProcessor(VisualFalseClass root)
                : base(root)
            {
            }
            public override void Space(char c)
            {
                SpaceAsNextChar(c);
            }
            public override void Hebrew(char c)
            {
                var x = new HebrewCharProcessor(_root);
                x.Hebrew(c);
                x.BeforeFinish = this;
                _chars = NumberReverser.Reverse(_chars,true);
                _root._state = x;

            }
            public override void English(char c)
            {
                var x = new EnglishCharProcessor(_root);
                x.English(c);
                x.BeforeFinish = this;
                _root._state = x;

            }

            public override void Brakets(char c)
            {
                var x = new BraketsAfterNumber(this);
                x.Brakets(c);
                x.BeforeFinish = this;
                _root._state = x;

            }
            class BraketsAfterNumber : myCharProcesser
            {
                NumberCharProcessor _parent;
                public BraketsAfterNumber(NumberCharProcessor parent)
                    : base(parent._root)
                {

                    _parent = parent;
                }
                public override void Hebrew(char c)
                {
                    var x = new HebrewCharProcessor(_root);
                    x.Hebrew(c);
                    BeforeFinish.BeforeFinish = AfterFinish;
                    AfterFinish = null;
                    x.BeforeFinish = this;
                    _root._state = x;
                    _parent._chars.Reverse();
                }
                public override void English(char c)
                {
                    var x = new EnglishCharProcessor(_root);
                    x.English(c);
                    x.BeforeFinish = this;
                    _root._state = x;
                }
            }

            
        }
        StringBuilder _sb = new StringBuilder();

        public override string ToString()
        {
            return _sb.ToString();
        }
        myCharProcesser _state;
        [DebuggerTypeProxy(typeof(DebuggerForMe))]
        class myCharProcesser
        {
            protected internal VisualFalseClass _root;
            public override string ToString()
            {
                return "\"" + new string(_chars.ToArray()) + "\"";
            }
            public myCharProcesser(VisualFalseClass root)
            {
                _root = root;
            }
            class DebuggerForMe
            {
                myCharProcesser _parent;

                public DebuggerForMe(myCharProcesser parent)
                {
                    _parent = parent;
                }
                public myCharProcesser A { get { return _parent.BeforeFinish; } }
                public myCharProcesser B { get { return _parent.AfterFinish; } }
            }

            protected internal List<char> _chars = new List<char>();
            public virtual void Default(char c)
            {
                _chars.Add(c);
            }

            public virtual void Hebrew(char c)
            {
                Default(c);
            }

            public virtual void English(char c)
            {
                Default(c);
            }

            public virtual void Number(char c)
            {
                Default(c);
            }

            public virtual void Space(char c)
            {
                Default(c);
            }
            protected void SpaceAsNextChar(char c)
            {
                _root._state = new SpaceAsNextCharClass(this);
                _root._state.BeforeFinish = this;
                _root._state.Space(c);
            }

            class SpaceAsNextCharClass : myCharProcesser
            {
                myCharProcesser _parent;

                public SpaceAsNextCharClass(myCharProcesser parent)
                    : base(parent._root)
                {
                    _parent = parent;
                }
                public override void Hebrew(char c)
                {
                    _root._state = _parent;
                    foreach (var c1 in _chars)
                    {
                        _root._state.Hebrew(c1);
                    }
                    _root._state.Hebrew(c);
                }
                public override void English(char c)
                {
                    _root._state = _parent;
                    foreach (var c1 in _chars)
                    {
                        _root._state.English(c1);
                    }
                    _root._state.English(c);
                }
                public override void Number(char c)
                {
                    _root._state = _parent;
                    foreach (var c1 in _chars)
                    {
                        _root._state.Number(c1);
                    }
                    _root._state.Number(c);
                }
                public override void Brakets(char c)
                {
                    _root._state = _parent;
                    foreach (var c1 in _chars)
                    {
                        _root._state.Brakets(c1);
                    }
                    _root._state.Brakets(c);
                }
                public override void Other(char c)
                {
                    _root._state = _parent;
                    foreach (var c1 in _chars)
                    {
                        _root._state.Other(c1);
                    }
                    _root._state.Other(c);
                }
                public override void Space(char c)
                {
                    Default(c);
                }
            }

            public virtual void Brakets(char c)
            {
                Default(c);
            }

            public virtual void Other(char c)
            {
                Default(c);
            }

            public virtual void NumericOperator(char c)
            {
                Default(c);
            }

            public myCharProcesser BeforeFinish, AfterFinish;
            public void Finish(StringBuilder sb)
            {
                if (BeforeFinish != null)
                {
                    BeforeFinish.Finish(sb);
                }
                DoFinish(sb);
                if (AfterFinish != null)
                {
                    AfterFinish.Finish(sb);
                }
            }
            protected virtual void DoFinish(StringBuilder sb)
            {
                sb.Append(new string(_chars.ToArray()));
            }
        }
    }

    internal class NumberReverser
    {
        public List<char> Result = new List<char>();
        int _position;
        CharProcessor _state;
        bool _reverseBraks;
        public static List<char> Reverse(List<char> chars, bool reverseBraks)
        {
            return new NumberReverser(chars, reverseBraks).Result;
        }
        NumberReverser(List<char> chars,bool reverseBraks)
        {
            _reverseBraks = reverseBraks;
            _state = new Number(this);
            foreach (var c in chars)
            {
                if (HebrewTextTools._numberChars.Contains(c))
                    _state.Digit(c);
                else if (HebrewTextTools._numericOperators.Contains(c))
                    _state.Operator(c);
                else
                    _state.Other(c);
                            
            }
            _state.Finish();
        }
        interface CharProcessor
        {
            void Digit(char c);
            void Operator(char c);
            void Finish();
            void Other(char c);
        }
        class OperatorProc : CharProcessor
        {
            NumberReverser _parent;

            public OperatorProc(NumberReverser parent)
            {
                _parent = parent;
            }

            public void Digit(char c)
            {
                _parent._state = new Number(_parent);
                _parent._state.Digit(c);
            }

            public void Operator(char c)
            {
                _parent.Add(c);
            }

            public void Finish()
            {

            }

            public void Other(char c)
            {
                _parent.Add(c);
            }
        }

        void Add(char c)
        {
            if (_reverseBraks)
                c = HebrewTextTools.ReverseBracks(c);
            Result.Add(c);
            _position = Result.Count;
        }

        class  Number : CharProcessor
        {
            NumberReverser _parent;
            public Number(NumberReverser parent)
            {
                _parent = parent;
            }

            public void Digit(char c)
            {
                _parent.Insert(c);
            }

            public void Operator(char c)
            {
                _parent._state = new OperatorAfterNumber(_parent, c);
            }
            class OperatorAfterNumber : CharProcessor
            {
                NumberReverser _parent;
                char _operator;

                public OperatorAfterNumber(NumberReverser parent, char @operator)
                {
                    _parent = parent;
                    _operator = @operator;
                }


                public void Digit(char c)
                {
                    _parent.Insert(_operator);
                    _parent._state = new Number(_parent);
                    _parent._state.Digit(c);
                }

                public void Operator(char c)
                {
                    Finish();
                    _parent._state.Operator(c);
                }

                public void Finish()
                {
                    _parent._state = new OperatorProc(_parent);
                    _parent._state.Operator(_operator);
                }

                public void Other(char c)
                {
                    Finish();
                    _parent._state.Other(c);
                }
            }

            public void Finish()
            {

            }

            public void Other(char c)
            {
                _parent._state = new OperatorProc(_parent);
                _parent._state.Other(c);
            }
        }

        void Insert(char c)
        {
            Result.Insert(_position, c);
        }
    }
}
