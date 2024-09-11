using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using ENV.BackwardCompatible;
using ENV.UI;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data
{
    public class ByteArrayColumn : Firefly.Box.Data.ByteArrayColumn, IENVColumn
    {
        public ByteArrayColumn(string name, string caption)
            : base(name, caption)
        {
            _expand = new ColumnHelper(this);
        }
        public ByteArrayColumn(string name)
            : this(name, null)
        {
            _expand = new ColumnHelper(this);
        }

        public ByteArrayColumn()
            : this(null)
        {
            _expand = new ColumnHelper(this);
        }
        void IENVColumn.EnterOnControl()
        {
            _expand.EnterOnControl();
        }
        internal static ByteArrayColumn _byteParameterConverter = new ByteArrayColumn { ContentType = ByteArrayColumnContentType.BinaryAnsi };
        public CustomHelp CustomHelp { get; set; }
        public bool AutoExpand { get { return _expand.AutoExpand; } set { _expand.AutoExpand = value; } }
        bool IENVColumn._internalReadOnly { get; set; }
        public bool ReadOnlyForExistingRows { get; set; }

        public Type ControlType { get; set; }
        public Type ControlTypeOnGrid { get; set; }
        public Type ControlTypePrinting { get; set; }
        public Type ControlTypePrintingOnGrid { get; set; }
        void IENVColumn.UpdateParameterReturnValue(Action performUpdate)
        {
            _expand.UpdateParameterReturnValue(performUpdate);
        }
        internal bool _wasStorageSet;
        public override IColumnStorageSrategy<byte[]> Storage
        {
            get
            {
                return base.Storage;

            }

            set
            {
                _wasStorageSet = true;
                base.Storage = value;
            }
        }
        void IENVColumn.InternalPerformExpandOperation(Action expand)
        {
            _expand.InternalPerformExpandOperation(expand);
        }
        public override byte[] Value
        {
            get
            {
                return base.Value;
            }
            set
            {

                if (!Common.OKToUpdateColumn(this))
                    base.Value = value;
            }
        }
        INullStrategy _nullStrategy = NullStrategy.GetStrategy(false);
        void IENVColumn.SetNullStrategy(INullStrategy instance)
        {
            _nullStrategy = instance;
            if (_uInstance != null)
                _nullStrategy.ApplyTo(_uInstance);
        }
        UserMethods _uInstance;
        protected UserMethods u
        {
            get
            {
                if (_uInstance == null)
                {
                    _uInstance = new UserMethods();
                    _nullStrategy.ApplyTo(_uInstance);
                }
                return _uInstance;
            }
        }
        public bool CompareForNullValue(object previousValue)
        {
            return true;
        }

        public override byte[] FromString(Firefly.Box.Data.TextColumn column)
        {
            var c = column as ENV.Data.TextColumn;
            if (column != null)

                if (ContentType == ByteArrayColumnContentType.BinaryUnicode)
                {
                    if (c.StorageType == TextStorageType.Unicode)
                        return ToUnicodeByteArray(c.Value);
                    return ToAnsiByteArray(c.Value);

                }
                else if (ContentType == ByteArrayColumnContentType.Unicode)
                {
                    {
                        return ToUnicodeByteArray(c.Value);
                    }
                }
            return FromString(column.Value);
        }
        /// <summary>
        /// Performs both the BindValue method and the IsEqualTo
        /// </summary>
        /// <param name="to">The bind expression or value</param>
        /// <returns>The IsEqualTo Fitler</returns>
        public FilterBase BindEqualTo(byte[] to)
        {
            BindValue(() => to);
            return IsEqualTo(to);
        }
        /// <summary>
        /// Performs both the BindValue method and the IsEqualTo
        /// </summary>
        /// <param name="to">The bind expression or value</param>
        /// <returns>The IsEqualTo Fitler</returns>
        public FilterBase BindEqualTo(TypedColumnBase<byte[]> to)
        {
            BindValue(to);
            return IsEqualTo(to);
        }
        /// <summary>
        /// Performs both the BindValue method and the IsEqualTo
        /// </summary>
        /// <param name="to">The bind expression or value</param>
        /// <returns>The IsEqualTo Fitler</returns>
        public FilterBase BindEqualTo(Func<byte[]> to)
        {
            BindValue(to);
            return IsEqualTo(to);
        }
        public static byte[] ToAnsiByteArray(string s)
        {
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms, LocalizationInfo.Current.OuterEncoding))
                {
                    sw.Write(s);
                }
                return ms.ToArray();
            }
        }

        static public byte[] ToUnicodeByteArray(string value)
        {

            return UnicodeWithoutGaps.GetBytes(value.ToCharArray());

        }
        static public byte[] ToUnicodeBOMByteArray(string value)
        {
            return ToUnicodeBOMByteArray(ToUnicodeByteArray(value));
        }
        static public byte[] ToUnicodeBOMByteArray(byte[] unicodeByteArray)
        {
            var result = new byte[unicodeByteArray.Length + 2];
            result[0] = 255;
            result[1] = 254;
            unicodeByteArray.CopyTo(result, 2);
            return result;

        }

        public byte[] JoinByteArray(params object[] args)
        {
            if (args == null)
                if (_nullStrategy.DoInsteadOfNull())
                    return new byte[0];
                else
                    return null;
            var result = new List<byte[]>();
            int length = 0;
            foreach (var item in args)
            {
                var s = PrivateToByteArray(item);
                if (s == null)
                    if (_nullStrategy.DoInsteadOfNull())
                        s = new byte[0];
                    else
                        return null;
                if (s.Length > 0)
                {
                    result.Add(s);
                    length += s.Length;
                }
            }

            var resultArray = new byte[length];
            int lastPosition = 0;
            foreach (var item in result)
            {
                item.CopyTo(resultArray, lastPosition);
                lastPosition += item.Length;
            }
            return AppendSuffixIfNeeded(resultArray);
        }
        byte[] PrivateToByteArray(object o)
        {
            if (o == null)
                if (_nullStrategy.DoInsteadOfNull())
                    return new byte[0];
            {
                var bac = o as ByteArrayColumn;
                if (bac != null)
                    return ToByteArray(bac);
            }
            {
                var ba = o as byte[];
                if (ba != null)
                {
                    return ToByteArray(u.ToText(ba));//based on the way we previously generated the expression
                }
            }
            {
                Text t;
                if (Text.TryCast(o, out t))
                {
                    return InternalFromString(t);
                }
            }
            return Cast(o);
        }

        public byte[] ToByteArray(string value)
        {
            return FromString(value);
        }
        public byte[] ToByteArray(ByteArrayParameter value)
        {
            return value.GetValue(this);
        }
        public byte[] ToByteArray<T>(ArrayColumn<T> column)
        {
            return column.ToByteArray();
        }
        public byte[] ToByteArray<T>(T[] value)
        {
            return UserMethods.TypedArrayToByteArray(value, null);
        }
        public byte[] ToByteArray(Firefly.Box.Data.TextColumn value)
        {
            return FromString(value);
        }
        public byte[] ToByteArray(TypedColumnBase<byte[]> value)
        {
            var z = value as ByteArrayColumn;
            if (z != null)
                return ToByteArray(z);
            return value;
        }

        public byte[] ToByteArray(byte[] value)
        {
            return value;
        }
        internal ByteArrayColumn _boundParameterColumnForXmlDataSource;
        public byte[] ToByteArray(Firefly.Box.Data.ByteArrayColumn value)
        {
            _boundParameterColumnForXmlDataSource = null;
            var z = value as ByteArrayColumn;
            if (z != null)
            {
                _boundParameterColumnForXmlDataSource = z;
                return ToByteArray(z);
            }
            return value;
        }
        public byte[] ToByteArray(ENV.Data.ByteArrayColumn value)
        {
            if (ContentType == ByteArrayColumnContentType.Ansi &&
               value.ContentType == ByteArrayColumnContentType.Unicode ||
               ContentType == ByteArrayColumnContentType.Unicode &&
               value.ContentType == ByteArrayColumnContentType.Ansi)
                return FromString(value.ToString());
            return value;
        }


        public override byte[] FromString(string value)
        {
            if (value == null)
                return null;
            var result = InternalFromString(value);
            return AppendSuffixIfNeeded(result);
        }
        private byte[] InternalFromString(string value)
        {
            if (value == null)
                return null;
            byte[] result;
            var contentType = _contentType;
            if (value.StartsWith("{\\rtf1") && contentType == ByteArrayColumnContentType.BinaryUnicode)
                contentType = ByteArrayColumnContentType.Ansi;

            switch (contentType)
            {
                case ByteArrayColumnContentType.BinaryUnicode:
                    result = ToUnicodeByteArray(value);
                    break;
                case ByteArrayColumnContentType.Unicode:
                    result = ToUnicodeByteArray(value);
                    break;
                case ByteArrayColumnContentType.BinaryAnsi:
                case ByteArrayColumnContentType.Ansi:
                    result = ENV.LocalizationInfo.Current.OuterEncoding.GetBytes(value.ToString().ToCharArray());
                    break;
                default:
                    throw new NotImplementedException();
            }
            return result;
        }

        private byte[] AppendSuffixIfNeeded(byte[] result)
        {
            if (_contentType == ByteArrayColumnContentType.BinaryAnsi&&!ForceAnsi)
            {
                var r2 = new byte[result.Length + 1];
                Array.Copy(result, r2, result.Length);
                r2[result.Length] = (byte)0;
                return r2;
            }
            return
                result;
        }

        public byte[] Cast(object value)
        {
            {
                var v = value as Firefly.Box.Data.ByteArrayColumn;
                if (v != null)
                    return v.Value;
            }
            {
                var v = value as byte[];
                if (v != null)
                    return v;
            }
            {
                Text t;
                if (Text.TryCast(value, out t))
                    return FromString(t);
            }
            return null;

        }
        protected override byte[] FromUIInputString(string value)
        {
            if (value == null)
                return null;
            switch (_contentType)
            {

                case ByteArrayColumnContentType.Unicode:
                    return UnicodeWithoutGaps.GetBytes(value);
                case ByteArrayColumnContentType.BinaryUnicode:
                case ByteArrayColumnContentType.BinaryAnsi:
                case ByteArrayColumnContentType.Ansi:
                    return ToAnsiByteArray(value);
                default:
                    throw new NotImplementedException();
            }
        }
        public string ToStringOrNull() {
            if (this.Value == null || this.Value.Length == 0)
                return null;
            return u.RTrim(ToStringForRichTextBox());
        }
        protected override string ToStringForRichTextBox(byte[] value)
        {
            try
            {

                if (value == null)
                    return null;
                if (_contentType == ByteArrayColumnContentType.BinaryAnsi)
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (value[i] == 0)
                        {
                            var x = new byte[i];
                            Array.Copy(value, x, i);
                            value = x;
                        }
                    }


                switch (_contentType)
                {

                    case ByteArrayColumnContentType.Unicode:

                        return new string(UnicodeWithoutGaps.GetChars(value));
                    case ByteArrayColumnContentType.BinaryUnicode:
                    case ByteArrayColumnContentType.BinaryAnsi:
                    case ByteArrayColumnContentType.Ansi:

                        return AnsiByteArrayToString(value);
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }

        public override string ToString(byte[] value)
        {
            var result = InternalToString(value);
            if (result == null)
                return null;
            int nullLocation = result.IndexOf('\0');
            if (nullLocation >= 0&&!ForceAnsi)
                result = result.Remove(nullLocation);
            if (result.StartsWith("{\\rtf1"))
            {
                var x = result;
                result = RtfToString(result);
                if (result == "")
                    if (x.EndsWith(@"\par }"))
                        return "\n";
                    else return "\r\n";
            }
            return result;
        }


        static System.Windows.Forms.RichTextBox _richTextBox;
        static object rtbSync = new object();
        static string RtfToString(string rtf)
        {
            lock (rtbSync)
            {
                if (_richTextBox == null)
                    _richTextBox = new RichTextBoxV5();
                _richTextBox.Rtf = rtf;
                var r = new StringBuilder(_richTextBox.Text);
                for (int i = r.Length - 1; i >= 0; i--)
                {
                    if (r[i] == '\n' && (i == 0 || r[i - 1] != '\r'))
                    {

                        r = r.Insert(i, "\r");
                    }
                }
                return r.ToString();
            }
        }
        class RichTextBoxV5 : System.Windows.Forms.RichTextBox
        {
            [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            static extern IntPtr LoadLibrary(string lpFileName);

            static IntPtr _msfteditHandle = IntPtr.Zero;
            static bool _msfteditNotFound;

            protected override System.Windows.Forms.CreateParams CreateParams
            {
                get
                {
                    var p = base.CreateParams;
                    if (p.ClassName != "RICHEDIT50W")
                    {
                        if (_msfteditHandle == IntPtr.Zero && !_msfteditNotFound)
                        {
                            _msfteditHandle = LoadLibrary("msftedit.dll");
                            if ((long)_msfteditHandle < 32L)
                                _msfteditNotFound = true;
                        }
                        if (!_msfteditNotFound)
                            p.ClassName = "RichEdit50W";
                        if (BackColor == System.Drawing.Color.Transparent)
                            p.ExStyle |= 0x20;
                    }
                    return p;
                }
            }

            public RichTextBoxV5()
            {
                SetStyle(System.Windows.Forms.ControlStyles.SupportsTransparentBackColor, true);
            }
        }


        string InternalToString(byte[] value)
        {
            try
            {

                if (value == null)
                    return null;
                if (_contentType == ByteArrayColumnContentType.BinaryAnsi&&!ForceAnsi)
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (value[i] == 0)
                        {
                            var x = new byte[i];
                            Array.Copy(value, x, i);
                            value = x;
                        }
                    }


                switch (_contentType)
                {

                    case ByteArrayColumnContentType.Unicode:
                        return new string(UnicodeWithoutGaps.GetChars(value));
                    case ByteArrayColumnContentType.BinaryAnsi:
                    case ByteArrayColumnContentType.Ansi:
                    case ByteArrayColumnContentType.BinaryUnicode:
                        return AnsiByteArrayToString(value);
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }

        internal static string AnsiByteArrayToString(byte[] value)
        {
            return new string(ENV.LocalizationInfo.Current.OuterEncoding.GetChars(value));
        }
        public static bool ForceAnsi = false;// when set to true matches `SpecialAnsiExpression`
        ByteArrayColumnContentType _contentType = UserSettings.Version10Compatible &&!ForceAnsi?
            ByteArrayColumnContentType.BinaryUnicode :
            ByteArrayColumnContentType.BinaryAnsi;
        public ByteArrayColumnContentType ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }
        public static implicit operator Text(ByteArrayColumn c)
        {
            return c.ToString();
        }
        public static implicit operator string(ByteArrayColumn c)
        {
            return c.ToString();
        }
        public static Bool operator ==(ByteArrayColumn a, Text b)
        {
            if (ReferenceEquals(b, null) && !ReferenceEquals(a, null))
                return false;
            if (ReferenceEquals(a, null) || ReferenceEquals(a.Value, null) || ReferenceEquals(b, null))
                return null;
            return a.ToString() == b;
        }
        public static Bool operator >=(ByteArrayColumn a, Text b)
        {
            if (ReferenceEquals(a, null) || ReferenceEquals(a.Value, null) || ReferenceEquals(b, null))
                return null;
            return a.ToString() >= b;
        }
        public static Bool operator <=(ByteArrayColumn a, Text b)
        {
            if (ReferenceEquals(a, null) || ReferenceEquals(a.Value, null) || ReferenceEquals(b, null))
                return null;
            return a.ToString() <= b;
        }
        public static Bool operator >(ByteArrayColumn a, Text b)
        {
            if (ReferenceEquals(a, null) || ReferenceEquals(a.Value, null) || ReferenceEquals(b, null))
                return null;
            return a.ToString() > b;
        }
        public static Bool operator <(ByteArrayColumn a, Text b)
        {
            if (ReferenceEquals(a, null) || ReferenceEquals(a.Value, null) || ReferenceEquals(b, null))
                return null;
            return a.ToString() < b;
        }

        public static Bool operator !=(ByteArrayColumn a, Text b)
        {
            if (ReferenceEquals(b, null) && !ReferenceEquals(a, null))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(a.Value, null) || ReferenceEquals(b, null))
                return null;
            return a.ToString() != b;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        ColumnHelper _expand;
        public new event Action Expand
        {
            add { _expand.Expand += value; }
            remove { _expand.Expand -= value; }
        }
        /// <summary>
        /// Only used for the UserMethods.ControlSelectProgram method - backward compatability only
        /// </summary>
        public Type ExpandClassType { set { _expand.ExpandClassType = value; } get { return _expand.ExpandClassType; } }
        public void ClearExpandEvent()
        {
            _expand.ClearExpandEvent();
        }
        public string StatusTip { get; set; }
        public string ToolTip { get; set; }
        bool IENVColumn.IsParameter { get; set; }
        internal static System.Text.Encoding UnicodeWithoutGaps = new UnicodeEncodingWithoutGaps();
        internal class UnicodeEncodingWithoutGaps : System.Text.Encoding
        {
            System.Text.Encoding _unicode = System.Text.Encoding.Unicode;
            public override int GetByteCount(char[] chars, int index, int count)
            {
                return count * 2;
            }


            const char errorChar = (char)65533;
            const byte error1 = 255, error2 = 253;
            public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
            {
                var result = _unicode.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
                for (int i = 0; i < bytes.Length; i += 2)
                {
                    if (bytes[i + 1] == error1 && bytes[i] == error2)
                    {
                        int c = chars[i / 2];
                        bytes[i] = (byte)(c % 256);
                        bytes[i + 1] = (byte)(c / 256);
                    }
                }
                return result;
            }

            public override string WebName
            {
                get { return _unicode.WebName; }
            }


            public override int GetCharCount(byte[] bytes, int index, int count)
            {
                return count / 2;
            }

            public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
            {
                var result = _unicode.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
                for (int i = 0; i < chars.Length; i++)
                {
                    if (chars[i] == errorChar)
                    {
                        chars[i] = (char)((bytes[i * 2 + 1]) * 256 + bytes[i * 2]);
                    }
                }

                return result;
            }

            public override int GetMaxByteCount(int charCount)
            {
                return charCount * 2;
            }

            public override int GetMaxCharCount(int byteCount)
            {
                return byteCount / 2;
            }

        }
        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }
        public static implicit operator ArrayParameter<Text>(ByteArrayColumn col)
        {
            return new ArrayParameter<Text>(c =>
                ((ArrayColumn<Text>)c).FromByteArray(col)
            , (c, val) =>
            {
                col.Value = ((ArrayColumn<Text>)c).ToByteArray();
            }, col);
        }
        public Text FullCaption { get { return UserMethods.InternalVarName(this); } }
        public Text FullDbName { get { return UserMethods.InternalVarDbName(this); } }
        public Bool WasChanged { get { return UserMethods.InternalVarMod(this); } }
        object IENVColumn._internalValueChangeStore { get; set; }

    }

    public enum ByteArrayColumnContentType
    {
        BinaryAnsi,
        BinaryUnicode,
        Unicode,
        Ansi
    }
}