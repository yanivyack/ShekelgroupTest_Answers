using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ENV.Data.Storage;
using ENV.IO.Advanced;
using Firefly.Box;


namespace ENV.IO
{
    public class FileReader : Reader, IOByName
    {
        #region Constructors
        internal string _name = null;
        public string Name { set { _name = PathDecoder.DecodePath(value); } get { return _name; } }

        static System.IO.TextReader TextReaderFromFileName(string fileName, System.Text.Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;
            var fn = PathDecoder.DecodePath(fileName);
            using (Utilities.Profiler.StartContext("Open File Reader for " + fn + " (" + fileName + ")"))
            {
                try
                {
                    return new System.IO.StreamReader(fn, encoding);
                }
                catch (Exception e)
                {
                    ENV.ErrorLog.WriteToLogFile(e, "");
                    return null;
                }
            }
        }
        public FileReader(string fileName)
            : this(fileName, ENV.LocalizationInfo.Current.InnerEncoding)
        {
        }

        public FileReader(string fileName, System.Text.Encoding encoding)
            : base(TextReaderFromFileName(fileName, encoding))
        {

        }
        public FileReader(MemoryStream stream)
            : base(new StreamReader(stream, ENV.LocalizationInfo.Current.InnerEncoding))
        {
        }

        public FileReader(System.IO.TextReader reader)
            : base(reader)
        {
        }

        public FileReader()
            : base(new System.IO.StringReader(""))
        { }
        #endregion

        #region Hebrew OEM issues
        bool _performRightToLeftManipulations = false;
        public bool PerformRightToLeftManipulations
        {
            get { return _performRightToLeftManipulations; }
            set { _performRightToLeftManipulations = value; }
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

        public bool RightToLeftFlip { get; set; }

        static UserMethods u = new UserMethods();
        protected override string ProcessLine(string originalLine)
        {
            string line = originalLine;
            if (_rightToLeftFlipLine && _performRightToLeftManipulations)
                line = u.Visual(((Text)line).Reverse(), true);

            return line;
        }

        protected override string ProcessControlData(string originalData, bool rightToLeft)
        {
            string data = originalData;
            if (RightToLeftFlip && rightToLeft)
                data = ((Text)data).Reverse();
            if (_oem)
                if (_rightToLeftFlipLine)
                    data = HebrewOemTextStorage.Encode(data);
                else
                    data = HebrewOemTextStorage.Decode(data);


            if (_performRightToLeftManipulations)
            {
                if (rightToLeft)
                    data = u.Visual(((Text)data).Reverse(), true);
            }
            return data;
        }
        #endregion

        public static FileReader FindIOByName(string baseStreamName)
        {
            return IOFinder.FindByName<FileReader>(baseStreamName);
        }

        int _usagesByName = 0;
        public override void Dispose()
        {
            if (_usagesByName == 0)
                base.Dispose();
            else
                _usagesByName--;

        }

        void IOByName.AddToUsageCount()
        {
            _usagesByName++;
        }
    }
}

