using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ENV;
using ENV.Data;
using ENV.Data.Storage;
using ENV.IO.Advanced;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Advanced;


namespace ENV
{

    public class HebrewTextTools
    {
        public static IDisposable ChangeEnviromentToV8HebrewOem()
        {
            return new MyDisposable();
        }
        static bool _visualV8Compatible = false;
        public static bool V8HebrewOem { get { return _visualV8Compatible; } }
        class MyDisposable:IDisposable
        {
            List<Action> _onDispose = new List<Action>();
            public MyDisposable()
            {

                {
                    var x = Utilities.FileViewer.TextBoxFactory;
                    FileViewer.TextBoxFactory = () => new UI.HebrewOemTextBox();
                    _onDispose.Add(() => FileViewer.TextBoxFactory = x);
                }
                {
                    var x = ENV.UserSettings.SuppressConst;
                    ENV.UserSettings.SuppressConst = true;
                    _onDispose.Add(() => ENV.UserSettings.SuppressConst = x);
                }
                {
                    var x = LocalizationInfo.Current.InnerEncoding;
                    LocalizationInfo.Current.InnerEncoding = new V8OemEncoding();
                    _onDispose.Add(() => LocalizationInfo.Current.InnerEncoding = x);
                }
                {
                    var x = LocalizationInfo.Current.OuterEncoding;
                    LocalizationInfo.Current.OuterEncoding = System.Text.Encoding.GetEncoding(1255);
                    _onDispose.Add(() => LocalizationInfo.Current.OuterEncoding = x);
                }
                {
                    var x = UserSettings.Encoding;
                    UserSettings.Encoding = new V8OemEncoding();
                    _onDispose.Add(() => UserSettings.Encoding = x);
                }
                {
                    var x = Text.TextComparer;
                    Text.TextComparer = new HebrewOemTextComparer();
                    _onDispose.Add(() => Text.TextComparer = x);
                }
                {
                    var x = UserSettings.Version8Compatible;
                    UserSettings.Version8Compatible = true;
                    _onDispose.Add(() => UserSettings.Version8Compatible = x);
                }
                {
                    var x = _visualV8Compatible;
                    _visualV8Compatible = true;
                    _onDispose.Add(() => _visualV8Compatible = x);
                }
                {
                    var x = DynamicSQLEntity.OemTextParameters;
                    DynamicSQLEntity.OemTextParameters = true;
                    _onDispose.Add(() => DynamicSQLEntity.OemTextParameters = x);
                }
                {
                    var x = PathDecoder.ResultProcessor;
                    PathDecoder.ResultProcessor = new HebrewOemPathDecoderResultProcessor();
                    _onDispose.Add(() => PathDecoder.ResultProcessor = x);

                }
                {
                    var x = UserMethods.PerformHebrewV8OemParts;
                    UserMethods.PerformHebrewV8OemParts = true;
                    _onDispose.Add(() => UserMethods.PerformHebrewV8OemParts = x);

                }
                {
                    var x = Command.Exit.Name;
                    Command.Exit.Name = "יציאה";
                    _onDispose.Add(() => Command.Exit.Name = x);

                }
                {
                    var x = Firefly.Box.Printing.Advanced.PrintDocumentPrintJob.MyPrintDocument._fixDocumentName;
                    Firefly.Box.Printing.Advanced.PrintDocumentPrintJob.MyPrintDocument._fixDocumentName = s => LocalizationInfo.Current.InnerEncoding.GetString(LocalizationInfo.Current.OuterEncoding.GetBytes(s));
                    _onDispose.Add(() => Firefly.Box.Printing.Advanced.PrintDocumentPrintJob.MyPrintDocument._fixDocumentName = x);
                }
            }
            class HebrewOemPathDecoderResultProcessor:PathDecoder.IResultProcessor
            {
                System.Text.Encoding _ascii = System.Text.Encoding.GetEncoding(1255);
                
                public string ProcessResult(string s)
                {
                    if (s == null)
                        return s;
                    return new string(_ascii.GetChars(LocalizationInfo.Current.InnerEncoding.GetBytes(s.ToCharArray())));
                }
            }


            public void Dispose()
            {
                foreach (var action in _onDispose)
                {
                    action();
                }
            }
        }

        public class V8OemEncoding : System.Text.Encoding
        {
            System.Text.Encoding _ansi = System.Text.Encoding.GetEncoding(1255);
            public override int GetByteCount(char[] chars, int index, int count)
            {
                return count;
            }



            public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
            {
                var result = _ansi.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
                FixBytes(charCount, bytes, byteIndex);
                return result;
            }

            void FixBytes(int charCount, byte[] bytes, int byteIndex)
            {
                for (int i = byteIndex; i < charCount; i++)
                {
                    if (bytes[i] >= 128 && bytes[i] <= 154)
                        bytes[i] += 96;
                    else if (bytes[i] >= 224 && bytes[i] <= 250)
                        bytes[i] -= 96;
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
                return _ansi.GetChars(ba, 0, byteCount, chars, charIndex);
            }

            public override int GetMaxByteCount(int charCount)
            {
                return charCount;
            }

            public override int GetMaxCharCount(int byteCount)
            {
                return byteCount;
            }
        }
        internal class HebrewOemTextComparer : Text.ITextComparer
        {
            ENV.UserMethods u = ENV.UserMethods.Instance;

            public bool StartsWith(string theString, string theSubstring)
            {
                return theString.StartsWith(theSubstring, StringComparison.InvariantCulture);
            }

            public int TrimAndCompare(string a, int additionalSpacesA, string b, int additionalSpacesB)
            {
                a = a.TrimEnd(' ');
                b = b.TrimEnd(' ');
                int maxLength = a.Length;
                if (b.Length > maxLength)
                    maxLength = b.Length;
                for (int i = 0; i < maxLength; i++)
                {
                    if (i >= a.Length)
                        return -1;
                    if (i >= b.Length)
                        return 1;
                    if (a[i] == b[i])
                        continue;
                    return u.Asc(a[i].ToString()).CompareTo(u.Asc(b[i].ToString()));

                }
                return 0;
            }

            public bool AreEqualTrim(string a, string b)
            {
                return string.Equals(a.TrimEnd(' '), b.TrimEnd(' '), StringComparison.InvariantCulture);

            }

            public bool AreEqualOrdinalTrim(string a, string b)
            {
                return AreEqualTrim(a, b);
            }
        }

        
        public static bool DefaultOemForNonRightToLeftColumns { get; set; }

        public static HashSet<char> _hebrewChars = new HashSet<char>(),
            _hebrewCharsWithOem = new HashSet<char>();
        public static HashSet<char> _englishChars = new HashSet<char>();
        public static HashSet<char> _numberChars = new HashSet<char>();
        public static HashSet<char> _numericOperators = new HashSet<char>();
        public static HashSet<char> _bracksChars = new HashSet<char>();
        public static HashSet<char> _otherChars = new HashSet<char>();
        public static HashSet<char> _spaceChars = new HashSet<char>();

        static HebrewTextTools()
        {


            string s = "אבגדהוזחטיכלמנסעפצקרשתךףץםן";
            _hebrewChars.AddRange(s.ToCharArray());
            _hebrewCharsWithOem.AddRange(s.ToCharArray());
            _hebrewCharsWithOem.AddRange(HebrewOemTextStorage.Encode(s).ToString().ToCharArray());


            s = "abcdefghijklmnopqrstuvwxyz";
            _englishChars.AddRange(s.ToCharArray());
            _englishChars.AddRange(s.ToUpper(CultureInfo.InvariantCulture).ToCharArray());

            s = "0123456789";
            _numberChars.AddRange(s.ToCharArray());
            _numericOperators.AddRange("-/.+*,:".ToCharArray());

            s = "[](){}<>";
            _bracksChars.AddRange(s.ToCharArray());
            _otherChars.AddRange("$%#;?/\\".ToCharArray());
            _otherChars.AddRange(s.ToCharArray());
            _spaceChars = new HashSet<char>(" ".ToCharArray());


        }



        static public Text Flip(Text text)
        {
            return text.Reverse();
        }

        internal static Text VisualFalse(Text source)
        {
            if (_visualV8Compatible)
                return new Utilities.VisualFalseV8Class(source, true).ToString();
            return new Utilities.VisualFalseClass(source,true).ToString();
        }

        public static Text VisualTrue(string source)
        {
            return new VisualTrueClass(source, true).ToString();
        }
        public static CheckChar NoChar = new CheckCharInList();
        public static string ReverseBracks(string s)
        {
            char[] chars = s.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = ReverseBracks(chars[i]);
            }
            return new string(chars);
        }

        public static char ReverseBracks(char c)
        {
            switch (c)
            {
                case '(':
                    return ')';
                case ')':
                    return '(';
                case '{':
                    return '}';
                case '}':
                    return '{';
                case '[':
                    return ']';
                case ']':
                    return '[';
                case '>':
                    return '<';
                case '<':
                    return '>';
                default:
                    return c;
            }
        }

        internal static void ExtractPrefixAndSuffix(string source, ExtractPrefixAndSuffixDelegate andDo)
        {
            ExtractPrefixAndSuffix(source, new CheckCharInList(_otherChars, _hebrewChars, _numberChars, _numericOperators, _spaceChars), new CheckCharInList(_otherChars, _hebrewChars, _spaceChars),
                                   delegate(string prefix, string middle, string suffix)
                                   {
                                       andDo(prefix, middle, suffix);
                                   });
        }
        internal delegate void ExtractPrefixAndSuffixDelegate(string prefix, string middle, string suffix);
        internal static void ExtractPrefixAndSuffix(string from, CheckChar prefixChecker, CheckChar suffixChecker, ExtractPrefixAndSuffixDelegate to)
        {
            List<char> result = new List<char>(from.ToCharArray());
            string prefix = string.Empty, suffix = string.Empty;

            foreach (char c in result.ToArray())
            {
                if (prefixChecker.Check(c))
                {
                    prefix += c;
                }
                else
                    break;
                result.RemoveAt(0);
            }
            for (int i = result.Count - 1; i >= 0; i--)
            {
                if (suffixChecker.Check(result[i]))
                {
                    suffix = result[i] + suffix;
                    result.RemoveAt(i);
                }
                else
                    break;
            }
            to(prefix, new string(result.ToArray()), suffix);
        }


        internal static bool HasOnly(IEnumerable<char> value, params HashSet<char>[] of)
        {
            foreach (char c in value)
            {
                bool found = false;
                foreach (HashSet<char> list in of)
                {
                    if (list.Contains(c))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;

            }
            return true;
        }
        internal static bool HasAny(IEnumerable<char> value, params  HashSet<char>[] of)
        {
            return HasAny(value, new CheckCharInList(of));
        }

        internal static bool HasAny(IEnumerable<char> value, CheckChar checker)
        {
            foreach (char c in value)
            {
                if (checker.Check(c))
                    return true;
            }
            return false;
        }
        static public bool EndsWith(string value, params HashSet<char>[] of)
        {
            if (value.Length == 0)
                return false;
            return HasAny(value.Substring(value.Length - 1), of);
        }
        static public bool StartsWith(string value, params HashSet<char>[] of)
        {
            if (value.Length == 0)
                return false;
            return HasAny(value[0].ToString(), of);
        }
        public static bool HasInMiddle(string value, params HashSet<char>[] of)
        {
            return HasInMiddle(value, new CheckCharInList(of));
        }

        public static bool HasInMiddle(string value, CheckChar check)
        {

            bool hadOther = false;
            bool found = false;
            bool hadAfterOther = false;
            foreach (char c in value)
            {
                if (check.Check(c))
                {
                    if (!found && hadOther)
                    {
                        found = true;
                    }
                }
                else if (!found)
                    hadOther = true;
                else
                    hadAfterOther = true;

            }
            return hadAfterOther;
        }

        internal static string Oem(string s)
        {
            return HebrewOemTextStorage.Encode(s);
        }


        public class TextWritingHelper
        {
            Writer _writer;
            public TextWritingHelper(Writer writer)
            {
                DetermineEncodingStrategy = w =>
                     {
                         if (V8Compatible)
                             if (Oem)
                                 return new V8CompatibleOem(w);
                             else
                                 return new V8CompatibleAnsi(w);
                         else if (Oem)
                             return new OemStrategy(w);
                         return w;
                     };

                _writer = writer;
                _strategy = new StrategyChooser(this);
                OemForNonRightToLeftColumns = DefaultOemForNonRightToLeftColumns;
            }
            public TextWritingHelper(Writer writer, bool oem)
                : this(writer)
            {
                Oem = oem;
                OemForNonRightToLeftColumns = DefaultOemForNonRightToLeftColumns;
            }

            bool _performRightToLeftManipulations = false;
            public bool PerformRightToLeftManipulations
            {
                get { return _performRightToLeftManipulations; }
                set
                {
                    _performRightToLeftManipulations = value;
                }
            }

            bool _rightToLeftFlipLine = false;
            public bool RightToLeftFlipLine
            {
                get { return _rightToLeftFlipLine; }
                set { _rightToLeftFlipLine = value; }
            }

            bool _oem = false;
            public bool Oem
            {
                get { return _oem; }
                set { _oem = value; }
            }
            internal interface WriteStrategy
            {
                string ProcessLine(string originalLine,int width);
                string ProcessControlData(string originalData, bool rightToLeft, bool hebrewDosCompatibleEditing);
            }

            class RightToLeftTransformStrategy : WriteStrategy
            {


                public string ProcessLine(string originalLine, int width)
                {
                    return originalLine;
                }

                public string ProcessControlData(string originalData, bool rightToLeft, bool hebrewDosCompatibleEditing)
                {
                    if (rightToLeft)
                    {
                        return new VisualTrueClass(Flip(originalData), false).ToString();
                    }
                    else
                    {
                        return originalData;
                    }
                }
            }
            class RightToLeftTransformStrategySpecialCaseInOem : WriteStrategy
            {

                public string ProcessLine(string originalLine, int width)
                {
                    return originalLine;
                }

                public string ProcessControlData(string originalData, bool rightToLeft, bool hebrewDosCompatibleEditing)
                {

                    if (rightToLeft)
                    {
                        if (hebrewDosCompatibleEditing)
                            return Flip(originalData);
                        return VisualTrue(Flip(originalData));
                    }
                    else
                        return originalData;
                }
            }
            class RightToLeftTransformWithFlipLine : WriteStrategy
            {

                public string ProcessLine(string originalLine, int width)
                {
                    return new VisualTrueClass(Flip(originalLine.PadRight(width)), false).ToString();
                }

                public string ProcessControlData(string originalData, bool rightToLeft, bool hebrewDosCompatibleEditing)
                {

                    if (!rightToLeft)
                        return ENV.LocalizationInfo.Current.InnerEncoding.GetString(ENV.LocalizationInfo.Current.OuterEncoding.GetBytes(originalData));
                    else
                        originalData = HebrewOemTextStorage.Decode(originalData);
                    if (rightToLeft||_visualV8Compatible)
                        return new VisualTrueClass(Flip(originalData), false).ToString();
                    return originalData;


                }
            }




            class FlipLine : WriteStrategy
            {

                public string ProcessLine(string originalLine, int width)
                {
                    return originalLine.TrimEnd(' ');
                }

                public string ProcessControlData(string originalData, bool rightToLeft, bool hebrewDosCompatibleEditing)
                {
                    if (rightToLeft)
                        return originalData;
                    else
                        return VisualTrue(originalData);
                }
            }
            class OemStrategy : WriteStrategy
            {
                WriteStrategy _decorated;

                public OemStrategy(WriteStrategy decorated)
                {
                    _decorated = decorated;
                }

                public string ProcessLine(string originalLine, int width)
                {

                    return _decorated.ProcessLine(originalLine, width);
                }

                public string ProcessControlData(string originalData, bool rightToLeft, bool hebrewDosCompatibleEditing)
                {
                    originalData = _decorated.ProcessControlData(originalData, rightToLeft, hebrewDosCompatibleEditing);
                    if (rightToLeft)
                        return HebrewTextTools.Oem(originalData);
                    else
                        return originalData;
                }
            }
            class SimpleStrategy : WriteStrategy
            {
                public string ProcessLine(string originalLine, int width)
                {
                    return originalLine;
                }

                public string ProcessControlData(string originalData, bool rightToLeft, bool hebrewDosCompatibleEditing)
                {
                    return originalData;
                }
            }

            class StrategyChooser : WriteStrategy
            {
                TextWritingHelper _parent;

                public StrategyChooser(TextWritingHelper parent)
                {
                    _parent = parent;
                }

                public string ProcessLine(string originalLine, int width)
                {
                    _parent.SetStrategy();
                    return _parent._strategy.ProcessLine(originalLine, width);
                }


                public string ProcessControlData(string originalData, bool rightToLeft, bool hebrewDosCompatibleEditing)
                {
                    _parent.SetStrategy();
                    return _parent._strategy.ProcessControlData(originalData, rightToLeft, hebrewDosCompatibleEditing);
                }
            }

            WriteStrategy _strategy;
            public bool V8Compatible { get; set; }
            public bool OemForNonRightToLeftColumns { get; set; }

            void SetStrategy()
            {
                if (PerformRightToLeftManipulations)
                {
                    if (RightToLeftFlipLine)
                        _strategy = new RightToLeftTransformWithFlipLine();
                    else
                        if (Oem)
                            _strategy = new RightToLeftTransformStrategySpecialCaseInOem();
                        else
                            _strategy = new RightToLeftTransformStrategy();
                }
                else
                {
                    if (RightToLeftFlipLine)
                        _strategy = new TextWritingHelper.FlipLine();
                    else
                        _strategy = new SimpleStrategy();
                }
                _strategy = DetermineEncodingStrategy(_strategy);
            }

            internal Func<WriteStrategy, WriteStrategy> DetermineEncodingStrategy;
            class V8CompatibleOem : WriteStrategy
            {
                WriteStrategy _delegated;

                System.Text.Encoding _oem = ENV.LocalizationInfo.Current.InnerEncoding,
                                     _ansi = ENV.LocalizationInfo.Current.OuterEncoding;
                public V8CompatibleOem(WriteStrategy delegated)
                {
                    _delegated = delegated;
                }

                public string ProcessLine(string originalLine, int width)
                {
                    return _delegated.ProcessLine(originalLine, width);
                }

                public string ProcessControlData(string originalData, bool rightToLeft, bool hebrewDosCompatibleEditing)
                {
                    var a = _delegated.ProcessControlData(originalData, rightToLeft, hebrewDosCompatibleEditing);
                    var bytes = _oem.GetBytes(a.ToCharArray());
                    if (rightToLeft)
                    {
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            bytes[i] = HebrewOemTextStorage.EncodeByte(bytes[i]);
                        }
                    }
                    return new string(_ansi.GetChars(bytes));
                }
            }
            class V8CompatibleAnsi : WriteStrategy
            {
                WriteStrategy _delegated;

                System.Text.Encoding _oem = ENV.LocalizationInfo.Current.InnerEncoding,
                                     _ansi = ENV.LocalizationInfo.Current.OuterEncoding;
                public V8CompatibleAnsi(WriteStrategy delegated)
                {
                    _delegated = delegated;
                }

                public string ProcessLine(string originalLine, int width)
                {
                    return _delegated.ProcessLine(originalLine, width);
                }

                public string ProcessControlData(string originalData, bool rightToLeft, bool hebrewDosCompatibleEditing)
                {
                    if (rightToLeft)
                    {
                        var a = _delegated.ProcessControlData(originalData, rightToLeft, hebrewDosCompatibleEditing);
                        var bytes = _oem.GetBytes(a.ToCharArray());
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            if (bytes[i] >= 128 && bytes[i] <= 157)
                                bytes[i] += 224 - 128;
                            else if (bytes[i] == 158)
                                bytes[i] = 142;
                            else if (bytes[i] == 159)
                                bytes[i] = 143;
                            else if (bytes[i] == 177)
                                bytes[i] = 129;
                        }
                        return new string(_ansi.GetChars(bytes));
                    }
                    else
                    {
                        var a = _delegated.ProcessControlData(originalData, rightToLeft, hebrewDosCompatibleEditing);

                        a = new VisualTrueClass(Flip(a), false).ToString();
                        return new string(_ansi.GetChars(_oem.GetBytes(a.ToCharArray())));
                    }

                }
            }


            public string ProcessLine(string originalLine,int width,bool donotTrim)
            {

                var result = _strategy.ProcessLine(originalLine, width);
                if (_writer.AutoNewLine&&!donotTrim)
                {
                    result =  result.TrimEnd(' ');
                    if (result.Length == 0 && UserSettings.Version8Compatible && !DoNotWriteSpaceInEmptyLine)
                        result = " ";
                    if (_performRightToLeftManipulations&&_rightToLeftFlipLine)
                        result = result.PadRight(width);
                }
                return result;

            }
            public string ProcessControlData(string originalData, bool rightToLeft,bool hebrewDosCompatibleEditing)
            {
                var result = _strategy.ProcessControlData(originalData, rightToLeft, hebrewDosCompatibleEditing);
                if (!rightToLeft && Oem && OemForNonRightToLeftColumns)
                    result = HebrewTextTools.Oem(result);
                return result;
            }
        }

        public static bool DoNotWriteSpaceInEmptyLine { get; set; }
    }
    
    enum CharTypeEnum
    {
        Other,
        Number,
        English,
        Hebrew,
        Bracks,
        Space,
        None
    }
    class CheckCharInList : CheckChar
    {
        List<HashSet<Char>> _lists;

        public CheckCharInList(params HashSet<char>[] lists)
        {
            _lists = new List<HashSet<char>>(lists);
        }

        public bool Check(char c)
        {
            foreach (HashSet<char> list in _lists)
            {
                if (list.Contains(c))
                    return true;
            }
            return false;
        }
    }

    class CheckCharNotInList : CheckChar
    {
        List<HashSet<Char>> _lists;

        public CheckCharNotInList(params HashSet<char>[] lists)
        {
            _lists = new List<HashSet<char>>(lists);
        }

        public bool Check(char c)
        {
            foreach (HashSet<char> list in _lists)
            {
                if (list.Contains(c))
                    return false;
            }
            return true;
        }
    }

    class CheckCharAndCharAfterChar : CheckChar
    {
        CheckCharInList _main, _charsThatAreAfterIfTheyWereAfter;

        public CheckCharAndCharAfterChar(HashSet<char> mainChars, HashSet<char> charsThatAreOkIfTheyWereAfter)
        {
            _main = new CheckCharInList(mainChars);
            _charsThatAreAfterIfTheyWereAfter = new CheckCharInList(charsThatAreOkIfTheyWereAfter);
        }
        bool _lastCharWasOk = false;

        public bool Check(char c)
        {
            if (_main.Check(c))
            {
                _lastCharWasOk = true;
                return true;
            }
            if (_lastCharWasOk && _charsThatAreAfterIfTheyWereAfter.Check(c))
            {
                return true;
            }
            _lastCharWasOk = false;
            return false;
        }
    }

    public interface CheckChar
    {
        bool Check(char c);
    }

    static class HashSetHelper
    {
        public static void AddRange(this HashSet<char> x, char[] args)
        {
            foreach (var c in args)
            {
                x.Add(c);
            }
        }
    }

}