using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ENV.IO.Advanced.Internal;
using ENV.Printing;
using Firefly.Box;

namespace ENV.IO.Advanced
{
    public class TextPrinterWriter : Writer
    {

        internal override Printer GetPrinter()
        {
            return Printer;
        }

        Action _startNewPage;
        public bool AutoNewPage { get; set; }
        public TextPrinterWriter()
        {
            AutoNewPage = true;
            _pagePrinter = new ENV.Utilities.Lazy<PageManager>(
                  delegate()
                  {
                      PageManager pageManager =
                          new PageManager(()=>AutoNewPage);
                      _startNewPage =
                    delegate
                    {
                        _startNewPage =
                            delegate()
                            {
                                if (!_ignoreNewPage)
                                    OnWrite("\f");
                            };
                    };
                      pageManager.NewPageEvent += () =>
                      {

                          _startNewPage();
                          if (_pageHeader != null)
                              _pageHeader.WriteTo(this);
                          if (_onNewPage != null)
                              _onNewPage();
                      };

                      if (_pageFooter != null)
                      {
                          pageManager.PageFooterHeight = _pageFooter.HeightInChars;
                          pageManager.PageEnds += (lastPage) =>
                          {
                              if (_pagePrinter.Instance.HeightUntilEndOfPage > 0)
                                  new TextSection { HeightInChars =(int) _pagePrinter.Instance.HeightUntilEndOfPage }
                                      .WriteTo(this);
                              _pageFooter.WriteTo(this);
                          };

                      }
                      pageManager.SetPageHeight(LinesPerPage);

                      return pageManager;
                  });

        }

        string _name = null;
        public virtual string Name
        {
            get { return _name ?? (_name = (System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetExecutingAssembly()).GetName().Name); }
            set { _name = value; }
        }
        ITextWriter _writer;
        public bool PrintDialog { get; set; }
        internal virtual ITextWriter _CreateWriter()
        {
            var printerName = _printer.PrinterName;
            if (PrintDialog)
            {
                var pDialog = new PrintDialog { UseEXDialog = true };
                if (pDialog.ShowDialog() == DialogResult.OK)
                {
                    printerName = pDialog.PrinterSettings.PrinterName;
                }
                else
                    return new DummyITextWriter();
                pDialog.Dispose();
            }
            var w = new WindowsTextPrinterAdapterToBasicWriter(printerName, Name);
            try
            {
                w.Open();
            }
            catch
            {
                w.Dispose();
                return new DummyITextWriter();

            }
            return w;
        }

        protected override void OnOpen()
        {
            _writer = _CreateWriter();
            base.OnOpen();
        }

        bool _wasEverWritenTo = false;
        protected override void OnWrite(string text)
        {
            _wasEverWritenTo = true;
            _writer.Write(text);
        }
        public override void Dispose()
        {
            _pagePrinter.Instance.Close();
            if (!_ignoreNewPage && _wasEverWritenTo && LinesPerPage != 0)
                OnWrite("\f");
            base.Dispose();
            if (_writer!=null)
                _writer.Dispose();
        }
        ENV.Utilities.Lazy<PageManager> _pagePrinter;

        Action _onNewPage = null;
        internal override void WriteSection(int width, int height, TextPrintingStyle style, Action<TextControlWriter> writeCommand, Action newPageStatedDueToLackOfSpace)
        {
            var command = new PrintCommandAdapterToActionTextControlWriter(width, height, style, writeCommand);
            Action after = delegate { };
            if (_pagePrinter.Instance.Height > 0 && _onNewPage == null)
            {
                _onNewPage = newPageStatedDueToLackOfSpace;
                after = () => _onNewPage = null;
            }
            _pagePrinter.Instance.Print(command.Height, () => command.PrintTo(this));
            after();
        }

        class PrintCommandAdapterToActionTextControlWriter
        {
            int _width;
            int _height;
            TextPrintingStyle _style;
            Action<TextControlWriter> _writeCommand;

            public PrintCommandAdapterToActionTextControlWriter(int width, int height, TextPrintingStyle style, Action<TextControlWriter> writeCommand)
            {
                _width = width;
                _height = height;
                _style = style;
                _writeCommand = writeCommand;
            }

            public int Height
            {
                get { return _height; }
            }

            public void PrintTo(Writer helper)
            {
                helper._ActualWriteSection(_width, _height, _style, _writeCommand, delegate { });
            }
        }
        /// <summary>
        /// Writes text directly to the printer. 
        /// </summary>
        /// <remarks>
        /// Uses the <see cref="String.Format(string,object[])"/> method to format the data.
        /// </remarks>
        /// <param name="text">The text to write</param>
        /// <param name="formatArgs">The arguments for the <see cref="String.Format(string,object[])"/> method</param>
        public void WriteLine(string text, params object[] formatArgs)
        {
            string newText = formatArgs.Length > 0 ? string.Format(text, formatArgs) : text;

            WriteSection(newText.Length, 1, TextPrintingStyle.Default,
                            delegate(TextControlWriter obj)
                            {
                                obj.Write(newText, 0, 0, newText.Length, TextPrintingStyle.Default, System.Drawing.ContentAlignment.TopLeft);
                            }, delegate
                            {

                            });

        }

        /// <summary>
        /// Gets the number of lines written from the beginning of the current page
        /// </summary>
        public virtual int HeightFromStartOfPage
        {
            get { return (int) _pagePrinter.Instance.Height; }
        }
        /// <summary>
        /// Gets the number of lines remaining in the current page
        /// </summary>
        public int HeightUntilEndOfPage
        {
            get { return (int)_pagePrinter.Instance.HeightUntilEndOfPage; }
        }
        [Obsolete("Had spelling mistake, replaced with HeightUntilEndOfPage")]
        public int HeightUntillEndOfPage
        {
            get { return (int)_pagePrinter.Instance.HeightUntilEndOfPage; }
        }




        int _linesPerPage = -1;
        TextSection _pageHeader = null;
        TextSection _pageFooter = null;


        public int LinesPerPage
        {
            get
            {
                if (_linesPerPage>-1)
                    return _linesPerPage;
                if (Printer != null)
                {
                    var x = Printer.LinesPerTextPage;
                    if (x > -1)
                        return x;
                }
                return 60;
            }
            set
            {
                _linesPerPage = value;
            }
        }

        Printing.Printer _printer = new Printing.Printer();
        bool _ignoreNewPage;

        public virtual Printing.Printer Printer
        {
            get { return _printer; }
            set
            {
                _printer = value;
            }
        }

        public TextSection PageHeader
        {
            set { _pageHeader = value; }
        }


        public TextSection PageFooter
        {
            set { _pageFooter = value; }
        }

        public int Page
        {
            get { return _pagePrinter.Instance.PageCount; }
        }

        public bool IgnoreNewPage
        {
            get { return _ignoreNewPage; }
            set { _ignoreNewPage = value; }
        }

        public virtual void NewPage()
        {
            _pagePrinter.Instance.NewPage();
        }
        public void EndCurrentPage()
        {
            _pagePrinter.Instance.EndCurrentPageWithoutStartingANewOne();
            NewPageOnNextWrite = true;
        }
        public bool NewPageOnNextWrite
        {
            set
            {
                _pagePrinter.Instance.NewPageOnNextWrite=value;
            }
            get {
                return _pagePrinter.Instance.NewPageOnNextWrite;
            }
        }
    }
}
