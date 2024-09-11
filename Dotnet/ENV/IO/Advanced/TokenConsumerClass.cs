using System;
using System.Collections.Generic;

namespace ENV.IO.Advanced
{
    class TokenConsumerClass : TokenConsumer
    {
        TokenConsumer _currentConsumer;
        string _valuePrefix;
        Func<string, string> _getContentOfInclude;
        int _repeatIndex = 0;

        class rootConsumer : TokenConsumer
        {
            protected TokenSetBuilder _set;
            TokenConsumerClass _parent;

            public rootConsumer(TokenSetBuilder set, TokenConsumerClass parent)
            {
                _set = set;
                _parent = parent;
            }
            public rootConsumer(TokenConsumerClass parent)
            {
                _set = new TokenSetBuilder();
                _parent = parent;
            }


            public virtual void Token(string token)
            {
                switch (token)
                {
                    case "REPEAT":
                        _parent._currentConsumer = new RepeatTokenReader(this, _parent._repeatIndex++);
                        break;
                    case "INCLUDE":
                        _parent._currentConsumer = new IncludeTokenReader(this);
                        break;
                    case "ENDIF":
                    case "ENDREPEAT":
                    case "ENDINCLUDE":
                        throw new InvalidOperationException("Invalid Token at this location " + token);
                    default:
                        if (token.StartsWith(_parent._valuePrefix))
                            _set.Add(new ValueToken(token.Substring(_parent._valuePrefix.Length)));
                        else if (token.StartsWith("IF" + _parent._valuePrefix))
                            _parent._currentConsumer = new IfTokenReader(this,
                                                                         token.Substring(_parent._valuePrefix.Length + 2));
                        else _set.Add(new InvalidToken(token));

                        break;
                }

            }

            public virtual void Text(string text)
            {

                _set.Add(new TextToken(text));
            }

            public virtual void Done()
            {

            }

            class RepeatTokenReader : rootConsumer
            {
                rootConsumer _prev;
                int _index;
                public RepeatTokenReader(rootConsumer prev, int index)
                    : base(prev._parent)
                {
                    _index = index;
                    _prev = prev;
                }
                public override void Token(string token)
                {
                    switch (token)
                    {
                        case "ENDREPEAT":
                            var key = "Items";
                            if (_index > 0)
                                key += _index.ToString();
                            _prev._set.Add(new RepeatToken(_set.Build(), key));
                            _parent._currentConsumer = _prev;
                            break;
                        default:
                            base.Token(token);
                            break;
                    }

                }
                public override void Done()
                {
                    throw new InvalidOperationException("Missing ENDREPEAT token ");
                }
                class RepeatToken : IToken
                {
                    IToken _set;
                    bool merged = false;
                    string _key;
                    List<IToken> _result = new List<IToken>();
                    public RepeatToken(IToken set, string key)
                    {
                        _key = key;
                        _set = set;
                    }

                    public string WriteStructure()
                    {
                        return string.Format("(Repeat {0})", _set.WriteStructure());
                    }
                    public void AddMembers(MemberCollection mem)
                    {
                        var my = mem.CreateChild();
                        _set.AddMembers(my);
                        mem.Add(_key, my.ToTypescript() + "[]");
                    }

                    public void WriteTo(ITemplateTokensWriter writer, string prefix, string sufix, string tagPrefix, bool withinRepeat)
                    {
                        writer.StartSection();
                        writer.StartRepeat(_key);
                        if (merged)
                        {

                            foreach (var tokenSet in _result)
                            {
                                writer.StartRepeatRow();
                                tokenSet.WriteTo(writer, prefix, sufix, tagPrefix, true);
                                writer.EndRepeatRow();

                            }

                        }
                        else
                        {
                            if (!withinRepeat)
                            {
                                writer.StartRepeatRow();
                                _set.WriteTo(writer, prefix, sufix, tagPrefix, withinRepeat);
                                writer.EndRepeatRow();
                            }
                        }
                        writer.EndRepeat();
                        writer.EndSection();
                    }

                    public bool IsChildAffectedBy(TemplateValues mSet)
                    {
                        return _set.IsChildAffectedBy(mSet);
                    }

                    IToken _lastOne = null;

                    public void MergeTokens(TemplateValues mSet)
                    {
                        if (_set.IsAffectedBy(mSet) || _lastOne == null && _set.IsChildAffectedBy(mSet))
                        {
                            _lastOne = _set.Duplicate();
                            _result.Add(_lastOne);
                            merged = true;
                        }

                        if (_lastOne != null)
                            _lastOne.MergeTokens(mSet);
                    }

                    public bool IsAffectedBy(TemplateValues mSet)
                    {
                        return false;
                    }

                    public IToken Duplicate()
                    {
                        return new RepeatToken(_set.Duplicate(), _key);
                    }

                }
            }
            class IfTokenReader : rootConsumer
            {
                rootConsumer _prev;
                string _name;
                public IfTokenReader(rootConsumer prev, string name)
                    : base(prev._parent)
                {
                    _prev = prev;
                    _name = name;
                }
                public override void Token(string token)
                {
                    switch (token)
                    {
                        case "ENDIF":
                            _prev._set.Add(new IfToken(_name, _set.Build()));
                            _parent._currentConsumer = _prev;
                            break;
                        case "ELSE":
                            _parent._currentConsumer = new ElseTokenReader(this);
                            break;
                        default:
                            base.Token(token);
                            break;
                    }
                }
                public override void Done()
                {
                    throw new InvalidOperationException("Missing ENDIF token for IF_" + _name);
                }
                class IfToken : IToken
                {
                    string _name;
                    IToken _if, _originalIf;

                    IToken _result;
                    bool _wasMerged = false;

                    public IfToken(string name, IToken ifSet)
                    {
                        _name = name;
                        _if = ifSet;
                        _originalIf = ifSet;

                        _result = null;

                    }

                    public string WriteStructure()
                    {
                        return string.Format("(if ({0}) then ({1}) )", _name, _if.WriteStructure());
                    }
                    public void AddMembers(MemberCollection mem)
                    {
                        mem.Add(_name, "boolean");
                        _originalIf.AddMembers(mem);
                    }

                    public void WriteTo(ITemplateTokensWriter writer, string prefix, string sufix, string tagPrefix, bool withinRepeat)
                    {
                        writer.IfToken(_name, _result != null);
                        if (_result != null)
                        {
                            writer.StartSection();
                            _result.WriteTo(writer, prefix, sufix, tagPrefix, withinRepeat);
                            writer.EndSection();
                        }
                        else writer.IfFalseWithoutElse();
                    }

                    public bool IsChildAffectedBy(TemplateValues mSet)
                    {
                        return IsAffectedBy(mSet);
                    }

                    #region Token Members

                    public void MergeTokens(TemplateValues mSet)
                    {
                        if (!_wasMerged)
                        {
                            mSet.IfExistThenDoOnIf(_name,
                                delegate (bool isTrue)
                                {
                                    if (isTrue)
                                    {
                                        _result = _if;
                                    }
                                    else
                                    {
                                        _if = null;
                                        _result = null;
                                    }
                                    _wasMerged = true;
                                });
                        }
                        if (_if != null)
                            _if.MergeTokens(mSet);

                    }

                    public bool IsAffectedBy(TemplateValues mSet)
                    {
                        if (mSet.Contains(_name)) return true;
                        return _if.IsAffectedBy(mSet);
                    }

                    public IToken Duplicate()
                    {
                        return new IfToken(_name, _if.Duplicate());
                    }

                    #endregion
                }
                class IfElseToken : IToken
                {
                    string _name;
                    IToken _if, _origIf, _origElse;
                    IToken _else;
                    IToken _result;
                    bool _wasMerged = false;

                    public IfElseToken(string name, IToken ifSet, IToken elseSet)
                    {
                        _name = name;
                        _if = ifSet;
                        _origIf = ifSet;
                        _else = elseSet;
                        _origElse = elseSet;
                        _result = _else;

                    }

                    public string WriteStructure()
                    {
                        return string.Format("(if ({0}) then ({1}) else ({2}))", _name, _if.WriteStructure(), _else.WriteStructure());
                    }
                    public void AddMembers(MemberCollection mem)
                    {
                        mem.Add(_name, "boolean");
                        _origIf.AddMembers(mem);
                        _origElse.AddMembers(mem);
                    }

                    public void WriteTo(ITemplateTokensWriter writer, string prefix, string sufix, string tagPrefix, bool withinRepeat)
                    {
                        writer.IfToken(_name, _result == _if);
                        writer.StartSection();
                        if (_result != null)
                            _result.WriteTo(writer, prefix, sufix, tagPrefix, withinRepeat);
                        writer.EndSection();
                    }

                    public bool IsChildAffectedBy(TemplateValues mSet)
                    {
                        return IsAffectedBy(mSet);
                    }

                    #region Token Members

                    public void MergeTokens(TemplateValues mSet)
                    {
                        if (!_wasMerged)
                        {
                            mSet.IfExistThenDoOnIf(_name,
                                delegate (bool isTrue)
                                {
                                    if (isTrue)
                                    {
                                        _result = _if;
                                        _else = null;
                                    }
                                    else
                                    {
                                        _result = _else;
                                        _if = null;
                                    }
                                    _wasMerged = true;
                                });
                        }
                        if (_if != null)
                            _if.MergeTokens(mSet);
                        if (_else != null)
                            _else.MergeTokens(mSet);
                    }

                    public bool IsAffectedBy(TemplateValues mSet)
                    {
                        if (mSet.Contains(_name)) return true;
                        return _if.IsAffectedBy(mSet) || _else != null && _else.IsAffectedBy(mSet);
                    }

                    public IToken Duplicate()
                    {
                        return new IfElseToken(_name, _if.Duplicate(), _else != null ? _else.Duplicate() : null);
                    }

                    #endregion
                }
                class ElseTokenReader : rootConsumer
                {
                    IfTokenReader _if;

                    public ElseTokenReader(IfTokenReader @if)
                        : base(@if._parent)
                    {
                        _if = @if;
                    }
                    public override void Token(string token)
                    {
                        switch (token)
                        {
                            case "ENDIF":
                                _if._prev._set.Add(new IfElseToken(_if._name, _if._set.Build(), _set.Build()));
                                _parent._currentConsumer = _if._prev;
                                break;
                            default:
                                base.Token(token);
                                break;

                        }
                    }
                    public override void Done()
                    {
                        throw new InvalidOperationException("Missing ENDIF token for ELSE for IF_" + _if._name);
                    }
                }
            }
            class IncludeTokenReader : rootConsumer
            {
                rootConsumer _prev;

                public IncludeTokenReader(rootConsumer prev)
                    : base(prev._parent)
                {
                    _prev = prev;
                }
                public override void Token(string token)
                {
                    switch (token)
                    {
                        case "ENDINCLUDE":
                            _prev._set.Add(new IncludeToken(_set.Build(), _parent));
                            _parent._currentConsumer = _prev;
                            break;
                        default:
                            base.Token(token);
                            break;
                    }

                }
                class IncludeToken : IToken
                {
                    IToken _set;
                    TokenConsumerClass _parent;

                    public IncludeToken(IToken set, TokenConsumerClass parent)
                    {
                        _set = set;
                        _parent = parent;
                    }

                    public string WriteStructure()
                    {
                        return string.Format("(Include {0})", _set.WriteStructure());
                    }
                    public void AddMembers(MemberCollection mem)
                    {

                    }

                    public void WriteTo(ITemplateTokensWriter writer, string prefix, string sufix, string tagPrefix, bool withinRepeat)
                    {
                        var x = new myWriter();
                        _set.WriteTo(x, prefix, sufix, tagPrefix, withinRepeat);
                        var fn = PathDecoder.DecodePath(x.Result);
                        try
                        {
                            writer.WriteText(_parent._getContentOfInclude(fn));
                        }
                        catch (Exception ex)
                        {
                            ErrorLog.WriteToLogFile(ex, "Merge Include Failed for {0}, {1}", x.Result, fn);
                        }

                    }
                    class myWriter : ITemplateTokensWriter
                    {
                        public string Result = "";
                        public void WriteText(string text)
                        {
                            Result += text;
                        }

                        public void WriteValue(string value, string name)
                        {
                            Result += value.TrimEnd(' ');
                        }
                        public void IfToken(string name, bool value)
                        {

                        }

                        public void EndSection()
                        {

                        }

                        public void StartSection()
                        {

                        }

                        public void IfFalseWithoutElse()
                        {

                        }

                        public void EndRepeat()
                        {

                        }

                        public void StartRepeat(string key)
                        {
                        }

                        public void StartRepeatRow()
                        {
                        }

                        public void EndRepeatRow()
                        {
                        }
                    }
                    public void MergeTokens(TemplateValues mSet)
                    {
                        _set.MergeTokens(mSet);
                    }

                    public bool IsChildAffectedBy(TemplateValues mSet)
                    {
                        return _set.IsChildAffectedBy(mSet);
                    }

                    public bool IsAffectedBy(TemplateValues mSet)
                    {
                        return _set.IsAffectedBy(mSet);
                    }

                    public IToken Duplicate()
                    {
                        return new IncludeToken(_set.Duplicate(), _parent);
                    }

                }
            }
            class TextToken : IToken
            {
                string _value;
                public TextToken(string Value)
                {
                    _value = Value;
                }
                public string WriteStructure()
                {
                    return _value;
                }
                public void AddMembers(MemberCollection mem)
                {

                }

                public void WriteTo(ITemplateTokensWriter writer, string prefix, string sufix, string tagPrefix, bool withinRepeat)
                {
                    writer.WriteText(_value);
                }

                public bool IsChildAffectedBy(TemplateValues mSet)
                {
                    return IsAffectedBy(mSet);
                }

                #region Token Members

                public void MergeTokens(TemplateValues mSet)
                {

                }

                public bool IsAffectedBy(TemplateValues mSet)
                {

                    return false;
                }

                public IToken Duplicate()
                {
                    return this;

                }

                #endregion


            }
            class InvalidToken : IToken
            {
                string _value;
                public InvalidToken(string Value)
                {
                    _value = Value;
                }
                public string WriteStructure()
                {
                    return _value;
                }
                public void AddMembers(MemberCollection mem)
                {

                }

                public void WriteTo(ITemplateTokensWriter writer, string prefix, string sufix, string tagPrefix, bool withinRepeat)
                {
                    writer.WriteText(prefix + _value + sufix);
                }



                public bool IsChildAffectedBy(TemplateValues mSet)
                {
                    return IsAffectedBy(mSet);
                }

                #region Token Members

                public void MergeTokens(TemplateValues mSet)
                {

                }

                public bool IsAffectedBy(TemplateValues mSet)
                {

                    return false;
                }

                public IToken Duplicate()
                {
                    return new TextToken(_value);
                }

                #endregion


            }
            class ValueToken : IToken
            {
                string _name;
                string _result = null;
                public ValueToken(string name)
                {
                    _name = name;
                }
                public string WriteStructure()
                {
                    return string.Format("(value name={0})", _name);
                }
                public void AddMembers(MemberCollection mem)
                {
                    mem.Add(_name, "string");
                }

                public void WriteTo(ITemplateTokensWriter writer, string prefix, string sufix, string tagPrefix, bool withinRepeat)
                {
                    if (_result == null)
                    {
                        writer.WriteValue(prefix + tagPrefix + _name + sufix, _name);
                        ENV.ErrorLog.WriteDebugLine("TextTemplate, Tag \"" + _name +
                                                           "\" exists in the tempalte but was not written to");
                    }
                    else
                        writer.WriteValue(_result, _name);
                }

                public bool IsChildAffectedBy(TemplateValues mSet)
                {
                    return IsAffectedBy(mSet);
                }

                #region Token Members

                public void MergeTokens(TemplateValues mSet)
                {
                    if (_result != null) return;
                    mSet.IfExistThenDo(_name,
                        delegate (string value)
                        {
                            _result = value ?? "";
                        });
                }
                public bool IsAffectedBy(TemplateValues mSet)
                {
                    return mSet.Contains(_name);
                }
                public IToken Duplicate()
                {
                    return new ValueToken(_name);
                }
                #endregion
            }

        }
        

        public TokenConsumerClass(TokenSetBuilder rootTokenSet, string valuePrefix, Func<string, string> getContentOfInclude)
        {
            _getContentOfInclude = getContentOfInclude;
            _currentConsumer = new rootConsumer(rootTokenSet, this);
            _valuePrefix = valuePrefix;
        }

        public void Token(string token)
        {
            _currentConsumer.Token(token);
        }

        public void Text(string text)
        {
            _currentConsumer.Text(text);
        }

        public void Done()
        {
            _currentConsumer.Done();
        }
    }
}