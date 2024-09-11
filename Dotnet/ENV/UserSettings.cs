using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ENV.Data;
using ENV.Data.DataProvider;
using ENV.Remoting;
using ENV.Security;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data;
using Firefly.Box.UI;
using Firefly.Box.UI.Advanced;

namespace ENV
{

    public static class UserSettings
    {
        internal static int _startApplication = 1;
        static bool _disableApplicationLogin = false;
        static void HandleSpecialIniValues(string sectionName, string entryName, string value, bool saveToShared,
            bool saveToContext)
        {

            string upperSection = sectionName.ToUpper(CultureInfo.InvariantCulture),
                upperEntryName = entryName.ToUpper(CultureInfo.InvariantCulture),
                upperValue = value.ToUpper(CultureInfo.InvariantCulture);
            string upperFirstChar = "";
            if (upperValue.Length > 0)
                upperFirstChar = upperValue.Substring(0, 1);
            bool used = false;
            bool isNo = upperFirstChar == "N",
                isYes = upperFirstChar == "Y";

            switch (upperSection)
            {
                case MAGIC_ENV:
                    used = true;
                    switch (upperEntryName)
                    {
                        case "APPLICATIONSTARTUP":
                        case "DEPLOYMENTMODE":
                            DoNotDisplayUI = upperValue == "B";
                            break;
                        case "ALLOWCREATEINMODIFY":
                            DefaultAllowInsertInUpdateActivity = !isNo;
                            break;
                        case "COPYRIGHTMESSAGES":
                            Common.ShowSpecificMDIText = isYes;
                            break;
                        case "RTUSERCOPYRIGHT":
                            Common.SpecificMDIText = value;
                            break;
                        case "COMMANDPROCESSOR":
                            Windows.DefaultOSCommand = value;
                            break;
                        case "TERMINAL":
                            UserMethods.SetTerminal(Number.Parse(value));
                            break;
                        case "REPOSITIONAFTERMODIFY":
                            ReloadDataAfterUpdatingOrderByColumns = isYes;
                            break;
                        case "ALLOWUPDATEINQUERY":
                            SuppressUpdatesInBrowseActivity = isNo;
                            break;
                        case "LOADRESIDENTFILES":
                            ENV.Data.DataProvider.EntityNameAcordingToCallStackEntityDataProviderDecorator.DisableInMemoryEntityLoading = isNo;
                            break;

                        case "CENTERSCREENINONLINE":
                            refreshGridStartOnRow();
                            break;
                        case "STARTAPPLICATION":
                            try
                            {
                                if (value.Trim().Length > 0)
                                    if ("1234567890".IndexOf(value.Trim()[0]) > -1)
                                    {
                                        _startApplication = Math.Max((int)Number.Parse(value), 1);
                                        SetStartApplicationBySystem(Get("MAGIC_SYSTEMS", "SYSTEM" + _startApplication));
                                    }
                            }
                            catch { }
                            break;
                        case "RESIDENTINI":
                            _saveOnEveryChange = isNo;
                            break;
                        case "ISAMTRANSACTION":
                            ConditionalTransaction = isNo;
                            ConnectionManager.EnableBtrieveTransactions = isYes;
                            break;
                        case "DEFAULTXMLENCODING":
                            if (value.ToUpper() != ENV.IO.XmlHelper.DefaultEncodingName.ToUpper())
                            {
                                ENV.IO.XmlHelper.DefaultEncodingName = value;
                                try
                                {
                                    ENV.IO.XmlHelper.DefaultEncoding = System.Text.Encoding.GetEncoding(value);
                                }
                                catch (Exception ex)
                                {
                                    ErrorLog.WriteToLogFile(ex);
                                }
                            }
                            break;
                        case "STARTINGLANGUAGE":
                            if (saveToContext)
                                Languages.ContextCurrentLanguage = value;
                            if (saveToShared)
                                Languages.SetSharedLanguage(value);
                            break;
                        case "DATEMODE":
                            string theFormat = "DD/MM/YYYY";
                            if (upperFirstChar == "A")
                                theFormat = "MM/DD/YYYY";
                            else if (upperFirstChar == "S" || upperFirstChar == "J")
                                theFormat = "YYYY/MM/DD";
                            if (saveToContext)
                                Date.ContextDefaultFormat = theFormat;
                            if (saveToShared)
                                Date.SharedDefaultFormat = theFormat;
                            break;
                        case "DATE":
                            if (Date.Parse(value, "##-##-##") != Date.Empty)
                                try
                                {
                                    ENV.UserMethods.UserDeterminedDate = Date.Parse(value, "##-##-##", true);
                                }
                                catch (Exception)
                                {
                                }
                            else
                                ENV.UserMethods.UserDeterminedDate = Date.Now;
                            break;
                        case "EXTERNALCODEPAGE":
                            try
                            {
                                LocalizationInfo.DefaultEncoding = System.Text.Encoding.GetEncoding(Number.Parse(value));

                            }
                            catch (Exception ex)
                            {
                                ErrorLog.WriteToLogFile(ex);
                            }
                            break;
                        case "CONSTFILE":
                            if (upperValue.Length > 3 && !SuppressConst)
                                switch (upperValue.Substring(upperValue.Length - 3))
                                {
                                    case "FRE":
                                        Date.DateTimeFormatInfo = new CultureInfo("fr-FR").DateTimeFormat;
                                        LocalizationInfo.Current = new FrenchLocalizationInfo();
                                        break;
                                    case "GER":
                                        Date.DateTimeFormatInfo = new CultureInfo("de-DE").DateTimeFormat;
                                        LocalizationInfo.Current = new GermanLocalizationInfo();
                                        break;
                                    case "HEB":
                                        LocalizationInfo.Current = new HebrewLocalizationInfo();
                                        if (Version10Compatible)
                                            Date.DateTimeFormatInfo = new CultureInfo("he-IL").DateTimeFormat;
                                        else
                                            Date.DateTimeFormatInfo = DateTimeFormatInfo.InvariantInfo;
                                        if (HebrewTextTools.V8HebrewOem)
                                            LocalizationInfo.Current.InnerEncoding = new HebrewTextTools.V8OemEncoding();
                                        break;
                                    case "JPN":
                                        LocalizationInfo.Current = new JapaneseLocalizationInfo();
                                        Date.DateTimeFormatInfo = DateTimeFormatInfo.InvariantInfo;
                                        break;
                                    case "ENG":
                                        LocalizationInfo.Current = new LocalizationInfo();
                                        Date.DateTimeFormatInfo = DateTimeFormatInfo.InvariantInfo;
                                        break;
                                    case "SWD":
                                        Date.DateTimeFormatInfo = new CultureInfo("sv-SE").DateTimeFormat;
                                        LocalizationInfo.Current = new SwedishLocalizationInfo();
                                        break;
                                    case "POL":
                                        Date.DateTimeFormatInfo = new CultureInfo("pl-PL").DateTimeFormat;
                                        LocalizationInfo.Current = new PolishLocalizationInfo();
                                        break;
                                    case "NLD":
                                        Date.DateTimeFormatInfo = new CultureInfo("nl-NL").DateTimeFormat;
                                        LocalizationInfo.Current = new DutchLocalizationInfo();
                                        break;
                                    case "NOR":
                                        Date.DateTimeFormatInfo = new CultureInfo("nb-NO").DateTimeFormat;
                                        LocalizationInfo.Current = new NorwegianLocalizationInfo();
                                        break;

                                    default:
                                        Date.DateTimeFormatInfo = DateTimeFormatInfo.InvariantInfo;
                                        break;
                                }
                            break;
                        case "CENTURY":
                            var x = Number.Parse(value);
                            if (x > 1900)
                                Date.TwoDigitYearMax = x + 99;
                            break;
                        case "DATESEPARATOR":
                            if (value.Length == 0)
                                value = "/";
                            if (saveToContext)
                                Date.DateSeparator.SetContextValue(value[0]);
                            if (saveToShared)
                                Date.DateSeparator.SetSharedValue(value[0]);
                            break;
                        case "THOUSANDSEPARATOR":
                            if (value.Length > 0)
                            {

                                if (saveToContext)
                                    Number.GroupSeparator.SetContextValue(value[0]);
                                if (saveToShared)
                                    Number.GroupSeparator.SetSharedValue(value[0]);
                            }
                            break;
                        case "DECIMALSEPARATOR":

                            if (saveToContext)
                                Number.DecimalSeparator.SetContextValue(value[0]);
                            if (saveToShared)
                                Number.DecimalSeparator.SetSharedValue(value[0]);
                            break;
                        case "USER":
                            Security.UserManager.DefaultUser = value;
                            break;
                        case "PASSWORD":
                            Security.UserManager.DefaultPassword = value;
                            break;
                        case "INPUTPASSWORD":
                            Security.UserManager.DisplayLoginDialog = isYes && !_disableApplicationLogin;
                            break;
                        case "INPUTDATE":
                            Security.Tasks.Login.InputDate = isYes;
                            break;
                        case "SYSTEMLOGIN":
                            if (upperValue == "L")
                            {
                                UserManager.DefaultUser = System.Environment.UserName;
                                UserManager.SetExternalUserDirectory(new UserManager.LdapUserDirectory());
                            }
                            else if (upperValue == "D")
                            {
                                _disableApplicationLogin = true;
                                UserManager.DefaultUser = System.Environment.UserName;
                                UserManager.SetExternalUserDirectory(new UserManager.ADsUserDirectory());
                                UserManager.DisplayLoginDialog = false;
                            }
                            //   else if (upperValue == "U" && string.IsNullOrEmpty(UserManager.DefaultUser))

                            //                                UserManager.DefaultUser = System.Environment.UserName;
                            break;
                        case "LDAPADDRESS":
                            LdapClient.DefaultServerAndPort = value;
                            break;
                        case "LDAPCONNECTIONSTRING":
                            UserManager.LdapUserDirectory.LdapAuthenticationConnectionString = value;
                            break;
                        case "LDAPDOMAINCONTEXT":
                            UserManager.LdapUserDirectory.LdapUserGroupsDomainContext = value;
                            break;
                        case "BATCHPAINTTIME":
                            BusinessProcessBase.DefaultUserInterfaceRefreshInterval = Number.Parse(value);
                            break;
                        case "RANGEPOPTIME":
                            AbstractUIController.FilterRowsScreenInterval = Number.Parse(value) * 1000;
                            break;
                        case "TEMPPOPTIME":
                            AbstractUIController.SortingScreenInterval = Math.Max((int)1, (int)Number.Parse(value)) * 1000;
                            break;

                        case "MAXCONCURRENTREQUESTS":
                            if (saveToShared)
                                HttpApplicationServer.MaxConcurrentRequests = Number.Parse(value);
                            break;
                        case "SECONDARYSERVERS":
                            HttpApplicationServer.SetSecondaryServers(value);
                            break;
                        case "RTTOOLBARGUI":
                            ShowToolStrip = !isNo;
                            break;

                        case "MDICLIENTIMAGEFILE":
                            ApplicationControllerBase.MDIImagePath = value;
                            break;
                        case "MDICLIENTIMAGESTYLE":
                            switch (upperValue)
                            {
                                case "TI":
                                    ApplicationControllerBase.MDIImageLayout = ImageLayout.Tile;
                                    break;
                                case "FL":
                                    ApplicationControllerBase.MDIImageLayout = ImageLayout.ScaleToFill;
                                    break;
                                default:
                                    ApplicationControllerBase.MDIImageLayout = ImageLayout.None;
                                    break;
                            }
                            break;
                        case "DBLOGFILE":
                            ConnectionManager.DatabaseLogFileName = value;
                            break;
                        case "PROFILER":
                            ENV.Utilities.Profiler.ProfilerFile = value;
                            break;
                        case "PROFILERTRACE":
                            ENV.Utilities.Profiler.Tracing = isYes;
                            break;
                        case "PROFILERTARCKCOMMANDS":
                        case "PROFILERTRACKCOMMANDS":
                            Profiler.TrackCommandsAndReaders = isYes;
                            break;
                        case "MEMORYMONITOR":
                            if (isYes)
                                MemoryMonitor.Run();
                            break;
                        case "TRACKMEMORY":
                            switch (upperValue)
                            {
                                case "Y":
                                    MemoryTracker.TrackStrategy = TrackMemoryStrategies.IncludeAllocationStack;
                                    break;
                                case "L":
                                    MemoryTracker.TrackStrategy = TrackMemoryStrategies.InstanceOnly;
                                    break;
                                default:
                                    MemoryTracker.TrackStrategy = TrackMemoryStrategies.None;
                                    break;
                            }

                            break;
                        case "REQUESTLOGFILE":
                            ProgramCollection.LogToFile(value);
                            break;
                        case "ALLWAYSSHOWDBERRORS":
                            Common.AllwaysShowDBErrors = isYes;
                            break;
                        case "TRACEFILE":
                            if (saveToContext)
                                Common.TraceFileName.SetContextValue(value);
                            if (saveToShared)
                                Common.TraceFileName.SetSharedValue(value);
                            break;
                        case "DBDEBUG":
                            ConnectionManager.DatabaseLogToDebugOutput = isYes;
                            break;
                        case "DBNOPARAMS":
                            ConnectionManager.UseDBParameters = !(isYes);
                            break;
                        case "ODBCNOPARAMS":
                            ConnectionManager.OdbcDbParameters = !(isYes);
                            break;
                        case "HELPFILE":
                            ApplicationControllerBase.DefaultHelpFile = value;
                            break;
                        case "DISPLAYFULLMSGS":
                            Common.DisplayDatabaseErrors = isYes;
                            break;
                        case "GENERALERRORLOG":
                            Common.LogFileName = value;
                            break;
                        case "FIREFLYSECURITY":
                            Security.UserManager.UsersFile = value;
                            break;
                        case "RESOURCELOCKFILEPATH":
                            LockFile.Path = value;
                            break;
                        case "PRINTDATAHTMLTEMPLATE":
                            UI.ExportDataDialog.templateFileName = value;
                            break;
                        case "OEM2ANSIFILE":
                            if (!Version8Compatible)
                                HebrewTextTools.DefaultOemForNonRightToLeftColumns = true;
                            break;
                        case "RUNTIMEAPPLICATIONCOLORDEFINITIONFILE":
                            Utilities.ColorFile.FileName = value;
                            break;
                        case "COLORDEFINITIONFILE":
                            if (!Version10Compatible)
                                Utilities.ColorFile.FileName = value;
                            break;
                        case "RUNTIMEAPPLICATIONFONTDEFINITIONFILE":
                            Utilities.FontFile.FileName = value;
                            break;
                        case "FONTDEFINITIONFILE":
                            if (!Version10Compatible)
                                Utilities.FontFile.FileName = value;
                            break;
                        case "RETRYOPERATIONTIME":
                            Common.RetryTimeoutInSeconds = Number.Parse(value);
                            break;
                        case "HTTPTIMEOUT":
                            var httpTimeout = Number.Parse(value);
                            if (httpTimeout > 0)
                                HttpWebRequestTimeoutInSeconds = httpTimeout;
                            break;
                        case "HTTPPROXYADDRESS":
                            if (saveToShared)
                                UserMethods.Proxy.SetSharedValue(value);
                            else
                                UserMethods.Proxy.SetContextValue(value);
                            break;
                        case "MAILOPERATIONTIMEOUT":
                            var timeout = Number.Parse(value);
                            if (timeout > 0)
                                SmtpMailClient.OperationTimeout = timeout * 1000;
                            break;
                        case "DROPUSERFORMATS":
                            UserMethods.SetDropUserFormats(value);
                            break;
                        case "IDLETIME":
                            UserMethods.SetIdleInterval(Number.Parse(value));
                            break;
                        case "LOGOFILE":
                            SplashScreenImageFileName = value;
                            break;
                        case "EMBEDFONTS":
                            ENV.Printing.PrinterWriter.UseEmbeddedFontsInPdf = !isNo;
                            break;

                        default:
                            used = false;
                            break;
                    }
                    break;
                case MAGIC_LOGICAL_NAMES:
                    used = true;
                    if (saveToContext)
                        PathDecoder.ContextPathDecoder.AddTokenName(entryName, value);
                    if (saveToShared)
                        PathDecoder.SharedPathDecoder.AddTokenName(entryName, value);
                    break;
                case "MAGIC_LOGGING":
                    if (upperEntryName == "EXTERNALLOGFILENAME")
                    {
                        used = true;
                        if (saveToContext)
                            Common.TraceFileName.SetContextValue(value);
                        if (saveToShared)
                            Common.TraceFileName.SetSharedValue(value);
                    }
                    break;
                case "MAGIC_DEFAULTS":
                    if (upperEntryName == "DEFAULTDATE")
                    {
                        used = true;
                        if (upperValue.Trim().Length > 1)
                            ENV.Data.DateColumn.GlobalDefault = Date.Parse(upperValue, Date.ContextDefaultFormat);
                    }
                    break;

                case "MAGIC_LANGUAGE":
                    used = true;
                    Languages.LoadFromFile(entryName, value);
                    break;
                case "MAGIC_SERVICES":
                    used = true;
                    if (string.IsNullOrEmpty(value))
                        value = ",";
                    var sp1 = value.Split(',');
                    if (sp1.Length >= 2)
                    {
                        string url = sp1[1];
                        if (_webservicesUrls.ContainsKey(entryName))
                            _webservicesUrls.Remove(entryName);
                        string wsdl = null;
                        if (sp1[0].ToUpperInvariant().Trim() == "SOAP")
                        {
                            if (sp1.Length > 5)
                                wsdl = sp1[5];
                        }
                        _webservicesUrls.Add(entryName, new WebServices.WebServiceInfo(url, wsdl));
                    }
                    break;
                case "MAGIC_SPECIALS":
                    used = true;
                    switch (upperEntryName)
                    {
                        case "SPECIAL3DSTYLE":
                            FixedBackColorInNonFlatStyles = isNo;
                            break;
                        case "SPECIALASYNCCOMEVENTS":
                            AsyncComEvents = isYes;
                            break;
                        case "SPECIALTRIMPARAMDSQL":
                            Data.DynamicSQLEntity.DefaultTrimTextColumnParameterValues = isYes;
                            break;
                        case "SPECIALSKIPINMULTIEDIT":
                            AllowLeaveTextboxUsingArrowKeysInMultiline = isYes;
                            break;
                        case "CLOSEPRINTEDTABLESINSUBTASKS":
                            RestartTableOnTaskClose = isYes;
                            break;
                        case "HIGHRESOLUTIONPRINT":
                            BackwardCompatibleLowResolutionPrinting = isNo;
                            break;
                        case "SPECIALOLDZORDER":
                            DisableMDIChildZOrdering = isYes;
                            break;
                        case "SPECIALPAINTFORMINCREATE":
                            AlwaysKeepNewRowValuesVisibleAfterExit = isYes;
                            break;
                        case "SPECIALPDFRESOLUTION":
                            ENV.Printing.PrinterWriter.DpiForPdf = Number.Parse(upperValue);
                            break;
                        default:
                            used = false;
                            break;
                    }
                    break;
                case "MAGIC_DATABASES":
                    used = true;
                    var itemsInString = value.Split(',');
                    if (itemsInString.Length >= 8)
                    {
                        string initialCatalog = (itemsInString[1 + _iniOfset]).Trim(),
                            dataSource = (itemsInString[4]).Trim(),
                            username = (itemsInString[6]).Trim(),
                            password = (itemsInString[7]).Trim(),
                            path = (itemsInString[3]).Trim();
                        string dbType = itemsInString[_version10Compatible ? 1 : 0].Trim();
                        int connectionInfoLocation = 18;
                        if (_version10Compatible)
                            connectionInfoLocation = 17;
                        if (itemsInString.Length > connectionInfoLocation && dbType == "14")
                        {
                            var connectionInfo = itemsInString[connectionInfoLocation].Trim();
                            if (!string.IsNullOrEmpty(connectionInfo))
                                dataSource = connectionInfo;
                        }
                        bool checkDefinition = true;
                        if (itemsInString.Length > 10)
                        {
                            checkDefinition = itemsInString[10].ToUpper(CultureInfo.InvariantCulture).Trim() ==
                                              "CheckDefinition".ToUpper(CultureInfo.InvariantCulture);
                        }
                        bool autoCreateTables = false;
                        if (itemsInString.Length > 15 && itemsInString[15].ToUpper().Trim().StartsWith("CHECKEXIST"))
                            autoCreateTables = true;
                        if (itemsInString.Length > 16 && itemsInString[16].ToUpper().Trim().StartsWith("CHECKEXIST"))
                            autoCreateTables = true;
                        string isolationLevelString = Get("MAGIC_DBMS",
                            _version10Compatible ? "MicrosoftSQLServer" : "MicrosoftSQL");
                        System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted;
                        if (isolationLevelString != null)
                        {
                            var sp = isolationLevelString.Split(',');
                            int position = _version10Compatible ? 8 : 11;
                            if (sp.Length > position)
                            {
                                string isolationLevel1 = sp[position];
                                if (isolationLevel1 != null)
                                    switch (isolationLevel1.Trim())
                                    {
                                        case "0":
                                            isolationLevel = System.Data.IsolationLevel.ReadUncommitted;
                                            break;
                                        case "1":
                                            isolationLevel = System.Data.IsolationLevel.ReadCommitted;
                                            break;
                                    }
                            }
                        }
                        string hint = string.Empty;
                        if (itemsInString.Length > 15)
                            hint = itemsInString[15];
                        var hasAcsFile = itemsInString.Length > 19 && Version10Compatible && !Text.IsNullOrWhiteSpace(itemsInString[19]);
                        if (saveToContext)
                            ConnectionManager.Context.AddDatabase(entryName, dbType, initialCatalog, dataSource, username, password,
                                isolationLevel, hint, path, checkDefinition, autoCreateTables, hasAcsFile);
                        if (saveToShared)
                            ConnectionManager.Shared.AddDatabase(entryName, dbType, initialCatalog, dataSource, username, password,
                                isolationLevel, hint, path, checkDefinition, autoCreateTables, hasAcsFile);
                    }
                    break;


                case "MAGIC_SYSTEMS":
                    used = true;
                    if (upperEntryName == "SYSTEM" + _startApplication)
                    {
                        SetStartApplicationBySystem(value);
                    }
                    break;

                case "MAGIC_JAVA":

                    if (upperEntryName == "JAVA_HOME")
                    {
                        ENV.Java.JavaNativeInterface.JavaHome = value;
                        used = true;
                    }
                    if (upperEntryName == "CLASSPATH")
                    {
                        ENV.Java.JavaNativeInterface.ClassPath = PathDecoder.DecodePath(value);
                        used = true;
                    }
                    if (upperEntryName == "JVM_ARGS")
                    {
                        ENV.Java.JavaNativeInterface.JvmOptions = value;
                        used = true;
                    }
                    break;
            }
            if (upperEntryName == "OWNER" && (!_suspendSave || upperSection == MAGIC_ENV))
                UserMethods.SetOwner(value);
            if (used)
                MarkUsed(sectionName, entryName);

        }

        public static void ShowSplashScreen()
        {
            if (ENV.Common._suppressDialogForTesting)
                return;
            var fileName = ENV.PathDecoder.DecodePath(SplashScreenImageFileName);
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                try
                {
                    var image = System.Drawing.Image.FromFile(fileName);
                    var f = new System.Windows.Forms.Form() { BackgroundImage = image, FormBorderStyle = System.Windows.Forms.FormBorderStyle.None, StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen, Size = image.Size };
                    f.Show();
                    ENV.UserMethods.Instance.Delay(10);

                    Context.Current.BeginInvoke(() => f.Close());
                }
                catch (Exception ex)
                {
                    ENV.ErrorLog.WriteToLogFile(ex);
                }
            }
        }
        /// <summary>
        /// Controls when transactions are opened - default true
        /// <seealso cref="http://doc.fireflymigration.com/special-transaction-behaviour.html"/>
        /// </summary>
        public static bool ConditionalTransaction = true;
        static string SplashScreenImageFileName;

        private static void SetStartApplicationBySystem(string value)
        {
            var y = value.Split(',');
            if (y.Length > 3)
            {
                ENV.UserMethods.SetApplicationPrefix(y[1]);
                ENV.UserMethods.SetSys(y[0]);
                Common.ApplicationTitle = y[0];
            }
        }

        static bool SaveAllIniValuesToFile;
        public static void MergeAndSave(string[] args)
        {
            try
            {
                SaveAllIniValuesToFile = true;
                InitUserSettings("magic.ini", args);
                using (var sw = new StreamWriter(_loadedINIPath + ".merged", false, Encoding))
                    SaveIniFile(sw);
            }
            finally
            {
                SaveAllIniValuesToFile = false;
            }
        }

        static UserSettings()
        {
            DefaultAllowInsertInUpdateActivity = true;
            DisableMDIChildZOrdering = true;
            HttpWebRequestTimeoutInSeconds = 120;
            Clear();
        }
        public static bool DoNotDisplayUI { get; set; }
        internal static string _loadedINIPath;
        static bool _saveOnEveryChange = false;
        static bool _suspendSave = false;
        public static bool EditSecuredValues { get; set; }
        public static bool AsyncComEvents { get; set; }
        public static bool DisableMDIChildZOrdering { get; set; }
        public static bool AlwaysKeepNewRowValuesVisibleAfterExit { get; set; }
        public static int HttpWebRequestTimeoutInSeconds { get; set; }
        public static bool ReadOnlyIniFile { get; set; }


        static bool _v8Compatible;
        public static bool Version8Compatible
        {
            get { return _v8Compatible; }
            set
            {
                _v8Compatible = value;
                AllowLeaveTextboxUsingArrowKeysInMultiline = value;
                Firefly.Box.Data.Advanced.TypedColumnBase<Number>.SupportInvalidValuesInInputRange = value;
            }
        }

        static bool _reloadDataAfterUpdatingOrderByColumns;
        public static bool ReloadDataAfterUpdatingOrderByColumns
        {
            get { return _reloadDataAfterUpdatingOrderByColumns; }
            set
            {
                if (_reloadDataAfterUpdatingOrderByColumns != value)
                {
                    _reloadDataAfterUpdatingOrderByColumns = value;
                    foreach (var activeTask in Firefly.Box.Context.Current.ActiveTasks)
                    {
                        var uic = activeTask as UIController;
                        if (uic != null)
                            uic.ReloadDataAfterUpdatingOrderByColumns = value;
                    }
                }
            }
        }

        static void Save()
        {
            try
            {
                if (!ReadOnlyIniFile && !_suspendSave && _loadedINIPath != null)
                    WriteFile(_loadedINIPath, SaveIniFile);
                _wasAnyChange = false;
            }
            catch
            {
                //System.Windows.Forms.MessageBox.Show("Failed to save ini to: " + _loadedINIPath);
            }

        }
        static bool _fixedBackColorInNonFlatStyles = false;
        public static bool FixedBackColorInNonFlatStyles
        {
            get { return _fixedBackColorInNonFlatStyles; }
            set
            {
                _fixedBackColorInNonFlatStyles = value;
                if (Firefly.Box.Context.Current.ActiveTasks.Count > 0)
                {
                    ITask task = Firefly.Box.Context.Current.ActiveTasks[Firefly.Box.Context.Current.ActiveTasks.Count - 1];
                    {
                        var f = task.View;
                        if (f != null)
                            SetControls(f.Controls);
                    }
                }
            }
        }

        static bool _suppressUpdatesInBrowseActivity = false;
        public static bool SuppressUpdatesInBrowseActivity
        {
            get
            {
                return _suppressUpdatesInBrowseActivity;
            }
            set
            {
                if (value == _suppressUpdatesInBrowseActivity) return;

                _suppressUpdatesInBrowseActivity = value;
                foreach (var task in Firefly.Box.Context.Current.ActiveTasks)
                    Common.SetColumnsReadonlyAcordingToActivity(task.Columns, task.Activity);
            }
        }
        public static bool _allowInsertInUpdateActivity = false;
        public static bool DefaultAllowInsertInUpdateActivity
        {
            get { return _allowInsertInUpdateActivity; }
            set
            {
                if (_allowInsertInUpdateActivity != value)
                {
                    _allowInsertInUpdateActivity = value;
                    foreach (var activeTask in Firefly.Box.Context.Current.ActiveTasks)
                    {
                        var uic = activeTask as UIController;
                        if (uic != null)
                        {
                            uic.AllowInsertInUpdateActivity = value;
                        }
                    }
                }
            }
        }
        static void SetControls(IEnumerable e)
        {
            foreach (System.Windows.Forms.Control o in e)
            {
                var c = o as ControlBase;
                if (c != null)
                    c.FixedBackColorInNonFlatStyles = _fixedBackColorInNonFlatStyles;
                SetControls(o.Controls);
            }
        }

        public static void InitUserSettings(string defaultIniFileName, string[] mainArgs)
        {
            Action<string, string> what = (iniFile, commandLine) =>
            {
                try
                {
                    LoadIniFile(iniFile, commandLine);
                }
                catch (Exception e)
                {
                    if (Environment.UserInteractive)
                        Common.ShowExceptionDialog(e, "Failed to load ini file");
                    else throw;
                    //Common.ShowMessageBox("Error", System.Windows.Forms.MessageBoxIcon.Error, "Failed to load ini " + _loadedINIPath);
                }

            };
            ParseCommandLine(defaultIniFileName, mainArgs, what);
        }

        internal static void ParseCommandLine(string defaultIniFileName, string[] mainArgs, Action<string, string> what)
        {

            var args = new List<string>(mainArgs);
            if (args.Count == 0)
            {
                what(defaultIniFileName, "");
                return;
            }
            string iniFile = defaultIniFileName;
            if (args[0].StartsWith("/ini", StringComparison.CurrentCultureIgnoreCase))
            {

                int i = args[0].IndexOf('=');
                if (i == -1)
                {
                    args.RemoveAt(0);
                    i = args[0].IndexOf('=');
                    if (i >= 0)
                        args[0] = args[0].Remove(0, i + 1);
                }
                else
                {
                    args[0] = args[0].Remove(0, i + 1);
                }
                if (args[0].Length == 0)
                    args.RemoveAt(0);
                iniFile = args[0];
                args.RemoveAt(0);

            }
            for (int i = 0; i < args.Count; i++)
            {
                if (args[i].StartsWith("@") && args[i].Contains(" "))
                {
                    args[i] = args[i].Insert(1, "\"") + "\"";
                }
                if (args[i].StartsWith("/@") && args[i].Contains(" "))
                {
                    args[i] = args[i].Insert(2, "\"") + "\"";
                }
            }


            string commandLine = string.Join(" ", args.ToArray());

            what(iniFile, commandLine);


        }
        public static void LoadIniFile(string iniFile)
        {
            LoadIniFile(iniFile, "");
        }

        static void LoadIniFile(string iniFile, string commandLineArgs)
        {
            _loadedINIPath = iniFile;
            if (!File.Exists(_loadedINIPath))
            {
                var ea = System.Reflection.Assembly.GetEntryAssembly();
                if (ea != null)
                {
                    var s = ea.Location;
                    s = Path.GetDirectoryName(s);
                    _loadedINIPath = Path.Combine(s, iniFile);
                }
            }
            ReadFile(_loadedINIPath, y => InitUserSettings(y, commandLineArgs));
        }

        public static Encoding Encoding = ENV.LocalizationInfo.Current.OuterEncoding;
        static void ReadFile(string path, Action<StreamReader> read)
        {
            if (!System.IO.File.Exists(path))
                throw new System.IO.FileNotFoundException("File not found - " + path);
            if (System.Web.HttpContext.Current == null && !ReadOnlyIniFile && (File.GetAttributes(path) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly &&
                (!File.Exists(path) || File.Exists(path + ".bak") || File.Exists(path + ".new")))
                Lock(path, () => CleanUpFile(path));

            using (var sr = new StreamReader(path, Encoding))
            {
                read(sr);
            }
        }

        static void CleanUpFile(string path)
        {
            if (!File.Exists(path) && File.Exists(path + ".bak"))
            {
                CatchIOException(() => File.Copy(path + ".bak", path));
            }
            CatchIOException(() => File.Delete(path + ".new"));
            CatchIOException(() => File.Delete(path + ".bak"));
        }

        static void Lock(string path, Action action)
        {
            FileStream lockFile = null;
            while (lockFile == null)
            {
                CatchIOException(() => lockFile = new FileStream(path + ".lck", FileMode.Create, FileAccess.ReadWrite, FileShare.None, 0x1000, FileOptions.DeleteOnClose));
            }
            try
            {
                action();
            }
            finally
            {
                lockFile.Dispose();
            }
        }

        static void CatchIOException(Action action)
        {
            try { action(); }
            catch (System.IO.IOException) { }
        }
        static object _lockReference = new object();
        static void WriteFile(string path, Action<StreamWriter> write)
        {
            if ((File.GetAttributes(path) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) return;
            lock (_lockReference)
            {
                Lock(path,
                     () =>
                     {
                         CleanUpFile(path);
                         CatchIOException(
                             () =>
                             {
                                 using (var tempSW = new StreamWriter(path + ".new", false, Encoding))
                                 {
                                     write(tempSW);
                                     tempSW.Flush();
                                     tempSW.Close();
                                 }
                                 File.Replace(path + ".new", path, path + ".bak");
                                 File.Delete(path + ".bak");
                             });
                     });
            }
        }

        internal static void SaveIniFile(System.IO.TextWriter tw)
        {
            SharedSettings.SaveIniFile(tw);

        }
        internal static void InitUserSettings(System.IO.TextReader iniValues, string commandLine)
        {
            try
            {
                Number.GroupSeparator.SetSharedValue(' ');
                _suspendSave = true;
                PathDecoder.SharedPathDecoder.NewValuesAreNotCaseSensative = false;
                string section = MAGIC_ENV;
                string line;
                while ((line = iniValues.ReadLine()) != null)
                {
                    while (line.TrimEnd(' ').EndsWith("+"))
                    {
                        line = line.TrimEnd(' ');
                        line = line.Remove(line.Length - 1);
                        line += iniValues.ReadLine();
                    }
                    if (line.StartsWith("["))
                    {
                        section = line.Substring(1, line.IndexOf(']') - 1);
                    }
                    else
                    {
                        if (line.Trim() == string.Empty)
                            continue;
                        if (line.IndexOf("=") == -1)
                            continue;
                        string name = line.Remove(line.IndexOf("=")).Trim();
                        string value = line.Substring(line.IndexOf("=") + 1).Trim(' ');
                        Set(section, name, value, true, true);
                    }
                }
                if (!string.IsNullOrEmpty(commandLine))
                    ParseCommandLineArguments(commandLine);
                if (Version10Compatible)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(System.Windows.Forms.Application.ExecutablePath))
                            Set(UserSettings.MAGIC_LOGICAL_NAMES, "EngineDir", System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\", false, true);
                        Set(UserSettings.MAGIC_LOGICAL_NAMES, "TempDir", System.IO.Path.GetTempPath(), false, true);
                        Set(UserSettings.MAGIC_LOGICAL_NAMES, "WorkingDir", System.Environment.CurrentDirectory + "\\", false, true);
                    }
                    finally
                    {
                    }
                }
                _suspendSave = false;
                _wasAnyChange = false;
                PathDecoder.SharedPathDecoder.NewValuesAreNotCaseSensative = true;
            }
            finally
            {
            }
        }
        public static void SetWorkingDirBasedOnStartApplication()
        {
            var startApplication = Get(MAGIC_ENV, "StartApplication");
            if (Text.IsNullOrWhiteSpace(startApplication))
                return;
            var startDir = Path.GetDirectoryName(startApplication);
            Environment.CurrentDirectory = startDir;
            UserMethods._projectDir = startDir + "\\";
            Set(MAGIC_LOGICAL_NAMES, "WorkingDir", startDir + "\\", false, true);
        }


        const string MAGIC_ENV = "MAGIC_ENV", MAGIC_PRINTERS = "MAGIC_PRINTERS", MAGIC_LOGICAL_NAMES = "MAGIC_LOGICAL_NAMES";

        class IniParseAndSet
        {
            bool _saveToFile;
            bool _saveToShared;
            string _section = MAGIC_ENV;
            string _name = "";
            bool _useColonAsEquals;

            public IniParseAndSet(bool saveToFile, bool useColonAsEquals, bool saveToShared)
            {
                _useColonAsEquals = useColonAsEquals;
                _saveToFile = saveToFile;
                _saveToShared = saveToShared;
            }
            class Start : CharProcessor
            {
                IniParseAndSet _parent;

                public Start(IniParseAndSet parent)
                {
                    _parent = parent;
                }

                public void Process(char c, SetCharProcessor setState)
                {
                    switch (c)
                    {
                        case ' ':
                            break;
                        case '[':
                            setState(new SectionReader(_parent), false);
                            break;
                        default:
                            setState(new Name(_parent,
                                s =>
                                {
                                    var z = s.ToUpper().Trim();
                                    return z == "THOUSANDSEPARATOR" || z == "DECIMALSEPARATOR";
                                }, IgnoreSlash), true);
                            break;
                    }
                }

                public void Finish()
                {

                }
            }
            class Name : CharProcessor
            {
                StringBuilder _sb = new StringBuilder();
                IniParseAndSet _parent;
                Func<string, bool> _ignoreCommans, _ignoreSlash;

                public Name(IniParseAndSet parent, Func<string, bool> ignoreCommans, Func<string, bool> ignoreSlash)
                {
                    _ignoreCommans = ignoreCommans;
                    _ignoreSlash = ignoreSlash;
                    _parent = parent;
                }

                public void Process(char c, SetCharProcessor setState)
                {
                    if (c == '=' || _parent._useColonAsEquals && c == ':')
                    {
                        _parent._name = _sb.ToString().Trim();
                        setState(new Value(_parent, _ignoreCommans(_parent._name), _ignoreSlash(_parent._name)), false);
                    }
                    else
                        _sb.Append(c);
                }

                public void Finish()
                {
                    _parent._name = _sb.ToString().Trim();
                    new Value(_parent, _ignoreCommans(_parent._name), _ignoreSlash(_parent._name)).Finish();
                }
            }
            class Value : CharProcessor
            {
                IniParseAndSet _parent;
                StringBuilder _sb = new StringBuilder();
                bool _ignoreCommans;
                bool _ignoreSlash;
                public Value(IniParseAndSet parent, bool ignoreCommans, bool ignoreSlash)
                {
                    _ignoreCommans = ignoreCommans;
                    _parent = parent;
                    _ignoreSlash = ignoreSlash;
                }
                bool _prevCharWasComma = true;
                public void Process(char c, SetCharProcessor setState)
                {
                    if (_ignoreCommans)
                        switch (c)
                        {
                            case ',':
                                _sb.Append(c);
                                _prevCharWasComma = true;
                                return;
                            case ' ':
                                if (_prevCharWasComma)
                                {
                                    _sb.Append(c);
                                    return;
                                }

                                break;
                            default:
                                _prevCharWasComma = false;
                                break;
                        }
                    switch (c)
                    {
                        case ',':

                            Finish();
                            _parent._section = MAGIC_ENV;
                            _parent._name = "";
                            setState(new Start(_parent), false);
                            break;
                        case '*':
                            setState(new ReadToEnd(delegate (string s)
                            {
                                _sb.Append(s);
                                Finish();
                            }), false);
                            break;
                        case '\\':
                            if (_ignoreSlash)
                                _sb.Append(c);
                            else
                                setState(new EscapeCharacter(this, a => _sb.Append(a)), false);
                            break;
                        case '\'':
                            setState(new ContainedString('\'', this, a => _sb.Append(a)), false);
                            break;
                        case ' ':
                            if (_sb.Length > 0)
                                _sb.Append(' ');
                            break;
                        default:
                            _sb.Append(c);
                            break;
                    }
                }

                public void Finish()
                {
                    Set(_parent._section, _parent._name, _sb.ToString(), _parent._saveToFile, _parent._saveToShared);
                }
            }
            class SectionReader : CharProcessor
            {
                StringBuilder _sb = new StringBuilder();
                IniParseAndSet _parent;

                public SectionReader(IniParseAndSet parent)
                {
                    _parent = parent;
                }

                public void Process(char c, SetCharProcessor setState)
                {
                    if (c == ']')
                    {
                        var s = _sb.ToString().Trim();
                        _parent._section = s;
                        setState(new Name(_parent, name => IgnoreCommas(s, name), IgnoreSlash), false);
                    }
                    else
                        _sb.Append(c);
                }

                public void Finish()
                {

                }
            }

            public void Parse(string s)
            {
                new StringParser().Parse(s, new Start(this));
            }
        }
        static bool IgnoreCommas(string sectionName, string parameterName)
        {
            return Array.Exists(new[]{"MAGIC_DATABASES" ,"MAGIC_DBMS" ,"MAGIC_COMMS",
                   "MAGIC_SERVERS" , "MAGIC_SYSTEMS","MAGIC_SERVICES",MAGIC_PRINTERS},
                   s => sectionName.Equals(s, StringComparison.InvariantCultureIgnoreCase)) ||
                   parameterName.Equals("LdapConnectionString", StringComparison.InvariantCultureIgnoreCase) ||
                   parameterName.Equals("DECIMALSEPARATOR", StringComparison.InvariantCultureIgnoreCase) ||
                   parameterName.Equals("THOUSANDSEPARATOR", StringComparison.InvariantCultureIgnoreCase) ||
                   parameterName.Equals("LdapDomainContext", StringComparison.InvariantCultureIgnoreCase);
        }
        static bool IgnoreSlash(string name)
        {
            return name.ToUpper() == "STARTAPPLICATION";
        }


        public static bool ParseAndSet(string textToPut, bool saveToFile, bool useColonAsEquals, bool saveToShared)
        {
            new IniParseAndSet(saveToFile, useColonAsEquals, saveToShared).Parse(textToPut);
            return true;
        }

        internal class IniInstance
        {
            OrderedDictionary<ASection> _sections = new OrderedDictionary<ASection>();
            IniInstance _parentIniInstance;

            public IniInstance(IniInstance parentIniInstance)
            {
                _parentIniInstance = parentIniInstance;
            }

            public void Set(string sectionName, string name, string value, bool saveToFile)
            {
                ASection s = null;
                if (!_sections.TryGetValue(sectionName, out s))
                {
                    s = new ASection(sectionName);
                    _sections.Add(sectionName, s);
                }
                s.Set(name, value, saveToFile);
            }

            public string Get(string section, string name)
            {
                ASection result;
                if (_sections.TryGetValue(section, out result))
                    return result.GetValue(name, _parentIniInstance, section);
                if (_parentIniInstance != null)
                    return _parentIniInstance.Get(section, name);
                return "";
            }

            public string GetItemAt(string section, int line)
            {
                ASection result;
                if (_sections.TryGetValue(section, out result))
                {
                    return result.GetItemAt(line);
                }
                return "";
            }

            public void SaveIniFile(TextWriter tw)
            {
                foreach (var section in _sections)
                {
                    section.SaveTo(tw);
                }
            }

            internal void SendSectionsAndNamesTo(Action<string, string> to)
            {
                foreach (var item in _sections)
                {
                    item.SendSectionAndNamesTo(to);
                }
            }
        }

        internal static IniInstance SharedSettings = new IniInstance(null);

        static readonly ContextStatic<IniInstance> ContextSettings = new ContextStatic<IniInstance>(() => new IniInstance(SharedSettings));


        static bool _gridStartOnRowPositionWasSet = false;
        static GridStartOnRowPosition _gridStartOnRowPosition = GridStartOnRowPosition.MiddleRowNeverBelow;
        public static GridStartOnRowPosition GridStartOnRowPosition
        {
            get
            {
                if (_gridStartOnRowPositionWasSet)
                    return _gridStartOnRowPosition;
                return Get(MAGIC_ENV, "CENTERSCREENINONLINE") == "N" ? GridStartOnRowPosition.TopRow : GridStartOnRowPosition.MiddleRow;

            }
            set
            {
                _gridStartOnRowPosition = value;
                _gridStartOnRowPositionWasSet = true;
                refreshGridStartOnRow();
            }
        }

        private static void refreshGridStartOnRow()
        {
            if (Firefly.Box.Context.Current.ActiveTasks.Count > 0)
            {
                ITask task = Firefly.Box.Context.Current.ActiveTasks[Firefly.Box.Context.Current.ActiveTasks.Count - 1];
                {
                    var f = task.View;
                    if (f != null)
                        foreach (var c in f.Controls)
                        {
                            var g = c as Grid;
                            if (g != null)
                                g.StartOnRowPosition = GridStartOnRowPosition;
                        }
                }
            }
        }

        public static void Set(string sectionName, string name, string value, bool saveToFile)
        {
            Set(sectionName, name, value, saveToFile, saveToFile);
        }
        public static void Set(string sectionName, string name, string value, bool saveToFile, bool saveToShared)
        {
            value = value.TrimEnd(' ');
            if (saveToFile)
                _wasAnyChange = true;

            else if (IgnoreCommas(sectionName, name))
            {
                var sb = new StringBuilder();
                bool first = true;
                foreach (var s1 in value.Split(','))
                {
                    if (first)
                        first = false;
                    else
                        sb.Append(',');
                    sb.Append(s1.Trim());
                }
                value = sb.ToString();
            }

            var saveToContext = !_suspendSave;


            value = value.Trim(' ');
            switch (sectionName)
            {
                case MAGIC_PRINTERS:

                    var v = value.Split(',');
                    if (v.Length < 2)
                        value += ",";
                    if (v.Length < 3)
                        value += ",";
                    if (v.Length < 4)
                        value += ",0";
                    break;
            }
            if (saveToContext)
                ContextSettings.Value.Set(sectionName, name, value, saveToFile);
            if (saveToShared)
                SharedSettings.Set(sectionName, name, value, saveToFile);

            HandleSpecialIniValues(sectionName, name, value, saveToShared, saveToContext);

            if (_saveOnEveryChange && saveToFile && !_suspendAutoSave)
                Save();
        }



        internal static Bool RestartTableOnTaskClose = true;
        internal class PrinterInfo
        {
            public PrinterInfo()
            {
                LinesPerTextPage = -1;
            }

            public string PrinterName = "", AttributeFileName = "", CustomEncodingFile = "";

            public int LinesPerTextPage { get; set; }
        }

        static Dictionary<string, WebServices.WebServiceInfo> _webservicesUrls = new Dictionary<string, WebServices.WebServiceInfo>();
        public static string GetWebServiceUrl(string name)
        {
            if (_webservicesUrls.ContainsKey(name))
                return _webservicesUrls[name].Url;
            else
                return "";
        }
        public static WebServices.WebServiceInfo GetWebServiceInfo(string name)
        {
            if (_webservicesUrls.ContainsKey(name))
                return _webservicesUrls[name];
            else
                return new WebServices.WebServiceInfo("", null);
        }
        internal static PrinterInfo GetPrinterName(string name)
        {
            if (!Text.IsNullOrEmpty(name))
            {
                var value = Get(MAGIC_PRINTERS, name.TrimEnd());
                if (!Text.IsNullOrEmpty(value))
                    try
                    {
                        var sp = value.Split(',');
                        string printerName = sp[0];
                        var attributeFile = "";
                        if (sp.Length > 1)
                            attributeFile = sp[1];
                        string customEncodingFile = null;
                        if (sp.Length > 2)
                            customEncodingFile = sp[2];
                        int linesPerTextPage = 0;
                        if (sp.Length > 3)
                            linesPerTextPage = Number.Parse(sp[3]);
                        var x = new PrinterInfo
                        {
                            PrinterName = printerName,
                            AttributeFileName = attributeFile,
                            CustomEncodingFile = customEncodingFile,
                            LinesPerTextPage = linesPerTextPage
                        };
                        return x;
                    }
                    catch
                    {
                    }
            }
            return new PrinterInfo();



        }
        public static bool ContainsPrinter(string name)
        {
            return !Text.IsNullOrEmpty(Get(MAGIC_PRINTERS, name.TrimEnd()));

        }

        static int _iniOfset = 0;

        static bool _version10Compatible = false;
        public static bool Version10Compatible
        {
            get
            {
                return _version10Compatible;
            }
            set
            {
                if (value)
                {
                    DisableMDIChildZOrdering = false;
                    ENV.Data.ByteArrayColumn._byteParameterConverter.ContentType =
                        Data.ByteArrayColumnContentType.BinaryUnicode;
                    DynamicSQLEntity.TrimExpressionValuesDefault = false;

                    _iniOfset = 1;

                }
                else
                {
                    DisableMDIChildZOrdering = true;
                    ENV.Data.ByteArrayColumn._byteParameterConverter.ContentType =
                        Data.ByteArrayColumnContentType.BinaryAnsi;
                    DynamicSQLEntity.TrimExpressionValuesDefault = true;

                    _iniOfset = 0;
                }


                _version10Compatible = value;
            }
        }

        static bool _xpa;
        public static bool VersionXpaCompatible
        {
            get { return _xpa; }
            set
            {
                _xpa = value;
                Relation.RecomputeOnNotifyRowWasFoundToParentColumn = value;
            }
        }

        class ASection
        {
            OrderedDictionary<Value> _values = new OrderedDictionary<Value>();
            string _name;
            public override string ToString()
            {
                return _name;
            }

            public ASection(string name)
            {
                _name = name;
            }

            class Value
            {
                string _name;

                string _value = "";
                string _valueInFile = null;
                public bool ShowFirstInGetLn = false;
                public Value(string name)
                {
                    _name = name;
                }

                public override string ToString()
                {
                    return GetString();
                }

                public void Set(string value, bool saveToFile)
                {
                    ShowFirstInGetLn = _readingCommandLineArguments;
                    _value = value.TrimEnd(' ');
                    if (saveToFile || SaveAllIniValuesToFile)
                        _valueInFile = _value;
                }

                public string GetValue()
                {
                    return _value;
                }

                public void SaveTo(TextWriter tw)
                {
                    if (_valueInFile != null && _valueInFile.Trim(' ').Length > 0 || SaveAllIniValuesToFile)
                        tw.WriteLine(_name + " = " + _valueInFile);
                }

                public string GetString()
                {
                    return _name + "=" + _value;
                }

                public bool IsInFile()
                {
                    return _valueInFile != null;
                }

                internal void SendNameTo(Action<string> p)
                {
                    p(_name);
                }

                internal bool isValid()
                {
                    return !_name.StartsWith(";");
                }
            }



            public void Set(string name, string value, bool saveTofile)
            {
                Value val = null;
                if (!_values.TryGetValue(name, out val))
                {
                    val = new Value(name);
                    _values.Add(name, val);
                }
                val.Set(value, saveTofile);
            }

            public string GetValue(string name, IniInstance parentInstance, string section)
            {
                Value result;
                if (_values.TryGetValue(name, out result))
                    return result.GetValue();
                if (parentInstance != null)
                    return parentInstance.Get(section, name);
                return "";
            }

            public void SaveTo(TextWriter tw)
            {
                tw.WriteLine("[" + _name + "]");
                foreach (var value in _values)
                {
                    value.SaveTo(tw);
                }
                tw.WriteLine();
            }

            public string GetItemAt(int line)
            {
                if (line > _values.Count || line < 1)
                    return "";
                foreach (var v in _values)
                {
                    if (v.isValid())
                        if (v.ShowFirstInGetLn)
                        {
                            line--;
                            if (line <= 0)
                                return v.ToString();
                        }
                }
                foreach (var v in _values)
                {
                    if (v.isValid())
                        if (!v.ShowFirstInGetLn)
                        {
                            line--;
                            if (line <= 0)
                                return v.ToString();
                        }
                }
                return "";
            }

            internal void SendSectionAndNamesTo(Action<string, string> to)
            {
                foreach (var item in _values)
                {
                    item.SendNameTo(n => { to(_name, n); });
                }
            }
        }
        public static string Get(string section, string name)
        {
            MarkUsed(section, name);
            return ContextSettings.Value.Get(section, name);


        }

        public static string ParseAndGet(string nameToGet)
        {
            string Section = "MAGIC_ENV";
            string Entry = nameToGet;
            if (Entry.Contains("["))
            {
                Entry = SubstringAfter(Entry, "[");
                if (!Entry.Contains("]"))
                    return "";
                Section = RemoveAt(Entry, "]").ToUpper(CultureInfo.InvariantCulture);
                Entry = SubstringAfter(Entry, "]").Trim();
            }
            Entry = Entry.Trim();

            if (Entry.EndsWith("="))
            {
                Entry = Entry.Remove(Entry.IndexOf("=")).Trim();
            }
            var r = Get(Section, Entry);
            if (Text.IsNullOrEmpty(r) && LocalizationInfo.Current.RightToLeft == System.Windows.Forms.RightToLeft.Yes && Section == "MAGIC_DATABASES")
                r = Get(Section, ENV.UserMethods.Instance.Flip(Entry));

            return r;
        }
        public static string SubstringAfter(string _string, string textToLookForAndStartAfter)
        {
            return _string.Substring(_string.IndexOf(textToLookForAndStartAfter) + textToLookForAndStartAfter.Length);
        }
        public static string RemoveAt(string _string, string textToLookForAndRemoveItAndWhatsAfterIt)
        {
            return _string.Remove(_string.IndexOf(textToLookForAndRemoveItAndWhatsAfterIt));
        }

        public static void Clear()
        {
            SharedSettings = new IniInstance(null);
            ContextSettings.DisposeAndClearValue();
            _suspendSave = false;
            Number.GroupSeparator.SetSharedValue(',');

        }



        static bool _readingCommandLineArguments = false;
        internal static void ParseCommandLineArguments(string commandLineArgs)
        {
            var x = _readingCommandLineArguments;
            _readingCommandLineArguments = true;
            try
            {
                CommandLineParser p = null;
                p = new CommandLineParser(s => ParseAndSet(s, false, true, true),
                    ReadIniAdditionsFile);
                p.Parse(new StringReader(commandLineArgs));
            }
            finally
            {
                _readingCommandLineArguments = x;
            }
        }

        public static void ReadIniAdditionsFile(string fileName)
        {
            var p = new CommandLineParser(s => ParseAndSet(s, false, true, true),
                ReadIniAdditionsFile);
            if (!File.Exists(fileName))
                return;
            var x = _suspendSave;
            _suspendSave = true;
            try
            {
                using (var sr = new StreamReader(fileName, LocalizationInfo.Current.OuterEncoding))
                {
                    p.Parse(sr);
                }
            }
            catch
            {
            }
            finally
            {
                _suspendSave = x;
            }


        }

        public static string GetItemAt(string section, int line)
        {
            return SharedSettings.GetItemAt(section, line);

        }
        static bool _wasAnyChange = false;
        public static void FinalizeINI()
        {
            if (!_saveOnEveryChange && _wasAnyChange)
                Save();
        }

        static bool _suspendAutoSave = false;
        public static bool ShowToolStrip = true;
        public static bool AllowLeaveTextboxUsingArrowKeysInMultiline { get; set; }
        public static void SuspendAutoSave()
        {
            _suspendSave = true;
        }

        public static bool ForceTextBoxMinimumHeightWhileFocused { get; set; }

        public static bool BackwardCompatibleLowResolutionPrinting { get; set; }
        public static bool SuppressConst { get; set; }

        public static bool ProcessFlowBeforeQueuedEventsAfterExitingSubForm { get; set; }

        public static string GetApplicationServerUrl(string serviceName)
        {
            var result = Get("ApplicationServers", serviceName.TrimEnd());
            if (!string.IsNullOrEmpty(result))
                return result;
            return Get(MAGIC_ENV, "TemporaryRemoteUrl");
        }
        static HashSet<string> _wasUsed = new HashSet<string>();
        static bool _disableUsed = false;
        static void MarkUsed(string section, string name)
        {
            if (_disableUsed)
                return;
            var x = GetUsedKey(section, name);
            if (!_wasUsed.Contains(x))
            {
                lock (_wasUsed)
                {
                    if (!_wasUsed.Contains(x))
                        _wasUsed.Add(x);
                }
            }
        }

        private static string GetUsedKey(string section, string name)
        {
            return section.ToUpperInvariant() + "|" + name.ToUpperInvariant();
        }

        public static void Display()
        {
            var e = new ENV.Data.Entity("UserSettings", new MemoryDatabase());
            Data.NumberColumn id = new ENV.Data.NumberColumn("order"), sectionid = new ENV.Data.NumberColumn("sectionOrder");
            ENV.Data.TextColumn section, name, value, translatedValue;
            var entryUsed = new Data.BoolColumn("used", "5");
            e.Columns.Add(section = new Data.TextColumn("Section", "50"), name = new ENV.Data.TextColumn("Name", "50"), value = new ENV.Data.TextColumn("Value", "1000"), translatedValue = new ENV.Data.TextColumn("TranslatedValue", "1000"), sectionid);
            e.Columns.Add(entryUsed);
            e.IdentityColumn = id;
            var used = new HashSet<string>();
            var sections = new Dictionary<string, int>();

            Action<string, string> addToViewer = (s, n) =>
            {
                if (used.Contains(s + n))
                    return;
                used.Add(s + n);
                int sectionOrder;
                if (!sections.TryGetValue(s, out sectionOrder))
                    sections.Add(s, sectionOrder = sections.Count);
                e.Insert(() =>
                {
                    section.Value = s;
                    name.Value = n;
                    value.Value = Get(s, n);
                    translatedValue.Value = PathDecoder.DecodePath(value);
                    sectionid.Value = sectionOrder;
                    entryUsed.Value = _wasUsed.Contains(GetUsedKey(s, n));
                });
            };
            _disableUsed = true;
            try
            {
                SharedSettings.SendSectionsAndNamesTo(addToViewer);
                ContextSettings.Value.SendSectionsAndNamesTo(addToViewer);
            }
            finally
            {
                _disableUsed = false;
            }
            var eb = new ENV.Utilities.EntityBrowser(e, false);
            eb.OrderBy = new Sort(sectionid, id);
            eb.AddColumns(section, name, value, translatedValue, entryUsed);
            eb.rtl = System.Windows.Forms.RightToLeft.No;
            eb.Run();
        }
    }
    class OrderedDictionary<DataType> : IEnumerable<DataType>
    {
        Dictionary<string, DataType> _dict = new Dictionary<string, DataType>();
        List<DataType> _list = new List<DataType>();

        public int Count
        {
            get { return _list.Count; }
        }

        public IEnumerator<DataType> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
        public void Add(string key, DataType value)
        {
            _dict.Add(key.ToUpper(CultureInfo.InvariantCulture), value);
            _list.Add(value);
        }

        public DataType this[int i]
        {
            get { return _list[i]; }
        }

        public bool TryGetValue(string key, out DataType value)
        {
            return _dict.TryGetValue(key.ToUpper(CultureInfo.InvariantCulture), out value);
        }

        public void Clear()
        {
            _list.Clear();
            _dict.Clear();
        }
    }
}