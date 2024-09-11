using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using ENV.Labs;
using ENV.Utilities;
using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;
using ContentAlignment = System.Drawing.ContentAlignment;
using Firefly.Box;

namespace ENV.UI
{
    public partial class Button : Firefly.Box.UI.Button, ICanShowCustomHelp
    {
        public static event Action<Button> ButtonCreated;
        public Button()
        {

            if (Labs.FaceLiftDemo.Enabled)
                CoolEnabled = true;
            if (EnableSpecialSubClassing)
            {
                _displayStrategy = new SortDisplayStrategy(this);
                BindText += new BindingEventHandler<StringBindingEventArgs>(Button_BindText);
            }
            else
                _displayStrategy = new NormalDisplayStrategy();
            InitializeComponent();
            _helper = new ENV.Utilities.ControlHelper(this);
            base.Enter += () => _helper.ControlEnter(Enter);
            ;
            base.Leave += () => _helper.ControlLeave(Leave);
            ;
            base.Change += () => _helper.ControlChange(Change);
            ;
            base.InputValidation += () => _helper.ControlInputValidation(InputValidation);
            ;
            Red();
            OpaqueBackgroundInVisualStyle = true;
            ForeColorChanged += (sender, args) =>
            {
                var i = 0;
            };
            if (ButtonCreated != null)
                ButtonCreated(this);
            HyperLinkColorScheme = new ColorScheme( Color.FromArgb(0,19,225), SystemColors.ButtonFace);
        }
        protected override void OnRaise(Command command)
        {
            ControllerBase.RaiseHappened(command);
        }

        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }

        void DefaultColors()
        {
            CoolFormDefaultButtonColor1 = Color.FromArgb(90, 155, 223);
            CoolFormDefaultButtonColor2 = Color.FromArgb(3, 102, 204);
            CoolFormDefaultButtonTextColor = Color.White;
            CoolNormalColor1 = Color.FromArgb(90, 155, 223);
            CoolNormalColor2 = Color.FromArgb(3, 102, 204);
            CoolNormalTextColor = Color.White;
            CoolFocusedColor1 = Color.FromArgb(90, 155, 223);
            CoolFocusedColor2 = Color.FromArgb(3, 102, 204);
            CoolFocusedTextColor = Color.White;
            CoolDisabledColor1 = Color.FromArgb(90, 155, 223);
            CoolDisabledColor2 = Color.FromArgb(3, 102, 204);
            CoolFocusedTextColor = Color.White;
            CoolClickedColor1 = Color.FromArgb(254, 142, 75);
            CoolClickedColor2 = Color.FromArgb(254, 201, 133);
            CoolClickedTextColor = Color.Black;
            CoolHoverColor1 = Color.FromArgb(255, 240, 194);
            CoolHoverColor2 = Color.FromArgb(255, 214, 155);
            CoolHoverTextColor = Color.Black;
        }
        void Green()
        {
            var color2 = Color.FromArgb(41, 125, 43);
            var color1 = Color.FromArgb(69, 180, 69);
            CoolFormDefaultButtonColor1 = color1;
            CoolFormDefaultButtonColor2 = color2;
            CoolFormDefaultButtonTextColor = Color.White;
            CoolNormalColor1 = color1;
            CoolNormalColor2 = color2;
            CoolNormalTextColor = Color.White;
            CoolFocusedColor1 = color1;
            CoolFocusedColor2 = color2;
            CoolFocusedTextColor = Color.White;
            CoolDisabledColor1 = color1;
            CoolDisabledColor2 = color2;
            CoolFocusedTextColor = Color.White;
            CoolClickedColor1 = Color.FromArgb(254, 142, 75);
            CoolClickedColor2 = Color.FromArgb(254, 201, 133);
            CoolClickedTextColor = Color.Black;
            CoolHoverColor1 = Color.FromArgb(255, 240, 194);
            CoolHoverColor2 = Color.FromArgb(255, 214, 155);
            CoolHoverTextColor = Color.Black;
        }
        void Red()
        {
            var color1 = Color.FromArgb(248, 43, 43);
            var color2 = Color.FromArgb(246, 12, 12);
            CoolFormDefaultButtonColor1 = color1;
            CoolFormDefaultButtonColor2 = color2;
            CoolFormDefaultButtonTextColor = Color.White;
            CoolNormalColor1 = color1;
            CoolNormalColor2 = color2;
            CoolNormalTextColor = Color.White;
            CoolFocusedColor1 = color1;
            CoolFocusedColor2 = color2;
            CoolFocusedTextColor = Color.White;
            CoolDisabledColor1 = color1;
            CoolDisabledColor2 = color2;
            CoolFocusedTextColor = Color.White;
            CoolClickedColor1 = Color.FromArgb(254, 142, 75);
            CoolClickedColor2 = Color.FromArgb(254, 201, 133);
            CoolClickedTextColor = Color.Black;
            CoolHoverColor1 = Color.FromArgb(255, 240, 194);
            CoolHoverColor2 = Color.FromArgb(255, 214, 155);
            CoolHoverTextColor = Color.Black;
        }

        public bool CoolEnabled { get; set; }

        Color CoolClickedColor1 { get; set; }
        Color CoolClickedColor2 { get; set; }
        Color CoolClickedTextColor { get; set; }
        Color CoolHoverColor1 { get; set; }
        Color CoolHoverColor2 { get; set; }
        Color CoolHoverTextColor { get; set; }
        Color CoolFormDefaultButtonColor1 { get; set; }
        Color CoolFormDefaultButtonColor2 { get; set; }
        Color CoolFormDefaultButtonTextColor { get; set; }
        Color CoolNormalColor1 { get; set; }
        Color CoolNormalColor2 { get; set; }
        Color CoolNormalTextColor { get; set; }
        Color CoolFocusedColor1 { get; set; }
        Color CoolFocusedColor2 { get; set; }
        Color CoolFocusedTextColor { get; set; }
        Color CoolDisabledColor1 { get; set; }
        Color CoolDisabledColor2 { get; set; }
        Color CoolDisabledTextColor { get; set; }
        bool _coolIsWorking = false;
        public bool FixedBackColorOnNormalStyle { get; set; }
        protected override void OnLoad()
        {

            if (CoolEnabled)
            {
                if (Style == Firefly.Box.UI.ButtonStyle.Normal || Style == ButtonStyle.ImageAndText)
                {
                    _coolIsWorking = true;
                    Style = ButtonStyle.TextWithImage;

                    ImageButtonImageStructure = ImageButtonImageStructure.FormsDefaultButton;
                    ImageLocation = "xx";
                }
            }
            else if (FixedBackColorOnNormalStyle && (Style == ButtonStyle.Normal || Style == ButtonStyle.HyperLink))
                ForeColor = System.Drawing.SystemColors.ControlText;
            if (FixedBackColorOnNormalStyle && Style == ButtonStyle.Normal)
                BackColor = Color.Empty;
            if (!AllowAlignmentChange && Style != Firefly.Box.UI.ButtonStyle.HyperLink)
                Alignment = ContentAlignment.MiddleCenter;
            base.OnLoad();
        }

        Action<StringBindingEventArgs> _bindText = delegate { };
        void Button_BindText(object sender, StringBindingEventArgs e)
        {
            _bindText(e);
        }
        public bool AllowAlignmentChange { get; set; }

        ENV.Utilities.ControlHelper _helper;

        public bool AutoExpand
        {
            get { return _helper.AutoExpand; }
            set { _helper.AutoExpand = value; }
        }

        protected override System.Drawing.Image GetImage(string imageLocation)
        {
            if (_coolIsWorking)
                return CreateImage(delegate { });
            return _displayStrategy.GetImage(imageLocation) ?? ENV.Common.GetImage(imageLocation);
        }
        protected Image CreateImage(Action<Graphics, Rectangle> draw)
        {
            var image = new Bitmap(Width * 6, Height);

            using (var g = Graphics.FromImage(image))
            {
                using (var b = new SolidBrush(BackColor))
                    g.FillRectangle(b, new Rectangle(Point.Empty, image.Size));
                using (var borderPen = new System.Drawing.Pen(Color.FromArgb(171, 171, 171)))
                    for (int i = 0; i < 6; i++)
                    {
                        DrawCoolButton(draw, g, borderPen, i);
                    }
            }
            return image;
        }
        private void DrawCoolButton(Action<Graphics, Rectangle> draw, Graphics g, Pen borderPen, int i)
        {
            var rect = new Rectangle(Width * i, 0, Width - 1, Height - 1);
            Brush b = null;
            switch (i)
            {

                case 1://pressed
                    b = new System.Drawing.Drawing2D.LinearGradientBrush(rect,
                                                                         CoolClickedColor1,
                                                                         CoolClickedColor2, 90);
                    break;

                case 4://hover
                    b = new System.Drawing.Drawing2D.LinearGradientBrush(rect,
                                                                         CoolHoverColor1,
                                                                         CoolHoverColor2, 90);
                    break;
                case 5://default
                    b = new System.Drawing.Drawing2D.LinearGradientBrush(rect,
                                                                      CoolFormDefaultButtonColor1,
                                                                         CoolFormDefaultButtonColor2, 90);
                    break;

                case 3://enabled
                    b = new System.Drawing.Drawing2D.LinearGradientBrush(rect,
                                                                      CoolNormalColor1,
                                                                         CoolNormalColor2, 90);
                    break;

                case 0://parked
                    b = new System.Drawing.Drawing2D.LinearGradientBrush(rect,
                                                                      CoolFocusedColor1,
                                                                         CoolFocusedColor2, 90);
                    break;
                case 2://disabled
                    b = new System.Drawing.Drawing2D.LinearGradientBrush(rect,
                                                                      CoolDisabledColor1,
                                                                         CoolDisabledColor2, 90);
                    break;

            }
            try
            {

                DrawRoundedRectangle(g, rect, 10,
                                     borderPen, b);
                draw(g, rect);
            }
            finally
            {
                b.Dispose();
            }
        }

        protected override Color GetTextColor(Color foreColor, Firefly.Box.UI.ButtonState state)
        {
            if (_coolIsWorking)
                switch (state)
                {
                    case Firefly.Box.UI.ButtonState.Hovered:
                        return CoolHoverTextColor;
                    case Firefly.Box.UI.ButtonState.Clicked:
                        return CoolClickedTextColor;
                    case Firefly.Box.UI.ButtonState.Disabled:
                        return CoolDisabledTextColor;
                    case Firefly.Box.UI.ButtonState.Focused:
                        return CoolFocusedTextColor;
                    case Firefly.Box.UI.ButtonState.FormsDefaultButton:
                        return CoolFormDefaultButtonTextColor;
                    case Firefly.Box.UI.ButtonState.Normal:
                        return CoolNormalTextColor;
                }
            if (_ignoreGridRowForeColorInImageButton)
                return ForeColor;
            return base.GetTextColor(foreColor, state);
        }

        bool _ignoreGridRowForeColorInImageButton;

        public static void DrawRoundedRectangle(Graphics g, Rectangle r, int d, Pen p, Brush fillBrush)
        {

            System.Drawing.Drawing2D.GraphicsPath gp =
                    new System.Drawing.Drawing2D.GraphicsPath();

            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.X + r.Width - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Y + r.Height - d, d, d, 90, 90);
            gp.AddLine(r.X, r.Y + r.Height - d, r.X, r.Y + d / 2);
            g.FillPath(fillBrush, gp);
            g.DrawPath(p, gp);

        }
        internal void RunGetImageForTesting(string imageLocation)
        {
            GetImage(imageLocation);
        }

        public void ClearExpandEvent()
        {
            _helper.ClearExpandEvent();
        }

        public override event System.Action Enter;
        public override event System.Action Leave;
        public override event System.Action Change;
        public override event System.Action InputValidation;

        public new event System.Action Expand
        {
            add { _helper.Expand += value; }
            remove { _helper.Expand -= value; }
        }
        /// <summary>
        /// Only used for the UserMethods.ControlSelectProgram method - backward compatability only
        /// </summary>
        public Type ExpandClassType { set { _helper.ExpandClassType = value; } get { return _helper.ExpandClassType; } }
        public AfterExpandGoToNextControlOptions AfterExpandGoToNextControl { get { return _helper.AfterExpandGoToNextControl; } set { _helper.AfterExpandGoToNextControl = value; } }

        public string StatusTip
        {
            get { return _helper.StatusTip; }
            set { _helper.StatusTip = value; }
        }

        public CustomHelp CustomHelp { get; set; }

        public static bool EnableSpecialSubClassing { get; set; }
        public static bool EnableSpecialSubClassing1 { get; set; }
        List<IDisposable> _dispose = new List<IDisposable>();

        protected override void Dispose(bool disposing)
        {
            foreach (var disposable in _dispose)
            {
                disposable.Dispose();
            }
            base.Dispose(disposing);
        }

        static string Parse(string s, char startChar, char endChar, Action<string> what)
        {
            int start = s.IndexOf(startChar);
            if (start >= 0)
            {
                int end = s.IndexOf(endChar, start);
                if (end > -1)
                {
                    var s2 = s.Substring(start + 1, end - start - 1);
                    try
                    {
                        what(s2);
                    }
                    catch
                    {
                    }
                    s = s.Remove(start, end - start + 1);
                }

            }
            return s;
        }

        internal Func<string, Image> _loadImage = imagePath =>
        {
            try
            {
                return ChangeColor(Common.GetImage(imagePath), Color.FromArgb(192, 192, 192), SystemColors.Control);
            }
            catch
            {
                return Common.GetImage(imagePath);
            }
        };

        static Image ChangeColor(Image source, Color oldColor, Color newColor)
        {
            var sourceBitmap = source as Bitmap;
            if (sourceBitmap == null || sourceBitmap.RawFormat.Guid == System.Drawing.Imaging.ImageFormat.Icon.Guid) return source;
            var size = source.Size;
            var bitmap = (Bitmap)null;
            var graphics = (Graphics)null;
            bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            try
            {
                graphics = Graphics.FromImage((Image)bitmap);
                graphics.Clear(newColor);
                var destRect = new Rectangle(0, 0, size.Width, size.Height);
                var imageAttrs = (ImageAttributes)null;
                try
                {
                    imageAttrs = new ImageAttributes();
                    imageAttrs.SetColorKey(oldColor, oldColor, ColorAdjustType.Bitmap);
                    graphics.DrawImage(sourceBitmap, destRect, 0, 0, size.Width, size.Height, GraphicsUnit.Pixel,
                                       imageAttrs, (Graphics.DrawImageAbort)null, IntPtr.Zero);
                }
                finally
                {
                    if (imageAttrs != null)
                        imageAttrs.Dispose();
                }
            }
            finally
            {
                if (graphics != null)
                    graphics.Dispose();
            }
            return bitmap;
        }

        internal static void ParseColor(string val, Action<Color> setColor)
        {
            if (val.Length < 6) return;
            setColor(ENV.Utilities.ColorFile.FromString(val));
        }

        protected override string Translate(string text)
        {
            if (text == ToolTip) return ENV.Languages.Translate(text);

            text = JapaneseMethods.HandleControlText(text, Format, Data);

            if (_coolIsWorking)
            {
                switch ((text ?? "").Trim().ToUpper().Replace("&", ""))
                {
                    case "OK":
                    case "SELECT":
                    case "GO":
                    case "CONFIRM":
                    case "CONFIRMA":
                    case "EXIT":
                    case "אישור":
                    case "יציאה":
                        Green();
                        break;
                    case "DELETE":
                    case "CANCELA":
                        Red();
                        break;
                    default:
                        DefaultColors();
                        break;
                }


            }
            return base.Translate(_displayStrategy.TranslateButtonText(ENV.Languages.Translate(text))); ;
        }

        string _previousText = "";
        protected override string TranslateText(string text)
        {
            var r = Translate(text);
            if (DontAllowBlank && Style == ButtonStyle.Normal && r == "")
                return _previousText;
            _previousText = r;
            return r;
        }

        interface DisplayStrategy
        {
            string TranslateButtonText(string newValue);
            void WasClicked(Action baseClick);
            Image GetImage(string imageLocation);
            string FormatChangedTo(string value);
        }

        class NormalDisplayStrategy : DisplayStrategy
        {

            public string TranslateButtonText(string newValue)
            {
                return newValue;
            }

            public void WasClicked(Action baseClick)
            {
                baseClick();
            }

            public Image GetImage(string imageLocation)
            {
                return null;
            }

            public string FormatChangedTo(string value)
            {
                return value;
            }
        }

        Action<PaintEventArgs> _onPaint = delegate { };

        protected override void OnPaint(PaintEventArgs e)
        {


            base.OnPaint(e);
            _onPaint(e);
        }
        class SortDisplayStrategy : DisplayStrategy
        {
            Button _parent;
            GetImageProvider _getImageProvider;

            public SortDisplayStrategy(Button parent)
            {
                _parent = parent;
                _getImageProvider = new GetImageProvider(_parent);
            }
            bool _inProgress = false;
            bool _inParse = false;
            public string TranslateButtonText(string newValue)
            {

                if (newValue == null)
                    return newValue;
                if (newValue.StartsWith("hl:zl:", StringComparison.InvariantCultureIgnoreCase))
                    newValue = newValue.Substring(3);
                if (_inParse)
                    return newValue;
                var p = new ParseResult(_parent);
                var result = newValue;
                _inParse = true;
                try
                {
                    result = p.Parse(newValue, true);
                }
                finally
                {
                    _inParse = false;
                }
                if (_inProgress)
                    return result;
                Func<string> t = () =>
                   {
                       _inProgress = true;
                       try
                       {
                           if (p.ForeColor != Color.Empty)
                               _parent.ForeColor = p.ForeColor;
                           if (p.BackColor != Color.Empty)
                               _parent.BackColor = p.BackColor;

                           if (p.Tag.ToUpper() == "IT")
                           {
                               _parent._displayStrategy = new IconAndText(_parent, null, ContentAlignment.TopCenter);
                               return result;
                           }
                           if (p.Tag.ToUpper() == "IL")
                           {
                               _parent._displayStrategy = new IconAndText(_parent, null, ContentAlignment.MiddleLeft);
                               return result;
                           }
                           if (p.Tag.ToUpper() == "IR")
                           {
                               _parent._displayStrategy = new IconAndText(_parent, null, ContentAlignment.MiddleRight);
                               return result;
                           }
                           if (p.Tag.ToUpper() == "BI")
                           {
                               _parent._displayStrategy = new IconAndText(_parent, null, ContentAlignment.MiddleCenter);
                               return result;
                           }

                           IconPathAndAlignment icon;
                           if (!(_parent.Parent != null && _parent.Parent.Parent is Grid) && _iconsForSpecialSubclassing.TryGetValue(result.Trim(), out icon) && (icon.Path.StartsWith("@") || System.IO.File.Exists(icon.Path)))
                           {
                               _parent._displayStrategy = new IconAndText(_parent, icon.Path, icon.Alignment);
                               return result;
                           }

                           if (p.Tag == "HT")
                           {
                               _parent._displayStrategy = new HotTrackButton(_parent);
                               return _parent._displayStrategy.TranslateButtonText(newValue);
                           }
                           if (p.Tag.ToUpper() == "HL")
                           {
                               int i;
                               _parent.Style = Firefly.Box.UI.ButtonStyle.HyperLink;
                               _parent.Alignment = SpecialSubClassingHyperLinkAlignment;
                               _parent.HyperLinkUnderline = SpecialSubClassingHyperLinkUnderline;

                               if (int.TryParse(p.ImageLocation, out i))
                               {
                                   var colorScheme = _parent.GetColorByIndex(i);
                                   _parent.HyperLinkColorScheme = colorScheme;
                                   _parent.HyperLinkMouseEnterColorScheme = colorScheme;
                                   _parent.HyperLinkPressedColorScheme = colorScheme;
                               }
                               else
                               {
                                   _parent.HyperLinkBackColor = SystemColors.Window;
                                   _parent.HyperLinkMouseEnterBackColor = SystemColors.Window;
                                   _parent.HyperLinkPressedBackColor = SystemColors.Window;
                                   _parent.HyperLinkForeColor = Color.Blue;
                                   _parent.HyperLinkMouseEnterForeColor = Color.Blue;
                                   _parent.HyperLinkPressedForeColor = Color.Red;
                               }

                               if (SpecialSubClassingHyperLinkMouseEnterForeColor != Color.Empty)
                                   _parent.HyperLinkMouseEnterForeColor = SpecialSubClassingHyperLinkMouseEnterForeColor;
                               return result;
                           }
                           if (p.Tag.ToUpper() == "XP")
                           {
                               _parent.Style = ButtonStyle.Normal;
                               return result;
                           }
                           if (p.Tag.ToUpper() == "GB" || p.Tag == "BM")
                           {
                               _parent.CoolEnabled = false;
                               _parent._coolIsWorking = false;
                               _parent._displayStrategy = new FiveStateButtonWithText(_parent, p.Tag == "BM");
                               return _parent._displayStrategy.TranslateButtonText(newValue);
                           }
                           if (p.Tag.ToUpper() == "BK")
                           {
                               _parent._displayStrategy = new BlinkDisplayStrategy(_parent);
                               return _parent._displayStrategy.TranslateButtonText(newValue);
                           }
                           if (p.Tag.ToUpper() == "ZR" || p.Tag.ToUpper() == "RZ")
                           {
                               _parent._displayStrategy = new ZoomRightDisplayStrategy(_parent);
                               return _parent._displayStrategy.TranslateButtonText(newValue);
                           }
                           if (p.Tag.ToUpper() == "ZL" || p.Tag.ToUpper() == "LZ")
                           {
                               _parent._displayStrategy = new ZoomLeftDisplayStrategy(_parent);
                               return _parent._displayStrategy.TranslateButtonText(newValue);
                           }
                           if (p.Tag.ToUpper() == "AI" || p.Tag.ToUpper() == "IC")
                           {
                               _parent._displayStrategy = new IconDisplayStrategy(_parent);
                               return _parent._displayStrategy.TranslateButtonText(newValue);
                           }
                           if (p.Tag.ToUpper() == "CL" && result == "DIGITAL")
                           {
                               _parent._displayStrategy = new DigitalClockDisplayStrategy(_parent);
                               return _parent._displayStrategy.TranslateButtonText(newValue);
                           }
                           if (p.Tag.ToUpper() == "CL" && result == "ANALOG")
                           {
                               _parent._displayStrategy = new AnalogClockDisplayStrategy(_parent);
                               return _parent._displayStrategy.TranslateButtonText(newValue);
                           }
                           if (p.Tag.ToUpper() == "BB")
                           {
                               _parent._displayStrategy = new BitmapDisplayButtonStrategy(_parent);
                               return _parent._displayStrategy.TranslateButtonText(newValue);
                           }
                           if (p.Tag.ToUpper() == "MC")
                           {
                               _parent._displayStrategy = new CalendarButton(_parent);
                               return _parent._displayStrategy.TranslateButtonText(newValue);
                           }
                           if (p.Tag.ToUpper() == "PB")
                           {
                               _parent._displayStrategy = new ProgressBarButtonStrategy(_parent);
                               return _parent._displayStrategy.TranslateButtonText(newValue);
                           }
                           if (p.Tag.ToUpper() == "PU" || p.Tag.ToUpper() == "PD" || p.Tag.ToUpper() == "PR" ||
                               p.Tag.ToUpper() == "PL")
                           {
                               var sb = System.Windows.Forms.ScrollButton.Up;
                               switch (p.Tag.ToUpper()[1])
                               {
                                   case 'D':
                                       sb = System.Windows.Forms.ScrollButton.Down;
                                       break;
                                   case 'L':
                                       sb = System.Windows.Forms.ScrollButton.Left;
                                       break;
                                   case 'R':
                                       sb = System.Windows.Forms.ScrollButton.Right;
                                       break;
                               }
                               _parent._displayStrategy = new ScrollButton(_parent, sb);
                               return _parent._displayStrategy.TranslateButtonText(newValue);

                           }
                           if (p.Tag.ToUpper() == "IS")
                           {
                               _parent._displayStrategy = new ImageButtonDisplayStrategy(_parent);
                           }
                           return _getImageProvider.TranslateButtonText(result);
                       }
                       finally
                       {
                           _inProgress = false;
                       }
                   };

                var r = result;
                Context.Current.InvokeUICommand(() => r = t());
                return r;
            }
            public Image GetImage(string imageLocation)
            {
                return _getImageProvider.GetImage(imageLocation);
            }
            public void WasClicked(Action baseClick)
            {
                baseClick();
            }

            public string FormatChangedTo(string value)
            {
                return _getImageProvider.FormatChangedTo(value);
            }
        }
        class CalendarButton : DisplayStrategy
        {
            Button _parent;
            MonthCalendar x;
            string _hotkey;
            public CalendarButton(Button parent)
            {
                _parent = parent;
                x = new System.Windows.Forms.MonthCalendar() { };
                if (_parent.Controls.Count > 0)
                    _parent.Controls[0].Visible = false;
                _parent.Controls.Add(x);

                x.ShowWeekNumbers = true;
                x.DateSelected += (s, a) =>
                {
                    if (!string.IsNullOrEmpty(_hotkey))
                        Common.RunOnLogicContext(_parent, () => Common.Raise((Keys)new KeysConverter().ConvertFromString(_hotkey)));
                };

                Position();
                x.SizeChanged += delegate { Position(); };
                var c = new Control() { BackColor = Color.White, Dock = DockStyle.Fill };
                _parent.Controls.Add(c);

                _parent.ControlAdded += _parent_ControlAdded;
                _parent._getMonthCalendar = hk => string.Equals(_hotkey, hk, StringComparison.InvariantCultureIgnoreCase) ? x : null;
            }

            void Position()
            {
                x.Size = _parent.Size;
                x.Left = (_parent.Width - x.Width) / 2;
                x.Top = (_parent.Height - x.Height) / 2;
            }
            private void _parent_ControlAdded(object sender, ControlEventArgs e)
            {
                e.Control.Visible = false;
            }
            public string FormatChangedTo(string value)
            {
                return value;
            }
            public Image GetImage(string imageLocation)
            {
                return null;
            }
            public string TranslateButtonText(string newValue)
            {
                var i = newValue.IndexOf('<');
                if (i != -1)
                {
                    var j = newValue.IndexOf('>');
                    if (j != -1)
                        _hotkey = newValue.Substring(i + 1, j - i - 1);
                }
                return newValue;
            }
            public void WasClicked(Action baseClick)
            {
            }
        }
        Func<string, MonthCalendar> _getMonthCalendar = hk => null;
        internal MonthCalendar GetMonthCalendar(string hotkey)
        {
            return _getMonthCalendar(hotkey);
        }
        protected virtual ColorScheme GetColorByIndex(int index)
        {
            return null;
        }

        class HotTrackButton : DisplayStrategy
        {
            Button _parent;

            public HotTrackButton(Button parent)
            {
                _parent = parent;
                _parent.Style = Firefly.Box.UI.ButtonStyle.HyperLink;

                _parent.HyperLinkBackColor = SystemColors.Window;
                _parent.HyperLinkMouseEnterBackColor = SystemColors.Window;
                _parent.HyperLinkPressedBackColor = SystemColors.Window;
                _parent.HyperLinkMouseEnterForeColor = Color.Blue;
                _parent.HyperLinkPressedForeColor = Color.Red;


            }


            public string FormatChangedTo(string value)
            {
                return value;
            }
            public string TranslateButtonText(string newValue)
            {
                return new ParseResult(_parent).Parse(newValue, false);

            }

            public void WasClicked(Action baseClick)
            {
                baseClick();
            }

            public Image GetImage(string imageLocation)
            {
                return null;
            }
        }
        class ParseResult
        {
            public string Tag = "";
            public string Separator;
            public string ImageLocation = "";
            ENV.UI.Button _parent;
            public Color BackColor = Color.Empty;
            public Color ForeColor = Color.Empty;
            public bool FlatStyle;

            public ParseResult(Button parent)
            {
                _parent = parent;
            }

            public void ApplyToButton(Action<System.Windows.Forms.Button> apply)
            {
                if (_parent.Controls.Count > 0)
                {
                    var b = _parent.Controls[0] as System.Windows.Forms.Button;
                    if (b != null)
                        apply(b);
                }
                else
                    _parent.ControlAdded += (sender, args) =>
                    {
                        var b = args.Control as System.Windows.Forms.Button;
                        if (b != null)
                            apply(b);
                    };
            }

            public string Parse(string newValue, bool lookForImage)
            {
                if (string.IsNullOrEmpty(newValue))
                    return newValue;
                bool done = false;
                bool foundTag = false;
                bool foundAlignment = false;
                var foundBC = false;
                var foundFC = false;

                try
                {

                    while (!done)
                    {
                        if (foundTag)
                        {
                            if (lookForImage && newValue.StartsWith("["))
                            {
                                var x = newValue.IndexOf(']');
                                ImageLocation = newValue.Substring(1, newValue.IndexOf(']') - 1);
                                newValue = newValue.Substring(x + 1);
                                continue;
                            }
                            if (newValue.StartsWith("{") && !foundFC)
                            {
                                int i = newValue.IndexOf('}');
                                ParseColor(newValue.Remove(i).Substring(1),
                                    x =>
                                    {
                                        ForeColor = x;
                                        ApplyToButton(b => b.ForeColor = x);
                                        newValue = newValue.Substring(i + 1);
                                    });
                                foundFC = true;
                                continue;
                            }
                            if (!foundBC)
                            {
                                if (newValue.StartsWith("("))
                                {
                                    int i = newValue.IndexOf(')');
                                    ParseColor(newValue.Remove(i).Substring(1),
                                        x =>
                                        {
                                            BackColor = x;
                                            ApplyToButton(b => b.BackColor = x);
                                            newValue = newValue.Substring(i + 1);
                                        });
                                    foundBC = true;
                                    continue;
                                }
                                else if (Tag.ToUpper() == "CR")
                                {
                                    ParseColor(newValue,
                                        x =>
                                        {
                                            BackColor = x;
                                            ApplyToButton(b => b.BackColor = x);
                                            newValue = "";
                                        });
                                    foundBC = true;
                                    continue;
                                }
                            }
                        }
                        var separator = '\0';
                        if (newValue.Length >= 3)
                        {
                            separator = newValue[2];
                            switch (separator)
                            {
                                case ':':
                                case ';':
                                    {
                                        var token = newValue.Substring(0, 2);
                                        switch (token)
                                        {
                                            case "sl":
                                                if (foundAlignment)
                                                    break;
                                                foundAlignment = true;
                                                if (_parent.Alignment != ContentAlignment.MiddleLeft)
                                                    _parent.Alignment = ContentAlignment.MiddleLeft;
                                                _parent.AllowAlignmentChange = true;
                                                break;
                                            case "sr":
                                                if (foundAlignment)
                                                    break;
                                                foundAlignment = true;
                                                if (_parent.Alignment != ContentAlignment.MiddleRight)
                                                    _parent.Alignment = ContentAlignment.MiddleRight;
                                                _parent.AllowAlignmentChange = true;
                                                break;
                                            case "sc":
                                                if (foundAlignment)
                                                    break;
                                                foundAlignment = true;
                                                if (_parent.Alignment != ContentAlignment.MiddleCenter)
                                                    _parent.Alignment = ContentAlignment.MiddleCenter;
                                                _parent.AllowAlignmentChange = true;
                                                break;
                                            case "sm":
                                                _parent.Multiline = true;
                                                break;
                                            case "bh":
                                                FlatStyle = true;
                                                break;
                                            case "tc":
                                                newValue = newValue.Substring(4);
                                                var s = newValue.Remove(newValue.IndexOf('}'));
                                                ParseColor(s, color => _parent.ForeColor = color);
                                                newValue = "xxx" + newValue.Substring(newValue.IndexOf('}') + 1);
                                                break;
                                            default:
                                                if (!foundTag)
                                                    Tag = token;
                                                foundTag = true;
                                                break;
                                        }
                                        newValue = newValue.Substring(3);
                                        continue;
                                    }
                                    break;
                                case '[':
                                    if (!foundTag)
                                        Tag = newValue.Substring(0, 2);
                                    foundTag = true;
                                    newValue = newValue.Substring(2);
                                    continue;
                            }


                        }
                        done = true;
                    }
                }
                catch
                {
                }

                return newValue;
            }
        }
        public override string Format
        {
            get
            {
                return base.Format;
            }
            set
            {

                base.Format = _displayStrategy.FormatChangedTo(value);
            }
        }
        class FiveStateButtonWithText : DisplayStrategy
        {
            Button _parent;
            ImageSeparator _ip;
            bool _applyImageScaling = false;
            bool _doesntHaveText = false;

            public FiveStateButtonWithText(Button parent, bool doesntHaveText)
            {
                _doesntHaveText = doesntHaveText;
                _ip = new ImageSeparator((ip) => _parent._loadImage(ip));
                _parent = parent;
                _parent.Style = Firefly.Box.UI.ButtonStyle.ImageAndText;
                _parent.AllowAlignmentChange = true;
                _parent.ImageButtonImageStructure = Firefly.Box.UI.ImageButtonImageStructure.Hover;
                if (_parent.Data.Column != null && !doesntHaveText)
                    _parent._bindText = y =>
                    {
                        y.Value = new ParseResult(_parent).Parse((_parent.Data.Value).ToString(), true);

                    };
            }

            public string FormatChangedTo(string value)
            {
                var pr = new ParseResult(_parent);
                var result = pr.Parse(value, true);
                if (!string.IsNullOrEmpty(pr.Tag) && _parent.Data.Column == null)
                {
                    if (_doesntHaveText)
                        _parent.ImageLocation = result;
                    else
                    {
                        _parent.ImageLocation = pr.ImageLocation;
                        _applyImageScaling = pr.Tag == "GB";
                    }

                }
                return result;
            }

            bool _first = true;
            public string TranslateButtonText(string newValue)
            {

                var pr = new ParseResult(_parent);
                var result = pr.Parse(newValue, true);
                if (_first)
                {
                    _first = false;
                    _parent.Format = "";
                    if (_doesntHaveText)
                        _lastImageLocation = newValue;
                    if (_parent.Data.Column == null)
                    {
                        if (_doesntHaveText)
                            _parent.ImageLocation = result;
                        else
                            _parent.ImageLocation = pr.ImageLocation;

                        result = "";
                        _applyImageScaling = pr.Tag == "GB";
                    }
                }

                return _doesntHaveText ? "" : result;

            }

            Bitmap GetButtonImage(string imageName, int imageId)
            {
                var result = _ip.GetButtonImage(imageName, imageId);
                if (result == null)
                    return null;
                if (_applyImageScaling && result.Size != _parent.ClientSize)
                {
                    var o = result;
                    result = new Bitmap(_parent.Width, _parent.Height);
                    using (var g = Graphics.FromImage(result))
                    {
                        g.FillRectangle(Brushes.Green, 0, 0, result.Width, result.Height);
                        int sw = (o.Width - 1) / 2;
                        int sh = (o.Height - 1) / 2;
                        //top Left Corner
                        g.DrawImage(o, new Rectangle(0, 0, sw, sh)
                            , new Rectangle(0, 0, sw, sh),
                            GraphicsUnit.Pixel);
                        //top border
                        g.DrawImage(o, new Rectangle(sw, 0, result.Width - sw * 2, sh)
                            , new Rectangle(sw, 0, 1, sh),
                            GraphicsUnit.Pixel);
                        //top right
                        g.DrawImage(o, new Rectangle(result.Width - sw, 0, sw, sh)
                            , new Rectangle(sw + 1, 0, sw, sh),
                            GraphicsUnit.Pixel);
                        //left border
                        g.DrawImage(o, new Rectangle(0, sh, sw, result.Height - sh * 2)
                            , new Rectangle(0, sh, sw, 1),
                            GraphicsUnit.Pixel);
                        try
                        {
                            using (var b = new System.Drawing.SolidBrush(o.GetPixel(sw, sh)))
                            {
                                g.FillRectangle(b, new Rectangle(sw, sh, result.Width - sw * 2, result.Height - sh * 2));
                            }
                        }
                        catch
                        {
                            //middle
                            g.DrawImage(o, new Rectangle(sw, sh, result.Width - sw * 2, result.Height - sh * 2)
                                , new Rectangle(sw, sh, 1, 1),
                                GraphicsUnit.Pixel);
                        }

                        //right Border
                        g.DrawImage(o, new Rectangle(result.Width - sw, sh, sw, result.Height - sh * 2)
                           , new Rectangle(sw + 1, sh, sw, 1),
                           GraphicsUnit.Pixel);

                        //bottom Left Corner
                        g.DrawImage(o, new Rectangle(0, result.Height - sh, sw, sh)
                            , new Rectangle(0, sh + 1, sw, sh),
                            GraphicsUnit.Pixel);
                        //bottom border
                        g.DrawImage(o, new Rectangle(sw, result.Height - sh, result.Width - sw * 2, sh)
                            , new Rectangle(sw, sh + 1, 1, sh),
                            GraphicsUnit.Pixel);
                        //bottom right
                        g.DrawImage(o, new Rectangle(result.Width - sw, result.Height - sh, sw, sh)
                            , new Rectangle(sw + 1, sh + 1, sw, sh),
                            GraphicsUnit.Pixel);
                    }

                }
                else if (true)
                {
                    var o = result;
                    result = new Bitmap(_parent.Width, _parent.Height);
                    using (var g = Graphics.FromImage(result))
                    {
                        g.DrawImage(o, 0, 0, result.Width, result.Height);
                    }
                }
                return result;
            }

            public void WasClicked(Action baseClick)
            {
                baseClick();
            }

            string _lastImageLocation = null;
            public Image GetImage(string imageLocation)
            {
                if (string.IsNullOrEmpty(imageLocation))
                {
                    if (string.IsNullOrEmpty(_lastImageLocation))
                        return null;
                    imageLocation = _lastImageLocation;
                }
                var pr = new ParseResult(_parent);
                var s = pr.Parse(imageLocation, true);

                if (pr.Tag == "")
                {
                    imageLocation = _lastImageLocation;
                    pr = new ParseResult(_parent);
                    s = pr.Parse(imageLocation, true);
                }

                _lastImageLocation = imageLocation;

                string image = "";
                if (pr.Tag != "" || _parent.Data.Column == null)
                {
                    if (pr.Tag != "")
                    {
                        _applyImageScaling = pr.Tag == "GB";
                        if (_doesntHaveText)
                            image = s;
                        else
                        {
                            image = pr.ImageLocation;
                        }

                    }
                    else
                        image = imageLocation;
                    if (!_applyImageScaling)
                        return _parent._loadImage(image);

                    var x = GetButtonImage(image, 0);
                    if (x == null)
                        return null;
                    var result = new Bitmap(x.Width * 5, x.Height);
                    using (var g = Graphics.FromImage(result))
                    {
                        g.DrawImage(x, 0, 0);
                        for (int i = 1; i < 5; i++)
                        {
                            g.DrawImage(GetButtonImage(image, i), x.Width * i, 0);
                        }
                    }
                    return result;
                }
                return null;

            }

            class ImageSeparator : IDisposable
            {
                System.Collections.Generic.Dictionary<string, Bitmap[]> _bitmapCache = new Dictionary<string, Bitmap[]>();

                Func<string, Image> _getImage;

                public ImageSeparator(Func<string, Image> getImage)
                {
                    _getImage = getImage;
                }

                public Bitmap GetButtonImage(string path, int index)
                {
                    var a = GetImages(path, delegate (int imageWidth) { return 5; });
                    if (a.Length > index)
                        return a[index];
                    return null;
                }

                delegate int GetImagesCountDelegate(int imageWidth);

                public Bitmap[] GetTabImages(string value)
                {
                    return GetImages(value, delegate (int imageWidth) { return (int)Math.Round((double)imageWidth / (double)16); });
                }

                Bitmap[] GetImages(string value, GetImagesCountDelegate getImagesCount)
                {
                    if (string.IsNullOrEmpty(value))
                        return new Bitmap[0];
                    Bitmap[] images;
                    if (!_bitmapCache.TryGetValue(value, out images))
                    {
                        Image sourceImage = _getImage(value);
                        if (sourceImage == null)
                            return new Bitmap[0];
                        int imageCount = getImagesCount(sourceImage.Width);
                        images = new Bitmap[imageCount];
                        float width = sourceImage.Width / imageCount;
                        int targetWidth = (int)Math.Floor(width);
                        for (int i = 0; i < imageCount; i++)
                        {
                            int left = (int)Math.Round(sourceImage.Width * i / imageCount + 0.01f);
                            System.Drawing.Bitmap b = new Bitmap(
                                Math.Min(sourceImage.Width - left, targetWidth), sourceImage.Height);
                            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(b);
                            g.DrawImage(sourceImage, new Rectangle(0, 0, b.Width, b.Height),
                                        left, 0, b.Width, b.Height, GraphicsUnit.Pixel);
                            images[i] = b;
                            g.Dispose();
                        }
                        _bitmapCache.Add(value, images);
                    }
                    return images;
                }

                public void Dispose()
                {
                    foreach (KeyValuePair<string, Bitmap[]> pair in _bitmapCache)
                    {
                        foreach (Bitmap bitmap in pair.Value)
                        {
                            bitmap.Dispose();
                        }
                    }
                }
            }
        }
        class BlinkDisplayStrategy : DisplayStrategy
        {
            Button _parent;
            Color _foreColor = Color.Black;
            System.Windows.Forms.Label _label;
            System.Windows.Forms.Timer _timer;
            public BlinkDisplayStrategy(Button parent)
            {
                _parent = parent;
                _label = new System.Windows.Forms.Label
                {
                    ForeColor = _foreColor,
                    BackColor = Color.Gray,
                    Dock = DockStyle.Fill,

                    TextAlign = ContentAlignment.TopCenter
                };
                _timer = new System.Windows.Forms.Timer { Interval = 250 };
                _parent._dispose.Add(_timer);

                bool x = false;
                _timer.Tick += delegate
                {
                    if (x = !x)
                        _label.ForeColor = _label.BackColor;
                    else
                        _label.ForeColor = _foreColor;

                };
                _timer.Start();
                _parent.Controls.Add(_label);
            }

            public string FormatChangedTo(string value)
            {
                return value;
            }
            public string TranslateButtonText(string newValue)
            {
                if (!newValue.ToUpper(CultureInfo.InvariantCulture).StartsWith("BK:"))
                    return newValue;


                var s = newValue.Substring(3);

                s = Parse(s, '[', ']', val => _timer.Interval = int.Parse(val));
                s = Parse(s, '(', ')', val => ParseColor(val, color => _label.BackColor = color));
                s = Parse(s, '{', '}', val => ParseColor(val, color => _foreColor = color));
                _label.Text = s;

                return s;
            }

            public void WasClicked(Action baseClick)
            {
                baseClick();
            }

            public Image GetImage(string imageLocation)
            {
                return null;
            }
        }
        class ZoomRightDisplayStrategy : DisplayStrategy
        {
            Button _parent;

            public ZoomRightDisplayStrategy(Button parent)
            {
                _parent = parent;
                _image = new GetImageProvider(_parent);
            }




            public string FormatChangedTo(string value)
            {

                return _image.FormatChangedTo(value);
            }
            public string TranslateButtonText(string newValue)
            {
                return _image.TranslateButtonText(newValue);
            }

            public void WasClicked(Action baseClick)
            {
                Firefly.Box.UI.TextBox tb = null;
                foreach (var control in _parent.Parent.Controls)
                {
                    var ctb = control as Firefly.Box.UI.TextBox;
                    if (ctb != null)
                    {
                        if (ctb.Left <= _parent.Left && ctb.Top <= _parent.Bottom && ctb.Bottom >= _parent.Top)
                        {
                            if (tb == null)
                                tb = ctb;
                            else if (ctb.Left > tb.Left)
                                tb = ctb;
                        }
                    }
                }
                if (tb != null)
                {
                    tb.TryFocus(() => ENV.Common.Raise(Firefly.Box.Command.Expand));
                }
            }
            GetImageProvider _image;

            public Image GetImage(string imageLocation)
            {
                return _image.GetImage(imageLocation);
            }
        }
        class GetImageProvider
        {
            string _image;
            Button _parent;
            public GetImageProvider(Button parent)
            {
                _parent = parent;
            }
            public string FormatChangedTo(string value)
            {
                return string.IsNullOrEmpty(_image) ? value : "";
            }
            public string TranslateButtonText(string newValue)
            {
                if (!string.IsNullOrEmpty(_image)) return "";
                newValue = new ParseResult(_parent).Parse(newValue, false);
                if (newValue.ToUpperInvariant() == "OPEN")
                    _image = "@get.2002";
                if (newValue.ToUpperInvariant() == "ZOOM")
                    _image = "@get.2006";
                if (newValue.ToUpperInvariant() == "CAL")
                    _image = "@get.2008";
                if (!string.IsNullOrEmpty(_image))
                {
                    _parent.Format = "";
                    _parent.Style = ButtonStyle.Image;
                    _parent.ImageButtonImageStructure = ImageButtonImageStructure.FormsDefaultButton;
                    return "";
                }
                return newValue;
            }
            public Image GetImage(string imageLocation)
            {
                if (_image == null) return null;

                var image = new Bitmap(_parent.Width * 6, _parent.Height);

                using (var image1 = ImageLoader.Load(_image))
                {
                    using (var g = Graphics.FromImage(image))
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            var bs = (Firefly.Box.UI.ButtonState)i;
                            var rect = new Rectangle(_parent.Width * i, 0, _parent.Width, _parent.Height);
                            ControlPaint.DrawButton(g, rect,
                                bs == Firefly.Box.UI.ButtonState.Clicked ? System.Windows.Forms.ButtonState.Pushed : (bs == Firefly.Box.UI.ButtonState.Disabled ? System.Windows.Forms.ButtonState.Inactive :
                                System.Windows.Forms.ButtonState.Normal));
                            if (bs == Firefly.Box.UI.ButtonState.Disabled)
                                ControlPaint.DrawImageDisabled(g, image1, rect.Left + (rect.Width - image1.Width) / 2,
                                    rect.Top + (rect.Height - image1.Height) / 2, SystemColors.ButtonFace);
                            else
                                g.DrawImageUnscaled(image1, rect.Left + (rect.Width - image1.Width) / 2,
                                    rect.Top + (rect.Height - image1.Height) / 2);
                            if (bs == Firefly.Box.UI.ButtonState.Focused || bs == Firefly.Box.UI.ButtonState.Clicked || bs == Firefly.Box.UI.ButtonState.Hovered)
                            {
                                var r = rect;
                                r.Inflate(-3, -3);
                                ControlPaint.DrawFocusRectangle(g, r);
                            }
                        }
                    }
                }

                return image;

            }
        }
        class ZoomLeftDisplayStrategy : DisplayStrategy
        {
            Button _parent;
            GetImageProvider _image;

            public ZoomLeftDisplayStrategy(Button parent)
            {
                _parent = parent;
                _image = new GetImageProvider(parent);
            }

            public string FormatChangedTo(string value)
            {
                return _image.FormatChangedTo(value);
            }
            public string TranslateButtonText(string newValue)
            {
                return _image.TranslateButtonText(newValue);
            }

            public void WasClicked(Action baseClick)
            {
                Firefly.Box.UI.TextBox tb = null;
                foreach (var control in _parent.Parent.Controls)
                {
                    var ctb = control as Firefly.Box.UI.TextBox;
                    if (ctb != null)
                    {
                        if (ctb.Right >= _parent.Right && ctb.Top <= _parent.Bottom && ctb.Bottom >= _parent.Top)
                        {
                            if (tb == null)
                                tb = ctb;
                            else if (ctb.Right < tb.Right)
                                tb = ctb;
                        }
                    }
                }
                if (tb != null)
                {
                    tb.TryFocus(() => ENV.Common.Raise(Firefly.Box.Command.Expand));
                }
            }

            public Image GetImage(string imageLocation)
            {
                return _image.GetImage(imageLocation);
            }
        }
        class BitmapDisplayButtonStrategy : DisplayStrategy
        {
            Button _parent;
            public BitmapDisplayButtonStrategy(Button parent)
            {
                _parent = parent;
                _parent.ControlAdded += new ControlEventHandler(_parent_ControlAdded);
            }

            public string FormatChangedTo(string value)
            {
                return value;
            }
            System.Windows.Forms.Button _button = null;
            void _parent_ControlAdded(object sender, ControlEventArgs e)
            {
                _button = e.Control as System.Windows.Forms.Button;
                SetButtonImage();
            }

            void SetButtonImage()
            {
                if (_button != null)
                {
                    try
                    {
                        _button.Image = System.Drawing.Bitmap.FromFile(_prevValue.Substring(3));
                    }
                    catch
                    {
                    }
                }
            }

            string _prevValue = null;
            public string TranslateButtonText(string newValue)
            {

                if (newValue != _prevValue)
                {
                    _prevValue = newValue;
                    SetButtonImage();
                }
                return "";
            }

            public void WasClicked(Action baseClick)
            {
                baseClick();
            }

            public Image GetImage(string imageLocation)
            {
                return null;
            }
        }
        class ProgressBarButtonStrategy : DisplayStrategy
        {
            Button _parent;
            System.Windows.Forms.ProgressBar _pb;
            public ProgressBarButtonStrategy(Button parent)
            {
                _parent = parent;
                _pb = new ProgressBar();
                _pb.Dock = DockStyle.Fill;
                _parent.Controls.Add(_pb);
                _pb.Maximum = 100;

            }

            public string FormatChangedTo(string value)
            {
                return value;
            }
            public string TranslateButtonText(string newValue)
            {
                try
                {
                    if (string.IsNullOrEmpty(newValue))
                        _pb.Value = 0;
                    else
                    {
                        if (newValue.Length > 3)
                            newValue = newValue.Substring(3);
                        _pb.Value = Firefly.Box.Number.Parse(newValue);
                    }

                }
                catch
                {

                }
                return "";
            }

            public void WasClicked(Action baseClick)
            {

            }

            public Image GetImage(string imageLocation)
            {
                return null;
            }
        }
        class IconDisplayStrategy : DisplayStrategy
        {
            Button _parent;

            System.Windows.Forms.PictureBox _picture;
            public IconDisplayStrategy(Button parent)
            {
                _parent = parent;
                _picture = new System.Windows.Forms.PictureBox { Dock = DockStyle.Fill };
                _parent.Controls.Add(_picture);
            }

            public string FormatChangedTo(string value)
            {
                return value;
            }
            public string TranslateButtonText(string newValue)
            {
                if (newValue.Length < 3)
                    return newValue;
                newValue = newValue.Substring(3);
                switch (newValue.ToUpper(CultureInfo.InvariantCulture))
                {
                    case "EXCLAMATION":
                        _picture.Image = SystemIcons.Exclamation.ToBitmap();
                        break;
                    case "HAND":
                        _picture.Image = SystemIcons.Hand.ToBitmap();
                        break;
                    case "ASTERISK":
                        _picture.Image = SystemIcons.Asterisk.ToBitmap();
                        break;
                    default:
                        if (File.Exists(newValue))
                            _picture.Image = Common.GetImage(newValue);
                        else
                            _picture.Image = SystemIcons.Error.ToBitmap();
                        break;
                }
                if (newValue.StartsWith("user32.dll,", StringComparison.InvariantCultureIgnoreCase))
                {
                    switch (newValue[newValue.Length - 1])
                    {
                        case '1':
                            _picture.Image = SystemIcons.Exclamation.ToBitmap();
                            break;
                    }
                }
                return "";
            }

            public void WasClicked(Action baseClick)
            {
                baseClick();
            }

            public Image GetImage(string imageLocation)
            {
                return null;
            }
        }
        class DigitalClockDisplayStrategy : DisplayStrategy
        {
            Button _parent;

            System.Windows.Forms.Label _label;
            System.Windows.Forms.Timer _timer;
            public DigitalClockDisplayStrategy(Button parent)
            {
                _parent = parent;
                _label = new System.Windows.Forms.Label
                {

                    BackColor = Color.Black,
                    ForeColor = Color.LightGreen,
                    RightToLeft = System.Windows.Forms.RightToLeft.No,
                    Font = new Font("Arial", 14),
                    Text = Firefly.Box.Time.Now.ToString("HH:MM"),
                    Dock = DockStyle.Fill,

                    TextAlign = ContentAlignment.TopCenter
                };
                _timer = new System.Windows.Forms.Timer { Interval = 500 };
                _parent._dispose.Add(_timer);

                bool x = false;
                _timer.Tick += delegate
                {
                    if (x = !x)
                        _label.Text = Firefly.Box.Time.Now.ToString("HH:MM");
                    else
                        _label.Text = Firefly.Box.Time.Now.ToString("HH MM");

                };
                _timer.Start();
                _parent.Controls.Add(_label);
            }

            public string FormatChangedTo(string value)
            {
                return value;
            }

            public string TranslateButtonText(string newValue)
            {
                return newValue;

            }

            public void WasClicked(Action baseClick)
            {
                baseClick();
            }

            public Image GetImage(string imageLocation)
            {
                return null;
            }
        }
        class AnalogClockDisplayStrategy : DisplayStrategy
        {
            Button _parent;


            public AnalogClockDisplayStrategy(Button parent)
            {
                _parent = parent;
                _parent.Controls.Add(new AnalogClock() { Dock = DockStyle.Fill });
            }

            public string FormatChangedTo(string value)
            {
                return value;
            }
            public string TranslateButtonText(string newValue)
            {
                return newValue;

            }

            public void WasClicked(Action baseClick)
            {
                baseClick();
            }

            public Image GetImage(string imageLocation)
            {
                return null;
            }

            class AnalogClock : System.Windows.Forms.UserControl
            {
                /// <summary>
                /// Control name: Analog Clock Control
                /// Description: A customizable and resizable clock control
                /// Developed by: Syed Mehroz Alam
                /// Email: smehrozalam@yahoo.com
                /// URL: Programming Home "http://www.geocities.com/smehrozalam/"
                /// </summary>
                const float PI = 3.141592654F;

                DateTime dateTime;

                float fRadius;
                float fCenterX;
                float fCenterY;
                float fCenterCircleRadius;

                float fHourLength;
                float fMinLength;
                float fSecLength;

                float fHourThickness;
                float fMinThickness;
                float fSecThickness;

                bool bDraw5MinuteTicks = true;
                bool bDraw1MinuteTicks = true;
                float fTicksThickness = 1;

                Color hrColor = Color.DarkMagenta;
                Color minColor = Color.Green;
                Color secColor = Color.Red;
                Color circleColor = Color.Red;
                Color ticksColor = Color.Black;

                private System.Windows.Forms.Timer timer1;
                private System.ComponentModel.IContainer components;

                public AnalogClock()
                {
                    // This call is required by the Windows.Forms Form Designer.
                    InitializeComponent();
                    DoubleBuffered = true;

                }

                /// <summary> 
                /// Clean up any resources being used.
                /// </summary>
                protected override void Dispose(bool disposing)
                {
                    if (disposing)
                    {
                        if (components != null)
                        {
                            components.Dispose();
                        }
                    }
                    base.Dispose(disposing);
                }

                #region Component Designer generated code
                /// <summary> 
                /// Required method for Designer support - do not modify 
                /// the contents of this method with the code editor.
                /// </summary>
                private void InitializeComponent()
                {
                    this.components = new System.ComponentModel.Container();
                    this.timer1 = new System.Windows.Forms.Timer(this.components);
                    // 
                    // timer1
                    // 
                    this.timer1.Enabled = true;
                    this.timer1.Interval = 1000;
                    this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
                    // 
                    // AnalogClock
                    // 
                    this.Name = "AnalogClock";
                    this.Resize += new System.EventHandler(this.AnalogClock_Resize);
                    this.Load += new System.EventHandler(this.AnalogClock_Load);
                    this.Paint += new System.Windows.Forms.PaintEventHandler(this.AnalogClock_Paint);

                }
                #endregion

                private void AnalogClock_Load(object sender, System.EventArgs e)
                {
                    dateTime = DateTime.Now;
                    this.AnalogClock_Resize(sender, e);
                }

                private void timer1_Tick(object sender, System.EventArgs e)
                {
                    this.dateTime = DateTime.Now;
                    this.Refresh();
                }

                public void Start()
                {
                    timer1.Enabled = true;
                    this.Refresh();
                }

                public void Stop()
                {
                    timer1.Enabled = false;
                }

                private void DrawLine(float fThickness, float fLength, Color color, float fRadians, System.Windows.Forms.PaintEventArgs e)
                {
                    e.Graphics.DrawLine(new Pen(color, fThickness),
                        fCenterX - (float)(fLength / 9 * System.Math.Sin(fRadians)),
                        fCenterY + (float)(fLength / 9 * System.Math.Cos(fRadians)),
                        fCenterX + (float)(fLength * System.Math.Sin(fRadians)),
                        fCenterY - (float)(fLength * System.Math.Cos(fRadians)));
                }

                private void DrawPolygon(float fThickness, float fLength, Color color, float fRadians, System.Windows.Forms.PaintEventArgs e)
                {

                    PointF A = new PointF((float)(fCenterX + fThickness * 2 * System.Math.Sin(fRadians + PI / 2)),
                        (float)(fCenterY - fThickness * 2 * System.Math.Cos(fRadians + PI / 2)));
                    PointF B = new PointF((float)(fCenterX + fThickness * 2 * System.Math.Sin(fRadians - PI / 2)),
                        (float)(fCenterY - fThickness * 2 * System.Math.Cos(fRadians - PI / 2)));
                    PointF C = new PointF((float)(fCenterX + fLength * System.Math.Sin(fRadians)),
                        (float)(fCenterY - fLength * System.Math.Cos(fRadians)));
                    PointF D = new PointF((float)(fCenterX - fThickness * 4 * System.Math.Sin(fRadians)),
                        (float)(fCenterY + fThickness * 4 * System.Math.Cos(fRadians)));
                    PointF[] points = { A, D, B, C };
                    e.Graphics.FillPolygon(new SolidBrush(color), points);
                }

                private void AnalogClock_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
                {
                    float fRadHr = (dateTime.Hour % 12 + dateTime.Minute / 60F) * 30 * PI / 180;
                    float fRadMin = (dateTime.Minute) * 6 * PI / 180;
                    float fRadSec = (dateTime.Second) * 6 * PI / 180;
                    e.Graphics.Clear(BackColor);
                    DrawPolygon(this.fHourThickness, this.fHourLength, hrColor, fRadHr, e);
                    DrawPolygon(this.fMinThickness, this.fMinLength, minColor, fRadMin, e);
                    DrawLine(this.fSecThickness, this.fSecLength, secColor, fRadSec, e);


                    for (int i = 0; i < 60; i++)
                    {
                        if (this.bDraw5MinuteTicks == true && i % 5 == 0) // Draw 5 minute ticks
                        {
                            e.Graphics.DrawLine(new Pen(ticksColor, fTicksThickness),
                                fCenterX + (float)(this.fRadius / 1.50F * System.Math.Sin(i * 6 * PI / 180)),
                                fCenterY - (float)(this.fRadius / 1.50F * System.Math.Cos(i * 6 * PI / 180)),
                                fCenterX + (float)(this.fRadius / 1.65F * System.Math.Sin(i * 6 * PI / 180)),
                                fCenterY - (float)(this.fRadius / 1.65F * System.Math.Cos(i * 6 * PI / 180)));
                        }
                        else if (this.bDraw1MinuteTicks == true) // draw 1 minute ticks
                        {
                            e.Graphics.DrawLine(new Pen(ticksColor, fTicksThickness),
                                fCenterX + (float)(this.fRadius / 1.50F * System.Math.Sin(i * 6 * PI / 180)),
                                fCenterY - (float)(this.fRadius / 1.50F * System.Math.Cos(i * 6 * PI / 180)),
                                fCenterX + (float)(this.fRadius / 1.55F * System.Math.Sin(i * 6 * PI / 180)),
                                fCenterY - (float)(this.fRadius / 1.55F * System.Math.Cos(i * 6 * PI / 180)));
                        }
                    }

                    //draw circle at center
                    e.Graphics.FillEllipse(new SolidBrush(circleColor), fCenterX - fCenterCircleRadius / 2, fCenterY - fCenterCircleRadius / 2, fCenterCircleRadius, fCenterCircleRadius);
                }

                private void AnalogClock_Resize(object sender, System.EventArgs e)
                {
                    //  this.Width = this.Height;
                    this.fRadius = this.Height / 2;
                    this.fCenterX = this.ClientSize.Width / 2;
                    this.fCenterY = this.ClientSize.Height / 2;
                    this.fHourLength = (float)this.Height / 3 / 1.65F;
                    this.fMinLength = (float)this.Height / 3 / 1.20F;
                    this.fSecLength = (float)this.Height / 3 / 1.15F;
                    this.fHourThickness = (float)this.Height / 100;
                    this.fMinThickness = (float)this.Height / 150;
                    this.fSecThickness = (float)this.Height / 200;
                    this.fCenterCircleRadius = this.Height / 50;
                    this.Refresh();
                }

                public Color HourHandColor
                {
                    get { return this.hrColor; }
                    set { this.hrColor = value; }
                }

                public Color MinuteHandColor
                {
                    get { return this.minColor; }
                    set { this.minColor = value; }
                }

                public Color SecondHandColor
                {
                    get { return this.secColor; }
                    set
                    {
                        this.secColor = value;
                        this.circleColor = value;
                    }
                }

                public Color TicksColor
                {
                    get { return this.ticksColor; }
                    set { this.ticksColor = value; }
                }

                public bool Draw1MinuteTicks
                {
                    get { return this.bDraw1MinuteTicks; }
                    set { this.bDraw1MinuteTicks = value; }
                }

                public bool Draw5MinuteTicks
                {
                    get { return this.bDraw5MinuteTicks; }
                    set { this.bDraw5MinuteTicks = value; }
                }

            }
        }


        DisplayStrategy _displayStrategy;



        protected override void OnClick(ButtonClickEventArgs e)
        {
            if (EnableSpecialSubClassing1 && Text == "...")
            {
                Firefly.Box.UI.TextBox tb = null;
                foreach (var control in Parent.Controls)
                {
                    var ctb = control as Firefly.Box.UI.TextBox;
                    if (ctb != null)
                    {
                        if (ctb.Left <= Left && ctb.Top <= Bottom && ctb.Bottom >= Top)
                        {
                            if (tb == null)
                                tb = ctb;
                            else if (ctb.Left > tb.Left)
                                tb = ctb;
                        }
                    }
                }
                if (tb != null)
                {
                    tb.TryFocus(() => ENV.Common.Raise(Firefly.Box.Command.Expand));
                    return;
                }
            }
            _displayStrategy.WasClicked(() => base.OnClick(e));
        }
        public override Firefly.Box.UI.FontScheme FontScheme
        {
            get
            {
                return base.FontScheme;
            }
            set
            {
                base.FontScheme = FaceLiftDemo.MatchFontScheme(value);
            }
        }

        void SetAsFormsDefaultImageAndTextButton()
        {
            Style = ButtonStyle.ImageAndText;
            ImageButtonImageStructure = ImageButtonImageStructure.FormsDefaultButton;
            Cursor = Cursors.Hand;
            _ignoreGridRowForeColorInImageButton = true;
        }

        class IconAndText : DisplayStrategy
        {
            Button _parent;
            string _text = "";
            ContentAlignment _iconAlignment;
            string _iconLocation;
            bool _myCoolIsWorking = false;
            string _coolText = "";
            public IconAndText(Button parent, string iconLocation, ContentAlignment iconAlignment)
            {
                _parent = parent;
                _parent.SetAsFormsDefaultImageAndTextButton();
                _myCoolIsWorking = _parent._coolIsWorking;
                _parent._coolIsWorking = false;
                _iconLocation = iconLocation;
                _iconAlignment = iconAlignment;
            }

            public string FormatChangedTo(string value)
            {
                _parent._displayStrategy = new SortDisplayStrategy(_parent);
                return value;
            }
            public string TranslateButtonText(string newValue)
            {
                if (_myCoolIsWorking) _coolText = newValue;
                return "";
            }


            public void WasClicked(Action baseClick)
            {
                baseClick();
            }

            public Image GetImage(string imageLocation)
            {
                var path = "";

                var p = new ParseResult(_parent);
                var parseResult = p.Parse(imageLocation, true);
                path = string.IsNullOrEmpty(_iconLocation) ? p.ImageLocation : _iconLocation;
                string text = "";
                if (_parent.Data != null)
                    text = (_parent.Data.Column ?? new ENV.Data.TextColumn()).ToString(string.IsNullOrEmpty(_parent.Format) ? null : _parent.Format).Trim();
                else
                    text = _parent.Format;
                p = new ParseResult(_parent);
                text = p.Parse(text, false);

                if (_myCoolIsWorking)
                    text = _coolText;
                else if (string.IsNullOrEmpty(_iconLocation) || string.IsNullOrEmpty(text))
                    text = parseResult.TrimEnd();

                if (string.IsNullOrEmpty(path))
                {
                    path = text;
                    text = "";
                }
                text = ENV.Languages.Translate(text);
                var image = new Bitmap(_parent.Width * 6, _parent.Height);
                using (var g = Graphics.FromImage(image))
                {
                    var f = _parent.FindForm();
                    var bc = _parent.BackColor;
                    while ((bc == Color.Empty || bc == Color.Transparent) && f != null)
                    {
                        bc = f.BackColor;
                        if (f.Parent == null)
                            break;
                        f = f.Parent.FindForm();
                    }

                    using (var b = new SolidBrush(bc != Color.Empty && bc != Color.Transparent ? bc : SystemColors.Control))
                        g.FillRectangle(b, new Rectangle(Point.Empty, image.Size));

                    var iconSize = 16;

                    var icon = ENV.Common.GetImage(path, Size.Empty);

                    if (icon != null)
                        iconSize = icon.Width;

                    for (int i = 0; i < 6; i++)
                    {
                        var left = _parent.Width * i;
                        var textBounds = new Rectangle(left + 3, 3, _parent.Width - 6, _parent.Height - 6);
                        var iconBounds = new Rectangle(left + (_parent.Width - iconSize) / 2, (_parent.Height - iconSize) / 2,
                            iconSize, iconSize);
                        if (_iconAlignment == ContentAlignment.MiddleLeft)
                        {
                            iconBounds = new Rectangle(left + 2, (_parent.Height - iconSize) / 2, iconSize, iconSize);
                            textBounds.X += 17;
                            textBounds.Width = textBounds.Width > 17 ? textBounds.Width - 17 : 0;
                        }
                        else if (_iconAlignment == ContentAlignment.MiddleRight)
                        {
                            iconBounds = new Rectangle(left + _parent.Width - iconSize - 4, (_parent.Height - iconSize) / 2, iconSize, iconSize);
                            textBounds.Width = textBounds.Width > 17 ? textBounds.Width - 17 : 0;
                        }
                        else if (_iconAlignment == ContentAlignment.TopCenter)
                        {
                            iconBounds = new Rectangle(left + (_parent.Width - iconSize) / 2, 2, iconSize, iconSize);
                            var deflateTextBoundsFromTop = Math.Min(iconSize + 1, textBounds.Height);
                            textBounds.Y += deflateTextBoundsFromTop;
                            textBounds.Height -= deflateTextBoundsFromTop;
                        }
                        var drawBorder = !p.FlatStyle || (i < 2 || i > 3);
                        using (var borderPen = new System.Drawing.Pen(Color.FromArgb(171, 171, 171)))
                            if (icon != null)
                            {
                                if (_myCoolIsWorking)
                                {
                                    _parent.DrawCoolButton((gg, r) => gg.DrawImage(icon, iconBounds), g, borderPen, i);
                                }
                                else if (drawBorder)
                                    ButtonRenderer.DrawButton(g, new Rectangle(left, 0, _parent.Width, _parent.Height),
                                        icon, iconBounds,
                                    i == 0,
                                    new[] { PushButtonState.Normal, PushButtonState.Pressed, PushButtonState.Disabled, PushButtonState.Normal, PushButtonState.Hot, PushButtonState.Default }[i]);
                                else
                                    g.DrawImage(icon, iconBounds);
                            }
                            else
                            {
                                if (_myCoolIsWorking)
                                    _parent.DrawCoolButton((gg, r) => { }, g, borderPen, i);
                                else if (drawBorder)
                                    ButtonRenderer.DrawButton(g, new Rectangle(left, 0, _parent.Width, _parent.Height), i == 0,
                                        new[] { PushButtonState.Normal, PushButtonState.Pressed, PushButtonState.Disabled, PushButtonState.Normal, PushButtonState.Hot, PushButtonState.Default }[i]);
                            }

                        if (p.BackColor != Color.Empty && p.BackColor != SystemColors.ButtonFace)
                        {
                            using (var b = new SolidBrush(p.BackColor))
                                g.FillRectangle(b, textBounds);
                        }

                        if (textBounds.Width * textBounds.Height != 0 && _iconAlignment != ContentAlignment.MiddleCenter)
                        {
                            var sf = new StringFormat()
                            {
                                LineAlignment = StringAlignment.Center,
                                HotkeyPrefix = HotkeyPrefix.Show,
                                FormatFlags = StringFormatFlags.NoWrap,
                                Trimming = StringTrimming.None
                            };
                            switch (_parent.Alignment)
                            {
                                case System.Drawing.ContentAlignment.BottomLeft:
                                case System.Drawing.ContentAlignment.MiddleLeft:
                                case System.Drawing.ContentAlignment.TopLeft:
                                    sf.Alignment = StringAlignment.Near;
                                    break;
                                case System.Drawing.ContentAlignment.BottomCenter:
                                case System.Drawing.ContentAlignment.MiddleCenter:
                                case System.Drawing.ContentAlignment.TopCenter:
                                    sf.Alignment = StringAlignment.Center;
                                    break;
                                case System.Drawing.ContentAlignment.BottomRight:
                                case System.Drawing.ContentAlignment.MiddleRight:
                                case System.Drawing.ContentAlignment.TopRight:
                                    sf.Alignment = StringAlignment.Far;
                                    break;
                            }
                            if (_iconAlignment == ContentAlignment.TopCenter)
                                sf.Alignment = StringAlignment.Center;
                            using (var b = new SolidBrush(i != 4 && _myCoolIsWorking ? _parent.CoolNormalTextColor : _parent.ForeColor))
                                g.DrawString(text, _parent.Font, b, textBounds, sf);
                        }
                    }
                }

                // make disabled part grayscale
                for (int i = _parent.Width * 2; i < _parent.Width * 3; i++)
                {
                    for (int j = 0; j < _parent.Height; j++)
                    {
                        var c = image.GetPixel(i, j);
                        var gray = (int)((c.R * .3) + (c.G * .59) + (c.B * .11));
                        image.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                    }
                }
                return image;
            }
        }


        class IconPathAndAlignment
        {
            public string Path;
            public ContentAlignment Alignment;
        }

        static Dictionary<string, IconPathAndAlignment> _iconsForSpecialSubclassing =
            new Dictionary<string, IconPathAndAlignment>();
        public static void LoadIconsIniFileForSpecialSubclassing(string path)
        {
            try
            {
                using (var r = new StreamReader(PathDecoder.DecodePath(path), ENV.LocalizationInfo.Current.OuterEncoding))
                {
                    var section = "";
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        while (line.TrimEnd(' ').EndsWith("+"))
                        {
                            line = line.TrimEnd(' ');
                            line = line.Remove(line.Length - 1);
                            line += r.ReadLine();
                        }
                        if (line.StartsWith("["))
                        {
                            section = line.Substring(1, line.IndexOf(']') - 1);
                        }
                        else
                        {
                            if (line.Trim() == string.Empty)
                                continue;
                            if (line.IndexOf("=") == -1)
                                continue;
                            var name = line.Remove(line.IndexOf("=")).Trim();
                            var value = line.Substring(line.IndexOf("=") + 1).Trim();

                            if (!string.IsNullOrEmpty(value))
                            {
                                if (value.IndexOf(".dll!", StringComparison.InvariantCultureIgnoreCase) != -1)
                                    value = "@" + value.Replace(".dll!", ".");
                                IconPathAndAlignment ipp = null;
                                if (!(_iconsForSpecialSubclassing.TryGetValue(name, out ipp) && ipp.Alignment == ContentAlignment.MiddleLeft))
                                {
                                    _iconsForSpecialSubclassing[name] = new IconPathAndAlignment()
                                    {
                                        Path = value,
                                        Alignment = section == "Left" ? ContentAlignment.MiddleLeft : section == "Top" ? ContentAlignment.TopCenter : ContentAlignment.MiddleCenter
                                    };
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e);
            }
        }
        internal static bool DontAllowBlank = false;
        internal static Color SpecialSubClassingHyperLinkMouseEnterForeColor = Color.Empty;
        internal static bool SpecialSubClassingHyperLinkUnderline;
        internal static ContentAlignment SpecialSubClassingHyperLinkAlignment = ContentAlignment.MiddleCenter;

        internal static void GetDllSetButton(string arg)
        {
            if (arg == "LOCATE")
            {
                DontAllowBlank = true;
            }
        }

        class ScrollButton : DisplayStrategy
        {
            Button _parent;
            System.Windows.Forms.ScrollButton _scrollButton;

            public ScrollButton(Button parent, System.Windows.Forms.ScrollButton scrollButton)
            {
                _parent = parent;
                _scrollButton = scrollButton;
                _parent.SetAsFormsDefaultImageAndTextButton();
            }

            public string FormatChangedTo(string value)
            {
                return value;
            }
            public string TranslateButtonText(string newValue)
            {
                return "";
            }

            public void WasClicked(Action baseClick)
            {
                baseClick();
            }

            public Image GetImage(string imageLocation)
            {
                var image = new Bitmap(_parent.Width * 6, _parent.Height);

                using (var g = Graphics.FromImage(image))
                {
                    using (var b = new SolidBrush(_parent.BackColor))
                        g.FillRectangle(b, new Rectangle(Point.Empty, image.Size));

                    for (int i = 0; i < 6; i++)
                    {
                        var left = _parent.Width * i;

                        ControlPaint.DrawScrollButton(g, new Rectangle(left, 0, _parent.Width, _parent.Height), _scrollButton,
                                new[] { System.Windows.Forms.ButtonState.Normal, System.Windows.Forms.ButtonState.Pushed, System.Windows.Forms.ButtonState.Inactive, System.Windows.Forms.ButtonState.Normal, System.Windows.Forms.ButtonState.Normal, System.Windows.Forms.ButtonState.Normal }[i]);
                    }
                }
                return image;
            }
        }


        class ImageButtonDisplayStrategy : DisplayStrategy
        {
            Button _parent;
            public ImageButtonDisplayStrategy(Button parent)
            {
                _parent = parent;
                _parent.Style = ButtonStyle.Image;
            }
            public string FormatChangedTo(string value)
            {
                return value;
            }

            public Image GetImage(string imageLocation)
            {
                return ENV.Common.GetImage(new ParseResult(_parent).Parse(imageLocation, false));
            }

            public string TranslateButtonText(string newValue)
            {
                return newValue;
            }

            public void WasClicked(Action baseClick)
            {
                baseClick();
            }
        }
    }

}
