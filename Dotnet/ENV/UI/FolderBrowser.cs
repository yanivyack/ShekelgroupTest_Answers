using System;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ENV.UI
{
    class FolderBrowser : Component
    {
        string _directoryPath;
        string _title;
        string m_strDisplayName;
        private BrowseFlags m_Flags;
        public FolderBrowser()
        {
            m_Flags = BrowseFlags.BIF_DEFAULT;
            _title = "";
        }
        public string Caption { get; set; }

        public string DirectoryPath
        {
            get { return this._directoryPath; }
            set { _directoryPath = value.TrimEnd(' '); }
        }

        public string DisplayName
        {
            get { return this.m_strDisplayName; }
        }

        public string Title
        {
            set { this._title = value; }
        }

        public BrowseFlags Flags
        {
            set { this.m_Flags = value; }
        }
         

 

        private int CallBack(IntPtr hwnd, int msg, IntPtr lParam, IntPtr lpData)
        {
            switch (msg)
            {
                case 1:
                    if (_directoryPath.Length != 0)
                    {
                        Win32SDK.SendMessage(new HandleRef(null, hwnd), 0x467/*BFFM_SETSELECTION*/ , 1, this._directoryPath);
                        
                    }
                    if (!string.IsNullOrEmpty( Caption))
                    Win32SDK.SetWindowText(hwnd, Caption);

                    break;

                case 2:
                    {
                       
                        break;
                    }
            }
            return 0;
        }

 

 


        public DialogResult ShowDialog()
        {
            BROWSEINFO bi = new BROWSEINFO();
            bi.pszDisplayName = IntPtr.Zero;
            bi.lpfn = new FolderBrowseCallBack(CallBack);
            bi.lParam = IntPtr.Zero;
            bi.lpszTitle = "Select Folder";
            IntPtr idListPtr = IntPtr.Zero;
            IntPtr pszPath = IntPtr.Zero;
            try
            {
                if (this._title.Length != 0)
                {
                    bi.lpszTitle = this._title;
                }
                bi.ulFlags = (int)this.m_Flags;
                bi.pszDisplayName = Marshal.AllocHGlobal(256);
                // Call SHBrowseForFolder
                idListPtr = Win32SDK.SHBrowseForFolder(bi);
                // Check if the user cancelled out of the dialog or not.
                if (idListPtr == IntPtr.Zero)
                {
                    return DialogResult.Cancel;
                }

                // Allocate ncessary memory buffer to receive the folder path.
                pszPath = Marshal.AllocHGlobal(256);
                // Call SHGetPathFromIDList to get folder path.
                bool bRet = Win32SDK.SHGetPathFromIDList(idListPtr, pszPath);
                // Convert the returned native poiner to string.
                _directoryPath = Marshal.PtrToStringAuto(pszPath);
                this.m_strDisplayName = Marshal.PtrToStringAuto(bi.pszDisplayName);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return DialogResult.Abort;
            }
            finally
            {
                // Free the memory allocated by shell.
                if (idListPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(idListPtr);
                }
                if (pszPath != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pszPath);
                }
                if (bi != null)
                {
                    Marshal.FreeHGlobal(bi.pszDisplayName);
                }
            }
            return DialogResult.OK;
        }

        private IntPtr GetStartLocationPath()
        {
            return IntPtr.Zero;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        [ComVisible(true)]
        class BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public IntPtr pszDisplayName;
            public string lpszTitle;
            public int ulFlags;
            public FolderBrowseCallBack lpfn;
            public IntPtr lParam;
            public int iImage;
        }
        delegate int FolderBrowseCallBack(IntPtr hwnd, int msg, IntPtr lParam, IntPtr lpData);

        class Win32SDK
        {
            [DllImport("shell32.dll", PreserveSig = true, CharSet = CharSet.Auto)]
            public static extern IntPtr SHBrowseForFolder(BROWSEINFO bi);

            [DllImport("shell32.dll", PreserveSig = true, CharSet = CharSet.Auto)]
            public static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);

            [DllImport("shell32.dll", PreserveSig = true, CharSet = CharSet.Auto)]
            public static extern int SHGetSpecialFolderLocation(IntPtr hwnd, int csidl, ref IntPtr ppidl);
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, string lParam);
            [DllImport("user32.dll")]
            public static extern bool SetWindowText(IntPtr hWnd, string lpString);


        }

        
    }
    [Flags, Serializable]
    public enum BrowseFlags
    {
        BIF_DEFAULT = 0x0000,
        BIF_BROWSEFORCOMPUTER = 0x1000,
        BIF_BROWSEFORPRINTER = 0x2000,
        BIF_BROWSEINCLUDEFILES = 0x4000,
        BIF_BROWSEINCLUDEURLS = 0x0080,
        BIF_DONTGOBELOWDOMAIN = 0x0002,
        BIF_EDITBOX = 0x0010,
        BIF_NEWDIALOGSTYLE = 0x0040,
        BIF_NONEWFOLDERBUTTON = 0x0200,
        BIF_RETURNFSANCESTORS = 0x0008,
        BIF_RETURNONLYFSDIRS = 0x0001,
        BIF_SHAREABLE = 0x8000,
        BIF_STATUSTEXT = 0x0004,
        BIF_UAHINT = 0x0100,
        BIF_VALIDATE = 0x0020,
        BIF_NOTRANSLATETARGETS = 0x0400,
    }
}