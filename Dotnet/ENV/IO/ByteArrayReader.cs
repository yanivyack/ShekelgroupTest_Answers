using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using ENV.Data;
using ENV.Data.Storage;
using ENV.IO.Advanced;
using Firefly.Box;

namespace ENV.IO
{
    public class ByteArrayReader : Reader, IXMLIO, IOByName
    {

        class myTextReader : System.IO.TextReader
        {
            ByteArrayColumn _column;
            string _data = "";
            int _position = 0;
            Encoding _encoding;
            public myTextReader(ByteArrayColumn column, Encoding encoding)
            {
                _column = column;
                _encoding = encoding;
                VerifyData();
                _column.ValueChanged += _column_ValueChanged;

            }

            private void _column_ValueChanged(Firefly.Box.Data.Advanced.ValueChangedEventArgs<byte[]> e)
            {
                VerifyData();
            }

            byte[] _previouslyKnownData;

            public Action OnColumnChange = () => { };

            void VerifyData()
            {
                if (_previouslyKnownData == _column.Value)
                {
                    return;
                }
                _data = "";
                if (_column.Value != null)
                {
                    if (_encoding == null)
                        _data = _column.ToString();
                    else
                        _data = _encoding.GetString(_column.Value);

                }
                _previouslyKnownData = _column.Value;
                OnColumnChange();
            }

            public override int Read()
            {
                VerifyData();
                if (_position >= _data.Length)
                {
                    if (_position > 0)
                        _position++;
                    return -1;
                }

                return _data[_position++];
            }
            public override int Peek()
            {
                VerifyData();
                if (_position  >= _data.Length)
                    return -1;
                return _data[_position];
            }
            public override string ReadLine()
            {
                VerifyData();
                if (_position >= _data.Length)
                    return null;
                StringBuilder result = new StringBuilder();
                int i;
                while ((i = Read()) >= 0)
                {
                    switch (i)
                    {
                        case '\r':
                        case '\n':
                            {
                                if (Peek() == '\r' || Peek() == '\n')
                                    Read();
                                return result.ToString();
                            }
                        default:
                            result.Append((char)i);
                            break;
                    }
                }
                _position += 2;
                return result.ToString();

            }
            protected override void Dispose(bool disposing)
            {
                _column.ValueChanged -= _column_ValueChanged;
                base.Dispose(disposing);
            }

        }

        ByteArrayColumn _column;
        public ByteArrayReader(ByteArrayColumn source, Encoding encoding = null)
            : this(new myTextReader(source, encoding))
        {
            _column = source;
        }
        TextReader _reader;
        ByteArrayReader(myTextReader r) : base(r)
        {
            _reader = r;
            r.OnColumnChange += () => base._fileEnded = false;
        }
        #region Hebrew OEM issues
        bool _rightToLeftTransform = false;
        public bool RightToLeftTransform
        {
            get { return _rightToLeftTransform; }
            set { _rightToLeftTransform = value; }
        }

        bool _rightToLeftFlipLine = false;
        public bool RightToLeftFlipLine
        {
            get { return _rightToLeftFlipLine; }
            set { _rightToLeftFlipLine = value; }
        }

        bool _oem = false;
        public bool Oem
        {
            get { return _oem; }
            set { _oem = value; }
        }

        static UserMethods u = new UserMethods();

        protected override string ProcessLine(string originalLine)
        {
            string line = originalLine;
            if (_rightToLeftFlipLine && _rightToLeftTransform)
                line = u.Visual(((Text)line).Reverse(), true);

            return line;
        }

        protected override string ProcessControlData(string originalData, bool rightToLeft)
        {
            string data = originalData;
            if (_oem)
                if (_rightToLeftFlipLine)
                    data = HebrewOemTextStorage.Encode(data);
                else
                    data = HebrewOemTextStorage.Decode(data);


            if (_rightToLeftTransform)
            {
                if (rightToLeft)
                    data = u.Visual(((Text)data).Reverse(), true);
            }
            return data;
        }
        #endregion

        public Bool Contains(Text path, Text attrubuteName)
        {
            return Xml.Contains(path, attrubuteName);
        }

        public Bool Contains(Text path)
        {
            return Xml.Contains(path);
        }

        public Number Count(Text path)
        {
            return Xml.Count(path);
        }

        public Number Search(Text path, Text elementName, Bool isAttribute, Text childElementName, Text value)
        {
            return Xml.Search(path, elementName, isAttribute, childElementName, value);
        }
        public Number Search(Text path, Text elementName, Number isAttribute, Text childElementName, Text value)
        {
            return Xml.Search(path, elementName, isAttribute, childElementName, value);
        }

        public Text Get(Text path, Text attributeName, bool returnNullIfNotFound)
        {
            return Xml.Get(path, attributeName, returnNullIfNotFound);
        }
        public Text Get(Text path, Text attributeName)
        {
            return Xml.Get(path, attributeName);
        }

        public Text Get(Text path)
        {
            return Xml.Get(path);
        }

        public Text GetAlias(Text path)
        {
            return Xml.GetAlias(path);
        }

        public byte[] GetByteArray(Text path, Text attributeName)
        {
            return Xml.GetByteArray(path, attributeName);
        }

        public byte[] GetByteArray(Text path)
        {
            return Xml.GetByteArray(path);
        }

        public Text GetEncoding()
        {
            return Xml.GetEncoding();
        }

        public XmlDocument XmlDocument
        {
            get { return Xml.XmlDocument; }
        }

        public XmlHelper Xml
        {
            get
            {
                if (_xmlHelper == null)
                {
                    _xmlHelper = new XmlHelper();
                    if (_column.Value != null)
                    {
                        if (_column.ContentType == ByteArrayColumnContentType.Unicode)
                            _xmlHelper.LoadXml(_column.ToString());
                        else
                            _xmlHelper.LoadXml(_column.Value);
                    }
                }
                return _xmlHelper;
            }
        }

        XmlHelper _xmlHelper;


        public static ByteArrayReader FindIOByName(string baseStreamName)
        {
            return IOFinder.FindByName<ByteArrayReader>(baseStreamName);
        }

        int _usagesByName = 0;
        public override void Dispose()
        {
            if (_usagesByName == 0)
            {
                base.Dispose();
                if (_xmlHelper != null)
                    _xmlHelper.Dispose();
            }
            else
                _usagesByName--;

        }

        internal string _name = null;
        public string Name { set { _name = PathDecoder.DecodePath(value); } get { return _name; } }

        void IOByName.AddToUsageCount()
        {
            _usagesByName++;
        }
    }
}
