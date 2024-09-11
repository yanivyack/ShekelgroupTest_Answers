using System;
using System.ComponentModel;
using ENV.BackwardCompatible;
using ENV.UI;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.Advanced;

namespace ENV.Data
{
    public class TimeColumn : Firefly.Box.Data.TimeColumn, IENVColumn, ControllerBase.ParameterColumn
    {
        public TimeColumn(string name, string format, string caption)
            : base(name, format, caption)
        {
            Init();
        }
        void Init()
        {
            _expand = new ColumnHelper(this);
            if (DefaultStorage != null)
                Storage = DefaultStorage;
        }
        public static Firefly.Box.Data.DataProvider.IColumnStorageSrategy<Time> DefaultStorage;

        public TimeColumn(string name, string format)
            : base(name, format)
        {
            Init();
        }

        public TimeColumn(string name)
            : base(name)
        {
            Init();
        }

        public TimeColumn()
        {
            Init();
        }
        public override string Format
        {
            get
            {
                return base.Format??"HH:MM:SS";
            }

            set
            {
                base.Format = value;
            }
        }
        protected override Time Cast(object value)
        {
            if (value == null)
                return null;
            {
                Time result;
                if (Time.TryCast(value, out result))
                    return result;
            }
            {
                Number result;
                if (Number.TryCast(value, out result))
                    return ENV.UserMethods.Instance.ToTime(result);
            }
            {
                Date result;
                if (Date.TryCast(value, out result))
                    return ENV.UserMethods.Instance.ToTime(result);
            }
            return Time.Cast(value);

        }
        internal Time GetInsteadOfNull()
        {
            return NullBehaviour.GetInsteadOfValue(InsteadOfNullValue);
        }
        public CustomHelp CustomHelp { get; set; }

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
        public override Time Value
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
        protected override Time AdjustGetValue(Time value)
        {
            return _nullStrategy.GetValue(this, value, InsteadOfNullValue);
        }
        /// <summary>
        /// Performs both the BindValue method and the IsEqualTo
        /// </summary>
        /// <param name="to">The bind expression or value</param>
        /// <returns>The IsEqualTo Fitler</returns>
        public FilterBase BindEqualTo(Time to)
        {
            BindValue(() => to);
            return IsEqualTo(to);
        }
        /// <summary>
        /// Performs both the BindValue method and the IsEqualTo
        /// </summary>
        /// <param name="to">The bind expression or value</param>
        /// <returns>The IsEqualTo Fitler</returns>
        public FilterBase BindEqualTo(TypedColumnBase<Time> to)
        {
            BindValue(to);
            return IsEqualTo(to);
        }
        /// <summary>
        /// Performs both the BindValue method and the IsEqualTo
        /// </summary>
        /// <param name="to">The bind expression or value</param>
        /// <returns>The IsEqualTo Fitler</returns>
        public FilterBase BindEqualTo(Func<Time> to)
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

        void IENVColumn.EnterOnControl()
        {
            _expand.EnterOnControl();
        }
        public bool AutoExpand { get { return _expand.AutoExpand; } set { _expand.AutoExpand = value; } }

        public InsteadOfNullValue<Time> InsteadOfNullValue { get; set; }
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
        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }
        public bool CompareForNullValue(object previousValue)
        {
            return _nullStrategy.CompareforNullValue(Value, previousValue);
        }

        INullStrategy _nullStrategy =  NullStrategy.GetStrategy(false);
        protected override Time AdjustSetValue(Time value, bool fromDB)
        {
            return base.AdjustSetValue(_nullStrategy.AdjustSetValue(value), fromDB);
        }
        protected override bool IsNull(Time value)
        {
            return BackwardCompatible.NullBehaviour.IsNull(value);
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