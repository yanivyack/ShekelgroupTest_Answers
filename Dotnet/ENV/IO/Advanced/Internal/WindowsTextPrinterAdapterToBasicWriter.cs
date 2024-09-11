using System;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

namespace ENV.IO.Advanced.Internal
{
    class WindowsTextPrinterAdapterToBasicWriter : ITextWriter
    {
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        static extern bool DeleteDC(HandleRef hDC);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        static extern int StartPage(HandleRef hDC);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int StartDoc(HandleRef hDC, DOCINFO lpDocInfo);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        static extern int EndPage(HandleRef hDC);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        static extern int EndDoc(HandleRef hDC);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        class DOCINFO
        {
            public int cbSize = 20;
            public string lpszDocName;
            public string lpszOutput;
            public string lpszDatatype;
            public int fwType;
        }


        string _printerName;
        IntPtr _hDC = IntPtr.Zero;
        DOCINFO _di = new DOCINFO();

        public WindowsTextPrinterAdapterToBasicWriter(string printerName, string printJobName)
        {
            var ps = new PrinterSettings();
            ps.PrinterName = printerName;
            if (ps.IsValid)
                _printerName = ps.PrinterName;
            else
            {
                var x = new PrinterSettings();
                if (x.IsValid)
                    _printerName = x.PrinterName;
            }
            _di.lpszDocName = printJobName;
        }

        public void Open()
        {
            _hDC = IntPtr.Zero;
            if (string.IsNullOrEmpty(_printerName))
                throw new Exception("Failed to initialize text printer");
            _hDC = CreateDC(null, _printerName, null, IntPtr.Zero);

            if (_hDC != IntPtr.Zero)
                _startDocDone = StartDoc(new HandleRef(null, _hDC), _di) > 0;

            if (!_startDocDone)
                throw new Exception("Failed to initialize text printer");
        }

        bool _startDocDone, _wasEverWrittenTo;

        [DllImport("gdi32.dll")]
        static extern int Escape(IntPtr hdc, int nEscape, int cbInput, IntPtr lpvInData, IntPtr lpvOutData);

        public void Write(string text)
        {
            if (!_startDocDone) return;
            _wasEverWrittenTo = true;
            if (text == "\f")
            {
                PageBreak();
                return;
            }
            var bytes = System.Text.Encoding.Default.GetBytes(text);
            IntPtr x = Marshal.AllocHGlobal(bytes.Length + 2);
            try
            {
                var s = (short)bytes.Length;
                var buf = new byte[bytes.Length + 2];
                buf[1] = (byte)(s >> 8);
                buf[0] = (byte)s;
                System.Array.Copy(bytes, 0, buf, 2, bytes.Length);
                for (var i = 0; i < buf.Length; i++)
                    Marshal.WriteByte(x, i, buf[i]);

                Escape(_hDC, 19, 0, x, IntPtr.Zero);
            }
            finally
            {
                Marshal.FreeHGlobal(x);
            }
        }

        public void WriteInitBytes(byte[] obj)
        {

        }

        void PageBreak()
        {
            StartPage(new HandleRef(null, _hDC));
            EndPage(new HandleRef(null, _hDC));
        }

        public void Dispose()
        {
            if (_startDocDone)
            {
                if (_wasEverWrittenTo)
                    PageBreak();
                EndDoc(new HandleRef(null, _hDC));
            }

            if (_hDC != IntPtr.Zero)
                DeleteDC(new HandleRef(null, _hDC));
        }
    }
}
