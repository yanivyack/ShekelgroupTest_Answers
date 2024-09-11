using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Data;
using ENV.IO.Advanced.Internal;

namespace ENV.IO.Advanced
{
    public class Reader : ReaderInterface, IDisposable, IStringToByteArrayConverter
    {

        public virtual void Dispose()
        {
            _reader.Dispose();
        }
        System.IO.TextReader _reader;





        public Reader(System.IO.TextReader reader)
        {
            if (reader == null)
            {
                _fileEnded = true;
                reader = new System.IO.StringReader("");
            }
            _reader = reader;
        }

        bool _ignoreLineFeed = false;

        /// <summary>
        /// When set to true, the <see cref="TextSection"/>s that will be read, will be read as a single line, including all linefeed as inputs.
        /// </summary>
        public bool IgnoreLineFeed { get { return _ignoreLineFeed; } set { _ignoreLineFeed = value; } }




        /// <summary>
        /// Returns true if the file has ended
        /// </summary>
        public bool EndOfFile
        {
            get { return _fileEnded || _reader.Peek() == -1; ; }
        }

        void ReaderInterface.ReadSection(int width, int height, Action<TextControlReader> readCommand)
        {
            if (_fileEnded)
                return;
            List<string> _lines = new List<string>();
            for (int i = 0; i < height; i++)
            {
                string line = ReadLine(width);
                _lines.Add(line);
            }

            myReader r = new myReader(_lines, this);
            readCommand(r);

        }
        protected bool _fileEnded = false;
        public string ReadLine()
        {
            // return _reader.ReadLine();

            var result = new StringBuilder();

            while (true)
            {
                int i = _reader.Read();
                if (i < 0)
                {
                    if (result.Length > 0)
                    {
                        _reader.Read();
                        //        _reader.Read();
                    }

                    return result.Length == 0 ? null : result.ToString();
                }
                else if (i == 13)
                {
                    while (true)
                    {
                        var z = _reader.Peek();
                        if (z == 13)
                        {
                            _reader.Read();
                            continue;
                        }
                        if (z == 10)
                            _reader.Read();
                        break;
                    }


                    return result.ToString();

                }
                else if (i == 10)
                    return result.ToString();
                else
                    result.Append((char)i);
            }

        }
        bool _ended = false;
        string ReadLine(int length)
        {
            if (!_ignoreLineFeed)
            {
                string result = ReadLine();
                if (result == null)
                {
                    _ended = true;
                    _fileEnded = true;
                    result = "";
                }

                if (result.Length < length)
                    result = result.PadRight(length);
                if (result.Length > length)
                    result = result.Remove(length);
                return ProcessLine(result);
            }
            else
            {
                if (_ended)
                    _ended = _reader.Peek() == -1;
                StringBuilder sb = new StringBuilder(length);

                for (int i = 0; i < length; i++)
                {

                    int readedChar = -1;
                    if (!_ended)
                        readedChar = _reader.Read();
                    if (readedChar < 0)
                    {
                        sb.Append(' ');
                        _ended = true;
                    }
                    else sb.Append((char)readedChar);
                }
                return ProcessLine(sb.ToString());
            }

        }
        /// <summary>
        /// Called to precess a line when it is read from the file, and before it is parsed to the deferent control.
        /// </summary>
        /// <remarks>
        /// Rarely if ever used.
        /// Used mainly in RightToLeft oem scenarios when recieving data from DOS enviourment
        /// </remarks>
        /// <param name="originalLine">The original readen line</param>
        /// <returns>The line after processing</returns>
        protected virtual string ProcessLine(string originalLine)
        {
            return originalLine;
        }
        /// <summary>
        /// Called when ever control data is read from the file.
        /// </summary>
        /// 
        /// <remarks>
        /// Used to perform data manipulation on the control evel
        /// Rarely if ever used.
        /// Used mainly in RightToLeft oem scenarios when recieving data from DOS enviourment
        /// </remarks>
        /// <param name="originalData">the data readen into the control</param>
        /// <param name="rightToLeft">the control readen is right to left</param>
        /// <returns></returns>
        protected virtual string ProcessControlData(string originalData, bool rightToLeft)
        {
            return originalData;
        }


        void ReaderInterface.ReadSection(int width, char separator, Action<ValueProviderDelegate> setTextValue)
        {
            new mySeperatedReader(ReadLine(width), separator, this).SendValuesTo(setTextValue);
        }

        void ReaderInterface.ReadSectionDoubleSeparator(int width, char separator, Action<ValueProviderDelegate> setTextValue)
        {
            var x = ReadLine(width);
            var s = x.Split(separator);
            if (s.Length > 1)
                for (int i = 1; i < s.Length; i += 2)
                {
                    setTextValue(left => s[i]);
                }
        }

        class mySeperatedReader
        {
            System.IO.TextReader _reader;
            char _separator;
            Reader _parent;

            public mySeperatedReader(string line, char separator, Reader parent)
            {
                _reader = new System.IO.StringReader(line);
                _separator = separator;
                _parent = parent;
            }

            delegate void ReadCharsDelegate(char c, Action stop);

            void ReadChars(ReadCharsDelegate reader)
            {
                bool stop = false;
                int c;
                while (!stop && (c = _reader.Read()) != -1)
                {
                    reader((char)c, delegate { stop = true; });
                }
            }
            public void SendValuesTo(Action<ValueProviderDelegate> sendValue)
            {
                StringBuilder result = new StringBuilder();
                Action sendTheValue = delegate()
                {
                    sendValue(
                        delegate(bool rightToLeft)
                        {
                            return _parent.ProcessControlData(result.ToString(), rightToLeft);
                        });

                    result = new StringBuilder();
                };
                ReadChars(
                    delegate(char c, Action stop)
                    {
                        if (c == '"' && result.Length == 0)
                        {
                            ReadChars(
                                delegate(char c2, Action stopInnerRead)
                                {
                                    if (c2 == '"')
                                    {
                                        sendTheValue();
                                        stopInnerRead();
                                        ReadChars(
                                            delegate(char c3, Action stop3)
                                            {
                                                if (c3 == _separator)
                                                    stop3();
                                            });
                                    }
                                    else
                                    {
                                        result.Append(c2);
                                    }
                                });
                        }
                        else
                            if (c == _separator)
                            {
                                sendTheValue();

                            }


                            else
                            {
                                result.Append(c);
                            }
                    });
                if (result.Length > 0)
                    sendTheValue();
                _reader.Dispose();
            }
        }

        class myReader : TextControlReader
        {
            List<string> _lines;
            Reader _parent;

            public myReader(List<string> lines, Reader parent)
            {
                _lines = lines;
                _parent = parent;
            }

            public string Read(int line, int position, int length, bool rightToLeft)
            {
                if (line >= _lines.Count)
                    return "".PadRight(length);
                var s = _lines[line];
                string result = "";
                if (position < s.Length)
                    result = s.Substring(position, Math.Min(length, s.Length - position));
                return _parent.ProcessControlData(result, rightToLeft);

            }
            public byte[] GetBytes(string s)
            {
                return  _parent._GetBytes(s);
            }
        }

        protected virtual byte[] _GetBytes(string s)
        {
            return ByteArrayColumn.ToAnsiByteArray(s);
        }
        byte[] IStringToByteArrayConverter.GetBytes(string s)
        {
            return _GetBytes(s);
        }
    }
}
