using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ENV.Utilities
{
    public class FileBasedLock : System.IDisposable
    {

        System.IO.StreamWriter _sw;
        string _fileName;

        public bool Lock(string fileName, string message, string title)
        {
            _fileName = ENV.PathDecoder.DecodePath(fileName);
            if (PerTerminal)
            {
                var x = ENV.UserMethods.Instance.Term();
                if (x == 0)
                    return true;
                _fileName += x.ToString();
            }
            try
            {
                _sw = new System.IO.StreamWriter(_fileName);
                return true;
            }
            catch (Exception ex)
            {
                ENV.Common.ShowMessageBox(title, System.Windows.Forms.MessageBoxIcon.Error, message);
                return false;
            }
        }

        public FileBasedLock()
        {

        }
        public bool PerTerminal { get; set; }


        public void Dispose()
        {
            if (_sw != null)
            {
                _sw.Dispose();
                try
                {
                    System.IO.File.Delete(_fileName);
                }
                catch { }
            }
        }
    }
}
