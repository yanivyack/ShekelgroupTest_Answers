using System;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ENV.Printing.Advanced
{
    sealed class PrintDialog : CommonDialog
    {
        const int
            PD_ALLPAGES = 0x00000000,
        PD_SELECTION = 0x00000001,
        PD_PAGENUMS = 0x00000002,
        PD_CURRENTPAGE = 0x00400000,
              PD_COLLATE = 0x00000010,
             PD_NOSELECTION = 0x00000004,
        PD_NOPAGENUMS = 0x00000008,
        PD_PRINTTOFILE = 0x00000020,
        PD_PRINTSETUP = 0x00000040,
        PD_NOWARNING = 0x00000080,
        PD_RETURNDC = 0x00000100,
        PD_RETURNIC = 0x00000200,
        PD_RETURNDEFAULT = 0x00000400,
        PD_SHOWHELP = 0x00000800,
        PD_ENABLEPRINTHOOK = 0x00001000,
        PD_ENABLESETUPHOOK = 0x00002000,
        PD_ENABLEPRINTTEMPLATE = 0x00004000,
        PD_ENABLESETUPTEMPLATE = 0x00008000,
        PD_ENABLEPRINTTEMPLATEHANDLE = 0x00010000,
        PD_ENABLESETUPTEMPLATEHANDLE = 0x00020000,
        PD_USEDEVMODECOPIES = 0x00040000,
        PD_USEDEVMODECOPIESANDCOLLATE = 0x00040000,
        PD_DISABLEPRINTTOFILE = 0x00080000,
        PD_HIDEPRINTTOFILE = 0x00100000,
        PD_NONETWORKBUTTON = 0x00200000,
        PD_NOCURRENTPAGE = 0x00800000,
        PD_EXCLUSIONFLAGS = 0x01000000,
        PD_USELARGETEMPLATE = 0x10000000,
             DM_IN_BUFFER = 8,
        DM_OUT_BUFFER = 2,
            DM_SPECVERSION = 0x0401,
        DM_ORIENTATION = 0x00000001,
        DM_PAPERSIZE = 0x00000002,
        DM_PAPERLENGTH = 0x00000004,
        DM_PAPERWIDTH = 0x00000008,
        DM_SCALE = 0x00000010,
        DM_COPIES = 0x00000100,
        DM_DEFAULTSOURCE = 0x00000200,
        DM_PRINTQUALITY = 0x00000400,
        DM_COLOR = 0x00000800,
        DM_DUPLEX = 0x00001000,
        DM_YRESOLUTION = 0x00002000,
        DM_TTOPTION = 0x00004000,
        DM_COLLATE = 0x00008000,
        DM_FORMNAME = 0x00010000,
        DM_LOGPIXELS = 0x00020000,
        DM_BITSPERPEL = 0x00040000,
        DM_PELSWIDTH = 0x00080000,
        DM_PELSHEIGHT = 0x00100000,
        DM_DISPLAYFLAGS = 0x00200000,
        DM_DISPLAYFREQUENCY = 0x00400000,
        DM_PANNINGWIDTH = 0x00800000,
        DM_PANNINGHEIGHT = 0x01000000,
        DM_ICMMETHOD = 0x02000000,
        DM_ICMINTENT = 0x04000000,
        DM_MEDIATYPE = 0x08000000,
        DM_DITHERTYPE = 0x10000000,
        DM_ICCMANUFACTURER = 0x20000000,
        DM_ICCMODEL = 0x40000000;

        const int printRangeMask = (int)(PrintRange.AllPages | PrintRange.SomePages
                                          | PrintRange.Selection | PrintRange.CurrentPage);

        PrinterSettings settings = null;
        PrintDocument printDocument = null;

        bool allowCurrentPage;

        bool allowPages;
        bool allowPrintToFile;
        bool allowSelection;
        bool printToFile;
        bool showHelp;
        bool showNetwork;

        bool useEXDialog = false;

        public PrintDialog()
        {
            Reset();
        }

        public bool AllowCurrentPage
        {
            get { return allowCurrentPage; }
            set { allowCurrentPage = value; }
        }

        public bool AllowSomePages
        {
            get { return allowPages; }
            set { allowPages = value; }
        }

        public bool AllowPrintToFile
        {
            get { return allowPrintToFile; }
            set { allowPrintToFile = value; }
        }

        public bool AllowSelection
        {
            get { return allowSelection; }
            set { allowSelection = value; }
        }

        public PrintDocument Document
        {
            get { return printDocument; }
            set
            {
                printDocument = value;
                if (printDocument == null)
                    settings = new PrinterSettings();
                else
                    settings = printDocument.PrinterSettings;
            }
        }

        private PageSettings PageSettings
        {
            get
            {
                if (Document == null)
                    return PrinterSettings.DefaultPageSettings;
                else
                    return Document.DefaultPageSettings;
            }
        }

        public PrinterSettings PrinterSettings
        {
            get
            {

                if (settings == null)
                {
                    settings = new PrinterSettings();
                }
                return settings;
            }
            set
            {
                if (value != PrinterSettings)
                {
                    settings = value;
                    printDocument = null;
                }
            }
        }

        public bool PrintToFile
        {
            get { return printToFile; }
            set { printToFile = value; }
        }

        public bool ShowHelp
        {
            get { return showHelp; }
            set { showHelp = value; }
        }

        public bool ShowNetwork
        {
            get { return showNetwork; }
            set { showNetwork = value; }
        }
        public bool UseEXDialog
        {
            get { return useEXDialog; }
            set { useEXDialog = value; }
        }

        private int GetFlags()
        {
            int flags = 0;

            if (!UseEXDialog || (Environment.OSVersion.Platform != System.PlatformID.Win32NT ||
                Environment.OSVersion.Version.Major < 5))
            {
                flags |= PD_ENABLEPRINTHOOK;
            }

            if (!allowCurrentPage) flags |= PD_NOCURRENTPAGE;
            if (!allowPages) flags |= PD_NOPAGENUMS;
            if (!allowPrintToFile) flags |= PD_DISABLEPRINTTOFILE;
            if (!allowSelection) flags |= PD_NOSELECTION;

            flags |= (int)PrinterSettings.PrintRange;

            if (printToFile) flags |= PD_PRINTTOFILE;
            if (showHelp) flags |= PD_SHOWHELP;
            if (!showNetwork) flags |= PD_NONETWORKBUTTON;
            if (PrinterSettings.Collate) flags |= PD_COLLATE;
            return flags;
        }

        public override void Reset()
        {
            allowCurrentPage = false;
            allowPages = false;
            allowPrintToFile = true;
            allowSelection = false;
            printDocument = null;
            printToFile = false;
            settings = null;
            showHelp = false;
            showNetwork = true;
        }

        delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        class PRINTDLGEX
        {
            public int lStructSize;

            public IntPtr hwndOwner;
            public IntPtr hDevMode;
            public IntPtr hDevNames;
            public IntPtr hDC;

            public int Flags;
            public int Flags2;

            public int ExclusionFlags;

            public int nPageRanges;
            public int nMaxPageRanges;

            public IntPtr pageRanges;

            public int nMinPage;
            public int nMaxPage;
            public int nCopies;

            public IntPtr hInstance;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpPrintTemplateName;

            public WndProc lpCallback = null;

            public int nPropertyPages;

            public IntPtr lphPropertyPages;

            public int nStartPage;
            public int dwResultAction;

        }

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        static extern IntPtr GlobalAlloc(int uFlags, int dwBytes);

        const int GMEM_FIXED = 0x0000,
           GMEM_MOVEABLE = 0x0002,
           GMEM_NOCOMPACT = 0x0010,
           GMEM_NODISCARD = 0x0020,
           GMEM_ZEROINIT = 0x0040,
           GMEM_MODIFY = 0x0080,
           GMEM_DISCARDABLE = 0x0100,
           GMEM_NOT_BANKED = 0x1000,
           GMEM_SHARE = 0x2000,
           GMEM_DDESHARE = 0x2000,
           GMEM_NOTIFY = 0x4000,
           GMEM_LOWER = GMEM_NOT_BANKED,
           GMEM_VALID_FLAGS = 0x7F72,
           GMEM_INVALID_HANDLE = 0x8000,
           GHND = (GMEM_MOVEABLE | GMEM_ZEROINIT),
           GPTR = (GMEM_FIXED | GMEM_ZEROINIT);

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Auto)]
        class PRINTPAGERANGE
        {
            public int nFromPage = 0;
            public int nToPage = 0;
        }

        static int START_PAGE_GENERAL = unchecked((int)0xffffffff);

        static PRINTDLGEX CreatePRINTDLGEX()
        {
            PRINTDLGEX data = new PRINTDLGEX();
            data.lStructSize = Marshal.SizeOf(data);
            data.hwndOwner = IntPtr.Zero;
            data.hDevMode = IntPtr.Zero;
            data.hDevNames = IntPtr.Zero;
            data.hDC = IntPtr.Zero;
            data.Flags = 0;
            data.Flags2 = 0;
            data.ExclusionFlags = 0;
            data.nPageRanges = 0;
            data.nMaxPageRanges = 1;
            data.pageRanges = GlobalAlloc(GPTR, data.nMaxPageRanges * Marshal.SizeOf(typeof(PRINTPAGERANGE)));
            data.nMinPage = 0;
            data.nMaxPage = 9999;
            data.nCopies = 1;
            data.hInstance = IntPtr.Zero;
            data.lpPrintTemplateName = null;
            data.nPropertyPages = 0;
            data.lphPropertyPages = IntPtr.Zero;
            data.nStartPage = START_PAGE_GENERAL;
            data.dwResultAction = 0;
            return data;
        }

        interface PRINTDLG
        {
            int lStructSize { get; set; }
            IntPtr hwndOwner { get; set; }
            IntPtr hDevMode { get; set; }
            IntPtr hDevNames { get; set; }
            IntPtr hDC { get; set; }
            int Flags { get; set; }
            short nFromPage { get; set; }
            short nToPage { get; set; }
            short nMinPage { get; set; }
            short nMaxPage { get; set; }
            short nCopies { get; set; }
            IntPtr hInstance { get; set; }
            IntPtr lCustData { get; set; }
            WndProc lpfnPrintHook { get; set; }
            WndProc lpfnSetupHook { get; set; }
            string lpPrintTemplateName { get; set; }
            string lpSetupTemplateName { get; set; }
            IntPtr hPrintTemplate { get; set; }
            IntPtr hSetupTemplate { get; set; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Auto)]
        class PRINTDLG_32 : PRINTDLG
        {
            int m_lStructSize;

            IntPtr m_hwndOwner;
            IntPtr m_hDevMode;
            IntPtr m_hDevNames;
            IntPtr m_hDC;

            int m_Flags;

            short m_nFromPage;
            short m_nToPage;
            short m_nMinPage;
            short m_nMaxPage;
            short m_nCopies;

            IntPtr m_hInstance;
            IntPtr m_lCustData;

            WndProc m_lpfnPrintHook;
            WndProc m_lpfnSetupHook;

            string m_lpPrintTemplateName;
            string m_lpSetupTemplateName;

            IntPtr m_hPrintTemplate;
            IntPtr m_hSetupTemplate;

            public int lStructSize { get { return m_lStructSize; } set { m_lStructSize = value; } }

            public IntPtr hwndOwner { get { return m_hwndOwner; } set { m_hwndOwner = value; } }
            public IntPtr hDevMode { get { return m_hDevMode; } set { m_hDevMode = value; } }
            public IntPtr hDevNames { get { return m_hDevNames; } set { m_hDevNames = value; } }
            public IntPtr hDC { get { return m_hDC; } set { m_hDC = value; } }

            public int Flags { get { return m_Flags; } set { m_Flags = value; } }

            public short nFromPage { get { return m_nFromPage; } set { m_nFromPage = value; } }
            public short nToPage { get { return m_nToPage; } set { m_nToPage = value; } }
            public short nMinPage { get { return m_nMinPage; } set { m_nMinPage = value; } }
            public short nMaxPage { get { return m_nMaxPage; } set { m_nMaxPage = value; } }
            public short nCopies { get { return m_nCopies; } set { m_nCopies = value; } }

            public IntPtr hInstance { get { return m_hInstance; } set { m_hInstance = value; } }
            public IntPtr lCustData { get { return m_lCustData; } set { m_lCustData = value; } }

            public WndProc lpfnPrintHook { get { return m_lpfnPrintHook; } set { m_lpfnPrintHook = value; } }
            public WndProc lpfnSetupHook { get { return m_lpfnSetupHook; } set { m_lpfnSetupHook = value; } }

            public string lpPrintTemplateName { get { return m_lpPrintTemplateName; } set { m_lpPrintTemplateName = value; } }
            public string lpSetupTemplateName { get { return m_lpSetupTemplateName; } set { m_lpSetupTemplateName = value; } }

            public IntPtr hPrintTemplate { get { return m_hPrintTemplate; } set { m_hPrintTemplate = value; } }
            public IntPtr hSetupTemplate { get { return m_hSetupTemplate; } set { m_hSetupTemplate = value; } }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        class PRINTDLG_64 : PRINTDLG
        {
            int m_lStructSize;

            IntPtr m_hwndOwner;
            IntPtr m_hDevMode;
            IntPtr m_hDevNames;
            IntPtr m_hDC;

            int m_Flags;

            short m_nFromPage;
            short m_nToPage;
            short m_nMinPage;
            short m_nMaxPage;
            short m_nCopies;

            IntPtr m_hInstance;
            IntPtr m_lCustData;

            WndProc m_lpfnPrintHook;
            WndProc m_lpfnSetupHook;

            string m_lpPrintTemplateName;
            string m_lpSetupTemplateName;

            IntPtr m_hPrintTemplate;
            IntPtr m_hSetupTemplate;

            public int lStructSize { get { return m_lStructSize; } set { m_lStructSize = value; } }

            public IntPtr hwndOwner { get { return m_hwndOwner; } set { m_hwndOwner = value; } }
            public IntPtr hDevMode { get { return m_hDevMode; } set { m_hDevMode = value; } }
            public IntPtr hDevNames { get { return m_hDevNames; } set { m_hDevNames = value; } }
            public IntPtr hDC { get { return m_hDC; } set { m_hDC = value; } }

            public int Flags { get { return m_Flags; } set { m_Flags = value; } }

            public short nFromPage { get { return m_nFromPage; } set { m_nFromPage = value; } }
            public short nToPage { get { return m_nToPage; } set { m_nToPage = value; } }
            public short nMinPage { get { return m_nMinPage; } set { m_nMinPage = value; } }
            public short nMaxPage { get { return m_nMaxPage; } set { m_nMaxPage = value; } }
            public short nCopies { get { return m_nCopies; } set { m_nCopies = value; } }

            public IntPtr hInstance { get { return m_hInstance; } set { m_hInstance = value; } }
            public IntPtr lCustData { get { return m_lCustData; } set { m_lCustData = value; } }

            public WndProc lpfnPrintHook { get { return m_lpfnPrintHook; } set { m_lpfnPrintHook = value; } }
            public WndProc lpfnSetupHook { get { return m_lpfnSetupHook; } set { m_lpfnSetupHook = value; } }

            public string lpPrintTemplateName { get { return m_lpPrintTemplateName; } set { m_lpPrintTemplateName = value; } }
            public string lpSetupTemplateName { get { return m_lpSetupTemplateName; } set { m_lpSetupTemplateName = value; } }

            public IntPtr hPrintTemplate { get { return m_hPrintTemplate; } set { m_hPrintTemplate = value; } }
            public IntPtr hSetupTemplate { get { return m_hSetupTemplate; } set { m_hSetupTemplate = value; } }
        }


        static PRINTDLG CreatePRINTDLG()
        {
            PRINTDLG data = null;
            if (IntPtr.Size == 4)
            {
                data = new PRINTDLG_32();
            }
            else {
                data = new PRINTDLG_64();
            }
            data.lStructSize = Marshal.SizeOf(data);
            data.hwndOwner = IntPtr.Zero;
            data.hDevMode = IntPtr.Zero;
            data.hDevNames = IntPtr.Zero;
            data.Flags = 0;
            data.hDC = IntPtr.Zero;
            data.nFromPage = 1;
            data.nToPage = 1;
            data.nMinPage = 0;
            data.nMaxPage = 9999;
            data.nCopies = 1;
            data.hInstance = IntPtr.Zero;
            data.lCustData = IntPtr.Zero;
            data.lpfnPrintHook = null;
            data.lpfnSetupHook = null;
            data.lpPrintTemplateName = null;
            data.lpSetupTemplateName = null;
            data.hPrintTemplate = IntPtr.Zero;
            data.hSetupTemplate = IntPtr.Zero;
            return data;
        }

        protected override bool RunDialog(IntPtr hwndOwner)
        {
            bool returnValue = false;

            WndProc hookProcPtr = new WndProc(this.HookProc);

            if (!UseEXDialog || (Environment.OSVersion.Platform != System.PlatformID.Win32NT ||
                Environment.OSVersion.Version.Major < 5))
            {
                PRINTDLG data = CreatePRINTDLG();
                returnValue = ShowPrintDialog(hwndOwner, hookProcPtr, data);
            }
            else {
                PRINTDLGEX data = CreatePRINTDLGEX();
                returnValue = ShowPrintDialog(hwndOwner, data);
            }

            return returnValue;
        }

        [DllImport("Comdlg32.dll", EntryPoint = "PrintDlg", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PrintDlg_32([In, Out] PRINTDLG_32 lppd);

        [DllImport("Comdlg32.dll", EntryPoint = "PrintDlg", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PrintDlg_64([In, Out] PRINTDLG_64 lppd);

        static bool PrintDlg([In, Out] PRINTDLG lppd)
        {
            if (IntPtr.Size == 4)
            {
                PRINTDLG_32 lppd_32 = lppd as PRINTDLG_32;
                if (lppd_32 == null)
                {
                    throw new System.NullReferenceException("PRINTDLG data is null");
                }
                return PrintDlg_32(lppd_32);
            }
            else {
                PRINTDLG_64 lppd_64 = lppd as PRINTDLG_64;
                if (lppd_64 == null)
                {
                    throw new System.NullReferenceException("PRINTDLG data is null");
                }
                return PrintDlg_64(lppd_64);
            }
        }

        [DllImport("Kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GlobalFree(HandleRef handle);

        private bool ShowPrintDialog(IntPtr hwndOwner, WndProc hookProcPtr, PRINTDLG data)
        {

            data.Flags = GetFlags();
            data.nCopies = (short)PrinterSettings.Copies;
            data.hwndOwner = hwndOwner;
            data.lpfnPrintHook = hookProcPtr;

            try
            {
                if (PageSettings == null)
                    data.hDevMode = PrinterSettings.GetHdevmode();
                else
                    data.hDevMode = PrinterSettings.GetHdevmode(PageSettings);

                data.hDevNames = PrinterSettings.GetHdevnames();
            }
            catch (InvalidPrinterException)
            {
                data.hDevMode = IntPtr.Zero;
                data.hDevNames = IntPtr.Zero;
            }
            finally
            {
            }

            try
            {
                if (AllowSomePages)
                {
                    if (PrinterSettings.FromPage < PrinterSettings.MinimumPage
                        || PrinterSettings.FromPage > PrinterSettings.MaximumPage)
                        throw new ArgumentException("FromPage");
                    if (PrinterSettings.ToPage < PrinterSettings.MinimumPage
                        || PrinterSettings.ToPage > PrinterSettings.MaximumPage)
                        throw new ArgumentException("ToPage");
                    if (PrinterSettings.ToPage < PrinterSettings.FromPage)
                        throw new ArgumentException("FromPage");

                    data.nFromPage = (short)PrinterSettings.FromPage;
                    data.nToPage = (short)PrinterSettings.ToPage;
                    data.nMinPage = (short)PrinterSettings.MinimumPage;
                    data.nMaxPage = (short)PrinterSettings.MaximumPage;
                }

                if (!PrintDlg(data))
                    return false;

                UpdatePrinterSettings(data.hDevMode, data.hDevNames, data.nCopies, data.Flags, settings, PageSettings);

                PrintToFile = ((data.Flags & PD_PRINTTOFILE) != 0);
                PrinterSettings.PrintToFile = PrintToFile;

                if (AllowSomePages)
                {
                    PrinterSettings.FromPage = data.nFromPage;
                    PrinterSettings.ToPage = data.nToPage;
                }

                if ((data.Flags & PD_USEDEVMODECOPIESANDCOLLATE) == 0)
                {
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        PrinterSettings.Copies = data.nCopies;
                        PrinterSettings.Collate = ((data.Flags & PD_COLLATE) == PD_COLLATE);
                    }
                }

                return true;
            }
            finally
            {
                GlobalFree(new HandleRef(data, data.hDevMode));
                GlobalFree(new HandleRef(data, data.hDevNames));
            }
        }

        [DllImport("Comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int PrintDlgEx([In, Out] PRINTDLGEX lppdex);

        const int PD_RESULT_CANCEL = 0;
        const int PD_RESULT_PRINT = 1;
        const int PD_RESULT_APPLY = 2;

        [DllImport("winspool.drv", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, BestFitMapping = false)]
        static extern int DocumentProperties(HandleRef hwnd, HandleRef hPrinter, string pDeviceName, IntPtr /*DEVMODE*/ pDevModeOutput, HandleRef /*DEVMODE*/ pDevModeInput, int fMode);

        static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr GlobalLock(HandleRef handle);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool GlobalUnlock(HandleRef handle);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        class DEVMODE
        {
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;
            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmICCManufacturer;
            public int dmICCModel;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        /*
        IntPtr GetHdevmode(PageSettings pageSettings)
        {
            var printer = PrinterSettings.PrinterName;
            int modeSize = DocumentProperties(NullHandleRef, NullHandleRef, printer, IntPtr.Zero, NullHandleRef, 0);
            if (modeSize < 1)
            {
                throw new InvalidPrinterException(PrinterSettings);
            }
            IntPtr handle = GlobalAlloc(GMEM_MOVEABLE, modeSize); // cannot be <0 anyway
            IntPtr pointer = GlobalLock(new HandleRef(null, handle));

            int returnCode = DocumentProperties(NullHandleRef, NullHandleRef, printer, pointer, NullHandleRef, DM_OUT_BUFFER);
            if (returnCode < 0)
            {
                throw new System.ComponentModel.Win32Exception();
            }

            DEVMODE mode = (DEVMODE)Marshal.PtrToStructure(pointer, typeof(DEVMODE));


            if ((mode.dmFields & DM_COPIES) == DM_COPIES)
            {
                if (copies != -1) mode.dmCopies = copies;
            }

            if ((mode.dmFields & DM_DUPLEX) == DM_DUPLEX)
            {
                if (unchecked((int)duplex) != -1) mode.dmDuplex = unchecked((short)duplex);
            }

            if ((mode.dmFields & DM_COLLATE) == DM_COLLATE)
            {
                if (collate.IsNotDefault)
                    mode.dmCollate = (short)(((bool)collate) ? DMCOLLATE_TRUE : DMCOLLATE_FALSE);
            }

            Marshal.StructureToPtr(mode, pointer, false);

            int retCode = DocumentProperties(NullHandleRef, NullHandleRef, printer, pointer, pointer, DM_IN_BUFFER | DM_OUT_BUFFER);
            if (retCode < 0)
            {
                GlobalFree(new HandleRef(null, handle));
                GlobalUnlock(new HandleRef(null, handle));
                return IntPtr.Zero;
            }


            GlobalUnlock(new HandleRef(null, handle));
            return handle;
        }

        void CopyToHdevmode(IntPtr hdevmode)
        {
            var ps = PageSettings ?? PrinterSettings.DefaultPageSettings;


            IntPtr modePointer = GlobalLock(new HandleRef(null, hdevmode));
            DEVMODE mode = (DEVMODE)Marshal.PtrToStructure(modePointer, typeof(DEVMODE));

            if (color.IsNotDefault && ((mode.dmFields & SafeNativeMethods.DM_COLOR) == SafeNativeMethods.DM_COLOR))
                mode.dmColor = unchecked((short)(((bool)color) ? SafeNativeMethods.DMCOLOR_COLOR : SafeNativeMethods.DMCOLOR_MONOCHROME));
            if (landscape.IsNotDefault && ((mode.dmFields & SafeNativeMethods.DM_ORIENTATION) == SafeNativeMethods.DM_ORIENTATION))
                mode.dmOrientation = unchecked((short)(((bool)landscape) ? SafeNativeMethods.DMORIENT_LANDSCAPE : SafeNativeMethods.DMORIENT_PORTRAIT));

            if (paperSize != null)
            {

                if ((mode.dmFields & SafeNativeMethods.DM_PAPERSIZE) == SafeNativeMethods.DM_PAPERSIZE)
                {
                    mode.dmPaperSize = unchecked((short)paperSize.RawKind);
                }

                bool setWidth = false;
                bool setLength = false;

                if ((mode.dmFields & SafeNativeMethods.DM_PAPERLENGTH) == SafeNativeMethods.DM_PAPERLENGTH)
                {
                    int length = PrinterUnitConvert.Convert(paperSize.Height, PrinterUnit.Display, PrinterUnit.TenthsOfAMillimeter);
                    mode.dmPaperLength = unchecked((short)length);
                    setLength = true;
                }
                if ((mode.dmFields & SafeNativeMethods.DM_PAPERWIDTH) == SafeNativeMethods.DM_PAPERWIDTH)
                {
                    int width = PrinterUnitConvert.Convert(paperSize.Width, PrinterUnit.Display, PrinterUnit.TenthsOfAMillimeter);
                    mode.dmPaperWidth = unchecked((short)width);
                    setWidth = true;
                }

                if (paperSize.Kind == PaperKind.Custom)
                {
                    if (!setLength)
                    {
                        mode.dmFields |= SafeNativeMethods.DM_PAPERLENGTH;
                        int length = PrinterUnitConvert.Convert(paperSize.Height, PrinterUnit.Display, PrinterUnit.TenthsOfAMillimeter);
                        mode.dmPaperLength = unchecked((short)length);
                    }
                    if (!setWidth)
                    {
                        mode.dmFields |= SafeNativeMethods.DM_PAPERWIDTH;
                        int width = PrinterUnitConvert.Convert(paperSize.Width, PrinterUnit.Display, PrinterUnit.TenthsOfAMillimeter);
                        mode.dmPaperWidth = unchecked((short)width);
                    }
                }
            }

            if (paperSource != null && ((mode.dmFields & SafeNativeMethods.DM_DEFAULTSOURCE) == SafeNativeMethods.DM_DEFAULTSOURCE))
            {
                mode.dmDefaultSource = unchecked((short)paperSource.RawKind);
            }

            if (printerResolution != null)
            {
                if (printerResolution.Kind == PrinterResolutionKind.Custom)
                {
                    if ((mode.dmFields & SafeNativeMethods.DM_PRINTQUALITY) == SafeNativeMethods.DM_PRINTQUALITY)
                    {
                        mode.dmPrintQuality = unchecked((short)printerResolution.X);
                    }
                    if ((mode.dmFields & SafeNativeMethods.DM_YRESOLUTION) == SafeNativeMethods.DM_YRESOLUTION)
                    {
                        mode.dmYResolution = unchecked((short)printerResolution.Y);
                    }
                }
                else {
                    if ((mode.dmFields & SafeNativeMethods.DM_PRINTQUALITY) == SafeNativeMethods.DM_PRINTQUALITY)
                    {
                        mode.dmPrintQuality = unchecked((short)printerResolution.Kind);
                    }
                }
            }

            Marshal.StructureToPtr(mode, modePointer, false);

           
            if (mode.dmDriverExtra >= ExtraBytes)
            {
                int retCode = SafeNativeMethods.DocumentProperties(NativeMethods.NullHandleRef, NativeMethods.NullHandleRef, printerSettings.PrinterName, modePointer, modePointer, SafeNativeMethods.DM_IN_BUFFER | SafeNativeMethods.DM_OUT_BUFFER);
                if (retCode < 0)
                {
                    SafeNativeMethods.GlobalFree(new HandleRef(null, modePointer));
                }
            }

            SafeNativeMethods.GlobalUnlock(new HandleRef(null, hdevmode));
        }

        */

        private bool ShowPrintDialog(IntPtr hwndOwner, PRINTDLGEX data)
        {


            try
            {
                try
                {
                    data.hDevMode = (IntPtr)typeof(PrinterSettings).GetMethod("GetHdevmodeInternal", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new Type[] { }, new System.Reflection.ParameterModifier[] { }).Invoke(PrinterSettings, new object[] { });
                    PrinterSettings.SetHdevmode(data.hDevMode);
                    (PageSettings ?? PrinterSettings.DefaultPageSettings).CopyToHdevmode(data.hDevMode);
                }
                catch
                {
                    if (PageSettings == null)
                        data.hDevMode = PrinterSettings.GetHdevmode();
                    else
                        data.hDevMode = PrinterSettings.GetHdevmode(PageSettings);
                }

                data.hDevNames = PrinterSettings.GetHdevnames();
            }
            catch (InvalidPrinterException)
            {
                data.hDevMode = IntPtr.Zero;
                data.hDevNames = IntPtr.Zero;
            }

            data.Flags = GetFlags();
            data.nCopies = PrinterSettings.Copies;
            data.hwndOwner = hwndOwner;

            try
            {
                if (AllowSomePages)
                {
                    if (PrinterSettings.FromPage < PrinterSettings.MinimumPage
                       || PrinterSettings.FromPage > PrinterSettings.MaximumPage)
                        throw new ArgumentException("FromPage");
                    if (PrinterSettings.ToPage < PrinterSettings.MinimumPage
                        || PrinterSettings.ToPage > PrinterSettings.MaximumPage)
                        throw new ArgumentException("ToPage");
                    if (PrinterSettings.ToPage < PrinterSettings.FromPage)
                        throw new ArgumentException("FromPage");

                    Marshal.WriteInt32(data.pageRanges, 0, PrinterSettings.FromPage);
                    Marshal.WriteInt32(data.pageRanges, 1, PrinterSettings.ToPage);

                    data.nPageRanges = 1;

                    data.nMinPage = PrinterSettings.MinimumPage;
                    data.nMaxPage = PrinterSettings.MaximumPage;
                }

                data.Flags &= ~(PD_SHOWHELP | PD_NONETWORKBUTTON);

                int hr = PrintDlgEx(data);
                if (hr < 0 || data.dwResultAction == PD_RESULT_CANCEL)
                {
                    return false;
                }

                UpdatePrinterSettings(data.hDevMode, data.hDevNames, (short)data.nCopies, data.Flags, PrinterSettings, PageSettings);

                PrintToFile = ((data.Flags & PD_PRINTTOFILE) != 0);
                PrinterSettings.PrintToFile = PrintToFile;
                if (AllowSomePages)
                {
                    PrinterSettings.FromPage = Marshal.ReadInt32(data.pageRanges, 0);
                    PrinterSettings.ToPage = Marshal.ReadInt32(data.pageRanges, 1);
                }
                if ((data.Flags & PD_USEDEVMODECOPIESANDCOLLATE) == 0)
                {
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        PrinterSettings.Copies = (short)(data.nCopies);
                        PrinterSettings.Collate = ((data.Flags & PD_COLLATE) == PD_COLLATE);
                    }
                }

                return (data.dwResultAction == PD_RESULT_PRINT);
            }
            finally
            {
                if (data.hDevMode != IntPtr.Zero)
                    GlobalFree(new HandleRef(data, data.hDevMode));
                if (data.hDevNames != IntPtr.Zero)
                    GlobalFree(new HandleRef(data, data.hDevNames));
                if (data.pageRanges != IntPtr.Zero)
                    GlobalFree(new HandleRef(data, data.pageRanges));
            }
        }
        private static void UpdatePrinterSettings(IntPtr hDevMode, IntPtr hDevNames, short copies, int flags, PrinterSettings settings, PageSettings pageSettings)
        {
            settings.SetHdevmode(hDevMode);
            settings.SetHdevnames(hDevNames);

            if (pageSettings != null)
                pageSettings.SetHdevmode(hDevMode);

            if (settings.Copies == 1)
                settings.Copies = copies;

            settings.PrintRange = (PrintRange)(flags & printRangeMask);
        }

    }
}
