using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Firefly.Box.UI;

namespace ENV.IO.Html
{
    class HtmlContentWriter
    {
        bool _inited = false;

        string _firstFormName;
        string _background = "";
        Color _bgColor, _textColor;
        myWriter _writer;
        public HtmlContentWriter(Action<string> write)
        {
            _writer = new myWriter(write, this);

        }
        HtmlSection _currentSection = null;
        myWriter.GridManager _currentGrid;
        public void Write(HtmlSection htmlSection)
        {
            if (!_inited)
            {
                _firstFormName = htmlSection.Text;
                _inited = true;
                if (htmlSection.ColorScheme != null)
                {
                    _bgColor = htmlSection.ColorScheme.BackColor;
                    _textColor = htmlSection.ColorScheme.ForeColor;
                }
                _writer.WriteOpenTag("HTML");
                _writer.WriteLine("");
                _writer.WriteOpenTag("HEAD");
                _writer.WriteOpenTag("META", "name", "generator", "content", "Migrated from magic to .Net using Firefly's technology, www.fireflymigration.com");
                _writer.WriteWithinTags("TITLE", _firstFormName);
                if (!string.IsNullOrEmpty(htmlSection.HeaderFile))
                {
                    try
                    {
                        var header = System.IO.File.ReadAllText(PathDecoder.DecodePath(htmlSection.HeaderFile).Trim(),
                                                   ENV.LocalizationInfo.Current.OuterEncoding);
                        if (!string.IsNullOrEmpty(header))
                        {
                            using (_writer.DisableNewLine())
                            { _writer.WriteLine(header); }
                        }
                    }
                    catch
                    {
                    }
                }
                _writer.WriteLine();
                _writer.WriteCloseTag("HEAD");
                _writer.WriteLine();
                _writer.WriteOpenTag("BODY", "background", _background, "bgcolor", _bgColor, "text", _textColor);
                _writer.WriteLine();
            }
            if (_currentGrid != null)
            {
                if (_currentSection == htmlSection)
                {
                    _currentGrid.WriteGridLine();
                    return;
                }
                else
                {
                    _currentGrid.ConcludeGridWriting();
                    _currentGrid = null;
                }
            }

            _currentSection = htmlSection;
            htmlSection.DoWrite(_writer);
            if (_currentGrid != null)
                _currentGrid.FinishFirstGridFormWriting();

        }


        public void Dispose()
        {

            _writer.WriteCloseTag("BODY");
            _writer.WriteLine();
            _writer.WriteCloseTag("HTML");

        }

        public class myWriter
        {
            Action<string> _write;
            event Action OnWrite;
            HtmlContentWriter _parent;
            public myWriter(Action<string> write, HtmlContentWriter parent)
            {
                _parent = parent;
                _write = x =>
                {
                    write(x);
                    if (x.Length > 0)
                        if (OnWrite != null)
                            OnWrite();
                };




            }
            internal class GridManager
            {
                Action<myWriter> _writeGridLine;
                myWriter _parent;
                Action<string> _tempWriter;
                StringBuilder _sb = new StringBuilder();
                public GridManager(Action<myWriter> writeGridLine, myWriter parent)
                {
                    _writeGridLine = writeGridLine;
                    _parent = parent;
                    _tempWriter = _parent._write;
                    _parent._write = x =>
                    {
                        if (x.Length > 0)
                            if (_parent.OnWrite != null)
                                _parent.OnWrite();
                        _sb.Append(x);
                    };
                }

                public void WriteGridLine()
                {
                    _writeGridLine(_parent);
                }
                public void ConcludeGridWriting()
                {
                    _parent._write(_sb.ToString());
                    _parent._parent._currentGrid = null;
                }

                public void FinishFirstGridFormWriting()
                {
                    _parent._write = _tempWriter;
                }
            }


            public void SetGridWriter(Action<myWriter> writeGridLine)
            {

                _parent._currentGrid = new GridManager(writeGridLine, this);
                _parent._currentGrid.WriteGridLine();
            }


            public void WriteOpenTag(string name, params object[] attributes)
            {
                var sb = new StringBuilder();
                sb.Append("<");
                sb.Append(name);
                string att = null;
                foreach (var attribute in attributes)
                {
                    if (att == null)
                    {
                        att = (string)attribute;
                    }
                    else
                    {
                        var s = attribute as string;
                        if (s != null)
                        {
                            s = "\"" + s + "\"";
                        }
                        else
                        {
                            if (attribute is Color)
                                s = '"' + GenerateColorString((Color)attribute) + '"';
                            if (attribute is Enum)
                                s = attribute.ToString().ToLower();
                        }
                        if (s == null)
                            s = attribute.ToString();
                        sb.Append(" ");
                        sb.Append(att);
                        sb.Append("=");
                        sb.Append(s);
                        att = null;
                    }

                }
                sb.Append(">");
                WriteLine(sb.ToString());
            }

            public string GenerateColorString(Color attribute)
            {
                return "#" + attribute.ToArgb().ToString("X").PadRight(8, '0').Substring(2);
            }

            bool _writeNewLine = true;


            public IDisposable DisableNewLine()
            {
                return new myNewLineDisposable(this);
            }
            class myNewLineDisposable : IDisposable
            {
                myWriter _parent;
                bool _previousValue;

                public myNewLineDisposable(myWriter parent)
                {
                    _parent = parent;
                    _previousValue = _parent._writeNewLine;

                    _parent._writeNewLine = false;
                }

                public void Dispose()
                {
                    _parent._writeNewLine = _previousValue;
                }
            }
            bool _lineIsEmpty = true;
            public void WriteLine(string what)
            {
                if (what == null)
                    return;
                if (_lineIsEmpty)
                    _write(new string(' ', 4 * _indent));
                _write(what);
                _lineIsEmpty = false;
                if (_writeNewLine)
                {
                    _write("\r\n");
                    _lineIsEmpty = true;
                }

            }

            public void WriteWithinTags(string title, string value)
            {
                WriteLine(string.Format("<{0}>{1}</{0}>", title, value));
            }

            public void WriteCloseTag(string head)
            {
                WriteLine("</" + head + ">");
            }

            internal void WriteLine()
            {
                WriteLine("");
            }


            public void WriteNewLine()
            {
                if (_writeNewLine)
                    WriteLine(" ");
            }

            public bool ShouldSerializeTextColor(Color foreColor)
            {
                return _parent._textColor.ToArgb() != foreColor.ToArgb();
            }

            int _indent = 0;
            public void Indent()
            {
                _indent++;
            }

            public void UnIndent()
            {
                _indent--;
            }
            internal ContentListener GetContentListener()
            {
                return new ContentListener(this);
            }
            internal class ContentListener : IDisposable
            {
                public bool ContentWasWriten;
                myWriter _parent;
                public ContentListener(myWriter parent)
                {
                    _parent = parent;
                    _parent.OnWrite += _parent_OnWrite;

                }

                void _parent_OnWrite()
                {
                    ContentWasWriten = true;
                }

                public void Dispose()
                {
                    _parent.OnWrite -= _parent_OnWrite;
                }
            }

            public void WriteEmptySpace()
            {
                _write("&nbsp;");

            }


            public void PrepareFont(FontScheme fontScheme, List<object> fontAttributes, Action<Action> addToAfterTextWrite)
            {

            }

            public void WriteText(string text, ColorScheme colorScheme, FontScheme fontScheme)
            {
                var writer = this;
                using (writer.DisableNewLine())
                {
                    var after = new Stack<Action>();
                    var fontAttributes = new List<object>();



                    bool forceBold = false;
                    bool forceItalic = false;
                    bool foundFont = false;
                    if (fontScheme != null)
                    {
                        int i = 0;
                        Action IfoundTheFont = () =>
                        {
                            if (foundFont)
                                return;
                            foundFont = true;
                            if (i > 0)
                            {
                                var j = i;
                                WriteOpenTag("H" + j);
                                after.Push(() => WriteCloseTag("H" + j));
                            }

                        };
                        {
                            i = 0;
                            foreach (var scheme in new[]
                                                       {
                                                           _parent._currentSection.DefaultFontScheme,
                                                           _parent._currentSection.H1FontScheme,
                                                           _parent._currentSection.H2FontScheme,
                                                           _parent._currentSection.H3FontScheme,
                                                           _parent._currentSection.H4FontScheme,
                                                           _parent._currentSection.H5FontScheme,
                                                           _parent._currentSection.H6FontScheme,
                                                       })
                            {
                                if (scheme != null && fontScheme.GetType() == scheme.GetType())
                                {
                                    IfoundTheFont();
                                }
                                i++;
                            }
                            i = 0;
                            foreach (var scheme in new[]
                                                       {
                                                           _parent._currentSection.BoldFontScheme,
                                                           _parent._currentSection.H1BoldFontScheme,
                                                           _parent._currentSection.H2BoldFontScheme,
                                                           _parent._currentSection.H3BoldFontScheme,
                                                           _parent._currentSection.H4BoldFontScheme,
                                                           _parent._currentSection.H5BoldFontScheme,
                                                           _parent._currentSection.H6BoldFontScheme,
                                                           
                                                       })
                            {
                                if (scheme != null && fontScheme.GetType() == scheme.GetType())
                                {
                                    IfoundTheFont();
                                    forceBold = true;
                                }
                                i++;
                            }
                            i = 0;
                            foreach (var scheme in new[]
                                                       {
                                                           _parent._currentSection.ItalicFontScheme,
                                                           _parent._currentSection.H1ItalicFontScheme,
                                                           _parent._currentSection.H2ItalicFontScheme,
                                                           _parent._currentSection.H3ItalicFontScheme,
                                                           _parent._currentSection.H4ItalicFontScheme,
                                                           _parent._currentSection.H5ItalicFontScheme,
                                                           _parent._currentSection.H6ItalicFontScheme,
                                                         
                                                           
                                                       })
                            {
                                if (scheme != null && fontScheme.GetType() == scheme.GetType())
                                {
                                    IfoundTheFont();
                                    forceItalic = true;
                                }
                                i++;
                            }
                            i = 0;
                            foreach (var scheme in new[]
                                                       {
                                                           _parent._currentSection.BoldItalicFontScheme,
                                                           _parent._currentSection.H1BoldItalicFontScheme,
                                                           _parent._currentSection.H2BoldItalicFontScheme,
                                                           _parent._currentSection.H3BoldItalicFontScheme,
                                                           _parent._currentSection.H4BoldItalicFontScheme,
                                                           _parent._currentSection.H5BoldItalicFontScheme,
                                                           _parent._currentSection.H6BoldItalicFontScheme,
                                                           
                                                       })
                            {
                                if (scheme != null && fontScheme.GetType() == scheme.GetType())
                                {
                                    IfoundTheFont();
                                    forceItalic = true;
                                    forceBold = true;
                                }
                                i++;
                            }
                        }


                        if (!foundFont)
                        {
                            var size = fontScheme.Font.SizeInPoints;
                            var result = 10;
                            if (size <= 8)
                                result = 1;
                            else if (size <= 10)
                                result = 2;
                            else if (size <= 12)
                                result = 3;
                            else result = 4;
                            fontAttributes.Add("size");
                            fontAttributes.Add(result);
                        }

                    }

                    if (colorScheme != null && writer.ShouldSerializeTextColor(colorScheme.ForeColor))
                    {
                        fontAttributes.Add("color");
                        fontAttributes.Add(new HtmlAttributeValue(writer.GenerateColorString(colorScheme.ForeColor)));
                    }
                    if (fontAttributes.Count > 0)
                    {
                        writer.WriteOpenTag("FONT", fontAttributes.ToArray());
                        after.Push(() => writer.WriteCloseTag("FONT"));
                    }
                    if (fontScheme != null)
                    {
                        if (fontScheme.Font.Bold && !foundFont || forceBold)
                        {
                            writer.WriteOpenTag("B");
                            after.Push(() => writer.WriteCloseTag("B"));
                        }
                        if (fontScheme.Font.Italic && !foundFont || forceItalic)
                        {
                            writer.WriteOpenTag("I");
                            after.Push(() => writer.WriteCloseTag("I"));
                        }
                    }
                    writer.WriteLine(text);
                    foreach (var action in after)
                    {
                        action();
                    }
                }
                writer.WriteNewLine();
            }

            public bool ShouldSerializeBackColor(ColorScheme colorScheme)
            {
                if (colorScheme == null)
                    return false;
                return colorScheme.BackColor.ToArgb() != _parent._bgColor.ToArgb();
            }
            class ColorContextClass : IDisposable
            {
                Color _previous;
                myWriter _parent;

                public ColorContextClass(myWriter parent)
                {
                    _parent = parent;
                    _previous = _parent._parent._bgColor;

                }


                public void Dispose()
                {
                    _parent._parent._bgColor = _previous;
                }
            }
            public IDisposable ColorContext(ColorScheme colorScheme)
            {
                var c = _parent._bgColor;
                if (colorScheme != null)
                    c = colorScheme.BackColor;
                var result = new ColorContextClass(this);
                _parent._bgColor = c;
                return result;
            }
        }
    }

    class HtmlAttributeValue
    {
        string _what;

        public HtmlAttributeValue(string what)
        {
            _what = what;
        }
        public override string ToString()
        {
            return _what;
        }
    }
}
