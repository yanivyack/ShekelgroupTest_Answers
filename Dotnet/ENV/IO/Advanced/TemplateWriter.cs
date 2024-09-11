using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ENV.IO.Advanced
{
    public class TemplateWriter
    {
        IToken _set;
        string _tokenPrefix, _tokenSuffix,_tagPrefix;
        public TemplateWriter(string fileName)
            : this(new StringReader(System.IO.File.ReadAllText(fileName, LocalizationInfo.Current.OuterEncoding)), "<!$MG", ">", "_")
        {
        }

        public TemplateWriter(System.IO.TextReader templateSource, string tokenPrefix, string tokenSuffix, string tagPrefix) :
            this(templateSource, tokenPrefix, tokenSuffix, tagPrefix, f=> {
                if (string.IsNullOrEmpty(f))
                    return "";
                var r = System.IO.File.ReadAllText(f, LocalizationInfo.Current.OuterEncoding);
                return r;
            })
        {
        }

        public TemplateWriter(System.IO.TextReader templateSource, string tokenPrefix, string tokenSuffix, string tagPrefix, Func<string, string> getIncludeContent)
        {

            _tokenPrefix = tokenPrefix;
            _tokenSuffix = tokenSuffix;
            _tagPrefix = tagPrefix;
            var tokenSetBuilder = new TokenSetBuilder();
            new TokenParser(new TokenConsumerClass(tokenSetBuilder, tagPrefix, getIncludeContent), tokenPrefix, tokenSuffix).Process(templateSource);
            _set = tokenSetBuilder.Build();
        }

        internal byte[] _initBytes;

        public string WriteStructure()
        {
            return _set.WriteStructure();
        }
        long _numberOfWrites = 0;
        public void MergeTokens(TemplateValues mSet)
        {
            _numberOfWrites++;
            _set.MergeTokens(mSet);
#if DEBUG
            mSet.ReportUnusedTags();
#endif
        }
        public void MergeTokens(params object[] vals)
        {
            TemplateValues ms = new TemplateValues();
            System.Diagnostics.Debug.Assert(vals.Length % 2 == 0, "paramenters count is not correct - shoud be name value pairs");
            for (int i = 0; i < vals.Length; i += 2)
            {
                object va = vals[i + 1];
                ms.Add((string)vals[i], va.ToString());
                if (va is bool)
                    ms.Add((string)vals[i], (bool)va);
            }
            MergeTokens(ms);
        }
        public static bool DisableGCCollect = false;
        public void WriteTo(Action<byte[]> writeInitBytes, Action<string> w)
        {
            if (_initBytes != null)
                writeInitBytes(_initBytes);
            _set.WriteTo(new TemplateTokensWriterBackwardComatability(
                             new TemplateTokensWriter(w)
                             ), _tokenPrefix, _tokenSuffix,_tagPrefix, false);
            _set = null;
            if (_numberOfWrites > 500&&!DisableGCCollect)
                GC.Collect();
        }
        public override string ToString()
        {
            lock (this)
            {
                using (System.IO.StringWriter sw = new System.IO.StringWriter())
                {
                    WriteTo(delegate { }, sw.Write);

                    return sw.ToString();
                }
            }
        }

        static Dictionary<string, string> _stringsToReplace = new Dictionary<string, string>();
        public static void AddStringToReplace(string what, string newValue)
        {
            if (!Firefly.Box.Text.IsNullOrEmpty(what))
                _stringsToReplace.Add(what, newValue);
        }

        class TemplateTokensWriter : ITemplateTokensWriter
        {
            Action<string> _write;

            public TemplateTokensWriter(Action<string> write)
            {
                _write = x =>
                {
                    foreach (var item in _stringsToReplace)
                    {
                        var stringToReplace = item.Key;
                        int i;
                        while ((i = x.IndexOf(stringToReplace, StringComparison.InvariantCultureIgnoreCase)) >= 0)
                        {
                            x = x.Remove(i, stringToReplace.Length).Insert(i, item.Value);
                        }
                    }

                    write(x);

                };
            }

            public void WriteText(string text)
            {
                _write(text);
            }

            public void WriteValue(string value, string name)
            {
                _write(value.TrimEnd(' '));
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

            public void StartRepeat(string name)
            {
            }

            public void StartRepeatRow()
            {
            }

            public void EndRepeatRow()
            {
            }
        }

        class TemplateTokensWriterBackwardComatability : ITemplateTokensWriter
        {
            ITemplateTokensWriter _writer;
            bool _removeEnterFromNextLine = false;
            bool _hasTextInSection = false;
            bool _hasActualTextInSection = false;

            public TemplateTokensWriterBackwardComatability(ITemplateTokensWriter writer)
            {
                _writer = writer;
            }

            public void WriteText(string text)
            {
                _hasTextInSection = true;

                if (_removeEnterFromNextLine)
                {
                    if (text.TrimStart(' ', '\t').StartsWith("\r\n"))
                    {
                        text = text.Substring(text.IndexOf("\r\n") + 2);
                    }
                }
                _writer.WriteText(text);
                string trimmedText = text.TrimEnd(new char[] { ' ', '\t' });
                if (trimmedText.Length != 0)
                {
                    _removeEnterFromNextLine = trimmedText.EndsWith("\r\n");
                    _hasActualTextInSection = true;
                }
                _prevWasValue = false;
            }

            bool _prevWasValue;
            public void WriteValue(string value, string name)
            {
                var x = value.IndexOf('\0');
                if (x >= 0)
                    value = value.Remove(x);
                _prevWasValue = true;
                _removeEnterFromNextLine = false;
                _writer.WriteValue(value, name);
            }
            public void IfToken(string name, bool value)
            {

            }

            public void EndSection()
            {
                if (!_hasTextInSection)
                {
                    _removeEnterFromNextLine = true;
                }
                _hasTextInSection = false;
                _hasActualTextInSection = false;
            }

            public void StartSection()
            {
                if (!_hasActualTextInSection)
                    _removeEnterFromNextLine = true;
                _hasTextInSection = false;
                _hasActualTextInSection = false;

            }

            public void IfFalseWithoutElse()
            {
                if (_prevWasValue)
                    _removeEnterFromNextLine = true;
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


        internal void Visit(ITemplateTokensWriter visitor)
        {
            _set.WriteTo(visitor, _tokenPrefix, _tokenSuffix,_tagPrefix, false);
        }

        internal string GetTypeScriptTypeDefinition()
        {
            var mem = new MemberCollection(1);
            _set.AddMembers(mem);
            return mem.ToTypescript();
        }
    }



}