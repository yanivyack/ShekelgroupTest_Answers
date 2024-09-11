using System;
using System.Collections.Generic;
using System.Text;
using ENV.BackwardCompatible;
using ENV.UI;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;

namespace ENV.Data
{
    public class ArrayColumn<dataType> : Firefly.Box.Data.ArrayColumn<dataType>, IENVColumn
    {


        public ArrayColumn(TypedColumnBase<dataType> column, string name)
            : base(column, name)
        {
            _expand = new ColumnHelper(this);
            _column = column;
        }
        protected UserMethods u { get { return UserMethods.Instance; } }
        public ArrayColumn(TypedColumnBase<dataType> column)
            : base(column)
        {
            _expand = new ColumnHelper(this);
            _column = column;
        }

        void IENVColumn.EnterOnControl()
        {
            _expand.EnterOnControl();
        }
        public override bool TrySetValue(int position, object value)
        {
            if (_column is Firefly.Box.Data.NumberColumn)
                value = u.CastToNumber(value);
            else if (_column is Firefly.Box.Data.DateColumn)
                value = u.CastToDate(value);
            else if (_column is Firefly.Box.Data.TimeColumn)
                value = u.CastToTime(value);
            else if (_column is ArrayColumn<byte[]>)
            {
                var cb = value as ColumnBase;
                if (cb != null)
                    value = cb.Value;
                if (value is Text[])
                    value = u.TextArrayToByteArrayArray((Text[])value);
            }

            return base.TrySetValue(position, value);
        }
        public CustomHelp CustomHelp { get; set; }
        public bool AutoExpand { get { return _expand.AutoExpand; } set { _expand.AutoExpand = value; } }
        TypedColumnBase<dataType> _column;
        public TypedColumnBase<dataType> BaseColumn
        {
            get { return _column; }
        }
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
        public override dataType[] Value
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
        void IENVColumn.SetNullStrategy(INullStrategy instance)
        {

        }

        public bool CompareForNullValue(object previousValue)
        {
            return true;
        }

        ColumnHelper _expand;
        public new event Action Expand
        {
            add { _expand.Expand += value; }
            remove { _expand.Expand -= value; }
        } /// <summary>
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


        static ENV.Remoting.ClientParameterManager _packer = new ENV.Remoting.ClientParameterManager() { DoNotTrim = true };


        public override byte[] ToByteArray()
        {
            var canBa = _column as ICanBeTranslatedToByteArray;
            if (canBa != null)
            {
                var ac = new ArrayColumn<byte[]>(new ByteArrayColumn());
                var l = new List<byte[]>();
                foreach (var item in Value)
                {
                    _column.Value = item;
                    l.Add(canBa.ToByteArray());

                }
                ac.Value = l.ToArray();
                return UserMethods.TypedArrayToByteArray(ac.Value, null);
            }
            else
                return UserMethods.TypedArrayToByteArray(Value, _column);
        }
        protected override object _fromByteArray(byte[] ba)
        {
            return FromByteArray(ba);
        }

        public dataType[] FromByteArray(byte[] bytes)
        {
            var c = BaseColumn as ICanBeTranslatedToByteArray;
            if (c != null)
            {
                var baac = new ArrayColumn<byte[]>(new ByteArrayColumn());
                var t = baac.FromByteArray(bytes);
                var l = new List<dataType>();
                foreach (var item in t)
                {
                    var temp = UserMethods.ByteArrayToTypedArray(item);
                    var x = new System.Collections.ArrayList(temp);
                    if (typeof(dataType).GetElementType() == typeof(byte[]) && x.Count > 0 && x[0].GetType() == typeof(Text))
                    {
                        var vec = _column as ArrayColumn<byte[]>;
                        if (vec != null)
                        {
                            var bac = vec._column as ByteArrayColumn;
                            if (bac != null)
                            {
                                x.Clear();
                                foreach (var inTemp in temp)
                                {
                                    x.Add(bac.FromString((Text)inTemp));
                                }
                            }
                        }

                    }
                    l.Add((dataType)(object)x.ToArray(typeof(dataType).GetElementType()));



                }
                return l.ToArray();
            }
            else
                return Cast(UserMethods.ByteArrayToTypedArray(bytes));
        }

        protected T Create<T>()
        {
            return AbstractFactory.Create<T>();
        }
        public Text FullCaption { get { return UserMethods.InternalVarName(this); } }
        public Text FullDbName { get { return UserMethods.InternalVarDbName(this); } }
        public Bool WasChanged { get { return UserMethods.InternalVarMod(this); } }
        object IENVColumn._internalValueChangeStore { get; set; }

    }
}