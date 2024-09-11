using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Xml;
using ENV.Data;
using Firefly.Box;


namespace ENV.IO
{
    public class XML : IOByName, IXMLIO, IDisposable
    {
        XmlHelper _helper = new XmlHelper();

        public Number Add(Text path, Text attributeName, Text value, Text beforeOrAfter)
        {
            return _helper.Add(path, attributeName, value, beforeOrAfter);
        }

        public Number Add(Text path, Text attributeName, byte[] value)
        {
            return _helper.Add(path, attributeName, value);
        }

        public Number Add(Text path, Text attributeName)
        {
            return _helper.Add(path, attributeName);
        }

        public Number Add(Text path, Text attributeName, Text value)
        {
            return _helper.Add(path, attributeName, value);
        }

        public Number Add(Text path, Text attributeName, byte[] value, Text beforeOrAfter)
        {
            return _helper.Add(path, attributeName, value, beforeOrAfter);
        }

        public Number Add(Text path, Text attributeName, Text value, Text beforeOrAfter, Text referencedElementName)
        {
            return _helper.Add(path, attributeName, value, beforeOrAfter, referencedElementName);
        }

        public Number Add(Text path, Text attributeName, Text value, Text beforeOrAfter, Text referencedElementName, Bool convertToXml)
        {
            return _helper.Add(path, attributeName, value, beforeOrAfter, referencedElementName, convertToXml);
        }

        public Number Add(Text path, Text attributeName, ByteArrayColumn value, Text beforeOrAfter)
        {
            return _helper.Add(path, attributeName, value, beforeOrAfter);
        }

        public Number Add(Text path, Text attributeName, ByteArrayColumn value)
        {
            return _helper.Add(path, attributeName, value);
        }

        public Bool Contains(Text path, Text attrubuteName)
        {
            return _helper.Contains(path, attrubuteName);
        }

        public Bool Contains(Text path)
        {
            return _helper.Contains(path);
        }

        public Number Count(Text path,Text attribute=null)
        {
            return _helper.Count(path,attribute);
        }

        public Number Search(Text path, Text elementName, Bool isAttribute, Text childElementName, Text value)
        {
            return Xml.Search(path, elementName, isAttribute, childElementName, value);
        }
        public Number Search(Text path, Text elementName, Number isAttribute, Text childElementName, Text value)
        {
            return Xml.Search(path, elementName, isAttribute, childElementName, value);
        }
        public Text Get(Text path, Text attributeName, Bool returnNullIfNotFound)
        {
            return _helper.Get(path, attributeName, returnNullIfNotFound);
        }
        public Text Get(Text path, Text attributeName)
        {
            return _helper.Get(path, attributeName);
        }

        public Text Get(Text path)
        {
            return _helper.Get(path);
        }

        public Text GetAlias(Text path)
        {
            return _helper.GetAlias(path);
        }

        public byte[] GetByteArray(Text path, Text attributeName)
        {
            return _helper.GetByteArray(path, attributeName);
        }

        public byte[] GetByteArray(Text path)
        {
            return _helper.GetByteArray(path);
        }

        public Text GetEncoding()
        {
            return _helper.GetEncoding();
        }

        public Number Remove(Text path, Text attribute)
        {
            return _helper.Remove(path, attribute);
        }

        public Number Set(Text path, Text attributeName, Text value)
        {
            return _helper.Set(path, attributeName, value);
        }

        public Number Set(Text path, Text attributeName, Text value, Bool convertToXml)
        {
            return _helper.Set(path, attributeName, value, convertToXml);
        }

        public Number SetEncoding(Text encoding)
        {
            return _helper.SetEncoding(encoding);
        }

        public Number SetNamespace(Text nameSpace, Text uri)
        {
            return _helper.SetNamespace(nameSpace, uri);
        }

        public XmlDocument XmlDocument
        {
            get { return _helper.XmlDocument; }
        }

        string _filename;
        public XML(string fileName)
            : this()
        {
            _filename = PathDecoder.DecodePath(fileName);
            try
            {
                var x = _filename;
                if (System.IO.File.Exists(x))
                    _helper.Load(x);
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
            }
        }
        public XML()
        {

        }
        public bool Readonly { get { return _helper.Readonly; } set { _helper.Readonly = value; } }
        internal string _name = null;
        public string Name { set { _name = PathDecoder.DecodePath(value); } get { return _name; } }






        public static XML FindIOByName(Text baseStreamName)
        {
            return IOFinder.FindByName<XML>(baseStreamName);
        }

        int _usagesByName = 0;
        public void Dispose()
        {
            if (_usagesByName == 0)
            {
                if (!Readonly && !string.IsNullOrEmpty(_filename))
                {
                    try
                    {
                        _helper.SaveTo(_filename);
                    }
                    catch (Exception e)
                    {
                        ENV.ErrorLog.WriteToLogFile(e, "");
                    }
                }
                _helper.Dispose();
            }
            else
                _usagesByName--;
        }

        void IOByName.AddToUsageCount()
        {
            _usagesByName++;
        }

        public XmlHelper Xml
        {
            get { return _helper; }
        }
    }

    interface IXMLIO
    {
        XmlHelper Xml { get; }
    }

    public class XmlHelper
    {
        System.Xml.XmlDocument _document = new XmlDocument { };
        public bool WasChanged { get; private set; }
        public bool WasAccessed { get; private set; }

        public XmlHelper()
        {

        }
        public System.Xml.XmlDocument XmlDocument { get { return _document; } }
        public void Load(string fileName)
        {
            try
            {
                _document.Load(fileName);
            }
            catch
            {//encoding fallback
                using (var sr = new StreamReader(fileName, LocalizationInfo.Current.OuterEncoding))
                {
                    _document.Load(sr);
                }
            }
        }
        System.Xml.XmlElement FindElement(string path)
        {
            WasAccessed = true;
            path = path.TrimEnd();
            System.Xml.XmlElement result = _document.DocumentElement;
            if (result == null)
                return null;
            bool first = true;
            char separator = DetermineSeparator(path);

            if (path.Length == 0)
                return null;
            if (path[0] == separator)
                path = path.Substring(1);
            string[] splittedPath = path.Split(separator);
            foreach (string s in splittedPath)
            {
                bool found = false;
                if (first)
                {
                    string elemntName = s;
                    first = false;
                    if (elemntName.Contains("["))
                    {
                        var indexer = int.Parse(SubstringBetween(elemntName, "[", "]"));
                        if (indexer != 1)
                            return null;
                        elemntName = RemoveAt(elemntName, "[");
                    }
                    if (!CompareName(result, elemntName))
                        return null;
                    found = true;
                }
                else
                {
                    result = FindElement(result, s);
                    found = result != null;
                }
                if (!found)
                    return null;
            }
            return result;
        }

        Dictionary<XmlElement, ElementSearchInfo> _cache = new Dictionary<XmlElement, ElementSearchInfo>();

        XmlElement FindElement(XmlElement result, string elemntName)
        {
            ElementSearchInfo si;
            if (!_cache.TryGetValue(result, out si))
            {
                _cache.Add(result, si = new ElementSearchInfo(result));
            }
            return si.Find(elemntName);


        }

        class ElementSearchInfo
        {
            XmlElement _element;
            public ElementSearchInfo(XmlElement element)
            {
                _element = element;
            }
            string _lastSearchString;
            string _lastSearchElement;
            int _lastSearchIndex;
            XmlElement _lastResult;
            public XmlElement Find(string searchString)
            {

                if (_lastSearchString == searchString)
                    return _lastResult;

                string elemntName = searchString;
                int index = 1;
                if (elemntName.Contains("["))
                {
                    index = int.Parse(SubstringBetween(elemntName, "[", "]"));
                    elemntName = RemoveAt(elemntName, "[");
                }
                elemntName = elemntName.TrimEnd();
                if (elemntName.StartsWith(":"))
                    elemntName = elemntName.Substring(1);
                bool useNameAndNotLocalName = elemntName.Contains(":");

                try
                {
                    var y = _lastResult;
                    _lastResult = null;
                    if (_lastSearchElement == elemntName && _lastSearchIndex < index && _lastSearchIndex > 0)
                    {
                        if (y == null)
                            return null;
                        int i = _lastSearchIndex;
                        while (i < index)
                        {
                            y = GetNextElement(y);


                            if (y == null)
                            {
                                return null;
                            }
                            if ((useNameAndNotLocalName ? y.Name : y.LocalName) == elemntName)
                            {
                                i++;
                                if (i == index)
                                {
                                    return _lastResult = y;
                                }
                            }
                        }
                    }






                    {

                        int i = 1;
                        if (index == 0)
                            return null;
                        foreach (object c in _element.ChildNodes)
                        {
                            var child = c as XmlElement;
                            if (child != null && (useNameAndNotLocalName ? child.Name : child.LocalName) == elemntName)
                            {
                                if (i == index)
                                {

                                    return _lastResult = child;
                                }
                                else
                                {
                                    i++;
                                }
                            }
                        }
                    }
                    return null;
                }
                finally
                {
                    _lastSearchElement = elemntName;
                    _lastSearchIndex = index;
                    _lastSearchString = searchString;

                }
            }

            private XmlElement GetNextElement(XmlNode y)
            {
                while (y.NextSibling != null)
                {
                    var z = y.NextSibling as XmlElement;
                    if (z != null)
                        return z;
                    y = y.NextSibling as XmlLinkedNode;
                    if (y == null)
                        return null;
                }
                return null;

            }
        }


        internal static bool CompareName(XmlNode e, string elementName)
        {
            elementName = elementName.TrimEnd();
            if (elementName.StartsWith(":"))
                elementName = elementName.Substring(1);
            if (elementName.Contains(":"))
                return e.Name == elementName;
            return e.LocalName == elementName;

        }




        static string SubstringBetween(Text source, string prefix, string suffix)
        {
            Text result = SubstringAfter(source, prefix);
            result = RemoveAt(result, suffix);
            return result;
        }
        public static Text SubstringAfter(Text _string, Text textToLookForAndStartAfter)
        {
            return _string.Substring(_string.IndexOf(textToLookForAndStartAfter) + textToLookForAndStartAfter.Length);
        }
        public static Text RemoveAt(string _string, Text textToLookForAndRemoveItAndWhatsAfterIt)
        {
            return _string.Remove(_string.IndexOf(textToLookForAndRemoveItAndWhatsAfterIt));
        }
        static string SubstringUntillLastApearanceOf(string source, char delimiter)
        {
            return source.ToString().Remove(source.ToString().LastIndexOf(delimiter)).TrimEnd();
        }

        static string SubstringAfterLastApearanceOf(string source, char delimiter)
        {
            return source.Substring(source.ToString().LastIndexOf(delimiter) + 1).TrimEnd();
        }

        public void LoadXml(string content)
        {
            try
            {
                _document.LoadXml(content);
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");

            }
        }
        public void LoadXml(byte[] content)
        {
            try
            {
                _document.Load(new MemoryStream(content));
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");

            }
        }
        public bool Readonly { get; set; }
        Dictionary<string, string> _namespaces = new Dictionary<string, string>();


        const string _ourCDataPlaceHolderString = "!!@#$FM~~NHDFHBWET@";

        internal static string DefaultEncodingName = "UTF-8";
        internal static Encoding DefaultEncoding = new System.Text.UTF8Encoding(false);
        string _encoding = DefaultEncodingName;

        internal string ToTestXml()
        {
            AddNamespaceAttributes();
            using (var ms = new MemoryStream())
            {
                System.Text.Encoding e = new System.Text.UTF8Encoding(false);
                {
                    var xmlDel = _document.FirstChild as XmlDeclaration;
                    if (xmlDel != null)
                    {
                        e = System.Text.Encoding.GetEncoding(xmlDel.Encoding);
                        if (e is System.Text.UTF8Encoding)
                            e = new System.Text.UTF8Encoding(false);
                    }
                }
                using (var s = new StreamWriter(ms, e))
                {

                    DoSave(s, e);
                }
                return e.GetString(ms.ToArray()).Replace("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>\r\n", "").Trim();
            }
        }

        class myFormatting : XmlTextWriter
        {
            string _encodingName;
            TextWriter _writer;
            public myFormatting(TextWriter w, string encodingName)
                : base(w)
            {
                _writer = w;
                _encodingName = encodingName;
            }

            public override void WriteCData(string text)
            {
                if (text.StartsWith(_ourCDataPlaceHolderString))
                    WriteRaw(text.Substring(_ourCDataPlaceHolderString.Length));
                else
                    base.WriteCData(text);
            }
            bool _lastWasStart;
            int _depth = 0;

            public override void WriteStartAttribute(string prefix, string localName, string ns)
            {
                if (string.IsNullOrEmpty(ns))
                    base.WriteStartAttribute(string.Empty, (string.IsNullOrEmpty(prefix) ? "" : prefix + ":") + localName, string.Empty);
                else
                    base.WriteStartAttribute(prefix, localName, ns);
            }
            static List<string[]> xmlSpecialCharacters = new List<string[]>
                                                         {
                                                             new[] {"&", "&amp;"},
                                                             new[] {"\"", "&quot;"},
                                                             new[] {"<", "&lt;"},
                                                             new[] {">", "&gt;"},
                                                             new []{"\r","&#xD;"},
                                                             new []{"\n","&#xA;"},
                                                             new []{"“","&#x201C;"},
                                                             new []{"”","&#x201D;"},
                                                         };
            public override void WriteString(string text)
            {
                if (base.WriteState == WriteState.Attribute)
                {
                    var sb = new StringBuilder(text);
                    foreach (var c in xmlSpecialCharacters)
                    {
                        sb.Replace(c[0], c[1]);
                    }
                    base.WriteRaw(sb.ToString());
                }
                else
                    base.WriteString(text);
            }

            public override void WriteStartElement(string prefix, string localName, string ns)
            {
                _lastWasStart = true;
                _depth++;
                if (string.IsNullOrEmpty(ns))
                    base.WriteStartElement(string.Empty, (string.IsNullOrEmpty(prefix) ? "" : prefix + ":") + localName, string.Empty);
                else
                    base.WriteStartElement(prefix, localName, ns);
            }

            public override void WriteStartDocument(bool standalone)
            {
                this.WriteRaw(string.Format("<?xml version=\"1.0\" encoding=\"{0}\" standalone=\"no\" ?>", _encodingName));

            }
            public override void WriteEndElement()
            {
                _depth--;
                if (_lastWasStart)
                    Formatting = System.Xml.Formatting.None;
                _lastWasStart = false;
                base.WriteEndElement();
                Formatting = System.Xml.Formatting.Indented;
            }
            public override void WriteFullEndElement()
            {
                _depth--;
                if (_lastWasStart)
                    Formatting = System.Xml.Formatting.None;
                _lastWasStart = false;

                base.WriteFullEndElement();
                Formatting = System.Xml.Formatting.Indented;

            }
        }



        void DoSave(StreamWriter s, Encoding e)
        {
            if (!UserSettings.Version10Compatible)
                s.NewLine = "\n";
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms, e))
                {
                    if (!UserSettings.Version10Compatible)
                        sw.NewLine = "\n";
                    using (var xmltextWriter = new myFormatting(sw, e.WebName.ToUpper()) { Formatting = Formatting.Indented })
                        _document.Save(xmltextWriter);

                }
                s.Write(e.GetString(ms.ToArray()).Replace(" />", "/>"));
                s.Dispose();
            }
        }


        public void SaveTo(string filename)
        {
            AddNamespaceAttributes();
            System.Text.Encoding e = new System.Text.UTF8Encoding(false);
            {
                var xmlDel = _document.FirstChild as XmlDeclaration;
                if (xmlDel != null)
                {
                    e = System.Text.Encoding.GetEncoding(xmlDel.Encoding);
                    if (e is System.Text.UTF8Encoding)
                        e = new System.Text.UTF8Encoding(false);
                }
            }
            using (var sw = new StreamWriter(filename, false, e))
                DoSave(sw, e);
        }




        void AddNamespaceAttributes()
        {
            if (_document.DocumentElement != null)
            {
                foreach (var ns in _namespaces)
                {
                    var x = _document.CreateAttribute(ns.Key);
                    x.Value = ns.Value;
                    _document.DocumentElement.SetAttributeNode(x);
                }
            }
        }

        internal void WriteTo(ENV.Data.ByteArrayColumn column)
        {
            AddNamespaceAttributes();
            using (var ms = new MemoryStream())
            {
                System.Text.Encoding e = new System.Text.UTF8Encoding(false);
                {
                    var xmlDel = _document.FirstChild as XmlDeclaration;
                    if (xmlDel != null)
                    {
                        e = System.Text.Encoding.GetEncoding(xmlDel.Encoding);
                        if (e is System.Text.UTF8Encoding)
                            e = new System.Text.UTF8Encoding(false);
                        if (e is UnicodeEncoding)
                        {
                            e = ByteArrayColumn.UnicodeWithoutGaps;
                                }
                    }
                }
                using (var s = new StreamWriter(ms, column.ContentType == ByteArrayColumnContentType.Ansi?ENV.Data.DataProvider.XmlEntityDataProvider.SaveAnsiAsUTF8?new UTF8Encoding(false) :  LocalizationInfo.Current.OuterEncoding : e))
                {
                    DoSave(s, e);
                    column.Value = ms.ToArray();
                }
            }
        }

        public Number SetNamespace(Text nameSpace, Text uri)
        {
            if (nameSpace == null || uri == null)
                return null;
            try
            {
                if (Readonly)
                    return -3;
                nameSpace = nameSpace.Trim();
                uri = uri.Trim();
                WasChanged = true;
                var attributeName = "xmlns";
                if (!string.IsNullOrEmpty(nameSpace))
                    attributeName += ":" + nameSpace;
                _namespaces[attributeName] = uri;
                return 0;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return -1;
            }


        }



        public Bool Contains(Text path)
        {
            if (path == null)
                return null;
            return FindElement(path) != null;
        }

        public Text Get(Text path)
        {
            return Get(path, false);
        }

        public Text Get(Text path, bool returnNullIfNotFound)
        {
            if (path == null)
                return null;
            System.Xml.XmlElement result = FindElement(path);
            if (result == null) return returnNullIfNotFound ? null : "";
            string sr = "";
            foreach (var cn in result.ChildNodes)
            {
                var cnt = cn as System.Xml.XmlCharacterData;
                if (cnt != null)
                {
                    var z = cnt as XmlCDataSection;
                    if (z != null && z.InnerText.StartsWith(_ourCDataPlaceHolderString))
                        sr += z.InnerText.Substring(_ourCDataPlaceHolderString.Length);
                    else
                        sr += cnt.InnerText;
                }
            }
            return sr;
        }
        public Number Search(Text path, Text elementName, Number isAttribute, Text childElementName, Text value)
        {
            return Search(path, elementName, isAttribute == 1, childElementName, value);
        }
        public Number Search(Text path, Text elementName, Bool isAttribute, Text childElementName, Text value)
        {
            if (path == null || elementName == null || isAttribute == null || childElementName == null || value == null)
                return null;
            elementName = elementName.TrimEnd();
            childElementName = childElementName.TrimEnd();
            value = value.TrimEnd();

            var top = FindElement(path);
            if (top == null)
                return 0;
            int i = 0;
            foreach (var n in top.ChildNodes)
            {
                var e = n as XmlElement;
                if (e != null && CompareName(e, elementName))
                {
                    i++;
                    if (isAttribute)
                    {
                        if (e.GetAttribute(childElementName) == value)
                            return i;
                    }
                    else
                    {
                        foreach (var cb in e.ChildNodes)
                        {
                            var ce = cb as XmlNode;

                            if (ce == null)
                                continue;
                            if (string.IsNullOrEmpty(childElementName) && ce.Value == value)
                                return i;
                            if (CompareName(ce, childElementName) && ce.InnerText == value)
                                return i;
                        }
                    }
                }
            }
            return 0;
        }


        /// <summary>
        /// Gets the value of an xml element or attribute
        /// </summary>
        /// <param name="path">the xml element path</param>
        /// <param name="attributeName">attribute name, if empty then the xml element itself will be referenced</param>
        /// <returns></returns>
        public Text Get(Text path, Text attributeName)
        {
            return Get(path, attributeName, false);
        }

        public Text Get(Text path, Text attributeName, bool returnNullIfNotFound)
        {
            if (path == null || attributeName == null)
                return null;
            if (!Firefly.Box.Text.IsNullOrEmpty(attributeName))
            {
                System.Xml.XmlElement result = FindElement(path);
                if (result == null) return returnNullIfNotFound ? null : "";
                if (attributeName.StartsWith("/"))
                    attributeName = attributeName.Substring(1);
                return result.GetAttribute(attributeName.Trim());
            }
            else
                return Get(path, returnNullIfNotFound);
        }
        /// <summary>
        /// Checks if the elemen or attribute exist
        /// </summary>
        /// <param name="path">the xml element path</param>
        /// <param name="attributeName">attribute name, if empty then the xml element itself will be referenced</param>
        /// <returns></returns>
        public Bool Contains(Text path, Text attrubiteName)
        {
            if (path == null || attrubiteName == null)
                return null;
            System.Xml.XmlElement result = FindElement(path);
            if (result == null) return false;
            if (attrubiteName.ToString() != "")
                return result.GetAttributeNode(attrubiteName.TrimEnd()) != null;
            return Contains(path);

        }
        internal static char DetermineSeparator(Text path)
        {

            if (path.Contains("/"))
                return '/';
            return '.';
        }
        public Number Count(Text path, Text attrubiteName = null)
        {
            if (path == null)
                return null;
            var separator = DetermineSeparator(path);
            var parent = _document.ChildNodes;
            var nameOfCountedNode = path;
            if (path.IndexOf(separator) > -1)
            {
                string pathOfParent = SubstringUntillLastApearanceOf(path, separator);
                nameOfCountedNode = SubstringAfterLastApearanceOf(path, separator);
                var y = FindElement(pathOfParent);
                if (y != null)
                    parent = y.ChildNodes;
            }
            ;
            if (parent == null)
                return 0;
            int count = 0;
            foreach (object ex in parent)
            {
                var e = ex as XmlElement;
                if (e != null && CompareName(e, nameOfCountedNode))
                {
                    if (Text.IsNullOrEmpty(attrubiteName) ||e.GetAttributeNode(attrubiteName.TrimEnd()) != null)
                        count++;
                }
            }
            return count == 0 && FindElement(path) != null ? 1 : count;

        }


        public Number Set(Text path, Text attributeName, Text value, Bool convertToXml)
        {
            if (path == null || attributeName == null || value == null ||
                convertToXml == null)
                return null;
            if (!Text.IsNullOrEmpty(attributeName))
                attributeName = attributeName.TrimEnd();
            path = path.TrimEnd();
            value = fixValue(value);
            if (Readonly)
                return -3;
            try
            {
                WasChanged = true;
                if (convertToXml)
                    value = ENV.UserMethods.Instance.InternalXMLVal(value, true);
                var e = FindElement(path);
                if (e == null)
                    return -4;
                _cache.Clear();
                if (string.IsNullOrEmpty(attributeName))
                {
                    var cdata = _document.CreateCDataSection(_ourCDataPlaceHolderString + value);
                    e.InnerXml = "";
                    e.AppendChild(cdata);
                    return 0;
                }
                else
                {
                    var attr = e.GetAttributeNode(attributeName);
                    if (attr == null)
                        return -5;
                    attr.Value = value;
                    return 0;
                }

            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return -101;
            }
        }

        public Number Remove(Text path, Text attribute)
        {
            if (attribute == null || path == null)
                return null;
            attribute = attribute.TrimEnd();
            if (Readonly)
                return -3;
            try
            {
                System.Xml.XmlElement result = FindElement(path);
                _cache.Clear();
                if (result != null)
                {
                    result.ParentNode.RemoveChild(result);
                    return 0;
                }
                else
                    return -4;

            }
            catch
            {
                return -101;
            }
        }

        public Number Add(Text path, Text attributeName, Text value, Text beforeOrAfter, Text referencedElementName)
        {
            return Add(path, attributeName, value, beforeOrAfter, referencedElementName, true);
        }
        public Number Add(Text path, Text attributeName, byte[] value, Text beforeOrAfter, Text referencedElementName, Bool convertToXml)
        {
            return Add(path, attributeName, TextColumn.FromByteArray(value), beforeOrAfter, referencedElementName, convertToXml);
        }
        string fixValue(string value)
        {
            return ENV.UserMethods.Instance.RemoveZeroChar(value.TrimEnd());

        }
        public Number Add(Text path, Text attributeName, Text value, Text beforeOrAfter, Text referencedElementName, Bool convertToXml)
        {
            if (path == null || attributeName == null ||
                beforeOrAfter == null || referencedElementName == null || convertToXml == null)
                return null;
            if (value == null)
                value = "";

            path = path.TrimEnd();
            value = fixValue(value);
            beforeOrAfter = beforeOrAfter.TrimEnd();
            referencedElementName = referencedElementName.TrimEnd();
            if (attributeName != null)
                attributeName = attributeName.TrimEnd();
            if (Readonly)
                return -3;
            try
            {
                WasChanged = true;
                var separator = DetermineSeparator(path);


                if (path.StartsWith(separator.ToString()))
                    path = path.Substring(1);
                if (string.IsNullOrEmpty(attributeName))
                {
                    if (convertToXml)
                        value = ENV.UserMethods.Instance.InternalXMLVal(value, true);
                    string nameOfItem = SubstringAfterLastApearanceOf(path, separator);
                    var de = _document.DocumentElement;
                    var e = _document.CreateElement(nameOfItem, de != null &&!de.Name.Contains(":") ? de.NamespaceURI : "");

                    if (!string.IsNullOrEmpty(value))
                        e.AppendChild(_document.CreateCDataSection(_ourCDataPlaceHolderString + value));
                    else
                        e.IsEmpty = false;


                    if (path.IndexOf(separator) != -1)
                    {
                        string pathOfParent = SubstringUntillLastApearanceOf(path, separator);

                        System.Xml.XmlElement parent;
                        try
                        {
                            parent = FindElement(pathOfParent);
                        }
                        catch
                        {
                            return -4;
                        }
                        _cache.Clear();
                        XmlElement referencedElement = null;
                        int indexToFind = 1;
                        int index = 0;
                        if (referencedElementName.Contains("["))
                        {
                            var x = referencedElementName.Substring(referencedElementName.IndexOf("[") + 1);
                            x = x.Remove(x.Length - 1);
                            indexToFind = int.Parse(x);
                            referencedElementName = referencedElementName.Remove(referencedElementName.IndexOf("["));
                        }
                        foreach (var element in parent.ChildNodes)
                        {
                            var el = element as XmlElement;
                            if (el != null && CompareName(el, referencedElementName))
                            {

                                index++;
                                if (index == indexToFind)
                                {
                                    referencedElement = el;
                                    break;
                                }
                            }
                        }
                        if (referencedElement == null && !string.IsNullOrEmpty(referencedElementName))
                            return -8;
                        switch (beforeOrAfter)
                        {
                            case "A":
                            default:
                                if (referencedElement != null)
                                    parent.InsertAfter(e, referencedElement);
                                else
                                    parent.AppendChild(e);
                                break;
                            case "B":
                                if (referencedElement != null)
                                    parent.InsertBefore(e, referencedElement);
                                else
                                    parent.InsertAfter(e, null);
                                break;
                        }
                        //   parent.AppendChild(e);
                        int i = 0;
                        foreach (var element in parent.ChildNodes)
                        {
                            var el = element as XmlElement;
                            if (el != null && el.Name == e.Name)
                                i++;
                            if (el == e)
                                return i;
                        }
                        return i;

                    }
                    else if (_document.ChildNodes.Count > 0)
                        return -9;
                    else
                    {
                        var x = _document.CreateXmlDeclaration("1.0", _encoding, "no");
                        _document.AppendChild(x);
                        _document.AppendChild(e);
                        return 0;
                    }

                }
                else
                {

                    XmlElement e;
                    try
                    {
                        e = FindElement(path);
                    }
                    catch
                    {
                        return -4;
                    }

                    XmlAttribute afterWhom = null;
                    foreach (XmlAttribute att in e.Attributes)
                    {
                        switch (att.Name.CompareTo(attributeName))
                        {
                            case -1:
                                afterWhom = att;
                                break;
                            case 0:
                                return -6;
                        }

                    }


                    var a = _document.CreateAttribute(attributeName);
                    a.Value = value;
                    e.Attributes.InsertAfter(a, afterWhom);
                }
                return 0;
            }
            catch
            {
                return -101;
            }
        }
        public Number SetEncoding(Text encoding)
        {
            if (encoding == null)
                return null;
            encoding = encoding.TrimEnd();
            try
            {
                if (Readonly)
                    return -3;
                WasChanged = true;
                _encoding = encoding;
                var decleration = _document.FirstChild as XmlDeclaration;
                if (decleration != null)
                    decleration.Encoding = encoding;
                return 0;
            }
            catch
            {
                return -1;
            }
        }
        public Number Add(Text path, Text attributeName, Text value)
        {
            return Add(path, attributeName, value, "", "", true);
        }

        public Number Add(Text path, Text attributeName)
        {
            return Add(path, attributeName, "", "", "", true);
        }

        public Number Add(Text path, Text attributeName, byte[] value)
        {
            return Add(path, attributeName, TextColumn.FromByteArray(value), "", "", true);
        }

        public Number Add(Text path, Text attributeName, ByteArrayColumn value)
        {
            return Add(path, attributeName, value.ToString(), "", "", true);
        }

        public Number Add(Text path, Text attributeName, Text value, Text beforeOrAfter)
        {
            return Add(path, attributeName, value, beforeOrAfter, "", true);
        }

        public Number Add(Text path, Text attributeName, byte[] value, Text beforeOrAfter)
        {
            return Add(path, attributeName, TextColumn.FromByteArray(value), beforeOrAfter,
                             "", true);
        }

        public Number Add(Text path, Text attributeName, ByteArrayColumn value, Text beforeOrAfter)
        {
            return Add(path, attributeName, value.ToString(), beforeOrAfter, "", true);
        }

        public byte[] GetByteArray(Text path, Text attributeName)
        {
            return ENV.UserMethods.Instance.UTF8ToAnsi(Get(path, attributeName));
        }

        public byte[] GetByteArray(Text path)
        {
            return GetByteArray(path, "");
        }
        public Number Set(Text path, Text attributeName, Text value)
        {
            return Set(path, attributeName, value, true);
        }
        [NotYetImplemented]
        public Text GetEncoding()
        {
            return "";
        }

        public Text GetAlias(Text path)
        {
            return _document.DocumentElement == null ? "" : _document.DocumentElement.GetPrefixOfNamespace(path);
        }

        internal void Dispose()
        {
            _document = null;
            _cache.Clear();
            _cache = null;
        }
    }
}