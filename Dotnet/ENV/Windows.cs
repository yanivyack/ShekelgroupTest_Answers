using System;
using System.Globalization;
using System.Text;
using Firefly.Box;
using System.Runtime.InteropServices;

namespace ENV
{
    public class Windows
    {


        internal static string DefaultOSCommand { get;  set; }

        public static void Run(string what, string commandLineArgs, string workingDirectory, bool wait = false)
        {
            var p = new System.Diagnostics.Process()
            {
                StartInfo =
                    new System.Diagnostics.ProcessStartInfo(what) { WorkingDirectory = workingDirectory, Arguments = commandLineArgs }
            };
            p.Start();
            if (wait)
                p.WaitForExit();

        }



        public static int OSCommand(string whatToRun)
        {
            return OSCommand(whatToRun, false, System.Diagnostics.ProcessWindowStyle.Normal);
        }

        public static int OSCommand(string whatToRun, System.Diagnostics.ProcessWindowStyle windowsStyle)
        {
            return OSCommand(whatToRun, false, windowsStyle);
        }
        public static int OSCommand(string whatToRun, bool wait)
        {
            return OSCommand(whatToRun, wait, System.Diagnostics.ProcessWindowStyle.Normal);
        }

        public static int OSCommand(string whatToRun, bool wait, System.Diagnostics.ProcessWindowStyle windowsStyle)
        {
            if (Text.IsNullOrEmpty(whatToRun))
                whatToRun = DefaultOSCommand;
            if (Text.IsNullOrEmpty(whatToRun))
                whatToRun = Environment.CurrentDirectory;
            whatToRun = PathDecoder.DecodePath(whatToRun).Trim();
            try
            {
                var s = SplitCommandLine(whatToRun);
                var executableName = s[0];

                if (wait)
                {
                    var extension = System.IO.Path.GetExtension(executableName).ToUpper(CultureInfo.InvariantCulture);
                    if (s[1].Length > 0 && Array.IndexOf(new[] { "", ".EXE", ".BAT", ".COM",".CMD" }, extension) == -1)
                    {
                        s = SplitCommandLine(s[1]);
                        executableName = s[0];
                    }

                    if (s[1].Length == 0)
                    {
                        uint l = 800;
                        var sb = new StringBuilder((int)l);
                        if (AssocQueryString(0, 1, extension.ToLowerInvariant(), "open", sb, ref l) == 0)
                        {
                            var x = sb.ToString();
                            if (x.Contains("%1"))
                                x = x.Replace("%1", s[0]);
                            else
                                x = string.Join(" ", x.TrimEnd(), s[0]);
                            s = SplitCommandLine(x);
                            executableName = s[0];
                        }
                    }
                }
                
                var info = new System.Diagnostics.ProcessStartInfo(executableName);
                info.Arguments = s[1];
                info.UseShellExecute = !UserSettings.DoNotDisplayUI;
                info.WindowStyle = windowsStyle;

                System.Diagnostics.Process runner;
                try
                {
                    runner = System.Diagnostics.Process.Start(info);
                }
                catch (InvalidOperationException)
                {
                    info.UseShellExecute = false;
                    runner = System.Diagnostics.Process.Start(info);
                }

                if (wait && runner != null)
                {
                    if (!runner.HasExited)
                    {
                        if (!UserSettings.VersionXpaCompatible)
                            Common.RunOnContextTopMostForm(mdi => mdi.Enabled = false);
                        try
                        {
                            while (!runner.WaitForExit(1000))
                            {
                                Context.Current.DiscardPendingCommands();
                                Context.Current.Suspend(0, true);
                            }
                        }
                        finally
                        {
                            Common.RunOnContextTopMostForm(mdi =>
                            {
                                mdi.Enabled = true;
                                mdi.Activate();
                            });
                        }
                    }
                    return runner.ExitCode;
                }
                else
                    return 42;


            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "OSCommand:" + whatToRun);
                Common.WarningInStatusBar(string.Format(LocalizationInfo.Current.ErrorInStartRun, whatToRun));
                return -1;

            }
        }

        internal static string[] SplitCommandLine(string commandLine)
        {
            var result = new[] { "", "" };
            if (commandLine.Length > 0)
                if (commandLine[0] == '\"')
                {
                    var i = commandLine.IndexOf('\"', 1);
                    result[0] = commandLine.Substring(1, i - 1).TrimEnd(' ');
                    result[1] = commandLine.Substring(i + 1).Trim();
                }
                else
                {
                    result[0] = commandLine.Split(' ')[0];
                    if (commandLine.Length > result[0].Length)
                        result[1] = commandLine.Substring(result[0].Length).Trim();
                }

            return result;
        }

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint AssocQueryString(uint flags, int str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, ref uint pcchOut);
    }
}
