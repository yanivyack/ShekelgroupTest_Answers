using System;
using System.ComponentModel;
using ENV.BackwardCompatible;
using ENV.UI;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.Advanced;

namespace ENV.Data
{
    public class NumberColumn : Firefly.Box.Data.NumberColumn, IENVColumn, SqlScriptGenerator.IColumn, ControllerBase.ParameterColumn
    {
        public NumberColumn()
        {
            _expand = new ColumnHelper(this);
            _defaultWasSet = false;
        }

        public NumberColumn(string name)
            : base(name)
        {
            _expand = new ColumnHelper(this);
            _defaultWasSet = false;
        }



        public NumberColumn(string name, string format)
            : base(name, format)
        {
            _expand = new ColumnHelper(this);
            _defaultWasSet = false;
        }

        public NumberColumn(string name, string format, string caption)
            : base(name, format, caption)
        {
            _expand = new ColumnHelper(this);
            _defaultWasSet = false;
        }
        internal bool _defaultWasSet;
        public override Number DefaultValue
        {
            get
            {
                return base.DefaultValue;
            }

            set
            {
                _defaultWasSet = true;
                base.DefaultValue = value;
            }
        }

        void IENVColumn.EnterOnControl()
        {
            _expand.EnterOnControl();
        }
        protected override Number Cast(object value)
        {
            if (value == null)
                return null;
            {
                Number result;
                if (Number.TryCast(value, out result))
                    return result;
            }
            {
                Time result;
                if (Time.TryCast(value, out result))
                    return ENV.UserMethods.Instance.ToNumber(result);
            }
            {
                Date result;
                if (Date.TryCast(value, out result))
                    return ENV.UserMethods.Instance.ToNumber(result);
            }
            return Number.Cast(value);

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
        public override Number Value
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
        protected override Number AdjustGetValue(Number value)
        {
            return _nullStrategy.GetValue(this, value, InsteadOfNullValue);
        }
        /// <summary>
        /// Performs both the BindValue method and the IsEqualTo
        /// </summary>
        /// <param name="to">The bind expression or value</param>
        /// <returns>The IsEqualTo Fitler</returns>
        public FilterBase BindEqualTo(Number to)
        {
            BindValue(() => to);
            return IsEqualTo(to);
        }
        /// <summary>
        /// Performs both the BindValue method and the IsEqualTo
        /// </summary>
        /// <param name="to">The bind expression or value</param>
        /// <returns>The IsEqualTo Fitler</returns>
        public FilterBase BindEqualTo(TypedColumnBase<Number> to)
        {
            BindValue(to);
            return IsEqualTo(to);
        }
        /// <summary>
        /// Performs both the BindValue method and the IsEqualTo
        /// </summary>
        /// <param name="to">The bind expression or value</param>
        /// <returns>The IsEqualTo Fitler</returns>
        public FilterBase BindEqualTo(Func<Number> to)
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
        public BackwardCompatible.InsteadOfNullValue<Number> InsteadOfNullValue { get; set; }
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

        INullStrategy _nullStrategy =  NullStrategy.GetStrategy(false);
        protected override Number AdjustSetValue(Number t, bool fromDb)
        {
            return base.AdjustSetValue(_nullStrategy.AdjustSetValue(t), fromDb);
        }
        protected override bool IsNull(Number value)
        {
            return BackwardCompatible.NullBehaviour.IsNull(value);
        }


        static Random _randForTimePlusRandom = null;




        public string DbType { get; set; }
        public string DbDefault { get; set; }
        public bool SetValueBasedOnTimePlusRandom
        {
            set
            {
                if (value)
                    BindValue(() =>
                    {
                        if (_randForTimePlusRandom == null)
                            _randForTimePlusRandom = new Random(Guid.NewGuid().GetHashCode());
                        return (Time.Now.TotalSeconds - 43200) * 32768 + _randForTimePlusRandom.NextDouble() * 32768;
                    });
            }
        }
        public void AddDeltaOf(Func<Date> expression)
        {
            base.AddDeltaOf(() => u.ToNumber(expression()));
        }
        public void AddDeltaOf(Func<Time> expression)
        {
            base.AddDeltaOf(() => u.ToNumber(expression()));
        }
        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }
        public Text FullCaption { get { return UserMethods.InternalVarName(this); } }
        public Text FullDbName { get { return UserMethods.InternalVarDbName(this); } }
        public Bool WasChanged { get { return UserMethods.InternalVarMod(this); } }

        bool ControllerBase.ParameterColumn._fireOnChangeEventIfEqual { get; set; }
        object IENVColumn._internalValueChangeStore { get; set; }
        UserMethods ControllerBase.ParameterColumn._getUserMethods() => u;
    }
}