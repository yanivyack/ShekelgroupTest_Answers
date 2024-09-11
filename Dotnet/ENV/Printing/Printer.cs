using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ENV.IO.Advanced;

namespace ENV.Printing
{
    public class Printer
    {

        public Printer()
        {

        }
        public Printer(string printerName)
        {
            Name = printerName;
        }

        public string Name { get; set; }

        /// <summary>
        /// The name of the printer, in the windows print spool.
        /// </summary>
        /// <remarks>
        /// When empty, the default printer is used.
        /// </remarks>
        public string PrinterName
        {
            get
            {
                if (_printerName==null)
                    return PathDecoder.DecodePath(UserSettings.GetPrinterName(Name).PrinterName);
                return _printerName;
            }
            set
            {
                _printerName = PathDecoder.DecodePath(value);
            }

        }

        string _printerName;
        public void LoadAtr(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                    LoadAtr(new StreamReader(fileName, LocalizationInfo.Current.OuterEncoding));
            }
            catch
            {

            }
        }


        internal static string FromHexaString(string source)
        {
            if (source.Length == 0)
                return source;
            List<char> result = new List<char>();
            foreach (string s in source.Split(','))
            {
                result.Add((char)int.Parse(s.Substring(2).Trim(), System.Globalization.NumberStyles.AllowHexSpecifier));
            }
            return new string(result.ToArray());
        }

        public void LoadAtr(TextReader sr)
        {
            ClearTextPrintingTokens();
            using (sr)
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    try
                    {
                        string[] parts = line.Split('=');
                        string key = parts[0];
                        parts = parts[1].Split(':');
                        string prefix = parts[0];
                        string suffix = parts.Length == 2 ? parts[1] : string.Empty;
                        AddTextPrintingToken(key, FromHexaString(prefix), FromHexaString(suffix));
                    }
                    catch
                    {
                    }
                }
            }
        }
        /// <summary>
        /// Clears the tokens added by the <see cref="AddTextPrintingToken"/> method.
        /// </summary>
        public void ClearTextPrintingTokens()
        {
            _tags.Clear();
        }
        /// <summary>
        /// Adds a token that will be used in conjunction with <see cref="TextPrintingStyle"/> to mark certain items of text.
        /// </summary>
        /// <param name="key">The key that identifies this token</param>
        /// <param name="prefix">Text to be added before a string that contains this content</param>
        /// <param name="suffix">Text to add after a string that contains this content</param>
        public void AddTextPrintingToken(string key, string prefix, string suffix)
        {
            _tags.Add(key, setPrintTag => setPrintTag(prefix, suffix));
        }

        bool _loaded = false;
        internal void SetPrintTag(string key, SetPrintTagDelegate action)
        {
            CheckLoaded();
            Action<SetPrintTagDelegate> result;
            if (!_tags.TryGetValue(key, out result))
                action(string.Empty, string.Empty);
            else
                result(action);
        }

        void CheckLoaded()
        {
            if (!_loaded)
            {
                lock (this)
                {
                    if (!_loaded)
                    {
                        LoadAtr(PathDecoder.DecodePath(UserSettings.GetPrinterName(Name).AttributeFileName));
                        _loaded = true;
                    }
                }
            }
        }

        internal delegate void SetPrintTagDelegate(string prefix, string suffix);
        Dictionary<string, Action<SetPrintTagDelegate>> _tags = new Dictionary<string, Action<SetPrintTagDelegate>>();

        int _linesPerTextPage = -1;

        /// <summary>
        /// The number of lines of a text page. 
        /// </summary>
        /// <remarks>When using this <this/> as the printer of a <see cref="TextPrinterWriter"/>, after this number of lines, a form feed will be sent to the printer to open a new page.</remarks>
        public int LinesPerTextPage
        {
            get
            {
                if (_linesPerTextPage >= 0)
                    return _linesPerTextPage;
                var x = UserSettings.GetPrinterName(Name).LinesPerTextPage;
                if (x >= 0)
                    return x;
                return 60;
            }
            set { _linesPerTextPage = value; }
        }
        public bool Oem { get; set; }

        public void Decorate(StringBuilder sb,List<string> printTags, string s)
        {
            Decorate(sb, printTags, s, false, false);
        }

        public void Decorate(StringBuilder sb, List<string> printTags, string s, bool ignorePrefix, bool ignoreSuffix)
        {
            CheckLoaded();
            StringBuilder val = null;
            for (int i = 0; i < printTags.Count; i++)
            {
                var printTag = printTags[i];
                Action<SetPrintTagDelegate> result;
                if (_tags.TryGetValue(printTag, out result))
                {
                    result((prefix, suffix) =>
                    {
                        if (!ignorePrefix)
                            sb.Append(prefix);
                        if (val == null)
                            val = new StringBuilder(s);
                        if (!ignoreSuffix)
                            val.Append(suffix);
                    });
                }
            }
            
            if (val == null)
                sb.Append(s);
            else
                sb.Append(val);
        }

        internal  void ApplyEncodingTo(TextPrinterWriter textPrinterWriter)
        {
            var x =PathDecoder.DecodePath( UserSettings.GetPrinterName(Name).CustomEncodingFile);
            if (!string.IsNullOrEmpty(x) && System.IO.File.Exists(x))
            {
                try
                {
                    var enc = new CustomEncoding(x);
                    textPrinterWriter.Encoding = enc;
                }
                catch (Exception ex)
                {
                    ErrorLog.WriteToLogFile(ex,"failed to load custom printer encoding");
                }
            }

        }
    }
    public class CustomEncoding : System.Text.Encoding
    {
        System.Text.Encoding _original = LocalizationInfo.Current.OuterEncoding;

        public CustomEncoding(string fileName):this(new StreamReader(fileName))
        {
          
        }

        int _maxBytes = 1;
        public CustomEncoding(TextReader tr)
        {
            using (var sr = tr)
            {
                string l;
                while ((l = sr.ReadLine()) != null)
                {
                    var s = l.Trim().Split('=');
                    if (s.Length == 2)
                    {
                        var list = new List<byte>();
                        foreach (var z in  s[1].Split(','))
                        {
                            list.Add(Parse(z));
                        }

                        if (list.Count>_maxBytes)
                            _maxBytes = list.Count;
                        try
                        {
                            if (_toBytes.ContainsKey(Parse(s[0])))
                                _toBytes[Parse(s[0])] = list;
                            else
                                _toBytes.Add(Parse(s[0]), list);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
        }

        byte Parse(string s)
        {
            return byte.Parse(s.Substring(2), System.Globalization.NumberStyles.HexNumber);
        }

        Dictionary<byte, List<byte>> _toBytes = new Dictionary<byte, List<byte>>();


        public override int GetByteCount(char[] chars, int index, int count)
        {
            var bytes = _original.GetBytes(chars);

            foreach (var item in bytes)
            {
                List<byte> r;
                if (_toBytes.TryGetValue(item, out r))
                    count += r.Count - 1;
            }
            return count;
        }



        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            var result = _original.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
            int i = byteIndex;
            while (i < result)
            {
                List<byte> ba;
                if (_toBytes.TryGetValue(bytes[i], out ba))
                {
                    for (int j = 0; j < ba.Count; j++)
                    {
                        if (j > 0)
                        {
                            for (int k = bytes.Length-2; k >i ; k--)
                            {
                                bytes[k + 1] = bytes[k];
                            }
                            i++;
                            result++;
                        }
                        bytes[i] = ba[j];
                    }
                    
                }
                i++;
            }
            return result;
        }

        void FixBytes(int charCount, byte[] bytes, int byteIndex)
        {
            for (int i = byteIndex; i < charCount; i++)
            {
                List<byte> newByte;
                if (_toBytes.TryGetValue(bytes[i], out newByte))
                    bytes[i] = newByte[0];

            }
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            var ba = new byte[byteCount];
            for (int i = byteIndex; i < byteCount; i++)
            {
                ba[i - byteIndex] = bytes[i];
            }
            FixBytes(byteCount, ba, 0);
            return _original.GetChars(ba, 0, byteCount, chars, charIndex);
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount*_maxBytes;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }
    }
}

