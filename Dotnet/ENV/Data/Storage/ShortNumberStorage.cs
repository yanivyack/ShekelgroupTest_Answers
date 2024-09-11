using System;
using ENV.Data.DataProvider;
using Firefly.Box;
using Firefly.Box.Data.DataProvider;
using ENV.Utilities;

namespace ENV.Data.Storage
{
    public class ShortNumberStorage : BtrieveIntegerStorage
    {
        protected override Number GetNumber(byte[] bytes)
        {
            if (bytes.Length == 0)
                return 0;
            return BitConverter.ToInt16(bytes, 0);
        }

        protected override byte[] GetBytes(long value)
        {
            return BitConverter.GetBytes(value >= short.MaxValue ? short.MaxValue : (value <= short.MinValue ? short.MinValue : (short)value));
        }
    }
    public class ShortNumberBoolStorage : IColumnStorageSrategy<Bool>
    {
        static ShortNumberStorage _storage = new ShortNumberStorage();
        public Bool LoadFrom(IValueLoader loader)
        {
            return _storage.LoadFrom(loader) == 1;
        }

        public void SaveTo(Bool value, IValueSaver saver)
        {
            _storage.SaveTo(value ? 1 : 0, saver);
        }
    }

    public class UShortNumberStorage : BtrieveIntegerStorage
    {
        protected override int DataTypeCode
        {
            get
            {
                return 14;
            }
        }
        protected override Number GetNumber(byte[] bytes)
        {
            return BitConverter.ToUInt16(bytes, 0);
        }

        protected override byte[] GetBytes(long value)
        {
            return BitConverter.GetBytes(value >= ushort.MaxValue ? ushort.MaxValue : (value <= ushort.MinValue ? ushort.MinValue : (ushort)value));
        }
    }

    public class PositiveNumberStorage : IColumnStorageSrategy<Number>
    {
        public Number LoadFrom(IValueLoader loader)
        {
            return loader.GetNumber();
        }

        public void SaveTo(Number value, IValueSaver saver)
        {
            saver.SaveInt(value < 0 ? 0 : (int)value);
        }
    }

    public class IntNumberStorage : BtrieveIntegerStorage
    {
        protected override Number GetNumber(byte[] bytes)
        {
            return BitConverter.ToInt32(bytes, 0);
        }

        protected override byte[] GetBytes(long value)
        {
            return BitConverter.GetBytes(value >= int.MaxValue ? int.MaxValue : (value <= int.MinValue ? int.MinValue : (int)value));
        }
    }

    public class UIntNumberStorage : BtrieveIntegerStorage, IStorageScriptCreator
    {
        protected override int DataTypeCode
        {
            get
            {
                return 14;
            }
        }

        protected override Number GetNumber(byte[] bytes)
        {
            return BitConverter.ToUInt32(bytes, 0);
        }

        protected override byte[] GetBytes(long value)
        {
            return BitConverter.GetBytes(value >= uint.MaxValue ? uint.MaxValue : (value <= uint.MinValue ? uint.MinValue : (uint)value));
        }

        public void AddTo(SqlScriptTableCreator sql, string name, string caption, bool allowNull, ScriptHelper helper)
        {
            sql.AddBinary(name, caption, 4);
        }
    }

    public class ByteNumberStorage : BtrieveIntegerStorage
    {
        protected override int DataTypeCode
        {
            get
            {
                return 14;
            }
        }

        protected override Number GetNumber(byte[] bytes)
        {
            return bytes[0];
        }

        protected override byte[] GetBytes(long value)
        {
            return new[] { (byte)value };
        }
    }

    public class SByteNumberStorage : BtrieveIntegerStorage
    {
        protected override Number GetNumber(byte[] bytes)
        {
            return (sbyte)bytes[0];
        }

        protected override byte[] GetBytes(long value)
        {
            return new [] { (byte)((sbyte)value) };
        }
    }

    public class SmallIntNumberStorage : IColumnStorageSrategy<Number>
    {
        public Number LoadFrom(IValueLoader loader)
        {
            return loader.GetNumber();
        }

        public void SaveTo(Number value, IValueSaver saver)
        {
            long l = value;
            saver.SaveInt(l >= short.MaxValue ? short.MaxValue : (l <= short.MinValue ? short.MinValue : (short)l));
        }
    }
}
