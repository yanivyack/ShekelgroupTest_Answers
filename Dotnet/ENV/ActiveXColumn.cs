using System;
using ENV.BackwardCompatible;
using ENV.UI;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Interop;

namespace ENV
{
    public class ActiveXColumn<T> : Firefly.Box.Interop.ActiveXColumn<T>, IENVColumn
        where T : System.Windows.Forms.Control, new()
    {
        public ActiveXColumn()
        {
            _expand = new ColumnHelper(this);
        }
        public CustomHelp CustomHelp { get; set; }
        public ActiveXColumn(string caption) : base(caption)
        {
            _expand = new ColumnHelper(this);
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

        public bool AutoExpand { get { return _expand.AutoExpand; } set { _expand.AutoExpand = value; } }
        public string StatusTip { get; set; }
        public string ToolTip { get; set; }
        bool IENVColumn.IsParameter { get; set; }
        public Text FullCaption { get { return UserMethods.InternalVarName(this); } }
         bool IENVColumn._internalReadOnly { get; set; }
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
}