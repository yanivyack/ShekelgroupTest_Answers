using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ENV.IO;
using Firefly.Box;
using System.Drawing;

namespace ENV.Printing
{
    public class TextPrinterWriter : ENV.IO.Advanced.TextPrinterWriter, IOByName
    {
        #region Constructors
        public TextPrinterWriter(string fileName)
            : this()
        {
            if (!Text.IsNullOrEmpty(fileName))
            {
                if (ENV.UserSettings.ContainsPrinter(fileName))
                {
                    Printer = new Printer(fileName);
                    _suspendPrinterChange = true;
                }
                else if (fileName.Trim().Equals("CONSOLE", StringComparison.InvariantCultureIgnoreCase))
                    _createTextWriter = () => new ITextWriterBridgeToPrintPreview(this);
                else
                    _createTextWriter =
                        () =>
                        new ITextWriterBridgeToTextWriter(fileName, Append, Encoding);
            }
        }

        class ITextWriterBridgeToPrintPreview : ITextWriter
        {
            PrinterWriter _writer = new PrinterWriter { PrintPreview = true, Landscape = true, PaperKind = System.Drawing.Printing.PaperKind.A3 };
            ReportSection _rs;
            static Font Font = new Font("Courier New", 12);
            static float PrinterWriterHorizontalFactor = 9.6f,
            PrinterWriterVerticalFactor = 17.11f;
            TextPrinterWriter _parent;
            public ITextWriterBridgeToPrintPreview(TextPrinterWriter parent)
            {
                _parent = parent;
            }

            public void Dispose()
            {
                if (_rs != null)
                    _rs.WriteTo(_writer);
                _writer.Dispose();
            }
            int _currentLine;
            public void Write(string s)
            {
                if (s == "\f")
                {
                    _rs.WriteTo(_writer);
                    _rs = null;
                    return;
                }
                if (_rs == null)
                {
                    int lines = _parent.LinesPerPage;
                    if (lines == 0)
                        lines = 100;
                    _rs = new ReportSection
                    {
                        Width = (int)(150 * PrinterWriterHorizontalFactor),
                        Height = (int)(lines * PrinterWriterVerticalFactor)
                    };
                    _currentLine = 0;
                }
                var l = new Label
                {
                    Font = Font,
                    Top = (int)Math.Round(_currentLine * PrinterWriterVerticalFactor),

                    Width = (int)Math.Round(s.Length * PrinterWriterHorizontalFactor),
                    Text = s,
                    BackColor = Color.White,
                };
                _currentLine++;
                _rs.Controls.Add(l);

            }

            public void WriteInitBytes(byte[] obj)
            {

            }
        }
        internal PrinterWriter _alternateWrite = null;
        public override void NewPage()
        {

            base.NewPage();
            if (_alternateWrite != null)
                _alternateWrite.NewPage();
        }
        class ITextWriterBridgeToPrintPrint : ITextWriter
        {
            PrinterWriter _writer = new PrinterWriter { PrintPreview = false, PrintDialog = true, PaperKind = System.Drawing.Printing.PaperKind.A4 };
            ReportSection _rs;
            static Font Font = new Font("MS Gothic", 11);
            static float PrinterWriterHorizontalFactor = 9.6f,
            PrinterWriterVerticalFactor = 17.11f;
            TextPrinterWriter _parent;
            public ITextWriterBridgeToPrintPrint(TextPrinterWriter parent)
            {
                _parent = parent;
                //  _writer.PrintPreview = true;
                //      _writer.PrintDialog = false;
                //    _writer.FileName = @"c:\temp\j2.oxps";
                _parent._alternateWrite = this._writer;
            }

            public void Dispose()
            {
                if (_rs != null)
                    _rs.WriteTo(_writer);
                _writer.Dispose();
            }
            int _currentLine;
            public void Write(string s)
            {
                return;
                if (s == "\f")
                {
                    _rs.WriteTo(_writer);
                    _rs = null;
                    return;
                }
                if (_rs == null)
                {
                    int lines = _parent.LinesPerPage;
                    if (lines == 0)
                        lines = 100;
                    _rs = new ReportSection
                    {
                        Width = (int)(150 * PrinterWriterHorizontalFactor),
                        Height = (int)(lines * PrinterWriterVerticalFactor)
                    };
                    _currentLine = 0;
                }
                var l = new Label
                {
                    Font = Font,
                    Top = (int)Math.Round(_currentLine * PrinterWriterVerticalFactor),

                    Width = (int)Math.Round(s.Length * PrinterWriterHorizontalFactor),
                    Text = s,
                    BackColor = Color.White,
                };
                _currentLine++;
                _rs.Controls.Add(l);

            }

            public void WriteInitBytes(byte[] obj)
            {

            }
        }
        bool _suspendPrinterChange = false;
        public TextPrinterWriter(Stream fileName)
            : this()
        {
            _createTextWriter = () => new ITextWriterBridgeToTextWriter(fileName, Encoding);
        }
        public System.Text.Encoding Encoding = LocalizationInfo.Current.OuterEncoding;

        public TextPrinterWriter(System.IO.TextWriter writer)
            : this()
        {
            _createTextWriter = () => new ITextWriterBridgeToTextWriter(writer);

        }
        public TextPrinterWriter()
        {

            _helper = new HebrewTextTools.TextWritingHelper(this, false);
            Oem = DefaultOem;
            if (JapaneseMethods.Enabled)
                _createTextWriter = () => new ITextWriterBridgeToPrintPrint(this);
        }
        bool _invalid = false;

        public override int HeightFromStartOfPage
        {
            get
            {
                if (_invalid)
                    return 0;
                return base.HeightFromStartOfPage;
            }
        }
        public override Printer Printer
        {
            get
            {
                return base.Printer;
            }
            set
            {
                if (_suspendPrinterChange)
                    return;
                if (!UserSettings.Version8Compatible)
                    Oem = value.Oem;
                base.Printer = value;
                if (value != null)
                    Printer.ApplyEncodingTo(this);
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

        bool _apppend = false;
        public bool Append
        {
            set { _apppend = value; }
            get { return _apppend; }
        }
        #endregion

        Func<ITextWriter> _createTextWriter = () => null;
        public static bool DefaultOem { get; set; }
        internal override ITextWriter _CreateWriter()
        {

            try
            {
                var x = _createTextWriter();
                if (x == null)

                    x = base._CreateWriter();
                return x;

            }
            catch
            {
                _invalid = true;
                return new DummyITextWriter();
            }

        }


        #region Hebrew OEM issues

        HebrewTextTools.TextWritingHelper _helper;

        protected override string ProcessLine(string originalLine, int width, bool donotTrim)
        {
            return _helper.ProcessLine(originalLine, width, donotTrim);
        }

        protected override string ProcessControlData(string originalData, bool rightToLeft, bool hebrewDosCompatibleEditing)
        {
            string result = _helper.ProcessControlData(originalData, rightToLeft, hebrewDosCompatibleEditing);
            if (Oem && !V8Compatible)
                result = HebrewTextTools.Oem(result);
            return result;
        }

        public bool PerformRightToLeftManipulations
        {
            get { return _helper.PerformRightToLeftManipulations; }
            set { _helper.PerformRightToLeftManipulations = value; }
        }

        public bool RightToLeftFlipLine
        {
            get { return _helper.RightToLeftFlipLine; }
            set { _helper.RightToLeftFlipLine = value; }
        }

        public bool Oem
        {
            get { return _helper.Oem; }
            set { _helper.Oem = value; }
        }
        public bool OemForNonRightToLeftColumns
        {
            get { return _helper.OemForNonRightToLeftColumns; }
            set { _helper.OemForNonRightToLeftColumns = value; }
        }

        #endregion

        public static TextPrinterWriter FindIOByName(Text baseStreamName)
        {
            return IOFinder.FindByName<TextPrinterWriter>(baseStreamName);
        }

        int _usagesByName = 0;
        public bool V8Compatible { get { return _helper.V8Compatible; } set { _helper.V8Compatible = value; } }

        public override void Dispose()
        {
            if (_usagesByName == 0)
            {

                base.Dispose();
            }
            else
                _usagesByName--;
        }

        void IOByName.AddToUsageCount()
        {
            _usagesByName++;
        }
    }
}
