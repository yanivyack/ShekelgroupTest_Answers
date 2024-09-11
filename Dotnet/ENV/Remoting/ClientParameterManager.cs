using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firefly.Box;
using Firefly.Box.Advanced;
using Firefly.Box.Data;
using Firefly.Box.Data.Advanced;

namespace ENV.Remoting
{
    public class ClientParameterManager : ResultConsumer
    {
        List<ColumnBase> _columns = new List<ColumnBase>();
        public bool DoNotTrim { get; set; }
        public object Pack(object o)
        {
            if (o == null)
                return null;
            if (o.GetType().IsSerializable)
                return o;
            {
                var t = o as Text;
                if (t != null)
                {
                    if (!DoNotTrim)
                        t = t.TrimEnd();
                    return new myText(t);
                }
            }
            {
                var t = o as Number;
                if (t != null)
                    return new myNumber(t);
            }
            {
                var t = o as Date;
                if (t != null)
                    return new myDate(t);
            }
            {
                var t = o as Time;
                if (t != null)
                    return new myTime(t);
            }
            {
                var t = o as Bool;
                if (t != null)
                    return new myBool(t);
            }
            {
                var t = o as NumberColumn;
                if (t != null)
                {
                    _columns.Add(t);
                    return new WrappedNumberColumn(_columns.Count - 1, Pack(t.Value));
                }
            }
            {
                var t = o as TextColumn;
                if (t != null)
                {
                    _columns.Add(t);
                    return new WrappedTextColumn(_columns.Count - 1, Pack(t.Value));
                }
            }
            {
                var t = o as DateColumn;
                if (t != null)
                {
                    _columns.Add(t);
                    return new WrappedDateColumn(_columns.Count - 1, Pack(t.Value));
                }
            }
            {
                var t = o as TimeColumn;
                if (t != null)
                {
                    _columns.Add(t);
                    return new WrappedTimeColumn(_columns.Count - 1, Pack(t.Value));
                }
            }
            {
                var t = o as BoolColumn;
                if (t != null)
                {
                    _columns.Add(t);
                    return new WrappedBoolColumn(_columns.Count - 1, Pack(t.Value));
                }
            }
            {
                var t = o as ByteArrayColumn;
                if (t != null)
                {
                    _columns.Add(t);
                    return new WrappedByteArrayColumn(_columns.Count - 1, Pack(t.Value));
                }
            }



            throw new Exception("Unable to pack type" + o.GetType());
        }
        [Serializable]
        class WrappedNumberColumn : SerializedByMe
        {

            int _index;
            object _value;

            public WrappedNumberColumn(int index, object value)
            {
                _index = index;
                _value = value;
            }

            public object GetValue(IUnPacker unpacker)
            {
                var nc = new NumberColumn { AllowNull = true, Value = (Number)unpacker.UnPack(_value) };
                unpacker.AddColumn(nc, _index);
                return nc;
            }
        }
        [Serializable]
        class WrappedTextColumn : SerializedByMe
        {

            int _index;
            object _value;

            public WrappedTextColumn(int index, object value)
            {
                _index = index;
                _value = value;
            }

            public object GetValue(IUnPacker unpacker)
            {
                var nc = new TextColumn { AllowNull = true, Value = (Text)unpacker.UnPack(_value) };
                unpacker.AddColumn(nc, _index);
                return nc;
            }
        }
        [Serializable]
        class WrappedDateColumn : SerializedByMe
        {

            int _index;
            object _value;

            public WrappedDateColumn(int index, object value)
            {
                _index = index;
                _value = value;
            }

            public object GetValue(IUnPacker unpacker)
            {
                var nc = new DateColumn { AllowNull = true, Value = (Date)unpacker.UnPack(_value) };
                unpacker.AddColumn(nc, _index);
                return nc;
            }
        }
        internal void ReportColumnResultToConsole()
        {
            if (_columns.Count == 0)
                return;
            Console.WriteLine("Arguments :");
            Console.WriteLine("=========== ");
            for (int i = 0; i < _columns.Count; i++)
            {
                Console.Write("#");
                Console.Write((i + 1).ToString().PadRight(3));
                Console.Write(!_updatedColumns.Contains(i) ? "(Not modified) : " : "               : ");
                var c = _columns[i];
                Console.Write(c is TextColumn ? "ALPHA  " : c is NumberColumn ? "NUMERIC" : c is BoolColumn ? "LOGICAL " : "UNKNOWN");
                Console.Write("  : ");
                Console.WriteLine(c.Value.ToString().TrimEnd(' '));
            }
        }
        [Serializable]
        class WrappedTimeColumn : SerializedByMe
        {

            int _index;
            object _value;

            public WrappedTimeColumn(int index, object value)
            {
                _index = index;
                _value = value;
            }

            public object GetValue(IUnPacker unpacker)
            {
                var nc = new TimeColumn { AllowNull = true, Value = (Time)unpacker.UnPack(_value) };
                unpacker.AddColumn(nc, _index);
                return nc;
            }
        }
        [Serializable]
        class WrappedBoolColumn : SerializedByMe
        {

            int _index;
            object _value;

            public WrappedBoolColumn(int index, object value)
            {
                _index = index;
                _value = value;
            }

            public object GetValue(IUnPacker unpacker)
            {
                var nc = new BoolColumn { AllowNull = true, Value = (Bool)unpacker.UnPack(_value) };
                unpacker.AddColumn(nc, _index);
                return nc;
            }
        }
        [Serializable]
        class WrappedByteArrayColumn : SerializedByMe
        {

            int _index;
            object _value;

            public WrappedByteArrayColumn(int index, object value)
            {
                _index = index;
                _value = value;
            }

            public object GetValue(IUnPacker unpacker)
            {
                var nc = new ByteArrayColumn { AllowNull = true, Value = (byte[])unpacker.UnPack(_value) };
                unpacker.AddColumn(nc, _index);
                return nc;
            }
        }

        [Serializable]
        class myText : SerializedByMe
        {
            string _value;

            public myText(string value)
            {
                _value = value;
            }

            public object GetValue(IUnPacker unpacker)
            {
                return (Text)_value;
            }
        }
        [Serializable]
        class myNumber : SerializedByMe
        {
            decimal _value;

            public myNumber(decimal value)
            {
                _value = value;
            }

            public object GetValue(IUnPacker unpacker)
            {
                return (Number)_value;
            }
        }
        [Serializable]
        class myDate : SerializedByMe
        {
            long _value;

            public myDate(Date value)
            {
                _value = ENV.UserMethods.Instance.ToNumber(value);
            }

            public object GetValue(IUnPacker unpacker)
            {
                return ENV.UserMethods.Instance.ToDate(_value);
            }
        }
        [Serializable]
        class myTime : SerializedByMe
        {
            long _value;

            public myTime(Time value)
            {
                _value = ENV.UserMethods.Instance.ToNumber(value);
            }

            public object GetValue(IUnPacker unpacker)
            {
                return ENV.UserMethods.Instance.ToTime(_value);
            }
        }
        [Serializable]
        class myBool : SerializedByMe
        {
            bool _value;

            public myBool(Bool value)
            {
                _value = value;
            }

            public object GetValue(IUnPacker unpacker)
            {
                return (Bool)_value;
            }
        }
        HashSet<int> _updatedColumns = new HashSet<int>();
        public void Set(int id, object value)
        {
            _columns[id].Value = value;
            _updatedColumns.Add(id);
        }

        public object _returnValue;
        public void SetReturnValue(object value)
        {
            _returnValue = value;
        }
    }
    interface SerializedByMe
    {
        object GetValue(IUnPacker unpacker);
    }

    interface IUnPacker
    {
        void AddColumn(ColumnBase column, int id);
        object UnPack(object o);
    }
    public interface ServerParameterByRefResultInformation
    {
        void ApplyTo(ResultConsumer client);
    }
    public interface ResultConsumer
    {
        void Set(int id, object value);
        void SetReturnValue(object value);
    }

    public class ServerParameterManager : IUnPacker
    {
        class ColumnInProcess
        {
            ColumnBase _column;
            int _id;
            object _originalValue;

            public ColumnInProcess(ColumnBase column, int id)
            {
                _column = column;
                _id = id;
                _originalValue = _column.Value;
            }

            public void AddTo(MyParameterByRefResultInformation information)
            {
                if (Comparer.Compare(_originalValue, _column.Value) != 0)
                    information.Set(_id, _column.Value);
            }
        }

        List<ColumnInProcess> _columns = new List<ColumnInProcess>();
        public object UnPack(object o)
        {
            var z = o as SerializedByMe;
            if (z != null)
                return z.GetValue(this);
            return o;
        }

        void IUnPacker.AddColumn(ColumnBase column, int id)
        {
            _columns.Add(new ColumnInProcess(column, id));
        }
        [Serializable]
        class MyParameterByRefResultInformation : ResultConsumer, ServerParameterByRefResultInformation
        {
            Dictionary<int, object> _values = new Dictionary<int, object>();

            public void Set(int id, object value)
            {
                _values.Add(id, new ClientParameterManager().Pack(value));
            }

            object _returnValue=null;
            public void SetReturnValue(object value)
            {
                _returnValue = new ClientParameterManager().Pack(value);
            }

            public void ApplyTo(ResultConsumer client)
            {
                foreach (var o in _values)
                {
                    client.Set(o.Key, new ServerParameterManager().UnPack(o.Value));
                }
                client.SetReturnValue(new ServerParameterManager().UnPack(_returnValue));
            }

        }
        public ServerParameterByRefResultInformation CreateResult(object result)
        {
            var x = new MyParameterByRefResultInformation();
            x.SetReturnValue(result);
            foreach (var c in _columns)
            {
                c.AddTo(x);
            }
            return x;
        }

    }
}
