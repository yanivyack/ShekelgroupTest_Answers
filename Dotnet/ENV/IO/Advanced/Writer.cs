using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ENV.IO.Advanced.Internal;
using ENV.Printing;

namespace ENV.IO.Advanced
{
    public abstract class Writer : SectionWriter, IDisposable
    {


        System.IO.TextWriter _theWriter;

        protected virtual void OnOpen()
        {
            _opened = true;
        }
        protected virtual void OnWrite(string text)
        {
            _theWriter.Write(text);
        }
        public void Open()
        {
            if (_opened)
                return;
            _opened = true;
            OnOpen();
        }




        Func<Printer> _printer;


        public Writer(System.IO.TextWriter writer)
        {
            _theWriter = writer;
        }

        protected Writer()
        {
            _theWriter = new System.IO.StringWriter();
            _printer = () =>
            {
                var result = GetPrinter();
                _printer = () => result;
                return result;
            };
        }
        internal virtual Printer GetPrinter()
        {
            return new Printer();
        }
        bool _opened = false;
        public static string DefaultNewLineString = "\r\n";
        string _newLineString = DefaultNewLineString;
        public string NewLineString { get { return _newLineString; } set { _newLineString = value; } }


        /// <summary>
        /// Determines is a new line is automatically added after every time a <see cref="TextSection"/> is written.
        /// </summary>
        public bool AutoNewLine
        {
            get { return _newLineForEveryWrite; }
            set { _newLineForEveryWrite = value; }
        }
        /// <summary>
        /// Called before a linbe is written to a file
        /// </summary>
        /// <remarks>
        /// Rarely if ever used.
        /// Used mainly in RightToLeft oem scenarios when providing data to DOS enviourment
        /// </remarks>
        /// <param name="originalLine">The line</param>
        /// <returns>The processed line</returns>
        protected virtual string ProcessLine(string originalLine, int lineWidth, bool donotTrim)
        {
            if (donotTrim)
                return originalLine;
            return originalLine.TrimEnd(' ');
        }

        /// <summary>
        /// Called before control data is written to a file
        /// </summary>
        /// <remarks>
        /// Rarely if ever used.
        /// Used mainly in RightToLeft oem scenarios when providing data to DOS enviourment
        /// </remarks>
        /// <param name="originalData">The control data</param>
        /// <param name="rightToLeft">Is the control rightToLeft</param>
        /// <returns>The data to write to the file</returns>
        protected virtual string ProcessControlData(string originalData, bool rightToLeft, bool hebrewDosCompatibleEditing)
        {
            return originalData;
        }

        bool _newLineForEveryWrite = true;


        public virtual void Dispose()
        {
            _theWriter.Dispose();
        }


        void SectionWriter.WriteSection(int width, int height, TextPrintingStyle style, Action<TextControlWriter> writeCommand, Action newPageStatedDueToLackOfSpace)
        {
            WriteSection(width, height, style, writeCommand, newPageStatedDueToLackOfSpace);
        }
        internal virtual void WriteSection(int width, int height, TextPrintingStyle style, Action<TextControlWriter> writeCommand, Action newPageStatedDueToLackOfSpace)
        {
            _ActualWriteSection(width, height, style, writeCommand, newPageStatedDueToLackOfSpace);
        }
        internal void _ActualWriteSection(int width, int height, TextPrintingStyle style, Action<TextControlWriter> writeCommand, Action newPageStatedDueToLackOfSpace)
        {
            var writer = new myNewSectionWriter(width, height, style, (originalData, rightToLeft, hebrewDosCompatibleEditing) => ProcessControlData(originalData, rightToLeft, hebrewDosCompatibleEditing), ProcessLine, _printer(),!AutoNewLine);
            writeCommand(writer);
            writer.WriteTo(this);
        }

        int _lines = 0;
        void InternalWrite(string what)
        {
            Open();
            if (!_opened)
                return;
            _lines++;
            if (_newLineForEveryWrite)
                OnWrite(what + NewLineString);
            else
                OnWrite(what);
        }
        class myNewSectionWriter : TextControlWriter
        {
            List<PrintableTextLine> _lines = new List<PrintableTextLine>();
            ProcessControlData _processField;
            ProcessLine _processLine;
            Printer _printer;
            int _width;
            Type _sectionStyleType;
            bool _donotBreakMultiline;
            public myNewSectionWriter(int width, int height, TextPrintingStyle sectionStyle, ProcessControlData processField, ProcessLine processLine, Printer printer,bool donotBreakMultiline)
            {
                _donotBreakMultiline = donotBreakMultiline;
                _width = width;
                _printer = printer;
                _processField = processField;
                _processLine = processLine;
                if (sectionStyle != null)
                    _sectionStyleType = sectionStyle.GetType();
                for (int i = 0; i < height; i++)
                {
                    _lines.Add(new PrintableTextLine(sectionStyle, width, _printer));
                }
            }
            class PrintableTextLine
            {
                TextPrintingStyle _sectionStyle;
                List<PrintableTextInLine> _textsInLine = new List<PrintableTextInLine>();
                int _width;
                Printer _printer;
                bool _sortRequired = false;
                public PrintableTextLine(TextPrintingStyle sectionStyle, int width, Printer printer)
                {
                    _sectionStyle = sectionStyle;
                    _width = width;
                    _printer = printer;
                }

                public string ToString(bool autoNewLine, bool flip,out bool donotTrim)
                {
                    donotTrim = false;
                    //   FillSpacesInLine(autoNewLine, flip);
                    if (_sortRequired)
                        _textsInLine.Sort();
                    var sb = new StringBuilder();
                    TextPrintingStyle lastControlStyle = TextPrintingStyle.Default;
                    if (_textsInLine.Count > 0)
                    {
                        var lastControlRight = 0;
                        
                        for (int i = 0; i < _textsInLine.Count; i++)
                        {
                            var control = _textsInLine[i];
                            if (control.Left > lastControlRight)
                            {
                                _sectionStyle.Decorate(sb, _printer, new string(' ', control.Left - lastControlRight), false, false);
                            }
                            control.AppendTo(sb);
                            lastControlRight = control.Right;
                            lastControlStyle = control._style;
                        }
                        if (lastControlRight < _width)

                        {
                            var length = autoNewLine && !flip ? 0 : _width - lastControlRight;
                            _sectionStyle.Decorate(sb, _printer, new string(' ', length), false, false);
                        }
                    }
                    else {
                        _sectionStyle.Decorate(sb, _printer, string.Empty, false, false);
                    }

                    var r = sb.ToString();
                    if (!autoNewLine)
                        r = r.PadRight(_width, ' ');
                    Type ct = null;
                    if (lastControlStyle != null)
                        ct = lastControlStyle.GetType();
                    Type st = null;
                    if (_sectionStyle != null)
                        st = _sectionStyle.GetType();
                    if (st != ct)
                        donotTrim = true;
                    return r;
                }

              
                int _lastRight = -1;

                public void AddTextInLine(string value, int left, int length, TextPrintingStyle style)
                {

                    var newTextInLine = new PrintableTextInLine(value, left, length, style, _printer);
                    if (left < _lastRight)
                    {
                        _sortRequired = true;
                        foreach (var c in _textsInLine.ToArray())
                        {
                            c.ResolveConflict(newTextInLine, _textsInLine);
                        }
                    }
                    if (left+length>_lastRight)
                        _lastRight = left + length;
                    if (newTextInLine.Left < newTextInLine.Right)
                        _textsInLine.Add(newTextInLine);
                }

                class PrintableTextInLine : IComparable<PrintableTextInLine>
                {
                    public readonly TextPrintingStyle _style;
                    string _value;
                    public int Left;
                    public int Right;

                    int _length;
                    Printer _printer;

                    public PrintableTextInLine(string value, int left, int length, TextPrintingStyle style, Printer printer)
                    {
                        _value = value;
                        Left = left;
                        _length = length;

                        Right = Left + _length;

                        _style = style;
                        _printer = printer;
                    }

                    public void ResolveConflict(PrintableTextInLine newTextInLine, List<PrintableTextInLine> textsInLine)
                    {
                        if (newTextInLine.Left > Right || newTextInLine.Right < Left)
                            return;
                        if (newTextInLine._style.Equals(_style))
                        {
                            if (newTextInLine.Right == Left)
                            {
                                newTextInLine.RemoveSuffix();
                                RemovePrefix();
                            }
                            else if (Right == newTextInLine.Left)
                            {
                                RemoveSuffix();
                                newTextInLine.RemovePrefix();
                            }
                        }

                        //if the new control surround me
                        if (newTextInLine.Left <= Left && newTextInLine.Right >= Right)
                        {
                            textsInLine.Remove(this);
                        }
                        //if the new control is over my beginning
                        else if (newTextInLine.Left <= Left && newTextInLine.Right > Left)
                        {
                            var offset = newTextInLine.Right - Left;
                            _length -= offset;
                            _value = _value.Substring(offset);
                            Left = newTextInLine.Right;
                        }
                        //if the new control is over my ending
                        else if (newTextInLine.Left < Right && newTextInLine.Right >= Right)
                        {
                            _length -= Right - newTextInLine.Left;
                            _value = _value.Remove(_length);
                            Right = newTextInLine.Left;
                        }
                        //if the new control is contained in me, split me
                        else if (Left < newTextInLine.Left && Right > newTextInLine.Right)
                        {
                            if (newTextInLine._value.StartsWith(" "))
                            {
                                var x = newTextInLine._value.TrimStart();
                                var d = newTextInLine._value.Length - x.Length;
                                newTextInLine.Left += d;
                                newTextInLine._length -= d;
                                newTextInLine._value = x;
                            }
                            if (Left < newTextInLine.Left && Right > newTextInLine.Right && newTextInLine._length > 0)
                            {



                                var firstLength = _length - (Right - newTextInLine.Left);
                            
                                var secondLength = _length - (newTextInLine.Right - Left);
                                var firstTextInLine = new PrintableTextInLine(_value.Substring(0, firstLength), Left,
                                    firstLength, _style, _printer);
                                var secondTextInLine =
                                    new PrintableTextInLine(_value.Substring(newTextInLine.Right - Left),
                                        newTextInLine.Right, secondLength, _style, _printer);
                                textsInLine.Remove(this);
                                textsInLine.Add(firstTextInLine);
                                textsInLine.Add(secondTextInLine);
                            }
                        }
                    }

                    bool _ignorePrefix = false;
                    bool _ignoreSuffix = false;
                    void RemovePrefix()
                    {
                        _ignorePrefix = true;
                    }

                    void RemoveSuffix()
                    {
                        _ignoreSuffix = true;
                    }

                    public int CompareTo(PrintableTextInLine other)
                    {
                        return Left.CompareTo(other.Left);
                    }

                    public void AppendTo(StringBuilder sb)
                    {
                        if (_length <= 0)
                            return;
                        if (_value.Length != _length)
                            _value = _value.PadRight(_length).Substring(0, _length);
                        _style.Decorate(sb, _printer, _value, _ignorePrefix, _ignoreSuffix);
                    }
                }
            }

            public void Write(string text, int lineNumber, int position, int length, TextPrintingStyle style, ContentAlignment alignment)
            {
                if (lineNumber >= _lines.Count)
                    return;
                _lines[lineNumber].AddTextInLine(text, position, length, style);
            }

            public string ProcessColumnData(string s, bool rightToLeft, bool hebrewDosCompatibleEditing)
            {
                return _processField(s, rightToLeft, hebrewDosCompatibleEditing);
            }
            public bool DoNotTrimToWidth()
            {
                return false;
            }

            public void WriteTo(Writer io)
            {
                bool flip = false;
                ENV.Printing.TextPrinterWriter tp = io as ENV.Printing.TextPrinterWriter;
                if (tp != null)
                    flip = tp.RightToLeftFlipLine;
                foreach (var line in _lines)
                {
                    bool donotTrim;
                    io.InternalWrite(_processLine(line.ToString(io.AutoNewLine, flip,out donotTrim), _width, donotTrim));
                }
            }

            public bool DoNotBreakMultiline()
            {
                return _donotBreakMultiline;
            }
        }














        /// <summary>
        /// Writes text directly to the file. 
        /// </summary>
        /// <remarks>
        /// Uses the <see cref="String.Format(string,object[])"/> method to format the data.
        /// </remarks>
        /// <param name="text">The text to write</param>
        /// <param name="formatArgs">The arguments for the <see cref="String.Format(string,object[])"/> method</param>
        public void WriteLine(string text, params object[] formatArgs)
        {

            string newText = formatArgs.Length > 0 ? string.Format(text, formatArgs) : text;

            ((SectionWriter)this).WriteSection(newText.Length, 1, TextPrintingStyle.Default,
                                               delegate (TextControlWriter obj)
                                               {
                                                   obj.Write(newText, 0, 0, newText.Length, TextPrintingStyle.Default, ContentAlignment.TopLeft);
                                               }, delegate { });
        }


        /// <summary>
        /// Returns the number of lines written to the file
        /// </summary>
        public virtual int LineNumber
        {
            get { return _lines; }
        }
    }
    internal delegate string ProcessControlData(string s, bool rightToLeft, bool hebrewDosCompatibleEditing);

    internal delegate string ProcessLine(string s, int lineWidth, bool donotTrim);
}
