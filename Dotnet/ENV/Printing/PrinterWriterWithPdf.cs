using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Web;
using System.Windows.Forms;
using Firefly.Box.Printing.Advanced;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = System.Drawing.Font;
using Image = System.Drawing.Image;
using Rectangle = System.Drawing.Rectangle;
using Color = System.Drawing.Color;
using BaseColor = iTextSharp.text.Color;

namespace ENV.Printing
{
    partial class PrinterWriter
    {
        IPrintJob SaveAsPdf(bool showProgressAndShowPDFWhenDone, bool showPdfWhenDone)
        {
            var fileName = (FileName ?? "").Trim();
            if (string.IsNullOrEmpty(fileName))
            {
                var d = new System.Windows.Forms.SaveFileDialog();
                d.DefaultExt = "pdf";
                d.Filter = "Portable Document Format files (*.pdf)|*.pdf";
                d.AddExtension = true;
                d.RestoreDirectory = true;
                if (Common._suppressDialogForTesting)
                    return new CanceledPrintJob();
                if (d.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return new CanceledPrintJob();

                fileName = d.FileName;
            }
            try
            {
                System.IO.File.WriteAllText(fileName, "");
            }
            catch { }
            var pj = new iTextSharpPrintJob(GetPaperSize(), fileName, showProgressAndShowPDFWhenDone, this);
            if (showPdfWhenDone && !Common._suppressDialogForTesting)
                return new PrintWithProgressUIAndActionToDoAfterwards(pj, fileName, () =>
                {
                    try
                    {
                        System.Diagnostics.Process.Start(fileName);
                    }
                    catch
                    {
                    }
                }, showProgressAndShowPDFWhenDone);
            return pj;
        }

        public class CapturePDFPrinting
        {
            public CapturePDFPrinting()
            {
                Stream = new MemoryStream();
                _capture = this;
            }

            internal MemoryStream Stream { get; set; }
            public Stream GetResult()
            {
                _capture = null;
                return new MemoryStream(Stream.ToArray());
            }
        }
        public static bool DefaultPdf { get; set; }
        [ThreadStatic]
        static CapturePDFPrinting _capture;
        partial void Init()
        {
            Pdf = DefaultPdf;
            _saveAsCommands.Add(addSaveAs => addSaveAs("PDF", () => SaveAsPdf(true, true)));
            AddSendAsCommand("PDF", tempFilePath => new iTextSharpPrintJob(GetPaperSize(), tempFilePath, true, this));
        }

        partial void CreateAlternativePrintJob(Action<IPrintJob> create)
        {
            if (Pdf)
                create(SaveAsPdf(false, PrintPreview));
            if (_capture != null)
                create(new iTextSharpPrintJob(GetPaperSize(), () => _capture.Stream, false, this));
        }




        public string PdfUserPassword { get; set; }
        public string PdfOwnerPassword { get; set; }
        bool _allowPrinting = true;
        public bool PdfAllowPrinting
        {
            get { return _allowPrinting; }
            set { _allowPrinting = value; }
        }

        bool _allowCopy = true;
        public bool PdfAllowCopy
        {
            get { return _allowCopy; }
            set { _allowCopy = value; }
        }

        bool _allowModifyContents = true;
        public bool PdfAllowModifyContents
        {
            get { return _allowModifyContents; }
            set { _allowModifyContents = value; }
        }

        bool _allowModifyAnnotations = true;
        public bool PdfAllowModifyAnnotations
        {
            get { return _allowModifyAnnotations; }
            set { _allowModifyAnnotations = value; }
        }

        void PdfApply(PdfWriter writer)
        {
            if (string.IsNullOrEmpty(PdfUserPassword) && string.IsNullOrEmpty(PdfOwnerPassword))
                return;

            var permissions = PdfWriter.ALLOW_ASSEMBLY | PdfWriter.ALLOW_DEGRADED_PRINTING | PdfWriter.ALLOW_FILL_IN | PdfWriter.ALLOW_SCREENREADERS;
            if (PdfAllowPrinting)
                permissions |= PdfWriter.ALLOW_PRINTING;
            if (PdfAllowCopy)
                permissions |= PdfWriter.ALLOW_COPY;
            if (PdfAllowModifyContents)
                permissions |= PdfWriter.ALLOW_MODIFY_CONTENTS;
            if (PdfAllowModifyAnnotations)
                permissions |= PdfWriter.ALLOW_MODIFY_ANNOTATIONS;

            writer.SetEncryption(true, PdfUserPassword, PdfOwnerPassword, permissions);
        }


        public static int DpiForPdf = 96;
        public static bool UseEmbeddedFontsInPdf = true;
        public static bool DefaultUseGdiBidiAlgorithmForRtlText { get; set; }

        public bool UseGdiBidiAlgorithmForRtlText = DefaultUseGdiBidiAlgorithmForRtlText;

        class iTextSharpPrintJob : IPrintJob
        {
            float _pageHeight, _pageWidth;
            int _pageHeightInPixels;

            Func<Stream> _getStream;
            Dictionary<string, BaseFontAndStyle> _fontCache = new Dictionary<string, BaseFontAndStyle>();
            class BaseFontAndStyle
            {
                public BaseFont baseFont;
                public int bfBoldItalicStyle;
            }

            bool _showErrors;
            PrinterWriter _security;
            public iTextSharpPrintJob(Size paperSizeInPixels, string fileName, bool showErrors, PrinterWriter security)
                : this(
                    paperSizeInPixels, () => new FileStream(fileName, FileMode.Create), showErrors, security)
            {
            }
            public iTextSharpPrintJob(Size paperSizeInPixels, Func<Stream> getStream, bool showErrors, PrinterWriter security)
            {
                _showErrors = showErrors;
                var pageHeightInPixels = paperSizeInPixels.Height;
                var pageWidthInPixels = paperSizeInPixels.Width;
                _pageHeightInPixels = pageHeightInPixels;
                _pageHeight = pageHeightInPixels * 72 / 96;
                _pageWidth = pageWidthInPixels * 72 / 96;
                _getStream = getStream;
                _security = security;
                _controlForGraphics = new System.Windows.Forms.Control();
                _g = _controlForGraphics.CreateGraphics();
                _gDpiX = _g.DpiX;
            }

            Control _controlForGraphics;
            Document _doc;
            PdfWriter _writer;
            Graphics _g;
            float _gDpiX;

            SizeF Measure(SizeF original)
            {
                if (_gDpiX != 96)
                {
                    original.Width *= 96 / _gDpiX;
                    original.Height *= 96 / _gDpiX;
                }
                return original;
            }

            RectangleF ScaleAndTransform(Rectangle rect)
            {
                var rectF = new RectangleF(scale(rect.X), scale(rect.Y), scale(rect.Width), scale(rect.Height));
                rectF.Y = _pageHeight - rectF.Bottom;
                return rectF;
            }

            PointF ScaleAndTransform(PointF point)
            {
                var pointF = new PointF(scale(point.X), scale(point.Y));
                pointF.Y = _pageHeight - pointF.Y;
                return pointF;
            }

            bool _abort = false;
            public void CreatePage(IPageContent printContent, bool lastPage)
            {
                if (_abort) return;

                if (_writer == null)
                {
                    try
                    {
                        _doc = new Document(new iTextSharp.text.Rectangle(_pageWidth, _pageHeight), 0, 0, 0, 0);
                        _writer = PdfWriter.GetInstance(_doc, _getStream());
                        _security.PdfApply(_writer);
                        _doc.Open();
                    }
                    catch (Exception ex)
                    {
                        if (_showErrors)
                            System.Windows.Forms.MessageBox.Show(ex.Message, @"Save As PDF Error", MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        _abort = true;
                        return;
                    }
                }
                try
                {
                    _doc.NewPage();
                    printContent.PrintContent(new myPageDrawer(this), new myConverter());
                }
                finally
                {
                    if (lastPage)
                    {
                        _doc.Close();
                        _g.Dispose();
                        _controlForGraphics.Dispose();
                        foreach (var bmp in _imageCache.Values)
                            bmp.Image.Dispose();
                        _writer = null;
                        if (_mg != null)
                            _mg.Dispose();
                    }
                }
            }

            static float scale(float val)
            {
                return val * 72 / (96 * 6);
            }

            static iTextSharpPrintJob()
            {
                if (FontFactory.RegisterDirectories() == 0)
                    FontFactory.RegisterDirectory(System.Environment.ExpandEnvironmentVariables("%windir%") + "\\fonts");
            }

            class myConverter : IPrintingUnitConvertor
            {
                public int ScaleHorizontal(float val)
                {
                    return (int)val * 6;
                }

                public int ScaleVertical(float val)
                {
                    return (int)val * 6;
                }

                public Rectangle ScaleForFill(RectangleF rect)
                {
                    return Rectangle.Truncate(new RectangleF(rect.X * 6, rect.Y * 6, rect.Width * 6, rect.Height * 6));
                }

                public Rectangle ScaleForString(RectangleF rect)
                {
                    return Rectangle.Truncate(new RectangleF(rect.X * 6, rect.Y * 6, rect.Width * 6, rect.Height * 6));
                }

                public float ScaleFontSize(float val)
                {
                    return val;
                }
            }

            public int PageHeight
            {
                get { return _pageHeightInPixels; }
            }

            public Size MeasureString(string text, Font font, int width, StringDrawingProperties stringDrawingProperties)
            {
                if (stringDrawingProperties.Multiline)
                {
                    for (var i = 0; i < text.Length; i++)
                    {
                        if (text[i] == '\r' && (i + 1 >= text.Length || text[i + 1] != '\n'))
                            text = text.Insert(i + 1, "\n");
                    }
                }
                return Size.Round(Measure(_g.MeasureString(text, font, width)));
            }

            public int MeasureStringAndReturnNumberOfCharsThatFit(string text, Font font, int width, int height, StringDrawingProperties stringDrawingProperties)
            {
                int chars = 0, lines;
                var s = new SizeF(width, height);
                if (_gDpiX != 96)
                {
                    s.Width *= _gDpiX / 96;
                    s.Height *= _gDpiX / 96;
                }
                stringDrawingProperties.UseMeasuringStringFormat(obj =>
                    _g.MeasureString(text, font, s, obj, out chars, out lines));
                return chars;
            }

            public Size MeasureRTFString(string rtf, Font font, int width, bool rightToLeft, bool useVersion41)
            {
                return Size.Round(Measure(PageDrawerClass.MeasureRTFString(rtf, _g, font, width, rightToLeft, useVersion41)));
            }

            Graphics _mg;

            public Graphics GetMeasurementGraphics()
            {
                if (_mg == null)
                {
                    lock (this)
                    {
                        if (_mg == null)
                        {
                            var pdc = new System.Drawing.Printing.PrintDocument();
                            _mg = pdc.PrinterSettings.CreateMeasurementGraphics(pdc.DefaultPageSettings);
                            pdc.Dispose();
                        }
                    }
                }
                return _mg;
            }

            class myPageDrawer : IPageDrawer
            {
                iTextSharpPrintJob _parent;

                public myPageDrawer(iTextSharpPrintJob parent)
                {
                    _parent = parent;
                }

                public IPen CreatePen(Color color, int penWidth, DashStyle dashStyle)
                {
                    if (color.IsEmpty || color == Color.Transparent) return new dummyPen();

                    return new myPen(_parent,
                        () =>
                        {
                            _parent._writer.DirectContent.SetColorStroke(new BaseColor(color));
                            _parent._writer.DirectContent.SetLineWidth(scale(penWidth));
                            switch (dashStyle)
                            {
                                case DashStyle.Solid:
                                    _parent._writer.DirectContent.SetLineDash(1, 0, 0);
                                    break;
                                default:
                                    _parent._writer.DirectContent.SetLineDash(scale(7), scale(4), 0);
                                    break;
                            }
                        },
                        () =>
                        {
                            _parent._writer.DirectContent.Stroke();
                        });
                }

                class dummyPen : IPen, IBrush
                {
                    public void Dispose()
                    {


                    }

                    public void DrawLines(PointF[] pointS)
                    {
                    }

                    public void DrawRectangle(Rectangle rectangle)
                    {
                    }

                    public void DrawEllipse(Rectangle area)
                    {
                    }

                    public void DrawAndFillEllipse(Rectangle area, Color fillColor)
                    {
                    }

                    public void DrawAndFillRectangleWithCurveEdges(Rectangle area, Point[] points, Color fillColor)
                    {
                    }

                    public void DrawArc(Rectangle area, int startAngle, int sweepAngle)
                    {
                    }

                    public void DrawRoundRectangle(Rectangle area)
                    {
                    }

                    public void DrawAndFillRoundRectangle(Rectangle area, Color fillColor)
                    {
                    }

                    public void FillPolygon(Point[] points)
                    {
                    }

                    public void FillRectangle(Rectangle area)
                    {
                    }
                }

                class myPen : IPen, IBrush
                {
                    iTextSharpPrintJob _parent;
                    Action _before, _after;

                    public myPen(iTextSharpPrintJob parent, Action before, Action after)
                    {
                        _parent = parent;
                        _before = before;
                        _after = after;
                    }

                    void Draw(Action what)
                    {
                        _before();
                        what();
                        _after();
                    }

                    public void DrawLines(PointF[] pointS)
                    {
                        Draw(
                            () =>
                            {
                                var p0 = _parent.ScaleAndTransform(pointS[0]);
                                _parent._writer.DirectContent.MoveTo(p0.X, p0.Y);
                                for (int i = 1; i < pointS.Length; i++)
                                {
                                    var p = _parent.ScaleAndTransform(pointS[i]);
                                    _parent._writer.DirectContent.LineTo(p.X, p.Y);
                                }
                            });
                    }

                    void Rect(Rectangle rectangle)
                    {
                        Draw(
                            () =>
                            {
                                var r = _parent.ScaleAndTransform(rectangle);
                                _parent._writer.DirectContent.Rectangle(r.X, r.Y, r.Width, r.Height);
                            });
                    }

                    void Ellipse(Rectangle rectangle)
                    {
                        Draw(
                            () =>
                            {
                                var r = _parent.ScaleAndTransform(rectangle);
                                _parent._writer.DirectContent.Ellipse(r.X, r.Y, r.X + r.Width, r.Y + r.Height);
                            });
                    }

                    public void DrawRectangle(Rectangle rectangle)
                    {
                        Rect(rectangle);
                    }

                    public void DrawEllipse(Rectangle area)
                    {
                        Ellipse(area);
                    }

                    public void DrawAndFillEllipse(Rectangle area, Color fillColor)
                    {
                        Ellipse(area);
                        area.Inflate(-1, -1);
                        new myPen(_parent,
                        () =>
                        {
                            _parent._writer.DirectContent.SetColorFill(new BaseColor(fillColor));
                        },
                        () =>
                        {
                            _parent._writer.DirectContent.Fill();
                        }).DrawEllipse(area);
                    }

                    public void DrawAndFillRectangleWithCurveEdges(Rectangle area, Point[] points, Color fillColor)
                    {
                    }

                    public void DrawArc(Rectangle area, int startAngle, int sweepAngle)
                    {
                    }

                    public void DrawRoundRectangle(Rectangle area)
                    {
                        Draw(
                            () =>
                            {
                                var r = _parent.ScaleAndTransform(area);
                                _parent._writer.DirectContent.RoundRectangle(r.X, r.Y, r.Width, r.Height, (r.Width + r.Height) / 20);
                            });
                    }

                    public void DrawAndFillRoundRectangle(Rectangle area, Color fillColor)
                    {
                        DrawRoundRectangle(area);
                        area.Inflate(-1, -1);
                        new myPen(_parent,
                        () =>
                        {
                            _parent._writer.DirectContent.SetColorFill(new BaseColor(fillColor));
                        },
                        () =>
                        {
                            _parent._writer.DirectContent.Fill();
                        }).DrawRoundRectangle(area);
                    }

                    public void FillPolygon(Point[] points)
                    {
                    }

                    public void FillRectangle(Rectangle area)
                    {
                        Rect(area);
                    }

                    public void Dispose()
                    {

                    }
                }

                static string[] ansiFonts = new string[] { "Dax" };

                static bool IsAnsiFont(string fontName)
                {
                    foreach (var f in ansiFonts)
                        if (fontName.StartsWith(f, StringComparison.InvariantCultureIgnoreCase)) return true;
                    return false;
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct GCP_RESULTS
                {
                    public int StructSize;
                    [MarshalAs(UnmanagedType.LPTStr)]
                    public string OutString;
                    public IntPtr Order;
                    public IntPtr Dx;
                    public IntPtr CaretPos;
                    public IntPtr Class;
                    public IntPtr Glyphs;
                    public int GlyphCount;
                    public int MaxFit;
                }

                [Flags]
                public enum GCPFlags : uint
                {
                    GCP_DBCS = 0x0001,
                    GCP_REORDER = 0x0002,
                    GCP_USEKERNING = 0x0008,
                    GCP_GLYPHSHAPE = 0x0010,
                    GCP_LIGATE = 0x0020,
                    GCP_DIACRITIC = 0x0100,
                    GCP_KASHIDA = 0x0400,
                    GCP_ERROR = 0x8000,
                    GCP_JUSTIFY = 0x00010000,
                    GCP_CLASSIN = 0x00080000,
                    GCP_MAXEXTENT = 0x00100000,
                    GCP_JUSTIFYIN = 0x00200000,
                    GCP_DISPLAYZWG = 0x00400000,
                    GCP_SYMSWAPOFF = 0x00800000,
                    GCP_NUMERICOVERRIDE = 0x01000000,
                    GCP_NEUTRALOVERRIDE = 0x02000000,
                    GCP_NUMERICSLATIN = 0x04000000,
                    GCP_NUMERICSLOCAL = 0x08000000,
                }

                [DllImport("gdi32.dll")]
                static extern uint GetCharacterPlacementW(IntPtr hdc, [MarshalAs(UnmanagedType.LPWStr)] string lpString, int nCount, int nMaxExtent, ref GCP_RESULTS lpResults, uint dwFlags);

                [DllImport("gdi32.dll")]
                static extern uint SetTextAlign(IntPtr hdc, uint fMode);

                static string BiDi(string text)
                {
                    int len = text.Length;
                    if (len == 0) return text;
                    using (var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                    {
                        var hdc = g.GetHdc();
                        try
                        {
                            SetTextAlign(hdc, 0x100);

                            int[] order = new int[len];
                            int[] dx = new int[len];
                            int[] caret = new int[len];
                            byte[] clss = new byte[len];
                            short[] glys = new short[len];

                            GCHandle ordHnd = GCHandle.Alloc(order, GCHandleType.Pinned);
                            GCHandle dxHnd = GCHandle.Alloc(dx, GCHandleType.Pinned);
                            GCHandle carHnd = GCHandle.Alloc(caret, GCHandleType.Pinned);
                            GCHandle clsHnd = GCHandle.Alloc(clss, GCHandleType.Pinned);
                            GCHandle glyHnd = GCHandle.Alloc(glys, GCHandleType.Pinned);

                            try
                            {
                                GCP_RESULTS rs = new GCP_RESULTS();
                                rs.StructSize = Marshal.SizeOf(typeof(GCP_RESULTS));

                                rs.OutString = new String('\0', len + 2);
                                rs.Order = ordHnd.AddrOfPinnedObject();
                                rs.Dx = dxHnd.AddrOfPinnedObject();
                                rs.CaretPos = carHnd.AddrOfPinnedObject();
                                rs.Class = clsHnd.AddrOfPinnedObject();
                                rs.Glyphs = glyHnd.AddrOfPinnedObject();

                                rs.GlyphCount = len;
                                rs.MaxFit = 0;

                                GCPFlags flg = GCPFlags.GCP_GLYPHSHAPE | GCPFlags.GCP_REORDER | GCPFlags.GCP_SYMSWAPOFF;
                                uint r = GetCharacterPlacementW(hdc, text, len, 0, ref rs, (uint)flg);
                                if (r == 0)
                                    throw new Exception(string.Format("GetCharacterPlacement Error {0}", Marshal.GetLastWin32Error()));

                                var s = rs.OutString.ToCharArray();
                                for (int i = 0; i < len; i++)
                                    if (clss[i] == 2)
                                        s[order[i]] = HebrewTextTools.ReverseBracks(s[order[i]]);
                                return new string(s);
                            }
                            finally
                            {
                                ordHnd.Free();
                                dxHnd.Free();
                                carHnd.Free();
                                clsHnd.Free();
                                glyHnd.Free();
                            }
                        }
                        finally
                        {
                            g.ReleaseHdc(hdc);
                        }
                    }
                }

                public void DrawString(string text, Font font, Color foreGround, Rectangle area, StringDrawingProperties stringDrawingProperties)
                {
                    if (font.Name.StartsWith("Free 3 of 9"))
                    {
                        DrawAsImage(area, Color.Transparent, (pd, r) => pd.DrawString(text, font, foreGround, r, stringDrawingProperties));
                        return;
                    }

                    var transformedArea = _parent.ScaleAndTransform(area);

                    BaseFontAndStyle baseFontAndStyle;
                    var fontBoldItalicStyle = ((int)(font.Style) & 3);
                    var fontNameAndStyle = font.Name + fontBoldItalicStyle;
                    if (!_parent._fontCache.TryGetValue(fontNameAndStyle, out baseFontAndStyle))
                    {
                        iTextSharp.text.Font f;
                        try
                        {
                            f = FontFactory.GetFont(font.Name, (!PrinterWriter.UseEmbeddedFontsInPdf || IsAnsiFont(font.Name)) ? ENV.LocalizationInfo.Current.OuterEncoding.WebName : BaseFont.IDENTITY_H, PrinterWriter.UseEmbeddedFontsInPdf, 12, fontBoldItalicStyle);
                        }
                        catch (DocumentException)
                        {
                            f = FontFactory.GetFont(font.Name, BaseFont.WINANSI, false, 12, fontBoldItalicStyle);
                        }
                        baseFontAndStyle = new BaseFontAndStyle
                        {
                            baseFont = f.GetCalculatedBaseFont(false),
                            bfBoldItalicStyle = fontBoldItalicStyle & ~f.Style
                        };
                        _parent._fontCache.Add(fontNameAndStyle, baseFontAndStyle);
                    }

                    var phraseFont = new iTextSharp.text.Font(baseFontAndStyle.baseFont, font.Size,
                        (int)(font.Style & ~FontStyle.Underline) & ~baseFontAndStyle.bfBoldItalicStyle);
                    phraseFont.Color = new BaseColor(foreGround);

                    var rtl = stringDrawingProperties.RightToLeft;
                    var runDirection = rtl ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR;

                    Chunk chunkToAdd = null;
                    if (_parent._security.UseGdiBidiAlgorithmForRtlText && rtl && !stringDrawingProperties.Multiline)
                    {
                        try
                        {
                            text = BiDi(text);
                            rtl = false;
                            runDirection = PdfWriter.RUN_DIRECTION_NO_BIDI;
                            var t1 = text.TrimEnd(' ');
                            if (text.Length > t1.Length)
                                chunkToAdd = new Chunk(new string(' ', text.Length - t1.Length));
                        }
                        catch
                        {
                        }
                    }

                    var chunk = new Chunk(text, phraseFont);
                    if (font.Underline)
                        chunk.SetUnderline(null, 0, 0.06F, 0, -0.15F, 0);

                    var hAlign = rtl ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT;

                    if (((int)stringDrawingProperties.Alignment & 0x222) != 0) // Horizontal Center
                        hAlign = Element.ALIGN_CENTER;

                    if (((int)stringDrawingProperties.Alignment & 0x444) != 0) // Horizontal Right
                        hAlign = rtl ? Element.ALIGN_LEFT : Element.ALIGN_RIGHT;

                    var vAlign = Element.ALIGN_TOP;

                    if (((int)stringDrawingProperties.Alignment & 0x70) != 0) // Vertical Center
                        vAlign = Element.ALIGN_MIDDLE;

                    if (((int)stringDrawingProperties.Alignment & 0x700) != 0) // Vertical Bottom
                        vAlign = Element.ALIGN_BOTTOM;

                    var pTable = new PdfPTable(1);

                    var rotation = 0;
                    var skew = 0;
                    Action<Action<RectangleF>> changeTextArea = x => { };
                    Action<PdfPCell, float, float> setPaddingAboveAndBelowText =
                        (cell, paddingTop, paddingBottom) =>
                        {
                            cell.PaddingTop = paddingTop;
                            cell.PaddingBottom = paddingBottom;
                        };

                    if (stringDrawingProperties.Angle != 0)
                    {
                        if (stringDrawingProperties.Angle % 90 == 0)
                        {
                            rotation = stringDrawingProperties.Angle;
                            if (stringDrawingProperties.Angle == 90)
                            {
                                var v = vAlign;
                                vAlign = hAlign == Element.ALIGN_CENTER ? Element.ALIGN_MIDDLE : (hAlign == Element.ALIGN_LEFT ? Element.ALIGN_TOP : Element.ALIGN_BOTTOM);
                                hAlign = v == Element.ALIGN_MIDDLE ? Element.ALIGN_CENTER : (v == Element.ALIGN_TOP ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT);
                                setPaddingAboveAndBelowText =
                                    (cell, paddingTop, paddingBottom) =>
                                    {
                                        cell.PaddingRight = paddingTop;
                                        cell.PaddingLeft = paddingBottom;
                                    };
                            }
                            else if (stringDrawingProperties.Angle == 270)
                            {
                                var v = vAlign;
                                vAlign = hAlign == Element.ALIGN_CENTER ? Element.ALIGN_MIDDLE : (hAlign == Element.ALIGN_LEFT ? Element.ALIGN_BOTTOM : Element.ALIGN_TOP);
                                hAlign = v == Element.ALIGN_MIDDLE ? Element.ALIGN_CENTER : (v == Element.ALIGN_TOP ? Element.ALIGN_LEFT : Element.ALIGN_RIGHT);
                                setPaddingAboveAndBelowText =
                                    (cell, paddingTop, paddingBottom) =>
                                    {
                                        cell.PaddingLeft = paddingTop;
                                        cell.PaddingRight = paddingBottom;
                                    };
                            }
                            else if (stringDrawingProperties.Angle == 180)
                            {
                                hAlign = hAlign == Element.ALIGN_LEFT ? Element.ALIGN_RIGHT : (hAlign == Element.ALIGN_RIGHT ? Element.ALIGN_LEFT : hAlign);
                                setPaddingAboveAndBelowText =
                                    (cell, paddingTop, paddingBottom) =>
                                    {
                                        cell.PaddingBottom = paddingTop;
                                        cell.PaddingTop = paddingBottom;
                                    };
                            }
                        }
                        else
                        {
                            skew = stringDrawingProperties.Angle % 180;
                            chunk.SetSkew(skew, -skew);
                            var factor = (float)Math.Abs(Math.Cos(Math.PI * skew / 180));
                            chunk.Font.Size = chunk.Font.Size * factor;

                            changeTextArea = y =>
                            {
                                var a = transformedArea;
                                var s = (SizeF)_parent.MeasureString(text, font, int.MaxValue, stringDrawingProperties);
                                s = new SizeF(s.Width * factor, s.Height * factor);
                                var rad = Math.PI * skew / 180;
                                a.X += (int)(s.Height / 2 * Math.Sin(rad));
                                a.Y = a.Y + a.Height - s.Height;
                                if (stringDrawingProperties.Angle < 180)
                                    a.Y -= (int)(s.Height * Math.Cos(rad) + (s.Width - s.Height / (2 * Math.Tan(rad / 2))) * Math.Sin(rad));
                                a.Size = s;
                                y(a);
                            };
                            if (stringDrawingProperties.Angle % 360 > 90 && stringDrawingProperties.Angle % 360 < 270)
                                rotation = 180;

                            hAlign = Element.ALIGN_UNDEFINED;
                            vAlign = Element.ALIGN_UNDEFINED;
                        }
                    }

                    _parent._writer.DirectContent.SaveState();
                    _parent._writer.DirectContent.Rectangle(transformedArea.X, transformedArea.Y, transformedArea.Width, transformedArea.Height);
                    _parent._writer.DirectContent.Clip();
                    _parent._writer.DirectContent.NewPath();

                    changeTextArea(o => transformedArea = o);

                    try
                    {
                        if (stringDrawingProperties.Angle == 0)
                        {
                            if (!stringDrawingProperties.Multiline)
                            {
                                transformedArea.Width += 20000;
                                if (((int)stringDrawingProperties.Alignment & 0x444) != 0 /*Horizontal Right*/)
                                    transformedArea.X -= 20000;
                                if (hAlign == Element.ALIGN_CENTER) transformedArea.X -= 10000;

                                if (vAlign == Element.ALIGN_BOTTOM)
                                    transformedArea.Height += 20000;
                                if (vAlign == Element.ALIGN_MIDDLE)
                                {
                                    transformedArea.Height += 20000;// - 0.75f;
                                    transformedArea.Y -= 10000;
                                }
                            }
                            else
                                transformedArea.Height += 0.3f * phraseFont.Size;
                        }
                        var phrase = new Phrase(chunk);
                        if (chunkToAdd != null)
                            phrase.Add(chunkToAdd);

                        var useAscenderDescender = phraseFont.BaseFont.GetFontDescriptor(BaseFont.ASCENT, phraseFont.CalculatedSize) > 0;

                        var pCell = new PdfPCell(phrase)
                        {
                            HorizontalAlignment = hAlign,
                            VerticalAlignment = vAlign,
                            Border = PdfPCell.NO_BORDER,
                            MinimumHeight = transformedArea.Height,
                            RunDirection = runDirection,
                            Padding = 0,
                            UseAscender = useAscenderDescender && !stringDrawingProperties.Multiline,
                            UseDescender = useAscenderDescender,
                            Rotation = rotation
                        };

                        if (stringDrawingProperties.Multiline)
                            pCell.SetLeading(0.3f, 1.1f);
                        else
                        {
                            var paddingAboveText = 0f;
                            var paddingBelowText = 0f;
                            if (useAscenderDescender)
                                paddingAboveText = 0.18f * phraseFont.Size;
                            else
                            {
                                paddingAboveText = phraseFont.BaseFont.GetAscent("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ") * phraseFont.CalculatedSize / -8089f;
                                paddingBelowText = phraseFont.BaseFont.GetDescent("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ") * phraseFont.CalculatedSize / -1100f; ;
                            }
                            setPaddingAboveAndBelowText(pCell, paddingAboveText, paddingBelowText);
                        }
                        pTable.TotalWidth = transformedArea.Width;
                        pTable.AddCell(pCell);

                        pTable.WriteSelectedRows(0, -1, 0, -1, transformedArea.X, transformedArea.Y + transformedArea.Height, _parent._writer.DirectContent);

                    }
                    finally
                    {
                        _parent._writer.DirectContent.RestoreState();
                    }
                }

                public void DrawImage(Image image, Rectangle sourceRectangle, Rectangle r)
                {
                    var targetRectangle = _parent.ScaleAndTransform(r);

                    var imageToDraw = image;
                    EncoderParameters p = null;
                    var imageFormat = image.RawFormat;

                    var isCYMK = (int)imageToDraw.PixelFormat == 8207;
                    var is1bppIndexed = image.PixelFormat == PixelFormat.Format1bppIndexed;

                    if (!isCYMK)
                    {
                        var ratio = (float)DpiForPdf / 72;
                        imageToDraw = _parent.GetLowResolutionImage(image, sourceRectangle,
                            new Size((int)(targetRectangle.Width * ratio),
                            (int)(targetRectangle.Height * ratio)));
                    }

                    var ms = new MemoryStream();
                    imageToDraw.Save(ms, FindEncoder(image.RawFormat), null);

                    if (!is1bppIndexed)
                    {
                        // Convert to RGB bitmap
                        try
                        {
                            var encoder = new System.Windows.Media.Imaging.BmpBitmapEncoder();
                            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(System.Windows.Media.Imaging.BitmapFrame.Create(new MemoryStream(ms.ToArray()))));
                            ms = new MemoryStream();
                            encoder.Save(ms);
                        }
                        catch (Exception)
                        {
                        }
                    }

                    _parent._writer.DirectContent.AddImage(iTextSharp.text.Image.GetInstance(ms.ToArray()),
                                                           targetRectangle.Width, 0, 0, targetRectangle.Height,
                                                           targetRectangle.X, targetRectangle.Y, true);
                }
                internal ImageCodecInfo FindEncoder(ImageFormat f)
                {
                    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                    foreach (ImageCodecInfo codec in codecs)
                    {
                        if (codec.FormatID.Equals(f.Guid))
                            return codec;
                    }
                    return codecs[0];
                    return null;
                }
                public IBrush CreateBrush(Color color)
                {
                    if (color.IsEmpty || color == Color.Transparent) return new dummyPen();

                    return new myPen(_parent,
                        () =>
                        {
                            _parent._writer.DirectContent.SetColorFill(new BaseColor(color));
                        },
                        () =>
                        {
                            _parent._writer.DirectContent.Fill();
                        });

                }

                [DllImport("gdi32.dll")]
                public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
                [DllImport("gdi32.dll")]
                public static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
                [DllImport("gdi32.dll")]
                public static extern bool DeleteObject(IntPtr hObject);
                [DllImport("gdi32.dll")]
                public static extern bool DeleteDC(IntPtr hdc);

                public void DrawRTFString(string rtf, Rectangle rectangle, Font font, Color color, Color backColor, bool rightToLeft, HorizontalAlignment alignment, bool useVersion41)
                {
                    DrawAsImage(rectangle, backColor,
                        (pd, imageRectangle) =>
                        {
                            pd.DrawRTFString(rtf, imageRectangle, font, color, backColor, rightToLeft, alignment, useVersion41);
                        });
                }

                void DrawAsImage(Rectangle rectangle, Color backColor, Action<PageDrawerClass, Rectangle> draw)
                {
                    Action<Graphics, Rectangle> drawRtfToGraphics =
                        (g, imageRectangle) =>
                        {
                            var pd = new PageDrawerClass(g);
                            try
                            {
                                draw(pd, imageRectangle);
                            }
                            finally
                            {
                                pd.Dispose();
                            }
                        };

                    Action<Bitmap> addBmpToPdf =
                        bmp =>
                        {
                            var targetRectangle = _parent.ScaleAndTransform(rectangle);
                            var ms = new MemoryStream();
                            bmp.Save(ms, FindEncoder(bmp.RawFormat), null);
                            var i = iTextSharp.text.Image.GetInstance(ms.ToArray());
                            i.SetDpi((int)bmp.HorizontalResolution, (int)bmp.VerticalResolution);
                            _parent._writer.DirectContent.AddImage(i, targetRectangle.Width, 0, 0, targetRectangle.Height, targetRectangle.X, targetRectangle.Y, backColor != Color.Transparent);
                        };

                    Func<int, Size> getImageSize = dpi => new Size(rectangle.Width * dpi / (96 * 6), rectangle.Height * dpi / (96 * 6));

                    try
                    {
                        var mg = _parent.GetMeasurementGraphics();

                        var dpi = (int)mg.DpiX;

                        var hdc = mg.GetHdc();

                        try
                        {
                            var imageRectangle = new Rectangle(Point.Empty, getImageSize(dpi));
                            var bmp1 = new Bitmap(imageRectangle.Width, imageRectangle.Height);
                            var hbmp = bmp1.GetHbitmap(backColor);
                            try
                            {
                                var cdc = CreateCompatibleDC(hdc);
                                try
                                {
                                    var old = SelectObject(cdc, hbmp);
                                    try
                                    {
                                        using (var g = Graphics.FromHdc(cdc))
                                            drawRtfToGraphics(g, imageRectangle);
                                    }
                                    finally
                                    {
                                        SelectObject(cdc, old);
                                    }
                                }
                                finally
                                {
                                    DeleteDC(cdc);
                                }

                                using (var bmp = Bitmap.FromHbitmap(hbmp))
                                    addBmpToPdf(bmp);
                            }
                            finally
                            {
                                DeleteObject(hbmp);
                            }
                        }
                        finally
                        {
                            mg.ReleaseHdc(hdc);
                        }
                    }
                    catch
                    {
                        var dpi = Math.Max(DpiForPdf, 300);
                        var imageRectangle = new Rectangle(Point.Empty, getImageSize(dpi));
                        using (var bmp = new Bitmap(imageRectangle.Width, imageRectangle.Height))
                        {
                            bmp.SetResolution(dpi, dpi);
                            using (var g = Graphics.FromImage(bmp))
                                drawRtfToGraphics(g, imageRectangle);
                            addBmpToPdf(bmp);
                        }
                    }
                }

                public int GetAverageCharWidth(Font font)
                {
                    return (int)(_parent.Measure(_parent._g.MeasureString("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", font)).Width / 52);
                }
            }

            class ImageCacheEntry
            {
                public Rectangle SourceRectangle;
                public Bitmap Image;
            }
            Dictionary<Image, ImageCacheEntry> _imageCache = new Dictionary<Image, ImageCacheEntry>();
            Bitmap GetLowResolutionImage(Image image, Rectangle sourceRectangle, Size requiredImageSize)
            {
                ImageCacheEntry cachedImage;
                if (_imageCache.TryGetValue(image, out cachedImage))
                {
                    if (cachedImage.SourceRectangle == sourceRectangle && cachedImage.Image.Size == requiredImageSize)
                        return cachedImage.Image;
                }
                var bmp = new Bitmap(requiredImageSize.Width, requiredImageSize.Height);
                Graphics g = null;
                try
                {
                    g = Graphics.FromImage(bmp);
                    g.Clear(Color.White);
                    // if (image.PixelFormat == PixelFormat.Format1bppIndexed)
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.None;
                    }
                    g.DrawImage(image, new Rectangle(0, 0, requiredImageSize.Width, requiredImageSize.Height),
                        sourceRectangle.X, sourceRectangle.Y, sourceRectangle.Width, sourceRectangle.Height, GraphicsUnit.Pixel);
                }
                finally
                {
                    if (g != null)
                    {
                        g.Dispose();
                    }
                }
                var e = new ImageCacheEntry { SourceRectangle = sourceRectangle, Image = bmp };
                if (cachedImage == null)
                    _imageCache.Add(image, e);
                else
                    _imageCache[image] = e;
                return image.PixelFormat == PixelFormat.Format1bppIndexed ? ConvertToBitonal(bmp) : bmp;
            }

            static Bitmap ConvertToBitonal(Bitmap original)
            {
                var source = original;

                BitmapData sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                int imageSize = sourceData.Stride * sourceData.Height;
                byte[] sourceBuffer = new byte[imageSize];
                Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, imageSize);

                source.UnlockBits(sourceData);

                Bitmap destination = new Bitmap(source.Width, source.Height, PixelFormat.Format1bppIndexed);
                destination.SetResolution(original.HorizontalResolution, original.VerticalResolution);

                BitmapData destinationData = destination.LockBits(new Rectangle(0, 0, destination.Width, destination.Height), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

                imageSize = destinationData.Stride * destinationData.Height;
                byte[] destinationBuffer = new byte[imageSize];

                int sourceIndex = 0;
                int destinationIndex = 0;
                int pixelTotal = 0;
                byte destinationValue = 0;
                int pixelValue = 128;
                int height = source.Height;
                int width = source.Width;
                int threshold = 500;

                for (int y = 0; y < height; y++)
                {
                    sourceIndex = y * sourceData.Stride;
                    destinationIndex = y * destinationData.Stride;
                    destinationValue = 0;
                    pixelValue = 128;

                    for (int x = 0; x < width; x++)
                    {
                        pixelTotal = sourceBuffer[sourceIndex] + sourceBuffer[sourceIndex + 1] + sourceBuffer[sourceIndex + 2];
                        if (pixelTotal > threshold)
                        {
                            destinationValue += (byte)pixelValue;
                        }
                        if (pixelValue == 1)
                        {
                            destinationBuffer[destinationIndex] = destinationValue;
                            destinationIndex++;
                            destinationValue = 0;
                            pixelValue = 128;
                        }
                        else
                        {
                            pixelValue >>= 1;
                        }
                        sourceIndex += 4;
                    }
                    if (pixelValue != 128)
                    {
                        destinationBuffer[destinationIndex] = destinationValue;
                    }
                }

                Marshal.Copy(destinationBuffer, 0, destinationData.Scan0, imageSize);

                destination.UnlockBits(destinationData);

                return destination;
            }
        }

    }
}
