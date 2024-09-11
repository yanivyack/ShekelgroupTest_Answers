using System;
using System.Runtime.InteropServices;
using ENV.BackwardCompatible;
using ENV.UI;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.Interop;

namespace ENV
{
    public class ComColumn<T> : Firefly.Box.Interop.ComColumn<T>, IENVColumn

    {
        public ComColumn()
        {
            Init();
        }

        public ComColumn(string caption)
            : base(caption)
        {
            Init();
        }
        class myDummyStorage : IColumnStorageSrategy<T>
        {
            public T LoadFrom(IValueLoader loader)
            {
                return default(T);
            }

            public void SaveTo(T value, IValueSaver saver)
            {

                saver.SaveByteArray(new byte[0]);
            }
        }
        void Init()
        {
            _expand = new ColumnHelper(this);

            Storage = new myDummyStorage();
            ValueChanged +=
                args =>
                {
                    AddRemoveReferences(args.PreviousValue, Value);
                };
        }
        public CustomHelp CustomHelp { get; set; }
        void IENVColumn.EnterOnControl()
        {
            _expand.EnterOnControl();
        }
        void IENVColumn.SetNullStrategy(INullStrategy instance)
        {

        }

        public bool CompareForNullValue(object previousValue)
        {
            return true;
        }
        bool IENVColumn._internalReadOnly { get; set; }
        public Text FullCaption { get { return UserMethods.InternalVarName(this); } }
        public bool ReadOnlyForExistingRows { get; set; }

        public Type ControlType { get; set; }
        public Type ControlTypeOnGrid { get; set; }
        public Type ControlTypePrinting { get; set; }
        public Type ControlTypePrintingOnGrid { get; set; }
        public override T Value
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
        void IENVColumn.UpdateParameterReturnValue(Action performUpdate)
        {
            _expand.UpdateParameterReturnValue(performUpdate);
        }

        void IENVColumn.InternalPerformExpandOperation(Action expand)
        {
            _expand.InternalPerformExpandOperation(expand);
        }
        public bool AutoExpand { get { return _expand.AutoExpand; } set { _expand.AutoExpand = value; } }
        public override void ResetToDefaultValue()
        {
            var oldValue = Value;
            base.ResetToDefaultValue();
            AddRemoveReferences(oldValue, Value);
        }

        public override void Dispose()
        {
        }

        protected virtual bool CancelSppression()
        {
            return false;
        }

        void AddRemoveReferences(T oldValue, T newValue)
        {
            if (CancelSppression())
                return;
            if (newValue != null)
                ReferenceCounter.Instance.AddReference(newValue);

            if (oldValue != null)
                ReferenceCounter.Instance.RemoveReference(oldValue,
                    () =>
                    {
                        Firefly.Box.Context.Current.InvokeUICommand(
                            () =>
                            {
                                if (Marshal.IsComObject(oldValue))
                                {
                                    Marshal.FinalReleaseComObject(oldValue);
                                    return;
                                }
                                var d = oldValue as IDisposable;
                                if (d != null)
                                {
                                    d.Dispose();
                                    return;
                                }
                            });
                    });
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
        protected override T CreateInstanceCore()
        {
            try
            {
                return base.CreateInstanceCore();
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e);
                throw;
            }
        }
        object IENVColumn._internalValueChangeStore { get; set; }
    }

    public class DotnetColumn<T> : ComColumn<T>
    {
        public Text FullCaption { get { return UserMethods.InternalVarName(this); } }
        protected override bool CancelSppression()
        {
            return true;
        }
        UserMethods _uInstance;
        protected UserMethods u
        {
            get
            {
                if (_uInstance == null)
                {
                    _uInstance = new UserMethods();

                }
                return _uInstance;
            }
        }
        protected override T Cast(object value)
        {
            var x = value as ColumnBase;
            if (x != null)
                value = x.Value;
            if (u.IsNull(value))
                value = null;
            {
                var t = value as Text;
                if (t != null)
                    value = u.ToObject(t);
            }
            {
                var t = value as Number;
                if (t != null)
                {
                    value = u.ToObject(t);
                    if (value is int)
                        value = (long)(int)value;
                }
            }
            {
                var t = value as Time;
                if (t != null)
                    value = u.ToObject(t);
            }
            {
                var t = value as Date;
                if (t != null)
                    value = u.ToObject(t);
            }
            {
                var t = value as Bool;
                if (t != null)
                    value = u.ToObject(t);
            }
            return base.Cast(value);
        }

        public DotnetColumn() : this(null)
        {
        }

        public DotnetColumn(string name) : base(name)
        {
            CreateInstance = false;
        }

        public delegate void SetStructPropertiesDelegate<TT>(ref TT @struct);
        public void SetStructProperties(SetStructPropertiesDelegate<T> setStruct)
        {
            ENV.Common.Try(() =>
            {
                var x = Value;
                setStruct(ref x);
                Value = x;
            });
        }
        protected override T CreateInstanceCore()
        {
            try
            {
                return base.CreateInstanceCore();
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e);
                throw;
            }
        }
        public static implicit operator T(DotnetColumn<T> x)
        {
            return x.Value;
        }


        public override void ResetToDefaultValue()
        {
            if (typeof(System.Windows.Forms.Control).IsAssignableFrom(typeof(T)))
                return;
            base.ResetToDefaultValue();
        }

        protected override bool ValueIsComObject()
        {
            return false;
        }
        
    }
}