using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ENV.Printing;
using Firefly.Box;


namespace ENV.IO.Advanced
{
    /// <summary>
    /// Used to merge an input text template, with values to create a result file
    /// </summary>
    /// <remarks>
    /// TextTemplate merges a text input with special tags to create a merged result.
    /// Text template are mostly used to create html, and xml file, but can be used to create any text content.
    /// <br/>
    /// Text template supports these special tags:
    /// <list type="table">
    /// <item>
    /// <code>PXXX</code>
    /// <description>a tag that will be replaced with specific content, where P is the tag prefix, the XXX represent the tag's name. The value to that tag is added using the <see cref="AddTag"/> method</description>
    /// </item>
    /// <item>
    /// <code>REPEAT, ENDREPEAT</code>
    /// <description>Wrap a text section that will be duplicated for each time the <see cref="WriteTo"/> method is called for a template that has a tag within the repeat clause.</description></item>
    /// </list>
    /// <item><code>IFPXXX, ELSE,ENDIF</code>
    /// <description>Wrap a section that will only be written if a condition represented by the XXX tag is set to true. otherwise the else block is writen. Such a tag can be added by using the <see cref="AddIfTag"/>. <br/> P is the TagPrefix</description>
    /// </item>
    /// <item><code>INCLUDE,ENDINCLUDE</code><description>Wraps a file name that it's content will be added to the file.</description></item>
    /// </remarks>
    /// Note that once a template file is used with a <see cref="WebWriter"/>, <see cref="Writer"/> or <see cref="TextPrinterWriter"/> all future writing text templates that are written to it, will be considered as tags in the original <see cref="TextTemplate"/>
    public class TextTemplate
    {
        List<Tag> _tags =
            new List<Tag>();


        Func<System.IO.TextReader> _reader;
        string _tokenPrefix = "<!--@#";
        public virtual string TokenPrefix
        {
            get { return _tokenPrefix; }
            set { _tokenPrefix = value; }
        }

        string _tokenSuffix = "#@-->";
        string _TagPrefix = "_";

        public string TokenSuffix
        {
            get { return _tokenSuffix; }
            set { _tokenSuffix = value; }
        }
        /// <summary>
        /// The prefix that will be used to identify a tag.
        /// </summary>
        public string TagPrefix
        {
            get { return _TagPrefix; }
            set { _TagPrefix = value; }
        }

        public bool ReplaceXmlSpecialCharacters { get; set; }
        byte[] _extraBytes;
        public static string TemplateRoot = "";
        string _templateFile;
        public TextTemplate(string templateFile)
        {
            _reader = () =>
            {
                templateFile = templateFile.Trim();
                var file = PathDecoder.DecodePath(templateFile);
                using (Utilities.Profiler.StartContext("Load TextTemplate " + file + " (" + templateFile + ")"))
                {
                    TextReader reader = null;
                    if (!System.IO.Path.IsPathRooted(file))
                        file = TemplateRoot + file;
                    _templateFile = file;
                    try
                    {
                        var sr = new System.IO.StreamReader(file, ENV.LocalizationInfo.Current.OuterEncoding);

                        int b = sr.BaseStream.ReadByte();
                        if (b == 239)
                        {
                            b = sr.BaseStream.ReadByte();
                            if (b == 187)
                            {
                                b = sr.BaseStream.ReadByte();
                                if (b == 191)
                                {
                                    _extraBytes = new byte[] { 239, 187, 191 };
                                }
                            }
                        }
                        sr.BaseStream.Position = 0;
                        reader = sr;
                    }
                    catch (Exception e)
                    {
                        var message = string.Format("TextTemplate failed to initalize, {0} ({2}) Error:{1}"
                                                    , templateFile, e.Message, file);
                        TemplateError.Value = message;
                        ErrorLog.WriteToLogFile(e, message);
                        ErrorLog.WriteTraceLine(message);
                        Common.SetTemporaryStatusMessage(message);
                        reader = new StringReader("");
                    }
                    return reader;
                }
            };
        }

        public static ContextStatic<string> TemplateError = new ContextStatic<string>(() => null);

        public TextTemplate(System.IO.TextReader templateContent)
        {
            _reader = () => templateContent;

        }
        public void Add(params Tag[] tags)
        {
            _tags.AddRange(tags);
        }



        public static event Action BeforeWrite;

        public void WriteTo(ITemplateEnabled writer)
        {
            using (ENV.Utilities.Profiler.StartContext("Write Text Template: " + _templateFile))
            {
                if (BeforeWrite != null)
                    BeforeWrite();
                writer.WriteTextTemplate(
                    () =>
                    {
                        try
                        {
                            var result = new TemplateWriter(_reader(), _tokenPrefix, _tokenSuffix, _TagPrefix);
                            if (_extraBytes != null)
                                result._initBytes = _extraBytes;
                            return result;
                        }
                        catch (Exception ex)
                        {
                            Common.SetTemporaryStatusMessage("Text Template Error: " + ex.Message);
                            ErrorLog.WriteToLogFile(ex);
                            return new TemplateWriter(new StringReader(""), _tokenPrefix, _tokenSuffix, _TagPrefix);
                        }
                    },
                    tw =>
                    {


                        foreach (var tag in _tags)
                        {
                            tag.ApplyTo(tw, ReplaceXmlSpecialCharacters);
                        }

                    });
            }
        }

    }

    public interface ITemplateEnabled
    {
        void WriteTextTemplate(Func<TemplateWriter> createTemplateWriter, Action<TemplateValues> provideTokens);
    }



    interface IToken
    {
        void MergeTokens(TemplateValues mSet);
        bool IsAffectedBy(TemplateValues mSet);
        IToken Duplicate();
        string WriteStructure();
        void WriteTo(ITemplateTokensWriter writer, string prefix, string sufix, string tagPrefix, bool withinRepeat);
        bool IsChildAffectedBy(TemplateValues mSet);
        void AddMembers(MemberCollection mem);
    }
    class MemberCollection
    {
        int _indent;
        public MemberCollection(int indent)
        {
            _indent = indent;
        }
        public MemberCollection CreateChild()
        {
            return new MemberCollection(_indent + 1);
        }
        StringBuilder _result = new StringBuilder();
        HashSet<string> _included = new HashSet<string>();
        void NewLine(int minus = 0)
        {
            _result.Append("\r\n" + new String('\t', _indent - minus).Replace("\t","    "));
        }
        public void Add(string key, string type)
        {
            if (_included.Contains(key))
                return;
            _included.Add(key);

            if (_result.Length > 0)
                _result.Append(",");
            NewLine();
            if (key.Contains(" ")||key.Contains("\"") ){
                key = "\"" + key.Replace("\"","\\\"") + "\"";
            }
            _result.Append(key + ": " + type);
        }
        bool done = false;
        internal string ToTypescript()
        {
            if (!done)
            {
                NewLine(1);
                _result.Append("}");
                done = true;
            }
            return "{" + _result;
        }
    }
    interface ITemplateTokensWriter
    {
        void WriteText(string text);
        void WriteValue(string value, string name);
        void EndSection();
        void StartSection();
        void IfFalseWithoutElse();
        void EndRepeat();
        void StartRepeat(string key);
        void StartRepeatRow();
        void EndRepeatRow();
        void IfToken(string name, bool value);
    }
    class TokenSetBuilder
    {
        List<IToken> _tokensList = new List<IToken>();
        public void Add(IToken t)
        {

            lock (this)
            {
                _tokensList.Add(t);
            }
        }

        internal IToken Build()
        {
            if (_tokensList.Count == 1)
                return _tokensList[0];
            if (_tokensList.Count == 0)
                return EmptyToken.Instance;
            return new TokenSet(_tokensList.ToArray());
        }
    }
    class EmptyToken : IToken
    {
        public static EmptyToken Instance = new EmptyToken();

        public void AddMembers(MemberCollection mem)
        {

        }

        public IToken Duplicate()
        {
            return this;
        }

        public bool IsAffectedBy(TemplateValues mSet)
        {
            return false;
        }

        public bool IsChildAffectedBy(TemplateValues mSet)
        {
            return false;
        }

        public void MergeTokens(TemplateValues mSet)
        {

        }


        public string WriteStructure()
        {
            return "";
        }

        public void WriteTo(ITemplateTokensWriter writer, string prefix, string sufix, string tagPrefix, bool withinRepeat)
        {

        }
    }
    class TokenSet : IToken
    {


        IToken[] _tokens;
        public TokenSet(IToken[] tokens)
        {
            _tokens = tokens;
        }


        public string WriteStructure()
        {
            string result = string.Empty;
            if (_tokens != null)
                foreach (IToken t in _tokens)
                {
                    result += t.WriteStructure();
                }
            if (result == string.Empty) return "{EMPTY}";
            return "{" + result + "}";
        }
        public void AddMembers(MemberCollection mem)
        {
            if (_tokens != null)
                foreach (IToken t in _tokens)
                {
                    t.AddMembers(mem);
                }

        }
        public void MergeTokens(TemplateValues mSet)
        {
            lock (this)
            {
                if (_tokens != null)
                    foreach (IToken t in _tokens)
                    {
                        t.MergeTokens(mSet);
                    }
            }
        }
        public bool IsAffectedBy(TemplateValues mSet)
        {
            lock (this)
            {
                if (_tokens != null)
                    foreach (IToken t in _tokens)
                    {
                        if (t.IsAffectedBy(mSet)) return true;
                    }
                return false;
            }
        }
        public IToken Duplicate()
        {
            lock (this)
            {
                var result = new IToken[this._tokens.Length];
                for (int i = 0; i < this._tokens.Length; i++)
                {
                    result[i] = this._tokens[i].Duplicate();
                }
                return new TokenSet(result);
            }
        }


        public void WriteTo(ITemplateTokensWriter writer, string prefix, string sufix, string tagPrefix, bool withinRepeat)
        {
            lock (this)
            {
                if (_tokens != null)
                    foreach (IToken token in _tokens)
                    {
                        token.WriteTo(writer, prefix, sufix, tagPrefix, withinRepeat);
                    }
            }
        }


        public bool IsChildAffectedBy(TemplateValues set)
        {
            lock (this)
            {
                if (_tokens != null)
                    foreach (IToken t in _tokens)
                    {
                        if (t.IsChildAffectedBy(set)) return true;
                    }
                return false;
            }
        }

        public bool HasContent()
        {
            if (_tokens != null)
                return _tokens.Length > 0;
            return false;
        }


    }

    public class TemplateValues
    {
        System.Collections.Generic.Dictionary<string, System.Action<Action<string>>> _items =
            new System.Collections.Generic.Dictionary<string, Action<Action<string>>>();
        System.Collections.Generic.Dictionary<string, System.Action<Action<bool>>> _ifItems =
            new System.Collections.Generic.Dictionary<string, Action<Action<bool>>>();

        Dictionary<string, int> _index = new Dictionary<string, int>();
        public TemplateValues()
        {
        }

        HashSet<string> _usedTags = new HashSet<string>();
        public void Add(string Name, string value)
        {
            if (!_items.ContainsKey(Name))
            {
                _index.Add(Name, _index.Count);
                _items.Add(Name,
                    delegate (Action<string> obj)
                    {
                        obj(value);
                        _usedTags.Add(Name);
                    });
            }

        }
        public void Add(string Name, bool isTrue)
        {
            if (!_ifItems.ContainsKey(Name))
            {

                _ifItems.Add(Name, delegate (Action<bool> obj)
                {
                    obj(isTrue);
                    _usedTags.Add(Name);
                });
            }
        }
        public bool Contains(string Name)
        {
            return _items.ContainsKey(Name);
        }

        public void IfExistThenDo(string name, Action<string> doMe)
        {
            System.Action<Action<string>> doIt;
            if (_items.TryGetValue(name, out doIt))
            {
                doIt(doMe);
            }
        }

        public void IfExistThenDoOnIf(string name, Action<bool> doMe)
        {
            System.Action<Action<bool>> a;
            if (_ifItems.TryGetValue(name, out a))
                a(doMe);
            else
            {
                int index;
                if (_index.TryGetValue(name, out index))
                {
                    Action<Action<bool>> what = y => y(true);
                    int i = -1;
                    foreach (var ifI in _ifItems)
                    {
                        var z = _index[ifI.Key];
                        if (z < index && z > i)
                        {
                            i = z;
                            what = ifI.Value;
                        }

                    }
                    what(doMe);
                }

            }

        }

        public void ReportUnusedTags()
        {
            foreach (var name in _items.Keys)
            {
                if (!_usedTags.Contains(name))
                {
                    ENV.ErrorLog.WriteDebugLine("TextTemplate, Tag \"" + name +
                                                       "\" was not found in the template");
                    _usedTags.Add(name);
                }
            }
            foreach (var name in _ifItems.Keys)
            {
                if (!_usedTags.Contains(name))
                {
                    ENV.ErrorLog.WriteDebugLine("TextTemplate, Tag \"" + name +
                                                       "\" was not found in the template");
                }
            }
        }
    }

}
