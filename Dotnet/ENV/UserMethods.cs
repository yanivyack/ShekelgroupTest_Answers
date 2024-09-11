using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using ENV.BackwardCompatible;
using ENV.Data;
using ENV.Data.DataProvider;
using ENV.Data.Storage;
using ENV.IO;
using ENV.IO.Advanced;
using ENV.Java;
using ENV.Remoting;
using ENV.Security;
using ENV.UI;
using ENV.Utilities;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Firefly.Box.Data.UnderConstruction;
using Firefly.Box.Flow;
using ENV.Printing;
using Firefly.Box.Testing;
using Firefly.Box.UI.Advanced;
using WizardOfOz.Witch.Engine;
using ControlBase = Firefly.Box.UI.Advanced.ControlBase;
using Form = Firefly.Box.UI.Form;
using TabControl = Firefly.Box.UI.TabControl;
using System.IO;
using ByteArrayColumn = ENV.Data.ByteArrayColumn;
using Comparer = Firefly.Box.Advanced.Comparer;
using Entity = ENV.Data.Entity;
using Grid = ENV.Printing.Grid;
using GridColumn = ENV.Printing.GridColumn;
using NumberFormatInfo = Firefly.Box.Data.Advanced.NumberFormatInfo;
using SearchOption = System.IO.SearchOption;
using SearchScope = System.DirectoryServices.SearchScope;
using TextPrinterWriter = ENV.Printing.TextPrinterWriter;
using TreeView = Firefly.Box.UI.TreeView;
using XmlWriter = ENV.UI.XmlWriter;
using Types = Firefly.Box;


namespace ENV
{
    public partial class UserMethods
    {
        static DateTime _lastInput = DateTime.Now;

        INullStrategy _nullStrategy = NullStrategy.GetStrategy(false);

        internal void SetNullStrategy(INullStrategy n)
        {
            if (ReferenceEquals(this, Instance))
                throw new InvalidOperationException();
            _nullStrategy = n;
        }

        public T UseDefaultInsteadOfNull<T>(Func<T> what)
        {
            T result = default(T);
            InsteadOfNullStrategy.Instance.OverrideAndCalculate(() => result = what());
            return result;
        }


        public void DenyUndoFor(ColumnBase col)
        {
            ITask t = this._indexOfHelper.ControllerOf(col);
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(t, c =>
            {
                var uic = c as AbstractUIController;
                if (uic != null)
                    uic.DenyUndoForCurrentRow();
                foreach (var item in c._boundParameters)
                {
                    item.DenyUndoFor(col, this);
                }


            });
        }

        #region conditional

        public Text If(Bool cond, Func<Text> trueValue, Text falseValue)
        {
            if (cond == null)
            {

                return null;
            }
            return cond ? trueValue() : falseValue;
        }
        public Text If(Bool cond, Func<Text> trueValue, Func<ByteArrayColumn> falseValue)
        {
            if (cond == null)
            {

                return null;
            }
            return cond ? trueValue() : (Text)falseValue().ToString();
        }
        public Text If(Bool cond, Text trueValue, Func<Text> falseValue)
        {
            if (cond == null)
            {
                falseValue();
                return null;
            }
            return cond ? trueValue : falseValue();
        }
        public byte[] If(Bool cond, Func<byte[]> trueValue, byte[] falseValue)
        {
            if (cond == null)
            {

                return null;
            }
            return cond ? trueValue() : falseValue;
        }
        public Text If(Bool cond, Func<byte[]> trueValue, Text falseValue)
        {
            if (cond == null)
            {

                return null;
            }
            return cond ? ToText(trueValue()) : falseValue;
        }
        public Number If(Bool cond, Number trueValue, Number falseValue)
        {
            if (cond == null) return null;
            return cond ? trueValue : falseValue;
        }
        public DateTime If(Bool cond, DateTime trueValue, Date falseValue)
        {

            return cond ? trueValue : ToDateTime(falseValue);
        }
        public DateTime If(Bool cond, DateTime trueValue, DateColumn falseValue)
        {
            return If(cond, trueValue, (Date)falseValue);
        }
        public DateTime If(Bool cond, Date trueValue, DateTime falseValue)
        {

            return cond ? ToDateTime(trueValue) : falseValue;
        }
        public DateTime If(Bool cond, DateColumn trueValue, DateTime falseValue)
        {

            return If(cond, (Date)trueValue, falseValue);
        }

        public Role If(Bool cond, Role trueValue, Role falseValue)
        {
            if (cond == null) return null;
            return cond ? trueValue : falseValue;
        }

        public object IfUntyped(Bool cond, object trueValue, object falseValue)
        {
            if (cond == null) return null;
            return cond ? trueValue : falseValue;
        }



        public object IfUntyped(Bool cond, Func<object> trueValue, Func<object> falseValue)
        {
            if (cond == null)
            {
                falseValue();
                return null;
            }
            return cond ? trueValue() : falseValue();
        }




        public Text If(Bool cond, Text trueValue, Text falseValue)
        {
            if (cond == null) return null;
            return cond ? trueValue : falseValue;
        }
        public Text If(Bool cond, Text trueValue, ByteArrayColumn falseValue)
        {
            return If(cond, trueValue, falseValue == null ? null : (Text)falseValue.ToString());


        }
        public Text If(Bool cond, TextColumn trueValue, ByteArrayColumn falseValue)
        {
            return If(cond, trueValue == null ? null : trueValue.Value, falseValue == null ? null : falseValue.ToString());
        }
        public Text If(Bool cond, Text trueValue, byte[] falseValue)
        {
            if (cond == null) return null;
            return cond ? trueValue : TextColumn.FromByteArray(falseValue);
        }



        public dataType[] If<dataType>(Bool cond, dataType[] trueValue, dataType[] falseValue)
        {
            if (cond == null) return null;
            return cond ? trueValue : falseValue;
        }

        public dataType[] If<dataType>(Bool cond, Types.Data.ArrayColumn<dataType> trueValue,
                                       Types.Data.ArrayColumn<dataType> falseValue)
        {
            if (cond == null) return null;
            return cond ? trueValue : falseValue;
        }

        public dataType[] If<dataType>(Bool cond, Types.Data.ArrayColumn<dataType> trueValue,
                                       dataType[] falseValue)
        {
            if (cond == null) return null;
            return cond ? trueValue : falseValue;
        }

        public dataType[] If<dataType>(Bool cond, dataType[] trueValue,
                                       Types.Data.ArrayColumn<dataType> falseValue)
        {
            if (cond == null) return null;
            return cond ? trueValue : falseValue;
        }

        public byte[] If(Bool cond, ByteArrayColumn trueValue, ByteArrayColumn falseValue)
        {
            return If(cond, trueValue == null ? null : trueValue.Value, falseValue == null ? null : falseValue.Value);

        }


        public byte[] If(Bool cond, ByteArrayColumn trueValue, byte[] falseValue)
        {
            return If(cond, trueValue == null ? null : trueValue.Value, falseValue);
        }

        public byte[] If(Bool cond, byte[] trueValue, ByteArrayColumn falseValue)
        {
            return If(cond, trueValue, falseValue == null ? null : falseValue.Value);
        }

        public byte[] If(Bool cond, byte[] trueValue, Text falseValue)
        {
            if (cond == null) return null;
            return cond ? trueValue : TextColumn.ToByteArray(falseValue);
        }
        public byte[] If(Bool cond, ENV.Data.ByteArrayColumn trueValue, Text falseValue)
        {
            return If(cond, trueValue == null ? null : trueValue.Value, falseValue);
        }

        public byte[] If(Bool cond, Func<Text> trueValue, Func<byte[]> falseValue)
        {
            if (cond == null)
            {
                falseValue();
                return null;
            }
            return cond ? TextColumn.ToByteArray(trueValue()) : falseValue();
        }

        public byte[] If(Bool cond, Func<byte[]> trueValue, Func<Text> falseValue)
        {
            if (cond == null)
            {
                falseValue();
                return null;
            }
            return cond ? trueValue() : TextColumn.ToByteArray(falseValue());
        }
        public byte[] If(Bool cond, Func<byte[]> trueValue, Func<ByteArrayColumn> falseValue)
        {
            if (cond == null)
            {
                falseValue();
                return null;
            }
            return cond ? trueValue() : falseValue();
        }
        public byte[] If(Bool cond, Func<ByteArrayColumn> trueValue, Func<Text> falseValue)
        {
            if (cond == null)
            {
                falseValue();
                return null;
            }
            return cond ? trueValue() : TextColumn.ToByteArray(falseValue());
        }
        public Bool If(Bool cond, Bool trueValue, Bool falseValue)
        {
            if (cond == null) return null;
            return cond ? trueValue : falseValue;
        }

        public Bool If(Bool cond, Func<Bool> trueValue, Func<Bool> falseValue)
        {
            if (cond == null)
            {
                falseValue();
                return null;
            }
            return cond ? trueValue() : falseValue();
        }
        public Bool IfUntyped(Bool cond, Action trueValue, Action falseValue)
        {
            if (cond == null) return null;
            if (cond)
                trueValue();
            else
                falseValue();
            return false;
        }

        public T If<T>(Bool cond, Func<T> trueValue, Func<T> falseValue)
        {
            if (cond == null)
            {
                falseValue();
                return default(T);
            }
            if (cond)
                return trueValue();
            else
                return falseValue();
            return default(T);
        }


        public Text If(Bool cond, Func<Text> trueValue, Func<Text> falseValue)
        {
            if (cond == null)
            {
                falseValue();
                return null;
            }
            return cond ? trueValue() : falseValue();
        }

        public Number If(Bool cond, Func<Number> trueValue, Func<Number> falseValue)
        {
            if (cond == null)
            {
                falseValue();
                return null;
            }
            return cond ? trueValue() : falseValue();
        }

        public Date If(Bool cond, Func<Date> trueValue, Func<Date> falseValue)
        {
            if (cond == null)
            {
                falseValue();
                return null;
            }
            return cond ? trueValue() : falseValue();
        }

        public Time If(Bool cond, Func<Time> trueValue, Func<Time> falseValue)
        {
            if (cond == null)
            {
                falseValue();
                return null;
            }
            return cond ? trueValue() : falseValue();
        }

        public Time If(Bool cond, Time trueValue, Time falseValue)
        {
            if (cond == null) return null;
            return cond ? trueValue : falseValue;
        }

        public Date If(Bool cond, Date trueValue, Date falseValue)
        {
            if (cond == null) return null;
            return cond ? trueValue : falseValue;
        }
        public Date If(Bool cond, DateColumn trueValue, Date falseValue)
        {
            return If(cond, trueValue == null ? null : trueValue.Value, falseValue);
        }
        public Date If(Bool cond, Date trueValue, DateColumn falseValue)
        {
            return If(cond, trueValue, falseValue == null ? null : falseValue.Value);
        }
        public Date If(Bool cond, DateColumn trueValue, DateColumn falseValue)
        {
            return If(cond, trueValue == null ? null : trueValue.Value, falseValue == null ? null : falseValue.Value);
        }
        public DateTime If(Bool cond, DateTime trueValue, DateTime falseValue)
        {
            if (cond == null) return default(DateTime);
            return cond ? trueValue : falseValue;
        }

        /*public object If(Bool cond, object trueValue, object falseValue)
        {
            if (cond == null) return null;
            return cond ? trueValue : falseValue;
        }*/

        public Bool Not(Bool val)
        {
            return !val;
        }

        public Bool In(Number item, params Number[] values)
        {
            return DoIn(item, values);
        }

        public Bool In(Bool item, params Bool[] values)
        {
            return DoIn(item, values);
        }

        public Bool In(Text item, params Text[] values)
        {
            return DoIn(item, values);
        }

        public Bool In(Date item, params Date[] values)
        {
            return DoIn(item, values);
        }

        public Bool In(Time item, params Time[] values)
        {
            return DoIn(item, values);
        }

        public Bool In(object item, params object[] values)
        {
            return DoIn(item, values);
        }

        static Bool DoIn<DataType>(DataType item, DataType[] values)
        {
            bool result = false;
            foreach (DataType c in values)
            {
                if (item == null)
                {
                    if (c == null)
                        return true;
                }
                else
                {
                    if (c == null)
                        return null;
                    if (Comparer.Compare(item, c) == 0)
                        result = true;
                }
            }
            if (item == null)
                return null;
            return result;
        }

        public Bool RangeUntyped(object value, object minValue, object maxValue)
        {
            return Compare(value, minValue) >= 0 && Compare(value, maxValue) <= 0;
        }

        public Bool Range(Number value, Number minValue, Number maxValue)
        {
            return DoRange(value, minValue, maxValue);
        }

        public Bool Range(Text value, Text minValue, Text maxValue)
        {
            return DoRange(value, minValue, maxValue);
        }

        public Bool Range(Bool value, Bool minValue, Bool maxValue)
        {
            return DoRange(value, minValue, maxValue);
        }

        public Bool Range(Date value, Date minValue, Date maxValue)
        {
            return DoRange(value, minValue, maxValue);
        }

        public Bool Range(Time value, Time minValue, Time maxValue)
        {
            return DoRange(value, minValue, maxValue);
        }

        public Bool Range3(Number minValue, Number maxValue, Number value)
        {
            return Range(value, minValue, maxValue);
        }

        static Bool DoRange(DataTypeBase value, DataTypeBase minValue, DataTypeBase maxValue)
        {
            if (IsNull(value, minValue, maxValue))
                return null;
            return value.CompareTo(minValue) >= 0 && value.CompareTo(maxValue) <= 0;
        }





        public Role Case(Number item, Number value1, Role result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return (Role)DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, null);
        }

        public Text Case(Number item, Number value1, Func<Text> result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return (Text)DoCase<Number, Text>(item, value1, result1, andSoOnWithDefaultAtTheEnd, Types.Text.Empty);
        }

        public Number Case(Number item, Number value1, Number result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Number.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Number.Zero));
        }
        public Number Case(Number item, Number value1, Func<Number> result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Number.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Number.Zero));
        }

        public Bool Case(Number item, Number value1, Bool result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Bool.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (Bool)false));
        }

        public Bool Case(Number item, Number value1, Func<Bool> result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Bool.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (Bool)false));
        }

        public Bool Case(Text item, Text value1, Func<Bool> result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Bool.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (Bool)false));
        }

        public Text Case(Text item, Text value1, Func<Text> result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Types.Text.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Types.Text.Empty));
        }

        public Text Case(Number item, Number value1, Text result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Types.Text.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Types.Text.Empty));
        }
        public Text Case(Number item, Func<Number> value1, Text result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Types.Text.Cast(DoCase(item, value1(), result1, andSoOnWithDefaultAtTheEnd, Types.Text.Empty));
        }

        public byte[] Case(Number item, Number value1, ENV.Data.ByteArrayColumn result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return CastToByteArray(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (byte[])null));
        }
        public byte[] Case(Number item, Number value1, byte[] result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return CastToByteArray(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (byte[])null));
        }
        public byte[] Case(Number item, Number value1, Firefly.Box.Data.ByteArrayColumn result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return CastToByteArray(DoCase(item, value1, result1.Value, andSoOnWithDefaultAtTheEnd, (byte[])null));
        }

        public Date Case(Number item, Number value1, Date result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return
                Types.Date.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Types.Date.Empty));
        }

        public Time Case(Number item, Number value1, Time result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return
                Types.Time.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd,
                                             Types.Time.StartOfDay));
        }

        public object Case(Number item, Number value1, object result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, 0);
        }

        public Bool Case(Date item, Date value1, Bool result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Bool.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (Bool)false));
        }

        public Text Case(Date item, Date value1, Text result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Types.Text.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Types.Text.Empty));
        }

        public Date Case(Date item, Date value1, Date result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return
                Types.Date.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Types.Date.Empty));
        }

        public Time Case(Date item, Date value1, Time result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return
                Types.Time.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd,
                                             Types.Time.StartOfDay));
        }

        public object Case(Date item, Date value1, object result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, null);
        }

        public Number Case(Date item, Date value1, Number result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Number.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Number.Zero));
        }

        public Bool Case(Time item, Time value1, Bool result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Bool.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (Bool)false));
        }

        public Text Case(Time item, Time value1, Text result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Types.Text.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Types.Text.Empty));
        }

        public byte[] Case(Time item, Time value1, byte[] result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return CastToByteArray(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (byte[])null));
        }

        public byte[] Case(Date item, Date value1, byte[] result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return CastToByteArray(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (byte[])null));
        }

        public Date Case(Time item, Time value1, Date result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return
                Types.Date.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Types.Date.Empty));
        }

        public Time Case(Time item, Time value1, Time result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return
                Types.Time.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd,
                                             Types.Time.StartOfDay));
        }

        public object Case(Time item, Time value1, object result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, null);
        }
        public Number Case(Time item, Time value1, Number result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Number.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, null));
        }

        public Number Case(Bool item, Bool value1, Number result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Number.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Number.Zero));
        }

        public Bool Case(Bool item, Bool value1, Bool result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Bool.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (Bool)false));
        }

        public Text Case(Bool item, Bool value1, Text result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Types.Text.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Types.Text.Empty));
        }

        public byte[] Case(Bool item, Bool value1, byte[] result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return CastToByteArray(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (byte[])null));
        }

        public Date Case(Bool item, Bool value1, Date result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return
                Types.Date.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Types.Date.Empty));
        }

        public Time Case(Bool item, Bool value1, Time result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return
                Types.Time.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd,
                                             Types.Time.StartOfDay));
        }

        public object Case(Bool item, Bool value1, object result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, null);
        }

        public Number Case(Text item, Text value1, Number result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Number.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Number.Zero));
        }

        public Bool Case(Text item, Text value1, Bool result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Bool.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (Bool)false));
        }

        public Text Case(Text item, Text value1, Text result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Types.Text.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Types.Text.Empty));
        }

        public byte[] Case(Text item, Text value1, byte[] result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return CastToByteArray((byte[])(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (byte[])null)));
        }

        public byte[] Case(Text item, string value1, byte[] result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return CastToByteArray(DoCase(item, (Text)value1, result1, andSoOnWithDefaultAtTheEnd, (byte[])null));
        }

        public byte[] Case(Text item, Text value1, ByteArrayColumn result1,
                                    params object[] andSoOnWithDefaultAtTheEnd)
        {
            return CastToByteArray(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (ByteArrayColumn)null));
        }

        public byte[] Case(Text item, string value1, ByteArrayColumn result1,
                                    params object[] andSoOnWithDefaultAtTheEnd)
        {
            return
                CastToByteArray
                (DoCase(item, (Text)value1, result1, andSoOnWithDefaultAtTheEnd, (ByteArrayColumn)null));
        }

        public Date Case(Text item, Text value1, Date result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return
                Types.Date.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Types.Date.Empty));
        }

        public Time Case(Text item, Text value1, Time result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return
                Types.Time.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd,
                                             Types.Time.StartOfDay));
        }

        public object Case(Text item, Text value1, object result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, null);
        }

        public Number Case(Text item, Text value1, Func<Number> result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Number.Cast(DoCase<Text, Number>(item, value1, result1, andSoOnWithDefaultAtTheEnd, null));
        }
        public Date Case(Text item, Text value1, Func<Date> result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Types.Date.Cast(DoCase<Text, Date>(item, value1, result1, andSoOnWithDefaultAtTheEnd, null));
        }

        public Number Case(object item, object value1, Number result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Number.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Number.Zero));
        }

        public Bool Case(object item, object value1, Bool result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Bool.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (Bool)false));
        }

        public Text Case(object item, object value1, Text result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return Types.Text.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Types.Text.Empty));
        }

        public byte[] Case(object item, object value1, byte[] result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return CastToByteArray(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, (byte[])null));
        }

        public Date Case(object item, object value1, Date result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return
                Types.Date.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, Types.Date.Empty));
        }

        public Time Case(object item, object value1, Time result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return
                Types.Time.Cast(DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd,
                                             Types.Time.StartOfDay));
        }

        public object Case(object item, object value1, object result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, null);
        }
        public object CaseUntyped(object item, object value1, object result1, params object[] andSoOnWithDefaultAtTheEnd)
        {
            return DoCase(item, value1, result1, andSoOnWithDefaultAtTheEnd, null);
        }

        static object DoCase<ValueType, ResultType>(ValueType item, ValueType value1, ResultType result1,
                                                    object[] andSoOnWithDefaultAtTheEnd, ResultType alternativeDefault)
        {
            if (Comparer.Compare(item, value1) == 0) return ReturnValue<ResultType>(result1);
            return CaseOtherValues(item, andSoOnWithDefaultAtTheEnd, alternativeDefault);
        }


        static object CaseOtherValues<ValueType, ResultType>(ValueType item, object[] andSoOnWithDefaultAtTheEnd,
                                                             ResultType alternativeDefault)
        {
            if (andSoOnWithDefaultAtTheEnd == null)
                return null;
            for (int i = 0; i < (andSoOnWithDefaultAtTheEnd.Length / 2) * 2; i += 2)
            {
                if (Comparer.Compare(item, ReturnValue<ValueType>(andSoOnWithDefaultAtTheEnd[i])) == 0)
                {
                    var val = andSoOnWithDefaultAtTheEnd[i + 1];
                    return ReturnValue<ResultType>(val);
                }
            }
            if (andSoOnWithDefaultAtTheEnd.Length % 2 == 0)
            {
                return alternativeDefault;
            }

            {
                var val = andSoOnWithDefaultAtTheEnd[andSoOnWithDefaultAtTheEnd.Length - 1];
                return ReturnValue<ResultType>(val);
            }

        }

        private static object ReturnValue<ResultType>(object val)
        {
            var retFunc = val as Func<ResultType>;
            if (retFunc != null)
                val = retFunc();
            if (val != null)
            {
                var m = val.GetType().GetProperty("Method");
                if (m != null && val.GetType().GetGenericTypeDefinition() == typeof(System.Func<>))
                {
                    val = ((System.Reflection.MethodInfo)m.GetValue(val, new object[0])).Invoke(val.GetType().GetProperty("Target").GetValue(val, new object[0]), new object[0]);
                }
            }
            //   
            if (typeof(ResultType) == typeof(bool))
            {
                var rf = val as Func<Bool>;
                if (rf != null)
                    return (bool)rf();
            }
            var col = val as ColumnBase;
            if (col != null)
                return col.Value;
            return val;
        }

        static object DoCase<ValueType, ResultType>(ValueType item, ValueType value1, Func<ResultType> result1,
                                                    object[] andSoOnWithDefaultAtTheEnd, ResultType alternativeDefault)
        {

            if (Comparer.Compare(item, value1) == 0) return result1();
            return CaseOtherValues(item, andSoOnWithDefaultAtTheEnd, alternativeDefault);
        }

        static String WildCardToRegular(String value)
        {
            return "^" + System.Text.RegularExpressions.Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        public Bool Like(Text source, Text compareString)
        {
            if (IsNull(source, compareString))
                return null;
            source = source.TrimEnd();
            var s = compareString.TrimEnd().ToString();
            return System.Text.RegularExpressions.Regex.IsMatch(source, WildCardToRegular(compareString));
            if (s.EndsWith("?"))
            {
                while (s.EndsWith("?"))
                    s = s.Remove(s.Length - 1);
                s += "*";
            }
            if (s.StartsWith("*"))
                if (s.EndsWith("*"))
                    return source.Contains(s.Replace("*", ""));
                else
                    return source.ToString().EndsWith(s.Replace("*", ""));
            else if (s.EndsWith("*"))
                return source.ToString().StartsWith(s.Replace("*", ""));
            return source == s;

            return false;
        }

        #endregion

        #region Number

        public Number Val(Text text, Text format)
        {
            if (text == null || format == null) return null;

            var result = Number.Parse(text, format);
            if (format.Trim().Length == 0 && result < 0)
                return result * -1;
            return result;
        }


        public Number Val3(Text text)
        {
            return Number.Parse(text);
        }

        public Text Str(Number number, Text format)
        {
            if (number == null || format == null) return null;
            if (Types.Text.IsNullOrEmpty(format))
                format = "100";
            var nf = new NumberFormatInfo(format);
            if (nf.MaxLength > 100)
                return new Text("*", 100);
            return number.ToString(format);
        }


        public Text Str3(Number number, Number x, Number y)
        {
            if (x < 1)
                return "";
            if (Math.Abs(y) == 4096)
            {
                return number.ToString(x + "P0");
            }
            if (y > 0)
            {
                return number.ToString((x - y - 1) + "." + y);
            }
            return number.ToString(x.ToString());

        }
        internal static void ResetRand()
        {
            _rand = null;
        }
        static Random _rand;

        public Number Rand(Number sid)
        {
            if (sid == null)
                return null;
            switch ((int)sid)
            {
                case 0:
                    if (_rand == null)
                        _rand = new Random(0);
                    break;
                case -1:
                    _rand = new Random(Guid.NewGuid().GetHashCode());
                    break;
                default:
                    _rand = new Random(sid);
                    break;
            }
            return _rand.NextDouble();
        }

        public Number Fix(Number number, Number numberOfWholeDigits, Number numberOfDecimalDigits)
        {
            if (number == null || numberOfWholeDigits == null || numberOfDecimalDigits == null) return null;
            string whole, dec = "", s;
            bool negative = false, hasDecimalValues = false;
            ;
            s = number.ToDecimal().ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (s.Contains("-"))
            {
                negative = true;
                s = s.Remove(s.IndexOf("-"), 1);
            }

            if (s.Contains("."))
            {
                whole = s.Remove(s.IndexOf('.'));
                dec = s.Substring(s.IndexOf('.') + 1);
                hasDecimalValues = true;
            }
            else
            {
                whole = s;
            }

            while (whole.Length > 0 && whole.Length > numberOfWholeDigits)
            {
                whole = whole.Substring(1);
            }
            if (numberOfDecimalDigits >= 0)
                while (dec.Length > numberOfDecimalDigits)
                {

                    dec = dec.Remove(dec.Length - 1);

                }
            else
            {
                if (-numberOfDecimalDigits < whole.Length)
                    whole = whole.Remove(whole.Length + numberOfDecimalDigits) + new string('0', -numberOfDecimalDigits);
                else
                    whole = "0";
            }
            if (whole == "") whole = "0";
            if (dec == "") dec = "0";
            if (hasDecimalValues && numberOfDecimalDigits > 0)
            {
                return decimal.Parse(whole + "." + dec, CultureInfo.InvariantCulture) * (negative ? -1 : 1);
            }
            else
            {
                return decimal.Parse(whole, CultureInfo.InvariantCulture) * (negative ? -1 : 1);
            }
        }

        public Number Abs(Number number)
        {
            if (number == null)
                return null;
            if (number < 0)
                return -number;
            return number;

        }

        public Number Min(Number value, params Number[] values)
        {
            return DoMinMax(1, value, values);
        }
        public object MinUntyped(object value, params object[] values)
        {
            return DoMinMax(1, value, values);
        }
        public object MaxUntyped(object value, params object[] values)
        {
            return DoMinMax(-11, value, values);
        }

        static Type DoMinMax<Type>(int compareResult, Type firstItem, Type[] values)
        {
            if (Instance.IsNull(values) || firstItem == null)
                return default(Type);
            Type result = firstItem;
            foreach (Type d in values)
                if (Comparer.Compare(result, d).CompareTo(0) == compareResult)
                    result = d;
            return result;
        }

        public Number Max(Number value, params Number[] values)
        {
            return DoMinMax(-1, value, values);
        }

        public Bool Min(Bool value, params Bool[] values)
        {
            return DoMinMax(1, value, values);
        }

        public Bool Max(Bool value, params Bool[] values)
        {
            return DoMinMax(-1, value, values);
        }

        public Text Min(Text value, params Text[] values)
        {
            return DoMinMax(1, value, values);
        }

        public Text Max(Text value, params Text[] values)
        {
            return DoMinMax(-1, value, values);
        }

        public Date Min(Date value, params Date[] values)
        {
            return DoMinMax(1, value, values);
        }

        public Date Max(Date value, params Date[] values)
        {
            return DoMinMax(-1, value, values);
        }

        public Time Min(Time value, params Time[] values)
        {
            return DoMinMax(1, value, values);
        }


        public Time Max(Time value, params Time[] values)
        {
            return DoMinMax(-1, value, values);
        }
        public Number Mod(Number value, Number devider)
        {
            if (IsNull(value, devider))
                return 0;
            return value % devider;

        }
        public Number Pow(Number value, Number power)
        {
            if (IsNull(value, power))
                return null;
            try
            {
                if (power % 1 == 0 && power > 0)
                {
                    Number result = 1;
                    for (int i = 0; i < power; i++)
                    {
                        result *= value;
                    }
                    return result;
                }
                return Math.Pow((double)value, power);
            }
            catch
            {
                return Number.Zero;
            }
        }

        public Number Log(Number value)
        {
            try
            {
                if (value == Number.Zero)
                    return Number.Zero;
                return Math.Log(value);
            }
            catch
            {
                return Number.Zero;

            }
        }



        public Number HVal(Text value)
        {
            if (InternalIsNull(value))
                return null;
            try
            {
                var val = value.Trim().ToString().Trim();
                if (val.Length == 0)
                    return 0;
                Number multiplier = 1;
                if (val.StartsWith("-"))
                {
                    multiplier = -1;
                    val = val.Substring(1);
                }
                if (val.StartsWith("0x"))
                    val = val.Substring(2);
                return multiplier * long.Parse(val.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            catch
            {
                return 0;
            }
        }

        public Number Round(Number value, Number wholeDigits, Number decimalDigits)
        {
            if (IsNull(value, wholeDigits, decimalDigits))
                return null;
            if (decimalDigits < 0)
            {
                for (int i = 0; i < decimalDigits * -1; i++)
                {
                    value /= 10;
                }
            }
            if (wholeDigits > 20)
                wholeDigits = 20;
            var result = System.Math.Round((value % this.Pow(10, (int)wholeDigits)).ToDecimal(),
                                           Math.Max(0, (int)decimalDigits), MidpointRounding.AwayFromZero);
            if (decimalDigits < 0)
            {
                for (int i = 0; i < decimalDigits * -1; i++)
                {
                    result *= 10;
                }
            }
            return result;
        }


        public Number DBRound(Number value, Number presicion)
        {
            if (IsNull(value, presicion))
                return null;
            return Round(value, 28, presicion);
        }

        public Number ChkDgt(Text source, Number algorithm)
        {
            if (source == null || algorithm == null)
                return null;

            if (source.Length % 4 > 0)
                source = source.ToString().PadLeft(source.Length + 4 - source.Length % 4, ' ');

            switch ((int)algorithm)
            {
                case 0:
                    {
                        int i = 0;
                        int sum = 0;
                        foreach (char c in source)
                        {
                            if (char.IsDigit(c))
                            {
                                int j = c - '0';
                                if (i % 2 == 1)
                                {
                                    j *= 2;
                                    if (j > 9)
                                        j -= 9;
                                }
                                sum += j;
                            }
                            i++;
                        }

                        if (sum % 10 == 0)
                            return 0;
                        return 10 - sum % 10;
                    }
                case 1:
                    {
                        return 0;
                    }
                default:
                    return 0;
            }
        }

        public Text HStr(Number value)
        {
            if (InternalIsNull(value))
                return null;
            string prefix = "";
            var a = (long)value.ToDecimal();
            if (a < 0)
            {
                prefix = "-";
                a *= -1;
            }

            return prefix + (a).ToString("X");
        }


        public Text MStr(Number number, int length)
        {
            if (length > 20)
                throw new NotImplementedException("Mstr with length greated then 20");
            if (length < 2)
                length = 2;
            var l = new byte[length];
            try
            {
                if (number != 0)
                {
                    decimal val = number;
                    byte i = 64;
                    if (val < 0)
                    {
                        i += 128;
                        val *= -1;
                    }

                    while (((long)val) > 0)
                    {
                        val /= (decimal)100;
                        i++;
                    }

                    if (val > 0)
                        while ((long)(val * 100) == 0)
                        {
                            val *= (decimal)100;
                            i--;
                        }



                    l[0] = i;
                    int j = 1;
                    while (val != 0 && j < length)
                    {
                        val = val * (decimal)100;
                        l[j] = (byte)(val % 100);
                        val -= l[j];
                        j++;
                    }

                }
            }
            catch
            {
            }

            var result = new StringBuilder();
            foreach (var b in l)
            {
                result.Append(Chr(b));
            }
            return result.ToString();
        }

        public Number MVal(Text value)
        {
            if (value == null)
                return 0;
            if (value.Length == 0)
                return 0;
            Number result = 0;
            Number multiplier = 1;
            try
            {
                Number m = 0;

                bool first = true;
                foreach (var c in value)
                {
                    var val = Asc(c.ToString());
                    if (first)
                    {
                        if (val > 128)
                        {
                            multiplier = -1;
                            val -= 128;
                        }
                        m = val - 65;
                        first = false;
                    }
                    else
                    {
                        result += val * Pow(100, m);
                        m--;
                    }
                }
            }
            catch
            {
            }
            return result * multiplier;
        }

        #endregion

        #region Text

        public Text AStr(Text text, Text format)
        {
            if (text == null || format == null)
                return null;
            return TextFormatInfo.ToFormattedText(text, format, new MyTextFormatHelper()).TrimEnd();
        }
        class MyTextFormatHelper : TextFormatHelper
        {
            public override string ToLower(string x)
            {
                return x.ToLowerInvariant();
            }
            public override string ToUpper(string x)
            {
                return x.ToUpperInvariant();
            }
        }

        public Text Trim(byte[] text)
        {
            if (text == null)
                return null;
            return Trim(TextColumn.FromByteArray(text));
        }
        public Text ByteArrayToText(byte[] value)
        {
            return TextColumn.FromByteArray(value);
        }

        public Text Trim(ENV.Data.ByteArrayColumn column)
        {
            if (column == null)
                return null;
            var x = column.ToString();
            if (x == null)
                return BackwardCompatible.NullBehaviour.NullText;
            return Trim(column.ToString());
        }
        public byte[] TrimBlob(ENV.Data.ByteArrayColumn column)
        {
            if (column == null)
                return null;
            var x = column.ToString();
            if (x == null)
                return null;
            return column.FromString(x);
        }
        /// <summary>
        /// Used to wrap calls made by `CallProg` in the original application, that couldn't return null char
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Text RemoveZeroChar(Text text)
        {
            if (ReferenceEquals(text, null))
                return "";
            int i = text.IndexOf('\0');
            if (i < 0)
                return text;
            return text.Remove(i);

        }
        /// <summary>
        /// Used to wrap calls made by `CallProg` in the original application, that couldn't return null 
        /// </summary>
        public object ReplaceNullWithDefault(object value)
        {
            return value ?? "";
        }
        /// <summary>
        /// Used to wrap calls made by `CallProg` in the original application, that couldn't return null 
        /// </summary>
        public Number ReplaceNullWithDefault(Number value)
        {
            return value ?? 0;
        }

        public static bool TrimTrailingNulls { get; set; }
        public Text Trim(Text text)
        {
            if (text == null)
                return null;
            if (BackwardCompatible.NullBehaviour.IsNull(text))
                return NullText;
            if (text is ErrorText)
                return text;
            if (TrimTrailingNulls)
                text = RemoveZeroChar(text);
            var r = text.Trim(' ');
            if (JapaneseMethods.Enabled)
                r = r.Trim((char)12288);
            return r;
        }

        public Text LTrim(Text t)
        {
            if (InternalIsNull(t))
                return null;
            if (BackwardCompatible.NullBehaviour.IsNull(t))
                return NullText;
            return t.TrimStart(' ');
        }

        public Text LTrim(byte[] text)
        {
            if (InternalIsNull(text))
                return null;
            if (BackwardCompatible.NullBehaviour.IsNull(text))
                return NullText;
            return TextColumn.FromByteArray(text).TrimStart(' ');
        }
        public Text LTrim(ByteArrayColumn text)
        {
            if (InternalIsNull(text))
                return null;
            if (BackwardCompatible.NullBehaviour.IsNull(text))
                return NullText;
            var x = text.ToString();
            if (x == null)
                return BackwardCompatible.NullBehaviour.NullText;
            return x.TrimStart(' ');
        }


        public byte[] Serialize(object value)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(ms, value);
                    return ms.ToArray();
                }
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "");
                return new byte[0];
            }
        }

        public T Deserialize<T>(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                var bf = new BinaryFormatter();
                var result = bf.Deserialize(ms);
                return (T)result;
            }
        }

        public Text TrimEnd(Text text)
        {
            return RTrim(RemoveZeroChar(text));
        }
        public Text TrimEnd(byte[] text)
        {
            return RTrim(RemoveZeroChar(TextColumn.FromByteArray(text)));
        }
        public Text TrimEnd(ByteArrayColumn text)
        {
            return RTrim(RemoveZeroChar(text));
        }
        public Text RTrim(Text text)
        {
            if (InternalIsNull(text))
                return null;
            if (BackwardCompatible.NullBehaviour.IsNull(text))
                return NullText;
            return text.TrimEnd(' ');
        }
        public Text RTrim(byte[] text)
        {
            if (InternalIsNull(text))
                return null;
            if (BackwardCompatible.NullBehaviour.IsNull(text))
                return NullText;
            return TextColumn.FromByteArray(text).TrimEnd(' ');
        }
        public Text RTrim(ByteArrayColumn text)
        {
            if (InternalIsNull(text))
                return null;
            if (BackwardCompatible.NullBehaviour.IsNull(text))
                return NullText;
            var x = text.ToString();
            if (x == null)
                return BackwardCompatible.NullBehaviour.NullText;
            return x.TrimEnd(' ');
        }

        public static bool EmptyInstrReturnsOne { get; set; }

        public Number InStr(Text text, Text textToLookFor)
        {
            if (text == null || textToLookFor == null)
                if (!ENV.BackwardCompatible.NullBehaviour.EqualToNullReturnsNull)
                    return 0;
                else
                    return null;
            if (textToLookFor.Length == 0)
                if (EmptyInstrReturnsOne)
                    return 1;
                else

                    return 0;
            return text.IndexOf(textToLookFor) + 1;
        }
        public Number InStr(byte[] text, Text textToLookFor)
        {
            return InStr(TextColumn.FromByteArray(text), textToLookFor);
        }
        public Number InStr(ByteArrayColumn text, Text textToLookFor)
        {
            if (ReferenceEquals(text, null))
                return InStr((Text)null, textToLookFor);
            return InStr(text.ToString(), textToLookFor);
        }
        [NotYetImplemented]
        public Text MidV(Text text, Number fromPosition, Number length)
        {
            return Mid(text, fromPosition, length);
        }

        public Text Mid(byte[] text, Number fromPosition, Number length)
        {
            return Mid(TextColumn.FromByteArray(text), fromPosition, length);
        }
        public Text Mid(ByteArrayColumn text, Number fromPosition, Number length)
        {
            return Mid(text.ToString(), fromPosition, length);
        }

        public Text Mid(Text text, Number fromPosition, Number length)
        {
            if (object.ReferenceEquals(text, null) || object.ReferenceEquals(fromPosition, null) ||
                object.ReferenceEquals(length, null))
                return null;
            if (JapaneseMethods.Enabled)
                return JapaneseMethods.Mid(text, fromPosition, length);
            return text.Substring(((int)fromPosition) - 1, length);
        }


        public Text Upper(Text text)
        {
            if (text == null) return null;
            if (text is ErrorText)
                return text;
            var ca = text.TrimEnd().ToString().ToCharArray();

            for (int i = 0; i < ca.Length; i++)
            {
                if (HebrewTextTools.V8HebrewOem)
                    ca[i] = OnlyEnglishUpper(ca[i]);
                else
                    ca[i] = Char.ToUpperInvariant(ca[i]);
            }
            return new Text(new string(ca)).PadRight(text.Length);

        }
        public Text Upper(ByteArrayColumn text)
        {
            return Upper(text.ToString());
        }
        public Text Upper(byte[] text)
        {
            return Upper(TextColumn.FromByteArray(text));
        }
        internal static char OnlyEnglishUpper(char c)
        {
            if (c >= 'a' && c <= 'z')
                return (char)('A' - 'a' + c);
            return c;
        }


        public Text Right(Text text, Number numOfChars)
        {
            if (text == null || numOfChars == null)
                return null;
            if (JapaneseMethods.Enabled)
                return JapaneseMethods.Mid(text, text.Length - numOfChars + 1, numOfChars);
            return text.Right(numOfChars);
        }
        public Text Right(byte[] text, Number numOfChars)
        {
            if (text == null || numOfChars == null)
                return null;
            return Right(TextColumn.FromByteArray(text), numOfChars);
        }
        public Text Right(ByteArrayColumn text, Number numOfChars)
        {
            if (text == null || numOfChars == null)
                return null;
            return Right(text.ToString(), numOfChars);
        }
        public Text StrToken(byte[] text, Number partNumber, Text separator)
        {
            return StrToken(TextColumn.FromByteArray(text), partNumber, separator);
        }
        public Text StrToken(ByteArrayColumn text, Number partNumber, Text separator)
        {
            return StrToken(text.ToString(), partNumber, separator);
        }
        public Text StrToken(Text text, Number partNumber, Text separator)
        {
            if (text == null || partNumber == null || separator == null)
                return null;
            text = RemoveZeroChar(text);
            var trimmedText = text.TrimEnd();

            string[] splited = text.TrimEnd(' ').ToString().Split(new string[] { Trim(separator) }, StringSplitOptions.None);

            if (partNumber > 0 && partNumber <= splited.Length)
                try
                {
                    if (partNumber < splited.Length)
                        return splited[partNumber - 1];
                    else
                        return ((Text)splited[partNumber - 1]).PadRight(text.Length - trimmedText.Length + splited[partNumber - 1].Length);
                }
                catch
                {
                }
            return "";
        }

        public Text StrBuild(Text format, params Text[] args)
        {
            if (format == null)
                return null;
            if (args == null)
                return null;
            foreach (var item in args)
            {
                if (item == null)
                    return null;
            }
            string workString = format;
            int indexOfBackslash = workString.IndexOf("\\@");
            string suffix = "";
            if (indexOfBackslash != -1)
            {
                suffix = workString.Substring(indexOfBackslash + 1);
                workString = workString.Remove(indexOfBackslash);
            }

            bool inValue = false;
            var result = new StringBuilder();
            var value = new StringBuilder();

            foreach (var c in workString)
            {
                if (c == '@')
                {
                    if (inValue)
                    {
                        int i = -1;
                        try
                        {
                            i = int.Parse(value.ToString().Substring(1));
                        }
                        catch
                        {
                        }
                        if (i > 0 && i <= args.Length)
                            result.Append(args[i - 1].Trim());
                        else
                            result.Append(value + "@");

                        value.Remove(0, value.Length);
                        inValue = false;
                    }
                    else
                    {
                        inValue = true;
                        value.Append(c);
                    }
                }
                else
                {
                    if (inValue)
                        value.Append(c);
                    else
                        result.Append(c);
                }
            }
            var outResult = result.ToString() + value + suffix;
            if (outResult.EndsWith("\\"))
            {
                outResult = outResult.Substring(0, outResult.Length - 1);
            }
            return outResult;
        }

        public Number StrTokenCnt(Text source, Text delimiter)
        {
            if (IsNull(source, delimiter))
                return null;
            source = RemoveZeroChar(source);
            if (source.Trim().Length == 0)
                return Number.Zero;
            string[] splited = source.ToString().Split(new string[] { RTrim(delimiter) }, StringSplitOptions.None);
            return splited.Length;
        }

        public Number StrTokenIdx(Text source, Text locate, Text separator)
        {
            if (source == null || locate == null || separator == null)
                return null;
            source = RemoveZeroChar(source);
            if (Types.Text.IsNullOrEmpty(source))
                return 0;
            string[] splited = source.ToString().Split(new string[] { separator }, StringSplitOptions.None);
            int i = 1;
            foreach (string s in splited)
            {
                if (locate.Equals(s)) return i;
                i++;
            }
            return Number.Zero;
        }

        public Number Len(Text text)
        {
            if (text == null)
                return null;
            return text.Length;
        }
        public Number Len(byte[] text)
        {
            if (text == null)
                return null;
            return TextColumn.FromByteArray(text).Length;
        }
        public Number Len(ByteArrayColumn text)
        {
            if (text == null || text.Value == null)
                return null;
            return text.ToString().Length;
        }
        public Text Left(byte[] text, Number numOfChars)
        {
            return Left(TextColumn.FromByteArray(text), numOfChars);
        }

        public Text Left(ByteArrayColumn text, Number numOfChars)
        {
            return Left(text.ToString(), numOfChars);
        }
        public Text Left(Text text, Number numOfChars)
        {
            if (text == null || numOfChars == null)
                return null;
            if (JapaneseMethods.Enabled)
                return JapaneseMethods.Mid(text, 1, numOfChars);
            return text.Substring(0, numOfChars);
        }


        public Text Lower(Text originalText)
        {
            if (originalText == null)
                return null;
            var ca = originalText.TrimEnd().ToString().ToCharArray();
            for (int i = 0; i < ca.Length; i++)
            {
                if (HebrewTextTools.V8HebrewOem)
                    ca[i] = OnlyEnglishLower(ca[i]);
                else
                    ca[i] = Char.ToLowerInvariant(ca[i]);
            }
            return new Text(new string(ca)).PadRight(originalText.Length);
        }
        internal static char OnlyEnglishLower(char c)
        {
            if (c >= 'A' && c <= 'Z')
                return (char)('a' - 'A' + c);
            return c;
        }

        internal static char CharTypedByUserWithUFormatToLower(char c)
        {
            if (ENV.UserSettings.VersionXpaCompatible)
                return Char.ToLower(c);
            return OnlyEnglishLower(c);
        }

        public Text Flip(Text text)
        {
            if (text == null)
                return null;
            return text.Reverse();
        }

        public Text Del(Text originalText, Number fromPosition, Number length)
        {
            if (originalText == null || fromPosition == null || length == null)
                return null;
            return originalText.Remove(fromPosition - 1, length);
        }

        public Text RepStr(Text sourceText, Text find, Text replace)
        {
            if (sourceText == null || find == null || replace == null)
                return null;
            sourceText = RemoveZeroChar(sourceText);
            if (find.Length == 0)
                return sourceText;
            return sourceText.Replace(find, replace);
        }
        public Text RepStr(byte[] ba, Text find, Text replace)
        {
            if (ba == null || find == null || replace == null)
                return null;
            var sourceText = TextColumn.FromByteArray(ba);
            sourceText = RemoveZeroChar(sourceText);
            if (find.Length == 0)
                return sourceText;
            return sourceText.Replace(find, replace);
        }
        public Text RepStr(ByteArrayColumn ba, Text find, Text replace)
        {
            if (ba == null || ba.Value == null || find == null || replace == null)
                return null;
            var sourceText = ba.ToString();
            sourceText = RemoveZeroChar(sourceText);
            if (find.Length == 0)
                return sourceText;
            return sourceText.Replace(find, replace);
        }



        public Text Ins(Text originalText, Text Text2Insert, Number position, Number length)
        {
            if (originalText == null || Text2Insert == null || position == null || length == null)
                return null;
            return originalText.Insert(Text2Insert, position - 1, length);
        }

        public Text Rep(Text originalText, Text textToInsert, Number position, Number length)
        {
            if (originalText == null || textToInsert == null || position == null || length == null)
                return null;
            if (JapaneseMethods.Enabled)
                return JapaneseMethods.Rep(originalText, textToInsert, position, length);
            string tToInsert = textToInsert.Substring(0, length);
            Text result = originalText.Remove(position - 1, tToInsert.Length);
            return result.Insert(tToInsert, position - 1, tToInsert.Length);
        }

        public Text Fill(Text text, Number numberOfRepeats)
        {
            if (IsNull(text, numberOfRepeats))
                return null;
            return new Text(text, numberOfRepeats);
        }

        public Text CRC(Text source, Number algorithem)
        {
            Crc16 crc = new Crc16();
            byte[] param = ENV.LocalizationInfo.Current.InnerEncoding.GetBytes(source.ToString().ToCharArray());
            var result = crc.ComputeChecksumBytes(param);
            return new string(ENV.LocalizationInfo.Current.InnerEncoding.GetChars(new[] { result[1], result[0] }));
        }

        static UserMethods()
        {
            var x = System.Windows.Forms.Application.ExecutablePath;
            __applicationPrefix = System.IO.Path.GetDirectoryName(x) + "\\" + System.IO.Path.GetFileName(x).Remove(2);
            Form.UserInput += () => _lastInput = DateTime.Now;
            string s = "erבגדהוזחטיכלמנסעפצקרשתךףץםן";
            _hebrewChars.AddRange(s.ToCharArray());
            _hebrewChars.AddRange(HebrewOemTextStorage.Encode(s).ToString().ToCharArray());

            s = "abcdefghijklmnopqrstuvwxyz";
            _englishChars.AddRange(s.ToCharArray());
            _englishChars.AddRange(s.ToUpper(CultureInfo.InvariantCulture).ToCharArray());
        }

        static List<char> _hebrewChars = new List<char>();
        static List<char> _englishChars = new List<char>();

        public Text Visual(Text source, Bool type)
        {
            if (IsNull(source, type))
                return null;
            if (!LocalizationInfo.Current.SupportVisualLogicalFunctions)
                return source;
            if (type)
                return HebrewTextTools.VisualTrue(source);
            else
                return HebrewTextTools.VisualFalse(source);


        }


        public Text UnicodeChr(Number unicodeCode)
        {
            if (unicodeCode == null)
                return null;
            return ((char)(int)unicodeCode).ToString();
        }

        public Number UnicodeVal(Text theChar)
        {
            if (theChar == null)
                return null;
            if (theChar.Length == 0)
                return 0;
            return (int)theChar[0];
        }

        public Text Chr(Number asciiCode)
        {
            if (asciiCode == null)
                return null;
            byte[] n = new byte[] { (byte)(int)asciiCode.ToDecimal() };
            char[] c = ENV.LocalizationInfo.Current.InnerEncoding.GetChars(n);
            return c[0].ToString();
        }
        static int[] _ascMap = new int[Char.MaxValue];
        static Encoding _lastEncoding = null;
        public Number Asc(Text theChar)
        {
            if (theChar == null)
                return null;
            if (theChar.Length == 0)
                return 0;
            var currentEncoding = ENV.LocalizationInfo.Current.InnerEncoding;
            if (_lastEncoding != currentEncoding)
            {
                _lastEncoding = currentEncoding;
                _ascMap = new int[char.MaxValue];
            }
            var c = theChar[0];
            var result = _ascMap[c];
            if (result != 0)
            {
                return result;
            }
            {
                char[] ca = new char[] { c };
                byte[] n = currentEncoding.GetBytes(ca);
                result = n[0];
                _ascMap[c] = result;
                return result;
            }
        }

        public Number ASCIIVal(Text theChar)
        {
            return Asc(theChar);
        }

        public Text ASCIIChr(Number theChar)
        {
            return Chr(theChar);
        }

        public Text Logical(Text text, Bool type)
        {
            if (text == null || type == null)
                return null;
            if (!LocalizationInfo.Current.SupportVisualLogicalFunctions)
                return text;
            if (type)
                return Visual(text, true);
            return Visual(text, false);
        }

        public Text OEM2ANSI(Text source)
        {
            if (source == null)
                return null;
            return HebrewOemTextStorage.Decode(source);

        }

        public Text ANSI2OEM(Text source)
        {
            if (source == null)
                return null;
            return HebrewOemTextStorage.Encode(source);

        }

        public byte[] UnicodeToAnsi(Text source, Number codePage)
        {
            if (source == null || codePage == null)
                return null;
            try
            {
                if (codePage == 0 || codePage == 1)
                    return LocalizationInfo.Current.InnerEncoding.GetBytes(source.ToString().ToCharArray());
                else
                    return
                        System.Text.Encoding.GetEncoding(codePage).GetBytes(source.ToString().ToCharArray());

            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "");
                return null;
            }
        }

        public byte[] UTF8FromAnsi(ByteArrayColumn value)
        {
            if (value.Value == null)
                return null;
            return UTF8FromAnsi(value.ToString());
        }

        public byte[] UTF8FromAnsi(byte[] value)
        {
            if (value == null)
                return null;
            return System.Text.Encoding.UTF8.GetBytes(ENV.LocalizationInfo.Current.OuterEncoding.GetChars(value));
        }

        public byte[] UTF8FromAnsi(Text value)
        {
            if (value == null)
                return null;
            return System.Text.Encoding.UTF8.GetBytes(value.ToString().ToCharArray());
        }

        public byte[] UTF8ToAnsi(ByteArrayColumn value)
        {
            if (value.Value == null)
                return null;
            return UTF8ToAnsi(value.Value);
        }
        public byte[] UTF8ToAnsi(byte[] value)
        {
            if (value == null)
                return null;
            return ENV.LocalizationInfo.Current.OuterEncoding.GetBytes(System.Text.Encoding.UTF8.GetChars(value));
        }
        public byte[] UTF8ToAnsi(Text value)
        {
            if (value == null)
                return null;
            return ENV.LocalizationInfo.Current.OuterEncoding.GetBytes(value.ToString().ToCharArray());
        }

        public Text MarkedTextGet()
        {
            var task = GetTaskByGeneration(0) as UIController;
            if (task != null)
            {
                var form = task.View;
                var focused = form != null ? (form is ENV.UI.Form ? ((ENV.UI.Form)form).FocusedControl : form.FocusedControl) : null;
                if (focused != null)
                {

                    {
                        var tb = focused as Firefly.Box.UI.TextBox;

                        if (tb != null && tb.Data != null && tb.Data.Column != null)
                        {
                            return tb.SelectedText;
                        }
                    }
                    {
                        var tb = focused as Firefly.Box.UI.RichTextBox;

                        if (tb != null && tb.Data != null && tb.Data.Column != null)
                        {
                            return tb.SelectedText;

                        }
                    }

                }
            }
            return null;
        }


        public Bool MarkedTextSet(Text value)
        {
            var task = GetTaskByGeneration(0) as UIController;
            if (task != null)
            {
                var form = task.View;
                var focused = form != null ? (form is ENV.UI.Form ? ((ENV.UI.Form)form).FocusedControl : form.FocusedControl) : null;
                if (focused != null)
                {

                    {
                        var tb = focused as Firefly.Box.UI.TextBox;

                        if (tb != null && tb.Data != null && tb.Data.Column != null && tb.SelectionLength > 0)
                        {
                            tb.SelectedText = value.TrimEnd();
                            return true;
                        }
                    }
                    {
                        var tb = focused as Firefly.Box.UI.RichTextBox;

                        if (tb != null && tb.Data != null && tb.Data.Column != null && tb.SelectionLength > 0)
                        {
                            tb.SelectedText = value.TrimEnd();
                            return true;

                        }
                    }

                }
            }
            return false;
        }

        #endregion

        #region Date

        public Text DStr(Date date, Text format)
        {
            if (format == null)
                return null;
            if (ReferenceEquals(date, null))
                return null;
            if (format == "MMMMMH" && !(ENV.LocalizationInfo.Current is HebrewLocalizationInfo))
                format = "MMMMM";
            return Types.Date.ToString(date, format) ?? Types.Text.Empty;
        }

        public Date AddDate(Date date, Number years, Number months, Number days)
        {
            try
            {
                if (IsNull(date, years, months, days))
                    return null;
                years = Math.Floor((decimal)years);
                months = Math.Floor((decimal)months);
                days = Math.Floor((decimal)days);

                if (date == Types.Date.Empty)
                {
                    Date result = new Date(1, 1, 1);
                    if (years == 0 && months == 0 && days > 0)
                        return result.AddDays(days - 1);

                    if (years < 1)
                    {
                        if (months < 13 || (months == 13 && days < 2))
                            return Types.Date.Empty;
                        if (months > 13)
                            return result.AddMonths(months - 13).AddDays(days - 2);
                    }
                    if (years == 1)
                    {
                        if (months < 1 || (months == 1 && days < 1))
                            return Types.Date.Empty;
                    }

                    int totalMonths = 12 * years + months - 13;
                    result = result.AddMonths(totalMonths).AddDays(days - 1);
                    if (days == 0)
                        result = result.AddDays(-1);
                    return result;
                }

                int leftMonths = date.Year * 12 + date.Month + years * 12 + months;
                if (leftMonths <= 12)
                    return Types.Date.Empty;

                var r = date;
                try
                {
                    r = r.AddMonths(years * 12 + months);

                }
                catch (ArgumentOutOfRangeException)
                {
                    int maxMonths = 120000;
                    if (r.Year * 12 + months > maxMonths)
                        r = new Types.Date(9999, 12, 31);
                }

                r = r.AddDays(days);
                return r;
            }
            catch
            {
                return Types.Date.Empty;
            }
        }

        public Bool AddDateTime(Number indexOfDateColumn, Number indexOfTimeColumn, Number years, Number months,
                                Number days, Number hours, Number minutes, Number seconds)
        {
            var dc = GetColumnByIndex(indexOfDateColumn) as Types.Data.DateColumn;
            var tc = GetColumnByIndex(indexOfTimeColumn) as Types.Data.TimeColumn;
            if (dc == null || tc == null)
                return false;
            var newDate =
                ToNumber(ToNumber(AddDate(dc, years, months, days)) * 86400 + AddTime(tc, hours, minutes, seconds));
            dc.Value = ToDate(Fix(newDate / 86400, 18, 0));
            tc.Value = ToTime((newDate % 86400));
            return true;


        }

        public Number DOW(Date date)
        {
            if (date == null)
                return null;
            if (date <= Types.Date.Empty)
                return 0;
            return (int)date.DayOfWeek + 1;
        }

        public Date Date()
        {
            return Types.Date.Now;
        }

        public Date BOM(Date date)
        {
            if (InternalIsNull(date))
                return null;
            if (date <= Types.Date.Empty)
                return Types.Date.Empty;
            return date.BeginningOfMonth;
        }

        public Date BOY(Date date)
        {
            if (InternalIsNull(date))
                return null;
            if (date <= Types.Date.Empty)
                return Types.Date.Empty;
            return date.BeginningOfYear;
        }

        public Date EOM(Date date)
        {
            if (InternalIsNull(date))
                return null;
            if (date <= Types.Date.Empty)
                return Types.Date.Empty;
            return date.EndOfMonth;
        }

        public Date EOY(Date date)
        {
            if (InternalIsNull(date))
                return null;
            if (date <= Types.Date.Empty)
                return Types.Date.Empty;
            return date.EndOfYear;
        }

        public Number Day(Date date)
        {
            if (InternalIsNull(date))
                return null;
            if (date <= Types.Date.Empty)
                return 0;
            return date.Day;
        }

        public Number Month(Date date)
        {
            if (InternalIsNull(date))
                return null;
            if (date <= Types.Date.Empty)
                return 0;
            return date.Month;
        }

        public Number Year(Date date)
        {
            if (InternalIsNull(date))
                return null;
            if (date <= Types.Date.Empty)
                return 0;
            return date.Year;
        }

        public Date DVal(Text date, Text format)
        {
            if (IsNull(date, format))
                return null;
            return Types.Date.Parse(date, format);
        }
        public Date DVal(byte[] dateb, Text format)
        {
            var date = this.ToText(dateb);
            if (IsNull(date, format))
                return null;
            return Types.Date.Parse(date, format);
        }
        public Date DVal(ByteArrayColumn dateb, Text format)
        {
            var date = dateb.ToString();
            if (IsNull(date, format))
                return null;
            return Types.Date.Parse(date, format);
        }

        public Date DVal3(Text date)
        {
            return Types.Date.Parse(date, "DD/MM/YY");
        }

        public Time TVal3(Text time)
        {
            return TVal(time, "HH:MM:SS");
        }

        [NotYetImplemented]
        public Text LSTR(Number format)
        {
            return "";
        }

        [NotYetImplemented]
        public Number LVAL(Text format)
        {
            return 0;
        }

        public Text DSTR3(Date value, Number format)
        {
            if (ReferenceEquals(value, null) || ReferenceEquals(format, null))
                return null;
            string form = Types.Date.SharedDefaultFormat;
            if (form.Contains("YYYY"))
                form = form.Replace("YYYY", "YY");

            switch ((int)format)
            {
                case 0:
                    break;
                case 1:
                    form = "MMMMMMM DD, YYYYT";
                    break;
                case 2:
                    form = "MMM DD, YYYY";
                    break;
                case 3:
                    form = "DD MMMMMMM, YYYYT";
                    break;
                case 4:
                    form = "DD MMM, YYYY";
                    break;
                default:
                    form = form.Replace("YY", "YYYY");
                    break;
            }
            return Types.Date.ToString(value, form);
        }

        public Text TSTR3(Time value, Number format)
        {
            if (ReferenceEquals(value, null) || ReferenceEquals(format, null))
                return null;
            return Types.Time.ToString(value, "HH:MM:SS");
        }

        static string[] _weekDays = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        public Text CMonth(Date date)
        {
            if (InternalIsNull(date))
                return null;
            if (date <= Types.Date.Empty)
                return "";
            return NMonth(date.Month);
        }

        public Text CDow(Date date)
        {

            if (InternalIsNull(date))
                return null;
            if (date <= Types.Date.Empty)
                return "";
            return NDOW(DOW(date));


        }

        public Text NDOW(Number numericDayOfWeek)
        {
            if (InternalIsNull(numericDayOfWeek))
                return null;
            if (numericDayOfWeek <= 0 || numericDayOfWeek > 7)
                return Types.Text.Empty;
            var result = Types.Date.DateTimeFormatInfo.GetDayName((DayOfWeek)(int)numericDayOfWeek - 1).Replace("יום ", "");
            return result[0].ToString().ToUpper() + result.Substring(1);

        }

        public Text NMonth(Number monthNumber)
        {
            if (InternalIsNull(monthNumber))
                return null;
            if (monthNumber < 1)
                return Types.Text.Empty;
            int i = (int)monthNumber;
            i = ((i - 1) % 12) + 1;
            var result = Types.Date.DateTimeFormatInfo.GetMonthName(i);
            return result[0].ToString().ToUpper() + result.Substring(1);
        }


        public Number Compare(object a, object b)
        {
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return null;
            return Comparer.Compare(a, b);
        }

        public DateTime ToDateTime(Date date, Time time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
        }
        public DateTime ToDateTime(Time time)
        {
            return new DateTime(1, 1, 1, time.Hour, time.Minute, time.Second);
        }

        public DateTime ToDateTime(Date date)
        {
            if (Types.Date.IsNullOrEmpty(date))
                return new Date(1901, 1, 1);
            return ToDateTime(date, Types.Time.StartOfDay);
        }


        public string ToString(Text value)
        {
            return RTrim(value);
        }

        public Text ToText(string text)
        {
            return text;
        }
        public Text ToText(char text)
        {
            return text.ToString();
        }
        public Text ToText(byte[] text)
        {
            return TextColumn.FromByteArray(text);
        }
        public Text ToRtfText(byte[] text)
        {
            return TextColumn.RtfFromByteArray(text);
        }
        public Text ToRtfText(ByteArrayColumn text)
        {
            return text.ToStringForRichTextBox(); ;
        }
        public Text ToRtfText(Text text)
        {
            return text;
        }
        public Text ToText(ByteArrayColumn text)
        {

            return text.ToString();
        }



        #endregion

        #region Time

        public Time Time()
        {
            return Types.Time.Now;
        }

        public Number mTime()
        {
            return (int)DateTime.Now.TimeOfDay.TotalMilliseconds;
        }

        public Time TVal(Text source, Text format)
        {
            if (source == null || format == null)
                return null;
            return Types.Time.Parse(source, format);
        }


        public Text TStr(Time time, Text format)
        {
            if (time == null || format == null)
                return null;
            return Types.Time.ToString(time, format);
        }

        public Number Hour(Time time)
        {
            if (InternalIsNull(time))
                return null;

            return time.Hour;
        }

        public Number Minute(Time time)
        {
            if (InternalIsNull(time))
                return null;
            return time.Minute;
        }

        public Number Second(Time time)
        {
            if (InternalIsNull(time))
                return null;
            return time.Second;
        }

        public Time AddTime(Time time, Number hours, Number minutes, Number seconds)
        {
            if (IsNull(time, hours, minutes, seconds))
                return null;
            return time.AddHours((int)hours.ToDecimal()).AddMinutes((int)minutes.ToDecimal()).AddSeconds(seconds);
        }
        public Time UTCTime()
        {
            return Types.Time.FromDateTime(DateTime.UtcNow);
        }
        public Number UTCmTime()
        {
            return (int)DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
        }
        public Date UTCDate()
        {
            return DateTime.UtcNow.Date;
        }

        #endregion

        #region Blobs

        public Bool Blb2File(ByteArrayColumn column, Text fileName)
        {
            Blb2FileDeleteBefore(fileName);
            if (IsNull(column, fileName))
                return null;
            if (InternalIsNull(column.Value))
                return null;
            if (column.Value.Length == 0)
                return false;
            if (UserSettings.Version10Compatible)
            {
                if (column.ContentType == ByteArrayColumnContentType.Unicode)
                {
                    return Blb2File(ByteArrayColumn.ToUnicodeBOMByteArray(column.Value), fileName);
                }
            }
            return Blb2File(column.Value, fileName);
        }

        public Bool ClientBlb2File(ByteArrayColumn column, Text fileName)
        {
            return Blb2File(column, fileName);
        }

        void Blb2FileDeleteBefore(Text fileName)
        {
            if (fileName == null)
                return;
            try
            {
                if (UserSettings.Version10Compatible)
                    if (IOExist(fileName))
                        FileDelete(fileName);
            }
            catch { }
        }
        public Bool Blb2File(byte[] value, Text fileName)
        {
            Blb2FileDeleteBefore(fileName);
            if (IsNull(value, fileName))
                return null;
            try
            {

                if (value.Length == 0)
                    return false;
                System.IO.File.WriteAllBytes(Translate(fileName), value);
                return true;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return false;
            }
        }

        public Bool Blb2File(ENV.Data.TextColumn column, Text fileName)
        {
            Blb2FileDeleteBefore(fileName);
            if (IsNull(column, fileName))
                return null;
            if (IsNull(column.Value))
                return null;
            if (Types.Text.IsNullOrEmpty(column))
                return false;
            if (column.StorageType == TextStorageType.Ansi)
            {

                return Blb2File(ByteArrayColumn.ToAnsiByteArray(column.Value), fileName);
            }
            else
                return Blb2File(column.Value, fileName);

        }

        public Bool Blb2File(Text value, Text fileName)
        {
            Blb2FileDeleteBefore(fileName);
            if (IsNull(value, fileName))
                return null;
            if (value.Length == 0)
                return false;

            return Blb2File(UserSettings.Version10Compatible
                                ? ByteArrayColumn.ToUnicodeBOMByteArray(value)
                                : ByteArrayColumn.ToAnsiByteArray(value), fileName);
        }

        public byte[] File2BlbKeepBOM(Text s)
        {
            return File2BlbInternal(s, true);
        }

        public byte[] File2Blb(Text s)
        {
            return File2BlbInternal(s, false);
        }

        public byte[] ClientFile2Blb(Text s)
        {
            return File2BlbInternal(s, false);
        }

        public byte[] File2BlbText(Text fileName)
        {
            var buffer = File2BlbInternal(fileName, true);
            if (buffer == null)
                return null;
            if (UserSettings.Version10Compatible && buffer.Length > 2 && buffer[0] == 255 && buffer[1] == 254)
            {
                var result = new byte[buffer.Length - 2];
                Array.Copy(buffer, 2, result, 0, result.Length);
                return result;
            }
            else if (UserSettings.Version10Compatible && buffer.Length > 3 && buffer[0] == 239 && buffer[1] == 187 && buffer[2] == 191)
            {
                var result = new byte[buffer.Length - 3];
                Array.Copy(buffer, 3, result, 0, result.Length);
                var ss = System.Text.Encoding.UTF8.GetString(result);
                return ByteArrayColumn.ToUnicodeByteArray(ss);
            }

            {
                var r = LocalizationInfo.Current.OuterEncoding.GetString(buffer);
                return ByteArrayColumn.ToUnicodeByteArray(r);
            }
        }

        byte[] File2BlbInternal(Text s, bool keepBOM)
        {
            if (InternalIsNull(s))
                return null;
            try
            {
                var a = Translate(s);
                if (!System.IO.File.Exists(a))
                    return null;
                byte[] buffer;
                using (var stream = new FileStream(a, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var offset = 0;
                    var length = stream.Length;
                    if (length > 0x7fffffffL)
                        throw new IOException("File too long (over 2GB)");

                    var count = (int)length;
                    buffer = new byte[count];
                    while (count > 0)
                    {
                        var bytesRead = stream.Read(buffer, offset, count);
                        offset += bytesRead;
                        count -= bytesRead;
                    }
                }
                if (UserSettings.Version10Compatible && !keepBOM)
                {
                    if (buffer.Length > 2 && buffer[0] == 255 && buffer[1] == 254)
                    {
                        var result = new byte[buffer.Length - 2];
                        Array.Copy(buffer, 2, result, 0, result.Length);
                        return result;
                    }
                    if (buffer.Length > 3 && buffer[0] == 239 && buffer[1] == 187 && buffer[2] == 191)
                    {
                        var result = new byte[buffer.Length - 3];
                        Array.Copy(buffer, 3, result, 0, result.Length);
                        return result;
                    }
                }
                return buffer;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return null;
            }
        }

        public Number BlobSize(byte[] a)
        {
            if (InternalIsNull(a))
                return 0;
            return GetBytesOfByteArrayColumn(a).Length;
        }
        public Number BlobSize(Text a)
        {
            if (InternalIsNull(a))
                return 0;
            return TextColumn.ToByteArray(a).Length;
        }
        public Number BlobSize(ByteArrayColumn a)
        {
            if (InternalIsNull(a) || InternalIsNull(a.Value))
                return 0;
            return GetBytesOfByteArrayColumn(a).Length;
        }

        public byte[] BlobToBase64(byte[] a)
        {

            if (InternalIsNull(a))
                return null;
            return LocalizationInfo.Current.OuterEncoding.GetBytes(Convert.ToBase64String(a));


        }

        public byte[] BlobToBase64(Text a)
        {

            if (InternalIsNull(a))
                return null;
            return BlobToBase64(LocalizationInfo.Current.OuterEncoding.GetBytes(a));


        }

        public byte[] BlobToBase64(ByteArrayColumn a)
        {

            if (InternalIsNull(a))
                return null;
            return BlobToBase64(a.Value);


        }

        public byte[] BlobFromBase64(Text a)
        {
            if (InternalIsNull(a))
                return null;
            try
            {
                return Convert.FromBase64String(a.TrimEnd());
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "");
                return new byte[0];
            }
        }

        public byte[] BlobFromBase64(byte[] a)
        {
            if (InternalIsNull(a))
                return null;
            return BlobFromBase64(LocalizationInfo.Current.OuterEncoding.GetString(a));
        }

        public byte[] BlobFromBase64(ENV.Data.ByteArrayColumn a)
        {
            if (InternalIsNull(a) || InternalIsNull(a.Value))
                return null;
            try
            {
                return Convert.FromBase64String(a.ToString());
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "");
                return new byte[0];
            }
        }

        public Number VariantType(byte[] val)
        {
            if (val == null)
                return null;
            try
            {
                using (var ms = new MemoryStream(val))
                {
                    var bf = new BinaryFormatter();
                    var result = bf.Deserialize(ms);
                    if (result.GetType() == typeof(System.Int16))
                        return 2;
                    if (result.GetType() == typeof(DateTime))
                        return 7;
                    if (result.GetType() == typeof(Int32))
                        return 3;
                    if (result.GetType() == typeof(string))
                        return 8;
                }

                return null;
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "");
                return null;
            }
        }

        #endregion

        #region vectors

        public arrayType VecGet<arrayType>(ENV.Data.ArrayColumn<arrayType> array, Number position)
            where arrayType : class
        {
            return VecGet(array.Value, position, array.BaseColumn.DefaultValue);
        }

        public arrayType VecGet<arrayType>(Types.Data.ArrayColumn<arrayType> array, Number position)
            where arrayType : class
        {
            return VecGet(array.Value, position);
        }

        public arrayType VecGet<arrayType>(arrayType[] array, Number position)
            where arrayType : class
        {
            return VecGet(array, position, default(arrayType));
        }
        public object VecGet(byte[] array, Number position)
        {
            object[] arr = ByteArrayToTypedArray(array);
            return VecGet<object>(arr, position, null);
        }
        public object VecGet(ByteArrayColumn array, Number position)
        {
            if (array == null)
                return null;
            return VecGet(array.Value, position);
        }

        arrayType VecGet<arrayType>(arrayType[] array, Number position, arrayType defaultValue)
            where arrayType : class
        {
            if (IsNull(position))
                return null;
            if (position < 1)
                return null;
            if (ReferenceEquals(array, null))
                if (!ReferenceEquals(defaultValue, null))
                    return defaultValue;
                else return null;
            if (array.Length < position)
                return defaultValue;

            return array[position - 1];
        }

        public object VecGet(object array, Number position)
        {
            if (IsNull(array, position))
                return null;
            var a = array as Array;
            if (a == null)
                return null;
            if (a.Length < position || position < 1)
                return null;
            return a.GetValue(position - 1);
        }


        public Text VecCellAttr(object column)
        {
            if (column == null)
                return null;
            if (column.GetType().IsArray)
            {
                var var = column.GetType().GetElementType();
                if (var == typeof(Bool))
                    return "L";
                if (var == typeof(Time))
                    return "T";
                if (var == typeof(Number))
                    return "N";
                if (var == typeof(Text))
                    return "A";
                if (var == typeof(Date))
                    return "D";
            }

            return "B";

        }



        public Bool VecSet(Number selectedColumnIndex, Number position, object value)
        {
            if (IsNull(selectedColumnIndex, position))
                return null;
            if (position < 1)
                return false;
            var p =
                GetColumnByIndex(selectedColumnIndex);

            if (p != null)
            {


                try
                {
                    return
                        (Bool)
                        (bool)p.GetType().GetMethod("TrySetValue").Invoke(p, new object[] { (int)position - 1, value });
                }
                catch (Exception e)
                {
                    ENV.ErrorLog.WriteToLogFile(e, "");
                    return false;
                }
            }
            else
                return false;
        }

        public Number VecSize<arrayType>(Types.Data.ArrayColumn<arrayType> array)
        {
            if (array == null)
                return -1;
            return array.Length;
        }

        public Number VecSize(byte[] array)
        {
            object[] arr = ByteArrayToTypedArray(array);
            return VecSize(arr);
        }
        public Number VecSize(ByteArrayColumn array)
        {
            if (array == null)
                return null;
            return VecSize(array.Value);
        }

        public Number VecSize(object array)
        {
            if (((object)array) == null)
                return 1;
            var a = array as Array;
            if (a == null)
                return -1;
            return a.Length;

        }

        #endregion

        #region database

        public Text NullText { get { return NullBehaviour.NullText; } }
        public Number NullNumber { get { return NullBehaviour.NullNumber; } }
        public Date NullDate { get { return NullBehaviour.NullDate; } }
        public Time NullTime { get { return NullBehaviour.NullTime; } }
        public Bool NullBool { get { return NullBehaviour.NullBool; } }


        public object Null()
        {
            return null;
        }

        public Text DBName(Number fileIndex)
        {
            return DBName(fileIndex, 1);
        }

        public Text DBName(Type entityType)
        {
            return DBName(entityType, 1);
        }


        public Bool SQLExecute(Text databaseName, Text SQL, params Number[] resultColumnsIndexes)
        {
            return SQLExecute(ConnectionManager.GetSQLDataProvider(databaseName.TrimEnd()), SQL, resultColumnsIndexes);
        }
        public Bool SQLExecute(DynamicSQLSupportingDataProvider dataProvider, Text SQL, params Number[] resultColumnsIndexes)
        {
            if (dataProvider == null)
                return null;
            if (SQL == null)
                return null;
            if (resultColumnsIndexes == null)
                return null;
            try
            {
                foreach (var item in resultColumnsIndexes)
                {
                    GetColumnByIndex(item).ResetToDefaultValue();
                }
                using (var c = dataProvider.CreateCommand())
                {
                    c.CommandText = SQL.TrimEnd();
                    List<object> result = null;
                    using (var r = c.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            result = new List<object>();
                            for (int i = 0; i < r.FieldCount; i++)
                            {
                                if (i < resultColumnsIndexes.Length)
                                    result.Add(r[i]);
                            }
                        }
                    }
                    if (result != null)
                    {
                        for (int i = 0; i < resultColumnsIndexes.Length; i++)
                        {
                            if (i < resultColumnsIndexes.Length)
                                VarSet(resultColumnsIndexes[i], result[i]);
                        }
                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex);
                return false;
            }
        }


        public Text DBName(Number fileIndex, Number infoType)
        {
            if (fileIndex == null || fileIndex == 0)
                return null;
            var entity = _application.AllEntities.GetByIndex(fileIndex);
            if (entity == null)
            {
                if (infoType == 4 && fileIndex < _application.AllEntities._entities.Keys.Max())
                    return "Table Not Used";
            }
            return InternalDbName(entity, infoType);
        }
        Dictionary<Type, Types.Data.Entity> _dbNameCache = new Dictionary<Type, Types.Data.Entity>();
        public Text DBName(Type entityType, Number infoType)
        {
            Types.Data.Entity e;
            if (!_dbNameCache.TryGetValue(entityType, out e))
            {
                e = System.Activator.CreateInstance(entityType) as Types.Data.Entity;
                _dbNameCache.Add(entityType, e);
            }
            return InternalDbName(e, infoType);
        }
        internal void ClearDBNameCache()
        {
            _dbNameCache.Clear();
        }
        public static bool TEMPRemoveOwnerFromDbName { get; set; }
        internal Text InternalDbName(Types.Data.Entity entity, Number infoType, bool donotRemoveDbo = false)
        {
            try
            {
                string result = "";
                if (entity != null)
                {
                    if (infoType == 2)
                        result = entity.Caption;
                    else if (infoType == 3)
                    {
                        var envE = entity as ENV.Data.Entity;
                        if (envE != null)
                        {
                            var db = envE.DataProvider as DynamicSQLSupportingDataProvider;
                            if (db != null)
                                return db.Name;
                            var b = envE.DataProvider as BtrieveDataProvider;
                            if (b != null)
                                return b.Name;
                            if (envE.DataProvider is MemoryDatabase)
                                return "Memory";
                            return envE.DataProvider.ToString();
                        }
                        return "database name";
                    }
                    else if (infoType == 4)
                    {
                        var envE = entity as ENV.Data.Entity;
                        if (envE != null)
                        {
                            var db = envE.DataProvider as DynamicSQLSupportingDataProvider;
                            if (db != null)
                            {
                                if (db.IsOracle)
                                    return "ORACLE";
                                return "MICROSOFTSQLSERVER";
                            }
                            var b = envE as BtrieveEntity;
                            if (b != null)
                                return "Btrieve";
                            if (envE.DataProvider is MemoryDatabase)
                                return "Memory";
                            return "SQL";
                            return "Other";
                        }
                    }
                    else
                    {

                        result = entity.EntityName;
                        var eb = entity as BtrieveEntity;
                        if (eb != null)
                        {
                            if (eb.BtrieveName.StartsWith(ENV.UserMethods.Instance.Pref() + "fil") && eb.Columns.Count == 0)//W9816
                                return "";
                            result = PathDecoder.DecodePath(eb.BtrieveName);
                        }
                    }
                }

                if (result.StartsWith("dbo.") && !donotRemoveDbo)
                    result = result.Remove(0, 4);
                var e = entity as ENV.Data.Entity;
                if (TEMPRemoveOwnerFromDbName && result.Contains('.') && e != null && e.DataProvider is DynamicSQLSupportingDataProvider)
                    result = result.Substring(result.LastIndexOf('.') + 1);

                return result;
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "DBName");
                return "";
            }
        }

        public Bool DBExist(Type entityType, Text tableName)
        {
            return InternalDBExist(System.Activator.CreateInstance(entityType) as Types.Data.Entity, tableName);
        }

        public Bool DBExist(Number fileIndex, Text tableName)
        {
            return InternalDBExist(_application.AllEntities.GetByIndex(fileIndex), tableName);
        }

        Bool InternalDBExist(Types.Data.Entity entity, Text tableName)
        {
            try
            {
                if (entity != null)
                {
                    if (tableName != null && tableName.TrimEnd() != string.Empty)
                        entity.EntityName = tableName;
                    return entity.Exists();
                }
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "DbExists");

            }
            return false;
        }

        public Bool DBCopy(Number fileIndex, Text originalTableName, Text newTableName)
        {
            try
            {
                if (IsNull(fileIndex, originalTableName, newTableName))
                    return null;

                var sourceEntity = _application.AllEntities.GetByIndex(fileIndex) as ENV.Data.Entity;
                var targetEntity = _application.AllEntities.GetByIndex(fileIndex) as ENV.Data.Entity;

                return InternalDbCopy(sourceEntity, targetEntity, originalTableName, newTableName);
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                if (IsNull(fileIndex, originalTableName, newTableName))
                    return null;
                return false;
            }
        }

        internal Bool InternalDbCopy(Entity sourceEntity, Entity targetEntity, Text originalTableName, Text newTableName)
        {
            if (!Types.Text.IsNullOrEmpty(originalTableName))
                sourceEntity.EntityName = originalTableName;
            if (!Types.Text.IsNullOrEmpty(newTableName))
                targetEntity.EntityName = newTableName;

            if (targetEntity.Exists())
                throw new InvalidOperationException("Target table exist: " + targetEntity.EntityName);

            targetEntity.AutoCreateTable = true;
            if (sourceEntity.DataProvider is DynamicSQLSupportingDataProvider)
            {
                ((DynamicSQLSupportingDataProvider)sourceEntity.DataProvider).CreateTable(targetEntity);
                new UserDbMethods(() => false).InsertAsSelect(sourceEntity, targetEntity);
                return true;
            }
            if (sourceEntity.DataProvider is BtrieveDataProvider)
            {
                return IOCopy(((BtrieveEntity)sourceEntity).GetFullBtrievePathAndName(),
                    ((BtrieveEntity)targetEntity).GetFullBtrievePathAndName());
            }
            if (sourceEntity.DataProvider is MemoryDatabase)
            {
                DeserializeEntity(targetEntity, SerializeEntity(sourceEntity));
                return true;
            }
            return false;
        }

        public Number DBRecs(Number fileIndex, Text tableName)
        {
            try
            {
                return InternalDBRecs(_application.AllEntities.GetByIndex(fileIndex), tableName);
            }
            catch
            {
                return Number.Zero;
            }
        }

        public Number DBRecs(Type entityType, Text tableName)
        {
            try
            {
                return InternalDBRecs(System.Activator.CreateInstance(entityType) as Types.Data.Entity, tableName);
            }
            catch
            {
                return Number.Zero;
            }
        }

        Number InternalDBRecs(Types.Data.Entity entity, Text tableName)
        {
            if (entity != null)
            {
                if (tableName != null && tableName.TrimEnd() != string.Empty)
                    entity.EntityName = tableName;
                return entity.CountRows();
            }
            return Number.Zero;
        }

        [NotYetImplemented]
        public Number DBSize(Number fileIndex, Text tableName)
        {
            try
            {
                var e = _application.AllEntities.GetByIndex(fileIndex);
                var btrieveEntity = e as BtrieveEntity;
                if (btrieveEntity != null)
                    return btrieveEntity.GetFileSize();
                return e.CountRows();
            }
            catch
            {
                return Number.Zero;
            }
        }

        Bool InternalIsNull(object value)
        {
            return (object)value == null;
        }

        public Bool IsNull(object value)
        {

            return NullBehaviour.IsNull(value);
        }



        public Bool IsNull(ColumnBase value)
        {
            if (ReferenceEquals(value, null))
                return null;
            return NullBehaviour.IsNull(value.Value);
        }

        public Bool DBDel(Number fileIndex, Text fileName)
        {
            try
            {
                if (fileIndex == 0 || fileIndex == null || fileName == null)
                    return null;
                return InternalDBDel(_application.AllEntities.GetByIndex(fileIndex), fileName);
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "DBDEL");
            }
            return false;
        }

        public Bool DBDel(Type entityType, Text fileName)
        {
            try
            {
                if (entityType == null || fileName == null)
                    return null;
                return InternalDBDel(System.Activator.CreateInstance(entityType) as Types.Data.Entity, fileName);


            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "DBDEL");
            }
            return false;
        }
        public Bool ClientDbDel(Number fileIndex, Text fileName)
        {
            return DBDel(fileIndex, fileName);
        }

        public static bool SuppressEntityExistenceCheckInDBDel;

        bool InternalDBDel(Types.Data.Entity entity, Text fileName)
        {
            if (entity != null)
            {
                foreach (var task in CurrentContext.ActiveTasks)
                {
                    foreach (var e in task.Entities)
                    {
                        if (e.GetType() == entity.GetType())
                            return false;
                    }
                }

                if (fileName != null && fileName.TrimEnd() != string.Empty)
                    entity.EntityName = fileName;
                if (SuppressEntityExistenceCheckInDBDel || entity.Exists())
                {
                    entity.Drop();
                    return true;
                }
                else
                    return false;
            }
            return false;
        }


        public Bool DBReload(Number fileIndex, Text fileName)
        {
            if (IsNull(fileIndex, fileName) || fileIndex == 0)
                return null;
            return DBReload(_application.AllEntities.GetByIndex(fileIndex), fileName);
        }
        public Bool DBReload(Type entityType, Text fileName)
        {
            if (IsNull(entityType, fileName))
                return null;
            return DBReload(System.Activator.CreateInstance(entityType) as Types.Data.Entity, fileName);
        }

        Bool DBReload(Types.Data.Entity entity, Text tableName)
        {
            try
            {
                var envEntity = entity as ENV.Data.Entity;
                if (envEntity != null)
                {
                    if (tableName != null && tableName.TrimEnd() != string.Empty)
                        envEntity.EntityName = tableName;
                    envEntity.ClearInMemoryData();
                    return true;
                }
                return false;

            }
            catch
            {
                return false;
            }
        }

        public Bool DbDiscnt(Text dbName)
        {
            if (dbName == null)
                return null;
            try
            {
                return ENV.Data.DataProvider.ConnectionManager.Disconnect(dbName);
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                Common.SetTemporaryStatusMessage(e.Message);
                return false;
            }
        }

        public Bool Rollback(Bool askUser, Number notsupported)
        {

            if (IsNull(askUser, notsupported))
                return null;
            if (InTrans())
            {
                var c = ENV.Common.GetTransactionOpenController();
                bool recover = c != null && ENV.Common.InDatabaseErrorRetry(c);
                if (askUser)
                {

                    if (
                        Common.ShowYesNoMessageBox("Are you sure you want to roll back the entire transaction?",
                                                   "Rollback", false))
                        throw new RollbackException(recover);

                }
                else
                    throw new RollbackException(recover);
            }
            return false;
        }

        #endregion

        #region task

        public Action SetContext(ITask task)
        {
            if (!Advanced.LevelProvider.IsCurrentHandlerIn(task) && !(task is ModuleController))
            {
                var t = _contextTask;
                _contextTask = task;
                return () => _contextTask = t;
            }
            else
                return () => { };
        }

        ITask _currentTask;
        ITask _contextTask;

        internal ITask GetTaskByGeneration(Number generation)
        {
            if (generation == THISConstant)
                generation = 0;
            int intGeneration = generation;
            var tasks = new List<ITask>(ControllerBase.GetActiveTasks(CurrentContext));
            if (intGeneration < 0)
                return null;
            int usedGenerations = 0;
            bool foundTask = false;
            if (_contextTask == null)
                foundTask = true;
            else
            {
                if (!new List<ITask>(tasks).Contains(_contextTask))
                {
                    foundTask = true;
                    intGeneration -= 1;
                    if (intGeneration == -1)
                        return _contextTask;
                }
            }
            ModuleController lastModuleController = null;
            for (int i = tasks.Count - 1; i >= 0; i--)
            {
                if (!foundTask)
                    foundTask = _contextTask == tasks[i];
                if (foundTask)
                {
                    lastModuleController = tasks[i] as ModuleController;
                    if (lastModuleController != null)
                        continue;
                    if (usedGenerations++ == intGeneration)
                        return tasks[i];
                }
            }
            if (intGeneration == usedGenerations && !ENV.UserSettings.Version8Compatible)
                return lastModuleController;
            return null;
        }

        public Number Counter(Number generation)
        {
            if (ReferenceEquals(generation, null)) return null;
            BusinessProcess task = GetTaskByGeneration(generation) as BusinessProcess;
            if (task != null)
                return BusinessProcessBase.AdjustCounter(task.Counter);
            return Number.Zero;
        }

        public Bool Stat(Number generation, Text status)
        {
            if (IsNull(generation, status))
                return null;
            ITask task = GetTaskByGeneration(generation);
            string statusToCompare = status.ToUpper(CultureInfo.InvariantCulture);
            if (task != null)
            {
                var x = task.Activity;

                bool returnFalse = false;
                ControllerBase.SendInstanceBasedOnTaskAndCallStack(task,
                    y =>
                    {
                        var z = y as AbstractUIController;
                        if (z != null)
                        {

                            if (generation == 0 && z._inOnLoad)
                                returnFalse = true;
                            x = z.Activity;



                        }
                    });
                if (returnFalse)
                    return false;

                if (task is ModuleController)
                    if (UserSettings.Version8Compatible)
                        return false;
                    else
                        x = Activities.Browse;
                switch (x)
                {
                    case Activities.Update:
                        return CheckContains(statusToCompare, "M", "ע");
                    case Activities.Insert:
                        return CheckContains(statusToCompare, "C", "ה");
                    case Activities.Delete:
                        return CheckContains(statusToCompare, "D", "מ");
                    case Activities.Browse:
                        return CheckContains(statusToCompare, "Q", "ד");
                    case (Activities)(-1):
                        return CheckContains(statusToCompare, "R", "ת");
                    case (Activities)(-2):
                        return CheckContains(statusToCompare, "L", "ח");
                    default:
                        return false;
                }
            }
            return false;
        }

        public Activities ActivityOfParent
        {
            get
            {
                ITask t = GetTaskByGeneration(1);
                if (t != null)
                    return t.Activity;
                return Activities.Browse;
            }
        }

        public Activities TranslateTaskActivity(Text text)
        {
            if (text == Command.SwitchToBrowseActivity)
                text = "Q";
            else if (text == Command.SwitchToUpdateActivity)
                text = "M";
            else if (text == Command.SwitchToInsertActivity)
                text = "C";
            foreach (var text1 in (text ?? "Q").ToUpper(CultureInfo.InvariantCulture).ToString().ToCharArray())
            {
                switch (text1.ToString())
                {
                    case "ע":
                    case "M":
                        return Activities.Update;
                    case "ד":
                    case "Q":
                        return Activities.Browse;
                    case "ה":
                    case "C":
                        return Activities.Insert;
                    case "מ":
                    case "D":
                        return Activities.Delete;
                    case "P":
                        {
                            var t = GetTaskByGeneration(1);
                            if (t != null)
                                return t.Activity;
                        }
                        break;
                }
            }

            return Activities.Browse;
        }

        static bool CheckContains(string statusToCompare, params string[] stringsToCompare)
        {
            foreach (string s in stringsToCompare)
            {
                if (statusToCompare.Contains(s))
                    return true;
            }
            return false;
        }

        Stack<int> _blockCounters = new Stack<int>();
        internal bool StartBlockLoopIfHasActiveLoop()
        {
            if (_blockCounters.Count > 0)
            {
                StartBlockLoop();
                return true;
            }
            return false;
        }
        public void StartBlockLoop()
        {
            _blockCounters.Push(0);
        }

        public bool AdvanceBlockLoop()
        {
            _blockCounters.Push(_blockCounters.Pop() + 1);
            return true;
        }

        public void EndBlockLoop()
        {
            _blockCounters.Pop();
        }

        public Number LoopCounter()
        {
            if (_blockCounters.Count > 0)
                return _blockCounters.Peek();
            return 0;
        }

        static string _sys;

        public static void SetSys(string sys)
        {
            _sys = sys;
        }


        public Text Sys()
        {
            if (_sys != null)
                return _sys;
            return (System.Reflection.Assembly.GetEntryAssembly()
                    ?? System.Reflection.Assembly.GetCallingAssembly()
                    ?? System.Reflection.Assembly.GetExecutingAssembly()).GetName().Name;

        }

        static int _runMode = 0;

        public static void SetRunMode(int value)
        {
            _runMode = value;
        }

        public Number RunMode()
        {
            if (ApplicationControllerBase.FirstRun && Common._suppressDialogForTesting)
                return -1;
            return _runMode;
        }
        Context __currentContextDONOTUSEME = Context.Current;
        internal Context CurrentContext
        {
            get
            {
                return __currentContextDONOTUSEME ?? (__currentContextDONOTUSEME = Context.Current);

            }
        }



        public Text Prog()
        {
            StringBuilder result = new StringBuilder();
            bool first = true;
            foreach (ITask t in ControllerBase.GetActiveTasks(CurrentContext))
            {
                if (t is ModuleController)
                    continue;
                if (first)
                    first = false;
                else
                    result.Append(";");
                result.Append(ENV.UserMethods.GetControllerName(t));
                if (_contextTask == t)
                    break;
            }
            return result.ToString();
        }

        public Text Level(Number generation)
        {

            if (UserSettings.Version8Compatible)
                return MainLevel(generation);
            if (generation == null)
                return null;

            ITask task = GetTaskByGeneration(generation);
            if (task != null)
            {
                return Advanced.LevelProvider.GetLevelOf(task);
            }
            return Types.Text.Empty;
        }



        public Bool ViewMod(Number generation)
        {
            if (generation == null)
                return null;
            ITask task = GetTaskByGeneration(generation);
            if (task != null)
            {
                ControllerBase t = null;
                ControllerBase.SendInstanceBasedOnTaskAndCallStack(task, envTask => t = envTask);
                var uic = task as UIController;
                if (uic != null && uic.NoData)
                    return false;
                foreach (ColumnBase column in task.Columns)
                {
                    if (!NullBehaviour.EqualsThatHandlesNullAsADifference(column.Value, column.OriginalValue) && (t == null || t.DoesColumnValueChangeCauseRowChanged(column)))
                        return true;
                }
                return false;
            }
            return false;
        }

        public Number TDepth()
        {
            int result = 0;
            foreach (ITask task in ControllerBase.GetActiveTasks(CurrentContext))
            {
                if (task is ModuleController)
                    continue;
                result++;
            }
            return result;
        }


        public Text GetComponentName()
        {
            if (_application != null)
                return Path.GetFileName(_application.GetType().Assembly.Location);
            return "";
        }

        public Bool IsComponent()
        {
            return !_application.RunnedAsApplication;

        }

        [NotYetImplemented]
        public object CabinetUnload(params object[] args)
        {
            return null;
        }

        public Bool InTrans()
        {
            foreach (ITask task in CurrentContext.RunningTasks)
            {
                if (task.InTransaction)
                    return true;
            }
            return false;
        }

        public Text TransMode()
        {
            if (InTrans())
                return "P";
            return "";
        }

        public byte[] CurrPosition(Number generation)
        {
            if (generation == null)
                return null;
            var t = GetTaskByGeneration(generation);
            if (t != null)
            {
                Types.Data.Entity from = null;
                {
                    var uic = t as UIController;
                    if (uic != null)
                        from = uic.From;
                }
                {
                    var uic = t as BusinessProcess;
                    if (uic != null)
                        from = uic.From;
                }
                if (from != null)
                {
                    return CreateFilterByteArrayBasedOnPrimaryKeyOfEntity(@from);
                }
                else
                    return null;

            }
            else
                return null;
        }

        static byte[] BtrievePositionToByteArray(int p)
        {
            var a = BitConverter.GetBytes(p);
            var r = new byte[BUF_PREFIX.Length + a.Length];
            Array.Copy(BUF_PREFIX, r, BUF_PREFIX.Length);
            Array.Copy(a, 0, r, BUF_PREFIX.Length, a.Length);
            return r;
        }
        static byte[] CreateFilterByteArrayBasedOnPrimaryKeyOfEntity(Firefly.Box.Data.Entity @from)
        {
            if (@from is BtrieveEntity)
            {
                var ic = @from.IdentityColumn as BtrievePositionColumn;
                if (ic != null)
                    return BtrievePositionToByteArray(ic.Value);
            }

            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                var fs = new FilterSaver();
                foreach (var col in @from.PrimaryKeyColumns)
                {
                    CastColumn(col, fs);
                }

                bf.Serialize(ms, fs.values.ToArray());
                return ms.ToArray();
            }
        }

        class FilterSaver : IColumnSpecifier
        {
            public ArrayList values = new ArrayList();
            public void DoOnColumn(TypedColumnBase<Text> column)
            {
                if (ReferenceEquals(column.OriginalValue, null))
                    values.Add(null);
                else
                    values.Add((string)column.OriginalValue);

            }

            public void DoOnColumn(TypedColumnBase<Number> column)
            {
                if (ReferenceEquals(column.OriginalValue, null))
                    values.Add(null);
                else
                    values.Add((Decimal)column.OriginalValue);
            }

            public void DoOnColumn(TypedColumnBase<Date> column)
            {
                if (ReferenceEquals(column.OriginalValue, null))
                    values.Add(null);
                else
                    values.Add(column.OriginalValue.ToString("YYYYMMDD"));
            }

            public void DoOnColumn(TypedColumnBase<Time> column)
            {
                if (ReferenceEquals(column.OriginalValue, null))
                    values.Add(null);
                else
                    values.Add(column.OriginalValue.ToString("HHMMSS"));
            }

            public void DoOnColumn(TypedColumnBase<Bool> column)
            {
                if (ReferenceEquals(column.OriginalValue, null))
                    values.Add(null);
                else
                    values.Add(column.OriginalValue ? 1 : 0);
            }

            public void DoOnColumn(TypedColumnBase<byte[]> column)
            {
                if (ReferenceEquals(column.OriginalValue, null))
                    values.Add(null);
                else
                    values.Add(column.OriginalValue);
            }

            public void DoOnUnknownColumn(ColumnBase column)
            {
                throw new NotImplementedException();
            }
        }

        class FilterLoader : IColumnSpecifier
        {
            object[] values = new object[0];
            int i = 0;

            public FilterCollection Result = new FilterCollection();
            public void DoOnColumn(TypedColumnBase<Text> column)
            {
                object o = null;
                if (i < values.Length)
                {
                    o = values[i++];
                }
                if (o != null)
                {
                    try
                    {
                        Result.Add(column.IsEqualTo((string)o));
                        return;
                    }
                    catch
                    {
                    }
                }
                Result.Add(column.IsEqualTo((Text)null));
            }

            public void DoOnColumn(TypedColumnBase<Number> column)
            {
                object o = null;
                if (i < values.Length)
                {
                    o = values[i++];
                }
                if (o != null)
                {
                    try
                    {
                        Result.Add(column.IsEqualTo((decimal)o));
                        return;
                    }
                    catch
                    {
                    }
                }
                Result.Add(column.IsEqualTo((Number)null));

            }

            public void DoOnColumn(TypedColumnBase<Date> column)
            {
                object o = null;
                if (i < values.Length)
                {
                    o = values[i++];
                }
                if (o != null)
                {
                    try
                    {
                        Result.Add(column.IsEqualTo(Types.Date.Parse((string)o, "YYYYMMDD")));
                        return;
                    }
                    catch
                    {
                    }
                }
                Result.Add(column.IsEqualTo((Date)null));

            }

            public void DoOnColumn(TypedColumnBase<Time> column)
            {
                object o = null;
                if (i < values.Length)
                {
                    o = values[i++];
                }
                if (o != null)
                {
                    try
                    {
                        Result.Add(column.IsEqualTo(Types.Time.Parse((string)o, "HHMMSS")));
                        return;
                    }
                    catch
                    {
                    }
                }
                Result.Add(column.IsEqualTo((Time)null));

            }

            public void DoOnColumn(TypedColumnBase<Bool> column)
            {
                object o = null;
                if (i < values.Length)
                {
                    o = values[i++];
                }
                if (o != null)
                {
                    try
                    {
                        Result.Add(column.IsEqualTo((int)o == 1));
                        return;
                    }
                    catch
                    {
                    }
                }
                Result.Add(column.IsEqualTo((Bool)null));

            }

            public void DoOnColumn(TypedColumnBase<byte[]> column)
            {
                object o = null;
                if (i < values.Length)
                {
                    o = values[i++];
                }
                if (o != null)
                {
                    try
                    {
                        Result.Add(column.IsEqualTo((byte[])o));
                        return;
                    }
                    catch
                    {
                    }
                }
                Result.Add(column.IsEqualTo((byte[])null));

            }

            public void DoOnUnknownColumn(ColumnBase column)
            {
                throw new NotImplementedException();
            }

            public void SetValues(object[] objects)
            {
                if (objects != null)
                    values = objects;
            }
        }

        public FilterBase CreatePrimaryKeyFilter(Firefly.Box.Data.Entity from, Types.Data.ByteArrayColumn storedFilter)
        {
            return CreatePrimaryKeyFilter(from, () => storedFilter.Value);
        }

        public FilterBase CreatePrimaryKeyFilter(Firefly.Box.Data.Entity from, Func<byte[]> storedFilter)
        {
            return new DynamicFilter(y =>
            {

                if (from != null)
                {
                    try
                    {
                        var ba = storedFilter();
                        if (ba != null)
                        {
                            BtrievePositionColumn bpc = null;
                            if (from is BtrieveEntity && (bpc = from.IdentityColumn as BtrievePositionColumn) != null)
                            {

                                int i = BitConverter.ToInt32(GetBytesOfByteArrayColumn(ba), 0);
                                y.Add(bpc.IsEqualTo(i));
                                return;
                            }
                            else
                                using (var ms = new MemoryStream(ba))
                                {
                                    var bf = new BinaryFormatter();
                                    var fs = new FilterLoader();

                                    var x = (object[])bf.Deserialize(ms);
                                    fs.SetValues(x);

                                    foreach (var col in from.PrimaryKeyColumns)
                                    {
                                        CastColumn(col, fs);
                                    }


                                    y.Add(fs.Result);
                                    return;
                                }
                        }
                    }
                    catch
                    {

                    }
                }
                y.Add(() => false);

            });





        }
        internal Command _alternativeCurrentCommand;
        public Text KBGet(Number type)
        {
            if (type == null)
                return null;
            var task = GetTaskByGeneration(0);
            switch ((int)type)
            {

                case 0:
                    if (task != null && !string.IsNullOrEmpty(task.CurrentHandledKey))
                        return task.CurrentHandledKey;
                    if (JapaneseMethods.Enabled)
                        return Types.Text.Empty;
                    else
                        return "<>";
                default:
                    {
                        if (_alternativeCurrentCommand != null)
                            return _alternativeCurrentCommand;
                        if (task != null && task.CurrentHandledCommand != null)
                            return task.CurrentHandledCommand;
                        return "[]";
                    }

            }
        }

        public Bool KBPut(Command command, params object[] args)
        {
            if (command == null)
                return null;
            if (CurrentContext.ActiveTasks.Count > 0)
            {
                var t = CurrentContext.ActiveTasks[CurrentContext.ActiveTasks.Count - 1];
                {
                    var uic = t as UIController;
                    if (uic != null && !KBPutInUIController(uic, command, args, Keys.None))
                        uic.Raise(command, args);
                }
                {
                    var bp = t as BusinessProcess;
                    if (bp != null)
                        bp.Raise(command, args);
                }
                {
                    var ap = t as ModuleController;
                    if (ap != null)
                        ap.Raise(command, args);
                }
            }
            ControllerBase.RaiseHappened(command);
            return true;
        }

        bool KBPutInUIController(UIController uic, Command command, object[] args, Keys key)
        {
            if (uic.View == null) return false;
            var c = FindGridOrTreeView(uic.View);
            {
                var g = c as Firefly.Box.UI.Grid;
                if (g != null && g.MultiSelectIterationIndex != -1)
                {
                    if (command != null)
                        g.RaiseForEachSelectedRow(command, args);
                    else
                        g.RaiseForEachSelectedRow(key);
                    return true;
                }
            }
            {
                var g = c as Firefly.Box.UI.TreeView;
                if (g != null && g.MultiSelectIterationIndex != -1)
                {
                    if (command != null)
                        g.RaiseForEachSelectedRow(command, args);
                    else
                        g.RaiseForEachSelectedRow(key);
                    return true;
                }
            }
            return false;
        }

        public Bool KBPut(Keys key)
        {
            if (CurrentContext.ActiveTasks.Count > 0)
            {
                var t = CurrentContext.ActiveTasks[CurrentContext.ActiveTasks.Count - 1];
                {
                    var uic = t as UIController;
                    if (uic != null && !KBPutInUIController(uic, null, null, key))
                        uic.Raise(key);
                }
                {
                    var bp = t as BusinessProcess;
                    if (bp != null)
                        bp.Raise(key);
                }
                {
                    var ap = t as ModuleController;
                    if (ap != null)
                        ap.Raise(key);
                }
            }
            ControllerBase.RaiseHappened(key);
            return true;
        }

        public Bool KBPut(Text text)
        {
            if (text == null)
                return null;
            new KBPutParser(text, new KBPutParserClient()).Parse();
            return true;
        }

        public Number ProgIdx(Text programName, Bool publicName)
        {
            if (IsNull(programName, publicName))
                return null;
            if (Types.Text.IsNullOrEmpty(programName))
                return 0;
            if (programName.Length > 30)
                programName = programName.Remove(30);
            if (publicName)
                return _application.AllPrograms.IndexOf(programName);
            else
                return _application.AllPrograms.IndexOfName(programName);
        }

        public Text PublicName(Number generation)
        {
            var t = GetTaskByGeneration(generation);
            Text result = Types.Text.Empty;
            try
            {
                ControllerBase.SendInstanceBasedOnTaskAndCallStack(t,
                    y =>
                    {
                        result = y._application.AllPrograms.GetPublicNameOf(y);

                    });
            }
            catch
            {
            }
            return result;
        }

        public static void SetMenu(ToolStripItem menuItem)
        {
            _menu = menuItem;
        }

        internal static ToolStripItem _menu = null;



        public Text Menu()
        {
            string result = "";
            if (_menu == null)
                return Types.Text.Empty;

            var x = _menu;
            while (x != null)
            {
                if (result.Length > 0)
                    result = ";" + result;
                result = MenuManager.GetOriginalMenuText(x) + result;
                x = x.OwnerItem;
            }
            return result;


        }

        public Text Flow3()
        {
            var visitor = new AbstractUIController.FlowVisitor();
            UIController uic = GetTaskByGeneration(0) as UIController;
            if (uic == null)
                return "S";
            uic.VisitFlow(visitor);

            return visitor.Result;
        }

        public Bool Flow(Text whatToCheckFor)
        {
            if (InternalIsNull(whatToCheckFor))
                return null;

            var visitor = new AbstractUIController.FlowVisitor();
            UIController uic = GetTaskByGeneration(0) as UIController;
            if (uic == null)
                return whatToCheckFor == "S";
            uic.VisitFlow(visitor);
            return visitor.Result != "" && whatToCheckFor.Contains(visitor.Result);
        }



        #region Tests

        #endregion

        #endregion

        #region Fields

        public object VarCurr(Number selecedColumnIndex)
        {
            if (selecedColumnIndex == null || selecedColumnIndex == 0) return null;
            ColumnBase column = GetColumnByIndex(selecedColumnIndex);
            if (column != null)
                return column.Value;
            return _nullStrategy.MemoryParameterResult(null);
        }

        public Bool IsDefault(Number selecedColumnIndex)
        {
            if (ReferenceEquals(selecedColumnIndex, null))
                return null;
            ColumnBase column = GetColumnByIndex(selecedColumnIndex);
            if (column != null)
                return IsNull(column.Value) ? column.DefaultValue == null :
                    column.DefaultValue == null ? false :
                    column.Value.Equals(column.DefaultValue);
            return false;
        }

        public object VarPrev(Number selecedColumnIndex)
        {
            if (selecedColumnIndex == null || selecedColumnIndex == 0) return null;
            ColumnBase column = GetColumnByIndex(selecedColumnIndex);
            if (column != null)
                return column.OriginalValue;
            return null;
        }
        public Text VarDisplayName(Number columnReference)
        {
            if (columnReference == null || columnReference == 0) return null;
            ColumnBase column = GetColumnByIndex(columnReference);
            if (column != null)
            {
                return column.DisplayName;
            }

            return Types.Text.Empty;
        }
        public Number VarControlID(Number columnReference)
        {
            if (columnReference == null || columnReference == 0) return null;
            ITask t = null;
            var c = GetColumnByIndex(columnReference, y => t = y);
            if (t == null)
                return null;
            var f = t.View as ENV.UI.Form;
            if (f == null)
                return null;
            if (c == null)
                return null;

            int result = 0;
            foreach (var item in CurrentContext.ActiveTasks)
            {
                var view = item.View as ENV.UI.Form;
                if (item == t)
                {
                    if (view != null)
                    {
                        return result + view.ControlsAndVariablesInFormHelper().IndexOf(c);
                    }
                    else
                        return 0;
                }
                else
                {
                    if (view != null)
                        result += view.ControlsAndVariablesInFormHelper().Count;
                }
            }

            return 0;
        }

        public Number ControlSelectProgram(Number controlId)
        {
            if (controlId == null)
                return null;
            Number result = 0;
            ITask t = null;
            var c = GetControlById(controlId, ref t);
            var x = c as ICanShowCustomHelp;
            ApplicationControllerBase app = null;
            if (x != null && x.ExpandClassType != null)
            {
                ControllerBase.SendInstanceBasedOnTaskAndCallStack(t, y =>
                {
                    app = y._application;
                    if (app != null)
                        result = app.AllPrograms.IndexOf(x.ExpandClassType);
                });

            }
            if (app != null)
            {
                var foundApps = new HashSet<ApplicationControllerBase>();
                var at = CurrentContext.ActiveTasks;
                for (int i = at.Count - 1; i >= 0; i--)
                {
                    bool done = false;
                    ControllerBase.SendInstanceBasedOnTaskAndCallStack(at[i], y =>
                    {
                        if (y._application == app)
                        {
                            done = true;
                            result += 0.01 * foundApps.Count;
                        }
                        else if (!foundApps.Contains(y._application))
                            foundApps.Add(y._application);
                    });
                    if (done)
                        break;
                }
            }
            return result;
        }
        Control GetControlById(Number controlId)
        {
            ITask t = null;
            return GetControlById(controlId, ref t);
        }
        Control GetControlById(Number controlId, ref ITask t)
        {

            foreach (var item in CurrentContext.ActiveTasks)
            {
                var view = item.View as ENV.UI.Form;
                if (view != null)
                {
                    var vars = view.ControlsAndVariablesInFormHelper();
                    if (vars.Count < controlId)
                    {
                        controlId -= vars.Count;
                    }
                    else
                    {
                        t = item;
                        return vars.GetControlAt(controlId);
                    }
                }

            }
            return null;
        }
        public Text ControlDisplayList(Number controlId)
        {
            if (controlId == null)
                return null;
            var c = GetControlById(controlId);
            var l = c as ListControlBase;
            if (l != null)
                return GetListControlDisplayValues(l);
            return Types.Text.Empty;
        }
        public Text ControlItemsList(Number controlId)
        {
            if (controlId == null)
                return null;
            var c = GetControlById(controlId);
            var l = c as ListControlBase;
            if (l != null)
                return GetListControlValues(l);
            return Types.Text.Empty;
        }
        internal static string GetListControlValues(ListControlBase l)
        {
            var result = new StringBuilder();
            l.ProvideListValues((v, d) =>
            {
                if (v != null)
                {
                    var s = v.ToString().Replace(",", "\\,");
                    if (result.Length > 0)
                        result.Append(", ");
                    if (Types.Text.IsNullOrEmpty(s))
                        s = "\\\\";

                    result.Append(s);
                }
            });
            return result.ToString();
        }
        internal static string GetListControlDisplayValues(ListControlBase l)
        {
            var result = new StringBuilder();
            l.ProvideListValues((v, d) =>
            {
                if (v != null)
                {
                    var s = d.ToString().Replace(",", "\\,");
                    if (result.Length > 0)
                        result.Append(", ");
                    if (Types.Text.IsNullOrEmpty(s))
                        s = "\\\\";

                    result.Append(s);
                }
            });
            return result.ToString();
        }
        public Text VarName(Number columnReference)
        {
            if (columnReference == null || columnReference == 0) return Types.Text.Empty;
            ColumnBase column = GetColumnByIndex(columnReference);
            return InternalVarName(column);
        }

        internal static Text InternalVarName(ColumnBase column)
        {
            if (column != null)
            {
                Entity e = column.Entity as Entity;
                if (e != null)
                {
                    return e.Caption + "." + column.Caption;
                }
                else
                {
                    var prefix = ENV.LocalizationInfo.Current.Local;
                    var cc = column as IENVColumn;
                    if (cc != null && cc.IsParameter)
                        prefix = ENV.LocalizationInfo.Current.Parameter;
                    return prefix + "." + column.Caption;
                }
            }

            return Types.Text.Empty;
        }

        public Text VarAttr(Number selecedColumnIndex)
        {
            if (selecedColumnIndex == null || selecedColumnIndex == 0) return "";
            ColumnBase var = GetColumnByIndex(selecedColumnIndex);
            return GetAttribute(var);
        }

        internal static Text GetAttribute(ColumnBase var)
        {
            if (ReferenceEquals(var, null))
                return "";
            if (var is Types.Data.BoolColumn)
                return "L";
            if (var is Types.Data.TimeColumn)
                return "T";
            if (var is Types.Data.NumberColumn)
                return "N";
            if (var is Types.Data.ByteArrayColumn)
                return "B";
            if (var is Types.Data.TextColumn)
                return "A";
            if (var is Types.Data.DateColumn)
                return "D";
            if (InheritsFrom(var.GetType(), typeof(Types.Interop.ActiveXColumn<>)))
                return "X";
            if (InheritsFrom(var.GetType(), typeof(Types.Interop.ComColumn<>)))
                return "O";
            if (InheritsFrom(var.GetType(), typeof(Types.Data.ArrayColumn<>)))
                return "V";

            return "";
        }
        public Bool VarSet(Number selectedColumnIndex, Func<object> value)
        {
            object o = null;
            if (value != null)
                InsteadOfNullStrategy.Instance.OverrideAndCalculate(() => o = value());
            if (o is InsteadOfNullStrategy.InvalidNullValue)
                o = null;
            return VarSet(selectedColumnIndex, o);
        }
        public Bool VarSet(Number selectedColumnIndex, object value)
        {
            try
            {
                var bac = value as ByteArrayColumn;
                var cb = value as ColumnBase;
                if (cb != null)
                    value = cb.Value;
                ColumnBase column = GetColumnByIndex(selectedColumnIndex);
                if (column is TextColumn)
                {
                    if (bac != null)
                        value = bac.ToString();
                    else if (value is byte[])
                        value = TextColumn.FromByteArray(value as byte[]);
                }
                if (column != null)
                    if (!Comparer.Equal(column.Value, value))
                    {
                        if (column is Types.Data.NumberColumn)
                            value = CastToNumber(value);
                        else if (column is Types.Data.DateColumn)
                            value = CastToDate(value);
                        else if (column is Types.Data.TimeColumn)
                            value = CastToTime(value);
                        else if (column is ArrayColumn<Text> && value is byte[][])
                        {
                            value = CastToTextArray(value);
                        }

                        _updateColumn(column, value);
                    }
                if (IsNull(selectedColumnIndex, value))
                    return null;
                return true;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return false;
            }
        }

        public Text VarDbName(Number selecedColumnIndex)
        {
            if (selecedColumnIndex == null || selecedColumnIndex == 0)
                return null;
            ColumnBase column = GetColumnByIndex(selecedColumnIndex);
            return InternalVarDbName(column);
        }

        internal static Text InternalVarDbName(ColumnBase column)
        {
            if (column != null)
            {
                Entity e = column.Entity as Entity;
                if (e != null)
                {
                    if (e.DataProvider is MemoryDatabase)
                        return Types.Text.Empty;
                    return Instance.InternalDbName(e, 1, true) + "." + column.Name;
                }
            }
            return Types.Text.Empty;
        }

        public Bool VarMod(Number selectedColumnIndex)
        {
            if (selectedColumnIndex == null || selectedColumnIndex == 0)
                return null;
            ColumnBase var = GetColumnByIndex(selectedColumnIndex);

            return var != null && InternalVarMod(var);
        }
        internal static Bool InternalVarMod(ColumnBase var)
        { return !NullBehaviour.EqualsThatHandlesNullAsADifference(var.Value, var.OriginalValue); }

        public Text VarPic(Number selecedColumnIndex, Number type)
        {
            if (IsNull(selecedColumnIndex, type))
                return null;
            if (selecedColumnIndex == 0)
                return Types.Text.Empty;

            ITask task = null;
            ColumnBase column = GetColumnByIndex(selecedColumnIndex, t => task = t);
            if (column == null)
                return Types.Text.Empty;

            var result = column.Format ?? Types.Text.Empty;

            switch ((int)type)
            {
                case 0:
                default:
                    return result;
                case 1:
                    var view = task.View as ENV.UI.Form;
                    if (view != null)
                    {
                        var vars = view.ControlsAndVariablesInFormHelper();
                        var control = vars.GetControlAt(vars.IndexOf(column));
                        if (control != null)
                        {
                            var textBox = control as Firefly.Box.UI.TextBox;
                            if (textBox != null && !string.IsNullOrEmpty(textBox.Format.Trim()))
                                result = textBox.Format;
                            else
                            {
                                var button = control as Firefly.Box.UI.Button;
                                if (button != null && !string.IsNullOrEmpty(button.Format.Trim()))
                                    result = button.Format;
                            }
                        }
                    }

                    return result;
            }
        }

        public Number VarInp(Number generation)
        {
            if (generation == null)
                return null;
            UIController task = GetTaskByGeneration(generation) as UIController;
            if (task != null)
            {
                var parkedColumn = task.View.LastFocusedControl;
                if (parkedColumn != null && parkedColumn.GetColumn() != null)
                    return IndexOf(parkedColumn.GetColumn());
            }
            return Number.Zero;
        }

        internal interface NotIncludedInVarIndexCalculations
        {
        }

        internal ColumnBase GetColumnByIndex(int index)
        {
            return GetColumnByIndex(index, delegate
            {
            });
        }

        internal ColumnBase GetColumnByIndex(int index, Action<ITask> andSendTask)
        {
            return _indexOfHelper.GetColumnByIndex(index, andSendTask);
        }

        class IndexOfManager
        {
            UserMethods _parent;

            public IndexOfManager(UserMethods parent)
            {
                _parent = parent;
            }
            void Verify()
            {
                var z = ControllerBase.GetActiveTasks(_parent.CurrentContext);
                if (_prevAC == null)
                {
                    _prevAC = z;
                    return;
                }
                if (_prevAC == z)
                    return;
                if (Compare(z.GetEnumerator(), _prevAC.GetEnumerator()))
                    return;
                _varIndexCache.Clear();
                _taskCache.Clear();
                _getColumnByIndexCache.Clear();
                _varByNameCache.Clear();
                _tasks.Clear();

                _prevAC = z;
            }
            Dictionary<ColumnBase, Number> _varIndexCache = new Dictionary<ColumnBase, Number>();
            Dictionary<ColumnBase, ITask> _taskCache = new Dictionary<ColumnBase, ITask>();
            public Number IndexOf(ColumnBase column)
            {
                Verify();
                Number result = null;
                if (column == null)
                    return Number.Zero;
                if (_varIndexCache.TryGetValue(column, out result))
                    return result;
                ITask t = null;
                result = SearchColumn(column, _prevAC.GetEnumerator(), 0, 0, out t);
                this.AddToCache(column, result, t);
                return result;
            }
            internal ITask ControllerOf(ColumnBase column)
            {
                Verify();
                ITask result;
                if (column == null)
                    return null;
                if (_taskCache.TryGetValue(column, out result))
                    return result;

                var r = SearchColumn(column, _prevAC.GetEnumerator(), 0, 0, out result);
                this.AddToCache(column, r, result);
                return result;
            }

            bool Compare(IEnumerator<ITask> a, IEnumerator<ITask> b)
            {
                bool hasAny = false;
                while (true)
                {
                    var ax = a.MoveNext();
                    var bx = b.MoveNext();
                    if (ax == false && bx == false && !hasAny)
                    {
                        return false;
                    }
                    hasAny = true;
                    if (ax != bx)
                        return false;
                    if (ax)
                    {
                        if (a.Current != b.Current)
                            return false;
                    }
                    else return true;
                }
            }

            IEnumerable<ITask> _prevAC = null;

            internal int SearchColumn(ColumnBase column, IEnumerator<ITask> ac, int depth, int parentColumns, out ITask t)
            {
                if (!ac.MoveNext())
                {
                    t = null;
                    return 0;
                }
                var c = ac.Current;
                var cols = ControllerBase.GetColumnsOf(c);
                var r = SearchColumn(column, ac, depth + 1, parentColumns + cols.Count, out t);
                if (r != 0)
                    return r;
                r = cols.IndexOf(column);
                if (r > -1)
                {
                    t = c;
                    return r + parentColumns + 1;
                }
                return 0;


            }
            Dictionary<int, ColumnBase> _getColumnByIndexCache = new Dictionary<int, ColumnBase>();
            Dictionary<int, ITask> _tasks = new Dictionary<int, ITask>();
            void AddToCache(ColumnBase column, int index, ITask task)
            {
                _getColumnByIndexCache.Add(index, column);
                this._varIndexCache.Add(column, index);
                this._taskCache.Add(column, task);
                _tasks.Add(index, task);
            }
            internal ColumnBase GetColumnByIndex(int index, Action<ITask> andSendTask)
            {
                Verify();
                ColumnBase result = null;

                int i = 0;
                if (index == THISConstant)
                    index = _parent.VarInp(0);
                if (index <= 0)
                    return null;
                if (_getColumnByIndexCache.TryGetValue(index, out result))
                {
                    andSendTask(_tasks[index]);
                    return result;
                }
                foreach (ITask task in _prevAC)
                {
                    var cols = ControllerBase.GetColumnsOf(task);
                    if (index <= i + cols.Count)
                    {
                        andSendTask(task);

                        result = cols[index - i - 1];

                        return result;
                    }
                    else
                        i += cols.Count;

                }
                return null;

            }

            Dictionary<string, ColumnBase> _varByNameCache = new Dictionary<string, ColumnBase>();
            public ColumnBase GetColumnByName(Text selectedColumnName)
            {
                Verify();
                if (Types.Text.IsNullOrEmpty(selectedColumnName))
                    return null;

                var name = selectedColumnName.ToString();
                ColumnBase result = null;
                if (_varByNameCache.TryGetValue(name, out result))
                    return result;
                foreach (ITask task in _prevAC)
                {
                    var cols = ControllerBase.GetColumnsOf(task);
                    result = cols[name] ?? result;

                }
                if (result != null)
                    _varByNameCache.Add(name, result);
                return result;
            }
        }
        IndexOfManager ___indexOfHelperDoNotUseMe = null;
        IndexOfManager _indexOfHelper { get { return ___indexOfHelperDoNotUseMe ?? (___indexOfHelperDoNotUseMe = new IndexOfManager(this)); } }

        public Number IndexOf(ColumnBase column)
        {
            return _indexOfHelper.IndexOf(column);

        }
        internal const int keyOfRightOtherApplication = 500000;
        public Number IndexOf(Role role)
        {
            try
            {
                return _application._applicationRoles.IndexOf(role);
            }
            catch
            {
                return keyOfRightOtherApplication;
            }


        }




        public object VarCurrN(Text selectedColumnName)
        {
            var result = _indexOfHelper.GetColumnByName(selectedColumnName);
            if (result == null)
                return _nullStrategy.MemoryParameterResult(result);
            return result.Value;
        }


        public Number VarIndex(Text selectedColumnName)
        {
            return IndexOf(_indexOfHelper.GetColumnByName(selectedColumnName));
        }

        #endregion

        #region io

        public Bool FileExist(Text fileName)
        {
            return IOExist(fileName);
        }

        public Bool ClientFileExist(Text filename)
        {
            return IOExist(filename);
        }

        public static bool FileExistUniPaaS18OrBelowBehavior { get; set; }

        public Bool IOExist(Text path)
        {
            if (path == null)
                return null;
            path = path.TrimEnd();
            var decodedPath = PathDecoder.DecodePath(path);
            if (!decodedPath.EndsWith(":") && Common.FileExists(decodedPath))
                return true;
            if (!UserSettings.Version10Compatible || FileExistUniPaaS18OrBelowBehavior)
            {
                if (decodedPath.EndsWith("\\"))
                {
                    foreach (var driveInfo in System.IO.DriveInfo.GetDrives())
                    {
                        if (driveInfo.Name == decodedPath.ToUpper(CultureInfo.InvariantCulture))
                            return true;
                    }
                    return false;
                }
                else if (decodedPath.EndsWith(":"))
                    return false;
            }


            return System.IO.Directory.Exists(decodedPath);

        }
        public Number FileSize(Text fileName)
        {
            return IOSize(fileName);
        }

        public Number IOSize(Text fileName)
        {
            if (fileName == null)
                return null;
            fileName = PathDecoder.DecodePath(fileName);
            if (!System.IO.File.Exists(fileName))
                return 0;
            try
            {
                return new System.IO.FileInfo(fileName).Length;
            }
            catch
            {
                return 0;
            }
        }

        public Bool FileDelete(Text fileName)
        {
            return IODel(fileName);
        }

        public Bool ClientFileDelete(Text fileName)
        {
            return IODel(fileName);
        }

        public Bool IODel(Text fileName)
        {
            if (ReferenceEquals(fileName, null))
                return null;

            var fn = PathDecoder.DecodePath(fileName);
            if (Types.Text.IsNullOrEmpty(fn))
                return false;

            try
            {
                return DeleteFile(Path.GetFullPath(fn));
            }
            catch (Exception e)
            {
                //removed error logging because customers had built on this as a way to see if the file is in used.
                return false;
            }
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        static extern bool DeleteFile(string path);

        public Bool FileRename(Text originalFileName, Text targetFileName)
        {
            return IORen(originalFileName, targetFileName);
        }

        public Bool ClientFileRename(Text originalFileName, Text targetFileName)
        {
            return IORen(originalFileName, targetFileName);
        }

        public Bool IORen(Text originalFileName, Text targetFileName)
        {
            try
            {
                if (IsNull(originalFileName, targetFileName))
                    return null;
                if (originalFileName == targetFileName)
                    return true;

                var originalPath = PathDecoder.DecodePath(originalFileName);
                var targetPath = PathDecoder.DecodePath(targetFileName);
                var tempPath = Path.GetRandomFileName();

                if (Directory.Exists(originalPath))
                {
                    var itIsSameDirectoryDifferentCasing = targetPath.Equals(originalPath, StringComparison.InvariantCultureIgnoreCase);
                    if (itIsSameDirectoryDifferentCasing)
                    {
                        Directory.Move(originalPath, tempPath);
                        Directory.Move(tempPath, targetPath);
                        return true;
                    }

                    if (Directory.Exists(targetPath))
                    {
                        return false;
                    }

                    Directory.Move(originalPath, targetPath);
                    return true;
                }

                if (Common.FileExists(originalPath))
                {
                    var itIsSameFileDifferentCasing = targetPath.Equals(originalPath, StringComparison.InvariantCultureIgnoreCase);
                    if (itIsSameFileDifferentCasing)
                    {
                        File.Move(originalPath, tempPath);
                        File.Move(tempPath, targetPath);
                        return true;
                    }

                    if (Common.FileExists(targetPath))
                        File.Delete(targetPath);
                    File.Move(originalPath, targetPath);
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                if (IsNull(originalFileName, targetFileName))
                    return null;
                return false;
            }
        }

        public Bool FileCopy(Text sourceFileName, Text targetFileName)
        {
            return IOCopy(sourceFileName, targetFileName);
        }

        public Bool ClientFileCopy(Text sourceFileName, Text targetFileName)
        {
            return IOCopy(sourceFileName, targetFileName);
        }


        public Bool IOCopy(Text sourceFileName, Text targetFileName)
        {
            try
            {
                if (IsNull(sourceFileName, targetFileName))
                    return null;
                var sourcePath = PathDecoder.DecodePath(sourceFileName);
                if (!Common.FileExists(sourcePath))
                    return false;
                var targetPath = PathDecoder.DecodePath(targetFileName);
                if (sourcePath.Equals(targetPath, StringComparison.InvariantCultureIgnoreCase))
                    return false;
                System.IO.File.Copy(sourcePath, targetPath, IsFile(sourcePath) && Common.FileExists(targetPath));
                return true;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                if (IsNull(sourceFileName, targetFileName))
                    return null;
                return false;
            }
        }

        private bool IsFile(string path)
        {
            return !IsDirectory(path);
        }

        private bool IsDirectory(string path)
        {
            var attr = File.GetAttributes(path);
            return attr.HasFlag(FileAttributes.Directory);

        }

        public Text[] FileListGet(Text path, Text searchPatterns, Bool includeSubDirectories)
        {
            var result = new HashSet<Text>();
            try
            {
                if (searchPatterns == null || searchPatterns == "")
                    searchPatterns = "*";
                path = PathDecoder.DecodePath(path);
                foreach (var searchPattern in searchPatterns.TrimEnd().ToString().Split('|'))
                {
                    foreach (
                        string file in
                            Directory.GetFiles(path, searchPattern,
                                               includeSubDirectories
                                                   ? SearchOption.AllDirectories
                                                   : SearchOption.TopDirectoryOnly))
                    {
                        var y = file.Substring(path.Length);
                        if (y.StartsWith("\\"))
                            y = y.Substring(1);
                        result.Add(y);
                    }
                }

            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
            }
            return result.ToArray();
        }

        public Text[] ClientFileListGet(Text path, Text searchPatterns, Bool includeSubDirectories)
        {
            return FileListGet(path, searchPatterns, includeSubDirectories);
        }


        object GetIO(Number generation, Number ioIndex)
        {
            ITask task = GetTaskByGeneration(generation);
            if (ioIndex < 1)
                return null;
            if (task != null)
            {
                var x = IOFinder.GetStreams(task);
                if (x.Count >= ioIndex)
                    return x[ioIndex - 1];
            }
            return null;
        }

        T DoForXml<T>(Number generation, Number ioIndex, Text path, T defaultValue, Func<XmlHelper, T> what) where T : class
        {
            if (generation == null || ioIndex == null || path == null)
                return null;
            var reader = GetIO(generation, ioIndex) as IXMLIO;
            if (reader != null)
                return what(reader.Xml);
            return defaultValue;
        }

        public Number XMLFind(Number generation, Number ioIndex, Text path, Text name, Number isAttrubte, Text childName,
                              Text value)
        {
            return DoForXml(generation, ioIndex, path, Number.Zero
                , x =>
                    x.Search(path, name, isAttrubte, childName, value)
                    );

        }

        public Bool XMLExist(Number generation, Number ioIndex, Text path)
        {
            return DoForXml<Bool>(generation, ioIndex, path, false
                , x =>
                    x.Contains(path)
                    );

        }

        public Bool XMLExist(Number generation, Number ioIndex, Text path, Text attributeName)
        {
            return DoForXml<Bool>(generation, ioIndex, path, false
                , x =>
                    x.Contains(path, attributeName)
                    );


        }

        public Number XMLCnt(Number generation, Number ioIndex, Text path)
        {
            return DoForXml<Number>(generation, ioIndex, path, -1
                , x =>
                    x.Count(path)
                    );
        }
        public Number XMLCnt(Number generation, Number ioIndex, Text path, Text attributeName)
        {
            return DoForXml<Number>(generation, ioIndex, path, -1
                , x =>
                    x.Count(path)
                    );
        }

        public Number XMLInsert(Number generation, Number ioIndex, Text path, Text attributeName, Text value)
        {
            return DoForXml<Number>(generation, ioIndex, path, -1
                , x =>
                    x.Add(path, attributeName, value)
                    );
        }
        public Number XMLInsert(Number generation, Number ioIndex, Text path, Text attributeName)
        {
            return DoForXml<Number>(generation, ioIndex, path, -1
                , x =>
                    x.Add(path, attributeName)
                    );
        }

        public Number XMLInsert(Number generation, Number ioIndex, Text path, Text attributeName, byte[] value)
        {
            return DoForXml<Number>(generation, ioIndex, path, -1
                , x =>
                    x.Add(path, attributeName, value)
                    );
        }

        public Number XMLInsert(Number generation, Number ioIndex, Text path, Text attributeName, ByteArrayColumn value)
        {
            return DoForXml<Number>(generation, ioIndex, path, -1
                , x =>
                    x.Add(path, attributeName, value)
                    );
        }

        public Number XMLInsert(Number generation, Number ioIndex, Text path, Text attributeName, Text value,
                                Text beforeOrAfter)
        {
            return DoForXml<Number>(generation, ioIndex, path, -1
                , x =>
                    x.Add(path, attributeName, value, beforeOrAfter)
                    );
        }

        public Number XMLInsert(Number generation, Number ioIndex, Text path, Text attributeName, byte[] value,
                                Text beforeOrAfter)
        {
            return DoForXml<Number>(generation, ioIndex, path, -1
                , x =>
                    x.Add(path, attributeName, value, beforeOrAfter)
                    );
        }

        public Number XMLInsert(Number generation, Number ioIndex, Text path, Text attributeName, ByteArrayColumn value,
                                Text beforeOrAfter)
        {
            return DoForXml<Number>(generation, ioIndex, path, -1
                , x =>
                        x.Add(path, attributeName, value, beforeOrAfter)
                    );


        }

        public Number XMLInsert(Number generation, Number ioIndex, Text path, Text attributeName, Text value,
                                Text beforeOrAfter, Text referencedElementName)
        {
            return DoForXml<Number>(generation, ioIndex, path, -1
                , x =>
                        x.Add(path, attributeName, value, beforeOrAfter, referencedElementName, true)
                    );

        }

        public Number XMLInsert(Number generation, Number ioIndex, Text path, Text attributeName, Text value,
                                Text beforeOrAfter, Text referencedElementName, Bool convertToXml)
        {
            return DoForXml<Number>(generation, ioIndex, path, -1
                , x =>
                        x.Add(path, attributeName, value, beforeOrAfter, referencedElementName, convertToXml)
                    );

        }

        public Number XMLSetEncoding(Number generation, Number ioIndex, Text encoding)
        {
            return DoForXml<Number>(generation, ioIndex, "", -1
                , x =>
                        x.SetEncoding(encoding)
                    );

        }


        public Number XMLDelete(Number generation, Number ioIndex, Text path, Text attributeName)
        {
            return DoForXml<Number>(generation, ioIndex, path, -1
                , x =>
                        x.Remove(path, attributeName)
                    );
        }

        public Number XMLModify(Number generation, Number ioIndex, Text path, Text attributeName, Text value)
        {
            return DoForXml<Number>(generation, ioIndex, path, -1
               , x =>
                       x.Set(path, attributeName, value)
                   );
        }

        public Number XMLModify(Number generation, Number ioIndex, Text path, Text attributeName, Text value,
                                Bool convertToXml)
        {
            return DoForXml<Number>(generation, ioIndex, path, -1
               , x =>
                       x.Set(path, attributeName, value, convertToXml)
                   );
        }


        public byte[] XMLBlobGet(Number generation, Number ioIndex, Text path, Text attributeName)
        {
            return DoForXml<byte[]>(generation, ioIndex, path, null
               , x =>
                       x.GetByteArray(path, attributeName)
                   );

        }
        public byte[] XMLBlobGet(Number generation, Number ioIndex, Text path)
        {
            return DoForXml<byte[]>(generation, ioIndex, path, null
               , x =>
                       x.GetByteArray(path)
                   );
        }


        public Text XMLGetEncoding(Number generation, Number ioIndex)
        {
            return DoForXml<Text>(generation, ioIndex, "", ""
               , x =>
                       x.GetEncoding()
                   );
        }

        public Text XMLGetAlias(Number generation, Number ioIndex, Text path)
        {
            return DoForXml<Text>(generation, ioIndex, path, ""
               , x =>
                       x.GetAlias(path)
                   );
        }

        public Text XMLGet(Number generation, Number ioIndex, Text path)
        {
            return DoForXml<Text>(generation, ioIndex, path, ""
               , x =>
                       x.Get(path)
                   );
        }

        public Text XMLGet(Number generation, Number ioIndex, Text path, Text attributeName)
        {
            return DoForXml<Text>(generation, ioIndex, path, ""
               , x =>
                       x.Get(path, attributeName)
                   );

        }
        public Text XMLGet(Number generation, Number ioIndex, Text path, Text attributeName, Bool returnNullIfNotFound)
        {
            return DoForXml<Text>(generation, ioIndex, path, ""
               , x =>
                       x.Get(path, attributeName, returnNullIfNotFound)
                   );

        }

        static List<string[]> xmlSpecialCharacters = new List<string[]>
                                                         {
                                                             new[] {"&", "&amp;"},
                                                             new[] {"\"", "&quot;"},
                                                             new[] {"'", "&apos;"},
                                                             new[] {"<", "&lt;"},
                                                             new[] {">", "&gt;"},
                                                             new []{"\r","&#xD;"},
                                                             new []{"\n","&#xA;"},
                                                             new []{"“","&#x201C;"},
                                                             new []{"”","&#x201D;"},
                                                         };


        public Number Val(byte[] text, Text format)
        {
            if (ReferenceEquals(text, null))
                return null;
            return Val(TextColumn.FromByteArray(text), format);
        }
        public Number Val(ByteArrayColumn text, Text format)
        {
            if (ReferenceEquals(text.Value, null))
                return null;
            return Val(text.ToString(), format);
        }
        public Text XMLStr(byte[] value)
        {
            if (ReferenceEquals(value, null))
                return null;
            return XMLStr(TextColumn.FromByteArray(value));
        }
        public Text XMLStr(ByteArrayColumn column)
        {
            if (ReferenceEquals(column, null) || ReferenceEquals(column.Value, null))
                return null;
            return XMLStr(column.ToString());
        }

        public Text XMLStr(Text value)
        {
            if (value == null)
                return null;
            string result = value;
            foreach (var list in xmlSpecialCharacters)
            {
                result = result.Replace(list[1], list[0]);
            }
            return result;
        }
        internal Text InternalXMLVal(Text value, bool forXmlInsertFunction)
        {
            if (value == null)
                return null;
            string result = value;
            foreach (var list in xmlSpecialCharacters)
            {
                if (forXmlInsertFunction && list[0] == "\"")
                    continue;
                if ((list[0] == "\r" || list[0] == "\n") && !forXmlInsertFunction)
                    continue;
                result = result.Replace(list[0], list[1]);
            }
            return result;
        }

        public Text XMLVal(Text value)
        {
            return InternalXMLVal(value, false);
        }
        public Text XMLVal(byte[] value)
        {
            if (value == null)
                return null;
            return InternalXMLVal(TextColumn.FromByteArray(value), false);
        }
        public Text XMLVal(ByteArrayColumn value)
        {
            if (ReferenceEquals(value, null) || ReferenceEquals(value.Value, null))
                return null;
            return InternalXMLVal(value, false);
        }



        public Number Page(Number generation, Number ioIndex)
        {
            if (IsNull(generation, ioIndex))
                return null;
            object io = GetIO(generation, ioIndex);
            Types.Printing.PrinterWriter printer = io as Types.Printing.PrinterWriter;
            if (printer != null)
                return printer.Page;
            TextPrinterWriter t = io as TextPrinterWriter;
            if (t != null)
                return t.Page;
            if (io == null)
                return 0;
            return 1;
        }

        public Number Line(int generation, int ioIndex)
        {
            if (IsNull(generation, ioIndex))
                return null;
            object io = GetIO(generation, ioIndex);
            PrinterWriter printerWriter = io as PrinterWriter;
            if (printerWriter != null)
                return printerWriter.HeightForLineFunc;
            TextPrinterWriter t = io as TextPrinterWriter;
            if (t != null)
                return t.HeightFromStartOfPage;
            Writer fileWriter = io as Writer;
            if (fileWriter != null)
                return fileWriter.LineNumber;
            return 0;
        }

        public Bool EOF(Number generation, Number ioIndex)
        {
            if (IsNull(generation, ioIndex))
                return null;
            object io = GetIO(generation, ioIndex);
            Reader fileReader = io as Reader;
            if (fileReader != null)
                return fileReader.EndOfFile;
            var pw = io as PrinterWriter;
            if (pw != null)
                return pw.CancelPrinting;
            var xml = io as IXMLIO;
            if (xml != null)
                return xml.Xml.WasAccessed;
            return io == null;
        }


        public Bool EOP(Number generation, Number ioIndex)
        {
            if (IsNull(generation, ioIndex))
                return null;
            object io = GetIO(generation, ioIndex);
            if (io == null)
                return true;
            var printerWriter = io as PrinterWriter;
            if (printerWriter != null)
                return printerWriter.HeightUntilEndOfPage < 0 || printerWriter.NewPageOnNextWrite || printerWriter.PageEnded;
            var tpw = io as TextPrinterWriter;
            if (tpw != null)
                return tpw.HeightUntilEndOfPage < 0 || tpw.NewPageOnNextWrite;
            return false;
        }

        #endregion

        #region ui


        public Bool PropSet(Number generation, Text controlName, Number property, params object[] values)
        {
            var t = GetTaskByGeneration(generation);

            if (t != null && t.View != null)
            {
                var form = t.View as ENV.UI.Form;
                if (form != null)
                {
                    var control = FindControlByTag(form, controlName.Trim()) as Firefly.Box.UI.Advanced.ControlBase;
                    if (control != null && values.Length > 0)
                    {
                        if (property == 14)
                        {
                            var g = control as ENV.UI.Grid;
                            if (g != null)
                            {
                                Number n;
                                if (Number.TryCast(values[0], out n))
                                {
                                    g.RowHeight = n * form.VerticalScale;
                                    return true;
                                }
                            }
                        }
                        if (property == 1 && values.Length > 3)
                        {
                            Number x, y, w, h;
                            if (Number.TryCast(values[0], out x) &&
                                Number.TryCast(values[1], out y) &&
                            Number.TryCast(values[2], out w) &&
                            Number.TryCast(values[3], out h))
                                Context.Current.InvokeUICommand(() =>
                                {
                                    control.Left = x * form.HorizontalScale;
                                    control.Top = y * form.VerticalScale;
                                    control.Width = w * form.HorizontalScale;
                                    control.Height = h * form.VerticalScale;
                                });

                        }
                    }
                }

            }
            return false;
        }



        public Bool SetCrsr(Number crsrType)
        {
            if (crsrType == null)
                return null;
            if (crsrType < 1 || crsrType > 14) return false;
            var newCursor =
                crsrType == 1
                    ? Cursors.Default
                    : crsrType == 2
                          ? Cursors.WaitCursor
                          : crsrType == 3
                                ? Cursors.Hand
                                : crsrType == 4
                                      ? Cursors.AppStarting
                                      : crsrType == 5
                                            ? Cursors.Cross
                                            : crsrType == 6
                                                  ? Cursors.Help
                                                  : crsrType == 7
                                                        ? Cursors.IBeam
                                                        : crsrType == 8
                                                              ? Cursors.No
                                                              : crsrType == 9
                                                                    ? Cursors.SizeAll
                                                                    : crsrType == 10
                                                                          ? Cursors.SizeNESW
                                                                          : crsrType == 11
                                                                                ? Cursors.SizeNS
                                                                                : crsrType == 12
                                                                                      ? Cursors.
                                                                                            SizeNWSE
                                                                                      : crsrType ==
                                                                                        13
                                                                                            ? Cursors
                                                                                                  .
                                                                                                  SizeWE
                                                                                            : crsrType ==
                                                                                              14
                                                                                                  ? Cursors
                                                                                                        .
                                                                                                        VSplit
                                                                                                  : Cursors
                                                                                                        .
                                                                                                        Arrow;

            var mdiFound = false;
            Common.RunOnContextTopMostForm(
                mdi =>
                {
                    mdi.Cursor = newCursor;
                    mdiFound = true;
                });
            if (!mdiFound)
                Context.Current.InvokeUICommand(
                    () =>
                    {
                        foreach (System.Windows.Forms.Form form in System.Windows.Forms.Application.OpenForms)
                            form.Cursor = newCursor;
                    });
            return true;
        }

        public Number SplitterOffset(Number zeroForPercentageOrOneForUnits)
        {

            try
            {
                ITask task = GetTaskByGeneration(0);
                if (task != null)
                {
                    ENV.UI.Form form = task.View as ENV.UI.Form;
                    if (form != null)
                    {
                        if (zeroForPercentageOrOneForUnits == 0) return form.SplitterPosition;
                        var parent = form.Parent != null ? form.Parent.FindForm() as Firefly.Box.UI.Form : null;
                        if (parent != null)
                            return (form.SplitterPosition / 100) * (form.SplittedChildDockStyle == DockStyle.Left || form.SplittedChildDockStyle == DockStyle.Right ?
                                parent.ClientSize.Width / form.HorizontalScale : parent.ClientSize.Height / form.VerticalScale);
                    }
                }
            }
            catch { }
            return Number.Zero;
        }
        public Number WinHWND(Number generation)
        {

            if (generation == null)
                return null;
            try
            {
                System.Windows.Forms.Form form = null;
                if (CurrentContext.ActiveTasks.Count - 1 == generation)
                    form = Common.ContextTopMostForm;
                if (form == null)
                {
                    ITask task = GetTaskByGeneration(generation);
                    if (task != null)
                    {
                        var f = task.View;
                        if (f != null && f.ContainerForm != null)
                            f = f.ContainerForm;
                        form = f;
                    }
                }
                if (form != null)
                    if (form.IsHandleCreated)
                    {
                        var n = Number.Zero;
                        Context.Current.InvokeUICommand(
                            () =>
                            {
                                n = (int)form.Handle;
                            });
                        return n;
                    }
            }
            catch { }
            return Number.Zero;
        }

        public Number CtrlHWND(Text controlName)
        {
            try
            {
                foreach (Control control in GetTaskByGeneration(0).View.Controls)
                {
                    if (control.Tag != null && control.Tag.ToString() == controlName && control.Visible)
                    {
                        var treeView = control as TreeView;
                        if (treeView != null)
                        {
                            foreach (System.Windows.Forms.Control c in treeView.Controls)
                            {
                                if (c is System.Windows.Forms.TreeView && c.IsHandleCreated)
                                    return (int)c.Handle;
                            }
                        }
                    }
                }
            }
            catch { }
            return Number.Zero;
        }

        public Text LastClicked()
        {
            return CTRLName();
        }

        public Number MainDisplay(Number generation)
        {
            ITask task = GetTaskByGeneration(generation);
            {
                var t = task as BusinessProcess;
                if (t != null)
                {
                    return BusinessProcessBase._actionBusinessProcess[t].GetMainDisplayIndex();
                }
            }
            {
                var t = task as UIController;
                if (t != null)
                {
                    return AbstractUIController._activeUIControllers[t].GetMainDisplayIndex();
                }
            }

            return Number.Zero;
        }

        public Number CurRow(Number i)
        {
            if (InternalIsNull(i))
                return null;
            UIController u = GetTaskByGeneration(i) as UIController;
            if (u != null)
            {
                var g = FindGridOrTreeView(u.View) as Types.UI.Grid;
                if (g != null)
                {
                    return g.ActiveRowIndex >= 0 ? g.ActiveRowIndex + 1 : 1;
                }
            }
            return 0;
        }

        public Text CTRLName()
        {

            if (Form.LastClickedControl == null)
                return "";
            if (Form.LastClickedControl.Tag == null)
                return "";
            return Form.LastClickedControl.Tag.ToString();
        }
        public Number FormUnitsToPixels(Number val, Bool isX)
        {
            if (val == null || isX == null)
                return null;

            ITask task = GetTaskByGeneration(0);
            if (task != null)
            {
                ENV.UI.Form form = task.View as ENV.UI.Form;
                if (form != null)
                {
                    if (isX)
                        return val * form.HorizontalScale;
                    else
                        return val * form.VerticalScale;
                }
            }
            return 0;
        }

        public Number WinBox(Number generation, Text dimension)
        {
            if (generation == null || dimension == null)
                return null;
            if (Common._suppressDialogForTesting && dimension.ToUpper().Trim() == "H")
                return 700;
            ITask task = GetTaskByGeneration(generation);
            if (task != null)
            {
                ENV.UI.Form form = task.View as ENV.UI.Form;
                if (form != null)
                {
                    var f = task.View;
                    if (f.SplittedChildDockStyle != DockStyle.None && f.Parent != null)
                        f = f.Parent.FindForm() as Types.UI.Form ?? f;
                    switch (dimension.ToUpper(CultureInfo.InvariantCulture))
                    {
                        case "X":
                            return Math.Round(f.Left * form.HorizontalExpressionFactor / form.HorizontalScale);
                        case "Y":
                            return Math.Round(f.Top * form.VerticalExpressionFactor / form.VerticalScale);
                        case "W":
                            return Math.Round(f.ClientSize.Width * form.HorizontalExpressionFactor / form.HorizontalScale);
                        case "H":
                            return Math.Round(f.ClientSize.Height * form.VerticalExpressionFactor / form.VerticalScale);
                    }
                }
            }
            return Number.Zero;
        }


        public Bool MnuReset()
        {
            ENV.MenuManager.ResetMenu();
            return true;
        }

        public Bool MnuCheck(Text menuName, Bool on)
        {
            if (IsNull(menuName))
                return null;
            if (IsNull(on))
                on = false;
            ENV.MenuManager.CheckMenu(menuName.TrimEnd(), on);
            return true;
        }

        public Bool MnuEnabl(Text menuName, Bool on)
        {
            if (IsNull(menuName))
                return null;
            if (IsNull(on))
                on = false;
            ENV.MenuManager.EnableMenu(menuName.TrimEnd(), on);
            return true;
        }

        public Bool MnuShow(Text menuName, Bool on)
        {
            if (IsNull(menuName))
                return null;
            if (IsNull(on))
                on = false;
            ENV.MenuManager.ShowMenu(menuName.TrimEnd(), on);
            return true;
        }

        public Bool MnuName(Text menuName, Text text)
        {
            if (IsNull(menuName, text))
                return null;
            ENV.MenuManager.SetMenuText(menuName.TrimEnd(), text);
            return true;
        }

        public Bool MnuAdd(Number menuKey, Text menuPath)
        {
            if (IsNull(menuKey, menuPath))
                return null;
            var result = false;
            ENV.MenuManager.DoOnMenuManagers(m => result = m.Activate(menuKey, true, menuPath));
            return result;
        }

        public Bool MnuRemove(Number menuKey)
        {
            return MnuRemove(menuKey, "");
        }

        public Bool MnuRemove(Number menuKey, Text menuPath)
        {
            if (IsNull(menuKey, menuPath))
                return null;
            if (!Types.Text.IsNullOrEmpty(menuPath))
                return false;
            var result = false;
            ENV.MenuManager.DoOnMenuManagers(m => result = m.Deactivate(menuKey));
            return result;
        }

        public Number MenuIdx(Text menuEntryName, Bool isPublicName)
        {
            if (isPublicName)
                return 0;
            if (_application == null)
                return 0;
            if (Types.Text.IsNullOrEmpty(menuEntryName))
                return 0;
            return Array.IndexOf(_application._getMenuNames(), menuEntryName.Trim().ToString()) + 1;

        }



        class ViewToWriterColumnListItem : ENV.UI.IColumnListItem
        {
            ColumnBase _col;
            string _caption;

            public ViewToWriterColumnListItem(ColumnBase col)
            {
                _col = col;
                _caption = _col.Caption;
            }

            public string GetValueString(bool xmlStyleDate)
            {
                if (ENV.UserMethods.Instance.IsNull(_col))
                    return "";
                if (xmlStyleDate)
                {
                    {
                        var dc = _col as DateColumn;
                        if (dc != null)
                            return dc.ToString("YYYY-MM-DD");
                    }
                    {
                        var dc = _col as TimeColumn;
                        if (dc != null)
                            return dc.ToString("HH:MM:SS");
                    }

                }

                return (ENV.UserMethods.Instance.RemoveZeroChar(_col.ToString()) ?? "").Trim();
                //return _col.ToString();
            }

            public ColumnBase GetColumn()
            {
                return _col;
            }

            public override string ToString()
            {
                return _caption;
            }

            public bool IsText()
            {
                return _col is Types.Data.TextColumn;
            }

            public void SetCaption(string caption)
            {
                _caption = caption;
            }
        }

        internal string[] SplitColumns(string columnCaptions)
        {
            var x = columnCaptions.Trim();
            var result = new List<string>();
            foreach (var s in x.Split(','))
            {
                if (!string.IsNullOrEmpty(s))
                    result.Add(s);
            }
            return result.ToArray();
        }


        public Text[] DataViewIndexNames(Number generation)
        {
            if (generation == null)
                return null;
            var t = GetTaskByGeneration(generation);
            if (t == null)
                return null;
            var result = new List<Text>();
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(t, c =>
            {

                if (c.From != null)
                {
                    foreach (var item in c.From.Indexes)
                    {
                        result.Add(item.Caption);
                    }
                }

            });

            return result.ToArray();
        }


        public Text[] DataViewIndexSegmentNames(Number generation, Number index)
        {
            if (generation == null)
                return null;
            var t = GetTaskByGeneration(generation);
            if (t == null)
                return null;
            var result = new List<Text>();
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(t, c =>
            {

                if (c.From != null)
                {
                    var id = c.From.Indexes[index - 1];
                    foreach (var item in id.Segments)
                    {
                        result.Add(item.Column.Caption);
                    }
                }

            });

            return result.ToArray();
        }

        public Bool DataViewToText(Number generation, Text columnCaptions, Text outputCaptions, Text resultFileName,
                                   Text delimiter, Text stringIdentifier, Number charSet)
        {
            if (IsNull(generation, columnCaptions, outputCaptions, resultFileName, delimiter, stringIdentifier, charSet))
                return null;
            return DataViewToWriter(generation, columnCaptions, outputCaptions, resultFileName, charSet,
                                    (writer, columns) => new TxtWriter(writer, delimiter, !Types.Text.IsNullOrEmpty(outputCaptions), true, stringIdentifier, columns.ToArray()));
        }
        public System.Data.DataTable DataViewToDNDataTable(Number generation, Text columnCaptions, Text outputCaptions)
        {
            if (IsNull(generation, columnCaptions, outputCaptions))
                return null;
            DataTableWriter result = null;
            var r = DataViewToWriter(generation, columnCaptions, outputCaptions, null, 0,
                                    (writer, columns) => result = new DataTableWriter(columns.ToArray()), true);
            if (result != null)
                return result.DataTableBuilder.Result;
            return null;
        }

        public Bool DataViewToXML(Number generation, Text columnCaptions, Text outputCaptions, Text resultFileName,
                                  Text schemaFileName, Text templateFileName, Number charSet)
        {
            if (IsNull(generation, columnCaptions, outputCaptions, resultFileName, schemaFileName, templateFileName,
                       charSet))
                return null;
            return DataViewToWriter(generation, columnCaptions, outputCaptions, resultFileName,
                                    charSet, (writer, columns) => new XmlWriter(writer, PathDecoder.DecodePath(schemaFileName).Trim(), columns.ToArray()));
        }


        public Bool DataViewToHTML(Number generation, Text columnCaptions, Text outputCaptions, Text resultFileName, Text templateFileName, Number charSet)
        {
            if (IsNull(generation, columnCaptions, outputCaptions, resultFileName, templateFileName,
                       charSet))
                return null;
            return DataViewToWriter(generation, columnCaptions, outputCaptions, resultFileName,
                                    charSet, (writer, columns) => new HtmlWriter(writer, templateFileName, RightToLeft.No, "", columns.ToArray()));
        }

        Bool DataViewToWriter(Number generation, Text columnCaptions, Text outputCaptions, Text resultFileName,
                              Number charSet, Func<StreamWriter, List<IColumnListItem>, IWriter> writer, bool createWriterWhenNoRows = false)
        {
            var encoding = LocalizationInfo.Current.OuterEncoding;
            if (charSet == 1)
                encoding = new System.Text.UnicodeEncoding(false, true);
            else if (charSet == 2)
                encoding = System.Text.Encoding.UTF8;
            var myCols = new List<ViewToWriterColumnListItem>();
            bool result = false;

            ControllerBase.SendInstanceBasedOnTaskAndCallStack(
                GetTaskByGeneration(generation),
                y =>
                {

                    try
                    {
                        foreach (var splitColumn in SplitColumns(columnCaptions))
                        {
                            ColumnBase col = null;
                            foreach (var column in y.Columns)
                            {
                                if (column.Caption == splitColumn)
                                {
                                    col = column;
                                    break;
                                }
                            }
                            if (col == null)
                            {
                                //result = false;
                                //return;
                            }
                            else
                                myCols.Add(new ViewToWriterColumnListItem(col));
                        }
                        //if (result)
                        {
                            if (outputCaptions != "@")
                            {
                                int i = 0;
                                foreach (var c in SplitColumns(outputCaptions))
                                {
                                    if (i < myCols.Count)
                                    {
                                        myCols[i++].SetCaption(c);
                                    }
                                }
                            }
                            var cols = new List<IColumnListItem>();
                            foreach (var c in myCols)
                            {
                                cols.Add(c);
                            }
                            StreamWriter fw = null;
                            IWriter xmlWriter = null;

                            Action createWriter = () =>
                            {
                                if (xmlWriter == null)
                                {
                                    if (!Types.Text.IsNullOrEmpty(resultFileName))
                                    {
                                        resultFileName = PathDecoder.DecodePath(resultFileName).Trim();

                                        fw = new System.IO.StreamWriter(resultFileName, false, encoding);
                                    }
                                    else
                                    {
                                        fw = new StreamWriter(new MemoryStream());
                                    }
                                    xmlWriter = writer(fw, cols);

                                }
                            };

                            if (createWriterWhenNoRows)
                                createWriter();
                            Action doWrite = () =>
                            {
                                createWriter();
                                result = true;
                                xmlWriter.WriteLine();
                            };
                            try
                            {
                                if (FileExist(resultFileName))
                                    FileDelete(resultFileName);
                                {
                                    var t = y as AbstractUIController;
                                    if (t != null)
                                    {
                                        ShowProgressInNewThread.ReadAllRowsWithProgress(t._uiController,
                                            "Export Data", doWrite);

                                    }
                                }
                                {
                                    var t = y as BusinessProcessBase;
                                    if (t != null)
                                    {
                                        ShowProgressInNewThread.ReadAllRowsWithProgress(t._businessProcess,
                                            "Export Data", doWrite);

                                    }
                                }
                            }
                            finally
                            {
                                if (xmlWriter != null)
                                    xmlWriter.Dispose();
                                if (fw != null)
                                    fw.Dispose();
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLog.WriteToLogFile(ex, "DataViewFunction");
                        result = false;
                    }


                });
            return result;
        }

        [NotYetImplemented]
        public Bool DataViewToDataSource(Number generation, Text columnCaptions, Number destinationSourceNumber, Text destinationSourceName, Text outputCaptions)
        {
            return false;

        }


        static System.Windows.Forms.Control FindControlByTag(ENV.UI.Form form, string controlTag)
        {
            if (string.IsNullOrEmpty(controlTag.Trim())) return form.LastFocusedControl;

            return form.FindControlByTag(controlTag);
        }

        public Number CX(Text controlName, Number generation)
        {
            return CLeft(controlName, generation);
        }

        public Number CWidth(Text controlName, Number generation)
        {
            if (IsNull(controlName, generation))
                return null;
            ITask task = GetTaskByGeneration(generation);
            if (task != null)
            {
                ENV.UI.Form form = task.View as ENV.UI.Form;
                if (form != null)
                {
                    var control = FindControlByTag(form, controlName.Trim());
                    if (control != null)
                    {
                        var cb = control as ControlBase;
                        return Math.Round((cb != null ? cb.DeferredWidth : control.Width) * form.HorizontalExpressionFactor / form.HorizontalScale);
                    }
                }
            }
            return Number.Zero;

        }
        public Bool ControlItemsRefresh(Text controlName, Number generation)
        {
            if (IsNull(controlName, generation))
                return null;
            ITask task = GetTaskByGeneration(generation);
            if (task != null)
            {
                ENV.UI.Form form = task.View as ENV.UI.Form;
                if (form != null)
                {
                    var control = FindControlByTag(form, controlName.Trim()) as ListControlBase;
                    if (control != null)
                    {
                        control.ReloadListSource();
                    }
                }
            }
            return false;

        }

        public Number CHeight(Text controlName, Number generation)
        {
            if (IsNull(controlName, generation))
                return null;
            ITask task = GetTaskByGeneration(generation);
            if (task != null)
            {
                ENV.UI.Form form = task.View as ENV.UI.Form;
                if (form != null)
                {
                    var control = FindControlByTag(form, controlName.Trim());
                    if (control != null)
                    {
                        var cb = control as ControlBase;
                        return Math.Round((cb != null ? cb.DeferredHeight : control.Height) * form.VerticalExpressionFactor / form.VerticalScale);
                    }
                }
            }
            return Number.Zero;
        }

        static System.Windows.Forms.Control GetParentControlOrGridColumn(System.Windows.Forms.Control controlBase)
        {
            var c = controlBase.Parent;
            while (c != null && !(c is Types.UI.Grid) && !(c is System.Windows.Forms.Form))
                c = c.Parent;
            var grid = c as Types.UI.Grid;
            if (grid != null)
            {
                foreach (System.Windows.Forms.Control control in grid.Controls)
                    if (control is Types.UI.GridColumn && control.Controls.ContainsKey(controlBase.Name))
                        return control;
            }
            return controlBase.Parent;
        }

        public Number CY(Text controlName, Number generation)
        {
            return CTop(controlName, generation);
        }

        public Number CTop(Text controlName, Number generation)
        {
            return GetControlTop(controlName, generation, false);
        }

        public Number CTopMDI(Text controlName, Number generation)
        {
            return GetControlTop(controlName, generation, true);
        }

        Number GetControlTop(Text controlName, Number generation, bool relativeToTopMostForm)
        {
            if (IsNull(controlName, generation))
                return null;
            ITask task = GetTaskByGeneration(generation);
            if (task != null)
            {
                ENV.UI.Form form = task.View as ENV.UI.Form;
                if (form != null)
                {
                    var control = FindControlByTag(form, controlName.Trim());
                    if (control != null)
                    {
                        var cb = control as ControlBase;
                        var top = cb != null ? cb.DeferredTop : control.Top;
                        System.Windows.Forms.Control parent = GetParentControlOrGridColumn(control);
                        while (!(parent is System.Windows.Forms.Form))
                        {
                            ControlBase controlBase = parent as ControlBase;
                            if (controlBase != null)
                            {
                                top += controlBase.DeferredTop;
                                if (controlBase is Types.UI.GridColumn)
                                {
                                    var g = controlBase.Parent as Types.UI.Grid;
                                    if (g != null && !Types.Text.IsNullOrEmpty(controlName))
                                    {
                                        top -= g.ActiveRowIndex * g.RowHeight;

                                    }
                                }
                            }
                            else
                                top += parent.Top;
                            parent = parent.Parent;
                        }
                        while (relativeToTopMostForm && parent != null && parent.Parent != null && parent.Parent != Common.ContextTopMostForm)
                        {
                            if (parent.Parent != null)
                                top = parent.Parent.PointToClient(parent.PointToScreen(new Point(0, top))).Y;
                            parent = parent.Parent;
                        }
                        return Math.Round(top * form.VerticalExpressionFactor / form.VerticalScale);
                    }
                }
            }
            return Number.Zero;
        }

        public Number CLeftMDI(Text controlName, Number generation)
        {
            return GetControlLeft(controlName, generation, true);
        }

        Number GetControlLeft(Text controlName, Number generation, bool relativeToTopMostForm)
        {
            if (IsNull(controlName, generation))
                return null;
            ITask task = GetTaskByGeneration(generation);
            if (task != null)
            {
                ENV.UI.Form form = task.View as ENV.UI.Form;
                if (form != null)
                {
                    var control = FindControlByTag(form, controlName.Trim());
                    if (control != null)
                    {
                        var cb = control as ControlBase;
                        var left = cb != null ? cb.DeferredLeft : control.Left;
                        System.Windows.Forms.Control parent = GetParentControlOrGridColumn(control);
                        while (!(parent is System.Windows.Forms.Form))
                        {
                            ControlBase controlBase = parent as ControlBase;
                            if (controlBase != null)
                                left += controlBase.DeferredLeft;
                            else
                                left += parent.Left;
                            parent = parent.Parent;
                        }
                        while (relativeToTopMostForm && parent != null && parent.Parent != null && parent.Parent != Common.ContextTopMostForm)
                        {
                            if (parent.Parent != null)
                                left = parent.Parent.PointToClient(parent.PointToScreen(new Point(left, 0))).X;
                            parent = parent.Parent;
                        }
                        return Math.Round(left * form.HorizontalExpressionFactor / form.HorizontalScale);
                    }
                }
            }
            return Number.Zero;
        }


        public Number CLeft(Text controlName, Number generation)
        {
            return GetControlLeft(controlName, generation, false);
        }

        public Text LastPark(Number generation)
        {
            if (InternalIsNull(generation))
                return null;
            ITask task = GetTaskByGeneration(generation);
            if (task != null)
            {
                Form form = task.View;
                if (form != null && form.LastFocusedControl != null)
                {
                    try
                    {
                        return (string)form.LastFocusedControl.Tag ?? Types.Text.Empty;
                    }
                    catch
                    {
                    }
                }
            }
            return Types.Text.Empty;
        }


        public Bool TreeNodeGoto(object nodeId)
        {
            if (nodeId is ColumnBase)
                nodeId = ((ColumnBase)nodeId).Value;
            var treeView = GetTree(_currentTask);
            if (treeView != null)
                return treeView.SelectNodeById(nodeId);
            return false;
        }

        public Bool WinRestore()
        {
            return ResMagic();
        }

        [DllImport("hhctrl.ocx", SetLastError = true, EntryPoint = "HtmlHelpW", CharSet = CharSet.Unicode)]
        static extern bool HtmlHelp(IntPtr hWndCaller, [MarshalAs(UnmanagedType.LPWStr)] string helpFile, uint command, uint data);
        public Bool WinHelp(Text helpFileName, Number command, Text key)
        {
            if (command < 1 || command > 9) return false;
            if (command > 2 && command < 9) return false;
            var result = false;
            if (Types.Text.IsNullOrEmpty(helpFileName))
                return false;
            helpFileName = PathDecoder.DecodePath(helpFileName).Trim();
            var c = new uint[] { 0xF, 1, 1, 1, 1, 1, 1, 1, 0x12 }[command - 1];
            if (c == 0xF && Types.Text.IsNullOrEmpty(key))
                c = 2;
            Common.RunOnContextTopMostForm(form => result = HtmlHelp(form.Handle, helpFileName, c, Number.Parse(key)));
            return result;
        }

        public Bool ResMagic()
        {
            Common.RunOnContextTopMostForm(mdi => mdi.WindowState = FormWindowState.Normal);
            return true;
        }

        public Bool WinMaximize()
        {
            return MAXMagic();
        }

        public Bool WinMinimize()
        {
            return MINMagic();
        }

        public Bool MAXMagic()
        {
            Common.RunOnContextTopMostForm(mdi => mdi.WindowState = FormWindowState.Maximized);
            return true;
        }

        public Bool MINMagic()
        {
            Common.RunOnContextTopMostForm(mdi => mdi.WindowState = FormWindowState.Minimized);
            return true;
        }

        public object EditGet()
        {

            var task = GetTaskByGeneration(0) as UIController;
            if (task != null)
            {
                var form = task.View;
                var focused = form != null ? (form is ENV.UI.Form ? ((ENV.UI.Form)form).FocusedControl : form.FocusedControl) : null;
                if (focused != null)
                {
                    ColumnBase dataColumn = null;
                    string text = null, format = null;
                    {
                        var tb = focused as Firefly.Box.UI.TextBox;

                        if (tb != null && tb.Data != null && tb.Data.Column != null)
                        {
                            dataColumn = tb.Data.Column;
                            text = tb.Text;
                            format = tb.Format;
                        }
                    }
                    {
                        var tb = focused as Firefly.Box.UI.RichTextBox;

                        if (tb != null && tb.Data != null && tb.Data.Column != null)
                        {
                            dataColumn = tb.Data.Column;
                            text = tb.Rtf;

                        }
                    }
                    {
                        var tb = focused as Firefly.Box.UI.Button;

                        if (tb != null && tb.Data != null && tb.Data.Column != null)
                        {
                            return tb.Data.Column.Value;
                        }
                    }
                    {
                        var tb = focused as Firefly.Box.UI.Advanced.ListControlBase;
                        if (tb != null && tb.Data != null && tb.Data.Column != null)
                            return tb.Data.Column.Value;
                    }


                    if (dataColumn != null)
                    {
                        var bac = dataColumn as ByteArrayColumn;
                        if (bac != null)
                        {
                            return bac.FromString(text);
                        }
                        var c = dataColumn;
                        try
                        {
                            var result = c.Parse(text, format);
                            var textResult = result as Text;
                            return textResult != null ? textResult.TrimEnd() : result;
                        }
                        catch
                        {
                            if (dataColumn.AllowNull && text == c.NullDisplayText)
                                return null;
                            return dataColumn.DefaultValue;
                        }
                    }
                }
            }
            return "";//W9691
        }

        public Number MarkText(Number start, Number length)
        {
            try
            {
                if (ReferenceEquals(start, null) || ReferenceEquals(length, null))
                    return null;
                var activeTasks = CurrentContext.ActiveTasks;
                if (activeTasks.Count == 0)
                    return 0;
                var currentTask = activeTasks[activeTasks.Count - 1];
                if (currentTask == null)
                    return 0;
                var currentView = currentTask.View;
                if (currentTask == null)
                    return 0;
                if (start < 1)
                    return 0;

                var activeTextBox = currentView.ActiveControl as Firefly.Box.UI.TextBox;
                if (activeTextBox == null)
                {
                    if (currentView.ActiveControl == null)
                        return 0;
                    activeTextBox = currentView.ActiveControl.Parent as Firefly.Box.UI.TextBox;
                    if (activeTextBox == null)
                        return 0;
                }

                if (length < 0)
                {
                    length = Math.Min(-length, start);
                    start = Math.Max(1, (int)(start - length) + 1);
                }

                activeTextBox.SelectionStart = start - 1;
                activeTextBox.SelectionLength = length;

                return activeTextBox.SelectedText.Length;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex);
                return 0;
            }
        }

        public Bool CtrlGoTo(Text controlName, Number gridRowNumber, Number generation)
        {
            bool result = false;
            if (controlName == null)
                return false;
            if (gridRowNumber == null)
                return false;
            var t = GetTaskByGeneration(generation);
            if (t == null)
                return false;
            var f = t.View as ENV.UI.Form;
            if (f == null)
                return false;
            var control = f.FindControlByTag(controlName.TrimEnd());
            var cb = control as ControlBase;

            if (control != null && (cb != null ? cb.Visible : control.Visible))
            {
                if (gridRowNumber > 0 || !UserSettings.VersionXpaCompatible)
                {
                    var p = control.Parent;
                    while (p != null && p != f)
                    {
                        var g = p as Firefly.Box.UI.Grid;
                        if (g == null)
                        {
                            p = p.Parent;
                            continue;
                        }
                        if (gridRowNumber > 0)
                        {
                            if (gridRowNumber - 1 == 0 || gridRowNumber - 1 <= g.GetIndexOfLastRowOfData())
                                g.TryActivatingRowByIndex(gridRowNumber - 1);
                            else
                            {
                                var hasFrom = true;
                                ControllerBase.SendInstanceBasedOnTaskAndCallStack(t, cb1 => hasFrom = cb1.From != null);
                                if (hasFrom)
                                    return false;
                                else
                                    g.TryActivatingRowByIndex(gridRowNumber - 1);
                            }
                        }
                        else if (g.ActiveRowIndex < 0)
                            return false;
                        break;
                    }
                }
                result = true;
                if (cb != null)
                    cb.TryFocus();
                else
                    f.TryFocusControl(control);
            }
            return result;
        }

        [NotYetImplemented]
        public Bool DragSetCrsr(Number i, Text s)
        {
            if (IsNull(i, s))
                return null;
            return false;
        }

        System.Drawing.Point DropPoint()
        {
            if (ControlBase.DragDropEventArgs == null || ControlBase.DragDropControl == null)
                return Point.Empty;
            var p = new System.Drawing.Point(ControlBase.DragDropEventArgs.X, ControlBase.DragDropEventArgs.Y);
            var f = Firefly.Box.UI.Advanced.ControlBase.DragDropControl.FindForm() as ENV.UI.Form;
            if (f == null)
                return Point.Empty;
            while (!f.IsMdiChild && f.Parent != null && f.Parent.FindForm() is ENV.UI.Form)
                f = f.Parent.FindForm() as ENV.UI.Form;
            Context.Current.InvokeUICommand(
                () =>
                {
                    p = f.PointToClient(p);
                    var t = GetTaskByGeneration(0);
                    p = TranslatePoint(p, t != null && t.View != null ? t.View : f);
                });
            return p;
        }

        public Number DropMouseX()
        {
            return DropPoint().X;
        }

        public Number DropMouseY()
        {
            return DropPoint().Y;
        }

        public Bool EditSet(object value)
        {

            if (IsNull(value))
                return null;
            var cb = value as ColumnBase;
            if (cb != null && IsNull(cb.Value))
                return null;

            var task = GetTaskByGeneration(0) as UIController;
            if (task != null)
            {
                var form = task.View;
                if (form != null)
                {
                    var tb = form.FocusedControl as Firefly.Box.UI.TextBox;
                    if (tb != null)
                    {
                        Firefly.Box.Context.Current.InvokeUICommand(() =>
                        {
                            string format = tb.Format;
                            if (string.IsNullOrEmpty(format) && tb.Data != null && tb.Data.Column != null)
                                format = tb.Data.Column.Format;
                            {
                                Text t;
                                if (Types.Text.TryCast(value, out t))
                                    tb.Text = t.TrimEnd();
                            }
                            {
                                Number t;
                                if (Number.TryCast(value, out t))
                                    tb.Text = t.ToString();
                            }
                            {
                                Date t;
                                if (Types.Date.TryCast(value, out t))
                                    tb.Text = t.ToString(format);
                            }
                            tb.Select(tb.Text.Length, 0);
                        });
                        return true;

                    }
                }
            }

            return true;
        }


        public Text HandledCtrl()
        {
            var task = GetTaskByGeneration(0);
            if (task != null && task.CurrentHandledControl != null && task.CurrentHandledControl.Tag != null)
                return task.CurrentHandledControl.Tag.ToString();
            return "";
        }

        public int HitZOrdr()
        {
            if (Form.LastClickedControl == null) return 0;
            var form = Form.LastClickedControl.FindForm() as Types.UI.Form;
            if (form == null) return 0;

            var i = -1;
            var result = 0;
            form.ForEachControlInZOrder(
                control =>
                {
                    i++;
                    if (control == Form.LastClickedControl)
                        result = i;
                    else
                    {
                        var grid = control as Firefly.Box.UI.Grid;
                        if (grid != null)
                        {
                            grid.ForEachControlInZOrder(
                                control1 =>
                                {
                                    i++;
                                    if (control1 == Form.LastClickedControl)
                                        result = i;
                                });
                        }
                    }
                });

            return result;
        }
        public Text DirDlg(Text description, Text root, Bool showNewFolder)
        {
            string result = string.Empty;
            Context.Current.InvokeUICommand(
                () =>
                {
                    using (var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog())
                    {
                        folderBrowserDialog.Description = description.TrimEnd();
                        var p = root.Trim().ToString();
                        folderBrowserDialog.SelectedPath = Directory.Exists(p) ? p : Directory.GetCurrentDirectory();
                        folderBrowserDialog.ShowNewFolderButton = showNewFolder;
                        DialogResult dialogResult = folderBrowserDialog.ShowDialog(Common.ContextTopMostForm);
                        if (dialogResult == DialogResult.OK)
                            result = folderBrowserDialog.SelectedPath;
                    }
                });
            return result;
        }
        public Text DirDlg(Text caption, Text description, Text root, Number flags)
        {
            string result = string.Empty;
            Context.Current.InvokeUICommand(
                () =>
                {
                    using (UI.FolderBrowser folderBrowserDialog = new UI.FolderBrowser())
                    {
                        folderBrowserDialog.Title = description;
                        folderBrowserDialog.DirectoryPath = root;
                        folderBrowserDialog.Caption = caption;
                        folderBrowserDialog.Flags = (ENV.UI.BrowseFlags)(int)flags;
                        DialogResult dialogResult = folderBrowserDialog.ShowDialog();
                        if (dialogResult == DialogResult.OK)
                            result = folderBrowserDialog.DirectoryPath;
                    }
                });
            return result;
        }

        public Text ClientDirDlg(Text description, Text root, Bool showNewFolder)
        {
            return DirDlg(description, root, showNewFolder);
        }

        public Text FileDlg(Text fileGroupDescritpion, Text fileExtentions)
        {
            Context.Current.DiscardPendingCommands();

            if (fileGroupDescritpion == null)
                fileGroupDescritpion = "";
            if (fileExtentions == null)
                fileExtentions = "";

            fileExtentions = PathDecoder.DecodePath(fileExtentions.TrimEnd());
            string result = "";
            Context.Current.InvokeUICommand(
                () =>
                {
                    var ofd = new System.Windows.Forms.OpenFileDialog
                    {
                        RestoreDirectory
                            =
                            true
                    };
                    try
                    {
                        var startPath = fileExtentions.ToString();
                        startPath = RemoveZeroChar(startPath.Split(';')[0]);

                        ofd.InitialDirectory =
                            System.IO.Path.GetDirectoryName(startPath);
                        if (string.IsNullOrEmpty(ofd.InitialDirectory))
                        {
                            if ((startPath.Length == 2 || startPath.Length == 3) &&
                                startPath[1] == ':')
                                ofd.InitialDirectory = fileExtentions;
                        }
                        if (!string.IsNullOrEmpty(ofd.InitialDirectory))
                        {
                            var indexOfZero = fileExtentions.IndexOf('\0');
                            if (indexOfZero > 0)
                            {
                                var before = fileExtentions.Remove(indexOfZero);
                                var after = fileExtentions.Substring(indexOfZero);
                                fileExtentions = Path.GetFileName(before) + after;
                            }
                            else

                                fileExtentions = System.IO.Path.GetFileName(fileExtentions);
                            while (fileExtentions.ToString().StartsWith("\\"))
                                fileExtentions = fileExtentions.Substring(1);
                            ofd.InitialDirectory =
                                Path.GetFullPath(ofd.InitialDirectory);
                        }
                    }
                    catch
                    {
                    }
                    try
                    {
                        if (fileExtentions.Length > 0)
                        {
                            if (fileExtentions[0] == '.')
                                fileExtentions = "*" + fileExtentions;
                            ofd.Filter = fileGroupDescritpion.Replace("\0", "|") +
                                         "|" + fileExtentions.Replace("\0", "|");
                        }
                        else
                            ofd.Filter = fileGroupDescritpion + "|" + "*.*";
                    }
                    catch
                    {
                    }

                    ofd.CheckFileExists = false;
                    ofd.AddExtension = false;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        if (PerformHebrewV8OemParts)
                            result = HebrewOemTextStorage.Encode(ofd.FileName);
                        else
                            result = ofd.FileName;
                    }
                });

            return result;
        }

        internal static bool PerformHebrewV8OemParts = false;

        System.Drawing.Point TranslatePoint(System.Drawing.Point point, System.Windows.Forms.Form f)
        {
            System.Drawing.Point result = point;
            var form = f as ENV.UI.Form;
            if (form == null)
                return point;
            result.X = (int)(result.X * form.HorizontalExpressionFactor / form.HorizontalScale);
            result.Y = (int)(result.Y * form.VerticalExpressionFactor / form.VerticalScale);
            return result;
        }

        System.Drawing.Point GetLastControlClickLocation()
        {
            var tabControl = Firefly.Box.UI.Form.LastClickedControl as TabControl;
            if (tabControl != null)
            {
                var x = tabControl.LastClickedTabIndex;
                if (x < 0)
                    x = tabControl.SelectedIndex;
                return new System.Drawing.Point(x + 1, x + 1);
            }
            else
            {
                var control = Firefly.Box.UI.Form.LastClickedControl;
                if (control == null)
                    return new System.Drawing.Point(1, 1);
                else
                    return TranslatePoint(Firefly.Box.UI.Form.LastClickedLocationInControl, control.FindForm());
            }
        }
        public Number PixelsToFormUnits(Number value, Bool horizontal)
        {
            if (value == null || horizontal == null)
                return null;
            var t = GetTaskByGeneration(0);
            var form = t.View as ENV.UI.Form;
            if (form == null)
                return null;
            if (horizontal)
                return value / form.HorizontalScale;
            else return value / form.VerticalScale;

        }

        public Number ClickCY()
        {
            return GetLastControlClickLocation().Y;
        }

        public Number ClickCX()
        {
            return GetLastControlClickLocation().X;
        }

        public Number ClickWY()
        {
            return TranslatePoint(Firefly.Box.UI.Form.LastClickedLocationInForm, Firefly.Box.UI.Form.LastClickedForm).Y;
        }

        public Number ClickWX()
        {
            return TranslatePoint(Firefly.Box.UI.Form.LastClickedLocationInForm, Firefly.Box.UI.Form.LastClickedForm).X;
        }

        public Bool SetWindowFocus(Text windowName)
        {
            if (string.IsNullOrEmpty(windowName)) return false;
            windowName = windowName.TrimEnd();
            if (windowName == "Magic xpa Print Preview")
                windowName = "Print Preview";
            foreach (System.Windows.Forms.Form f in System.Windows.Forms.Application.OpenForms)
            {
                if (f.Text == windowName)
                {
                    var form = f as Firefly.Box.UI.Form;
                    if (form != null)
                    {
                        form.TryFocus(() => { });
                        return true;
                    }
                    else
                    {

                        f.BeginInvoke(new Action(f.Activate));
                    }
                }
            }
            return false;
        }

        #endregion

        #region Settings

        public object GetParam(Text paramName)
        {
            var result = ParametersInMemory.Instance.Get(paramName);
            var bac = result as ByteArrayColumnValue;
            if (bac != null)
                result = bac.Value;
            return _nullStrategy.MemoryParameterResult(result);
        }

        public Text GetTextParam(Text paramName)
        {
            return CastToText(_nullStrategy.MemoryParameterResult(ParametersInMemory.Instance.Get(paramName)));
        }

        public Text[][][] GetTextArrayArrayArrayParam(Text paramName)
        {
            return GetTypedParam<Text[][][]>(paramName);
        }

        T GetTypedParam<T>(Text paramName) where T : class
        {
            var o = GetParam(paramName);
            var result = o as T;
            if (result != null)
                return result;
            var c = o as TypedColumnBase<T>;
            if (c != null)
                return c.Value;
            return null;

        }


        public Text[][] GetTextArrayArrayParam(Text paramName)
        {
            return GetTypedParam<Text[][]>(paramName);
        }
        public Text[] GetTextArrayParam(Text paramName)
        {
            return GetTypedParam<Text[]>(paramName);
        }
        public Number GetNumberParam(Text paramName)
        {
            return CastToNumber(GetParam(paramName));
        }

        public Date GetDateParam(Text paramName)
        {
            return CastToDate(GetParam(paramName));
        }

        public Time GetTimeParam(Text paramName)
        {
            return CastToTime(GetParam(paramName));
        }

        public Bool GetBoolParam(Text paramName)
        {
            return CastToBool(GetParam(paramName));
        }

        public byte[] GetByteArrayParam(Text paramName)
        {
            return CastToByteArray(GetParam(paramName));
        }

        public object SharedValGet(Text paramName)
        {
            var result = ParametersInMemory.SharedInstance.Get(paramName);
            var bac = result as ByteArrayColumnValue;
            if (bac != null)
                result = bac.Value;
            return _nullStrategy.MemoryParameterResult(result);
        }
        public Number[] SharedValGetNumberArray(Text paramName)
        {
            return CastToNumberArray(SharedValGet(paramName));
        }
        public Text[] SharedValGetTextArray(Text paramName)
        {
            return CastToTextArray(SharedValGet(paramName));
        }

        public Text SharedValGetText(Text paramName)
        {
            return CastToText(_nullStrategy.MemoryParameterResult(ParametersInMemory.SharedInstance.Get(paramName)));
        }

        public Time SharedValGetTime(Text paramName)
        {
            return CastToTime(SharedValGet(paramName));
        }

        public Date SharedValGetDate(Text paramName)
        {
            return CastToDate(SharedValGet(paramName));
        }

        public Number SharedValGetNumber(Text paramName)
        {
            return CastToNumber(SharedValGet(paramName));
        }

        public Bool SharedValGetBool(Text paramName)
        {
            return CastToBool(SharedValGet(paramName));
        }

        public byte[] SharedValGetByteArray(Text paramName)
        {
            return CastToByteArray(SharedValGet(paramName));
        }

        public Bool SharedValSet(Text paramName, object v)
        {
            object value = v;
            var c = value as ColumnBase;
            if (c != null)
            {
                value = c.Value;

            }
            if (NullBehaviour.IsNull(value))
                value = null;
            var bac = v as ByteArrayColumn;
            if (bac != null && bac.ContentType == ByteArrayColumnContentType.Unicode)
            {
                value = new ByteArrayColumnValue(bac);
            }
            bool result = ParametersInMemory.SharedInstance.Set(paramName, value);
            if (paramName == null || (object)value == null)
                return null;
            return result;
        }
        class ByteArrayColumnValue
        {
            public readonly byte[] Value;
            public readonly string StringValue;
            public ByteArrayColumnValue(ByteArrayColumn bac)
            {
                Value = bac.Value;
                StringValue = bac.ToString();
            }
        }

        public Bool SetParam(Text paramName, Func<object> v)
        {
            object o = null;
            if (v != null)
                InsteadOfNullStrategy.Instance.OverrideAndCalculate(() => o = v());

            return SetParam(paramName, o);
        }
        public Bool SetParam(Text paramName, object v)
        {
            object value = v;
            var c = value as ColumnBase;
            if (c != null)
            {
                value = c.Value;

            }
            if (NullBehaviour.IsNull(value))
                value = null;
            var bac = v as ByteArrayColumn;
            if (bac != null && bac.ContentType == ByteArrayColumnContentType.Unicode)
            {
                value = new ByteArrayColumnValue(bac);
            }
            bool result = ParametersInMemory.Instance.Set(paramName, value);
            if (paramName == null || (object)v == null)
                return null;
            return result;
        }

        public bool IniPut(Text textToPut, Bool writeToIniFile)
        {
            try
            {
                return UserSettings.ParseAndSet(textToPut, writeToIniFile, false, writeToIniFile);
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "IniPut");
                return false;
            }
        }

        public bool IniPut(Text textToPut)
        {
            return IniPut(textToPut, true);
        }

        public Text IniGet(Text nameToGet)
        {
            if (ReferenceEquals(nameToGet, null))
                return null;
            var result = UserSettings.ParseAndGet(nameToGet);
            if (nameToGet.StartsWith("[MAGIC_DATABASES]"))
            {
                var s = result.Split(',');
                for (int i = 0; i < s.Length; i++)
                {
                    s[i] = s[i].TrimStart();
                }
                return string.Join(",", s);
            }
            return result;
        }

        public Text IniGetLn(Text iniTagName, Number lineNumber)
        {
            if (IsNull(iniTagName, lineNumber))
                return null;
            string textToFind = iniTagName.Trim();
            if (iniTagName.Length > 2)
                textToFind = textToFind.Substring(1, textToFind.Length - 2);
            return UserSettings.GetItemAt(textToFind, lineNumber);
        }

        static string _owner;

        public static void SetOwner(string value)
        {
            _owner = value;
        }

        public Text Owner()
        {
            return _owner ?? "";
        }

        public Text Pref()
        {
            return _applicationPrefix();
        }

        public static string __applicationPrefix = "";
        public Func<string> _applicationPrefix = () => __applicationPrefix;

        public static void SetApplicationPrefix(string appPrefix)
        {
            if (!string.IsNullOrEmpty(__applicationPrefix))
                __applicationPrefix = appPrefix;
        }

        static Number _terminal = 1;

        internal static void SetTerminal(Number terminalNumber)
        {
            _terminal = terminalNumber;
        }

        public Number Term()
        {
            return _terminal;
        }

        public static void SetTerminalAcordingToMSSQLSpid(DynamicSQLSupportingDataProvider connection)
        {
            try
            {
                using (var c = connection.CreateCommand())
                {
                    c.CommandText = "select @@spid";
                    using (var r = c.ExecuteReader())
                    {
                        r.Read();
                        Number n = (short)r[0];
                        ENV.UserMethods.SetTerminal(n % 10000);
                    }
                }
            }
            catch
            {
            }
        }

        #endregion

        #region Security

        public Text User(Number type)
        {
            if (type == null)
                return null;
            switch ((int)type)
            {

                case 1:
                    return Security.UserManager.CurrentUser.Description;
                case 2:
                    return Security.UserManager.CurrentUser.AdditionalInfo;
                default:
                case 0:
                    return Security.UserManager.CurrentUser.Name;

            }
        }
        public Bool UserAdd(Text userName, Text description, ByteArrayColumn password, Text additionalInfo)
        {
            return UserAdd(userName, description, password.ToString(), additionalInfo);
        }
        public Bool UserAdd(Text userName, Text description, byte[] password, Text additionalInfo)
        {
            return UserAdd(userName, description, TextColumn.FromByteArray(password), additionalInfo);
        }
        public Bool UserAdd(Text userName, Text description, Text password, Text additionalInfo)
        {
            if (IsNull(userName, description, password, additionalInfo))
                return null;
            try
            {
                UserManager.ChangeUserFile(db =>
                {
                    db.CreateUser(userName, password, description, additionalInfo);
                    return true;
                });

                return true;
            }
            catch
            {
            }
            return false;
        }

        public Bool RightAdd(Text username, Text roleKey)
        {
            if (IsNull(username, roleKey))
                return null;
            try
            {
                bool ok = false;
                UserManager.ChangeUserFile(db =>
                {
                    Security.User u = db.FindUser(username);
                    if (u != null)
                    {
                        u.AddRole(roleKey, db);
                        ok = true;
                    }
                    return true;
                });
                return ok;
            }
            catch
            {
            }
            return false;
        }

        public Bool GroupAdd(Text username, Text groupName)
        {
            if (IsNull(username, groupName))
                return null;
            try
            {
                bool ok = false;
                UserManager.ChangeUserFile(
                    db =>
                    {
                        Security.User u = db.FindUser(username);
                        if (u != null)
                        {
                            u.AddGroup(groupName, db);
                            ok = true;

                        }
                        return true;
                    });
                return ok;

            }
            catch
            {
            }
            return false;
        }

        public Bool UserDel(Text username)
        {
            if (InternalIsNull(username))
                return null;
            try
            {

                if (!UserManager.CanDelete(username))
                    return false;
                UserManager.ChangeUserFile(db =>
                {
                    db.DeleteUser(username);
                    return true;
                });
                return true;
            }
            catch
            {
            }
            return false;
        }

        public Bool Logon(Text username, Text password)
        {
            if (username == null || password == null)
                return null;
            return Security.UserManager.Login(username, password);


        }
        public Bool Logon(Text username, byte[] password)
        {
            return Logon(username, TextColumn.FromByteArray(password));


        }
        public Bool Logon(Text username, ByteArrayColumn password)
        {
            return Logon(username, password.ToString());


        }

        public Bool Rights(Role role)
        {
            if (role == null)
                return null;
            return role.Allowed;
        }
        internal static Text InvalidRight = "345678okvcg4fgther6731q~!@#%$@#$^#$%&";
        public Bool Rights(Text roleKey)
        {
            if (ReferenceEquals(roleKey, null))
                return null;
            if (roleKey == InvalidRight)
                return false;
            return new Role(roleKey, roleKey.Trim()).Allowed;
        }

        public Bool Rights(Number roleNumber)
        {
            if (_application == null || _application._applicationRoles == null)
                return false;
            return _application._applicationRoles.IsRoleAllowed(roleNumber);
        }

        public Text LDAPError()
        {
            return LdapClient.LastException != null ? LdapClient.LastException.Message : "Success";
        }

        [NotYetImplemented]
        public object LMChkOut(Text a, Text b, Text c)
        {
            if (IsNull(a, b, c))
                return null;
            return null;
        }

        [NotYetImplemented]
        public object LMChkIn(Text a)
        {
            if (InternalIsNull(a))
                return null;
            return null;
        }

        System.DirectoryServices.Protocols.SearchScope ParseLdapSearchLevel(Text searchlevel)
        {
            var scope = System.DirectoryServices.Protocols.SearchScope.Base;
            switch (searchlevel.ToUpper(CultureInfo.InvariantCulture))
            {
                case "T":
                    scope = System.DirectoryServices.Protocols.SearchScope.Subtree;
                    break;
                case "O":
                    scope = System.DirectoryServices.Protocols.SearchScope.OneLevel;
                    break;
            }
            return scope;
        }

        public Text LDAPGet(Text searchBase, Text searchlevel, Text searchfilter, Text attribute, Text delimiter,
                            Number ldapConnection)
        {
            LdapClient c;
            if (!_ldapConnections.TryGetValue((ushort)ldapConnection, out c))
                return "";
            try
            {
                return string.Join(delimiter,
                                   c.Search(searchBase, ParseLdapSearchLevel(searchlevel), searchfilter, attribute));
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }

        public Text LDAPGet(Text searchBase, Text searchlevel, Text searchfilter, Text attribute, Text delimiter)
        {
            try
            {
                return string.Join(delimiter,
                                   LdapClient.DefaultClient.Search(searchBase, ParseLdapSearchLevel(searchlevel),
                                                                   searchfilter, attribute));
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }

        static ushort _ldapConnectionHandleSequence = 0;
        Dictionary<ushort, LdapClient> _ldapConnections = new Dictionary<ushort, LdapClient>();

        public Number LDAPConnect(Text ldapAddress, Text ldapConnectionString, Text LdapDomainContext, Text user,
                                  Text ldapUserPassword)
        {
            var handle = ++_ldapConnectionHandleSequence;
            try
            {
                var c = new LdapClient(ldapAddress);
                c.Connect(ldapConnectionString.Replace("$USER$", user.TrimEnd()), ldapUserPassword.TrimEnd());
                _ldapConnections.Add(handle, c);
            }
            catch (Exception e)
            {
                return -1;
            }
            return handle;
        }

        public Bool LDAPDisconnect(Number ldapContext)
        {
            LdapClient c;
            if (_ldapConnections.TryGetValue((ushort)ldapContext, out c))
            {
                c.Dispose();
                _ldapConnections.Remove((ushort)ldapContext);
            }
            return false;
        }

        public int Group()
        {
            return 0;
        }

        #endregion

        #region Others

        public Bool FlwMtr(Text valueToWrite, Bool breakToDebugger)
        {

            if (valueToWrite == null || breakToDebugger == null)
                return null;
            ENV.ErrorLog.WriteTraceLine(valueToWrite);
            ErrorLog.WriteTrace(() => valueToWrite.TrimEnd());
            if (breakToDebugger && System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
            return false;
        }
        public Bool FlwMtrVars()
        {
            return false;
        }

        public Text Translate(Text text)
        {
            if (text == null)
                return null;
            return PathDecoder.DecodePath(text);
        }
        public Text Translate(ByteArrayColumn text)
        {
            if (text == null)
                return null;
            return PathDecoder.DecodePath(text.ToString());
        }
        public Text Translate(byte[] text)
        {
            if (text == null)
                return null;
            return PathDecoder.DecodePath(TextColumn.FromByteArray(text));
        }

        public Text TranslateNR(Text text)
        {
            if (text == null)
                return null;
            return PathDecoder.DecodePathAndKeepChar(text);
        }
        public static bool SplitDelay { get; set; }
        public Bool Delay(Number i)
        {
            if (i == null)
                return null;
            if (i > 3599999)
                i = 1;
            if (!SplitDelay)
            {
                Context.Current.Suspend(i * 100, true);
            }
            else
            {
                for (int j = 0; j < i; j++)
                {
                    var result = (int)Context.Current.Suspend(100, true);
                    if (result != 0)
                        return true;
                }
            }
            return true;
        }

        internal static void SetIdleInterval(int interval)
        {
            _idleInterval = interval;
        }

        static int _idleInterval = 1;

        public Number Idle()
        {
            if (_idleInterval == 0) return 0;
            return (System.Math.Floor((double)((DateTime.Now - _lastInput).TotalSeconds / _idleInterval)) * _idleInterval) * 10;
        }


        public Text EvalStr(Text expression, Text defaultResult)
        {
            if (IsNull(expression, defaultResult))
                return null;
            return e.Evaluate(expression, defaultResult);
        }
        public Number EvalStr(Text expression, Number defaultResult)
        {

            if (IsNull(expression, defaultResult))
                return null;
            return e.Evaluate(expression, defaultResult);

        }

        public Date EvalStr(Text expression, Date defaultResult)
        {
            if (IsNull(expression, defaultResult))
                return null;
            return e.Evaluate(expression, defaultResult);
        }

        public Time EvalStr(Text expression, Time defaultResult)
        {
            if (IsNull(expression, defaultResult))
                return null;
            return e.Evaluate(expression, defaultResult);
        }

        public Bool EvalStr(Text expression, Bool defaultResult)
        {
            if (IsNull(expression))
                return null;
            return e.Evaluate(expression, defaultResult);
        }

        EvaluateExpressions e;



        public T[] EvalStr<T>(Text expression, T[] defaultResult)
        {

            if (IsNull(expression))
                return null;
            return e.Evaluate(expression, defaultResult);
        }

        public object EvalStr(Text expression, object defaultResult)
        {
            if (IsNull(expression))
                return null;
            return e.Evaluate(expression, defaultResult);
        }


        public Text EvalStrInfo(Text expression, Number option)
        {
            try
            {
                var x = e.GetReturnValue(expression);
                if (option == 1)
                {
                    return x;

                }
                if (option == 3)
                    return e.GetExpression(expression);
                return "";
            }
            catch (Exception e)
            {
                if (option == 1)
                    return "Error";
                else if (option == 2)
                    return e.Message;
                else if (option == 3)
                    return expression;
                return "";
            }

        }

        public Number[] EvalStr(Text expression, Number[] defaultResult)
        {
            return EvalStr<Number>(expression, defaultResult);
        }

        public Text[] EvalStr(Text expression, Text[] defaultResult)
        {
            return EvalStr<Text>(expression, defaultResult);
        }

        public Date[] EvalStr(Text expression, Date[] defaultResult)
        {
            return EvalStr<Date>(expression, defaultResult);
        }

        public Time[] EvalStr(Text expression, Time[] defaultResult)
        {
            return EvalStr<Time>(expression, defaultResult);
        }

        public Bool[] EvalStr(Text expression, Bool[] defaultResult)
        {
            return EvalStr<Bool>(expression, defaultResult);
        }

        public Text MlsTrans(Text textToTranslate)
        {
            if (textToTranslate == null)
                return null;
            return Languages.Translate(textToTranslate);
        }

        public Text Env(Text s)
        {
            if (InternalIsNull(s))
                return null;

            switch (s.ToUpper(CultureInfo.InvariantCulture))
            {
                case "0":
                case "A":
                    return "";
                case "O":
                case "OWN":
                case "OWNER":
                    return Owner();
                default:
                    throw new NotImplementedException("Yet to implement the function Env that gets " + s +
                                                      " as a parameter");
            }
        }

        [NotYetImplemented]
        public Bool CallURL(Bool s, Bool s1)
        {
            if (IsNull(s, s1))
                return null;
            return false;
        }

        #region error handeling

        public static DatabaseErrorEventArgs _currentDatabaseError
        {
            get { return Context.Current["UserMethodsDatabaseError"] as DatabaseErrorEventArgs; }

        }

        public static void SetCurrentHandledDatabaseError(DatabaseErrorEventArgs e)
        {
            Context.Current["UserMethodsDatabaseError"] = e;
        }

        public Text DbERR(Text dataSourceName)
        {
            if (dataSourceName == null)
                return null;
            return "";
        }

        public Text ErrDatabaseName()
        {
            if (_currentDatabaseError != null)
            {
                {
                    var e = _currentDatabaseError.Entity as Entity;
                    if (e != null)
                    {
                        var d = e.DataProvider as DynamicSQLSupportingDataProvider;
                        if (d != null)
                            return d.Name;
                        var bd = e.DataProvider as BtrieveDataProvider;
                        if (bd != null)
                            return bd.Name;
                        return e.DataProvider.ToString();
                    }
                }
                {
                    var e = _currentDatabaseError.Entity as DynamicSQLEntity;
                    if (e != null)
                    {
                        var d = e.DataProvider as DynamicSQLSupportingDataProvider;
                        if (d != null)
                            return d.Name;
                        return e.DataProvider.ToString();
                    }
                }
            }
            return Types.Text.Empty;

        }

        public Text ErrDbmsMessage()
        {
            if (_currentDatabaseError != null)
                if (_currentDatabaseError.Exception != null)
                    return _currentDatabaseError.Exception.Message;
                else
                    return _currentDatabaseError.ErrorType.ToString();
            return Types.Text.Empty;
        }

        public Text ErrTableName()
        {
            if (_currentDatabaseError != null)
            {
                if (_currentDatabaseError.Entity is DynamicSQLEntity)
                    return "Direct SQL Statement";
                var e = _currentDatabaseError.Entity as Entity;
                if (e != null)
                    return InternalDbName(e, 0);
                return _currentDatabaseError.Entity.EntityName;
            }
            return Types.Text.Empty;
        }

        public Number ErrDbmsCode()
        {
            if (_currentDatabaseError != null)
            {
                var e = _currentDatabaseError.Exception;
                while (e is DatabaseErrorException)
                    e = e.InnerException;

                var ee = e as System.Data.SqlClient.SqlException;
                if (ee != null)
                    return ee.Number;
                if (_currentDatabaseError.Exception.Message.StartsWith("ORA-"))
                {
                    var s = _currentDatabaseError.Exception.Message.Substring(4);
                    s = s.Remove(s.IndexOf(':'));
                    return Number.Parse(s);
                }
            }
            return Number.Zero;

        }

        public Text ErrMagicName()
        {
            if (_currentDatabaseError != null)
                return "[" + LocalizationInfo.Current.GetErrorFor(_currentDatabaseError.ErrorType) + "]";
            return Types.Text.Empty;
        }

        #endregion

        #endregion

        #region mail

        static ContextStatic<SmtpMailClient> _smtp = new ContextStatic<SmtpMailClient>();
        static ContextStatic<ImapMailClient> _imap = new ContextStatic<ImapMailClient>();
        static ContextStatic<Pop3MailClient> _pop3 = new ContextStatic<Pop3MailClient>();
        static ContextStatic<MailClient> _activeMailClient = new ContextStatic<MailClient>(() => _imap.Value);
        [NotYetImplemented]
        public Number MailBoxSet(Text folderName)
        {
            return 0;
        }

        public Number MailSend(string from, string to, string cc, string bcc, string subject,
                               string messageText, params string[] attachments)
        {
            try
            {
                _lastMailErrorCode = 0;
                var attachmentsList = new List<string>();
                foreach (var s in attachments)
                    foreach (var s1 in s.Split(','))
                        attachmentsList.Add(PathDecoder.DecodePath(s1));
                _smtp.Value.SendMail(from, (to ?? "").Trim(), (cc ?? "").Trim(), (bcc ?? "").Trim(), (subject ?? "").Trim(),
                               (messageText ?? "").Trim(), attachmentsList.ToArray());
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                ENV.ErrorLog.WriteToLogFile(ex, "");
                return _lastMailErrorCode = Math.Min(((int)ex.StatusCode) * -1, -1);
            }
            catch (InvalidRecipientAddressException ex)
            {
                ENV.ErrorLog.WriteToLogFile(ex, "");
                return _lastMailErrorCode = -100;
            }
            catch (AttachmentNotFoundException ex)
            {
                ENV.ErrorLog.WriteToLogFile(ex, "");
                return _lastMailErrorCode = -80;
            }
            catch (Exception ex)
            {
                ENV.ErrorLog.WriteToLogFile(ex, "");
                return _lastMailErrorCode = -1;
            }
            return 0;
        }

        Number _lastMailErrorCode = 0;
        public Number MailConnect(Number type, Text server, Text user, Text password)
        {
            _lastMailErrorCode = 0;
            if (!string.IsNullOrEmpty(user))
                user = Security.Entities.SecuredValues.Decode(user);
            if (!string.IsNullOrEmpty(password))
                password = Security.Entities.SecuredValues.Decode(password);
            try
            {
                if (type == 1)
                {
                    _smtp.Value.Connect(server.Trim(), user.Trim(), password.Trim());
                }
                else if (type == 2)
                {
                    _pop3.Value.Connect(server.Trim(), user.Trim(), password.Trim());
                    _activeMailClient.Value = _pop3.Value;
                    return _pop3.Value.TotalMessages;
                }
                else if (type == 3)
                {
                    _imap.Value.Connect(server.Trim(), user.Trim(), password.Trim());
                    _activeMailClient.Value = _imap.Value;
                    return _activeMailClient.Value.TotalMessages;
                }
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                ErrorLog.WriteToLogFile(ex, "Mail Failed with error {0}", ex.StatusCode);
                return _lastMailErrorCode = Math.Min(((int)ex.StatusCode) * -1, -1);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                ErrorLog.WriteToLogFile(ex, "Mail Failed with error {0}", ex.SocketErrorCode);
                switch (ex.SocketErrorCode)
                {
                    case System.Net.Sockets.SocketError.HostNotFound:
                        return _lastMailErrorCode = -100;
                    default:
                        return _lastMailErrorCode = -1;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex, "Mail Failed with error ");
                return _lastMailErrorCode = -1;
            }
            return 0;
        }

        public Text MailError(Number a)
        {
            if (InternalIsNull(a))
                return null;
            try
            {
                if (a < 0)
                    a *= -1;
                switch ((int)a)
                {
                    case 200:
                        return "(nonstandard success response, see rfc876)";
                    case 211:
                        return "System status, or system help reply";
                    case 214:
                        return "Help message";
                    case 220:
                        return "Service ready";
                    case 221:
                        return "Service closing transmission channel";
                    case 250:
                        return "Requested mail action okay, completed";
                    case 251:
                        return "User not local; will forward to <forward-path>";
                    case 354:
                        return "Start mail input; end with <CRLF>.<CRLF>";
                    case 421:
                        return "Service not available, closing transmission channel";
                    case 450:
                        return "Requested mail action not taken: mailbox unavailable";
                    case 451:
                        return "Requested action aborted: local error in processing";
                    case 452:
                        return "Requested action not taken: insufficient system storage";
                    case 500:
                        return "Syntax error, command unrecognized";
                    case 501:
                        return "Syntax error in parameters or arguments";
                    case 502:
                        return "Command not implemented";
                    case 503:
                        return "Bad sequence of commands";
                    case 504:
                        return "Command parameter not implemented";
                    case 521:
                        return "Domain does not accept mail (see rfc1846)";
                    case 530:
                        return "Access denied";
                    case 535:
                        return "SMTP Authentication unsuccessful/Bad username or password";
                    case 550:
                        return "Requested action not taken: mailbox unavailable";
                    case 551:
                        return "User not local; please try <forward-path>";
                    case 552:
                        return "Requested mail action aborted: exceeded storage allocation";
                    case 553:
                        return "Requested action not taken: mailbox name not allowed";
                    case 554:
                        return "Transaction failed";
                    case 100:
                        return "The address is not valid";
                    case 1:
                        return "No Connection";
                    case 80:
                        return "Failed to add attachment to the message";

                }
                var x = (System.Net.Mail.SmtpStatusCode)(int)a;
                return x.ToString();
            }
            catch
            {
                return "";
            }

        }


        public Number MailLastRC()
        {
            return _lastMailErrorCode;
        }

        public Number MailDisconnect(Number type, Bool deleteAll)
        {
            try
            {
                if (type == 1)
                {
                    _smtp.Value.Disconnect();
                }
                else if (type == 2)
                {
                    if (deleteAll)
                        _activeMailClient.Value.ClearMailbox();
                    _activeMailClient.Value.Disconnect();
                }
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return 0;
            }
            return 0;
        }

        MailClient.Message GetMessage(Number messageIndex)
        {
            return _activeMailClient.Value.GetMessage(messageIndex - 1);
        }

        public Text MailMsgDate(Number messageIndex)
        {
            try
            {
                return GetMessage(messageIndex).Date;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }

        public Number MailFileSave(Number messageIndex, Number fileIndex, Text path, bool overwrite)
        {
            try
            {
                Action<ImapMailClient.Message.Attachment> save =
                    a =>
                    {
                        var targetFileName = System.IO.Path.GetFileName(path).Trim() != ""
                                                 ? (string)path
                                                 : System.IO.Path.Combine(path, a.Name);
                        if (overwrite || !System.IO.File.Exists(targetFileName))
                            System.IO.File.WriteAllBytes(targetFileName, a.Data);
                    };
                var m = GetMessage(messageIndex);
                if (fileIndex > 0)
                    save(m.Attachments[fileIndex - 1]);
                else
                    foreach (var attachment in m.Attachments)
                        save(attachment);
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return -1;
            }
            return 0;
        }

        public Text MailMsgFile(Number messageIndex, Number fileIndex)
        {
            try
            {
                return GetMessage(messageIndex).Attachments[fileIndex - 1].Name;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }

        public Text MailMsgFrom(Number messageIndex)
        {
            try
            {
                return GetMessage(messageIndex).From;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }

        public Text MailMsgCC(Number messageIndex)
        {
            try
            {
                return GetMessage(messageIndex).Cc;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }

        public Text MailMsgBCC(Number messageIndex)
        {

            try
            {
                return GetMessage(messageIndex).Bcc;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }

        public Text MailMsgHeader(Number messageIndex, Text headerKey)
        {
            try
            {
                return GetMessage(messageIndex).GetHeader(headerKey);
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }

        public Number MailMsgDel(Number messageIndex)
        {
            try
            {
                _activeMailClient.Value.DeleteMessage(messageIndex - 1);
                return 0;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return -1;
            }
        }

        public Text MailMsgTo(Number messageIndex)
        {
            try
            {
                return GetMessage(messageIndex).To;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }

        public Text MailMsgSubj(Number messageIndex)
        {
            try
            {
                return GetMessage(messageIndex).Subject;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }

        public Text MailMsgText(Number messageIndex)
        {
            try
            {
                return GetMessage(messageIndex).Text;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }

        public Number MailMsgFiles(Number messageIndex)
        {
            try
            {
                return GetMessage(messageIndex).Attachments.Length;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return 0;
            }
        }

        public Text MailMsgId(Number messageIndex)
        {
            try
            {
                return GetMessage(messageIndex).Id;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }

        #endregion

        #region bridges to com and stuff

        public object UDF(Text f, params object[] args)
        {
            return UDF(f, args, 0);
        }

        public object UDFF(Text f, params object[] args)
        {
            return UDF(f, args, System.Runtime.InteropServices.CallingConvention.FastCall);
        }

        public object UDFS(Text f, params object[] args)
        {
            return UDF(f, args, System.Runtime.InteropServices.CallingConvention.StdCall);
        }

        public static Func<string, object[], object> _as400SpecialFunctions;

        int MgExtendMessageBox(object[] args)
        {
            Func<int, string> getArg = i => Firefly.Box.Text.Cast(args[i]).TrimEnd().ToString();
            var mbIcon = MessageBoxIcon.None;
            switch (getArg(3))
            {
                case "1":
                    mbIcon = MessageBoxIcon.Exclamation;
                    break;
                case "2":
                    mbIcon = MessageBoxIcon.Information;
                    break;
                case "3":
                    mbIcon = MessageBoxIcon.Question;
                    break;
                case "4":
                    mbIcon = MessageBoxIcon.Stop;
                    break;
            }

            var mbButtons = MessageBoxButtons.OK;
            switch (getArg(2))
            {
                case "1":
                    mbButtons = MessageBoxButtons.AbortRetryIgnore;
                    break;
                case "2":
                    mbButtons = MessageBoxButtons.OKCancel;
                    break;
                case "3":
                    mbButtons = MessageBoxButtons.YesNo;
                    break;
                case "4":
                    mbButtons = MessageBoxButtons.YesNoCancel;
                    break;
            }

            var mbDefaultButton = MessageBoxDefaultButton.Button1;
            switch (getArg(4))
            {
                case "1":
                    mbDefaultButton = MessageBoxDefaultButton.Button1;
                    break;
                case "2":
                    mbDefaultButton = MessageBoxDefaultButton.Button2;
                    break;
                case "3":
                    mbDefaultButton = MessageBoxDefaultButton.Button3;
                    break;
                case "4":
                    mbDefaultButton = MessageBoxDefaultButton.Button3;
                    break;
            }

            return (int)Common.ShowMessageBox(getArg(1), mbIcon, getArg(0), mbButtons, mbDefaultButton);
        }

        object UDF(Text f, object[] args, System.Runtime.InteropServices.CallingConvention callingConvention)
        {
            if (InternalIsNull(f) || InternalIsNull(args))
                return null;
            f = ENV.PathDecoder.DecodePath(f);

            if (_as400SpecialFunctions != null &&
                (f.ToString().ToUpperInvariant().StartsWith("@MGEAC") || f.ToString().ToUpperInvariant().StartsWith("MGEAC")))
            {
                var arguments = new List<object>(args);
                if (f.ToString().StartsWith("@"))
                    arguments.RemoveAt(0);

                var func = GetFunctionNameFrom(f).ToUpperInvariant();
                if (func == "ISERIES")
                {
                    func = arguments[0].ToString();
                    arguments.RemoveAt(0);
                }
                return _as400SpecialFunctions(func, arguments.ToArray());

            }
            if (f.ToString().StartsWith("@"))
            {
                List<object> ar2 = new List<object>(args);
                ar2.RemoveAt(0);
                return CallDLL(f.Substring(1), args[0].ToString(), ar2.ToArray(), callingConvention);
            }
            try
            {
                var dllName = Path.GetFileName(GetDllNameFrom(f)).ToUpper(CultureInfo.InvariantCulture);
                if (dllName.StartsWith("GENDLL"))
                {
                    var func = GetFunctionNameFrom(f).ToUpper(CultureInfo.InvariantCulture);
                    if (func == "V2L") return Logical(args[0].ToString(), true);
                    if (func == "CALL_DLL")
                    {
                        var a = new object[(int)args[2]];
                        Array.Copy(args, 5, a, 0, a.Length);
                        var argString = (args[3] + "4").Replace('l', '4');
                        var r = CallDLL(string.Format("{0}.{1}", args[0], args[1]), argString, a,
                                        System.Runtime.InteropServices.CallingConvention.StdCall);
                        var nc = args[4] as NumberColumn;
                        if (nc != null) nc.Value = Number.Cast(r);
                        return null;
                    }
                }
                else if (dllName == "IOSUTIL")
                {
                    switch (GetFunctionNameFrom(f).ToUpper())
                    {
                        case "GETDESKTOPNAME":
                            {
                                if (args.Length > 0)
                                {
                                    var tc = args[0] as TextColumn;
                                    if (tc != null)
                                        tc.Value = System.Environment.MachineName;
                                }
                                return System.Environment.MachineName;
                            }
                        case "SETWINDOWTITLE":
                            if (args.Length > 1)
                            {
                                _getDllApplicationTitle = args[1].ToString();
                                GetDllSetApplicationTitle();
                            }
                            return null;

                    }
                    callingConvention = CallingConvention.Cdecl;
                }
                else if (dllName == "MGSTRUTL")
                {
                    if (GetFunctionNameFrom(f).ToUpper() == "MGFILUTL_CHECKDIREXISTENCE")
                    {
                        try
                        {
                            var path = args[0].ToString().TrimEnd();
                            path = System.IO.Path.GetDirectoryName(path);
                            Directory.CreateDirectory(path);
                            return 0;
                        }
                        catch (Exception ex)
                        {
                            ErrorLog.WriteToLogFile(ex, dllName);
                            return 123;
                        }
                    }
                }
                else if (dllName == "IOSUTILW")
                {
                    switch (GetFunctionNameFrom(f).ToUpper())
                    {
                        case "IS_CHILD":
                            {
                                Number n;
                                if (args.Length > 0 && Number.TryCast(args[0], out n))
                                {
                                    var frm = Control.FromChildHandle(new IntPtr(n)) as Form;
                                    if (frm != null)
                                    {
                                        var formToCheck = Form.LastClickedControl is ENV.UI.Button ? Form.LastClickedForm : null;
                                        formToCheck = formToCheck ?? GetTaskByGeneration(0).View;
                                        if (formToCheck != null)
                                        {
                                            var c = formToCheck as Control;
                                            while (c != frm && c != null)
                                                c = c.Parent;
                                            return c == frm ? 1 : 0;
                                        }
                                    }
                                }
                                return 0;
                            }
                    }
                    callingConvention = CallingConvention.Cdecl;
                }
                else if (dllName == "GET")
                {
                    callingConvention = CallingConvention.Cdecl;
                    switch (GetFunctionNameFrom(f).ToLower())
                    {
                        case "set_default_preview":
                            return null;
                        case "change_button":
                            ENV.UI.Button.EnableSpecialSubClassing = true;
                            return null;
                            break;
                        case "_set_button":
                            ENV.UI.Button.GetDllSetButton(args[0].ToString());
                            return null;
                        case "_set_icon_button":
                            ENV.UI.Button.LoadIconsIniFileForSpecialSubclassing(args[0].ToString());
                            return null;
                        case "set_class_cursor":
                            return null;
                        case "load_colors":
                            return null;
                        case "edit_color":
                            ENV.UI.TextBox.SetFocusedBackColor(args[1].ToString().Trim(), args[2].ToString().Trim());
                            return null;
                        case "local_date_time":
                            Text t;
                            if (!Types.Text.TryCast(args[0], out t) || Firefly.Box.Text.IsNullOrEmpty(t))
                                return null;
                            break;
                        case "input_attributes":
                            Common.RunOnRootMDI(
                                mdi =>
                                {
                                    var size = mdi.Size;
                                    var border = FormBorderStyle.FixedSingle;
                                    switch (args[5].ToString().ToLower())
                                    {
                                        case "t":
                                            mdi.FormBorderStyle = FormBorderStyle.Sizable;
                                            break;
                                        case "n":
                                            mdi.FormBorderStyle = FormBorderStyle.None;
                                            break;
                                    }
                                    mdi.FormBorderStyle = border;
                                    mdi.Size = size;
                                });
                            return null;

                        case "get_preview":
                            var nc = args[0] as NumberColumn;
                            if (nc != null) nc.Value = WinHWND(0);
                            return null;
                        case "set_icon":
                            try
                            {
                                var ico = new System.Drawing.Icon(args[0].ToString().Trim());
                                ENV.Common.DefaultIcon = ico;
                                Common.RunOnRootMDI(mdi => mdi.Icon = ico);
                                return null;

                            }
                            catch (Exception e)
                            {
                                ENV.ErrorLog.WriteToLogFile(e, "");
                            }
                            break;
                        case "get_handle":
                            var nc1 = args[4] as NumberColumn;
                            if (nc1 != null)
                            {
                                Number n;
                                if (Number.TryCast(args[3], out n))
                                {
                                    var frm = Control.FromChildHandle(new IntPtr(n)) as Form;
                                    if (frm != null)
                                    {
                                        foreach (var c in frm.Controls)
                                        {
                                            var tcb = c as Firefly.Box.UI.Advanced.TextControlBase;
                                            if (tcb != null && tcb.Text == args[1].ToString().TrimEnd() &&
                                                tcb.IsHandleCreated)
                                            {
                                                nc1.Value = (int)tcb.Handle;
                                                var btn = tcb as Firefly.Box.UI.Button;
                                                if (btn != null)
                                                {
                                                    foreach (System.Windows.Forms.Control c1 in btn.Controls)
                                                    {
                                                        if (c1 is System.Windows.Forms.Button && c1.IsHandleCreated)
                                                            nc1.Value = (int)c1.Handle;
                                                    }
                                                }
                                                return null;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "set_avi":
                            {
                                Number n;
                                if (Number.TryCast(args[1], out n))
                                {
                                    var p = Control.FromChildHandle(new IntPtr(n)) as Firefly.Box.UI.Button;
                                    var b = Control.FromChildHandle(new IntPtr(n)) as System.Windows.Forms.Button;
                                    if (b != null)
                                        p = b.Parent as Firefly.Box.UI.Button;
                                    if (p != null && p.Parent != null)
                                        AviPlayer.Show(p, args[0].ToString().TrimEnd());
                                }
                                return null;
                            }
                            break;
                        case "set_title":
                            _getDllApplicationTitle = args[0].ToString();
                            GetDllSetApplicationTitle();
                            return null;
                        case "title_clock":
                            var fmt = args[1].ToString();
                            if (fmt.StartsWith("dt:["))
                                fmt = fmt.Substring(4).Replace(']', ' ');
                            System.Threading.TimerCallback set = (x) =>
                            {
                                try
                                {

                                    _getDllApplicationTitleClock = DateTime.Now.ToString(fmt,
                                        ENV.LocalizationInfo.Current is ENV.HebrewLocalizationInfo ? CultureInfo.GetCultureInfo("he-IL") : CultureInfo.CurrentUICulture);
                                }
                                catch { }
                                GetDllSetApplicationTitle();
                            };

                            var tt = new System.Timers.Timer(1000);
                            tt.Elapsed += (o, e) => set(null);

                            tt.Start();
                            return null;
                        case "get_calendar_date":
                            {
                                var mc = GetMonthCalendar(args[0], args[1]);
                                if (mc != null)
                                    return "SELECT" + " : " + ((Date)mc.SelectionRange.Start).ToString("YYYYMMDD");
                            }
                            return "";
                        case "set_day_state":
                            {
                                var mc = GetMonthCalendar(args[0], args[1]);
                                if (mc != null)
                                {
                                    Date d;
                                    if (Firefly.Box.Date.TryCast(args[2], out d))
                                    {
                                        if (args[3].ToString().Trim() == "On")
                                            mc.AddBoldedDate(d.ToDateTime());
                                        else
                                            mc.RemoveBoldedDate(d.ToDateTime());
                                        mc.UpdateBoldedDates();
                                    }
                                }
                            }
                            return "";
                    }
                }
                else if (dllName == "RMDLL" && GetFunctionNameFrom(f).ToUpperInvariant() == "SETCLASS")
                {
                    ENV.UI.Button.EnableSpecialSubClassing = true;
                    ENV.UI.Button.SpecialSubClassingHyperLinkUnderline = true;
                    ENV.UI.Button.SpecialSubClassingHyperLinkMouseEnterForeColor = Color.Red;
                    ENV.UI.Button.SpecialSubClassingHyperLinkAlignment = ContentAlignment.MiddleLeft;
                    ENV.UI.Button.DontAllowBlank = true;
                    ENV.UI.Button.EnableSpecialSubClassing1 = true;
                }
                else if (dllName == "PHOENIX")
                    callingConvention = CallingConvention.StdCall;
                else if (dllName == "MGEXTEND")
                {
                    callingConvention = CallingConvention.StdCall;

                    if (GetFunctionNameFrom(f).ToLower() == "messagebox")
                    {
                        var x = MgExtendMessageBox(args);
                        var nc = args[5] as NumberColumn;
                        if (nc != null) nc.Value = x;
                        return null;
                    }
                }
                else if (dllName == "EXPORT")
                    callingConvention = CallingConvention.Cdecl;
                else if (dllName == "ALEXNPROC")
                    callingConvention = CallingConvention.Cdecl;
                else if (dllName == "MAGICTKN")
                {
                    if (GetFunctionNameFrom(f).ToLower() == "gettoken")
                    {
                        var separator = args[3].ToString().Trim();
                        if (separator == "")
                            separator = ",";

                        return StrToken(args[1].ToString(), StrTokenIdx(args[0].ToString(), args[2].ToString(), separator), separator);
                    }
                }
                else if (dllName == "MG_CRYPT")
                {
                    if (GetFunctionNameFrom(f).ToUpperInvariant() == "ENCRYPT")
                    {
                        Text key, text;
                        if (Firefly.Box.Text.TryCast(args[0], out key) && Firefly.Box.Text.TryCast(args[1], out text))
                            return Mg_crypt_encrypt(key.TrimEnd(' '), text.TrimEnd(' '));
                        return "";
                    }
                }
                else if (dllName == "MGCHART")
                {
                    if (GetFunctionNameFrom(f).ToLower() == "preparechart")
                    {
                        Text fileName, count;
                        if (Firefly.Box.Text.TryCast(args[0], out fileName) &&
                            Firefly.Box.Text.TryCast(args[1], out count))
                        {
                            using (StreamWriter writer = new StreamWriter(fileName))
                            {
                                writer.Write('\t');
                                writer.Write(new String('\0', 3));
                                writer.Write(":GRAFIKON");
                                writer.Write((char)2); //STX
                                writer.Write(new String('\0', 7));
                                for (int i = 1; i <= int.Parse(count); i++)
                                {
                                    writer.Write((char)6); //ACK
                                    writer.Write(new String('\0', 3));
                                    writer.Write(":NAME" + i);
                                }
                            }
                            return null;
                        }
                    }
                }

                System.IntPtr magicBindResult = (System.IntPtr)CallDllFunction(GetDllNameFrom(f), "MAGIC_BIND",
                                                                                typeof(System.IntPtr), new Type[] { },
                                                                                new object[] { },
                                                                                System.Runtime.InteropServices.
                                                                                    CallingConvention.StdCall, "", false);
                ushort functionCount = (ushort)System.Runtime.InteropServices.Marshal.ReadInt16(magicBindResult,
                                                                                                 2 +
                                                                                                 System.IntPtr.Size * 2);
                string functionName, arguments;
                System.IntPtr functions = System.Runtime.InteropServices.Marshal.ReadIntPtr(magicBindResult,
                                                                                            4 + System.IntPtr.Size * 2);

                string functionNameToLookFor = GetFunctionNameFrom(f);
                string resultArgumentsString = "";
                IntPtr functionAddress = IntPtr.Zero;

                var functionFound = false;

                using (var sw = new StringWriter())
                {
                    for (int j = 0; j < functionCount * 14; j += 14)
                    {
                        functionName = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(
                            System.Runtime.InteropServices.Marshal.ReadIntPtr(functions, j));
                        functionAddress = Marshal.ReadIntPtr(functions, j + 4);
                        arguments = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(
                            System.Runtime.InteropServices.Marshal.ReadIntPtr(functions, j + 10));
                        sw.WriteLine(functionName);
                        if (functionName.Equals(functionNameToLookFor, StringComparison.InvariantCultureIgnoreCase))
                        {
                            resultArgumentsString = arguments;
                            functionFound = true;
                            break;
                        }
                    }

                    if (!functionFound)
                        throw new System.ApplicationException(
                            "Cannot find function definition in MAGIC_BIND result. Available functions are:\r\n" +
                            sw.ToString());
                }
                if (resultArgumentsString.Length <= args.Length)
                    resultArgumentsString += "0";

                if (callingConvention == 0)
                    callingConvention = CallingConvention.Cdecl;

                var replaceReturnValueWithArgOfThisIndex = -1;
                if (dllName == "MGDATUTL" && GetFunctionNameFrom(f).ToUpper() == "DUTIMEDIFF")
                    replaceReturnValueWithArgOfThisIndex = 12;
                if (dllName == "MGWORD" && GetFunctionNameFrom(f).ToUpper() == "MGWORD_GETTEMPFILENAME")
                    replaceReturnValueWithArgOfThisIndex = 0;
                using (Profiler.StartContext("UDF " + f))
                {
                    return CallDLL(resultArgumentsString, args,
                               (returnType, argumentTypes, argumentsArr) =>
                               CallDllFunction(GetDllNameFrom(f) + functionAddress.ToString() + argumentTypes,
                                               (builder, typeName) =>
                                               {
                                                   var delegateBuilder = builder.DefineType(typeName,
                                                                                            System.Reflection.
                                                                                                TypeAttributes.Class |
                                                                                            System.Reflection.
                                                                                                TypeAttributes.
                                                                                                Public |
                                                                                            System.Reflection.
                                                                                                TypeAttributes.
                                                                                                Sealed |
                                                                                            System.Reflection.
                                                                                                TypeAttributes.
                                                                                                AnsiClass |
                                                                                            System.Reflection.
                                                                                                TypeAttributes.
                                                                                                AutoClass,
                                                                                            typeof(
                                                                                                MulticastDelegate));

                                                   var constructorBuilder =
                                                       delegateBuilder.DefineConstructor(
                                                           System.Reflection.MethodAttributes.RTSpecialName |
                                                           System.Reflection.MethodAttributes.HideBySig |
                                                           System.Reflection.MethodAttributes.Public,
                                                           System.Reflection.CallingConventions.Standard,
                                                           new System.Type[] { typeof(object), typeof(IntPtr) });

                                                   constructorBuilder.SetImplementationFlags(
                                                       System.Reflection.MethodImplAttributes.Runtime |
                                                       System.Reflection.MethodImplAttributes.Managed);

                                                   var methodBuilder = delegateBuilder.DefineMethod("Invoke",
                                                                                                    System.
                                                                                                        Reflection.
                                                                                                        MethodAttributes
                                                                                                        .Public |
                                                                                                    System.
                                                                                                        Reflection.
                                                                                                        MethodAttributes
                                                                                                        .HideBySig
                                                                                                    |
                                                                                                    System.
                                                                                                        Reflection.
                                                                                                        MethodAttributes
                                                                                                        .NewSlot |
                                                                                                    System.
                                                                                                        Reflection.
                                                                                                        MethodAttributes
                                                                                                        .Virtual,
                                                                                                    returnType,
                                                                                                    argumentTypes);

                                                   methodBuilder.SetImplementationFlags(
                                                       System.Reflection.MethodImplAttributes.Runtime |
                                                       System.Reflection.MethodImplAttributes.Managed);
                                                   delegateBuilder.SetCustomAttribute(
                                                       new CustomAttributeBuilder(
                                                           typeof(UnmanagedFunctionPointerAttribute).GetConstructor
                                                               (
                                                                   new[] { typeof(CallingConvention) }),
                                                           new object[] { callingConvention }));

                                                   return delegateBuilder.CreateType();
                                               },
                                               type =>
                                               Marshal.GetDelegateForFunctionPointer(functionAddress, type).
                                                   DynamicInvoke(argumentsArr)), replaceReturnValueWithArgOfThisIndex);
                }
            }
            catch (Exception e)
            {
                var message = string.Format(LocalizationInfo.Current.UserModuleNotLoaded, f);
                ENV.ErrorLog.WriteToLogFile(e, message);
                if (e.InnerException != null)
                    e = e.InnerException;
                if (e is System.BadImageFormatException)
                    throw;

                ENV.Message.ShowWarningInStatusBar(message);
                return null;
            }
        }

        static string _getDllApplicationTitle;
        static string _getDllApplicationTitleClock;
        static void GetDllSetApplicationTitle()
        {
            var title = string.IsNullOrEmpty(_getDllApplicationTitle) ? Common.ApplicationTitle : _getDllApplicationTitle;
            var t = string.IsNullOrEmpty(_getDllApplicationTitleClock) ? title : title.TrimEnd() + " " + _getDllApplicationTitleClock;
            Common.SpecificMDIText = t;
            Common.ShowSpecificMDIText = true;
            Common.RunOnRootMDI(form => form.Text = t);

        }

        static MonthCalendar GetMonthCalendar(object args0, object args1)
        {
            Number n;
            if (Number.TryCast(args0, out n))
            {
                var frm = Control.FromChildHandle(new IntPtr(n)) as Form;
                if (frm != null)
                {
                    foreach (var c in frm.Controls)
                    {
                        var b = c as ENV.UI.Button;
                        if (b != null)
                        {
                            var mc = b.GetMonthCalendar(args1.ToString());
                            if (mc != null)
                                return mc;
                        }
                    }
                }
            }
            return null;
        }

        string Mg_crypt_encrypt(string key, string text)
        {
            var keyBytes = LocalizationInfo.Current.InnerEncoding.GetBytes(key);
            var textBytes = LocalizationInfo.Current.InnerEncoding.GetBytes(text);
            var resultBytes = new byte[textBytes.Length];
            for (int j = 0; j < textBytes.Length; j++)
                resultBytes[j] = (byte)(keyBytes[j] ^ textBytes[j]);
            return new string(Encoding.Default.GetChars(resultBytes));
        }

        public object CallDLL(Text moduleName)
        {
            return CallDLL(moduleName, "", new object[0], 0);
        }

        public object CallDLL(Text moduleName, Text argumentTypes, params object[] args)
        {
            return CallDLL(moduleName, argumentTypes, args, 0);
        }

        public object CallDLLF(Text moduleName, Text argumentTypes, params object[] args)
        {
            return CallDLL(moduleName, argumentTypes, args, System.Runtime.InteropServices.CallingConvention.FastCall);
        }

        public object CallDLLS(Text moduleName, Text argumentTypes, params object[] args)
        {
            return CallDLL(moduleName, argumentTypes, args, System.Runtime.InteropServices.CallingConvention.StdCall);
        }
        public static bool ApplyEncodingToCallDll = true;
        object CallDLL(Text moduleName, Text argumentTypes, object[] args,
                       System.Runtime.InteropServices.CallingConvention callingConvention)
        {
            moduleName = PathDecoder.DecodePath(moduleName);
            if (callingConvention == 0)
                callingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl;
            try
            {
                var m = System.IO.Path.GetFileNameWithoutExtension(GetDllNameFrom(moduleName)).ToUpper(CultureInfo.InvariantCulture);
                switch (m)
                {
                    case "INTR32":
                    case "QAUWVED":
                    case "WBTRV32":
                    case "BIO":
                    case "DBGHELP":
                    case "BIOTC":
                    case "DZ_EZ32":
                        callingConvention = System.Runtime.InteropServices.CallingConvention.StdCall;
                        break;
                    case "USER32":
                        if (GetFunctionNameFrom(moduleName).ToUpperInvariant() == "GETSYSTEMMETRICS" && argumentTypes != "44")
                        {
                            argumentTypes = "44";
                            var x = args;
                            args = new[] { x.Length > 0 ? x[0] : 0 };
                        }
                        if (GetFunctionNameFrom(moduleName).ToUpperInvariant() == "FINDWINDOWA")
                        {
                            argumentTypes = "AA4";
                            if (args.Length < 2)
                            {
                                var x = args;
                                args = new object[2];
                                Array.Copy(x, args, x.Length);
                            }
                        }
                        if (GetFunctionNameFrom(moduleName).ToUpperInvariant() == "GETPARENT")
                        {
                            Number n;
                            if (Number.TryCast(args[0], out n))
                            {
                                var f = Control.FromHandle(new IntPtr(n)) as Firefly.Box.UI.Form;
                                if (f != null && f.TopLevel && f.Owner != null)
                                {
                                    Number result = null;
                                    Context.Current.InvokeUICommand(() => result = f.Owner.Handle.ToInt32());
                                    return result;
                                }
                            }
                        }
                        if (GetFunctionNameFrom(moduleName).ToUpperInvariant() == "MESSAGEBOXA" ||
                            GetFunctionNameFrom(moduleName).ToUpperInvariant() == "GETACTIVEWINDOW" ||
                            GetFunctionNameFrom(moduleName).ToUpperInvariant() == "GETWINDOWRECT")
                            Context.Current.Suspend(0);
                        if (GetFunctionNameFrom(moduleName).ToUpperInvariant() == "OEMTOCHARBUFFA")
                        {
                            var src = CastToText(args[0]);
                            var dest = args[1] as TextColumn;
                            var length = CastToNumber(args[2]);
                            if (dest != null)
                                dest.Value = OEM2ANSI(src.Substring(0, length));
                            return 1;
                        }
                        callingConvention = CallingConvention.Winapi;
                        break;
                    case "KERNEL32":
                        if (GetFunctionNameFrom(moduleName).ToUpperInvariant() == "CREATEDIRECTORYA" && argumentTypes == "A4" && args.Length == 1)
                        {
                            argumentTypes = "A44";
                            var x = args;
                            args = new object[2];
                            Array.Copy(x, args, x.Length);
                            args[1] = 0;
                        }
                        callingConvention = CallingConvention.Winapi;
                        break;
                    case "ADVAPI32":
                    case "SHELL32":
                        callingConvention = CallingConvention.Winapi;
                        break;
                    case "SHLWAPI":
                        if (GetFunctionNameFrom(moduleName).ToUpperInvariant().StartsWith("PATHFINDEXTENSION") && argumentTypes == "AA" && args.Length == 1)
                            return System.IO.Path.GetExtension(CastToText(args[0]));
                        callingConvention = CallingConvention.Winapi;
                        break;
                    case "MSVCRT":
                    case "GET":
                    case "MG_OCX":
                    case "MODVER":
                    case "RDEVLIST":
                        callingConvention = CallingConvention.Cdecl;
                        break;
                    case "WINMM":
                        if (GetFunctionNameFrom(moduleName).ToUpperInvariant().StartsWith("PLAYSOUND"))
                        {
                            argumentTypes = "A444";
                            Array.Resize(ref args, 3);
                            callingConvention = CallingConvention.StdCall;
                        }
                        break;
                    case "V2L32":
                        if (GetFunctionNameFrom(moduleName).ToUpperInvariant() == "VIS_2_LOG")
                        {
                            var t = args[0] as TextColumn;
                            var s = args[2] as TextColumn;
                            if (s != null && t != null)
                                t.Value = Visual(Flip(Trim(s)), true);
                            return null;
                        }
                        if (GetFunctionNameFrom(moduleName).ToUpperInvariant() == "LOG_2_VIS")
                        {
                            var t = args[0] as TextColumn;
                            var s = args[2] as TextColumn;
                            if (s != null && t != null)
                                t.Value = Visual(s, false);
                            return null;
                        }
                        break;
                    case "SPELLCHECKER":
                        if (string.Equals(GetFunctionNameFrom(moduleName), "CorrectStringSpellingSpecifyFile", StringComparison.InvariantCultureIgnoreCase) &&
                            string.Equals(argumentTypes, "LAALA", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var x = Marshal.StringToHGlobalAnsi(CastToText(args[1]));
                            try
                            {
                                return CallDLL(moduleName, "L4ALA", args[0], x.ToInt32(), args[2], args[3]);
                            }
                            finally
                            {
                                Marshal.FreeHGlobal(x);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");

            }

            if (IsNull(moduleName, argumentTypes) || InternalIsNull(args))
                return null;
            using (Profiler.StartContext("CallDll " + moduleName))
            {
                return CallDLL(argumentTypes, args,
                               (returnType, argumentTypesArr, arguments) =>
                               CallDllFunction(GetDllNameFrom(moduleName), GetFunctionNameFrom(moduleName), returnType,
                                               argumentTypesArr, arguments, callingConvention, argumentTypes, argumentTypes.Contains("U")), -1);
            }
        }

        object CallDLL(Text argumentTypes, object[] args, Func<Type, Type[], object[], object> actualDllCall, int replaceReturnValueWithArgOfThisIndex)
        {
            string profilerString = "";
            if (!Profiler.DoNotProfile())
            {
                foreach (var item in args)
                {
                    if (profilerString != "")
                        profilerString += ", ";
                    else
                        profilerString = "Args(" + argumentTypes + "): ";
                    profilerString += (item ?? "null").ToString().TrimEnd();
                }
            }
            using (Profiler.StartContext(profilerString))
            {
                var ptrs = new List<IntPtr>();
                try
                {
                    Func<object, Number> castNumber =
                        o =>
                        {
                            {
                                Number r;
                                if (Number.TryCast(o, out r))
                                    return r;
                            }
                            {
                                Time r = o as Time;
                                if (r != null)
                                    return ToNumber(r);
                            }
                            {
                                Date r = o as Date;
                                if (r != null)
                                    return ToNumber(r);
                            }
                            {
                                var r = o as DateColumn;
                                if (r != null && r.Value != null)
                                    return ToNumber(r.Value);
                            }
                            return Number.Parse(o.ToString());
                        };
                    var argTypes = argumentTypes.ToUpper(CultureInfo.InvariantCulture);

                    var argumentTypesForDll = new List<Type>();
                    var argumentsForDll = new List<object>();

                    var i = 0;
                    for (i = 0; i < argTypes.Length - 1; i++)
                    {
                        if (args.Length <= i) continue;
                        switch (argTypes[i])
                        {
                            case 'A':
                            case 'U':
                                argumentTypesForDll.Add(typeof(StringBuilder));
                                if (args[i] == null)
                                    argumentsForDll.Add(null);
                                else
                                {
                                    var s = args[i].ToString();
                                    if (argTypes[i] == 'U')
                                        argumentsForDll.Add(new System.Text.StringBuilder(s.TrimEnd(' '), 32000));
                                    else
                                    {
                                        var x = s.TrimEnd(' ');
                                        var ca = s.ToCharArray();
                                        if (x.Length < s.Length)
                                            ca[x.Length] = '\0';
                                        if (ApplyEncodingToCallDll)
                                            x = Encoding.Default.GetString(LocalizationInfo.Current.InnerEncoding.GetBytes(ca));

                                        argumentsForDll.Add(new StringBuilder(x, 32000));
                                    }
                                }
                                break;
                            case 'L':
                                int value = castNumber(args[i]);
                                argumentTypesForDll.Add(System.Type.GetType("System.Int32&"));
                                argumentsForDll.Add(value);
                                break;
                            case 'D':
                                double dVal1 = castNumber(args[i]);
                                argumentTypesForDll.Add(System.Type.GetType("System.Double&"));
                                argumentsForDll.Add(dVal1);
                                break;
                            case '1':
                            case '2':
                            case '4':
                                if (argTypes[i] == '1')
                                {
                                    Text t;
                                    if (Firefly.Box.Text.TryCast(args[i], out t) && t.ToString().Length > 0)
                                    {
                                        argumentTypesForDll.Add(typeof(char));
                                        argumentsForDll.Add(t.ToString()[0]);
                                        break;
                                    }
                                }
                                int value1;
                                var v = castNumber(args[i]);
                                try
                                {
                                    value1 = v;
                                }
                                catch (OverflowException)
                                {
                                    value1 = (int)(uint)v;
                                }
                                argumentTypesForDll.Add(typeof(int));
                                argumentsForDll.Add(value1);
                                break;
                            case 'F':
                            case '8':
                                double dVal = castNumber(args[i]);
                                argumentTypesForDll.Add(typeof(double));
                                argumentsForDll.Add(dVal);
                                break;
                            case 'V':
                            case 'T':
                                argumentTypesForDll.Add(typeof(IntPtr));
                                var bytes = args[i] as byte[];
                                if (bytes != null)
                                    bytes = GetBytesOfByteArrayColumn(bytes);
                                var bac = args[i] as ByteArrayColumn;
                                if (bac != null)
                                    bytes = GetBytesOfByteArrayColumn(bac.Value);
                                var tc = args[i] as TextColumn;
                                if (tc != null)
                                    bytes = ByteArrayColumn._byteParameterConverter.FromString(tc.Value);
                                if (bytes != null)
                                {
                                    Array.Resize(ref bytes, bytes.Length + 1000);
                                    var p = Marshal.AllocHGlobal(bytes.Length);
                                    Marshal.Copy(bytes, 0, p, bytes.Length);
                                    ptrs.Add(p);
                                    argumentsForDll.Add(p);
                                }
                                else
                                    argumentsForDll.Add(IntPtr.Zero);
                                break;
                            default:
                                throw new Exception("Unknown argument type");
                        }
                    }
                    var returnType = typeof(void);
                    Func<object, object> convertReturnValue = x => x;
                    switch (argTypes[i])
                    {
                        case 'A':
                            returnType = typeof(System.IntPtr);
                            convertReturnValue = x =>
                                new string(
                                ENV.LocalizationInfo.Current.InnerEncoding.GetChars(
                                    System.Text.Encoding.Default.GetBytes(
                                        System.Runtime.InteropServices.Marshal.PtrToStringAnsi((System.IntPtr)x).ToCharArray())));

                            ;
                            break;
                        case '4':
                        case '2':
                            returnType = typeof(System.Int32);
                            break;
                        case 'L':
                            returnType = typeof(System.IntPtr);
                            convertReturnValue = x => System.Runtime.InteropServices.Marshal.ReadInt32((System.IntPtr)x);
                            break;
                        case 'F':
                        case '8':
                        case 'E':
                            returnType = typeof(double);
                            break;
                        case 'D':
                            returnType = typeof(System.IntPtr);
                            convertReturnValue = x => BitConverter.ToDouble(BitConverter.GetBytes(Marshal.ReadInt64((System.IntPtr)x)), 0);
                            break;

                    }
                    var argumentsArray = argumentsForDll.ToArray();
                    object resultFromDll = null;
                    Context.Current.InvokeUICommand(
                        () =>
                        {
                            resultFromDll = actualDllCall(returnType, argumentTypesForDll.ToArray(), argumentsArray);
                        });

                    for (i = 0; i < argTypes.Length - 1; i++)
                    {
                        if (argumentsArray.Length <= i) continue;

                        switch (argTypes[i])
                        {
                            case 'A':
                                SetTextValueFromDll(args[i], argumentsArray[i]);
                                break;
                            case 'L':
                                SetNumberValueFromDll(args[i], castNumber(argumentsArray[i]));
                                break;
                            case 'D':
                                SetNumberValueFromDll(args[i], castNumber(argumentsArray[i]));
                                break;
                            case 'V':
                            case 'T':
                                var bac = args[i] as ByteArrayColumn;
                                if (bac != null && bac.Value != null)
                                {
                                    var oldBytes = bac.Value;
                                    var oldBytesWithoutBufPrefix = GetBytesOfByteArrayColumn(oldBytes);
                                    var bytes = new byte[oldBytesWithoutBufPrefix.Length];
                                    Marshal.Copy((IntPtr)argumentsArray[i], bytes, 0, bytes.Length);

                                    if (oldBytesWithoutBufPrefix.Length == oldBytes.Length)
                                        bac.Value = bytes;
                                    else
                                    {
                                        var bytesWithPrefix = new byte[BUF_PREFIX.Length + bytes.Length];
                                        Array.Copy(BUF_PREFIX, bytesWithPrefix, BUF_PREFIX.Length);
                                        Array.Copy(bytes, 0, bytesWithPrefix, BUF_PREFIX.Length, bytes.Length);
                                        bac.Value = bytesWithPrefix;
                                    }
                                }
                                else
                                {
                                    var tc = args[i] as Firefly.Box.Data.TextColumn;
                                    if (tc != null)
                                    {
                                        var returnedBytes = new byte[tc.FormatInfo.MaxLength];
                                        Marshal.Copy((IntPtr)argumentsArray[i], returnedBytes, 0, returnedBytes.Length);
                                        SetTextValueFromDll(tc, new StringBuilder(ByteArrayColumn.AnsiByteArrayToString(returnedBytes)));
                                    }
                                }
                                break;
                        }
                    }


                    resultFromDll = convertReturnValue(resultFromDll);
                    using (Profiler.StartContext("result from dll " + (resultFromDll ?? "").ToString()))
                    {
                    }

                    if (replaceReturnValueWithArgOfThisIndex > -1)
                    {
                        var tc = new TextColumn();
                        SetTextValueFromDll(tc, argumentsArray[replaceReturnValueWithArgOfThisIndex]);
                        resultFromDll = tc.Value;
                    }

                    if (args.Length < argTypes.Length)
                        return resultFromDll;

                    switch (argTypes[i])
                    {
                        case 'A':
                            SetTextValueFromDll(args[i], resultFromDll);
                            break;
                        case '4':
                        case '2':
                        case 'L':
                        case 'F':
                        case '8':
                        case 'D':
                        case 'E':
                            SetNumberValueFromDll(args[i], castNumber(resultFromDll));
                            break;
                    }
                    return null;
                }

                catch (Exception e)
                {
                    ENV.ErrorLog.WriteToLogFile(e, "");
                    if (e.InnerException != null)
                        e = e.InnerException;
                    if (e is System.BadImageFormatException)
                        throw;
                    return null;
                }
                finally
                {
                    foreach (var p in ptrs)
                        Marshal.FreeHGlobal(p);
                }
            }
        }

        void SetTextValueFromDll(object arg, object dllValue)
        {
            if (!(arg is Firefly.Box.Data.TextColumn)) return;
            var dllValueString = Types.Text.Cast(dllValue is string ? dllValue :
                new string(
                    ENV.LocalizationInfo.Current.InnerEncoding.GetChars(
                        System.Text.Encoding.Default.GetBytes(
                            ((System.Text.StringBuilder)dllValue).ToString().ToCharArray()))));
            if (dllValueString != ((Firefly.Box.Data.TextColumn)arg).Value)
                using (Profiler.StartContext("set: " + ((Firefly.Box.Data.TextColumn)arg).Caption + " = " + dllValueString))
                    ((Firefly.Box.Data.TextColumn)arg).Value = dllValueString;
        }

        void SetNumberValueFromDll(object arg, Number dllValue)
        {
            if (arg is Firefly.Box.Data.NumberColumn &&
                ((Firefly.Box.Data.NumberColumn)arg).Value != dllValue)
                using (Profiler.StartContext("set: " + ((Firefly.Box.Data.NumberColumn)arg).Caption + " = " + dllValue))
                    ((Firefly.Box.Data.NumberColumn)arg).Value = dllValue;
        }

        static string GetDllNameFrom(string UDFModuleName)
        {
            var x = UDFModuleName.Substring(0, UDFModuleName.LastIndexOf('.'));
            if (x.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                return x.Substring(0, x.Length - 4);
            return x;
        }

        static string GetFunctionNameFrom(string UDFModuleName)
        {
            return UDFModuleName.Substring(UDFModuleName.LastIndexOf('.') + 1).Trim();
        }

        static object CallDllFunction(string dllName, string functionName, Type returnType, Type[] argumentTypes,
                                      object[] arguments,
                                      System.Runtime.InteropServices.CallingConvention callingConvention,
                                      string uniqueFunctionSuffix, bool allowUnicode)
        {
            return CallDllFunction(dllName + functionName + uniqueFunctionSuffix,
                                   (builder, typeName) =>
                                   {
                                       System.Reflection.Emit.TypeBuilder typeBuilder = builder.DefineType(typeName);
                                       System.Reflection.Emit.MethodBuilder mb =
                                           typeBuilder.DefinePInvokeMethod(functionName, dllName,
                                                                           System.Reflection.MethodAttributes.Public |
                                                                           System.Reflection.MethodAttributes.Static |
                                                                           System.Reflection.MethodAttributes.
                                                                               PinvokeImpl |
                                                                           System.Reflection.MethodAttributes.
                                                                               HideBySig,
                                                                           System.Reflection.CallingConventions.
                                                                               Standard,
                                                                           returnType, argumentTypes,
                                                                           callingConvention,
                                                                           allowUnicode ?
                                                                           System.Runtime.InteropServices.CharSet.
                                                                           Auto : CharSet.Ansi);
                                       mb.SetImplementationFlags(System.Reflection.MethodImplAttributes.PreserveSig);
                                       return typeBuilder.CreateType();
                                   }, type => type.GetMethod(functionName).Invoke(null, arguments));
        }

        static object CallDllFunction(string dynamicTypeName,
                                      Func<System.Reflection.Emit.ModuleBuilder, string, Type> createDynamicType,
                                      Func<Type, object> invokeFunction)
        {
            try
            {

                if (_moduleBuilder == null)
                {
                    System.Reflection.Emit.AssemblyBuilder assemblyBuilder =
                        System.AppDomain.CurrentDomain.DefineDynamicAssembly(
                            new System.Reflection.AssemblyName("DynamicAssemblyForCallingDlls"),
                            System.Reflection.Emit.AssemblyBuilderAccess.Run);
                    _moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicAssemblyForCallingDlls");
                }
                System.Type type;
                if (!_dynamicallyDefinedTypes.TryGetValue(dynamicTypeName, out type))
                    lock (_dynamicallyDefinedTypes)
                    {
                        if (!_dynamicallyDefinedTypes.TryGetValue(dynamicTypeName, out type))
                        {
                            type = createDynamicType(_moduleBuilder, dynamicTypeName);
                            _dynamicallyDefinedTypes.Add(dynamicTypeName, type);
                        }
                    }
                return invokeFunction(type);
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "");
                throw;
            }
        }

        static System.Reflection.Emit.ModuleBuilder _moduleBuilder = null;

        static System.Collections.Generic.Dictionary<string, Type> _dynamicallyDefinedTypes =
            new Dictionary<string, Type>();

        public Number COMObjCreate(Number selecedColumnIndex)
        {
            if (selecedColumnIndex == null || selecedColumnIndex == 0) return null;
            ColumnBase column = GetColumnByIndex(selecedColumnIndex);
            if (column != null && IsDerivedFromGenericType(column.GetType(), typeof(Firefly.Box.Interop.ComColumn<>)))
            {
                try
                {
                    var InstanceCreation =
                        (bool)
                        column.GetType().GetProperty("CreateInstance").GetValue(column, new object[0]);
                    var instanceType =
                        (Type)column.GetType().GetProperty("InstanceType").GetValue(column, new object[0]);
                    if (InstanceCreation)
                        return -2;
                    if (column.Value != null)
                        return -3;
                    object obj = null;
                    Context.Current.InvokeUICommand(
                        () =>
                        {
                            try
                            {
                                obj = Activator.CreateInstance(instanceType, true);
                            }
                            catch (Exception e1)
                            {
                                ENV.ErrorLog.WriteToLogFile(e1, "");
                            }
                        });
                    column.Value = obj;
                    var ocxStateProperty = column.GetType().GetProperty("OcxState");
                    if (ocxStateProperty != null)
                    {
                        var ocxState = (AxHost.State)ocxStateProperty.GetValue(column, new object[0]);
                        if (ocxState != null)
                        {
                            var axHost = obj as AxHost;
                            if (axHost != null)
                                Context.Current.InvokeUICommand(() => axHost.OcxState = ocxState);
                        }
                    }

                    return 0;
                }
                catch (Exception e)
                {
                    ENV.ErrorLog.WriteToLogFile(e, "");
                }
            }
            return -1;
        }


        public Number COMObjRelease(Number selecedColumnIndex)
        {
            if (selecedColumnIndex == null || selecedColumnIndex == 0) return null;
            ColumnBase column = GetColumnByIndex(selecedColumnIndex);
            if (column != null && IsDerivedFromGenericType(column.GetType(), typeof(Firefly.Box.Interop.ComColumn<>)))
            {
                try
                {
                    var InstanceCreation =
                        (bool)
                        column.GetType().GetProperty("CreateInstance").GetValue(column, new object[0]);
                    if (InstanceCreation)
                        return -2;
                    var result = 0;
                    try
                    {
                        if (IsDerivedFromGenericType(column.GetType(), typeof(ENV.ComColumn<>)))
                            column.GetType().GetMethod("ResetToDefaultValue").Invoke(column, new object[0]);
                        else
                            column.GetType().GetMethod("Dispose").Invoke(column, new object[0]);
                    }
                    catch (Exception e1)
                    {
                        ENV.ErrorLog.WriteToLogFile(e1, "");
                        result = -3;
                    }
                    return result;
                }
                catch (Exception e)
                {
                    ENV.ErrorLog.WriteToLogFile(e, "");
                    return -3;

                }

            }

            return -1;
        }

        public Number COMHandleSet(Number comColumnIndex, Number handle)
        {
            if (comColumnIndex == null || comColumnIndex == 0) return null;
            var column = GetColumnByIndex(comColumnIndex);
            if (column != null && IsDerivedFromGenericType(column.GetType(), typeof(Firefly.Box.Interop.ComColumn<>)))
            {
                try
                {
                    object o = null;
                    Context.Current.InvokeUICommand(() => o = Marshal.GetObjectForIUnknown(new IntPtr(handle)));
                    column.Value = o;
                    return 0;
                }
                catch (Exception e)
                {
                    ENV.ErrorLog.WriteToLogFile(e, "");
                }

            }
            return -1;
        }



        public Bool DDExec(string serviceName, string topic, string itemName, Text command)
        {
            if (IsNull(serviceName, topic, itemName, command))
                return null;
            if (PerformHebrewV8OemParts)
            {
                topic = HebrewOemTextStorage.Decode(topic);
                itemName = HebrewOemTextStorage.Decode(itemName);
                command = HebrewOemTextStorage.Decode(command);
            }
            return DDE.Instance.Execute(serviceName, topic, itemName, command);
        }

        public Bool DDEBegin(Text serviceName, Text topic)
        {
            if (IsNull(serviceName, topic))
                return null;
            if (PerformHebrewV8OemParts)
                topic = HebrewOemTextStorage.Decode(topic);
            return DDE.Instance.Begin(serviceName, topic);
        }

        public Bool DDEEnd(Text serviceName, Text topic)
        {
            if (IsNull(serviceName, topic))
                return null;
            if (PerformHebrewV8OemParts)
                topic = HebrewOemTextStorage.Decode(topic);
            return DDE.Instance.End(serviceName, topic);
        }

        public Bool DDEPoke(Text serviceName, Text topic, Text itemName, Text data)
        {
            if (IsNull(serviceName, topic, itemName, data))
                return null;
            if (PerformHebrewV8OemParts)
            {
                topic = HebrewOemTextStorage.Decode(topic);
                itemName = HebrewOemTextStorage.Decode(itemName);
                data = HebrewOemTextStorage.Decode(data);
            }
            return DDE.Instance.Poke(serviceName, topic, itemName, data);
        }

        public Text DDEGet(Text serviceName, Text topic, Text itemName, Number length)
        {
            if (IsNull(serviceName, topic, itemName, length))
                return null;
            if (PerformHebrewV8OemParts)
            {
                topic = HebrewOemTextStorage.Decode(topic);
                itemName = HebrewOemTextStorage.Decode(itemName);

            }
            var s = DDE.Instance.Get(serviceName, topic, itemName, length);
            if (PerformHebrewV8OemParts)
                s = HebrewOemTextStorage.Encode(s);
            if (!string.IsNullOrEmpty(s))
                return s.Replace("\0", "");

            return null;
        }

        public int DDERR()
        {
            return DDE.Instance.GetLastError();
        }



        #endregion

        #region utils

        #region Constructors

        ApplicationControllerBase _application;

        public void SetApplication(ApplicationControllerBase application)
        {
            _application = application;

        }

        UpdateFieldDelegate _updateColumn;

        internal delegate void UpdateFieldDelegate(ColumnBase column, object value);

        UserMethods(UpdateFieldDelegate updateField)
        {
            _updateColumn = updateField;
            e = new EvaluateExpressions(this, () => CurrentContext.ActiveTasks, () => this._nullStrategy, () => _application != null ? _application._applicationRoles : null);
        }

        public UserMethods()
            : this((column, value) => column.Value = value)
        {
        }
        public UserMethods(ApplicationControllerBase app)
            : this((column, value) => column.Value = value)
        {
            e.AddObjectForFunctions((add) => app.addToEvaluateExpression(add));
        }

        internal UserMethods(BusinessProcessBase bp)
            : this(bp._businessProcess)
        {
            _applicationPrefix = () =>
            {
                if (bp._application == null || bp._application._applicationPrefix == null)
                    return __applicationPrefix;
                return bp._application._applicationPrefix;
            };
            addCustomFunctionsForEvalStr(bp);
        }

        internal UserMethods(BusinessProcess bp) :
            this((column, value) =>
            {
                if (bp.RowLocking == LockingStrategy.OnUserEdit && bp.Columns.Contains(column))
                    bp.LockCurrentRow();
                column.Value = value;
            })
        {

            _currentTask = bp;

        }

        internal UserMethods(AbstractUIController uic)
            : this(uic._uiController)
        {
            _applicationPrefix = () =>
            {
                if (uic._application == null || uic._application._applicationPrefix == null)
                    return __applicationPrefix;
                return uic._application._applicationPrefix;
            };
            addCustomFunctionsForEvalStr(uic);
        }

        private void addCustomFunctionsForEvalStr(ControllerBase uic)
        {
            e.AddObjectForFunctions((add) =>
            {
                add(uic);
                var t = uic.GetType();
                int i = 2;
                var tasks = Context.Current.ActiveTasks;
                while (t.DeclaringType != null)
                {
                    var stop = true;
                    if (i > tasks.Count)
                        return;
                    ControllerBase.SendInstanceBasedOnTaskAndCallStack(tasks[tasks.Count - i++], parent =>
                    {
                        if (parent.GetType() == t.DeclaringType)
                        {
                            add(parent);
                            t = t.DeclaringType;
                            stop = false;
                        }

                    });
                    if (stop)
                        return;
                }

            });

            e.AddObjectForFunctions((add) =>
            {
                if (_application != null) _application.addToEvaluateExpression(add);
            });
        }

        internal UserMethods(UIController uic) :
            this(delegate (ColumnBase column, object value)
            {
                Action setValue = () =>
                {
                    var r = column as IENVColumn;
                    if (r != null)
                    {
                        var x = r._internalReadOnly;
                        r._internalReadOnly = false;
                        try
                        {
                            column.Value = value;
                        }
                        finally
                        {
                            r._internalReadOnly = x;
                        }
                    }
                    else
                        column.Value = value;

                };
                if (uic.Columns.Contains(column))
                {
                    var x = column.OnChangeMarkRowAsChanged;
                    column.OnChangeMarkRowAsChanged = false;
                    try
                    {
                        if (uic.RowLocking == LockingStrategy.OnUserEdit)
                            uic.LockCurrentRow();
                        setValue();
                    }
                    finally
                    {
                        column.OnChangeMarkRowAsChanged = x;
                    }

                }
                else
                    setValue();
            })
        {
            _currentTask = uic;
        }

        #endregion

        static bool IsNull(params object[] args)
        {
            foreach (object o in args)
            {
                if (o == null)
                    return true;
            }
            return false;
        }

        #endregion

        #region multi mark

        static Control FindGridOrTreeView(Form form)
        {
            Control c = null;
            form.ForEachControlInTabOrder(ctrl => c = c ?? (Control)(ctrl as Firefly.Box.UI.Grid) ?? ctrl as Firefly.Box.UI.TreeView);
            return c;
        }

        public Bool MMClear()
        {
            var t = GetTaskByGeneration(0) as UIController;
            if (t != null)
            {
                var c = FindGridOrTreeView(t.View);
                {
                    var g = c as Firefly.Box.UI.Grid;
                    if (g != null)
                    {
                        g.ClearMultiSelect();
                        return true;
                    }
                }
                {
                    var g = c as Firefly.Box.UI.TreeView;
                    if (g != null)
                    {
                        g.ClearMultiSelect();
                        return true;
                    }
                }
            }
            return false;

        }

        public Number MMCount(Number generation)
        {
            var t = GetTaskByGeneration(generation) as UIController;
            if (t != null && t.View != null)
            {
                var c = FindGridOrTreeView(t.View);
                {
                    var g = c as Firefly.Box.UI.Grid;
                    if (g != null)
                        return g.MultiSelectRowsCount;
                }
                {
                    var g = c as Firefly.Box.UI.TreeView;
                    if (g != null)
                        return g.MultiSelectRowsCount;
                }
            }
            return 0;
        }

        public Number MMCurr(Number generation)
        {
            var t = GetTaskByGeneration(generation) as UIController;
            if (t != null)
            {
                var c = FindGridOrTreeView(t.View);
                {
                    var g = c as Firefly.Box.UI.Grid;
                    if (g != null)
                        return g.MultiSelectIterationIndex + 1;
                }
                {
                    var g = c as Firefly.Box.UI.TreeView;
                    if (g != null)
                        return g.MultiSelectIterationIndex + 1;
                }
            }
            return 0;
        }

        public Bool MMStop()
        {
            var t = GetTaskByGeneration(0) as UIController;
            if (t != null)
            {
                var c = FindGridOrTreeView(t.View);
                {
                    var g = c as Firefly.Box.UI.Grid;
                    if (g != null)
                        g.AbortMultiSelectIteration();
                }
                {
                    var g = c as Firefly.Box.UI.TreeView;
                    if (g != null)
                        g.AbortMultiSelectIteration();
                }
                return true;
            }
            return false;
        }

        #endregion


        public Text GetLang()
        {
            return Languages.ContextCurrentLanguage;
        }

        public Bool SetLang(Text language)
        {
            if (!Object.ReferenceEquals(null, language))
            {
                Languages.ContextCurrentLanguage = language.TrimEnd();
                if (System.Web.HttpContext.Current == null)
                    Languages.SetSharedLanguage(language.TrimEnd());
            }
            return true;
        }



        public Text OSEnvGet(Text name)
        {
            if (IsNull(name))
                return null;
            try
            {
                return Environment.GetEnvironmentVariable(name);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex);
                return null;
            }

        }

        public Bool OSEnvSet(Text name, Text value)
        {
            if (name == null)
                return null;
            try
            {
                Environment.SetEnvironmentVariable(name, value);
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex);
                return false;
            }
        }

        static readonly Number THISConstant = 500000;
        public Number THIS()
        {
            return THISConstant;
        }
        internal static ContextStatic<PrinterWriter> CurrentIO = new ContextStatic<PrinterWriter>(() => null);

        public Number IOCurr()
        {
            if (CurrentIO.Value == null)
                return 0;
            Number result = -1;
            var at = CurrentContext.ActiveTasks;
            for (int i = at.Count - 1; i >= 0; i--)
            {
                ControllerBase.SendInstanceBasedOnTaskAndCallStack(at[i],
                    z =>
                    {
                        result = z.Streams.IndexOf(CurrentIO.Value);
                    });
                if (result >= 0)
                    return result + 1;
            }
            return 0;
        }
        [NotYetImplemented]
        public Text EncryptionError(params object[] unknownArgs)
        {
            return Types.Text.Empty;
        }

        [NotYetImplemented]
        public object CallJS(Text unknown)
        {
            return null;
        }
        [NotYetImplemented]
        public Bool SNMPNotify(Text message, Number severity)
        {
            return false;
        }

        static int GetContextId(Context context)
        {
            return context.GetHashCode();
        }

        internal static string GetContextName(Context context)
        {
            return (Text)context[CONTEXT_NAME_KEY] ?? GetContextId(context).ToString();
        }

        public int CtxGetId(Text contextName)
        {
            contextName = contextName.TrimEnd();
            if (string.IsNullOrEmpty(contextName)) return GetContextId(CurrentContext);
            foreach (var c in Context.ActiveContexts)
            {
                if (GetContextName(c) == contextName)
                    return GetContextId(c);
            }
            return 0;
        }

        internal const string CONTEXT_NAME_KEY = "CONTEXT_NAME";

        public bool CtxSetName(Text name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            name = name.TrimEnd();
            foreach (var c in Context.ActiveContexts)
            {
                if (GetContextName(c) == name)
                    return false;
            }
            CurrentContext[CONTEXT_NAME_KEY] = name;
            return true;
        }

        public string CtxGetName()
        {
            return GetContextName(CurrentContext);
        }

        public Context GetContextByName(string contextName)
        {
            if (string.IsNullOrEmpty(contextName)) return null;
            contextName = contextName.TrimEnd();
            foreach (var c in Context.ActiveContexts)
            {
                if (GetContextName(c) == contextName)
                    return c;
            }
            return null;
        }

        public Text[] CtxGetAllNames()
        {
            var l = new List<Text>();
            foreach (var c in Context.ActiveContexts)
                l.Add(GetContextName(c));
            return l.ToArray();
        }


        public bool CtxKill(Number contextId, Bool killBusy)
        {
            var result = false;
            DoOnContext(contextId,
                context =>
                {
                    if (context == Context.Current) return;
                    context.Thread.Abort();
                    result = true;
                });
            return result;
        }

        public Number CtxLstUse(Number contextId)
        {
            var result = -1;
            DoOnContext(contextId, context => result = 0);
            return result;
        }

        public Text CtxProg(Number contextId)
        {
            var result = "";
            DoOnContext(contextId,
                context =>
                {
                    if (context == CurrentContext)
                        result = Prog();
                    else
                    {
                        foreach (var t in context.ActiveTasks)
                        {
                            if (t is ModuleController)
                                continue;
                            result = ENV.UserMethods.GetControllerName(t);
                            break;
                        }

                    }
                });
            return result;
        }

        public Number CtxSize(Number contextId)
        {
            return 0;
        }



        public Number DbViewSize(Number generation)
        {
            var uic = GetTaskByGeneration(generation) as UIController;
            if (uic != null)
                if (uic.PreloadData)
                    return uic.CachedRowsInfo.Count;
            return 0;
        }

        public object FileInfo(Text fileName, Number infoType)
        {
            if (InternalIsNull(fileName) || InternalIsNull(infoType))
                return null;
            fileName = PathDecoder.DecodePath(fileName.TrimEnd());
            if (fileName == "")
                return null;
            try
            {
                var fi = new FileInfo(fileName);
                if ((int)fi.Attributes == -1)
                {
                    if (infoType < 5)
                        return "";
                    return null;
                }
                switch ((int)infoType)
                {
                    case 1:
                        return fi.Name;
                    case 2:
                        return fi.DirectoryName;
                    case 3:
                        return fi.FullName;
                    case 4:
                        var r = fi.Attributes.ToString().Replace("|", ",").Replace(" ", "").ToUpper();
                        if (!r.Contains("DIRECTORY"))
                        {
                            if (r == "")
                            {
                                r = "FILE";
                            }
                            else
                                r = "FILE," + r;
                        }
                        return r;
                    case 5:
                        return fi.Length;
                    case 6:
                        return ToDate(fi.CreationTime);
                    case 7:
                        return ToTime(fi.CreationTime);
                    case 8:
                        return ToDate(fi.LastWriteTime);
                    case 9:
                        return ToTime(fi.LastWriteTime);
                    case 10:
                        return ToDate(fi.LastAccessTime);
                    case 11:
                        return ToTime(fi.LastAccessTime);

                    default:
                        return null;

                }
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return null;
            }

        }

        public object ClientFileInfo(Text fileName, Number infoType)
        {
            return FileInfo(fileName, infoType);
        }

        public Number ClientFileToServer(Text sourceFileName, Text targetFileName)
        {
            Bool Success;
            Success = IOCopy(sourceFileName, targetFileName);
            if (Success)
                return 0;
            return 1;
        }

        public Text ServerFileToClient(Text sourceFileName)
        {
            String uniqueFileName = Path.GetTempFileName() + "_" + System.IO.Path.GetFileName(sourceFileName);
            Bool Success = IOCopy(sourceFileName, uniqueFileName);
            if (Success)
                return uniqueFileName;
            return "";
        }



        public Text ClientFileOpenDlg(Text title, Text initailPath, Text filter, Bool checkFileExists, Bool multiSelect)
        {
            var ofd = new OpenFileDialog
            {
                Title = title,
                FileName = Path.GetFileName(initailPath),
                CheckFileExists = checkFileExists,
                Multiselect = multiSelect,
                InitialDirectory = Path.GetDirectoryName(initailPath),
            };

            if (filter != Firefly.Box.Text.Empty)
            {
                if (filter.IndexOf("|") < 0)
                    filter = "|" + filter.Trim();

                ofd.Filter = filter.Trim();
            }

            if (ofd.ShowDialog() == DialogResult.OK)
                return string.Join("|", ofd.FileNames);
            return "";
        }

        public Text ClientFileSaveDlg(Text title, Text initailPath, Text filter, Text defaultExtension, Bool overwritePrompt)
        {
            var sfd = new SaveFileDialog
            {
                Title = title,
                FileName = initailPath,
                Filter = filter,
                DefaultExt = defaultExtension,
                OverwritePrompt = overwritePrompt
            };
            if (sfd.ShowDialog() == DialogResult.OK)
                return sfd.FileName;
            return "";
        }


        [NotYetImplemented]
        public Bool RqRtTrmEx(params object[] unknown)
        {
            return false;
        }
        [NotYetImplemented]
        public Bool RqRtResume(params object[] unknown)
        {
            return false;
        }


        public Number SubformExecMode(Number generation)
        {
            if (ReferenceEquals(generation, null))
                return null;
            Number result = -1;
            var x = GetTaskByGeneration(generation);
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(x,
                                                                                       y =>
                                                                                       {
                                                                                           var uic =
                                                                                               y as
                                                                                               AbstractUIController;
                                                                                           if (uic != null)
                                                                                               result =
                                                                                                   uic.
                                                                                                       _subformExecMode;
                                                                                       });
            return result;
        }

        public object VariantGet(byte[] val, Text type)
        {
            if (val == null)
                return null;

            try
            {
                using (var ms = new MemoryStream(val))
                {
                    var result = new BinaryFormatter().Deserialize(ms);
                    if (type == "A")
                    {
                        if (result is DateTime)
                        {
                            var dt = (DateTime)result;
                            return dt.ToShortDateString();
                        }

                        return ByteArrayColumn.ToAnsiByteArray(result.ToString());
                    }

                    if (type == "T")
                    {
                        if (result is DateTime)
                            return Firefly.Box.Time.FromDateTime((DateTime)result);
                    }

                    return result;
                }
            }
            catch (Exception e)
            {
                switch (type)
                {
                    case "B":
                        return val;
                    case "A":
                        if (val.Length > 2 && val[1] == 0)
                        {
                            return ByteArrayColumn.ToAnsiByteArray(ByteArrayColumn.UnicodeWithoutGaps.GetString(val));
                        }

                        ENV.ErrorLog.WriteToLogFile(e, "");
                        return val;
                    case "U":
                        if (val.Length > 2 && val[1] == 0)
                        {
                            return ByteArrayColumn.UnicodeWithoutGaps.GetString(val);
                        }

                        ENV.ErrorLog.WriteToLogFile(e, "");
                        return val;
                    default:
                        throw new NotImplementedException("VariantGet with type " + type);
                }
            }
        }

        public object VariantGet(ByteArrayColumn val, Text type)
        {
            return VariantGet(val.Value, type);
        }

        public object VariantGet(Text val, Text type)
        {
            if (type == "A")
                return ByteArrayColumn._byteParameterConverter.FromString(val);
            else
                throw new NotImplementedException("VariantGet with type " + type);

        }

        [ThreadStatic]
        static Text[] _xmlValidationErrors = null;

        public Bool XMLValidate(byte[] xmlData)
        {
            return XMLValidate(xmlData, null);
        }
        public Bool XMLValidate(Text xmlData, Text xmlSchemaLocation)
        {
            return XMLValidateInternal((s, r) =>
            {

                using (var ms = new StringReader(RTrim((xmlData))))
                {
                    r(XmlReader.Create(ms, s));
                }
            }, xmlSchemaLocation);
        }
        public Bool XMLValidate(ByteArrayColumn xmlData, Text xmlSchemaLocation)
        {
            if (xmlData == null)
                return null;
            return XMLValidate(xmlData.Value, xmlSchemaLocation);
        }
        public Bool XMLValidate(byte[] xmlData, Text xmlSchemaLocation)
        {
            return XMLValidateInternal((s, r) =>
            {

                using (var ms = new MemoryStream(xmlData))
                {
                    r(XmlReader.Create(ms, s));
                }
            }, xmlSchemaLocation);
        }

        private static Bool XMLValidateInternal(Action<XmlReaderSettings, Action<XmlReader>> xmlReader, Text xmlSchemaLocation)
        {
            var errors = new List<Text>();
            try
            {

                var settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                if (xmlSchemaLocation != null)
                {
                    var s = xmlSchemaLocation.ToString().TrimEnd().Split(' ');
                    try
                    {
                        settings.Schemas.Add(s.Length > 1 ? s[0] : "",
                            PathDecoder.DecodePath(s.Length > 1 ? s[1] : s[0]));
                    }
                    catch (System.Xml.Schema.XmlSchemaException ex)
                    {
                        errors.Add("Error: schema document " + ex.Message);
                        var start = ex.Message.IndexOf("'http", StringComparison.CurrentCultureIgnoreCase);
                        var end = ex.Message.IndexOf("\'", start + 1, StringComparison.CurrentCultureIgnoreCase);
                        var schema = ex.Message.Substring(start + 1, end - start - 1);
                        try { settings.Schemas.Add(schema, s[0]); }
                        catch { }
                    }
                    catch (Exception ex)
                    {
                        errors.Add("Error: schema document " + ex.Message);
                    }
                }
                settings.ValidationEventHandler += (sender, args) =>
                {
                    if (args.Severity == XmlSeverityType.Error)
                        errors.Add("Error: " + args.Message);
                };

                // Create the XmlReader object.

                xmlReader(settings, Reader =>
                {

                    // Parse the file. 
                    try
                    {
                        while (Reader.Read()) ;
                    }
                    catch (XmlException ex)
                    {
                        errors.Add("Fatal: " + ex.Message);
                    }
                });


            }
            catch (Exception e)
            {

                errors.Add(e.Message);
            }
            _xmlValidationErrors = errors.ToArray();
            return errors.Count == 0;
        }

        public Text[] XMLValidationError()
        {
            return _xmlValidationErrors ?? new Text[0];
        }



        public Bool IsFirstRecordCycle(Number generation)
        {
            var t = GetTaskByGeneration(generation);
            var result = false;
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(t, c =>
            {
                {
                    var bpb = c as BusinessProcessBase;
                    if (bpb != null)
                    {
                        if (MainLevel(generation) == "TS")
                            result = bpb.Counter == 0;
                        else if (bpb._leaveRowHappened)
                            result = false;
                        else result = bpb.Counter <= 1;
                    }

                }
                {
                    var uic = c as AbstractUIController;
                    if (uic != null)
                    {
                        if (MainLevel(generation) == "TS")
                            result = uic._enterRow == 0;
                        else result = uic._enterRow <= 1;
                    }
                }

            });

            return result;
        }

        static byte[] BUF_PREFIX = new byte[] { 0x42, 0x4d, 0, 0, 0, 0, 0x40, 0x23, 0, 0, 0, 0 };

        void InsertValueIntoByteArrayColumn<T>(ByteArrayColumn byteArrayColumn, T value,
                                               IColumnStorageSrategy<T> storage, int startIndex, bool reverseBytes = false)
        {
            var binaryValueSaver = new BinaryValueSaver();
            storage.SaveTo(value, binaryValueSaver);

            if (reverseBytes)
                Array.Reverse(binaryValueSaver.ValueByteArray);

            var sizeRequired = BUF_PREFIX.Length + startIndex + binaryValueSaver.ValueByteArray.Length - 1;

            var fullSize = sizeRequired;
            if (byteArrayColumn.Value != null && byteArrayColumn.Value.Length > fullSize)
                fullSize = byteArrayColumn.Value.Length;

            var bytes = new byte[fullSize];
            if (byteArrayColumn.Value != null)
                Array.Copy(byteArrayColumn.Value, bytes, byteArrayColumn.Value.Length);
            Array.Copy(BUF_PREFIX, bytes, BUF_PREFIX.Length);
            Array.Copy(binaryValueSaver.ValueByteArray, 0, bytes, BUF_PREFIX.Length + startIndex - 1,
                       binaryValueSaver.ValueByteArray.Length);
            byteArrayColumn.Value = bytes;
        }

        void InsertValueIntoByteArrayColumnAsPointer<T>(ByteArrayColumn byteArrayColumn, T value,
            IColumnStorageSrategy<T> storage, int startIndex)
        {
            var binaryValueSaver = new BinaryValueSaver();
            storage.SaveTo(value, binaryValueSaver);
            var bytes = binaryValueSaver.ValueByteArray;
            var ptr = Marshal.AllocHGlobal(bytes.Length);
            InsertValueIntoByteArrayColumn(byteArrayColumn, ptr.ToInt32(), new IntNumberStorage(), startIndex);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            var t = GetTaskByGeneration(0) as ControllerBase;
            if (t != null)
                t.Disposables.Add(new DisposablePointer(ptr));
        }

        byte[] GetBytesOfByteArrayColumn(byte[] byteArrayColumnValue)
        {
            if (byteArrayColumnValue == null) return null;
            var bytes = byteArrayColumnValue;

            if (bytes.Length < BUF_PREFIX.Length) return bytes;

            if (bytes[0] == BUF_PREFIX[0] &&
                bytes[1] == BUF_PREFIX[1] &&
                bytes[6] == BUF_PREFIX[6] &&
                bytes[7] == BUF_PREFIX[7] &&
                bytes[8] == BUF_PREFIX[8] &&
                bytes[9] == BUF_PREFIX[9] &&
                bytes[10] == BUF_PREFIX[10] &&
                bytes[11] == BUF_PREFIX[11])
            {
                var result = new byte[bytes.Length - BUF_PREFIX.Length];
                Array.Copy(byteArrayColumnValue, BUF_PREFIX.Length, result, 0, result.Length);
                return result;
            }
            return bytes;
        }

        IColumnStorageSrategy<Number> GetNumberStorage(int storageType, int length)
        {
            if (storageType == 1)
            {
                if (length == 4) return new IntNumberStorage();
                if (length == 2) return new ShortNumberStorage();
                if (length == 1) return new SByteNumberStorage();
            }
            if (storageType == 2)
            {
                if (length == 4) return new UIntNumberStorage();
                if (length == 2) return new UShortNumberStorage();
                if (length == 1) return new ByteNumberStorage();
            }
            if (storageType == 3)
            {
                if (length == 4) return new SingleDecimalNumberStorage();
                if (length == 8) return new DoubleNumberStorage();
            }
            if (storageType == 4)
                return new FloatMSBasicNumberStorage(length);
            if (storageType == 5)
            {
                return new FloatDecimalNumberStorage(length);
            }
            throw new ArgumentException(
                string.Format("Storage type '{0}' or length '{1}' not supported", storageType, length), "storageType");
        }

        IColumnStorageSrategy<Date> GetDateStorage(int storageType)
        {
            if (storageType == 2) return new Number1901DateStorage();
            if (storageType == 3) return new BtrieveDateStorage();
            return new NumberDateStorage();
        }

        public Bool BufSetNum(Number byteArrayColumnIndex, int startIndex, Number value, int storageType, int length)
        {
            var bac = GetColumnByIndex(byteArrayColumnIndex) as ByteArrayColumn;
            if (bac == null)
                return false;
            try
            {
                InsertValueIntoByteArrayColumn(bac, value, GetNumberStorage(storageType, length), startIndex, _reverseBytesInBufGetSetOfNumber);
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex);
                return false;
            }
        }


        class ByteArrayStorage : IColumnStorageSrategy<byte[]>
        {
            public byte[] LoadFrom(IValueLoader loader)
            {
                if (loader.IsNull())
                    return null;
                return loader.GetByteArray();
            }

            public void SaveTo(byte[] value, IValueSaver saver)
            {
                saver.SaveByteArray(value);
            }
        }
        [NotYetImplemented]
        public Bool BufSetVariant(Number byteArrayColumnIndex, int startIndex, object value, bool storeAsPointer)
        {
            return false;
        }
        [NotYetImplemented]
        public Text BufGetVariant(Number byteArrayColumnIndex, int startIndex, bool storeAsPointer)
        {
            return null;
        }
        public Bool BufSetBlob(Number byteArrayColumnIndex, int startIndex, ByteArrayColumn value, int storageType, bool storeAsPointer)
        {
            if (value == null)
                return false;
            return BufSetBlob(byteArrayColumnIndex, startIndex, value.Value, storageType, storeAsPointer);
        }
        public Bool BufSetBlob(Number byteArrayColumnIndex, int startIndex, byte[] value, int storageType, bool storeAsPointer)
        {
            var bac = GetColumnByIndex(byteArrayColumnIndex) as ByteArrayColumn;
            if (bac == null)
                return false;
            if (storageType == 1)
            {
                var ba = new byte[value.Length + 4];
                Array.Copy(value, 0, ba, 4, value.Length);
                Array.Copy(BitConverter.GetBytes(value.Length + 4), ba, 4);
                value = ba;
            }
            if (storeAsPointer)
                InsertValueIntoByteArrayColumnAsPointer(bac, value, new ByteArrayStorage(), startIndex);
            else
                InsertValueIntoByteArrayColumn(bac, value, new ByteArrayStorage(), startIndex);
            return true;
        }

        public Bool BufSetBlob(Number byteArrayColumnIndex, int startIndex, Text value, int storageType, bool storeAsPointer)
        {
            return BufSetBlob(byteArrayColumnIndex, startIndex,
                ByteArrayColumn._byteParameterConverter.FromString(value), storageType, storeAsPointer);
        }

        class DisposablePointer : IDisposable
        {
            IntPtr _ptr;

            public DisposablePointer(IntPtr ptr)
            {
                _ptr = ptr;
            }

            public void Dispose()
            {
                Marshal.FreeHGlobal(_ptr);
            }
        }


        public Time BufGetTime(Number byteArrayColumnIndex, int startIndex, int storageType)
        {
            var vl = GetValueLoaderForByteArrayColumn(byteArrayColumnIndex, startIndex, 4, 1);
            return vl == null ? null : storageType == 1 ? new TotalSecondsStorage().LoadFrom(vl) : new HMSHTimeStorage().LoadFrom(vl);
        }

        public Bool BufSetTime(Number byteArrayColumnIndex, int startIndex, Time value, int storageType)
        {
            var bac = GetColumnByIndex(byteArrayColumnIndex) as ByteArrayColumn;
            if (bac == null)
                return false;
            InsertValueIntoByteArrayColumn(bac, value, storageType == 1 ? (IColumnStorageSrategy<Time>)new TotalSecondsStorage() : new HMSHTimeStorage(), startIndex);
            return true;
        }

        class TotalSecondsStorage : IColumnStorageSrategy<Time>
        {
            public Time LoadFrom(IValueLoader loader)
            {
                if (loader.IsNull())
                    return null;
                return new Time().AddSeconds(loader.GetNumber());
            }

            public void SaveTo(Time value, IValueSaver saver)
            {
                if (value == null)
                    saver.SaveNull();
                else
                    saver.SaveInt((int)value.TotalSeconds);
            }
        }

        public Bool BufGetBit(Number byteArrayColumnIndex, int startIndex, int position)
        {
            if (position < 1 || position > 8) return false;
            var bac = GetColumnByIndex(byteArrayColumnIndex) as ByteArrayColumn;
            if (bac == null) return false;
            var bytes = GetBytesOfByteArrayColumn(bac.Value);
            if (bytes == null || bytes.Length < startIndex) return false;
            return (bytes[startIndex - 1] & (1 << 8 - position)) != 0;
        }

        public Bool BufSetAlpha(Number byteArrayColumnIndex, int startIndex, Func<Text> value, int storageType, int length, bool asPointer)
        {
            Text val = null;
            InsteadOfNullStrategy.Instance.OverrideAndCalculate(() => val = value());
            return BufSetAlpha(byteArrayColumnIndex, startIndex, val, storageType, length, asPointer);
        }
        public Bool BufSetAlpha(Number byteArrayColumnIndex, int startIndex, Text value, int storageType, int length, bool asPointer)
        {
            var bac = GetColumnByIndex(byteArrayColumnIndex) as ByteArrayColumn;
            if (IsNull(bac, value))
                return false;
            if (length < 1) return true;
            if (value.Length > length)
                value = value.Substring(0, length);
            else if (value.Length < length)
                value = value.PadRight(length);
            var storage = new AnsiStringTextStorage(new TextColumn() { Format = length.ToString() });
            if (asPointer)
            {
                InsertValueIntoByteArrayColumnAsPointer(bac, value, storage, startIndex);
            }
            else
            {
                InsertValueIntoByteArrayColumn(bac, value, storage, startIndex);
            }
            return true;
        }

        public Text BufGetAlpha(Number byteArrayColumnIndex, int startIndex, int storageType, int length, bool asPointer)
        {
            var bac = GetColumnByIndex(byteArrayColumnIndex) as ByteArrayColumn;
            if (bac == null)
                return "";

            var bytes = GetBytesOfByteArrayColumn(bac.Value);
            if (bytes == null || bytes.Length < startIndex - 1 + (asPointer ? IntPtr.Size : length))
                return "";
            try
            {
                if (asPointer)
                {
                    var b = new byte[length];
                    Marshal.Copy(new IntPtr(new IntNumberStorage().LoadFrom(new BinaryValueLoader().GetFor(bytes, 1, startIndex - 1, 4))), b, 0, length);
                    bytes = b;
                    startIndex = 1;
                }
                return new BinaryValueLoader().GetFor(bytes, 0, startIndex - 1, length).GetString();
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex);
                return "";
            }

        }

        public byte[] BufGetBlob(Number byteArrayColumnIndex, int startIndex, int storageType, int length, bool asPointer)
        {
            try
            {
                var bac = GetColumnByIndex(byteArrayColumnIndex) as ByteArrayColumn;
                if (bac == null)
                    return new byte[0];

                var bytes = GetBytesOfByteArrayColumn(bac.Value);
                if (bytes == null || bytes.Length < startIndex - 1 + (asPointer ? IntPtr.Size : length))
                    return new byte[0];


                if (asPointer)
                {
                    var b = new byte[length];
                    Marshal.Copy(new IntPtr(new IntNumberStorage().LoadFrom(new BinaryValueLoader().GetFor(bytes, 1, startIndex - 1, 4))), b, 0, length);
                    bytes = b;
                    startIndex = 1;
                }

                if (storageType == 1)
                {
                    startIndex += 4;

                }

                return new BinaryValueLoader().GetFor(bytes, 0, startIndex - 1, length).GetByteArray();
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex);
                return null;
            }
        }

        BinaryValueLoader GetValueLoaderForByteArrayColumn(Number byteArrayColumnIndex, int startIndex, int length, int dataTypeCode, bool reverseBytes = false)
        {
            var bac = GetColumnByIndex(byteArrayColumnIndex) as ByteArrayColumn;
            if (bac == null) return null;
            var bytes = GetBytesOfByteArrayColumn(bac.Value);
            if (bytes == null || bytes.Length < startIndex - 1 + length) return null;
            if (reverseBytes)
                Array.Reverse(bytes, startIndex - 1, length);
            return new BinaryValueLoader().GetFor(bytes, dataTypeCode, startIndex - 1, length);
        }

        static bool _reverseBytesInBufGetSetOfNumber;

        public Number BufGetNum(Number byteArrayColumnIndex, int startIndex, int storageType, int length)
        {
            try
            {
                var vl = GetValueLoaderForByteArrayColumn(byteArrayColumnIndex, startIndex, length,
                    storageType == 1 ? 1 : storageType == 2 ? 14 : storageType == 3 ? 2 : 0, _reverseBytesInBufGetSetOfNumber);
                return vl == null ? null : GetNumberStorage(storageType, length).LoadFrom(vl);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex);
                return 0;
            }

        }

        public Date BufGetDate(Number byteArrayColumnIndex, int startIndex, int storageType)
        {
            var vl = GetValueLoaderForByteArrayColumn(byteArrayColumnIndex, startIndex, 4, 1);
            return vl == null ? null : GetDateStorage(storageType).LoadFrom(vl);
        }

        public Bool BufSetDate(Number byteArrayColumnIndex, int startIndex, Date value, int storageType)
        {
            var bac = GetColumnByIndex(byteArrayColumnIndex) as ByteArrayColumn;
            if (bac == null)
                return false;
            InsertValueIntoByteArrayColumn(bac, value, GetDateStorage(storageType), startIndex);
            return true;
        }

        public Bool BufGetLog(Number byteArrayColumnIndex, int startIndex, int storageType)
        {
            var vl = GetValueLoaderForByteArrayColumn(byteArrayColumnIndex, startIndex, 1, 0);
            return vl == null ? null : storageType == 1 ? (Bool)vl.GetBoolean() : new ByteArrayBoolStorage().LoadFrom(vl);
        }

        public Bool BufSetLog(Number byteArrayColumnIndex, int startIndex, Bool value, int storageType)
        {
            var bac = GetColumnByIndex(byteArrayColumnIndex) as ByteArrayColumn;
            if (bac == null)
                return false;
            InsertValueIntoByteArrayColumn(bac, value, storageType == 1 ? new BoolColumn().Storage : new ByteArrayBoolStorage(), startIndex);
            return true;
        }

        public Text[] DataViewVars(Number generation, Number option)
        {
            if (ReferenceEquals(generation, null) || ReferenceEquals(option, null))
                return null;


            var result = new List<Text>();
            DataViewVars(generation, option, col => result.Add(col.Caption));
            return result.ToArray();
        }
        public Number[] DataViewVarsIndex(Number generation, Number option)
        {
            if (ReferenceEquals(generation, null) || ReferenceEquals(option, null))
                return null;


            var result = new List<Number>();
            DataViewVars(generation, option, col => result.Add(IndexOf(col)));
            return result.ToArray();
        }
        void DataViewVars(Number generation, Number option, Action<ColumnBase> col)
        {
            var t = GetTaskByGeneration(generation);
            if (t != null)
            {

                if (option == 0)
                {
                    foreach (var column in ControllerBase.GetColumnsOf(t))
                    {
                        col(column);
                    }
                }
                else
                {
                    ControllerBase.SendInstanceBasedOnTaskAndCallStack(t,
                        y =>
                        {
                            y._provideColumnsForFilter(
                                (c, icb) =>
                                {
                                    if (icb != null && !(icb is Firefly.Box.UI.Button))
                                    {
                                        if (c != null && (option == 1 || icb.Available))
                                            col(c);
                                    }
                                });
                        });
                }
            }
        }
        public byte[] Cipher(Number algorithmId, Text plainText, Text key)
        {
            return Cipher(algorithmId, Data.ByteArrayColumn._byteParameterConverter.FromString(plainText),
                   Data.ByteArrayColumn._byteParameterConverter.FromString(key));
        }

        public byte[] Cipher(Number algorithmId, Text plainText, Text key, Text mode)
        {
            return Cipher(algorithmId, Data.ByteArrayColumn._byteParameterConverter.FromString(plainText),
                   Data.ByteArrayColumn._byteParameterConverter.FromString(key), mode);
        }
        public byte[] Cipher(Number algorithmId, Text plainText, TextColumn key, Text mode)
        {
            return Cipher(algorithmId, Data.ByteArrayColumn._byteParameterConverter.FromString(plainText),
                   Data.ByteArrayColumn._byteParameterConverter.FromString(key), mode);
        }
        public byte[] Cipher(Number algorithmId, TextColumn plainText, TextColumn key, Text mode)
        {
            return Cipher(algorithmId, Data.ByteArrayColumn._byteParameterConverter.FromString(plainText),
                   Data.ByteArrayColumn._byteParameterConverter.FromString(key), mode);
        }
        public byte[] Cipher(Number algorithmId, byte[] plainText, Text key, Text mode)
        {
            return Cipher(algorithmId, plainText,
                   Data.ByteArrayColumn._byteParameterConverter.FromString(key), mode);
        }
        public byte[] Cipher(Number algorithmId, Text plainText, Text key, Text mode, Text vector)
        {
            return Cipher(algorithmId, Data.ByteArrayColumn._byteParameterConverter.FromString(plainText),
                   Data.ByteArrayColumn._byteParameterConverter.FromString(key), mode, Data.ByteArrayColumn._byteParameterConverter.FromString(key));
        }

        public byte[] Cipher(Number algorithmId, Firefly.Box.Data.TextColumn plainText, Text key, Text mode)
        {
            return Cipher(algorithmId, Data.ByteArrayColumn._byteParameterConverter.FromString(plainText),
                   Data.ByteArrayColumn._byteParameterConverter.FromString(key), mode);
        }

        public byte[] Cipher(Number algorithmId, Firefly.Box.Data.TextColumn plainText, Firefly.Box.Data.TextColumn key)
        {
            return Cipher(algorithmId, Data.ByteArrayColumn._byteParameterConverter.FromString(plainText),
                   Data.ByteArrayColumn._byteParameterConverter.FromString(key));
        }

        public byte[] Cipher(Number algorithmId, Firefly.Box.Data.ByteArrayColumn plainText, Firefly.Box.Data.ByteArrayColumn key)
        {
            return Cipher(algorithmId, plainText, key, null, null);
        }
        public byte[] Cipher(Number algorithmId, byte[] plainText, byte[] key)
        {
            return Cipher(algorithmId, plainText, key, null, null);
        }
        public byte[] Cipher(Number algorithmId, ByteArrayColumn plainText, ByteArrayColumn key)
        {
            return Cipher(algorithmId, plainText, key, null, (byte[])null);
        }

        public byte[] Cipher(Number algorithmId, byte[] plainText, byte[] key, string mode)
        {
            return Cipher(algorithmId, plainText, key, mode, null);
        }
        public byte[] Cipher(Number algorithmId, Text plainText, byte[] key, string mode)
        {
            return Cipher(algorithmId, ByteArrayColumn._byteParameterConverter.FromString(plainText), key, mode, null);
        }
        public byte[] Cipher(Number algorithmId, TextColumn plainText, byte[] key, string mode)
        {
            return Cipher(algorithmId, ByteArrayColumn._byteParameterConverter.FromString(plainText), key, mode, null);
        }
        public byte[] Cipher(Number algorithmId, TextColumn plainText, ENV.Data.ByteArrayColumn key, string mode)
        {
            return Cipher(algorithmId, ByteArrayColumn._byteParameterConverter.FromString(plainText), key, mode, null);
        }

        public byte[] Cipher(Number algorithmId, Text plainText, ByteArrayColumn key, string mode)
        {
            return Cipher(algorithmId, ByteArrayColumn._byteParameterConverter.FromString(plainText), key, mode, null);
        }

        public byte[] Cipher(Number algorithmId, ByteArrayColumn plainText, ByteArrayColumn key, string mode)
        {
            return Cipher(algorithmId, plainText, key, mode, null);
        }

        [NotYetImplemented]
        public Bool Logging(Bool value, Text what)
        {
            return false;
        }

        public byte[] Cipher(Number algorithmId, byte[] plainText, byte[] key, string mode, byte[] vector)
        {
            return CipherOrDecipher(false, algorithmId, plainText, key, mode, vector);
        }

        byte[] CipherOrDecipher(bool decipher, Number algorithmId, byte[] input, byte[] key, string mode, byte[] vector)
        {
            if (input == null || key == null || algorithmId == null)
                return null;
            var x = new DoCipherDecipherParams(key, algorithmId, decipher, input, vector, mode);
            try
            {
                DoCipherDecipher(x);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex, "");
            }
            return x.Result;
        }

        public class DoCipherDecipherParams
        {
            byte[] _key;
            Number _algorithmId;
            bool _decipher;
            byte[] _input;
            byte[] _vector;
            string _mode;
            public byte[] Result = null;
            public DoCipherDecipherParams(byte[] key, Number algorithmId, bool decipher, byte[] input, byte[] vector, string mode)
            {
                _key = key;
                _algorithmId = algorithmId;
                _decipher = decipher;
                _input = input;
                _vector = vector;
                _mode = mode;
            }

            public byte[] Key
            {
                get { return _key; }
            }

            public Number AlgorithmId
            {
                get { return _algorithmId; }
            }

            public bool Decipher
            {
                get { return _decipher; }
            }

            public byte[] Input
            {
                get { return _input; }
                set { _input = value; }
            }

            public byte[] Vector
            {
                get { return _vector; }
            }

            public string Mode
            {
                get { return _mode; }
            }
        }

        partial void DoCipherDecipher(DoCipherDecipherParams doCipherDecipherParams);

        public byte[] DeCipher(Number algorithmId, byte[] cipherText, byte[] key)
        {
            return DeCipher(algorithmId, cipherText, key, "", null);
        }
        public byte[] DeCipher(Number algorithmId, byte[] cipherText, Text key)
        {
            return DeCipher(algorithmId, cipherText, ByteArrayColumn._byteParameterConverter.FromString(key), "", null);
        }
        public byte[] DeCipher(Number algorithmId, byte[] cipherText, byte[] key, string mode, byte[] vector)
        {
            return CipherOrDecipher(true, algorithmId, cipherText, key, mode, vector);
        }
        public byte[] DeCipher(Number algorithmId, ByteArrayColumn cipherText, ByteArrayColumn key, string mode, ByteArrayColumn vector)
        {
            return CipherOrDecipher(true, algorithmId, cipherText, key, mode, vector);
        }
        public byte[] DeCipher(Number algorithmId, byte[] cipherText, byte[] key, string mode)
        {
            return DeCipher(algorithmId, cipherText, key, mode, null);
        }
        public byte[] DeCipher(Number algorithmId, Text cipherText, Text key, string mode)
        {
            return DeCipher(algorithmId, cipherText, key, mode, null);
        }
        public byte[] DeCipher(Number algorithmId, byte[] cipherText, ByteArrayColumn key, string mode)
        {
            return DeCipher(algorithmId, cipherText, key, mode, null);
        }
        public byte[] DeCipher(Number algorithmId, byte[] cipherText, Text key, string mode)
        {
            return DeCipher(algorithmId, cipherText, ByteArrayColumn._byteParameterConverter.FromString(key), mode, null);
        }
        public byte[] DeCipher(Number algorithmId, ByteArrayColumn cipherText, Text key, string mode)
        {
            return DeCipher(algorithmId, cipherText, ByteArrayColumn._byteParameterConverter.FromString(key), mode, null);
        }
        public byte[] DeCipher(Number algorithmId, ByteArrayColumn cipherText, TextColumn key, string mode)
        {
            return DeCipher(algorithmId, cipherText, ByteArrayColumn._byteParameterConverter.FromString(key), mode, null);
        }
        public byte[] DeCipher(Number algorithmId, TextColumn cipherText, Text key, string mode)
        {
            return DeCipher(algorithmId, ByteArrayColumn._byteParameterConverter.FromString(cipherText), ByteArrayColumn._byteParameterConverter.FromString(key), mode, null);
        }
        public byte[] DeCipher(Number algorithmId, Text cipherText, Text key, string mode, Text vector)
        {
            return DeCipher(algorithmId, ByteArrayColumn._byteParameterConverter.FromString(cipherText), ByteArrayColumn._byteParameterConverter.FromString(key), mode, ByteArrayColumn._byteParameterConverter.FromString(vector));
        }
        public byte[] DeCipher(Number algorithmId, ByteArrayColumn cipherText, byte[] key, string mode)
        {
            return DeCipher(algorithmId, cipherText, key, mode, null);
        }
        public byte[] DeCipher(Number algorithmId, ByteArrayColumn cipherText, ByteArrayColumn key, string mode)
        {
            return DeCipher(algorithmId, cipherText, key, mode, (byte[])null);
        }
        public byte[] DeCipher(Number algorithmId, Text cipherText, byte[] key, string mode)
        {
            return DeCipher(algorithmId, ByteArrayColumn._byteParameterConverter.FromString(cipherText), key, mode, null);
        }
        public byte[] DeCipher(Number algorithmId, TextColumn cipherText, byte[] key, string mode)
        {
            return DeCipher(algorithmId, ByteArrayColumn._byteParameterConverter.FromString(cipherText), key, mode, null);
        }
        public byte[] DeCipher(Number algorithmId, Text plainText, Text key)
        {
            return DeCipher(algorithmId, Data.ByteArrayColumn._byteParameterConverter.FromString(plainText),
                   Data.ByteArrayColumn._byteParameterConverter.FromString(key));
        }
        public byte[] DeCipher(Number algorithmId, ByteArrayColumn cipherText, Text key)
        {
            return DeCipher(algorithmId, cipherText,
                   Data.ByteArrayColumn._byteParameterConverter.FromString(key));
        }
        public byte[] DeCipher(Number algorithmId, TextColumn plainText, Text key, Text mode)
        {
            return DeCipher(algorithmId, Data.ByteArrayColumn._byteParameterConverter.FromString(plainText),
                   Data.ByteArrayColumn._byteParameterConverter.FromString(key), mode);
        }
        public byte[] DeCipher(Number algorithmId, Firefly.Box.Data.TextColumn plainText, Firefly.Box.Data.TextColumn key)
        {
            return DeCipher(algorithmId, Data.ByteArrayColumn._byteParameterConverter.FromString(plainText),
                   Data.ByteArrayColumn._byteParameterConverter.FromString(key));
        }

        public byte[] DeCipher(Number algorithmId, ByteArrayColumn plainText, ByteArrayColumn key)
        {
            return DeCipher(algorithmId, plainText, key, null, (byte[])null);
        }
        public byte[] MTblGet(Number entityIndex, Text name)
        {
            if (entityIndex == null || name == null)
                return null;
            return MTblGet(name, _application.AllEntities.GetByIndex(entityIndex) as Entity);
        }

        public byte[] MTblGet(Type entityType, Text name)
        {
            if (entityType == null || name == null)
                return null;
            var e = System.Activator.CreateInstance(entityType) as ENV.Data.Entity;
            return MTblGet(name, e);
        }

        byte[] MTblGet(Text name, Entity e)
        {
            if (e == null)
                return null;
            if (!Types.Text.IsNullOrEmpty(name))
                e.EntityName = name.TrimEnd();
            return SerializeEntity(e);
        }

        public byte[] SerializeEntity(Entity e, FilterBase where = null)
        {
            byte[] result = null;
            SerializeEntity(e, long.MaxValue, y => result = y, where);
            return result;
        }
        public void SerializeEntity(Entity e, long rowsPerTransfer, Action<byte[]> result, FilterBase where = null)
        {
            SerializeEntity(e, rowsPerTransfer, result, callMeForEachRow =>
            {

                var bp = new BusinessProcess { From = e };
                if (where != null)
                    bp.Where.Add(where);
                bp.ForEachRow(() => callMeForEachRow());
            });

        }
        public long SerializeEntity(Entity e, long rowsPerTransfer, Action<byte[]> result, Action<Action> provideRows)
        {
            var s = new TableStructure();
            foreach (var column in e.Columns)
            {
                CastColumn(column, s);
            }


            var rows = new List<object[]>();

            var ser = new ClientParameterManager();
            Action onDone = () =>
            {
                var x = new SerializedTableData(rows.ToArray(), s.Result);
                using (var ms = new MemoryStream())
                {
                    new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(ms, x);
                    result(ms.ToArray());
                }
                rows.Clear();
            };
            long rowsSoFar = 0;
            provideRows(() =>
            {
                rowsSoFar++;
                var vals = new object[e.Columns.Count];
                for (int i = 0; i < vals.Length; i++)
                {
                    vals[i] = newPack(e.Columns[i].Value);
                }
                rows.Add(vals);
                if (rowsSoFar % rowsPerTransfer == 0)
                    onDone();
            });
            if (rowsSoFar == 0 || rows.Count > 0)
                onDone();
            return rowsSoFar;




        }
        static object newPack(object o)
        {
            if (o == null)
                return null;

            {
                var t = o as Text;
                if (t != null)
                {
                    return t.TrimEnd().ToString();
                }
            }
            {
                var t = o as Number;
                if (t != null)
                    return t.ToDecimal();
            }
            {
                var t = o as Date;
                if (t != null)
                    return (long)ENV.UserMethods.Instance.ToNumber(t);
            }
            {
                var t = o as Time;
                if (t != null)
                    return (long)ENV.UserMethods.Instance.ToNumber(t);
            }
            {
                var t = o as Bool;
                if (t != null)
                    return (bool)t;
            }
            return o;
        }
        class TableStructure : IColumnSpecifier
        {
            public string Result = "";
            public void DoOnColumn(TypedColumnBase<Text> column)
            {
                Result += "Text";
            }

            public void DoOnColumn(TypedColumnBase<Number> column)
            {
                Result += "Number";
            }

            public void DoOnColumn(TypedColumnBase<Date> column)
            {
                Result += "Date";
            }

            public void DoOnColumn(TypedColumnBase<Time> column)
            {
                Result += "Time";
            }

            public void DoOnColumn(TypedColumnBase<Bool> column)
            {
                Result += "Bool";
            }

            public void DoOnColumn(TypedColumnBase<byte[]> column)
            {
                Result += "Byte[]";
            }

            public void DoOnUnknownColumn(ColumnBase column)
            {
                throw new NotImplementedException();
            }
        }
        [Serializable]
        internal class SerializedTableData
        {
            string _structure;
            internal object[][] _data;
            public SerializedTableData(object[][] data, string structire)
            {
                _data = data;
                _structure = structire;

            }


            public Number Populate(Entity entity)
            {
                var ts = new TableStructure();
                foreach (var column in entity.Columns)
                {
                    CastColumn(column, ts);
                }
                if (ts.Result != _structure)
                    return -2;
                var ser = new ServerParameterManager();
                var bp = new BusinessProcess { From = entity, Activity = Activities.Insert, TransactionScope = TransactionScopes.Task };
                var en = _data.GetEnumerator();
                bp.Exit(ExitTiming.BeforeRow, () => !en.MoveNext());
                bp.DatabaseErrorOccurred += e =>
                {
                    if (e.ErrorType == DatabaseErrorType.DuplicateIndex)
                        e.HandlingStrategy = DatabaseErrorHandlingStrategy.Ignore;
                };
                bp.ForEachRow(() =>
                {
                    var row = (object[])en.Current;
                    for (int i = 0; i < row.Length; i++)
                    {
                        bp.Columns[i].Value = ser.UnPack(row[i]);
                    }
                    "".ToString();
                });

                return 0;
            }
        }
        public Number MTblSet(byte[] value, Number entityIndex, Text name, Number method)
        {
            return MTblSet(_application.AllEntities.GetByIndex(entityIndex) as Entity, name, method, value);
        }

        public Number MTblSet(byte[] value, Type entityType, Text name, Number method)
        {
            var e = System.Activator.CreateInstance(entityType) as ENV.Data.Entity;
            if (e == null)
                return null;
            return MTblSet(e, name, method, value);
        }
        internal class MtblGetter : IDisposable
        {
            class ColumnInfo
            {
                ColumnBase _column;
                public string Attribute;
                public int Length;
                Action<Func<int, byte[]>, Row> _populateRow;

                class myValueLoader : IValueLoader
                {
                    byte[] _bytes;

                    public myValueLoader(byte[] bytes)
                    {
                        _bytes = bytes;
                    }

                    public bool IsNull()
                    {
                        return false;
                    }

                    public Number GetNumber()
                    {
                        throw new NotImplementedException();
                    }

                    public string GetString()
                    {
                        throw new NotImplementedException();
                    }

                    public DateTime GetDateTime()
                    {
                        throw new NotImplementedException();
                    }

                    public TimeSpan GetTimeSpan()
                    {
                        throw new NotImplementedException();
                    }

                    public bool GetBoolean()
                    {
                        throw new NotImplementedException();
                    }

                    public byte[] GetByteArray()
                    {
                        return _bytes;
                    }
                }

                public ColumnInfo(ColumnBase column)
                {
                    _column = column;
                    Attribute = ENV.UserMethods.GetAttribute(column);
                    if (Attribute == "L")
                        Attribute = "B";
                    switch (Attribute)
                    {
                        case "B":
                            Length = 1;
                            _populateRow = (getBytes, row) =>
                            {
                                var val = getBytes(Length);
                                row.Set((Firefly.Box.Data.BoolColumn)column, val[0] == 1);
                            };
                            break;
                        case "N":
                            Length = 20;
                            _populateRow = (getBytes, row) =>
                            {

                                var val = getBytes(Length);
                                long result = 0;
                                val[0].ShouldBe((byte)255);
                                for (int i = 19; i > 0; i--)
                                {
                                    result = result * 256 + val[i];
                                }
                                row.Set((Firefly.Box.Data.NumberColumn)column, result);

                            };
                            break;
                        case "A":
                            Length = ((Firefly.Box.Data.TextColumn)column).FormatInfo.MaxDataLength;
                            _populateRow = (getBytes, row) =>
                            {
                                row.Set((Firefly.Box.Data.TextColumn)column,
                                        LocalizationInfo.Current.OuterEncoding.GetString(
                                            getBytes(Length)));
                            };
                            break;
                        default:
                            new NotSupportedException("Column in mtblget With attribute " + Attribute);
                            break;
                    }

                }

                public void Read(Func<int, byte[]> stream, Row populate)
                {
                    _populateRow(stream, populate);
                }
            }

            Entity _entity;
            Stream _r;
            List<ColumnInfo> _colInfo = new List<ColumnInfo>();
            public int TotalRows;
            public MtblGetter(Entity entity, Stream r)
            {
                _entity = entity;
                _r = r;
                foreach (var column in _entity.Columns)
                {
                    _colInfo.Add(new ColumnInfo(column));
                }
            }
            public void Read()
            {
                ReadToComma("MGBT");
                ReadToComma("1");
                ReadToComma("10");
                {
                    var s = new Queue<char>(ReadToComma().ToCharArray());
                    foreach (var c in _colInfo)
                    {
                        c.Attribute.ShouldBe(s.Dequeue().ToString());
                    }
                    s.Count.ShouldBe(0);
                }
                int totalLength = 0;
                foreach (var c in _colInfo)
                {
                    totalLength += c.Length;
                    ReadToComma(c.Length.ToString());
                }
                ReadToComma(totalLength.ToString());
                TotalRows = int.Parse(ReadToComma());
                ReadToComma("0");//i don't know what it is.
                var iterator = new Iterator(_entity);
                for (int i = 0; i < TotalRows; i++)
                {

                    var r = iterator.CreateRow();
                    foreach (var columnInfo in _colInfo)
                    {
                        columnInfo.Read(Read, r);
                    }
                    r.UpdateDatabase();
                }

            }
            byte[] Read(int chars)
            {
                var result = new List<byte>(chars);
                for (int i = 0; i < chars; i++)
                {
                    result.Add((byte)_r.ReadByte());
                }
                return result.ToArray();
            }

            public string ReadToComma()
            {
                var result = new List<byte>();
                int i;
                while ((i = _r.ReadByte()) != 44)
                {
                    result.Add((byte)i);
                }
                return LocalizationInfo.Current.OuterEncoding.GetString(result.ToArray());
            }
            public void ReadToComma(string expected)
            {
                ReadToComma().ShouldBe(expected);
            }
            public void Dispose()
            {
                _r.Dispose();
            }

        }
        internal Number MTblSet(Entity e, Text name, Number method, byte[] value)
        {
            if (!Types.Text.IsNullOrEmpty(name))
                e.EntityName = name.TrimEnd();
            if (value == null)
                return -4;
            if (method == 3)
                if (e.Exists())
                    e.Truncate();
            try
            {
                if (value.Length > 4 && value[0] == 77 && value[1] == 71 && value[2] == 66 && value[3] == 84)
                {
                    new MtblGetter(e, new MemoryStream(value)).Read();
                    return 0;
                }
                return DeserializeEntity(e, value);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex, "");
            }
            return -4;
        }

        public Number DeserializeEntity(Entity e, byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return ((SerializedTableData)new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(ms)).Populate(e);
            }

        }

        public Number EXP(Number value)
        {
            try
            {
                return Math.Exp(value);
            }
            catch (Exception)
            {

                return 0;
            }

        }
        public Date MDate()
        {
            return UserDeterminedDate;
            ;
        }
        public static Date UserDeterminedDate = Types.Date.Now;



        public static byte[] StreamToByteArray(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                int i;
                while ((i = stream.ReadByte()) != -1)
                {
                    ms.WriteByte((byte)i);
                }
                return ms.ToArray();
            }
        }

        public byte[] HTTPPost(Text url, Text body, params Text[] headerInformation)
        {
            byte[] b = null;
            if (body != null)
                b = LocalizationInfo.Current.OuterEncoding.GetBytes(body.ToString().ToCharArray());
            return HTTPPost(url, b,
                            headerInformation);
        }

        public byte[] HTTPPost(string url)
        {
            return HTTPPost(url, new byte[0]);
        }

        public byte[] HTTPPost(Text url, ENV.Data.ByteArrayColumn body, params Text[] headerInformation)
        {
            return HTTPPost(url, body.Value, headerInformation);
        }

        public byte[] HTTPCall(string verb, string url, Text body, params Text[] headerInformation)
        {
            byte[] b = null;
            if (body != null)
                b = LocalizationInfo.Current.OuterEncoding.GetBytes(body.ToString().ToCharArray());
            return HTTPCall(verb, url, b, headerInformation);

        }
        public byte[] HTTPCall(string verb, string url, byte[] body, params Text[] headerInformation)
        {
            return DoWebRequest(url, headerInformation,
               r =>
               {
                   r.Method = verb;

                   if (body != null && body.Length > 0 && verb != "GET")
                   {
                       using (var content = r.GetRequestStream())
                       {
                           foreach (var b in body)
                           {
                               content.WriteByte(b);
                           }
                       }
                       r.ContentType = "application/x-www-form-urlencoded";
                   }
               });
        }
        public byte[] HTTPCall(string verb, string url, ByteArrayColumn body, params Text[] headerInformation)
        {
            return HTTPCall(verb, url, body.Value, headerInformation);
        }

        static Dictionary<byte, byte[]> _urlEncode;
        public byte[] HTTPPost(Text url, byte[] body, params Text[] headerInformation)
        {
            return DoWebRequest(url, headerInformation,
                r =>
                {
                    r.Method = "POST";
                    if (body == null)
                        body = new byte[0];
                    var useUrlEncoding = true;
                    foreach (var header in headerInformation)
                    {
                        if (header.ToString().StartsWith(_contentTypeHeader, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (!header.ToString().ToLower().Contains("urlencoded"))
                                useUrlEncoding = false;
                        }
                    }
                    if (useUrlEncoding)
                    {
                        if (_urlEncode == null)
                        {
                            var dict = new Dictionary<byte, byte[]>();
                            for (int i = 0; i < 256; i++)
                            {
                                if (i == 47 || i == 61 || i == 38)
                                    continue;
                                var enc = System.Web.HttpUtility.UrlEncode(new byte[] { (byte)i });
                                if (i == 32)
                                    enc = "%20";
                                if (enc.StartsWith("%"))
                                {
                                    dict.Add((byte)i, System.Text.Encoding.ASCII.GetBytes(enc.ToUpper()));
                                }
                            }
                            _urlEncode = dict;
                        }
                        r.ContentType = "application/x-www-form-urlencoded";
                        using (var ms = new MemoryStream())
                        {
                            foreach (var b in body)
                            {
                                byte[] rep;
                                if (_urlEncode.TryGetValue(b, out rep))
                                {
                                    foreach (var rb in rep)
                                    {
                                        ms.WriteByte(rb);
                                    }
                                }
                                else
                                    ms.WriteByte(b);
                            }
                            body = ms.ToArray();
                        }

                    }

                    using (var content = r.GetRequestStream())
                    {
                        foreach (var b in body)
                        {
                            content.WriteByte(b);
                        }
                    }

                });

        }

        public Bool CodePage(Number codePageCode)
        {
            if (ReferenceEquals(codePageCode, null))
                return null;
            try
            {
                var enc = System.Text.Encoding.GetEncoding(codePageCode);
                LocalizationInfo.Current.OuterEncoding = enc;
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "");
            }

            return null;
        }


        static HashSet<char> _soundx1 = new HashSet<char>("bfpvBFPV".ToCharArray()),
            _soundx2 = new HashSet<char>("cgjkqsxzCGJKQSXZ".ToCharArray()),
            _soundx3 = new HashSet<char>("dtDT".ToCharArray()),
            _soundx4 = new HashSet<char>("lL".ToCharArray()),
            _soundx5 = new HashSet<char>("mnMN".ToCharArray()),
            _soundx6 = new HashSet<char>("rR".ToCharArray());
        public Text SoundX(Text test)
        {
            if (test == null)
                return null;
            string result = "";
            int previousCharGroup = -1;
            foreach (var c in test)
            {
                int currentCharGroup;
                if (_soundx1.Contains(c))
                    currentCharGroup = 1;
                else if (_soundx2.Contains(c))
                    currentCharGroup = 2;
                else if (_soundx3.Contains(c))
                    currentCharGroup = 3;
                else if (_soundx4.Contains(c))
                    currentCharGroup = 4;
                else if (_soundx5.Contains(c))
                    currentCharGroup = 5;
                else if (_soundx6.Contains(c))

                    currentCharGroup = 6;
                else
                    currentCharGroup = 0;

                if (result.Length == 0)
                    result = c.ToString().ToUpper(CultureInfo.InvariantCulture);
                else
                {
                    if (currentCharGroup != previousCharGroup && currentCharGroup != 0)
                    {
                        result += currentCharGroup.ToString();
                        if (result.Length == 4)
                            return result;
                    }
                }
                previousCharGroup = currentCharGroup;

            }
            return result.PadRight(4, '0');
        }
        public Text AppName()
        {
            return Sys();
        }

        public Number CtxNum()
        {
            return Firefly.Box.Context.ActiveContexts.Count();
        }

        public Bool FILE2REQ(Text fileName)
        {
            if (ReferenceEquals(fileName, null))
                return false;
            try
            {
                if (WebWriter.FixedWriter == null)
                {
                    System.Web.HttpContext.Current.Response.WriteFile(PathDecoder.DecodePath(fileName), true);
                    WebWriter.ThereWasAnOutput();
                }
                else
                {
                    WebWriter.FixedWriter.Write(System.IO.File.ReadAllText(PathDecoder.DecodePath(fileName)));

                }
                return true;
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "File2Req");
                return false;
            }
        }


        public Text HTTPLastHeader(Text headerType)
        {
            if (ReferenceEquals(headerType, null))
                return null;
            if (_lastHttpResponse == null)
                return null;
            if (headerType == Types.Text.Empty)
            {
                Text result = "HTTP/" + _lastHttpResponse.ProtocolVersion + " " + ((int)_lastHttpResponse.StatusCode) + " " + _lastHttpResponse.StatusDescription + "\r";
                for (int i = 0; i < _lastHttpResponse.Headers.Count; i++)
                {
                    result += _lastHttpResponse.Headers.GetKey(i) + ":" + _lastHttpResponse.Headers[i] + "\r";
                }
                return result;
            }
            else
                return _lastHttpResponse.Headers[headerType.TrimEnd()];

        }
        internal readonly static ContextAndSharedValue<string> Proxy = new ContextAndSharedValue<string>(null);
        const string _contentTypeHeader = "Content-Type:", _userAgentHeader = "User-Agent:", _expectHeader = "expect:", _acceptHeader = "accept:", _proxyConnection = "Proxy-Connection", _connection = "connection:";
        [ThreadStatic]
        static HttpWebResponse _lastHttpResponse;
        byte[] DoWebRequest(Text url, Text[] headerInformation, Action<WebRequest> doAlso)
        {
            if (ReferenceEquals(url, null))
                return null;
            try
            {
                try
                {
                    var r = (HttpWebRequest)WebRequest.Create(PathDecoder.DecodePathAndKeepChar(url).Trim());
                    r.Timeout = UserSettings.HttpWebRequestTimeoutInSeconds * 1000;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    if (Types.Text.IsNullOrEmpty(Proxy.Value))
                        r.Proxy = null;
                    else
                        r.Proxy = new WebProxy(Proxy.Value);
                    doAlso(r);
                    foreach (var s in headerInformation)
                    {
                        foreach (var header in s.ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (header.ToString().StartsWith(_contentTypeHeader, StringComparison.InvariantCultureIgnoreCase))
                            {
                                r.ContentType = header.Substring(_contentTypeHeader.Length);
                                continue;
                            }
                            else if (header.ToString().StartsWith(_userAgentHeader, StringComparison.InvariantCultureIgnoreCase))
                            {
                                r.UserAgent = header.Substring(_contentTypeHeader.Length);
                                continue;
                            }
                            else if (header.ToString().StartsWith(_acceptHeader, StringComparison.InvariantCultureIgnoreCase))
                            {
                                r.Accept = header.Substring(_acceptHeader.Length);
                                continue;
                            }
                            else if (header.ToString().StartsWith(_expectHeader, StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (!header.Substring(_expectHeader.Length).ToString().StartsWith("100-", StringComparison.InvariantCultureIgnoreCase))
                                    r.Expect = header.Substring(_expectHeader.Length);
                                continue;
                            }
                            else if (header.ToString().StartsWith(_proxyConnection, StringComparison.InvariantCultureIgnoreCase))
                            {
                                continue;
                            }
                            else if (header.ToString().StartsWith(_connection, StringComparison.InvariantCultureIgnoreCase))
                            {
                                r.KeepAlive = header.Contains("Keep-Alive");
                                continue;
                            }
                            if (header.Contains(":"))
                            {
                                var x = header.Split(':');
                                if (x.Length == 2)
                                    r.Headers.Add(x[0].Trim() + ':' + x[1]);
                                else
                                    r.Headers.Add(header);
                            }
                        }
                    }
                    var ar = r.BeginGetResponse(delegate { }, null);

                    var response = r.EndGetResponse(ar);
                    try
                    {
                        _lastHttpResponse = response as HttpWebResponse;
                        return StreamToByteArray(response.GetResponseStream());
                    }
                    finally
                    {
                        response.Close();
                    }
                }
                catch (WebException e)
                {

                    ErrorLog.WriteToLogFile(e, "WebRequest");
                    _lastHttpResponse = e.Response as HttpWebResponse;
                    if (e.Response == null)
                        return new byte[0];
                    return StreamToByteArray(e.Response.GetResponseStream());


                }
                finally
                {
                }
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "WebRequest");
                return new byte[0];
            }

        }

        public byte[] HTTPGet(Text url, params Text[] headerInformation)
        {
            return DoWebRequest(url, headerInformation, r => { r.Method = "GET"; });
        }
        public byte[] HTTPGet(byte[] url, params Text[] headerInformation)
        {
            return DoWebRequest(ByteArrayToText(url), headerInformation, r => { r.Method = "GET"; });
        }
        public byte[] HTTPGet(ByteArrayColumn url, params Text[] headerInformation)
        {
            return DoWebRequest(url, headerInformation, r => { r.Method = "GET"; });
        }


        static System.Runtime.InteropServices.COMException _lastComException = null;
        internal static void SetLastComError(System.Runtime.InteropServices.COMException e)
        {
            _lastComException = e;

        }
        public Text ComError(int type)
        {
            if (_lastComException == null)
                return Types.Text.Empty;
            switch (type)
            {
                case 1:
                    return _lastComException.Message;
                case 2:
                    return _lastComException.Source;
                case 3:
                    return "0";
                case 4:
                    {
                        var x = _lastComException.HelpLink;
                        if (x == null)
                            return "";
                        if (x.Contains("#"))
                            return x.Remove(x.IndexOf("#"));
                        return x;
                    }
                case 5:
                    {
                        var x = _lastComException.HelpLink;
                        if (x == null)
                            return "";
                        if (x.Contains("#"))
                            return x.Substring(x.IndexOf("#") + 1);
                        return x;
                    }
                case 0:
                    return string.Format("0x{0:x}", _lastComException.ErrorCode).ToUpper(CultureInfo.InvariantCulture); ;
            }
            return "";
        }

        public static bool IsDerivedFromGenericType(Type givenType, Type genericType)
        {
            if (givenType == null) return false;
            if (givenType.IsGenericType)
            {
                if (givenType.GetGenericTypeDefinition() == genericType) return true;
            }
            return IsDerivedFromGenericType(givenType.BaseType, genericType);
        }

        public Number COMHandleGet(ColumnBase column)
        {
            if (column == null)
                return null;
            if (IsDerivedFromGenericType(column.GetType(), typeof(Firefly.Box.Interop.ComColumn<>)))
            {
                if (column.Value == null) return -3;
                var result = (long)-1;
                Context.Current.InvokeUICommand(() => result = Marshal.GetIUnknownForObject(column.Value).ToInt64());
                return result;
            }
            return -1;
        }
        [NotYetImplemented]
        public byte[] File2OLE(Text t, Bool x)
        {
            return null;
        }

        [NotYetImplemented]
        public Text IStr(Number n)
        {
            return null;
        }
        [NotYetImplemented]
        public Number IVal(Text n)
        {
            return null;
        }

        List<object> GetParamsForJava(object[] p)
        {
            var l = new List<object>(p);
            return l.ConvertAll(CastToObject);
        }

        string _lastJavaException;
        public object JCallStatic(string classAndMethodName, string methodSignature, params object[] parameters)
        {
            JavaNativeInterface jni = null;
            try
            {
                jni = new JavaNativeInterface();
                return jni.CallStaticMethod(
                            classAndMethodName.Substring(0, classAndMethodName.LastIndexOf('.')).Replace(".", "/"),
                            classAndMethodName.Substring(classAndMethodName.LastIndexOf('.') + 1),
                            methodSignature, GetParamsForJava(parameters));
            }
            catch { }
            finally
            {
                if (jni != null)
                    _lastJavaException = jni.GetJavaException();
            }
            return null;
        }

        static ContextStatic<Dictionary<Guid, Java.JavaNativeInterface>> _javaObjects =
            new ContextStatic<Dictionary<Guid, JavaNativeInterface>>();

        public byte[] JCreate(string className, string contructorSignature, params object[] contructorParams)
        {
            className = className.Replace(".", "/");
            JavaNativeInterface jni = null;
            try
            {
                jni = new JavaNativeInterface();
                jni.CreateObject(className, contructorSignature, GetParamsForJava(contructorParams).ToArray());

                var guid = Guid.NewGuid();
                _javaObjects.Value.Add(guid, jni);
                return guid.ToByteArray();
            }
            catch { }
            finally
            {
                if (jni != null)
                    _lastJavaException = jni.GetJavaException();
            }
            return null;
        }

        void ReleaseJavaObjects()
        {
            if (_javaObjects.Value.Count > 0)
            {
                var byteArrayValues = new HashSet<Guid>();
                Action<ColumnBase> processColumn =
                    column =>
                    {
                        var bac = column as Firefly.Box.Data.ByteArrayColumn;
                        if (bac != null && bac.Value != null && bac.Value.Length == 16)
                            byteArrayValues.Add(new Guid(bac.Value));
                    };
                foreach (ITask task in Context.Current.ActiveTasks)
                {
                    foreach (ColumnBase column in ControllerBase.GetColumnsOf(task))
                        processColumn(column);
                }
                ApplicationControllerBase.ForEachActiveController(
                    ac => ApplicationControllerBase.SendColumnsOf(ac,
                    cols =>
                    {
                        foreach (var c in cols)
                            processColumn(c);
                    }));
                var dic = new Dictionary<Guid, Java.JavaNativeInterface>(_javaObjects.Value);
                foreach (var i in _javaObjects.Value)
                {
                    if (!byteArrayValues.Contains(i.Key))
                        dic.Remove(i.Key);
                }
                _javaObjects.Value = dic;
            }
        }

        public object JCall(ByteArrayColumn objReference, string methodName, string methodSignature, params object[] methodParameters)
        {
            if (objReference.Value == null)
                return null;
            var jni = _javaObjects.Value[new Guid(objReference.Value)];
            try
            {
                return jni.CallMethod(methodName, methodSignature, GetParamsForJava(methodParameters));
            }
            catch { }
            finally
            {
                _lastJavaException = jni.GetJavaException();
            }
            return null;

        }

        public object JGet(ByteArrayColumn objReference, string fieldName, string signature)
        {
            var jni = _javaObjects.Value[new Guid(objReference.Value)];
            try
            {
                return jni.GetFieldValue(fieldName, signature);
            }
            catch { }
            finally
            {
                _lastJavaException = jni.GetJavaException();
            }
            return null;
        }

        [NotYetImplemented]
        public byte[] JExplore(Text p)
        {
            return new byte[0];
        }

        public string JException()
        {
            return _lastJavaException ?? "";
        }

        public string JExceptionText(bool b)
        {
            return _lastJavaException ?? "";
        }

        public bool JExceptionOccurred()
        {
            return !string.IsNullOrEmpty(_lastJavaException);
        }

        public Bool SetBufCnvParam(string paramName, object paramValue)
        {
            if (paramName.Trim() == "Low-Hi" && paramValue is bool)
            {
                _reverseBytesInBufGetSetOfNumber = !((bool)paramValue);
                return true;
            }
            return false;
        }
        Firefly.Box.UI.TreeView GetTree(ITask t)
        {
            if (t != null)
            {
                var f = t.View;
                if (f != null)
                {
                    TreeView tree = null;
                    f.ForEachControlInTabOrder(c => tree = c as TreeView ?? tree);
                    return tree;
                }
            }
            return null;
        }

        public Number TreeLevel()
        {
            var t = GetTree(GetTaskByGeneration(0));
            if (t == null)
                return 0;
            var node = t.GetCurrentNode();
            if (node == null) return 0;
            int i = -1;
            while (node != null)
            {
                node = node.Parent;
                i++;
            }
            return i;
        }

        public object TreeValue(Number value)
        {
            var t = GetTree(GetTaskByGeneration(0));
            if (t == null)
                return null;
            var node = t.GetCurrentNode();
            int i = value;
            while (node != null && i > 0)
            {
                node = node.Parent;
                i--;
            }
            object result = null;
            if (i == 0 && node != null)
                result = t.GetValueOf(node);
            if (result == null && t.NodeID != null)
            {
                var v = t.NodeID.Value;
                if (v != null)
                {

                    if (v is Number)
                        return 0;
                    if (v is Bool)
                        return false;
                    if (v is Date)
                        return Types.Date.Empty;
                    if (v is Time)
                        return Types.Time.StartOfDay;
                    if (v is Bool)
                        return false;
                }

            }

            return result;
        }

        public Bool ClrCache()
        {
            var t = GetTaskByGeneration(0);
            if (t != null)
            {
                foreach (var entity in t.Entities)
                {
                    entity.ClearRelationCache();
                }
                return true;
            }
            else
                return false;
        }

        public Text MainLevel(Number generation)
        {
            var x = _contextTask;
            try
            {

                _contextTask = _currentTask;
                if (generation == null)
                    return null;
                ITask task = GetTaskByGeneration(generation);
                if (task != null)
                {
                    return Advanced.LevelProvider.GetMainLevelOf(task);
                }
                return Types.Text.Empty;
            }
            finally
            {
                _contextTask = x;
            }
        }
        [NotYetImplemented]
        public Number DbCache(Number p, Number p_2)
        {
            return 0;
        }

        public Bool DragSetData(Text data, Number format)
        {
            return DragSetData(data, format, "");
        }

        public Bool DragSetData(Text data, Number format, Text userDefinedFormat)
        {
            try
            {
                if (ControlBase.DragDropData == null)
                    return false;
                var dataFormat = GetDataFormat(format, userDefinedFormat);
                ControlBase.DragDropData.SetData(dataFormat, dataFormat == DataFormats.FileDrop ? (object)new string[] { data.TrimEnd().ToString() } : data.ToString());
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex);
                return false;
            }
        }

        string GetDataFormat(Number format)
        {
            return GetDataFormat(format, "");
        }

        string GetDataFormat(Number format, string userDefinedFormat)
        {
            switch ((int)format)
            {
                case 0:
                    if (_dropUserFormats.Contains(userDefinedFormat.TrimEnd()))
                        return userDefinedFormat.TrimEnd();
                    return null;
                case 1: return DataFormats.Text;
                case 2: return DataFormats.OemText;
                case 3: return DataFormats.Rtf;
                case 4: return DataFormats.Html;
                case 5: return DataFormats.SymbolicLink;
                case 6: return DataFormats.FileDrop;
                default:
                    throw new NotImplementedException("Unknown drop format " + format);
            }
        }

        public Bool DropFormat(Number formatType)
        {
            return DropFormat(formatType, "");
        }
        public Bool DropFormat(Number formatType, Text userDefinedFormat)
        {
            try
            {
                if (ControlBase.DragDropData == null) return false;
                if (formatType == 0 && userDefinedFormat == null) return false;
                var format = formatType == 0 ? userDefinedFormat.ToString().TrimEnd() : GetDataFormat(formatType);
                return ControlBase.DragDropData.GetData(format) != null;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex);
                return false;
            }
        }

        static string[] _dropUserFormats;

        internal static void SetDropUserFormats(string value)
        {
            _dropUserFormats = value.Split(',');
            ControlBase.DragDropFormats = _dropUserFormats;
        }

        public Text DropGetData(Number format, Text userDefinedFormat)
        {
            try
            {
                if (ControlBase.DragDropData == null) return null;
                var f = GetDataFormat(format, userDefinedFormat);
                if (f == null) return null;
                var x = ControlBase.DragDropData.GetData(f);
                if (x == null) return null;
                if (x is string[]) return string.Join(FilesSeparator, (string[])x);
                return Types.Text.Cast(x);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex);
                return "";
            }
        }
        public Text DropGetData(Number format)
        {
            return DropGetData(format, "");
        }
        [NotYetImplemented]
        public byte[] VariantCreate(params object[] args)
        {
            return new byte[0];
        }

        public Text VariantAttr(ByteArrayColumn val)
        {
            if (val == null)
                return null;
            return VariantAttr(val.Value);
        }

        public Text VariantAttr(byte[] val)
        {
            int type = VariantType(val);
            switch (type)
            {
                case 2:
                case 3:
                    return "N";
                case 7:
                    return "D";
                case 8:
                    return "A";
                default:
                    return "0";
            }
        }

        public Bool ClipWrite()
        {
            try
            {
                if (!string.IsNullOrEmpty(_clipboardBuffer))
                    Clipboard.SetText(_clipboardBuffer);
                _clipboardBuffer = "";
                return true;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return false;
            }
        }

        static string _clipboardBuffer = "";
        public Bool ClipAdd(params object[] valuesAndFormats)
        {
            try
            {

                int i = 0;
                object value = null;
                foreach (var o in valuesAndFormats)
                {
                    if (i++ % 2 == 0)
                    {
                        value = o;
                    }
                    else
                    {
                        if (i > 2)
                            _clipboardBuffer += "\t";
                        var x = value as DataTypeBase;
                        var y = value as ColumnBase;
                        if (x != null)
                        {
                            _clipboardBuffer += x.ToString(o.ToString());
                        }
                        else if (y != null)
                            _clipboardBuffer += y.ToString(o.ToString());
                        else
                        {
                            _clipboardBuffer += (value ?? "").ToString();
                        }
                    }
                }
                if (i % 2 == 1)
                    _clipboardBuffer += (value ?? "").ToString();
                if (_clipboardBuffer.Length > 0)
                    _clipboardBuffer += "\r\n";
                return true;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return false;
            }
        }
        public Text ClipRead()
        {
            try
            {
                return Clipboard.GetText();
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return null;
            }
        }
        public byte[] ClipReadV9Compatible()
        {
            var r = ClipRead();
            if (ReferenceEquals(r, null))
                return null;
            return LocalizationInfo.Current.OuterEncoding.GetBytes(ClipRead().ToString().ToCharArray());
        }
        public static string _projectDir = System.Windows.Forms.Application.StartupPath + "\\";
        public Text ProjectDir()
        {
            return Translate(_projectDir);
        }
        public string ToString(object value, string format)
        {
            if (value == null)
                return "";
            var bac = value as ByteArrayColumn;
            if (bac != null)
                if (bac.ContentType == ByteArrayColumnContentType.BinaryUnicode || bac.ContentType == ByteArrayColumnContentType.BinaryAnsi)
                {
                    if (InternalIsNull(bac.Value))
                        return "";
                    else
                        return Convert.ToBase64String(bac.Value);
                }
                else
                    return bac.ToString();
            var ba = value as byte[];
            if (ba != null)
                return Convert.ToBase64String(ba);
            var c = value as ColumnBase;
            if (c != null)
                return c.ToString(format);
            var x = value as DataTypeBase;
            if (x != null)
                return x.ToString(format);
            {
                Number v;
                if (Types.Number.TryCast(value, out v))
                    return v.ToString(format);
            }
            {
                Bool v;
                if (Firefly.Box.Bool.TryCast(value, out v))
                    return v.ToString(format);
            }
            Text t = value.ToString();
            return t.ToText(format).TrimEnd(' ');
        }


        public Text TranslateTerminalChars(Text chars)
        {
            if (chars == Chr(16))
                return Chr((int)0x34);
            if (chars == Chr(17))
                return Chr((int)0x33);
            if (chars == Chr(16) + Chr(16))
                return Chr((int)0x38);
            if (chars == Chr(17) + Chr(17))
                return Chr((int)0x37);
            return chars;
        }
        [NotYetImplemented]
        public Text StripTrailingZeros(Number value)
        {
            return value.ToString();
        }

        public bool EmptyDataview(int generation)
        {
            var uic = GetTaskByGeneration(generation) as UIController;
            if (uic != null)
                return uic.NoData;
            return false;
        }




        public Text MTStr(Number mTime, Text format)
        {
            if (mTime == null || format == null)
                return null;
            var time = this.Fix(mTime / 1000, 18, 0);
            var result = TStr(ToTime(time), format);
            result = result.Replace("mmm", Str(Fix((mTime % 1000), 9, 0), "3P0"));
            return result;
        }
        public Number MTVal(Text text, Text format)
        {
            if (text == null || format == null)
                return null;
            var timeInMs = ToNumber(TVal(text, format)) * 1000;
            int position = InStr(format, "mmm");
            if (position > 0)
            {
                return timeInMs + Val(Mid(text, position, 3), "3");
            }
            else
                return timeInMs - 1;
        }


        public Bool RqHTTPHeader(Text headerOne, params Text[] otherHeaders)
        {
            if (ReferenceEquals(headerOne, null))
                return null;
            var l = new List<Text>();
            l.Add(headerOne);
            l.AddRange(otherHeaders);
            foreach (var ss in l)
            {
                if (Types.Text.IsNullOrEmpty(ss))
                    continue;
                var s = ss.ToString().Trim();
                var i = s.IndexOf(":");

                if (i < 0)
                    throw new Exception("Not supported Header " + s);

                var name = s.Remove(i);
                var value = s.Substring(i + 1);


                switch (name.ToUpper())
                {
                    case "CONTENT-TYPE":
                        System.Web.HttpContext.Current.Response.ContentType = value;
                        break;
                    case "SET-COOKIE":
                        var cookieVals = value.Split('=');
                        System.Web.HttpContext.Current.Response.AppendCookie(new HttpCookie(cookieVals[0], cookieVals[1]));
                        break;
                    default:
                        System.Web.HttpContext.Current.Response.AddHeader(name, value);
                        break;
                }

            }

            WebWriter.ThereWasAnOutput();
            return true;
        }


        public byte[] ErrPosition()
        {
            if (_currentDatabaseError == null)
                return null;
            if (_currentDatabaseError.Entity is BtrieveEntity && _currentDatabaseError.Exception != null && _currentDatabaseError.Exception.InnerException is BtrieveException)
                return BtrievePositionToByteArray(((BtrieveException)_currentDatabaseError.Exception.InnerException).ErrorPosition);
            return CreateFilterByteArrayBasedOnPrimaryKeyOfEntity(_currentDatabaseError.Entity);
        }


        public bool FormStateClear(Text name)
        {
            return ENV.UI.Form.FormStateClear(name);

        }


        public bool Blob2Req(byte[] data)
        {
            if (data == null)
                return false;
            if (System.Web.HttpContext.Current == null)
                return false;
            System.Web.HttpContext.Current.Response.BinaryWrite(data);
            WebWriter.ThereWasAnOutput();
            return true;
        }

        public bool Blob2Req(ENV.Data.ByteArrayColumn data)
        {
            if (ReferenceEquals(data, null))
                return false;
            if (ReferenceEquals(data.Value, null))
                return false;
            if (System.Web.HttpContext.Current == null)
                return false;
            System.Web.HttpContext.Current.Response.BinaryWrite(data);
            WebWriter.ThereWasAnOutput();
            return true;
        }

        public bool Blob2Req(Text data)
        {
            if (ReferenceEquals(data, null))
                return false;
            if (System.Web.HttpContext.Current == null)
                return false;
            System.Web.HttpContext.Current.Response.Write(data);
            WebWriter.ThereWasAnOutput();
            return true;
        }

        public Number Lock(string s, int timeoutInSeconds)
        {
            if (s.Length > 128)
                s = s.Substring(0, 128);
            var byteToLock = ((uint)s.GetHashCode());
            if (LockFile.IsByteLocked(byteToLock))
                return 1;
            var start = Environment.TickCount;
            do
            {
                if (LockFile.LockByte(byteToLock))
                    return 0;
                if (timeoutInSeconds > 0)
                    System.Threading.Thread.Sleep(500);
            } while (Environment.TickCount - start < 1000 * timeoutInSeconds);
            return 2;
        }

        public Number UnLock(string s)
        {
            if (s.Length > 128)
                s = s.Substring(0, 128);
            var byteToLock = ((uint)s.GetHashCode());
            if (!LockFile.IsByteLocked(byteToLock))
                return 0;
            LockFile.UnlockByte(byteToLock);
            return 1;
        }


        public bool ParamsUnPack(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return false;
            try
            {
                return ParametersInMemory.Instance.Deserialize(bytes);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex);
                return false;
            }
        }

        public byte[] ParamsPack()
        {
            try
            {
                return ParametersInMemory.Instance.Serialize();
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToLogFile(ex);
                return new byte[0];
            }
        }




        public Number XMLSetNS(Number generation, Number ioIndex, Text @namespace, Text url)
        {
            return DoForXml<Number>(generation, ioIndex, "", -1
               , x =>
                       x.SetNamespace(@namespace, url)
                   );
        }

        public Bool DbXmlExist(Number entityIndex, ByteArrayColumn blob)
        {
            return InternalDbXmlExist(_application.AllEntities.GetByIndex(entityIndex), blob);
        }
        public Bool DbXmlExist(Type entityType, ByteArrayColumn blob)
        {
            return InternalDbXmlExist(System.Activator.CreateInstance(entityType) as Types.Data.Entity, blob);
        }
        Bool InternalDbXmlExist(Types.Data.Entity entityType, ByteArrayColumn blob)
        {
            Bool result = false;
            var xe = entityType as ENV.Data.DataProvider.XmlEntity;
            if (xe == null)
                return false;
            xe.SetXmlSource(blob);
            return new BusinessProcess { From = xe }.ForFirstRow(() => { });
        }

        public Number Cos(Number value)
        {
            try
            {
                return Math.Cos(value);
            }
            catch
            {
                return 0;
            }
        }
        public Number Sin(Number value)
        {
            try
            {
                return Math.Sin(value);
            }
            catch
            {
                return 0;
            }
        }

        public Number Acos(Number value)
        {
            try
            {
                return Math.Acos(value);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public Number Asin(Number value)
        {
            try
            {
                return Math.Asin(value);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public Number Atan(Number value)
        {
            try
            {
                return Math.Atan(value);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public Number Tan(Number value)
        {
            try
            {
                return Math.Tan(value);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        [NotYetImplemented]
        public Bool RqExe(Text serviceName, Text entryToExecute, Text arguments, Text password)
        {
            Common.ShowMessageBox("RqExe",
                MessageBoxIcon.Exclamation,
                "Execution an engine on the server is not required in .NET");
            return false;
        }

        public Text RqLoad(Text serviceName, Text password)
        {
            if (IsNull(serviceName, password))
                return null;
            return new Remoting.HttpApplication(UserSettings.GetApplicationServerUrl(serviceName)).GetRqLoad();
        }

        public Text RqRtInf(Text serviceName, Number engineNumber)
        {
            if (IsNull(serviceName, engineNumber))
                return null;
            return new Remoting.HttpApplication(UserSettings.GetApplicationServerUrl(serviceName)).GetRqRtInf(engineNumber);
        }

        static Dictionary<string, HttpApplicationServer.RequestInfo[]> _requests = new Dictionary<string, HttpApplicationServer.RequestInfo[]>();

        public Number RqReqLst(Text serviceName, Text requestIdMin, Text requestIdMax, Text password)
        {
            if (IsNull(serviceName, requestIdMin, requestIdMax, password))
                return null;
            serviceName = serviceName.TrimEnd();
            var url = UserSettings.GetApplicationServerUrl(serviceName);
            if (string.IsNullOrWhiteSpace(url))
                return 0;
            var result = new Remoting.HttpApplication(url).GetRequests(Number.Parse(requestIdMin), Number.Parse(requestIdMax), false);
            if (_requests.ContainsKey(serviceName))
            {
                _requests[serviceName] = result;
            }
            else
                _requests.Add(serviceName, result);
            return result.Length;
        }
        public Text RqReqInf(Text serviceName, Number entryNumber)
        {
            if (IsNull(serviceName, entryNumber))
                return null;
            serviceName = serviceName.TrimEnd();
            if (_requests.ContainsKey(serviceName))
            {
                var y = _requests[serviceName];
                if (entryNumber - 1 < y.Length && entryNumber > 0)
                    return y[entryNumber - 1].ToReqInfString();

            }
            return Types.Text.Empty;
        }

        public Bool RqQueDel(Text serviceName, Text requestId, Text password)
        {
            if (IsNull(serviceName, requestId))
                return null;
            return new Remoting.HttpApplication(UserSettings.GetApplicationServerUrl(serviceName)).AbortRequest(Number.Parse(requestId));
        }

        public object RqQueLst(Text serviceName, Text password)
        {
            if (IsNull(serviceName, password))
                return null;
            serviceName = serviceName.TrimEnd();
            var result = new Remoting.HttpApplication(UserSettings.GetApplicationServerUrl(serviceName)).GetRequests(0, long.MaxValue, true);
            if (_requests.ContainsKey(serviceName))
            {
                _requests[serviceName] = result;
            }
            else
                _requests.Add(serviceName, result);
            return result.Length;
        }

        [NotYetImplemented]
        public Bool RqQuePri(Text serviceName, Text requestId, Number newPrioriry, Text password)
        {
            return false;
        }

        public Number RqRts(Text serviceName, Text password)
        {
            if (IsNull(serviceName, password))
                return null;
            return new Remoting.HttpApplication(UserSettings.GetApplicationServerUrl(serviceName)).GetRqRts();
        }
        [NotYetImplemented]
        public Number RqRtApps(Text serviceName, Number weDontCareAboutIt, Text password)
        {
            return 1;
        }

        public Text RqRtApp(Text serviceName, Number weDontCareAboutIt)
        {
            if (IsNull(serviceName))
                return null;
            return new Remoting.HttpApplication(UserSettings.GetApplicationServerUrl(serviceName)).RqRtApp();
        }

        public bool RqRtTrm(Text serviceName, Number engineNumber, Text password)
        {
            Common.ShowMessageBox("RqRtTrm",
                MessageBoxIcon.Exclamation,
                "Terminate Server Engine is not required in .NET");
            return false;
        }
        public Number RQStat(Text serviceName, Text requestId, Text password)
        {
            if (IsNull(serviceName, requestId, password))
                return null;
            return new Remoting.HttpApplication(UserSettings.GetApplicationServerUrl(serviceName)).RQStat(Number.Parse(requestId));
        }

        [NotYetImplemented]
        public Number EuroCnv(Text a, Text b, Number n)
        {
            return n;
        }
        [NotYetImplemented]
        public Text EuroGet()
        {
            return "";
        }
        [NotYetImplemented]
        public Bool EuroUpd(Text a, Text b, Number c, Number d)
        {
            return false;
        }
        [NotYetImplemented]
        public Bool EuroDel(Text a)
        {
            return false;
        }
        [NotYetImplemented]
        public Bool EuroSet(Text a)
        {
            return false;
        }

        public Text StatusBarSetText(Text value)
        {
            if (ReferenceEquals(value, null))
                return null;
            Common.SetDefaultStatusText(value.Trim());
            return value.Trim();
        }

        public Text TaskID(Number generation)
        {
            if (generation == null)
                return null;
            Text result = "NULL";
            var t = GetTaskByGeneration(generation);
            if (t != null)
            {
                result = "";
                ControllerBase.SendInstanceBasedOnTaskAndCallStack(t, y => result = y.TaskID ?? "");
            }
            return result;

        }
        [NotYetImplemented]
        public Bool DISCSRVR(Text t)
        {
            return false;
        }



        /*1000*/
        public Bool DifDateTime(DateColumn date1, Time time1, Date date2, Time time2, Number refColumnForDaysResult,
            Number refColumnForTimeResult)
        {
            return DifDateTime(date1.Value ?? date1.GetInsteadOfNull(), time1, date2, time2, refColumnForDaysResult,
                refColumnForTimeResult);
        }
        /*1100*/
        public Bool DifDateTime(DateColumn date1, TimeColumn time1, Date date2, Time time2, Number refColumnForDaysResult,
            Number refColumnForTimeResult)
        {
            return DifDateTime(date1.Value ?? date1.GetInsteadOfNull(), time1.Value ?? time1.GetInsteadOfNull(), date2, time2, refColumnForDaysResult,
                refColumnForTimeResult);
        }
        /*1110*/
        public Bool DifDateTime(DateColumn date1, TimeColumn time1, DateColumn date2, Time time2, Number refColumnForDaysResult,
          Number refColumnForTimeResult)
        {
            return DifDateTime(date1.Value ?? date1.GetInsteadOfNull(), time1.Value ?? time1.GetInsteadOfNull(), date2.Value ?? date2.GetInsteadOfNull(), time2, refColumnForDaysResult,
                refColumnForTimeResult);
        }

        /*0111*/
        public Bool DifDateTime(Date date1, TimeColumn time1, DateColumn date2, TimeColumn time2, Number refColumnForDaysResult,
         Number refColumnForTimeResult)
        {
            return DifDateTime(date1, time1.Value ?? time1.GetInsteadOfNull(), date2.Value ?? date2.GetInsteadOfNull(), time2.Value ?? time2.GetInsteadOfNull(), refColumnForDaysResult,
                refColumnForTimeResult);
        }
        /*0011*/
        public Bool DifDateTime(Date date1, Time time1, DateColumn date2, TimeColumn time2, Number refColumnForDaysResult,
         Number refColumnForTimeResult)
        {
            return DifDateTime(date1, time1, date2.Value ?? date2.GetInsteadOfNull(), time2.Value ?? time2.GetInsteadOfNull(), refColumnForDaysResult,
                refColumnForTimeResult);
        }
        /*0001*/
        public Bool DifDateTime(Date date1, Time time1, Date date2, TimeColumn time2, Number refColumnForDaysResult,
         Number refColumnForTimeResult)
        {
            return DifDateTime(date1, time1, date2, time2.Value ?? time2.GetInsteadOfNull(), refColumnForDaysResult,
                refColumnForTimeResult);
        }
        /*1011*/
        public Bool DifDateTime(DateColumn date1, Time time1, DateColumn date2, TimeColumn time2, Number refColumnForDaysResult,
         Number refColumnForTimeResult)
        {
            return DifDateTime(date1.Value ?? date1.GetInsteadOfNull(), time1, date2.Value ?? date2.GetInsteadOfNull(), time2.Value ?? time2.GetInsteadOfNull(), refColumnForDaysResult,
                refColumnForTimeResult);
        }
        /*1001*/
        public Bool DifDateTime(DateColumn date1, Time time1, Date date2, TimeColumn time2, Number refColumnForDaysResult,
         Number refColumnForTimeResult)
        {
            return DifDateTime(date1.Value ?? date1.GetInsteadOfNull(), time1, date2, time2.Value ?? time2.GetInsteadOfNull(), refColumnForDaysResult,
                refColumnForTimeResult);
        }
        /*1010*/
        public Bool DifDateTime(DateColumn date1, Time time1, DateColumn date2, Time time2, Number refColumnForDaysResult,
         Number refColumnForTimeResult)
        {
            return DifDateTime(date1.Value ?? date1.GetInsteadOfNull(), time1, date2.Value ?? date2.GetInsteadOfNull(), time2, refColumnForDaysResult,
                refColumnForTimeResult);
        }
        /*1101*/
        public Bool DifDateTime(DateColumn date1, TimeColumn time1, Date date2, TimeColumn time2, Number refColumnForDaysResult,
         Number refColumnForTimeResult)
        {
            return DifDateTime(date1.Value ?? date1.GetInsteadOfNull(), time1.Value ?? time1.GetInsteadOfNull(), date2, time2.Value ?? time2.GetInsteadOfNull(), refColumnForDaysResult,
                refColumnForTimeResult);
        }
        /*1111*/
        public Bool DifDateTime(DateColumn date1, TimeColumn time1, DateColumn date2, TimeColumn time2, Number refColumnForDaysResult,
         Number refColumnForTimeResult)
        {
            return DifDateTime(date1.Value ?? date1.GetInsteadOfNull(), time1.Value ?? time1.GetInsteadOfNull(), date2.Value ?? date2.GetInsteadOfNull(), time2.Value ?? time2.GetInsteadOfNull(), refColumnForDaysResult,
                refColumnForTimeResult);
        }
        /*0110*/
        public Bool DifDateTime(Date date1, TimeColumn time1, DateColumn date2, Time time2, Number refColumnForDaysResult,
         Number refColumnForTimeResult)
        {
            return DifDateTime(date1, time1.Value ?? time1.GetInsteadOfNull(), date2.Value ?? date2.GetInsteadOfNull(), time2, refColumnForDaysResult,
                refColumnForTimeResult);
        }
        /*0101*/
        public Bool DifDateTime(Date date1, TimeColumn time1, Date date2, TimeColumn time2, Number refColumnForDaysResult,
         Number refColumnForTimeResult)
        {
            return DifDateTime(date1, time1.Value ?? time1.GetInsteadOfNull(), date2, time2.Value ?? time2.GetInsteadOfNull(), refColumnForDaysResult,
                refColumnForTimeResult);
        }
        /*0100*/
        public Bool DifDateTime(Date date1, TimeColumn time1, Date date2, Time time2, Number refColumnForDaysResult,
         Number refColumnForTimeResult)
        {
            return DifDateTime(date1, time1.Value ?? time1.GetInsteadOfNull(), date2, time2, refColumnForDaysResult,
                refColumnForTimeResult);
        }
        /*0010*/
        public Bool DifDateTime(Date date1, Time time1, DateColumn date2, Time time2, Number refColumnForDaysResult,
         Number refColumnForTimeResult)
        {
            return DifDateTime(date1, time1, date2.Value ?? date2.GetInsteadOfNull(), time2, refColumnForDaysResult,
                refColumnForTimeResult);
        }


        /*0000*/
        public Bool DifDateTime(Date date1, Time time1, Date date2, Time time2, Number refColumnForDaysResult, Number refColumnForTimeResult)
        {

            if (date1 == null)
                date1 = NullBehaviour.NullDate;
            if (time1 == null)
                time1 = NullBehaviour.NullTime;
            if (date2 == null)
                date2 = NullBehaviour.NullDate;
            if (time2 == null)
                time2 = NullBehaviour.NullTime;


            var days = GetColumnByIndex(refColumnForDaysResult) as Firefly.Box.Data.NumberColumn;
            if (days != null)
            {
                var x = date1 - date2;
                if (time1 < time2 && x > 0)
                    x -= 1;
                if (x != days.Value)
                    days.Value = x;

            }
            {
                var x = ToNumber(time1) - ToNumber(time2);
                var a = ToNumber(time2) <= 86400;
                var b = ToNumber(time1) <= 86400;
                if (date1 > date2 && time1 < time2 && ((a && b) || (!a && !b)))
                    x += 86400;
                var col = GetColumnByIndex(refColumnForTimeResult);
                {
                    var seconds = col as Firefly.Box.Data.NumberColumn;
                    if (seconds != null)
                        if (x != seconds.Value)
                            seconds.Value = x;
                }
                {
                    var seconds = col as Firefly.Box.Data.TimeColumn;
                    if (seconds != null)
                    {
                        var z = ToTime(x);
                        if (z != seconds.Value)
                            seconds.Value = z;
                    }
                }
            }
            return true;
        }

        public object VariantCreate(Number type, params object[] args)
        {
            if (type == 8192 + 17)
            {
                if (args.Length != 1)
                    throw new InvalidOperationException("Expected one arg in variant create of a byte array");
                {
                    var y = args[0] as byte[];
                    if (y != null)
                        return y;
                }
                {
                    var y = args[0] as Firefly.Box.Data.ByteArrayColumn;
                    if (y != null)
                        return y.Value;
                }
                {
                    var y = args[0] as Text;
                    if (y != null)
                        return TextColumn.ToByteArray(y);
                }
                {
                    var y = args[0] as string;
                    if (y != null)
                        return TextColumn.ToByteArray(y);
                }
                {
                    var y = args[0] as Firefly.Box.Data.TextColumn;
                    if (y != null)
                        return TextColumn.ToByteArray(y);
                }
                return null;
            }
            if (type == 12 || type == 5 | type == 8)
                return CastToObject(args[0]);
            else throw new InvalidOperationException("Unknown variant create type " + type);
        }
        static readonly ContextStatic<UserMethods> __instance = new ContextStatic<UserMethods>(() => new UserMethods());
        public static UserMethods Instance { get { __instance.Value.__currentContextDONOTUSEME = null; return __instance.Value; } }

        public static string FilesSeparator
        {
            get
            {
                if (UserSettings.Version10Compatible)
                    return "|";
                return ",";
            }
        }

        public static void CastColumn(ColumnBase column, IColumnSpecifier caster)
        {
            {
                var c = column as TypedColumnBase<Text>;
                if (c != null)
                {
                    caster.DoOnColumn(c);
                    return;
                }
            }
            {
                var c = column as TypedColumnBase<Number>;
                if (c != null)
                {
                    caster.DoOnColumn(c);
                    return;
                }
            }
            {
                var c = column as TypedColumnBase<Date>;
                if (c != null)
                {
                    caster.DoOnColumn(c);
                    return;
                }
            }
            {
                var c = column as TypedColumnBase<Time>;
                if (c != null)
                {
                    caster.DoOnColumn(c);
                    return;
                }
            }
            {
                var c = column as TypedColumnBase<Bool>;
                if (c != null)
                {
                    caster.DoOnColumn(c);
                    return;
                }
            }
            {
                var c = column as TypedColumnBase<byte[]>;
                if (c != null)
                {
                    caster.DoOnColumn(c);
                    return;
                }
            }
            caster.DoOnUnknownColumn(column);
        }
        public interface IColumnSpecifier
        {
            void DoOnColumn(TypedColumnBase<Text> column);
            void DoOnColumn(TypedColumnBase<Number> column);
            void DoOnColumn(TypedColumnBase<Date> column);
            void DoOnColumn(TypedColumnBase<Time> column);
            void DoOnColumn(TypedColumnBase<Bool> column);
            void DoOnColumn(TypedColumnBase<byte[]> column);

            void DoOnUnknownColumn(ColumnBase column);
        }

        public Bool LocateReset(Number generation)
        {
            if (ReferenceEquals(generation, null))
                return null;
            var t = GetTaskByGeneration(generation) as UIController;
            if (t == null)
                return false;
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(t,
                y =>
                {
                    var uic = y as AbstractUIController;
                    if (uic != null)
                    {
                        uic.UserMethodsLocateReset();
                    }
                });
            return true;
        }

        public Bool RangeReset(Number generation)
        {
            if (ReferenceEquals(generation, null))
                return null;
            var t = GetTaskByGeneration(generation) as UIController;
            if (t == null)
                return false;
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(t,
                y =>
                {
                    var uic = y as AbstractUIController;
                    if (uic != null)
                    {
                        uic.UserMethodsRangeReset();
                    }
                });
            return true;
        }

        public Bool RangeAdd(Number columnReference, object fromValue, object toValue)
        {
            if (ReferenceEquals(columnReference, null))
                return null;
            ITask t = null;
            var c = GetColumnByIndex(columnReference, y => t = y);
            if (t == null)
                return false;
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(t,
                y =>
                {
                    var uic = y as AbstractUIController;
                    if (uic != null)
                    {
                        CastColumn(c, new FilterColumnSpecifier(fromValue, toValue, uic.AddUserMethodRange));
                    }
                });
            return true;
        }

        public Bool RangeAdd(Number columnReference, object fromValue)
        {
            return RangeAdd(columnReference, fromValue, null);
        }
        public Bool RangeExpAdd(Number generation, Text expression)
        {
            if (ReferenceEquals(generation, null))
                return null;
            if (ReferenceEquals(expression, null))
                return null;
            Bool result = false;
            var t = GetTaskByGeneration(generation);
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(t, y =>
            {
                var uic = y as AbstractUIController;
                if (uic != null)
                {

                    var fc = new FilterCollection();
                    var envE = y.From as ENV.Data.Entity;
                    var done = false;
                    if (envE != null && envE.DataProvider is DynamicSQLSupportingDataProvider)
                    {
                        var e = new Utilities.ExpressionBasedSQLFilter(expression, y.db);
                        if (e.isValidForUseAsSqlExpression(uic))
                        {
                            fc.Add(e);
                            uic.AddUserMethodRange(fc);
                            done = true;
                        }
                    }
                    if (!done)
                    {
                        fc.Add(() => EvalStr(expression, false));
                        uic.AddUserMethodNonDbRange(fc);
                    }

                    result = true;
                }
            });
            return result;
        }

        public Bool LocateAdd(Number columnReference, object fromValue, object toValue)
        {
            if (ReferenceEquals(columnReference, null))
                return null;
            ITask t = null;
            var c = GetColumnByIndex(columnReference, y => t = y);
            if (t == null)
                return false;
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(t,
                y =>
                {
                    var uic = y as AbstractUIController;
                    if (uic != null)
                    {
                        CastColumn(c, new FilterColumnSpecifier(fromValue, toValue, uic.AddUserMethodLocate));
                    }
                });
            return true;
        }
        public Bool LocateAdd(Number columnReference, object fromValue)
        {
            return LocateAdd(columnReference, fromValue, null);
        }

        internal class FilterColumnSpecifier : IColumnSpecifier
        {
            object _fromValue;
            bool _hasFrom = true;
            bool _hasTo = true;
            object _toValue;
            Action<ColumnBase, FilterBase> _to;

            public FilterColumnSpecifier(object fromValue, object toValue, Action<ColumnBase, FilterBase> to)
            {
                _fromValue = fromValue;
                _toValue = toValue;
                _to = to;
                _hasFrom = _fromValue != null;
                _hasTo = _toValue != null;
                {
                    Text s;
                    if (Types.Text.TryCast(_fromValue, out s) && _fromValue != null)
                    {
                        _hasFrom = (s != Types.Text.Empty);
                    }
                }
                {
                    Text s;
                    if (Types.Text.TryCast(_toValue, out s) && _toValue != null)
                    {
                        _hasTo = (s != Types.Text.Empty);
                    }
                }
            }

            public void DoOnColumn(TypedColumnBase<Text> column)
            {
                Text from = null, to = null;
                if (_hasFrom)
                {
                    _hasFrom = Types.Text.TryCast(_fromValue, out from);
                }
                if (_hasTo)
                    _hasTo = Types.Text.TryCast(_toValue, out to);
                AddFilter(column, from, to);
            }
            void AddFilter<T>(TypedColumnBase<T> col, T fromValue, T toValue)
            {
                if (_hasFrom && _hasTo)
                {
                    if (fromValue != null && toValue != null && fromValue.Equals(toValue))
                        _to(col, col.IsEqualTo(fromValue));
                    else
                        _to(col, col.IsBetween(fromValue, toValue));
                }
                else
                    if (_hasFrom)
                    _to(col, col.IsGreaterOrEqualTo(fromValue));
                else if (_hasTo)
                    _to(col, col.IsLessOrEqualTo(toValue));
            }

            public void DoOnColumn(TypedColumnBase<Number> column)
            {
                Number from = null, to = null;
                if (_hasFrom)
                {
                    _hasFrom = Number.TryCast(_fromValue, out from);
                }
                if (_hasTo)
                    _hasTo = Number.TryCast(_toValue, out to);
                AddFilter(column, from, to);
            }

            public void DoOnColumn(TypedColumnBase<Date> column)
            {
                Date from = null, to = null;
                if (_hasFrom)
                {
                    _hasFrom = Types.Date.TryCast(_fromValue, out from);
                }
                if (_hasTo)
                    _hasTo = Types.Date.TryCast(_toValue, out to);
                AddFilter(column, from, to);
            }

            public void DoOnColumn(TypedColumnBase<Time> column)
            {
                Time from = null, to = null;
                if (_hasFrom)
                {
                    _hasFrom = Types.Time.TryCast(_fromValue, out from);
                }
                if (_hasTo)
                    _hasTo = Types.Time.TryCast(_toValue, out to);
                AddFilter(column, from, to);
            }

            public void DoOnColumn(TypedColumnBase<Bool> column)
            {
                Bool from = null, to = null;
                if (_hasFrom)
                {
                    _hasFrom = Bool.TryCast(_fromValue, out from);
                }
                if (_hasTo)
                    _hasTo = Bool.TryCast(_toValue, out to);
                AddFilter(column, from, to);
            }

            public void DoOnColumn(TypedColumnBase<byte[]> column)
            {
                byte[] from = null, to = null;
                if (_hasFrom)
                {
                    _hasFrom = _fromValue != null && (from = _fromValue as byte[]) != null;
                }
                if (_hasTo)
                    _hasTo = _toValue != null && (to = _toValue as byte[]) != null;
                AddFilter(column, from, to);
            }

            public void DoOnUnknownColumn(ColumnBase column)
            {
                throw new NotImplementedException();
            }
        }

        public Bool SortAdd(Number columnReference, Bool ascending)
        {
            if (ReferenceEquals(columnReference, null) || ReferenceEquals(columnReference, null))
                return null;
            ITask t = null;
            var c = GetColumnByIndex(columnReference, y => t = y);
            if (t != null)
            {
                ControllerBase.SendInstanceBasedOnTaskAndCallStack(t,
                    y =>
                    {
                        var uic = y as AbstractUIController;
                        if (uic != null)
                            uic.AddUserMethodsSort(c, ascending);
                    });
                return true;
            }
            return false;

        }

        public Bool SortReset(Number generation)
        {
            if (ReferenceEquals(generation, 0))
                return null;
            var t = GetTaskByGeneration(generation) as UIController;
            if (t == null)
                return false;
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(t,
                y =>
                {
                    var uic = y as AbstractUIController;
                    if (uic != null)
                        uic.ResetSort();
                });
            return true;
        }
        internal static object[] UseValuesInsteadOfColumns(object[] args)

        {
            var theArgs = new ArrayList();
            foreach (var arg in args)
            {
                var c = arg as ColumnBase;
                if (c != null)
                    theArgs.Add(c.Value);
                else
                    theArgs.Add(arg);
            }
            return theArgs.ToArray();
        }

        public object CallProg(Number programNumber, params object[] args)
        {
            return _application.AllPrograms.InternalRunByIndex(programNumber, UseValuesInsteadOfColumns(args));
        }
        [Obsolete("DO NOT USE, To use in a filter, please use CndRange of the UIControllerBase or BusinessProcessBase class")]
        public Number CndRange(Bool condition, Number value)
        {
            if (condition)
                return value;
            return Types.Number.Zero;
        }
        [Obsolete("DO NOT USE, To use in a filter, please use CndRange of the UIControllerBase or BusinessProcessBase class")]
        public Bool CndRange(Bool condition, Bool value)
        {
            if (condition)
                return value;
            return true;
        }
        [Obsolete("DO NOT USE, To use in a filter, please use CndRange of the UIControllerBase or BusinessProcessBase class")]
        public Text CndRange(Bool condition, Text value)
        {
            if (condition)
                return value;
            return Types.Text.Empty;
        }

        [Obsolete("DO NOT USE, To use in a filter, please use CndRange of the UIControllerBase or BusinessProcessBase class")]
        public Date CndRange(Bool condition, Date value)
        {
            if (condition)
                return value;
            return Types.Date.Empty;
        }

        public Number DbViewRowIdx(int generation)
        {
            var uic = GetTaskByGeneration(generation) as UIController;
            if (uic != null)
                return uic.CachedRowsInfo.Position + 1;
            return 0;
        }
        public MessageBoxDefaultButton TranslateDefaultButtonExpression(Number value)
        {
            if (value == 3)
                return MessageBoxDefaultButton.Button3;
            else if (value == 2)
                return MessageBoxDefaultButton.Button2;
            return MessageBoxDefaultButton.Button1;
        }
        #region Conversion methods

        public Number ToNumber(Date date)
        {
            if (date == null)
                return null;
            return (date - StartDate) + 693961;
        }
        public Number ToNumber(DateColumn column)
        {
            return ToNumber(column.Value);
        }

        public Number ToNumber(Time time)
        {
            if (time == null)
                return null;
            return time.TotalSeconds;
        }
        public Number ToNumber(TimeColumn column)
        {
            return ToNumber(column.Value);
        }

        public Number ToNumber(Number value)
        {
            return value;
        }

        /// <summary>
        /// This method is for migrated code only. It is used to cast an object that we know is a <see cref="Number"/>, to a <see cref="Number"/>.<br/>
        /// use <see cref="Number.Cast"/> and <see cref="Number.Parse(string)"/> instead.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Browsable(false)]
        public Number CastToNumber(object value)
        {
            if (_nullStrategy.ReturnValueIfNull(ref value, NullBehaviour.NullNumber))
                return (Number)value;

            Number result = 0;
            if (Number.TryCast(value, out result))
                return result;
            {
                var x = value as Date;
                if (x != null)
                    return ToNumber(x);
            }
            {
                var x = value as Firefly.Box.Data.DateColumn;
                if (x != null)
                    return ToNumber(x);
            }
            {
                var x = value as Time;
                if (x != null)
                    return ToNumber(x);
            }
            {
                var x = value as Firefly.Box.Data.TimeColumn;
                if (x != null)
                    return ToNumber(x);
            }
            {
                if (value is DateTime)

                    return ToNumber((DateTime)value);
            }
            {
                var x = value as Firefly.Box.Data.DateTimeColumn;
                if (x != null)
                    return ToNumber(x.Value);
            }
            {
                var x = value as ENV.DotnetColumn<DateTime>;
                if (x != null)
                    return ToNumber(x.Value);
            }
            return 0;
        }

        public Number NumberFromObject(object value)
        {
            Text t;
            if (Types.Text.TryCast(value, out t))
                return Number.Parse(t);
            Date d;
            if (Types.Date.TryCast(value, out d))
                return d - new Date(1899, 12, 30);
            return CastToNumber(value);
        }
        class ErrorText : Types.Text
        {
            ErrorText() : base("")
            { }
            public static ErrorText Instance = new ErrorText();
        }

        /// <summary>
        /// This method is for migrated code only. It is used to cast an object that we know is a <see cref="Types.Text"/>, to a <see cref="Types.Text"/>.<br/>
        /// use <see cref="Types.Text.Cast"/>  instead.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Text CastToText(object value)
        {
            var cb = value as ColumnBase;
            if (cb != null)
                value = cb.Value;
            if (_nullStrategy.ReturnValueIfNull(ref value, NullBehaviour.NullText))
                return (Text)value;
            Text result;
            if (Types.Text.TryCast(value, out result))
                return result;
            {


                if (value is char)
                    return value.ToString();
            }
            if (value is DateTime)
                return Types.Date.FromDateTime((DateTime)value).ToString();
            var bac = value as ByteArrayColumnValue;
            if (bac != null)
                return bac.StringValue;
            var ba = value as byte[];
            if (ba != null)
                return TextColumn.FromByteArray(ba);
            if (value.GetType().Assembly == typeof(Text).Assembly || value.GetType().Assembly == typeof(UserMethods).Assembly)
                return ErrorText.Instance;
            return value.ToString();
        }
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Text ToTextFromCom(object value)
        {
            if (_nullStrategy.ReturnValueIfNull(ref value, NullBehaviour.NullText))
                return (Text)value;
            Text result;
            if (Types.Text.TryCast(value, out result))
                return result;
            {


                if (value is char)
                    return value.ToString();
            }
            if (value is DateTime)
                return Types.Date.FromDateTime((DateTime)value).ToString();
            return value.ToString();
        }

        public Bool IsEmptyTextParam(Text paramName)
        {
            var x = GetParam(paramName);
            if (_nullStrategy.ReturnValueIfNull(ref x, NullBehaviour.NullBool))
                return (Bool)x;
            Text result;
            if (Types.Text.TryCast(x, out result))
            {
                return result == "";
            }
            return false;
        }
        /// <summary>
        /// This method is for migrated code only. It is used to cast an object that we know is a <see cref="byte"/> array, to a <see cref="byte"/> array.<br/>

        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public byte[] CastToByteArray(object value)
        {
            var x = value as Firefly.Box.Data.ByteArrayColumn;
            if (x != null)
                return x;
            if (value is byte[])
                return (byte[])value;
            Text result;
            if (Types.Text.TryCast(value, out result))
            {
                return TextColumn.ToByteArray(result);
            }
            return ToTypeArray<byte>(value);
        }
        /// <summary>
        /// This method is for migrated code only. It is used to cast an object that we know is a <see cref="Number"/> array, to a <see cref="Number"/> array.<br/>

        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Number[] CastToNumberArray(object value)
        {
            return ToTypeArray<Number>(value);
        }
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Number[][] CastToNumberArrayArray(object value)
        {
            return ToTypeArray<Number[]>(value);
        }
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Text[][][] CastToTextArrayArrayArray(object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is for migrated code only. It is used to cast an object that we know is a <see cref="Types.Text"/> array, to a <see cref="Types.Text"/> array.<br/>

        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Text[] CastToTextArray(object value)
        {
            var r = ToTypeArray<Text>(value);
            if (r != null) return r;
            var x = value as object[];
            if (x != null)
                return Array.ConvertAll(x, input => CastToText(input));
            return null;
        }
        public byte[][] TextArrayToByteArrayArray(Types.Text[] value)
        {
            return Array.ConvertAll(value, input => CastToByteArray(input));
        }
        [NotYetImplemented]
        public Text[][] CastToTextArrayArray(object value)
        {
            var r = ToTypeArray<Text[]>(value);
            if (r != null)
                return r;
            var x = value as object[,];
            if (x != null)
            {
                var b0 = x.GetUpperBound(0);
                // need to support arrays that don't start from 0 - you can only create these kind of arrays in com
                //so there is no unit test for it - it was tested with an excel com file issue W10226
                var b0l = x.GetLowerBound(0);
                r = new Text[b0 - b0l + 1][];
                var b1 = x.GetUpperBound(1);
                var b1l = x.GetLowerBound(1);
                for (int i = b0l; i <= b0; i++)
                {
                    var r1 = new Text[b1 - b1l + 1];
                    for (int j = b1l; j <= b1; j++)
                    {
                        r1[j - b1l] = CastToText(x[i, j]);
                    }
                    r[i - b0l] = r1;
                }
                return r;
            }
            return null;
        }

        /// <summary>
        /// This method is for migrated code only. It is used to cast an object that we know is a <see cref="Date"/> array, to a <see cref="Date"/> array.<br/>
        /// use <see cref="Number.Cast"/> and <see cref="Number.Parse(string)"/> instead.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Date[] CastToDateArray(object value)
        {
            return ToTypeArray<Date>(value);
        }
        /// <summary>
        /// This method is for migrated code only. It is used to cast an object that we know is a <see cref="Time"/> array, to a <see cref="Time"/> array.<br/>
        /// use <see cref="Number.Cast"/> and <see cref="Number.Parse(string)"/> instead.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Time[] CastToTimeArray(object value)
        {
            return ToTypeArray<Time>(value);
        }
        /// <summary>
        /// This method is for migrated code only. It is used to cast an object that we know is a <see cref="Bool"/> array, to a <see cref="Bool "/> array.<br/>
        /// use <see cref="Number.Cast"/> and <see cref="Number.Parse(string)"/> instead.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Bool[] CastToBoolArray(object value)
        {
            return ToTypeArray<Bool>(value);
        }

        static T[] ToTypeArray<T>(object value)
        {
            var x = value as T[];
            if (x != null)
                return x;
            var y = value as Firefly.Box.Data.ArrayColumn<T>;
            if (y != null)
                return y.Value;
            return null;
        }
        /// <summary>
        /// This method is for migrated code only. It is used to cast an object that we know is a <see cref="Bool"/>, to a <see cref="Bool"/>.<br/>
        /// use <see cref="Bool.Cast"/> instead.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Bool CastToBool(object value)
        {
            if (_nullStrategy.ReturnValueIfNull(ref value, NullBehaviour.NullBool))
                return (Bool)value;
            Bool result;
            if (Bool.TryCast(value, out result))
                return result;
            Text t;
            if (Types.Text.TryCast(value, out t))
                return true;
            if (value.GetType().IsCOMObject)
                return true;

            return CastToNumber(value) > Number.Zero;

        }
        public double ToDouble(Number value)
        {
            return value;
        }
        public decimal ToDecimal(Number value)
        {
            return value;
        }
        public double ToDouble(NumberColumn value)
        {
            return value;
        }
        public double ToDouble(int value)
        {
            return value;
        }
        public double ToDouble(double value)
        {
            return value;
        }
        public Single ToSingle(Number value)
        {
            return value;
        }
        public Single ToSingle(Time value)
        {
            return ToSingle(ToNumber(value));
        }
        public Single ToSingle(NumberColumn value)
        {
            return value;
        }
        public Single ToSingle(int value)
        {
            return value;
        }
        public Single ToSingle(double value)
        {
            return (Number)value;
        }

        public bool ToBool(Bool value)
        {
            return value;
        }


        public int ToInt(Number value)
        {
            if (ReferenceEquals(value, null))
                return 0;
            return value;
        }
        public byte ToByte(Number value)
        {
            if (ReferenceEquals(value, null))
                return 0;
            return value;
        }
        public int ToInt(Time value)
        {
            return ToInt(ToNumber(value));
        }
        public int ToInt(NumberColumn value)
        {
            return value;
        }
        public int ToInt(int value)
        {
            return value;
        }
        public uint ToUInt32(NumberColumn value)
        {
            return (uint)ToInt(value);
        }
        public uint ToUInt32(Number value)
        {
            return (uint)ToInt(value);
        }
        public Int16 ToInt16(Number value)
        {
            return (Int16)ToInt(value);
        }
        public Int16 ToInt16(NumberColumn value)
        {
            return (Int16)ToInt(value);
        }
        public Int16 ToInt16(int value)
        {
            return (Int16)ToInt(value);
        }
        public long ToInt64(Number value)
        {
            return (long)ToInt(value);
        }
        public long ToInt64(NumberColumn value)
        {
            return (long)ToInt(value);
        }
        public long ToInt64(int value)
        {
            return (long)ToInt(value);
        }
        public long ToInt64(long value)
        {
            return (long)ToInt(value);
        }
        public Int16 ToInt16(double value)
        {
            return (Int16)ToInt(value);
        }
        public short ToShort(Number value)
        {
            return (Int16)ToInt(value);
        }
        public Int16 ToShort(NumberColumn value)
        {
            return (Int16)ToInt(value);
        }
        public Int16 ToShort(int value)
        {
            return (Int16)ToInt(value);
        }
        public Int16 ToShort(double value)
        {
            return (Int16)ToInt(value);
        }


        public object ToObject(Time value)
        {
            return (TimeSpan)value;
        }

        public object ToObject(Date value)
        {
            return ToDateTime(value);
        }
        public object ToObject(DateColumn value)
        {
            if (IsNull(value))
                return Null();
            return ToDateTime(value);
        }

        public object ToObject(Data.ArrayColumn<Date> value)
        {
            if (value == null)
                return System.Reflection.Missing.Value;
            var result = new List<object>();
            foreach (var t in value.Value)
            {
                result.Add(ToObject(t));
            }
            return result.ToArray();
        }

        public object ToObject(Data.ArrayColumn<Number> value)
        {
            return ToObject(value.Value);
        }
        public object ToObject(Number[] value)
        {
            if (value == null)
                return System.Reflection.Missing.Value;
            var result = new List<object>();
            foreach (var t in value)
            {
                result.Add(ToObject(t));
            }
            return result.ToArray();
        }
        public object ToObject(Data.ArrayColumn<Number[]> value)
        {
            if (value == null || value.Value == null)
                return System.Reflection.Missing.Value;
            var result = new List<object>();
            foreach (var t in value.Value)
            {
                result.Add(ToObject(t));
            }
            return result.ToArray();
        }

        public object ToObject(Data.ArrayColumn<Bool> value)
        {
            if (value == null || value.Value == null)
                return System.Reflection.Missing.Value;
            var result = new List<object>();
            foreach (var t in value.Value)
            {
                result.Add(ToObject(t));
            }
            return result.ToArray();
        }
        public object ToObject(Data.ArrayColumn<Text> value)
        {
            if (value == null || value.Value == null)
                return System.Reflection.Missing.Value;
            return ToObject(value.Value);
        }
        public object ToObject(Text[] value)
        {
            if (value == null)
                return System.Reflection.Missing.Value;
            var result = new List<object>();
            foreach (var t in value)
            {
                result.Add(ToObject(t));
            }
            return result.ToArray();
        }
        public object ToObject(ArrayColumn<Text[]> value)
        {
            if (value == null)
                return System.Reflection.Missing.Value;
            var firstDimensionLength = value.Length;
            var secondDimensionLength = 0;
            foreach (var arr in value.Value)
            {
                if (arr.Length > secondDimensionLength)
                    secondDimensionLength = arr.Length;
            }
            var result = new object[firstDimensionLength, secondDimensionLength];
            for (var i = 0; i < firstDimensionLength; i++)
                for (var j = 0; j < value.Value[i].Length; j++)
                    result[i, j] = ToObject(value.Value[i][j]);

            return result;
        }
        public object ToObject(Bool value)
        {
            return (bool)value;
        }
        public object ToObject(Text value)
        {
            if (value == null)
                return null;
            return value.TrimEnd().ToString();
        }
        public object ToObject(ENV.Data.ByteArrayColumn value)
        {
            return ToObject(value.Value);
        }
        public object ToObject(byte[] value)
        {
            if (value == null)
                return null;

            try
            {
                using (var ms = new MemoryStream(value))
                {
                    return new BinaryFormatter().Deserialize(ms);
                }
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "");
                return value;
            }
        }
        public object ToObject(string value)
        {
            return ToObject((Text)value);
        }
        public object ToObject(bool value)
        {
            return value;
        }
        public object ToObject(int value)
        {
            return value;
        }
        public object ToObject(short value)
        {
            return value;
        }
        public object ToObject(Number value)
        {
            if (value == null)
                return System.Reflection.Missing.Value;
            var d = value.ToDecimal();
            if (!d.ToString(CultureInfo.InvariantCulture).Contains(".") && d > int.MinValue && d < int.MaxValue)
                return ToInt(value);//note that in xpa.NET it should be int64, we're currently decided to go with int for backward compatability with com calls
            else
            {
                var f = (float)d;
                if ((decimal)(double)f == d)
                    return f;
                return (double)d;
            }
        }
        public object ToObject(BoolColumn value)
        {
            if (value == null || value.Value == null)
                return System.Reflection.Missing.Value;
            return (bool)value;
        }
        public object ToObject(TextColumn value)
        {
            if (value == null || value.Value == null)
                return System.Reflection.Missing.Value;
            return value.TrimEnd().ToString();
        }
        public object ToObject(NumberColumn value)
        {
            if (value == null || value.Value == null)
                return System.Reflection.Missing.Value;
            return ToObject(value.Value);
        }

        public System.Drawing.Color ToColor(Number value)
        {
            return ColorTranslator.FromOle(value);
        }
        public System.Drawing.Color ToColor(NumberColumn value)
        {
            return ToColor(value.Value);
        }

        public Font ToFont(object fontFromCom)
        {
            var oleFont = (IFont)fontFromCom;
            try
            {
                if (oleFont != null)
                {
                    var f = Font.FromHfont(oleFont.GetHFont());

                    if (f.Unit != GraphicsUnit.Point)
                        f = new Font(f.Name, f.SizeInPoints, f.Style, GraphicsUnit.Point, f.GdiCharSet, f.GdiVerticalFont);

                    return f;
                }
            }
            catch
            {

            }
            return SystemFonts.DefaultFont;
        }

        public object ToComFont(Font f)
        {
            return c1.GetStdFontFromFont(f);
        }
        public object ToIFontDisp(object f)
        {
            if (f is ColumnBase)
                f = ((ColumnBase)f).Value;
            if (f is Font)
                return c1.GetStdIFontDispFromFont((Font)f);
            return c1.GetStdIFontDispFromFont(c1.GetFontFromStdFont(f));
        }

        class c1 : System.Windows.Forms.AxHost
        {
            public c1(string clsid)
                : base(clsid)
            {
            }

            public c1(string clsid, int flags)
                : base(clsid, flags)
            {
            }

            public static object GetStdFontFromFont(System.Drawing.Font f)
            {
                return GetIFontFromFont(f);
            }

            public static object GetStdIFontDispFromFont(System.Drawing.Font f)
            {
                return GetIFontDispFromFont(f);
            }

            public static Font GetFontFromStdFont(object f)
            {
                return GetFontFromIFont(f);
            }

            public static Font GetFontFromStdIFontDisp(object f)
            {
                return GetFontFromIFontDisp(f);
            }

        }

        [ComImport(), Guid("BEF6E002-A874-101A-8BBA-00AA00300CAB"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
        public interface IFont
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetName();

            void SetName([In, MarshalAs(UnmanagedType.BStr)] string pname);

            [return: MarshalAs(UnmanagedType.U8)]
            long GetSize();

            void SetSize([In, MarshalAs(UnmanagedType.U8)] long psize);

            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetBold();

            void SetBold([In, MarshalAs(UnmanagedType.Bool)] bool pbold);

            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetItalic();

            void SetItalic([In, MarshalAs(UnmanagedType.Bool)] bool pitalic);

            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetUnderline();

            void SetUnderline([In, MarshalAs(UnmanagedType.Bool)] bool punderline);

            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetStrikethrough();

            void SetStrikethrough([In, MarshalAs(UnmanagedType.Bool)] bool pstrikethrough);

            [return: MarshalAs(UnmanagedType.I2)]
            short GetWeight();

            void SetWeight([In, MarshalAs(UnmanagedType.I2)] short pweight);

            [return: MarshalAs(UnmanagedType.I2)]
            short GetCharset();

            void SetCharset([In, MarshalAs(UnmanagedType.I2)] short pcharset);

            IntPtr GetHFont();

            void Clone(out IFont ppfont);

            [System.Runtime.InteropServices.PreserveSig]
            int IsEqual([In, MarshalAs(UnmanagedType.Interface)] IFont pfontOther);

            void SetRatio(int cyLogical, int cyHimetric);

            void QueryTextMetrics(out IntPtr ptm);

            void AddRefHfont(IntPtr hFont);

            void ReleaseHfont(IntPtr hFont);

            void SetHdc(IntPtr hdc);
        }
        /// <summary>
        /// This method is used to convert an object, to it's matching type to be used in COM interop.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public object CastToObject(object o)
        {
            if (o == null)
                return Missing.Value;
            {
                Number result;
                if (Number.TryCast(o, out result))
                    return ToObject(result);
            }
            {
                Date result;
                if (Types.Date.TryCast(o, out result))
                    return ToObject(result);
            }
            {
                Bool result;
                if (Firefly.Box.Bool.TryCast(o, out result))
                    return ToObject(result);
            }
            {
                Text result;
                if (Types.Text.TryCast(o, out result))
                    return ToObject(result);
            }
            var bac = o as ByteArrayColumn;
            if (bac != null) return bac.Value;
            var cb = o as ColumnBase;
            if (!ReferenceEquals(cb, null) && ReferenceEquals(cb.Value, null))
                return System.Reflection.Missing.Value;
            return o;
        }
        /// <summary>
        /// This method is for migrated code only. It is used to cast an object that we know is a <see cref="Date"/>, to a <see cref="Date"/>.<br/>
        /// use <see cref="Date.Cast"/> and <see cref="Date.Parse(string,string)"/> instead.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Date CastToDate(object value)
        {

            if (_nullStrategy.ReturnValueIfNull(ref value, NullBehaviour.NullDate))
                return (Date)value;
            Date result;
            if (Types.Date.TryCast(value, out result))
                return result;
            Number n;
            if (Types.Number.TryCast(value, out n))
                return ToDate(n);
            {
                var t = value as Time;
                if (t != null)
                    return ToDate(t);
            }
            {
                var t = value as Firefly.Box.Data.TimeColumn;
                if (t != null)
                    return ToDate(t);
            }
            return Types.Date.Empty;
        }
        /// <summary>
        /// This method is for migrated code only. It is used to cast an object that we know is a <see cref="Time"/>, to a <see cref="Time"/>.<br/>
        /// use <see cref="Time.Cast"/> and <see cref="Time.Parse(string,string)"/> instead.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public Time CastToTime(object value)
        {
            if (_nullStrategy.ReturnValueIfNull(ref value, NullBehaviour.NullTime))
                return (Time)value;
            Time result;
            if (Types.Time.TryCast(value, out result))
                return result;
            Number n;
            if (Types.Number.TryCast(value, out n))
                return ToTime(n);
            {
                var t = value as Date;
                if (t != null)
                    return ToTime(t);
            }
            {
                var t = value as Firefly.Box.Data.DateColumn;
                if (t != null)
                    return ToTime(t.Value);
            }
            return Types.Time.StartOfDay;
        }

        public Time ToTime(Number value)
        {
            if (value == null)
                return null;

            return new Time().AddSeconds(value);
        }
        public Time ToTime(NumberColumn value)
        {
            if (value == null)
                return null;

            return ToTime(value.Value);
        }
        public Time ToTime(Firefly.Box.Data.NumberColumn value)
        {
            return ToTime((Number)value);
        }

        public Time ToTime(int value)
        {
            return ToTime((Number)value);
        }
        public Time ToTime(double value)
        {
            return ToTime((Number)value);
        }
        public Time ToTime(long value)
        {
            return ToTime((Number)value);
        }
        public Time ToTime(Date value)
        {
            return ToTime(ToNumber(value));
        }
        public Time ToTime(DateTime value)
        {
            return Types.Time.FromDateTime(value);
        }
        public Time ToTime(DateColumn value)
        {
            return ToTime(value.Value);
        }
        public static Time ToTime(Time value)
        {
            return value;
        }
        public static Date StartDate = new Date(1901, 1, 1);

        public Date ToDate(Number value)
        {
            if (value == null)
                return null;
            if (value == 0)
                return Types.Date.Empty;

            return StartDate + (long)value - (long)693961;
        }
        public Func<Date> ToDate(Func<Number> value)
        {
            return () => ToDate(value());
        }
        public Date ToDate(Firefly.Box.Data.NumberColumn value)
        {
            if (value == null)
                return null;
            return ToDate(value.Value);
        }

        public Date ToDate(NumberColumn value)
        {
            return ToDate(value.Value);
        }
        public Date ToDate(int value)
        {
            return ToDate(ToNumber(value));
        }
        public Date ToDate(Time value)
        {
            return ToDate(ToNumber(value));
        }
        public System.Drawing.Image ToImage(byte[] value)
        {
            return System.Drawing.Image.FromStream(new MemoryStream(value));
        }

        public byte[] ToByteArray(object value)
        {
            try
            {
                Text t;
                if (Types.Text.TryCast(value, out t))
                    return TextColumn.ToByteArray(t);
                var y = value as System.Drawing.Image;
                if (y != null)
                {
                    using (var resultStream = new MemoryStream())
                    {
                        y.Save(resultStream, System.Drawing.Imaging.ImageFormat.Bmp);
                        return resultStream.ToArray();
                    }
                }

                var x = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                using (var resultStream = new MemoryStream())
                {
                    x.Serialize(resultStream, value);
                    return resultStream.ToArray();
                }
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
                return null;
            }

        }
        public Date ToDate(TimeColumn value)
        {
            return ToDate(ToNumber(value));
        }
        public Date ToDate(DateColumn value)
        {
            return ToDate(value.Value);
        }
        public Date ToDate(Date value)
        {
            return value;
        }
        public Date ToDate(DateTime value)
        {
            return Types.Date.FromDateTime(value);
        }

        public IntPtr ToVariant(Text value)
        {
            return ToVariant(value.ToString());
        }

        public IntPtr ToVariant(Bool value)
        {
            return ToVariant((bool)value);
        }
        public IntPtr ToVariant(Number value)
        {
            return (IntPtr)(int)value;
        }

        IntPtr ToVariant(object value)
        {
            var ptr = Marshal.AllocHGlobal(16);
            Common.DisposeAfterTry(() => Marshal.FreeHGlobal(ptr));
            Marshal.GetNativeVariantForObject(value, ptr);
            return ptr;
        }

        public Bool Or(Bool a, Func<Bool> bExpression)
        {
            if (ReferenceEquals(a, null))
                return null;
            if (a)
                return true;
            var b = bExpression();
            if (ReferenceEquals(b, null))
                return null;
            return b;
        }

        public Bool Or(Bool a, Bool b)
        {
            if (ReferenceEquals(a, null))
                return null;
            if (a)
                return true;
            if (ReferenceEquals(b, null))
                return null;
            return b;
        }
        public Bool Or(Bool a, Bool b, params Bool[] moreBools)
        {
            var result = Or(a, b);
            foreach (var item in moreBools)
            {
                result = Or(result, item);
            }
            return result;
        }
        public Bool Or(Bool a, Func<Bool> b, params Func<Bool>[] moreBools)
        {
            var result = Or(a, b);
            foreach (var item in moreBools)
            {
                result = Or(result, item);
            }
            return result;
        }
        public Bool And(Bool a, Bool b)
        {
            if (ReferenceEquals(a, null))
            {
                return _nullStrategy.AndWithLeftNull();
            }
            if (!a)
                return false;
            if (ReferenceEquals(b, null))
                return null;
            return b;

        }
        public Bool And(Bool a, Func<Bool> bExpression)
        {
            if (ReferenceEquals(a, null))
                return null;
            if (!a)
                return false;
            var b = bExpression();
            if (ReferenceEquals(b, null))
                return null;
            return b;

        }
        public Bool And(Bool a, Bool b, params Bool[] moreBools)
        {
            var result = And(a, b);
            foreach (var item in moreBools)
            {
                result = And(result, item);
            }
            return result;
        }
        public Bool And(Bool a, Func<Bool> b, params Func<Bool>[] moreBools)
        {
            var result = And(a, b);
            foreach (var item in moreBools)
            {
                result = And(result, item);
            }
            return result;
        }








        public Bool EqualsUntyped(object a, object b)
        {
            return _nullStrategy.EqualsUntyped(a, b);

        }

        public Bool Equals(object a, object b)
        {
            if (a is ErrorText || b is ErrorText)
                return false;
            return _nullStrategy.Equals(a, b);

        }










        #endregion

        public static bool InheritsFrom(Type child, Type parent)
        {

            parent = ResolveGenericTypeDefinition(parent);

            var currentChild = child.IsGenericType
                                   ? child.GetGenericTypeDefinition()
                                   : child;

            while (currentChild != typeof(object))
            {
                if (parent == currentChild || HasAnyInterfaces(parent, currentChild))
                    return true;

                currentChild = currentChild.BaseType != null
                               && currentChild.BaseType.IsGenericType
                                   ? currentChild.BaseType.GetGenericTypeDefinition()
                                   : currentChild.BaseType;

                if (currentChild == null)
                    return false;
            }
            return false;
        }

        private static bool HasAnyInterfaces(Type parent, Type child)
        {
            return child.GetInterfaces()
                .Any(childInterface =>
                {
                    var currentInterface = childInterface.IsGenericType
                        ? childInterface.GetGenericTypeDefinition()
                        : childInterface;

                    return currentInterface == parent;
                });
        }

        private static Type ResolveGenericTypeDefinition(Type parent)
        {
            var shouldUseGenericType = true;
            if (parent.IsGenericType && parent.GetGenericTypeDefinition() != parent)
                shouldUseGenericType = false;

            if (parent.IsGenericType && shouldUseGenericType)
                parent = parent.GetGenericTypeDefinition();
            return parent;
        }

        public Text WsSetIdentity(Text user, Text password)
        {
            WebServices.WebService.Username.Value = user;
            WebServices.WebService.Password.Value = password;
            return Types.Text.Empty;
        }

        public bool SetContextFocus(Text contextName)
        {
            if (string.IsNullOrEmpty(contextName)) return false;
            contextName = contextName.TrimEnd();
            foreach (var c in Context.ActiveContexts)
            {
                if (GetContextName(c) == contextName)
                {
                    if (c == Context.Current) return true;
                    var tasks = c.ActiveTasks;
                    for (int i = tasks.Count - 1; i >= 0; i--)
                    {
                        var uic = tasks[i] as UIController;
                        if (uic != null && uic.View != null)
                        {
                            Context.Current.Suspend(0);
                            var topMostForm = Common.ContextTopMostForm;
                            c.BeginInvoke(() =>
                            {
                                Common.RunOnContextTopMostForm(
                                    f =>
                                    {
                                        if (f == topMostForm) return;
                                        f.Activate();
                                        if (f.WindowState == FormWindowState.Minimized)
                                            f.WindowState = FormWindowState.Normal;
                                    });
                            });
                            uic.View.TryFocus(() => { });
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool CtxClose(Number contextIdToClose)
        {
            return CtxClose(contextIdToClose, false);
        }
        public bool CtxClose(Number contextIdToClose, Bool wait)
        {
            var result = false;
            DoOnContext(contextIdToClose,
                c =>
                {
                    if (c == Context.Current) return;
                    var e = new System.Threading.ManualResetEvent(!wait);
                    var succeeded = false;
                    c.CloseAllTasksAndRun(
                        () =>
                        {
                            succeeded = true;
                            e.Set();
                        });
                    e.WaitOne();
                    result = succeeded;
                });
            return false;
        }

        void DoOnContext(Number contextId, Action<Context> doThis)
        {
            foreach (var c in Context.ActiveContexts)
            {
                if (contextId == GetContextId(c))
                {
                    doThis(c);
                    return;
                }
            }
        }

        [NotYetImplemented]
        public Text ZIMERead(Number generation)
        {
            return "";
        }
        [NotYetImplemented]
        public Text Han(Text str)
        {
            if (ReferenceEquals(str, null))
                return null;
            return JapaneseMethods.Han(str);
        }

        [NotYetImplemented]
        public Text Zen(Text str)
        {
            if (ReferenceEquals(str, null))
                return null;
            return JapaneseMethods.Zen(str);

        }
        public Number JYear(Date date)
        {
            if (Types.Date.IsNullOrEmpty(date))
                return 0;
            return new JapaneseCalendar().GetYear(date.ToDateTime());
        }
        public Text JGengo(Date date, Number i)
        {
            if (Types.Date.IsNullOrEmpty(date))
                return "";
            if (i == 1)
                return date.ToString("J");
            else if (i == 2)
                return date.ToString("JJ");
            return date.ToString("JJJJ");
        }
        public Text JCDOW(Date date)
        {
            if (Types.Date.IsNullOrEmpty(date))
                return "";
            var c = new JapaneseCalendar();
            var ci = new CultureInfo("ja-JP");
            ci.DateTimeFormat.Calendar = c;
            return ci.DateTimeFormat.GetDayName(date.DayOfWeek);
        }

        public Bool Text()
        {
            return UserSettings.DoNotDisplayUI;
        }

        public Number CaretPosGet()
        {
            var task = GetTaskByGeneration(0) as UIController;
            if (task != null)
            {
                var form = task.View;
                var focused = form != null ? (form is ENV.UI.Form ? ((ENV.UI.Form)form).FocusedControl : form.FocusedControl) : null;
                if (focused != null)
                {

                    var tb = focused as Firefly.Box.UI.TextBox;

                    if (tb != null)
                    {
                        var result = 0;
                        Context.Current.InvokeUICommand(() => result = tb.SelectionStart + 1);
                        return result;
                    }
                }
            }
            return 0;
        }

        public Text CtxStat(Number contextId)
        {
            var result = "I";
            DoOnContext(contextId, context => result = context.Busy ? "E" : "P");
            return result;
        }

        [NotYetImplemented]
        public Bool ClientCertificateAdd(Text a, Text b)
        {
            return false;
        }
        [NotYetImplemented]
        public bool ClientCertificateDiscard(Text text)
        {
            return false;
        }

        [NotYetImplemented]
        public Bool IsMobileClient()
        {
            return false;
        }
        [NotYetImplemented]
        public Bool ClientSessionSet(Text environmentKey, Bool value)
        {
            return false;
        }
        [NotYetImplemented]
        public Text ClientImageCapture(Number source, Number width, Number height, Number quality, Bool allowCrop)
        {
            return "";
        }
        [NotYetImplemented]
        public object ClientNativeCodeExecute(Text moduleName, Text argumentTypes, params object[] args)
        {
            return null;
        }

        public Bool BrowserSetContent(Text controlName, Text value)
        {
            var t = GetTaskByGeneration(0);
            if (t != null && t.View != null)
            {
                var form = t.View as ENV.UI.Form;
                if (form != null)
                {
                    var control = FindControlByTag(form, controlName.Trim()) as Firefly.Box.UI.WebBrowser;
                    if (control != null)
                    {
                        Context.Current.InvokeUICommand(() =>
                            control.DocumentText = value);
                        return true;
                    }
                }
            }
            return false;
        }

        [NotYetImplemented]
        public Text BrowserGetContent(Text controlName)
        {
            var t = GetTaskByGeneration(0);
            if (t != null && t.View != null)
            {
                var form = t.View as ENV.UI.Form;
                if (form != null)
                {
                    var control = FindControlByTag(form, controlName.Trim()) as Firefly.Box.UI.WebBrowser;
                    if (control != null)
                    {
                        var content = "";
                        Context.Current.InvokeUICommand(() => content =  control.DocumentText);
                        return content;
                    }
                }
            }
            return "";
        }



        [NotYetImplemented]
        public Text MailMsgReplyTo(Number messageCounter)
        {
            return "";
        }

        [NotYetImplemented]
        public byte[] WsProviderAttachmentGet(Number index)
        {
            return new byte[0];
        }
        /*     [NotYetImplemented]
             public byte[] WsProviderAttachmentGet(Text key)
             {
                 return new byte[0];
             }*/
        [NotYetImplemented]
        public Bool WsProviderAttachmentAdd(byte[] atachment)
        {
            return false;
        }
        public Text UnicodeFromANSI(Text source, Number codePage)
        {
            if (source == null || codePage == null)
                return null;
            try
            {
                if (codePage == 0)
                    return source;
                else
                    return LocalizationInfo.Current.InnerEncoding.GetString(
                        System.Text.Encoding.GetEncoding(codePage).GetBytes(source.ToString().ToCharArray()));

            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }
        public Text UnicodeFromANSI(ByteArrayColumn source, Number codePage)
        {
            if (source == null || codePage == null)
                return null;
            try
            {
                if (codePage == 0)
                    return source;
                else
                    return
                        System.Text.Encoding.GetEncoding(codePage).GetString(source.Value);

            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }

        public Text UnicodeFromANSI(byte[] source, Number codePage)
        {
            if (source == null || codePage == null)
                return null;
            try
            {
                if (codePage == 0)
                    return LocalizationInfo.Current.InnerEncoding.GetString(source);
                else
                    return Encoding.GetEncoding(codePage).GetString(source);

            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e, "");
                return "";
            }
        }


        internal static string GetControllerName(ITask task)
        {
            if (task.Title != "Unnamed Task" && !string.IsNullOrEmpty(task.Title))
                return task.Title;
            string result = "";
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(
                task, y => result = y.GetType().Name);
            if (!string.IsNullOrEmpty(result))
                return result;
            if (task.View != null)
                return task.View.Text;
            return task.Title;
        }


        public Number TaskInstance(Number generation)
        {
            if (generation == null)
                return null;
            Number result = 0;
            var t = GetTaskByGeneration(generation);
            ControllerBase.SendInstanceBasedOnTaskAndCallStack(t, cb => result = cb._instanceId);
            return result;

        }


        public Text TaskTypeGet(Number generation)
        {
            var t = GetTaskByGeneration(generation);
            if (t is Firefly.Box.UIController)
                return "O";
            if (t is ModuleController)
                return "M";

            return "B";
        }

        [NotYetImplemented]
        public Text ClientOSEnvGet(Text environmentVariable)
        {
            return OSEnvGet(environmentVariable);
        }

        [NotYetImplemented]
        public Text GetGUID(int i)
        {
            return "";
        }

        public Bool DNExceptionOccurred()
        {
            return _dotNetException.Value != null;
        }

        public Bool ColorSet(Number colorIndex, Text forecolor, Text backcolor)
        {
            try
            {
                return ColorFile.Set(colorIndex, forecolor, backcolor);
            }
            catch
            {
                return false;
            }
        }
        public Bool FontSet(Number index, Text fontName, Number size, Number scriptCode, Number orientation, Bool bold, Bool italic, Bool strike, Bool underline)
        {
            try
            {
                return FontFile.Set(index, fontName, size, scriptCode, orientation, bold, italic, strike, underline);
            }
            catch
            {
                return false;
            }
        }

        public Exception DNException()
        {
            return _dotNetException.Value;
        }

        static ContextStatic<Exception> _dotNetException = new ContextStatic<Exception>(() => null);
        internal static void SetLastDotnetError(Exception exception)
        {
            _dotNetException.Value = exception;
        }

        internal static object[] ByteArrayToTypedArray(byte[] bytes)
        {
            if (bytes == null)
                return null;
            try
            {
                var ms = new MemoryStream(bytes);
                using (var sr = new BinaryReader(ms))
                {
                    Func<string, string> readToComma =
                        s =>
                        {
                            var r = "";
                            int i;
                            while ((i = sr.Read()) != 44)
                                r += (char)i;
                            if (!string.IsNullOrEmpty(s))
                                r.ShouldBe(s);
                            return r;
                        };

                    readToComma("MGBTMGVEC");
                    readToComma("");
                    readToComma("1");
                    var attr = readToComma("");

                    if (attr == "O")
                    {
                        for (int i = 0; i < 5; i++)
                            readToComma("");
                        var arrLength = int.Parse(readToComma(""));
                        for (int i = 0; i < 4; i++)
                            readToComma("");
                        var result = new List<object>();
                        for (int j = 0; j < arrLength; j++)
                        {
                            var l = int.Parse(readToComma(""));
                            for (int i = 0; i < 5; i++)
                                readToComma("");
                            var c = sr.ReadBytes(l);
                            sr.Read();
                            result.Add(c);
                        }
                        return result.ToArray();
                    }
                    else
                    {
                        var cellSize = int.Parse(readToComma(""));
                        readToComma("");
                        readToComma("0");
                        readToComma("0");
                        readToComma(cellSize.ToString());
                        var arrLength = int.Parse(readToComma(""));
                        readToComma("0");
                        var result = new List<object>();
                        for (int i = 0; i < arrLength; i++)
                        {
                            var c = sr.ReadChars(cellSize);
                            var s = new string(c);
                            result.Add(attr == "A" ? (object)new Text(s) :
                                attr == "B" ? (object)(Bool)(s == "true ") : Number.Parse(s));
                        }
                        return result.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                ErrorLog.WriteToLogFile(e);
                return null;
            }
        }

        internal static byte[] TypedArrayToByteArray<T>(T[] array, ColumnBase column)
        {
            if (array == null)
                return null;

            var sb = new StringBuilder();
            sb.AppendFormat("MGBTMGVEC,{1},1,{0},", typeof(T) == typeof(Text) ? "A" :
                typeof(T) == typeof(Bool) ? "B" : typeof(T) == typeof(byte[]) ? "O" :
                "N", UserSettings.Version10Compatible ? "5" : "1");

            if (typeof(T) == typeof(byte[]))
            {
                sb.AppendFormat("28,{0},1,1,28,{1},1,0,{2},{3},", new string(' ', 28), array.Length, new string(' ', 28 * array.Length), new string(' ', array.Length));

                var ms = new MemoryStream();
                using (var sw = new BinaryWriter(ms, Encoding.ASCII))
                {
                    sw.Write(new ByteArrayColumn() { ContentType = ByteArrayColumnContentType.Ansi }.FromString(sb.ToString()));
                    for (int i = 0; i < array.Length; i++)
                    {
                        var ba = array[i] as byte[];
                        sw.Write(ba.Length.ToString().ToCharArray());
                        sw.Write(",0,0, , ,1,".ToCharArray());
                        sw.Write(ba);
                        sw.Write(';');
                    }
                    sw.Write("MGBT".ToCharArray());
                }
                return ms.ToArray();
            }

            var cellSize = 0;
            if (column != null && column.Format != null)
                cellSize = column.FormatInfo.MaxLength;
            if (cellSize <= 0)
                cellSize = 100;
            if (typeof(T) == typeof(Bool))
                cellSize = 5;
            sb.AppendFormat("{0},", cellSize);
            sb.Append(new string(' ', cellSize));
            sb.AppendFormat(",0,0,{0},{1},0,", cellSize, array.Length);

            for (int i = 0; i < array.Length; i++)
                sb.Append(array[i].ToString().PadRight(cellSize));

            sb.AppendFormat(",{0},MGBT", new String('\0', array.Length));

            return new ByteArrayColumn() { ContentType = ByteArrayColumnContentType.Ansi }.FromString(sb.ToString());
        }
        [NotYetImplemented]
        public Number ServerLastAccessStatus()
        {
            return 0;
        }




        public void DoEndOfTaskCleanup()
        {
            ReleaseJavaObjects();
        }

        public Number FromColor(System.Drawing.Color c)
        {
            return ColorTranslator.ToOle(c);
        }


        public Text CreatePublicNameUrl(Text publicName, params object[] args)
        {
            using (var sw = new StringWriter())
            {
                sw.Write(IniGet("InternetDispatcherPath"));
                sw.Write("?PRGNAME=");
                sw.Write(publicName.TrimEnd().Replace(" ", "%20"));
                if (args.Length > 0)
                {
                    bool first = true;
                    sw.Write("&ARGUMENTS=");
                    foreach (var a in args)
                    {
                        if (first)
                            first = false;
                        else
                            sw.Write(",");
                        Number n;
                        if (Number.TryCast(a, out n))
                        {
                            sw.Write("-N" + n.ToString());
                        }
                        else
                        {
                            sw.Write("-A" + a.ToString().TrimEnd().Replace(" ", "%20"));
                        }
                    }

                }
                return sw.ToString();
            }
        }


    }
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class NotYetImplementedAttribute : Attribute
    {
    }
}