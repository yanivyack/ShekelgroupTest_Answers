using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ENV.IO.Advanced;
using ENV.IO.Html;
using Firefly.Box;
using Firefly.Box.Advanced;
using Microsoft.Win32.SafeHandles;
using Writer = ENV.IO.Advanced.Writer;

namespace ENV.IO
{
    public class FileWriter : Writer, IOByName, ITemplateEnabled
    {
        #region Constructors
        public FileWriter()
        {
            _helper = new HebrewTextTools.TextWritingHelper(this);
        }
        public FileWriter(System.Text.Encoding encoding)
        {
            _helper = new HebrewTextTools.TextWritingHelper(this);
        }
        public FileWriter(string fileName)
            : this(fileName, LocalizationInfo.Current.OuterEncoding)
        {
        }

        public FileWriter(string fileName, System.Text.Encoding encoding)
            : this()
        {
            _createTextWriter = _fileWriterFactory(fileName, encoding, this);

        }
        public static Func<string, System.Text.Encoding, FileWriter, Func<ITextWriter>> _fileWriterFactory =
            (fileName, encoding, fw) => () =>
            {
                if (fileName == null || fileName.Trim() == "")
                    return null;
                return new ITextWriterBridgeToTextWriter(fileName, fw.Append, encoding);
            };

        public FileWriter(Stream s)
            : this(s, LocalizationInfo.Current.OuterEncoding)
        {
        }

        public FileWriter(Stream s, System.Text.Encoding encoding)
            : this()
        {
            _createTextWriter = () => new ITextWriterBridgeToTextWriter(s, encoding);
        }



        public FileWriter(System.IO.TextWriter writer)
            : this()
        {
            _createTextWriter = () => new ITextWriterBridgeToTextWriter(writer);

        }
        bool _apppend = false;
        public bool Append
        {
            set { _apppend = value; }
            get { return _apppend; }
        }
        #endregion

        bool _invalid = false;
        public override int LineNumber
        {
            get
            {
                if (_invalid)
                    return 0;
                return base.LineNumber;
            }
        }
        Func<ITextWriter> _createTextWriter = () => null;


        ITextWriter _writer;
        protected override void OnOpen()
        {
            try
            {
                _writer = _createTextWriter();
                if (_writer != null)
                    return;

            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "");
            }
            finally
            {
                base.OnOpen();
            }
            _invalid = true;
            _writer = new DummyITextWriter();

        }
        protected override void OnWrite(string text)
        {
            try
            {
                _writer.Write(text);
            }
            catch (Exception ex)
            {
                ENV.ErrorLog.WriteToLogFile(ex);
            }
        }
        internal string _name = null;
        public string Name { set { _name = PathDecoder.DecodePath(value); } get { return _name; } }
        #region Hebrew OEM issues

        HebrewTextTools.TextWritingHelper _helper;

        internal void SetHebrewStrategy(Func<HebrewTextTools.TextWritingHelper.WriteStrategy, HebrewTextTools.TextWritingHelper.WriteStrategy> strategyProvider)
        {
            _helper.DetermineEncodingStrategy = strategyProvider;
        }

        protected override string ProcessLine(string originalLine, int width, bool donotTrim)
        {
            return _helper.ProcessLine(originalLine, width, false);
        }

        protected override string ProcessControlData(string originalData, bool rightToLeft, bool hebrewDosCompatibleEditing)
        {
            return _helper.ProcessControlData(originalData, rightToLeft, hebrewDosCompatibleEditing);
        }

        public bool PerformRightToLeftManipulations
        {
            get { return _helper.PerformRightToLeftManipulations; }
            set { _helper.PerformRightToLeftManipulations = value; }
        }

        public bool RightToLeftFlipLine
        {
            get { return _helper.RightToLeftFlipLine; }
            set { _helper.RightToLeftFlipLine = value; }
        }

        public bool Oem
        {
            get { return _helper.Oem; }
            set { _helper.Oem = value; }
        }

        public bool OemForNonRightToLeftColumns
        {
            get { return _helper.OemForNonRightToLeftColumns; }
            set { _helper.OemForNonRightToLeftColumns = value; }
        }

        #endregion

        void ITemplateEnabled.WriteTextTemplate(Func<TemplateWriter> createTemplateWriter, Action<TemplateValues> provideTokens)
        {
            if (_template == null)
                _template = createTemplateWriter();
            var v = new TemplateValues();
            provideTokens(v);
            _template.MergeTokens(v);



        }

        TemplateWriter _template = null;

        public static FileWriter FindIOByName(Text baseStreamName)
        {
            return IOFinder.FindByName<FileWriter>(baseStreamName);
        }

        int _usagesByName = 0;
        public bool V8Compatible { get { return _helper.V8Compatible; } set { _helper.V8Compatible = value; } }

        public Action WriteWhenDone = null;
        public override void Dispose()
        {
            if (_usagesByName == 0)
            {
                if (_template != null)
                {
                    if (_writer == null)
                        OnOpen();
                    _template.WriteTo(_writer.WriteInitBytes, a => OnWrite(a));
                }
                if (_htmlContentWriter != null)
                    _htmlContentWriter.Dispose();
                if (WriteWhenDone != null)
                    WriteWhenDone();
                if (_writer != null)
                    _writer.Dispose();
                base.Dispose();

            }
            else
                _usagesByName--;
        }
        IO.Html.HtmlContentWriter _htmlContentWriter;
        internal void Write(HtmlSection htmlSection)
        {
            if (_htmlContentWriter == null)
            {
                _htmlContentWriter = new HtmlContentWriter(OnWrite);
                Open();
            }
            _htmlContentWriter.Write(htmlSection);
        }

        void IOByName.AddToUsageCount()
        {
            _usagesByName++;
        }


    }


    class DummyITextWriter : ITextWriter
    {
        public void Dispose()
        {

        }

        public void Write(string s)
        {

        }

        public void WriteInitBytes(byte[] obj)
        {

        }
    }
    public interface ITextWriter : IDisposable
    {
        void Write(string s);
        void WriteInitBytes(byte[] obj);
    }
    class ITextWriterBridgeToTextWriter : ITextWriter
    {
        System.IO.TextWriter _writer;

        public ITextWriterBridgeToTextWriter(TextWriter writer)
        {
            _writer = writer;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, int dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        public ITextWriterBridgeToTextWriter(string fileName, bool append, System.Text.Encoding encoding)
            : this(GetFileStream(fileName, append), encoding)
        {

        }

        static FileStream GetFileStream(string fileName, bool append)
        {
            var fn = Path.GetFullPath(PathDecoder.DecodePath(fileName));
            using (ENV.Utilities.Profiler.StartContext("Create file writer for " + fn + " (" + fileName + ")"))
            {
                var handle = CreateFile(fn, 0x40000000 /* Write */, 0, IntPtr.Zero, append ? 4 : 2, 0, IntPtr.Zero);
                if (handle == null || handle.ToInt32() == -1)
                {
                    var message = string.Format(
                                "Failed to create file \"{0}\" - Error: " + Marshal.GetLastWin32Error(), fn);
                    switch (Marshal.GetLastWin32Error())
                    {
                        case 5:
                            throw new UnauthorizedAccessException("UnauthorizedAccess" + message);
                        case 3:
                            throw new DirectoryNotFoundException(message);
                        case 2:
                            throw new FileNotFoundException(message);
                        case 206:
                            throw new PathTooLongException(message);
                        case 995:
                            throw new OperationCanceledException(message);
                        case 183:
                            throw new IOException("Name already exists " + message);
                        case 32:
                            throw new IOException("Sharing violation " + message);
                        case 80:
                            throw new IOException("File exist " + message);
                        case 15:
                            throw new DriveNotFoundException(message);
                        default:

                            throw new IOException(message);
                    }
                }

                var x = new FileStream(new SafeFileHandle(handle, true), FileAccess.Write);
                if (System.IO.File.Exists(fn))
                    x.Seek(0, SeekOrigin.End);
                return x;
            }
        }
        public void Test()
        {

        }

        public ITextWriterBridgeToTextWriter(Stream s, System.Text.Encoding encoding)
        {
            _writer = new StreamWriter(s, encoding);
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

        public void Write(string s)
        {
            _writer.Write(s);
        }

        public void WriteInitBytes(byte[] obj)
        {
            var x = _writer as StreamWriter;
            if (x != null)
                foreach (var b in obj)
                {
                    x.Flush();
                    x.BaseStream.WriteByte(b);
                }
        }
    }
    interface IOByName
    {
        string Name { get; }
        void AddToUsageCount();
    }
    static class IOFinder
    {
        public static type FindByName<type>(Text name) where type : class, IOByName
        {
            if (name == null)
                return null;
            string translatedName = PathDecoder.DecodePath(name.TrimEnd());
            for (int i = Context.Current.ActiveTasks.Count - 2; i >= 0; i--)
            {


                foreach (object s in GetStreams(Context.Current.ActiveTasks[i]))
                {
                    var result = s as type;
                    if (result != null)
                        if (result.Name == translatedName)
                        {
                            result.AddToUsageCount();
                            return result;
                        }
                }
            }
            return null;
        }
        public static List<IDisposable> GetStreams(ITask task)
        {
            List<IDisposable> streams = new List<IDisposable>();
            {
                var x = task as BusinessProcess;
                if (x != null)
                {
                    BusinessProcessBase result;
                    if (BusinessProcessBase._actionBusinessProcess.TryGetValue(x, out result))
                    {
                        return result.Streams;
                    }
                }
            }
            {
                var x = task as UIController;
                if (x != null)
                {
                    AbstractUIController result;
                    if (AbstractUIController._activeUIControllers.TryGetValue(x, out result))
                    {
                        return result.Streams;
                    }
                }
            }
            return streams;
        }
    }


}
