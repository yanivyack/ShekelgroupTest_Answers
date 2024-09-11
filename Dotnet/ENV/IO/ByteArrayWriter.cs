using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ENV.Data;
using ENV.IO.Advanced;
using Firefly.Box;

namespace ENV.IO
{
    public class ByteArrayWriter : Writer, ITemplateEnabled, IOByName, IXMLIO
    {
        XmlHelper __xmlHelper;

        private XmlHelper _xmlHelper
        {
            get
            {
                if (__xmlHelper == null)
                {
                    __xmlHelper = new XmlHelper();
                    if (_column.ContentType == ByteArrayColumnContentType.Unicode)
                        __xmlHelper.SetEncoding("UTF-16");
                    if (!ReferenceEquals(_column.Value, null) && _column.Value.Length > 0)
                    {
                        try
                        {
                            var s = _column.ToString().Trim();


                            if (!string.IsNullOrEmpty(s))
                            {
                                _xmlHelper.LoadXml(s);
                            }
                        }
                        catch (Exception e)
                        {
                            ErrorLog.WriteToLogFile(e, "read xml from byte array column");
                        }
                    }
                }
                return __xmlHelper;
            }
        }

        internal string _name = null;
        public string Name { set { _name = PathDecoder.DecodePath(value); } get { return _name; } }
        ByteArrayColumn _column;
        Encoding _encoding;
        public ByteArrayWriter(ByteArrayColumn column, System.Text.Encoding encoding = null)
        {
            _column = column;
            if (encoding == null)
                encoding = LocalizationInfo.Current.OuterEncoding;
            _encoding = encoding;

            _helper = new HebrewTextTools.TextWritingHelper(this);
        }


        protected override void OnWrite(string text)
        {
            if (text == null)
                return;
            var x = _encoding.GetBytes(text);
            var val = _column.Value ?? new byte[0];
            bool addCharAtTheEnd = false;
            if (val.Length > 0 && val[val.Length - 1] == 0)
            {
                var z = val;
                val = new byte[z.Length - 1];
                Array.Copy(z, val, z.Length - 1);
                addCharAtTheEnd = !UserSettings.Version10Compatible;
            }

            var newVal = new byte[val.Length + x.Length + (addCharAtTheEnd ? 1 : 0)];
            Array.Copy(val, newVal, val.Length);
            Array.Copy(x, 0, newVal, val.Length, x.Length);
            if (addCharAtTheEnd)
                newVal[newVal.Length - 1] = 0;
            //var currentText = _column.ContentType == ByteArrayColumnContentType.BinaryUnicode && _column.Value != null ? new string(ByteArrayColumn.UnicodeWithoutGaps.GetChars(_column.Value)) : _column.ToString() ?? "";
            _column.Value = newVal;
        }

        void ITemplateEnabled.WriteTextTemplate(Func<TemplateWriter> createTemplateWriter, Action<TemplateValues> provideTokens)
        {
            if (_template == null)
                _template = createTemplateWriter();
            var v = new TemplateValues();
            provideTokens(v);
            _template.MergeTokens(v);



        }

        #region Hebrew OEM issues

        HebrewTextTools.TextWritingHelper _helper;



        protected override string ProcessLine(string originalLine, int width, bool donotTrim)
        {
            return _helper.ProcessLine(originalLine, width, false);
        }

        protected override string ProcessControlData(string originalData, bool rightToLeft, bool hebrewDosCompatibleEditing)
        {
            return _helper.ProcessControlData(originalData, rightToLeft, hebrewDosCompatibleEditing);
        }

        public bool PerformRightToLeftManipulations
        {
            get { return _helper.PerformRightToLeftManipulations; }
            set { _helper.PerformRightToLeftManipulations = value; }
        }

        public bool RightToLeftFlipLine
        {
            get { return _helper.RightToLeftFlipLine; }
            set { _helper.RightToLeftFlipLine = value; }
        }

        public bool Oem
        {
            get { return _helper.Oem; }
            set { _helper.Oem = value; }
        }
        public bool OemForNonRightToLeftColumns
        {
            get { return _helper.OemForNonRightToLeftColumns; }
            set { _helper.OemForNonRightToLeftColumns = value; }
        }
        #endregion


        TemplateWriter _template = null;
        public static ByteArrayReader FindIOByName(string baseStreamName)
        {
            return IOFinder.FindByName<ByteArrayReader>(baseStreamName);
        }

        int _usagesByName = 0;
        public override void Dispose()
        {
            if (_usagesByName == 0)
            {
                if (_template != null)
                {
                    StringBuilder sb = new StringBuilder();
                    _template.WriteTo(delegate { }, a => sb.Append(a));
                    OnWrite(sb.ToString());
                }
                if (__xmlHelper != null)
                {
                    if (__xmlHelper.WasChanged)
                        __xmlHelper.WriteTo(_column);
                    __xmlHelper.Dispose();
                }

                base.Dispose();

            }
            else
                _usagesByName--;

        }

        void IOByName.AddToUsageCount()
        {
            _usagesByName++;
        }

        public Number Add(Text path, Text attributeName, Text value, Text beforeOrAfter)
        {
            return _xmlHelper.Add(path, attributeName, value, beforeOrAfter);
        }

        public Number Add(Text path, Text attributeName, byte[] value)
        {
            return _xmlHelper.Add(path, attributeName, value);
        }

        public Number Add(Text path, Text attributeName)
        {
            return _xmlHelper.Add(path, attributeName);
        }

        public Number Add(Text path, Text attributeName, Text value)
        {
            return _xmlHelper.Add(path, attributeName, value);
        }

        public Number Add(Text path, Text attributeName, byte[] value, Text beforeOrAfter)
        {
            return _xmlHelper.Add(path, attributeName, value, beforeOrAfter);
        }
        public Number Add(Text path, Text attributeName, Text value, Text beforeOrAfter, Text referencedElementName)
        {
            return _xmlHelper.Add(path, attributeName, value, beforeOrAfter, referencedElementName);
        }
        public Number Add(Text path, Text attributeName, Text value, Text beforeOrAfter, Text referencedElementName, Bool convertToXml)
        {
            return _xmlHelper.Add(path, attributeName, value, beforeOrAfter, referencedElementName, convertToXml);
        }
        public Number Add(Text path, Text attributeName, byte[] value, Text beforeOrAfter, Text referencedElementName, Bool convertToXml)
        {
            return _xmlHelper.Add(path, attributeName, value, beforeOrAfter, referencedElementName, convertToXml);
        }

        public Number Add(Text path, Text attributeName, ByteArrayColumn value, Text beforeOrAfter)
        {
            return _xmlHelper.Add(path, attributeName, value, beforeOrAfter);
        }

        public Number Add(Text path, Text attributeName, ByteArrayColumn value)
        {
            return _xmlHelper.Add(path, attributeName, value);
        }

        public Bool Contains(Text path, Text attrubuteName)
        {
            return _xmlHelper.Contains(path, attrubuteName);
        }

        public Bool Contains(Text path)
        {
            return _xmlHelper.Contains(path);
        }

        public Number Count(Text path)
        {
            return _xmlHelper.Count(path);
        }

        public Number Search(Text path, Text elementName, Bool isAttribute, Text childElementName, Text value)
        {
            return _xmlHelper.Search(path, elementName, isAttribute, childElementName, value);
        }
        public Number Search(Text path, Text elementName, Number isAttribute, Text childElementName, Text value)
        {
            return _xmlHelper.Search(path, elementName, isAttribute, childElementName, value);
        }
        public Text Get(Text path, Text attributeName, Bool returnNullIfNotFound)
        {
            return _xmlHelper.Get(path, attributeName, returnNullIfNotFound);
        }

        public Text Get(Text path, Text attributeName)
        {
            return _xmlHelper.Get(path, attributeName);
        }

        public Text Get(Text path)
        {
            return _xmlHelper.Get(path);
        }

        public Text GetAlias(Text path)
        {
            return _xmlHelper.GetAlias(path);
        }

        public byte[] GetByteArray(Text path, Text attributeName)
        {
            return _xmlHelper.GetByteArray(path, attributeName);
        }

        public byte[] GetByteArray(Text path)
        {
            return _xmlHelper.GetByteArray(path);
        }

        public Text GetEncoding()
        {
            return _xmlHelper.GetEncoding();
        }

        public bool Readonly
        {
            get { return _xmlHelper.Readonly; }
            set { _xmlHelper.Readonly = value; }
        }

        public Number Remove(Text path, Text attribute)
        {
            return _xmlHelper.Remove(path, attribute);
        }

        public Number Set(Text path, Text attributeName, Text value)
        {
            return _xmlHelper.Set(path, attributeName, value);
        }

        public Number Set(Text path, Text attributeName, Text value, Bool convertToXml)
        {
            return _xmlHelper.Set(path, attributeName, value, convertToXml);
        }

        public Number SetEncoding(Text encoding)
        {
            return _xmlHelper.SetEncoding(encoding);
        }

        public Number SetNamespace(Text nameSpace, Text uri)
        {
            return _xmlHelper.SetNamespace(nameSpace, uri);
        }

        public XmlDocument XmlDocument
        {
            get { return _xmlHelper.XmlDocument; }
        }

        public XmlHelper Xml
        {
            get { return _xmlHelper; }
        }
    }
}
