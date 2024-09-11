using System;
using System.ComponentModel;
using ENV.BackwardCompatible;
using ENV.UI;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data;
using Firefly.Box.Data.Advanced;
using ENV.Data.Storage;
using Firefly.Box.Data.DataProvider;

namespace ENV.Data
{
    public class DateColumn : Firefly.Box.Data.DateColumn, IENVColumn, ControllerBase.ParameterColumn
    {
        public DateColumn(string name, string format, string caption)
            : base(name, format, caption)
        {
            Init();
        }

        public DateColumn(string name, string format)
            : base(name, format)
        {
            Init();
        }

        public DateColumn(string name)
            : base(name)
        {
            Init();
        }

        protected override Date Cast(object value)
        {
            if (value == null)
                return null;
            {
                Date result;
                if (Date.TryCast(value, out result))
                    return result;
            }
            {
                Number result;
                if (Number.TryCast(value, out result))
                    return ENV.UserMethods.Instance.ToDate(result);
            }
            {
                Time result;
                if (Time.TryCast(value, out result))
                    return ENV.UserMethods.Instance.ToDate(result);
            }
            return Date.Cast(value);

        }
        public DateColumn()
        {
            Init();
        }
        void Init()
        {
            DefaultValue = _globalDefault;
            _expand = new ColumnHelper(this);
            if (NewInstance != null)
                NewInstance(this);
        }
        public override string Format
        {
            get
            {
                return base.Format??Date.ContextDefaultFormat;
            }

            set
            {
                base.Format = value;
            }
        }
        void IENVColumn.EnterOnControl()
        {
            _expand.EnterOnControl();
        }
        public override Firefly.Box.Data.TimeColumn TimeColumnForDateTimeStorage
        {
            get { return base.TimeColumnForDateTimeStorage; }
            set
            {
                base.TimeColumnForDateTimeStorage = value;
                var x = new StorageTester();
                Storage.SaveTo(new Date(1976, 6, 16), x);
                if (!x.SavesDateTime)
                    Storage = new DateTimeDateStorage();
            }
        }
        class StorageTester : IValueSaver
        {
            public void SaveAnsiString(string value, int length, bool fixedWidth)
            {
            }

            public void SaveBoolean(bool value)
            {
            }

            public void SaveByteArray(byte[] value)
            {
            }

            public void SaveDateTime(DateTime value)
            {
                SavesDateTime = true;
            }

            public void SaveDecimal(decimal value, byte precision, byte scale)
            {
            }

            public void SaveEmptyDateTime()
            {
                SavesDateTime = true;
            }

            public void SaveInt(int value)
            {
            }

            public void SaveNull()
            {
            }

            public void SaveString(string value, int length, bool fixedWidth)
            {
            }

            public void SaveTimeSpan(TimeSpan value)
            {
            }
            public bool SavesDateTime;
        }
        public CustomHelp CustomHelp { get; set; }
        public bool AutoExpand { get { return _expand.AutoExpand; } set { _expand.AutoExpand = value; } }
        internal static Action<DateColumn> NewInstance;

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
        public override Date Value
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

        /// <summary>
        /// Gets or Sets the global default of date items.
        /// The default value is <see cref="Date"/>.<see cref="Date.Empty"/>. Another commonly used default is 01/01/1901.
        /// </summary>
        public static Date GlobalDefault
        {
            get { return _globalDefault; }
            set
            {
                _globalDefault = value;

            }
        }
        static Date _globalDefault = Date.Empty;
        public static Date ErrorDate = ENV.UserMethods.Instance.ToDate(1000000000);

        /// <summary>
        /// Performs both the BindValue method and the IsEqualTo
        /// </summary>
        /// <param name="to">The bind expression or value</param>
        /// <returns>The IsEqualTo Fitler</returns>
        public FilterBase BindEqualTo(Date to)
        {
            BindValue(() => to);
            return IsEqualTo(to);
        }
        /// <summary>
        /// Performs both the BindValue method and the IsEqualTo
        /// </summary>
        /// <param name="to">The bind expression or value</param>
        /// <returns>The IsEqualTo Fitler</returns>
        public FilterBase BindEqualTo(TypedColumnBase<Date> to)
        {
            BindValue(to);
            return IsEqualTo(to);
        }
        /// <summary>
        /// Performs both the BindValue method and the IsEqualTo
        /// </summary>
        /// <param name="to">The bind expression or value</param>
        /// <returns>The IsEqualTo Fitler</returns>
        public FilterBase BindEqualTo(Func<Date> to)
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

        internal Date GetInsteadOfNull()
        {
            return NullBehaviour.GetInsteadOfValue(InsteadOfNullValue);
        }

        public InsteadOfNullValue<Date> InsteadOfNullValue { get; set; }

        INullStrategy _nullStrategy =  NullStrategy.GetStrategy(false);
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
            return _nullStrategy.CompareforNullValue(Value, previousValue);
        }

        protected override Date AdjustSetValue(Date value, bool fromDB)
        {
            if (ReferenceEquals(value, null) && !AllowNull)
            {
                if (fromDB)
                    value = Date.Empty;
                else if (ENV.UserSettings.Version8Compatible)
                    value = NullBehaviour.NullDate + 0;
            }
            return base.AdjustSetValue(_nullStrategy.AdjustSetValue(value), fromDB);
        }
        protected override Date AdjustGetValue(Date value)
        {
            return _nullStrategy.GetValue(this, value, InsteadOfNullValue);
        }
        protected override bool IsNull(Date value)
        {
            return BackwardCompatible.NullBehaviour.IsNull(value);
        }
        DateColumn _userInputFormatHelper = null;
        protected override void ProcessUserInput(string value, string format, bool suppressValueChanged)
        {
            if (!ENV.UserSettings.Version10Compatible)
            {
                if (Text.IsNullOrEmpty(format))
                {
                    value = value.PadRight(FormatInfo.MaxLength);
                }
                else
                {
                    if (_userInputFormatHelper == null)
                        _userInputFormatHelper = new Data.DateColumn();
                    _userInputFormatHelper.Format = format;
                    value = value.PadRight(FormatInfo.MaxLength);
                }
            }

            base.ProcessUserInput(value, format, suppressValueChanged);
        }
        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }
        public void AddDeltaOf(Func<Date> expression)
        {
            base.AddDeltaOf(() => u.ToNumber(expression()));
        }
        public void AddDeltaOf(Func<Time> expression)
        {
            base.AddDeltaOf(() => u.ToNumber(expression()));
        }
        public Text FullCaption { get { return UserMethods.InternalVarName(this); } }
        public Text FullDbName { get { return UserMethods.InternalVarDbName(this); } }
        public Bool WasChanged { get { return UserMethods.InternalVarMod(this); } }
        bool ControllerBase.ParameterColumn._fireOnChangeEventIfEqual { get; set; }
        object IENVColumn._internalValueChangeStore { get; set; }
        UserMethods ControllerBase.ParameterColumn._getUserMethods() => u;
    }
}