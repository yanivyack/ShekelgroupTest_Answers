using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ENV.Utilities
{
    public class ImageLoader
    {
        public static Image Load(string filePath, Size preferredSize)
        {
            if (filePath.StartsWith("@"))
            {
                filePath = filePath.Substring(1).TrimEnd();
                var i = filePath.LastIndexOf('.');
                if (i == -1) return null;
                var img = LoadNativeResource(filePath.Substring(0, i) + ".dll", filePath.Substring(i + 1));
                if (img is Icon) return ((Icon)img).ToBitmap();
                else return img as Image;
            }

            if (!System.IO.File.Exists(filePath)) return null;
            filePath = filePath.TrimEnd();
            var extention = Path.GetExtension(filePath).ToUpper(CultureInfo.InvariantCulture).TrimEnd();
            if (extention == ".PCX")
                return LoadPcx(filePath);
            if (extention == ".ICO")
            {
                if (preferredSize.IsEmpty)
                {
                    try
                    {
                        var bd = IconBitmapDecoder.Create(new MemoryStream(File.ReadAllBytes(filePath)), System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat, System.Windows.Media.Imaging.BitmapCacheOption.None);
                        if (bd.Frames.Count > 0)
                            preferredSize = new Size(bd.Frames[bd.Frames.Count - 1].PixelWidth, bd.Frames[bd.Frames.Count - 1].PixelHeight);
                    }
                    catch (Exception) { }
                }

                return new Icon(filePath, preferredSize).ToBitmap();
            }
            if (extention == ".ANI")
                return Icon.FromHandle(LoadImage(IntPtr.Zero, filePath, 1, preferredSize.Width, preferredSize.Height, 0x00000010)).ToBitmap();

            var ms = new MemoryStream(File.ReadAllBytes(filePath));
            if (extention == ".JPG" && preferredSize != Size.Empty)
            {
                var targetWidthOrHeight = Math.Max(2048, Math.Max(preferredSize.Width, preferredSize.Height));
                using (var image = System.Drawing.Image.FromStream(ms, useEmbeddedColorManagement: true, validateImageData: false))
                {
                    var imageSize = image.Size;
                    ms.Seek(0, SeekOrigin.Begin);
                    if (imageSize.Width * imageSize.Height > targetWidthOrHeight * targetWidthOrHeight)
                    {
                        var shrinkRatio = Math.Min((float)imageSize.Width / targetWidthOrHeight, (float)imageSize.Height / targetWidthOrHeight);
                        try
                        {
                            var bd = BitmapDecoder.Create(ms, System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat, System.Windows.Media.Imaging.BitmapCacheOption.None);
                            var tb = new TransformedBitmap(bd.Frames[0], new ScaleTransform(1 / shrinkRatio, 1 / shrinkRatio, 0, 0));
                            var bmpFrame = BitmapFrame.Create(tb);
                            var encoder = new BmpBitmapEncoder();
                            encoder.Frames.Add(bmpFrame);
                            var ms1 = new MemoryStream();
                            encoder.Save(ms1);
                            ms = ms1;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

            return Image.FromStream(ms);
        }

        public static Image Load(string filePath)
        {
            return Load(filePath, Size.Empty);
        }
        static Icon lastIcon = null;
        static string lastIconPath = null;

        public static Icon LoadIcon(string path)
        {
            if (path == lastIconPath)
                return lastIcon;
            if (path.StartsWith("@"))
            {
                path = path.Substring(1).TrimEnd();
                var i = path.LastIndexOf('.');
                if (i == -1) return null;
                var img = LoadNativeResource(path.Substring(0, i) + ".dll", path.Substring(i + 1)) as Icon;
                if (img != null) return img;
            }
            Icon icon;
            if (Path.GetExtension(path) == ".png")
            {
                icon = Icon.FromHandle(((Bitmap)Load(path)).GetHicon());
            }
            else icon = new System.Drawing.Icon(path);
            lastIconPath = path;
            lastIcon = icon;
            return icon;
        }

        static object LoadNativeResource(string dllPath, string resourceName)
        {
            var lib = LoadLibrary(dllPath);
            if (lib == IntPtr.Zero)
            {
                lib = LoadLibraryEx(dllPath, IntPtr.Zero, 2 /* LOAD_LIBRARY_AS_DATAFILE */);
                if (lib == IntPtr.Zero)
                {
                    switch (Marshal.GetLastWin32Error())
                    {
                        case 2:
                            throw new FileNotFoundException("Specified file '" + dllPath + "' not found.");

                        case 193:
                            throw new ArgumentException("Specified file '" + dllPath + "' is not an executable file or DLL.");

                        default:
                            throw new Win32Exception();
                    }
                }
            }
            try
            {
                var hBitmap = LoadBitmap(lib, resourceName);
                if (hBitmap != IntPtr.Zero)
                {
                    try
                    {
                        return Image.FromHbitmap(hBitmap);
                    }
                    finally
                    {
                        DeleteObject(hBitmap);
                    }
                }
                var hIcon = LoadImage(lib, resourceName, 1, 0, 0, 0);
                if (hIcon != IntPtr.Zero)
                {
                    try
                    {
                        var icn = Icon.FromHandle(hIcon);
                        if (icn != null)
                            return (Icon)icn.Clone();
                    }
                    finally
                    {
                        DestroyIcon(hIcon);
                    }
                }

                int i;
                if (int.TryParse(resourceName, out i) && (hIcon = LoadImage(lib, i, 1, 0, 0, 0)) != IntPtr.Zero)
                {
                    try
                    {
                        var icn = Icon.FromHandle(hIcon);
                        if (icn != null)
                            return (Icon)icn.Clone();
                    }
                    finally
                    {
                        DestroyIcon(hIcon);
                    }
                }
                throw new Win32Exception();
            }
            finally
            {
                FreeLibrary(lib);
            }

        }

        [StructLayout(LayoutKind.Sequential)]
        struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct BITMAPINFO
        {
            public int bmiHeader_biSize;
            public int bmiHeader_biWidth;
            public int bmiHeader_biHeight;
            public short bmiHeader_biPlanes;
            public short bmiHeader_biBitCount;
            public int bmiHeader_biCompression;
            public int bmiHeader_biSizeImage;
            public int bmiHeader_biXPelsPerMeter;
            public int bmiHeader_biYPelsPerMeter;
            public int bmiHeader_biClrUsed;
            public int bmiHeader_biClrImportant;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public RGBQUAD[] pallete;
        }

        static Image LoadPcx(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);

            var bitPlanes = (short)(bytes[65]);
            var bytesPerImageRow = BitConverter.ToInt16(bytes, 66);
            int iScanLineSize = bitPlanes * bytesPerImageRow;
            iScanLineSize = (int)((Math.Ceiling((double)iScanLineSize / sizeof(long))) * sizeof(long));

            var x1 = BitConverter.ToInt16(bytes, 4);
            var y1 = BitConverter.ToInt16(bytes, 6);
            var x2 = BitConverter.ToInt16(bytes, 8);
            var y2 = BitConverter.ToInt16(bytes, 10);

            var BitmapSize = new Size(x2 - x1 + 1, y2 - y1 + 1);
            long clImageSize = iScanLineSize * BitmapSize.Height;

            var bitPerPixel = (short)bytes[3];

            var psBmpInfo = new BITMAPINFO
            {
                bmiHeader_biSize = 40,
                bmiHeader_biWidth = BitmapSize.Width,
                bmiHeader_biHeight = -BitmapSize.Height,
                bmiHeader_biPlanes = bitPlanes,
                bmiHeader_biBitCount = bitPerPixel,
                bmiHeader_biCompression = 0,
                bmiHeader_biSizeImage = 0,
                bmiHeader_biXPelsPerMeter = 0,
                bmiHeader_biYPelsPerMeter = 0,
                bmiHeader_biClrUsed = 0,
                bmiHeader_biClrImportant = 0
            };

            var pabRawBitmap = new byte[clImageSize];


            long lDataPos = 0;
            long lPos = 128;

            for (int iY = 0; iY < BitmapSize.Height; iY++)
            {
                var iX = 0;
                while (iX < bytesPerImageRow)
                {
                    var uiValue = bytes[lPos++];
                    if (uiValue > 192)
                    {
                        uiValue -= 192;
                        byte Color = bytes[lPos++];

                        if (iX <= BitmapSize.Width)
                        {
                            for (short bRepeat = 0; bRepeat < uiValue; bRepeat++)
                            {
                                pabRawBitmap[lDataPos++] = Color;
                                iX++;
                            }
                        }
                        else
                            iX += uiValue;
                    }
                    else
                    {
                        if (iX <= BitmapSize.Width)
                            pabRawBitmap[lDataPos++] = uiValue;
                        iX++;
                    }
                }

                if (iX < iScanLineSize)
                {
                    for (; iX < iScanLineSize; iX++)
                        pabRawBitmap[lDataPos++] = 0;
                }
            }

            if (bytes[lPos++] == 12)
            {
                psBmpInfo.pallete = new RGBQUAD[256];
                for (short Entry = 0; Entry < 256; Entry++)
                {
                    psBmpInfo.pallete[Entry].rgbRed = bytes[lPos++];
                    psBmpInfo.pallete[Entry].rgbGreen = bytes[lPos++];
                    psBmpInfo.pallete[Entry].rgbBlue = bytes[lPos++];
                    psBmpInfo.pallete[Entry].rgbReserved = 0;
                }
            }
            var result = new Bitmap(BitmapSize.Width, BitmapSize.Height);
            using (var g = Graphics.FromImage(result))
            {
                var hdc = g.GetHdc();
                try
                {
                    SetDIBitsToDevice(hdc, 0, 0, BitmapSize.Width, BitmapSize.Height, 0, 0, 0, BitmapSize.Height,
                                      pabRawBitmap, ref psBmpInfo, 0);
                }
                finally
                {
                    g.ReleaseHdc(hdc);
                }
            }
            return result;
        }

        [DllImport("gdi32.dll")]
        static extern int SetDIBitsToDevice(IntPtr hdc, int XDest, int YDest, int dwWidth, int dwHeight, int XSrc, int YSrc, int uStartScan, int cScanLines,
           byte[] lpvBits, [In] ref BITMAPINFO lpbmi, uint fuColorUse);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, int dwFlags);

        [DllImport("user32.dll")]
        static extern IntPtr LoadBitmap(IntPtr hInstance, string lpBitmapName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType,
           int cxDesired, int cyDesired, uint fuLoad);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr LoadImage(IntPtr hinst, int lpszName, uint uType,
           int cxDesired, int cyDesired, uint fuLoad);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);
    }
}

