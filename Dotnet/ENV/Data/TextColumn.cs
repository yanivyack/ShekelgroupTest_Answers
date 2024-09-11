using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using ENV.BackwardCompatible;
using ENV.Data.Storage;
using ENV.UI;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.Advanced;

namespace ENV.Data
{
    public class TextColumn : Firefly.Box.Data.TextColumn, IENVColumn, SqlScriptGenerator.IColumn, ControllerBase.ParameterColumn
    {

        public TextColumn(string name, string format, string caption)
            : base(name, format, caption)
        {
            Init();
        }
        public static TextStorageType DefaultStorageType = TextStorageType.Ansi;
        void Init()
        {
            _expand = new ColumnHelper(this);
            StorageType = DefaultStorageType;
            UseBackslashToDistinguishWildcards = DefaultUseBackslashToDistinguishWildcards;
            _defaultWasSet = false;
        }

        public TextColumn(string name, string format)
            : base(name, format)
        {
            Init();
        }

        public TextColumn(string name)
            : base(name)
        {
            Init();
        }

        public TextColumn()
        {
            Init();
        }
        void IENVColumn.EnterOnControl()
        {
            _expand.EnterOnControl();
        }
        internal bool _defaultWasSet;
        public override Text DefaultValue
        {
            get
            {
                return base.DefaultValue;
            }

            set
            {
                _defaultWasSet=true;
                base.DefaultValue = value;
            }
        }


        public static bool ForceIncrementalSearchWhenCurrentValueMeetsCriteriaDefault = false;
        protected override bool ForceIncrementalSearchWhenCurrentValueMeetsCriteria()
        {
            return ForceIncrementalSearchWhenCurrentValueMeetsCriteriaDefault;
        }

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

        void IENVColumn.InternalPerformExpandOperation(Action expand)
        {
            _expand.InternalPerformExpandOperation(expand);
        }
        public override Text Value
        {
            get { return base.Value; }
            set
            {
                if (!Common.OKToUpdateColumn(this))
                    base.Value = value;
            }
        }

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

        bool IENVColumn.CompareForNullValue(object previousValue)
        {
            return _nullStrategy.CompareforNullValue(Value, previousValue);

        }
        internal Func<Text, Text, int> _compareValueFromDbWithFilterValue = null;
        public bool DbCaseInsensitive { get; set; }
        protected override int CompareValueFromDbWithFilterValue(Text valueFromDb, Text filterValue)
        {
            if (_compareValueFromDbWithFilterValue != null)
                return _compareValueFromDbWithFilterValue(valueFromDb, filterValue);
            if (!DbCaseInsensitive)
                return base.CompareValueFromDbWithFilterValue(valueFromDb, filterValue);
            return ENV.Common.IgnoreCaseTextComparer.Instance.TrimAndCompare(valueFromDb, 0, filterValue, 0);
        }
        protected override bool DbValueStartsWithFilterValue(string dbValue, string startsWithFilterValue)
        {
            if (!DbCaseInsensitive)
                return base.DbValueStartsWithFilterValue(dbValue, startsWithFilterValue);
            return ENV.Common.IgnoreCaseTextComparer.Instance.StartsWith(dbValue, startsWithFilterValue);
        }
        protected override bool DbAreEqual(Text a, Text b)
        {
            if (!DbCaseInsensitive)
                return base.DbAreEqual(a, b);
            else
            {
                if (ReferenceEquals(a, null))
                    return ReferenceEquals(b, null);
                if (ReferenceEquals(b, null))
                    return false;
                return ENV.Common.IgnoreCaseTextComparer.Instance.AreEqualTrim(a, b);
            }
        }
        protected override bool AreEqualOrdinal(Text a, Text b)
        {
            if (!DbCaseInsensitive)
                return base.AreEqualOrdinal(a, b);
            else
            {
                if (ReferenceEquals(a, null))
                    return ReferenceEquals(b, null);
                if (ReferenceEquals(b, null))
                    return false;
                return ENV.Common.IgnoreCaseTextComparer.Instance.AreEqualOrdinalTrim(a, b);
            }
        }

        INullStrategy _nullStrategy = NullStrategy.GetStrategy(false);
        protected override Text AdjustGetValue(Text value)
        {
            return _nullStrategy.GetValue(this, value, InsteadOfNullValue);
        }
        public void SilentSet(Text value)
        {
            var x = base.OnChangeMarkRowAsChanged;
            OnChangeMarkRowAsChanged = false;
            Value = value;
            OnChangeMarkRowAsChanged = x;
        }

        TextStorageType _unicodeStorage;

        public TextStorageType StorageType
        {
            get { return _unicodeStorage; }
            set
            {

                switch (value)
                {
                    case TextStorageType.Unicode:
                        Storage = new Firefly.Box.Data.Storage.StringTextStorage(this);
                        break;
                    case TextStorageType.Ansi:
                        Storage = new ENV.Data.Storage.AnsiStringTextStorageThatRemovesNullChars(this, false);
                        break;
                    case TextStorageType.AnsiFixedLength:
                        Storage = new ENV.Data.Storage.AnsiStringFixedLengthTextStorage(this);
                        break;
                        case TextStorageType.NullPaddedAnsiFixedLength:
                        if (!IgnoreNullPaddedAnsiFixedLengthStorageType)
                            Storage = new NullPaddedAnsiStringFixedLengthTextStorage(this);
                        break;
                    case TextStorageType.Oem:
                        if (LocalizationInfo.Current is FrenchLocalizationInfo)
                            Storage = new FrenchOemTextStorage(this);
                        else if (LocalizationInfo.Current is HebrewLocalizationInfo)
                            Storage = new ENV.Data.Storage.HebrewOemTextStorage(this);
                        else
                            Storage = new ENV.Data.Storage.SlovenianOemTextStorage(this);
                        break;
                    case TextStorageType.BackwardCompatibleHebrewOemToAnsi:
                        Storage = new HebrewOemToAnsiTextStorage(this);
                        break;
                }
                _unicodeStorage = value;

            }
        }
        public override Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Text> Storage
        {
            get
            {
                return base.Storage;
            }
            set
            {
                if (value != null)
                    base.Storage = value;
                else
                    StorageType = TextStorageType.Ansi;
            }
        }
        public static bool DefaultUseBackslashToDistinguishWildcards { get; set; }
        static char[] _defaultFilterWildcards = new char[] { '*', '?', ENV.UserMethods.Instance.Chr(255)[0] };
        public static char[] DefaultFilterWildcards
        {
            get { return _defaultFilterWildcards; }
            set
            {
                _defaultFilterWildcards = value;
            }
        }
        protected override char[] FilterWildcards
        {
            get
            {
                return _defaultFilterWildcards;
            }
            set
            {
                base.FilterWildcards = value;
            }
        }


        /// <summary>
        /// Performs both the BindValue method and the IsEqualTo
        /// </summary>
        /// <param name="to">The bind expression or value</param>
        /// <returns>The IsEqualTo Fitler</returns>
        public FilterBase BindEqualTo(Text to)
        {
            BindValue(() => to);
            return IsEqualTo(to);
        }
        /// <summary>
        /// Performs both the BindValue method and the IsEqualTo
        /// </summary>
        /// <param name="to">The bind expression or value</param>
        /// <returns>The IsEqualTo Fitler</returns>
        public FilterBase BindEqualTo(TypedColumnBase<Text> to)
        {
            BindValue(to);
            return IsEqualTo(to);
        }
        /// <summary>
        /// Performs both the BindValue method and the IsEqualTo
        /// </summary>
        /// <param name="to">The bind expression or value</param>
        /// <returns>The IsEqualTo Fitler</returns>
        public FilterBase BindEqualTo(Func<Text> to)
        {
            BindValue(to);
            return IsEqualTo(to);
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
        BackwardCompatible.InsteadOfNullValue<Text> _insteadOfNullValue;
        public BackwardCompatible.InsteadOfNullValue<Text> InsteadOfNullValue
        {
            get
            {
                if (_nullStrategy.DoInsteadOfNull() && _insteadOfNullValue == null)
                    InsteadOfNullValue = NullBehaviour.InsteadOfNullValueText;
                return _insteadOfNullValue;
            }
            set { _insteadOfNullValue = BackwardCompatible.NullBehaviour.CreateText(value.GetNullValueInstance().ToText(Format)); }
        }

        protected override void OnFormatChanged(string value)
        {
            base.OnFormatChanged(value);
            if (_insteadOfNullValue != null)
                InsteadOfNullValue = _insteadOfNullValue;
        }

        internal bool RecievedParameterValueOtherThenNull
        {
            get;
            set;
        }

        public string DbType { get; set; }
        public string DbDefault { get; set; }

        protected override Text AdjustSetValue(Text t, bool fromDb)
        {
            if (ENV.JapaneseMethods.Enabled)
            {
                t = JapaneseMethods.MatchForJapaneseLength(t, FormatInfo);
            }
            return base.AdjustSetValue(_nullStrategy.AdjustSetValue(t), fromDb);
        }
        protected override bool IsNull(Text value)
        {
            return BackwardCompatible.NullBehaviour.IsNull(value);
        }



        readonly static ByteArrayColumn _defaultByteArrayColumn = new ByteArrayColumn();
        public static Text FromByteArray(byte[] value)
        {
            lock (_defaultByteArrayColumn)
            {
                _defaultByteArrayColumn.Value = value;
                return _defaultByteArrayColumn.ToString();
            }
        }
        internal static Text RtfFromByteArray(byte[] value)
        {
            lock (_defaultByteArrayColumn)
            {
                _defaultByteArrayColumn.Value = value;
                return _defaultByteArrayColumn.ToStringForRichTextBox();
            }
        }
        public static byte[] ToByteArray(Text value)
        {
            lock (_defaultByteArrayColumn)
            {
                return _defaultByteArrayColumn.FromString(value);
            }
        }
        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }
        public Text FullCaption { get { return UserMethods.InternalVarName(this); } }
        public Text FullDbName { get { return UserMethods.InternalVarDbName(this); } }
        public Bool WasChanged { get { return UserMethods.InternalVarMod(this); } }

        bool ControllerBase.ParameterColumn._fireOnChangeEventIfEqual { get; set; }
        UserMethods ControllerBase.ParameterColumn._getUserMethods() => u;
        object IENVColumn._internalValueChangeStore { get; set; }
		public static bool IgnoreNullPaddedAnsiFixedLengthStorageType { get; set; }
        public static implicit operator DotnetParameter<string>(TextColumn tc)
        {

            return new DotnetParameter<string>(tc.u.RTrim(tc), rv => tc.Value = rv);
        }


    }
    public enum TextStorageType
    {
        Ansi,
        AnsiFixedLength,
        NullPaddedAnsiFixedLength,
        Oem,
        Unicode,
        BackwardCompatibleHebrewOemToAnsi
    }
}