using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Firefly.Box.Printing.Advanced
{
    [ComVisible(true)]
    [DefaultProperty("Document")]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    class MyPreviewControl : Control
    {
        private static readonly object EVENT_STARTPAGECHANGED = new object();
        private Size virtualSize = new Size(1, 1);
        private Point position = new Point(0, 0);
        private int rows = 1;
        private int columns = 1;
        private bool autoZoom = true;
        private Size imageSize = Size.Empty;
        private Point screendpi = new Point(96, 96);
        private double zoom = 0.3;
        private const int SCROLL_PAGE = 100;
        private const int SCROLL_LINE = 5;
        private const double DefaultZoom = 0.3;
        private const int border = 10;
        private Point lastOffset;
        Size _physicalSize = Size.Empty;

        Size GetPhysicalSize()
        {
            if (_physicalSize == Size.Empty)
                _physicalSize = GetPageSizeWithoutMargins();
            return _physicalSize;
        }

        Size GetPageSizeWithoutMargins()
        {
            var g = GetMeasurementGraphics();
            var hdc = g.GetHdc();
            try
            {
                var vertres = GetDeviceCaps(hdc, 10/*VERTRES*/);
                var logpixels = GetDeviceCaps(hdc, 90 /*LOGPIXELSY*/);

                return new Size(GetDeviceCaps(hdc, 8 /*HORZRES*/) * 100 / GetDeviceCaps(hdc, 88 /*LOGPIXELSX*/),(int) Math.Ceiling((double) vertres * 100 / logpixels));
            }
            finally
            {
                g.ReleaseHdc(hdc);
            }
        }

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

        internal int GetPageHeight()
        {
            return GetPhysicalSize().Height;
        }

        const int _numberOfPagesToPrint = 1;

        private bool layoutOk;
        private bool pageInfoCalcPending;
        private bool exceptionPrinting;

        [DefaultValue(true)]
        public bool AutoZoom
        {
            get
            {
                return this.autoZoom;
            }
            set
            {
                if (this.autoZoom == value)
                    return;
                var x = StartPage;
                this.autoZoom = value;
                this.InvalidateLayout();
                ComputeLayout();
                StartPage = x;
            }
        }
        [DefaultValue(0)]
        public int StartPage
        {
            get
            {
                if (_pages.Count > 0)
                {
                    if (position.Y == this.virtualSize.Height - this.Height)
                        return _pages.Count - 1;
                    return Math.Min(_pages.Count - 1, Position.Y / GetPageSizeWithBorders().Y * columns);
                }
                return 0;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("StartPage", "");
                }
                else
                {
                    if (value == this.StartPage)
                        return;
                    Position = new Point(0, GetPageSizeWithBorders().Y * (value / columns));
                }
            }
        }


        [DefaultValue(1)]
        public int Columns
        {
            get
            {
                return this.columns;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("Columns", "");
                }
                else
                {
                    this.columns = value;
                    CalcRows();
                    this.InvalidateLayout();
                }
            }
        }

        protected override CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.Style |= 1048576;
                createParams.Style |= 2097152;
                return createParams;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Point Position
        {
            get
            {
                return this.position;
            }
            set
            {
                if (value.Y > virtualSize.Height)
                    value.Y = virtualSize.Height;
                this.SetPositionNoInvalidate(value);
            }
        }

        long _lastRowsInvalidateTick = -1;
        [DefaultValue(1)]
        public int Rows
        {
            get
            {
                return this.rows;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("Rows", "");
                }
                else
                {
                    this.rows = value;
                    layoutOk = false;
                    if (System.Environment.TickCount - _lastRowsInvalidateTick > 500)
                    {
                        this.InvalidateLayout();
                        _lastRowsInvalidateTick = System.Environment.TickCount;

                    }
                }
            }
        }



        [Localizable(true)]
        [AmbientValue(RightToLeft.Inherit)]
        public override RightToLeft RightToLeft
        {
            get
            {
                return base.RightToLeft;
            }
            set
            {
                base.RightToLeft = value;
                this.InvalidatePreview();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Bindable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }








        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        private Size VirtualSize
        {
            get
            {
                return this.virtualSize;
            }
            set
            {
                this.SetVirtualSizeNoInvalidate(value);
                this.Invalidate();
            }
        }

        [DefaultValue(0.3)]
        public double Zoom
        {
            get
            {
                return this.zoom;
            }
            set
            {
                if (value <= 0.0)
                    throw new ArgumentException("");
                this.autoZoom = false;
                var x = StartPage;
                this.zoom = value;
                this.InvalidateLayout();
                ComputeLayout();
                StartPage = x;
            }
        }

        public double MaxRasterZoom { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public new event EventHandler TextChanged
        {
            add
            {
                base.TextChanged += value;
            }
            remove
            {
                base.TextChanged -= value;
            }
        }

        public event EventHandler StartPageChanged
        {
            add
            {
                this.Events.AddHandler(EVENT_STARTPAGECHANGED, value);
            }
            remove
            {
                this.Events.RemoveHandler(EVENT_STARTPAGECHANGED, value);
            }
        }




        List<PrintPage> _pages = new List<PrintPage>();
        PrintDocumentPrintJob.MyPrintDocument _document;
        public MyPreviewControl(PrintDocumentPrintJob.MyPrintDocument document, List<PrintPage> pages)
        {
            _document = document;
            _pages = pages;
            this.ResetBackColor();
            this.ResetForeColor();
            this.Size = new Size(100, 100);
            this.SetStyle(ControlStyles.ResizeRedraw, false);
            this.SetStyle(ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer, true);
        }

        private int AdjustScroll(Message m, int pos, int maxPos, bool horizontal)
        {
            switch (LOWORD(m.WParam))
            {
                case 0:
                    if (pos > 5)
                    {
                        pos -= 5;
                        break;
                    }
                    else
                    {
                        pos = 0;
                        break;
                    }
                case 1:
                    if (pos < maxPos - 5)
                    {
                        pos += 5;
                        break;
                    }
                    else
                    {
                        pos = maxPos;
                        break;
                    }
                case 2:
                    if (pos > 100)
                    {
                        pos -= 100;
                        break;
                    }
                    else
                    {
                        pos = 0;
                        break;
                    }
                case 3:
                    if (pos < maxPos - 100)
                    {
                        pos += 100;
                        break;
                    }
                    else
                    {
                        pos = maxPos;
                        break;
                    }
                case 4:
                case 5:
                    var si = new SCROLLINFO();
                    si.cbSize = Marshal.SizeOf(typeof(SCROLLINFO));
                    si.fMask = 16;
                    int fnBar = horizontal ? 0 : 1;
                    pos = !GetScrollInfo(new HandleRef((object)this, m.HWnd), fnBar, si) ? HIWORD(m.WParam) : si.nTrackPos;
                    break;
            }
            return pos;
        }
        static int LOWORD(int n)
        {
            return n & (int)ushort.MaxValue;
        }
        static int LOWORD(IntPtr n)
        {
            return LOWORD((int)(long)n);
        }
        static int HIWORD(int n)
        {
            return n >> 16 & (int)ushort.MaxValue;
        }

        static int HIWORD(IntPtr n)
        {
            return HIWORD((int)(long)n);
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool GetScrollInfo(HandleRef hWnd, int fnBar, [In, Out] SCROLLINFO si);
        [StructLayout(LayoutKind.Sequential)]
        class SCROLLINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(SCROLLINFO));
            public int fMask;
            public int nMin;
            public int nMax;
            public int nPage;
            public int nPos;
            public int nTrackPos;

            public SCROLLINFO()
            {
            }

            public SCROLLINFO(int mask, int min, int max, int page, int pos)
            {
                this.fMask = mask;
                this.nMin = min;
                this.nMax = max;
                this.nPage = page;
                this.nPos = pos;
            }
        }
        bool _firstTime = true;
        private void ComputeLayout()
        {
            this.layoutOk = true;
            if (this._pages.Count == 0)
            {
                this.ClientSize = this.Size;
            }
            else
            {
                CalculateZoom();
                var x = GetPageSizeWithBorders();
                var s = new Size(x.X * columns + 10, x.Y * rows + 10);
                SetVirtualSizeNoInvalidate(s);
            }
        }

        private void InvalidateLayout()
        {
            this.layoutOk = false;
            this.Invalidate();
        }

        public void InvalidatePreview()
        {
            this.InvalidateLayout();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            this.InvalidateLayout();
            base.OnResize(eventargs);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_mg != null)
            {
                _mg.Dispose();
                _mg = null;
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            using (var brush = (Brush)new SolidBrush(this.BackColor))
            {
                try
                {
                    int i = 0;
                    {
                        if (_firstTime)
                        {
                            Graphics graphicsInternal = this.CreateGraphics();
                            this.screendpi = new Point((int)graphicsInternal.DpiX, (int)graphicsInternal.DpiY);
                            graphicsInternal.Dispose();
                            _firstTime = false;
                        }

                        if (!this.layoutOk)
                            this.ComputeLayout();

                        Point virtualSize = new Point(this.VirtualSize);
                        Point point2 = new Point(Math.Max(0, (this.Size.Width - virtualSize.X) / 2),
                                                 Math.Max(0, (this.Size.Height - virtualSize.Y) / 2));
                        point2.X -= this.Position.X;
                        point2.Y -= this.Position.Y;
                        this.lastOffset = point2;
                        int num1 = PhysicalToPixels(10, this.screendpi.X);
                        int num2 = MyPreviewControl.PhysicalToPixels(10, this.screendpi.Y);
                        Region clip = pevent.Graphics.Clip;
                        Rectangle[] rectangleArray = new Rectangle[_pages.Count];

                        var pageSizeInPixels = GetPageSizeInPixel();

                        Point currentPagePosition = Point.Empty;
                        try
                        {
                            for (int currentRow = 0; currentRow < this.rows; ++currentRow)
                            {
                                currentPagePosition.X = 0;
                                currentPagePosition.Y = pageSizeInPixels.Y * currentRow;
                                for (int currentColumn = 0; currentColumn < this.columns; ++currentColumn)
                                {
                                    int currentPage = currentColumn + currentRow * this.columns;
                                    if (currentPage < rectangleArray.Length)
                                    {
                                        int x = point2.X + num1 * (currentColumn + 1) + currentPagePosition.X;
                                        if (RightToLeft == System.Windows.Forms.RightToLeft.Yes)
                                        {
                                            x = virtualSize.X - pageSizeInPixels.X - x + point2.X * 2;
                                        }


                                        int y = point2.Y + num2 * (currentRow + 1) + currentPagePosition.Y;
                                        currentPagePosition.X += pageSizeInPixels.X;

                                        rectangleArray[currentPage] = new Rectangle(x, y, pageSizeInPixels.X, pageSizeInPixels.Y);
                                        if (!rectangleArray[currentPage].IntersectsWith(ClientRectangle))
                                            continue;
                                        pevent.Graphics.ExcludeClip(rectangleArray[currentPage]);
                                    }
                                }
                            }
                            pevent.Graphics.FillRectangle(brush, this.ClientRectangle);
                        }
                        finally
                        {
                            pevent.Graphics.Clip = clip;
                        }
                        for (int index = 0; index < rectangleArray.Length; ++index)
                        {
                            if (index < this._pages.Count)
                            {
                                Rectangle rect = rectangleArray[index];
                                if (!rect.IntersectsWith(ClientRectangle))
                                    continue;
                                var z = rect;
                                z.Inflate(3, 3);
                                pevent.Graphics.DrawRectangle(Pens.Black, z);
                                pevent.Graphics.FillRectangle((Brush)new SolidBrush(this.ForeColor), z);
                                z.Inflate(-1, -1);
                                i++;

                                var size = GetPhysicalSize();

                                if (zoom > MaxRasterZoom || zoom < 0.7)
                                {

                                    var mg = GetMeasurementGraphics();

                                    var dpiX = (int)mg.DpiX;
                                    var dpiY = (int)mg.DpiY;

                                    var hdc = mg.GetHdc();

                                    try
                                    {

                                        var metafileSize = PrinterUnitConvert.Convert(size, PrinterUnit.Display, PrinterUnit.HundredthsOfAMillimeter);

                                        using (var metafile = new Metafile(hdc, new Rectangle(0, 0, metafileSize.Width, metafileSize.Height), MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusOnly))
                                        {
                                            using (var g = Graphics.FromImage(metafile))
                                            {
                                                var pd = new PageDrawerClass(g);
                                                try
                                                {
                                                    _pages[index].PrintContentToPreview(pd, new PrintingLogicalToPhysicalUnits(dpiX, dpiY, 96));
                                                }
                                                finally
                                                {
                                                    pd.Dispose();
                                                }
                                            }
                                            pevent.Graphics.DrawImage(metafile, rect);
                                        }
                                    }
                                    finally
                                    {
                                        mg.ReleaseHdc(hdc);
                                    }

                                }
                                else
                                {
                                    var bitmapSize = new Size(size.Width * screendpi.X / 96, size.Height * screendpi.Y / 96);
                                    using (var bmp = new Bitmap(bitmapSize.Width, bitmapSize.Height))
                                    {
                                        using (var g = Graphics.FromImage(bmp))
                                        {
                                            using (var bg = BufferedGraphicsManager.Current.Allocate(pevent.Graphics, new Rectangle(0, 0, bitmapSize.Width, bitmapSize.Height)))
                                            {
                                                bg.Graphics.Clear(Color.White);
                                                var pd = new PageDrawerClass(bg.Graphics);
                                                try
                                                {
                                                    _pages[index].PrintContentToPreview(pd, new PrintingLogicalToPhysicalUnits(100 * screendpi.X, 100 * screendpi.Y, 96 * 96));
                                                }
                                                finally
                                                {
                                                    pd.Dispose();
                                                }
                                                bg.Render(g);
                                            }
                                        }
                                        pevent.Graphics.DrawImage(bmp, rect);
                                    }
                                }

                                --z.Width;
                                --z.Height;
                                pevent.Graphics.DrawRectangle(Pens.Black, z);
                            }
                        }
                    }
                }

                finally
                {
                    brush.Dispose();
                }
            }
            base.OnPaint(pevent);
        }
        Point GetPageSizeWithBorders()
        {
            var p = GetPageSizeInPixel();
            p.X += PhysicalToPixels(10, this.screendpi.X);
            p.Y += PhysicalToPixels(10, this.screendpi.Y);
            return p;
        }

        Point GetPageSizeInPixel()
        {
            CalculateZoom();

            this.imageSize = new Size((int)(this.zoom * (double)_physicalSize.Width),
                                      (int)(this.zoom * (double)_physicalSize.Height));
            Point pageSizeInPixels = MyPreviewControl.PhysicalToPixels(new Point(this.imageSize),
                                                                       this.screendpi);
            return pageSizeInPixels;
        }

        void CalculateZoom()
        {
            if (!autoZoom) return;
            Size size = new Size(MyPreviewControl.PixelsToPhysical(new Point(this.Size), this.screendpi));
            Size physicalSize = this.GetPhysicalSize();
            this.zoom =
                Math.Min(
                    ((double)size.Width - (double)(10 * (this.columns) + 30)) /
                    (double)(this.columns * physicalSize.Width),
                    ((double)size.Height - (double)(10 * (2))) /
                    (double)(physicalSize.Height));
        }

        protected virtual void OnStartPageChanged(EventArgs e)
        {
            EventHandler eventHandler = this.Events[MyPreviewControl.EVENT_STARTPAGECHANGED] as EventHandler;
            if (eventHandler == null)
                return;
            eventHandler((object)this, e);
        }

        private static int PhysicalToPixels(int physicalSize, int dpi)
        {
            return (int)((double)(physicalSize * dpi) / 96.0);
        }

        private static Point PhysicalToPixels(Point physical, Point dpi)
        {
            return new Point(MyPreviewControl.PhysicalToPixels(physical.X, dpi.X), MyPreviewControl.PhysicalToPixels(physical.Y, dpi.Y));
        }

        private static Size PhysicalToPixels(Size physicalSize, Point dpi)
        {
            return new Size(MyPreviewControl.PhysicalToPixels(physicalSize.Width, dpi.X), MyPreviewControl.PhysicalToPixels(physicalSize.Height, dpi.Y));
        }

        private static int PixelsToPhysical(int pixels, int dpi)
        {
            return (int)((double)pixels * 96.0 / (double)dpi);
        }

        private static Point PixelsToPhysical(Point pixels, Point dpi)
        {
            return new Point(MyPreviewControl.PixelsToPhysical(pixels.X, dpi.X), MyPreviewControl.PixelsToPhysical(pixels.Y, dpi.Y));
        }

        private static Size PixelsToPhysical(Size pixels, Point dpi)
        {
            return new Size(MyPreviewControl.PixelsToPhysical(pixels.Width, dpi.X), MyPreviewControl.PixelsToPhysical(pixels.Height, dpi.Y));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void ResetBackColor()
        {
            this.BackColor = SystemColors.AppWorkspace;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void ResetForeColor()
        {
            this.ForeColor = Color.White;
        }

        private void WmHScroll(ref Message m)
        {
            if (m.LParam != IntPtr.Zero)
            {
                base.WndProc(ref m);
            }
            else
            {
                Point point = this.position;
                int x = point.X;
                int maxPos = Math.Max(this.Width, this.virtualSize.Width);
                point.X = this.AdjustScroll(m, x, maxPos, true);
                this.Position = point;
            }
        }

        private void SetPositionNoInvalidate(Point value)
        {
            var startPage = StartPage;
            Point point = this.position;
            this.position = value;
            this.position.X = Math.Min(this.position.X, this.virtualSize.Width - this.Width);
            this.position.Y = Math.Min(this.position.Y, this.virtualSize.Height - this.Height);
            if (this.position.X < 0)
                this.position.X = 0;
            if (this.position.Y < 0)
                this.position.Y = 0;
            Rectangle clientRectangle = this.ClientRectangle;
            var rect = RECT.FromXYWH(clientRectangle.X, clientRectangle.Y, clientRectangle.Width, clientRectangle.Height);
            ScrollWindow(new HandleRef((object)this, this.Handle), point.X - this.position.X, point.Y - this.position.Y, ref rect, ref rect);
            SetScrollPos(new HandleRef((object)this, this.Handle), 0, this.position.X, true);
            SetScrollPos(new HandleRef((object)this, this.Handle), 1, this.position.Y, true);
            if (startPage != StartPage)
                this.OnStartPageChanged(EventArgs.Empty);

        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool ScrollWindow(HandleRef hWnd, int nXAmount, int nYAmount, ref RECT rectScrollRegion, ref RECT rectClip);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SetScrollPos(HandleRef hWnd, int nBar, int nPos, bool bRedraw);


        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public Size Size
            {
                get
                {
                    return new Size(this.right - this.left, this.bottom - this.top);
                }
            }

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public RECT(Rectangle r)
            {
                this.left = r.Left;
                this.top = r.Top;
                this.right = r.Right;
                this.bottom = r.Bottom;
            }

            public static RECT FromXYWH(int x, int y, int width, int height)
            {
                return new RECT(x, y, x + width, y + height);
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SetScrollInfo(HandleRef hWnd, int fnBar, SCROLLINFO si, bool redraw);

        internal void SetVirtualSizeNoInvalidate(Size value)
        {
            this.virtualSize = value;
            this.SetPositionNoInvalidate(this.position);
            var si = new SCROLLINFO();
            si.fMask = 3;
            si.nMin = 0;
            si.nMax = Math.Max(this.Height, this.virtualSize.Height) - 1;
            si.nPage = this.Height;
            SetScrollInfo(new HandleRef((object)this, this.Handle), 1, si, true);
            si.fMask = 3;
            si.nMin = 0;
            si.nMax = Math.Max(this.Width, this.virtualSize.Width) - 1;
            si.nPage = this.Width;
            SetScrollInfo(new HandleRef((object)this, this.Handle), 0, si, true);

        }

        private void WmVScroll(ref Message m)
        {
            if (m.LParam != IntPtr.Zero)
            {
                base.WndProc(ref m);
            }
            else
            {
                Point position = this.Position;
                int y = position.Y;
                int maxPos = Math.Max(this.Height, this.virtualSize.Height);
                position.Y = this.AdjustScroll(m, y, maxPos, false);
                this.Position = position;
            }
        }

        private void WmKeyDown(ref Message msg)
        {
            var s = GetPageSizeWithBorders();
            var fullPageInView = this.Height > s.Y && this.Width > s.X;

            Keys keys = (Keys)(int)msg.WParam | Control.ModifierKeys;
            Point position = this.Position;
            switch (keys & Keys.KeyCode)
            {
                case Keys.LButton | Keys.Space:
                    if ((keys & Keys.Modifiers) == Keys.Control)
                    {
                        int x = position.X;
                        int num = x <= 100 ? 0 : x - 100;
                        position.X = num;
                        this.Position = position;
                        break;
                    }
                    else
                    {
                        if (this.StartPage <= 0)
                            break;
                        this.StartPage -= columns;
                        break;
                    }
                case Keys.RButton | Keys.Space:
                    if ((keys & Keys.Modifiers) == Keys.Control)
                    {
                        int x = position.X;
                        int num1 = Math.Max(this.Width, this.virtualSize.Width);
                        int num2 = x >= num1 - 100 ? num1 : x + 100;
                        position.X = num2;
                        this.Position = position;
                        break;
                    }
                    else
                    {
                        if (this.StartPage + 1 >= _pages.Count)
                            break;
                        this.StartPage += columns;
                        break;
                    }
                case Keys.LButton | Keys.RButton | Keys.Space:
                    if ((keys & Keys.Modifiers) != Keys.Control)
                        break;
                    this.StartPage = _pages.Count;
                    break;
                case Keys.MButton | Keys.Space:
                    if ((keys & Keys.Modifiers) != Keys.Control)
                        break;
                    this.StartPage = 0;
                    break;
                case Keys.LButton | Keys.MButton | Keys.Space:
                    int x1 = position.X;
                    int num3 = x1 <= 5 ? 0 : x1 - 5;
                    position.X = num3;
                    this.Position = position;
                    break;
                case Keys.RButton | Keys.MButton | Keys.Space:
                    if (!fullPageInView)
                    {
                        int y1 = position.Y;
                        int num4 = y1 <= 5 ? 0 : y1 - 5;
                        position.Y = num4;
                        this.Position = position;
                    }
                    else if (!(this.StartPage <= 0))
                        this.StartPage -= columns; 
                    break;
                case Keys.LButton | Keys.RButton | Keys.MButton | Keys.Space:
                    int x2 = position.X;
                    int num5 = Math.Max(this.Width, this.virtualSize.Width);
                    int num6 = x2 >= num5 - 5 ? num5 : x2 + 5;
                    position.X = num6;
                    this.Position = position;
                    break;
                case Keys.Back | Keys.Space:
                    if (!fullPageInView)
                    {
                        int y2 = position.Y;
                        int num7 = Math.Max(this.Height, this.virtualSize.Height);
                        int num8 = y2 >= num7 - 5 ? num7 : y2 + 5;
                        position.Y = num8;
                        this.Position = position;
                    }
                    else if (!(this.StartPage + 1 >= _pages.Count))
                        this.StartPage += columns;
                    break;
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData == Keys.Down || keyData == Keys.Up || base.IsInputKey(keyData);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 256:
                    this.WmKeyDown(ref m);
                    break;
                case 276:
                    this.WmHScroll(ref m);
                    break;
                case 277:
                    this.WmVScroll(ref m);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /*  internal override bool ShouldSerializeBackColor()
          {
              return !this.BackColor.Equals((object)SystemColors.AppWorkspace);
          }

          internal override bool ShouldSerializeForeColor()
          {
              return !this.ForeColor.Equals((object)Color.White);
          }*/

        Action<int> _totalPagesListener = delegate { };
        public void AddPage(PrintPage page)
        {
            _totalPagesListener(_pages.Count + 1);
            lock (page)
            {
                _pages.Add(page);
            }
            CalcRows();

        }

        void CalcRows()
        {
            Rows = (int)Math.Ceiling((decimal)_pages.Count / columns);
        }

        public void NotifyTotalPages(Action<int> to)
        {
            _totalPagesListener = y => DoOnUI(() => to(y));
            _totalPagesListener(_pages.Count);
        }
        public void DoOnUI(Action what)
        {
            Action z = () =>
            {
                if (!IsDisposed)
                    what();
            };
            if (!IsDisposed)
                if (InvokeRequired)
                    BeginInvoke(z);
                else
                    z();
        }

        Graphics _mg;

        public Graphics GetMeasurementGraphics()
        {
            if (_mg == null)
            {
                lock (this)
                {
                    if (_mg == null)
                        _mg = _document.CreateMeasurementGraphics();
                }
            }
            return _mg;
        }

        public void Scroll(int i)
        {
            Position = new Point(Position.X, position.Y - 25 * i);
        }
    }
}