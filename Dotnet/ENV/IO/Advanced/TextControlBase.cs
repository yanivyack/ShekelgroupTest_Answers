using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ENV.IO.Advanced.Internal;
using Firefly.Box.Data.Advanced;
using System.ComponentModel;
using Firefly.Box.UI.Advanced;

namespace ENV.IO.Advanced
{
    public abstract class TextControlBase : ENV.Printing.TextBox, TextControl
    {
        bool _inCharsChange;
        public TextControlBase()
        {
            Alignment = System.Drawing.ContentAlignment.TopLeft;
            Font = new Font("Courier New", 12);
        }
        Size _prevSize = Size.Empty, _prevSizeInChars = Size.Empty;
        public Size SizeInChars
        {
            get
            {
                var z = Size;
                if (_prevSize == z)
                    return _prevSizeInChars;
                _prevSize = z;
                return _prevSizeInChars = ToChars(z);

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
        [Obsolete("Use WidthInChars instead, will not work if set!!!")]
        public new int Width { get { return base.Width; } set { base.Width = value; } }
        [Obsolete("Use HeightInChars instead, will not work if set!!!")]
        public new int Height { get { return base.Height; } set { base.Height = value; } }
        [Obsolete("Use TopInChars instead, will not work if set!!!")]
        public new int Top { get { return base.Top; } set { base.Top = value; } }
        [Obsolete("Use LeftInChars instead, will not work if set!!!")]
        public new int Left { get { return base.Left; } set { base.Left = value; } }
        /*   protected override string _VirtualGetText()
           {

               using (var sw = new System.IO.StringWriter())
               {
                   using (var fw = new FileWriter(sw))
                   {


                       ((SectionWriter)fw).WriteSection(LeftInChars + WidthInChars, TopInChars + HeightInChars, new TextPrintingStyle(), y =>((TextControl)this).WriteTo(y,
                           LeftInChars+WidthInChars,TopInChars+HeightInChars), delegate { });

                   }
                   var s = sw.ToString();
                   if (s.Length>LeftInChars)
                       return s.Substring(LeftInChars);
                   return "";

               }
           }*/
        class myForm
        { }

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
            LocationInChars = ToCharsWithMovement(Location);
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
        Point _prevPoint = Point.Empty, _prevPointInChars = Point.Empty;
        public Point LocationInChars
        {
            get
            {
                var p = Location;
                if (p == _prevPoint)
                    return _prevPointInChars;
                _prevPoint = p;
                return _prevPointInChars = ToChars(p);


            }
            set
            {
                _inCharsChange = true;
                try
                {
                    Location = ToPixel(value);
                }
                finally
                {
                    _inCharsChange = false;
                }
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Size Size { get { return base.Size; } set { base.Size = value; } }
        internal double HorizontalScale = 9.6D;
        internal double VerticalScale = 17.11D;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Point Location { get { return base.Location; } set { base.Location = value; } }


        

        public bool KeepNullChars;



        void TextControl.WriteTo(TextControlWriter form, int doNotExceedWidth, int doNotExceedHeight)
        {
            var left = LeftInChars;
            if (!Visible || left > doNotExceedWidth)
                return;
            if (left + WidthInChars > doNotExceedWidth)
            {
                WidthInChars = doNotExceedWidth - LeftInChars;
            }
            if (TopInChars + HeightInChars > doNotExceedHeight)
                HeightInChars = doNotExceedHeight - TopInChars;
            string s = "";
            bool isTextValue = false;
            WriteTo(delegate (string text, bool isTextValuex)
            {
                if (text != null)
                    s = text;
                isTextValue = isTextValuex;
            });
            if (!KeepNullChars)
                s = s.TrimEnd('\0');
            if (Multiline&&!form.DoNotBreakMultiline())
            {
                s = s.Replace("\r", "");
                StringBuilder sb = new StringBuilder();
                int line = 0;
                Action WriteLine =
                    delegate
                    {
                        WriteStringTo(form, sb.ToString(), line++, isTextValue);
                        sb = new StringBuilder();
                    };
                foreach (char c in s)
                {
                    if (c == '\n')
                    {
                        WriteLine();
                    }
                    else
                    {
                        sb.Append(c);
                        if (sb.Length == WidthInChars)
                            WriteLine();
                    }
                }
                if (sb.Length > 0)
                    WriteLine();
            }
            else
            {
                WriteStringTo(form, s, 0, isTextValue);
            }
        }
        internal delegate void Write(string text, bool isTextValue);
        internal abstract void WriteTo(Write writer);
        void TextControl.ReadFrom(TextControlReader reader)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < HeightInChars; i++)
            {
                sb.Append(reader.Read(TopInChars + i, LeftInChars, WidthInChars, _performRightToLeftManipulations()));
            }
            SetYourValue(sb.ToString(), reader);
        }

        internal abstract void SetYourValue(string value, IStringToByteArrayConverter sba);





        public bool PerformRightToLeftManipulations { get; set; }


        internal virtual bool _performRightToLeftManipulations()
        {
            return PerformRightToLeftManipulations;
        }

        public bool HebrewDosCompatibleEditing { get; set; }
        public bool TruncateRightOnRightAlign { get; set; }
        void WriteStringTo(TextControlWriter form, string whatToWrite, int line, bool isTextValue)
        {
            if (line >= HeightInChars)
                return;
            switch (Alignment)
            {
                case System.Drawing.ContentAlignment.BottomCenter:
                case System.Drawing.ContentAlignment.BottomLeft:
                case System.Drawing.ContentAlignment.BottomRight:
                    line += HeightInChars - 1;
                    break;
                case System.Drawing.ContentAlignment.MiddleCenter:
                case System.Drawing.ContentAlignment.MiddleLeft:
                case System.Drawing.ContentAlignment.MiddleRight:
                    line += (int)Math.Round((double)(HeightInChars - 1) / 2, 0, MidpointRounding.AwayFromZero);
                    break;

            }
            string result = whatToWrite;
            if (result.Length > WidthInChars && !form.DoNotTrimToWidth())
            {
                if (!TruncateRightOnRightAlign && (Alignment == ContentAlignment.TopRight ||
                    Alignment == ContentAlignment.MiddleRight ||
                    Alignment == ContentAlignment.BottomRight))
                    result = result.Substring(result.Length - WidthInChars);
                else
                    result = result.Remove(WidthInChars);
            }
            if (isTextValue)
                result = form.ProcessColumnData(result, _performRightToLeftManipulations(), HebrewDosCompatibleEditing);

            switch (Alignment)
            {
                case System.Drawing.ContentAlignment.BottomLeft:
                case System.Drawing.ContentAlignment.MiddleLeft:
                case System.Drawing.ContentAlignment.TopLeft:
                    result = result.PadRight(WidthInChars);
                    break;
                case System.Drawing.ContentAlignment.BottomCenter:
                case System.Drawing.ContentAlignment.MiddleCenter:
                case System.Drawing.ContentAlignment.TopCenter:
                    int left = (WidthInChars - result.Length) / 2;
                    result = result.PadLeft(left + result.Length);
                    result = result.PadRight(WidthInChars);
                    break;
                case System.Drawing.ContentAlignment.BottomRight:
                case System.Drawing.ContentAlignment.MiddleRight:
                case System.Drawing.ContentAlignment.TopRight:
                    result = result.PadLeft(WidthInChars);
                    break;
                default:
                    throw new System.InvalidOperationException();
            }
            form.Write(result, line + TopInChars, LeftInChars, WidthInChars, TextPrintingStyle, Alignment);
        }






        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int LeftInChars
        {
            set { LocationInChars = new Point(value, LocationInChars.Y); }
            get { return LocationInChars.X; }
        }
        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TopInChars
        {
            set { LocationInChars = new Point(LocationInChars.X, value); }
            get { return LocationInChars.Y; }
        }
        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int WidthInChars
        {
            set { SizeInChars = new Size(value, SizeInChars.Height); }
            get { return SizeInChars.Width; }
        }
        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int HeightInChars
        {
            set { SizeInChars = new Size(SizeInChars.Width, value); }
            get { return SizeInChars.Height; }
        }



        public TextPrintingStyle TextPrintingStyle

        {
            get { return _textPrintingStyle; }
            set { _textPrintingStyle = value ?? TextPrintingStyle.Default; }
        }


        TextPrintingStyle _textPrintingStyle = TextPrintingStyle.Default;


        internal void SetValueAcordingTo(ValueProviderDelegate obj, IStringToByteArrayConverter c)
        {
            SetYourValue(obj(_performRightToLeftManipulations()), c);
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
        public event BindingEventHandler<IntBindingEventArgs> BindLeftInChars
        {
            add { AddBindingEvent("LeftInChars", () => LeftInChars, x => LeftInChars = x, value); }
            remove { RemoveBindingEvent("LeftInChars", value); }
        }
        public event BindingEventHandler<IntBindingEventArgs> BindTopInChars
        {
            add { AddBindingEvent("TopInChars", () => TopInChars, x => TopInChars = x, value); }
            remove { RemoveBindingEvent("TopInChars", value); }
        }




        internal virtual void DoOnColumn(Action<ColumnBase> column)
        {

        }
    }
    public class TextPrintingStyleBindingEventArgs : BindingEventArgs<TextPrintingStyle>
    {
        public TextPrintingStyleBindingEventArgs(TextPrintingStyle value)
            : base(value)
        {
        }
    }
}
