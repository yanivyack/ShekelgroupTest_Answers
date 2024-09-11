using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENV.Data;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;

namespace ENV.BackwardCompatible
{
    public class NullBehaviour
    {
        public static bool EqualToNullReturnsNull = true;

        internal static InsteadOfNullValue<Number> InsteadOfValueNullNumber = new NullNumberClass(0);
        internal static InsteadOfNullValue<Text> InsteadOfNullValueText = new NullTextClass("");
        static InsteadOfNullValue<Bool> InsteadOfNullValueBool = new NullBoolClass(false);
        static InsteadOfNullValue<Date> InsteadOfNullValueDate = new NullDateClass(Date.Empty);
        internal static InsteadOfNullValue<Date> InsteadOfNullValueDateForColumn = new NullDateClass(new Date(1901, 1, 1));
        static InsteadOfNullValue<Time> InsteadOfNullValueTime = new NullTimeClass(Time.StartOfDay);

        public static Number NullNumber = InsteadOfValueNullNumber.GetNullValueInstance();
        public static Date NullDate = InsteadOfNullValueDate.GetNullValueInstance();
        public static Text NullText = InsteadOfNullValueText.GetNullValueInstance();
        public static Time NullTime = InsteadOfNullValueTime.GetNullValueInstance();
        public static Bool NullBool = InsteadOfNullValueBool.GetNullValueInstance();



        class NullNumberClass : Firefly.Box.Number.DecimalNumber, InsteadOfNullValue<Firefly.Box.Number>
        {
            public NullNumberClass(Firefly.Box.Number value)
                : base(value)
            {
            }
            protected override bool IsSpecialNullInstance()
            {
                return true;
            }

            public Number GetNullValueInstance()
            {
                return this;
            }
        }
        public static bool EqualsThatHandlesNullAsADifference(object a, object b)
        {
            if (a is NullValue)
                return b is NullValue;
            else if (b is NullValue)
                return false;
            return Comparer.Equals(a, b);
        }

        public static void SetDefaultInsteadOfNullValueForDateColumns(Date value)
        {
            InsteadOfNullValueDateForColumn = CreateDate(value);
        }

        internal static T GetInsteadOfValue<T>(InsteadOfNullValue<T> insteadOfNullValue)
        {
            if (ReferenceEquals(insteadOfNullValue, null))
            {
                if (typeof(Number).IsAssignableFrom(typeof(T)))
                    return (T)InsteadOfValueNullNumber;
                if (typeof(Bool).IsAssignableFrom(typeof(T)))
                    return (T)InsteadOfNullValueBool;
                if (typeof(Text).IsAssignableFrom(typeof(T)))
                    return (T)InsteadOfNullValueText;
                if (typeof(Date).IsAssignableFrom(typeof(T)))
                    return (T)InsteadOfNullValueDateForColumn;
                if (typeof(Time).IsAssignableFrom(typeof(T)))
                    return (T)InsteadOfNullValueTime;
                throw new Exception();
            }
            else
                return insteadOfNullValue.GetNullValueInstance();
        }

        public static bool IsNull(object value)
        {
            if (ReferenceEquals(value, null))
                return true;
            if (value.GetType().IsCOMObject)
            {
                return false;
            }
            else
            {
                var x = value as NullValue;
                if (x != null)
                    return true;
            }
            return false;
        }

        public static InsteadOfNullValue<Number> CreateNumber(Number i)
        {
            return new NullNumberClass(i);
        }

        public static InsteadOfNullValue<Text> CreateText(Text value)
        {
            return new NullTextClass(value);
        }
        class NullTextClass : Firefly.Box.Text, InsteadOfNullValue<Text>
        {
            public NullTextClass(Text value)
                : base(value)
            {
            }

            public Text GetNullValueInstance()
            {
                return this;
            }
            protected override bool IsSpecialNullInstance()
            {
                return true;
            }
        }

        class NullBoolClass : Bool, InsteadOfNullValue<Bool>
        {
            bool _value;

            public NullBoolClass(bool value)
            {
                _value = value;
            }

            protected override bool IsTrue()
            {
                return _value;
            }
            protected override bool IsSpecialNullInstance()
            {
                return true;
            }
            protected override Bool Not()
            {
                return !_value;
            }

            public Bool GetNullValueInstance()
            {
                return this;
            }
        }
        public static InsteadOfNullValue<Bool> CreateBool(bool b)
        {
            return new NullBoolClass(b);
        }

        class NullDateClass : Date, InsteadOfNullValue<Date>
        {
            public NullDateClass(Date value)
                : base(value.Year, value.Month, value.Day)
            {
            }
            protected override bool IsSpecialNullInstance()
            {
                return true;
            }

            public Date GetNullValueInstance()
            {
                return this;
            }
            
        }
        public static InsteadOfNullValue<Date> CreateDate(Date date)
        {
            return new NullDateClass(date);
        }
        class NullTimeClass : Time, InsteadOfNullValue<Time>
        {
            public NullTimeClass(Time value)
                : base(value.Hour, value.Minute, value.Second)
            {
            }
            protected override bool IsSpecialNullInstance()
            {
                return true;
            }

            public Time GetNullValueInstance()
            {
                return this;
            }
        }
        public static InsteadOfNullValue<Time> CreateTime(Time value)
        {
            return new NullTimeClass(value);
        }


    }
    public interface NullValue
    {

    }

    public interface InsteadOfNullValue<T> : NullValue
    {
        T GetNullValueInstance();
    }

    interface INullStrategy
    {
        T AdjustSetValue<T>(T value) where T : class;
        T GetValue<T>(TypedColumnBase<T> numberColumn, T value, InsteadOfNullValue<T> insteadOfNullValue) where T : class;
        bool CompareforNullValue(object value, object originalValue);
        bool DoInsteadOfNull();
        object MemoryParameterResult(object result);
        Bool EqualsUntyped(object a, object b);
        Bool Equals(object a, object b);
        bool ReturnValueIfNull<T>(ref object value, T nullValue);
        Bool AndWithLeftNull();
        void ApplyTo(UserMethods uInstance);
        void OverrideAndCalculate(Action what);
        bool UseBlankTextInsteadOfNullInDynamicSQL();
    }

    class NullStrategy : INullStrategy
    {


        public static INullStrategy Instance = new NullStrategy();
        public virtual T AdjustSetValue<T>(T value) where T : class
        {
            return value;
        }
        public virtual T GetValue<T>(TypedColumnBase<T> numberColumn, T value, InsteadOfNullValue<T> insteadOfNullValue) where T : class
        {
            return value;

        }

        public virtual bool CompareforNullValue(object value, object originalValue)
        {

            return true;

        }

        public virtual bool DoInsteadOfNull()
        {
            return false;
        }

        public virtual object MemoryParameterResult(object result)
        {
            return result;
        }

        public virtual Bool EqualsUntyped(object a, object b)
        {
            if (ReferenceEquals(a, null))
                if (ReferenceEquals(b, null))
                    if (UserSettings.Version10Compatible)
                        return true;
                    else
                    {
                        return null;
                    }
                else
                {
                    if (NullBehaviour.EqualToNullReturnsNull)
                        return null;
                    else return false;

                }
            else if (ReferenceEquals(b, null))
            {
                if (NullBehaviour.EqualToNullReturnsNull)
                    return null;
                else return false;
            }
            return Equals(a, b);
        }
        public Text NullText { get { return NullBehaviour.NullText; } }
        public Number NullNumber { get { return NullBehaviour.NullNumber; } }
        public Date NullDate { get { return NullBehaviour.NullDate; } }
        public Time NullTime { get { return NullBehaviour.NullTime; } }
        public Bool NullBool { get { return NullBehaviour.NullBool; } }
        protected virtual Bool ColumnEqualNullImp(ColumnBase column)
        {
            if (UserSettings.Version10Compatible || !(column is Firefly.Box.Data.DateColumn))
            {

                if (ReferenceEquals(column.Value, null))
                    return true;
                if (NullBehaviour.EqualToNullReturnsNull)
                {
                    return null;
                }
                else
                    return false;
            }
            if (column is DateColumn)
            {
                var dc = column as ENV.Data.DateColumn;
                if (dc.Value == null)
                {
                    if (dc.GetInsteadOfNull() == Date.Empty)
                        return true;
                    return false;
                }
                if (UserSettings.Version8Compatible)
                    return false;
                return null;

            }
            return false;
        }

        public virtual Bool Equals(object a, object b)
        {
            bool aIsCol = false, bIsCol = false;
            var c = a as ColumnBase;
            if (c != null)
            {
                if (NullBehaviour.IsNull(b) && !(b is ColumnBase))
                    return ColumnEqualNullImp(c);
                aIsCol = true;
                var ba = c as ByteArrayColumn;
                if (ba != null && (b is string || b is Text || b is TextColumn))
                    a = (Text)ba.ToString();
                else
                    a = c.Value;
            }
            c = b as ColumnBase;
            if (c != null)
            {
                if (NullBehaviour.IsNull(a) && !aIsCol)
                    return ColumnEqualNullImp(c);
                bIsCol = true;
                var ba = c as ByteArrayColumn;
                if (ba != null && (a is string || a is Text || a is TextColumn))
                    b = (Text)ba.ToString();
                else

                    b = c.Value;

            }
            if (a is byte[] && (b is string || b is Text || b is TextColumn))
            {
                a = UserMethods.Instance.ByteArrayToText(a as byte[]);
            }

            if (ReferenceEquals(a, null))
                if (ReferenceEquals(b, null))
                    if (UserSettings.Version10Compatible || aIsCol && bIsCol)
                        return true;
                    else
                    {
                        if (NullBehaviour.EqualToNullReturnsNull)
                            return null;
                        else
                            return true;
                    }
                else
                {
                    if (NullBehaviour.EqualToNullReturnsNull)
                        return null;
                    else
                        return false;
                }
            else if (ReferenceEquals(b, null))
            {
                if (NullBehaviour.EqualToNullReturnsNull)
                    return null;
                return false;
            }

            return Comparer.Equal(a, b);
        }

        public virtual bool ReturnValueIfNull<T>(ref object value, T nullValue)
        {
            if (ReferenceEquals(value, null))
                return true;
            return false;
        }

        public virtual Bool AndWithLeftNull()
        {
            return null;
        }

        public virtual void ApplyTo(UserMethods uInstance)
        {

        }

        public void OverrideAndCalculate(Action what)
        {
            var x = _nullOverride.Value.Value;
            try
            {
                _nullOverride.Value.Value = this;
                what();
            }
            finally
            {
                _nullOverride.Value.Value = x;
            }
        }

        public virtual bool UseBlankTextInsteadOfNullInDynamicSQL()
        {
            return false;
        }

        public static void ApplyToUserInstance(ColumnCollection columns, EntityCollection entities, UserMethods um, Firefly.Box.UI.Form f, INullStrategy nullStrategy)
        {
            ApplyToUserInstance(nullStrategy, columns);
            foreach (var e in entities)
            {
                var de = e as DynamicSQLEntity;
                if (de != null)
                    de.SetNullStrategy(nullStrategy);
                var ee = e as ENV.Data.Entity;
                if (ee != null)
                {
                    ee.SetNullStrategy(nullStrategy);
                }

            }
            if (um != null)
                ApplyToUserInstance(nullStrategy, um);

            var ef = f as ENV.UI.Form;
            if (ef != null)
            {
                ef.SetNullStrategy(nullStrategy);
            }

        }

        public static void ApplyToUserInstance(INullStrategy nullStrategy, params ColumnBase[] cols)
        {
            ApplyToUserInstance(nullStrategy, (IEnumerable<ColumnBase>)cols);
        }
        public static void ApplyToUserInstance(INullStrategy nullStrategy, UserMethods uInstance)
        {
            uInstance.SetNullStrategy(nullStrategy);
        }
        public static void ApplyToUserInstance(INullStrategy nullStrategy, IEnumerable<ColumnBase> cols)
        {
            foreach (var column in cols)
            {
                var c = column as IENVColumn;
                if (c != null)
                {
                    c.SetNullStrategy(nullStrategy);
                }
            }
        }

        class NullStrategyOverrideValue
        {
            public NullStrategy Value = null;
        }

        class NullStrategyOverrideDelegate : INullStrategy
        {
            INullStrategy _delegated;
            NullStrategyOverrideValue _override;

            public NullStrategyOverrideDelegate(INullStrategy delegated, NullStrategyOverrideValue @override)
            {
                _delegated = delegated;
                _override = @override;
            }

            public T AdjustSetValue<T>(T value) where T : class
            {
                if (_override.Value != null)
                    return _override.Value.AdjustSetValue(value);
                return _delegated.AdjustSetValue(value);
            }

            public T GetValue<T>(TypedColumnBase<T> numberColumn, T value, InsteadOfNullValue<T> insteadOfNullValue) where T : class
            {
                if (_override.Value != null)
                    return _override.Value.GetValue(numberColumn, value, insteadOfNullValue);
                return _delegated.GetValue(numberColumn, value, insteadOfNullValue);
            }

            public bool CompareforNullValue(object value, object originalValue)
            {
                if (_override.Value != null)
                    return _override.Value.CompareforNullValue(value, originalValue);
                return _delegated.CompareforNullValue(value, originalValue);
            }

            public bool DoInsteadOfNull()
            {
                if (_override.Value != null)
                    return _override.Value.DoInsteadOfNull();
                return _delegated.DoInsteadOfNull();
            }

            public object MemoryParameterResult(object result)
            {
                if (_override.Value != null)
                    return _override.Value.MemoryParameterResult(result);
                return _delegated.MemoryParameterResult(result);
            }

            public Bool EqualsUntyped(object a, object b)
            {
                if (_override.Value != null)
                    return _override.Value.EqualsUntyped(a, b);
                return _delegated.EqualsUntyped(a, b);
            }

            public Bool Equals(object a, object b)
            {
                if (_override.Value != null)
                    return _override.Value.Equals(a, b);
                return _delegated.Equals(a, b);
            }

            public bool ReturnValueIfNull<T>(ref object value, T nullValue)
            {
                if (_override.Value != null)
                    return _override.Value.ReturnValueIfNull(ref value, nullValue);
                return _delegated.ReturnValueIfNull(ref value, nullValue);
            }

            public Bool AndWithLeftNull()
            {
                if (_override.Value != null)
                    return _override.Value.AndWithLeftNull();
                return _delegated.AndWithLeftNull();
            }

            public void ApplyTo(UserMethods uInstance)
            {
                uInstance.SetNullStrategy(this);
            }

            public void OverrideAndCalculate(Action what)
            {
                _delegated.OverrideAndCalculate(what);
            }

            public bool UseBlankTextInsteadOfNullInDynamicSQL()
            {
                return _delegated.UseBlankTextInsteadOfNullInDynamicSQL();
            }
        }

        static ContextStatic<NullStrategyOverrideValue> _nullOverride = new ContextStatic<NullStrategyOverrideValue>(() => new NullStrategyOverrideValue());
        internal static INullStrategy GetStrategy(bool useInsteadOfNull)
        {
            if (useInsteadOfNull)
                return new NullStrategyOverrideDelegate(InsteadOfNullStrategy.Instance, _nullOverride.Value);
            else
                return new NullStrategyOverrideDelegate(NullStrategy.Instance, _nullOverride.Value);
        }


    }

    class InsteadOfNullStrategy : NullStrategy
    {
        public static void ApplyToUserInstanceForTestingOnly(params ColumnBase[] cols)
        {
            ApplyToUserInstance(Instance, (IEnumerable<ColumnBase>)cols);
        }
        public static void ApplyToUserInstanceForTestingOnly(IEnumerable<ColumnBase> cols)
        {
            foreach (var column in cols)
            {
                var c = column as IENVColumn;
                if (c != null)
                {
                    c.SetNullStrategy(Instance);
                }
            }
        }
        public new static InsteadOfNullStrategy Instance = new InsteadOfNullStrategy();
        public override T AdjustSetValue<T>(T value)
        {
            if (NullBehaviour.IsNull(value))
                return null;
            return value;
        }
        public override T GetValue<T>(TypedColumnBase<T> numberColumn, T value, InsteadOfNullValue<T> insteadOfNullValue)
        {
            if (ReferenceEquals(value, null))
            {
                return NullBehaviour.GetInsteadOfValue(insteadOfNullValue);
            }
            else
                return value;
        }

        public override bool CompareforNullValue(object value, object originalValue)
        {
            if (!BackwardCompatible.NullBehaviour.IsNull(value) ||
                            BackwardCompatible.NullBehaviour.IsNull(originalValue))
                return true;
            return false;
        }

        public override bool DoInsteadOfNull()
        {
            return true;
        }

        public override object MemoryParameterResult(object result)
        {
            if (ReferenceEquals(result, null))
                return InvalidNullValue.Instance;
            return result;
        }

        public override Bool EqualsUntyped(object a, object b)
        {
            if (!UserSettings.Version10Compatible)
                if (a is InvalidNullValue || b is InvalidNullValue)
                    return false;

            if (NullBehaviour.IsNull(a))
                if (NullBehaviour.IsNull(b))
                    return true;
                else
                {
                    return false;
                }
            else if (NullBehaviour.IsNull(b))
                return false;
            return Equals(a, b);
        }

        protected override Bool ColumnEqualNullImp(ColumnBase column)
        {

            if (UserSettings.Version10Compatible)
            {
                if (NullBehaviour.IsNull(column.Value))
                    return true;
                else
                {
                    var tc = column as Firefly.Box.Data.TextColumn;
                    if (tc != null && Firefly.Box.Text.IsNullOrEmpty(tc))
                        return true;
                }
            }

            else
            {
                {
                    var bc = column as Firefly.Box.Data.BoolColumn;
                    if (bc != null)
                    {
                        return bc.Value == NullBool;
                    }
                }
                {
                    var bc = column as Firefly.Box.Data.NumberColumn;
                    if (bc != null)
                    {
                        return bc.Value == NullNumber;
                    }
                }
                {
                    var bc = column as Firefly.Box.Data.TextColumn;
                    if (bc != null)
                    {
                        return bc.Value == NullText;
                    }
                }

                {
                    var bc = column as Firefly.Box.Data.TimeColumn;
                    if (bc != null)
                    {
                        return bc.Value == NullTime;
                    }
                }
                return ReferenceEquals(column.Value, null);
            }


            return false;
        }

        public override Bool Equals(object a, object b)
        {
            if (!UserSettings.Version10Compatible)
                if (a is InvalidNullValue || b is InvalidNullValue)
                    return false;

            bool aIsCol = false;
            var c = a as ColumnBase;
            if (c != null)
            {
                if (NullBehaviour.IsNull(b) && !(b is ColumnBase))
                    return ColumnEqualNullImp(c);
                aIsCol = true;
                var ba = c as ByteArrayColumn;
                if (ba != null && (b is string || b is Text || b is TextColumn))
                    a = (Text)ba.ToString();
                else
                    a = c.Value;
            }
            c = b as ColumnBase;
            if (c != null)
            {
                if (NullBehaviour.IsNull(a) && !aIsCol)
                    return ColumnEqualNullImp(c);
                var ba = c as ByteArrayColumn;
                if (ba != null && (a is string || a is Text || a is TextColumn))
                    b = (Text)ba.ToString();
                else

                    b = c.Value;

            }




            if (NullBehaviour.IsNull(a) && NullBehaviour.IsNull(b) && UserSettings.Version10Compatible)
                return true;
            object val = null;
            object nullVal = null;
            bool doit = false;
            if (NullBehaviour.IsNull(a))
            {
                nullVal = a;
                val = b;
                doit = true;
            }
            else if (NullBehaviour.IsNull(b))
            {
                nullVal = b;
                val = a;
                doit = true;
            }
            if (doit)
            {
                if (ReferenceEquals(nullVal, null))
                {
                    if (val != null)
                    {
                        {

                            {
                                Time n;
                                if (Firefly.Box.Time.TryCast(val, out n))
                                    return
                                        n.Equals(NullTime);
                            }

                            {
                                Date n;
                                if (Firefly.Box.Date.TryCast(val, out n))
                                    return
                                        n.Equals(NullDate);
                            }


                            {
                                Number n;
                                if (Number.TryCast(val, out n))
                                    return
                                        n.Equals(NullNumber);
                            }
                            {
                                Bool bo;
                                if (Bool.TryCast(val, out bo))
                                    return
                                        bo.Equals(NullBool);
                            }
                            {
                                Text n;
                                if (Firefly.Box.Text.TryCast(val, out n))
                                    return
                                        n.Equals(NullText);
                            }
                        }
                    }
                }
                else
                    return nullVal.Equals(val);


            }




            if (ReferenceEquals(a, null))
                if (ReferenceEquals(b, null))
                    return true;
                else
                    return false;
            else if (ReferenceEquals(b, null))
                return false;



            return Comparer.Equal(a, b);
        }

        public override bool ReturnValueIfNull<T>(ref object value, T nullValue)
        {
            if (ReferenceEquals(value, null))
            {
                value = nullValue;
                return true;
            }
            return false;
        }

        public override Bool AndWithLeftNull()
        {
            return false;
        }

        internal class InvalidNullValue : NullValue
        {
            public static InvalidNullValue Instance = new InvalidNullValue();
        }


        public override void ApplyTo(UserMethods uInstance)
        {
            ApplyToUserInstance(this, uInstance);
        }

        public override bool UseBlankTextInsteadOfNullInDynamicSQL()
        {
            return true;
        }
    }
}
