using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using ENV.IO;
using Firefly.Box;
using Firefly.Box.Printing;
using Firefly.Box.Printing.Advanced;
using Encoder = System.Text.Encoder;


namespace ENV.Printing
{

    public partial class PrinterWriter : Firefly.Box.Printing.PrinterWriter, IOByName
    {
        public static bool PrintPreviewDialogDefaultZoom100
        {
            get { return PrintDocumentPrintJob.PreviewDefaultZoom100; }
            set { PrintDocumentPrintJob.PreviewDefaultZoom100 = value; }
        }
        public static bool PrintPreviewBackwardCompatibleToolbar
        {
            get { return PrintDocumentPrintJob.PreviewBackwardCompatibleToolbar; }
            set { PrintDocumentPrintJob.PreviewBackwardCompatibleToolbar = value; }
        }
        public static double PrintPreviewMaxRasterZoomFactor
        {
            get { return PrintDocumentPrintJob.PreviewMaxRasterZoomFactor; }
            set { PrintDocumentPrintJob.PreviewMaxRasterZoomFactor = value; }
        }
        static PrinterWriter()
        {
            PrintDocumentPrintJob._onException = e =>
            {
                //Common.ShowExceptionDialog(e, true, "Error printing");
                ErrorLog.WriteToLogFile(e, "");
            };
        }
        bool _fixPrinterName = false;
        bool _forcePrintPreview = false;
        public PrinterWriter(string fileName)
            : this()
        {
            if (!Text.IsNullOrEmpty(fileName))
            {
                var fn = fileName.ToUpper(CultureInfo.InvariantCulture).TrimEnd();
                if (fn == "CONSOLE" || fn == "CON:")
                {
                    PrintPreview = true;
                    PrintDialog = false;
                    Landscape = true;
                    _forcePrintPreview = true;
                }
                else if (fn == "PRINTER" || fn.Equals("prn:", StringComparison.InvariantCultureIgnoreCase))
                {
                    FileName = "";
                }
                else if (UserSettings.ContainsPrinter(fileName.Trim()))
                {
                    var ps = UserSettings.GetPrinterName(fileName.Trim());
                    this.PrinterName = ps.PrinterName;
                    _fixPrinterName = true;
                }
                else
                    FileName = fileName;

            }

        }

        public override string PrinterName
        {
            get
            {
                return base.PrinterName;
            }
            set
            {
                if (_fixPrinterName)
                    return;
                base.PrinterName = value;
            }
        }
        public override string FileName
        {
            get
            {
                return base.FileName;
            }
            set
            {
                if (value != null)
                    value = value.TrimEnd();
                base.FileName = PathDecoder.DecodePath(value);
            }
        }
        public override bool PrintPreview
        {
            get
            {
                return base.PrintPreview;
            }
            set
            {
                if (_forcePrintPreview)
                    return;
                base.PrintPreview = value;

            }
        }

        protected override DialogResult ShowPrintDialogCore(PrintDocument document)
        {
            return Common.ShowDialog(
                form =>
                {
                    using (var pDialog = new Advanced.PrintDialog { Document = document, UseEXDialog = true })
                    {
                        var result = pDialog.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            if (document.DefaultPageSettings.Landscape)
                                this.Landscape = true;
                            if (UserSettings.Version10Compatible)
                            {
                                PrinterWriter.DefaultPrinterName = pDialog.PrinterSettings.PrinterName;
                            }
                        }
                        return result;
                    }

                });

        }

        public override bool PrintDialog
        {
            get
            {
                return base.PrintDialog;
            }
            set
            {
                if (_forcePrintPreview) return;
                base.PrintDialog = value;
            }
        }
        void AddSendAsCommand(string fileExtention, Func<string, IPrintJob> createPrintJobBasedOnFileName)
        {
            _sendAsCommands.Add(addSaveAs => addSaveAs(fileExtention,
               () =>
               {
                   string name = Name;

                   foreach (var c in System.IO.Path.GetInvalidFileNameChars())
                   {
                       name = name.Replace(c, '_');
                   }
                   if (name == null || name.Trim().Length == 0)
                       name = Guid.NewGuid().ToString();
                   string tempFilePath = "";
                   try
                   {
                       tempFilePath = Path.Combine(Path.GetTempPath(), name + "." + fileExtention);
                   }
                   catch
                   {
                       name = Guid.NewGuid().ToString();
                       tempFilePath = Path.Combine(Path.GetTempPath(), name + "." + fileExtention);
                   }

                   return new PrintWithProgressUIAndActionToDoAfterwards(createPrintJobBasedOnFileName(tempFilePath), tempFilePath,
                       () =>
                       {
                           new System.Threading.Thread(
                               () =>
                               {
                                   var msg = new MapiMessage();
                                   msg.subject = "";
                                   msg.noteText = "";

                                   msg.recips = IntPtr.Zero;
                                   var mapiFileDesc = new MapiFileDesc();
                                   mapiFileDesc.position = -1;
                                   mapiFileDesc.name = Path.GetFileName(tempFilePath);
                                   mapiFileDesc.path = tempFilePath;
                                   msg.files = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MapiFileDesc)));
                                   Marshal.StructureToPtr(mapiFileDesc, msg.files, false);
                                   msg.fileCount = 1;

                                   var success = false;
                                   var currentDirectory = System.Environment.CurrentDirectory;
                                   Action restoreCurrentDirectory =
                                       () =>
                                       {
                                           if (System.Environment.CurrentDirectory != currentDirectory)
                                               System.Environment.CurrentDirectory = currentDirectory;
                                       };
                                   var timer = new System.Timers.Timer { Interval = 1000 };
                                   timer.Elapsed += (sender, args) => restoreCurrentDirectory();
                                   Func<int> send = () => MAPISendMail(new IntPtr(0), new IntPtr(0), msg, 0x1 | 0x8, 0);
                                   try
                                   {
                                       timer.Start();
                                       var r = send();
                                       if (r == 2)
                                       {
                                           var path = System.Environment.GetEnvironmentVariable("PATH");
                                           System.Environment.SetEnvironmentVariable("PATH", ".;" + path, EnvironmentVariableTarget.Process);
                                           try
                                           {
                                               r = send();
                                           }
                                           finally
                                           {
                                               System.Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
                                           }
                                       }
                                       if (r <= 1)
                                           success = true;
                                   }
                                   finally
                                   {
                                       timer.Stop();
                                       restoreCurrentDirectory();
                                       if (!success)
                                           MessageBox.Show("Send as attachment failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                       Marshal.FreeHGlobal(msg.files);
                                   }
                               })
                           { ApartmentState = System.Threading.ApartmentState.STA, IsBackground = true }.Start();
                       }, true);
               }));
        }
        public string TotalPagesPlaceholderToken { get; set; }
        public PrinterWriter()
        {
            StartPage += new Action(PrinterWriter_StartPage);
            BeforeStartPage += () => Invoke(Commands.PageHeader);
            EndPage += lastPage =>
            {
                if (!lastPage)
                    Invoke(Commands.PageFooter);
            };
            _saveAsCommands.Add(addSaveAs => addSaveAs("Tiff", () => SaveAsImage(true)));
            AddSendAsCommand("Tiff", fileName => new ImagePrintJob(GetPaperSize(), fileName));
            Init();
        }

        void Invoke(Command command)
        {
            ENV.UserMethods.CurrentIO.Value = this;
            try
            {
                Common.Invoke(command);
            }
            finally
            {
                ENV.UserMethods.CurrentIO.Value = null;
            }
        }
        protected Size GetPaperSize()
        {
            Size result = new Size(794, 1123);
            try
            {
                switch (PaperKind)
                {
                    case PaperKind.Custom:
                    case PaperKind.A4:
                        result = new Size(793, 1122);
                        break;
                    case PaperKind.A3:
                        result = new Size(1122, 1587);
                        break;
                    case PaperKind.Number9Envelope:
                        result = new Size(372, 852);
                        break;
                    case PaperKind.Legal:
                        result = new Size(816, 1344);
                        break;
                    case PaperKind.Letter:
                        result = new Size(816, 1056);
                        break;
                    default:
                        var pps = new PrinterSettings();
                        foreach (PaperSize ps in pps.PaperSizes)
                        {
                            if (ps.Kind == PaperKind && ps.Width > 0 && ps.Height > 0)
                            {
                                result = new Size(ps.Width * 96 / 100, ps.Height * 96 / 100);
                                break;
                            }
                        }
                        break;
                }
            }
            catch
            {
            }
            if (Landscape)
                return new Size(result.Height, result.Width);
            return result;
        }

        IPrintJob SaveAsImage(bool showProgressAndShowPDFWhenDone)
        {
            var fileName = (FileName ?? "").Trim();
            if (string.IsNullOrEmpty(fileName))
            {
                var d = new System.Windows.Forms.SaveFileDialog();
                d.DefaultExt = "tiff";
                d.Filter = "Tagged Image File Format (*.Tiff)|*.Tiff";
                d.AddExtension = true;
                d.RestoreDirectory = true;
                if (d.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return new CanceledPrintJob();

                fileName = d.FileName;
            }
            var pj = new ImagePrintJob(GetPaperSize(), fileName);
            if (showProgressAndShowPDFWhenDone)
                return new PrintWithProgressUIAndActionToDoAfterwards(pj, fileName, () =>
                {
                    try
                    {
                        System.Diagnostics.Process.Start(fileName);
                    }
                    catch
                    {
                    }
                }, true);
            return pj;
        }

        bool _firstPage = true;

        void PrinterWriter_StartPage()
        {
            if (_firstPage)
                _firstPage = false;
            else
                if (_callLayoutOnNewPage != null)
                _callLayoutOnNewPage();
        }


        Action _callLayoutOnNewPage;
        public Action SetCurrentLayout(ReportLayout layout, ReportSection section)
        {
            bool isNewPage = HeightFromStartOfPage == 0;
            if (_callLayoutOnNewPage != null)
                return delegate { };
            _callLayoutOnNewPage = () =>
            {
                if (layout != null && !isNewPage)
                    layout.NewPageStartedBecauseOf(this, section);
            };
            return () =>
            {
                _callLayoutOnNewPage = null;
                if (_lastSection != section)
                {
                    _lastSection = section;
                    _hasGrid = false;
                    _gridRowHeight = 0;
                    _lineFuncValue = 0;
                    foreach (var control in section.Controls)
                    {
                        var g = control as Firefly.Box.UI.Grid;
                        if (g != null)
                        {
                            _hasGrid = true;
                            _gridRowHeight = g.RowHeight;
                            _lineFuncValue = HeightFromStartOfPage - section.Height + _gridRowHeight;
                        }
                    }
                }
                else
                    if (_hasGrid)
                    _lineFuncValue = HeightFromStartOfPage - section.Height + _gridRowHeight * 2;
            };

        }


        public Number VerticalExpressionFactor { get { return _obseleteLineFactor; } }
        Number _obseleteLineFactor = 1;
        bool _obseleteLineFactorWasSet = false;
        public void SetScaleUnits(Number verticalFactor)
        {
            if (_obseleteLineFactorWasSet) return;
            _obseleteLineFactor = verticalFactor;
            _obseleteLineFactorWasSet = true;
        }
        public static void ShowPrinterSettingsDialog()
        {
            System.Windows.Forms.PrintDialog psd = new System.Windows.Forms.PrintDialog();
            psd.ShowNetwork = true;
            if (PrinterWriter.DefaultPrinterSettings != null)
                psd.PrinterSettings = (PrinterSettings)PrinterWriter.DefaultPrinterSettings.Clone();
            else
                psd.PrinterSettings = new PrinterSettings();
            if (psd.ShowDialog() == DialogResult.OK)
            {
                PrinterWriter.DefaultPrinterSettings = psd.PrinterSettings;
            }
        }
        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = PathDecoder.DecodePath(value);
            }
        }
        public static PrinterWriter FindIOByName(Text baseStreamName)
        {
            return IOFinder.FindByName<PrinterWriter>(baseStreamName);
        }

        int _usagesByName = 0;
        public override void Dispose()
        {
            if (_usagesByName == 0)
                base.Dispose();
            else
                _usagesByName--;
        }
        internal void BusinessProcessIsEnding()
        {
            if (_usagesByName == 0)
                Common.Invoke(Commands.PageFooter);

        }

        void IOByName.AddToUsageCount()
        {
            _usagesByName++;
        }


        public Number HeightForLineFunc
        {
            get
            {
                if (_lineFuncValue > 0)
                    return _lineFuncValue * VerticalExpressionFactor;
                else
                    return (int)Math.Round((double)(HeightFromStartOfPage * VerticalExpressionFactor));
            }
        }

        ReportSection _lastSection;
        bool _hasGrid;
        int _gridRowHeight;
        double _lineFuncValue;

        partial void CreateAlternativePrintJob(Action<IPrintJob> create);
        partial void Init();

        List<Action<PrintDocumentPrintJob.AddSaveAs>> _saveAsCommands = new List<Action<PrintDocumentPrintJob.AddSaveAs>>();
        List<Action<PrintDocumentPrintJob.AddSaveAs>> _sendAsCommands = new List<Action<PrintDocumentPrintJob.AddSaveAs>>();

        protected override IPrintJob CreatePrintJob()
        {
            var result = MyPrivateCreatePrintJob();
            if (!string.IsNullOrWhiteSpace(TotalPagesPlaceholderToken))
                result = new StorePagesIPrintJob(result, TotalPagesPlaceholderToken);
            return result;
        }

        private IPrintJob MyPrivateCreatePrintJob()
        {
            if (JapaneseMethods.Enabled)
            {
                if (!string.IsNullOrEmpty(PrinterName) && PrinterName.Contains("/"))
                {
                    if (PrinterName.Contains("/OPORT"))
                        this.Landscape = false;
                    else if (PrinterName.Contains("/OLAND"))
                        this.Landscape = true;
                    if (PrinterName.Contains("/PA4"))
                        PaperKind = System.Drawing.Printing.PaperKind.A4;
                    else if (PrinterName.Contains("/PB4"))
                        PaperKind = System.Drawing.Printing.PaperKind.B4;
                    else if (PrinterName.Contains("/P15X11"))
                        PaperKind = System.Drawing.Printing.PaperKind.Standard15x11;
                    PrinterName = PrinterName.Remove(PrinterName.IndexOf("/"));
                }
            }

            IPrintJob pj = null;
            CreateAlternativePrintJob(job => pj = job);
            if (pj != null) return pj;
            if (PrintPreview)
                FileName = "";
            pj = PrintDocumentPrintJob.Create(this, _saveAsCommands, _sendAsCommands, ENV.LocalizationInfo.Current.RightToLeft, new PreviewTerms()
            {
                PrintPreview = LocalizationInfo.Current.PrintPreview,
                Print = LocalizationInfo.Current.Print,
                As = LocalizationInfo.Current.PrevieAsWord,
                Save = LocalizationInfo.Current.Save,
                Send = LocalizationInfo.Current.Send,
                View = LocalizationInfo.Current.View,
                OnePageView = LocalizationInfo.Current.OnePageView,
                TwoPageView = LocalizationInfo.Current.TwoPageView,
                FourPageView = LocalizationInfo.Current.FourPageView,
                Fit = LocalizationInfo.Current.PrintPreviewFit,
                Zoom = LocalizationInfo.Current.PrintPreviewZoom,
                Page = LocalizationInfo.Current.Page,
                Of = LocalizationInfo.Current.PrintPreviewOf


            });
            if (UserSettings.BackwardCompatibleLowResolutionPrinting)
                pj = new myPintJobWrapperForBackwardCompatibleLowResolutionPrinting(pj);

            return pj;
        }

        class StorePagesIPrintJob : IPrintJob
        {
            IPrintJob _original;
            string _token;
            public StorePagesIPrintJob(IPrintJob original, string token)
            {
                _token = token;
                _original = original;
            }

            public int PageHeight { get { return _original.PageHeight; } }
            List<IPageContent> _pages = new List<IPageContent>();
            public void CreatePage(IPageContent printContent, bool lastPage)
            {
                _pages.Add(printContent);

                if (lastPage)
                {
                    foreach (var page in _pages)
                    {
                        _original.CreatePage(new TotalPageReplacingIPageContent(page, this), page == _pages[_pages.Count - 1]);
                    }
                }
            }
            class TotalPageReplacingIPageContent : IPageContent
            {
                IPageContent _original;
                StorePagesIPrintJob _parent;
                public TotalPageReplacingIPageContent(IPageContent original, StorePagesIPrintJob parent)
                {
                    _original = original;
                    _parent = parent;
                }

                public void PrintContent(IPageDrawer drawer, IPrintingUnitConvertor converter)
                {
                    _original.PrintContent(new TotalPageReplacingPageDrawer(drawer, _parent), converter);
                }
            }
            class TotalPageReplacingPageDrawer : IPageDrawer
            {
                IPageDrawer _original;
                StorePagesIPrintJob _parent;
                public TotalPageReplacingPageDrawer(IPageDrawer original, StorePagesIPrintJob parent)
                {
                    _original = original;
                    _parent = parent;
                }

                public IBrush CreateBrush(Color color)
                {
                    return _original.CreateBrush(color);
                }

                public IPen CreatePen(Color color, int penWidth, DashStyle dashStyle)
                {
                    return _original.CreatePen(color, penWidth, dashStyle);
                }

                public void DrawImage(Image image, Rectangle sourceRectangle, Rectangle targetRectangle)
                {
                    _original.DrawImage(image, sourceRectangle, targetRectangle);
                }

                public void DrawRTFString(string rtf, Rectangle rectangle, Font font, Color color, Color backColor, bool rightToLeft, HorizontalAlignment alignment, bool useVersion41)
                {
                    _original.DrawRTFString(rtf, rectangle, font, color, backColor, rightToLeft, alignment, useVersion41);
                }

                public void DrawString(string text, Font font, Color foreGround, Rectangle area, StringDrawingProperties stringDrawingProperties)
                {
                    _original.DrawString(text.Replace(_parent._token, _parent._pages.Count.ToString()), font, foreGround, area, stringDrawingProperties);
                }

                public int GetAverageCharWidth(Font font)
                {
                    return _original.GetAverageCharWidth(font);
                }
            }

            public Size MeasureRTFString(string rtf, Font font, int width, bool rightToLeft, bool useVersion41)
            {
                return _original.MeasureRTFString(rtf, font, width, rightToLeft, useVersion41);
            }

            public Size MeasureString(string text, Font font, int width, StringDrawingProperties stringDrawingProperties)
            {
                return _original.MeasureString(text, font, width, stringDrawingProperties);
            }

            public int MeasureStringAndReturnNumberOfCharsThatFit(string text, Font font, int width, int height, StringDrawingProperties stringDrawingProperties)
            {
                return _original.MeasureStringAndReturnNumberOfCharsThatFit(text, font, width, height, stringDrawingProperties);
            }
        }

        class myPintJobWrapperForBackwardCompatibleLowResolutionPrinting : IPrintJob
        {
            IPrintJob _pj;

            public myPintJobWrapperForBackwardCompatibleLowResolutionPrinting(IPrintJob pj)
            {
                _pj = pj;
            }

            public void CreatePage(IPageContent printContent, bool lastPage)
            {
                _pj.CreatePage(new myPageContentWrapper(printContent), lastPage);
            }

            class myPageContentWrapper : IPageContent
            {
                IPageContent _pc;

                public myPageContentWrapper(IPageContent pc)
                {
                    _pc = pc;
                }

                public void PrintContent(IPageDrawer drawer, IPrintingUnitConvertor converter)
                {
                    _pc.PrintContent(drawer, new myPrintingUnitConvertor(converter));
                }
            }

            class myPrintingUnitConvertor : IPrintingUnitConvertor
            {
                IPrintingUnitConvertor _uc;

                public myPrintingUnitConvertor(IPrintingUnitConvertor uc)
                {
                    _uc = uc;
                }
                public int ScaleHorizontal(float val)
                {
                    return _uc.ScaleHorizontal(val);
                }

                public int ScaleVertical(float val)
                {
                    return _uc.ScaleVertical(val);
                }

                public Rectangle ScaleForFill(RectangleF rect)
                {
                    rect.Offset(-0.5F, -0.5F);
                    return _uc.ScaleForFill(rect);
                }

                public Rectangle ScaleForString(RectangleF rect)
                {
                    rect.Inflate(0.2F, 0.2F);
                    rect.Offset(-0.5F, -0.5F);
                    return _uc.ScaleForString(rect);
                }

                public float ScaleFontSize(float val)
                {
                    return _uc.ScaleFontSize(val);
                }
            }

            public int PageHeight
            {
                get { return _pj.PageHeight; }
            }

            public Size MeasureString(string text, Font font, int width, StringDrawingProperties stringDrawingProperties)
            {
                return _pj.MeasureString(text, font, width, stringDrawingProperties);
            }

            public int MeasureStringAndReturnNumberOfCharsThatFit(string text, Font font, int width, int height, StringDrawingProperties stringDrawingProperties)
            {
                return _pj.MeasureStringAndReturnNumberOfCharsThatFit(text, font, width, height, stringDrawingProperties);
            }

            public Size MeasureRTFString(string rtf, Font font, int width, bool rightToLeft, bool useVersion41)
            {
                return _pj.MeasureRTFString(rtf, font, width, rightToLeft, useVersion41);
            }
        }
        [DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
        static extern int MAPISendMail(IntPtr sess, IntPtr hwnd, MapiMessage message, int flg, int rsv);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class MapiMessage
        {
            public int reserved;
            public string subject;
            public string noteText;
            public string messageType;
            public string dateReceived;
            public string conversationID;
            public int flags;
            public IntPtr originator;
            public int recipCount;
            public IntPtr recips;
            public int fileCount;
            public IntPtr files;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class MapiFileDesc
        {
            public int reserved;
            public int flags;
            public int position;
            public string path;
            public string name;
            public IntPtr type;
        }

        internal bool ContinuePrintingEvenIfCanceledForTesting = false;
        internal bool DoNotPrint()
        {
            return CancelPrinting && !ContinuePrintingEvenIfCanceledForTesting;
        }
    }
    class ImagePrintJob : IPrintJob
    {
        float _pageHeight, _pageWidth;
        int _pageHeightInPixels;
        Graphics _g;
        string _fileName;
        Image _myImage;
        System.Drawing.Imaging.Encoder _enc = System.Drawing.Imaging.Encoder.SaveFlag;
        ImageCodecInfo _info;
        EncoderParameters _ep = new EncoderParameters(1);
        int _horizontalResolution = 300;
        int _verticalResolution = 300;
        public ImagePrintJob(Size paperSize, string fileName)
        {

            var pageHeightInPixels = paperSize.Height;
            var pageWidthInPixels = paperSize.Width;
            _pageHeightInPixels = pageHeightInPixels;
            _pageHeight = pageHeightInPixels * _verticalResolution / 96;
            _pageWidth = pageWidthInPixels * _horizontalResolution / 96;
            _fileName = fileName;

            var b = new Bitmap((int)_pageWidth, (int)_pageHeight);
            b.SetResolution(_horizontalResolution, _verticalResolution);
            _myImage = b;
            _g = Graphics.FromImage(b);
            foreach (var ice in ImageCodecInfo.GetImageEncoders())
            {
                if (ice.MimeType == "image/tiff")
                    _info = ice;
            }
            _ep.Param[0] = new EncoderParameter(_enc, (long)EncoderValue.MultiFrame);

        }

        bool _first = true;
        public void CreatePage(IPageContent printContent, bool lastPage)
        {
            var x = new Firefly.Box.Printing.Advanced.PrintPage(printContent, lastPage, delegate
            { });
            var image = _myImage;
            if (!_first)
            {
                var b = new Bitmap((int)_pageWidth, (int)_pageHeight);
                b.SetResolution(_horizontalResolution, _verticalResolution);
                image = b;

            }
            else
            {
            }
            using (var g = Graphics.FromImage(image))
            {

                g.Clear(Color.White);

                x.PrintPageAndReturnIfHasMorePages(g, false);
            }
            if (_first)
            {
                _myImage.Save(_fileName, _info, _ep);
                _ep.Param[0] = new EncoderParameter(_enc, (long)EncoderValue.FrameDimensionPage);
                _first = false;
            }
            else
            {
                _myImage.SaveAdd(image, _ep);
                image.Dispose();
            }

            if (lastPage)
            {
                _ep.Param[0] = new EncoderParameter(_enc, (long)EncoderValue.Flush);
                _myImage.SaveAdd(_ep);
                _myImage.Dispose();
                _g.Dispose();
            }

        }

        public int PageHeight
        {
            get { return _pageHeightInPixels; }
        }

        public Size MeasureString(string text, Font font, int width, StringDrawingProperties stringDrawingProperties)
        {
            width = width * _horizontalResolution / 96;
            Size result = new Size();
            stringDrawingProperties.UseMeasuringStringFormat(
                delegate (StringFormat obj)
                {

                    SizeF measured = _g.MeasureString(text, font, width, obj);
                    result = new Size((int)Math.Ceiling(measured.Width * 96 / _horizontalResolution), (int)Math.Ceiling(measured.Height * 96 / _verticalResolution));

                });

            return result;
        }

        public int MeasureStringAndReturnNumberOfCharsThatFit(string text, Font font, int width, int height, StringDrawingProperties stringDrawingProperties)
        {
            width = width * _horizontalResolution / 96;
            height = height * _verticalResolution / 96;
            int chars = 0, lines;
            stringDrawingProperties.UseMeasuringStringFormat(
                obj =>
                _g.MeasureString(text, font, new Size(width, height), obj, out chars, out lines));
            return chars;
        }

        public Size MeasureRTFString(string rtf, Font font, int width, bool rightToLeft, bool useVersion41)
        {
            width = width * _horizontalResolution / 96;
            return PageDrawerClass.MeasureRTFString(rtf, _g, font, width, rightToLeft, useVersion41);
        }
    }
    class PrintWithProgressUIAndActionToDoAfterwards : IPrintJob
    {
        IPrintJob _job;
        Action _doAfter;
        string _fileName;

        Action addPageToUI = () => { };
        Action closeUI = () => { };

        bool _firstPage = true;
        bool _openUI;
        public PrintWithProgressUIAndActionToDoAfterwards(IPrintJob job, string fileName, Action doAfter, bool openUI)
        {
            _openUI = openUI;
            _job = job;
            _doAfter = doAfter;
            _fileName = fileName;
        }

        public void CreatePage(IPageContent printContent, bool lastPage)
        {
            if (_firstPage && _openUI)
            {
                var progressForm = new Form
                {
                    Text = "Saving " + Path.GetFileName(_fileName),
                    ControlBox = false,
                    FormBorderStyle = FormBorderStyle.FixedToolWindow,
                    ClientSize = new Size(260, 100),
                    StartPosition = FormStartPosition.CenterScreen
                };
                var label = new Label { Location = new Point(60, 40), Width = 120 };
                progressForm.Controls.Add(label);
                label.BackColor = progressForm.BackColor;
                var pageCount = 0;
                addPageToUI = () =>
                {
                    label.Text = "Processing page " + ++pageCount;
                    progressForm.Update();
                    label.Update();
                };
                progressForm.Show();
                closeUI = () => progressForm.Close();
            }
            _firstPage = false;
            addPageToUI();
            try
            {
                _job.CreatePage(printContent, lastPage);
            }
            finally
            {
                if (lastPage)
                {
                    closeUI();
                    _doAfter();
                }
            }
        }

        public int PageHeight
        {
            get { return _job.PageHeight; }
        }

        public Size MeasureString(string text, Font font, int width, StringDrawingProperties stringDrawingProperties)
        {
            return _job.MeasureString(text, font, width, stringDrawingProperties);
        }

        public int MeasureStringAndReturnNumberOfCharsThatFit(string text, Font font, int width, int height, StringDrawingProperties stringDrawingProperties)
        {
            return _job.MeasureStringAndReturnNumberOfCharsThatFit(text, font, width, height, stringDrawingProperties);
        }

        public Size MeasureRTFString(string rtf, Font font, int width, bool rightToLeft, bool useVersion41)
        {
            return _job.MeasureRTFString(rtf, font, width, rightToLeft, useVersion41);
        }
    }
    class CanceledPrintJob : IPrintJob
    {
        public void CreatePage(IPageContent printContent, bool lastPage)
        {

        }

        public int PageHeight
        {
            get { return 0; }
        }


        public Size MeasureString(string text, Font font, int width, StringDrawingProperties stringDrawingProperties)
        {

            return new Size(text.Length, 10);
        }

        public int MeasureStringAndReturnNumberOfCharsThatFit(string text, Font font, int width, int height, StringDrawingProperties stringDrawingProperties)
        {
            return text.Length;
        }

        public Size MeasureRTFString(string rtf, Font font, int width, bool rightToLeft, bool useVersion41)
        {
            return new Size(rtf.Length, 10);
        }
    }


}
