using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Firefly.Box.Printing.Advanced;

namespace Firefly.Box.Printing.Advanced
{
    class PrintDocumentPrintJob : IPrintJob
    {
        public delegate void AddSaveAs(string saveAsWhat, Func<IPrintJob> getPrintJob);
        public static bool PreviewDefaultZoom100 { get; set; }
        public static bool PreviewBackwardCompatibleToolbar { get; set; }
        public static double PreviewMaxRasterZoomFactor { get; set; }
        static PrintDocumentPrintJob()
        {
            PreviewMaxRasterZoomFactor = 1.3;
        }
        internal static Action<Exception> _onException = delegate { };
        public static IPrintJob Create(PrinterWriter pw, List<Action<AddSaveAs>> saveAsCommands, List<Action<AddSaveAs>> sendAsCommands, RightToLeft rightToLeft, PreviewTerms previewTerms)
        {
            var pdc = new PrintDocument();
            pdc.DocumentName = pw.Name;
            if (PrinterWriter.DefaultPrinterSettings != null)
            {
                var ps = (PrinterSettings)PrinterWriter.DefaultPrinterSettings.Clone();
                ps.DefaultPageSettings.PrinterSettings = ps;
                pdc.PrinterSettings = ps;
            }
            if (PrinterWriter.DefaultPageSettings != null)
                pdc.DefaultPageSettings = (PageSettings)PrinterWriter.DefaultPageSettings.Clone();
            string oldPrinterName = pdc.PrinterSettings.PrinterName;
            if (!string.IsNullOrEmpty(pw.PrinterName))
            {
                pdc.PrinterSettings.PrinterName = pw.PrinterName;
                if (!pdc.PrinterSettings.IsValid)
                    pdc.PrinterSettings.PrinterName = oldPrinterName;
            }
            else if (!string.IsNullOrEmpty(PrinterWriter.DefaultPrinterName) && !oldPrinterName.Equals(PrinterWriter.DefaultPrinterName, StringComparison.InvariantCultureIgnoreCase))
            {
                pdc.PrinterSettings.PrinterName = PrinterWriter.DefaultPrinterName;
                if (!pdc.PrinterSettings.IsValid)
                    pdc.PrinterSettings.PrinterName = oldPrinterName;
            }
            else pdc.PrinterSettings.PrinterName = oldPrinterName; // Improves performance when using default printer

            if (PrinterWriter.DefaultPrinterSettings == null || pw.PrintDialog)
            {
                pdc.PrinterSettings.Copies = (short)pw.Copies;
                pdc.DefaultPageSettings.Landscape = pw.Landscape;
                if (pw.PaperKind != 0)
                {
                    foreach (PaperSize size in pdc.PrinterSettings.PaperSizes)
                    {
                        if (size.Kind == pw.PaperKind)
                        {
                            pdc.DefaultPageSettings.PaperSize = size;
                            break;
                        }
                    }
                }
            }

            pw.ShowPrintDialog(pdc);
            var fileName = (pw.FileName ?? "").Trim();
            if (fileName != "")
            {
                pdc.PrinterSettings.PrintToFile = true;
                try
                {
                    if (System.IO.File.Exists(fileName))
                        System.IO.File.Delete(fileName);
                    pdc.PrinterSettings.PrintFileName = fileName;
                }
                catch
                {
                    pw.CancelPrinting = true;
                }
            }
            if (pw.CancelPrinting)
                return new CanceledPrintJob(1056);
            return new PrintDocumentPrintJob(pdc, pw.PrintPreview, saveAsCommands, sendAsCommands, rightToLeft, previewTerms);
        }

        MyPrintDocument _printDocument;
        event Action DoDispose;
        bool _preview;
        List<Action<AddSaveAs>> _saveAsCommands;
        List<Action<AddSaveAs>> _sendAsCommands;

        delegate Action<int> AddDropDown(string caption, string[] options, Action<int> selectedItemChanged, int defaultItem, int width);
        internal class MyPrintDocument
        {
            PrintDocument _document;
            PreviewTerms _previewTerms;

            public MyPrintDocument(PrintDocument document, PreviewTerms previewTerms)
            {
                _document = document;
                _previewTerms = previewTerms;
                _document.PrintPage += (s, e) => Implementation.PrintPage(e);
                _document.EndPrint += delegate { Implementation.EndPrint(); };
            }
            public void Dispose()
            {
                _document.Dispose();
            }

            public IPrintDocumentImplementation Implementation { get; set; }
            public bool LandscapeMismatchBetweenDefaultPageSettingsAndDefaultPrinterSettings { get { return _document.DefaultPageSettings.Landscape != _document.PrinterSettings.DefaultPageSettings.Landscape; } }

            internal interface IPrintDocumentImplementation
            {
                void BeginPrint(int fromPage, int toPage);
                void PrintPage(PrintPageEventArgs e);
                void EndPrint();
            }
            public Graphics CreateMeasurementGraphics()
            {
                return _document.PrinterSettings.CreateMeasurementGraphics(_document.DefaultPageSettings);
            }
            static internal Func<string, string> _fixDocumentName = s => s;
            public void Print()
            {
                Implementation.BeginPrint(0, 0);
                var x = _document.DocumentName;
                try
                {
                    _document.DocumentName = _fixDocumentName(x);
                    _document.Print();
                }
                finally
                {
                    _document.DocumentName = x;
                }
            }

            internal PrintingStrategy CreatePrinterStrategy(PrintDocumentPrintJob printDocumentPrintJob)
            {
                return new PrintingStrategyWrapperForRunningInUIThread(new Printer(printDocumentPrintJob, _document));
            }

            public void PrintWithDialog(int totalPages)
            {
                var pd = new PrintDialog()
                {
                    Document = _document,
                    AllowSelection = false,
                    AllowSomePages = true,
                    UseEXDialog = true,

                };

                var oPrinterSettings = (PrinterSettings)_document.PrinterSettings.Clone();
                var oPageSettings = (PageSettings)oPrinterSettings.DefaultPageSettings.Clone();
                var oPageSize = oPageSettings.Bounds.Size;

                if (pd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (oPageSize != pd.PrinterSettings.DefaultPageSettings.Bounds.Size && MessageBox.Show(_previewTerms.PageSizeChangedWarningText, _previewTerms.PageSizeChangedWarningCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                        {
                            oPageSettings.PrinterSettings = oPrinterSettings;
                            _document.PrinterSettings = oPrinterSettings;
                            _document.DefaultPageSettings = oPageSettings;
                            return;
                        }
                        if (pd.PrinterSettings.FromPage > totalPages)
                            return;
                        Implementation.BeginPrint(pd.PrinterSettings.FromPage, Math.Min(totalPages, pd.PrinterSettings.ToPage));
                        _document.Print();
                    }
                    catch (Exception e)
                    {
                        _onException(e);
                    }
                }
            }

            public void PrintUsing(IPrintDocumentImplementation impl, PrintController previewPrintController)
            {
                var original = _document.PrintController;
                var originalImpl = Implementation;
                try
                {
                    Implementation = impl;
                    _document.PrintController = previewPrintController;
                    _document.Print();
                }
                catch (Exception ex)
                {
                    var x = _document.PrinterSettings.PrinterName;
                    try
                    {
                        _document.PrinterSettings.PrinterName = new PrinterSettings().PrinterName;
                        _document.Print();
                    }
                    catch
                    {
                        throw ex;
                    }
                    finally
                    {
                        _document.PrinterSettings.PrinterName = x;
                    }
                }
                finally
                {
                    _document.PrintController = original;
                    Implementation = originalImpl;
                }

            }
        }

        class PrintSelectedPages : MyPrintDocument.IPrintDocumentImplementation
        {
            int _currentPage = 0;
            int _fromPage = 0;
            int _toPage = 0;
            PrintDocumentPrintJob _parent;

            public PrintSelectedPages(PrintDocumentPrintJob parent)
            {
                _parent = parent;
            }

            public void BeginPrint(int fromPage, int toPage)
            {
                _fromPage = fromPage;
                _toPage = toPage;
                _currentPage = fromPage > 0 ? fromPage - 1 : 0;
            }

            public void PrintPage(PrintPageEventArgs e)
            {

                e.HasMorePages = _parent._pages[_currentPage].PrintPageAndReturnIfHasMorePages(e.Graphics, false) &&
                            (_toPage == 0 || _currentPage < _toPage - 1);
                _currentPage++;
            }

            public void EndPrint()
            {

            }
        }
        PreviewTerms _previewTerms;
        PrintDocumentPrintJob(PrintDocument printDocument, bool preview, List<Action<AddSaveAs>> saveAsCommands, List<Action<AddSaveAs>> sendAsCommands, RightToLeft rightToLeft, PreviewTerms previewTerms)
        {
            _previewTerms = previewTerms;
            _printDocument = new MyPrintDocument(printDocument, previewTerms);
            _saveAsCommands = saveAsCommands;
            _sendAsCommands = sendAsCommands;
            _getMeasurementGraphics = () =>
            {
                try
                {
                    var result = _printingStrategy.CreateMeasurementGraphics();
                    _getMeasurementGraphics = () => result;
                    DoDispose += result.Dispose;
                    return result;
                }
                catch (Exception ex)
                {
                    _printingCanceled = true;
                    _getMeasurementGraphics = () => null;
                    return null;
                }

            };
            _newPageCreated = () => _waitForBusinessLogicToDeliverPages.Set();
            DoDispose += _printDocument.Dispose;
            _preview = preview;

            _printDocument.Implementation = new PrintSelectedPages(this);

            if (_preview)
            {
                _printingStrategy = new Preview(this, rightToLeft);
            }
            else
                _printingStrategy = _printDocument.CreatePrinterStrategy(this);

        }
        public static Action<Action<System.Windows.Forms.Form>> SetPreviewOwnerForm;

        Queue<PrintPage> _printingActions = new Queue<PrintPage>();
        internal interface PrintingStrategy
        {
            void StartNewPage(PrintPage printAction);
            void FinishedStartPage(bool lastPage);
            void StartPrintJob();
            Graphics CreateMeasurementGraphics();
            int GetPageHeight();
        }

        class PrintingStrategyWrapperForRunningInUIThread : PrintingStrategy
        {
            PrintingStrategy _wrapped;
            public PrintingStrategyWrapperForRunningInUIThread(PrintingStrategy wrapped)
            {
                _wrapped = wrapped;
            }

            public Graphics CreateMeasurementGraphics()
            {
                Graphics result = null;
                Context.Current.InvokeUICommand(() => result = _wrapped.CreateMeasurementGraphics());
                return result;
            }

            public void FinishedStartPage(bool lastPage)
            {
                Context.Current.InvokeUICommand(() => _wrapped.FinishedStartPage(lastPage));
            }

            public int GetPageHeight()
            {
                int result = 0;
                Context.Current.InvokeUICommand(() => result = _wrapped.GetPageHeight());
                return result;
            }

            public void StartNewPage(PrintPage printAction)
            {
                Context.Current.InvokeUICommand(() => _wrapped.StartNewPage(printAction));
            }

            public void StartPrintJob()
            {
                Context.Current.InvokeUICommand(() => _wrapped.StartPrintJob());
            }
        }

        class Preview : PrintingStrategy
        {

            class FormWhichEscapeKeyCloses : System.Windows.Forms.Form
            {
                public Action PrintCommand = delegate { };
                protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
                {

                    if (keyData == (Keys.Control | Keys.P))
                        PrintCommand();

                    CloseIfNeeded(keyData);


                    return base.ProcessCmdKey(ref msg, keyData);
                }

                void CloseIfNeeded(Keys keyData)
                {
                    if (keyData == Command.Exit.Shortcut)
                    {
                        Close();
                        return;
                    }
                    foreach (var s in Command.Exit.AdditionalShortcuts)
                    {
                        if (s == keyData)
                        {
                            Close();
                            return;
                        }
                    }

                }
            }

            PrintDocumentPrintJob _parent;

            bool _first = true;
            public Preview(PrintDocumentPrintJob parent, RightToLeft rightToLeft)
            {
                _parent = parent;
                _rightToLeft = rightToLeft;
            }

            Graphics _g;
            Dictionary<Image, Image> _imageCache = new Dictionary<System.Drawing.Image, System.Drawing.Image>();
            Action _whenReportIsDone = delegate { };

            public void StartNewPage(PrintPage printAction)
            {
                printAction.StoreDrawingCommands(_imageCache);
                var result = printAction.PrintPageAndReturnIfHasMorePages(GetMeasurementGraphics(), false);
                if (!result)
                {
                    _g.Dispose();
                    _g = null;
                    _whenReportIsDone();
                    _myControl.Invalidate();
                }
                _myControl.AddPage(printAction);
                if (_first)
                {
                    ShowPreview();
                    _first = false;
                }
                _parent._printingActions.Enqueue(printAction);
            }


            public void FinishedStartPage(bool lastPage)
            {

            }

            RightToLeft _rightToLeft;

            MyPreviewControl _myControl;

            public void StartPrintJob()
            {
                _myControl = new MyPreviewControl(_parent._printDocument, _parent._pages)
                {
                    Zoom = 1.0 * ZOOM_FACTOR,
                    Dock = DockStyle.Fill,
                    AutoZoom = true,
                    RightToLeft = _rightToLeft,
                    MaxRasterZoom = PreviewMaxRasterZoomFactor
                };
            }

            const double ZOOM_FACTOR = 1;

            void ShowPreview()
            {
                var parentFormHandle = IntPtr.Zero;
                if (SetPreviewOwnerForm != null)
                    SetPreviewOwnerForm(parentForm => parentFormHandle = parentForm.Handle);
                var wait = new ManualResetEvent(parentFormHandle == IntPtr.Zero);
                var printingThread = new System.Threading.Thread(
                    delegate ()
                    {
                        var f = new FormWhichEscapeKeyCloses()
                        {
                            WindowState = FormWindowState.Maximized,
                            ShowInTaskbar = true,
                            MinimizeBox = true,
                            Text = _parent._previewTerms.PrintPreview
                        };

                        try
                        {
                            f.MouseWheel +=
                                (sender, args) =>
                                    _myControl.Scroll((args.Delta / SystemInformation.MouseWheelScrollDelta));
                            f.Controls.Add(_myControl);

                            var toolStrip = new System.Windows.Forms.ToolStrip();

                            toolStrip.RightToLeft = _rightToLeft;

                            var resources =
                                new System.ComponentModel.ComponentResourceManager(
                                    typeof(PrintDocumentPrintJob));

                            if (PreviewBackwardCompatibleToolbar)
                            {
                                var closeImage = (System.Drawing.Bitmap)resources.GetObject("Close");
                                closeImage.MakeTransparent();

                                toolStrip.Items.Add("",
                                closeImage,
                                (sender, args) =>
                                {
                                    f.Close();
                                }).ToolTipText = _parent._previewTerms.Exit;
                            }

                            var whenReportIsDone = new List<Action>();
                            Func<ToolStripItem, ToolStripItem>
                                commandThatShouldOnlyBeEnabledWhenComplete =
                                    y =>
                                    {
                                        y.Enabled = false;
                                        whenReportIsDone.Add(() => y.Enabled = true);
                                        return y;
                                    };
                            var printImage = (System.Drawing.Bitmap)resources.GetObject("Print");
                            printImage.MakeTransparent();

                            f.PrintCommand = delegate
                            {
                                try
                                {

                                    _parent._printDocument.Print();
                                }
                                catch (Exception e)
                                {
                                    _onException(e);
                                }
                            };
                            commandThatShouldOnlyBeEnabledWhenComplete(toolStrip.Items.Add("",
                                printImage,
                                (sender, args) =>
                                {
                                    f.PrintCommand();
                                })).ToolTipText = _parent._previewTerms.Print;

                            var printDialogImage =
                                (System.Drawing.Bitmap)resources.GetObject("Legend");
                            printDialogImage.MakeTransparent();
                            commandThatShouldOnlyBeEnabledWhenComplete(toolStrip.Items.Add("",
                                printDialogImage,
                                (sender, args) =>
                                {
                                    _parent._printDocument.PrintWithDialog(_parent._pages.Count);

                                })).ToolTipText = _parent._previewTerms.Print + "...";

                            Action<Func<IPrintJob>> saveAs =
                                getPrintJob =>
                                {
                                    var pj = getPrintJob();
                                    _parent._pages.ForEach(page => page.Print(pj));
                                };

                            Action<string, Bitmap, List<Action<AddSaveAs>>> addCommandsToMenu =
                                (commandString, bitmap, addSaveAsses) =>
                                {
                                    if (addSaveAsses.Count > 0)
                                    {
                                        toolStrip.Items.Add(new ToolStripSeparator());

                                        if (addSaveAsses.Count == 1)
                                        {
                                            addSaveAsses[0](
                                                (what, getPrintJob) =>
                                                {
                                                    commandThatShouldOnlyBeEnabledWhenComplete(
                                                        toolStrip.Items.Add("", bitmap,
                                                            (sender, args) => saveAs(getPrintJob)))
                                                        .ToolTipText = commandString + _parent._previewTerms.As + what;
                                                });
                                        }
                                        else
                                        {
                                            var saveAsTSI = new ToolStripDropDownButton("", bitmap);
                                            commandThatShouldOnlyBeEnabledWhenComplete(saveAsTSI);
                                            saveAsTSI.ToolTipText = commandString + _parent._previewTerms.As;

                                            toolStrip.Items.Add(saveAsTSI);
                                            addSaveAsses.ForEach(
                                                action => action((what, getPrintJob) =>
                                                    saveAsTSI.DropDownItems.Add(
                                                        commandString + _parent._previewTerms.As + what, null,
                                                        (sender, args) => saveAs(getPrintJob))));
                                        }
                                    }
                                };

                            var saveAsBitmap = (System.Drawing.Bitmap)resources.GetObject("Save");
                            saveAsBitmap.MakeTransparent();
                            var sendAsBitmap = (System.Drawing.Bitmap)resources.GetObject("Send");
                            sendAsBitmap.MakeTransparent();

                            if (!PreviewBackwardCompatibleToolbar)
                            {
                                addCommandsToMenu(_parent._previewTerms.Save, saveAsBitmap, _parent._saveAsCommands);
                                addCommandsToMenu(_parent._previewTerms.Send, sendAsBitmap, _parent._sendAsCommands);
                            }

                            AddDropDown addDropDown =
                                (caption, options, selectedItemChanged, defaultItem, width) =>
                                {
                                    var menu = new System.Windows.Forms.ToolStripDropDownButton()
                                    {
                                        AutoSize = false,
                                        Width = width,
                                        TextAlign = ContentAlignment.MiddleLeft,
                                        ToolTipText = caption
                                    };
                                    for (var i = 0; i < options.Length; i++)
                                    {
                                        var item =
                                            new System.Windows.Forms.ToolStripMenuItem(options[i]);
                                        var itemIndex = i;
                                        item.Click +=
                                            (sender, args) =>
                                            {
                                                foreach (ToolStripMenuItem x in menu.DropDownItems)
                                                    x.Checked = x == item ? true : false;
                                                menu.Text = item.Text;
                                                selectedItemChanged(itemIndex);
                                            };
                                        if (i == defaultItem)
                                        {
                                            item.Checked = true;
                                            menu.Text = item.Text;
                                        }
                                        menu.DropDownItems.Add(item);
                                        menu.Width = Math.Max(menu.Width, item.Width);
                                    }
                                    toolStrip.Items.Add(new ToolStripSeparator());
                                    toolStrip.Items.Add(menu);

                                    return (x => menu.DropDownItems[x].PerformClick());
                                };

                            Action<int> changeSelectedZoom = i1 => { };
                            addDropDown(_parent._previewTerms.View, new[] { _parent._previewTerms.OnePageView, _parent._previewTerms.TwoPageView, _parent._previewTerms.FourPageView },
                                i =>
                                {
                                    changeSelectedZoom(7);
                                    _myControl.Columns = i == 0 ? 1 : (i == 1 ? 2 : 4);
                                    _myControl.InvalidatePreview();
                                }, 0, 100);

                            var zooms = new[] { "500%", "200%", "150%", "100%", "75%", "50%", "25%", _parent._previewTerms.Fit };
                            changeSelectedZoom =
                                addDropDown(_parent._previewTerms.Zoom, zooms,
                                    i =>
                                    {
                                        var s = zooms[i];
                                        if (s == _parent._previewTerms.Fit)
                                            _myControl.AutoZoom = true;
                                        else
                                            _myControl.Zoom = (double.Parse(s.Remove(s.Length - 1)) / 100) * ZOOM_FACTOR;
                                    }, PreviewDefaultZoom100 ? 3 : 7, 70);
                            if (PreviewDefaultZoom100)
                                _myControl.Zoom = ZOOM_FACTOR;
                            var lastClickChangedZoomToFit = true;
                            _myControl.Click += (o, eventArgs) =>
                            {
                                changeSelectedZoom(lastClickChangedZoomToFit ? 3 : 7);
                                lastClickChangedZoomToFit = !lastClickChangedZoomToFit;
                            };


                            toolStrip.Items.Add(new ToolStripSeparator());
                            toolStrip.Items.Add(new System.Windows.Forms.ToolStripLabel()
                            {
                                Text = _parent._previewTerms.Page
                            });
                            var pageSelector = new NumericUpDown() { Minimum = 1, Width = 20 };

                            pageSelector.ValueChanged +=
                                (sender, args) => _myControl.StartPage = (int)pageSelector.Value - 1;
                            _myControl.StartPageChanged += delegate
                            {
                                Action a =
                                    () =>
                                    {
                                        pageSelector.Value =
                                            Math.Max(pageSelector.Minimum,
                                                Math.Min(pageSelector.Maximum,
                                                    _myControl.StartPage + 1));
                                    };
                                if (pageSelector.InvokeRequired)
                                {
                                    pageSelector.BeginInvoke(a);
                                }
                                else
                                    a();
                            };

                            toolStrip.Items.Add(new ToolStripControlHost(pageSelector));
                            var ofLabel = new System.Windows.Forms.ToolStripLabel()
                            {
                                Text =
                                                  _parent._previewTerms.Of +
                                                  _parent
                                                  ._pages
                                                  .Count
                            };
                            toolStrip.Items.Add(ofLabel);
                            var pb = new ProgressBar
                            {
                                MarqueeAnimationSpeed = 59,
                                Width = 200,
                                Style = ProgressBarStyle.Marquee,
                                Height = 22,
                                Top = 2,
                                Left = f.ClientSize.Width - 202,
                                Anchor = AnchorStyles.Top | AnchorStyles.Right
                            };

                            f.Controls.Add(pb);
                            whenReportIsDone.Add(() => pb.Visible = false);
                            _myControl.NotifyTotalPages(c =>
                            {
                                Action a = () =>
                                {
                                    pageSelector.Maximum = c;
                                    ofLabel.Text = _parent._previewTerms.Of + c;
                                };
                                if (toolStrip.InvokeRequired)
                                    toolStrip.BeginInvoke(a);
                                else
                                    a();
                            });
                            Action z = () => _myControl.DoOnUI(() =>
                            {
                                Action what = () =>
                                {
                                    foreach (
                                        var action in
                                            whenReportIsDone)
                                    {
                                        action();
                                    }
                                };
                                if (toolStrip.InvokeRequired)
                                    toolStrip.BeginInvoke(what);
                                else
                                {
                                    what();
                                }
                            });
                            if (_g == null)
                                z();
                            else
                                _whenReportIsDone = z;

                            f.Controls.Add(toolStrip);

                            f.Shown += delegate { wait.Set(); };

                            if (parentFormHandle != IntPtr.Zero)
                            {
                                SetWindowLongPtr32(new HandleRef(f, f.Handle), -8, new HandleRef((object)f, (IntPtr)parentFormHandle));
                                Application.Run(f);
                            }
                            else
                                f.ShowDialog();
                        }
                        catch
                        {
                        }
                        finally
                        {
                            if (!f.IsDisposed)
                                f.Dispose();
                            wait.Set();
                            Firefly.Box.Context.Current.Dispose();
                        }

                    })
                {
                    IsBackground = true
                };

                Context.Current.Suspend(0);
                printingThread.ApartmentState = ApartmentState.STA;
                printingThread.Start();
                wait.WaitOne();
            }

            Graphics GetMeasurementGraphics()
            {
                if (_g == null)
                {
                    var g = _myControl.GetMeasurementGraphics();
                    var hdc = g.GetHdc();
                    try
                    {
                        var cdc = CreateCompatibleDC(hdc);
                        _g = Graphics.FromHdc(cdc);
                        _myControl.Disposed += delegate { DeleteDC(cdc); };
                    }
                    finally
                    {
                        g.ReleaseHdc();
                    }
                }
                return _g;
            }

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

            [DllImport("gdi32.dll")]
            public static extern IntPtr DeleteDC(IntPtr hDC);


            public Graphics CreateMeasurementGraphics()
            {
                return GetMeasurementGraphics();
            }

            public int GetPageHeight()
            {
                return (int) Math.Round((double) _myControl.GetPageHeight() * 96 / 100,0);
            }
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        static extern IntPtr SetWindowLongPtr32(HandleRef hWnd, int nIndex, HandleRef dwNewLong);

        public class DC : IDisposable
        {
            IntPtr _handle;

            public DC(IntPtr handle)
            {
                _handle = handle;
            }

            public IntPtr Handle
            {
                get { return _handle; }
            }

            public void Dispose()
            {

            }
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class DOCINFO
        {
            public int cbSize = 20;
            public string lpszDocName;
            public string lpszOutput;
            public string lpszDatatype;
            public int fwType;
        }

        [DllImport("gdi32.dll", EntryPoint = "CreateDC", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateDC(string lpszDriverName, string lpszDeviceName, string lpszOutput, HandleRef lpInitData);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool DeleteDC(HandleRef hDC);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int EndPage(HandleRef hDC);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int AbortDoc(HandleRef hDC);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr GlobalLock(HandleRef handle);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int EndDoc(HandleRef hDC);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int StartPage(HandleRef hDC);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int StartDoc(HandleRef hDC, DOCINFO lpDocInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr GlobalFree(HandleRef handle);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr ResetDC(HandleRef hDC, HandleRef lpDevMode);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool GlobalUnlock(HandleRef handle);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(HandleRef hDC, int nIndex);
        class Printer : PrintingStrategy
        {
            PrintDocumentPrintJob _parent;
            PrintDocument _document;

            public Printer(PrintDocumentPrintJob parent, PrintDocument document)
            {
                _parent = parent;
                _document = document;
            }

            public void StartNewPage(PrintPage printAction)
            {
                if (_first)
                {
                    IntPtr handle = GlobalLock(new HandleRef(this, modeHandle));
                    try
                    {
                        ResetDC(new HandleRef(this.dc, this.dc.Handle), new HandleRef(null, handle));
                    }
                    finally
                    {
                        GlobalUnlock(new HandleRef(this, modeHandle));
                        _first = false;
                    }
                }
                this.graphics = System.Drawing.Graphics.FromHdcInternal(this.dc.Handle);
                if ((this.graphics != null) && _document.OriginAtMargins)
                {
                    int deviceCaps = GetDeviceCaps(new HandleRef(this.dc, this.dc.Handle), 0x58);
                    int num2 = GetDeviceCaps(new HandleRef(this.dc, this.dc.Handle), 90);
                    int num3 = GetDeviceCaps(new HandleRef(this.dc, this.dc.Handle), 0x70);
                    int num4 = GetDeviceCaps(new HandleRef(this.dc, this.dc.Handle), 0x71);
                    float num5 = (num3 * 100) / deviceCaps;
                    float num6 = (num4 * 100) / num2;
                    this.graphics.TranslateTransform(-num5, -num6);
                    this.graphics.TranslateTransform((float)_document.DefaultPageSettings.Margins.Left,
                                                     (float)_document.DefaultPageSettings.Margins.Top);
                }
                if (StartPage(new HandleRef(this.dc, this.dc.Handle)) <= 0)
                {
                    throw new Win32Exception();
                }

                printAction.PrintPageAndReturnIfHasMorePages(this.graphics, false);
            }

            public void FinishedStartPage(bool lastPage)
            {
                try
                {
                    if (EndPage(new HandleRef(this.dc, this.dc.Handle)) <= 0)
                    {
                        throw new Win32Exception();
                    }
                }
                finally
                {

                    this.graphics.Dispose();
                    this.graphics = null;
                }

                if (lastPage)
                {
                    GlobalFree(new HandleRef(this, this.modeHandle));
                    this.modeHandle = IntPtr.Zero;

                    if (this.dc != null)
                    {
                        try
                        {
                            int num = _parent._printingCanceled ? AbortDoc(new HandleRef(this.dc, this.dc.Handle)) : EndDoc(new HandleRef(this.dc, this.dc.Handle));
                            if (num <= 0)
                            {
                                throw new Win32Exception();
                            }
                        }
                        finally
                        {
                            DeleteDC(new HandleRef(this.dc, this.dc.Handle));
                            this.dc = null;
                        }
                    }
                }
            }

            public void StartPrintJob()
            {
                try
                {
                    modeHandle = _document.PrinterSettings.GetHdevmode(_document.DefaultPageSettings);
                    IntPtr handle = GlobalLock(new HandleRef(null, this.modeHandle));
                    this.dc = new DC(CreateDC(GetProperty<string>(_document.PrinterSettings, "DriverName"), GetProperty<string>(_document.PrinterSettings, "PrinterNameInternal"), null, new HandleRef(null, handle)));
                    GlobalUnlock(new HandleRef(null, modeHandle));
                    var lpDocInfo = new DOCINFO();
                    lpDocInfo.lpszDocName = _document.DocumentName;
                    if (_document.PrinterSettings.PrintToFile)
                    {
                        lpDocInfo.lpszOutput = GetProperty<string>(_document.PrinterSettings, "OutputPort");
                    }
                    else
                    {
                        lpDocInfo.lpszOutput = null;
                    }
                    lpDocInfo.lpszDatatype = null;
                    lpDocInfo.fwType = 0;
                    if (StartDoc(new HandleRef(this.dc, this.dc.Handle), lpDocInfo) <= 0)
                        _parent._printingCanceled = true;
                }
                catch (Exception ex)
                {
                    _onException(ex);
                    _parent._printingCanceled = true;
                }
            }

            public Graphics CreateMeasurementGraphics()
            {
                return Graphics.FromHdc(this.dc.Handle);
            }

            public int GetPageHeight()
            {
                return (int)Math.Round(Math.Ceiling((float)GetDeviceCaps(new HandleRef(this, dc.Handle), 10) * 100 / GetDeviceCaps(new HandleRef(this, dc.Handle), 90)) * 96 / 100);
            }

            private DC dc;
            private System.Drawing.Graphics graphics;
            IntPtr modeHandle;
            bool _first = false;


        }
        public static T GetProperty<T>(PrinterSettings s, string name)
        {
            var t = s.GetType();
            return (T)t.GetProperty(name, System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(s, new object[0]);
        }

        bool _printingCanceled;

        System.Threading.AutoResetEvent _waitForBusinessLogicToDeliverPages = new System.Threading.AutoResetEvent(false);
        PrintingStrategy _printingStrategy;


        List<PrintPage> _pages = new List<PrintPage>();
        int _pageHeight;



        Action _newPageCreated;
        void VerifyStarted()
        {
            if (!_everStarted)
                _printingStrategy.StartPrintJob();
            _everStarted = true;
        }
        bool _everStarted = false;
        public void CreatePage(IPageContent printContent, bool lastPage)
        {
            if (_printingCanceled) return;
            try
            {
                VerifyStarted();

                _printingStrategy.StartNewPage(new PrintPage(printContent, lastPage, DoDispose));

                lock (this)
                {
                    _newPageCreated();
                }
                _printingStrategy.FinishedStartPage(lastPage);
            }
            catch
            {
                _printingCanceled = true;
                return;
            }
        }

        public int PageHeight
        {
            get
            {
                if (_pageHeight == 0)
                {
                    VerifyStarted();
                    _pageHeight = _printingStrategy.GetPageHeight();
                }
                return _pageHeight;
            }
        }

        Func<System.Drawing.Graphics> _getMeasurementGraphics;
        public Size MeasureString(string text, Font font, int width, StringDrawingProperties stringDrawingProperties)
        {
            if (_printingCanceled) return Size.Empty;

            if (stringDrawingProperties.Multiline)
            {
                for (var i = 0; i < text.Length; i++)
                {
                    if (text[i] == '\r' && (i + 1 >= text.Length || text[i + 1] != '\n'))
                        text = text.Insert(i + 1, "\n");
                }
            }

            var result = new Size();
            Measure(graphics => result = PageDrawerClass.MeasureString(text, graphics, font, width, stringDrawingProperties));
            return result;
        }

        void Measure(Action<Graphics> m)
        {
            var g = _getMeasurementGraphics();
            if (g != null)
                lock (g)
                {
                    m(g);
                }
        }

        public int MeasureStringAndReturnNumberOfCharsThatFit(string text, Font font, int width, int height, StringDrawingProperties stringDrawingProperties)
        {
            if (_printingCanceled) return int.MaxValue;

            int chars = 0, lines;

            Measure(graphics => stringDrawingProperties.UseMeasuringStringFormat(obj => graphics.MeasureString(text, font, new Size(width, height), obj, out chars, out lines)));

            return chars;
        }

        public Size MeasureRTFString(string rtf, Font font, int width, bool rightToLeft, bool useVersion41)
        {
            var result = new Size();
            Measure(graphics => result = PageDrawerClass.MeasureRTFString(rtf, graphics, font, width, rightToLeft, useVersion41));
            return result;
        }
    }
    class PreviewTerms
    {
        public string PrintPreview = "Print Preview";

        public string Exit = "Exit";
        public string Print = "Print";
        public string As = " as ";
        public string Save = "Save";

        public string Send = "Send";
        public string View = "View";
        public string OnePageView = "1 Page View";
        public string TwoPageView = "2 Pages View";
        public string FourPageView = "4 Pages View";
        public string Fit = "Fit";

        public string Zoom = "Zoom";
        public string Page = "Page";
        public string Of = "of ";
        public string PageSizeChangedWarningText = "Selected page size is different from preview. Continue printing?";
        public string PageSizeChangedWarningCaption = "Page Size Modified";
    }
    class PrintingLogicalToPhysicalUnits : IPrintingUnitConvertor
    {
        int _dcDpiX;
        int _dcDpiY;
        int _screenDpi;
        bool _scaleFonts;

        public PrintingLogicalToPhysicalUnits(int dcDpiX, int dcDpiY, int screenDpi, bool scaleFonts = false)
        {
            _dcDpiX = dcDpiX;
            _dcDpiY = dcDpiY;
            _screenDpi = screenDpi;
            _scaleFonts = scaleFonts;
        }

        public int ScaleHorizontal(float val)
        {
            return (int)Math.Round((decimal)val * _dcDpiX / _screenDpi);
        }

        public int ScaleVertical(float val)
        {
            return (int)Math.Round((decimal)val * _dcDpiY / _screenDpi);
        }

        public Rectangle ScaleForFill(RectangleF rect)
        {
            return new Rectangle(ScaleHorizontal(rect.X), ScaleVertical(rect.Y), ScaleHorizontal(rect.Width), ScaleVertical(rect.Height));
        }

        public Rectangle ScaleForString(RectangleF rect)
        {
            return new Rectangle(ScaleHorizontal(rect.X), ScaleVertical(rect.Y), ScaleHorizontal(rect.Width), ScaleVertical(rect.Height));
        }

        public float ScaleFontSize(float val)
        {
            return _scaleFonts ? val * ScaleVertical(1000) / 1000 : val;
        }
    }

    class PrintPage
    {
        IPageContent _printContent;

        Action _doAfterLastPAge;
        bool lastPage;

        public PrintPage(IPageContent printContent, bool lastPage, Action doAfterLastPage)
        {
            _doAfterLastPAge = doAfterLastPage;
            this._printContent = printContent;

            this.lastPage = lastPage;
        }

        class fixInsideBorderPageDrawer : IPageDrawer
        {
            IPageDrawer _drawer;

            public fixInsideBorderPageDrawer(IPageDrawer drawer)
            {
                _drawer = drawer;
            }

            public IBrush CreateBrush(Color color)
            {
                return _drawer.CreateBrush(color);
            }

            class penFixer : IPen
            {
                IPen _pen;
                fixInsideBorderPageDrawer _parent;
                int _penWidth;

                public penFixer(IPen pen, fixInsideBorderPageDrawer parent, int penWidth)
                {
                    _pen = pen;
                    _parent = parent;
                    _penWidth = penWidth;
                }

                public void Dispose()
                {
                    _pen.Dispose();
                }

                public void DrawAndFillEllipse(Rectangle area, Color fillColor)
                {
                    _pen.DrawAndFillEllipse(area, fillColor);
                }

                public void DrawAndFillRectangleWithCurveEdges(Rectangle area, Point[] points, Color fillColor)
                {
                    _pen.DrawAndFillRectangleWithCurveEdges(area, points, fillColor);
                }

                public void DrawAndFillRoundRectangle(Rectangle area, Color fillColor)
                {
                    _pen.DrawAndFillRoundRectangle(area, fillColor);
                }

                public void DrawArc(Rectangle area, int startAngle, int sweepAngle)
                {
                    _pen.DrawArc(area, startAngle, sweepAngle);
                }

                public void DrawEllipse(Rectangle area)
                {
                    _pen.DrawEllipse(area);
                }

                public void DrawLines(PointF[] pointS)
                {
                    _pen.DrawLines(pointS);
                }

                public void DrawRectangle(Rectangle rectangle)
                {
                    var r = new Rectangle(rectangle.Location, rectangle.Size);
                    r.Inflate(-_penWidth / 2, -_penWidth / 2);
                    _pen.DrawRectangle(r);
                }

                public void DrawRoundRectangle(Rectangle area)
                {
                    _pen.DrawRoundRectangle(area);
                }
            }

            public IPen CreatePen(Color color, int penWidth, DashStyle dashStyle)
            {
                return new penFixer(_drawer.CreatePen(color, penWidth, dashStyle), this, penWidth);
            }

            public void DrawImage(Image image, Rectangle sourceRectangle, Rectangle targetRectangle)
            {
                _drawer.DrawImage(image, sourceRectangle, targetRectangle);
            }

            public void DrawRTFString(string rtf, Rectangle rectangle, Font font, Color color, Color backColor, bool rightToLeft, HorizontalAlignment alignment, bool useVersion41)
            {
                _drawer.DrawRTFString(rtf, rectangle, font, color, backColor, rightToLeft, alignment, useVersion41);
            }

            public void DrawString(string text, Font font, Color foreGround, Rectangle area,
                StringDrawingProperties stringDrawingProperties)
            {
                _drawer.DrawString(text, font, foreGround, area, stringDrawingProperties);
            }

            public int GetAverageCharWidth(Font font)
            {
                return _drawer.GetAverageCharWidth(font);
            }
        }

        public float AdditionalScale = 1;
        public bool PrintPageAndReturnIfHasMorePages(System.Drawing.Graphics args, bool fixInsideBorder)
        {
            var screenDpi = 96;
            var dpiX = (int)args.DpiX;
            var dpiY = (int)args.DpiY;

            AdditionalScale = (float)args.DpiX / (float)screenDpi;
            var x = new PageDrawerClass(args);
            IPageDrawer y = x;
            if (fixInsideBorder)
                y = new fixInsideBorderPageDrawer(x);

            try
            {
                _printContent.PrintContent(y, new PrintingLogicalToPhysicalUnits(dpiX, dpiY, screenDpi));
            }
            finally
            {
                x.Dispose();
            }
            bool HasMorePages = !lastPage;
            if (lastPage)
            {
                _doAfterLastPAge();
            }
            return HasMorePages;
        }

        public void Print(IPrintJob job)
        {
            job.CreatePage(_printContent, lastPage);
        }

        public void MarkLastPage()
        {
            lastPage = true;
        }

        class PageContentSaver : IPageContent
        {
            PrintPage _parent;
            IPageContent _originalContent;
            public PageContentSaver(PrintPage parent, IPageContent originalContent, Dictionary<Image, Image> imageCache)
            {
                _imageCache = imageCache;
                _originalContent = originalContent;
                _parent = parent;
            }

            class DrawingSaver : IPageDrawer
            {
                IPageDrawer _drawer;
                public DrawingSaver(IPrintingUnitConvertor unitConveter, IPageDrawer drawer, Dictionary<Image, Image> imageCache)
                {
                    _drawer = drawer;
                    _savedContent = new SavedDrawings(unitConveter, imageCache);
                }

                SavedDrawings _savedContent;
                interface DrawItem
                {
                    void Draw(IPageDrawer drawer, SavedDrawings cache);
                }

                class SavedDrawings : IPageContent
                {
                    List<DrawItem> _item = new List<DrawItem>();
                    float _originalMasurementHorizontal;
                    float _originalMasurementVertical;
                    float _originalMasurementFontSize;
                    public SavedDrawings(IPrintingUnitConvertor unitConveter, Dictionary<Image, Image> imageCache)
                    {
                        _imageCopies = imageCache;
                        _originalMasurementHorizontal = unitConveter.ScaleHorizontal(1000);
                        _originalMasurementVertical = unitConveter.ScaleVertical(1000);
                        _originalMasurementFontSize = unitConveter.ScaleFontSize(10);
                    }

                    float _currentScaleHorizontal;
                    float _currentScaleVertical;
                    float _currentScaleFontSize;
                    public void PrintContent(IPageDrawer drawer, IPrintingUnitConvertor converter)
                    {
                        _currentScaleHorizontal = converter.ScaleHorizontal(1000) / _originalMasurementHorizontal;
                        _currentScaleVertical = converter.ScaleVertical(1000) / _originalMasurementVertical;
                        _currentScaleFontSize = converter.ScaleFontSize(10) / _originalMasurementFontSize;
                        foreach (var drawItem in _item)
                        {
                            drawItem.Draw(drawer, this);
                        }
                    }

                    public void Add(DrawItem item)
                    {
                        _item.Add(item);
                    }

                    Dictionary<Type, ArrayList> _valsNew = new Dictionary<Type, ArrayList>();


                    public Type GetFromCache<Type>(Type x)
                    {
                        ArrayList l;
                        if (!_valsNew.TryGetValue(typeof(Type), out l))
                        {
                            l = new ArrayList();
                            _valsNew.Add(typeof(Type), l);
                        }


                        foreach (var item in l)
                        {
                            if (x.Equals(item))
                                return (Type)item;
                        }
                        l.Add(x);
                        return x;
                    }
                    public int ScaleHorizontal(float v)
                    {
                        var r = (int)Math.Round(v * _currentScaleHorizontal, MidpointRounding.AwayFromZero);
                        return r == 0 && v != 0 ? (v > 0 ? 1 : -1) : r;
                    }

                    public int ScaleVertical(float v)
                    {
                        var r = (int)Math.Round(v * _currentScaleVertical, MidpointRounding.AwayFromZero);
                        return r == 0 && v != 0 ? (v > 0 ? 1 : -1) : r;
                    }

                    public PointF Scale(PointF location)
                    {
                        return new PointF(ScaleHorizontal(location.X), ScaleVertical(location.Y));
                    }
                    Point Scale(Point location)
                    {
                        return new Point(ScaleHorizontal(location.X), ScaleVertical(location.Y));
                    }
                    public PointF[] Scale(PointF[] location)
                    {
                        var result = new PointF[location.Length];
                        for (int i = 0; i < location.Length; i++)
                        {
                            result[i] = Scale(location[i]);
                        }
                        return result;
                    }
                    public Point[] Scale(Point[] location)
                    {
                        var result = new Point[location.Length];
                        for (int i = 0; i < location.Length; i++)
                        {
                            result[i] = Scale(location[i]);
                        }
                        return result;
                    }
                    public Rectangle Scale(Rectangle rect)
                    {
                        return new Rectangle(Scale(rect.Location), new Size(ScaleHorizontal(rect.Width), ScaleVertical(rect.Height)));
                    }

                    Dictionary<Image, Image> _imageCopies;
                    public Image SavedImage(Image image)
                    {
                        Image result;
                        if (!_imageCopies.TryGetValue(image, out result))
                        {
                            var i = image as Bitmap;
                            if (i != null)
                                result = i.Clone(new Rectangle(0, 0, image.Width, image.Height), image.PixelFormat);
                            else
                                result = new Bitmap(image);
                            _imageCopies.Add(image, result);
                        }
                        return result;
                    }

                    internal Font Scale(Font font)
                    {
                        if (_currentScaleFontSize != 1)
                            return new Font(font.OriginalFontName ?? font.Name, font.Size * _currentScaleFontSize, font.Style, font.Unit, font.GdiCharSet, font.GdiVerticalFont);
                        return font;
                    }
                }
                public IPageContent GetContent()
                {
                    return _savedContent;
                }

                class CachedPenInfo
                {
                    readonly CachedColor Color;
                    readonly int PenWidth;
                    readonly DashStyle DashStyle;

                    public CachedPenInfo(CachedColor color, int penWidth, DashStyle dashStyle)
                    {
                        Color = color;
                        PenWidth = penWidth;
                        DashStyle = dashStyle;
                    }
                    public override bool Equals(object obj)
                    {
                        var y = obj as CachedPenInfo;
                        if (y != null)
                            return Color.Equals(y.Color) && PenWidth.Equals(y.PenWidth) && DashStyle.Equals(y.DashStyle);
                        return false;
                    }

                    public IPen CreatePen(IPageDrawer drawer, SavedDrawings cache)
                    {
                        return drawer.CreatePen(Color.Color, cache.ScaleHorizontal(PenWidth), DashStyle);
                    }
                }
                class MySavedPen : IPen
                {
                    DrawingSaver _parent;
                    CachedPenInfo _info;

                    public MySavedPen(DrawingSaver parent, CachedPenInfo info)
                    {
                        _parent = parent;
                        _info = info;
                    }

                    public void Dispose()
                    {

                    }

                    public void DrawLines(PointF[] pointS)
                    {
                        _parent._savedContent.Add(new DrawLinesItem(_info, pointS));
                    }
                    class DrawLinesItem : DrawItem
                    {
                        CachedPenInfo _info;
                        PointF[] _points;

                        public DrawLinesItem(CachedPenInfo info, PointF[] points)
                        {
                            _info = info;
                            _points = points;
                        }

                        public void Draw(IPageDrawer drawer, SavedDrawings cache)
                        {
                            using (var p = _info.CreatePen(drawer, cache))
                            {
                                p.DrawLines(cache.Scale(_points));
                            }
                        }
                    }
                    class DrawRectangleItem : DrawItem
                    {
                        CachedPenInfo _info;
                        Rectangle _rect;

                        public DrawRectangleItem(CachedPenInfo info, Rectangle rect)
                        {
                            _info = info;
                            _rect = rect;
                        }

                        public void Draw(IPageDrawer drawer, SavedDrawings cache)
                        {
                            using (var p = _info.CreatePen(drawer, cache))
                            {
                                p.DrawRectangle(cache.Scale(_rect));
                            }
                        }
                    }

                    public void DrawRectangle(Rectangle rectangle)
                    {
                        _parent._savedContent.Add(new DrawRectangleItem(_info, rectangle));
                    }

                    class DrawEllipseItem : DrawItem
                    {
                        CachedPenInfo _info;
                        Rectangle _rect;

                        public DrawEllipseItem(CachedPenInfo info, Rectangle rect)
                        {
                            _info = info;
                            _rect = rect;
                        }

                        public void Draw(IPageDrawer drawer, SavedDrawings cache)
                        {
                            using (var p = _info.CreatePen(drawer, cache))
                            {
                                p.DrawEllipse(cache.Scale(_rect));
                            }
                        }
                    }
                    public void DrawEllipse(Rectangle area)
                    {
                        _parent._savedContent.Add(new DrawEllipseItem(_info, area));
                    }
                    class DrawAndFillEllipseItem : DrawItem
                    {
                        CachedPenInfo _info;
                        Rectangle _rect;
                        CachedColor _color;

                        public DrawAndFillEllipseItem(CachedPenInfo info, Rectangle rect, CachedColor color)
                        {
                            _info = info;
                            _rect = rect;
                            _color = color;
                        }

                        public void Draw(IPageDrawer drawer, SavedDrawings cache)
                        {
                            using (var p = _info.CreatePen(drawer, cache))
                            {
                                p.DrawAndFillEllipse(cache.Scale(_rect), _color.Color);
                            }
                        }
                    }

                    public void DrawAndFillEllipse(Rectangle area, Color fillColor)
                    {
                        _parent._savedContent.Add(new DrawAndFillEllipseItem(_info, area, _parent._savedContent.GetFromCache(new CachedColor(fillColor))));
                    }
                    class DrawAndFillRectangleWithCurveEdgesItem : DrawItem
                    {
                        CachedPenInfo _info;
                        Rectangle _rect;
                        CachedColor _color;
                        Point[] _points;

                        public DrawAndFillRectangleWithCurveEdgesItem(CachedPenInfo info, Rectangle rect, Point[] points, CachedColor color)
                        {
                            _points = points;
                            _info = info;
                            _rect = rect;
                            _color = color;
                        }

                        public void Draw(IPageDrawer drawer, SavedDrawings cache)
                        {
                            using (var p = _info.CreatePen(drawer, cache))
                            {
                                p.DrawAndFillRectangleWithCurveEdges(cache.Scale(_rect), cache.Scale(_points), _color.Color);
                            }
                        }
                    }
                    public void DrawAndFillRectangleWithCurveEdges(Rectangle area, Point[] points, Color fillColor)
                    {
                        _parent._savedContent.Add(new DrawAndFillRectangleWithCurveEdgesItem(_info, area, points,
                                                                                             _parent._savedContent.
                                                                                                 GetFromCache(
                                                                                                     new CachedColor(
                                                                                                         fillColor))));
                    }
                    class DrawArcItem : DrawItem
                    {
                        CachedPenInfo _info;
                        Rectangle _rect;
                        int _startAngle;
                        int _sweepAngle;

                        public DrawArcItem(CachedPenInfo info, Rectangle rect, int startAngle, int sweepAngle)
                        {
                            _info = info;
                            _rect = rect;
                            _startAngle = startAngle;
                            _sweepAngle = sweepAngle;
                        }

                        public void Draw(IPageDrawer drawer, SavedDrawings cache)
                        {
                            using (var p = _info.CreatePen(drawer, cache))
                            {
                                p.DrawArc(cache.Scale(_rect), _startAngle, _sweepAngle);
                            }
                        }
                    }
                    public void DrawArc(Rectangle area, int startAngle, int sweepAngle)
                    {
                        _parent._savedContent.Add(new DrawArcItem(_info, area, startAngle, sweepAngle));
                    }
                    class DrawRoundRectangleItem : DrawItem
                    {
                        CachedPenInfo _info;
                        Rectangle _rect;

                        public DrawRoundRectangleItem(CachedPenInfo info, Rectangle rect)
                        {
                            _info = info;
                            _rect = rect;
                        }

                        public void Draw(IPageDrawer drawer, SavedDrawings cache)
                        {
                            using (var p = _info.CreatePen(drawer, cache))
                            {
                                p.DrawRoundRectangle(cache.Scale(_rect));
                            }
                        }
                    }
                    public void DrawRoundRectangle(Rectangle area)
                    {
                        _parent._savedContent.Add(new DrawRoundRectangleItem(_info, area));
                    }
                    class DrawAndFillRoundRectangleItem : DrawItem
                    {
                        CachedPenInfo _info;
                        Rectangle _rect;
                        CachedColor _color;

                        public DrawAndFillRoundRectangleItem(CachedPenInfo info, Rectangle rect, CachedColor color)
                        {
                            _info = info;
                            _rect = rect;
                            _color = color;
                        }

                        public void Draw(IPageDrawer drawer, SavedDrawings cache)
                        {
                            using (var p = _info.CreatePen(drawer, cache))
                            {
                                p.DrawAndFillRoundRectangle(cache.Scale(_rect), _color.Color);
                            }
                        }
                    }
                    public void DrawAndFillRoundRectangle(Rectangle area, Color fillColor)
                    {
                        _parent._savedContent.Add(new DrawAndFillRoundRectangleItem(_info, area,
                                                                                    _parent._savedContent.GetFromCache(
                                                                                        new CachedColor(fillColor))));
                    }
                }

                public IPen CreatePen(Color color, int penWidth, DashStyle dashStyle)
                {
                    return new MySavedPen(this,
                                          _savedContent.GetFromCache(new CachedPenInfo(_savedContent.GetFromCache(new CachedColor(color)), penWidth, dashStyle)));
                }

                class DrawStringDrawItem : DrawItem
                {
                    DrawStringSharedParameters _params;
                    string text;
                    Point location;


                    public DrawStringDrawItem(string text, Point location, DrawStringSharedParameters p)
                    {
                        _params = p;
                        this.text = text;

                        this.location = location;

                    }

                    public void Draw(IPageDrawer drawer, SavedDrawings cache)
                    {
                        _params.DrawString(drawer, text, location, cache);

                    }
                }
                class DrawStringSharedParameters
                {
                    readonly StringDrawingProperties _props;
                    readonly Color _foreGround;
                    readonly Font _font;
                    readonly Size _size;

                    public DrawStringSharedParameters(StringDrawingProperties props, Color foreGround, Font font, Size size)
                    {
                        _props = props;
                        _foreGround = foreGround;
                        _font = font;
                        _size = size;
                    }
                    public override bool Equals(object obj)
                    {
                        var y = obj as DrawStringSharedParameters;
                        if (y == null)
                            return false;
                        return _props.Equals(y._props) &&
                            _foreGround.Equals(y._foreGround) &&
                            _font.Equals(y._font) &&
                            _size.Equals(y._size);

                    }

                    public void DrawString(IPageDrawer drawer, string text, Point location, SavedDrawings cache)
                    {
                        drawer.DrawString(text, cache.Scale(_font), _foreGround, cache.Scale(new Rectangle(location, _size)), _props);
                    }
                }
                public void DrawString(string text, Font font, Color foreGround, Rectangle area, StringDrawingProperties stringDrawingProperties)
                {
                    _savedContent.Add(new DrawStringDrawItem(text, area.Location, _savedContent.GetFromCache(new DrawStringSharedParameters(stringDrawingProperties, foreGround, font, area.Size))));
                }
                class DrawImageItem : DrawItem
                {
                    Image _image;
                    Rectangle _sourceRectangle, _targetRectangle;

                    public DrawImageItem(Image image, Rectangle sourceRectangle, Rectangle targetRectangle)
                    {
                        _image = image;
                        _sourceRectangle = sourceRectangle;
                        _targetRectangle = targetRectangle;
                    }

                    public void Draw(IPageDrawer drawer, SavedDrawings cache)
                    {
                        drawer.DrawImage(_image, _sourceRectangle, cache.Scale(_targetRectangle));
                    }
                }

                public void DrawImage(Image image, Rectangle sourceRectangle, Rectangle targetRectangle)
                {
                    _savedContent.Add(new DrawImageItem(_savedContent.SavedImage(image), sourceRectangle, targetRectangle));
                }
                class CachedColor
                {
                    readonly Color _c;

                    public CachedColor(Color c)
                    {
                        _c = c;
                    }

                    public Color Color
                    {
                        get { return _c; }
                    }

                    public override bool Equals(object obj)
                    {
                        var c = obj as CachedColor;
                        if (c != null)
                            return _c.Equals(c._c);
                        return false;
                    }
                    public override int GetHashCode()
                    {
                        return _c.GetHashCode();
                    }
                }
                public IBrush CreateBrush(Color color)
                {
                    var b = new MySavedBrush(_savedContent.GetFromCache(new CachedColor(color)), this);

                    return b;
                }

                class MySavedBrush : IBrush
                {
                    CachedColor _color;





                    DrawingSaver _parent;
                    public MySavedBrush(CachedColor color, DrawingSaver parent)
                    {
                        _parent = parent;
                        _color = color;
                    }


                    class CachedColorAndRect
                    {
                        public readonly CachedColor Color;
                        public readonly Size Size;

                        public CachedColorAndRect(CachedColor color, Size size)
                        {
                            Color = color;
                            Size = size;
                        }
                        public override bool Equals(object obj)
                        {
                            var y = obj as CachedColorAndRect;
                            if (y != null)
                                return Color.Equals(y.Color) && Size.Equals(y.Size);
                            return false;
                        }
                    }


                    class FillRectangleDrawItem : DrawItem
                    {
                        CachedColorAndRect _positionAndColor;
                        Point _rect;

                        public FillRectangleDrawItem(CachedColorAndRect positionAndColor, Point rect)
                        {
                            _positionAndColor = positionAndColor;
                            _rect = rect;
                        }

                        public void Draw(IPageDrawer drawer, SavedDrawings cache)
                        {

                            using (var b = drawer.CreateBrush(_positionAndColor.Color.Color))
                                b.FillRectangle(cache.Scale(new Rectangle(_rect, _positionAndColor.Size)));
                        }
                    }

                    class FillPolygonItem : DrawItem
                    {
                        CachedColor _color;
                        Point[] _points;

                        public FillPolygonItem(CachedColor color, Point[] points)
                        {
                            _color = color;
                            _points = points;
                        }

                        public void Draw(IPageDrawer drawer, SavedDrawings cache)
                        {
                            using (var b = drawer.CreateBrush(_color.Color))
                                b.FillPolygon(cache.Scale(_points));
                        }
                    }

                    public void FillPolygon(Point[] points)
                    {
                        _parent._savedContent.Add(new FillPolygonItem(_color, points));
                    }


                    public void FillRectangle(Rectangle area)
                    {
                        _parent._savedContent.Add(
                                    new FillRectangleDrawItem(
                                        _parent._savedContent.GetFromCache(new CachedColorAndRect(_color, area.Size)), area.Location));
                    }


                    public void Dispose()
                    {

                    }
                }
                class DrawRTFStringItem : DrawItem
                {
                    string _rtf;
                    Rectangle _rect;
                    Font _font;
                    CachedColor _color;
                    CachedColor _backColor;
                    bool _rightToLeft;
                    readonly HorizontalAlignment _alignment;
                    bool _useVersion41;

                    public DrawRTFStringItem(string rtf, Rectangle rect, Font font, CachedColor color, CachedColor backColor, bool rightToLeft, HorizontalAlignment alignment, bool useVersion41)
                    {
                        _rtf = rtf;
                        _rect = rect;
                        _font = font;
                        _color = color;
                        _backColor = backColor;
                        _rightToLeft = rightToLeft;
                        _alignment = alignment;
                        _useVersion41 = useVersion41;
                    }

                    public void Draw(IPageDrawer drawer, SavedDrawings cache)
                    {
                        drawer.DrawRTFString(_rtf, cache.Scale(_rect), _font, _color.Color, _backColor.Color,
                                             _rightToLeft, _alignment, _useVersion41);
                    }
                }

                public void DrawRTFString(string rtf, Rectangle rectangle, Font font, Color color, Color backColor, bool rightToLeft, HorizontalAlignment alignment, bool useVersion41)
                {
                    _savedContent.Add(new DrawRTFStringItem(rtf, rectangle, font,
                                                            _savedContent.GetFromCache(new CachedColor(color)),
                                                            _savedContent.GetFromCache(new CachedColor(backColor)),
                                                            rightToLeft, alignment, useVersion41));
                }

                public int GetAverageCharWidth(Font font)
                {
                    return _drawer.GetAverageCharWidth(font);
                }
            }

            Dictionary<Image, Image> _imageCache;
            public void PrintContent(IPageDrawer drawer, IPrintingUnitConvertor converter)
            {
                var s = new DrawingSaver(converter, drawer, _imageCache);
                _originalContent.PrintContent(s, converter);
                _parent._printContent = s.GetContent();
                //     _parent._printContent.PrintContent(drawer, converter);
            }
        }
        public void StoreDrawingCommands(Dictionary<Image, Image> imageCache)
        {

            this._printContent = new PageContentSaver(this, _printContent, imageCache);

        }

        internal void PrintContentToPreview(IPageDrawer pd, PrintingLogicalToPhysicalUnits converter)
        {
            _printContent.PrintContent(new fixInsideBorderPageDrawer(pd), converter);
        }
    }

    class CanceledPrintJob : IPrintJob
    {
        int _pageHeight;

        public CanceledPrintJob(int pageHeight)
        {
            _pageHeight = pageHeight;
        }

        public void CreatePage(IPageContent printContent, bool lastPage)
        {

        }

        public int PageHeight
        {
            get { return _pageHeight; }
        }

        public Size MeasureString(string text, Font font, int width, StringDrawingProperties stringDrawingProperties)
        {
            if (Firefly.Box.Testing.Should.__sizeForMeasureStringForTesting != Size.Empty)
                return Firefly.Box.Testing.Should.__sizeForMeasureStringForTesting;
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

        public void Dispose()
        {

        }
    }
}
