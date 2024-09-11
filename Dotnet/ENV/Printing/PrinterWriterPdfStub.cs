using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ENV.Printing
{
    partial class PrinterWriter
    {
        bool _pdf;
        public bool Pdf
        {
            get { return _pdf; }
            set
            {
                if (!_pdf && value)
                    PrinterName = PathDecoder.DecodePath(UserSettings.GetPrinterName("PrintToPDF").PrinterName);
                _pdf = value;
            }
        }


      
    }
}
