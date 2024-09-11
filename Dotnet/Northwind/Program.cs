using Firefly.Box;
namespace Northwind
{
    public class Program
    {
        [System.STAThread]
        public static void Main(string[] args)
        {
            try
            {
                Init(args);
                ApplicationCore.Run();
                ENV.UserSettings.FinalizeINI();
            }
            catch(System.Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "TOTAL CRASH");
                System.Environment.ExitCode = 1;
                ENV.Common.ShowExceptionDialog(e);
            }
        }
        public static void Init(string[] args)
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            ENV.Data.DataProvider.XmlEntityDataProvider.SaveAnsiAsUTF8 = true;
            ENV.UserSettings.Version10Compatible = true;
            Text.StopProcessingFormatOnCharF = true;
            ENV.Data.DateColumn.GlobalDefault = new Date(1901,1,1);
            ENV.Commands.SetDefaultKeyboardMapping();
            ENV.Commands.SetVersion10CompatibleKeyMapping();
            ENV.Common.ApplicationTitle = "Northwind";
            ENV.UserSettings.FixedBackColorInNonFlatStyles = true;
            ENV.UserSettings.InitUserSettings("Northwind.ini", args);
        }
    }
}
