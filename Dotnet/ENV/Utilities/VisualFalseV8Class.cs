using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ENV.Utilities
{
    class VisualFalseV8Class
    {
        HashSet<char> _hebrewChars;
        bool _v8 = true;
        public VisualFalseV8Class(string source, bool alsoOem)
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
            public EnglishCharProcessor(VisualFalseV8Class root)
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
                x.AfterFinish = this;
                _root._state = x;
                x.Number(c);
            }
            public override void Space(char c)
            {
                SpaceAsPreviousChar(c);
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

                    _root._state.English(c);

                }
                public override void Space(char c)
                {
                    SpaceAsPreviousChar(c);

                }


                public override void Hebrew(char c)
                {
                    var x = new HebrewCharProcessor(_root);
                    _chars = NumberReverser.Reverse(_chars, false);
                    _chars.Reverse();
                    x.Hebrew(c);
                    x.BeforeFinish = this;
                    x.AfterFinish = AfterFinish;
                    AfterFinish = null;
                    _root._state = x;
                }
                protected override void DoFinish(StringBuilder sb)
                {
                    _chars.Reverse();
                    base.DoFinish(sb);
                }
            }
            protected override void DoFinish(StringBuilder sb)
            {
                if (_root._v8)
                    _chars.Reverse();
                base.DoFinish(sb);
            }
        }
        class HebrewCharProcessor : myCharProcesser
        {
            public HebrewCharProcessor(VisualFalseV8Class parent)
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

                    _parent._chars.AddRange(NumberReverser.Reverse(_chars, true));
                    _root._state = _parent;
                    _root._state.Hebrew(c);
                }
                public override void English(char c)
                {
                    var x = new EnglishCharProcessor(_root);

                    x.English(c);
                    x.AfterFinish = this;


                    _root._state = x;
                }
                protected override void DoFinish(StringBuilder sb)
                {
                    _chars.Reverse();
                    base.DoFinish(sb);
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
            public Start(VisualFalseV8Class root)
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
                SwitchToEnglish().Brakets(c);
                return;
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
            public NumberCharProcessor(VisualFalseV8Class root)
                : base(root)
            {
            }
            public override void Space(char c)
            {
                SpaceAsPreviousChar(c);
            }
            public override void Hebrew(char c)
            {
                var x = new HebrewCharProcessor(_root);
                x.Hebrew(c);
                x.BeforeFinish = this;
                _chars = NumberReverser.Reverse(_chars, false);
                _chars.Reverse();
                _root._state = x;

            }
            public override void English(char c)
            {
                var x = new EnglishCharProcessor(_root);
                x.English(c);
                x.AfterFinish = this;
                _root._state = x;

            }

            protected override void DoFinish(StringBuilder sb)
            {
                _chars.Reverse();// = NumberReverser.Reverse(_chars, true);
                base.DoFinish(sb);
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
            protected internal VisualFalseV8Class _root;
            public override string ToString()
            {
                return "\"" + new string(_chars.ToArray()) + "\"";
            }
            public myCharProcesser(VisualFalseV8Class root)
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
            protected void SpaceAsPreviousChar(char c)
            {
                _root._state = new SpaceAsPreviousCharClass(this);
                _root._state.BeforeFinish = this;
                _root._state.Space(c);
            }

            class SpaceAsPreviousCharClass : myCharProcesser
            {
                myCharProcesser _parent;

                public SpaceAsPreviousCharClass(myCharProcesser parent)
                    : base(parent._root)
                {
                    _parent = parent;
                }
                public override void Hebrew(char c)
                {
                    _root._state = _parent;
                    _parent._chars.AddRange(_chars);
                    _root._state.Hebrew(c);
                }
                public override void English(char c)
                {
                    _root._state = _parent;
                    _parent._chars.AddRange(_chars);
                    _root._state.English(c);
                }
                public override void Number(char c)
                {
                    _root._state = _parent;
                    _parent._chars.AddRange(_chars);
                    _root._state.Number(c);
                }
                public override void Brakets(char c)
                {
                    _root._state = _parent;
                    _parent._chars.AddRange(_chars);
                    _root._state.Brakets(c);
                }
                public override void Other(char c)
                {
                    _root._state = _parent;
                    _parent._chars.AddRange(_chars);
                    _root._state.Other(c);
                }
                public override void NumericOperator(char c)
                {
                    _root._state = _parent;
                    _parent._chars.AddRange(_chars);
                    _root._state.NumericOperator(c);
                }
                public override void Space(char c)
                {
                    Default(c);
                }
               
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


}
