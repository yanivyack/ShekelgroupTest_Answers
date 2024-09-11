using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Firefly.Box;

namespace ENV.Utilities
{
    class DDE
    {
        const int MAX_STRING_SIZE = 255;

        const int APPCMD_CLIENTONLY = unchecked((int)0x00000010);
        const int APPCMD_FILTERINITS = unchecked((int)0x00000020);
        const int APPCMD_MASK = unchecked((int)0x00000FF0);
        const int APPCLASS_STANDARD = unchecked((int)0x00000000);
        const int APPCLASS_MONITOR = unchecked((int)0x00000001);
        const int APPCLASS_MASK = unchecked((int)0x0000000F);

        const int CBR_BLOCK = unchecked((int)0xFFFFFFFF);

        const int CBF_FAIL_SELFCONNECTIONS = unchecked((int)0x00001000);
        const int CBF_FAIL_CONNECTIONS = unchecked((int)0x00002000);
        const int CBF_FAIL_ADVISES = unchecked((int)0x00004000);
        const int CBF_FAIL_EXECUTES = unchecked((int)0x00008000);
        const int CBF_FAIL_POKES = unchecked((int)0x00010000);
        const int CBF_FAIL_REQUESTS = unchecked((int)0x00020000);
        const int CBF_FAIL_ALLSVRXACTIONS = unchecked((int)0x0003f000);
        const int CBF_SKIP_CONNECT_CONFIRMS = unchecked((int)0x00040000);
        const int CBF_SKIP_REGISTRATIONS = unchecked((int)0x00080000);
        const int CBF_SKIP_UNREGISTRATIONS = unchecked((int)0x00100000);
        const int CBF_SKIP_DISCONNECTS = unchecked((int)0x00200000);
        const int CBF_SKIP_ALLNOTIFICATIONS = unchecked((int)0x003c0000);

        const int CF_TEXT = 1;

        const int CP_WINANSI = 1004;
        const int CP_WINUNICODE = 1200;

        const int DDE_FACK = unchecked((int)0x8000);
        const int DDE_FBUSY = unchecked((int)0x4000);
        const int DDE_FDEFERUPD = unchecked((int)0x4000);
        const int DDE_FACKREQ = unchecked((int)0x8000);
        const int DDE_FRELEASE = unchecked((int)0x2000);
        const int DDE_FREQUESTED = unchecked((int)0x1000);
        const int DDE_FAPPSTATUS = unchecked((int)0x00ff);
        const int DDE_FNOTPROCESSED = unchecked((int)0x0000);

        const int DMLERR_NO_ERROR = unchecked((int)0x0000);
        const int DMLERR_FIRST = unchecked((int)0x4000);
        const int DMLERR_ADVACKTIMEOUT = unchecked((int)0x4000);
        const int DMLERR_BUSY = unchecked((int)0x4001);
        const int DMLERR_DATAACKTIMEOUT = unchecked((int)0x4002);
        const int DMLERR_DLL_NOT_INITIALIZED = unchecked((int)0x4003);
        const int DMLERR_DLL_USAGE = unchecked((int)0x4004);
        const int DMLERR_EXECACKTIMEOUT = unchecked((int)0x4005);
        const int DMLERR_INVALIDPARAMETER = unchecked((int)0x4006);
        const int DMLERR_LOW_MEMORY = unchecked((int)0x4007);
        const int DMLERR_MEMORY_ERROR = unchecked((int)0x4008);
        const int DMLERR_NOTPROCESSED = unchecked((int)0x4009);
        const int DMLERR_NO_CONV_ESTABLISHED = unchecked((int)0x400A);
        const int DMLERR_POKEACKTIMEOUT = unchecked((int)0x400B);
        const int DMLERR_POSTMSG_FAILED = unchecked((int)0x400C);
        const int DMLERR_REENTRANCY = unchecked((int)0x400D);
        const int DMLERR_SERVER_DIED = unchecked((int)0x400E);
        const int DMLERR_SYS_ERROR = unchecked((int)0x400F);
        const int DMLERR_UNADVACKTIMEOUT = unchecked((int)0x4010);
        const int DMLERR_UNFOUND_QUEUE_ID = unchecked((int)0x4011);
        const int DMLERR_LAST = unchecked((int)0x4011);

        const int DNS_REGISTER = unchecked((int)0x0001);
        const int DNS_UNREGISTER = unchecked((int)0x0002);
        const int DNS_FILTERON = unchecked((int)0x0004);
        const int DNS_FILTEROFF = unchecked((int)0x0008);

        const int EC_ENABLEALL = unchecked((int)0x0000);
        const int EC_ENABLEONE = unchecked((int)0x0080);
        const int EC_DISABLE = unchecked((int)0x0008);
        const int EC_QUERYWAITING = unchecked((int)0x0002);

        const int HDATA_APPOWNED = unchecked((int)0x0001);

        const int MF_HSZ_INFO = unchecked((int)0x01000000);
        const int MF_SENDMSGS = unchecked((int)0x02000000);
        const int MF_POSTMSGS = unchecked((int)0x04000000);
        const int MF_CALLBACKS = unchecked((int)0x08000000);
        const int MF_ERRORS = unchecked((int)0x10000000);
        const int MF_LINKS = unchecked((int)0x20000000);
        const int MF_CONV = unchecked((int)0x40000000);

        const int MH_CREATE = 1;
        const int MH_KEEP = 2;
        const int MH_DELETE = 3;
        const int MH_CLEANUP = 4;

        const int QID_SYNC = unchecked((int)0xFFFFFFFF);
        const int TIMEOUT_ASYNC = unchecked((int)0xFFFFFFFF);

        const int XTYPF_NOBLOCK = unchecked((int)0x0002);
        const int XTYPF_NODATA = unchecked((int)0x0004);
        const int XTYPF_ACKREQ = unchecked((int)0x0008);
        const int XCLASS_MASK = unchecked((int)0xFC00);
        const int XCLASS_BOOL = unchecked((int)0x1000);
        const int XCLASS_DATA = unchecked((int)0x2000);
        const int XCLASS_FLAGS = unchecked((int)0x4000);
        const int XCLASS_NOTIFICATION = unchecked((int)0x8000);
        const int XTYP_ERROR = unchecked((int)(0x0000 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK));
        const int XTYP_ADVDATA = unchecked((int)(0x0010 | XCLASS_FLAGS));
        const int XTYP_ADVREQ = unchecked((int)(0x0020 | XCLASS_DATA | XTYPF_NOBLOCK));
        const int XTYP_ADVSTART = unchecked((int)(0x0030 | XCLASS_BOOL));
        const int XTYP_ADVSTOP = unchecked((int)(0x0040 | XCLASS_NOTIFICATION));
        const int XTYP_EXECUTE = unchecked((int)(0x0050 | XCLASS_FLAGS));
        const int XTYP_CONNECT = unchecked((int)(0x0060 | XCLASS_BOOL | XTYPF_NOBLOCK));
        const int XTYP_CONNECT_CONFIRM = unchecked((int)(0x0070 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK));
        const int XTYP_XACT_COMPLETE = unchecked((int)(0x0080 | XCLASS_NOTIFICATION));
        const int XTYP_POKE = unchecked((int)(0x0090 | XCLASS_FLAGS));
        const int XTYP_REGISTER = unchecked((int)(0x00A0 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK));
        const int XTYP_REQUEST = unchecked((int)(0x00B0 | XCLASS_DATA));
        const int XTYP_DISCONNECT = unchecked((int)(0x00C0 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK));
        const int XTYP_UNREGISTER = unchecked((int)(0x00D0 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK));
        const int XTYP_WILDCONNECT = unchecked((int)(0x00E0 | XCLASS_DATA | XTYPF_NOBLOCK));
        const int XTYP_MONITOR = unchecked((int)(0x00F0 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK));
        const int XTYP_MASK = unchecked((int)0x00F0);
        const int XTYP_SHIFT = unchecked((int)0x0004);

        delegate IntPtr DdeCallback(
            int uType, int uFmt, IntPtr hConv, IntPtr hsz1, IntPtr hsz2, IntPtr hData, IntPtr dwData1, IntPtr dwData2);

        [DllImport("user32.dll", EntryPoint = "DdeClientTransaction", CharSet = CharSet.Ansi)]
        static extern IntPtr DdeClientTransaction(
            IntPtr pData, int cbData, IntPtr hConv, IntPtr hszItem, int wFmt, int wType, int dwTimeout, ref int pdwResult);

        [DllImport("user32.dll", EntryPoint = "DdeClientTransaction", CharSet = CharSet.Ansi)]
        static extern IntPtr DdeClientTransaction(
            byte[] pData, int cbData, IntPtr hConv, IntPtr hszItem, int wFmt, int wType, int dwTimeout, ref int pdwResult);

        [DllImport("user32.dll", EntryPoint = "DdeConnect", CharSet = CharSet.Ansi)]
        static extern IntPtr DdeConnect(int idInst, IntPtr hszService, IntPtr hszTopic, IntPtr pCC);

        [DllImport("user32.dll", EntryPoint = "DdeDisconnect", CharSet = CharSet.Ansi)]
        static extern bool DdeDisconnect(IntPtr hConv);

        [DllImport("user32.dll", EntryPoint = "DdeInitialize", CharSet = CharSet.Ansi)]
        static extern int DdeInitialize(ref int pidInst, DdeCallback pfnCallback, int afCmd, int ulRes);

        [DllImport("user32.dll", EntryPoint = "DdeCreateStringHandle", CharSet = CharSet.Ansi)]
        static extern IntPtr DdeCreateStringHandle(int idInst, string psz, int iCodePage);

        [DllImport("user32.dll", EntryPoint = "DdeFreeStringHandle", CharSet = CharSet.Ansi)]
        static extern bool DdeFreeStringHandle(int idInst, IntPtr hsz);

        [DllImport("user32.dll", EntryPoint = "DdeCreateDataHandle", CharSet = CharSet.Ansi)]
        static extern IntPtr DdeCreateDataHandle(int idInst, byte[] pSrc, int cb, int cbOff, IntPtr hszItem, int wFmt, int afCmd);

        [DllImport("user32.dll", EntryPoint = "DdeFreeDataHandle", CharSet = CharSet.Ansi)]
        static extern bool DdeFreeDataHandle(IntPtr hData);

        [DllImport("user32.dll", EntryPoint = "DdeGetData", CharSet = CharSet.Ansi)]
        static extern int DdeGetData(IntPtr hData, [Out] byte[] pDst, int cbMax, int cbOff);

        [DllImport("user32.dll", EntryPoint = "DdeGetLastError", CharSet = CharSet.Ansi)]
        static extern int DdeGetLastError(int idInst);

        [DllImport("user32.dll")]
        static extern uint DdeQueryConvInfo(IntPtr hConv, int idTransaction, ref CONVINFO pConvInfo);

        [StructLayout(LayoutKind.Sequential)]
        public struct CONVINFO
        {
            public int cb;
            public IntPtr hUser;
            public IntPtr hConvPartner;
            public IntPtr hszSvcPartner;
            public IntPtr hszServiceReq;
            public IntPtr hszTopic;
            public IntPtr hszItem;
            public int wFmt;
            public int wType;
            public int wStatus;
            public int wConvst;
            public int wLastError;
            public IntPtr hConvList;
            public CONVCONTEXT ConvCtxt;
            public IntPtr hwnd;
            public IntPtr hwndPartner;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CONVCONTEXT
        {
            public int cb;
            public int wFlags;
            public int wCountryID;
            public int iCodePage;
            public int dwLangID;
            public int dwSecurity;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] filler;

        }

        int _instanceId = 0;

        DDE()
        {
            DdeInitialize(ref _instanceId, delegate { return new IntPtr(1); }, APPCLASS_STANDARD | APPCMD_CLIENTONLY, 0);
        }

        static ContextStatic<DDE> _instance = new ContextStatic<DDE>(() => new DDE());

        public static DDE Instance { get { return _instance.Value; } }

        Dictionary<string, IntPtr> _conversations = new Dictionary<string, IntPtr>();

        public bool Begin(string serviceName, string topic)
        {
            if (_conversations.ContainsKey(GetConversationName(serviceName, topic)))
                End(serviceName, topic);

            IntPtr serviceHandle = DdeCreateStringHandle(_instanceId, serviceName, CP_WINANSI);
            IntPtr topicHandle = DdeCreateStringHandle(_instanceId, topic, CP_WINANSI);

            IntPtr handle = DdeConnect(_instanceId, serviceHandle, topicHandle, IntPtr.Zero);

            DdeFreeStringHandle(_instanceId, topicHandle);
            DdeFreeStringHandle(_instanceId, serviceHandle);

            if (handle != IntPtr.Zero)
            {
                _conversations.Add(GetConversationName(serviceName, topic), handle);
                return true;
            }
            return false;
        }

        static string GetConversationName(string serviceName, string topicName)
        {
            return string.Format("{0} {1}", serviceName.Trim().ToLower(), topicName.Trim().ToLower());
        }

        void DoOnConversation(string serviceName, string topicName, Action<IntPtr> doThis, bool iComeFromEnd)
        {
            var conversationHandle = new IntPtr(0);
            if (_conversations.TryGetValue(GetConversationName(serviceName, topicName), out conversationHandle))
            {
                if (iComeFromEnd)
                {
                    doThis(conversationHandle);
                    return;
                }
                var x = new CONVINFO();
                if (DdeQueryConvInfo(conversationHandle, QID_SYNC, ref x) != 0 && x.wStatus != 0x20)
                {
                    doThis(conversationHandle);
                    return;
                }
                End(serviceName, topicName);
            }
            if (Begin(serviceName, topicName))
            {
                try
                {
                    if (_conversations.TryGetValue(GetConversationName(serviceName, topicName), out conversationHandle))
                        doThis(conversationHandle);
                }
                finally
                {
                    if (!iComeFromEnd)
                        End(serviceName, topicName);
                }
            }
        }
        public bool End(string serviceName, string topic)
        {
            var result = false;
            DoOnConversation(serviceName, topic,
                delegate(IntPtr ptr)
                {
                    result = DdeDisconnect(ptr);
                    _conversations.Remove(GetConversationName(serviceName, topic));
                }, true);
            return result;
        }

        public bool Poke(string serviceName, string topic, string itemName, string data)
        {
            var result = false;
            DoOnConversation(serviceName, topic,
                delegate(IntPtr ptr)
                {
                    IntPtr itemHandle = DdeCreateStringHandle(_instanceId, itemName, 0);

                    try
                    {
                        byte[] dataBytes = ENV.LocalizationInfo.Current.OuterEncoding.GetBytes(data + "\0");
                        int returnFlags = 0;
                        result = DdeClientTransaction(dataBytes, dataBytes.Length, ptr, itemHandle, 1, XTYP_POKE, 10000, ref returnFlags) != IntPtr.Zero;
                    }
                    finally
                    {
                        DdeFreeStringHandle(_instanceId, itemHandle);
                    }
                }, false);
            return result;
        }

        public string Get(string serviceName, string topic, string itemName, int length)
        {
            var result = "";
            DoOnConversation(serviceName, topic,
                delegate(IntPtr ptr)
                {
                    IntPtr itemHandle = DdeCreateStringHandle(_instanceId, itemName, CP_WINANSI);

                    int returnFlags = 0;
                    IntPtr dataHandle = DdeClientTransaction(
                        IntPtr.Zero, 0, ptr, itemHandle, 1, XTYP_REQUEST, 1000, ref returnFlags);

                    DdeFreeStringHandle(_instanceId, itemHandle);

                    if (dataHandle == IntPtr.Zero)
                        return;

                    int dataLength = DdeGetData(dataHandle, null, 0, 0);
                    var data = new byte[dataLength];
                    DdeGetData(dataHandle, data, data.Length, 0);

                    DdeFreeDataHandle(dataHandle);

                    string dataString = ENV.LocalizationInfo.Current.OuterEncoding.GetString(data);
                    result = dataString.Length > length ? dataString.Substring(0, length) : dataString.PadRight(length, ' ');
                }, false);
            return result;
        }

        public bool Execute(string serviceName, string topic, string itemName, string command)
        {
            var result = false;
            DoOnConversation(serviceName, topic,
                delegate(IntPtr ptr)
                {
                    byte[] data = ENV.LocalizationInfo.Current.OuterEncoding.GetBytes(command + "\0");

                    int returnFlags = 0;
                    result = DdeClientTransaction(data, data.Length, ptr, IntPtr.Zero, CF_TEXT, XTYP_EXECUTE, 5000, ref returnFlags) != IntPtr.Zero;
                }, false);
            return result;
        }

        public int GetLastError()
        {
            switch (DdeGetLastError(_instanceId))
            {
                case DMLERR_NO_ERROR: return 0;
                case DMLERR_DLL_NOT_INITIALIZED: return 1;
                case DMLERR_NO_CONV_ESTABLISHED: return 2;
                case DMLERR_BUSY: return 3;
                case DMLERR_NOTPROCESSED: return 4;
                case DMLERR_INVALIDPARAMETER: return 5;
                case DMLERR_SYS_ERROR: return 15;
            }
            return 14;
        }
    }
}
