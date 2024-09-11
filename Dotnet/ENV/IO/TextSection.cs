using System;
using System.Collections.Generic;
using System.Drawing;
using ENV.IO.Advanced;
using ENV.IO.Advanced.Internal;
using ENV.Printing;
using Firefly.Box.Advanced;
using Firefly.Box.UI.Advanced;
using System.ComponentModel;
using Firefly.Box.Data.Advanced;

namespace ENV.IO
{
    public class TextSection : ENV.Printing.ReportSection
    {
        ITask _task;
        UserMethods u;
        public TextSection()
        {
            TextPrintingStyle = TextPrintingStyle.Default;
            WidthInChars = 100;
            HeightInChars = 1;
            Font = new Font("Courier New", 12);

        }


        [Obsolete("Use WidthInChars instead, will not work if set!!!")]
        public new int Width { get { return base.Width; } set { base.Width = value; } }

        [Obsolete("Use TopInChars instead, will not work if set!!!")]
        public new int Top { get { return base.Top; } set { base.Top = value; } }
        [Obsolete("Use LeftInChars instead, will not work if set!!!")]
        public new int Left { get { return base.Left; } set { base.Left = value; } }
        internal void NewPageStarted(SectionWriter writer)
        {
            if (PageHeader)
                WriteTo(writer);
        }
        protected override bool IsControlTypeSupported(Type controlType)
        {
            return typeof(ENV.IO.Advanced.TextControlBase).IsAssignableFrom(controlType);
        }
        public TextPrintingStyle TextPrintingStyle { get; set; }

        bool _printingNewPage = false;

        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int WidthInChars
        {
            set { SizeInChars = new Size(value, SizeInChars.Height); }
            get { return SizeInChars.Width; }
        }

        public int HeightInChars
        {
            set { SizeInChars = new Size(SizeInChars.Width, value); }
            get { return SizeInChars.Height; }
        }
        public override Type GetControlTypeForWizard(ColumnBase column)
        {
            return DefaultTextBoxType;
        }

        public void WriteTo(SectionWriter io)
        {

            using (ENV.Utilities.Profiler.StartContext("Write Text Section: " + Name))
            {
                Action doWrite =
                () =>
                {

                    io.WriteSection(WidthInChars, HeightInChars, TextPrintingStyle,
                                      delegate (TextControlWriter w)
                                      {
                                          ApplyBinding();
                                          for (int i = Controls.Count - 1; i >= 0; i--)
                                          {
                                              (Controls[i] as TextControl).WriteTo(w, WidthInChars, HeightInChars);

                                          }
                                      }, () =>
                                      {
                                          if (_printingNewPage)
                                              return;
                                          try
                                          {
                                              _printingNewPage = true;
                                              TextLayout parentForm = Parent as TextLayout;
                                              if (parentForm != null)
                                              {
                                                  parentForm.NewPageStartedBecauseOf(io, this);
                                              }

                                          }
                                          finally
                                          {
                                              _printingNewPage = false;
                                          }
                                      });
                    if (JapaneseMethods.Enabled)
                    {

                        var x = io as ENV.Printing.TextPrinterWriter;
                        if (x != null && x._alternateWrite != null)
                        {
                            Font = new Font("MS Gothic", 10);
                            //         HorizontalScale = 7.5f;

                            //       VerticalScale = 15f;
                            WriteTo(x._alternateWrite);

                        }
                    }
                };
                if (_task == null)
                {
                    TextLayout parentForm = Parent as TextLayout;
                    if (parentForm != null)
                    {
                        _task = parentForm.Controller;
                        u = parentForm.u;
                    }
                }
                Action after = () => { };
                if (u != null)
                    after = u.SetContext(_task);
                if (BeforeWrite != null)
                    BeforeWrite();
                doWrite();
                after();
            }
        }
        public static event Action BeforeWrite, BeforeRead;

        void ApplyBinding()
        {
            PrepareControlsForPrinting();
        }


        public override string Text { get; set; }

        bool _inCharsChange;
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            RefreshSize();
        }
        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            RefreshSize();
        }

        void RefreshSize()
        {
            if (_inCharsChange)
                return;

            SizeInChars = ToSize(ToCharsWithMovement(ToPoint(Size)));
        }
        private Point ToCharsWithMovement(Point p)
        {
            var x = ToChars(p);
            var xp = ToPixel(x);
            if (p.X == xp.X + 1)
            {
                x.X += 1;
            }
            else if (p.X == xp.X - 1)
            {
                x.X -= 1;
            }
            if (p.Y == xp.Y + 1)
            {
                x.Y += 1;
            }
            else if (p.Y == xp.Y - 1)
                x.Y -= 1;
            return x;
        }
        Point ToPoint(Size s)
        {
            return new Point(s.Width, s.Height);
        }
        Size ToSize(Point s)
        {
            return new Size(s.X, s.Y);
        }
        Point ToChars(Point p)
        {
            return new Point((int)Math.Round(p.X / HorizontalScale),
                    (int)Math.Round(p.Y / VerticalScale));
        }
        Size ToChars(Size s)
        {
            return ToSize(ToChars(ToPoint(s)));
        }
        Point ToPixel(Point value)
        {
            return new Point((int)(value.X * HorizontalScale),
                 (int)(value.Y * VerticalScale));
        }
        Size ToPixel(Size s)
        {
            return ToSize(ToPixel(ToPoint(s)));
        }
        Size SizeInChars
        {
            get
            {
                return ToChars(Size);

            }
            set
            {
                _inCharsChange = true;
                try
                {
                    Size = ToPixel(value);
                }
                finally
                {
                    _inCharsChange = false;
                }
            }
        }


        public void WriteTo(Writer writer)
        {
            WriteTo((SectionWriter)writer);
        }
        public void WriteTo(ENV.Printing.PrinterWriter writer)
        {
            WriteTo(new ReportSectionBridgeToSectionWriter(this, writer));
        }
        public Font Font { get; set; }

        internal double HorizontalScale = 9.6D;
        internal double VerticalScale = 17.11D;
        internal class ReportSectionBridgeToSectionWriter : SectionWriter
        {
            TextSection _parent;
            PrinterWriter _pw;

            public ReportSectionBridgeToSectionWriter(TextSection parent, PrinterWriter pw)
            {
                _parent = parent;
                _pw = pw;
            }

            public void WriteSection(int width, int height, TextPrintingStyle style, Action<TextControlWriter> writeCommand,
                                     Action newPageStatedDueToLackOfSpace)
            {
                var rs = new ReportSection
                {
                    Width = (int)Math.Round(width * _parent.HorizontalScale),
                    Height = (int)(height * _parent.VerticalScale)
                };
                var r = new List<ControlBase>();
                writeCommand(new TextControlWriterBridgeToSection(_parent, r));
                r.Reverse();
                rs.Controls.AddRange(r.ToArray());
                rs.WriteTo(_pw);
            }

            public class TextControlWriterBridgeToSection : TextControlWriter
            {
                TextSection _parent;
                List<ControlBase> _rs;

                public TextControlWriterBridgeToSection(TextSection parent, List<ControlBase> rs)
                {
                    _parent = parent;
                    _rs = rs;
                }

                public void Write(string text, int lineNumber, int position, int length, TextPrintingStyle style, ContentAlignment alignment)
                {
                    var l = new Label
                    {
                        Font = _parent.Font,
                        Top = (int)Math.Round(lineNumber * _parent.VerticalScale),
                        Left = (int)Math.Round(position * _parent.HorizontalScale),
                        Width = (int)Math.Round(length * _parent.HorizontalScale),
                        Height = (int)Math.Round(1 * _parent.VerticalScale),
                        Text = text.TrimEnd(' '),
                        BackColor = Color.White,
                        Alignment = alignment
                    };
                    if (l.Text == new string('-', length))
                    {
                        _rs.Add(new ENV.Printing.Shape {

                            LineHorizontal = true,
                            Bounds = l.Bounds
                        });
                        
                        
                    }
                    else
                        _rs.Add(l);
                }

                public string ProcessColumnData(string s, bool rightToLeft, bool HebrewDosCompatibleEditing)
                {
                    return s;
                }
                public bool DoNotTrimToWidth()
                {
                    return true;
                }

                public bool DoNotBreakMultiline()
                {
                    return false;
                }
            }
        }


        /// <summary>
        /// Reads this <This/> from the <see cref="Reader"/>
        /// </summary>
        /// <param name="reader">The <paramType/> to read from</param>
        public void ReadFrom(Reader reader)
        {
            using (ENV.Utilities.Profiler.StartContext("Read Text Section: " + Name))
            {
                if (BeforeRead != null)
                    BeforeRead();
                ReadFrom((ReaderInterface)reader);
            }
        }


        public void ReadFrom(ITextSectionReader reader)
        {
            reader.Read(this);
        }
        internal void ReadFrom(ReaderInterface reader)
        {
            ApplyBinding();
            reader.ReadSection(WidthInChars, HeightInChars,
                delegate (TextControlReader obj)
                    {
                        for (int i = Controls.Count - 1; i >= 0; i--)
                            ((TextControl)Controls[i]).ReadFrom(obj);

                    });

        }






        List<ENV.IO.Advanced.TextControlBase> CreateSortedListForSeperatedRead()
        {
            List<ENV.IO.Advanced.TextControlBase> result = new List<ENV.IO.Advanced.TextControlBase>();
            foreach (var textControlBase in Controls)
            {
                var tb = textControlBase as TextBox;
                if (tb != null && tb.Data != null && tb.Data.Column != null)
                    result.Add(tb);
            }

            result.Sort(delegate (ENV.IO.Advanced.TextControlBase x, ENV.IO.Advanced.TextControlBase y)
            {
                return x.LeftInChars.CompareTo(y.LeftInChars);
            });
            return result;
        }



        List<ENV.IO.Advanced.TextControlBase> _sortedControls;
        /// <summary>
        /// Reads that data in this <This/> from the <see cref="Reader"/> as a separated string. 
        /// </summary>
        /// <remarks>The string is separated by the <paramref name="separator"/>.
        /// Used mainly for comma separated reading.</remarks>
        /// <param name="reader">The <paramType/> from which to read</param>
        /// <param name="separator">The separator to use</param>
        public void ReadSeparated(Reader reader, char separator)
        {
            ApplyBinding();
            if (_sortedControls == null)
            {
                _sortedControls = CreateSortedListForSeperatedRead();

            }
            int i = 0;
            if (BeforeRead != null)
                BeforeRead();
            ((ReaderInterface)reader).ReadSection(WidthInChars, separator,
                delegate (ValueProviderDelegate obj)
                {
                    if (i >= _sortedControls.Count)
                        return;
                    _sortedControls[i++].SetValueAcordingTo(obj, reader);
                });
        }
        public void ReadDoubleSeparated(Reader reader, char separator)
        {
            ApplyBinding();
            if (_sortedControls == null)
            {
                _sortedControls = CreateSortedListForSeperatedRead();

            }
            int i = 0;
            if (BeforeRead != null)
                BeforeRead();
            ((ReaderInterface)reader).ReadSectionDoubleSeparator(WidthInChars, separator,
                delegate (ValueProviderDelegate obj)
                {
                    if (i >= _sortedControls.Count)
                        return;
                    _sortedControls[i++].SetValueAcordingTo(obj, reader);
                });
        }
        [Obsolete("Due to spelling mistake in the name")]
        public void ReadDoubleSeperated(Reader reader, char separator)
        {
            this.ReadDoubleSeparated(reader, separator);
        }

        public event BindingEventHandler<TextPrintingStyleBindingEventArgs> BindTextPrintingStyle
        {
            add
            {

                BindProperties += () =>
                {
                    var x = new TextPrintingStyleBindingEventArgs(TextPrintingStyle);
                    value(this, x);
                    TextPrintingStyle = x.Value;
                };

            }
            remove { throw new NotImplementedException(); }
        }
        public event BindingEventHandler<IntBindingEventArgs> BindWidthInChars
        {
            add { AddBindingEvent("WidthInChars", () => WidthInChars, x => WidthInChars = x, value); }
            remove { RemoveBindingEvent("WidthInChars", value); }
        }
        public event BindingEventHandler<IntBindingEventArgs> BindHeightInChars
        {
            add { AddBindingEvent("HeightInChars", () => HeightInChars, x => HeightInChars = x, value); }
            remove { RemoveBindingEvent("HeightInChars", value); }
        }

        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Use HeightInChars instead, will not work if set!!!")]
        public override int Height
        {
            get
            {
                return base.Height;
            }

            set
            {
                base.Height = value;
            }
        }
    }
}
